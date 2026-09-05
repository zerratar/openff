// FF4's own 2D pieces for the OpenFF battle and menus on FF4: the window frames, the glove
// cursor and the ATB gauge, drawn from the game's sheets and cell banks.
//
// The phone and Steam builds keep the DS names but not the DS formats: a .NCGR/.NCBR is a
// PNG sheet, a .NCER a bank of cells whose parts are seven plain words (FF3.Content.CellBanks).
// MENU_Common.dat has window_frame_00..05 (two frame styles per sheet, eight cells each in the
// bank: corner, left edge, corner, top edge, corner, right edge, corner, bottom edge - 16-px
// corners, 64-px edges; style 0 a white line, style 1 the blue bevel the battle uses) and
// cursor (two 48 x 48 gloves, pointing and pressed, hanging 48 left and 12 up of the spot);
// battle2d_Common.dat has gauge_atb (cell 0 the 80 x 12 trough, cells 1..3 72 x 6 fills:
// grey, yellow, red). FF4's windows are menu::BasicWindow with these frames over a fill the
// code paints (the port tints a white texel 0x4A2214, blue-green-red) - the same class draws
// FF4's dialogue in this client already.
//
// Sizes: the phone UI lays these out in a 1136 x 640 space (the Steam window is that, letter
// for letter: the glove is 85 px of 1122); the port draws at 800 x 480, so every piece is
// drawn at 800/1136 of its sheet size. The battle windows' positions come from the Steam
// screenshots for now (Docs/Client-Plan.md) - FF4 builds them in code (btl::TouchWindow,
// ui::CWidgetMng::addWidget), still to be read from the binary.

using System;
using System.Collections.Generic;
using FF3.Content;
using OpenFF;

namespace FF3
{
	internal static class Ff4Ui
	{
		/// <summary>FF4's phone UI space (1136 wide) onto the port's 800.</summary>
		public const float Scale = 800f / 1136f;

		public const string FrameSheet = "window_frame_00.NCGR", FrameBank = "window_frame_00.NCER";
		public const string CursorSheet = "cursor.NCGR", CursorBank = "cursor.NCER";
		public const string GaugeSheet = "gauge_atb.NCGR", GaugeBank = "gauge_atb.NCER";
		public const string FillSheet = "winsample.NCGR";

		/// <summary>The fill under FF4's frames: BasicWindow's tint, 0x4A2214 stored blue-green-red.</summary>
		public static readonly Color Fill = new Color(0x14, 0x22, 0x4A, 255);

		private static readonly Dictionary<string, Texture> _sheets = new Dictionary<string, Texture>(StringComparer.OrdinalIgnoreCase);
		private static readonly Dictionary<string, CellBank> _banks = new Dictionary<string, CellBank>(StringComparer.OrdinalIgnoreCase);
		private static readonly HashSet<string> _missing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>A file by name through the game's own file system (the archives' members included); null when absent.</summary>
		public static byte[] ReadFile(string name)
		{
			try
			{
				uint size = GlobalScope.ds.g_File.getSize(name);
				if (size == 0) return null;
				Array array = GlobalScope.ds.CHeap.alloc_app(size);
				if (array == null || !GlobalScope.ds.g_File.load(array, name)) return null;
				if (array is byte[] bytes) return bytes;
				byte[] copy = new byte[array.Length];
				Buffer.BlockCopy(array, 0, copy, 0, Math.Min(copy.Length, Buffer.ByteLength(array)));
				return copy;
			}
			catch (Exception) { return null; }
		}

		public static Texture Sheet(string name)
		{
			if (_sheets.TryGetValue(name, out Texture t)) return t;
			if (_missing.Contains(name)) return null;
			byte[] data = ReadFile(name);
			Texture texture = data != null ? Game.Draw.LoadTexture("ff4:" + name, data) : null;
			if (texture == null)
			{
				_missing.Add(name);
				Log.Write(LogChannel.General, "ff4 ui: no sheet " + name);
				return null;
			}
			_sheets[name] = texture;
			return texture;
		}

		public static CellBank Bank(string name)
		{
			if (_banks.TryGetValue(name, out CellBank b)) return b;
			if (_missing.Contains(name)) return null;
			byte[] data = ReadFile(name);
			CellBank bank = data != null ? CellBanks.Read(data, name) : null;
			if (bank == null)
			{
				_missing.Add(name);
				Log.Write(LogChannel.General, "ff4 ui: no cell bank " + name);
				return null;
			}
			_banks[name] = bank;
			return bank;
		}

		/// <summary>One part of a cell drawn with its origin at (x, y), the sheet's pixels times <paramref name="scale"/>; a width or height given stretches the part to it.</summary>
		private static void Part(DrawList d, Texture sheet, CellPart p, float x, float y, float scale, Color? tint = null, float? width = null, float? height = null, float? sourceWidth = null)
		{
			float w = width ?? p.Width * scale, h = height ?? p.Height * scale;
			float sw = sourceWidth ?? p.Width;
			d.Sprite(sheet, x + p.X * scale, y + p.Y * scale, w, h, tint, 0f, p.SourceX, p.SourceY, sw, p.Height);
		}

		/// <summary>A whole cell at (x, y).</summary>
		public static bool Cell(DrawList d, string bankName, string sheetName, int index, float x, float y, float scale = Scale, Color? tint = null)
		{
			CellBank bank = Bank(bankName);
			Texture sheet = Sheet(sheetName);
			Cell cell = bank?[index];
			if (cell == null || sheet == null) return false;
			foreach (CellPart p in cell.Parts) Part(d, sheet, p, x, y, scale, tint);
			return true;
		}

		/// <summary>FF4's window: the fill, then the frame's eight cells of <paramref name="style"/> (0 white line, 1 blue bevel) around the rectangle. False (nothing drawn) when the assets are missing, so the caller can fall back.</summary>
		public static bool Window(DrawList d, float x, float y, float w, float h, int style = 1, float alpha = 0.82f)
		{
			CellBank bank = Bank(FrameBank);
			Texture sheet = Sheet(FrameSheet);
			if (bank == null || sheet == null || bank.Cells.Count < 8 * (style + 1)) return false;
			Color fill = new Color(Fill.R, Fill.G, Fill.B, (byte)Math.Clamp((int)(alpha * 255), 0, 255));
			Texture fillSheet = Sheet(FillSheet);
			if (fillSheet != null) d.Sprite(fillSheet, x, y, w, h, new Color(255, 255, 255, fill.A));
			else d.Rect(x, y, w, h, fill);
			int b = style * 8;
			float k = Scale;
			CellPart tl = bank[b]?.Parts.Count > 0 ? bank[b].Parts[0] : null;
			CellPart left = bank[b + 1]?.Parts.Count > 0 ? bank[b + 1].Parts[0] : null;
			CellPart bl = bank[b + 2]?.Parts.Count > 0 ? bank[b + 2].Parts[0] : null;
			CellPart top = bank[b + 3]?.Parts.Count > 0 ? bank[b + 3].Parts[0] : null;
			CellPart tr = bank[b + 4]?.Parts.Count > 0 ? bank[b + 4].Parts[0] : null;
			CellPart right = bank[b + 5]?.Parts.Count > 0 ? bank[b + 5].Parts[0] : null;
			CellPart br = bank[b + 6]?.Parts.Count > 0 ? bank[b + 6].Parts[0] : null;
			CellPart bottom = bank[b + 7]?.Parts.Count > 0 ? bank[b + 7].Parts[0] : null;
			if (tl == null || left == null || bl == null || top == null || tr == null || right == null || br == null || bottom == null) return false;
			// The corners hang half over the rectangle's corners (their parts sit at -8, -8); the
			// edges run between them, stretched to the side.
			float corner = tl.Width * k;
			Part(d, sheet, tl, x, y, k);
			Part(d, sheet, tr, x + w, y, k);
			Part(d, sheet, bl, x, y + h, k);
			Part(d, sheet, br, x + w, y + h, k);
			float edgeH = h - corner, edgeW = w - corner;
			if (edgeH > 0)
			{
				Part(d, sheet, left, x, y + corner / 2, k, null, null, edgeH);
				Part(d, sheet, right, x + w, y + corner / 2, k, null, null, edgeH);
			}
			if (edgeW > 0)
			{
				Part(d, sheet, top, x + corner / 2, y, k, null, edgeW, null);
				Part(d, sheet, bottom, x + corner / 2, y + h, k, null, edgeW, null);
			}
			return true;
		}

		/// <summary>The glove, its fingertip at (x, y); the pressed one while a choice is being confirmed.</summary>
		public static bool Glove(DrawList d, float x, float y, bool pressed = false)
		{
			return Cell(d, CursorBank, CursorSheet, pressed ? 1 : 0, x, y);
		}

		/// <summary>The ATB gauge with its origin at (x, y): the trough (cell 0) and a fill (1 grey, 2 yellow, 3 red) cut to <paramref name="fraction"/>.</summary>
		public static bool Gauge(DrawList d, float x, float y, float fraction, int fill = 2)
		{
			CellBank bank = Bank(GaugeBank);
			Texture sheet = Sheet(GaugeSheet);
			if (bank == null || sheet == null || bank.Cells.Count < 4) return false;
			foreach (CellPart p in bank[0].Parts) Part(d, sheet, p, x, y, Scale);
			fraction = Math.Clamp(fraction, 0f, 1f);
			if (fraction <= 0f) return true;
			Cell f = bank[Math.Clamp(fill, 1, 3)];
			foreach (CellPart p in f.Parts) Part(d, sheet, p, x, y, Scale, null, p.Width * Scale * fraction, null, p.Width * fraction);
			return true;
		}

		/// <summary>The gauge's drawn width at the port's scale.</summary>
		public static float GaugeWidth => 80f * Scale;
	}
}
