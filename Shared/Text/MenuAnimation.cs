// Crystal Style Sheets in motion: transitions and @keyframes animations, as CSS has them.
//
//   .row          { transition: opacity 0.2s ease-out, color 0.2s; }
//   .row:focus    { color: #ffd080; }
//   @keyframes pulse { from { opacity: 1; } 50% { opacity: 0.4; } to { opacity: 1; } }
//   .picked       { animation: pulse 1.2s ease-in-out infinite; }
//
//   transition                  <property | all> <duration> [<timing>] [<delay>], ...
//   transition-property / -duration / -timing-function / -delay   the same, one list each
//   animation                   <name> <duration> [<timing>] [<delay>] [<count | infinite>] [<direction>] [<fill-mode>] [<play-state>], ...
//   animation-name / -duration / -timing-function / -delay / -iteration-count / -direction / -fill-mode / -play-state
//   timing                      linear, ease, ease-in, ease-out, ease-in-out, cubic-bezier(a, b, c, d), steps(n[, start | end]), step-start, step-end
//   direction                   normal, reverse, alternate, alternate-reverse; fill-mode none, forwards, backwards, both
//
// What moves is the look - opacity, color, -ff-tint, the background's colours, gradients, borders,
// corners and shadows, the text's shadow, outline and spacing - and translate, which moves what the
// frame draws (and its frames') from where the layout put it; never the rest of the layout, which
// the game is given as plain numbers. A value moves from one to the other when the two have the same shape
// (the same words, with numbers and colours in the same places: "0 2px 8px #000" to "0 4px 16px
// #f00"); otherwise it changes half way, as CSS changes what it cannot interpolate.
//
// MenuStyles.Computed gives every frame's cascaded properties; the Animator is handed them each
// frame with the time, and gives back what to show - the transitions and animations applied. The
// client's screen keeps one (ModMenus); Crystal's preview runs a port of it (menu-animation.js).

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OpenFF.Content
{
	internal static class MenuAnimation
	{
		/// <summary>The properties that say how a frame moves (baked into &lt;transition&gt; and &lt;animation&gt;; never into the game's).</summary>
		public static readonly string[] TransitionProperties = { "transition", "transition-property", "transition-duration", "transition-timing-function", "transition-delay" };
		public static readonly string[] AnimationProperties =
		{
			"animation", "animation-name", "animation-duration", "animation-timing-function", "animation-delay",
			"animation-iteration-count", "animation-direction", "animation-fill-mode", "animation-play-state"
		};

		/// <summary>The properties that move: the look, not the layout nor what says how things move.</summary>
		public static bool Animatable(string property)
		{
			if (string.IsNullOrEmpty(property)) return false;
			// translate moves what is drawn, not the layout (the frame's place stays): the one layout property that moves.
			if (property.Equals("translate", StringComparison.OrdinalIgnoreCase)) return true;
			if (MenuLayout.Properties.Contains(property, StringComparer.OrdinalIgnoreCase)) return false;
			if (TransitionProperties.Contains(property, StringComparer.OrdinalIgnoreCase) || AnimationProperties.Contains(property, StringComparer.OrdinalIgnoreCase)) return false;
			// A panel of its own (the game's window made or taken away) and display (the layout's) do not move.
			return !property.Equals("display", StringComparison.OrdinalIgnoreCase) && !property.Equals("-ff-panel", StringComparison.OrdinalIgnoreCase);
		}

		// ------------------------------------------------------------------ the values

		/// <summary>A palette word's colour (0xRRGGBBAA), for colours to move between the game's words and #hex; the host sets it (the client: the game's text palette).</summary>
		public static Func<string, uint?> Palette;

		private static readonly Regex Token = new Regex(@"#[0-9a-fA-F]{3,8}\b|rgba?\([^)]*\)|-?(?:\d+\.?\d*|\.\d+)(?:[a-zA-Z%]+)?|[A-Za-z][\w-]*", RegexOptions.Compiled);

		private enum Kind { Number, Colour }

		private struct Slot
		{
			public Kind Kind;
			public double Number;
			public string Unit;
			public uint Colour;
		}

		/// <summary>A value as its shape (the text with its numbers and colours taken out) and those numbers and colours.</summary>
		private static (string Shape, List<Slot> Slots) Split(string value)
		{
			List<Slot> slots = new List<Slot>();
			StringBuilder shape = new StringBuilder();
			int at = 0;
			foreach (Match m in Token.Matches(value ?? ""))
			{
				shape.Append(value, at, m.Index - at);
				at = m.Index + m.Length;
				string t = m.Value;
				if (t[0] == '#' || t.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
				{
					uint? c = MenuBackground.ParseColour(t);
					if (c.HasValue) { slots.Add(new Slot { Kind = Kind.Colour, Colour = c.Value }); shape.Append("\u0001C"); continue; }
				}
				else if (char.IsDigit(t[0]) || t[0] == '-' || t[0] == '.')
				{
					Match n = Regex.Match(t, @"^(-?(?:\d+\.?\d*|\.\d+))([a-zA-Z%]*)$");
					if (n.Success && double.TryParse(n.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
					{
						slots.Add(new Slot { Kind = Kind.Number, Number = d, Unit = n.Groups[2].Value });
						shape.Append("\u0001N").Append(n.Groups[2].Value);
						continue;
					}
				}
				else if (Palette != null && t != "transparent" && Palette(t.ToLowerInvariant()) is uint p)
				{
					slots.Add(new Slot { Kind = Kind.Colour, Colour = p });
					shape.Append("\u0001C");
					continue;
				}
				else if (t.Equals("transparent", StringComparison.OrdinalIgnoreCase))
				{
					slots.Add(new Slot { Kind = Kind.Colour, Colour = 0 });
					shape.Append("\u0001C");
					continue;
				}
				shape.Append(t);
			}
			shape.Append(value, at, (value ?? "").Length - at);
			return (Regex.Replace(shape.ToString(), @"\s+", " ").Trim(), slots);
		}

		/// <summary>The value a fraction t of the way from a to b: numbers and colours moved where the two have the same shape, else a until half way, then b.</summary>
		public static string Interpolate(string a, string b, double t)
		{
			if (t <= 0) return a;
			if (t >= 1) return b;
			if (a == null || b == null || a == b) return t < 0.5 ? a : b;
			(string sa, List<Slot> pa) = Split(a);
			(string sb, List<Slot> pb) = Split(b);
			if (sa != sb || pa.Count != pb.Count || pa.Count == 0) return t < 0.5 ? a : b;
			StringBuilder o = new StringBuilder();
			int slot = 0;
			for (int i = 0; i < sa.Length; i++)
			{
				if (sa[i] != '\u0001') { o.Append(sa[i]); continue; }
				char kind = sa[++i];
				Slot x = pa[slot], y = pb[slot];
				slot++;
				if (kind == 'N')
				{
					double v = x.Number + (y.Number - x.Number) * t;
					o.Append(v.ToString("0.###", CultureInfo.InvariantCulture));   // the unit follows in the shape
				}
				else o.Append(ColourText(Mix(x.Colour, y.Colour, t)));
			}
			return o.ToString();
		}

		/// <summary>Two colours (0xRRGGBBAA) mixed, premultiplied as CSS mixes them (a colour fading from transparent keeps its hue).</summary>
		public static uint Mix(uint a, uint b, double t)
		{
			double aa = (a & 0xFF) / 255.0, ba = (b & 0xFF) / 255.0;
			double alpha = aa + (ba - aa) * t;
			byte Ch(int shift)
			{
				double ca = ((a >> shift) & 0xFF) * aa, cb = ((b >> shift) & 0xFF) * ba;
				double v = alpha <= 0.0001 ? 0 : (ca + (cb - ca) * t) / alpha;
				return (byte)Math.Clamp((int)Math.Round(v), 0, 255);
			}
			return (uint)(Ch(24) << 24 | Ch(16) << 16 | Ch(8) << 8) | (uint)Math.Clamp((int)Math.Round(alpha * 255), 0, 255);
		}

		/// <summary>#rrggbb, or #rrggbbaa when it is not opaque.</summary>
		public static string ColourText(uint rgba)
		{
			string rgb = "#" + (rgba >> 8).ToString("x6", CultureInfo.InvariantCulture);
			return (rgba & 0xFF) == 0xFF ? rgb : rgb + (rgba & 0xFF).ToString("x2", CultureInfo.InvariantCulture);
		}

		// ------------------------------------------------------------------ time

		/// <summary>"0.3s" / "300ms" in seconds; null when it is not a time.</summary>
		public static double? Seconds(string text)
		{
			string t = text?.Trim().ToLowerInvariant();
			if (string.IsNullOrEmpty(t)) return null;
			if (t.EndsWith("ms") && double.TryParse(t.Substring(0, t.Length - 2), NumberStyles.Float, CultureInfo.InvariantCulture, out double ms)) return ms / 1000;
			if (t.EndsWith("s") && double.TryParse(t.Substring(0, t.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double s)) return s;
			return null;
		}

		/// <summary>A timing function: the progress shown at a fraction of the time.</summary>
		public static Func<double, double> Timing(string text)
		{
			string t = (text ?? "ease").Trim().ToLowerInvariant();
			switch (t)
			{
				case "linear": return x => x;
				case "ease": return Bezier(0.25, 0.1, 0.25, 1);
				case "ease-in": return Bezier(0.42, 0, 1, 1);
				case "ease-out": return Bezier(0, 0, 0.58, 1);
				case "ease-in-out": return Bezier(0.42, 0, 0.58, 1);
				case "step-start": return Steps(1, true);
				case "step-end": return Steps(1, false);
			}
			Match m = Regex.Match(t, @"^cubic-bezier\(\s*([-\d.]+)\s*,\s*([-\d.]+)\s*,\s*([-\d.]+)\s*,\s*([-\d.]+)\s*\)$");
			if (m.Success)
			{
				double N(int i) => double.Parse(m.Groups[i].Value, CultureInfo.InvariantCulture);
				return Bezier(Math.Clamp(N(1), 0, 1), N(2), Math.Clamp(N(3), 0, 1), N(4));
			}
			m = Regex.Match(t, @"^steps\(\s*(\d+)\s*(?:,\s*(start|end|jump-start|jump-end)\s*)?\)$");
			if (m.Success) return Steps(Math.Max(1, int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture)), m.Groups[2].Value.EndsWith("start"));
			return Bezier(0.25, 0.1, 0.25, 1);
		}

		private static bool IsTiming(string word)
		{
			string w = word.Trim().ToLowerInvariant();
			return w == "linear" || w == "ease" || w == "ease-in" || w == "ease-out" || w == "ease-in-out" || w == "step-start" || w == "step-end" || w.StartsWith("cubic-bezier(") || w.StartsWith("steps(");
		}

		private static Func<double, double> Steps(int n, bool start) => x => Math.Clamp((start ? Math.Ceiling(x * n) : Math.Floor(x * n)) / n, 0, 1);

		/// <summary>CSS's cubic-bezier: x solved for the time by Newton's method (then bisection), y given back.</summary>
		private static Func<double, double> Bezier(double x1, double y1, double x2, double y2)
		{
			double Coord(double s, double p1, double p2) => 3 * (1 - s) * (1 - s) * s * p1 + 3 * (1 - s) * s * s * p2 + s * s * s;
			double Slope(double s, double p1, double p2) => 3 * (1 - s) * (1 - s) * p1 + 6 * (1 - s) * s * (p2 - p1) + 3 * s * s * (1 - p2);
			return x =>
			{
				if (x <= 0) return 0;
				if (x >= 1) return 1;
				double s = x;
				for (int i = 0; i < 8; i++)
				{
					double d = Coord(s, x1, x2) - x, slope = Slope(s, x1, x2);
					if (Math.Abs(d) < 1e-6) break;
					if (Math.Abs(slope) < 1e-6) break;
					s -= d / slope;
				}
				if (s < 0 || s > 1 || Math.Abs(Coord(s, x1, x2) - x) > 1e-4)
				{
					double lo = 0, hi = 1;
					for (int i = 0; i < 30; i++) { s = (lo + hi) / 2; if (Coord(s, x1, x2) < x) lo = s; else hi = s; }
				}
				return Coord(s, y1, y2);
			};
		}

		/// <summary>A comma list split at its top level (not inside brackets: cubic-bezier(a, b, c, d) stays one).</summary>
		public static List<string> CommaList(string text)
		{
			List<string> items = new List<string>();
			if (string.IsNullOrWhiteSpace(text)) return items;
			int depth = 0, start = 0;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (c == '(') depth++;
				else if (c == ')') depth = Math.Max(0, depth - 1);
				else if (c == ',' && depth == 0) { items.Add(text.Substring(start, i - start).Trim()); start = i + 1; }
			}
			items.Add(text.Substring(start).Trim());
			return items.Where(s => s.Length > 0).ToList();
		}

		/// <summary>Words split at spaces at the top level (not inside brackets).</summary>
		public static List<string> Words(string text)
		{
			List<string> words = new List<string>();
			if (string.IsNullOrWhiteSpace(text)) return words;
			int depth = 0;
			StringBuilder w = new StringBuilder();
			foreach (char c in text)
			{
				if (c == '(') depth++;
				else if (c == ')') depth = Math.Max(0, depth - 1);
				if (char.IsWhiteSpace(c) && depth == 0) { if (w.Length > 0) { words.Add(w.ToString()); w.Clear(); } continue; }
				w.Append(c);
			}
			if (w.Length > 0) words.Add(w.ToString());
			return words;
		}

		// ------------------------------------------------------------------ transitions

		public sealed class Transition
		{
			public string Property;   // a property's name, or "all"
			public double Duration, Delay;
			public string Timing = "ease";
		}

		/// <summary>A frame's transitions, from its computed properties (the shorthand, then the longhands over it, list by list as CSS pairs them).</summary>
		public static List<Transition> Transitions(IReadOnlyDictionary<string, string> computed)
		{
			List<Transition> list = new List<Transition>();
			if (computed.TryGetValue("transition", out string shorthand) && !shorthand.Trim().Equals("none", StringComparison.OrdinalIgnoreCase))
			{
				foreach (string item in CommaList(shorthand))
				{
					Transition t = new Transition { Property = "all" };
					bool timeSeen = false;
					foreach (string word in Words(item))
					{
						if (Seconds(word) is double s) { if (!timeSeen) { t.Duration = s; timeSeen = true; } else t.Delay = s; }
						else if (IsTiming(word)) t.Timing = word;
						else t.Property = word.ToLowerInvariant();
					}
					list.Add(t);
				}
			}
			List<string> Longhand(string name) => computed.TryGetValue(name, out string v) ? CommaList(v) : null;
			List<string> props = Longhand("transition-property"), durations = Longhand("transition-duration"), timings = Longhand("transition-timing-function"), delays = Longhand("transition-delay");
			if (props != null)
			{
				List<Transition> old = list;
				list = props.Select((p, i) => new Transition { Property = p.Trim().ToLowerInvariant(), Duration = i < old.Count ? old[i].Duration : 0, Delay = i < old.Count ? old[i].Delay : 0, Timing = i < old.Count ? old[i].Timing : "ease" }).ToList();
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (durations != null && durations.Count > 0) list[i].Duration = Seconds(durations[i % durations.Count]) ?? list[i].Duration;
				if (timings != null && timings.Count > 0) list[i].Timing = timings[i % timings.Count];
				if (delays != null && delays.Count > 0) list[i].Delay = Seconds(delays[i % delays.Count]) ?? list[i].Delay;
			}
			return list.Where(t => t.Duration > 0 && t.Property != "none").ToList();
		}

		// ------------------------------------------------------------------ animations

		public sealed class Animation
		{
			public string Name;
			public double Duration, Delay;
			public string Timing = "ease";
			public double Count = 1;          // double.PositiveInfinity for infinite
			public string Direction = "normal";
			public string Fill = "none";
			public bool Paused;
		}

		/// <summary>A frame's animations, from its computed properties.</summary>
		public static List<Animation> Animations(IReadOnlyDictionary<string, string> computed)
		{
			List<Animation> list = new List<Animation>();
			if (computed.TryGetValue("animation", out string shorthand) && !shorthand.Trim().Equals("none", StringComparison.OrdinalIgnoreCase))
			{
				foreach (string item in CommaList(shorthand))
				{
					Animation a = new Animation();
					bool timeSeen = false;
					foreach (string word in Words(item))
					{
						string w = word.ToLowerInvariant();
						if (Seconds(w) is double s) { if (!timeSeen) { a.Duration = s; timeSeen = true; } else a.Delay = s; }
						else if (IsTiming(w)) a.Timing = w;
						else if (w == "infinite") a.Count = double.PositiveInfinity;
						else if (double.TryParse(w, NumberStyles.Float, CultureInfo.InvariantCulture, out double n)) a.Count = Math.Max(0, n);
						else if (w == "normal" || w == "reverse" || w == "alternate" || w == "alternate-reverse") a.Direction = w;
						else if (w == "none" && a.Name != null || w == "forwards" || w == "backwards" || w == "both") a.Fill = w;
						else if (w == "paused") a.Paused = true;
						else if (w == "running") a.Paused = false;
						else a.Name = word.Trim('"', '\'');
					}
					if (a.Name != null) list.Add(a);
				}
			}
			List<string> Longhand(string name) => computed.TryGetValue(name, out string v) ? CommaList(v) : null;
			List<string> names = Longhand("animation-name");
			if (names != null)
			{
				List<Animation> old = list;
				list = names.Where(n => !n.Equals("none", StringComparison.OrdinalIgnoreCase)).Select((n, i) => i < old.Count ? Named(old[i], n) : new Animation { Name = n.Trim('"', '\'') }).ToList();
			}
			void Each(string name, Action<Animation, string> put)
			{
				List<string> values = Longhand(name);
				if (values == null || values.Count == 0) return;
				for (int i = 0; i < list.Count; i++) put(list[i], values[i % values.Count].Trim().ToLowerInvariant());
			}
			Each("animation-duration", (a, v) => a.Duration = Seconds(v) ?? a.Duration);
			Each("animation-timing-function", (a, v) => a.Timing = v);
			Each("animation-delay", (a, v) => a.Delay = Seconds(v) ?? a.Delay);
			Each("animation-iteration-count", (a, v) => a.Count = v == "infinite" ? double.PositiveInfinity : double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out double n) ? Math.Max(0, n) : a.Count);
			Each("animation-direction", (a, v) => a.Direction = v);
			Each("animation-fill-mode", (a, v) => a.Fill = v);
			Each("animation-play-state", (a, v) => a.Paused = v == "paused");
			return list.Where(a => a.Duration > 0 && a.Count > 0).ToList();
		}

		private static Animation Named(Animation a, string name)
		{
			return new Animation { Name = name.Trim('"', '\''), Duration = a.Duration, Delay = a.Delay, Timing = a.Timing, Count = a.Count, Direction = a.Direction, Fill = a.Fill, Paused = a.Paused };
		}

		/// <summary>One stop of a @keyframes: where (0..1) and what it says (eased to the next by the animation's timing).</summary>
		public sealed class Keyframe
		{
			public double Offset;
			public Dictionary<string, string> Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}

		// ------------------------------------------------------------------ the animator

		/// <summary>
		/// What a screen's frames show as time goes on: handed each frame's computed properties (MenuStyles.Computed)
		/// and the time, it gives back those properties with the transitions and animations applied. It keeps, per
		/// frame, what was shown, the transitions under way and when each animation began.
		/// </summary>
		public sealed class Animator
		{
			private sealed class Tween
			{
				public string From, To;
				public double Start, Duration;
				public Func<double, double> Ease;
			}

			private sealed class Running
			{
				public string Key;     // the animation as declared: a new declaration starts it again
				public double Start;
			}

			private sealed class FrameState
			{
				public Dictionary<string, string> Target = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				public Dictionary<string, string> Shown = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				public Dictionary<string, Tween> Tweens = new Dictionary<string, Tween>(StringComparer.OrdinalIgnoreCase);
				public Dictionary<string, Running> Animations = new Dictionary<string, Running>(StringComparer.Ordinal);
			}

			private readonly Dictionary<XElement, FrameState> _frames = new Dictionary<XElement, FrameState>();
			private bool _first = true;

			/// <summary>Whether anything is moving: a transition under way, or an animation not yet done - the host steps it every frame while it is.</summary>
			public bool Active { get; private set; }

			/// <summary>
			/// The properties to show at a time (seconds, any clock that only goes forward), frame by frame. The first call
			/// only sets things down: nothing transitions from before the screen was there, but animations start.
			/// </summary>
			public Dictionary<XElement, Dictionary<string, string>> Step(IEnumerable<(XElement Frame, Dictionary<string, string> Computed)> frames, MenuStyles.Sheet sheet, double now)
			{
				Dictionary<XElement, Dictionary<string, string>> shown = new Dictionary<XElement, Dictionary<string, string>>();
				bool active = false;
				HashSet<XElement> seen = new HashSet<XElement>();
				foreach ((XElement frame, Dictionary<string, string> computed) in frames)
				{
					seen.Add(frame);
					if (!_frames.TryGetValue(frame, out FrameState state)) { state = new FrameState(); _frames[frame] = state; }
					Dictionary<string, string> result = new Dictionary<string, string>(computed, StringComparer.OrdinalIgnoreCase);

					// Transitions: a property whose value changed moves from what was shown to the new value.
					List<Transition> transitions = Transitions(computed);
					HashSet<string> names = new HashSet<string>(computed.Keys.Concat(state.Target.Keys).Where(Animatable), StringComparer.OrdinalIgnoreCase);
					foreach (string name in names)
					{
						computed.TryGetValue(name, out string to);
						state.Target.TryGetValue(name, out string before);
						if (!_first && to != before && transitions.Count > 0)
						{
							Transition t = transitions.LastOrDefault(x => x.Property == name || x.Property == "all" || Longhand(x.Property, name));
							string from = state.Shown.TryGetValue(name, out string s) ? s : before;
							if (t != null && from != null && to != null && from != to)
								state.Tweens[name] = new Tween { From = from, To = to, Start = now + t.Delay, Duration = t.Duration, Ease = Timing(t.Timing) };
							else state.Tweens.Remove(name);
						}
						else if (to != before) state.Tweens.Remove(name);
						if (to == null) state.Target.Remove(name); else state.Target[name] = to;
					}
					foreach (KeyValuePair<string, Tween> tw in state.Tweens.ToList())
					{
						Tween w = tw.Value;
						double p = w.Duration <= 0 ? 1 : (now - w.Start) / w.Duration;
						if (p >= 1) { state.Tweens.Remove(tw.Key); continue; }
						active = true;
						result[tw.Key] = p <= 0 ? w.From : Interpolate(w.From, w.To, w.Ease(p));
					}

					// Animations: each started as it is declared, over what the transitions leave.
					List<Animation> animations = Animations(computed);
					HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
					foreach (Animation a in animations)
					{
						if (sheet == null || !sheet.Keyframes.TryGetValue(a.Name, out List<Keyframe> stops) || stops.Count == 0) continue;
						string key = a.Name + "|" + a.Duration + "|" + a.Delay + "|" + a.Count + "|" + a.Direction;
						keys.Add(key);
						if (!state.Animations.TryGetValue(key, out Running run)) { run = new Running { Key = key, Start = now }; state.Animations[key] = run; }
						double elapsed = now - run.Start - a.Delay;
						bool done = !double.IsInfinity(a.Count) && elapsed >= a.Duration * a.Count;
						if (!done && !a.Paused) active = true;
						double? progress = Progress(a, elapsed);
						if (progress == null) continue;
						foreach (KeyValuePair<string, string> v in Sample(stops, progress.Value, result, a.Timing)) result[v.Key] = v.Value;
					}
					foreach (string gone in state.Animations.Keys.Where(k => !keys.Contains(k)).ToList()) state.Animations.Remove(gone);

					state.Shown.Clear();
					foreach (KeyValuePair<string, string> kv in result) if (Animatable(kv.Key)) state.Shown[kv.Key] = kv.Value;
					shown[frame] = result;
				}
				foreach (XElement gone in _frames.Keys.Where(f => !seen.Contains(f)).ToList()) _frames.Remove(gone);
				_first = false;
				Active = active;
				return shown;
			}

			/// <summary>"border" covers "border-color", "background" "background-color" and so on: a shorthand's transition moves its longhands.</summary>
			private static bool Longhand(string shorthand, string name) => name.StartsWith(shorthand + "-", StringComparison.OrdinalIgnoreCase);

			/// <summary>Where in its keyframes an animation is (0..1, the direction applied), or null when it shows nothing (before its delay without backwards fill, after its end without forwards).</summary>
			private static double? Progress(Animation a, double elapsed)
			{
				if (elapsed < 0)
				{
					if (a.Fill != "backwards" && a.Fill != "both") return null;
					return Directed(a, 0, 0);
				}
				double total = a.Duration * a.Count;
				if (!double.IsInfinity(a.Count) && elapsed >= total)
				{
					if (a.Fill != "forwards" && a.Fill != "both") return null;
					double lastIteration = Math.Ceiling(a.Count) - 1;
					double frac = a.Count - Math.Floor(a.Count);
					return Directed(a, frac == 0 ? 1 : frac, frac == 0 ? lastIteration : Math.Floor(a.Count));
				}
				double iteration = Math.Floor(elapsed / a.Duration);
				double within = (elapsed - iteration * a.Duration) / a.Duration;
				return Directed(a, within, iteration);
			}

			private static double Directed(Animation a, double p, double iteration)
			{
				bool odd = ((long)iteration % 2) == 1;
				switch (a.Direction)
				{
					case "reverse": return 1 - p;
					case "alternate": return odd ? 1 - p : p;
					case "alternate-reverse": return odd ? p : 1 - p;
					default: return p;
				}
			}

			/// <summary>The keyframes' values at a progress: each property between the stops around it that give it (from and to fall back on the frame's own), eased by the animation's timing.</summary>
			private static Dictionary<string, string> Sample(List<Keyframe> stops, double progress, Dictionary<string, string> under, string timing)
			{
				Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				Func<double, double> ease = Timing(timing);
				foreach (string name in stops.SelectMany(s => s.Values.Keys).Distinct(StringComparer.OrdinalIgnoreCase).Where(Animatable))
				{
					List<(double Offset, string Value)> points = stops.Where(s => s.Values.ContainsKey(name)).Select(s => (s.Offset, s.Values[name])).OrderBy(p => p.Offset).ToList();
					under.TryGetValue(name, out string own);
					if (points.Count == 0) continue;
					if (points[0].Offset > 0 && own != null) points.Insert(0, (0, own));
					if (points[points.Count - 1].Offset < 1 && own != null) points.Add((1, own));
					(double o0, string v0) = points[0];
					if (progress <= o0) { values[name] = v0; continue; }
					bool set = false;
					for (int i = 1; i < points.Count; i++)
					{
						(double o1, string v1) = points[i];
						if (progress <= o1)
						{
							double local = o1 - o0 <= 0 ? 1 : (progress - o0) / (o1 - o0);
							values[name] = Interpolate(v0, v1, ease(local));
							set = true;
							break;
						}
						(o0, v0) = (o1, v1);
					}
					if (!set) values[name] = points[points.Count - 1].Value;
				}
				return values;
			}
		}
	}
}
