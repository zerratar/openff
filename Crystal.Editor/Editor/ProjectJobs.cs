// A project's job ladders: defs/jobs/<id>.json (Shared/Data/ModJobs.cs says what one is and
// how the client plays it - the mastery progression, FF5's way). One per job; the editor
// lists them, shows one as a form and writes it back. The ability catalogue the form picks
// from is FF3's table (Ff3Abilities) plus the passives the project's own ladders name.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenFF.Data;

namespace Crystal.Editor
{
	internal static class ProjectJobs
	{
		public static string Directory(Project project) => Path.Combine(project.Directory, "defs", "jobs");

		public static List<ModJob> All(Project project, List<string> notes = null)
		{
			return project == null ? new List<ModJob>() : ModJobs.Load(new[] { project.Directory }, notes);
		}

		public static object Describe(ModJob j, IEnumerable<ModJob> all)
		{
			int job = j.IsOwn ? -1 : j.JobNumber;
			int[] layout = j.CommandIds();
			return new
			{
				id = j.Id, job = job >= 0 ? (int?)job : null, jobName = job >= 0 ? ModCharacters.Jobs[job].Name : null,
				own = j.IsOwn, number = j.JobNumber,
				@base = j.IsOwn && j.BaseJob >= 0 ? (int?)j.BaseJob : null, baseName = j.IsOwn && j.BaseJob >= 0 ? ModCharacters.Jobs[j.BaseJob].Name : null,
				look = j.IsOwn && !string.IsNullOrWhiteSpace(j.Look) && ModCharacters.JobNumber(j.Look) >= 0 ? (int?)ModCharacters.JobNumber(j.Look) : null,
				name = j.Name, inherits = j.Inherits,
				commands = j.Commands, innate = j.Innate,
				stats = ModJob.StatWords.Select((w, i) => new { word = w, value = j.Stats[i] }).ToList(),
				layout = layout.Select(c => c < 0 ? "*" : (Ff3Abilities.ById(c)?.Word ?? c.ToString())).ToList(),
				layoutNames = layout.Select(c => c < 0 ? "free slot" : (Ff3Abilities.ById(c)?.Name ?? c.ToString())).ToList(),
				freeSlots = j.FreeSlots,
				totalAbp = j.TotalAbp,
				abilities = j.Abilities.Select(a => new
				{
					abp = a.Abp, ability = a.Ability, name = a.Name, passive = a.Passive, grants = a.Grants, carries = a.Carries,
					id = a.Id, shownName = a.ShownName, isPassive = a.IsPassive,
					works = a.Id >= Ff3Abilities.FirstOwnId || (Ff3Abilities.ById(a.Id)?.Works ?? false)
				}).ToList(),
				file = "defs/jobs/" + j.Id + ".json"
			};
		}

		/// <summary>The abilities a ladder can name: FF3's (word, name, kind, whether the battle acts on it) and the project's own passives.</summary>
		public static object Catalogue(IEnumerable<ModJob> all)
		{
			List<object> list = new List<object>();
			foreach (Ff3Ability a in Ff3Abilities.All)
			{
				list.Add(new { id = a.Id, word = a.Word, name = a.Name, passive = a.Passive, works = a.Works, own = false });
			}
			HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (ModJob j in all)
			{
				foreach (ModJobAbility step in j.Abilities)
				{
					if (step.Id < Ff3Abilities.FirstOwnId || !seen.Add(ModCharacters.Slug(step.Ability))) continue;
					list.Add(new { id = step.Id, word = ModCharacters.Slug(step.Ability), name = step.ShownName, passive = true, works = true, own = true });
				}
			}
			return list;
		}

		/// <summary>The game's own four commands of every job, by word, for the form's defaults.</summary>
		public static object GameCommands() => Ff3Abilities.JobCommands.Select(c => c.Select(id => Ff3Abilities.ById(id)?.Word ?? id.ToString()).ToList()).ToList();

		public static string Save(Project project, ModJob j)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			if (string.IsNullOrWhiteSpace(j.Id)) throw new ArgumentException("a ladder needs an id");
			if (j.IsOwn && j.BaseJob < 0) throw new ArgumentException("a job of the mod's own stands on one of FF3's: base '" + j.Base + "' is none of them");
			if (!j.IsOwn && ModCharacters.JobNumber(j.Job) < 0) throw new ArgumentException("no job called '" + j.Job + "'");
			string directory = Directory(project);
			System.IO.Directory.CreateDirectory(directory);
			string path = Path.Combine(directory, j.Id + ".json");
			File.WriteAllText(path, j.ToJson(), new UTF8Encoding(false));
			return path;
		}

		/// <summary>A ladder for a job, with the game's own commands and the third made a free slot, no steps yet.</summary>
		public static ModJob New(Project project, int job)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			if (job < 0 || job >= ModCharacters.Jobs.Length) throw new ArgumentException("no such job");
			List<ModJob> all = All(project);
			if (all.Any(j => j.JobNumber == job)) throw new ArgumentException(ModCharacters.Jobs[job].Name + " already has a ladder: " + all.First(j => j.JobNumber == job).Id);
			string id = ModCharacters.Jobs[job].Enum;
			string unique = id;
			for (int n = 2; File.Exists(Path.Combine(Directory(project), unique + ".json")); n++) unique = id + "-" + n;
			ModJob made = new ModJob { Id = unique, Job = ModCharacters.Jobs[job].Enum, Name = ModCharacters.Jobs[job].Name };
			made.Commands = Ff3Abilities.JobCommands[job].Select((c, i) => i == 2 ? "*" : Ff3Abilities.ById(c)?.Word ?? c.ToString()).ToList();
			if (job == 0) { made.Commands[1] = "*"; made.Inherits = true; }   // Freelancer: two free slots, the mastered jobs' passives
			Save(project, made);
			return made;
		}

		/// <summary>A job of the mod's own on an FF3 base: named, the base's commands with the third freed, no steps yet.</summary>
		public static ModJob NewOwn(Project project, string name, int baseJob, int look)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("a job of the mod's own needs a name");
			if (baseJob < 0 || baseJob >= ModCharacters.Jobs.Length) throw new ArgumentException("the base is one of FF3's jobs");
			if (ModCharacters.JobNumber(name) >= 0) throw new ArgumentException("'" + name + "' is one of FF3's jobs; give it a ladder instead");
			string id = ProjectItems.Slug(name);
			if (string.IsNullOrEmpty(id)) id = "job";
			string unique = id;
			for (int n = 2; File.Exists(Path.Combine(Directory(project), unique + ".json")); n++) unique = id + "-" + n;
			ModJob made = new ModJob { Id = unique, Base = ModCharacters.Jobs[baseJob].Enum, Name = name.Trim() };
			if (look >= 0 && look < ModCharacters.Jobs.Length && look != baseJob) made.Look = ModCharacters.Jobs[look].Enum;
			made.Commands = Ff3Abilities.JobCommands[baseJob].Select((c, i) => i == 2 ? "*" : Ff3Abilities.ById(c)?.Word ?? c.ToString()).ToList();
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
