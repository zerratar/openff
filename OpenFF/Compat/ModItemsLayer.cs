// The mods' item definitions (defs/items/*.json in each enabled mod and in the --project)
// composed into item_parameter.pak and eureka_item.msd as the game reads them, through
// the content chain's transform step. With no definitions anywhere the step is not even
// registered: FF3 reads its files as shipped. FF3 only for now - FF4 keeps its items in
// other files (Ff4Tables) and gets its own composer when its tables are the engine's.

using System;
using System.Collections.Generic;
using System.IO;
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
				// The game keeps a copy per language (en.lproj/eureka_permanent.msd): the folder says
				// which is being read; without one, the game's setting.
				string language = FolderLanguage(name) ?? GameLanguage();
				if (!byLanguage.TryGetValue(language, out Dictionary<uint, string> picked))
					byLanguage[language] = picked = ModText.Load(roots, null, language);
				byte[] composed = ModText.Compose(data, picked);
				Log.Write(LogChannel.File, "text: eureka_permanent.msd composed (" + language + "), " + data.Length + " -> " + composed.Length + " bytes");
				return composed;
			});
		}

		/// <summary>"de.lproj/eureka_permanent.msd" -> "de"; null when the name has no language folder.</summary>
		private static string FolderLanguage(string name)
		{
			string folder = Path.GetDirectoryName((name ?? "").Replace('\\', '/'));
			if (string.IsNullOrEmpty(folder)) return null;
			string last = Path.GetFileName(folder);
			return last.EndsWith(".lproj", StringComparison.OrdinalIgnoreCase) && last.Length > 6 ? last.Substring(0, last.Length - 6) : null;
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

		/// <summary>The mods' monsters and formations in play, in load order.</summary>
		public static IReadOnlyList<ModMonster> Monsters { get; private set; } = new List<ModMonster>();
		public static IReadOnlyList<ModFormation> Formations { get; private set; } = new List<ModFormation>();

		/// <summary>Whether a monster party id is one of the mods' formations (the map's encounter tables may name one).</summary>
		public static bool HasFormation(int number)
		{
			foreach (ModFormation f in Formations) if (f.Number == number) return true;
			return false;
		}

		/// <summary>
		/// The mods' monsters (defs/monsters) into monster.chaindata and eureka_battle.msd, their
		/// formations (defs/formations) into monster_party_table.bbd, as the game reads them; and a
		/// mod monster's texture answered with the one it wears (Shared/Data/ModMonsters.cs).
		/// </summary>
		private static void RegisterMonsters(ContentChain chain, List<string> roots)
		{
			List<string> notes = new List<string>();
			List<ModMonster> monsters = ModMonsters.Load(roots, notes);
			List<ModFormation> formations = ModMonsters.LoadFormations(roots, notes);
			foreach (string note in notes) Log.Write(LogChannel.General, "monsters: " + note);
			HashSet<int> numbers = new HashSet<int>();
			List<ModMonster> kept = new List<ModMonster>();
			foreach (ModMonster m in monsters)
			{
				if (!numbers.Add(m.Number)) { Log.Write(LogChannel.General, "monsters: " + m.Id + " (" + m.Source + ") has number " + m.Number + ", already taken - skipped"); continue; }
				kept.Add(m);
			}
			HashSet<int> parties = new HashSet<int>();
			List<ModFormation> keptFormations = new List<ModFormation>();
			foreach (ModFormation f in formations)
			{
				if (!parties.Add(f.Number)) { Log.Write(LogChannel.General, "monsters: formation " + f.Id + " (" + f.Source + ") has number " + f.Number + ", already taken - skipped"); continue; }
				keptFormations.Add(f);
			}
			Monsters = kept;
			Formations = keptFormations;
			if (kept.Count == 0 && keptFormations.Count == 0) return;
			if (kept.Count > 0) Log.Write(LogChannel.General, "monsters: " + kept.Count + " of the mods' own: " + string.Join(", ", kept.Select(m => m.Number + " " + (m.Name ?? m.Id) + " (from " + m.Base + ")")));
			if (keptFormations.Count > 0) Log.Write(LogChannel.General, "monsters: " + keptFormations.Count + " formation(s): " + string.Join(", ", keptFormations.Select(f => f.Number + " " + (f.Name ?? f.Id))));
			chain.AddTransform((name, data) =>
			{
				if (kept.Count > 0 && ModMonsters.IsChain(name))
				{
					List<string> problems = new List<string>();
					byte[] composed = ModMonsters.ComposeChain(data, kept, problems);
					foreach (string p in problems) Log.Write(LogChannel.General, "monsters: " + p);
					Log.Write(LogChannel.File, "monsters: monster.chaindata composed, " + data.Length + " -> " + composed.Length + " bytes");
					return composed;
				}
				if (kept.Count > 0 && ModMonsters.IsMsd(name))
				{
					byte[] composed = ModMonsters.ComposeMsd(data, kept);
					Log.Write(LogChannel.File, "monsters: " + name + " composed, " + data.Length + " -> " + composed.Length + " bytes");
					return composed;
				}
				if (keptFormations.Count > 0 && ModMonsters.IsParties(name))
				{
					byte[] composed = ModMonsters.ComposeParties(data, keptFormations);
					Log.Write(LogChannel.File, "monsters: monster_party_table.bbd composed, " + data.Length + " -> " + composed.Length + " bytes");
					return composed;
				}
				return data;
			});
			if (kept.Count > 0) chain.AddAlias(name => ModMonsters.TextureAlias(name, kept));
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
			RegisterMonsters(chain, roots);
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
			WeaponMeshes.Register(kept);
			if (kept.Count == 0) return;
			Log.Write(LogChannel.General, "items: " + kept.Count + " of the mods' own: " + string.Join(", ", kept.Select(i => i.Number + " " + (i.Name ?? i.Id) + (i.Model != null ? " (" + i.Model + ")" : ""))));
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
