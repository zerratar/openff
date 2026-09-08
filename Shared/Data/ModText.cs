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

		/// <summary>Every line under the roots' defs/text, by id; the first root's wins a clash.</summary>
		public static Dictionary<uint, string> Load(IEnumerable<string> roots, List<string> notes = null)
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
								string text = pair.Value?.GetValue<string>();
								if (text == null) continue;
								if (!lines.ContainsKey(id)) lines[id] = text; else notes?.Add(file + ": " + id + " is defined already; kept the first");
							}
						}
						else if (node is JsonArray a)
						{
							foreach (JsonNode item in a)
							{
								uint id = item?["id"]?.GetValue<uint>() ?? 0;
								string text = item?["text"]?.GetValue<string>();
								if (id == 0 || text == null) continue;
								if (!lines.ContainsKey(id)) lines[id] = text; else notes?.Add(file + ": " + id + " is defined already; kept the first");
							}
						}
					}
					catch (Exception ex) { notes?.Add(file + ": " + ex.Message); }
				}
			}
			return lines;
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
