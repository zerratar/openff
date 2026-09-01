// Parser for the script language: tokens in, syntax tree out.
//
// The grammar, in full:
//
//   file        = item*
//   item        = "map" number
//               | "cast" number "{" entry* "}"
//               | "function" number "=" label
//               | label ":"
//               | "data" number ("," number)*
//               | mnemonic [ argument ("," argument)* ]
//   entry       = ("init" | "main" | "exit") "=" (label | "none")
//   argument    = number | string | label
//
// One statement per line. Labels may share a line with the statement that follows
// them, so "loop: wait 10" is legal, and so is a label on its own line.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace FF3.ContentTool.Ffs
{
	internal sealed class Parser
	{
		private readonly List<Token> _tokens;
		private int _at;

		private Parser(List<Token> tokens)
		{
			_tokens = tokens;
		}

		public static ScriptDocument Parse(string source)
		{
			return new Parser(Lexer.Tokenize(source)).ParseDocument();
		}

		private Token Current => _tokens[_at];

		private Token Take()
		{
			return _tokens[_at++];
		}

		private bool TakeIf(TokenKind kind, string text)
		{
			if (Current.Is(kind, text))
			{
				_at++;
				return true;
			}
			return false;
		}

		private Token Expect(TokenKind kind, string what)
		{
			if (Current.Kind != kind)
			{
				throw Error("expected " + what + ", found " + Current);
			}
			return Take();
		}

		private void ExpectPunctuation(string text)
		{
			if (!TakeIf(TokenKind.Punctuation, text))
			{
				throw Error("expected '" + text + "', found " + Current);
			}
		}

		private ScriptSyntaxException Error(string message)
		{
			return new ScriptSyntaxException(message, Current.Line, Current.Column);
		}

		private void SkipNewLines()
		{
			while (Current.Kind == TokenKind.NewLine)
			{
				_at++;
			}
		}

		private void EndOfStatement()
		{
			if (TakeIf(TokenKind.Punctuation, ";"))
			{
				return;
			}
			if (Current.Kind == TokenKind.NewLine || Current.Kind == TokenKind.End)
			{
				return;
			}
			throw Error("expected the end of the line, found " + Current);
		}

		private ScriptDocument ParseDocument()
		{
			ScriptDocument document = new ScriptDocument();
			bool sawMap = false;

			while (true)
			{
				SkipNewLines();
				if (Current.Kind == TokenKind.End)
				{
					break;
				}

				Token token = Current;
				if (token.Kind != TokenKind.Identifier)
				{
					throw Error("expected a statement, found " + token);
				}

				// A label is an identifier followed by a colon.
				if (_tokens[_at + 1].Is(TokenKind.Punctuation, ":"))
				{
					_at += 2;
					document.Code.Add(new LabelItem { Name = token.Text, Token = token });
					continue;
				}

				switch (token.Text)
				{
					case "map":
						if (sawMap)
						{
							throw Error("map is declared twice");
						}
						_at++;
						document.Map = (int)Expect(TokenKind.Number, "the map number").Value;
						sawMap = true;
						EndOfStatement();
						continue;

					case "cast":
						document.Casts.Add(ParseCast());
						continue;

					case "function":
						document.Functions.Add(ParseFunction());
						continue;

					case "data":
						document.Code.Add(ParseData());
						continue;

					default:
						document.Code.Add(ParseInstruction());
						continue;
				}
			}

			return document;
		}

		private CastDeclaration ParseCast()
		{
			Token token = Take();                       // cast
			CastDeclaration cast = new CastDeclaration
			{
				Number = Expect(TokenKind.Number, "the cast number").Value,
				Token = token
			};

			ExpectPunctuation("{");
			while (true)
			{
				SkipNewLines();
				if (TakeIf(TokenKind.Punctuation, "}"))
				{
					break;
				}

				Token entry = Expect(TokenKind.Identifier, "init, main or exit");
				ExpectPunctuation("=");
				EntryTarget label = ParseTarget();

				switch (entry.Text)
				{
					case "init":
						cast.Init = label;
						break;
					case "main":
						cast.Main = label;
						break;
					case "exit":
						cast.Exit = label;
						break;
					default:
						throw new ScriptSyntaxException(
							"a cast has init, main and exit, not '" + entry.Text + "'",
							entry.Line, entry.Column);
				}
			}
			EndOfStatement();
			return cast;
		}

		private FunctionDeclaration ParseFunction()
		{
			Token token = Take();                       // function
			FunctionDeclaration function = new FunctionDeclaration
			{
				Id = Expect(TokenKind.Number, "the function id").Value,
				Token = token
			};
			ExpectPunctuation("=");
			function.Target = ParseTarget();
			EndOfStatement();
			return function;
		}

		/// <summary>A label, the word none, or a bare address.</summary>
		private EntryTarget ParseTarget()
		{
			if (Current.Kind == TokenKind.Number)
			{
				return new EntryTarget { Address = Take().Value };
			}
			Token target = Expect(TokenKind.Identifier, "a label, an address or 'none'");
			return target.Text == "none"
				? EntryTarget.None
				: new EntryTarget { Label = target.Text };
		}

		private DataItem ParseData()
		{
			Token token = Take();                       // data
			DataItem data = new DataItem { Token = token };
			do
			{
				Token value = Expect(TokenKind.Number, "a byte");
				if (value.Value < 0 || value.Value > 255)
				{
					throw new ScriptSyntaxException(
						"data takes bytes, and " + value.Value + " is not one",
						value.Line, value.Column);
				}
				data.Bytes.Add((byte)value.Value);
			}
			while (TakeIf(TokenKind.Punctuation, ","));
			EndOfStatement();
			return data;
		}

		private InstructionItem ParseInstruction()
		{
			Token token = Take();
			InstructionItem instruction = new InstructionItem
			{
				Mnemonic = token.Text,
				Token = token
			};

			// op(123) addresses an opcode by number, for anything without a name.
			if (token.Text == "op" && Current.Is(TokenKind.Punctuation, "("))
			{
				_at++;
				instruction.Opcode = (int)Expect(TokenKind.Number, "an opcode number").Value;
				ExpectPunctuation(")");
			}

			if (Current.Kind != TokenKind.NewLine && Current.Kind != TokenKind.End
				&& !Current.Is(TokenKind.Punctuation, ";"))
			{
				do
				{
					instruction.Arguments.Add(ParseArgument());
				}
				while (TakeIf(TokenKind.Punctuation, ","));
			}

			EndOfStatement();
			return instruction;
		}

		private Argument ParseArgument()
		{
			Token token = Current;
			switch (token.Kind)
			{
				case TokenKind.Number:
					_at++;
					return Argument.FromNumber(token.Value, token);
				case TokenKind.String:
					_at++;
					return Argument.FromString(token.Text, token);
				case TokenKind.Identifier:
					_at++;
					return Argument.FromLabel(token.Text, token);
				default:
					throw Error("expected a number, a string or a label, found " + token);
			}
		}
	}
}
