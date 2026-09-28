// Named sprites on a picture - a sheet of a mod's own or one of the game's - each with its own
// 9-slice borders, as Unity's Sprite Editor keeps them in Multiple mode. A folder of screens keeps
// them in one file beside its layouts, sprites.json:
//
//   { "sheets": {
//       "url:images/ui.png": { "sprites": [
//           { "name": "panel_blue", "x": 0, "y": 0, "w": 48, "h": 48, "left": 12, "top": 12, "right": 12, "bottom": 12 },
//           { "name": "cursor", "x": 48, "y": 0, "w": 16, "h": 16 } ] },
//       "resource:files/m000_window.NCGR": { "sprites": [ ... ] } } }
//
// A frame names one by -ff-sprite with the sheet as its background-image (MenuBackground): the
// sprite gives the part of the picture and its borders, and a frame's own -ff-background-rect or
// -ff-slice still win over them. Crystal's Sprite editor writes the file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OpenFF.Content
{
	internal static class MenuSprites
	{
		public const string FileName = "sprites.json";

		public sealed class Sprite
		{
			public string Name;
			public int X, Y, W, H;
			public float Left, Top, Right, Bottom;
		}

		private static readonly Dictionary<string, (DateTime Stamp, Dictionary<string, List<Sprite>> Sheets)> Cache = new Dictionary<string, (DateTime, Dictionary<string, List<Sprite>>)>(StringComparer.OrdinalIgnoreCase);

		/// <summary>The key a sheet is kept under: "url:images/ui.png" or "resource:files/m000_window.NCGR".</summary>
		public static string Key(string kind, string path) => (kind ?? "url") + ":" + (path ?? "").Replace('\\', '/');

		/// <summary>A sprite of a sheet, from the folder's sprites.json; null when there is no such.</summary>
		public static Sprite Find(string folder, string kind, string path, string name)
		{
			if (string.IsNullOrWhiteSpace(name) || folder == null) return null;
			Dictionary<string, List<Sprite>> sheets = Read(folder);
			if (sheets == null || !sheets.TryGetValue(Key(kind, path), out List<Sprite> sprites)) return null;
			return sprites.Find(s => string.Equals(s.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>The folder's sheets, read again when the file has changed; null when it has none.</summary>
		public static Dictionary<string, List<Sprite>> Read(string folder)
		{
			string file = Path.Combine(folder, FileName);
			if (!File.Exists(file)) return null;
			DateTime stamp = File.GetLastWriteTimeUtc(file);
			if (Cache.TryGetValue(file, out (DateTime Stamp, Dictionary<string, List<Sprite>> Sheets) hit) && hit.Stamp == stamp) return hit.Sheets;
			Dictionary<string, List<Sprite>> sheets = new Dictionary<string, List<Sprite>>(StringComparer.OrdinalIgnoreCase);
			try
			{
				using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(file), new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
				if (doc.RootElement.TryGetProperty("sheets", out JsonElement all) && all.ValueKind == JsonValueKind.Object)
				{
					foreach (JsonProperty sheet in all.EnumerateObject())
					{
						List<Sprite> list = new List<Sprite>();
						if (sheet.Value.TryGetProperty("sprites", out JsonElement sprites) && sprites.ValueKind == JsonValueKind.Array)
						{
							foreach (JsonElement s in sprites.EnumerateArray())
							{
								float F(string n) => s.TryGetProperty(n, out JsonElement v) && v.ValueKind == JsonValueKind.Number ? (float)v.GetDouble() : 0;
								string name = s.TryGetProperty("name", out JsonElement nm) ? nm.GetString() : null;
								if (string.IsNullOrWhiteSpace(name)) continue;
								list.Add(new Sprite { Name = name.Trim(), X = (int)F("x"), Y = (int)F("y"), W = (int)F("w"), H = (int)F("h"), Left = F("left"), Top = F("top"), Right = F("right"), Bottom = F("bottom") });
							}
						}
						sheets[sheet.Name.Replace('\\', '/')] = list;
					}
				}
			}
			catch (Exception) { return null; }
			Cache[file] = (stamp, sheets);
			return sheets;
		}
	}
}
