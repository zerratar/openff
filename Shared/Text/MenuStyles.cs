// Crystal Style Sheets - the menus' styles. A small CSS: stylesheets (styles/*.css beside a
// screen's layout, and <style> elements in the layout itself) and a frame's own style attribute,
// cascaded as a browser cascades them, onto the frames of a <menu>.
//
//   #gambits .row        { -ff-panel: window; opacity: 0.9; }
//   text.heading         { color: pale-blue; font-size: 14px; }
//   #w_rules > frame     { -ff-panel: bar; }
//   .picked              { color: #ffd080; }
//
// Selectors: a type (menu, frame, text - a frame with the Text behaviour -, window - a frame with
// a panel), #id (a frame's <id>, a screen's <name>), .class (the class attribute), *, and those
// joined by a space (inside) or > (directly inside), in lists by commas. Specificity and order
// decide as in CSS; a frame's own style attribute is over every sheet; a layout's old style
// words (<colour>, <font>, <align>, <window/>) are under them all, as HTML's attributes are.
// Anything else - pseudo-classes, attribute selectors, @rules - is passed over.
//
// What the cascade comes to is baked, as the layout rules are (MenuLayout): the layout
// properties into the frame's style attribute, for MenuLayout.Bake; the look into the elements
// the client reads as the screen opens:
//
//   color            a palette word (white, yellow, pale-blue, disabled...) or #rgb / #rrggbb   <colour>
//   font-size        6..31 (px optional), or large / normal                                    <font>
//   text-align       left, right, center, menu (the hand stands clear), button                 <align>
//   opacity          0..1 or a percent; a parent's multiplies its frames' (as in CSS)          <opacity>
//   visibility       hidden (the frame's text and panel not drawn); display: none the same     <hidden/>
//   -ff-panel        window (the game's window art), bar (its translucent bar), none           <window/>, <panel>
//   -ff-tint         #rgb / #rrggbb, the window's art multiplied by it; a bar's colour         <tint>
//   background-*, -ff-background-*, -ff-slice-*   a colour and a picture behind the frame (MenuBackground)   <background>
//
// color, font-size, text-align and visibility are inherited, as in CSS. Crystal previews the
// same cascade from a port of this (menu-styles.js).

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OpenFF.Content
{
	internal static class MenuStyles
	{
		/// <summary>The properties a frame's children take from it unless they say otherwise.</summary>
		private static readonly HashSet<string> Inherited = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "color", "font-size", "text-align", "visibility" };

		/// <summary>The look: what the cascade bakes into elements rather than into the style attribute.</summary>
		public static readonly string[] LookProperties = new[] { "color", "font-size", "text-align", "opacity", "visibility", "display", "-ff-panel", "-ff-tint" }.Concat(MenuBackground.Properties).ToArray();

		/// <summary>The stylesheets of a folder of layouts: styles/*.css beside them, in name order.</summary>
		public static List<string> SheetsBeside(string layoutPath)
		{
			List<string> sheets = new List<string>();
			try
			{
				string folder = Path.Combine(Path.GetDirectoryName(layoutPath) ?? ".", "styles");
				if (Directory.Exists(folder))
				{
					foreach (string file in Directory.EnumerateFiles(folder, "*.css").OrderBy(f => f, StringComparer.OrdinalIgnoreCase)) sheets.Add(File.ReadAllText(file));
				}
			}
			catch (IOException) { }
			catch (UnauthorizedAccessException) { }
			return sheets;
		}

		/// <summary>Whether a tree has anything for the cascade to do: a style, a class, a &lt;style&gt;.</summary>
		public static bool Has(XElement root)
		{
			return root != null && root.DescendantsAndSelf().Any(e => e.Name.LocalName == "style" || e.Attribute("style") != null || e.Attribute("class") != null);
		}

		/// <summary>
		/// Cascades the sheets (and the tree's own &lt;style&gt; elements, after them) onto every screen's frames:
		/// the layout properties into each frame's style attribute, the look into its elements. The &lt;style&gt;
		/// elements go; the class attributes stay for MenuLayout.Strip.
		/// </summary>
		public static void Apply(XElement root, IEnumerable<string> sheets = null)
		{
			if (root == null) return;
			List<string> all = (sheets ?? Enumerable.Empty<string>()).ToList();
			foreach (XElement style in root.DescendantsAndSelf().Where(e => e.Name.LocalName == "style").ToList())
			{
				all.Add(style.Value);
				style.Remove();
			}
			Apply(root, Compile(all));
		}

		/// <summary>The cascade of sheets already compiled (the client keeps a screen's for restyling it as it runs).</summary>
		public static void Apply(XElement root, Sheet sheet)
		{
			if (root == null) return;
			List<Rule> rules = sheet?.Rules ?? new List<Rule>();
			IEnumerable<XElement> screens = IsScreen(root) ? new[] { root } : root.Elements().Where(IsScreen);
			foreach (XElement screen in screens)
			{
				Dictionary<string, string> screenStyle = Cascade(screen, rules, null);
				Walk(screen, rules, screenStyle, Opacity(screenStyle, 1));
			}
		}

		// ------------------------------------------------------------------ as the screen runs

		/// <summary>Sheets parsed once, in cascade order.</summary>
		public sealed class Sheet
		{
			internal List<Rule> Rules = new List<Rule>();
		}

		public static Sheet Compile(IEnumerable<string> sheets)
		{
			Sheet sheet = new Sheet();
			int order = 0;
			foreach (string css in sheets ?? Enumerable.Empty<string>()) Parse(css, sheet.Rules, ref order);
			return sheet;
		}

		/// <summary>What the cascade comes to on one frame, as the client puts it on.</summary>
		public sealed class Look
		{
			/// <summary>A palette word or #rrggbb; null for the layout's own.</summary>
			public string Colour;
			/// <summary>&lt;font&gt;'s word: a size 6..31, large or 12; null for none.</summary>
			public string Font;
			public string Align;
			public double Opacity = 1;
			public bool Hidden;
			/// <summary>A panel behind it: the game's window, or with Bar its translucent bar.</summary>
			public bool Window;
			public bool Bar;
			/// <summary>#rrggbb, or null.</summary>
			public string Tint;
			/// <summary>The background's declarations (MenuBackground.Parse), or null.</summary>
			public string Background;

			public bool SameAs(Look o) => o != null && Colour == o.Colour && Font == o.Font && Align == o.Align && Math.Abs(Opacity - o.Opacity) < 0.001
				&& Hidden == o.Hidden && Window == o.Window && Bar == o.Bar && Tint == o.Tint && Background == o.Background;

			public bool SamePanel(Look o) => o != null && Window == o.Window && Bar == o.Bar && Tint == o.Tint && Math.Abs(Opacity - o.Opacity) < 0.001 && Hidden == o.Hidden && Background == o.Background;

			/// <summary>Whether anything is drawn behind the frame: the game's window, or a background.</summary>
			public bool HasPanel => Window || Background != null;
		}

		/// <summary>
		/// Every frame's look under a sheet, the screen itself left as it is: the cascade run on a copy by the very
		/// code the bake uses, and each copy's elements read back - keyed by the frame of the screen passed in.
		/// </summary>
		public static Dictionary<XElement, Look> Looks(XElement screen, Sheet sheet)
		{
			Dictionary<XElement, Look> looks = new Dictionary<XElement, Look>();
			if (screen == null) return looks;
			XElement copy = new XElement(screen);
			Apply(copy, sheet);
			void Pair(XElement from, XElement to)
			{
				List<XElement> a = from.Elements("frame").ToList(), b = to.Elements("frame").ToList();
				for (int i = 0; i < a.Count && i < b.Count; i++)
				{
					looks[a[i]] = ReadLook(b[i]);
					Pair(a[i], b[i]);
				}
			}
			Pair(screen, copy);
			return looks;
		}

		/// <summary>A frame's look as its elements say it (what the bake wrote, or a layout's own words).</summary>
		public static Look ReadLook(XElement frame)
		{
			string Text(string name) { XElement e = frame.Element(name); string v = (string)e?.Attribute("value") ?? e?.Value; return string.IsNullOrWhiteSpace(v) ? null : v.Trim(); }
			Look look = new Look
			{
				Colour = Text("colour") ?? Text("color"),
				Font = Text("font"),
				Align = Text("align"),
				Hidden = frame.Element("hidden") != null,
				Window = frame.Element("window") != null,
				Bar = string.Equals(Text("panel"), "bar", StringComparison.OrdinalIgnoreCase),
				Tint = Hex(Text("tint")),
				Background = Text("background")
			};
			if (double.TryParse(Text("opacity"), NumberStyles.Float, CultureInfo.InvariantCulture, out double o)) look.Opacity = Math.Clamp(o, 0, 1);
			return look;
		}

		/// <summary>Whether an element is picked out by a selector (a list, as a sheet's: "#hero, .row > text").</summary>
		public static bool Matches(XElement element, string selector)
		{
			if (element == null || string.IsNullOrWhiteSpace(selector)) return false;
			foreach (string one in selector.Split(','))
			{
				Selector s = ParseSelector(one.Trim());
				if (s != null && s.Matches(element)) return true;
			}
			return false;
		}

		private static void Walk(XElement parent, List<Rule> rules, Dictionary<string, string> inherited, double opacity)
		{
			foreach (XElement frame in parent.Elements("frame"))
			{
				Dictionary<string, string> computed = Cascade(frame, rules, inherited);
				double own = Opacity(computed, opacity);
				Bake(frame, computed, own);
				Walk(frame, rules, computed, own);
			}
		}

		/// <summary>An element's properties: inherited ones from its parent, then the sheets' rules that match it in cascade order, then its own style attribute.</summary>
		private static Dictionary<string, string> Cascade(XElement element, List<Rule> rules, Dictionary<string, string> inherited)
		{
			Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (inherited != null)
			{
				foreach (KeyValuePair<string, string> kv in inherited) if (Inherited.Contains(kv.Key)) values[kv.Key] = kv.Value;
			}
			foreach ((Rule rule, int specificity) in rules.Select(r => (r, r.Match(element))).Where(m => m.Item2 >= 0).OrderBy(m => m.Item2).ThenBy(m => m.r.Order))
			{
				foreach (KeyValuePair<string, string> d in rule.Declarations) values[d.Key] = d.Value;
			}
			foreach (KeyValuePair<string, string> d in Declarations((string)element.Attribute("style"))) values[d.Key] = d.Value;
			return values;
		}

		/// <summary>The cascade into the frame: its layout properties as its style attribute, its look as its elements.</summary>
		private static void Bake(XElement frame, Dictionary<string, string> computed, double opacity)
		{
			// The layout rules: what MenuLayout reads (not inherited, so only what this frame's rules say).
			Dictionary<string, string> own = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (string name in MenuLayout.Properties) if (computed.TryGetValue(name, out string v)) own[name] = v;
			if (computed.TryGetValue("display", out string display) && display.Trim().Equals("none", StringComparison.OrdinalIgnoreCase)) own["display"] = "none";
			if (own.Count > 0) frame.SetAttributeValue("style", string.Join("; ", own.Select(kv => kv.Key + ": " + kv.Value)));
			else frame.Attribute("style")?.Remove();

			if (computed.TryGetValue("color", out string colour) && Colour(colour) != null) frame.SetElementValue("colour", Colour(colour));
			if (computed.TryGetValue("font-size", out string size))
			{
				string word = size.Trim().ToLowerInvariant();
				if (word.EndsWith("px")) word = word.Substring(0, word.Length - 2).Trim();
				if (word == "large" || word == "normal" || (int.TryParse(word, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) && n >= 6 && n <= 31)) frame.SetElementValue("font", word == "normal" ? "12" : word);
			}
			if (computed.TryGetValue("text-align", out string align))
			{
				string word = align.Trim().ToLowerInvariant();
				if (word == "centre") word = "center";
				if (word == "left" || word == "right" || word == "center" || word == "menu" || word == "button") frame.SetElementValue("align", word);
			}
			// Only ever written, never taken away: a second cascade over a baked frame (the client's own, then FromXml's) finds no opacity left in its style.
			if (opacity < 0.999) frame.SetElementValue("opacity", opacity.ToString("0.###", CultureInfo.InvariantCulture));
			bool hidden = (computed.TryGetValue("visibility", out string visibility) && visibility.Trim().Equals("hidden", StringComparison.OrdinalIgnoreCase))
				|| (display != null && display.Trim().Equals("none", StringComparison.OrdinalIgnoreCase));
			if (hidden && frame.Element("hidden") == null) frame.Add(new XElement("hidden"));
			if (computed.TryGetValue("-ff-panel", out string panel))
			{
				string word = panel.Trim().ToLowerInvariant();
				if (word == "none")
				{
					frame.Elements("window").Remove();
					frame.Elements("panel").Remove();
				}
				else if (word == "window" || word == "bar")
				{
					if (frame.Element("window") == null) frame.AddFirst(new XElement("window"));
					if (word == "bar") frame.SetElementValue("panel", "bar");
					else frame.Elements("panel").Remove();
				}
			}
			if (computed.TryGetValue("-ff-tint", out string tint) && Hex(tint) != null) frame.SetElementValue("tint", Hex(tint));
			// The background (MenuBackground): its declarations as one element, for the client to draw as the frame's panel.
			List<string> background = MenuBackground.Properties.Where(computed.ContainsKey).Select(k => k + ": " + computed[k]).ToList();
			if (background.Count > 0 && MenuBackground.Parse(string.Join("; ", background)) != null) frame.SetElementValue("background", string.Join("; ", background));
			else if (background.Count > 0) frame.Element("background")?.Remove();
		}

		private static double Opacity(Dictionary<string, string> computed, double parent)
		{
			if (!computed.TryGetValue("opacity", out string v)) return parent;
			string t = v.Trim();
			double o;
			if (t.EndsWith("%") && double.TryParse(t.Substring(0, t.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double p)) o = p / 100;
			else if (!double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out o)) return parent;
			return parent * Math.Clamp(o, 0, 1);
		}

		/// <summary>A colour as the client's &lt;colour&gt; takes it: a palette word, or #rrggbb; null for neither.</summary>
		public static string Colour(string value)
		{
			string hex = Hex(value);
			if (hex != null) return hex;
			string word = value?.Trim().ToLowerInvariant();
			return string.IsNullOrEmpty(word) || !Regex.IsMatch(word, "^[a-z][a-z0-9 -]*$") ? null : word;
		}

		/// <summary>#rgb or #rrggbb as #rrggbb; null when it is not one.</summary>
		public static string Hex(string value)
		{
			string v = value?.Trim().ToLowerInvariant();
			if (v == null || !Regex.IsMatch(v, "^#([0-9a-f]{3}|[0-9a-f]{6})$")) return null;
			if (v.Length == 4) v = "#" + v[1] + v[1] + v[2] + v[2] + v[3] + v[3];
			return v;
		}

		private static bool IsScreen(XElement e) => e.Name.LocalName == "menu" || e.Name.LocalName == "unit";

		// ------------------------------------------------------------------ the sheets

		internal sealed class Rule
		{
			public List<Selector> Selectors;
			public List<KeyValuePair<string, string>> Declarations;
			public int Order;

			/// <summary>The highest specificity of the selectors that match, or -1 for none.</summary>
			public int Match(XElement element)
			{
				int best = -1;
				foreach (Selector s in Selectors) if (s.Matches(element)) best = Math.Max(best, s.Specificity);
				return best;
			}
		}

		/// <summary>Compounds joined by combinators, the last one the element itself.</summary>
		internal sealed class Selector
		{
			public List<Compound> Parts = new List<Compound>();
			public List<char> Joins = new List<char>();   // ' ' or '>' before each part after the first
			public int Specificity;

			public bool Matches(XElement element) => MatchFrom(Parts.Count - 1, element);

			private bool MatchFrom(int index, XElement element)
			{
				if (!Parts[index].Matches(element)) return false;
				if (index == 0) return true;
				char join = Joins[index - 1];
				for (XElement up = element.Parent; up != null; up = up.Parent)
				{
					if (MatchFrom(index - 1, up)) return true;
					if (join == '>') return false;
				}
				return false;
			}
		}

		internal sealed class Compound
		{
			public string Type;             // null or * for any
			public string Id;
			public List<string> Classes = new List<string>();

			public bool Matches(XElement e)
			{
				string name = e.Name.LocalName;
				if (name != "frame" && name != "menu" && name != "unit") return false;
				if (Type != null && Type != "*")
				{
					bool ok = Type switch
					{
						"menu" => name == "menu" || name == "unit",
						"frame" => name == "frame",
						"text" => name == "frame" && (string)e.Element("behavior")?.Attribute("value") == "Text",
						"window" => name == "frame" && e.Element("window") != null,
						_ => false
					};
					if (!ok) return false;
				}
				if (Id != null)
				{
					string id = name == "frame" ? (string)e.Element("id") : (string)e.Element("name");
					if (!string.Equals(id?.Trim(), Id, StringComparison.Ordinal)) return false;
				}
				if (Classes.Count > 0)
				{
					HashSet<string> has = new HashSet<string>(((string)e.Attribute("class") ?? "").Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);
					if (!Classes.All(has.Contains)) return false;
				}
				return true;
			}
		}

		private static void Parse(string css, List<Rule> into, ref int order)
		{
			if (string.IsNullOrWhiteSpace(css)) return;
			string text = Regex.Replace(css, @"/\*.*?\*/", " ", RegexOptions.Singleline);
			int at = 0;
			while (at < text.Length)
			{
				int open = text.IndexOf('{', at);
				if (open < 0) break;
				int close = text.IndexOf('}', open);
				if (close < 0) break;
				string head = text.Substring(at, open - at).Trim();
				string body = text.Substring(open + 1, close - open - 1);
				at = close + 1;
				if (head.StartsWith("@")) continue;   // @media and the rest: not for a menu
				List<Selector> selectors = head.Split(',').Select(s => ParseSelector(s.Trim())).Where(s => s != null).ToList();
				if (selectors.Count == 0) continue;
				into.Add(new Rule { Selectors = selectors, Declarations = Declarations(body).ToList(), Order = order++ });
			}
		}

		/// <summary>A selector, or null for one this does not read (a pseudo-class, an attribute) - its rule is passed over, as a browser passes over what it cannot parse.</summary>
		private static Selector ParseSelector(string text)
		{
			if (text.Length == 0 || text.IndexOfAny(new[] { ':', '[', '+', '~' }) >= 0) return null;
			Selector selector = new Selector();
			string spaced = text.Replace(">", " > ");
			char join = ' ';
			foreach (string token in spaced.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (token == ">") { join = '>'; continue; }
				Compound c = ParseCompound(token);
				if (c == null) return null;
				if (selector.Parts.Count > 0) selector.Joins.Add(join);
				selector.Parts.Add(c);
				join = ' ';
				selector.Specificity += (c.Id != null ? 10000 : 0) + c.Classes.Count * 100 + (c.Type != null && c.Type != "*" ? 1 : 0);
			}
			return selector.Parts.Count > 0 ? selector : null;
		}

		private static Compound ParseCompound(string token)
		{
			Match m = Regex.Match(token, @"^(\*|[A-Za-z][\w-]*)?((?:[#.][\w-]+)*)$");
			if (!m.Success) return null;
			Compound c = new Compound { Type = m.Groups[1].Success && m.Groups[1].Value.Length > 0 ? m.Groups[1].Value.ToLowerInvariant() : null };
			foreach (Match part in Regex.Matches(m.Groups[2].Value, @"[#.][\w-]+"))
			{
				if (part.Value[0] == '#') c.Id = part.Value.Substring(1);
				else c.Classes.Add(part.Value.Substring(1));
			}
			return c;
		}

		/// <summary>"name: value; ..." as its declarations, in order (!important is read as nothing more than the value).</summary>
		public static IEnumerable<KeyValuePair<string, string>> Declarations(string text)
		{
			foreach (string part in (text ?? "").Split(';'))
			{
				int colon = part.IndexOf(':');
				if (colon <= 0) continue;
				string name = part.Substring(0, colon).Trim().ToLowerInvariant();
				string value = part.Substring(colon + 1).Replace("!important", "").Trim();
				if (name.Length > 0 && value.Length > 0) yield return new KeyValuePair<string, string>(name, value);
			}
		}
	}
}
