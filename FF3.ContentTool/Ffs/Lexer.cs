// Tokenizer for the script language.
//
// Line structure matters - one statement per line - so newlines are tokens rather
// than whitespace. Everything else is ordinary: identifiers, decimal and hex numbers,
// quoted strings, and a handful of punctuation marks.
//
// Errors carry a line and column, because a modder editing a 4000 line script needs
// to be told where the problem is, not that there is one.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FF3.ContentTool.Ffs
{
	internal enum TokenKind
	{
		Identifier,
		Number,
		String,
		Punctuation,
		NewLine,
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
			return Kind == TokenKind.NewLine ? "end of line"
				: Kind == TokenKind.End ? "end of file"
				: "'" + Text + "'";
		}
	}

	/// <summary>A problem in the source, with the place it happened.</summary>
	internal sealed class ScriptSyntaxException : Exception
	{
		public int Line { get; }
		public int Column { get; }

		public ScriptSyntaxException(string message, int line, int column)
			: base(string.Format(CultureInfo.InvariantCulture,
				"line {0}, column {1}: {2}", line, column, message))
		{
			Line = line;
			Column = column;
		}
	}

	internal static class Lexer
	{
		private const string Punctuation = "{}(),:=;";

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

				if (c == '\r')
				{
					i++;
					continue;
				}
				if (c == '\n')
				{
					tokens.Add(new Token(TokenKind.NewLine, "\n", 0, line, Column(i)));
					i++;
					line++;
					lineStart = i;
					continue;
				}
				if (c == ' ' || c == '\t')
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

				if (Punctuation.IndexOf(c) >= 0)
				{
					tokens.Add(new Token(TokenKind.Punctuation, c.ToString(), 0, line, Column(i)));
					i++;
					continue;
				}

				throw new ScriptSyntaxException(
					"unexpected character '" + c + "'", line, Column(i));
			}

			tokens.Add(new Token(TokenKind.NewLine, "\n", 0, line, Column(i)));
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
