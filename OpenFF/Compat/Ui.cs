// The client's own look, for what is not the game's: the menu (PauseMenu), the name entry's
// keys (TextEntry), the mod list. Modern and crisp - the game's window blue so it does not
// feel foreign, TrueType at the window's resolution, rounded key plates, button glyphs in
// the pad's own language (Cross / Circle / Square / Triangle on a PlayStation pad, A / B / X
// / Y on the rest). Everything is laid out in the game's 800x480 text space and scaled to
// the viewport, in two passes: shapes through a SpriteBatch, then text through the game's
// Graphics (which is TrueType when a face is loaded).

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace OpenFF.Client
{
	using Color = Microsoft.Xna.Framework.Color;

	internal static class Ui
	{
		public const float W = 800f, H = 480f;

		// The palette: the game's window blue and its light frame, an amber for what is selected.
		public static readonly Color Dim = new Color(0, 0, 0, 150);
		public static readonly Color Panel = new Color(16, 30, 72, 238);
		public static readonly Color PanelEdge = new Color(205, 214, 236, 255);
		public static readonly Color Plate = new Color(38, 58, 122, 255);
		public static readonly Color PlateEdge = new Color(70, 96, 170, 255);
		public static readonly Color Lit = new Color(238, 206, 96, 255);
		public static readonly Color LitEdge = new Color(255, 240, 190, 255);
		public static readonly Color Text = Color.White;
		public static readonly Color TextOnLit = new Color(28, 24, 10, 255);
		public static readonly Color Muted = new Color(168, 180, 208, 255);
		public static readonly Color Accent = new Color(255, 220, 120, 255);

		private static Texture2D _pixel, _disc, _round;
		private static GraphicsDevice _device;

		/// <summary>A text-space rectangle in viewport pixels.</summary>
		public static Rectangle Scale(Rectangle r, Viewport v)
		{
			float sx = v.Width / W, sy = v.Height / H;
			return new Rectangle((int)Math.Round(r.X * sx), (int)Math.Round(r.Y * sy), (int)Math.Round(r.Width * sx), (int)Math.Round(r.Height * sy));
		}

		public static void Ensure(GraphicsDevice device)
		{
			if (_device != device || _pixel == null || _pixel.IsDisposed)
			{
				_device = device;
				_pixel = new Texture2D(device, 1, 1);
				_pixel.SetData(new[] { Color.White });
				_disc = MakeDisc(device, 64);
				_round = MakeRound(device, 32, 8);
			}
		}

		/// <summary>A filled, anti-aliased disc (a pad button's face).</summary>
		private static Texture2D MakeDisc(GraphicsDevice device, int size)
		{
			Color[] px = new Color[size * size];
			float c = (size - 1) / 2f, r = size / 2f - 1.5f;
			for (int y = 0; y < size; y++)
				for (int x = 0; x < size; x++)
				{
					float d = (float)Math.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
					float a = Math.Clamp(r + 0.5f - d, 0f, 1f);
					px[y * size + x] = Color.White * a;
				}
			Texture2D t = new Texture2D(device, size, size);
			t.SetData(px);
			return t;
		}

		/// <summary>A rounded square of which the corners are drawn: nine-sliced into a plate with soft corners.</summary>
		private static Texture2D MakeRound(GraphicsDevice device, int size, int radius)
		{
			Color[] px = new Color[size * size];
			for (int y = 0; y < size; y++)
				for (int x = 0; x < size; x++)
				{
					float dx = Math.Max(Math.Max(radius - x, x - (size - 1 - radius)), 0);
					float dy = Math.Max(Math.Max(radius - y, y - (size - 1 - radius)), 0);
					float d = (float)Math.Sqrt(dx * dx + dy * dy);
					float a = Math.Clamp(radius + 0.5f - d, 0f, 1f);
					px[y * size + x] = Color.White * a;
				}
			Texture2D t = new Texture2D(device, size, size);
			t.SetData(px);
			return t;
		}

		// ---- shapes (inside a SpriteBatch.Begin/End) ----

		public static void Fill(SpriteBatch b, Rectangle r, Color colour) => b.Draw(_pixel, r, colour);

		public static void Frame(SpriteBatch b, Rectangle r, Color colour, int t = 1)
		{
			b.Draw(_pixel, new Rectangle(r.X, r.Y, r.Width, t), colour);
			b.Draw(_pixel, new Rectangle(r.X, r.Bottom - t, r.Width, t), colour);
			b.Draw(_pixel, new Rectangle(r.X, r.Y, t, r.Height), colour);
			b.Draw(_pixel, new Rectangle(r.Right - t, r.Y, t, r.Height), colour);
		}

		/// <summary>A plate with rounded corners: the middle and the four edges as plain fills, the corners from the round texture.</summary>
		public static void RoundPlate(SpriteBatch b, Rectangle r, Color colour, Viewport v)
		{
			int cr = Math.Max(2, (int)Math.Round(8 * v.Width / W));
			cr = Math.Min(cr, Math.Min(r.Width, r.Height) / 2);
			int s = _round.Width, q = 8;   // the texture's corner is 8 of its 32
			// Corners.
			b.Draw(_round, new Rectangle(r.X, r.Y, cr, cr), new Rectangle(0, 0, q, q), colour);
			b.Draw(_round, new Rectangle(r.Right - cr, r.Y, cr, cr), new Rectangle(s - q, 0, q, q), colour);
			b.Draw(_round, new Rectangle(r.X, r.Bottom - cr, cr, cr), new Rectangle(0, s - q, q, q), colour);
			b.Draw(_round, new Rectangle(r.Right - cr, r.Bottom - cr, cr, cr), new Rectangle(s - q, s - q, q, q), colour);
			// Edges and the middle.
			b.Draw(_pixel, new Rectangle(r.X + cr, r.Y, r.Width - 2 * cr, cr), colour);
			b.Draw(_pixel, new Rectangle(r.X + cr, r.Bottom - cr, r.Width - 2 * cr, cr), colour);
			b.Draw(_pixel, new Rectangle(r.X, r.Y + cr, r.Width, r.Height - 2 * cr), colour);
		}

		/// <summary>A key or button plate, lit or not.</summary>
		public static void Key(SpriteBatch b, Rectangle textSpace, bool lit, Viewport v)
		{
			Rectangle r = Scale(textSpace, v);
			RoundPlate(b, new Rectangle(r.X, r.Y + 1, r.Width, r.Height), lit ? LitEdge : PlateEdge, v);
			RoundPlate(b, new Rectangle(r.X + 1, r.Y, r.Width - 2, r.Height - 2), lit ? Lit : Plate, v);
		}

		/// <summary>The whole panel: a dim over the game, the plate, its frame.</summary>
		public static void PanelPlate(SpriteBatch b, Rectangle textSpace, Viewport v, bool dim = true)
		{
			if (dim) Fill(b, new Rectangle(0, 0, v.Width, v.Height), Dim);
			Rectangle r = Scale(textSpace, v);
			RoundPlate(b, new Rectangle(r.X - 2, r.Y - 2, r.Width + 4, r.Height + 4), PanelEdge, v);
			RoundPlate(b, r, Panel, v);
		}

		/// <summary>The disc of a pad button's glyph at a text-space point (its centre) and size.</summary>
		public static void GlyphDisc(SpriteBatch b, float cx, float cy, float size, Color colour, Viewport v)
		{
			float sx = v.Width / W, sy = v.Height / H;
			Rectangle r = new Rectangle((int)Math.Round((cx - size / 2) * sx), (int)Math.Round((cy - size / 2) * sy), (int)Math.Round(size * sx), (int)Math.Round(size * sy));
			b.Draw(_disc, r, colour);
		}

		// ---- text (between Graphics.DrawStringStart / End) ----

		/// <summary>The height a line of text at a nominal size takes, in text space (the face is drawn at twice the nominal size).</summary>
		public static float LineHeight(int size) => size * TrueTypeText.SizeFactor * 1.15f;

		public static float Width(GlobalScope.Graphics g, string text, int size) => TrueTypeText.Enabled ? TrueTypeText.Width(text, size) : g.StringWidth(text, size);

		/// <summary>Text centred in a text-space rectangle.</summary>
		public static void Centred(GlobalScope.Graphics g, string text, Rectangle r, int size, Color colour)
		{
			float w = Width(g, text, size), h = LineHeight(size);
			g.SetColor(colour.R, colour.G, colour.B, colour.A);
			g.DrawString(text, r.X + (r.Width - w) / 2f, r.Y + (r.Height - h) / 2f + size * 0.1f, size);
		}

		/// <summary>Text at a point, its left edge there, vertically centred on a line of the given height.</summary>
		public static void Left(GlobalScope.Graphics g, string text, float x, float top, float height, int size, Color colour)
		{
			g.SetColor(colour.R, colour.G, colour.B, colour.A);
			g.DrawString(text, x, top + (height - LineHeight(size)) / 2f + size * 0.1f, size);
		}

		// ---- icons, drawn from geometry at the pixel size asked (crisp at any window size) ----
		//
		// What an SVG set would give, without a parser: each icon is a few anti-aliased strokes
		// or a filled polygon, rasterised once per size and kept. Add one by adding a case.

		public enum Icon { Cross, Circle, Square, Triangle, Menu, Up, Down, Left, Right, Shift, Backspace }

		private static readonly System.Collections.Generic.Dictionary<(Icon, int), Texture2D> _icons = new System.Collections.Generic.Dictionary<(Icon, int), Texture2D>();

		/// <summary>The icon as a white texture of the given pixel size (tint it when drawing).</summary>
		public static Texture2D IconTexture(Icon icon, int px)
		{
			px = Math.Max(8, px);
			if (_icons.TryGetValue((icon, px), out Texture2D have) && !have.IsDisposed) return have;
			float[] a = new float[px * px];
			float s = px, c = (px - 1) / 2f, t = Math.Max(1.2f, px * 0.11f);   // stroke width
			switch (icon)
			{
				case Icon.Cross:
					Stroke(a, px, c - s * 0.26f, c - s * 0.26f, c + s * 0.26f, c + s * 0.26f, t);
					Stroke(a, px, c + s * 0.26f, c - s * 0.26f, c - s * 0.26f, c + s * 0.26f, t);
					break;
				case Icon.Circle:
					Ring(a, px, c, c, s * 0.30f, t);
					break;
				case Icon.Square:
					Stroke(a, px, c - s * 0.27f, c - s * 0.27f, c + s * 0.27f, c - s * 0.27f, t);
					Stroke(a, px, c + s * 0.27f, c - s * 0.27f, c + s * 0.27f, c + s * 0.27f, t);
					Stroke(a, px, c + s * 0.27f, c + s * 0.27f, c - s * 0.27f, c + s * 0.27f, t);
					Stroke(a, px, c - s * 0.27f, c + s * 0.27f, c - s * 0.27f, c - s * 0.27f, t);
					break;
				case Icon.Triangle:
					Stroke(a, px, c, c - s * 0.31f, c + s * 0.30f, c + s * 0.22f, t);
					Stroke(a, px, c + s * 0.30f, c + s * 0.22f, c - s * 0.30f, c + s * 0.22f, t);
					Stroke(a, px, c - s * 0.30f, c + s * 0.22f, c, c - s * 0.31f, t);
					break;
				case Icon.Menu:
					for (int i = -1; i <= 1; i++) Stroke(a, px, c - s * 0.26f, c + i * s * 0.2f, c + s * 0.26f, c + i * s * 0.2f, t);
					break;
				case Icon.Up: Fill(a, px, (c, c - s * 0.28f), (c + s * 0.3f, c + s * 0.2f), (c - s * 0.3f, c + s * 0.2f)); break;
				case Icon.Down: Fill(a, px, (c, c + s * 0.28f), (c - s * 0.3f, c - s * 0.2f), (c + s * 0.3f, c - s * 0.2f)); break;
				case Icon.Left: Fill(a, px, (c - s * 0.28f, c), (c + s * 0.2f, c - s * 0.3f), (c + s * 0.2f, c + s * 0.3f)); break;
				case Icon.Right: Fill(a, px, (c + s * 0.28f, c), (c - s * 0.2f, c + s * 0.3f), (c - s * 0.2f, c - s * 0.3f)); break;
				case Icon.Shift:
					// An arrow head over a short stem, outlined.
					Fill(a, px, (c, c - s * 0.32f), (c + s * 0.3f, c + s * 0.02f), (c - s * 0.3f, c + s * 0.02f));
					Stroke(a, px, c, c, c, c + s * 0.3f, t * 1.8f);
					break;
				case Icon.Backspace:
					// A tag pointing left with a cross in it.
					Stroke(a, px, c - s * 0.34f, c, c - s * 0.1f, c - s * 0.24f, t);
					Stroke(a, px, c - s * 0.1f, c - s * 0.24f, c + s * 0.34f, c - s * 0.24f, t);
					Stroke(a, px, c + s * 0.34f, c - s * 0.24f, c + s * 0.34f, c + s * 0.24f, t);
					Stroke(a, px, c + s * 0.34f, c + s * 0.24f, c - s * 0.1f, c + s * 0.24f, t);
					Stroke(a, px, c - s * 0.1f, c + s * 0.24f, c - s * 0.34f, c, t);
					Stroke(a, px, c - s * 0.02f, c - s * 0.1f, c + s * 0.2f, c + s * 0.1f, t * 0.8f);
					Stroke(a, px, c + s * 0.2f, c - s * 0.1f, c - s * 0.02f, c + s * 0.1f, t * 0.8f);
					break;
			}
			Color[] pxs = new Color[px * px];
			for (int i = 0; i < pxs.Length; i++) pxs[i] = Color.White * Math.Clamp(a[i], 0f, 1f);
			Texture2D tex = new Texture2D(_device, px, px);
			tex.SetData(pxs);
			_icons[(icon, px)] = tex;
			return tex;
		}

		/// <summary>An anti-aliased stroke from (x0,y0) to (x1,y1), width w, added into the coverage.</summary>
		private static void Stroke(float[] a, int px, float x0, float y0, float x1, float y1, float w)
		{
			float dx = x1 - x0, dy = y1 - y0, len2 = dx * dx + dy * dy;
			int minX = (int)Math.Floor(Math.Min(x0, x1) - w), maxX = (int)Math.Ceiling(Math.Max(x0, x1) + w);
			int minY = (int)Math.Floor(Math.Min(y0, y1) - w), maxY = (int)Math.Ceiling(Math.Max(y0, y1) + w);
			for (int y = Math.Max(0, minY); y <= Math.Min(px - 1, maxY); y++)
				for (int x = Math.Max(0, minX); x <= Math.Min(px - 1, maxX); x++)
				{
					float u = len2 <= 0 ? 0 : Math.Clamp(((x - x0) * dx + (y - y0) * dy) / len2, 0f, 1f);
					float qx = x0 + u * dx - x, qy = y0 + u * dy - y;
					float d = (float)Math.Sqrt(qx * qx + qy * qy);
					a[y * px + x] = Math.Max(a[y * px + x], Math.Clamp(w / 2 + 0.5f - d, 0f, 1f));
				}
		}

		private static void Ring(float[] a, int px, float cx, float cy, float r, float w)
		{
			for (int y = 0; y < px; y++)
				for (int x = 0; x < px; x++)
				{
					float d = Math.Abs((float)Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) - r);
					a[y * px + x] = Math.Max(a[y * px + x], Math.Clamp(w / 2 + 0.5f - d, 0f, 1f));
				}
		}

		/// <summary>A filled, anti-aliased triangle.</summary>
		private static void Fill(float[] a, int px, (float x, float y) p0, (float x, float y) p1, (float x, float y) p2)
		{
			float Edge((float x, float y) u, (float x, float y) v, float x, float y)
			{
				float ex = v.x - u.x, ey = v.y - u.y, len = (float)Math.Sqrt(ex * ex + ey * ey);
				return len <= 0 ? 0 : ((x - u.x) * ey - (y - u.y) * ex) / len;   // signed distance to the edge's line
			}
			// Orientation, so "inside" is positive for all three edges: the centroid is inside.
			float mx = (p0.x + p1.x + p2.x) / 3f, my = (p0.y + p1.y + p2.y) / 3f;
			float sign = Edge(p0, p1, mx, my) >= 0 ? 1 : -1;
			for (int y = 0; y < px; y++)
				for (int x = 0; x < px; x++)
				{
					float d0 = Edge(p0, p1, x, y) * sign, d1 = Edge(p1, p2, x, y) * sign, d2 = Edge(p2, p0, x, y) * sign;
					float d = Math.Min(d0, Math.Min(d1, d2));   // positive inside; the distance to the nearest edge
					a[y * px + x] = Math.Max(a[y * px + x], Math.Clamp(d + 0.5f, 0f, 1f));
				}
		}

		/// <summary>Draws an icon centred on a text-space point at a text-space size, tinted.</summary>
		public static void IconAt(SpriteBatch b, Icon icon, float cx, float cy, float size, Color tint, Viewport v)
		{
			float sx = v.Width / W, sy = v.Height / H;
			int px = Math.Max(8, (int)Math.Round(size * sx));
			Texture2D tex = IconTexture(icon, px);
			b.Draw(tex, new Rectangle((int)Math.Round(cx * sx - px / 2f), (int)Math.Round(cy * sy - px / 2f), px, px), tint);
		}

		// ---- pad glyphs ----

		public enum PadButton { A, B, X, Y, Start, L, R }

		/// <summary>The icon a PlayStation pad's button carries, or null for a lettered one.</summary>
		private static Icon? IconFor(PadButton button, bool ps)
		{
			switch (button)
			{
				case PadButton.A: return ps ? Icon.Cross : (Icon?)null;
				case PadButton.B: return ps ? Icon.Circle : (Icon?)null;
				case PadButton.X: return ps ? Icon.Square : (Icon?)null;
				case PadButton.Y: return ps ? Icon.Triangle : (Icon?)null;
				case PadButton.Start: return Icon.Menu;
				default: return null;
			}
		}

		/// <summary>Whether the connected pad speaks PlayStation (glyphs) rather than Xbox (letters).</summary>
		public static bool PlayStationPad
		{
			get
			{
				// settings.json's "padStyle" (ps | xbox) or --padstyle= decides outright; else the pad's name.
				string style = Options.Get("padstyle") ?? DisplaySettings.Current.PadStyle;
				if (string.Equals(style, "ps", StringComparison.OrdinalIgnoreCase)) return true;
				if (string.Equals(style, "xbox", StringComparison.OrdinalIgnoreCase)) return false;
				for (int i = 0; i < 4; i++)
				{
					try
					{
						if (!GamePad.GetState((PlayerIndex)i).IsConnected) continue;
						string name = GamePad.GetCapabilities((PlayerIndex)i).DisplayName ?? "";
						return name.IndexOf("PS", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("DualS", StringComparison.OrdinalIgnoreCase) >= 0
							|| name.IndexOf("Sony", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Wireless Controller", StringComparison.OrdinalIgnoreCase) >= 0
							|| name.IndexOf("PlayStation", StringComparison.OrdinalIgnoreCase) >= 0;
					}
					catch (Exception) { }
				}
				return false;
			}
		}

		/// <summary>The glyph's letter or mark, and its colour, for the pad in hand (the PlayStation colours, or a neutral plate for letters).</summary>
		public static (string Mark, Color Face, Color Ink) Glyph(PadButton button)
		{
			bool ps = PlayStationPad;
			switch (button)
			{
				case PadButton.A: return ps ? ("×", new Color(112, 160, 226), Color.White) : ("A", new Color(96, 176, 96), Color.White);
				case PadButton.B: return ps ? ("○", new Color(230, 100, 100), Color.White) : ("B", new Color(214, 84, 84), Color.White);
				case PadButton.X: return ps ? ("□", new Color(220, 132, 196), Color.White) : ("X", new Color(84, 140, 214), Color.White);
				case PadButton.Y: return ps ? ("△", new Color(114, 204, 132), Color.White) : ("Y", new Color(220, 190, 70), Color.White);
				case PadButton.Start: return (ps ? "≡" : "≡", new Color(150, 160, 190), Color.White);
				case PadButton.L: return (ps ? "L1" : "LB", new Color(150, 160, 190), Color.White);
				default: return (ps ? "R1" : "RB", new Color(150, 160, 190), Color.White);
			}
		}

		/// <summary>The width a hint takes in text space: the disc, a gap, the word.</summary>
		public static float HintWidth(GlobalScope.Graphics g, string word, int size, float disc) => disc + 8 + Width(g, word, size);

		/// <summary>Shape pass of a hint: the button's disc at (x, cy), and its icon when it has one (the PlayStation marks, the menu lines).</summary>
		public static void HintShape(SpriteBatch b, PadButton button, float x, float cy, float disc, Viewport v)
		{
			(string _, Color face, Color ink) = Glyph(button);
			GlyphDisc(b, x + disc / 2, cy, disc, face, v);
			Icon? icon = IconFor(button, PlayStationPad);
			if (icon.HasValue) IconAt(b, icon.Value, x + disc / 2, cy, disc * 0.92f, ink, v);
		}

		/// <summary>Text pass of a hint: the letter in the disc when the button has one rather than an icon, the word beside it. Returns where the next hint may start.</summary>
		public static float HintText(GlobalScope.Graphics g, PadButton button, string word, float x, float cy, float disc, int size)
		{
			(string mark, Color _, Color ink) = Glyph(button);
			if (!IconFor(button, PlayStationPad).HasValue)
			{
				int markSize = mark.Length > 1 ? Math.Max(6, size - 3) : size;
				Centred(g, mark, new Rectangle((int)x, (int)(cy - disc / 2), (int)disc, (int)disc), markSize, ink);
			}
			Left(g, word, x + disc + 8, cy - disc / 2, disc, size, Muted);
			return x + HintWidth(g, word, size, disc) + 26;
		}
	}
}
