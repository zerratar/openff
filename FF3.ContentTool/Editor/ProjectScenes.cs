// A project's scene files: which of the mod's behaviours sit on which map objects.
//
// <project>/scenes/<map>.json, the shape OpenFF.Engine/Scenes.cs reads:
//   { "map": "d01_05", "attachments": [ { "target": "object:3", "behaviour": "Greeter", "fields": {...} } ] }
// Export to OpenFF copies the folder into the mod as scenes/. Targets are the editor's
// selection keys: "map", "object:<index>" (the .hich row), "exit:<slot>".

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FF3.ContentTool.Editor
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

		/// <summary>The maps with a scene file, and how many attachments each has.</summary>
		public static List<KeyValuePair<string, int>> Maps(Project project)
		{
			List<KeyValuePair<string, int>> maps = new List<KeyValuePair<string, int>>();
			string directory = Directory(project);
			if (!System.IO.Directory.Exists(directory)) return maps;
			foreach (string file in System.IO.Directory.EnumerateFiles(directory, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
			{
				int count = 0;
				try
				{
					JsonNode node = JsonNode.Parse(File.ReadAllText(file));
					count = node?["attachments"] is JsonArray array ? array.Count : 0;
				}
				catch (Exception) { }
				maps.Add(new KeyValuePair<string, int>(Path.GetFileNameWithoutExtension(file), count));
			}
			return maps;
		}

		/// <summary>The attachments of one map, as JSON (an empty array when there is no file).</summary>
		public static JsonArray Read(Project project, string map)
		{
			string path = PathFor(project, map);
			if (!File.Exists(path)) return new JsonArray();
			try
			{
				JsonNode node = JsonNode.Parse(File.ReadAllText(path));
				return node?["attachments"] as JsonArray ?? new JsonArray();
			}
			catch (JsonException ex)
			{
				throw new IOException(Path.GetFileName(path) + " does not read as JSON: " + ex.Message);
			}
		}

		/// <summary>Writes a map's attachments; an empty list deletes the file.</summary>
		public static void Write(Project project, string map, JsonArray attachments)
		{
			string path = PathFor(project, map);
			if (attachments == null || attachments.Count == 0)
			{
				if (File.Exists(path)) File.Delete(path);
				return;
			}
			System.IO.Directory.CreateDirectory(Directory(project));
			JsonObject file = new JsonObject
			{
				["map"] = map,
				["attachments"] = JsonNode.Parse(attachments.ToJsonString()),
			};
			File.WriteAllText(path, file.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
		}
	}
}
