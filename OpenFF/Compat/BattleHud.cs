// The battle's HUD geometry from the "battle_hud" screen of BattleDefine.xbn (Shared's
// BattleHudLayout puts the game's own numbers there when the file has none; a mod's copy
// replaces them). The game's code asks here instead of its constants: the command window's
// rows, the Run Away button, the cursor's seat, the scroll arrows, the party lines.
//
// Read once per load of the file (MenuManager.LoadXbnFile tells us), straight off the XBN
// node tree the game holds - no Medgets, since the battle never builds this screen: it only
// needs the rectangles.

using System;
using System.Collections.Generic;
using OpenFF.Content;

namespace OpenFF.Client
{
	internal static class BattleHud
	{
		public struct Rect
		{
			public int X, Y, Width, Height;
			public bool Found;
		}

		private static Dictionary<string, Rect> _frames;   // "screen/path" -> rectangle, from the layout file the game holds
		private static bool _noted;

		/// <summary>The file was loaded again: read the geometry afresh next time.</summary>
		public static void Invalidate() { _frames = null; }

		/// <summary>A frame of battle_hud by id ("cmd1", or "player2/hp" for a child), absolute; Found false when the layout lacks it.</summary>
		public static Rect Frame(string path) => Frame(BattleHudLayout.Screen, path);

		/// <summary>A frame of any screen in the layout file the game holds now, by screen and id path.</summary>
		public static Rect Frame(string screen, string path)
		{
			try
			{
				if (_frames == null) Read();
				if (_frames != null && _frames.TryGetValue(screen + "/" + path, out Rect r)) return r;
			}
			catch (Exception ex) { if (!_noted) { _noted = true; Log.Write(LogChannel.General, "battle hud: " + ex.Message); } }
			return default;
		}

		/// <summary>A frame, or the fallback rectangle when the layout has none (the game's own numbers).</summary>
		public static Rect Frame(string path, int x, int y, int w, int h) => Frame(BattleHudLayout.Screen, path, x, y, w, h);

		public static Rect Frame(string screen, string path, int x, int y, int w, int h)
		{
			Rect r = Frame(screen, path);
			return r.Found ? r : new Rect { X = x, Y = y, Width = w, Height = h, Found = false };
		}

		private static void Read()
		{
			_frames = new Dictionary<string, Rect>(StringComparer.OrdinalIgnoreCase);
			GlobalScope.XbnNode root = GlobalScope.menu.MenuManager.getSingleton()?.xbnRoot();
			if (root == null) { _frames = null; return; }
			// Every screen of the file: the ones the code reads (battle_hud, field_hud) are few, and the walk is cheap.
			for (GlobalScope.XbnNode menu = root.firstChild(); menu != null; menu = menu.nextSibling())
			{
				if (menu.nodeName() != "menu") continue;
				string screen = menu.getFirstNodeByTagNameFromChildren("name")?.nodeValueString();
				if (string.IsNullOrEmpty(screen)) continue;
				for (GlobalScope.XbnNode frame = menu.firstChild(); frame != null; frame = frame.nextSibling())
				{
					if (frame.nodeName() == "frame") Walk(frame, screen, 0, 0);
				}
			}
		}

		private static void Walk(GlobalScope.XbnNode frame, string parentPath, int px, int py)
		{
			string id = frame.getFirstNodeByTagNameFromChildren("id")?.nodeValueString();
			if (string.IsNullOrEmpty(id)) return;
			int x = px + Int(frame, "x"), y = py + Int(frame, "y");
			string path = parentPath == null ? id : parentPath + "/" + id;
			_frames[path] = new Rect { X = x, Y = y, Width = Int(frame, "width"), Height = Int(frame, "height"), Found = true };
			for (GlobalScope.XbnNode child = frame.firstChild(); child != null; child = child.nextSibling())
			{
				if (child.nodeName() == "frame") Walk(child, path, x, y);
			}
		}

		private static int Int(GlobalScope.XbnNode frame, string tag)
		{
			GlobalScope.XbnNode n = frame.getFirstNodeByTagNameFromChildren(tag);
			if (n == null) return 0;
			try { return n.nodeValueInt(); } catch (Exception) { return int.TryParse(n.nodeValueString(), out int v) ? v : 0; }
		}

		// ---- what the game's code asks ----

		/// <summary>Command row i (0..2), or the Run Away window (3).</summary>
		public static Rect CommandRow(int i) => i < 3
			? Frame("cmd" + i, GlobalScope.BATTLE_COMMAND_X(), GlobalScope.BATTLE_COMMAND_Y() + (i + 1) * 40, 128, 40)
			: Frame("run_away", 400, 0, 80, 40);

		/// <summary>Where a row's name sits, relative to the row.</summary>
		public static (int X, int Y) CommandTextOffset(int i)
		{
			Rect row = CommandRow(i);
			Rect text = Frame((i < 3 ? "cmd" + i : "run_away") + "/text");
			return text.Found ? (text.X - row.X, text.Y - row.Y) : (8, 14);
		}

		/// <summary>Where the hand points in a row, absolute.</summary>
		public static (int X, int Y) CommandCursor(int i)
		{
			Rect row = CommandRow(i);
			Rect cursor = Frame((i < 3 ? "cmd" + i : "run_away") + "/cursor");
			if (cursor.Found) return (cursor.X, cursor.Y);
			return i < 3 ? (row.X + 8, row.Y + 20) : (408, 20);
		}

		/// <summary>The scroll triangle: 0 the up one, 1 the down one.</summary>
		public static (int X, int Y) Arrow(int i)
		{
			Rect a = Frame(i == 0 ? "arrow_up" : "arrow_down", GlobalScope.BATTLE_COMMAND_X() + 128, GlobalScope.BATTLE_COMMAND_Y() + (i == 0 ? 38 : 98), 16, 16);
			return (a.X, a.Y);
		}

		/// <summary>A party line's name position.</summary>
		public static (int X, int Y) PlayerName(int i)
		{
			Rect r = Frame("player" + i + "/name", 284, GlobalScope.BATTLE_PLAYER_Y() + i * 16, 136, 16);
			return (r.X, r.Y);
		}

		/// <summary>A party line's HP position (the game right-aligns the number to it).</summary>
		public static (int X, int Y) PlayerHp(int i)
		{
			Rect r = Frame("player" + i + "/hp", 424, GlobalScope.BATTLE_PLAYER_Y() + i * 16, 52, 16);
			return (r.X, r.Y);
		}

		/// <summary>The help line's window: centre and size, the wide or the small one.</summary>
		public static (int CX, int CY, int W, int H) Help(bool small)
		{
			Rect r = small ? Frame("help_small", 84, 4, 392, 24) : Frame("help", 4, 4, 472, 24);
			return (r.X + r.Width / 2, r.Y + r.Height / 2, r.Width, r.Height);
		}

		// ---- the field's windows (field_hud in WorldDefine.xbn) ----

		/// <summary>The dialogue window's rectangle.</summary>
		public static Rect Dialogue() => Frame(BattleHudLayout.FieldScreen, "dialogue", 5, 233, 470, 84);
		/// <summary>Where the dialogue's first line starts.</summary>
		public static (int X, int Y) DialogueText() { Rect r = Frame(BattleHudLayout.FieldScreen, "dialogue/text", 16, 246, 448, 60); return (r.X, r.Y); }
		/// <summary>Where the speaker's name sits.</summary>
		public static (int X, int Y) DialogueName() { Rect r = Frame(BattleHudLayout.FieldScreen, "dialogue/name", 24, 139, 200, 20); return (r.X, r.Y); }
		/// <summary>Where the page-turn icon sits.</summary>
		public static (int X, int Y) DialogueNext() { Rect w = Dialogue(); Rect r = Frame(BattleHudLayout.FieldScreen, "dialogue/next", w.X + w.Width - 28, w.Y + w.Height - 28, 24, 24); return (r.X, r.Y); }
		/// <summary>The map-name window's rectangle (the game centres its text in it).</summary>
		public static Rect MapName() => Frame(BattleHudLayout.FieldScreen, "map_name", 4, 4, 472, 28);

		/// <summary>A party line's bar: corner 0 the top-left, 1 the bottom-right.</summary>
		public static (int X, int Y) PlayerGauge(int i, int corner)
		{
			Rect r = Frame("player" + i + "/gauge", 282, GlobalScope.BATTLE_PLAYER_Y() - 2 + i * 16, 194, 16);
			return corner == 0 ? (r.X, r.Y) : (r.X + r.Width, r.Y + r.Height);
		}
	}
}
