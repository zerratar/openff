// The engine's abilities API on the legacy game: spells, monsters, the formulas.
//
// The spells come from itm.ItemManager's magic table (item_parameter.pak, chain 3) with
// their effect and sound from pl.PlayerParty's normal-magic table (player.chaindata,
// chain 12); names are text ids resolved through the message system, so they read right
// in whatever language the game runs. The monsters come from mon.MonsterManager
// (monster.chaindata), which the battle loads on entry and frees on exit - here it is
// loaded on first use and left loaded. The formulas are btl.NewMagicFormula's, ported
// line for line over plain Stats so they work on a mod's own creatures too, not only on
// battle characters (which only exist inside a battle).

using System;
using System.Collections.Generic;
using System.Linq;
using OpenFF;

namespace FF3
{
	internal sealed class LegacyMagic : GameService, IMagic
	{
		private readonly List<Spell> _spells = new List<Spell>();
		private readonly Dictionary<int, Spell> _byId = new Dictionary<int, Spell>();
		private readonly HashSet<int> _soundBanks = new HashSet<int>();
		private readonly Random _random = new Random();
		private bool _tableRead;

		public IReadOnlyList<Spell> All
		{
			get
			{
				EnsureTable();
				return _spells;
			}
		}

		public Spell Find(int id)
		{
			EnsureTable();
			return _byId.TryGetValue(id, out Spell spell) ? spell : null;
		}

		public Spell Find(string name)
		{
			EnsureTable();
			if (string.IsNullOrEmpty(name)) return null;
			foreach (Spell spell in _spells)
			{
				if (spell.Name == null) spell.Name = Text(spell, nameNotCaption: true);
				if (string.Equals(spell.Name, name, StringComparison.OrdinalIgnoreCase)) return spell;
			}
			return null;
		}

		public void Add(Spell spell)
		{
			if (spell == null) return;
			EnsureTable();
			spell.Custom = true;
			Remove(spell.Id);
			_spells.Add(spell);
			_byId[spell.Id] = spell;
		}

		public void Remove(int id)
		{
			if (_byId.TryGetValue(id, out Spell old))
			{
				_spells.Remove(old);
				_byId.Remove(id);
			}
		}

		// ---- the table ----

		private readonly Dictionary<Spell, short[]> _textIds = new Dictionary<Spell, short[]>();

		private void EnsureTable()
		{
			if (_tableRead) return;
			// FF4's tables have their own layouts (PakRecordsFf4 in the editor); reading them as FF3's throws.
			if (GameProfile.IsFf4) { _tableRead = true; return; }
			try
			{
				GlobalScope.itm.ItemManager items = GlobalScope.itm.ItemManager.instance();
				int count = items.magicCount();
				if (count == 0) return;   // not loaded yet (before a game part read its tables); try again later
				for (int i = 0; i < count; i++)
				{
					GlobalScope.itm.MagicParameter p = items.magicAt(i);
					if (p == null) continue;
					Spell spell = new Spell
					{
						Id = p.itemId(),
						School = (MagicSchool)Math.Clamp((int)p.system(), 0, 5),
						Level = p.magicClass() + 1,
						Kind = (MagicKind)Math.Clamp((int)p.magicUseKind(), 0, 3),
						Power = p.magicAggressivity(),
						Accuracy = p.successProbability(),
						Elements = (Element)p.magicType(),
						Targets = (Targeting)p.targetPossible(),
						HitsAll = p.allTarget() != 0,
						InBattle = p.useBattle() != 0,
						InField = p.useField() != 0,
						Reflectable = p.isReflect() != 0,
						Conditions = (Condition)(p.changeCondition() & 0xFF),
						Jobs = p.equipJob(),
					};
					_textIds[spell] = new[] { p.nameId(), p.captionId() };
					try
					{
						GlobalScope.pl.PlayerNormalMagicParameter normal = GlobalScope.pl.PlayerParty.instance().normalMagic(spell.Id);
						if (normal != null)
						{
							GlobalScope.ys.Effects effect = normal.effect();
							GlobalScope.ys.Effects sound = normal.se();
							if (effect != null) { spell.EffectCategory = effect.category_; spell.EffectMember = effect.member_; }
							if (sound != null) { spell.SoundArchive = sound.category_; spell.SoundNumber = sound.member_; }
						}
					}
					catch (Exception) { }
					_spells.Add(spell);
					_byId[spell.Id] = spell;
				}
				_tableRead = true;
				OpenFF.Game.Log("engine api: " + _spells.Count + " spells read from the game's tables");
			}
			catch (Exception ex)
			{
				EngineApi.Warn("magic-table", "reading the magic table: " + ex.Message);
			}
		}

		/// <summary>Names are texts, and the texts loaded change with the game part; resolve when asked, on a map.</summary>
		private string Text(Spell spell, bool nameNotCaption)
		{
			if (!_textIds.TryGetValue(spell, out short[] ids)) return null;
			short id = nameNotCaption ? ids[0] : ids[1];
			if (id <= 0) return null;
			try
			{
				string text = GlobalScope.dgs.msg.CMessageSys.getInstance().Main().getMessage((uint)id);
				return string.IsNullOrEmpty(text) ? null : text.Trim();
			}
			catch (Exception) { return null; }
		}

		/// <summary>Each frame on a map: fill in names still missing (cheap once done).</summary>
		internal void Tick()
		{
			if (!EngineApi.InWorld) return;
			EnsureTable();
			if (!_tableRead || _namesDone) return;
			_namesDone = true;
			foreach (Spell spell in _spells)
			{
				if (spell.Custom) continue;
				if (spell.Name == null) spell.Name = Text(spell, nameNotCaption: true);
				if (spell.Caption == null) spell.Caption = Text(spell, nameNotCaption: false);
				if (spell.Name == null) _namesDone = false;
			}
		}
		private bool _namesDone;

		// ---- casting: the effect and the sound on the field ----

		public int Cast(Spell spell, Vector3 at, float scale = 1f) => CastImpl(spell, at, null, false, scale);
		public int CastOn(Spell spell, Npc target, float scale = 1f) => CastImpl(spell, target?.Position ?? Vector3.Zero, target, false, scale);
		public int CastOnHero(Spell spell, float scale = 1f) => CastImpl(spell, OpenFF.Game.Hero.Position, null, true, scale);

		private int CastImpl(Spell spell, Vector3 at, Npc target, bool onHero, float scale)
		{
			if (spell == null || !EngineApi.InWorld) return -1;
			int id = -1;
			try
			{
				IEffects effects = OpenFF.Game.Effects;
				if (spell.EffectCategory >= 0 && spell.EffectMember >= 0)
				{
					if (!effects.Loaded(spell.EffectCategory) && !effects.Load(spell.EffectCategory))
					{
						EngineApi.Warn("cast-pack", "Magic.Cast: no room for effect pack " + spell.EffectCategory + " on this map");
					}
					else
					{
						id = effects.Spawn(spell.EffectCategory, spell.EffectMember, at);
						if (id >= 0)
						{
							if (Math.Abs(scale - 1f) > 0.001f) effects.Scale(id, scale);
							if (target != null) effects.Follow(id, target);
							else if (onHero) effects.FollowHero(id);
						}
					}
				}
				if (spell.SoundArchive >= 0 && spell.SoundNumber >= 0)
				{
					if (_soundBanks.Add(spell.SoundArchive))
					{
						GlobalScope.MatrixSound.MtxSENDS_Load(spell.SoundArchive);
					}
					GlobalScope.MatrixSound.MtxSENDS_Play(spell.SoundArchive, spell.SoundNumber, 192, 127);
				}
			}
			catch (Exception ex) { EngineApi.Warn("cast", "Magic.Cast: " + ex.Message); }
			if (spell.OnCast != null)
			{
				SpellCast cast = new SpellCast { Spell = spell, At = at, Target = target, OnHero = onHero, Effect = id, Scale = scale };
				OpenFF.Game.Guard("Spell.OnCast " + spell.Name, () => spell.OnCast(cast));
			}
			return id;
		}

		// ---- the formulas (btl.NewMagicFormula, over Stats) ----

		public int Damage(Spell spell, Stats caster, Stats target, int targets = 1, bool roll = true)
		{
			if (spell == null || caster == null || target == null) return 0;
			// calcAttackMagicDamage
			int basePart = (spell.Power + caster.JobSkill - target.MagicDefense - target.Mind) * caster.Intellect / 3;
			if (basePart < 0) basePart = 0;
			// calcMagicSuccess + calcMagicSuccessValue (fx32 -> fraction)
			double success;
			if (roll)
			{
				bool hit = caster.Intellect - (target.Mind + 30) >= _random.Next(101);
				success = hit ? (_random.Next(21) + 90) / 100.0 : (_random.Next(11) + 50) / 100.0;
			}
			else
			{
				success = 1.0;
			}
			int attr = Attribute(spell.Elements, target);
			double many = TargetsValue(spell, targets, 80);
			long value = (long)(basePart * success * many);
			value = attr != 0 ? value * attr : value / 2;
			return (int)Math.Max(0, Math.Min(value, 999999));
		}

		public int Healing(Spell spell, Stats caster, Stats target, int targets = 1, bool roll = true)
		{
			if (spell == null || caster == null || target == null) return 0;
			int basePart = (caster.Mind / 2 + caster.JobSkill / 4 + caster.Vitality / 8) * spell.Power;
			double luck = roll ? (100 - _random.Next(10)) / 100.0 : 1.0;
			int attr = Attribute(spell.Elements == Element.None ? Element.Recovery : spell.Elements, target);
			double many = TargetsValue(spell, targets, 90);
			long value = (long)(basePart * luck) * attr;
			value = (long)(value * many);
			return (int)Math.Max(0, Math.Min(value, 999999));
		}

		private static int Attribute(Element attack, Stats target)
		{
			if ((attack & target.Weakness) != 0) return 2;
			if ((attack & target.Resist) != 0) return 0;
			return 1;
		}

		private static double TargetsValue(Spell spell, int targets, int rate)
		{
			Targeting t = spell.Targets;
			const Targeting singles = Targeting.Nothing | Targeting.Enemy | Targeting.EnemyGroup | Targeting.RandomEnemy
				| Targeting.Self | Targeting.Friend | Targeting.FriendGroup | (Targeting)0x20 | (Targeting)0x400 | (Targeting)0x800;
			bool onlyAll = (t & (Targeting.EnemyAll | Targeting.FriendAll)) != 0 && (t & singles) == 0;
			if (onlyAll || targets <= 1) return 1.0;
			return Math.Max(0, rate - targets * 10) / 100.0;
		}

		// ---- charges and the game's own field use ----

		public bool CanCast(int memberId, Spell spell)
		{
			if (spell == null) return false;
			try
			{
				GlobalScope.pl.Player player = GlobalScope.pl.PlayerParty.instance().playerForId((byte)memberId);
				if (player == null || !player.isEnable()) return false;
				if (player.condition().isDeath() || player.condition().isSilence() || player.condition().isStone()) return false;
				int level = Math.Clamp(spell.Level, 1, 8) - 1;
				if (!spell.Custom && spell.School != MagicSchool.Song && player.mp(level).getNow() <= 0) return false;
				if (spell.Custom) return true;
				GlobalScope.pl.EquipmentMagic equipped = player.equipParameter().equipMagic((GlobalScope.pl.MAGIC_LEVEL)level);
				for (int i = 0; i < GlobalScope.pl.MAGIC_ONCE_LEVEL_EQUIP_MAX; i++)
				{
					if (equipped.magicId(i) == spell.Id) return true;
				}
				return false;
			}
			catch (Exception) { return false; }
		}

		public bool Spend(int memberId, Spell spell)
		{
			if (spell == null) return false;
			try
			{
				GlobalScope.pl.Player player = GlobalScope.pl.PlayerParty.instance().playerForId((byte)memberId);
				if (player == null) return false;
				int level = Math.Clamp(spell.Level, 1, 8) - 1;
				GlobalScope.ys.MPoint<int> charges = player.mp(level);
				if (charges.getNow() <= 0) return false;
				charges.subNow(1);
				player.setJobChangeMp(level, (byte)charges.getNow());
				return true;
			}
			catch (Exception ex) { EngineApi.Warn("spend", "Magic.Spend: " + ex.Message); return false; }
		}

		public bool UseInField(Spell spell, int casterId, int targetId, bool all = false)
		{
			if (spell == null || !EngineApi.InWorld) return false;
			try
			{
				int casterSlot = SlotOf(casterId), targetSlot = SlotOf(targetId);
				if (casterSlot < 0 || targetSlot < 0) return false;
				return new GlobalScope.itm.ItemUse().useMagicInField(spell.Id, casterSlot, targetSlot, all);
			}
			catch (Exception ex) { EngineApi.Warn("field-use", "Magic.UseInField: " + ex.Message); return false; }
		}

		private static int SlotOf(int memberId)
		{
			GlobalScope.pl.PlayerParty party = GlobalScope.pl.PlayerParty.instance();
			for (byte s = 0; s < 4; s++)
			{
				GlobalScope.pl.Player p = party.player(s);
				if (p != null && p.isEnable() && p.playerId() == memberId) return s;
			}
			return -1;
		}
	}

	internal sealed class LegacyMonsters : GameService, IMonsters
	{
		private readonly List<Monster> _monsters = new List<Monster>();
		private readonly Dictionary<int, Monster> _byId = new Dictionary<int, Monster>();
		private readonly Dictionary<int, MonsterGroup> _groups = new Dictionary<int, MonsterGroup>();
		private bool _read, _groupsRead;

		public IReadOnlyList<Monster> All
		{
			get { EnsureTable(); return _monsters; }
		}

		public Monster Find(int id)
		{
			EnsureTable();
			return _byId.TryGetValue(id, out Monster m) ? m : null;
		}

		public Monster Find(string name)
		{
			EnsureTable();
			return _monsters.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
		}

		public MonsterGroup Group(int partyId)
		{
			if (_groups.TryGetValue(partyId, out MonsterGroup cached)) return cached;
			try
			{
				GlobalScope.mon.MonsterPartyManager parties = GlobalScope.mon.MonsterPartyManager.instance();
				if (!_groupsRead)
				{
					// Outside a battle the table is not loaded; inside one the battle loaded its own (normal or event).
					if (!parties.isLoaded() && !parties.loadNormalTable()) return null;
					_groupsRead = true;
				}
				GlobalScope.mon.MonsterParty party = parties.findMonsterParty(partyId);
				if (party == null) return null;
				MonsterGroup group = new MonsterGroup { Id = partyId };
				for (int i = 0; i < GlobalScope.mon.MONSTERS_MAX; i++)
				{
					GlobalScope.mon.Monsters m = party.monsters(i);
					if (m == null || m.monsterId() <= 0) continue;
					group.Members.Add(new MonsterCount { MonsterId = m.monsterId(), Min = m.min(), Max = m.max() });
				}
				_groups[partyId] = group;
				return group;
			}
			catch (Exception ex) { EngineApi.Warn("monster-party", "Monsters.Group: " + ex.Message); return null; }
		}

		// Monster names live in the battle's own text block (eureka_battle.msd), which only the
		// battle part loads, into the slot the field's common text holds. Read the file here and
		// keep it, without registering it - the field's own texts stay as they are.
		private GlobalScope.dgs.MSDINFO _battleText;
		private bool _battleTextTried;

		private string BattleText(uint number)
		{
			try
			{
				string field = GlobalScope.dgs.msg.CMessageSys.getInstance().Main().getMessage(number);
				if (!string.IsNullOrEmpty(field)) return field.Trim();
			}
			catch (Exception) { }
			if (!_battleTextTried)
			{
				_battleTextTried = true;
				try
				{
					GlobalScope.changeCompanyDirectory();
					uint size = GlobalScope.ds.g_File.getSize("eureka_battle.msd");
					if (size != 0)
					{
						Array bytes = GlobalScope.ds.CHeap.alloc_app(size);
						if (bytes != null && GlobalScope.ds.g_File.load(bytes, "eureka_battle.msd"))
						{
							_battleText = (GlobalScope.dgs.MSDINFO)bytes;
						}
					}
				}
				catch (Exception ex) { EngineApi.Warn("battle-text", "reading eureka_battle.msd: " + ex.Message); }
				finally
				{
					try { GlobalScope.changeGlobalDirectory(); } catch (Exception) { }
				}
			}
			if (_battleText == null) return null;
			try
			{
				for (int i = 0; i < _battleText.num_msg; i++)
				{
					if (_battleText.elements[i].number == number)
					{
						string text = StringUtil.createString(_battleText.m_abyData, (int)_battleText.elements[i].offset);
						return string.IsNullOrEmpty(text) ? null : text.Trim();
					}
				}
			}
			catch (Exception) { }
			return null;
		}

		private void EnsureTable()
		{
			if (_read) return;
			if (GameProfile.IsFf4) { _read = true; return; }
			try
			{
				GlobalScope.mon.MonsterManager monsters = GlobalScope.mon.MonsterManager.instance();
				if (!monsters.isLoaded() && !monsters.load()) return;
				int count = monsters.monsterCount();
				for (int i = 0; i < count; i++)
				{
					GlobalScope.mon.MonsterParameter p = monsters.monsterAt(i);
					if (p == null) continue;
					Monster m = new Monster
					{
						Id = p.monsterId(),
						Family = p.familyId(),
						Model = "f" + p.familyId().ToString("D3"),
						MotionSet = "b_f" + p.familyId().ToString("D3"),
						Level = p.level(),
						MaxHp = p.maxHp(),
						Size = p.size(),
						Special = p.isSpecial(),
					};
					try
					{
						GlobalScope.ys.BodyParameter body = p.body();
						m.Stats.Strength = body.strength().get();
						m.Stats.Vitality = body.vitality().get();
						m.Stats.Agility = body.dexterity().get();
						m.Stats.Intellect = body.intelligence().get();
						m.Stats.Mind = body.mind().get();
					}
					catch (Exception) { }
					try
					{
						m.Stats.MagicDefense = p.magicDefense().magicPhylacticPower();
						m.Stats.Weakness = (Element)p.magicDefense().weakType();
						m.Stats.Resist = (Element)p.physicsDefense().antiType();
					}
					catch (Exception) { }
					m.Stats.JobSkill = Math.Max(1, m.Level / 2);   // CommonFormula.calcJobSkill for monsters
					try
					{
						GlobalScope.mon.DroppingDataParameter drop = p.droppingParameter();
						if (drop != null) { m.Gil = drop.gold(); m.Experience = drop.exp(); }
					}
					catch (Exception) { }
					m.Name = BattleText((uint)p.nameId());
					_monsters.Add(m);
					_byId[m.Id] = m;
				}
				_read = true;
				OpenFF.Game.Log("engine api: " + _monsters.Count + " monsters read from the game's table");
			}
			catch (Exception ex) { EngineApi.Warn("monster-table", "reading the monster table: " + ex.Message); }
		}
	}
}
