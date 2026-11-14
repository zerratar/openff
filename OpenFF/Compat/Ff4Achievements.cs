// FF4's achievements by its own rules: achievement_param.bbd beside FF4.exe (AchievementResource::load), 320-byte
// records - the Steam name (+0x00, 64 bytes), the moments it is looked at (+0x40, a mask; 0 any), its check (+0x44,
// AchievementCheckFuncs' table) and the check's numbers (+0x48, 62 ints, -1 ending a list). One achievement may have a
// record for each moment that can earn it (Gil 10000 after a fight, a scene, a step on a map and a shop alike).
//
// The moments (AchievementObserver::reqUpdate's callers in libff4 and FF4.exe):
//   0x01 a scene starts (babilCommand_EventStart)     0x02 a scene ends (EventEnd)      0x04 New Game on the title
//   0x08 the party starts walking a map (WSMove)      0x10 aboard a vehicle (WSVehicleMove)
//   0x20 a fight is won (BattleEnding::execute)       0x80 an augment learned (MSSItem's decant learning)
//   0x100 a shop's business (MSSShop::mssProcess)     0x40 after others were earned (achievementComp: all of them)
//
// The checks (AchievementCheckFuncs, by number):
//   0 flag: (group, index, on) triples all as said         1 flagRange: (group, first, last, on) all as said
//   2 hardModeBeat: this fight's party won on Hard          3 beatCount: this fight's party won, the game cleared n times
//   4 getItem: every item held or worn by someone          5 getDecant: every augment held or learned from one
//   6 getDecantCnt: augments held and learned, n or more   7 learnSummon: (character, ability) pairs all learned
//   8 learnSummonCnt: a character's summons, n or more     9 completedMap: every map flag (1:701, 1:702..849)
//   10 overPlayerLevel: (character, level) all reached      11 gill: n gil or more
//   12 monsterKillCount: the Bestiary's kills, n or more   13 monsterDictionary: the Bestiary n% complete
//   14 monsterKillParty: this fight's party one of these won  15 noCheck            16 field: on a map (f00, t02_00...)
//   17 achievementComp: every other achievement earned

using System;
using System.Collections.Generic;
using System.IO;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal static class Ff4Achievements
	{
		public const uint SceneStart = 0x1, SceneEnd = 0x2, NewGame = 0x4, Walk = 0x8, Vehicle = 0x10, BattleWon = 0x20, AllOthers = 0x40, Augment = 0x80, Shop = 0x100;

		private sealed class Rule
		{
			public string Name;
			public uint Mask;
			public int Check;
			public int[] P;
		}

		private static readonly List<Rule> _rules = new List<Rule>();
		private static uint _pending;

		public static int Count => _rules.Count;

		/// <summary>The achievements the rules name (58 on Steam), and how many Steam has as earned.</summary>
		public static int NameCount
		{
			get { HashSet<string> names = new HashSet<string>(); foreach (Rule r in _rules) names.Add(r.Name); return names.Count; }
		}

		public static int EarnedCount()
		{
			HashSet<string> names = new HashSet<string>();
			foreach (Rule r in _rules) if (Achievements.IsEarned(r.Name)) names.Add(r.Name);
			return names.Count;
		}

		/// <summary>The party of the fight under way or last fought (StartParty) - what checks 2, 3 and 14 compare.</summary>
		public static int LastParty = -1;

		public static void Load(string install)
		{
			_rules.Clear();
			string path = Path.Combine(install, "achievement_param.bbd");
			if (!File.Exists(path)) { Log.Write(LogChannel.General, "achievements: no achievement_param.bbd beside FF4.exe"); return; }
			byte[] data = File.ReadAllBytes(path);
			for (int at = 0; at + 0x140 <= data.Length; at += 0x140)
			{
				int end = at;
				while (end < at + 0x40 && data[end] != 0) end++;
				Rule rule = new Rule
				{
					Name = System.Text.Encoding.ASCII.GetString(data, at, end - at),
					Mask = BitConverter.ToUInt32(data, at + 0x40),
					Check = BitConverter.ToInt32(data, at + 0x44),
					P = new int[62],
				};
				for (int k = 0; k < 62; k++) rule.P[k] = BitConverter.ToInt32(data, at + 0x48 + 4 * k);
				if (rule.Name.Length > 0) _rules.Add(rule);
			}
		}

		/// <summary>A moment that may earn something: its rules are looked at on the next step.</summary>
		public static void Request(uint moment)
		{
			if (Achievements.Active) _pending |= moment;
		}

		public static void Tick()
		{
			if (_pending == 0 || _rules.Count == 0 || Ff4Party.Party == null) return;
			uint moment = _pending;
			_pending = 0;
			bool any = false;
			foreach (Rule rule in _rules)
			{
				if (rule.Mask != 0 && (rule.Mask & moment) == 0) continue;
				if (Achievements.IsEarned(rule.Name)) continue;
				bool passes;
				try { passes = Passes(rule); }
				catch (Exception ex) { Log.First(LogChannel.General, "achievements-" + rule.Name, 1, () => "achievements: " + rule.Name + ": " + ex.Message); continue; }
				if (!passes) continue;
				Achievements.Unlock(rule.Name);
				any = true;
			}
			if (any) _pending |= AllOthers;
		}

		private static bool Flag(int group, int index) =>
			GlobalScope.flags != null && group >= 0 && group < GlobalScope.flags.GetLength(0) && index >= 0 && index < GlobalScope.flags.GetLength(1) && GlobalScope.flags[group, index] != 0;

		/// <summary>The fight just won (BattleEnding's moment): its party, and won - not run from or lost.</summary>
		private static bool Won(int party) => party == LastParty && Ff4Battle.LastResult == 0;

		private static Character Type(int type) => type >= 0 ? Ff4Party.Party.Get(type) : null;

		private static bool Knows(Character c, int ability) => c != null && (c.Spells.Contains(ability) || c.Abilities.Contains(ability));

		/// <summary>Flag gates (check 8, 16): one or two (group, index, on) triples at P[0..5].</summary>
		private static bool Gates(int[] p)
		{
			for (int k = 0; k < 6; k += 3)
			{
				if (p[k] < 0 || p[k + 1] < 0) break;
				if ((p[k + 2] != 0) != Flag(p[k], p[k + 1])) return false;
			}
			return true;
		}

		private static IEnumerable<int> List(int[] p, int step = 1)
		{
			for (int k = 0; k < p.Length && p[k] >= 0; k += step) yield return k;
		}

		private static bool Passes(Rule r)
		{
			int[] p = r.P;
			Party party = Ff4Party.Party;
			switch (r.Check)
			{
				case 0:   // flag
					for (int k = 0; k + 2 < 62; k += 3)
					{
						if (p[k] < 0 || p[k + 1] < 0) return true;
						if ((p[k + 2] != 0) != Flag(p[k], p[k + 1])) return false;
					}
					return true;
				case 1:   // flagRange
					for (int k = 0; k + 3 < 62 && p[k] >= 0; k += 4)
						for (int i = p[k + 1]; i <= p[k + 2]; i++)
							if ((p[k + 3] != 0) != Flag(p[k], i)) return false;
					return true;
				case 2:   // hardModeBeat
					return Won(p[0]) && Ff4Settings.Current.Hard;
				case 3:   // beatCount: the game's clear count - OpenFF knows "cleared before" by flag 0:998, which New Game+ keeps
					return Won(p[0]) && (Flag(0, 998) ? 1 : 0) == p[1];
				case 4:   // getItem
					foreach (int k in List(p))
					{
						int item = p[k];
						if (party.CountItem(item) > 0) continue;
						bool worn = false;
						foreach (Character c in party.Roster) if (Array.IndexOf(c.Equipment, item) >= 0) { worn = true; break; }
						if (!worn) return false;
					}
					return true;
				case 5:   // getDecant
					foreach (int k in List(p))
					{
						int item = p[k];
						if (party.CountItem(item) > 0) continue;
						int ability = Ff4Augments.AbilityOf(item);
						bool learned = false;
						foreach (Character c in party.Roster) if (ability > 0 && c.Abilities.Contains(ability)) { learned = true; break; }
						if (!learned) return false;
					}
					return true;
				case 6:   // getDecantCnt: held, plus each who learned one (Dark Knight Cecil and young Rydia are counted as their other forms)
				{
					int count = 0;
					foreach (int item in Ff4Augments.AllItems)
					{
						count += party.CountItem(item);
						int ability = Ff4Augments.AbilityOf(item);
						foreach (Character c in party.Roster) if (c.Id != 0 && c.Id != 4 && ability > 0 && c.Abilities.Contains(ability)) count++;
					}
					return count >= p[0];
				}
				case 7:   // learnSummon
					for (int k = 0; k + 1 < 62 && p[k] >= 0; k += 2)
						if (!Knows(Type(p[k]), p[k + 1])) return false;
					return true;
				case 8:   // learnSummonCnt
				{
					if (!Gates(p)) return false;
					Character c = Type(p[9]);
					int known = 0;
					for (int a = 1500; a <= 1517; a++) if (Knows(c, a)) known++;
					return known >= p[10];
				}
				case 9:   // completedMap
					if (!Flag(1, 0x2bd)) return false;
					for (int i = 0; i < 0x94; i++) if (!Flag(1, 0x2be + i)) return false;
					return true;
				case 10:  // overPlayerLevel
					for (int k = 0; k + 1 < 62 && p[k] >= 0; k += 2)
					{
						Character c = Type(p[k]);
						if (c == null || c.Level < p[k + 1]) return false;
					}
					return true;
				case 11:  // gill
					return party.Gil >= p[0];
				case 12:  // monsterKillCount
				{
					int kills = 0;
					foreach (Ff4Bestiary.Entry e in Ff4Bestiary.Book) kills += Ff4Bestiary.Kills(e.Monster);
					return kills >= p[0];
				}
				case 13:  // monsterDictionary
					return Ff4Bestiary.Complete() >= p[0];
				case 14:  // monsterKillParty
					foreach (int k in List(p)) if (Won(p[k])) return true;
					return false;
				case 15:  // noCheck
					return true;
				case 16:  // field: on the map of that kind (0 f, 1 t, 2 d), number and floor (not for the world maps)
				{
					if (!Gates(p)) return false;
					string map = OpenFF.Game.Field.Map ?? "";
					char kind = p[9] == 0 ? 'f' : p[9] == 1 ? 't' : p[9] == 2 ? 'd' : '?';
					if (map.Length < 3 || char.ToLowerInvariant(map[0]) != kind) return false;
					if (!int.TryParse(map.Substring(1, 2), out int number) || number != p[10]) return false;
					if (kind == 'f') return true;
					return map.Length >= 6 && int.TryParse(map.Substring(4, 2), out int floor) && floor == p[11];
				}
				case 17:  // achievementComp
					foreach (Rule other in _rules) if (other.Check != 17 && !Achievements.IsEarned(other.Name)) return false;
					return true;
			}
			return false;
		}
	}
}
