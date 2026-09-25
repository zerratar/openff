// The title screen's entries from the mods (Game.Title), and the new game a mod starts from one.
//
// The title lists Continue / New Game / Load and the mod list in one column; a mod's entries go
// in rows under them, drawn as text where the title's own pictures would be (ttl.CTitle reads
// Entries as it builds its commands, ModListScreen draws the labels with its own). A press runs
// the entry's action while the title is up; an action that starts a game (Game.Title.NewGame)
// leaves a Pending order the title's New Game path then follows - the same fade, the same
// party set-up, but with the hero, map and save profile the mod asked for.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OpenFF.Client
{
	internal static class TitleEntries
	{
		public sealed class Entry
		{
			public string Label;
			public Action OnPress;
		}

		/// <summary>A new game or a continue the title carries out on its New Game / Continue path.</summary>
		public sealed class Order
		{
			public bool Continue;
			public int Hero;
			public string Map;
			public Vector3? Position;
			public string SaveProfile;
		}

		public static readonly List<Entry> Entries = new List<Entry>();
		/// <summary>What the title does next, set by a mod's entry (Game.Title.NewGame / Continue); the title clears it as it acts.</summary>
		public static Order Pending;
		/// <summary>Whether the title's command list is up (between SetUpMainSelect and leaving).</summary>
		public static bool Showing;

		/// <summary>How many entries the title shows: in its one column, under its own commands, two fit above the copyright line.</summary>
		public const int MaxShown = 2;

		public static void Add(string label, Action onPress)
		{
			if (string.IsNullOrWhiteSpace(label) || onPress == null) return;
			Entries.Add(new Entry { Label = label.Trim(), OnPress = onPress });
			Log.Write(LogChannel.General, "title: entry '" + label.Trim() + "' from a mod" + (Entries.Count > MaxShown ? " - beyond the " + MaxShown + " the title has room for; not shown" : ""));
		}

		/// <summary>The title pressed an entry: its action runs guarded.</summary>
		public static void Run(int index)
		{
			if (index < 0 || index >= Entries.Count) return;
			OpenFF.Game.Guard("title entry " + Entries[index].Label, Entries[index].OnPress);
		}

		/// <summary>The title is showing its commands (TitleShown for the mods).</summary>
		public static void Shown()
		{
			Showing = true;
			Gambits.Reset();   // a new game from here starts with the default auto-battle rules; a save brings its own
			try { OpenFF.Game.Events.Publish(new OpenFF.Events.TitleShown()); } catch (Exception ex) { Log.Write(LogChannel.General, "title: " + ex.Message); }
		}

		public static void Left() => Showing = false;
	}
}
