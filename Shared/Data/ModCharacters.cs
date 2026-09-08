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
//   { "id": "luneth", "slot": 0, "name": "Luneth", "job": "knight", "level": 5 }
//
// Jobs by the game's own enum names or the English ones (pl.JOB_TYPE: freelancer/suppinn,
// onion-knight, warrior/fighter, monk, white-mage, black-mage, red-mage, ranger/hunter,
// knight, thief, scholar/book-man, geomancer, dragoon/dragon-knight, viking, dark-knight/
// evil-sworder, evoker/phantomer, bard, black-belt/karate-master, devout/imam, magus/
// devil-man, summoner/devildom-phantomer, sage, ninja) or the number.
//
// What comes next on this line, and is not here yet: a progression per character (jobs, or
// a fixed class as FF4's), a model of the character's own, new slots beyond the four.

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
		public string Source;

		public static ModCharacter Parse(string json, string source = null)
		{
			JsonNode node = JsonNode.Parse(json);
			if (node == null) return null;
			return new ModCharacter
			{
				Id = node["id"]?.GetValue<string>(),
				Slot = node["slot"]?.GetValue<int>() ?? 0,
				Name = node["name"]?.GetValue<string>(),
				Job = node["job"] is JsonValue j ? (j.TryGetValue(out int n) ? n.ToString() : j.GetValue<string>()) : null,
				Level = node["level"]?.GetValue<int>() ?? 0,
				Source = source
			};
		}

		public string ToJson()
		{
			JsonObject node = new JsonObject { ["id"] = Id, ["slot"] = Slot, ["name"] = Name ?? "" };
			if (!string.IsNullOrEmpty(Job)) node["job"] = Job;
			if (Level > 0) node["level"] = Level;
			return node.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
		}
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
						all.Add(c);
					}
					catch (Exception ex) { notes?.Add(file + ": " + ex.Message); }
				}
			}
			return all;
		}
	}
}
