// Tokenizer for the script language.
//
// Statements end at a semicolon and blocks are braced, so newlines are whitespace and
// a statement can be laid out however reads best. Everything else is ordinary:
// identifiers, decimal and hex numbers, quoted strings, and punctuation - including
// the two character operators, which have to be matched before the one character ones
// or "==" lexes as two assignments.
//
// Errors carry a line and column, because a modder editing a 4000 line script needs
// to be told where the problem is, not that there is one.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Crystal.Ffs
{
	internal enum TokenKind
	{
		Identifier,
		Number,
		String,
		Punctuation,
		End
	}

	internal readonly struct Token
	{
		public readonly TokenKind Kind;
		public readonly string Text;
		public readonly long Value;      // numbers only
		public readonly int Line;
		public readonly int Column;

		public Token(TokenKind kind, string text, long value, int line, int column)
		{
			Kind = kind;
			Text = text;
			Value = value;
			Line = line;
			Column = column;
		}

		public bool Is(TokenKind kind, string text)
		{
			return Kind == kind && string.Equals(Text, text, StringComparison.Ordinal);
		}

		public override string ToString()
		{
			return Kind == TokenKind.End ? "the end of the file" : "'" + Text + "'";
		}
	}

	/// <summary>A problem in the source, with the place it happened.</summary>
	internal sealed class ScriptSyntaxException : Exception
	{
		public int Line { get; }
		public int Column { get; }

		/// <summary>The message on its own, for callers that show the place themselves.</summary>
		public string Detail { get; }

		public ScriptSyntaxException(string message, int line, int column)
			: base(string.Format(CultureInfo.InvariantCulture,
				"line {0}, column {1}: {2}", line, column, message))
		{
			Line = line;
			Column = column;
			Detail = message;
		}
	}

	internal static class Lexer
	{
		private const string Punctuation = "{}(),:=;!<>";

		/// <summary>Matched before the single characters, so == is one token.</summary>
		private static readonly string[] Pairs = { "==", "!=", "<=", ">=" };

		public static List<Token> Tokenize(string source)
		{
			List<Token> tokens = new List<Token>();
			int i = 0;
			int line = 1;
			int lineStart = 0;

			int Column(int at) => at - lineStart + 1;

			while (i < source.Length)
			{
				char c = source[i];

				// Newlines only advance the line counter now: statements end at a
				// semicolon, so where they sit on the page is nobody's business.
				if (c == '\n')
				{
					i++;
					line++;
					lineStart = i;
					continue;
				}
				if (c == '\r' || c == ' ' || c == '\t')
				{
					i++;
					continue;
				}

				// // to end of line
				if (c == '/' && i + 1 < source.Length && source[i + 1] == '/')
				{
					while (i < source.Length && source[i] != '\n')
					{
						i++;
					}
					continue;
				}

				// /* ... */, which may span lines
				if (c == '/' && i + 1 < source.Length && source[i + 1] == '*')
				{
					int startLine = line;
					int startColumn = Column(i);
					i += 2;
					while (true)
					{
						if (i + 1 >= source.Length)
						{
							throw new ScriptSyntaxException(
								"block comment is never closed", startLine, startColumn);
						}
						if (source[i] == '*' && source[i + 1] == '/')
						{
							i += 2;
							break;
						}
						if (source[i] == '\n')
						{
							line++;
							lineStart = i + 1;
						}
						i++;
					}
					continue;
				}

				if (c == '"')
				{
					int startColumn = Column(i);
					i++;
					StringBuilder text = new StringBuilder();
					while (true)
					{
						if (i >= source.Length || source[i] == '\n')
						{
							throw new ScriptSyntaxException(
								"string is never closed", line, startColumn);
						}
						if (source[i] == '"')
						{
							i++;
							break;
						}
						if (source[i] == '\\' && i + 1 < source.Length)
						{
							text.Append(Unescape(source[i + 1], line, Column(i)));
							i += 2;
							continue;
						}
						text.Append(source[i]);
						i++;
					}
					tokens.Add(new Token(TokenKind.String, text.ToString(), 0, line, startColumn));
					continue;
				}

				if (char.IsDigit(c) || (c == '-' && i + 1 < source.Length && char.IsDigit(source[i + 1])))
				{
					int start = i;
					int startColumn = Column(i);
					bool negative = c == '-';
					if (negative)
					{
						i++;
					}

					long value;
					if (source[i] == '0' && i + 1 < source.Length
						&& (source[i + 1] == 'x' || source[i + 1] == 'X'))
					{
						i += 2;
						int digits = i;
						while (i < source.Length && Uri.IsHexDigit(source[i]))
						{
							i++;
						}
						if (i == digits)
						{
							throw new ScriptSyntaxException(
								"0x with no digits after it", line, startColumn);
						}
						value = (long)ulong.Parse(source.Substring(digits, i - digits),
							NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					}
					else
					{
						int digits = i;
						while (i < source.Length && char.IsDigit(source[i]))
						{
							i++;
						}
						value = long.Parse(source.Substring(digits, i - digits),
							CultureInfo.InvariantCulture);
					}

					tokens.Add(new Token(TokenKind.Number, source.Substring(start, i - start),
						negative ? -value : value, line, startColumn));
					continue;
				}

				if (char.IsLetter(c) || c == '_' || c == '.')
				{
					int start = i;
					int startColumn = Column(i);
					while (i < source.Length
						&& (char.IsLetterOrDigit(source[i]) || source[i] == '_' || source[i] == '.'))
					{
						i++;
					}
					tokens.Add(new Token(TokenKind.Identifier, source.Substring(start, i - start),
						0, line, startColumn));
					continue;
				}

				bool paired = false;
				foreach (string pair in Pairs)
				{
					if (i + 1 < source.Length && source[i] == pair[0] && source[i + 1] == pair[1])
					{
						tokens.Add(new Token(TokenKind.Punctuation, pair, 0, line, Column(i)));
						i += 2;
						paired = true;
						break;
					}
				}
				if (paired)
				{
					continue;
				}

				if (Punctuation.IndexOf(c) >= 0)
				{
					tokens.Add(new Token(TokenKind.Punctuation, c.ToString(), 0, line, Column(i)));
					i++;
					continue;
				}

				throw new ScriptSyntaxException(
					"unexpected character '" + c + "'", line, Column(i));
			}

			tokens.Add(new Token(TokenKind.End, string.Empty, 0, line, Column(i)));
			return tokens;
		}

		private static char Unescape(char c, int line, int column)
		{
			switch (c)
			{
				case 'n': return '\n';
				case 't': return '\t';
				case 'r': return '\r';
				case '0': return '\0';
				case '"': return '"';
				case '\\': return '\\';
				default:
					throw new ScriptSyntaxException(
						"unknown escape \\" + c, line, column);
			}
		}
	}
}
