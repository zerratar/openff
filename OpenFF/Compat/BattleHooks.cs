// The battle's events for the mods (OpenFF.Engine/BattleUnits.cs, Events.cs): who is in it, live, and what
// happens in it - the monsters ready (their stats a mod may change for this battle), each blow, spell and
// heal as it lands (its amount a mod may change), a turn starting, someone falling (a mod may revive them
// at once), and the spoils. The battle code calls in here at the places it does these things
// (BattleSystem.initialize, TurnSystem.setNormalAttackDamage / initializeTurn, BattleCalculation's attack and
// recovery magic and damageCharacter, BattleWin.windowOpenPhase, BattleCharacterManager.getTrueExp), and
// each call is a few field reads when no mod listens.
//
// Also here: a monster party a mod makes at run time (IBattle.Start(MonsterGroup)) - the party table's lookup
// (MonsterPartyManager.monsterParty) answers CustomFormation with it - and the "a battle you may lose" switch
// a mod's battle can ask for.

using System;
using System.Collections.Generic;
using OpenFF.Events;

namespace OpenFF.Client
{
	/// <summary>A hero or a monster of the running battle, over the battle's own character.</summary>
	internal sealed class LegacyBattleUnit : BattleUnit
	{
		internal readonly GlobalScope.btl.BaseBattleCharacter C;
		private string _name;
		public LegacyBattleUnit(GlobalScope.btl.BaseBattleCharacter c) { C = c; }

		private GlobalScope.btl.BattleMonster M => C as GlobalScope.btl.BattleMonster;
		private GlobalScope.btl.BattlePlayer P => C as GlobalScope.btl.BattlePlayer;

		public override int Id => C.battleCharacterId();
		public override bool IsMonster => C.breed() == 1;
		public override int MonsterId => M != null ? M.monsterId() : -1;
		public override PartyMember Member { get { try { return P != null ? OpenFF.Game.Party?.Member(P.playerId()) : null; } catch (Exception) { return null; } } }
		public override string Name
		{
			get
			{
				if (_name != null) return _name;
				try
				{
					if (P != null) _name = P.player().name();
					else if (M != null) _name = BattleCommands.BattleName((uint)M.monster().nameId()) ?? ("monster " + M.monsterId());
				}
				catch (Exception) { }
				return _name ?? "?";
			}
		}
		public override bool Alive { get { try { return C.hp().getNow() > 0 && !C.condition().isDeath() && !C.condition().isStone(); } catch (Exception) { return false; } } }
		public override int Hp
		{
			get => C.hp().getNow();
			set { int v = Math.Max(0, Math.Min(value, C.hp().getLimit())); C.hp().setNow(v); }
		}
		public override int MaxHp
		{
			get => C.hp().getLimit();
			set { if (M == null) return; int v = Math.Max(1, Math.Min(999999, value)); C.hp().setLimit(v); if (C.hp().getNow() > v) C.hp().setNow(v); }
		}
		public override int Level { get => C.level(); set { if (M != null) C.setLevel((byte)Math.Max(1, Math.Min(99, value))); } }
		public override int Attack { get => Read(() => C.handAttack(GlobalScope.pl.HAND_TYPE.RIGHT_HAND).aggressivity().get()); set { if (M != null) Write(() => C.handAttack(GlobalScope.pl.HAND_TYPE.RIGHT_HAND).aggressivity().set(Clamp(value))); } }
		public override int Defense { get => Read(() => C.physicsDefense().phylacticPower().get()); set { if (M != null) Write(() => C.physicsDefense().phylacticPower().set(Clamp(value))); } }
		public override int MagicDefense { get => Read(() => C.magicDefense().magicPhylacticPower()); set { if (M != null) Write(() => C.magicDefense().magicPhylacticPower_set((short)Clamp(value))); } }
		public override int Strength { get => Read(() => C.bodyAndBonus().strength().get()); set { if (M != null) Write(() => C.bodyAndBonus().strength().set(Clamp(value))); } }
		public override int Agility { get => Read(() => C.bodyAndBonus().dexterity().get()); set { if (M != null) Write(() => C.bodyAndBonus().dexterity().set(Clamp(value))); } }
		public override int Vitality { get => Read(() => C.bodyAndBonus().vitality().get()); set { if (M != null) Write(() => C.bodyAndBonus().vitality().set(Clamp(value))); } }
		public override int Intellect { get => Read(() => C.bodyAndBonus().intelligence().get()); set { if (M != null) Write(() => C.bodyAndBonus().intelligence().set(Clamp(value))); } }
		public override int Mind { get => Read(() => C.bodyAndBonus().mind().get()); set { if (M != null) Write(() => C.bodyAndBonus().mind().set(Clamp(value))); } }

		public override void Revive(int hp)
		{
			try
			{
				hp = Math.Max(1, Math.Min(hp, C.hp().getLimit()));
				if (C.condition().isDeath())
				{
					C.condition().offDeath();
					C.clearFlag(GlobalScope.btl.PLAYER_FLAG.PF_2D);
					if (P != null) P.setIdleType(0);
				}
				C.hp().setNow(hp);
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "battle: revive: " + ex.Message); }
		}

		private static int Clamp(int v) => Math.Max(0, Math.Min(9999, v));
		private static int Read(Func<int> f) { try { return f(); } catch (Exception) { return 0; } }
		private static void Write(Action a) { try { a(); } catch (Exception) { } }
	}

	internal static class BattleHooks
	{
		/// <summary>The party number a mod's run-time party goes by (past the game's 259 and the mods' appended ones).</summary>
		public const short CustomFormation = 30000;

		private static GlobalScope.mon.MonsterParty _custom;
		private static GlobalScope.btl.BattleCharacterManager _manager;
		private static readonly Dictionary<GlobalScope.btl.BaseBattleCharacter, LegacyBattleUnit> _units = new Dictionary<GlobalScope.btl.BaseBattleCharacter, LegacyBattleUnit>();
		private static int? _exp;

		/// <summary>The party a mod made for the next battle (MonsterPartyManager.monsterParty answers CustomFormation with it).</summary>
		public static GlobalScope.mon.MonsterParty CustomParty(int id) => id == CustomFormation ? _custom : null;

		public static void SetCustomParty(MonsterGroup group)
		{
			var members = new List<(short, byte, byte)>();
			foreach (MonsterCount m in group.Members)
			{
				if (members.Count == GlobalScope.mon.MONSTERS_MAX) break;
				int min = Math.Max(1, Math.Min(6, m.Min)), max = Math.Max(min, Math.Min(6, m.Max));
				members.Add(((short)m.MonsterId, (byte)min, (byte)max));
			}
			_custom = GlobalScope.mon.MonsterParty.Of(CustomFormation, members);
		}

		private static bool Listening => EngineHost.Attached && OpenFF.Game.Events.HandlerCount > 0;

		public static LegacyBattleUnit Unit(GlobalScope.btl.BaseBattleCharacter c)
		{
			if (c == null) return null;
			if (!_units.TryGetValue(c, out LegacyBattleUnit u)) { u = new LegacyBattleUnit(c); _units[c] = u; }
			return u;
		}

		/// <summary>Everyone in the battle now: heroes, then monsters standing in it.</summary>
		public static IReadOnlyList<BattleUnit> Units()
		{
			var list = new List<BattleUnit>();
			try
			{
				if (_manager == null || !OpenFF.Game.Battle.InBattle) return list;
				for (int i = 0; i < 4; i++) { GlobalScope.btl.BattlePlayer p = _manager.playerParty().battlePlayer(i); if (p != null && p.isEnable()) list.Add(Unit(p)); }
				for (int i = 0; i < 6; i++) { GlobalScope.btl.BattleMonster m = _manager.monsterParty().battleMonster(i); if (m != null && m.isEnable()) list.Add(Unit(m)); }
			}
			catch (Exception) { }
			return list;
		}

		/// <summary>BattleSystem.initialize: the characters are in place - the monsters ready for a mod's changes.</summary>
		public static void MonstersReady(GlobalScope.btl.BattleCharacterManager manager)
		{
			_manager = manager;
			_units.Clear();
			_exp = null;
			if (!Listening) return;
			try
			{
				var monsters = new List<BattleUnit>();
				for (int i = 0; i < 6; i++) { GlobalScope.btl.BattleMonster m = manager.monsterParty().battleMonster(i); if (m != null && m.isEnable()) monsters.Add(Unit(m)); }
				int formation = -1;
				try { formation = GlobalScope.btl.OutsideToBattle.getInstance().initializeMonster().monsterPartyId(); } catch (Exception) { }
				Publish(new BattleMonstersReady { Monsters = monsters, Formation = formation });
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "battle: monsters ready: " + ex.Message); }
		}

		/// <summary>TurnSystem.setNormalAttackDamage, per target: a blow's damage as it is about to land.</summary>
		public static int Blow(GlobalScope.btl.BaseBattleCharacter attacker, GlobalScope.btl.BaseBattleCharacter target, int amount)
		{
			if (!Listening) return amount;
			try
			{
				bool jump = attacker.flag(GlobalScope.btl.PLAYER_FLAG.PF_JUMP);
				bool ability = jump || attacker.flag(GlobalScope.btl.PLAYER_FLAG.PF_DARK);
				bool missed = target.flag(GlobalScope.btl.PLAYER_FLAG.PF_MISS);
				bool healing = target.flag(GlobalScope.btl.PLAYER_FLAG.PF_RECOVER);
				var e = new BattleDamage
				{
					Attacker = Unit(attacker), Target = Unit(target), Kind = healing ? DamageKind.Healing : ability ? DamageKind.Ability : DamageKind.Physical,
					Critical = attacker.flag(GlobalScope.btl.PLAYER_FLAG.PF_CRITICAL), Jump = jump, Missed = missed, Amount = amount
				};
				Publish(e);
				return missed ? amount : Math.Max(0, Math.Min(9999, e.Amount));
			}
			catch (Exception) { return amount; }
		}

		/// <summary>BattleCalculation's attack and recovery magic: a spell's damage or healing on one target as it is about to land.</summary>
		public static int Spell(GlobalScope.btl.BaseBattleCharacter user, GlobalScope.btl.BaseBattleCharacter target, int spellId, int magicType, bool recovery, int amount)
		{
			if (!Listening) return amount;
			try
			{
				bool healing = recovery && target.flag(GlobalScope.btl.PLAYER_FLAG.PF_RECOVER);
				var e = new BattleDamage
				{
					Attacker = Unit(user), Target = Unit(target), Kind = healing ? DamageKind.Healing : DamageKind.Magic,
					SpellId = spellId, Element = ElementOf(magicType), Missed = target.flag(GlobalScope.btl.PLAYER_FLAG.PF_MISS), Amount = amount
				};
				Publish(e);
				return e.Missed ? amount : Math.Max(0, Math.Min(9999, e.Amount));
			}
			catch (Exception) { return amount; }
		}

		/// <summary>A spell's elements: its type's element bits (Thunder 0x8 .. Dark 0x400), which the API's Element shares.</summary>
		private static Element ElementOf(int magicType) => (Element)(magicType & 0x7F8);

		/// <summary>TurnSystem.initializeTurn: someone's turn begins.</summary>
		public static void TurnStarting(GlobalScope.btl.BaseBattleCharacter c)
		{
			if (!Listening || c == null) return;
			Publish(new BattleTurnStarting { Unit = Unit(c) });
		}

		/// <summary>BattleCalculation.damageCharacter, at 0 HP and before the fall: true when a mod revived them (they stand).</summary>
		public static bool Falling(GlobalScope.btl.BaseBattleCharacter target)
		{
			if (!Listening || target == null) return false;
			try
			{
				if (target.condition().isDeath()) return false;
				GlobalScope.btl.BaseBattleCharacter by = GlobalScope.btl.TurnSystem.Current?.nowCharacter();
				Publish(new BattleUnitFell { Unit = Unit(target), By = by != null && by != target ? Unit(by) : null });
				return target.hp().getNow() > 0;
			}
			catch (Exception) { return false; }
		}

		/// <summary>BattleWin.windowOpenPhase: the spoils - a mod's say on the gil now, and on each hero's experience for getTrueExp after.</summary>
		public static int Rewards(GlobalScope.btl.BattleSystem battle, int gil)
		{
			_exp = null;
			if (!Listening) return gil;
			try
			{
				var e = new BattleRewards { Exp = battle.characterManager().getTrueExp(), Gil = gil };
				Publish(e);
				_exp = Math.Max(0, e.Exp);
				return Math.Max(0, e.Gil);
			}
			catch (Exception) { return gil; }
		}

		/// <summary>BattleCharacterManager.getTrueExp: each hero's experience, as a mod's BattleRewards set it.</summary>
		public static int Exp(int exp) => _exp ?? exp;

		private static void Publish<T>(T evt)
		{
			try { OpenFF.Game.Events.Publish(evt); }
			catch (Exception ex) { Log.First(LogChannel.General, "battle-hook-" + typeof(T).Name, 3, () => "battle: hook " + typeof(T).Name + " failed: " + ex.Message); }
		}
	}
}
