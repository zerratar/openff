// A project's character definitions: defs/characters/<id>.json (Shared/Data/ModCharacters.cs
// says what one is and how the client applies it). One per hero slot; the editor lists
// them, shows one as a form and writes it back.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenFF.Data;

namespace Crystal.Editor
{
	internal static class ProjectCharacters
	{
		public static readonly string[] Heroes = { "Luneth", "Arc", "Refia", "Ingus" };

		public static string Directory(Project project) => Path.Combine(project.Directory, "defs", "characters");

		public static List<ModCharacter> All(Project project, List<string> notes = null)
		{
			return project == null ? new List<ModCharacter>() : ModCharacters.Load(new[] { project.Directory }, notes);
		}

		public static object Describe(ModCharacter c)
		{
			int job = ModCharacters.JobNumber(c.Job);
			return new
			{
				id = c.Id, slot = c.Slot, hero = c.Slot >= 0 && c.Slot < Heroes.Length ? Heroes[c.Slot] : null,
				name = c.Name, job = job >= 0 ? (int?)job : null, jobName = job >= 0 ? ModCharacters.Jobs[job].Name : null,
				level = c.Level, fixedJob = c.FixedJob, look = c.Look,
				progression = Progressions.Word(c.Progression), progressionGame = Progressions.Game(c.Progression),
				learn = c.Learn.Select(l => new { level = l.Level, spell = l.Spell }).ToList(),
				file = "defs/characters/" + c.Id + ".json"
			};
		}

		/// <summary>The jobs for a picker: number, the enum's word, the English name.</summary>
		public static object Jobs() => ModCharacters.Jobs.Select((j, i) => new { number = i, word = j.Enum, name = j.Name }).ToList();

		public static string Save(Project project, ModCharacter c)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			if (string.IsNullOrWhiteSpace(c.Id)) throw new ArgumentException("a character needs an id");
			if (c.Slot < 0 || c.Slot > 3) throw new ArgumentException("the slot is one of the four heroes, 0..3");
			if (!string.IsNullOrEmpty(c.Job) && ModCharacters.JobNumber(c.Job) < 0) throw new ArgumentException("no job called '" + c.Job + "'");
			string directory = Directory(project);
			System.IO.Directory.CreateDirectory(directory);
			string path = Path.Combine(directory, c.Id + ".json");
			File.WriteAllText(path, c.ToJson(), new UTF8Encoding(false));
			return path;
		}

		public static ModCharacter New(Project project, string name, int slot)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			List<ModCharacter> all = All(project);
			if (all.Any(c => c.Slot == slot)) throw new ArgumentException("slot " + slot + " (" + Heroes[slot] + ") already has a definition: " + all.First(c => c.Slot == slot).Id);
			string id = ProjectItems.Slug(name);
			if (string.IsNullOrEmpty(id)) id = "hero-" + slot;
			string unique = id;
			for (int n = 2; all.Any(c => string.Equals(c.Id, unique, StringComparison.OrdinalIgnoreCase)) || File.Exists(Path.Combine(Directory(project), unique + ".json")); n++) unique = id + "-" + n;
			ModCharacter made = new ModCharacter { Id = unique, Slot = slot, Name = name?.Trim() ?? "" };
			Save(project, made);
			return made;
		}

		public static bool Delete(Project project, string id)
		{
			if (project == null || string.IsNullOrWhiteSpace(id) || id.IndexOfAny(new[] { '/', '\\', '.' }) >= 0) return false;
			string path = Path.Combine(Directory(project), id + ".json");
			if (!File.Exists(path)) return false;
			File.Delete(path);
			return true;
		}
	}
}
