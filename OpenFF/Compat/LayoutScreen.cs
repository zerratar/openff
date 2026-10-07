// A screen the client draws itself, laid out and styled as data: a layout of frames (the menus' XML), its Crystal
// Style Sheets and its bindings, drawn through the DrawList. The field's HUD restyles the game's own windows; this
// is for the screens that are the client's from the start - FF4's battle HUD first - so that a mod reshapes them
// the way it reshapes a menu, without code.
//
// Where a screen comes from, by its id (ff4_battle_hud):
//   a mod's  menus/hud/<id>.xml  - the first loaded mod with one; it takes the place of the client's layout;
//   else the client's  Data/hud/<id>.xml.
// Its sheets: the layout's folder's styles/*.css, then every loaded mod's menus/hud/styles/*.css (a restyle with no
// layout of its own), then the layout's <style> elements; a frame's own style attribute over them all.
//
// The layout's units: the <menu>'s style gives the canvas (width, height) - FF4's battle HUD is written in Steam's
// 1080p pixels, 1920 x 1080 - and it is drawn stretched onto the DrawList's 800 x 480.
//
// What a frame draws, from the cascade:
//   display / visibility / opacity     as CSS (opacity multiplies down the tree)
//   scale                              as CSS (one number or two, or percents): the frame and its frames drawn that
//                                      size about its centre - a window opening from its middle
//   background-color                   a fill (#rrggbbaa, rgba(), transparent)
//   background-image                   linear-gradient(to bottom | to right | <angle>, stops) as strips; url("x.png")
//                                      (a picture beside the layout, -ff-background-rect x y w h for a part of it)
//   border, border-*-width/-color      per side
//   box-shadow                         x y [blur [spread]] colour, the outer ones, as a rectangle behind
//   color, font-size, text-align,      its text (bind-text, or <data>): px in the layout's units; text-align left,
//   vertical-align, text-shadow        center or right; vertical-align top, middle (the line's box) or central (the capitals' middle, as FF4.exe sets its text); the shadows drawn under it
//   -ff-cell: <name> <index>           a sprite cell of the game's art, by a name the client registers (Cells): FF4's
//                                      cursor, gauge, number; "glove" for FF4's pointing glove
//   -ff-cell-origin: x y               where the cell's own origin goes in the frame (px or %; default 0 50%)
//   -ff-cell-crop: <percent>           the cell drawn that much of its width (a gauge's fill), bindable
//   -ff-cell-scale: <number>           the cell's size against the game's (1)
//   -ff-cell-shadow: x y colour        the cell drawn under itself in the colour, moved (a drop shadow)
//   -ff-panel: <name>                  a panel of the game's art under the frame, by a name the client registers: FF4's
//                                      window, "ff4-window"
// Bindings as the menus have them: bind-text, bind-visible, bind-display, bind-class, bind-style, data-source -
// worked out every frame against the roots the code gives (MenuBindings).

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OpenFF.Content;

namespace OpenFF.Client
{
	internal sealed class LayoutScreen
	{
		/// <summary>Draws a named cell of the game's art with its origin at (x, y): the index, the size against the game's, the part of its width shown (0..1), the tint.</summary>
		public delegate bool CellDrawer(DrawList d, int index, float x, float y, float scale, float crop, Color tint);

		/// <summary>The cells a layout's -ff-cell can name, as the client registers them (FF4's: Ff4Ui).</summary>
		public static readonly Dictionary<string, CellDrawer> Cells = new Dictionary<string, CellDrawer>(StringComparer.OrdinalIgnoreCase);

		/// <summary>Draws a named panel of the game's art over a frame's box (x, y, width, height) at an opacity.</summary>
		public delegate bool PanelDrawer(DrawList d, float x, float y, float w, float h, float opacity);

		/// <summary>The panels a layout's -ff-panel can name beyond the menus' own words, as the client registers them (FF4's window: "ff4-window").</summary>
		public static readonly Dictionary<string, PanelDrawer> Panels = new Dictionary<string, PanelDrawer>(StringComparer.OrdinalIgnoreCase);

		public string Id { get; }

		/// <summary>The whole screen's opacity, over its frames' own (a screen that fades with the game's, Ff4Dialogue).</summary>
		public float Opacity = 1f;
		public string Source { get; }

		private readonly XElement _menu;
		private readonly MenuStyles.Sheet _sheet;
		private readonly string _directory;
		private readonly float _width, _height;
		private readonly Dictionary<XElement, string> _ownStyle = new Dictionary<XElement, string>();
		private readonly Dictionary<XElement, string> _texts = new Dictionary<XElement, string>();
		private readonly MenuAnimation.Animator _animator = new MenuAnimation.Animator();
		private readonly Stopwatch _clock = Stopwatch.StartNew();
		private XElement _laid;     // the layout as last laid out, and what it was laid out from
		private string _laidKey;
		private readonly Dictionary<string, Texture> _pictures = new Dictionary<string, Texture>(StringComparer.OrdinalIgnoreCase);

		private LayoutScreen(string id, string source, XElement menu, MenuStyles.Sheet sheet)
		{
			Id = id;
			Source = source;
			_menu = menu;
			_sheet = sheet;
			_directory = Path.GetDirectoryName(source);
			MenuLayout.Style style = MenuLayout.Style.Of(menu);
			_width = (float)(style.Length("width", 800, 0) ?? 800);
			_height = (float)(style.Length("height", 480, 0) ?? 480);
			foreach (XElement frame in menu.Descendants("frame"))
			{
				string own = (string)frame.Attribute("style");
				if (!string.IsNullOrWhiteSpace(own)) _ownStyle[frame] = own;
			}
		}

		/// <summary>A screen by its id: a mod's layout, else the client's; null (logged) when there is none or it does not read.</summary>
		public static LayoutScreen Load(string id)
		{
			string path = null;
			foreach (string folder in ModMenus.ModFolders)
			{
				string candidate = Path.Combine(folder, "hud", id + ".xml");
				if (File.Exists(candidate)) { path = candidate; break; }
			}
			path ??= Path.Combine(AppContext.BaseDirectory, "Data", "hud", id + ".xml");
			if (!File.Exists(path)) { Log.Write(LogChannel.General, "hud: no layout for " + id); return null; }
			try
			{
				XDocument doc = XDocument.Load(path);
				XElement menu = doc.Root.Name.LocalName == "menu" ? doc.Root : doc.Root.Element("menu");
				if (menu == null) { Log.Write(LogChannel.General, "hud: " + path + " has no <menu>"); return null; }
				List<string> sheets = MenuStyles.SheetsBeside(path);
				foreach (string folder in ModMenus.ModFolders)
				{
					string styles = Path.Combine(folder, "hud", "styles");
					if (!Directory.Exists(styles) || string.Equals(Path.GetFullPath(styles), Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), "styles")), StringComparison.OrdinalIgnoreCase)) continue;
					foreach (string css in Directory.GetFiles(styles, "*.css").OrderBy(f => f, StringComparer.OrdinalIgnoreCase)) sheets.Add(File.ReadAllText(css));
				}
				foreach (XElement style in menu.Descendants("style").ToList()) { sheets.Add(style.Value); style.Remove(); }
				Log.Write(LogChannel.File, "hud: " + id + " from " + path + " (" + sheets.Count + " sheet(s))");
				return new LayoutScreen(id, path, menu, MenuStyles.Compile(sheets));
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "hud: " + path + ": " + ex.Message);
				return null;
			}
		}

		// ---- a frame of it ----

		/// <summary>The screen drawn now, its bindings worked out against <paramref name="root"/> (a name to its value).</summary>
		public void Draw(DrawList d, Func<string, (bool, object)> root)
		{
			try
			{
				Bind(root);
				List<(XElement Frame, Dictionary<string, string> Computed)> computed = MenuStyles.Computed(_menu, _sheet);
				Dictionary<XElement, Dictionary<string, string>> shown = _animator.Step(computed, _sheet, _clock.Elapsed.TotalSeconds);
				Dictionary<XElement, MenuStyles.Look> looks = MenuStyles.Looks(_menu, computed, shown);
				Dictionary<XElement, Dictionary<string, string>> values = new Dictionary<XElement, Dictionary<string, string>>();
				foreach ((XElement frame, Dictionary<string, string> c) in computed) values[frame] = shown != null && shown.TryGetValue(frame, out Dictionary<string, string> s) ? s : c;
				// Laid out again only when a frame's classes, styles, state or text changed (the layout is the same otherwise).
				string key = string.Join("|", _menu.Descendants("frame").Select(f => string.Join("~", f.Attributes().Select(a => a.Value)) + "~" + (string)f.Element("data")));
				if (key != _laidKey || _laid == null)
				{
					XElement laid = new XElement(_menu);
					MenuStyles.Apply(laid, _sheet);
					MenuLayout.Bake(laid);
					_laid = laid;
					_laidKey = key;
				}
				float sx = DrawList.ScreenWidth / _width, sy = DrawList.ScreenHeight / _height;
				// A point of the layout (x, y) is drawn at (ax + x kx, ay + y ky); a frame's scale (CSS scale: about its centre)
				// changes that for it and its frames - their boxes, texts, borders and slices with them.
				void Walk(XElement frame, XElement copy, float px, float py, float ax, float ay, float kx, float ky)
				{
					float x = px + Number(copy, "x"), y = py + Number(copy, "y");
					float w = Number(copy, "width"), h = Number(copy, "height");
					if (looks.TryGetValue(frame, out MenuStyles.Look look) && look.Hidden) return;
					Dictionary<string, string> v = values.TryGetValue(frame, out Dictionary<string, string> found) ? found : new Dictionary<string, string>();
					if (v.TryGetValue("scale", out string scale) && Scale(scale, out float scx, out float scy))
					{
						ax += (x + w / 2) * kx * (1 - scx);
						ay += (y + h / 2) * ky * (1 - scy);
						kx *= scx;
						ky *= scy;
					}
					bool visible = !(v.TryGetValue("visibility", out string vis) && vis.Trim().Equals("hidden", StringComparison.OrdinalIgnoreCase));
					if (visible && kx > 0 && ky > 0) DrawFrame(d, frame, look, v, ax + x * kx, ay + y * ky, w * kx, h * ky, kx, ky);
					List<XElement> frames = frame.Elements("frame").ToList(), copies = copy.Elements("frame").ToList();
					for (int i = 0; i < frames.Count && i < copies.Count; i++) Walk(frames[i], copies[i], x, y, ax, ay, kx, ky);
				}
				List<XElement> top = _menu.Elements("frame").ToList(), topCopies = _laid.Elements("frame").ToList();
				for (int i = 0; i < top.Count && i < topCopies.Count; i++) Walk(top[i], topCopies[i], 0, 0, 0, 0, sx, sy);
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "hud-draw-" + Id, 3, () => "hud: " + Id + ": " + ex);
			}
		}

		/// <summary>The frames' bindings: their texts, shown or not, classes and bound styles - on the frames themselves, so a transition follows a change.</summary>
		private void Bind(Func<string, (bool, object)> root)
		{
			MenuBindingScope top = new MenuBindingScope { Root = root };
			Dictionary<XElement, MenuBindingScope> scopes = new Dictionary<XElement, MenuBindingScope>();
			foreach (XElement frame in _menu.Descendants("frame"))
			{
				MenuBindingScope scope = ScopeOf(frame, top, scopes);
				foreach (KeyValuePair<string, string> c in MenuStyles.Declarations((string)frame.Attribute("bind-class")))
					SetClass(frame, c.Key, MenuBindings.Test(c.Value, scope));
				string own = _ownStyle.TryGetValue(frame, out string o) ? o : null;
				string bound = frame.Attribute("bind-style") is XAttribute bs ? MenuBindings.Format(bs.Value, scope) : null;
				string visible = frame.Attribute("bind-visible") is XAttribute bv ? (MenuBindings.Test(bv.Value, scope) ? null : "visibility: hidden") : null;
				string display = frame.Attribute("bind-display") is XAttribute bd ? (MenuBindings.Test(bd.Value, scope) ? null : "display: none") : null;
				string style = string.Join("; ", new[] { own, bound, visible, display }.Where(x => !string.IsNullOrWhiteSpace(x)));
				if (style != ((string)frame.Attribute("style") ?? "")) frame.SetAttributeValue("style", style.Length == 0 ? null : style);
				if (frame.Attribute("bind-text") is XAttribute bt) _texts[frame] = MenuBindings.Format(bt.Value, scope);
				else if (frame.Element("data") is XElement data && !string.IsNullOrWhiteSpace(data.Value)) _texts[frame] = data.Value.Trim();
			}
		}

		private static MenuBindingScope ScopeOf(XElement frame, MenuBindingScope root, Dictionary<XElement, MenuBindingScope> scopes)
		{
			if (frame == null || frame.Name.LocalName != "frame") return root;
			if (scopes.TryGetValue(frame, out MenuBindingScope known)) return known;
			MenuBindingScope parent = ScopeOf(frame.Parent, root, scopes);
			string source = (string)frame.Attribute("data-source");
			MenuBindingScope scope = string.IsNullOrWhiteSpace(source) ? parent : parent.With(MenuBindings.Resolve(source, parent));
			scopes[frame] = scope;
			return scope;
		}

		private static void SetClass(XElement frame, string name, bool on)
		{
			HashSet<string> classes = new HashSet<string>(((string)frame.Attribute("class") ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);
			if (!(on ? classes.Add(name) : classes.Remove(name))) return;
			frame.SetAttributeValue("class", classes.Count == 0 ? null : string.Join(" ", classes));
		}

		// ---- drawing a frame ----

		private void DrawFrame(DrawList d, XElement frame, MenuStyles.Look look, Dictionary<string, string> v, float x, float y, float w, float h, float sx, float sy)
		{
			double opacity = (look?.Opacity ?? 1) * Opacity;
			if (opacity <= 0.001) return;
			Color Fade(uint rgba) => new Color((byte)(rgba >> 24), (byte)(rgba >> 16), (byte)(rgba >> 8), (byte)Math.Round((rgba & 0xFF) * opacity));
			if (v.TryGetValue("-ff-panel", out string panel) && Panels.TryGetValue(panel.Trim(), out PanelDrawer drawPanel)) drawPanel(d, x, y, w, h, (float)opacity);
			MenuBackground bg = look?.Background != null ? MenuBackground.Parse(look.Background) : null;
			if (bg != null)
			{
				foreach (MenuBackground.BoxShadow shadow in bg.Shadows)
				{
					if (shadow.Inset) continue;
					float sp = shadow.Spread;
					d.Rect(x + (shadow.X - sp) * sx, y + (shadow.Y - sp) * sy, w + 2 * sp * sx, h + 2 * sp * sy, Fade(shadow.Colour));
				}
				if (bg.Colour.HasValue && (bg.Colour.Value & 0xFF) != 0) d.Rect(x, y, w, h, Fade(bg.Colour.Value));
				if (bg.Gradient != null) Gradient(d, bg.Gradient, x, y, w, h, sx, sy, Fade);
				if (bg.ImagePath != null) Picture(d, bg, x, y, w, h, sx, sy, opacity);
				if (bg.HasBorder)
				{
					float t = bg.BorderWidth[0] * sy, r = bg.BorderWidth[1] * sx, b = bg.BorderWidth[2] * sy, l = bg.BorderWidth[3] * sx;
					if (t > 0) d.Rect(x, y, w, t, Fade(bg.BorderColour[0]));
					if (r > 0) d.Rect(x + w - r, y, r, h, Fade(bg.BorderColour[1]));
					if (b > 0) d.Rect(x, y + h - b, w, b, Fade(bg.BorderColour[2]));
					if (l > 0) d.Rect(x, y, l, h, Fade(bg.BorderColour[3]));
				}
			}
			if (v.TryGetValue("-ff-cell", out string cell) && !string.IsNullOrWhiteSpace(cell)) DrawCell(d, cell, v, x, y, w, h, sx, sy, opacity);
			if (_texts.TryGetValue(frame, out string text) && !string.IsNullOrEmpty(text)) DrawText(d, text, look, v, x, y, w, h, sx, sy, opacity);
		}

		private static void Gradient(DrawList d, MenuGradient g, float x, float y, float w, float h, float sx, float sy, Func<uint, Color> fade)
		{
			if (g.Kind != "linear" || g.Stops.Count == 0) return;
			// To the bottom (180) or the right (90); anything else drawn as the nearer of the two.
			float angle = g.CornerX != 0 || g.CornerY != 0 ? (g.CornerY > 0 ? 180 : g.CornerY < 0 ? 0 : g.CornerX > 0 ? 90 : 270) : g.Angle;
			angle = ((angle % 360) + 360) % 360;
			bool across = (angle > 45 && angle < 135) || (angle > 225 && angle < 315);
			bool reverse = across ? angle > 180 : angle < 90 || angle > 270;
			float length = across ? w : h;
			float[] at = StopsAt(g, across ? w / sx : h / sy);
			int n = at.Length;
			// Between two stops of one colour (a hard stop's band, a menu's flat rows) one rectangle, edge to edge, so
			// translucent bands meet without a seam; a blend in strips, 24 to the whole length.
			void Band(float t0, float t1, uint colour)
			{
				if (t1 <= t0) return;
				float a = length * (reverse ? 1 - t1 : t0), b = length * (reverse ? 1 - t0 : t1);
				if (across) d.Rect(x + a, y, b - a, h, fade(colour));
				else d.Rect(x, y + a, w, b - a, fade(colour));
			}
			Band(0, Math.Clamp(at[0], 0, 1), g.Stops[0].Colour);
			for (int i = 1; i < n; i++)
			{
				float t0 = Math.Clamp(at[i - 1], 0, 1), t1 = Math.Clamp(at[i], 0, 1);
				if (t1 <= t0) continue;
				uint c0 = g.Stops[i - 1].Colour, c1 = g.Stops[i].Colour;
				if (c0 == c1) { Band(t0, t1, c0); continue; }
				int strips = Math.Max(1, (int)Math.Ceiling(24 * (t1 - t0)));
				for (int k = 0; k < strips; k++) Band(t0 + (t1 - t0) * k / strips, t0 + (t1 - t0) * (k + 1) / strips, Mix(c0, c1, (k + 0.5f) / strips));
			}
			Band(Math.Clamp(at[n - 1], 0, 1), 1, g.Stops[n - 1].Colour);
		}

		/// <summary>Where each stop falls along the gradient (0..1), <paramref name="length"/> the line's length in layout units
		/// (for px stops); a stop without a position spread between its neighbours, one before an earlier stop's at that.</summary>
		private static float[] StopsAt(MenuGradient g, float length)
		{
			int n = g.Stops.Count;
			float[] at = new float[n];
			for (int i = 0; i < n; i++)
			{
				string p = g.Stops[i].Position?.Trim();
				if (p != null && p.EndsWith("%", StringComparison.Ordinal) && float.TryParse(p.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out float pc)) at[i] = pc / 100f;
				else if (p != null && float.TryParse(p.Replace("px", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out float px) && length > 0) at[i] = px / length;
				else at[i] = n == 1 ? 0 : i / (float)(n - 1);
				if (i > 0 && at[i] < at[i - 1]) at[i] = at[i - 1];
			}
			return at;
		}

		private static uint Mix(uint a, uint b, float k)
		{
			uint C(int shift) => (uint)Math.Round(((a >> shift) & 0xFF) * (1 - k) + ((b >> shift) & 0xFF) * k) & 0xFF;
			return (C(24) << 24) | (C(16) << 16) | (C(8) << 8) | C(0);
		}

		private void Picture(DrawList d, MenuBackground bg, float x, float y, float w, float h, float sx, float sy, double opacity)
		{
			// url: a file beside the layout; resource: one of the game's ("files/MENU_Common.dat/frame_00.NCGR").
			bool resource = bg.ImageKind == "resource";
			string path = resource ? bg.ImagePath : Path.Combine(_directory, bg.ImagePath);
			if (!_pictures.TryGetValue(path, out Texture texture))
			{
				try
				{
					byte[] data = resource ? GameArchive.Read(path) : File.Exists(path) ? File.ReadAllBytes(path) : null;
					texture = data != null ? d.LoadTexture("hud:" + path, data) : null;
				}
				catch (Exception) { texture = null; }
				_pictures[path] = texture;
			}
			if (texture == null) return;
			uint tint = bg.Tint;
			Color c = new Color((byte)(tint >> 24), (byte)(tint >> 16), (byte)(tint >> 8), (byte)Math.Round((tint & 0xFF) * opacity));
			if (bg.Sliced)
			{
				// 9-sliced: the corners kept at -ff-slice-scale layout units a picture pixel, the edges and the middle stretched.
				foreach (MenuBackground.Quad q in bg.Layout(w / sx, h / sy, texture.Width, texture.Height))
					d.Sprite(texture, x + q.X * sx, y + q.Y * sy, q.W * sx, q.H * sy, c, 0f, q.U, q.V, q.UW, q.VH);
				return;
			}
			int[] r = bg.Rect;
			if (r != null && r.Length == 4) d.Sprite(texture, x, y, w, h, c, 0f, r[0], r[1], r[2], r[3]);
			else d.Sprite(texture, x, y, w, h, c);
		}

		private static void DrawCell(DrawList d, string declaration, Dictionary<string, string> v, float x, float y, float w, float h, float sx, float sy, double opacity)
		{
			string[] parts = declaration.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0 || !Cells.TryGetValue(parts[0], out CellDrawer drawer)) return;
			int index = parts.Length > 1 && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int i) ? i : 0;
			(float ox, float oy) = (0f, h / 2);
			if (v.TryGetValue("-ff-cell-origin", out string origin))
			{
				(float tx, float ty) = MenuStyles.TranslateOf(origin, w / sx, h / sy);
				ox = tx * sx;
				oy = ty * sy;
			}
			float scale = v.TryGetValue("-ff-cell-scale", out string sc) && float.TryParse(sc.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float s) ? s : 1f;
			float crop = 1f;
			if (v.TryGetValue("-ff-cell-crop", out string cr))
			{
				string c = cr.Trim().TrimEnd('%');
				if (float.TryParse(c, NumberStyles.Float, CultureInfo.InvariantCulture, out float pc)) crop = Math.Clamp(cr.Trim().EndsWith("%", StringComparison.Ordinal) ? pc / 100f : pc, 0f, 1f);
			}
			// color: the cell tinted (a greyed list entry's icon), white otherwise.
			uint tint = (v.TryGetValue("color", out string cs) ? MenuBackground.ParseColour(MenuStyles.Hex(cs) ?? cs) : null) ?? 0xFFFFFFFF;
			// -ff-cell-shadow: x y colour - the cell once more under it, in the colour, moved (a glyph's drop shadow, as Steam's
			// orbs before the spells' names have the lettering's).
			if (v.TryGetValue("-ff-cell-shadow", out string shadow))
			{
				string[] bits = shadow.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (bits.Length >= 3 && float.TryParse(bits[0].Replace("px", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out float dx)
					&& float.TryParse(bits[1].Replace("px", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out float dy)
					&& MenuBackground.ParseColour(MenuStyles.Hex(bits[2]) ?? bits[2]) is uint shade)
					drawer(d, index, x + ox + dx * sx, y + oy + dy * sy, scale, crop, new Color((byte)(shade >> 24), (byte)(shade >> 16), (byte)(shade >> 8), (byte)Math.Round((shade & 0xFF) * opacity)));
			}
			drawer(d, index, x + ox, y + oy, scale, crop, new Color((byte)(tint >> 24), (byte)(tint >> 16), (byte)(tint >> 8), (byte)Math.Round((tint & 0xFF) * opacity)));
		}

		private static void DrawText(DrawList d, string text, MenuStyles.Look look, Dictionary<string, string> v, float x, float y, float w, float h, float sx, float sy, double opacity)
		{
			float size = 25f;
			if (v.TryGetValue("font-size", out string fs) && float.TryParse(fs.Trim().Replace("px", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out float f)) size = f;
			// The font's whole size at or above the one asked for, scaled down to it - and across by the layout's own aspect, so a
			// glyph is as wide in the layout's units as it is high (the 800 x 480 space is stretched over a 16:9 screen).
			float wanted = Math.Max(4f, size * sy);
			int drawn = (int)Math.Ceiling(wanted - 0.001f);
			float ky = wanted / drawn, kx = ky * sx / sy;
			uint colour = (v.TryGetValue("color", out string cs) ? MenuBackground.ParseColour(MenuStyles.Hex(cs) ?? cs) : null) ?? 0xFFFFFFFF;
			// Lines (a message's): each placed as a text of its own, line-height apart (px in the layout's units; 1.25 em
			// when not given), the first where a single line would be.
			if (text.IndexOf('\n') >= 0)
			{
				float step = v.TryGetValue("line-height", out string lh) && float.TryParse(lh.Trim().Replace("px", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out float l) ? l : size * 1.25f;
				string[] lines = text.Split('\n');
				for (int i = 0; i < lines.Length; i++)
					if (lines[i].Length > 0) DrawText(d, lines[i], look, v, x, y + i * step * sy, w, h, sx, sy, opacity);
				return;
			}
			string align = v.TryGetValue("text-align", out string ta) ? ta.Trim().ToLowerInvariant() : "left";
			string valign = v.TryGetValue("vertical-align", out string va) ? va.Trim().ToLowerInvariant() : "top";
			bool middle = valign == "middle", central = valign == "central";
			float width = d.MeasureText(text, drawn) * kx;
			float tx = align == "center" ? x + (w - width) / 2 : align == "right" ? x + w - width : x;
			// middle: the line's box centred; central: the capitals' middle on the frame's (FF4.exe's text) - an Arial capital's
			// middle 0.547 em under the line's top, the em the size's 2 / 1.117, less the 0.1 the text path lifts it.
			float ty = middle ? y + (h - wanted * 1.3f) / 2 : central ? y + h / 2 - wanted * 0.879f : y;
			if (look?.TextStyle != null)
			{
				MenuText lettering = MenuText.Parse(look.TextStyle);
				if (lettering.Shadows != null)
				{
					for (int i = lettering.Shadows.Count - 1; i >= 0; i--)
					{
						MenuText.Shadow s = lettering.Shadows[i];
						uint c = s.Colour;
						d.Text(text, tx + s.X * sx, ty + s.Y * sy, new Color((byte)(c >> 24), (byte)(c >> 16), (byte)(c >> 8), (byte)Math.Round((c & 0xFF) * opacity)), drawn, kx, ky);
					}
				}
			}
			d.Text(text, tx, ty, new Color((byte)(colour >> 24), (byte)(colour >> 16), (byte)(colour >> 8), (byte)Math.Round((colour & 0xFF) * opacity)), drawn, kx, ky);
		}

		/// <summary>CSS scale: one number (or percent) for both directions, or two; none is 1.</summary>
		private static bool Scale(string value, out float x, out float y)
		{
			x = y = 1f;
			string[] parts = value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0 || parts[0] == "none") return false;
			static bool One(string t, out float f)
			{
				bool percent = t.EndsWith("%", StringComparison.Ordinal);
				bool ok = float.TryParse(percent ? t.TrimEnd('%') : t, NumberStyles.Float, CultureInfo.InvariantCulture, out f);
				if (percent) f /= 100f;
				return ok;
			}
			if (!One(parts[0], out x)) return false;
			y = parts.Length > 1 && One(parts[1], out float second) ? second : x;
			return true;
		}

		private static float Number(XElement frame, string tag) => float.TryParse(((string)frame.Element(tag))?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : 0;
	}
}
