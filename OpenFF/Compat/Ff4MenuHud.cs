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

		public sealed class ScrollData
		{
			public bool Shown;
			public float KnobTop, KnobHeight;
		}

		public sealed class Data
		{
			public bool Root, Picking, Bubble, Question, Yes, No;
			public string Thought = "", Location = "", Gil = "", GilLabel = "Gil";
			public string LvLabel = "Lv", HpLabel = "HP", MpLabel = "MP", Confirm = "Confirm", Back = "Back";
			public string QuestionText = "", YesLabel = "Yes", NoLabel = "No";
			public List<CommandRow> Command = new List<CommandRow> { new CommandRow(), new CommandRow(), new CommandRow(), new CommandRow(), new CommandRow(), new CommandRow() };
			public ScrollData Scroll = new ScrollData();
			public List<MemberRow> Member = new List<MemberRow> { new MemberRow(), new MemberRow(), new MemberRow(), new MemberRow(), new MemberRow() };
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

		// The command column's track: 742.5 long from 33.75 down (Steam's scrollbar between its two arrows).
		private const float TrackTop = 33.75f, TrackLength = 742.5f;

		/// <summary>The scroll bar for <paramref name="count"/> rows, <paramref name="visible"/> of them in view from <paramref name="top"/>.</summary>
		public static void Scroll(ScrollData s, int count, int visible, int top)
		{
			s.Shown = count > visible;
			if (!s.Shown) return;
			s.KnobHeight = TrackLength * visible / count;
			s.KnobTop = TrackTop + (TrackLength - s.KnobHeight) * top / Math.Max(1, count - visible);
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
