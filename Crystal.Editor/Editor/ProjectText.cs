// The mod's text: defs/text/<name>.json files of message id -> line (Shared/Data/ModText.cs
// has the format and the composition). The client composes them into eureka_permanent.msd
// as it reads it; a Steam target gets the composed file written under files/ at Install
// and Export, like the item tables. Here: the list of files with their line counts, a new
// file with the next free id, and every line by id for the editor's "@id" hints.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenFF.Data;

namespace Crystal.Editor
{
	internal static class ProjectText
	{
		public static string Directory(Project project) => Path.Combine(project.Directory, "defs", "text");

		/// <summary>Every line of the project's text by id; the first file's (by name) wins a clash.</summary>
		public static Dictionary<uint, string> Lines(Project project, List<string> notes = null)
		{
			return project == null ? new Dictionary<uint, string>() : ModText.Load(new[] { project.Directory }, notes);
		}

		/// <summary>The files under defs/text, each with its line count and its first id.</summary>
		public static List<object> Files(Project project)
		{
			List<object> files = new List<object>();
			string directory = Directory(project);
			if (!System.IO.Directory.Exists(directory)) return files;
			foreach (string file in System.IO.Directory.EnumerateFiles(directory, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
			{
				Dictionary<uint, string> lines = new Dictionary<uint, string>();
				List<string> notes = new List<string>();
				try
				{
					JsonNode node = JsonNode.Parse(File.ReadAllText(file));
					if (node is JsonObject o)
					{
						foreach (KeyValuePair<string, JsonNode> pair in o)
							if (uint.TryParse(pair.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint id) && pair.Value is JsonValue v && v.TryGetValue(out string text)) lines[id] = text;
					}
					else if (node is JsonArray a)
					{
						foreach (JsonNode item in a)
						{
							uint id = item?["id"]?.GetValue<uint>() ?? 0;
							string text = item?["text"]?.GetValue<string>();
							if (id != 0 && text != null) lines[id] = text;
						}
					}
				}
				catch (Exception ex) { notes.Add(ex.Message); }
				files.Add(new
				{
					name = Path.GetFileNameWithoutExtension(file),
					path = "defs/text/" + Path.GetFileName(file),
					lines = lines.Count,
					first = lines.Count > 0 ? lines.Keys.Min() : 0,
					last = lines.Count > 0 ? lines.Keys.Max() : 0,
					entries = lines.OrderBy(p => p.Key).Select(p => new { id = p.Key, text = p.Value }).ToList(),
					problem = notes.Count > 0 ? notes[0] : null
				});
			}
			return files;
		}

		/// <summary>The next id no file of the project uses, from ModText.FirstId.</summary>
		public static uint NextId(Project project)
		{
			Dictionary<uint, string> lines = Lines(project);
			uint next = ModText.FirstId;
			if (lines.Count > 0) next = Math.Max(next, lines.Keys.Max() + 1);
			return next;
		}

		/// <summary>A new defs/text/<name>.json with one line at the next free id; the path under the project.</summary>
		public static string New(Project project, string name)
		{
			string slug = ProjectItems.Slug(name);
			if (string.IsNullOrEmpty(slug)) throw new ArgumentException("a text file needs a name");
			string path = Path.Combine(Directory(project), slug + ".json");
			if (File.Exists(path)) throw new ArgumentException("defs/text/" + slug + ".json exists already");
			System.IO.Directory.CreateDirectory(Directory(project));
			uint id = NextId(project);
			string body = "{\n  \"" + id.ToString(CultureInfo.InvariantCulture) + "\": \"Hello there, traveller!\\nThe line breaks here.\"\n}\n";
			File.WriteAllText(path, body);
			return "defs/text/" + slug + ".json";
		}
	}
}
