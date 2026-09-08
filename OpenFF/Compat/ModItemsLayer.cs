// The mods' item definitions (defs/items/*.json in each enabled mod and in the --project)
// composed into item_parameter.pak and eureka_item.msd as the game reads them, through
// the content chain's transform step. With no definitions anywhere the step is not even
// registered: FF3 reads its files as shipped. FF3 only for now - FF4 keeps its items in
// other files (Ff4Tables) and gets its own composer when its tables are the engine's.

using System;
using System.Collections.Generic;
using System.Linq;
using OpenFF.Content;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal static class ModItemsLayer
	{
		/// <summary>The mods' text (defs/text/*.json), composed into eureka_permanent.msd - the file every map falls back to - as it is read.</summary>
		private static void RegisterText(ContentChain chain, List<string> roots)
		{
			List<string> notes = new List<string>();
			Dictionary<uint, string> lines = ModText.Load(roots, notes);
			foreach (string note in notes) Log.Write(LogChannel.General, "text: " + note);
			if (lines.Count == 0) return;
			Log.Write(LogChannel.General, "text: " + lines.Count + " line(s) of the mods' own (" + lines.Keys.Min() + ".." + lines.Keys.Max() + ")");
			// A line written per language follows the game's language setting, which the player
			// may change after this runs: the lines are picked per language the first time the
			// file is read in it (the map's load), English by default.
			Dictionary<string, Dictionary<uint, string>> byLanguage = new Dictionary<string, Dictionary<uint, string>> { ["en"] = lines };
			chain.AddTransform((name, data) =>
			{
				if (!ModText.IsPermanent(name)) return data;
				string language = GameLanguage();
				if (!byLanguage.TryGetValue(language, out Dictionary<uint, string> picked))
					byLanguage[language] = picked = ModText.Load(roots, null, language);
				byte[] composed = ModText.Compose(data, picked);
				Log.Write(LogChannel.File, "text: eureka_permanent.msd composed (" + language + "), " + data.Length + " -> " + composed.Length + " bytes");
				return composed;
			});
		}

		/// <summary>The game's language setting as a code the text files use: en, ja, fr, de, it, es, zh-CN, zh-TW, ko.</summary>
		private static string GameLanguage()
		{
			string[] codes = { "ja", "en", "fr", "de", "it", "es", "zh-CN", "zh-TW", "ko" };
			try
			{
				int index = AppShell.getLanguage();
				return index >= 0 && index < codes.Length ? codes[index] : "en";
			}
			catch (Exception) { return "en"; }
		}

		/// <summary>The definitions in play, in load order (a --project's first, then the mods').</summary>
		public static IReadOnlyList<ModItem> Items { get; private set; } = new List<ModItem>();

		public static void Register(ContentChain chain)
		{
			// --nomods: the game as shipped, definitions included - what Tools/parity.ps1 compares against.
			if (chain == null || chain.Game != "ff3" || Options.Get("nomods") != null) return;
			List<string> roots = new List<string>();
			if (!string.IsNullOrEmpty(GameArchive.ProjectDirectory)) roots.Add(GameArchive.ProjectDirectory);
			roots.AddRange(GameArchive.ActiveMods.Select(m => m.Directory).Where(d => !string.IsNullOrEmpty(d)));
			RegisterText(chain, roots);
			List<string> notes = new List<string>();
			List<ModItem> items = ModItems.Load(roots, notes);
			foreach (string note in notes) Log.Write(LogChannel.General, "items: " + note);
			// Two definitions with one number: the first loaded keeps it, as a mod's files do.
			HashSet<int> numbers = new HashSet<int>();
			List<ModItem> kept = new List<ModItem>();
			foreach (ModItem item in items)
			{
				if (!numbers.Add(item.Number)) { Log.Write(LogChannel.General, "items: " + item.Id + " (" + item.Source + ") has number " + item.Number + ", already taken - skipped"); continue; }
				kept.Add(item);
			}
			Items = kept;
			if (kept.Count == 0) return;
			Log.Write(LogChannel.General, "items: " + kept.Count + " of the mods' own: " + string.Join(", ", kept.Select(i => i.Number + " " + (i.Name ?? i.Id))));
			chain.AddTransform((name, data) =>
			{
				if (ModItems.IsPak(name))
				{
					List<string> problems = new List<string>();
					byte[] composed = ModItems.ComposePak(data, kept, problems);
					foreach (string p in problems) Log.Write(LogChannel.General, "items: " + p);
					Log.Write(LogChannel.File, "items: item_parameter.pak composed, " + data.Length + " -> " + composed.Length + " bytes");
					return composed;
				}
				if (ModItems.IsMsd(name))
				{
					byte[] composed = ModItems.ComposeMsd(data, kept);
					Log.Write(LogChannel.File, "items: eureka_item.msd composed, " + data.Length + " -> " + composed.Length + " bytes");
					return composed;
				}
				return data;
			});
		}
	}
}
