// A menu frame's box painted as CSS paints one (MenuBackground's gradient, border, round corners
// and shadows): into pictures of its own, since the game draws only textured quads. The client
// makes them textures and draws them with the frame's panel (MenuPanel.cs); Crystal's preview
// paints the same with the browser's canvas.
//
//   Shadow  the outer shadows (the first on top, cut away under the frame itself) - behind the frame,
//           behind the game's window too
//   Fill    the colour and the gradient - under the frame's picture, if it has one
//   Over    the inset shadows and the border - over it
//
// Painted at a scale (pixels per menu unit, the window's), anti-aliased at the round corners. A
// big frame is painted at less, so that one takes no more than about a million pixels.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenFF.Content
{
	/// <summary>A CSS gradient: linear, radial or conic, repeating or not, and its colour stops.</summary>
	internal sealed class MenuGradient
	{
		public string Kind;          // linear, radial, conic
		public bool Repeating;
		/// <summary>Linear: the direction, in CSS degrees (0 up, 90 right), or a corner (Corner set).</summary>
		public float Angle = 180;
		public int CornerX, CornerY;   // -1 / 1 for "to left top" and the rest; 0 for an angle
		/// <summary>Radial: circle or ellipse, and its size: a keyword or lengths.</summary>
		public bool Circle;
		public string SizeWord = "farthest-corner";
		public string SizeX, SizeY;
		/// <summary>Radial and conic: the centre ("50%" by default), as CSS positions it.</summary>
		public string AtX = "50%", AtY = "50%";
		/// <summary>Conic: the angle it starts from.</summary>
		public float From;
		public List<(uint Colour, string Position)> Stops = new List<(uint, string)>();

		/// <summary>A gradient from background-image's value; null for one that is not.</summary>
		public static MenuGradient Parse(string value)
		{
			string v = value?.Trim();
			if (string.IsNullOrEmpty(v)) return null;
			Match m = Regex.Match(v, @"^(repeating-)?(linear|radial|conic)-gradient\((.*)\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
			if (!m.Success) return null;
			MenuGradient g = new MenuGradient { Repeating = m.Groups[1].Success && m.Groups[1].Length > 0, Kind = m.Groups[2].Value.ToLowerInvariant() };
			List<string> args = MenuAnimation.CommaList(m.Groups[3].Value);
			if (args.Count == 0) return null;
			int first = 0;
			// The first argument says the direction or the shape when it holds no colour.
			if (MenuText.Colour(MenuAnimation.Words(args[0]).FirstOrDefault() ?? "") == null)
			{
				first = 1;
				List<string> words = MenuAnimation.Words(args[0].ToLowerInvariant());
				if (g.Kind == "linear")
				{
					if (words.Count > 0 && words[0] == "to")
					{
						foreach (string w in words.Skip(1))
						{
							if (w == "left") g.CornerX = -1; else if (w == "right") g.CornerX = 1;
							else if (w == "top") g.CornerY = -1; else if (w == "bottom") g.CornerY = 1;
						}
						if (g.CornerX == 0 || g.CornerY == 0)
						{
							g.Angle = g.CornerX == 1 ? 90 : g.CornerX == -1 ? 270 : g.CornerY == -1 ? 0 : 180;
							g.CornerX = g.CornerY = 0;
						}
					}
					else if (words.Count > 0 && Degrees(words[0]) is float a) g.Angle = a;
				}
				else
				{
					int at = words.IndexOf("at");
					List<string> shape = at >= 0 ? words.Take(at).ToList() : words;
					if (at >= 0)
					{
						List<string> pos = words.Skip(at + 1).ToList();
						(g.AtX, g.AtY) = Position(pos);
					}
					if (g.Kind == "conic")
					{
						int from = shape.IndexOf("from");
						if (from >= 0 && from + 1 < shape.Count && Degrees(shape[from + 1]) is float a) g.From = a;
					}
					else
					{
						List<string> lengths = new List<string>();
						foreach (string w in shape)
						{
							if (w == "circle") g.Circle = true;
							else if (w == "ellipse") g.Circle = false;
							else if (w == "closest-side" || w == "farthest-side" || w == "closest-corner" || w == "farthest-corner") g.SizeWord = w;
							else lengths.Add(w);
						}
						if (lengths.Count > 0) { g.SizeWord = null; g.SizeX = lengths[0]; g.SizeY = lengths.Count > 1 ? lengths[1] : lengths[0]; if (lengths.Count == 1) g.Circle = true; }
					}
				}
			}
			for (int i = first; i < args.Count; i++)
			{
				List<string> words = MenuAnimation.Words(args[i]);
				uint? colour = null;
				List<string> positions = new List<string>();
				foreach (string w in words)
				{
					if (colour == null && MenuText.Colour(w) is uint c) colour = c;
					else positions.Add(w);
				}
				if (colour == null) continue;   // a colour hint alone: passed over
				if (positions.Count == 0) g.Stops.Add((colour.Value, null));
				foreach (string p in positions.Take(2)) g.Stops.Add((colour.Value, p));
			}
			return g.Stops.Count >= 1 ? g : null;
		}

		private static (string, string) Position(List<string> words)
		{
			string x = "50%", y = "50%";
			foreach (string w in words)
			{
				if (w == "left") x = "0%"; else if (w == "right") x = "100%";
				else if (w == "top") y = "0%"; else if (w == "bottom") y = "100%";
				else if (w == "center") { }
				else if (x == "50%" && words.IndexOf(w) == 0) x = w;
				else y = w;
			}
			return (x, y);
		}

		public static float? Degrees(string word)
		{
			string w = word.Trim().ToLowerInvariant();
			float Num(string t) => float.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out float f) ? f : float.NaN;
			float v = float.NaN;
			if (w.EndsWith("deg")) v = Num(w.Substring(0, w.Length - 3));
			else if (w.EndsWith("grad")) v = Num(w.Substring(0, w.Length - 4)) * 0.9f;
			else if (w.EndsWith("rad")) v = Num(w.Substring(0, w.Length - 3)) * 180 / (float)Math.PI;
			else if (w.EndsWith("turn")) v = Num(w.Substring(0, w.Length - 4)) * 360;
			return float.IsNaN(v) ? (float?)null : v;
		}
	}

	internal static class MenuPaint
	{
		public enum Layer { Shadow, Fill, Over, All }

		/// <summary>A painted picture: RGBA (not premultiplied), and where it goes against the frame (menu units, from its top left).</summary>
		public sealed class Raster
		{
			public byte[] Pixels;
			public int Width, Height;
			public float X, Y, W, H;
		}

		private const int MaxPixels = 1_000_000;

		/// <summary>How far past the frame the outer shadows reach (menu units): the picture's margin.</summary>
		public static float Reach(MenuBackground b)
		{
			float m = 0;
			foreach (MenuBackground.BoxShadow s in b.Shadows.Where(s => !s.Inset)) m = Math.Max(m, s.Blur + Math.Max(0, s.Spread) + Math.Max(Math.Abs(s.X), Math.Abs(s.Y)));
			return (float)Math.Ceiling(m);
		}

		/// <summary>Whether a layer has anything in it for this background.</summary>
		public static bool Has(MenuBackground b, Layer layer)
		{
			bool shadow = b.Shadows.Any(s => !s.Inset);
			bool fill = b.Gradient != null || (b.Colour.HasValue && (b.Colour.Value & 0xFF) > 0);
			bool over = b.Shadows.Any(s => s.Inset) || b.HasBorder;
			return layer == Layer.Shadow ? shadow : layer == Layer.Fill ? fill : layer == Layer.Over ? over : shadow || fill || over;
		}

		/// <summary>One layer of a frame's box (w by h menu units) at a scale, or null for an empty one.</summary>
		public static Raster Paint(MenuBackground b, float w, float h, float scale, Layer layer)
		{
			if (b == null || w <= 0 || h <= 0 || !Has(b, layer)) return null;
			bool shadows = layer == Layer.Shadow || layer == Layer.All, fills = layer == Layer.Fill || layer == Layer.All, over = layer == Layer.Over || layer == Layer.All;
			float margin = shadows ? Reach(b) : 0;
			float fw = w + margin * 2, fh = h + margin * 2;
			scale = Math.Max(0.5f, Math.Min(scale, (float)Math.Sqrt(MaxPixels / Math.Max(1, fw * fh))));
			int pw = Math.Max(1, (int)Math.Ceiling(fw * scale)), ph = Math.Max(1, (int)Math.Ceiling(fh * scale));
			Canvas c = new Canvas(pw, ph, scale, margin);
			float[] radii = b.Radii(w, h);
			Box border = new Box(0, 0, w, h, radii);
			float bt = b.BorderWidth[0], br = b.BorderWidth[1], bb = b.BorderWidth[2], bl = b.BorderWidth[3];
			Box padding = new Box(bl, bt, w - br, h - bb, new[] { Math.Max(0, radii[0] - Math.Max(bl, bt)), Math.Max(0, radii[1] - Math.Max(br, bt)), Math.Max(0, radii[2] - Math.Max(br, bb)), Math.Max(0, radii[3] - Math.Max(bl, bb)) });

			float[] boxCover = shadows || fills ? c.Coverage(border) : null;
			if (shadows)
			{
				// Outer shadows, the last underneath; each cut away where the frame is (CSS draws none under a see-through box).
				foreach (MenuBackground.BoxShadow s in Enumerable.Reverse(b.Shadows).Where(s => !s.Inset))
				{
					Box shape = border.Offset(s.X, s.Y).Grown(s.Spread);
					float[] mask = c.Coverage(shape);
					if (s.Blur > 0) Blur(mask, pw, ph, s.Blur / 2 * c.Scale, 0);
					for (int i = 0; i < mask.Length; i++) mask[i] *= 1 - boxCover[i];
					c.Fill(mask, s.Colour);
				}
			}
			if (fills)
			{
				if (b.Colour.HasValue && (b.Colour.Value & 0xFF) > 0) c.Fill(boxCover, b.Colour.Value);
				if (b.Gradient != null) c.Gradient(boxCover, b.Gradient, w, h);
			}
			if (over)
			{
				float[] inner = c.Coverage(padding);
				foreach (MenuBackground.BoxShadow s in Enumerable.Reverse(b.Shadows).Where(s => s.Inset))
				{
					Box hole = padding.Offset(s.X, s.Y).Grown(-s.Spread);
					float[] mask = c.Coverage(hole);
					for (int i = 0; i < mask.Length; i++) mask[i] = 1 - mask[i];
					if (s.Blur > 0) Blur(mask, pw, ph, s.Blur / 2 * c.Scale, 1);
					for (int i = 0; i < mask.Length; i++) mask[i] *= inner[i];
					c.Fill(mask, s.Colour);
				}
				if (b.HasBorder)
				{
					float[] outer = c.Coverage(border);
					float[] ring = new float[outer.Length];
					for (int i = 0; i < ring.Length; i++) ring[i] = Math.Max(0, outer[i] - inner[i]);
					if (b.BorderColour.Distinct().Count() == 1) c.Fill(ring, b.BorderColour[0]);
					else c.Sides(ring, b.BorderWidth, b.BorderColour, w, h);
				}
			}
			return new Raster { Pixels = c.Bytes(), Width = pw, Height = ph, X = -margin, Y = -margin, W = fw, H = fh };
		}

		// ------------------------------------------------------------------ shapes

		/// <summary>A rectangle with round corners (top-left, top-right, bottom-right, bottom-left), in menu units.</summary>
		private readonly struct Box
		{
			public readonly float X0, Y0, X1, Y1;
			public readonly float[] R;

			public Box(float x0, float y0, float x1, float y1, float[] r) { X0 = x0; Y0 = y0; X1 = Math.Max(x0, x1); Y1 = Math.Max(y0, y1); R = r; }

			public Box Offset(float dx, float dy) => new Box(X0 + dx, Y0 + dy, X1 + dx, Y1 + dy, R);

			/// <summary>Grown (or, by less than nought, shrunk) on every side, its corners with it - as a shadow's spread does.</summary>
			public Box Grown(float by)
			{
				float[] r = R.Select(x => x > 0 ? Math.Max(0, x + by) : 0).ToArray();
				return new Box(X0 - by, Y0 - by, X1 + by, Y1 + by, r);
			}

			/// <summary>The signed distance from a point to the edge (menu units; less than nought inside).</summary>
			public float Distance(float x, float y)
			{
				float cx = (X0 + X1) / 2, cy = (Y0 + Y1) / 2, hw = (X1 - X0) / 2, hh = (Y1 - Y0) / 2;
				float r = x < cx ? (y < cy ? R[0] : R[3]) : (y < cy ? R[1] : R[2]);
				r = Math.Min(r, Math.Min(hw, hh));
				float qx = Math.Abs(x - cx) - (hw - r), qy = Math.Abs(y - cy) - (hh - r);
				float ox = Math.Max(qx, 0), oy = Math.Max(qy, 0);
				return (float)Math.Sqrt(ox * ox + oy * oy) + Math.Min(Math.Max(qx, qy), 0) - r;
			}
		}

		/// <summary>The picture being painted: premultiplied RGBA floats, the frame's top left at (margin, margin) units in.</summary>
		private sealed class Canvas
		{
			public readonly int W, H;
			public readonly float Scale, Margin;
			private readonly float[] _r, _g, _b, _a;

			public Canvas(int w, int h, float scale, float margin)
			{
				W = w; H = h; Scale = scale; Margin = margin;
				_r = new float[w * h]; _g = new float[w * h]; _b = new float[w * h]; _a = new float[w * h];
			}

			private float UnitX(int i) => (i + 0.5f) / Scale - Margin;
			private float UnitY(int j) => (j + 0.5f) / Scale - Margin;

			/// <summary>How much of each pixel a box covers (0..1), smoothed over a pixel at its edge.</summary>
			public float[] Coverage(Box box)
			{
				float[] c = new float[W * H];
				if (box.X1 <= box.X0 || box.Y1 <= box.Y0) return c;
				for (int j = 0; j < H; j++)
				{
					float y = UnitY(j);
					for (int i = 0; i < W; i++)
					{
						float d = box.Distance(UnitX(i), y) * Scale;
						c[j * W + i] = Math.Clamp(0.5f - d, 0, 1);
					}
				}
				return c;
			}

			/// <summary>A colour laid over the picture where a mask says, as much as it says.</summary>
			public void Fill(float[] mask, uint rgba)
			{
				float r = (rgba >> 24 & 0xFF) / 255f, g = (rgba >> 16 & 0xFF) / 255f, b = (rgba >> 8 & 0xFF) / 255f, a = (rgba & 0xFF) / 255f;
				for (int i = 0; i < mask.Length; i++)
				{
					float sa = a * mask[i];
					if (sa <= 0) continue;
					Over(i, r * sa, g * sa, b * sa, sa);
				}
			}

			private void Over(int i, float r, float g, float b, float a)
			{
				float k = 1 - a;
				_r[i] = r + _r[i] * k; _g[i] = g + _g[i] * k; _b[i] = b + _b[i] * k; _a[i] = a + _a[i] * k;
			}

			/// <summary>A gradient over the frame (w by h), where the mask says.</summary>
			public void Gradient(float[] mask, MenuGradient g, float w, float h)
			{
				List<(float At, float R, float G, float B, float A)> stops = Stops(g, w, h);
				if (stops.Count == 0) return;
				float cx = Length(g.AtX, w), cy = Length(g.AtY, h);
				// Linear: the gradient line through the middle, as long as the frame's corners need.
				double dx = 0, dy = 0, length = 1;
				if (g.Kind == "linear")
				{
					if (g.CornerX != 0 && g.CornerY != 0)
					{
						// To a corner: at right angles to the line between the two corners beside it.
						dx = h * g.CornerX; dy = w * g.CornerY;
						double n = Math.Sqrt(dx * dx + dy * dy); dx /= n; dy /= n;
					}
					else { double a = g.Angle * Math.PI / 180; dx = Math.Sin(a); dy = -Math.Cos(a); }
					length = Math.Abs(w * dx) + Math.Abs(h * dy);
				}
				(double rx, double ry) = g.Kind == "radial" ? RadialSize(g, w, h, cx, cy) : (1, 1);
				float first = stops[0].At, last = stops[stops.Count - 1].At, span = last - first;
				for (int j = 0; j < H; j++)
				{
					float y = UnitY(j);
					for (int i = 0; i < W; i++)
					{
						int k = j * W + i;
						if (mask[k] <= 0) continue;
						float x = UnitX(i);
						double t;
						if (g.Kind == "linear") t = ((x - w / 2) * dx + (y - h / 2) * dy) / length + 0.5;
						else if (g.Kind == "radial") { double ex = (x - cx) / rx, ey = (y - cy) / ry; t = Math.Sqrt(ex * ex + ey * ey); }
						else
						{
							double deg = Math.Atan2(x - cx, -(y - cy)) * 180 / Math.PI - g.From;
							t = ((deg % 360) + 360) % 360 / 360;
						}
						if (g.Repeating && span > 0.0001f) t = first + (((t - first) % span) + span) % span;
						(float r, float gg, float b, float a) = At(stops, (float)t);
						float m = mask[k];
						Over(k, r * a * m, gg * a * m, b * a * m, a * m);
					}
				}
			}

			/// <summary>The border's ring coloured side by side: each pixel by the side it is nearest, measured in that side's widths.</summary>
			public void Sides(float[] ring, float[] widths, uint[] colours, float w, float h)
			{
				for (int j = 0; j < H; j++)
				{
					float y = UnitY(j);
					for (int i = 0; i < W; i++)
					{
						int k = j * W + i;
						if (ring[k] <= 0) continue;
						float x = UnitX(i);
						float[] d = { widths[0] > 0 ? y / widths[0] : float.MaxValue, widths[1] > 0 ? (w - x) / widths[1] : float.MaxValue, widths[2] > 0 ? (h - y) / widths[2] : float.MaxValue, widths[3] > 0 ? x / widths[3] : float.MaxValue };
						int side = Array.IndexOf(d, d.Min());
						uint c = colours[side];
						float a = (c & 0xFF) / 255f * ring[k];
						Over(k, (c >> 24 & 0xFF) / 255f * a, (c >> 16 & 0xFF) / 255f * a, (c >> 8 & 0xFF) / 255f * a, a);
					}
				}
			}

			public byte[] Bytes()
			{
				byte[] o = new byte[W * H * 4];
				for (int i = 0; i < _a.Length; i++)
				{
					float a = _a[i];
					if (a <= 0) continue;
					o[i * 4] = (byte)Math.Clamp((int)Math.Round(_r[i] / a * 255), 0, 255);
					o[i * 4 + 1] = (byte)Math.Clamp((int)Math.Round(_g[i] / a * 255), 0, 255);
					o[i * 4 + 2] = (byte)Math.Clamp((int)Math.Round(_b[i] / a * 255), 0, 255);
					o[i * 4 + 3] = (byte)Math.Clamp((int)Math.Round(a * 255), 0, 255);
				}
				return o;
			}
		}

		// ------------------------------------------------------------------ the gradient's stops

		/// <summary>The stops at their places along the gradient (0..1 of its line or radius, or of the turn), missing places spread between their neighbours, none before the one before it - as CSS places them.</summary>
		private static List<(float At, float R, float G, float B, float A)> Stops(MenuGradient g, float w, float h)
		{
			float length = g.Kind == "linear" ? LinearLength(g, w, h) : g.Kind == "radial" ? (float)RadialSize(g, w, h, Length(g.AtX, w), Length(g.AtY, h)).Item1 : 360;
			List<float?> at = g.Stops.Select(s => Place(s.Position, length, g.Kind == "conic")).ToList();
			if (at.Count > 0 && at[0] == null) at[0] = 0;
			if (at.Count > 1 && at[at.Count - 1] == null) at[at.Count - 1] = 1;
			for (int i = 0; i < at.Count; i++)
			{
				if (at[i] != null) continue;
				int next = i;
				while (next < at.Count && at[next] == null) next++;
				float a = at[i - 1].Value, b = at[next].Value;
				for (int k = i; k < next; k++) at[k] = a + (b - a) * (k - i + 1) / (next - i + 1);
			}
			float max = float.MinValue;
			List<(float, float, float, float, float)> stops = new List<(float, float, float, float, float)>();
			for (int i = 0; i < at.Count; i++)
			{
				float p = Math.Max(max, at[i] ?? 0);
				max = p;
				uint c = g.Stops[i].Colour;
				stops.Add((p, (c >> 24 & 0xFF) / 255f, (c >> 16 & 0xFF) / 255f, (c >> 8 & 0xFF) / 255f, (c & 0xFF) / 255f));
			}
			if (stops.Count == 1) stops.Add((1, stops[0].Item2, stops[0].Item3, stops[0].Item4, stops[0].Item5));
			return stops;
		}

		private static float? Place(string position, float length, bool conic)
		{
			if (position == null) return null;
			string p = position.Trim().ToLowerInvariant();
			if (p.EndsWith("%") && float.TryParse(p.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out float pc)) return pc / 100;
			if (conic && MenuGradient.Degrees(p) is float deg) return deg / 360;
			if (MenuText.Length(p) is float px) return length > 0 ? px / length : 0;
			return null;
		}

		/// <summary>The colour at a place along the stops (straight RGBA), mixed premultiplied between the two around it.</summary>
		private static (float, float, float, float) At(List<(float At, float R, float G, float B, float A)> stops, float t)
		{
			if (t <= stops[0].At) return (stops[0].R, stops[0].G, stops[0].B, stops[0].A);
			for (int i = 1; i < stops.Count; i++)
			{
				if (t > stops[i].At) continue;
				var a = stops[i - 1]; var b = stops[i];
				float f = b.At - a.At <= 0 ? 1 : (t - a.At) / (b.At - a.At);
				float alpha = a.A + (b.A - a.A) * f;
				if (alpha <= 0) return (0, 0, 0, 0);
				float Ch(float x, float y) => (x * a.A + (y * b.A - x * a.A) * f) / alpha;
				return (Ch(a.R, b.R), Ch(a.G, b.G), Ch(a.B, b.B), alpha);
			}
			var last = stops[stops.Count - 1];
			return (last.R, last.G, last.B, last.A);
		}

		private static float LinearLength(MenuGradient g, float w, float h)
		{
			if (g.CornerX != 0 && g.CornerY != 0) { double dx = h, dy = w, n = Math.Sqrt(dx * dx + dy * dy); return (float)(Math.Abs(w * dx / n) + Math.Abs(h * dy / n)); }
			double a = g.Angle * Math.PI / 180;
			return (float)(Math.Abs(w * Math.Sin(a)) + Math.Abs(h * Math.Cos(a)));
		}

		/// <summary>A radial gradient's radii (menu units): its lengths, or its size keyword from the centre to the frame's sides or corners.</summary>
		private static (double, double) RadialSize(MenuGradient g, float w, float h, float cx, float cy)
		{
			if (g.SizeWord == null)
			{
				double sx = Length(g.SizeX, w), sy = g.Circle ? sx : Length(g.SizeY, h);
				return (Math.Max(0.01, sx), Math.Max(0.01, sy));
			}
			double left = cx, right = w - cx, top = cy, bottom = h - cy;
			double nearX = Math.Min(Math.Abs(left), Math.Abs(right)), farX = Math.Max(Math.Abs(left), Math.Abs(right));
			double nearY = Math.Min(Math.Abs(top), Math.Abs(bottom)), farY = Math.Max(Math.Abs(top), Math.Abs(bottom));
			double rx, ry;
			switch (g.SizeWord)
			{
				case "closest-side": rx = nearX; ry = nearY; if (g.Circle) rx = ry = Math.Min(nearX, nearY); break;
				case "farthest-side": rx = farX; ry = farY; if (g.Circle) rx = ry = Math.Max(farX, farY); break;
				case "closest-corner":
					if (g.Circle) rx = ry = Math.Sqrt(nearX * nearX + nearY * nearY);
					else { rx = nearX * Math.Sqrt(2); ry = nearY * Math.Sqrt(2); }
					break;
				default:
					if (g.Circle) rx = ry = Math.Sqrt(farX * farX + farY * farY);
					else { rx = farX * Math.Sqrt(2); ry = farY * Math.Sqrt(2); }
					break;
			}
			return (Math.Max(0.01, rx), Math.Max(0.01, ry));
		}

		private static float Length(string text, float whole)
		{
			string t = text?.Trim().ToLowerInvariant() ?? "50%";
			if (t.EndsWith("%") && float.TryParse(t.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out float p)) return whole * p / 100;
			return MenuText.Length(t) ?? whole / 2;
		}

		// ------------------------------------------------------------------ blur

		/// <summary>A mask blurred as a Gaussian of sigma pixels would blur it: three box blurs each way; past the edges it is 'outside'.</summary>
		private static void Blur(float[] mask, int w, int h, float sigma, float outside)
		{
			if (sigma < 0.3f) return;
			int[] boxes = Boxes(sigma, 3);
			float[] tmp = new float[mask.Length];
			foreach (int size in boxes)
			{
				int r = (size - 1) / 2;
				BoxPass(mask, tmp, w, h, r, true, outside);
				BoxPass(tmp, mask, w, h, r, false, outside);
			}
		}

		private static int[] Boxes(float sigma, int n)
		{
			double wIdeal = Math.Sqrt(12 * sigma * sigma / n + 1);
			int wl = (int)Math.Floor(wIdeal);
			if (wl % 2 == 0) wl--;
			int wu = wl + 2;
			double mIdeal = (12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
			int m = (int)Math.Round(mIdeal);
			int[] sizes = new int[n];
			for (int i = 0; i < n; i++) sizes[i] = i < m ? wl : wu;
			return sizes;
		}

		private static void BoxPass(float[] src, float[] dst, int w, int h, int r, bool horizontal, float outside)
		{
			if (r <= 0) { Array.Copy(src, dst, src.Length); return; }
			int lines = horizontal ? h : w, len = horizontal ? w : h;
			float k = 1f / (r * 2 + 1);
			for (int line = 0; line < lines; line++)
			{
				int Index(int p) => horizontal ? line * w + p : p * w + line;
				float Get(int p) => p < 0 || p >= len ? outside : src[Index(p)];
				float sum = 0;
				for (int p = -r; p <= r; p++) sum += Get(p);
				for (int p = 0; p < len; p++)
				{
					dst[Index(p)] = sum * k;
					sum += Get(p + r + 1) - Get(p - r);
				}
			}
		}
	}
}
