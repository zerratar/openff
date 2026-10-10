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
			public bool Present, Lit, Dim, Low, Back, Picked;
			public string Name = "", Level = "", Hp = "", MaxHp = "", Mp = "", MaxMp = "";
			public int Face;
		}

		/// <summary>A cell of a list (an item, a spell): its symbol's cell, its name, how many.</summary>
		public sealed class CellRow
		{
			public bool Present, Lit, Dim, Foe, Ally, Gold;
			public string Name = "", Count = "";
			public int Icon = -1;
		}

		/// <summary>A gambit: its number, ON / OFF, its condition and its action; which of its three cells the hand is on.</summary>
		public sealed class RuleRow
		{
			public bool Foe, Ally, On, Off, Picked, LitOn, LitCondition, LitAction;
			public string Number = "", State = "", Condition = "", Action = "";
		}

		/// <summary>A line of figures: its word and its value (Strength 13).</summary>
		public sealed class StatRow
		{
			public string Label = "", Value = "";
		}

		/// <summary>An equipment slot: its word (Right, Head...), what is worn there and its symbol.</summary>
		public sealed class SlotRow
		{
			public bool Lit, Picked;
			public string Label = "", Name = "";
			public int Icon = -1;
		}

		/// <summary>A line of Settings: its word, the hand on it, and what it holds - a slider (Music, Sound Effects, Voices: the
		/// number and where the marker stands), two choices (Battle Mode, Subtitles), Battle Speed's six steps between Fast and
		/// Slow, Window Design's six, or a button of its own (Help, Quit). Box: the choices, Lit on the one set.</summary>
		public sealed class SettingRow
		{
			public bool Present, Lit, Slider, Pair, Steps, Design, Button;
			public string Label = "", Number = "", FastLabel = "", SlowLabel = "";
			public float Marker;
			public List<CellRow> Box = Rows<CellRow>(6);
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
			// Status: the member's job, figures, experience and what they wear.
			public bool Status;
			public string Job = "", ExpLabel = "EXP", Exp = "", NextLabel = "For next level", Next = "", AbilitiesLabel = "Abilities";
			public List<StatRow> Stat = Rows<StatRow>(11);
			public List<SlotRow> Slot = Rows<SlotRow>(5);
			// Equipment: what in the bag fits the lit slot, and the screen's key words.
			public bool Equipment;
			public List<CellRow> Choice = Rows<CellRow>(5);
			public ScrollData ChoiceScroll = new ScrollData();
			public string OptimizeLabel = "Optimize", RemoveLabel = "Remove";
			// Abilities: the auto-battle command and the five battle commands (menu.slot), the hand on one, one picked up.
			public bool Abilities;
			public SlotRow Auto = new SlotRow();
			public string AutoLabel = "Auto-Battle Command", CommandsLabel = "Battle Commands";
			// Gambits: six of the member's twelve rules, and the picker's choices (two columns, six rows).
			public bool Gambits, GambitPicking;
			public List<RuleRow> Rule = Rows<RuleRow>(6);
			public ScrollData RuleScroll = new ScrollData(), PickScroll = new ScrollData();
			public List<CellRow> Pick = Rows<CellRow>(12);
			public string MoveUpLabel = "Move Up";
			// Party: the two pills (Swap Rows, Party Formation), the hand on one of them or on the member panel's places.
			public bool Party, SwapRowsLit, FormationLit;
			public string SwapRowsLabel = "Swap Rows", FormationLabel = "Party Formation";
			// Save: the slots and Title Menu, the hand on one; what the lit slot holds (its party by places, place, time, gil),
			// or "No save data found."; NoTitle: a full screen without the title bar (Party, Save).
			public bool Save, NoTitle, SlotFilled;
			public List<CommandRow> SaveRow = new List<CommandRow> { new CommandRow(), new CommandRow(), new CommandRow(), new CommandRow() };
			public List<MemberRow> SaveMember = Rows<MemberRow>(5);
			public string SavePlace = "", SaveTime = "", SaveGil = "", NoDataLabel = "No save data found.";
			// Settings: five of its nine lines in view, and the scroll bar; Design the window design (1..6) the screens wear.
			public bool Settings, SettingConfirm;
			public List<SettingRow> Setting = Rows<SettingRow>(5);
			public ScrollData SettingScroll = new ScrollData();
			public int Design = 1;
			public string DesignClass = "design-1";
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
			// The window design the settings chose: the layout's design-N class on every screen (MENU_Common's set N - 1).
			data.Design = Ff4Settings.Current.WindowDesign;
			data.DesignClass = "design-" + data.Design;
			_screen.Draw(Game.Draw, name => string.Equals(name, "menu", StringComparison.OrdinalIgnoreCase)
				? (true, data)
				: (Game.Hud.TryGet(name, out object hud) ? (true, hud) : (false, null)));
		}
	}
}
