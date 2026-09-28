// A menu frame's background (Crystal Style Sheets): a colour and an image behind the frame, the
// image stretched, cropped, fitted, repeated or 9-sliced, as UI Toolkit's Background panel does it.
//
//   #w_rules {
//     -ff-panel: none;                                  /* no game window: the picture is the panel */
//     background-image: url("images/panel.png");        /* a file beside the layout */
//     -ff-slice: 12;                                    /* borders kept, 12 px of the picture each side */
//     -ff-slice-scale: 1;                               /* a picture pixel a menu unit */
//     -ff-slice-type: sliced;                           /* the middle and the edges stretched (tiled: repeated) */
//   }
//   #portrait { background-image: resource("files/face_00.png"); -ff-background-scale-mode: scale-to-fit; }
//   #banner   { background-color: #10204080; background-image: url("images/stars.png"); background-repeat: repeat; }
//
//   background-color            #rgb, #rrggbb, #rrggbbaa, rgba(r, g, b, a), transparent - under the image
//   background-image            url("...") a file beside the layout (a mod's menus/, the client's Data/menus/);
//                               resource("...") one of the game's files by name; none
//   -ff-background-rect         x y w h: the part of the picture to use (a sprite on a sheet), in its pixels
//   -ff-background-tint         the picture multiplied by it (UI Toolkit's image tint)
//   -ff-background-scale-mode   stretch-to-fill (the default), scale-and-crop, scale-to-fit
//   background-size             auto, cover, contain, or a width and height (px or %) - with repeat and position
//   background-position         x y: left / center / right, top / center / bottom, px or %
//   background-repeat           no-repeat, repeat, repeat-x, repeat-y
//   -ff-slice                   top right bottom left (one to four, as margin), or -ff-slice-left/-top/-right/-bottom
//   -ff-slice-scale             the borders' size on the screen per picture pixel (1)
//   -ff-slice-type              sliced (stretched) or tiled (the edges and the middle repeated)
//   -ff-background-filter       linear (smooth when scaled, the default) or point (the pixels kept)
//   -ff-sprite                  a named sprite of the sheet (sprites.json, MenuSprites): its part and its borders
//
// And the box round it, as CSS draws a box (MenuPaint paints these into pictures of their own):
//
//   background-image            linear-gradient(), radial-gradient(), conic-gradient() and their repeating-
//                               kinds, in place of a picture: [angle | to side] / [shape size at x y] / [from
//                               angle at x y], then colour stops (colour [position]: %, px, deg)
//   border                      width [style] colour - and border-width (one to four), border-color (one to
//                               four), border-style (solid; none takes it away), border-top / -right / -bottom /
//                               -left and their -width / -color
//   border-radius               one to four corners (top-left, top-right, bottom-right, bottom-left), px or %
//                               (of the shorter side); border-top-left-radius and the rest
//   box-shadow                  none, or [inset] x y [blur [spread]] colour, ... - the first on top; an outer
//                               one is drawn behind the frame (behind the game's window too), an inset one inside it
//
// A slice over 0 decides it: the frame is drawn 9-sliced and the scale mode, size and repeat are
// not used. Otherwise background-size or -repeat lay the picture out as CSS does; with neither, the
// scale mode does. MenuStyles bakes the declarations into the frame's <background>; the client draws
// what Layout gives as the frame's panel (MenuPanel.cs), and Crystal's preview draws the same.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OpenFF.Content
{
	internal sealed class MenuBackground
	{
		/// <summary>The properties that make a background, in the order MenuStyles writes them.</summary>
		public static readonly string[] Properties =
		{
			"background-color", "background-image", "-ff-background-rect", "-ff-background-tint", "-ff-background-scale-mode",
			"background-size", "background-position", "background-repeat",
			"-ff-slice", "-ff-slice-left", "-ff-slice-top", "-ff-slice-right", "-ff-slice-bottom", "-ff-slice-scale", "-ff-slice-type",
			"-ff-background-filter", "-ff-sprite",
			"border", "border-width", "border-color", "border-style",
			"border-top", "border-right", "border-bottom", "border-left",
			"border-top-width", "border-right-width", "border-bottom-width", "border-left-width",
			"border-top-color", "border-right-color", "border-bottom-color", "border-left-color",
			"border-radius", "border-top-left-radius", "border-top-right-radius", "border-bottom-right-radius", "border-bottom-left-radius",
			"box-shadow"
		};

		/// <summary>"url" (a file beside the layout) or "resource" (one of the game's), and its path; null for no picture.</summary>
		public string ImageKind;
		public string ImagePath;
		/// <summary>RGBA, 0xRRGGBBAA; null for none.</summary>
		public uint? Colour;
		public uint Tint = 0xFFFFFFFF;
		public int[] Rect;
		public string ScaleMode;
		public string Size;
		public string Position;
		public string Repeat;
		public float SliceLeft, SliceTop, SliceRight, SliceBottom;
		public float SliceScale = 1;
		public bool Tiled;
		public bool Linear = true;
		/// <summary>A named sprite of the sheet (MenuSprites: sprites.json beside the layouts) - its part and its borders; null for none.</summary>
		public string Sprite;
		/// <summary>Whether the frame gave slices of its own (then a sprite's borders do not replace them).</summary>
		public bool SliceGiven;

		/// <summary>A gradient in place of a picture; null for none.</summary>
		public MenuGradient Gradient;
		/// <summary>The border's widths and colours, top, right, bottom, left (menu units, 0xRRGGBBAA).</summary>
		public float[] BorderWidth = new float[4];
		public uint[] BorderColour = { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
		/// <summary>The corners' radii, top-left, top-right, bottom-right, bottom-left, and whether each is a percent (of the shorter side).</summary>
		public float[] Radius = new float[4];
		public bool[] RadiusPercent = new bool[4];
		public List<BoxShadow> Shadows = new List<BoxShadow>();

		public struct BoxShadow
		{
			public float X, Y, Blur, Spread;
			public uint Colour;
			public bool Inset;
		}

		public bool Sliced => SliceLeft > 0 || SliceTop > 0 || SliceRight > 0 || SliceBottom > 0;
		public bool HasBorder => BorderWidth.Any(w => w > 0);
		public bool HasRadius => Radius.Any(r => r > 0);
		/// <summary>Whether the box is painted (MenuPaint) rather than drawn as a colour and a picture's quads: a gradient, a border, round corners or a shadow.</summary>
		public bool Decorated => Gradient != null || HasBorder || HasRadius || Shadows.Count > 0;
		public bool Empty => ImagePath == null && Gradient == null && !HasBorder && Shadows.Count == 0 && (Colour == null || (Colour.Value & 0xFF) == 0);

		/// <summary>The corners' radii on a frame w by h (menu units), shrunk together where two would overlap, as CSS shrinks them.</summary>
		public float[] Radii(float w, float h)
		{
			float[] r = new float[4];
			float shorter = Math.Min(w, h);
			for (int i = 0; i < 4; i++) r[i] = Math.Max(0, RadiusPercent[i] ? shorter * Radius[i] / 100 : Radius[i]);
			float f = 1;
			if (r[0] + r[1] > w) f = Math.Min(f, w / (r[0] + r[1]));
			if (r[3] + r[2] > w) f = Math.Min(f, w / (r[3] + r[2]));
			if (r[0] + r[3] > h) f = Math.Min(f, h / (r[0] + r[3]));
			if (r[1] + r[2] > h) f = Math.Min(f, h / (r[1] + r[2]));
			if (f < 1) for (int i = 0; i < 4; i++) r[i] *= f;
			return r;
		}

		/// <summary>One rectangle to draw: where in the frame (menu units), and which part of the picture (its pixels).</summary>
		public struct Quad
		{
			public float X, Y, W, H;
			public float U, V, UW, VH;
			public Quad(float x, float y, float w, float h, float u, float v, float uw, float vh) { X = x; Y = y; W = w; H = h; U = u; V = v; UW = uw; VH = vh; }
		}

		/// <summary>A background from its declarations ("background-image: url(...); -ff-slice: 8"), as MenuStyles bakes them; null when it draws nothing.</summary>
		public static MenuBackground Parse(string declarations)
		{
			if (string.IsNullOrWhiteSpace(declarations)) return null;
			MenuBackground b = new MenuBackground();
			foreach (KeyValuePair<string, string> d in MenuStyles.Declarations(declarations))
			{
				string v = d.Value.Trim();
				switch (d.Key)
				{
					case "background-color": b.Colour = ParseColour(v); break;
					case "background-image":
						b.Gradient = MenuGradient.Parse(v);
						(b.ImageKind, b.ImagePath) = b.Gradient != null ? (null, null) : ParseImage(v);
						break;
					case "border": b.BorderSide(0, v); b.BorderSide(1, v); b.BorderSide(2, v); b.BorderSide(3, v); break;
					case "border-top": b.BorderSide(0, v); break;
					case "border-right": b.BorderSide(1, v); break;
					case "border-bottom": b.BorderSide(2, v); break;
					case "border-left": b.BorderSide(3, v); break;
					case "border-width": Four(v, BorderWidthOf, b.BorderWidth); break;
					case "border-top-width": b.BorderWidth[0] = BorderWidthOf(v); break;
					case "border-right-width": b.BorderWidth[1] = BorderWidthOf(v); break;
					case "border-bottom-width": b.BorderWidth[2] = BorderWidthOf(v); break;
					case "border-left-width": b.BorderWidth[3] = BorderWidthOf(v); break;
					case "border-color": Four(v, w => MenuText.Colour(w) ?? 0xFFFFFFFF, b.BorderColour); break;
					case "border-top-color": b.BorderColour[0] = MenuText.Colour(v) ?? b.BorderColour[0]; break;
					case "border-right-color": b.BorderColour[1] = MenuText.Colour(v) ?? b.BorderColour[1]; break;
					case "border-bottom-color": b.BorderColour[2] = MenuText.Colour(v) ?? b.BorderColour[2]; break;
					case "border-left-color": b.BorderColour[3] = MenuText.Colour(v) ?? b.BorderColour[3]; break;
					case "border-style":
						b._borderNone = v.Equals("none", StringComparison.OrdinalIgnoreCase) || v.Equals("hidden", StringComparison.OrdinalIgnoreCase);
						break;
					case "border-radius":
					{
						string corners = v.Split('/')[0];   // elliptical corners: their horizontal radii
						string[] parts = corners.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						if (parts.Length == 0) break;
						string[] four = parts.Length == 1 ? new[] { parts[0], parts[0], parts[0], parts[0] }
							: parts.Length == 2 ? new[] { parts[0], parts[1], parts[0], parts[1] }
							: parts.Length == 3 ? new[] { parts[0], parts[1], parts[2], parts[1] } : parts.Take(4).ToArray();
						for (int i = 0; i < 4; i++) b.Corner(i, four[i]);
						break;
					}
					case "border-top-left-radius": b.Corner(0, v.Split(' ')[0]); break;
					case "border-top-right-radius": b.Corner(1, v.Split(' ')[0]); break;
					case "border-bottom-right-radius": b.Corner(2, v.Split(' ')[0]); break;
					case "border-bottom-left-radius": b.Corner(3, v.Split(' ')[0]); break;
					case "box-shadow":
						b.Shadows.Clear();
						if (v.Equals("none", StringComparison.OrdinalIgnoreCase)) break;
						foreach (string item in MenuAnimation.CommaList(v))
						{
							List<float> numbers = new List<float>();
							BoxShadow shadow = new BoxShadow { Colour = 0x00000080 };
							foreach (string word in MenuAnimation.Words(item))
							{
								if (word.Equals("inset", StringComparison.OrdinalIgnoreCase)) shadow.Inset = true;
								else if (MenuText.Length(word) is float n) numbers.Add(n);
								else if (MenuText.Colour(word) is uint c) shadow.Colour = c;
							}
							if (numbers.Count < 2) continue;
							shadow.X = numbers[0];
							shadow.Y = numbers[1];
							shadow.Blur = numbers.Count > 2 ? Math.Max(0, numbers[2]) : 0;
							shadow.Spread = numbers.Count > 3 ? numbers[3] : 0;
							b.Shadows.Add(shadow);
						}
						break;
					case "-ff-background-rect":
						int[] r = v.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => int.TryParse(p.Replace("px", ""), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : -1).ToArray();
						b.Rect = r.Length == 4 && r.All(n => n >= 0) && r[2] > 0 && r[3] > 0 ? r : null;
						break;
					case "-ff-background-tint": b.Tint = ParseColour(v) ?? 0xFFFFFFFF; break;
					case "-ff-background-scale-mode": b.ScaleMode = v.ToLowerInvariant(); break;
					case "background-size": b.Size = v.ToLowerInvariant(); break;
					case "background-position": b.Position = v.ToLowerInvariant(); break;
					case "background-repeat": b.Repeat = v.ToLowerInvariant(); break;
					case "-ff-sprite": b.Sprite = v.Trim().Trim('"', '\''); if (b.Sprite.Length == 0 || b.Sprite == "none") b.Sprite = null; break;
					case "-ff-slice":
						b.SliceGiven = true;
						float[] s = v.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(Number).ToArray();
						if (s.Length == 1) b.SliceTop = b.SliceRight = b.SliceBottom = b.SliceLeft = s[0];
						else if (s.Length == 2) { b.SliceTop = b.SliceBottom = s[0]; b.SliceRight = b.SliceLeft = s[1]; }
						else if (s.Length == 3) { b.SliceTop = s[0]; b.SliceRight = b.SliceLeft = s[1]; b.SliceBottom = s[2]; }
						else if (s.Length >= 4) { b.SliceTop = s[0]; b.SliceRight = s[1]; b.SliceBottom = s[2]; b.SliceLeft = s[3]; }
						break;
					case "-ff-slice-left": b.SliceLeft = Number(v); b.SliceGiven = true; break;
					case "-ff-slice-top": b.SliceTop = Number(v); b.SliceGiven = true; break;
					case "-ff-slice-right": b.SliceRight = Number(v); b.SliceGiven = true; break;
					case "-ff-slice-bottom": b.SliceBottom = Number(v); b.SliceGiven = true; break;
					case "-ff-slice-scale": b.SliceScale = Math.Max(0.01f, Number(v) is float f && f > 0 ? f : 1); break;
					case "-ff-slice-type": b.Tiled = v.Equals("tiled", StringComparison.OrdinalIgnoreCase); break;
					case "-ff-background-filter": b.Linear = !v.Equals("point", StringComparison.OrdinalIgnoreCase); break;
				}
			}
			if (b._borderNone) b.BorderWidth = new float[4];
			return b.Empty ? null : b;
		}

		private bool _borderNone;

		/// <summary>"2px solid #fff" onto one side (0 top, 1 right, 2 bottom, 3 left): its width and colour; a style of none takes it away.</summary>
		private void BorderSide(int side, string value)
		{
			float width = 3;   // CSS's medium
			bool none = false;
			foreach (string word in MenuAnimation.Words(value))
			{
				string w = word.ToLowerInvariant();
				if (w == "none" || w == "hidden") none = true;
				else if (w == "thin" || w == "medium" || w == "thick" || MenuText.Length(w) != null) width = BorderWidthOf(w);
				else if (MenuText.Colour(w) is uint c) BorderColour[side] = c;
			}
			BorderWidth[side] = none ? 0 : width;
		}

		private static float BorderWidthOf(string word)
		{
			string w = word.Trim().ToLowerInvariant();
			if (w == "thin") return 1;
			if (w == "medium") return 3;
			if (w == "thick") return 5;
			return Math.Max(0, MenuText.Length(w) ?? 0);
		}

		private void Corner(int i, string word)
		{
			string w = word.Trim();
			RadiusPercent[i] = w.EndsWith("%");
			Radius[i] = Math.Max(0, RadiusPercent[i] ? (float.TryParse(w.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out float p) ? p : 0) : MenuText.Length(w) ?? 0);
		}

		/// <summary>One to four values (as margin: top, right, bottom, left) into four.</summary>
		private static void Four<T>(string value, Func<string, T> read, T[] into)
		{
			T[] v = MenuAnimation.Words(value).Select(read).ToArray();
			if (v.Length == 0) return;
			if (v.Length == 1) { into[0] = into[1] = into[2] = into[3] = v[0]; }
			else if (v.Length == 2) { into[0] = into[2] = v[0]; into[1] = into[3] = v[1]; }
			else if (v.Length == 3) { into[0] = v[0]; into[1] = into[3] = v[1]; into[2] = v[2]; }
			else { into[0] = v[0]; into[1] = v[1]; into[2] = v[2]; into[3] = v[3]; }
		}

		/// <summary>A named sprite's part and borders put on, where the frame did not give its own.</summary>
		public void Use(MenuSprites.Sprite sprite)
		{
			if (sprite == null) return;
			if (Rect == null && sprite.W > 0 && sprite.H > 0) Rect = new[] { sprite.X, sprite.Y, sprite.W, sprite.H };
			if (!SliceGiven) { SliceLeft = sprite.Left; SliceTop = sprite.Top; SliceRight = sprite.Right; SliceBottom = sprite.Bottom; }
		}

		/// <summary>The picture laid out over a frame w by h (menu units), for a picture iw by ih pixels.</summary>
		public List<Quad> Layout(float w, float h, int iw, int ih)
		{
			List<Quad> quads = new List<Quad>();
			if (w <= 0 || h <= 0 || iw <= 0 || ih <= 0) return quads;
			float sx = 0, sy = 0, sw = iw, sh = ih;
			if (Rect != null) { sx = Rect[0]; sy = Rect[1]; sw = Math.Min(Rect[2], iw - sx); sh = Math.Min(Rect[3], ih - sy); }
			if (sw <= 0 || sh <= 0) return quads;

			if (Sliced) { NineSlice(quads, w, h, sx, sy, sw, sh); return quads; }

			bool css = Size != null || (Repeat != null && Repeat != "no-repeat") || Position != null;
			string mode = css ? null : (ScaleMode ?? "stretch-to-fill");
			if (Size == "cover") { mode = "scale-and-crop"; css = false; }
			else if (Size == "contain") { mode = "scale-to-fit"; css = false; }

			if (mode == "scale-and-crop")
			{
				float s = Math.Max(w / sw, h / sh);
				float vw = w / s, vh = h / s;
				(float px, float py) = PositionFraction();
				quads.Add(new Quad(0, 0, w, h, sx + (sw - vw) * px, sy + (sh - vh) * py, vw, vh));
				return quads;
			}
			if (mode == "scale-to-fit")
			{
				float s = Math.Min(w / sw, h / sh);
				float dw = sw * s, dh = sh * s;
				(float px, float py) = PositionFraction();
				quads.Add(new Quad((w - dw) * px, (h - dh) * py, dw, dh, sx, sy, sw, sh));
				return quads;
			}
			if (!css)
			{
				quads.Add(new Quad(0, 0, w, h, sx, sy, sw, sh));
				return quads;
			}

			// CSS: a tile of background-size (auto: the picture's own size), placed by background-position, repeated as background-repeat says.
			(float tw, float th) = TileSize(w, h, sw, sh);
			(float ox, float oy) = TilePosition(w, h, tw, th);
			bool rx = Repeat == "repeat" || Repeat == "repeat-x", ry = Repeat == "repeat" || Repeat == "repeat-y";
			float x0 = rx ? ox - (float)Math.Ceiling(ox / tw) * tw : ox;
			float y0 = ry ? oy - (float)Math.Ceiling(oy / th) * th : oy;
			int guard = 0;
			for (float y = y0; y < h && guard < 4096; y += th)
			{
				for (float x = x0; x < w && guard < 4096; x += tw)
				{
					Clip(quads, x, y, tw, th, sx, sy, sw, sh, w, h);
					guard++;
					if (!rx) break;
				}
				if (!ry) break;
			}
			return quads;
		}

		private void NineSlice(List<Quad> quads, float w, float h, float sx, float sy, float sw, float sh)
		{
			float l = Math.Min(SliceLeft, sw), r = Math.Min(SliceRight, sw - l), t = Math.Min(SliceTop, sh), b = Math.Min(SliceBottom, sh - t);
			float L = l * SliceScale, R = r * SliceScale, T = t * SliceScale, B = b * SliceScale;
			// Borders wider than the frame shrink together, as UI Toolkit's do.
			if (L + R > w && L + R > 0) { float k = w / (L + R); L *= k; R *= k; }
			if (T + B > h && T + B > 0) { float k = h / (T + B); T *= k; B *= k; }
			float[] dx = { 0, L, w - R, w }, dy = { 0, T, h - B, h };
			float[] ux = { sx, sx + l, sx + sw - r, sx + sw }, uy = { sy, sy + t, sy + sh - b, sy + sh };
			for (int row = 0; row < 3; row++)
			{
				for (int col = 0; col < 3; col++)
				{
					float x = dx[col], y = dy[row], cw = dx[col + 1] - x, ch = dy[row + 1] - y;
					float u = ux[col], v = uy[row], uw = ux[col + 1] - u, vh = uy[row + 1] - v;
					if (cw <= 0 || ch <= 0 || uw <= 0 || vh <= 0) continue;
					bool corner = col != 1 && row != 1;
					if (!Tiled || corner) { quads.Add(new Quad(x, y, cw, ch, u, v, uw, vh)); continue; }
					// Tiled: the edge repeated along itself at the border's scale, the middle both ways.
					float tw = col == 1 ? uw * SliceScale : cw, th = row == 1 ? vh * SliceScale : ch;
					if (tw <= 0.5f || th <= 0.5f) { quads.Add(new Quad(x, y, cw, ch, u, v, uw, vh)); continue; }
					int guard = 0;
					for (float ty = y; ty < y + ch - 0.01f && guard < 4096; ty += th)
					{
						for (float tx = x; tx < x + cw - 0.01f && guard < 4096; tx += tw, guard++)
						{
							float qw = Math.Min(tw, x + cw - tx), qh = Math.Min(th, y + ch - ty);
							quads.Add(new Quad(tx, ty, qw, qh, u, v, uw * qw / tw, vh * qh / th));
						}
					}
				}
			}
		}

		/// <summary>A tile at (x, y) of tw by th, cut to the frame, its part of the picture cut the same.</summary>
		private static void Clip(List<Quad> quads, float x, float y, float tw, float th, float sx, float sy, float sw, float sh, float w, float h)
		{
			float x1 = Math.Max(0, x), y1 = Math.Max(0, y), x2 = Math.Min(w, x + tw), y2 = Math.Min(h, y + th);
			if (x2 <= x1 || y2 <= y1) return;
			float ku = sw / tw, kv = sh / th;
			quads.Add(new Quad(x1, y1, x2 - x1, y2 - y1, sx + (x1 - x) * ku, sy + (y1 - y) * kv, (x2 - x1) * ku, (y2 - y1) * kv));
		}

		private (float, float) TileSize(float w, float h, float sw, float sh)
		{
			string[] parts = (Size ?? "auto").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			float? a = Length(parts.Length > 0 ? parts[0] : "auto", w), b = Length(parts.Length > 1 ? parts[1] : "auto", h);
			if (a == null && b == null) return (sw, sh);
			if (a == null) return (b.Value * sw / sh, b.Value);
			if (b == null) return (a.Value, a.Value * sh / sw);
			return (Math.Max(0.5f, a.Value), Math.Max(0.5f, b.Value));
		}

		private (float, float) TilePosition(float w, float h, float tw, float th)
		{
			string[] parts = (Position ?? "left top").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			string px = parts.Length > 0 ? parts[0] : "left", py = parts.Length > 1 ? parts[1] : (px == "top" || px == "bottom" ? px : "center");
			if (px == "top" || px == "bottom") { string t = px; px = py == "top" || py == "bottom" ? "center" : py; py = t; }
			return (Place(px, w - tw, "left", "right"), Place(py, h - th, "top", "bottom"));
		}

		/// <summary>Where a picture that does not fill the frame sits in it (scale-to-fit, scale-and-crop): 0 at the start, 0.5 in the middle, 1 at the end.</summary>
		private (float, float) PositionFraction()
		{
			if (Position == null) return (0.5f, 0.5f);
			(float x, float y) = TilePosition(1, 1, 0, 0);
			return (Math.Clamp(x, 0, 1), Math.Clamp(y, 0, 1));
		}

		private static float Place(string word, float room, string start, string end)
		{
			if (word == start) return 0;
			if (word == end) return room;
			if (word == "center" || word == "centre") return room / 2;
			if (word.EndsWith("%") && float.TryParse(word.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out float p)) return room * p / 100;
			return Number(word);
		}

		private static float? Length(string text, float whole)
		{
			if (text == "auto") return null;
			if (text.EndsWith("%") && float.TryParse(text.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out float p)) return whole * p / 100;
			return Number(text);
		}

		private static float Number(string text)
		{
			string t = text.Trim().ToLowerInvariant();
			if (t.EndsWith("px")) t = t.Substring(0, t.Length - 2);
			return float.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : 0;
		}

		/// <summary>url("x") / url(x) / resource("x") as its kind and path; none (or anything else) as nothing.</summary>
		public static (string, string) ParseImage(string value)
		{
			string v = value.Trim();
			foreach (string kind in new[] { "url", "resource" })
			{
				if (!v.StartsWith(kind + "(", StringComparison.OrdinalIgnoreCase) || !v.EndsWith(")")) continue;
				string inner = v.Substring(kind.Length + 1, v.Length - kind.Length - 2).Trim().Trim('"', '\'');
				return inner.Length > 0 ? (kind, inner.Replace('\\', '/')) : (null, null);
			}
			return (null, null);
		}

		/// <summary>#rgb, #rgba, #rrggbb, #rrggbbaa, rgb()/rgba(), transparent - as 0xRRGGBBAA.</summary>
		public static uint? ParseColour(string value)
		{
			string v = value?.Trim().ToLowerInvariant();
			if (string.IsNullOrEmpty(v)) return null;
			if (v == "transparent") return 0;
			if (v.StartsWith("#"))
			{
				string hex = v.Substring(1);
				if (hex.Length == 3 || hex.Length == 4) hex = string.Concat(hex.Select(c => new string(c, 2)));
				if (hex.Length == 6) hex += "ff";
				return hex.Length == 8 && uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint rgba) ? rgba : (uint?)null;
			}
			if (v.StartsWith("rgb"))
			{
				int open = v.IndexOf('('), close = v.LastIndexOf(')');
				if (open < 0 || close < open) return null;
				string[] parts = v.Substring(open + 1, close - open - 1).Split(',');
				if (parts.Length < 3) return null;
				uint C(string p) => (uint)Math.Clamp((int)Math.Round(Number(p)), 0, 255);
				uint a = parts.Length > 3 ? (uint)Math.Clamp((int)Math.Round((parts[3].Trim().EndsWith("%") ? Number(parts[3].Trim().TrimEnd('%')) / 100 : Number(parts[3])) * 255), 0, 255) : 255;
				return C(parts[0]) << 24 | C(parts[1]) << 16 | C(parts[2]) << 8 | a;
			}
			if (v.StartsWith("hsl"))
			{
				int open = v.IndexOf('('), close = v.LastIndexOf(')');
				if (open < 0 || close < open) return null;
				string[] parts = v.Substring(open + 1, close - open - 1).Replace("/", ",").Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 3) return null;
				double hue = ((Number(parts[0].Replace("deg", "")) % 360) + 360) % 360 / 360;
				double sat = Math.Clamp(Number(parts[1].TrimEnd('%')) / 100, 0, 1), light = Math.Clamp(Number(parts[2].TrimEnd('%')) / 100, 0, 1);
				double q = light < 0.5 ? light * (1 + sat) : light + sat - light * sat, pp = 2 * light - q;
				double Hue(double t) { t = (t + 1) % 1; return t < 1.0 / 6 ? pp + (q - pp) * 6 * t : t < 0.5 ? q : t < 2.0 / 3 ? pp + (q - pp) * (2.0 / 3 - t) * 6 : pp; }
				uint Ch(double x) => (uint)Math.Clamp((int)Math.Round(x * 255), 0, 255);
				uint a = parts.Length > 3 ? (uint)Math.Clamp((int)Math.Round((parts[3].EndsWith("%") ? Number(parts[3].TrimEnd('%')) / 100 : Number(parts[3])) * 255), 0, 255) : 255;
				return Ch(Hue(hue + 1.0 / 3)) << 24 | Ch(Hue(hue)) << 16 | Ch(Hue(hue - 1.0 / 3)) << 8 | a;
			}
			return Named.TryGetValue(v, out uint named) ? named : (uint?)null;
		}

		/// <summary>CSS's colour names, the common ones (0xRRGGBBAA).</summary>
		private static readonly Dictionary<string, uint> Named = new Dictionary<string, uint>(StringComparer.Ordinal)
		{
			["black"] = 0x000000FF, ["white"] = 0xFFFFFFFF, ["red"] = 0xFF0000FF, ["lime"] = 0x00FF00FF, ["green"] = 0x008000FF, ["blue"] = 0x0000FFFF,
			["yellow"] = 0xFFFF00FF, ["cyan"] = 0x00FFFFFF, ["aqua"] = 0x00FFFFFF, ["magenta"] = 0xFF00FFFF, ["fuchsia"] = 0xFF00FFFF,
			["gray"] = 0x808080FF, ["grey"] = 0x808080FF, ["silver"] = 0xC0C0C0FF, ["maroon"] = 0x800000FF, ["olive"] = 0x808000FF,
			["navy"] = 0x000080FF, ["purple"] = 0x800080FF, ["teal"] = 0x008080FF, ["orange"] = 0xFFA500FF, ["gold"] = 0xFFD700FF,
			["pink"] = 0xFFC0CBFF, ["brown"] = 0xA52A2AFF, ["crimson"] = 0xDC143CFF, ["coral"] = 0xFF7F50FF, ["salmon"] = 0xFA8072FF,
			["tomato"] = 0xFF6347FF, ["orchid"] = 0xDA70D6FF, ["violet"] = 0xEE82EEFF, ["indigo"] = 0x4B0082FF, ["plum"] = 0xDDA0DDFF,
			["khaki"] = 0xF0E68CFF, ["beige"] = 0xF5F5DCFF, ["ivory"] = 0xFFFFF0FF, ["tan"] = 0xD2B48CFF, ["chocolate"] = 0xD2691EFF,
			["skyblue"] = 0x87CEEBFF, ["steelblue"] = 0x4682B4FF, ["royalblue"] = 0x4169E1FF, ["midnightblue"] = 0x191970FF,
			["darkblue"] = 0x00008BFF, ["darkred"] = 0x8B0000FF, ["darkgreen"] = 0x006400FF, ["darkgray"] = 0xA9A9A9FF, ["darkgrey"] = 0xA9A9A9FF,
			["lightgray"] = 0xD3D3D3FF, ["lightgrey"] = 0xD3D3D3FF, ["lightblue"] = 0xADD8E6FF, ["slategray"] = 0x708090FF,
			["dimgray"] = 0x696969FF, ["whitesmoke"] = 0xF5F5F5FF, ["goldenrod"] = 0xDAA520FF, ["firebrick"] = 0xB22222FF,
		};
	}
}
