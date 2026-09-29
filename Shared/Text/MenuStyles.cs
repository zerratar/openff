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
// a panel -, portrait - the hero's face), #id (a frame's <id>, a screen's <name>), .class (the class attribute), *, and those
// joined by a space (inside) or > (directly inside), in lists by commas. Specificity and order
// decide as in CSS; a frame's own style attribute is over every sheet; a layout's old style
// words (<colour>, <font>, <align>, <window/>) are under them all, as HTML's attributes are.
// Pseudo-classes: :focus (the cursor is on the frame), :focus-within (on it or a frame inside it),
// :disabled / :enabled (IMenuWidget.Enabled), :checked (a choice the game shows as set: a config row's), :first-child, :last-child, :only-child, :nth-child(),
// :nth-last-child() (An+B, odd, even) and :not() of one of these or a type, #id, .class. The states
// are the running screen's (the client restyles as they change); what the game is given is baked
// with none of them. @keyframes are read for the animations (MenuAnimation); any other @rule, an
// attribute selector, a pseudo-element or a pseudo-class not here is passed over.
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
//   border-*, box-shadow, gradients              the box round it (MenuBackground, MenuPaint)                  <background>
//   text-shadow, font-family, font-weight, font-style, letter-spacing, line-height,
//   text-decoration, text-transform, -ff-text-stroke   the lettering (MenuText)                                 <textstyle>
//   transition-*, animation-*                    how the look moves (MenuAnimation)                            <transition>, <animation>
//
// color, font-size, text-align, visibility and the lettering are inherited, as in CSS. Crystal
// previews the same cascade from a port of this (menu-styles.js).

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
		private static readonly HashSet<string> Inherited = new HashSet<string>(new[] { "color", "font-size", "text-align", "visibility" }.Concat(MenuText.Properties), StringComparer.OrdinalIgnoreCase);

		/// <summary>The look: what the cascade bakes into elements rather than into the style attribute.</summary>
		public static readonly string[] LookProperties = new[] { "color", "font-size", "text-align", "opacity", "visibility", "display", "-ff-panel", "-ff-tint" }
			.Concat(MenuBackground.Properties).Concat(MenuText.Properties).Concat(MenuAnimation.TransitionProperties).Concat(MenuAnimation.AnimationProperties).ToArray();

		/// <summary>The attribute that carries a frame's states for the pseudo-classes ("focus disabled"), on the layout the running screen cascades.</summary>
		public const string StateAttribute = "data-ff-state";

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
			/// <summary>The @keyframes by name (the last of a name wins, as in CSS).</summary>
			public Dictionary<string, List<MenuAnimation.Keyframe>> Keyframes = new Dictionary<string, List<MenuAnimation.Keyframe>>(StringComparer.Ordinal);
			/// <summary>Whether a rule asks for a state (:focus, :disabled...): the running screen restyles as those change.</summary>
			public bool UsesStates => Rules.Any(r => r.Selectors.Any(s => s.UsesStates));
		}

		public static Sheet Compile(IEnumerable<string> sheets)
		{
			Sheet sheet = new Sheet();
			int order = 0;
			foreach (string css in sheets ?? Enumerable.Empty<string>()) Parse(css, sheet, ref order);
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
			/// <summary>No panel said outright (-ff-panel: none): the game's own window taken away where the game makes one regardless.</summary>
			public bool NoPanel;
			/// <summary>#rrggbb, or null.</summary>
			public string Tint;
			/// <summary>The background's declarations (MenuBackground.Parse), or null.</summary>
			public string Background;
			/// <summary>A portrait frame (&lt;portrait/&gt;): the hero's face drawn in it in place of the game's own - "" the screen's hero, a number a party slot; null for none.</summary>
			public string Portrait;
			/// <summary>The lettering's declarations (MenuText.Parse), or null.</summary>
			public string TextStyle;
			/// <summary>How the look moves: the transition's and the animation's declarations (MenuAnimation), or null.</summary>
			public string Transition;
			public string Animation;
			/// <summary>The frame's translate as it is now (a transition's, an animation's): what it draws moved by the difference from the one its layout was built with.</summary>
			public string Translate;

			public bool SameAs(Look o) => o != null && Colour == o.Colour && Font == o.Font && Align == o.Align && Math.Abs(Opacity - o.Opacity) < 0.001
				&& Hidden == o.Hidden && Window == o.Window && Bar == o.Bar && NoPanel == o.NoPanel && Tint == o.Tint && Background == o.Background && Portrait == o.Portrait
				&& TextStyle == o.TextStyle && Transition == o.Transition && Animation == o.Animation && Translate == o.Translate;

			public bool SamePanel(Look o) => o != null && Window == o.Window && Bar == o.Bar && Tint == o.Tint && Math.Abs(Opacity - o.Opacity) < 0.001 && Hidden == o.Hidden && Background == o.Background && Portrait == o.Portrait;

			/// <summary>Whether anything is drawn behind the frame: the game's window, or a background.</summary>
			public bool HasPanel => Window || Background != null || Portrait != null;
		}

		/// <summary>
		/// Every frame's look under a sheet, the screen itself left as it is: the cascade run on a copy by the very
		/// code the bake uses, and each copy's elements read back - keyed by the frame of the screen passed in.
		/// </summary>
		public static Dictionary<XElement, Look> Looks(XElement screen, Sheet sheet)
		{
			return Looks(screen, Computed(screen, sheet));
		}

		/// <summary>Every frame's look from its properties (the cascade's, or what an animator made of them): each frame's opacity its parents' times its own.</summary>
		public static Dictionary<XElement, Look> Looks(XElement screen, IEnumerable<(XElement Frame, Dictionary<string, string> Computed)> computed, IReadOnlyDictionary<XElement, Dictionary<string, string>> shown = null)
		{
			Dictionary<XElement, Look> looks = new Dictionary<XElement, Look>();
			if (screen == null) return looks;
			Dictionary<XElement, double> opacity = new Dictionary<XElement, double>();
			HashSet<XElement> gone = new HashSet<XElement>();
			foreach ((XElement frame, Dictionary<string, string> own) in computed)
			{
				Dictionary<string, string> values = shown != null && shown.TryGetValue(frame, out Dictionary<string, string> s) ? s : own;
				if (frame == screen) { opacity[frame] = Opacity(values, 1); continue; }
				double parent = frame.Parent != null && opacity.TryGetValue(frame.Parent, out double po) ? po : 1;
				double mine = Opacity(values, parent);
				opacity[frame] = mine;
				Look look = LookOf(frame, values, mine);
				// display: none takes the frame and everything in it away, as CSS does (it is not inherited, but what is inside goes with it).
				if ((values.TryGetValue("display", out string display) && display.Trim().Equals("none", StringComparison.OrdinalIgnoreCase)) || (frame.Parent != null && gone.Contains(frame.Parent)))
				{
					gone.Add(frame);
					look.Hidden = true;
				}
				looks[frame] = look;
			}
			return looks;
		}

		/// <summary>
		/// The screen and every frame of it, in the layout's order, with its properties cascaded (the inherited ones from its
		/// parent's) - the screen first. What Apply bakes, before it is baked: an animator works on these.
		/// </summary>
		public static List<(XElement Frame, Dictionary<string, string> Computed)> Computed(XElement screen, Sheet sheet)
		{
			List<(XElement, Dictionary<string, string>)> list = new List<(XElement, Dictionary<string, string>)>();
			if (screen == null) return list;
			List<Rule> rules = sheet?.Rules ?? new List<Rule>();
			Dictionary<string, string> top = Cascade(screen, rules, null);
			list.Add((screen, top));
			void Walk(XElement parent, Dictionary<string, string> inherited)
			{
				foreach (XElement frame in parent.Elements("frame"))
				{
					Dictionary<string, string> c = Cascade(frame, rules, inherited);
					list.Add((frame, c));
					Walk(frame, c);
				}
			}
			Walk(screen, top);
			return list;
		}

		/// <summary>One frame's look from its properties: baked onto a copy of it (its children left out) and read back, by the very code the bake uses.</summary>
		public static Look LookOf(XElement frame, Dictionary<string, string> computed, double opacity)
		{
			XElement copy = new XElement(frame.Name, frame.Attributes(), frame.Elements().Where(e => e.Name.LocalName != "frame"));
			Bake(copy, computed, opacity);
			Look look = ReadLook(copy);
			look.Translate = computed.TryGetValue("translate", out string t) ? t.Trim() : null;
			return look;
		}

		/// <summary>A translate ("x [y]", px or % of the frame's own size) in menu units; nothing for none.</summary>
		public static (float X, float Y) TranslateOf(string value, float width, float height)
		{
			if (string.IsNullOrWhiteSpace(value) || value.Trim().Equals("none", StringComparison.OrdinalIgnoreCase)) return (0, 0);
			string[] parts = value.Trim().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
			float One(string p, float whole)
			{
				string t = p.Trim().ToLowerInvariant();
				if (t.EndsWith("%") && float.TryParse(t.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out float pc)) return whole * pc / 100;
				if (t.EndsWith("px")) t = t.Substring(0, t.Length - 2);
				return float.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : 0;
			}
			return (parts.Length > 0 ? One(parts[0], width) : 0, parts.Length > 1 ? One(parts[1], height) : 0);
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
				NoPanel = string.Equals(Text("panel"), "none", StringComparison.OrdinalIgnoreCase),
				Tint = Hex(Text("tint")),
				Background = Text("background"),
				Portrait = frame.Element("portrait") == null ? null : (Text("portrait") ?? ""),
				TextStyle = Text("textstyle"),
				Transition = Text("transition"),
				Animation = Text("animation")
			};
			if (double.TryParse(Text("opacity"), NumberStyles.Float, CultureInfo.InvariantCulture, out double o)) look.Opacity = Math.Clamp(o, 0, 1);
			return look;
		}

		/// <summary>A screen's own background: the background properties its &lt;menu&gt;'s style and the sheets' rules for it (menu, #name) say; null for none.</summary>
		public static string ScreenBackground(XElement screen, Sheet sheet)
		{
			if (screen == null) return null;
			Dictionary<string, string> computed = Cascade(screen, sheet?.Rules ?? new List<Rule>(), null);
			List<string> background = MenuBackground.Properties.Where(computed.ContainsKey).Select(k => k + ": " + computed[k]).ToList();
			return background.Count > 0 && MenuBackground.Parse(string.Join("; ", background)) != null ? string.Join("; ", background) : null;
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
					// No window - and said so (<panel>none</panel>), for a frame whose window the game makes whatever the layout
					// says (field_hud's dialogue and map name: FieldHud takes it away). The game passes over the word.
					frame.Elements("window").Remove();
					frame.SetElementValue("panel", "none");
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
			// The lettering, and how the look moves: their declarations as one element each, for the client (the game passes over them).
			Element(frame, "textstyle", MenuText.Properties, computed, d => MenuText.Parse(d) != null);
			Element(frame, "transition", MenuAnimation.TransitionProperties, computed, d => true);
			Element(frame, "animation", MenuAnimation.AnimationProperties, computed, d => true);
		}

		private static void Element(XElement frame, string name, string[] properties, Dictionary<string, string> computed, Func<string, bool> draws)
		{
			List<string> said = properties.Where(computed.ContainsKey).Select(k => k + ": " + computed[k]).ToList();
			string declarations = string.Join("; ", said);
			if (said.Count > 0 && draws(declarations)) frame.SetElementValue(name, declarations);
			else if (said.Count > 0) frame.Element(name)?.Remove();
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

			public bool UsesStates => Parts.Any(p => p.UsesStates);

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
			public List<(string Name, string Argument, Compound Not)> Pseudos = new List<(string, string, Compound)>();

			/// <summary>As CSS counts it: an id, then classes and pseudo-classes (a :not() by what it holds), then a type.</summary>
			public int Specificity => (Id != null ? 10000 : 0) + Classes.Count * 100 + Pseudos.Sum(p => p.Name == "not" ? p.Not?.Specificity ?? 0 : 100) + (Type != null && Type != "*" ? 1 : 0);

			public bool UsesStates => Pseudos.Any(p => p.Name == "focus" || p.Name == "focus-within" || p.Name == "disabled" || p.Name == "enabled" || p.Name == "checked" || (p.Not?.UsesStates ?? false));

			public bool Matches(XElement e)
			{
				string name = e.Name.LocalName;
				if (name != "frame" && name != "menu" && name != "unit") return false;
				foreach ((string pseudo, string argument, Compound not) in Pseudos)
				{
					if (!Pseudo(e, pseudo, argument, not)) return false;
				}
				if (Type != null && Type != "*")
				{
					bool ok = Type switch
					{
						"menu" => name == "menu" || name == "unit",
						"frame" => name == "frame",
						"text" => name == "frame" && (string)e.Element("behavior")?.Attribute("value") == "Text",
						"window" => name == "frame" && e.Element("window") != null,
						"portrait" => name == "frame" && e.Element("portrait") != null,
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

			private static bool HasState(XElement e, string state) => ((string)e.Attribute(StateAttribute) ?? "").Split(' ').Contains(state);

			private static bool Pseudo(XElement e, string name, string argument, Compound not)
			{
				switch (name)
				{
					case "focus": return HasState(e, "focus");
					case "focus-within": return HasState(e, "focus") || e.Descendants("frame").Any(d => HasState(d, "focus"));
					case "disabled": return HasState(e, "disabled");
					case "enabled": return !HasState(e, "disabled");
					case "checked": return HasState(e, "checked");
					case "not": return not != null && !not.Matches(e);
				}
				List<XElement> siblings = e.Parent?.Elements(e.Name).ToList() ?? new List<XElement> { e };
				int index = siblings.IndexOf(e);
				switch (name)
				{
					case "first-child": return index == 0;
					case "last-child": return index == siblings.Count - 1;
					case "only-child": return siblings.Count == 1;
					case "nth-child": return Nth(argument, index + 1);
					case "nth-last-child": return Nth(argument, siblings.Count - index);
				}
				return false;
			}

			/// <summary>Whether the n-th (from 1) is picked by An+B, odd or even.</summary>
			private static bool Nth(string formula, int n)
			{
				string f = (formula ?? "").Replace(" ", "").ToLowerInvariant();
				if (f == "odd") f = "2n+1";
				else if (f == "even") f = "2n";
				Match m = Regex.Match(f, @"^(?:([+-]?\d*)n)?([+-]?\d+)?$");
				if (!m.Success || f.Length == 0) return false;
				bool hasN = f.Contains('n');
				int a = !hasN ? 0 : m.Groups[1].Value == "" || m.Groups[1].Value == "+" ? 1 : m.Groups[1].Value == "-" ? -1 : int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
				int b = m.Groups[2].Success && m.Groups[2].Value.Length > 0 ? int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture) : 0;
				if (a == 0) return n == b;
				int k = n - b;
				return k % a == 0 && k / a >= 0;
			}
		}

		private static void Parse(string css, Sheet into, ref int order)
		{
			if (string.IsNullOrWhiteSpace(css)) return;
			string text = Regex.Replace(css, @"/\*.*?\*/", " ", RegexOptions.Singleline);
			foreach ((string head, string body) in Blocks(text))
			{
				if (head.StartsWith("@keyframes", StringComparison.OrdinalIgnoreCase) || head.StartsWith("@-webkit-keyframes", StringComparison.OrdinalIgnoreCase))
				{
					string name = head.Substring(head.IndexOf("keyframes", StringComparison.OrdinalIgnoreCase) + 9).Trim().Trim('"', '\'');
					if (name.Length == 0) continue;
					List<MenuAnimation.Keyframe> stops = new List<MenuAnimation.Keyframe>();
					foreach ((string at, string declarations) in Blocks(body))
					{
						foreach (string one in at.Split(','))
						{
							string o = one.Trim().ToLowerInvariant();
							double? offset = o == "from" ? 0 : o == "to" ? 1 : o.EndsWith("%") && double.TryParse(o.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out double p) ? Math.Clamp(p / 100, 0, 1) : (double?)null;
							if (offset == null) continue;
							MenuAnimation.Keyframe stop = new MenuAnimation.Keyframe { Offset = offset.Value };
							foreach (KeyValuePair<string, string> d in Declarations(declarations)) stop.Values[d.Key] = d.Value;
							stops.Add(stop);
						}
					}
					into.Keyframes[name] = stops.OrderBy(k => k.Offset).ToList();
					continue;
				}
				if (head.StartsWith("@")) continue;   // @media and the rest: not for a menu
				List<Selector> selectors = SplitSelectors(head).Select(s => ParseSelector(s.Trim())).Where(s => s != null).ToList();
				if (selectors.Count == 0) continue;
				into.Rules.Add(new Rule { Selectors = selectors, Declarations = Declarations(body).ToList(), Order = order++ });
			}
		}

		/// <summary>The top-level blocks of a sheet: each one's head and what is between its braces (a nested block kept whole, as @keyframes has them).</summary>
		private static IEnumerable<(string Head, string Body)> Blocks(string text)
		{
			int at = 0;
			while (at < text.Length)
			{
				int open = text.IndexOf('{', at);
				if (open < 0) yield break;
				int depth = 1, close = open + 1;
				for (; close < text.Length && depth > 0; close++)
				{
					if (text[close] == '{') depth++;
					else if (text[close] == '}') depth--;
				}
				if (depth > 0) yield break;
				string head = text.Substring(at, open - at).Trim();
				// A declaration left before a block ("a: b; x { }") is no part of its head.
				int semi = head.LastIndexOf(';');
				if (semi >= 0) head = head.Substring(semi + 1).Trim();
				yield return (head, text.Substring(open + 1, close - open - 2));
				at = close;
			}
		}

		/// <summary>A selector list split at its commas - not the ones inside :not() or :nth-child().</summary>
		private static List<string> SplitSelectors(string head) => MenuAnimation.CommaList(head);

		/// <summary>A selector, or null for one this does not read (a pseudo-class, an attribute) - its rule is passed over, as a browser passes over what it cannot parse.</summary>
		private static Selector ParseSelector(string text)
		{
			// Sibling combinators and attributes are not read - but a + inside brackets (:nth-child(2n+1)) is no combinator.
			string outside = Regex.Replace(text, @"\([^)]*\)", "()");
			if (text.Length == 0 || outside.IndexOfAny(new[] { '[', '+', '~' }) >= 0 || text.Contains("::")) return null;
			// Spaces inside brackets (":nth-child(2n + 1)", ":not(.a)") are no combinators.
			text = Regex.Replace(text, @"\([^)]*\)", m => m.Value.Replace(" ", ""));
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
				selector.Specificity += c.Specificity;
			}
			return selector.Parts.Count > 0 ? selector : null;
		}

		private static readonly HashSet<string> KnownPseudos = new HashSet<string>(StringComparer.Ordinal)
		{
			"focus", "focus-within", "disabled", "enabled", "checked", "first-child", "last-child", "only-child", "nth-child", "nth-last-child", "not"
		};

		private static Compound ParseCompound(string token)
		{
			Match m = Regex.Match(token, @"^(\*|[A-Za-z][\w-]*)?((?:[#.][\w-]+|:[\w-]+(?:\([^)]*\))?)*)$");
			if (!m.Success) return null;
			Compound c = new Compound { Type = m.Groups[1].Success && m.Groups[1].Value.Length > 0 ? m.Groups[1].Value.ToLowerInvariant() : null };
			foreach (Match part in Regex.Matches(m.Groups[2].Value, @"[#.][\w-]+|:([\w-]+)(?:\(([^)]*)\))?"))
			{
				if (part.Value[0] == '#') c.Id = part.Value.Substring(1);
				else if (part.Value[0] == '.') c.Classes.Add(part.Value.Substring(1));
				else
				{
					string name = part.Groups[1].Value.ToLowerInvariant();
					if (!KnownPseudos.Contains(name)) return null;   // :hover and the rest: the rule is passed over
					string argument = part.Groups[2].Success ? part.Groups[2].Value : null;
					Compound not = null;
					if (name == "not")
					{
						not = argument == null ? null : ParseCompound(argument);
						if (not == null) return null;
					}
					else if ((name == "nth-child" || name == "nth-last-child") && string.IsNullOrWhiteSpace(argument)) return null;
					c.Pseudos.Add((name, argument, not));
				}
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
