// The phone's menu code over Steam's menu layouts.
//
// The menus are built from MenuDefine.xbn and its siblings. The Steam build rewrote
// those for its desktop UI: the main menu's commands, for instance, moved from the
// phone's two columns of 96x56 frames to one column of 96x28 frames inside a panel. The
// phone code that positions the hand cursor and the text was written for the phone's
// frames - the hand sits 14 px inside a command frame because the phone left a margin
// there, and text hangs from the frame's top because the phone frames were tall and the
// text sat where it was drawn. Over Steam's frames that puts the hand on top of the word
// and the word above the middle of its row.
//
// So when the content is a Steam FF3 install, two small rules apply: text is centred in
// its frame's height when the frame is taller than the line, and the hand cursor stands
// wholly to the left of what it points at. Nothing here changes the layouts themselves,
// which stay Steam's.

using System;

namespace OpenFF.Client
{
	internal static class SteamLayout
	{
		/// <summary>Whether the menus come from a Steam FF3 install.</summary>
		public static bool Active => SteamCells.SteamFf3;

		/// <summary>The line height of the message fonts, by the font index the menus use.</summary>
		public static int LineHeight(int font) => font == 0 ? 16 : 12;

		/// <summary>
		/// How far down from the frame's top a line of text goes to sit in the middle of
		/// the frame: 0 when not on Steam's layouts, or when the frame is no taller than
		/// the line (the phone's tall frames are not centred, and neither are Steam's
		/// frames that were sized to their text).
		/// </summary>
		public static int TextDrop(int frameHeight, int font)
		{
			if (!Active)
			{
				return 0;
			}
			int line = LineHeight(font);
			return frameHeight > line + 2 ? (frameHeight - line) / 2 : 0;
		}

		/// <summary>
		/// Steam's MenuDefine uses an alignment value the phone's enum does not have, 6, for
		/// the command lists in its side panels: text at the frame's left, the hand outside.
		/// </summary>
		public const int STEAM_ALIGN_MENU = 6;

		/// <summary>The gap between the hand's cell and the text it points at, for a command in a panel (the hand stands outside the panel's margin) and for a centred line.</summary>
		public const int MENU_GAP = 18;
		public const int CENTER_GAP = 12;

		/// <summary>
		/// Where the hand cursor's position goes, relative to a frame's left edge, so that
		/// the hand's cell ends a gap before the text begins. The cell's OAM says how far
		/// the picture extends right of the position (Steam's icon_yubi: 8 px as drawn).
		/// </summary>
		public static int CursorOffset(GlobalScope.sys2d.Cell hand, int textStart, int gap)
		{
			int right = SteamCells.CellRight(hand, 0, 8);
			return textStart - right - gap;
		}

		/// <summary>
		/// How far the hand's position moves up from the middle of its frame so that the
		/// hand, not its shadow, is level with the text: Steam's icon_yubi picture sits low
		/// in its cell (measured: the hand's middle is 7 px below the position).
		/// </summary>
		public static int CursorLift => Active ? -7 : 0;

		/// <summary>Diagnostic: one log line per cursor placement, with the frame and the hand's first OAM.</summary>
		public static void Trace(GlobalScope.menu.Medget m, GlobalScope.sys2d.Cell hand)
		{
			if (m == null || !Log.IsEnabled(LogChannel.Event))
			{
				return;
			}
			string oam = "none";
			int n = 0;
			try
			{
				GlobalScope.NNSG2dCellDataBank bank = hand?.GetCellBank();
				GlobalScope.NNSG2dCellOAMAttrData[] oams = bank?.pCellDataArrayHead?[0]?.pOamAttrArray;
				if (oams != null && oams.Length > 0)
				{
					n = oams.Length;
					short[] a = new short[7];
					oams[0].copy(a, 14);
					oam = a[0] + "," + a[1] + " " + a[2] + "x" + a[3];
				}
			}
			catch (Exception) { }
			Log.Write(LogChannel.Event, "cursor: medget " + m._id() + " at " + m.x() + "," + m.y() + " " + m.width() + "x" + m.height()
				+ " cursor " + m.cursorX() + "," + m.cursorY() + " hand oam[" + n + "] " + oam + " steam=" + Active);
		}
	}
}
