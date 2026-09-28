using System;
using System.Collections.Generic;
using System.Linq;

// PORT: the row lines of the menu backdrops. Every backdrop draws its rows' separating lines with one
// piece of menu_bg_01 - a 16-pixel strip from its row 96 (half size, so a line of the screen's) - laid
// at the heights of the rows the game's own layout has. When the client adds rows to a list (the main
// menu's commands with the mods' entries: Gambits), those heights no longer fit; and a mod may not
// want them at all. So as a backdrop is set up (CWMenuManager.SetPrimaryBG) its lines are left as the
// game's, taken away ("none"), or laid again at the list's rows as they are now ("fit").
internal static partial class GlobalScope
{
	/// <summary>A list's rows as the game's layout had them and as they are now, on the screen (the menu's units).</summary>
	internal sealed class BackdropRows
	{
		public int Top;
		public float OldPitch;
		public int OldCount;
		public float NewPitch;
		public int NewCount;
		/// <summary>The list's left and right on the screen: which of the backdrop's columns of lines are its.</summary>
		public int Left, Right;
	}

	private const short LineSourceY = 96;
	private const short LineHeight = 16;

	/// <summary>The backdrop just set up on a BG: its lines as the mode says ("game", "none", "fit" with the rows).</summary>
	internal static void AdjustBackdropLines(int index, string mode, BackdropRows rows)
	{
		if (index < 0 || index >= bgCell.Length || bgCell[index] == null || bgCell[index].oam == null) return;
		if (string.IsNullOrEmpty(mode) || mode == "game") return;
		BG_CELL cell = bgCell[index];
		List<short[]> parts = new List<short[]>();
		for (int j = 0; j < cell.numOAM; j++)
		{
			short[] p = new short[9];
			Array.Copy(cell.oam, j * 9, p, 0, 9);
			parts.Add(p);
		}
		bool IsLine(short[] p) => p[5] == LineSourceY && p[3] == LineHeight;
		List<short[]> lines = parts.Where(IsLine).ToList();
		if (lines.Count == 0) return;
		List<short[]> kept = parts.Where(p => !IsLine(p)).ToList();

		if (mode == "none")
		{
			// Only the lines drawn over a panel go: one that fills a gap between two (the main menu's party panels) is
			// the panels' edge, and would leave a hole - all of it, the pieces of its row that reach over a panel's fill too.
			HashSet<short> edges = new HashSet<short>(lines.Where(l => !Covered(l, kept)).Select(l => l[1]));
			Store(cell, kept.Concat(lines.Where(l => edges.Contains(l[1]))).ToList());
			return;
		}
		if (mode != "fit" || rows == null || rows.OldCount < 2 || rows.NewCount < 1) return;

		// The lines in columns: the heights that share the same run of pieces across. The list's column is the
		// one over the list's left and right; the others (the party's, beside it) stay as they were.
		var byHeight = lines.GroupBy(p => p[1]).Select(g => (Y: g.Key, Xs: string.Join(",", g.Select(p => p[0]).OrderBy(x => x)), Parts: g.ToList())).ToList();
		var columns = byHeight.GroupBy(h => h.Xs).Select(g => g.ToList()).ToList();
		List<(short Y, string Xs, List<short[]> Parts)> mine = null;
		int best = 0;
		foreach (var column in columns)
		{
			int x0 = cell.x + column[0].Parts.Min(p => p[0]), x1 = cell.x + column[0].Parts.Max(p => p[0] + (p[2] >> 1));
			int overlap = Math.Min(x1, rows.Right) - Math.Max(x0, rows.Left);
			if (overlap > best) { best = overlap; mine = column; }
		}
		if (mine == null || mine.Count == 0) return;
		List<short[]> others = byHeight.Where(h => !mine.Any(m => m.Y == h.Y && m.Xs == h.Xs)).SelectMany(h => h.Parts).ToList();

		// Where the lines sit against the rows the game had: their distance from the nearest boundary between two
		// rows (the same for every one of them), and which boundaries have one (the first and last may be the panel's edge).
		List<float> offsets = new List<float>();
		List<int> boundaries = new List<int>();
		foreach (var line in mine)
		{
			float y = cell.y + line.Y * cell.scale;
			int k = (int)Math.Round((y - rows.Top) / rows.OldPitch);
			offsets.Add(y - (rows.Top + k * rows.OldPitch));
			boundaries.Add(k);
		}
		offsets.Sort();
		float offset = offsets[offsets.Count / 2];
		int first = boundaries.Min(), lastFromEnd = rows.OldCount - boundaries.Max();
		List<short[]> template = mine[0].Parts;
		List<short[]> laid = new List<short[]>();
		for (int k = first; k <= rows.NewCount - lastFromEnd; k++)
		{
			short y = (short)Math.Round((rows.Top + k * rows.NewPitch + offset - cell.y) / cell.scale);
			foreach (short[] piece in template)
			{
				short[] copy = (short[])piece.Clone();
				copy[1] = y;
				laid.Add(copy);
			}
		}
		Store(cell, kept.Concat(others).Concat(laid).ToList());
	}

	/// <summary>Whether another piece lies under a piece's middle (its size as drawn: half for the half-size flag).</summary>
	private static bool Covered(short[] piece, List<short[]> others)
	{
		float Size(short[] p, int i) => p[i] * ((p[6] & 4) != 0 ? 0.5f : 1f);
		float cx = piece[0] + Size(piece, 2) / 2, cy = piece[1] + Size(piece, 3) / 2;
		return others.Any(o => cx >= o[0] && cx < o[0] + Size(o, 2) && cy >= o[1] && cy < o[1] + Size(o, 3));
	}

	private static void Store(BG_CELL cell, List<short[]> parts)
	{
		short[] oam = new short[parts.Count * 9];
		for (int j = 0; j < parts.Count; j++) Array.Copy(parts[j], 0, oam, j * 9, 9);
		cell.oam = oam;
		cell.numOAM = parts.Count;
	}
}
