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
		/// <summary>The definitions in play, in load order (a --project's first, then the mods').</summary>
		public static IReadOnlyList<ModItem> Items { get; private set; } = new List<ModItem>();

		public static void Register(ContentChain chain)
		{
			// --nomods: the game as shipped, definitions included - what Tools/parity.ps1 compares against.
			if (chain == null || chain.Game != "ff3" || Options.Get("nomods") != null) return;
			List<string> roots = new List<string>();
			if (!string.IsNullOrEmpty(GameArchive.ProjectDirectory)) roots.Add(GameArchive.ProjectDirectory);
			roots.AddRange(GameArchive.ActiveMods.Select(m => m.Directory).Where(d => !string.IsNullOrEmpty(d)));
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
