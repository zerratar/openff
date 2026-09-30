// The project's own effects, written: what Crystal's effect editor (wwwroot/effects-editor.js) saves,
// makes, copies and deletes under defs/effects/, the pictures beside them, and which spell plays one
// (defs/spells/<id>.json - the spell looks of Shared/Data/ModSpells.cs).
//
// Server: /api/project/effect/save {name, effect}, /new {id, category?, member?, from?}, /delete {name},
// /image?effect=&name= (a PNG's bytes as the body), /images?effect=, /spells, /spell {spell, effect, cast}.
// /api/effect/textures lists every picture of the game's effect packs, for the texture picker.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using OpenFF.Data;

namespace Crystal.Editor
{
	internal static class EffectsProject
	{
		private static string Folder(Project project)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			return Path.Combine(project.Directory, "defs", "effects");
		}

		private static string PathOf(Project project, string name)
		{
			if (!Effects.IsOwn(name)) throw new ArgumentException("not a project effect: " + name);
			string file = Path.GetFileName(name.Substring(Effects.OwnFolder.Length + 1));
			if (!file.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("an effect is a .json file");
			return Path.Combine(Folder(project), file);
		}

		/// <summary>An effect as its file has it: indented, a list of numbers on one line ([0, 1, 0], a key).</summary>
		public static string Text(JsonNode effect)
		{
			string text = effect.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
			return Regex.Replace(text, @"\[\s+([-\d.,eE\s]+?)\s+\]", m => "[" + Regex.Replace(m.Groups[1].Value, @"\s+", " ").Trim() + "]") + "\n";
		}

		public static object Save(Project project, string name, JsonNode effect)
		{
			if (effect is not JsonObject o) throw new ArgumentException("an effect is an object");
			if (o["format"] == null) o["format"] = OpenFF.Effects.EffectImport.Format;
			if (o["tracks"] is not JsonArray) o["tracks"] = new JsonArray();
			string path = PathOf(project, name);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, Text(o), new UTF8Encoding(false));
			return new { ok = true, name };
		}

		/// <summary>A new effect: one of the game's imported (category, member), a copy of the project's (from), or a spark to start from.</summary>
		public static object New(Project project, Workspace workspace, string id, int? category, int? member, string from)
		{
			string wanted = Slug(string.IsNullOrWhiteSpace(id) ? (category != null ? "e" + category + "-" + (member ?? 1) : "effect") : id);
			string folder = Folder(project);
			Directory.CreateDirectory(folder);
			string unique = wanted;
			for (int n = 2; File.Exists(Path.Combine(folder, unique + ".json")); n++) unique = wanted + "-" + n;
			JsonNode effect;
			if (!string.IsNullOrEmpty(from)) effect = JsonNode.Parse(File.ReadAllText(PathOf(project, from)));
			else if (category != null) effect = Effects.ImportJson(workspace, category.Value, member ?? 1);
			else effect = Spark(folder);
			string name = Effects.OwnFolder + "/" + unique + ".json";
			Save(project, name, effect);
			return new { ok = true, name };
		}

		public static object Delete(Project project, string name)
		{
			string path = PathOf(project, name);
			if (File.Exists(path)) File.Delete(path);
			return new { ok = true };
		}

		/// <summary>The PNGs beside the project's effects, for the texture picker.</summary>
		public static object Images(Project project)
		{
			string folder = project == null ? null : Folder(project);
			if (folder == null || !Directory.Exists(folder)) return new object[0];
			return Directory.EnumerateFiles(folder, "*.png").OrderBy(f => f, StringComparer.OrdinalIgnoreCase).Select(f =>
			{
				(int w, int h) = PngSize(f);
				return (object)new { image = Path.GetFileName(f), width = w, height = h };
			}).ToList();
		}

		/// <summary>A PNG put beside the effects (the body is the file).</summary>
		public static object Upload(Project project, string name, byte[] png)
		{
			if (png == null || png.Length < 24 || png[0] != 0x89 || png[1] != 'P') throw new ArgumentException("not a PNG");
			string file = Slug(Path.GetFileNameWithoutExtension(name ?? "picture")) + ".png";
			string folder = Folder(project);
			Directory.CreateDirectory(folder);
			File.WriteAllBytes(Path.Combine(folder, file), png);
			(int w, int h) = PngSize(Path.Combine(folder, file));
			return new { ok = true, image = file, width = w, height = h };
		}

		/// <summary>The game's spells, and which of the project's spell looks plays which effect (defs/spells).</summary>
		public static object Spells(Project project, Workspace workspace)
		{
			GameTables tables = GameData.Tables(workspace);
			List<object> looks = new List<object>();
			string dir = project == null ? null : Path.Combine(project.Directory, "defs", "spells");
			if (dir != null && Directory.Exists(dir))
			{
				foreach (string f in Directory.EnumerateFiles(dir, "*.json"))
				{
					try
					{
						JsonObject o = JsonNode.Parse(File.ReadAllText(f)).AsObject();
						looks.Add(new { file = "defs/spells/" + Path.GetFileName(f), spell = o["spell"]?.ToString(), effect = o["effect"]?.ToString(), cast = o["cast"]?.ToString() });
					}
					catch (Exception) { }
				}
			}
			// The effect each plays (its record's pack and member), for a copy of it to start a spell's own from.
			Dictionary<int, (int, int)> effects = Effects.SpellEffects(workspace);
			return new
			{
				spells = tables.Spells.Where(s => !string.IsNullOrEmpty(s.Name)).Select(s => new
				{
					id = s.Id, name = s.Name, school = s.School.ToString(), level = s.Level,
					category = effects.TryGetValue(s.Id, out (int, int) e) ? e.Item1 : -1,
					member = effects.TryGetValue(s.Id, out (int, int) e2) ? e2.Item2 : -1,
				}).ToList(),
				looks,
			};
		}

		/// <summary>A spell played with one of the project's effects (or its cast), written into the spell's look - the one already naming it, else a new file.</summary>
		public static object Spell(Project project, string spell, string effect, bool cast)
		{
			if (string.IsNullOrWhiteSpace(spell)) throw new ArgumentException("which spell?");
			string dir = Path.Combine(project.Directory, "defs", "spells");
			Directory.CreateDirectory(dir);
			string path = null;
			JsonObject look = null;
			foreach (string f in Directory.EnumerateFiles(dir, "*.json"))
			{
				try
				{
					JsonObject o = JsonNode.Parse(File.ReadAllText(f)).AsObject();
					if (string.Equals(o["spell"]?.ToString(), spell, StringComparison.OrdinalIgnoreCase)) { path = f; look = o; break; }
				}
				catch (Exception) { }
			}
			if (look == null) { look = new JsonObject { ["spell"] = spell }; path = Path.Combine(dir, Slug(spell) + ".json"); }
			string key = cast ? "cast" : "effect";
			if (string.IsNullOrEmpty(effect)) look.Remove(key); else look[key] = effect;
			if (look.Count <= 1) { if (File.Exists(path)) File.Delete(path); }
			else File.WriteAllText(path, look.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n", new UTF8Encoding(false));
			return new { ok = true, file = "defs/spells/" + Path.GetFileName(path) };
		}

		/// <summary>
		/// The project's saved curves and gradients (defs/effects/presets.json): { gradients: { name: preset }, curves: { name: preset } },
		/// a preset's keys at their share of the life (0 its first frame, 1 its last), so it fits any life it is put on.
		/// </summary>
		public static JsonObject Presets(Project project)
		{
			string path = project == null ? null : Path.Combine(Folder(project), "presets.json");
			JsonObject o = null;
			if (path != null && File.Exists(path)) { try { o = JsonNode.Parse(File.ReadAllText(path)) as JsonObject; } catch (Exception) { } }
			o ??= new JsonObject();
			if (o["gradients"] is not JsonObject) o["gradients"] = new JsonObject();
			if (o["curves"] is not JsonObject) o["curves"] = new JsonObject();
			return o;
		}

		/// <summary>A preset saved under its name (or, remove, taken out): kind is gradients or curves.</summary>
		public static object SavePreset(Project project, string kind, string name, JsonNode value, bool remove)
		{
			if (kind != "gradients" && kind != "curves") throw new ArgumentException("a preset is a gradient or a curve");
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("a preset needs a name");
			JsonObject all = Presets(project);
			JsonObject list = all[kind].AsObject();
			if (remove) list.Remove(name.Trim());
			else
			{
				if (value is not JsonObject) throw new ArgumentException("no preset to save");
				list[name.Trim()] = value.DeepClone();
			}
			string folder = Folder(project);
			Directory.CreateDirectory(folder);
			File.WriteAllText(Path.Combine(folder, "presets.json"), Text(all), new UTF8Encoding(false));
			return new { ok = true, presets = all };
		}

		/// <summary>An effect to start from: a spark of the project's own picture (glow.png, made once) rising and fading.</summary>
		private static JsonNode Spark(string folder)
		{
			string glow = Path.Combine(folder, "glow.png");
			if (!File.Exists(glow)) File.WriteAllBytes(glow, Glow(64));
			return JsonNode.Parse(@"{
  ""format"": 1, ""length"": 30,
  ""tracks"": [
    { ""type"": ""emitter"", ""name"": ""sparks"", ""start"": 0, ""anchor"": ""target"",
      ""emission"": { ""duration"": 20, ""interval"": 2, ""count"": 3, ""bursts"": 10 }, ""life"": 18,
      ""shape"": { ""box"": [2, 2, 2] }, ""size"": [2, 4],
      ""speed"": { ""direction"": [0, 1, 0], ""value"": [0.2, 0.5], ""spread"": [30, 180, 30] },
      ""colour"": [[1, 255, 240, 160, 0], [4, 255, 220, 120, 230], [17, 255, 120, 40, 0]],
      ""scale"": [[1, 0.5, 0.5], [6, 1, 1], [17, 0.3, 0.3]],
      ""texture"": { ""image"": ""glow.png"", ""width"": 64, ""height"": 64 },
      ""render"": { ""blend"": ""additive"" } }
  ]
}");
		}

		/// <summary>A soft round light: white, its alpha falling from the middle.</summary>
		public static byte[] Glow(int n)
		{
			byte[] rgba = new byte[n * n * 4];
			for (int y = 0; y < n; y++)
				for (int x = 0; x < n; x++)
				{
					double r = Math.Sqrt(Math.Pow(x - (n - 1) / 2.0, 2) + Math.Pow(y - (n - 1) / 2.0, 2)) / (n / 2.0);
					double a = Math.Pow(Math.Max(0, 1 - r), 2);
					int i = 4 * (y * n + x);
					rgba[i] = rgba[i + 1] = rgba[i + 2] = 255;
					rgba[i + 3] = (byte)Math.Round(a * 255);
				}
			return Png.Encode(n, n, rgba);
		}

		private static (int, int) PngSize(string path)
		{
			try
			{
				using FileStream s = File.OpenRead(path);
				byte[] head = new byte[24];
				if (s.Read(head, 0, 24) < 24) return (0, 0);
				int w = (head[16] << 24) | (head[17] << 16) | (head[18] << 8) | head[19];
				int h = (head[20] << 24) | (head[21] << 16) | (head[22] << 8) | head[23];
				return (w, h);
			}
			catch (Exception) { return (0, 0); }
		}

		private static string Slug(string text)
		{
			string s = Regex.Replace((text ?? "").Trim().ToLowerInvariant(), @"[^a-z0-9_-]+", "-").Trim('-');
			return s.Length == 0 ? "effect" : s;
		}
	}
}
