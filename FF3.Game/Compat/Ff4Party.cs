// FF4's party on the unified data layer (Shared/Data, OpenFF.Data).
//
// FF3's party lives in the decompiled pl.PlayerParty with FF3's tables and jobs; FF4's
// roster changes every chapter and its numbers come from other files. Rather than bend
// pl.PlayerParty to FF4's shape, FF4 is the first game whose party is an OpenFF.Data.Party
// built from Ff4Tables: the scripts' roster and item commands act on it here, and
// Game.Party (Ff4PartyService) shows it to mods. FF3's party is mapped onto the same
// shape later; the menu and battles for FF4 will read this and nothing else.

using System;
using System.Collections.Generic;
using System.Text.Json;
using OpenFF;
using OpenFF.Data;

namespace FF3
{
	internal static class Ff4Party
	{
		private static GameTables _tables;
		private static Party _party;

		/// <summary>FF4's tables, read once from the content chain.</summary>
		public static GameTables Tables
		{
			get
			{
				if (_tables == null && GameArchive.Chain != null)
				{
					try
					{
						_tables = Ff4Tables.Read(GameArchive.Chain);
						Log.Write(LogChannel.General, "party: " + _tables.Describe().Split('\n')[0]);
						foreach (string note in _tables.Notes) Log.Write(LogChannel.General, "party: " + note);
					}
					catch (Exception ex)
					{
						Log.Write(LogChannel.General, "party: FF4 tables: " + ex.Message);
						_tables = new GameTables { Game = "ff4" };
					}
				}
				return _tables;
			}
		}

		/// <summary>The party as it stands; a new game's until the scripts change it.</summary>
		public static Party Party
		{
			get
			{
				if (_party == null) NewGame();
				return _party;
			}
		}

		/// <summary>What FF4's initForNewgame leaves: Cecil (type 0) alone, at level 10, with nothing.</summary>
		public static void NewGame()
		{
			_party = new Party(Tables);
			_party.Join(0, 10);
			// --party=4:10,3:12 - extra members for a test start (the child Rydia at 10, Rosa at 12).
			string extra = Options.Get("party");
			if (!string.IsNullOrEmpty(extra))
			{
				foreach (string part in extra.Split(',', ';'))
				{
					string[] bits = part.Trim().Split(':');
					if (bits.Length == 0 || !int.TryParse(bits[0], out int type)) continue;
					int level = bits.Length > 1 && int.TryParse(bits[1], out int l) ? l : (_party.Leader?.Level ?? 1);
					_party.Join(type, level);
				}
			}
			string gil = Options.Get("gil");
			if (!string.IsNullOrEmpty(gil) && int.TryParse(gil, out int g)) _party.Gil = Math.Max(0, g);
			Log.Write(LogChannel.File, "party: new game - " + _party.Describe().Replace("\n", " | "));
		}

		// ---- the save chunk (Ff4Saves) ----

		public sealed class SavedCharacter
		{
			public int Id;
			public string Name;
			public int Level, Experience, Hp, MaxHp, Mp, MaxMp, Slot;
			public int[] Equipment;
			public List<int> Abilities;
			public List<int> Spells;
		}

		public sealed class Saved
		{
			public int Gil;
			public List<SavedCharacter> Roster = new List<SavedCharacter>();
			/// <summary>(item id, count) pairs, in bag order.</summary>
			public List<int[]> Items = new List<int[]>();
		}

		public static Saved Snapshot()
		{
			Party p = Party;
			Saved s = new Saved { Gil = p.Gil };
			foreach (Character c in p.Roster)
			{
				s.Roster.Add(new SavedCharacter
				{
					Id = c.Id, Name = c.Name, Level = c.Level, Experience = c.Experience,
					Hp = c.Hp, MaxHp = c.MaxHp, Mp = c.Mp, MaxMp = c.MaxMp, Slot = c.Slot,
					Equipment = (int[])c.Equipment.Clone(),
					Abilities = new List<int>(c.Abilities), Spells = new List<int>(c.Spells),
				});
			}
			foreach (OpenFF.Data.ItemStack stack in p.Inventory) s.Items.Add(new[] { stack.ItemId, stack.Count });
			return s;
		}

		/// <summary>The party as a save left it; replaces the one in play.</summary>
		public static void Restore(Saved s)
		{
			if (s == null) return;
			Party p = new Party(Tables) { Gil = Math.Max(0, s.Gil) };
			foreach (SavedCharacter sc in s.Roster)
			{
				Character c = p.Ensure(sc.Id, sc.Level);
				if (c == null) continue;
				if (!string.IsNullOrEmpty(sc.Name)) c.Name = sc.Name;
				c.Experience = sc.Experience;
				c.MaxHp = Math.Max(1, sc.MaxHp);
				c.MaxMp = Math.Max(0, sc.MaxMp);
				c.Hp = Math.Clamp(sc.Hp, 0, c.MaxHp);
				c.Mp = Math.Clamp(sc.Mp, 0, c.MaxMp);
				if (sc.Equipment != null) for (int i = 0; i < 5 && i < sc.Equipment.Length; i++) c.Equipment[i] = sc.Equipment[i];
				c.Abilities.Clear(); if (sc.Abilities != null) c.Abilities.AddRange(sc.Abilities);
				c.Spells.Clear(); if (sc.Spells != null) c.Spells.AddRange(sc.Spells);
			}
			List<SavedCharacter> lineUp = s.Roster.FindAll(r => r.Slot >= 0);
			lineUp.Sort((a, b) => a.Slot.CompareTo(b.Slot));
			foreach (SavedCharacter sc in lineUp) p.Join(sc.Id, sc.Level);
			foreach (int[] stack in s.Items) if (stack != null && stack.Length >= 2) p.AddItem(stack[0], stack[1]);
			_party = p;
			Log.Write(LogChannel.General, "party: restored - " + p.Describe().Replace("\n", " | "));
		}

		/// <summary>The level a character joins at: the leader's, as FF4 scales joiners to the party (the exact rule is not read yet).</summary>
		private static int JoinLevel(int type)
		{
			Character leader = Party.Leader;
			return leader != null ? leader.Level : 1;
		}

		// ---- the scripts' commands, by name in Ff4Commands ----

		/// <summary>addItem(item, count).</summary>
		public static void AddItem(GlobalScope.ScriptEngine engine)
		{
			int id = (int)engine.getWord();
			int count = engine.getByte();
			Party.AddItem(id, count);
			ItemDefinition item = Tables?.Item(id);
			Log.Write(LogChannel.File, "party: +" + count + " " + (item?.Name ?? ("item " + id)) + " (now " + Party.CountItem(id) + ")");
			EngineHooks.ItemGained(id, count);
		}

		/// <summary>subItem(item, count).</summary>
		public static void SubItem(GlobalScope.ScriptEngine engine)
		{
			int id = (int)engine.getWord();
			int count = engine.getByte();
			Party.RemoveItem(id, count);
		}

		/// <summary>addPartyPC(type, ?): the character joins the line-up.</summary>
		public static void AddPartyPC(GlobalScope.ScriptEngine engine)
		{
			int type = (int)engine.getDword();
			engine.getByte();
			bool joined = Party.Join(type, JoinLevel(type));
			Character c = Party.Get(type);
			Log.Write(LogChannel.General, "party: " + (c?.Name ?? ("type " + type)) + (joined ? " joins - " : " could not join - ") + Party.Members.Count + " in the party");
		}

		/// <summary>subPartyPC(type, ?): the character leaves the line-up.</summary>
		public static void SubPartyPC(GlobalScope.ScriptEngine engine)
		{
			int type = (int)engine.getDword();
			engine.getByte();
			Character c = Party.Get(type);
			bool left = Party.Leave(type);
			Log.Write(LogChannel.General, "party: " + (c?.Name ?? ("type " + type)) + (left ? " leaves - " : " was not in the party - ") + Party.Members.Count + " in the party");
		}

		/// <summary>setPartyPCEquipItem(type, left hand, right hand, head, body, arm): what the character wears, as ids (the castle gives Cecil the Dark Shield first, then the Dark Sword).</summary>
		public static void SetPartyPCEquipItem(GlobalScope.ScriptEngine engine)
		{
			int type = (int)engine.getDword();
			int[] items = new int[5];
			for (int i = 0; i < 5; i++) items[i] = (int)engine.getWord();
			Character c = Party.Ensure(type, JoinLevel(type));
			if (c == null) return;
			c.Equipment[(int)OpenFF.Data.EquipSlot.LeftHand] = items[0];
			c.Equipment[(int)OpenFF.Data.EquipSlot.RightHand] = items[1];
			c.Equipment[(int)OpenFF.Data.EquipSlot.Head] = items[2];
			c.Equipment[(int)OpenFF.Data.EquipSlot.Body] = items[3];
			c.Equipment[(int)OpenFF.Data.EquipSlot.Arms] = items[4];
			Log.Write(LogChannel.File, "party: " + c.Name + " equips " + string.Join(", ", Array.ConvertAll(items, id => id == 0 ? "-" : Tables?.Item(id)?.Name ?? id.ToString())));
		}

		/// <summary>addAbility(type, ability).</summary>
		public static void AddAbility(GlobalScope.ScriptEngine engine)
		{
			int type = (int)engine.getDword();
			int ability = (int)engine.getDword();
			Character c = Party.Ensure(type, JoinLevel(type));
			if (c != null && !c.Abilities.Contains(ability)) c.Abilities.Add(ability);
		}
	}

	/// <summary>Game.Party on FF4: the unified party through the engine's party interface.</summary>
	internal sealed class Ff4PartyService : GameService, IParty, ISaveable
	{
		private readonly Dictionary<int, Condition> _conditions = new Dictionary<int, Condition>();

		private static Party P => Ff4Party.Party;

		// ISaveable: the whole party rides in the save slot (Ff4Saves).
		public string ChunkId => "ff4/party";
		public int ChunkVersion => 1;
		public object Save() => Ff4Party.Snapshot();
		public void Load(int version, JsonElement data)
		{
			Ff4Party.Restore(JsonSerializer.Deserialize<Ff4Party.Saved>(data.GetRawText(), Ff4Saves.Json));
			_conditions.Clear();
		}

		public int Gil
		{
			get => P.Gil;
			set => P.Gil = Math.Max(0, value);
		}

		public void AddItem(int itemId, int count) => P.AddItem(itemId, count);
		public int ItemCount(int itemId) => P.CountItem(itemId);
		public bool RemoveItem(int itemId, int count) => P.RemoveItem(itemId, count);

		public IReadOnlyList<PartyMember> Members
		{
			get
			{
				List<PartyMember> members = new List<PartyMember>();
				foreach (Character c in P.Members) members.Add(Describe(c));
				return members;
			}
		}

		public PartyMember Member(int id)
		{
			Character c = P.Get(id);
			return c == null ? null : Describe(c);
		}

		private PartyMember Describe(Character c)
		{
			OpenFF.Data.Stats s = c.StatsWith(P.Tables);
			PartyMember m = new PartyMember
			{
				Id = c.Id,
				Slot = c.Slot,
				Name = c.Name,
				Level = c.Level,
				Experience = c.Experience,
				Hp = c.Hp,
				MaxHp = c.MaxHp,
				Mp = c.Mp,
				Job = 0,
				JobSkill = 0,
				Alive = c.Alive,
				Conditions = _conditions.TryGetValue(c.Id, out Condition cond) ? cond : Condition.None,
			};
			m.Stats.Strength = s.Strength;
			m.Stats.Vitality = s.Vitality;
			m.Stats.Agility = s.Agility;
			m.Stats.Intellect = s.Intellect;
			m.Stats.Mind = s.Spirit;
			m.Charges[0] = c.Mp;
			m.MaxCharges[0] = c.MaxMp;
			m.Spells.AddRange(c.Spells);
			return m;
		}

		public bool AddMember(int id) => P.Join(id, P.Leader?.Level ?? 1);
		public bool RemoveMember(int id) => P.Leave(id);

		public void SetLevel(int id, int level)
		{
			Character c = P.Ensure(id, level);
			c?.SetLevel(level, false);
			if (c != null && P.Tables.ExperienceToLevel.Length >= level) c.Experience = P.Tables.ExperienceToLevel[Math.Clamp(level, 1, P.Tables.ExperienceToLevel.Length) - 1];
		}

		public void HealAll()
		{
			foreach (Character c in P.Roster) { c.Hp = c.MaxHp; c.Mp = c.MaxMp; }
			_conditions.Clear();
		}

		/// <summary>Every status off everyone (an inn's night, a tent).</summary>
		public void CureAll() => _conditions.Clear();

		public int Hurt(int id, int amount, bool canKill = false)
		{
			Character c = P.Get(id);
			if (c == null) return 0;
			c.Hp = Math.Max(canKill ? 0 : 1, c.Hp - Math.Max(0, amount));
			return c.Hp;
		}

		public int Heal(int id, int amount, bool revive = false)
		{
			Character c = P.Get(id);
			if (c == null) return 0;
			if (c.Hp <= 0 && !revive) return 0;
			c.Hp = Math.Min(c.MaxHp, c.Hp + Math.Max(0, amount));
			return c.Hp;
		}

		public void SetHp(int id, int now, int max = -1)
		{
			Character c = P.Get(id);
			if (c == null) return;
			if (max >= 0) c.MaxHp = Math.Max(1, max);
			c.Hp = Math.Clamp(now, 0, c.MaxHp);
		}

		/// <summary>FF4 has magic points, not charges: level 1 stands for the pool; other levels are ignored.</summary>
		public void SetCharges(int id, int level, int now, int max = -1)
		{
			Character c = P.Get(id);
			if (c == null || level != 1) return;
			if (max >= 0) c.MaxMp = Math.Max(0, max);
			c.Mp = Math.Clamp(now, 0, c.MaxMp);
		}

		public bool GiveExperience(int id, int amount)
		{
			Character c = P.Get(id);
			if (c == null) return false;
			int before = c.Level;
			c.Experience += Math.Max(0, amount);
			int level = P.Tables.LevelForExperience(c.Experience);
			if (level > before) c.SetLevel(level, false);
			return level > before;
		}

		/// <summary>FF4's characters have fixed classes; a job change means nothing here.</summary>
		public void SetJob(int id, Job job) { }

		public void SetStat(int id, Stat stat, int value)
		{
			Character c = P.Get(id);
			if (c == null) return;
			switch (stat)
			{
				case Stat.Strength: c.Base.Strength = value; break;
				case Stat.Vitality: c.Base.Vitality = value; break;
				case Stat.Agility: c.Base.Agility = value; break;
				case Stat.Intellect: c.Base.Intellect = value; break;
				case Stat.Mind: c.Base.Spirit = value; break;
			}
		}

		public bool LearnSpell(int id, int spellId)
		{
			Character c = P.Get(id);
			if (c == null || c.Abilities.Contains(spellId)) return false;
			c.Abilities.Add(spellId);
			return true;
		}

		public bool ForgetSpell(int id, int spellId) => P.Get(id)?.Abilities.Remove(spellId) ?? false;

		public void Inflict(int id, Condition conditions)
		{
			_conditions[id] = (_conditions.TryGetValue(id, out Condition have) ? have : Condition.None) | conditions;
			if ((conditions & Condition.Death) != 0) { Character c = P.Get(id); if (c != null) c.Hp = 0; }
		}

		public void Cure(int id, Condition conditions)
		{
			if (_conditions.TryGetValue(id, out Condition have)) _conditions[id] = have & ~conditions;
		}

		public IReadOnlyList<OpenFF.ItemStack> Items
		{
			get
			{
				List<OpenFF.ItemStack> items = new List<OpenFF.ItemStack>();
				foreach (OpenFF.Data.ItemStack s in P.Inventory) items.Add(new OpenFF.ItemStack { ItemId = s.ItemId, Count = s.Count });
				return items;
			}
		}

		public bool Equip(int id, int itemId, OpenFF.EquipSlot slot = OpenFF.EquipSlot.Auto)
		{
			Character c = P.Get(id);
			ItemDefinition item = P.Tables.Item(itemId);
			if (c == null || item?.Equip == null) return false;
			if (slot == OpenFF.EquipSlot.Auto)
			{
				slot = item.Kind == ItemKind.Weapon ? OpenFF.EquipSlot.RightHand : OpenFF.EquipSlot.Body;
			}
			if ((int)slot < 0 || (int)slot > 4) return false;
			if (P.CountItem(itemId) > 0) P.RemoveItem(itemId, 1);
			return P.Equip(id, (OpenFF.Data.EquipSlot)(int)slot, itemId);
		}

		public void Unequip(int id, OpenFF.EquipSlot slot)
		{
			if ((int)slot < 0 || (int)slot > 4) return;
			P.Equip(id, (OpenFF.Data.EquipSlot)(int)slot, 0);
		}

		public int Equipped(int id, OpenFF.EquipSlot slot)
		{
			Character c = P.Get(id);
			return c == null || (int)slot < 0 || (int)slot > 4 ? 0 : c.Equipment[(int)slot];
		}
	}
}
