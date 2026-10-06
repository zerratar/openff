// The menus' layout extension: a frame placed by rules instead of numbers, the way Unity's UI
// Toolkit places a VisualElement - anchored to its parent's edges, sized in units or percent,
// or laid out in a row or a column by its parent. OpenFF reads it; the game never sees it:
// MenuXbn.FromXml bakes every frame's rules into the plain x, y, width and height the game's
// menu.MenuManager reads, and strips the rules, so a Steam or GOG build gets ordinary frames.
//
// The rules are the layout part of Crystal Style Sheets (MenuStyles), in a frame's style attribute:
//
//   <frame style="left: 0; right: 0; top: 64px; height: 29px">                    anchored
//   <frame style="left: 50%; translate: -50% 0; width: 200px">                    centred
//   <frame style="flex-direction: column; gap: 2px; padding: 6px; height: auto">  a column
//     <frame style="height: 21px"> ... </frame>                                   its rows
//
// A number is menu units (px optional), a percent is of the parent's size (translate's: of the
// frame's own). A frame's own <x>, <y>, <width>, <height> stay what anything the style does not
// say falls back on, so a style may take over only part of the rect and a frame without one is
// exactly what it was. A top-level frame's parent is the screen: 480 by 288 (the game's menu
// area above the bottom bar), or what the <menu>'s own style says its width and height are.
//
// Absolute (the default, and a frame in a column or row that says position: absolute):
//   left and right both, no width: stretched between them; left or right with a width: held to
//   that edge; neither: <x>. top, bottom, height the same way down. margin-* moves it off the
//   edge it is held to; translate moves it by that much after.
//
// In a parent with flex-direction (column or row): the frames follow each other along it, from
// the parent's padding in, gap apart (plus their margins). flex-grow shares out the space left;
// justify-content (flex-start, center, flex-end, space-between) places the run when there is
// space left over. Across, align-items (or the frame's align-self) - stretch (the default: the
// frame takes the parent's whole width inside the padding unless its style gives one),
// flex-start, center, flex-end. width or height: auto on a column or row is what its frames
// take plus its padding.
//
// Crystal's menu editor draws the same layout from a port of this (menu-layout.js) - the two
// are kept to the same rules; this one is what the game gets.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace OpenFF.Content
{
	internal static class MenuLayout
	{
		/// <summary>The menu area a top-level frame sits in: 480 across, 288 down above the bottom bar.</summary>
		public const int ScreenWidth = 480;
		public const int ScreenHeight = 288;

		/// <summary>The style properties this reads (the rest of a style - phase three's panels, opacity - is left for the client).</summary>
		public static readonly string[] Properties =
		{
			"position", "left", "top", "right", "bottom", "width", "height",
			"min-width", "min-height", "max-width", "max-height", "translate",
			"margin", "margin-left", "margin-top", "margin-right", "margin-bottom",
			"padding", "padding-left", "padding-top", "padding-right", "padding-bottom",
			"flex-direction", "gap", "justify-content", "align-items", "align-self", "flex-grow"
		};
		// display: none also leaves a frame out of its column or row (MenuStyles keeps it in the style for this).

		/// <summary>Whether anything in the tree carries layout rules (a file with none goes through untouched).</summary>
		public static bool Has(XElement root)
		{
			return root != null && root.DescendantsAndSelf().Any(e => e.Attribute("style") != null);
		}

		/// <summary>
		/// Writes every styled frame's rect into its x, y, width and height, relative to its parent as
		/// the game reads them. Frames with no rules, in parents with no flow, are left as they are.
		/// </summary>
		public static void Bake(XElement root)
		{
			if (root == null) return;
			IEnumerable<XElement> screens = IsScreen(root) ? new[] { root } : root.Elements().Where(IsScreen);
			foreach (XElement screen in screens)
			{
				Style style = Style.Of(screen);
				double width = style.Length("width", ScreenWidth, 0) ?? ScreenWidth;
				double height = style.Length("height", ScreenHeight, 0) ?? ScreenHeight;
				LayOut(screen, style, width, height);
			}
		}

		/// <summary>Takes the rules out once baked: the style attributes (and the class names phase three reads them by).</summary>
		public static void Strip(XElement root)
		{
			if (root == null) return;
			foreach (XElement element in root.DescendantsAndSelf())
			{
				element.Attribute("style")?.Remove();
				element.Attribute("class")?.Remove();
			}
		}

		private static bool IsScreen(XElement e) => e.Name.LocalName == "menu" || e.Name.LocalName == "unit";

		// ------------------------------------------------------------------ laying out

		/// <summary>The rects of a parent's frames, given its size, and then each frame's own.</summary>
		private static void LayOut(XElement parent, Style parentStyle, double width, double height)
		{
			List<XElement> frames = parent.Elements("frame").ToList();
			if (frames.Count == 0) return;
			string direction = parentStyle.Word("flex-direction");
			bool flows = direction == "row" || direction == "column";
			Dictionary<XElement, Rect> rects = new Dictionary<XElement, Rect>();

			List<XElement> flowing = flows ? frames.Where(f => Style.Of(f).Word("position") != "absolute" && Style.Of(f).Word("display") != "none").ToList() : new List<XElement>();
			if (flowing.Count > 0) Flow(flowing, parentStyle, direction == "row", width, height, rects);
			foreach (XElement frame in frames)
			{
				if (rects.ContainsKey(frame)) continue;
				Style style = Style.Of(frame);
				if (style.IsEmpty) continue;       // no rules: its numbers are what they were
				rects[frame] = Absolute(frame, style, width, height);
			}

			foreach (XElement frame in frames)
			{
				Style style = Style.Of(frame);
				double w, h;
				if (rects.TryGetValue(frame, out Rect rect))
				{
					frame.SetElementValue("x", Round(rect.X));
					frame.SetElementValue("y", Round(rect.Y));
					frame.SetElementValue("width", Math.Max(0, Round(rect.W)));
					frame.SetElementValue("height", Math.Max(0, Round(rect.H)));
					w = rect.W;
					h = rect.H;
				}
				else
				{
					w = Number(frame, "width");
					h = Number(frame, "height");
				}
				LayOut(frame, style, w, h);
			}
		}

		private static Rect Absolute(XElement frame, Style style, double parentW, double parentH)
		{
			(double x, double w) = Axis(style, "left", "right", "width", "min-width", "max-width", "margin-left", "margin-right", Number(frame, "x"), Size(frame, style, true, parentW, parentH), parentW);
			(double y, double h) = Axis(style, "top", "bottom", "height", "min-height", "max-height", "margin-top", "margin-bottom", Number(frame, "y"), Size(frame, style, false, parentW, parentH), parentH);
			(double tx, double ty) = style.Translate(w, h);
			return new Rect(x + tx, y + ty, w, h);
		}

		/// <summary>One direction of an absolute frame: where it starts and how long it is.</summary>
		private static (double at, double length) Axis(Style style, string start, string end, string size, string min, string max,
			string marginStart, string marginEnd, double fallbackAt, double fallbackLength, double parent)
		{
			double? from = style.Length(start, parent, 0);
			double? to = style.Length(end, parent, 0);
			double? length = style.Has(size) && style.Word(size) != "auto" ? style.Length(size, parent, 0) : null;
			double ms = style.Margin(marginStart, parent), me = style.Margin(marginEnd, parent);
			double l;
			double at;
			if (from != null && to != null && length == null)
			{
				l = Clamp(parent - from.Value - to.Value - ms - me, style, min, max, parent);
				at = from.Value + ms;
			}
			else
			{
				l = Clamp(length ?? fallbackLength, style, min, max, parent);
				at = from != null ? from.Value + ms : to != null ? parent - to.Value - me - l : fallbackAt;
			}
			return (at, l);
		}

		/// <summary>Frames one after another along a column or a row.</summary>
		private static void Flow(List<XElement> frames, Style parentStyle, bool row, double width, double height, Dictionary<XElement, Rect> rects)
		{
			(double padL, double padT, double padR, double padB) = parentStyle.Box("padding", width);
			double gap = parentStyle.Length("gap", row ? width : height, 0) ?? 0;
			double innerMain = (row ? width - padL - padR : height - padT - padB);
			double innerCross = (row ? height - padT - padB : width - padL - padR);
			string justify = parentStyle.Word("justify-content") ?? "flex-start";
			string alignItems = parentStyle.Word("align-items") ?? "stretch";

			int n = frames.Count;
			double[] main = new double[n], before = new double[n], after = new double[n], grow = new double[n];
			Style[] styles = new Style[n];
			for (int i = 0; i < n; i++)
			{
				Style s = styles[i] = Style.Of(frames[i]);
				main[i] = Size(frames[i], s, row, width, height);
				(double ml, double mt, double mr, double mb) = s.Box("margin", width);
				before[i] = row ? ml : mt;
				after[i] = row ? mr : mb;
				grow[i] = Math.Max(0, s.Number("flex-grow") ?? 0);
			}
			double used = main.Sum() + before.Sum() + after.Sum() + gap * (n - 1);
			double free = innerMain - used;
			double growing = grow.Sum();
			if (free > 0 && growing > 0)
			{
				for (int i = 0; i < n; i++) main[i] = Clamp(main[i] + free * grow[i] / growing, styles[i], row ? "min-width" : "min-height", row ? "max-width" : "max-height", row ? width : height);
				free = 0;
			}
			double start = 0, spacing = 0;
			if (free > 0)
			{
				if (justify == "center") start = free / 2;
				else if (justify == "flex-end") start = free;
				else if (justify == "space-between" && n > 1) spacing = free / (n - 1);
			}

			double cursor = (row ? padL : padT) + start;
			for (int i = 0; i < n; i++)
			{
				Style s = styles[i];
				cursor += before[i];
				string align = s.Word("align-self") ?? alignItems;
				string crossSize = row ? "height" : "width";
				(double ml, double mt, double mr, double mb) = s.Box("margin", width);
				double crossBefore = row ? mt : ml, crossAfter = row ? mb : mr;
				double cross;
				bool own = s.Has(crossSize) && s.Word(crossSize) != "auto";
				if (align == "stretch" && !own) cross = innerCross - crossBefore - crossAfter;
				else cross = Size(frames[i], s, !row, width, height);
				cross = Clamp(cross, s, row ? "min-height" : "min-width", row ? "max-height" : "max-width", row ? height : width);
				double crossAt = (row ? padT : padL) + crossBefore;
				if (align == "center") crossAt = (row ? padT : padL) + (innerCross - cross) / 2;
				else if (align == "flex-end") crossAt = (row ? padT : padL) + innerCross - cross - crossAfter;
				(double tx, double ty) = s.Translate(row ? main[i] : cross, row ? cross : main[i]);
				rects[frames[i]] = row
					? new Rect(cursor + tx, crossAt + ty, main[i], cross)
					: new Rect(crossAt + tx, cursor + ty, cross, main[i]);
				cursor += main[i] + after[i] + gap + spacing;
			}
		}

		/// <summary>
		/// A frame's own width (or height): its style's, in units or percent of the parent; for a column
		/// or row that says auto, what its frames take; otherwise its element's.
		/// </summary>
		private static double Size(XElement frame, Style style, bool across, double parentW, double parentH)
		{
			string name = across ? "width" : "height";
			if (style.Word(name) == "auto") return Intrinsic(frame, style, across);
			return style.Length(name, across ? parentW : parentH, 0) ?? Number(frame, name);
		}

		/// <summary>What a column's or a row's frames take along one direction, with its padding - its size when it says auto.</summary>
		private static double Intrinsic(XElement frame, Style style, bool across)
		{
			string direction = style.Word("flex-direction");
			(double padL, double padT, double padR, double padB) = style.Box("padding", 0);
			double pad = across ? padL + padR : padT + padB;
			List<XElement> kids = frame.Elements("frame").Where(f => Style.Of(f).Word("position") != "absolute" && Style.Of(f).Word("display") != "none").ToList();
			if (direction != "row" && direction != "column" || kids.Count == 0) return pad + Number(frame, across ? "width" : "height");
			bool alongMain = (direction == "row") == across;
			double total = 0;
			foreach (XElement kid in kids)
			{
				Style s = Style.Of(kid);
				(double ml, double mt, double mr, double mb) = s.Box("margin", 0);
				// A percent of a size still being worked out counts as nothing, as a browser counts it.
				double own = s.Word(across ? "width" : "height") == "auto" ? Intrinsic(kid, s, across)
					: s.Has(across ? "width" : "height") ? (s.Length(across ? "width" : "height", 0, 0) ?? 0) : Number(kid, across ? "width" : "height");
				double withMargins = own + (across ? ml + mr : mt + mb);
				total = alongMain ? total + withMargins : Math.Max(total, withMargins);
			}
			if (alongMain) total += (style.Length("gap", 0, 0) ?? 0) * (kids.Count - 1);
			return pad + total;
		}

		private static double Clamp(double value, Style style, string min, string max, double parent)
		{
			double? lo = style.Length(min, parent, 0);
			double? hi = style.Length(max, parent, 0);
			if (hi != null) value = Math.Min(value, hi.Value);
			if (lo != null) value = Math.Max(value, lo.Value);
			return value;
		}

		private static double Number(XElement frame, string name)
		{
			XElement e = frame.Element(name);
			string text = (string)e?.Attribute("value") ?? e?.Value;
			return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : 0;
		}

		private static int Round(double v) => (int)Math.Round(v, MidpointRounding.AwayFromZero);

		private readonly struct Rect
		{
			public readonly double X, Y, W, H;
			public Rect(double x, double y, double w, double h) { X = x; Y = y; W = w; H = h; }
		}

		// ------------------------------------------------------------------ the style attribute

		/// <summary>An element's style="name: value; ..." as its properties (the last of a name wins, as in CSS).</summary>
		internal sealed class Style
		{
			private readonly Dictionary<string, string> _values;
			private Style(Dictionary<string, string> values) { _values = values; }

			public bool IsEmpty => _values.Count == 0;

			public static Style Of(XElement element) => Parse((string)element.Attribute("style"));

			public static Style Parse(string text)
			{
				Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				foreach (string part in (text ?? "").Split(';'))
				{
					int colon = part.IndexOf(':');
					if (colon <= 0) continue;
					string name = part.Substring(0, colon).Trim().ToLowerInvariant();
					string value = part.Substring(colon + 1).Trim();
					if (name.Length > 0 && value.Length > 0) values[name] = value;
				}
				return new Style(values);
			}

			public bool Has(string name) => _values.ContainsKey(name);

			public string Word(string name) => _values.TryGetValue(name, out string v) ? v.Trim().ToLowerInvariant() : null;

			public double? Number(string name) => _values.TryGetValue(name, out string v) && double.TryParse(v.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double n) ? n : (double?)null;

			/// <summary>A length: units (px optional) or a percent of `whole`; null when not given or auto.</summary>
			public double? Length(string name, double whole, double fallback)
			{
				if (!_values.TryGetValue(name, out string v)) return null;
				return ParseLength(v, whole);
			}

			/// <summary>A margin side: its own property, or its part of the margin shorthand.</summary>
			public double Margin(string side, double whole)
			{
				(double l, double t, double r, double b) = Box("margin", whole);
				return side.EndsWith("left", StringComparison.Ordinal) ? l : side.EndsWith("top", StringComparison.Ordinal) ? t : side.EndsWith("right", StringComparison.Ordinal) ? r : b;
			}

			/// <summary>A box property (margin, padding): the shorthand's one to four values, each side's own property over it.</summary>
			public (double left, double top, double right, double bottom) Box(string name, double whole)
			{
				double t = 0, r = 0, b = 0, l = 0;
				if (_values.TryGetValue(name, out string all))
				{
					double[] parts = all.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Select(p => ParseLength(p, whole) ?? 0).ToArray();
					if (parts.Length == 1) t = r = b = l = parts[0];
					else if (parts.Length == 2) { t = b = parts[0]; r = l = parts[1]; }
					else if (parts.Length == 3) { t = parts[0]; r = l = parts[1]; b = parts[2]; }
					else if (parts.Length >= 4) { t = parts[0]; r = parts[1]; b = parts[2]; l = parts[3]; }
				}
				l = Length(name + "-left", whole, 0) ?? l;
				t = Length(name + "-top", whole, 0) ?? t;
				r = Length(name + "-right", whole, 0) ?? r;
				b = Length(name + "-bottom", whole, 0) ?? b;
				return (l, t, r, b);
			}

			/// <summary>translate: x [y], a percent of the frame's own size.</summary>
			public (double x, double y) Translate(double width, double height)
			{
				if (!_values.TryGetValue("translate", out string v)) return (0, 0);
				string[] parts = v.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				double x = parts.Length > 0 ? ParseLength(parts[0], width) ?? 0 : 0;
				double y = parts.Length > 1 ? ParseLength(parts[1], height) ?? 0 : 0;
				return (x, y);
			}

			private static double? ParseLength(string text, double whole)
			{
				string v = text.Trim().ToLowerInvariant();
				if (v.Length == 0 || v == "auto") return null;
				if (v.EndsWith("%", StringComparison.Ordinal))
				{
					return double.TryParse(v.Substring(0, v.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double p) ? whole * p / 100 : (double?)null;
				}
				if (v.EndsWith("px", StringComparison.Ordinal)) v = v.Substring(0, v.Length - 2);
				return double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out double n) ? n : (double?)null;
			}
		}
	}
}
