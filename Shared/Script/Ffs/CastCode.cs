// A cast's code as the mod writes it - the main function's lines, one per entry, as the
// disassembly writes them - made into a script the compiler takes: one map, a cast block
// per cast, the lines under castN_main. The client compiles it this way to run it
// (Compat/LegacyScripts.cs); Crystal compiles it the same way to check it while the
// modder types, so a problem shows in the editor with the line it is on, not in the log
// at the next Play.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using OpenFF.Script;

namespace Crystal.Ffs
{
	/// <summary>A problem in a cast's lines: which cast, which of its lines (1-based; 0 when it is the frame's), the words.</summary>
	internal sealed class CastCodeProblem
	{
		public int Cast;
		public int Line;
		public int Column;
		public string Message;

		public override string ToString()
		{
			return Line > 0
				? string.Format(CultureInfo.InvariantCulture, "cast {0} line {1}: {2}", Cast, Line, Message)
				: string.Format(CultureInfo.InvariantCulture, "cast {0}: {1}", Cast, Message);
		}
	}

	internal static class CastCode
	{
		/// <summary>The map number the mod's script is registered under: above every map the game has.</summary>
		public const uint ModMap = 60000;

		/// <summary>"@me" in a line is the cast itself: an object of the mod's own has no number to write.</summary>
		public static string[] Substitute(int cast, IEnumerable<string> lines)
		{
			string number = cast.ToString(CultureInfo.InvariantCulture);
			return (lines ?? Enumerable.Empty<string>()).Where(l => l != null).Select(l => l.TrimEnd().Replace("@me", number)).ToArray();
		}

		/// <summary>The source for the casts' lines, and for each source line (1-based) the cast and the cast's line it came from.</summary>
		public static string Source(IReadOnlyDictionary<int, string[]> code, out Dictionary<int, (int cast, int line)> origin, out CastCodeProblem refused)
		{
			origin = new Dictionary<int, (int, int)>();
			refused = null;
			StringBuilder source = new StringBuilder();
			int at = 1;
			void Emit(string text) { source.Append(text).Append('\n'); at++; }

			Emit("map " + ModMap.ToString(CultureInfo.InvariantCulture) + ";");
			foreach (int cast in code.Keys.OrderBy(k => k))
			{
				Emit("cast " + cast.ToString(CultureInfo.InvariantCulture) + " {");
				Emit("    init = none;");
				Emit("    main = cast" + cast.ToString(CultureInfo.InvariantCulture) + "_main;");
				Emit("    exit = none;");
				Emit("}");
			}
			foreach (int cast in code.Keys.OrderBy(k => k))
			{
				Emit("");
				Emit("cast" + cast.ToString(CultureInfo.InvariantCulture) + "_main:");
				bool ends = false;
				string[] lines = code[cast] ?? Array.Empty<string>();
				for (int i = 0; i < lines.Length; i++)
				{
					string t = (lines[i] ?? "").Trim();
					if (t.Length == 0) continue;
					if (t.StartsWith("call(", StringComparison.Ordinal) && !t.StartsWith("call(2,", StringComparison.Ordinal))
					{
						// A call into the map's own script: the mod's script is registered on its own
						// and has no function of the map's to reach.
						if (refused == null) refused = new CastCodeProblem { Cast = cast, Line = i + 1, Message = "calls a function of the map's own script (" + t + ") - not in the mod's; only the library's (call(2, ...)) is" };
					}
					origin[at] = (cast, i + 1);
					// A label stands at the margin; a command is indented, as the disassembly writes them.
					Emit(t.EndsWith(":", StringComparison.Ordinal) && !t.Contains("(") ? t : "    " + t);
					if (t.StartsWith("end(", StringComparison.Ordinal)) ends = true;
				}
				if (!ends) Emit("    end();");
			}
			return source.ToString();
		}

		/// <summary>Compiles the casts' lines; the bytes, or null with the problems (the first refusal, or the compiler's, each with its cast and line).</summary>
		public static byte[] Compile(IReadOnlyDictionary<int, string[]> code, ScriptOpTable table, out List<CastCodeProblem> problems)
		{
			problems = new List<CastCodeProblem>();
			string source = Source(code, out Dictionary<int, (int cast, int line)> origin, out CastCodeProblem refused);
			if (refused != null) { problems.Add(refused); return null; }
			try
			{
				ScriptDocument document = Parser.Parse(source);
				byte[] bytes = Compiler.Compile(document, table);
				if (bytes == null) problems.Add(new CastCodeProblem { Cast = code.Keys.FirstOrDefault(), Message = "the compiler gave nothing back" });
				return bytes;
			}
			catch (ScriptCompileException ex)
			{
				foreach (Diagnostic d in ex.Diagnostics) problems.Add(Place(d.Line, d.Column, d.Message, origin, code));
				return null;
			}
			catch (ScriptSyntaxException ex)
			{
				problems.Add(Place(ex.Line, ex.Column, ex.Detail, origin, code));
				return null;
			}
			catch (Exception ex)
			{
				problems.Add(new CastCodeProblem { Cast = code.Keys.FirstOrDefault(), Message = ex.GetType().Name + ": " + ex.Message });
				return null;
			}
		}

		private static CastCodeProblem Place(int sourceLine, int column, string message, Dictionary<int, (int cast, int line)> origin, IReadOnlyDictionary<int, string[]> code)
		{
			if (origin.TryGetValue(sourceLine, out (int cast, int line) where))
				return new CastCodeProblem { Cast = where.cast, Line = where.line, Column = Math.Max(0, column - 4), Message = message };
			// A line the frame added (the missing end(), the end of the file after an unfinished
			// command): the modder's last line before it is where the trouble is.
			KeyValuePair<int, (int cast, int line)> before = origin.Where(p => p.Key < sourceLine).OrderByDescending(p => p.Key).FirstOrDefault();
			if (before.Key > 0) return new CastCodeProblem { Cast = before.Value.cast, Line = before.Value.line, Column = 0, Message = message };
			return new CastCodeProblem { Cast = code.Keys.FirstOrDefault(), Line = 0, Column = 0, Message = message };
		}
	}
}
