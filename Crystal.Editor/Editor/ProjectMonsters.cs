// The project's monsters (defs/monsters/<id>.json) and formations (defs/formations/<id>.json):
// Shared/Data/ModMonsters.cs has the format and the composition into the game's tables. Here
// what the editor needs - the list, one described beside the base's record (every field with
// the base's value greyed in), new, save, delete - and the game's own monsters and parties
// for the pickers.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenFF.Data;

namespace Crystal.Editor
{
	internal static class ProjectMonsters
	{
		public static string Directory(Project project) => Path.Combine(project.Directory, "defs", "monsters");
		public static string FormationsDirectory(Project project) => Path.Combine(project.Directory, "defs", "formations");

		public static List<ModMonster> All(Project project, List<string> notes = null)
		{
			return project == null ? new List<ModMonster>() : ModMonsters.Load(new[] { project.Directory }, notes);
		}

		public static List<ModFormation> AllFormations(Project project, List<string> notes = null)
		{
			return project == null ? new List<ModFormation>() : ModMonsters.LoadFormations(new[] { project.Directory }, notes);
		}

		/// <summary>The shipped monster.chaindata as a chain pack; null when the workspace has none.</summary>
		public static ChainPack Pack(Workspace workspace)
		{
			if (workspace == null) return null;
			byte[] data = null;
			foreach (string name in new[] { "files/monster.chaindata", "monster.chaindata" })
			{
				try { data = workspace.Read(name); } catch (Exception) { data = null; }
				if (data != null) break;
			}
			if (data == null) return null;
			try { return ChainPack.Read(data); } catch (Exception) { return null; }
		}

		/// <summary>The base monster's record, or null when the id is no monster.</summary>
		public static byte[] BaseRecord(Workspace workspace, int baseId)
		{
			ChainPack pack = Pack(workspace);
			return pack == null ? null : ModMonsters.RecordOf(pack, baseId);
		}

		/// <summary>The game's monsters by id, for names and families.</summary>
		private static Dictionary<int, MonsterDefinition> GameMonsters(Workspace workspace)
		{
			Dictionary<int, MonsterDefinition> map = new Dictionary<int, MonsterDefinition>();
			try
			{
				foreach (MonsterDefinition m in GameData.Tables(workspace).Monsters) if (!map.ContainsKey(m.Id)) map[m.Id] = m;
			}
			catch (Exception) { }
			return map;
		}

		/// <summary>What the editor shows for one definition: the base's name and family, and every field with the base's value beside the set one.</summary>
		public static object Describe(Workspace workspace, ModMonster m)
		{
			byte[] record = BaseRecord(workspace, m.Base);
			Dictionary<int, MonsterDefinition> game = GameMonsters(workspace);
			List<object> fields = new List<object>();
			if (record != null)
			{
				foreach (string name in ModMonsters.FieldNames())
				{
					int? value = ModMonsters.Get(record, name);
					fields.Add(new { name, group = ModMonsters.GroupOf(name), baseValue = value, value = m.Fields.TryGetValue(name, out int set) ? (int?)set : null });
				}
			}
			int family = record != null ? ChainPack.S16(record, 4) : -1;
			game.TryGetValue(m.Base, out MonsterDefinition baseMonster);
			// The family's other monsters: the textures a `look` may borrow.
			List<object> family_ = game.Values.Where(x => x.Family == family).OrderBy(x => x.Id).Select(x => new { id = x.Id, name = x.Name }).ToList<object>();
			return new
			{
				id = m.Id, number = m.Number, @base = m.Base, baseName = baseMonster?.Name, family,
				name = m.Name, look = m.Look, lookName = m.Look > 0 && game.TryGetValue(m.Look, out MonsterDefinition l) ? l.Name : null,
				familyMonsters = family_,
				fields,
				file = "defs/monsters/" + m.Id + ".json"
			};
		}

		/// <summary>A formation as the editor shows it: each slot with the monster's name (the game's or the mod's).</summary>
		public static object DescribeFormation(Workspace workspace, Project project, ModFormation f)
		{
			Dictionary<int, string> names = MonsterNames(workspace, project);
			return new
			{
				id = f.Id, number = f.Number, name = f.Name,
				slots = f.Slots.Select(s => new { monster = s.Monster, monsterName = names.TryGetValue(s.Monster, out string n) ? n : null, min = s.Min, max = s.Max }).ToList(),
				file = "defs/formations/" + f.Id + ".json"
			};
		}

		/// <summary>The game's monsters and the project's, by id, for a picker.</summary>
		public static Dictionary<int, string> MonsterNames(Workspace workspace, Project project)
		{
			Dictionary<int, string> names = new Dictionary<int, string>();
			foreach (KeyValuePair<int, MonsterDefinition> pair in GameMonsters(workspace)) names[pair.Key] = pair.Value.Name ?? ("monster " + pair.Key);
			foreach (ModMonster m in All(project)) if (!names.ContainsKey(m.Number)) names[m.Number] = (m.Name ?? m.Id) + " (mod)";
			return names;
		}

		/// <summary>The game's monsters then the mod's, for the pickers: id, name, family, level, hp.</summary>
		public static List<object> Monsters(Workspace workspace, Project project)
		{
			List<object> list = new List<object>();
			HashSet<int> seen = new HashSet<int>();
			foreach (MonsterDefinition m in GameMonsters(workspace).Values.OrderBy(x => x.Id))
			{
				if (!seen.Add(m.Id)) continue;
				list.Add(new { id = m.Id, name = m.Name ?? ("monster " + m.Id), family = m.Family, level = m.Level, hp = m.MaxHp, mod = false });
			}
			foreach (ModMonster m in All(project))
			{
				if (!seen.Add(m.Number)) continue;
				byte[] record = BaseRecord(workspace, m.Base);
				list.Add(new
				{
					id = m.Number, name = m.Name ?? m.Id,
					family = record != null ? ChainPack.S16(record, 4) : -1,
					level = m.Fields.TryGetValue("level", out int level) ? level : (record != null ? record[0xA] : 0),
					hp = m.Fields.TryGetValue("maxHp", out int hp) ? hp : (record != null ? ChainPack.S32(record, 0xC) : 0),
					mod = true
				});
			}
			return list;
		}

		/// <summary>The game's monster parties then the mod's formations, for the [FormationField] picker: id, a label of the monsters, mod flag.</summary>
		public static List<object> Formations(Workspace workspace, Project project)
		{
			Dictionary<int, string> names = MonsterNames(workspace, project);
			List<object> list = new List<object>();
			HashSet<int> seen = new HashSet<int>();
			try
			{
				foreach (MonsterParty p in GameData.Tables(workspace).MonsterParties.OrderBy(p => p.Id))
				{
					if (!seen.Add(p.Id) || p.Slots.Count == 0) continue;
					string label = string.Join(", ", p.Slots.Select(s => (names.TryGetValue(s.MonsterId, out string n) ? n : "monster " + s.MonsterId) + (s.Count > 1 ? " x" + s.Count : "")));
					list.Add(new { id = p.Id, name = label, mod = false });
				}
			}
			catch (Exception) { }
			foreach (ModFormation f in AllFormations(project))
			{
				if (!seen.Add(f.Number)) continue;
				string monsters = string.Join(", ", f.Slots.Select(s => (names.TryGetValue(s.Monster, out string n) ? n : "monster " + s.Monster) + (s.Max > 1 ? " x" + s.Max : "")));
				list.Add(new { id = f.Number, name = (string.IsNullOrWhiteSpace(f.Name) ? f.Id : f.Name) + " - " + monsters, mod = true });
			}
			return list;
		}

		public static string Save(Project project, ModMonster m)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			if (string.IsNullOrWhiteSpace(m.Id)) throw new ArgumentException("a monster needs an id");
			if (m.Number <= 0) throw new ArgumentException("a monster needs a number");
			if (m.Base < 0) throw new ArgumentException("a monster needs a base monster");
			System.IO.Directory.CreateDirectory(Directory(project));
			string path = Path.Combine(Directory(project), m.Id + ".json");
			File.WriteAllText(path, m.ToJson(), new UTF8Encoding(false));
			return path;
		}

		public static string SaveFormation(Project project, ModFormation f)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			if (string.IsNullOrWhiteSpace(f.Id)) throw new ArgumentException("a formation needs an id");
			if (f.Number <= 0) throw new ArgumentException("a formation needs a number");
			System.IO.Directory.CreateDirectory(FormationsDirectory(project));
			string path = Path.Combine(FormationsDirectory(project), f.Id + ".json");
			File.WriteAllText(path, f.ToJson(), new UTF8Encoding(false));
			return path;
		}

		public static ModMonster New(Project project, string name, int baseId)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			List<ModMonster> all = All(project);
			string id = Unique(ProjectItems.Slug(name), "monster", all.Select(m => m.Id), Directory(project));
			ModMonster m = new ModMonster { Id = id, Number = ModMonsters.NextNumber(all), Base = baseId, Name = name?.Trim() ?? "" };
			Save(project, m);
			return m;
		}

		public static ModFormation NewFormation(Project project, string name, IEnumerable<int> monsters)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			List<ModFormation> all = AllFormations(project);
			string id = Unique(ProjectItems.Slug(name), "formation", all.Select(f => f.Id), FormationsDirectory(project));
			ModFormation f = new ModFormation { Id = id, Number = ModMonsters.NextFormationNumber(all), Name = name?.Trim() ?? "" };
			foreach (int monster in (monsters ?? Enumerable.Empty<int>()).Take(ModFormation.SlotCount)) f.Slots.Add(new ModFormationSlot { Monster = monster, Min = 1, Max = 1 });
			SaveFormation(project, f);
			return f;
		}

		private static string Unique(string slug, string fallback, IEnumerable<string> taken, string directory)
		{
			string id = string.IsNullOrEmpty(slug) ? fallback : slug;
			HashSet<string> have = new HashSet<string>(taken, StringComparer.OrdinalIgnoreCase);
			string unique = id;
			for (int n = 2; have.Contains(unique) || File.Exists(Path.Combine(directory, unique + ".json")); n++) unique = id + "-" + n;
			return unique;
		}

		public static bool Delete(Project project, string id) => DeleteIn(Directory(project), id);
		public static bool DeleteFormation(Project project, string id) => DeleteIn(FormationsDirectory(project), id);

		private static bool DeleteIn(string directory, string id)
		{
			if (string.IsNullOrWhiteSpace(id) || id.IndexOfAny(new[] { '/', '\\', '.' }) >= 0) return false;
			string path = Path.Combine(directory, id + ".json");
			if (!File.Exists(path)) return false;
			File.Delete(path);
			return true;
		}

		/// <summary>
		/// The Steam side, as ProjectItems.WriteTables: the shipped monster.chaindata, every
		/// language's eureka_battle.msd and monster_party_table.bbd composed with the project's
		/// definitions and written into the target's files/. Returns what was written.
		/// </summary>
		public static List<string> WriteTables(Project project, Workspace workspace, string target)
		{
			List<string> written = new List<string>();
			if (project == null || workspace == null || !string.Equals(Targets.GameOf(target), "ff3", StringComparison.OrdinalIgnoreCase)) return written;
			List<ModMonster> monsters = All(project);
			List<ModFormation> formations = AllFormations(project);
			if (monsters.Count == 0 && formations.Count == 0) return written;
			string files = project.FilesFor(target);
			List<(string name, Func<byte[], byte[]> compose)> jobs = new List<(string, Func<byte[], byte[]>)>();
			if (monsters.Count > 0)
			{
				jobs.Add(("files/monster.chaindata", data => ModMonsters.ComposeChain(data, monsters)));
				foreach (string msd in workspace.List(".msd").Select(e => e.Name).Where(ModMonsters.IsMsd))
					jobs.Add((msd.StartsWith("files/", StringComparison.OrdinalIgnoreCase) ? msd : "files/" + msd, data => ModMonsters.ComposeMsd(data, monsters)));
			}
			if (formations.Count > 0) jobs.Add(("files/monster_party_table.bbd", data => ModMonsters.ComposeParties(data, formations)));
			foreach ((string name, Func<byte[], byte[]> compose) in jobs)
			{
				byte[] shipped = workspace.ReadShipped(name);
				if (shipped == null) continue;
				byte[] composed = compose(shipped);
				if (composed == null || ReferenceEquals(composed, shipped)) continue;
				string path = Path.Combine(files, name.Replace('/', Path.DirectorySeparatorChar));
				System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllBytes(path, composed);
				written.Add(name);
			}
			return written;
		}
	}
}
