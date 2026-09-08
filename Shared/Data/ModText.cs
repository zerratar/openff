// A mod's text: lines of its own with message ids, composed into the game's permanent
// message file (eureka_permanent.msd, the one every map falls back to) as the game reads
// it - so a CastScript's startMessage2(0, 40000001, 0, 0), a Chest's "@40000001", a Talk
// line "@40000001" all say the mod's words through the game's own message window, with the
// game's control codes (%shuyaku1%, %unfixed_item%) working as they do everywhere.
//
//   defs/text/<name>.json
//   {
//     "40000001": "Hello there, traveller!",
//     "40000002": "The elders are looking for you.\nHurry along now."
//   }
//
// Ids are the mod's to choose; 40000000 and up is clear of every id the game's files use
// (the maps' own run to the tens of millions; the permanent file's to 1002058). "\n" in the
// text is a line break in the window, as the game's own lines have them. Two mods with one
// id: the first in load order keeps it, as a mod's files do.
//
// A line may be given per language instead of once - the first step of localisation:
//
//     "40000003": { "en": "Welcome!", "de": "Willkommen!", "ja": "ようこそ！" }
//
// The client picks the game's language (its setting: ja, en, fr, de, it, es, zh-CN, zh-TW,
// ko), then English, then the first written. The array form [{ "id", "text" }] is read too;
// Crystal's table writes it while two rows share an id, so that nothing is lost meanwhile.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenFF.Content;

namespace OpenFF.Data
{
	internal static class ModText
	{
		public const string Folder = "defs/text";
		public const uint FirstId = 40000000;

		/// <summary>Every line under the roots' defs/text, by id; the first root's wins a clash. A line may be one text or a text per language ({ "en": …, "de": … }): the one for the language asked for, else English, else the first written.</summary>
		public static Dictionary<uint, string> Load(IEnumerable<string> roots, List<string> notes = null, string language = "en")
		{
			Dictionary<uint, string> lines = new Dictionary<uint, string>();
			foreach (string root in roots ?? Enumerable.Empty<string>())
			{
				string directory = Path.Combine(root, Folder.Replace('/', Path.DirectorySeparatorChar));
				if (!Directory.Exists(directory)) continue;
				foreach (string file in Directory.EnumerateFiles(directory, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
				{
					try
					{
						JsonNode node = JsonNode.Parse(File.ReadAllText(file));
						if (node is JsonObject o)
						{
							foreach (KeyValuePair<string, JsonNode> pair in o)
							{
								if (!uint.TryParse(pair.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint id)) { notes?.Add(file + ": '" + pair.Key + "' is not a message id"); continue; }
								string text = TextOf(pair.Value, language);
								if (text == null) continue;
								if (!lines.ContainsKey(id)) lines[id] = text; else notes?.Add(file + ": " + id + " is defined already; kept the first");
							}
						}
						else if (node is JsonArray a)
						{
							foreach (JsonNode item in a)
							{
								string text = TextOf(item?["text"], language);
								if (!TryId(item?["id"], out uint id) || text == null) { if (item != null) notes?.Add(file + ": an entry without an id or a text"); continue; }
								if (!lines.ContainsKey(id)) lines[id] = text; else notes?.Add(file + ": " + id + " is defined already; kept the first");
							}
						}
					}
					catch (Exception ex) { notes?.Add(file + ": " + ex.Message); }
				}
			}
			return lines;
		}

		/// <summary>The line's text: the string itself, or from a per-language object the language asked for, else "en", else the first.</summary>
		public static string TextOf(JsonNode node, string language)
		{
			if (node is JsonValue value) return value.TryGetValue(out string s) ? s : null;
			if (node is not JsonObject languages || languages.Count == 0) return null;
			foreach (string want in new[] { language ?? "en", "en" })
				if (want != null && languages[want] is JsonValue v && v.TryGetValue(out string text)) return text;
			foreach (KeyValuePair<string, JsonNode> pair in languages)
				if (pair.Value is JsonValue v && v.TryGetValue(out string text)) return text;
			return null;
		}

		/// <summary>An id written as a number or as a string ("40000001"), as the editor's table may leave it.</summary>
		public static bool TryId(JsonNode node, out uint id)
		{
			id = 0;
			if (node is not JsonValue value) return false;
			if (value.TryGetValue(out uint number)) { id = number; return id != 0; }
			if (value.TryGetValue(out string text) && uint.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out id)) return id != 0;
			return false;
		}

		/// <summary>The message file with the lines appended (an id the file has already is left as it is).</summary>
		public static byte[] Compose(byte[] msd, IReadOnlyDictionary<uint, string> lines, List<string> notes = null)
		{
			if (msd == null || lines == null || lines.Count == 0) return msd;
			MsdFile file;
			try { file = Msd.Read(msd); }
			catch (Exception ex) { notes?.Add("msd: " + ex.Message); return msd; }
			HashSet<uint> have = new HashSet<uint>(file.Messages.Select(m => m.Id));
			bool any = false;
			foreach (KeyValuePair<uint, string> pair in lines.OrderBy(p => p.Key))
			{
				if (have.Contains(pair.Key)) continue;
				file.Messages.Add(new MsdMessage { Id = pair.Key, Pages = new List<string> { pair.Value } });
				any = true;
			}
			return any ? Msd.Write(file) : msd;
		}

		public static bool IsPermanent(string name) => string.Equals(Path.GetFileName(name ?? ""), "eureka_permanent.msd", StringComparison.OrdinalIgnoreCase);
	}
}
