// FF4's names and layouts for the 2D assets the FF3 logic asks for by FF3's names.
//
// The FF3 logic loads its window frame as m000_window (a cell bank whose cells 1-10 are
// the corners and edges, cell 0 the wallpaper) and its icons and buttons by FF3's names.
// FF4 has the same pictures in MENU_Common.dat under other names, cut differently:
// window_frame_00 keeps three frame styles as eight cells each - corner, left edge,
// corner, top edge, corner, right edge, corner, bottom edge - at the size FF3 draws its
// own (16-pixel corners, 64-pixel edges). So when the content is FF4, a request for an
// FF3 name is answered with FF4's file, and the loaded bank is rearranged into the cell
// order FF3's window code indexes. Nothing is shipped for this; it is a table of names.

using System;
using System.Collections.Generic;

namespace FF3
{
	internal static class Ff4Assets
	{
		private static readonly Dictionary<string, string> _names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ "m000_window.NCER", "window_frame_00.NCER" },
			{ "m000_window.NCBR", "window_frame_00.NCGR" },
			{ "m000_window.NCGR", "window_frame_00.NCGR" },
		};

		private static readonly HashSet<string> _reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>FF4's name for an FF3 2D asset, or the name unchanged.</summary>
		public static string MapName(string name)
		{
			if (string.IsNullOrEmpty(name) || !GameProfile.IsFf4)
			{
				return name;
			}
			return _names.TryGetValue(name, out string mapped) ? mapped : name;
		}

		/// <summary>
		/// Rearranges a bank loaded under FF4's name into the cell order the FF3 code
		/// expects for the FF3 asset it stands in for. No-op for anything else.
		/// </summary>
		public static void RemapCells(string loadedName, GlobalScope.NNSG2dCellDataBank bank)
		{
			if (bank == null || bank.pCellDataArrayHead == null || !GameProfile.IsFf4 || string.IsNullOrEmpty(loadedName))
			{
				return;
			}
			if (string.Equals(System.IO.Path.GetFileName(loadedName), "window_frame_00.NCER", StringComparison.OrdinalIgnoreCase))
			{
				// FF3's m000_window: 0 wallpaper (none here), 1 top-left, 2 left edge, 3 left
				// edge's lower piece, 4 bottom-left, 5 top edge, 6 top edge's end, 7 top-right,
				// 8 right edge, 9 bottom-right, 10 bottom edge. FF4's first style: 0 top-left,
				// 1 left edge, 2 bottom-left, 3 top edge, 4 top-right, 5 right edge,
				// 6 bottom-right, 7 bottom edge.
				int[] order = { -1, 0, 1, 1, 2, 3, 3, 4, 5, 6, 7 };
				Rebuild(bank, order);
				Report(loadedName, "FF3's window frame order");
			}
		}

		private static void Rebuild(GlobalScope.NNSG2dCellDataBank bank, int[] order)
		{
			GlobalScope.NNSG2dCellData[] source = bank.pCellDataArrayHead;
			GlobalScope.NNSG2dCellData[] cells = new GlobalScope.NNSG2dCellData[order.Length];
			for (int i = 0; i < order.Length; i++)
			{
				int from = order[i];
				if (from >= 0 && from < source.Length && source[from] != null)
				{
					cells[i] = source[from];
				}
				else
				{
					cells[i] = new GlobalScope.NNSG2dCellData
					{
						numOAMAttrs = 0,
						cellAttr = 0,
						pOamAttrArray = new GlobalScope.NNSG2dCellOAMAttrData[0]
					};
				}
			}
			bank.pCellDataArrayHead = cells;
			bank.numCells = (ushort)cells.Length;
		}

		private static void Report(string name, string what)
		{
			lock (_reported)
			{
				if (_reported.Add(name))
				{
					Log.Write(LogChannel.File, "ff4 assets: " + name + " rearranged into " + what);
				}
			}
		}
	}
}
