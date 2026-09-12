// The game's code-drawn windows as layouts: the battle's HUD in BattleDefine.xbn and the
// field's dialogue window and map-name window in WorldDefine.xbn.
//
// FF3 draws these from numbers in code (BATTLE_COMMAND_X/Y, BATTLE_PLAYER_Y, 40 a row; the
// dialogue at (5, 233) 470 x 84; the map name centred at (240, 18)). The client reads them from
// a screen in the layout file instead, and puts these defaults there when the file has none -
// so a mod that takes the screen into its menus/ (Crystal: the file ▸ the screen ▸ Take into
// the mod) moves and resizes the windows like any other screen. The ids are what the code reads.
//
// battle_hud (BattleDefine.xbn)
//   cmd0 cmd1 cmd2   the three visible command rows (children "text": where the name sits,
//                    "cursor": where the hand points)
//   run_away         the fourth window, the Run Away button (the same children)
//   help/help_small  the help line across the top, its text centred (two widths the code picks between)
//   arrow_up/down    the scroll triangles beside the rows
//   player0..3       a party line each (children "name", "hp" - the number is right-aligned to
//                    its x - and "gauge", the bar under the line)
//
// field_hud (WorldDefine.xbn)
//   dialogue         the talk window (children "text": the first line's corner, "name": the
//                    speaker's name above, "next": the page-turn icon)
//   map_name         the map's name as one arrives, its text centred in the frame
//
// Crystal shows the same defaults under the files' screens so they can be taken and edited.

using System;
using System.Xml.Linq;

namespace OpenFF.Content
{
	internal static class BattleHudLayout
	{
		public const string Screen = "battle_hud";
		public const string File = "BattleDefine.xbn";
		public const string FieldScreen = "field_hud";
		public const string FieldFile = "WorldDefine.xbn";

		/// <summary>Whether a layout file gets a screen of the client's.</summary>
		public static bool HasVirtual(string file) => string.Equals(file, File, StringComparison.OrdinalIgnoreCase) || string.Equals(file, FieldFile, StringComparison.OrdinalIgnoreCase);

		/// <summary>The battle's geometry as a layout. The game's own numbers on a 480 x 320 screen: command window at (16, 157), party lines from y 254.</summary>
		public static XElement Default(int commandX = 16, int commandY = 157, int playerY = 254)
		{
			XElement menu = new XElement("menu", new XElement("name", Screen));
			for (int i = 0; i < 3; i++)
			{
				menu.Add(Frame("cmd" + i, commandX, commandY + (i + 1) * 40, 128, 40,
					Frame("text", 8, 14, 112, 20),
					Frame("cursor", 8, 20, 0, 0)));
			}
			menu.Add(Frame("run_away", 400, 0, 80, 40, Frame("text", 8, 14, 64, 20), Frame("cursor", 8, 20, 0, 0)));
			menu.Add(Frame("help", 4, 4, 472, 24));          // the help line across the top (the ability's name, a monster's), its text centred
			menu.Add(Frame("help_small", 84, 4, 392, 24));   // the shorter one some screens use
			menu.Add(Frame("arrow_up", commandX + 128, commandY + 38, 16, 16));
			menu.Add(Frame("arrow_down", commandX + 128, commandY + 98, 16, 16));
			for (int i = 0; i < 4; i++)
			{
				menu.Add(Frame("player" + i, 284, playerY + i * 16, 192, 16,
					Frame("name", 0, 0, 136, 16),
					Frame("hp", 140, 0, 52, 16),
					Frame("gauge", -2, -2, 194, 16)));
			}
			return menu;
		}

		/// <summary>The field's dialogue and map-name windows as a layout, the game's own numbers.</summary>
		public static XElement FieldDefault()
		{
			XElement menu = new XElement("menu", new XElement("name", FieldScreen));
			menu.Add(Frame("dialogue", 5, 233, 470, 84,
				Frame("text", 11, 13, 448, 60),
				Frame("name", 19, -94, 200, 20),
				Frame("next", 442, 56, 24, 24)));
			menu.Add(Frame("map_name", 4, 4, 472, 28));
			return menu;
		}

		private static XElement Frame(string id, int x, int y, int w, int h, params XElement[] children)
		{
			XElement f = new XElement("frame", new XElement("id", id), new XElement("x", x), new XElement("y", y), new XElement("width", w), new XElement("height", h));
			f.Add(children);
			return f;
		}

		/// <summary>Adds the file's default screen to a document that has none yet. Returns true when it did.</summary>
		public static bool Ensure(string file, XDocument document, int commandX = 16, int commandY = 157, int playerY = 254)
		{
			XElement list = document?.Root;
			if (list == null) return false;
			bool battle = string.Equals(file, File, StringComparison.OrdinalIgnoreCase);
			bool field = string.Equals(file, FieldFile, StringComparison.OrdinalIgnoreCase);
			if (!battle && !field) return false;
			string screen = battle ? Screen : FieldScreen;
			foreach (XElement m in list.Elements("menu"))
			{
				if ((string)m.Element("name") == screen) return false;
			}
			list.Add(battle ? Default(commandX, commandY, playerY) : FieldDefault());
			return true;
		}

		/// <summary>The older call: BattleDefine's default.</summary>
		public static bool Ensure(XDocument document, int commandX = 16, int commandY = 157, int playerY = 254) => Ensure(File, document, commandX, commandY, playerY);
	}
}
