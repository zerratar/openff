// The battle's HUD as a layout: the command window's rows, the Run Away button, the scroll
// arrows, the cursor's seat in a row, and the four party rows (name, HP, the bar under them).
//
// FF3 draws these from numbers in code (BATTLE_COMMAND_X/Y, BATTLE_PLAYER_Y, 40 a row, 16 a
// party line). The client reads them from a "battle_hud" screen in BattleDefine.xbn instead,
// and puts this one there when the file has none - so a mod that takes battle_hud into its
// menus/ (Crystal: BattleDefine ▸ battle_hud ▸ Take into the mod) moves and resizes the
// battle's windows like any other screen. The ids are what the code reads:
//
//   cmd0 cmd1 cmd2   the three visible command rows (a child "text" says where the name sits,
//                    a child "cursor" where the hand points)
//   run_away         the fourth window, the Run Away button
//   arrow_up/down    the scroll triangles beside the rows
//   player0..3       a party line each, with children "name", "hp" (right-aligned to its x + width)
//                    and "gauge" (the bar under the line)
//
// Crystal shows the same default under BattleDefine's screens so it can be taken and edited.

using System.Xml.Linq;

namespace OpenFF.Content
{
	internal static class BattleHudLayout
	{
		public const string Screen = "battle_hud";
		public const string File = "BattleDefine.xbn";

		/// <summary>The game's own geometry as a layout. Steam's build: command window at (0, 173), party lines from y 270.</summary>
		public static XElement Default(int commandX = 0, int commandY = 173, int playerY = 270)
		{
			XElement menu = new XElement("menu", new XElement("name", Screen));
			for (int i = 0; i < 3; i++)
			{
				menu.Add(Frame("cmd" + i, commandX, commandY + (i + 1) * 40, 128, 40,
					Frame("text", 8, 14, 112, 20),
					Frame("cursor", 8, 20, 0, 0)));
			}
			menu.Add(Frame("run_away", 400, 0, 80, 40, Frame("text", 8, 14, 64, 20), Frame("cursor", 8, 20, 0, 0)));
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

		private static XElement Frame(string id, int x, int y, int w, int h, params XElement[] children)
		{
			XElement f = new XElement("frame", new XElement("id", id), new XElement("x", x), new XElement("y", y), new XElement("width", w), new XElement("height", h));
			f.Add(children);
			return f;
		}

		/// <summary>Adds the default to a BattleDefine document that has no battle_hud yet. Returns true when it did.</summary>
		public static bool Ensure(XDocument document, int commandX = 0, int commandY = 173, int playerY = 270)
		{
			XElement list = document?.Root;
			if (list == null) return false;
			foreach (XElement m in list.Elements("menu"))
			{
				if ((string)m.Element("name") == Screen) return false;
			}
			list.Add(Default(commandX, commandY, playerY));
			return true;
		}
	}
}
