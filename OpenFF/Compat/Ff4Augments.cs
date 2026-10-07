// FF4's augments ("decants"), read from libff4.so: abilities learned from key items and set as battle commands. A member
// fights with its command slots (PlayerAbilityManager list 2: five chosen, then Defend and Swap Rows - each class's
// default layout put back on every addPartyPC); a passive works while it sits in a slot (isCommand). An augment item used
// on a member (world::mssdLearnAbility) teaches it - and the member's other form, Cecil's Dark Knight and Paladin, Rydia
// young and grown - adds one to its decant level, and is used up. The leave scenes test that level
// (decantLevelChekcJump) and give back more the more were given. Until the Abilities menu is in, a newly learned augment
// takes the first free slot (an OpenFF convenience, not Steam's way).

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal static class Ff4Augments
	{
		// pl::Player::initializeDefaultCommand's table (libff4.so 0x1bb788), by player type: the five slots, 0 for empty.
		private static readonly int[][] DefaultCommands =
		{
			new[] { 1, 32, 4, 0, 0 },      // 0 Cecil, Dark Knight
			new[] { 1, 10, 6, 4, 0 },      // 1 Cecil, Paladin
			new[] { 1, 31, 4, 0, 0 },      // 2 Kain
			new[] { 1, 25, 64, 6, 4 },     // 3 Rosa
			new[] { 1, 6, 5, 13, 4 },      // 4 Rydia (young)
			new[] { 1, 5, 13, 4, 0 },      // 5 Rydia
			new[] { 1, 52, 6, 5, 4 },      // 6 Tellah
			new[] { 1, 65, 66, 6, 4 },     // 7 Porom
			new[] { 1, 65, 67, 5, 4 },     // 8 Palom
			new[] { 1, 19, 21, 18, 4 },    // 9 Edward
			new[] { 1, 37, 56, 68, 4 },    // 10 Yang
			new[] { 1, 28, 54, 4, 0 },     // 11 Cid
			new[] { 1, 7, 42, 83, 4 },     // 12 Edge
			new[] { 1, 69, 6, 5, 4 },      // 13 FuSoYa
		};

		public const int Defend = 3, SwapRows = 46;

		// The augment key items (AchievementCheckFuncs::DecantItemTbl) and what each teaches (item -> efficacy +0xC).
		private static readonly Dictionary<int, int> Items = new Dictionary<int, int>
		{
			{ 9104, 18 }, { 9105, 21 }, { 9106, 19 }, { 9107, 65 }, { 9108, 66 }, { 9109, 67 }, { 9110, 52 }, { 9111, 53 },
			{ 9112, 17 }, { 9113, 28 }, { 9114, 54 }, { 9115, 167 }, { 9116, 37 }, { 9117, 56 }, { 9118, 68 }, { 9119, 69 },
			{ 9120, 55 }, { 9121, 70 }, { 9122, 72 }, { 9136, 166 }, { 9137, 71 }, { 9138, 30 }, { 9139, 15 }, { 9140, 168 },
			{ 9142, 78 }, { 9143, 169 }, { 9145, 171 }, { 9146, 172 }, { 9147, 173 }, { 9148, 174 }, { 9149, 73 }, { 9150, 74 },
			{ 9151, 125 }, { 9152, 76 }, { 9153, 80 }, { 9154, 82 }, { 9155, 8 }, { 9158, 32 }, { 9166, 178 },
		};

		// The passives (ability ids) whose effects the battle and field look for.
		public const int Counter = 15, LastStand = 55, FastTalker = 53, Phoenix = 70, Omnicasting = 71, LimitBreak = 72,
			PiercingMagic = 78, DrawAttacks = 80, MpEfficiency = 81, ItemLore = 30, AutoPotion = 166, Adrenaline = 167,
			MpPlus50 = 168, HpPlus50 = 169, Reach = 171, LevelLust = 172, GilFarmer = 173, TreasureHunter = 174, SafeTravel = 8;

		private static readonly HashSet<int> Passives = new HashSet<int>
		{
			Counter, LastStand, FastTalker, Phoenix, Omnicasting, LimitBreak, PiercingMagic, DrawAttacks, MpEfficiency, ItemLore,
			AutoPotion, Adrenaline, MpPlus50, HpPlus50, Reach, LevelLust, GilFarmer, TreasureHunter, SafeTravel,
		};

		/// <summary>ability.bbd +0x24 bit 0 clear: a passive, listed in its slot but not a command to choose.</summary>
		public static bool IsPassive(int ability) => Passives.Contains(ability);

		/// <summary>The ability an augment item teaches, 0 for an item that is not one.</summary>
		public static int AbilityOf(int itemId) => Items.TryGetValue(itemId, out int ability) ? ability : 0;

		/// <summary>The slots as laid out, laid out by the class's default first if they are not yet.</summary>
		public static int[] Slots(Character c)
		{
			if (c.CommandSlots == null || c.CommandSlots.Length != 7) DefaultLayout(c);
			PlaceLearned(c);
			return c.CommandSlots;
		}

		/// <summary>initializeDefaultCommand: the class's five, then Defend and Swap Rows (a type without one: what its levels give).</summary>
		public static void DefaultLayout(Character c)
		{
			int[] slots = new int[7];
			if (c.Id >= 0 && c.Id < DefaultCommands.Length) Array.Copy(DefaultCommands[c.Id], slots, 5);
			else
			{
				int n = 0;
				foreach (int id in c.Definition.CommandsAt(c.Level))
					if (id != Defend && id != SwapRows && n < 5 && Array.IndexOf(slots, id) < 0) slots[n++] = id;
			}
			slots[5] = Defend;
			slots[6] = SwapRows;
			c.CommandSlots = slots;
			Refresh(c);
		}

		/// <summary>An augment learned and in no slot: into the first free one of the five (OpenFF's, until the Abilities menu).</summary>
		private static void PlaceLearned(Character c)
		{
			GameTables t = Ff4Party.Tables;
			foreach (int id in c.Abilities)
			{
				if (id <= 0 || id >= 256 || Array.IndexOf(c.CommandSlots, id) >= 0 || !IsAugment(id)) continue;
				int free = Array.IndexOf(c.CommandSlots, 0);
				if (free < 0 || free > 4) break;
				c.CommandSlots[free] = id;
				Log.Write(LogChannel.File, "augments: " + c.Name + " sets " + (t?.AbilityName(3000 + id)?.Trim() ?? id.ToString()) + " in slot " + free);
				Refresh(c);
			}
		}

		private static bool IsAugment(int ability)
		{
			foreach (int a in Items.Values) if (a == ability) return true;
			return false;
		}

		/// <summary>Player::isCommand: the ability sits in one of its slots.</summary>
		public static bool Has(Character c, int ability) => c != null && Array.IndexOf(Slots(c), ability) >= 0;

		/// <summary>PlayerParty::isCommand: any member has it.</summary>
		public static bool PartyHas(int ability)
		{
			foreach (Character c in Ff4Party.Party.Members) if (Has(c, ability)) return true;
			return false;
		}

		/// <summary>HP +50% / MP +50% (setPlus50Percent, setParameter): the maximums x1.5 (HP to 9999) while in a slot.</summary>
		public static void Refresh(Character c)
		{
			bool hp = c.CommandSlots != null && Array.IndexOf(c.CommandSlots, HpPlus50) >= 0;
			bool mp = c.CommandSlots != null && Array.IndexOf(c.CommandSlots, MpPlus50) >= 0;
			if (hp != c.HpPlus)
			{
				c.MaxHp = hp ? Math.Min(9999, c.MaxHp * 3 / 2) : Math.Max(1, c.MaxHp * 2 / 3);
				c.Hp = Math.Min(c.Hp, c.MaxHp);
				c.HpPlus = hp;
			}
			if (mp != c.MpPlus)
			{
				c.MaxMp = mp ? Math.Min(9999, c.MaxMp * 3 / 2) : c.MaxMp * 2 / 3;
				c.Mp = Math.Min(c.Mp, c.MaxMp);
				c.MpPlus = mp;
			}
		}

		/// <summary>A level climbed: MP's maximum is the table's again (and HP's, for one whose HP is not rolled on), the +50%s on them again.</summary>
		public static void AfterLevelUp(Character c)
		{
			if (!c.Definition.RollsHp) c.HpPlus = false;
			c.MpPlus = false;
			Refresh(c);
		}

		/// <summary>mssdLearnAbility: the item's ability learned by the member and its other form, the decant level up, the item gone.</summary>
		public static string Use(int itemId, Character c)
		{
			int ability = AbilityOf(itemId);
			if (ability <= 0 || c == null) return "That cannot be used here.";
			string name = Ff4Party.Tables?.AbilityName(3000 + ability)?.Trim() ?? ("ability " + ability);
			if (c.Abilities.Contains(ability) || Array.IndexOf(Slots(c), ability) >= 0) return c.Name + " already knows " + name + ".";
			if (!Ff4Party.Party.RemoveItem(itemId, 1)) return "";
			c.DecantLevel = Math.Min(255, c.DecantLevel + 1);
			Learn(c, ability);
			int twin = c.Id switch { 0 => 1, 1 => 0, 4 => 5, 5 => 4, _ => -1 };
			if (twin >= 0 && Ff4Party.Party.Get(twin) is Character other) Learn(other, ability);
			Log.Write(LogChannel.General, "augments: " + c.Name + " learns " + name + " (decant level " + c.DecantLevel + ")");
			return c.Name + " learned " + name + ".";
		}

		private static void Learn(Character c, int ability)
		{
			if (!c.Abilities.Contains(ability)) c.Abilities.Add(ability);
			if (c.CommandSlots != null) PlaceLearned(c);
		}

		// ---- the scripts' commands ----

		/// <summary>removeAbility(type, ability): forgetAbility.</summary>
		public static void RemoveAbility(GlobalScope.ScriptEngine engine)
		{
			int type = (int)engine.getDword();
			int ability = (int)engine.getDword();
			Character c = Ff4Party.Party.Get(type);
			if (c == null) return;
			c.Abilities.Remove(ability);
			c.Spells.Remove(ability);
			if (c.CommandSlots != null) { int at = Array.IndexOf(c.CommandSlots, ability); if (at >= 0 && at < 5) c.CommandSlots[at] = 0; Refresh(c); }
		}

		/// <summary>copyDecantAbility(from, to, ex1..ex4): every ability and spell the one form learned, the other learns, but the four named.</summary>
		public static void CopyDecantAbility(GlobalScope.ScriptEngine engine)
		{
			int from = (int)engine.getDword(), to = (int)engine.getDword();
			int[] except = { (int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword() };
			Character a = Ff4Party.Party.Get(from), b = Ff4Party.Party.Get(to);
			if (a == null || b == null) return;
			foreach (int id in a.Abilities) if (Array.IndexOf(except, id) < 0 && (id < 256 || id >= 1500) && !b.Abilities.Contains(id)) b.Abilities.Add(id);
			foreach (int id in a.Spells) if (Array.IndexOf(except, id) < 0 && !b.Spells.Contains(id)) b.Spells.Add(id);
			b.DecantLevel = Math.Max(b.DecantLevel, a.DecantLevel);
			Log.Write(LogChannel.File, "augments: " + b.Name + " takes " + a.Name + "'s abilities");
		}

		/// <summary>decantLevelChekcJump(type, level, label): jumps when the character has been given exactly that many augments.</summary>
		public static void DecantLevelCheckJump(GlobalScope.ScriptEngine engine)
		{
			int type = (int)engine.getDword(), level = (int)engine.getDword();
			uint label = engine.getDword();
			Character c = Ff4Party.Party.Get(type);
			if ((c?.DecantLevel ?? 0) == level) engine.jump(label);
		}
	}
}
