// A mod's characters: definitions as data (defs/characters/<id>.json), the first slice.
//
// FF3 has four heroes in fixed slots (0..3: Luneth, Arc, Refia, Ingus), named in the code
// (pl.getPlayerInitialName), each a Freelancer at level 1 when a game begins, changing job
// through the crystals' job system; their models are j<slot+1><job+1> by job. A definition
// says what a slot's hero is when a game begins: the name (the name entry still lets the
// player change it), the job, the level. The client applies them where the game sets the
// party up - the boot and the title's New Game - and a save loaded after that carries its
// own, as it always did.
//
//   { "id": "luneth", "slot": 0, "name": "Luneth", "job": "knight", "level": 5,
//     "fixedJob": true, "look": 2 }
//
// fixedJob keeps the hero in its job - the job menu beeps at a change, as it does for a job
// not yet won - the first step toward a character with a class of its own rather than the
// job system. look is which hero's model set the character wears, 0..3 (Luneth's, Arc's,
// Refia's, Ingus's: j<look+1><job+1> on the field, in battle, in the menus), so a definition
// can give slot 0 Refia's figures; every job has a model in every set, so nothing is
// missing anywhere. A model of the character's own comes later: an NPC model has no job
// figures and no battle motions to stand in with.
//
// Jobs by the game's own enum names or the English ones (pl.JOB_TYPE: freelancer/suppinn,
// onion-knight, warrior/fighter, monk, white-mage, black-mage, red-mage, ranger/hunter,
// knight, thief, scholar/book-man, geomancer, dragoon/dragon-knight, viking, dark-knight/
// evil-sworder, evoker/phantomer, bard, black-belt/karate-master, devout/imam, magus/
// devil-man, summoner/devildom-phantomer, sage, ninja) or the number.
//
// progression says how the hero grows - which game's system it plays by:
//
//   "jobs"     FF3's: any won job, changed freely, the job's own commands; the default.
//   "class"    FF4's: one class for good (the job is fixed) and a list of what arrives by
//              level - "learn": [{ "level": 5, "spell": "Cure" }, ...] equips the spell when
//              the level is reached (a spell by name or item id).
//   "mastery"  FF5's: jobs changed freely, each job climbing its own ladder of abilities on
//              ABP won in battle (defs/jobs/<id>.json, ModJobs.cs); a learned ability goes into
//              a free command slot of any job; no penalty time on a change.
//
// The games' names are taken too (ff3, ff4, ff5). What is not here yet: a model of the
// character's own, new slots beyond the four.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenFF.Data
{
	internal sealed class ModCharacter
	{
		public string Id;
		/// <summary>The hero slot, 0..3.</summary>
		public int Slot;
		public string Name;
		/// <summary>The starting job, by name or number; null keeps the game's (Freelancer).</summary>
		public string Job;
		/// <summary>The starting level, 1..99; 0 keeps the game's (1).</summary>
		public int Level;
		/// <summary>The hero keeps its job: the job menu refuses a change.</summary>
		public bool FixedJob;
		/// <summary>Which hero's model set it wears, 0..3; -1 its own slot's.</summary>
		public int Look = -1;
		/// <summary>How the hero grows: Jobs (FF3), Class (FF4), Mastery (FF5).</summary>
		public Progression Progression = Progression.Jobs;
		/// <summary>Class: what arrives by level (spells by name or item id).</summary>
		public List<ModLearned> Learn = new List<ModLearned>();
		public string Source;

		/// <summary>Whether the job cannot be changed: said outright, or implied by a class.</summary>
		public bool JobIsFixed => FixedJob || Progression == Progression.Class;

		public static ModCharacter Parse(string json, string source = null)
		{
			JsonNode node = JsonNode.Parse(json);
			if (node == null) return null;
			ModCharacter c = new ModCharacter
			{
				Id = node["id"]?.GetValue<string>(),
				Slot = node["slot"]?.GetValue<int>() ?? 0,
				Name = node["name"]?.GetValue<string>(),
				Job = node["job"] is JsonValue j ? (j.TryGetValue(out int n) ? n.ToString() : j.GetValue<string>()) : null,
				Level = node["level"]?.GetValue<int>() ?? 0,
				FixedJob = node["fixedJob"]?.GetValue<bool>() ?? false,
				Look = node["look"]?.GetValue<int>() ?? -1,
				Progression = Progressions.Parse(node["progression"]?.GetValue<string>()),
				Source = source
			};
			if (node["learn"] is JsonArray learn)
			{
				foreach (JsonNode l in learn)
				{
					if (l == null) continue;
					JsonNode spell = l["spell"];
					c.Learn.Add(new ModLearned
					{
						Level = l["level"]?.GetValue<int>() ?? 1,
						Spell = spell is JsonValue sv ? (sv.TryGetValue(out int sid) ? sid.ToString() : sv.GetValue<string>()) : null
					});
				}
			}
			return c;
		}

		public string ToJson()
		{
			JsonObject node = new JsonObject { ["id"] = Id, ["slot"] = Slot, ["name"] = Name ?? "" };
			if (!string.IsNullOrEmpty(Job)) node["job"] = Job;
			if (Level > 0) node["level"] = Level;
			if (Progression != Progression.Jobs) node["progression"] = Progressions.Word(Progression);
			if (FixedJob && Progression != Progression.Class) node["fixedJob"] = true;
			if (Look >= 0) node["look"] = Look;
			if (Learn.Count > 0)
			{
				JsonArray learn = new JsonArray();
				foreach (ModLearned l in Learn)
				{
					if (string.IsNullOrWhiteSpace(l.Spell)) continue;
					JsonObject o = new JsonObject { ["level"] = l.Level };
					o["spell"] = int.TryParse(l.Spell, out int id) ? JsonValue.Create(id) : JsonValue.Create(l.Spell);
					learn.Add(o);
				}
				node["learn"] = learn;
			}
			return node.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
		}
	}

	/// <summary>Which game's growth a hero plays by.</summary>
	internal enum Progression
	{
		/// <summary>FF3: the job system, any won job at any time, the job's own commands.</summary>
		Jobs,
		/// <summary>FF4: one class for good, learning by level.</summary>
		Class,
		/// <summary>FF5: jobs with ability ladders climbed on ABP; learned abilities slotted into any job.</summary>
		Mastery
	}

	/// <summary>Something a class learns at a level: a spell (by name or item id).</summary>
	internal sealed class ModLearned
	{
		public int Level = 1;
		public string Spell;
	}

	internal static class Progressions
	{
		/// <summary>A definition's word for a progression: the system's name or the game's (jobs/ff3, class/ff4, mastery/ff5); Jobs when it says nothing.</summary>
		public static Progression Parse(string word)
		{
			switch (ModCharacters.Slug(word))
			{
				case "class": case "ff4": case "fixed": return Progression.Class;
				case "mastery": case "ff5": case "abilities": return Progression.Mastery;
				default: return Progression.Jobs;
			}
		}

		public static string Word(Progression p) => p == Progression.Class ? "class" : p == Progression.Mastery ? "mastery" : "jobs";

		/// <summary>The game the system comes from, for the editor and the log.</summary>
		public static string Game(Progression p) => p == Progression.Class ? "FF4" : p == Progression.Mastery ? "FF5" : "FF3";
	}

	internal static class ModCharacters
	{
		public const string Folder = "defs/characters";

		/// <summary>FF3's jobs in pl.JOB_TYPE order: the enum's name, then the English name, both accepted in a definition.</summary>
		public static readonly (string Enum, string Name)[] Jobs =
		{
			("suppinn", "Freelancer"), ("onion-sworder", "Onion Knight"), ("fighter", "Warrior"), ("monk", "Monk"),
			("white-magician", "White Mage"), ("black-magician", "Black Mage"), ("red-magician", "Red Mage"), ("hunter", "Ranger"),
			("knight", "Knight"), ("thief", "Thief"), ("book-man", "Scholar"), ("geomancer", "Geomancer"),
			("dragon-knight", "Dragoon"), ("viking", "Viking"), ("evil-sowrder", "Dark Knight"), ("phantomer", "Evoker"),
			("bard", "Bard"), ("karate-master", "Black Belt"), ("imam", "Devout"), ("devil-man", "Magus"),
			("devildom-phantomer", "Summoner"), ("sage", "Sage"), ("ninja", "Ninja"),
		};

		/// <summary>A job's number from a definition's word (either name, a slug of it, or a number); -1 for none.</summary>
		public static int JobNumber(string job)
		{
			if (string.IsNullOrWhiteSpace(job)) return -1;
			string s = job.Trim();
			if (int.TryParse(s, out int n)) return n >= 0 && n < Jobs.Length ? n : -1;
			string slug = Slug(s);
			for (int i = 0; i < Jobs.Length; i++)
			{
				if (slug == Jobs[i].Enum || slug == Slug(Jobs[i].Name)) return i;
			}
			// A misspelling the enum carries: "evil-sworder" for the code's EVIL_SOWRDER.
			if (slug == "evil-sworder" || slug == "dark-knight") return 14;
			return -1;
		}

		public static string Slug(string s) => new string((s ?? "").Trim().ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray()).Trim('-').Replace("--", "-");

		public static List<ModCharacter> Load(IEnumerable<string> roots, List<string> notes = null)
		{
			List<ModCharacter> all = new List<ModCharacter>();
			foreach (string root in roots ?? Enumerable.Empty<string>())
			{
				string directory = Path.Combine(root, Folder.Replace('/', Path.DirectorySeparatorChar));
				if (!Directory.Exists(directory)) continue;
				foreach (string file in Directory.EnumerateFiles(directory, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
				{
					try
					{
						ModCharacter c = ModCharacter.Parse(File.ReadAllText(file), file);
						if (c == null) continue;
						if (string.IsNullOrWhiteSpace(c.Id)) c.Id = Path.GetFileNameWithoutExtension(file);
						if (c.Slot < 0 || c.Slot > 3) { notes?.Add(file + ": slot " + c.Slot + " - FF3 has heroes 0..3"); continue; }
						if (!string.IsNullOrEmpty(c.Job) && JobNumber(c.Job) < 0) { notes?.Add(file + ": no job called '" + c.Job + "'"); c.Job = null; }
						if (c.Level < 0 || c.Level > 99) { notes?.Add(file + ": level " + c.Level + " - 1..99"); c.Level = 0; }
						if (c.Look > 3) { notes?.Add(file + ": look " + c.Look + " - a hero's model set is 0..3"); c.Look = -1; }
						if (c.Learn.Count > 0 && c.Progression != Progression.Class) notes?.Add(file + ": learn is for a class (progression \"class\"); a " + Progressions.Word(c.Progression) + " hero learns nothing by level");
						c.Learn.RemoveAll(l => string.IsNullOrWhiteSpace(l.Spell));
						foreach (ModLearned l in c.Learn) l.Level = Math.Clamp(l.Level, 1, 99);
						all.Add(c);
					}
					catch (Exception ex) { notes?.Add(file + ": " + ex.Message); }
				}
			}
			return all;
		}
	}
}
