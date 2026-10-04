// Data bindings on menu screens: a frame's text, visibility and classes worked out from the game's
// data (or a screen's own) as the screen runs, the way Unity's UI Toolkit binds a VisualElement to
// a data source by a path. In a layout:
//
//   <menu data-source="hero">                                     the screen's data: the picked hero
//     <frame bind-text="{name}   Lv {level}" ...>                 a template: {path} or {path:format}
//     <frame bind-text="HP {hp}/{maxHp}" bind-class="low: hp &lt; maxHp / 4; ko: !alive">
//     <frame data-source="party[1]" bind-text="{name}">            a frame's own data for it and its frames
//     <frame bind-visible="gil &gt;= 1000" bind-text="{gil:N0} G">
//
// and from code: widget.Bind("text", "{name}"), Menu.Data["rules"] = list, Menu.Refresh().
//
// A path is names joined by dots, with [index] for a list or a dictionary ([0], [menu.hero]):
// public properties and fields, any case. Its first name is looked for on the data source, then
// among the roots: what the screen's code put in Menu.Data, then hero (the party member the screen
// asked for), party (the members), gil, items (the bag), menu (the screen: focused, marked, hero) and this
// (the data source itself). An expression (bind-visible, a class's condition) is a path or a
// literal - a number, 'text', true, false, null - optionally compared (== != < <= > >=) with
// another, with ! in front to turn it round, and joined by && and ||; a plain value counts as true
// when it is true, a number not 0, a text not empty, anything else not null.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace OpenFF
{
	/// <summary>Where a binding's names are looked up: the data source first, then the roots.</summary>
	public sealed class MenuBindingScope
	{
		/// <summary>The data source (data-source, or null): a path's first name is looked for on it first.</summary>
		public object Source { get; set; }
		/// <summary>The roots by name (Menu.Data's, then hero, party, gil...); null for none.</summary>
		public Func<string, (bool Found, object Value)> Root { get; set; }

		public MenuBindingScope With(object source) => new MenuBindingScope { Source = source, Root = Root };
	}

	/// <summary>Paths, templates and conditions for menu bindings - the same the layouts' bind-* attributes use, for code of a mod's own.</summary>
	public static class MenuBindings
	{
		private static readonly ConcurrentDictionary<(Type, string), MemberInfo> Members = new ConcurrentDictionary<(Type, string), MemberInfo>();

		/// <summary>A path's value ("hero.name", "party[0].hp", "rules[menu.hero].count"), or null when any step of it is missing.</summary>
		public static object Resolve(string path, MenuBindingScope scope)
		{
			if (string.IsNullOrWhiteSpace(path)) return scope?.Source;
			List<string> steps = Split(path.Trim());
			if (steps.Count == 0) return null;
			object at;
			string first = steps[0];
			if (first.StartsWith("[")) at = Index(scope?.Source, first, scope);
			else if (string.Equals(first, "this", StringComparison.OrdinalIgnoreCase)) at = scope?.Source;
			else if (scope?.Source != null && TryMember(scope.Source, first, out object own)) at = own;
			else if (scope?.Root != null && scope.Root(first) is (true, object root)) at = root;
			else return null;
			for (int i = 1; i < steps.Count && at != null; i++)
			{
				string step = steps[i];
				if (step.StartsWith("[")) at = Index(at, step, scope);
				else if (!TryMember(at, step, out at)) return null;
			}
			return at;
		}

		/// <summary>A template's text: each {path} or {path:format} replaced by its value ({{ and }} for braces themselves); a path with no value writes nothing.</summary>
		public static string Format(string template, MenuBindingScope scope)
		{
			if (string.IsNullOrEmpty(template)) return "";
			StringBuilder text = new StringBuilder();
			for (int i = 0; i < template.Length; i++)
			{
				char c = template[i];
				if (c == '{' && i + 1 < template.Length && template[i + 1] == '{') { text.Append('{'); i++; continue; }
				if (c == '}' && i + 1 < template.Length && template[i + 1] == '}') { text.Append('}'); i++; continue; }
				if (c != '{') { text.Append(c); continue; }
				int close = template.IndexOf('}', i + 1);
				if (close < 0) { text.Append(template, i, template.Length - i); break; }
				string inner = template.Substring(i + 1, close - i - 1);
				i = close;
				int colon = inner.IndexOf(':');
				string path = colon >= 0 ? inner.Substring(0, colon) : inner;
				string format = colon >= 0 ? inner.Substring(colon + 1) : null;
				object value = Value(path.Trim(), scope);
				text.Append(Text(value, format));
			}
			return text.ToString();
		}

		/// <summary>An expression's truth: "alive", "!alive", "hp &lt; 100", "gil &gt;= 1000 &amp;&amp; menu.hero == 0".</summary>
		public static bool Test(string expression, MenuBindingScope scope)
		{
			if (string.IsNullOrWhiteSpace(expression)) return false;
			foreach (string any in SplitTop(expression, "||"))
			{
				bool all = true;
				foreach (string part in SplitTop(any, "&&"))
				{
					if (!TestOne(part.Trim(), scope)) { all = false; break; }
				}
				if (all) return true;
			}
			return false;
		}

		/// <summary>A value as the menus print it: a format of its own ({gil:N0}), numbers invariant, true/false as words, nothing for null.</summary>
		public static string Text(object value, string format = null)
		{
			if (value == null) return "";
			if (!string.IsNullOrEmpty(format) && value is IFormattable f) return f.ToString(format, CultureInfo.InvariantCulture);
			if (value is IFormattable g) return g.ToString(null, CultureInfo.InvariantCulture);
			return value.ToString();
		}

		/// <summary>Whether a value counts as true: a bool itself, a number not 0, a text not empty, a list not empty, anything else not null.</summary>
		public static bool Truthy(object value)
		{
			switch (value)
			{
				case null: return false;
				case bool b: return b;
				case string s: return s.Length > 0;
				case ICollection c: return c.Count > 0;
				case IConvertible n when IsNumber(n): return Convert.ToDouble(n, CultureInfo.InvariantCulture) != 0;
				default: return true;
			}
		}

		// ------------------------------------------------------------------ inside

		private static bool TestOne(string text, MenuBindingScope scope)
		{
			bool not = false;
			while (text.StartsWith("!") && !text.StartsWith("!=")) { not = !not; text = text.Substring(1).Trim(); }
			foreach (string op in new[] { "==", "!=", "<=", ">=", "<", ">" })
			{
				int at = IndexOutsideQuotes(text, op);
				if (at <= 0) continue;
				object left = Value(text.Substring(0, at).Trim(), scope);
				object right = Value(text.Substring(at + op.Length).Trim(), scope);
				return Compare(left, right, op) != not;
			}
			return Truthy(Value(text, scope)) != not;
		}

		private static bool Compare(object a, object b, string op)
		{
			if (Number(a) is double x && Number(b) is double y)
			{
				return op switch { "==" => x == y, "!=" => x != y, "<" => x < y, "<=" => x <= y, ">" => x > y, ">=" => x >= y, _ => false };
			}
			string sa = a == null ? null : Text(a), sb = b == null ? null : Text(b);
			int c = string.Compare(sa, sb, StringComparison.OrdinalIgnoreCase);
			return op switch { "==" => c == 0, "!=" => c != 0, "<" => c < 0, "<=" => c <= 0, ">" => c > 0, ">=" => c >= 0, _ => false };
		}

		/// <summary>A literal (a number, 'text' or "text", true, false, null) or a path's value, with simple sums (hp / 4, level + 1) between them.</summary>
		private static object Value(string text, MenuBindingScope scope)
		{
			text = text.Trim();
			if (text.Length == 0) return null;
			foreach (char op in new[] { '+', '-', '*', '/' })
			{
				int at = IndexOutsideQuotes(text, op.ToString(), fromEnd: true);
				if (at > 0 && at < text.Length - 1)
				{
					object l = Value(text.Substring(0, at), scope), r = Value(text.Substring(at + 1), scope);
					if (Number(l) is double x && Number(r) is double y)
					{
						double v = op == '+' ? x + y : op == '-' ? x - y : op == '*' ? x * y : (y == 0 ? 0 : x / y);
						return v == Math.Floor(v) && Math.Abs(v) < int.MaxValue ? (object)(int)v : v;
					}
					if (op == '+') return Text(l) + Text(r);
					return null;
				}
			}
			if ((text.StartsWith("'") && text.EndsWith("'") || text.StartsWith("\"") && text.EndsWith("\"")) && text.Length >= 2) return text.Substring(1, text.Length - 2);
			if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double n)) return n == Math.Floor(n) && Math.Abs(n) < int.MaxValue ? (object)(int)n : n;
			if (string.Equals(text, "true", StringComparison.OrdinalIgnoreCase)) return true;
			if (string.Equals(text, "false", StringComparison.OrdinalIgnoreCase)) return false;
			if (string.Equals(text, "null", StringComparison.OrdinalIgnoreCase)) return null;
			return Resolve(text, scope);
		}

		private static bool IsNumber(IConvertible n)
		{
			switch (n.GetTypeCode())
			{
				case TypeCode.Byte: case TypeCode.SByte: case TypeCode.Int16: case TypeCode.UInt16: case TypeCode.Int32: case TypeCode.UInt32:
				case TypeCode.Int64: case TypeCode.UInt64: case TypeCode.Single: case TypeCode.Double: case TypeCode.Decimal:
					return true;
				default:
					return false;
			}
		}

		private static double? Number(object v)
		{
			if (v is Enum e) return Convert.ToDouble(e, CultureInfo.InvariantCulture);
			if (v is IConvertible c && IsNumber(c)) return Convert.ToDouble(c, CultureInfo.InvariantCulture);
			return null;
		}

		private static bool TryMember(object on, string name, out object value)
		{
			value = null;
			if (on == null) return false;
			if (on is IDictionary<string, object> d)
			{
				foreach (KeyValuePair<string, object> kv in d) if (string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase)) { value = kv.Value; return true; }
				return false;
			}
			if (on is IDictionary nd)
			{
				foreach (DictionaryEntry kv in nd) if (string.Equals(kv.Key?.ToString(), name, StringComparison.OrdinalIgnoreCase)) { value = kv.Value; return true; }
				return false;
			}
			MemberInfo member = Members.GetOrAdd((on.GetType(), name.ToLowerInvariant()), key =>
			{
				const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
				PropertyInfo p = key.Item1.GetProperty(name, flags);
				if (p != null && p.GetIndexParameters().Length == 0) return p;
				return key.Item1.GetField(name, flags);
			});
			try
			{
				if (member is PropertyInfo prop) { value = prop.GetValue(on); return true; }
				if (member is FieldInfo field) { value = field.GetValue(on); return true; }
			}
			catch (Exception) { }
			return false;
		}

		private static object Index(object on, string step, MenuBindingScope scope)
		{
			if (on == null) return null;
			string inner = step.Substring(1, step.Length - 2).Trim();
			object key = Value(inner, scope);
			if (on is IDictionary nd)
			{
				foreach (DictionaryEntry kv in nd) if (Equals(kv.Key, key) || string.Equals(kv.Key?.ToString(), Text(key), StringComparison.OrdinalIgnoreCase)) return kv.Value;
				return null;
			}
			if (Number(key) is double n && on is IList list)
			{
				int i = (int)n;
				return i >= 0 && i < list.Count ? list[i] : null;
			}
			if (Number(key) is double m && on is IEnumerable seq && !(on is string))
			{
				int i = (int)m, at = 0;
				foreach (object item in seq) { if (at++ == i) return item; }
				return null;
			}
			return key is string name && TryMember(on, name, out object v) ? v : null;
		}

		/// <summary>"a.b[0].c[x.y]" as its steps: "a", "b", "[0]", "c", "[x.y]".</summary>
		private static List<string> Split(string path)
		{
			List<string> steps = new List<string>();
			StringBuilder name = new StringBuilder();
			for (int i = 0; i < path.Length; i++)
			{
				char c = path[i];
				if (c == '.') { if (name.Length > 0) steps.Add(name.ToString()); name.Clear(); }
				else if (c == '[')
				{
					if (name.Length > 0) steps.Add(name.ToString());
					name.Clear();
					int depth = 0, j = i;
					for (; j < path.Length; j++) { if (path[j] == '[') depth++; else if (path[j] == ']' && --depth == 0) break; }
					steps.Add(path.Substring(i, Math.Min(j, path.Length - 1) - i + 1));
					i = j;
				}
				else if (!char.IsWhiteSpace(c)) name.Append(c);
			}
			if (name.Length > 0) steps.Add(name.ToString());
			return steps;
		}

		private static IEnumerable<string> SplitTop(string text, string by)
		{
			int start = 0;
			while (true)
			{
				int at = IndexOutsideQuotes(text.Substring(start), by);
				if (at < 0) { yield return text.Substring(start); yield break; }
				yield return text.Substring(start, at);
				start += at + by.Length;
			}
		}

		private static int IndexOutsideQuotes(string text, string what, bool fromEnd = false)
		{
			int found = -1;
			char quote = '\0';
			int depth = 0;
			for (int i = 0; i + what.Length <= text.Length; i++)
			{
				char c = text[i];
				if (quote != '\0') { if (c == quote) quote = '\0'; continue; }
				if (c == '\'' || c == '"') { quote = c; continue; }
				if (c == '[' || c == '(') depth++;
				else if (c == ']' || c == ')') depth--;
				if (depth == 0 && string.CompareOrdinal(text, i, what, 0, what.Length) == 0)
				{
					// A minus that starts a number (or follows another operator) is its sign, not a sum.
					if (what == "-" && (i == 0 || "+-*/<>=!&|".IndexOf(text[i - 1]) >= 0 || char.IsWhiteSpace(text[i - 1]) && i >= 2 && "+-*/<>=!&|".IndexOf(text[i - 2]) >= 0)) continue;
					if ((what == "<" || what == ">") && i + 1 < text.Length && text[i + 1] == '=') continue;
					if ((what == "<" || what == ">" || what == "=") && i > 0 && (text[i - 1] == '<' || text[i - 1] == '>' || text[i - 1] == '!' || text[i - 1] == '=') && what != "==") continue;
					found = i;
					if (!fromEnd) return found;
				}
			}
			return found;
		}
	}
}
