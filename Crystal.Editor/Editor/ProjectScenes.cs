// A project's scene files: the mod's own objects on a map, and which of the mod's
// behaviours sit on which objects - the game's or the mod's.
//
// <project>/scenes/<map>.json, the shape OpenFF.Engine/Scenes.cs reads:
//   { "map": "d01_05",
//     "objects": [ { "name": "Chest", "x": 12, "y": 0, "z": -20, "yaw": 0, "scale": 1, "model": "o001",
//                    "tags": ["chest"], "children": [ ... ] } ],
//     "attachments": [ { "target": "object:3", "behaviour": "Greeter", "fields": {...} },
//                      { "target": "chest", "behaviour": "GiveItem", "fields": {...} } ] }
// Export to OpenFF copies the folder into the mod as scenes/. Targets are the editor's
// selection keys: "map", "object:<index>" (the .hich row), "exit:<slot>", and a scene
// object's path ("chest", "chest/trigger").

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Crystal.Editor
{
	internal static class ProjectScenes
	{
		public const string FolderName = "scenes";

		public static string Directory(Project project) => Path.Combine(project.Directory, FolderName);

		private static string PathFor(Project project, string map)
		{
			if (string.IsNullOrWhiteSpace(map) || map.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
			{
				throw new ArgumentException("not a map name: " + map);
			}
			return Path.Combine(Directory(project), map + ".json");
		}

		/// <summary>One scene file, as the project panel lists it.</summary>
		public sealed class Summary
		{
			public string Map { get; set; }
			public int Attachments { get; set; }
			/// <summary>The mod's own objects, the whole tree counted.</summary>
			public int Points { get; set; }
			public long Bytes { get; set; }
			public DateTime Modified { get; set; }
		}

		/// <summary>The maps with a scene file, and how many attachments and objects each has.</summary>
		public static List<Summary> Maps(Project project)
		{
			List<Summary> maps = new List<Summary>();
			string directory = Directory(project);
			if (!System.IO.Directory.Exists(directory)) return maps;
			foreach (string file in System.IO.Directory.EnumerateFiles(directory, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
			{
				Summary summary = new Summary { Map = Path.GetFileNameWithoutExtension(file) };
				try
				{
					FileInfo info = new FileInfo(file);
					summary.Bytes = info.Length;
					summary.Modified = info.LastWriteTime;
					JsonNode node = JsonNode.Parse(File.ReadAllText(file));
					summary.Attachments = node?["attachments"] is JsonArray array ? array.Count : 0;
					summary.Points = Count(node?["objects"] as JsonArray) + (node?["points"] is JsonArray points ? points.Count : 0);
				}
				catch (Exception) { }
				maps.Add(summary);
			}
			return maps;
		}

		/// <summary>One tag as the project's scene files use it: on how many objects, on which maps.</summary>
		public sealed class TagUse
		{
			public string Tag { get; set; }
			public int Count { get; set; }
			public List<string> Maps { get; set; } = new List<string>();
		}

		/// <summary>
		/// Every tag on any object in any of the project's scene files, with its use - what the
		/// inspector's tag picker offers, so a tag is picked from the ones the mod already
		/// speaks of (Game.World.Legacy.WithTag) rather than typed anew each time.
		/// </summary>
		public static List<TagUse> Tags(Project project)
		{
			Dictionary<string, TagUse> uses = new Dictionary<string, TagUse>(StringComparer.OrdinalIgnoreCase);
			string directory = Directory(project);
			if (!System.IO.Directory.Exists(directory)) return new List<TagUse>();
			foreach (string file in System.IO.Directory.EnumerateFiles(directory, "*.json"))
			{
				string map = Path.GetFileNameWithoutExtension(file);
				try
				{
					JsonNode node = JsonNode.Parse(File.ReadAllText(file));
					Walk(node?["objects"] as JsonArray);
					Walk(node?["points"] as JsonArray);
				}
				catch (Exception) { }

				void Walk(JsonArray objects)
				{
					foreach (JsonNode o in objects ?? new JsonArray())
					{
						if (o == null) continue;
						foreach (JsonNode t in o["tags"] as JsonArray ?? new JsonArray())
						{
							string tag = t?.GetValue<string>()?.Trim();
							if (string.IsNullOrEmpty(tag)) continue;
							if (!uses.TryGetValue(tag, out TagUse use)) uses[tag] = use = new TagUse { Tag = tag };
							use.Count++;
							if (!use.Maps.Contains(map)) use.Maps.Add(map);
						}
						Walk(o["children"] as JsonArray);
					}
				}
			}
			return uses.Values.OrderBy(u => u.Tag, StringComparer.OrdinalIgnoreCase).ToList();
		}

		private static int Count(JsonArray objects)
		{
			int n = 0;
			foreach (JsonNode o in objects ?? new JsonArray())
			{
				if (o == null) continue;
				n += 1 + Count(o["children"] as JsonArray);
			}
			return n;
		}

		/// <summary>The attachments of one map, as JSON (an empty array when there is no file).</summary>
		public static JsonArray Read(Project project, string map) => Section(project, map, "attachments");

		/// <summary>
		/// The mod's own objects of one map, a tree: { name, x, y, z, yaw, scale, model, tags,
		/// children } each. A file from before the objects (with "points") answers with them as
		/// objects without a model, so the page has one shape to work with.
		/// </summary>
		public static JsonArray Objects(Project project, string map)
		{
			JsonArray objects = Section(project, map, "objects");
			foreach (JsonNode point in Section(project, map, "points"))
			{
				if (point == null) continue;
				string name = point["name"]?.GetValue<string>();
				if (string.IsNullOrWhiteSpace(name)) continue;
				if (objects.Any(o => string.Equals(o?["name"]?.GetValue<string>(), name, StringComparison.OrdinalIgnoreCase))) continue;
				JsonObject o = JsonNode.Parse(point.ToJsonString()) as JsonObject ?? new JsonObject();
				objects.Add(o);
			}
			return objects;
		}

		private static JsonArray Section(Project project, string map, string key)
		{
			string path = PathFor(project, map);
			if (!File.Exists(path)) return new JsonArray();
			try
			{
				JsonNode node = JsonNode.Parse(File.ReadAllText(path));
				return node?[key] as JsonArray ?? new JsonArray();
			}
			catch (JsonException ex)
			{
				throw new IOException(Path.GetFileName(path) + " does not read as JSON: " + ex.Message);
			}
		}

		/// <summary>Writes a map's attachments and objects; nothing of either deletes the file. The "points" of an older file are gone once it is written (they came back in as objects).</summary>
		public static void Write(Project project, string map, JsonArray attachments, JsonArray objects)
		{
			string path = PathFor(project, map);
			bool empty = (attachments == null || attachments.Count == 0) && (objects == null || objects.Count == 0);
			if (empty)
			{
				if (File.Exists(path)) File.Delete(path);
				return;
			}
			System.IO.Directory.CreateDirectory(Directory(project));
			JsonObject file = new JsonObject { ["map"] = map };
			if (objects != null && objects.Count > 0) file["objects"] = JsonNode.Parse(objects.ToJsonString());
			file["attachments"] = JsonNode.Parse((attachments ?? new JsonArray()).ToJsonString());
			File.WriteAllText(path, file.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
		}
	}
}
