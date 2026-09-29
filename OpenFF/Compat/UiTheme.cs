// The client's menus in their finished look (the Esc menu first): a navy panel with a faint
// texture and crest, a gold filigree frame, a gold divider under the title, rows as dark plates
// with a gold-lit selected one, steppers and switches for values, a note box, and the icons.
// The pictures are Data/ui's: frame.png (a nine-slice, 150 px corners, its lines 50 px in),
// divider.png (its ornament the middle eighth), crest.png, panel.png, icons.png (5 x 4 cells of
// 128 px). Everything else is drawn here, crisp at any size. Laid out in Ui's 800x480 text space;
// without the pictures (a build that lost Data/ui) the shapes stand in plainly.

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenFF.Client
{
	using Color = Microsoft.Xna.Framework.Color;

	internal static class UiTheme
	{
		public static readonly Color Gold = new Color(232, 190, 92, 255);
		public static readonly Color GoldBright = new Color(255, 224, 138, 255);
		public static readonly Color Ink = new Color(238, 242, 252, 255);
		public static readonly Color Sub = new Color(150, 178, 232, 255);
		public static readonly Color Hint = new Color(140, 170, 226, 255);
		private static readonly Color RowFill = new Color(9, 19, 46, 205);
		private static readonly Color RowEdge = new Color(74, 94, 138, 190);
		private static readonly Color InsetFill = new Color(4, 10, 28, 235);
		private static readonly Color InsetEdge = new Color(92, 112, 158, 210);

		/// <summary>The icons' cells in icons.png, in its order.</summary>
		public enum Icon
		{
			Resume, Settings, Quality, Exit, Abilities,
			Speed, Encounters, Exp, JobExp, Save,
			Adjustment, Indicator, Back, Window, WindowSize,
			AntiAliasing, VSync, FrameRate, Run, Pad,
		}

		private static GraphicsDevice _device;
		private static Texture2D _frame, _panel, _divider, _crest, _icons, _grad, _glow, _diamond;

		public static void Ensure(GraphicsDevice device)
		{
			Ui.Ensure(device);
			if (_device == device && _grad != null && !_grad.IsDisposed) return;
			_device = device;
			_frame = Load("frame");
			_panel = Load("panel");
			_divider = Load("divider");
			_crest = Load("crest");
			_icons = Load("icons");
			_grad = MakeGradient(device);
			_glow = MakeGlow(device, 64, 20);
			_diamond = MakeDiamond(device, 32);
		}

		private static Texture2D Load(string name)
		{
			string path = Path.Combine(AppContext.BaseDirectory, "Data", "ui", name + ".png");
			try
			{
				if (!File.Exists(path)) { Log.First(LogChannel.General, "ui-missing-" + name, 1, () => "ui: " + path + " is missing; the menus draw without it"); return null; }
				Texture2D loaded;
				using (FileStream stream = File.OpenRead(path)) loaded = Texture2D.FromStream(_device, stream);
				Texture2D mipped = WithMipmaps(loaded);
				if (!ReferenceEquals(mipped, loaded)) loaded.Dispose();
				return mipped;
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "ui: " + name + ".png: " + ex.Message); return null; }
		}

		/// <summary>
		/// The picture drawn smaller cleanly: its mipmaps, each a box-filtered half of the one above (colours weighted by
		/// alpha), and its transparent pixels given their neighbours' colour first - drawn a third of its size or less, an
		/// ornament's fine gold lines and an icon's edges keep their shape, with no dark fringe from the black round them.
		/// </summary>
		private static Texture2D WithMipmaps(Texture2D source)
		{
			int w = source.Width, h = source.Height;
			Color[] px = new Color[w * h];
			source.GetData(px);
			Bleed(px, w, h);
			int levels = 1;
			for (int s = Math.Max(w, h); s > 1; s >>= 1) levels++;
			Texture2D tex = new Texture2D(_device, w, h, true, SurfaceFormat.Color);
			levels = Math.Min(levels, tex.LevelCount);
			tex.SetData(0, null, px, 0, px.Length);
			for (int level = 1; level < levels; level++)
			{
				int nw = Math.Max(1, w >> 1), nh = Math.Max(1, h >> 1);
				Color[] next = new Color[nw * nh];
				for (int y = 0; y < nh; y++)
					for (int x = 0; x < nw; x++)
					{
						float r = 0, g = 0, b = 0, a = 0, n = 0;
						for (int dy = 0; dy < 2; dy++)
							for (int dx = 0; dx < 2; dx++)
							{
								int sx = Math.Min(w - 1, x * 2 + dx), sy = Math.Min(h - 1, y * 2 + dy);
								Color c = px[sy * w + sx];
								float ca = c.A / 255f + 1e-4f;
								r += c.R * ca; g += c.G * ca; b += c.B * ca; a += c.A; n += ca;
							}
						next[y * nw + x] = new Color((int)(r / n), (int)(g / n), (int)(b / n), (int)(a / 4));
					}
				tex.SetData(level, null, next, 0, next.Length);
				px = next; w = nw; h = nh;
			}
			return tex;
		}

		/// <summary>Transparent pixels take the colour of the opaque ones beside them, a few pixels out, so filtering blends toward the edge's own colour rather than black.</summary>
		private static void Bleed(Color[] px, int w, int h)
		{
			for (int pass = 0; pass < 6; pass++)
			{
				Color[] from = (Color[])px.Clone();
				bool any = false;
				for (int y = 0; y < h; y++)
					for (int x = 0; x < w; x++)
					{
						int i = y * w + x;
						if (from[i].A > 8) continue;
						int r = 0, g = 0, b = 0, n = 0;
						for (int dy = -1; dy <= 1; dy++)
							for (int dx = -1; dx <= 1; dx++)
							{
								int sx = x + dx, sy = y + dy;
								if (sx < 0 || sy < 0 || sx >= w || sy >= h) continue;
								Color c = from[sy * w + sx];
								if (c.A <= 8 && !(c.R > 0 || c.G > 0 || c.B > 0)) continue;
								r += c.R; g += c.G; b += c.B; n++;
							}
						if (n == 0) continue;
						px[i] = new Color(r / n, g / n, b / n, from[i].A);
						any = true;
					}
				if (!any) break;
			}
		}

		// The lit row's fill: warm gold at the left falling off to a deep amber at the right.
		private static Texture2D MakeGradient(GraphicsDevice device)
		{
			const int w = 256;
			Color[] px = new Color[w];
			for (int x = 0; x < w; x++)
			{
				float t = x / (float)(w - 1);
				float k = (float)Math.Pow(1 - t, 1.4);
				px[x] = new Color((int)(104 + 126 * k), (int)(72 + 104 * k), (int)(22 + 48 * k), (int)(214 + 36 * k));
			}
			Texture2D tex = new Texture2D(device, w, 1);
			tex.SetData(px);
			return tex;
		}

		// A soft glow round a rounded box, to nine-slice: full inside, falling off over the margin.
		private static Texture2D MakeGlow(GraphicsDevice device, int size, int margin)
		{
			Color[] px = new Color[size * size];
			for (int y = 0; y < size; y++)
				for (int x = 0; x < size; x++)
				{
					float dx = Math.Max(Math.Max(margin - x, x - (size - 1 - margin)), 0);
					float dy = Math.Max(Math.Max(margin - y, y - (size - 1 - margin)), 0);
					float d = (float)Math.Sqrt(dx * dx + dy * dy) / margin;
					float a = d >= 1 ? 0 : (float)Math.Pow(1 - d, 2.2);
					px[y * size + x] = Color.White * a;
				}
			Texture2D tex = new Texture2D(device, size, size);
			tex.SetData(px);
			return tex;
		}

		private static Texture2D MakeDiamond(GraphicsDevice device, int size)
		{
			Color[] px = new Color[size * size];
			float c = (size - 1) / 2f;
			for (int y = 0; y < size; y++)
				for (int x = 0; x < size; x++)
				{
					float d = Math.Abs(x - c) + Math.Abs(y - c);
					px[y * size + x] = Color.White * Math.Clamp(c - d + 0.5f, 0f, 1f);
				}
			Texture2D tex = new Texture2D(device, size, size);
			tex.SetData(px);
			return tex;
		}

		private static float Sx(Viewport v) => v.Width / Ui.W;
		private static float Sy(Viewport v) => v.Height / Ui.H;

		private static Rectangle Px(float x, float y, float w, float h, Viewport v)
		{
			float sx = Sx(v), sy = Sy(v);
			return new Rectangle((int)Math.Round(x * sx), (int)Math.Round(y * sy), (int)Math.Round(w * sx), (int)Math.Round(h * sy));
		}

		// ---- shapes (inside a SpriteBatch.Begin/End, NonPremultiplied) ----

		/// <summary>The panel: the game dimmed, the navy fill with its texture and crest, the gold frame round it.</summary>
		public static void Panel(SpriteBatch b, Rectangle r, Viewport v, bool dim = true)
		{
			if (dim) Ui.Fill(b, new Rectangle(0, 0, v.Width, v.Height), new Color(0, 0, 0, 150));
			Rectangle p = Px(r.X, r.Y, r.Width, r.Height, v);
			Ui.Fill(b, p, new Color(12, 26, 64, 250));
			if (_panel != null) b.Draw(_panel, p, new Color(255, 255, 255, 235));
			// The crest, faint, in the top right corner.
			if (_crest != null)
			{
				float size = r.Height * 0.3f;
				b.Draw(_crest, Px(r.Right - size - 10, r.Y + 8, size, size, v), new Color(255, 255, 255, 34));
			}
			Frame(b, r, v);
		}

		/// <summary>The gold frame: its corners' filigree, its two lines along the panel's edges.</summary>
		public static void Frame(SpriteBatch b, Rectangle r, Viewport v)
		{
			if (_frame == null) { Ui.Frame(b, Px(r.X, r.Y, r.Width, r.Height, v), Gold, Math.Max(1, (int)Math.Round(Sx(v)))); return; }
			float k = 36f;                         // the corner's size in text space
			float inset = k / 3f;                  // the lines lie a third of the corner in
			float s = Math.Min(Sx(v), Sy(v));
			int kp = (int)Math.Round(k * s);
			Rectangle o = Px(r.X - inset, r.Y - inset, r.Width + 2 * inset, r.Height + 2 * inset, v);
			int t = _frame.Width, c = t / 3;       // 150 of 450
			void Part(Rectangle src, Rectangle dst) { if (dst.Width > 0 && dst.Height > 0) b.Draw(_frame, dst, src, Color.White); }
			Part(new Rectangle(0, 0, c, c), new Rectangle(o.X, o.Y, kp, kp));
			Part(new Rectangle(t - c, 0, c, c), new Rectangle(o.Right - kp, o.Y, kp, kp));
			Part(new Rectangle(0, t - c, c, c), new Rectangle(o.X, o.Bottom - kp, kp, kp));
			Part(new Rectangle(t - c, t - c, c, c), new Rectangle(o.Right - kp, o.Bottom - kp, kp, kp));
			Part(new Rectangle(c, 0, c, c), new Rectangle(o.X + kp, o.Y, o.Width - 2 * kp, kp));
			Part(new Rectangle(c, t - c, c, c), new Rectangle(o.X + kp, o.Bottom - kp, o.Width - 2 * kp, kp));
			Part(new Rectangle(0, c, c, c), new Rectangle(o.X, o.Y + kp, kp, o.Height - 2 * kp));
			Part(new Rectangle(t - c, c, c, c), new Rectangle(o.Right - kp, o.Y + kp, kp, o.Height - 2 * kp));
		}

		/// <summary>The divider under a title: its ornament at (cx, y) at its own shape, its lines stretched out to the width.</summary>
		public static void Divider(SpriteBatch b, float cx, float y, float width, Viewport v, float height = 11f)
		{
			if (_divider == null) { Ui.Fill(b, Px(cx - width / 2, y, width, 1, v), Gold); return; }
			// The picture's content lies in rows 56..120; its ornament in the columns 451..573 of 1024.
			int srcY = (int)(_divider.Height * 56 / 183f), srcH = (int)(_divider.Height * 64 / 183f);
			int a = (int)(_divider.Width * 0.44f), z = (int)(_divider.Width * 0.56f);
			float mid = height * (z - a) / srcH;
			float top = y - height / 2;
			b.Draw(_divider, Px(cx - width / 2, top, width / 2 - mid / 2, height, v), new Rectangle(0, srcY, a, srcH), Color.White);
			b.Draw(_divider, Px(cx - mid / 2, top, mid, height, v), new Rectangle(a, srcY, z - a, srcH), Color.White);
			b.Draw(_divider, Px(cx + mid / 2, top, width / 2 - mid / 2, height, v), new Rectangle(z, srcY, _divider.Width - z, srcH), Color.White);
		}

		/// <summary>A row's plate: dark with a fine edge, or lit - gold, glowing, with a diamond at each end when asked.</summary>
		public static void Row(SpriteBatch b, Rectangle r, bool lit, Viewport v, bool diamonds = false)
		{
			Rectangle p = Px(r.X, r.Y, r.Width, r.Height, v);
			if (!lit)
			{
				Ui.RoundPlate(b, p, RowEdge, v);
				Ui.RoundPlate(b, Inset(p, 1), RowFill, v);
				return;
			}
			Glow(b, r, new Color(255, 196, 84, 150), 9f, v);
			Ui.RoundPlate(b, p, GoldBright, v);
			Rectangle inner = Inset(p, Math.Max(2, (int)Math.Round(2 * Sx(v))));
			Ui.RoundPlate(b, inner, new Color(70, 52, 20, 255), v);
			b.Draw(_grad, Inset(inner, Math.Max(1, (int)Math.Round(2 * Sx(v)))), Color.White);
			if (diamonds)
			{
				float d = r.Height * 0.36f;
				b.Draw(_diamond, Px(r.X - d / 2, r.Y + r.Height / 2f - d / 2, d, d, v), GoldBright);
				b.Draw(_diamond, Px(r.Right - d / 2, r.Y + r.Height / 2f - d / 2, d, d, v), GoldBright);
			}
		}

		/// <summary>A soft glow of a colour round a text-space box, spread by the given text-space distance.</summary>
		public static void Glow(SpriteBatch b, Rectangle r, Color colour, float spread, Viewport v)
		{
			Rectangle o = Px(r.X - spread, r.Y - spread, r.Width + 2 * spread, r.Height + 2 * spread, v);
			int m = (int)Math.Round(spread * 2 * Sx(v)), t = _glow.Width, c = t / 2 - 2;
			m = Math.Min(m, Math.Min(o.Width, o.Height) / 2);
			void Part(Rectangle src, Rectangle dst) { if (dst.Width > 0 && dst.Height > 0) b.Draw(_glow, dst, src, colour); }
			Part(new Rectangle(0, 0, c, c), new Rectangle(o.X, o.Y, m, m));
			Part(new Rectangle(t - c, 0, c, c), new Rectangle(o.Right - m, o.Y, m, m));
			Part(new Rectangle(0, t - c, c, c), new Rectangle(o.X, o.Bottom - m, m, m));
			Part(new Rectangle(t - c, t - c, c, c), new Rectangle(o.Right - m, o.Bottom - m, m, m));
			Part(new Rectangle(c, 0, t - 2 * c, c), new Rectangle(o.X + m, o.Y, o.Width - 2 * m, m));
			Part(new Rectangle(c, t - c, t - 2 * c, c), new Rectangle(o.X + m, o.Bottom - m, o.Width - 2 * m, m));
			Part(new Rectangle(0, c, c, t - 2 * c), new Rectangle(o.X, o.Y + m, m, o.Height - 2 * m));
			Part(new Rectangle(t - c, c, c, t - 2 * c), new Rectangle(o.Right - m, o.Y + m, m, o.Height - 2 * m));
			Part(new Rectangle(c, c, t - 2 * c, t - 2 * c), new Rectangle(o.X + m, o.Y + m, o.Width - 2 * m, o.Height - 2 * m));
		}

		/// <summary>A value's stepper: a dark inset box, an arrow cell at each end.</summary>
		public static void Stepper(SpriteBatch b, Rectangle r, bool lit, Viewport v)
		{
			Rectangle p = Px(r.X, r.Y, r.Width, r.Height, v);
			Ui.RoundPlate(b, p, lit ? new Color(150, 118, 52, 230) : InsetEdge, v);
			Ui.RoundPlate(b, Inset(p, 1), InsetFill, v);
			float cell = r.Height * 1.25f;
			Color line = lit ? new Color(150, 118, 52, 200) : InsetEdge;
			Ui.Fill(b, Px(r.X + cell, r.Y + 1, 1, r.Height - 2, v), line);
			Ui.Fill(b, Px(r.Right - cell, r.Y + 1, 1, r.Height - 2, v), line);
			Color arrow = lit ? GoldBright : Ink;
			Ui.IconAt(b, Ui.Icon.Left, r.X + cell / 2, r.Y + r.Height / 2f, r.Height * 0.62f, arrow, v);
			Ui.IconAt(b, Ui.Icon.Right, r.Right - cell / 2, r.Y + r.Height / 2f, r.Height * 0.62f, arrow, v);
		}

		/// <summary>The stepper's middle, where its value is written.</summary>
		public static Rectangle StepperValue(Rectangle r)
		{
			int cell = (int)Math.Round(r.Height * 1.25f);
			return new Rectangle(r.X + cell, r.Y, r.Width - 2 * cell, r.Height);
		}

		/// <summary>An on / off switch: a pill with its knob at the left, blue when on, grey when off.</summary>
		public static void Switch(SpriteBatch b, Rectangle r, bool on, bool lit, Viewport v)
		{
			Rectangle p = Px(r.X, r.Y, r.Width, r.Height, v);
			Ui.RoundPlate(b, p, on ? new Color(90, 140, 220, 230) : new Color(92, 100, 122, 220), v);
			Ui.RoundPlate(b, Inset(p, 1), on ? new Color(10, 26, 62, 245) : new Color(10, 16, 32, 245), v);
			float d = r.Height * 0.68f, cx = r.X + r.Height * 0.62f, cy = r.Y + r.Height / 2f;
			if (on) Ui.GlyphDisc(b, cx, cy, d * 1.5f, new Color(80, 170, 255, 70), v);
			Ui.GlyphDisc(b, cx, cy, d, on ? new Color(84, 176, 255, 255) : new Color(150, 156, 170, 255), v);
		}

		/// <summary>Where the switch's word goes (right of its knob).</summary>
		public static Rectangle SwitchWord(Rectangle r) => new Rectangle(r.X + (int)(r.Height * 1.2f), r.Y, r.Width - (int)(r.Height * 1.2f) - 4, r.Height);

		/// <summary>The note box under the rows: a dark plate with a fine gold edge.</summary>
		public static void NoteBox(SpriteBatch b, Rectangle r, Viewport v)
		{
			Rectangle p = Px(r.X, r.Y, r.Width, r.Height, v);
			Ui.RoundPlate(b, p, new Color(196, 156, 74, 230), v);
			Ui.RoundPlate(b, Inset(p, 1), new Color(6, 14, 38, 235), v);
		}

		/// <summary>A small box: an edge and a fill, rounded - a checkbox, an arrow button, a pill.</summary>
		public static void Box(SpriteBatch b, Rectangle r, Color edge, Color fill, Viewport v, float radius = 3f)
		{
			Rectangle p = Px(r.X, r.Y, r.Width, r.Height, v);
			Ui.RoundPlate(b, p, edge, v, radius);
			Ui.RoundPlate(b, Inset(p, Math.Max(1, (int)Math.Round(Sx(v) * 0.8f))), fill, v, radius);
		}

		/// <summary>A progress bar: a dark inset with a gold edge, filled with the lit row's gold to the fraction.</summary>
		public static void Progress(SpriteBatch b, Rectangle r, float fraction, Viewport v)
		{
			Rectangle p = Px(r.X, r.Y, r.Width, r.Height, v);
			Ui.RoundPlate(b, p, new Color(196, 156, 74, 240), v, 4f);
			Rectangle inner = Inset(p, Math.Max(2, (int)Math.Round(2 * Sx(v))));
			Ui.RoundPlate(b, inner, InsetFill, v, 3f);
			int w = (int)Math.Round(inner.Width * Math.Clamp(fraction, 0f, 1f));
			if (w <= 0) return;
			Rectangle fill = new Rectangle(inner.X, inner.Y, w, inner.Height);
			Glow(b, new Rectangle(r.X, r.Y, (int)Math.Round(r.Width * Math.Clamp(fraction, 0f, 1f)), r.Height), new Color(255, 196, 84, 70), 4f, v);
			b.Draw(_grad, fill, null, Color.White, 0f, Microsoft.Xna.Framework.Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);   // bright at the leading edge
			Ui.Fill(b, new Rectangle(fill.X, fill.Y, fill.Width, Math.Max(1, fill.Height / 3)), new Color(255, 240, 200, 50));
		}

		/// <summary>A gold diamond centred on a text-space point.</summary>
		public static void Diamond(SpriteBatch b, float cx, float cy, float size, Color colour, Viewport v) => b.Draw(_diamond, Px(cx - size / 2, cy - size / 2, size, size, v), colour);

		/// <summary>One of icons.png's, centred on a text-space point at a text-space size.</summary>
		public static void IconAt(SpriteBatch b, Icon icon, float cx, float cy, float size, Viewport v, float alpha = 1f)
		{
			if (_icons == null) return;
			int i = (int)icon, cell = _icons.Width / 5;
			float s = Math.Min(Sx(v), Sy(v));
			int px = (int)Math.Round(size * s);
			Rectangle dst = new Rectangle((int)Math.Round(cx * Sx(v) - px / 2f), (int)Math.Round(cy * Sy(v) - px / 2f), px, px);
			b.Draw(_icons, dst, new Rectangle(i % 5 * cell, i / 5 * cell, cell, cell), Color.White * alpha);
		}

		private static Rectangle Inset(Rectangle r, int by) => new Rectangle(r.X + by, r.Y + by, Math.Max(0, r.Width - 2 * by), Math.Max(0, r.Height - 2 * by));
	}
}
