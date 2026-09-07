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
			public int Points { get; set; }
			public long Bytes { get; set; }
			public DateTime Modified { get; set; }
		}

		/// <summary>The maps with a scene file, and how many attachments and points each has.</summary>
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
					summary.Points = node?["points"] is JsonArray points ? points.Count : 0;
				}
				catch (Exception) { }
				maps.Add(summary);
			}
			return maps;
		}

		/// <summary>The attachments of one map, as JSON (an empty array when there is no file).</summary>
		public static JsonArray Read(Project project, string map) => Section(project, map, "attachments");

		/// <summary>The points of one map: { name, x, y, z, yaw, tags } each, placed in the editor for mods to find.</summary>
		public static JsonArray Points(Project project, string map) => Section(project, map, "points");

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

		/// <summary>Writes a map's attachments and points; nothing of either deletes the file.</summary>
		public static void Write(Project project, string map, JsonArray attachments, JsonArray points)
		{
			string path = PathFor(project, map);
			bool empty = (attachments == null || attachments.Count == 0) && (points == null || points.Count == 0);
			if (empty)
			{
				if (File.Exists(path)) File.Delete(path);
				return;
			}
			System.IO.Directory.CreateDirectory(Directory(project));
			JsonObject file = new JsonObject { ["map"] = map };
			if (points != null && points.Count > 0) file["points"] = JsonNode.Parse(points.ToJsonString());
			file["attachments"] = JsonNode.Parse((attachments ?? new JsonArray()).ToJsonString());
			File.WriteAllText(path, file.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
		}
	}
}
