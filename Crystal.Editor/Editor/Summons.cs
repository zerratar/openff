// FF3's summons as the battle plays them: files/summon_script_command.pack, a chain pack of 24 scripts - eight
// summons (by the spell's level, 0 Chocobo ... 7 Bahamut) times three outcomes (0 white, 1 black, 2 combine; chain
// type + 3 * level, btl.SummonDataManager.load) - each a list of 36-byte records: seven s32 parameters, a byte
// `again` (run in the same step as the one before), three bytes of padding and the s32 command
// (btl.SUMMON_BEHAVIOR; btl.CommandParameter.parse). btl.BaseSummon.run steps through them, btl.SummonCommand
// does each. Crystal's summon preview (wwwroot/summons.js) plays them on the effect Stage with the same rules.
//
// Server: /api/summons (the eight and their outcomes), /api/summon?level=&type= (one script, and what playing it
// needs: the summon's model and motions, its monster's offsets, the spells its steps draw).

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenFF.Content;
using OpenFF.Data;

namespace Crystal.Editor
{
	internal static class Summons
	{
		public const string Pack = "summon_script_command.pack";
		public const int Record = 36;

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

		public static readonly string[] Outcomes = { "white", "black", "combine" };

		private static string Named(Workspace workspace, string file)
		{
			string direct = "files/" + file;
			if (workspace.Exists(direct)) return direct;
			return workspace.List(System.IO.Path.GetExtension(file)).Select(e => e.Name).FirstOrDefault(n => n.EndsWith("/" + file, StringComparison.OrdinalIgnoreCase) || n.Equals(file, StringComparison.OrdinalIgnoreCase));
		}

		private static List<int[]>[] Read(Workspace workspace)
		{
			string name = Named(workspace, Pack) ?? throw new InvalidOperationException("no " + Pack + " (FF3's summons) in this game");
			byte[] data = workspace.Read(name);
			int count = BitConverter.ToInt32(data, 0);
			List<int[]>[] chains = new List<int[]>[count];
			for (int c = 0; c < count; c++)
			{
				int offset = BitConverter.ToInt32(data, 16 + 8 * c), size = BitConverter.ToInt32(data, 20 + 8 * c);
				List<int[]> records = new List<int[]>();
				for (int at = offset; at + Record <= offset + size && at + Record <= data.Length; at += Record)
				{
					int[] r = new int[9];
					for (int p = 0; p < 7; p++) r[p] = BitConverter.ToInt32(data, at + 4 * p);
					r[7] = data[at + 28];
					r[8] = BitConverter.ToInt32(data, at + 32);
					records.Add(r);
				}
				chains[c] = records;
			}
			return chains;
		}

		private static Dictionary<uint, string> BattleMessages(Workspace workspace)
		{
			try
			{
				ContentChain chain = ContentChain.Around(workspace.Source, workspace.ContentDirectory, new[] { workspace.OverrideDirectory });
				return TableFiles.ReadNames(chain, "eureka_battle.msd", GameData.Tables(workspace)) ?? new Dictionary<uint, string>();
			}
			catch (Exception) { return new Dictionary<uint, string>(); }
		}

		/// <summary>The eight summons and their three outcomes, by the spells' and the battle messages' names.</summary>
		public static object List(Workspace workspace)
		{
			List<int[]>[] chains = Read(workspace);
			GameTables tables = GameData.Tables(workspace);
			Dictionary<uint, string> messages = BattleMessages(workspace);
			List<object> summons = new List<object>();
			for (int level = 0; level < 8; level++)
			{
				SpellDefinition spell = tables.Spell(4201 + level);
				// The creature: the monster its first outcome's script makes it (SET_SUMMON_PARAMETER), by that monster's name.
				string creature = null;
				int monster = level * 3 < chains.Length ? chains[level * 3].Where(r => r[8] == 28).Select(r => r[0]).FirstOrDefault() : 0;
				if (monster > 0) creature = tables.Monsters.FirstOrDefault(m => m.Id == monster)?.Name?.TrimStart('!', ' ');
				summons.Add(new
				{
					level,
					name = spell?.Name ?? ("summon " + (level + 1)),
					creature,
					outcomes = Enumerable.Range(0, 3).Select(type => new
					{
						type,
						kind = Outcomes[type],
						name = messages.TryGetValue((uint)(1300 + level * 10 + type + 1), out string n) && !string.IsNullOrWhiteSpace(n) ? n.Trim() : Outcomes[type],
						steps = type + 3 * level < chains.Length ? chains[type + 3 * level].Count : 0,
					}).ToList(),
				});
			}
			return new { ok = true, summons };
		}

		/// <summary>The project's own script for a summon's outcome (defs/summons), if it has one: its file and definition.</summary>
		public static (string File, JsonObject Def) Mine(Project project, int level, int type)
		{
			if (project == null) return (null, null);
			string folder = Path.Combine(project.Directory, ModSummons.Folder);
			if (!Directory.Exists(folder)) return (null, null);
			(string, JsonObject) found = (null, null);
			foreach (string f in Directory.EnumerateFiles(folder, "*.json").OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
			{
				try
				{
					JsonObject o = JsonNode.Parse(File.ReadAllText(f)) as JsonObject;
					if (o == null) continue;
					if (ModSummons.Level(o["summon"]?.ToString()) == level && ModSummons.OutcomeNumber(o["outcome"]) == type) found = (ModSummons.Folder + "/" + Path.GetFileName(f), o);
				}
				catch (Exception) { }
			}
			return found;
		}

		/// <summary>A summon's outcome copied into the mod: the game's script as steps (defs/summons/&lt;creature&gt;-&lt;outcome&gt;.json).</summary>
		public static object Copy(Project project, Workspace workspace, int level, int type)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			List<int[]>[] chains = Read(workspace);
			List<int[]> records = chains[type + 3 * level];
			JsonArray steps = new JsonArray();
			foreach (int[] r in records)
			{
				int used = 7;
				while (used > 0 && r[used - 1] == 0) used--;
				JsonObject step = new JsonObject { ["do"] = r[8] >= 0 && r[8] < ModSummons.Commands.Length ? ModSummons.Commands[r[8]] : r[8].ToString(CultureInfo.InvariantCulture) };
				if (used > 0) step["p"] = new JsonArray(r.Take(used).Select(v => (JsonNode)v).ToArray());
				if (r[7] != 0) step["again"] = true;
				steps.Add(step);
			}
			JsonObject def = new JsonObject { ["summon"] = ModSummons.Names[level].Creature, ["outcome"] = ModSummons.Outcomes[type], ["steps"] = steps };
			string folder = Path.Combine(project.Directory, ModSummons.Folder);
			Directory.CreateDirectory(folder);
			string name = (ModSummons.Names[level].Creature + "-" + ModSummons.Outcomes[type]).ToLowerInvariant();
			File.WriteAllText(Path.Combine(folder, name + ".json"), Text(def), new UTF8Encoding(false));
			return new { ok = true, file = ModSummons.Folder + "/" + name + ".json", def };
		}

		public static object Save(Project project, string file, JsonNode def)
		{
			string path = PathOf(project, file);
			if (def is not JsonObject) throw new ArgumentException("a summon is an object");
			File.WriteAllText(path, Text(def), new UTF8Encoding(false));
			return new { ok = true, file };
		}

		public static object Delete(Project project, string file)
		{
			string path = PathOf(project, file);
			if (File.Exists(path)) File.Delete(path);
			return new { ok = true };
		}

		private static string PathOf(Project project, string file)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			string name = Path.GetFileName(file ?? "");
			if (!name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("a summon is a .json of defs/summons");
			string folder = Path.Combine(project.Directory, ModSummons.Folder);
			Directory.CreateDirectory(folder);
			return Path.Combine(folder, name);
		}

		/// <summary>A definition as its file has it: indented, a step's numbers on one line.</summary>
		private static string Text(JsonNode def) => EffectsProject.Text(def);

		/// <summary>One summon's script and what playing it needs.</summary>
		public static object Script(Workspace workspace, int level, int type, Project project = null)
		{
			List<int[]>[] chains = Read(workspace);
			int index = type + 3 * level;
			if (level < 0 || level > 7 || type < 0 || type > 2 || index >= chains.Length) throw new ArgumentException("no summon " + level + "/" + type);
			List<int[]> records = chains[index];
			GameTables tables = GameData.Tables(workspace);
			Dictionary<uint, string> messages = BattleMessages(workspace);

			// The summon: its model (SET_MODEL f###), motions (SET_MOTION b_sm###) and monster (SET_SUMMON_PARAMETER): its size and hit point.
			int model = records.Where(r => r[8] == 8).Select(r => r[0]).FirstOrDefault();
			int motion = records.Where(r => r[8] == 10).Select(r => r[0]).FirstOrDefault();
			int monster = records.Where(r => r[8] == 28).Select(r => r[0]).FirstOrDefault();
			object target = null;
			try { if (monster > 0) target = Effects.Target(workspace, monster); } catch (Exception) { }

			// The spells DRAW_TARGET_EFFECT plays: their effect, how long before the damage, and on which side.
			Dictionary<int, (int Category, int Member, int Frame, int Offset)> records2 = SpellRecords(workspace);
			var spells = records.Where(r => r[8] == 52).Select(r => r[0]).Distinct().Select(id =>
			{
				records2.TryGetValue(id, out var rec);
				SpellDefinition s = tables.Spell(id);
				// On the party: a recovery (Curaja), or the summons' own shields (Reflect's 4021, Bahamut's Aura 4208).
				bool party = (s != null && s.UseKind == 1) || id == 4021 || id == 4208;
				return new { id, name = s?.Name, category = rec.Category, member = rec.Member, frame = rec.Frame, offset = rec.Offset, party };
			}).ToList();

			(string mineFile, JsonObject mineDef) = Mine(project, level, type);
			return new
			{
				ok = true,
				level, type,
				mine = mineFile == null ? null : new { file = mineFile, def = mineDef },
				summon = tables.Spell(4201 + level)?.Name,
				name = messages.TryGetValue((uint)(1300 + level * 10 + type + 1), out string n) ? n?.Trim() : Outcomes[type],
				model = model > 0 ? "files/f" + model.ToString("000", CultureInfo.InvariantCulture) + ".nmdp.lz" : null,
				motions = motion > 0 ? "b_sm" + motion.ToString("000", CultureInfo.InvariantCulture) : null,
				monster, target, spells,
				steps = records.Select(r => new { op = r[8], name = r[8] >= 0 && r[8] < Commands.Length ? Commands[r[8]] : "?" + r[8], p = r.Take(7).ToArray(), again = r[7] != 0 }).ToList(),
			};
		}

		/// <summary>player.chaindata's effect table: spell id -> the effect's category and member, its play frame and offset.</summary>
		private static Dictionary<int, (int, int, int, int)> SpellRecords(Workspace workspace)
		{
			Dictionary<int, (int, int, int, int)> found = new Dictionary<int, (int, int, int, int)>();
			try
			{
				string name = Named(workspace, "player.chaindata");
				ChainPack chain = ChainPack.Read(workspace.Read(name));
				int n = chain.Records(ModSpells.Chain, ModSpells.Stride);
				for (int i = 0; i < n; i++)
				{
					byte[] r = chain.Record(ModSpells.Chain, ModSpells.Stride, i);
					int spell = ChainPack.S16(r, 0);
					if (!found.ContainsKey(spell)) found[spell] = (ChainPack.S16(r, 10), ChainPack.S16(r, 12), ChainPack.S16(r, 30), ChainPack.S16(r, 2));
				}
			}
			catch (Exception) { }
			return found;
		}
	}
}
