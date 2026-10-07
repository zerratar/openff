// FF4's field menu drawn as a layout (LayoutScreen "ff4_menu": Data/hud/ff4_menu.xml and styles/ff4_menu.css), as the
// battle HUD is - Ff4Menu keeps the menu's logic and fills Data, the layout draws it, and a mod reshapes or restyles it
// with menus/hud/. The paths the layout binds are listed in its header.

using System;
using System.Collections.Generic;

namespace OpenFF.Client
{
	internal static class Ff4MenuHud
	{
		public const string ScreenId = "ff4_menu";

		public sealed class CommandRow
		{
			public bool Present, Lit, Disabled;
			public string Name = "";
		}

		public sealed class MemberRow
		{
			public bool Present, Lit, Dim, Low;
			public string Name = "", Level = "", Hp = "", MaxHp = "", Mp = "", MaxMp = "";
			public int Face;
		}

		/// <summary>A cell of a list (an item, a spell): its symbol's cell, its name, how many.</summary>
		public sealed class CellRow
		{
			public bool Present, Lit, Dim;
			public string Name = "", Count = "";
			public int Icon = -1;
		}

		public sealed class ScrollData
		{
			public bool Shown, Knob;
			public float KnobTop, KnobHeight;
		}

		public sealed class Data
		{
			public bool Root, Picking, Bubble, Question, Yes, No;
			// The full screens: which is up, its title, its help line; Inventory's list and its use on a member; Magic's member
			// and spells.
			public bool Full, Inventory, Magic, Using;
			public string Title = "", Help = "", UseName = "", UseCount = "";
			public string KeyItemsLabel = "Key Items", SortLabel = "Sort", UseLabel = "Use", ChangeLabel = "Change Characters", SchoolLabel = "";
			public int UseIcon = -1;
			public List<CellRow> Item = Rows<CellRow>(14), Spell = Rows<CellRow>(15);
			public ScrollData ItemScroll = new ScrollData(), SpellScroll = new ScrollData();
			public List<MemberRow> Target = Rows<MemberRow>(5);
			public MemberRow Head = new MemberRow();
			public string Thought = "", Location = "", Gil = "", GilLabel = "Gil";
			public string LvLabel = "Lv", HpLabel = "HP", MpLabel = "MP", Confirm = "Confirm", Back = "Back";
			public string QuestionText = "", YesLabel = "Yes", NoLabel = "No";
			public List<CommandRow> Command = new List<CommandRow> { new CommandRow(), new CommandRow(), new CommandRow(), new CommandRow(), new CommandRow(), new CommandRow() };
			public ScrollData Scroll = new ScrollData();
			public List<MemberRow> Member = new List<MemberRow> { new MemberRow(), new MemberRow(), new MemberRow(), new MemberRow(), new MemberRow() };
		}

		private static List<T> Rows<T>(int n) where T : new()
		{
			List<T> rows = new List<T>();
			for (int i = 0; i < n; i++) rows.Add(new T());
			return rows;
		}

		private static LayoutScreen _screen;
		private static bool _loaded;

		/// <summary>Whether the layout is there to draw the menu with.</summary>
		public static bool Available
		{
			get
			{
				if (!_loaded)
				{
					_loaded = true;
					Ff4Ui.RegisterLayoutCells();
					_screen = LayoutScreen.Load(ScreenId);
				}
				return _screen != null;
			}
		}

		// A scroll bar's track starts under its up arrow, 33.75 down, and ends over its down arrow: the command column's is 742.5
		// long (a bar 810 high), the Inventory's 675 (742.5), Magic's 526.5 (594).
		private const float TrackTop = 33.75f;

		/// <summary>The scroll bar for <paramref name="count"/> rows, <paramref name="visible"/> of them in view from <paramref name="top"/>,
		/// on a track <paramref name="track"/> long. A list's bar stands always (<paramref name="always"/>), its knob only when it
		/// scrolls; the command column's comes and goes with its knob.</summary>
		public static void Scroll(ScrollData s, int count, int visible, int top, float track = 742.5f, bool always = false)
		{
			s.Knob = count > visible;
			s.Shown = s.Knob || always;
			if (!s.Knob) return;
			s.KnobHeight = track * visible / count;
			s.KnobTop = TrackTop + (track - s.KnobHeight) * top / Math.Max(1, count - visible);
		}

		public static void Draw(Data data)
		{
			if (!Available) return;
			_screen.Draw(Game.Draw, name => string.Equals(name, "menu", StringComparison.OrdinalIgnoreCase)
				? (true, data)
				: (Game.Hud.TryGet(name, out object hud) ? (true, hud) : (false, null)));
		}
	}
}
