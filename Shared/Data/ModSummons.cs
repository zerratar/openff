// The mods' summons (defs/summons/*.json): a summon's outcome played by steps of the mod's own in place of the
// game's script. FF3's summons are scripts in files/summon_script_command.pack - a chain for each of the eight
// summons' three outcomes (chain outcome + 3 * level: btl.SummonDataManager.load), a list of 36-byte records, a
// command each (btl.SUMMON_BEHAVIOR) that btl.BaseSummon.run steps through a game step at a time. A definition
// here is such a list, written out; the client rebuilds the pack with it as the pack is read, so the battle plays it
// with its own interpreter and nothing of the battle's code changes.
//
//   { "summon": "Shiva", "outcome": "combine",
//     "steps": [ { "do": "SET_DARK_SCREEN", "p": [5, 15, 15, 15] },
//                { "do": "IS_DARK_SCREEN" },
//                { "do": "DRAW_SUMMON_EFFECT_TARGET_ALL", "p": ["frost-nova", 1, 0], "again": true }, ... ] }
//
// "summon": the creature (Chocobo, Shiva, Ramuh, Ifrit, Titan, Odin, Leviathan, Bahamut), its spell's name (Escape,
// Icen ...) or its level 0..7. "outcome": white, black, combine (or 0, 1, 2). A step: "do", the command by its name;
// "p", up to seven numbers (positions in the battle's fixed point, 4096 a unit; angles in degrees); "again", run in
// the same step as the one before. The effect of a draw command (SET_EFFECT, DRAW_SUMMON_EFFECT,
// DRAW_SUMMON_EFFECT_TARGET_ALL, CREATE_EFFECT_AND_SET_POSITION) may be one of the mods' own by its id
// (defs/effects), as a string: its category is put in.
//
// A new summon: "spell" names a spell of the mod's (defs/items) based on one of the eight summons' spells, and the
// steps are that spell's for the outcome - the game's summon keeps its own. Such scripts go in after the game's 24
// chains (24, 25 ... in load order: NewSummons); the battle plays the one for the spell and outcome it rolls, the
// base's damage and the rest of its outcome as they are.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace OpenFF.Data
{
	internal sealed class ModSummonStep
	{
		public string Do;
		public JsonNode[] P = new JsonNode[7];
		public bool Again;
	}

	internal sealed class ModSummon
	{
		public string Id;
		public string Summon;
		/// <summary>A new summon's spell (a mod item based on a summon), by its name or id; null for one of the game's eight.</summary>
		public string Spell;
		public int Outcome;
		public List<ModSummonStep> Steps = new List<ModSummonStep>();
		public string Source;
	}

	internal static class ModSummons
	{
		public const string Folder = "defs/summons";
		public const string Pack = "summon_script_command.pack";
		public const int Record = 36;
		/// <summary>The game's chains: eight summons times three outcomes. A new summon's scripts come after.</summary>
		public const int GameChains = 24;

		/// <summary>The commands in btl.SUMMON_BEHAVIOR's order: a record's command is its index here.</summary>
		public static readonly string[] Commands =
		{
			"SET_DARK_SCREEN", "IS_DARK_SCREEN", "SET_NORMAL_SCREEN", "IS_NORMAL_SCREEN", "SET_FADE_OUT", "IS_FADE_OUT", "SET_FADE_IN", "IS_FADE_IN",
			"SET_MODEL", "IS_MODEL", "SET_MOTION", "IS_MOTION", "START_MOTION", "IS_MOTION_FRAME", "DEL_SUMMON", "REGISTER_PLAYERS",
			"SET_EFFECT", "CLEAR_EFFECT", "IS_EFFECT", "DRAW_SUMMON_EFFECT", "DRAW_SUMMON_EFFECT_TARGET_ALL", "IS_END_SUMMON_EFFECT", "CHANGE_CAMERA", "SET_CAMERA_POSITION",
			"SET_CAMERA_TARGET", "SET_MOVE_CAMERA_FRAME", "MOVING_CAMERA", "SET_BATTLE_CAMERA", "SET_SUMMON_PARAMETER", "SET_POSITION_AND_ROTATION", "SHOW_MONSTERS", "HIDE_MONSTERS",
			"SHOW_SUMMON", "HIDE_SUMMON", "SUMMON_ALPHA_RATE", "SET_PLAYERS_ALPHA", "APPEAR_PLAYERS", "SET_TURN_FLAG", "IS_TURN_FLAG", "MOVE_CAMERA_AND_SUMMON_ALPHA",
			"FRAME_COUNT", "SET_2D", "IS_2D", "DEAD_CHARACTERS", "SUMMON_BEHAVIOR_END", "SHAKE_CAMERA", "ROTATE_CHARACTER", "READY_MOVE_CHARACTER",
			"MOVE_CHARACTER", "SET_FLASH", "READY_AUTO_CAMERA", "IS_AUTO_CAMERA", "DRAW_TARGET_EFFECT", "IS_SHAKE_CAMERA", "SET_SHOW_PLAYER_WINDOW", "CREATE_EFFECT_AND_SET_POSITION",
			"IS_END_MONSTER_EFFECT", "IS_END_PLAYER_EFFECT", "SET_MONSTERS_ALPHA", "APPEAR_MONSTERS", "DISAPPEAR_MONSTERS", "SET_SE", "PLAY_SE", "CLEAR_SE",
		};

		/// <summary>The commands whose first number is an effect's category: one of the mods' may be named instead.</summary>
		public static readonly HashSet<int> EffectCommands = new HashSet<int> { 16, 19, 20, 55 };

		public static readonly string[] Outcomes = { "white", "black", "combine" };

		/// <summary>The eight by level: the creature, then its spell's name as FF3 has it.</summary>
		public static readonly (string Creature, string Spell)[] Names =
		{
			("Chocobo", "Escape"), ("Shiva", "Icen"), ("Ramuh", "Spark"), ("Ifrit", "Heatra"),
			("Titan", "Hyper"), ("Odin", "Catastro"), ("Leviathan", "Leviath"), ("Bahamut", "Bahamur"),
		};

		public static int CommandNumber(string word)
		{
			if (string.IsNullOrWhiteSpace(word)) return -1;
			string w = word.Trim().ToUpperInvariant().Replace(' ', '_').Replace('-', '_');
			if (w == "END") w = "SUMMON_BEHAVIOR_END";
			if (w == "CREATE_EFFECT_AND_SET_POSITION" || w == "CRAETE_EFFECT_AND_SET_POSITION") return 55;
			if (int.TryParse(w, out int n)) return n >= 0 && n < Commands.Length ? n : -1;
			return Array.IndexOf(Commands, w);
		}

		/// <summary>A summon's level from a definition's word: the creature, the spell, or 0..7.</summary>
		public static int Level(string word)
		{
			if (string.IsNullOrWhiteSpace(word)) return -1;
			string w = word.Trim();
			if (int.TryParse(w, out int n)) return n >= 0 && n < 8 ? n : -1;
			for (int i = 0; i < Names.Length; i++)
				if (string.Equals(w, Names[i].Creature, StringComparison.OrdinalIgnoreCase) || string.Equals(w, Names[i].Spell, StringComparison.OrdinalIgnoreCase)) return i;
			return -1;
		}

		public static int OutcomeNumber(JsonNode node)
		{
			if (node is JsonValue v)
			{
				if (v.TryGetValue(out int n)) return n >= 0 && n < 3 ? n : -1;
				if (v.TryGetValue(out string s)) return Array.FindIndex(Outcomes, o => string.Equals(o, s?.Trim(), StringComparison.OrdinalIgnoreCase));
			}
			return 0;
		}

		public static ModSummon Parse(string json, string source = null)
		{
			JsonObject node = JsonNode.Parse(json) as JsonObject;
			if (node == null) return null;
			ModSummon s = new ModSummon
			{
				Id = node["id"]?.ToString(),
				Summon = node["summon"]?.ToString(),
				Spell = string.IsNullOrWhiteSpace(node["spell"]?.ToString()) ? null : node["spell"].ToString().Trim(),
				Outcome = OutcomeNumber(node["outcome"]),
				Source = source,
			};
			if (node["steps"] is JsonArray steps)
			{
				foreach (JsonNode n in steps)
				{
					if (n is not JsonObject o) continue;
					ModSummonStep step = new ModSummonStep { Do = o["do"]?.ToString(), Again = o["again"] is JsonValue a && a.TryGetValue(out bool b) && b };
					if (o["p"] is JsonArray p) for (int i = 0; i < 7 && i < p.Count; i++) step.P[i] = p[i]?.DeepClone();
					s.Steps.Add(step);
				}
			}
			return s;
		}

		public static List<ModSummon> Load(IEnumerable<string> roots, List<string> notes = null)
		{
			List<ModSummon> found = new List<ModSummon>();
			foreach (string root in roots ?? Enumerable.Empty<string>())
			{
				string folder = Path.Combine(root, Folder);
				if (!Directory.Exists(folder)) continue;
				foreach (string file in Directory.EnumerateFiles(folder, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
				{
					try
					{
						ModSummon s = Parse(File.ReadAllText(file), file);
						if (s == null) continue;
						if (string.IsNullOrWhiteSpace(s.Id)) s.Id = Path.GetFileNameWithoutExtension(file);
						if (s.Spell == null && Level(s.Summon) < 0) { notes?.Add(file + ": no summon '" + s.Summon + "' (Chocobo, Shiva, Ramuh, Ifrit, Titan, Odin, Leviathan, Bahamut, or 0..7)"); continue; }
						if (s.Outcome < 0) { notes?.Add(file + ": no outcome (white, black or combine)"); continue; }
						if (s.Steps.Count == 0) { notes?.Add(file + ": no steps"); continue; }
						found.Add(s);
					}
					catch (Exception ex) { notes?.Add(file + ": " + ex.Message); }
				}
			}
			return found;
		}

		/// <summary>
		/// The new summons' scripts in the order their chains follow the game's (GameChains + index): one for each spell
		/// and outcome, a later definition of the same in place of the earlier (load order).
		/// </summary>
		public static List<ModSummon> NewSummons(IEnumerable<ModSummon> summons)
		{
			List<ModSummon> found = new List<ModSummon>();
			foreach (ModSummon s in summons ?? Enumerable.Empty<ModSummon>())
			{
				if (s.Spell == null) continue;
				int at = found.FindIndex(f => f.Outcome == s.Outcome && string.Equals(f.Spell, s.Spell, StringComparison.OrdinalIgnoreCase));
				if (at >= 0) found[at] = s; else found.Add(s);
			}
			return found;
		}

		public static bool IsPack(string name) => name != null && name.Replace('\\', '/').EndsWith(Pack, StringComparison.OrdinalIgnoreCase);

		/// <summary>
		/// The pack with the definitions' scripts in place of the game's: its chains read, the chosen ones written anew
		/// from the steps (a record a step, the last END if the steps have none), the pack put back together - its
		/// count, its table of offsets and sizes, each chain on a 16-byte boundary. A later definition of the same
		/// summon and outcome wins (load order).
		/// </summary>
		public static byte[] Compose(byte[] pack, IEnumerable<ModSummon> summons, Func<string, int?> effectCategory, List<string> problems = null)
		{
			if (pack == null || pack.Length < 16) return pack;
			int count = BitConverter.ToInt32(pack, 0);
			if (count <= 0 || count > 256 || 16 + 8 * count > pack.Length) return pack;
			byte[][] chains = new byte[count][];
			for (int c = 0; c < count; c++)
			{
				int offset = BitConverter.ToInt32(pack, 16 + 8 * c), size = BitConverter.ToInt32(pack, 20 + 8 * c);
				if (offset < 0 || size < 0 || offset + size > pack.Length) return pack;
				chains[c] = new byte[size];
				Buffer.BlockCopy(pack, offset, chains[c], 0, size);
			}
			bool changed = false;
			foreach (ModSummon s in summons ?? Enumerable.Empty<ModSummon>())
			{
				if (s.Spell != null) continue;
				int index = s.Outcome + 3 * Level(s.Summon);
				if (index < 0 || index >= count) { problems?.Add(s.Id + ": no chain for " + s.Summon + "/" + s.Outcome); continue; }
				chains[index] = Records(s, effectCategory, problems);
				changed = true;
			}
			// The new summons' scripts, after the game's.
			List<ModSummon> added = count == GameChains ? NewSummons(summons) : new List<ModSummon>();
			if (count != GameChains && (summons ?? Enumerable.Empty<ModSummon>()).Any(s => s.Spell != null)) problems?.Add("the pack has " + count + " chains, not " + GameChains + " - the new summons' scripts are left out");
			if (added.Count > 0)
			{
				chains = chains.Concat(added.Select(s => Records(s, effectCategory, problems))).ToArray();
				count = chains.Length;
				changed = true;
			}
			if (!changed) return pack;
			int table = 16 + 8 * count;
			int at = (table + 15) & ~15;
			List<int> offsets = new List<int>();
			foreach (byte[] chain in chains) { offsets.Add(at); at = (at + chain.Length + 15) & ~15; }
			byte[] outPack = new byte[at];
			BitConverter.GetBytes(count).CopyTo(outPack, 0);
			for (int c = 0; c < count; c++)
			{
				BitConverter.GetBytes(offsets[c]).CopyTo(outPack, 16 + 8 * c);
				BitConverter.GetBytes(chains[c].Length).CopyTo(outPack, 20 + 8 * c);
				Buffer.BlockCopy(chains[c], 0, outPack, offsets[c], chains[c].Length);
			}
			return outPack;
		}

		/// <summary>A definition's steps as the pack's records: a record a step, the last END if the steps have none.</summary>
		private static byte[] Records(ModSummon s, Func<string, int?> effectCategory, List<string> problems)
		{
			List<byte> records = new List<byte>();
			bool ended = false;
			foreach (ModSummonStep step in s.Steps)
			{
				int op = CommandNumber(step.Do);
				if (op < 0) { problems?.Add(s.Id + ": no command '" + step.Do + "' - skipped"); continue; }
				byte[] r = new byte[Record];
				for (int i = 0; i < 7; i++)
				{
					int v = 0;
					JsonNode p = step.P[i];
					if (p is JsonValue pv)
					{
						if (pv.TryGetValue(out int n)) v = n;
						else if (pv.TryGetValue(out double d)) v = (int)Math.Round(d);
						else if (pv.TryGetValue(out string text))
						{
							// One of the mods' effects by its id, in a draw command's first place: its category.
							int? category = i == 0 && EffectCommands.Contains(op) ? effectCategory?.Invoke(text) : null;
							if (category == null && int.TryParse(text, out int parsed)) category = parsed;
							if (category == null) problems?.Add(s.Id + ": " + Commands[op] + " - no effect '" + text + "'");
							v = category ?? 0;
						}
					}
					BitConverter.GetBytes(v).CopyTo(r, 4 * i);
				}
				r[28] = (byte)(step.Again ? 1 : 0);
				r[29] = r[30] = r[31] = 1;
				BitConverter.GetBytes(op).CopyTo(r, 32);
				records.AddRange(r);
				if (op == 44) ended = true;
			}
			if (!ended)
			{
				// Without an END the battle would run off the list: one is put last.
				byte[] r = new byte[Record];
				r[29] = r[30] = r[31] = 1;
				BitConverter.GetBytes(44).CopyTo(r, 32);
				records.AddRange(r);
				problems?.Add(s.Id + ": no SUMMON_BEHAVIOR_END - one put last");
			}
			return records.ToArray();
		}
	}
}
