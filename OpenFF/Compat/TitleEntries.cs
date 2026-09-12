// The title screen's entries from the mods (Game.Title), and the new game a mod starts from one.
//
// The title lists New Game / Load / Continue and the mod list in two columns; a mod's entries go
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

		/// <summary>The entries' row: two side by side under the title's own two rows (moved up to 204 and 236 for it); one row fits above the copyright line, so two entries show.</summary>
		public const int FirstRowY = 268;
		public const int RowStep = 32;
		public const int MaxShown = 2;
		public static int ColumnX(int index) => index % 2 == 0 ? GlobalScope.ttl.NEW_GAME_POS_X : GlobalScope.ttl.CONTINUE_POS_X;
		public static int RowY(int index) => FirstRowY + (index / 2) * RowStep;

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
			try { OpenFF.Game.Events.Publish(new OpenFF.Events.TitleShown()); } catch (Exception ex) { Log.Write(LogChannel.General, "title: " + ex.Message); }
		}

		public static void Left() => Showing = false;
	}
}
