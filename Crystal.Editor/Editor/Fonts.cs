// The game's own font, for drawing text the way the game draws it.
//
// A menu is mostly words: 348 of the 403 widgets across the eight .xbn files are Text.
// So a preview that draws them in the browser's font is showing the layout of something
// else - wrong widths, wrong heights, wrong wrapping. This is what makes it the game's.
//
// The font is two things beside the game rather than in the archive:
//
//   Font{size}.glp        two bytes per Unicode code point, 65536 of them
//   Font{size}_{page}.xnb an XNA SpriteFont holding some of the glyphs
//
// and the .glp says which page each character is on. Read straight off the draw loop:
//
//   byte page = glp[c * 2];
//   if (page == 255) { c = '?'; page = glp['?' * 2]; }        // no glyph for it
//   spriteFont = Font{size}_{page};
//   y -= fontShiftY[size][ glp[c * 2 + 1] ];                  // a per character nudge
//   draw one character, then advance by MeasureString of it
//
// Two sizes ship, 12 and 16. Font12 references 57 pages and there are exactly 57
// Font12_*.xnb files, which is the check that this reading is right rather than merely
// plausible.
//
// Note the game draws one character at a time, each from its own page, and advances by
// that character's own measured width. So a string can be built from several atlases and
// there is no kerning between neighbours - which is why the layout below is per
// character rather than per string.
//
// And the atlas is not in the same units as a menu. Font.drawString works it out:
//
//   float num1 = 800f / LCD_WIDTH;      // 800 / 480
//   float num2 = 480f / LCD_HEIGHT;     // 480 / 320
//   DrawString(str, x * num1, y * num2, size);      // drawn in an 800x480 screen
//   x += StringWidth(str, size) / num1;             // advanced in 480x320 menu units
//
// So the glyphs are cut for an 800 wide screen and a .xbn lays out in 480. Everything
// below comes back in menu units, already divided, because a caller that has to
// remember to do it is a caller that will forget - the first attempt drew "Equipment"
// 118 units wide in a box declared 96.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Crystal.Editor
{
	/// <summary>Where one character comes from and where it goes.</summary>
	internal sealed class GlyphPlacement
	{
		public string Char { get; set; }

		/// <summary>Which Font{size}_{page} atlas holds it.</summary>
		public int Page { get; set; }

		/// <summary>Where it sits in that atlas: x, y, width, height.</summary>
		public int[] Source { get; set; }

		/// <summary>Where to put it relative to the pen, from the glyph's cropping.</summary>
		public int[] Offset { get; set; }

		/// <summary>How far the pen moves afterwards.</summary>
		public double Advance { get; set; }

		/// <summary>The per character vertical nudge, from the second .glp byte.</summary>
		public double ShiftY { get; set; }

		/// <summary>True when the character had no glyph and became a question mark.</summary>
		public bool Missing { get; set; }
	}

	internal sealed class FontLayout
	{
		public int Size { get; set; }

		/// <summary>How wide the string is, in the menu units a .xbn uses.</summary>
		public double Width { get; set; }

		/// <summary>What to multiply an atlas pixel by to get a menu unit.</summary>
		public double[] Scale { get; set; }

		public int LineSpacing { get; set; }
		public List<GlyphPlacement> Glyphs { get; set; } = new List<GlyphPlacement>();
		public List<int> Pages { get; set; } = new List<int>();
		public string Error { get; set; }
	}

	internal sealed class Fonts
	{
		/// <summary>800f / LCD_WIDTH, from Font.drawString.</summary>
		public const double ScaleX = 800.0 / 480.0;

		/// <summary>480f / LCD_HEIGHT.</summary>
		public const double ScaleY = 480.0 / 320.0;

		/// <summary>fontShiftY, straight out of LoadContent. The second .glp byte indexes it.</summary>
		private static readonly Dictionary<int, double[]> ShiftY = new Dictionary<int, double[]>
		{
			{ 12, new[] { 2.0, 2.0, 2.0, 2.0, 3.0, 2.0 } },
			{ 16, new[] { 3.0, 3.0, 3.0, 3.0, 4.5, 3.0 } }
		};

		private readonly string _content;
		private readonly Dictionary<int, byte[]> _tables = new Dictionary<int, byte[]>();
		private readonly Dictionary<string, XnbSpriteFont> _pages =
			new Dictionary<string, XnbSpriteFont>(StringComparer.OrdinalIgnoreCase);

		public Fonts(string contentDirectory)
		{
			_content = contentDirectory;
		}

		public bool Has(int size)
		{
			return File.Exists(Path.Combine(_content, "Font" + size + ".glp"));
		}

		/// <summary>The two sizes that ship, if they are there.</summary>
		public List<int> Sizes()
		{
			return new[] { 12, 16 }.Where(Has).ToList();
		}

		/// <summary>
		/// Where every character of a string comes from and goes, following the game's
		/// own loop one character at a time.
		/// </summary>
		public FontLayout Layout(int size, string text)
		{
			FontLayout layout = new FontLayout { Size = size };
			byte[] table;
			try
			{
				table = Table(size);
			}
			catch (Exception problem)
			{
				layout.Error = problem.Message;
				return layout;
			}

			double[] shift = ShiftY.TryGetValue(size, out double[] found)
				? found
				: new double[6];
			double pen = 0;

			foreach (char raw in text ?? string.Empty)
			{
				// The three the game folds into a space before looking anything up.
				char c = raw == '' || raw == '­' ? ' ' : raw;

				bool missing = false;
				int page = table[c * 2];
				int nudge = table[c * 2 + 1];
				if (page == 255)
				{
					missing = true;
					c = '?';
					page = table['?' * 2];
					nudge = table['?' * 2 + 1];
				}

				XnbSpriteFont font;
				try
				{
					font = Page(size, page);
				}
				catch (Exception problem)
				{
					layout.Error = problem.Message;
					return layout;
				}

				layout.LineSpacing = font.LineSpacing;
				if (!layout.Pages.Contains(page)) layout.Pages.Add(page);

				int at = font.Characters.IndexOf(c);
				if (at < 0 || at >= font.Glyphs.Count)
				{
					// On its page but not in it. Nothing to draw, and nothing to move by.
					continue;
				}

				float[] kerning = at < font.Kerning.Count
					? font.Kerning[at]
					: new float[3];
				// MeasureString of a single character, which is what the game advances by.
				double advance = kerning[0] + kerning[1] + kerning[2];

				layout.Glyphs.Add(new GlyphPlacement
				{
					Char = c.ToString(),
					Page = page,
					Source = font.Glyphs[at],
					// Everything but the source rectangle is in menu units. The source
					// is where to cut from the atlas, so it stays in atlas pixels.
					Offset = at < font.Cropping.Count
						? new[] { font.Cropping[at][0], font.Cropping[at][1] }
						: new[] { 0, 0 },
					Advance = advance / ScaleX,
					ShiftY = (nudge < shift.Length ? shift[nudge] : 0) / ScaleY,
					Missing = missing
				});

				pen += advance / ScaleX;
			}

			layout.Width = pen;
			layout.Scale = new[] { 1.0 / ScaleX, 1.0 / ScaleY };
			return layout;
		}

		/// <summary>One atlas page as a PNG, for the browser to blit from.</summary>
		public byte[] PagePng(int size, int page)
		{
			XnbSpriteFont font = Page(size, page);
			return Png.Encode(font.Texture.Width, font.Texture.Height,
				SurfaceFormats.ToRgba(font.Texture.SurfaceFormat, font.Texture.Width,
					font.Texture.Height, font.Texture.Mips[0]));
		}

		private byte[] Table(int size)
		{
			if (_tables.TryGetValue(size, out byte[] found)) return found;

			string path = Path.Combine(_content, "Font" + size + ".glp");
			if (!File.Exists(path))
			{
				throw new FileNotFoundException("no Font" + size + ".glp beside the game");
			}

			byte[] table = File.ReadAllBytes(path);
			if (table.Length < 65536 * 2)
			{
				throw new InvalidDataException("Font" + size + ".glp is "
					+ table.Length + " bytes, and a glyph table is two per code point");
			}
			_tables[size] = table;
			return table;
		}

		private XnbSpriteFont Page(int size, int page)
		{
			string key = size + "_" + page;
			if (_pages.TryGetValue(key, out XnbSpriteFont found)) return found;

			string path = Path.Combine(_content, string.Format(CultureInfo.InvariantCulture,
				"Font{0}_{1}.xnb", size, page));
			if (!File.Exists(path))
			{
				throw new FileNotFoundException("no Font" + size + "_" + page + ".xnb");
			}

			Xnb xnb = Xnb.Open(path);
			try
			{
				XnbSpriteFont font = xnb.ReadSpriteFont();
				_pages[key] = font;
				return font;
			}
			finally
			{
				xnb.Close();
			}
		}
	}
}
