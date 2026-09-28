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
			"-ff-background-filter"
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

		public bool Sliced => SliceLeft > 0 || SliceTop > 0 || SliceRight > 0 || SliceBottom > 0;
		public bool Empty => ImagePath == null && (Colour == null || (Colour.Value & 0xFF) == 0);

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
					case "background-image": (b.ImageKind, b.ImagePath) = ParseImage(v); break;
					case "-ff-background-rect":
						int[] r = v.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => int.TryParse(p.Replace("px", ""), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : -1).ToArray();
						b.Rect = r.Length == 4 && r.All(n => n >= 0) && r[2] > 0 && r[3] > 0 ? r : null;
						break;
					case "-ff-background-tint": b.Tint = ParseColour(v) ?? 0xFFFFFFFF; break;
					case "-ff-background-scale-mode": b.ScaleMode = v.ToLowerInvariant(); break;
					case "background-size": b.Size = v.ToLowerInvariant(); break;
					case "background-position": b.Position = v.ToLowerInvariant(); break;
					case "background-repeat": b.Repeat = v.ToLowerInvariant(); break;
					case "-ff-slice":
						float[] s = v.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(Number).ToArray();
						if (s.Length == 1) b.SliceTop = b.SliceRight = b.SliceBottom = b.SliceLeft = s[0];
						else if (s.Length == 2) { b.SliceTop = b.SliceBottom = s[0]; b.SliceRight = b.SliceLeft = s[1]; }
						else if (s.Length == 3) { b.SliceTop = s[0]; b.SliceRight = b.SliceLeft = s[1]; b.SliceBottom = s[2]; }
						else if (s.Length >= 4) { b.SliceTop = s[0]; b.SliceRight = s[1]; b.SliceBottom = s[2]; b.SliceLeft = s[3]; }
						break;
					case "-ff-slice-left": b.SliceLeft = Number(v); break;
					case "-ff-slice-top": b.SliceTop = Number(v); break;
					case "-ff-slice-right": b.SliceRight = Number(v); break;
					case "-ff-slice-bottom": b.SliceBottom = Number(v); break;
					case "-ff-slice-scale": b.SliceScale = Math.Max(0.01f, Number(v) is float f && f > 0 ? f : 1); break;
					case "-ff-slice-type": b.Tiled = v.Equals("tiled", StringComparison.OrdinalIgnoreCase); break;
					case "-ff-background-filter": b.Linear = !v.Equals("point", StringComparison.OrdinalIgnoreCase); break;
				}
			}
			return b.Empty ? null : b;
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
			return null;
		}
	}
}
