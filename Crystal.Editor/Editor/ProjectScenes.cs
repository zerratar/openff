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
using System.Globalization;
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
			/// <summary>A map of the mod's own (no game map behind it): its display name.</summary>
			public string Title { get; set; }
			/// <summary>Whether the map is the mod's own - a scene file and nothing of the game's.</summary>
			public bool Own { get; set; }
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
					summary.Own = node?["own"]?.GetValue<bool>() ?? false;
					summary.Title = node?["title"]?.GetValue<string>();
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

		/// <summary>
		/// Renames a tag (or removes it, with no new name) on every object in every scene file
		/// of the project, and in the project's own tag list. Returns the maps whose files
		/// changed - the editor refreshes those it has open.
		/// </summary>
		public static List<string> Retag(Project project, string from, string to)
		{
			List<string> touched = new List<string>();
			from = (from ?? "").Trim();
			to = (to ?? "").Trim();
			if (from.Length == 0) return touched;
			string directory = Directory(project);
			if (System.IO.Directory.Exists(directory))
			{
				foreach (string file in System.IO.Directory.EnumerateFiles(directory, "*.json"))
				{
					try
					{
						JsonNode node = JsonNode.Parse(File.ReadAllText(file));
						bool changed = Walk(node?["objects"] as JsonArray) | Walk(node?["points"] as JsonArray);
						if (!changed) continue;
						File.WriteAllText(file, node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
						touched.Add(Path.GetFileNameWithoutExtension(file));
					}
					catch (Exception) { }
				}
			}
			List<string> tags = project.File.Tags ?? new List<string>();
			if (tags.RemoveAll(t => string.Equals(t, from, StringComparison.OrdinalIgnoreCase)) > 0 || to.Length > 0)
			{
				if (to.Length > 0 && !tags.Any(t => string.Equals(t, to, StringComparison.OrdinalIgnoreCase))) tags.Add(to);
				tags.Sort(StringComparer.OrdinalIgnoreCase);
				project.File.Tags = tags;
				project.Save();
			}
			return touched;

			bool Walk(JsonArray objects)
			{
				bool changed = false;
				foreach (JsonNode o in objects ?? new JsonArray())
				{
					if (o == null) continue;
					if (o["tags"] is JsonArray list)
					{
						List<string> next = new List<string>();
						bool hit = false;
						foreach (JsonNode t in list)
						{
							string tag = t?.GetValue<string>()?.Trim() ?? "";
							if (string.Equals(tag, from, StringComparison.OrdinalIgnoreCase)) { hit = true; tag = to; }
							if (tag.Length > 0 && !next.Contains(tag)) next.Add(tag);
						}
						if (hit)
						{
							o["tags"] = new JsonArray(next.Select(t => (JsonNode)t).ToArray());
							changed = true;
						}
					}
					changed |= Walk(o["children"] as JsonArray);
				}
				return changed;
			}
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
			// A map of the mod's own is the file: it stays, empty or not, with its title.
			JsonObject before = File.Exists(path) ? JsonNode.Parse(File.ReadAllText(path)) as JsonObject : null;
			bool own = before?["own"]?.GetValue<bool>() ?? false;
			bool empty = (attachments == null || attachments.Count == 0) && (objects == null || objects.Count == 0);
			if (empty && !own)
			{
				if (File.Exists(path)) File.Delete(path);
				return;
			}
			System.IO.Directory.CreateDirectory(Directory(project));
			JsonObject file = new JsonObject { ["map"] = map };
			if (own)
			{
				file["own"] = true;
				if (before["title"] != null) file["title"] = before["title"].GetValue<string>();
			}
			if (objects != null && objects.Count > 0) file["objects"] = JsonNode.Parse(objects.ToJsonString());
			file["attachments"] = JsonNode.Parse((attachments ?? new JsonArray()).ToJsonString());
			File.WriteAllText(path, file.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
		}

		/// <summary>Whether a map is one of the mod's own: a scene file marked so, nothing of the game's behind it.</summary>
		public static bool IsOwn(Project project, string map)
		{
			if (project == null || string.IsNullOrWhiteSpace(map) || map.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return false;
			string path = Path.Combine(Directory(project), map + ".json");
			if (!File.Exists(path)) return false;
			try { return (JsonNode.Parse(File.ReadAllText(path)) as JsonObject)?["own"]?.GetValue<bool>() ?? false; }
			catch (JsonException) { return false; }
		}

		/// <summary>
		/// A new map of the mod's own: a scene file under a stage name the game does not use,
		/// with a title, a Ground object - a Solid Mesh of the glTF given, or of a flat slab the
		/// editor writes into assets/ when none is - and, when asked, a Music object. The
		/// stage name is shaped like the game's (t90_00, d90_00 ...) because the client's stage
		/// loader reads the map's kind off the first letter; it finds no files behind the name
		/// and draws the scene alone.
		/// </summary>
		public static string Create(Project project, string title, string kind, string ground, int size, int bgm, Func<string, bool> gameHas)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("a name for the map");
			char prefix = string.Equals(kind, "dungeon", StringComparison.OrdinalIgnoreCase) ? 'd' : 't';
			string map = null;
			for (int area = 90; area <= 99 && map == null; area++)
			{
				for (int room = 0; room <= 99; room++)
				{
					string candidate = prefix + area.ToString("00", CultureInfo.InvariantCulture) + "_" + room.ToString("00", CultureInfo.InvariantCulture);
					if (File.Exists(Path.Combine(Directory(project), candidate + ".json"))) continue;
					if (gameHas != null && gameHas(candidate)) continue;
					map = candidate;
					break;
				}
			}
			if (map == null) throw new InvalidOperationException("no free stage name left between " + prefix + "90_00 and " + prefix + "99_99");
			if (string.IsNullOrWhiteSpace(ground))
			{
				// A slab to stand on, the size asked (a town's square is about 100 across), one unit thick, a dull green.
				int side = Math.Clamp(size <= 0 ? 100 : size, 10, 2000);
				string file = "ground-" + map + ".gltf";
				GltfWriter.WriteBox(Path.Combine(project.Directory, GltfBundle.Folder, file), side, 1, side, 0.36f, 0.48f, 0.30f);
				ground = GltfBundle.Folder + "/" + file;
			}
			JsonArray objects = new JsonArray
			{
				new JsonObject { ["name"] = "Ground", ["x"] = 0, ["y"] = -1, ["z"] = 0, ["yaw"] = 0, ["scale"] = 1, ["model"] = ground }
			};
			JsonArray attachments = new JsonArray
			{
				// The map itself carries its settings: a sky behind the scene, the game's usual camera.
				new JsonObject { ["target"] = "map", ["behaviour"] = "MapSettings", ["fields"] = new JsonObject
				{
					["Background"] = new JsonObject { ["r"] = 74, ["g"] = 112, ["b"] = 156, ["a"] = 255 },
					["CameraOffset"] = new JsonObject { ["x"] = 0, ["y"] = 110, ["z"] = 110 },
					["LookOffset"] = new JsonObject { ["x"] = 0, ["y"] = 10, ["z"] = 0 },
					["ZoomRange"] = 60
				} },
				new JsonObject { ["target"] = "Ground", ["behaviour"] = "Mesh", ["fields"] = new JsonObject { ["Path"] = ground, ["Solid"] = true } }
			};
			if (bgm > 0)
			{
				objects.Add(new JsonObject { ["name"] = "Music", ["x"] = 0, ["y"] = 0, ["z"] = 0, ["yaw"] = 0, ["scale"] = 1 });
				attachments.Add(new JsonObject { ["target"] = "Music", ["behaviour"] = "Music", ["fields"] = new JsonObject { ["Bgm"] = bgm } });
			}
			System.IO.Directory.CreateDirectory(Directory(project));
			JsonObject fileNode = new JsonObject { ["map"] = map, ["own"] = true, ["title"] = title.Trim(), ["objects"] = objects, ["attachments"] = attachments };
			File.WriteAllText(PathFor(project, map), fileNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
			return map;
		}
	}
}
