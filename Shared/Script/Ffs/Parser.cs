// Parser for the script language: tokens in, syntax tree out.
//
// The grammar, in full:
//
//   file        = declaration*
//   declaration = "map" number ";"
//               | "cast" number "{" entry* "}"
//               | "function" number "=" target ";"          // an existing table entry
//               | "func" name "(" ")" block                 // a new one
//               | "extern" name "(" ")" "=" number "," number ";"
//               | statement
//   entry       = ("init" | "main" | "exit") ( "=" target ";" | block )
//   target      = label | number | "none"
//
//   statement   = label ":"
//               | "if" "(" condition ")" block [ "else" (block | if) ]
//               | "while" "(" condition ")" block
//               | "do" block "while" "(" condition ")" ";"
//               | "for" "(" [call] ";" [condition] ";" [call] ")" block
//               | "goto" label ";"
//               | "break" ";" | "continue" ";"
//               | "data" number ("," number)* ";"
//               | name "(" [ argument ("," argument)* ] ")" ";"
//               | block
//
//   condition   = "!" condition
//               | "value" "(" argument "," argument ")" compare argument
//               | name "(" [ argument ("," argument)* ] ")"
//   argument    = number | string | label
//
// Statements end at a semicolon and blocks are braced, so a statement can be laid out
// over as many lines as reads well.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Crystal.Ffs
{
	internal sealed class Parser
	{
		private static readonly HashSet<string> Keywords = new HashSet<string>(StringComparer.Ordinal)
		{
			"map", "cast", "function", "func", "extern", "data",
			"if", "else", "while", "do", "for", "goto", "break", "continue",
			"init", "main", "exit", "none"
		};

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

		private Token Next => _tokens[Math.Min(_at + 1, _tokens.Count - 1)];

		private Token Take()
		{
			return _tokens[_at++];
		}

		private bool TakeIf(string text)
		{
			if (Current.Is(TokenKind.Punctuation, text))
			{
				_at++;
				return true;
			}
			return false;
		}

		private bool TakeWord(string word)
		{
			if (Current.Kind == TokenKind.Identifier
				&& string.Equals(Current.Text, word, StringComparison.Ordinal))
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
			if (!TakeIf(text))
			{
				throw Error("expected '" + text + "', found " + Current);
			}
		}

		private ScriptSyntaxException Error(string message)
		{
			return new ScriptSyntaxException(message, Current.Line, Current.Column);
		}

		// ------------------------------------------------------------- declarations

		private ScriptDocument ParseDocument()
		{
			ScriptDocument document = new ScriptDocument();
			bool sawMap = false;

			while (Current.Kind != TokenKind.End)
			{
				Token token = Current;
				if (token.Kind != TokenKind.Identifier)
				{
					throw Error("expected a declaration or a statement, found " + token);
				}

				switch (token.Text)
				{
					case "map" when !Next.Is(TokenKind.Punctuation, "("):
						if (sawMap)
						{
							throw Error("map is declared twice");
						}
						_at++;
						document.Map = (int)Expect(TokenKind.Number, "the map number").Value;
						ExpectPunctuation(";");
						sawMap = true;
						continue;

					case "cast" when !Next.Is(TokenKind.Punctuation, "("):
						document.Casts.Add(ParseCast());
						continue;

					case "function" when !Next.Is(TokenKind.Punctuation, "("):
						document.Functions.Add(ParseFunctionPointer());
						continue;

					case "func":
						document.Functions.Add(ParseFunctionBody());
						continue;

					case "extern":
						document.Externs.Add(ParseExtern());
						continue;

					default:
						document.Code.Add(ParseStatement());
						continue;
				}
			}

			return document;
		}

		private CastDeclaration ParseCast()
		{
			Token token = Take();
			CastDeclaration cast = new CastDeclaration
			{
				Number = Expect(TokenKind.Number, "the cast number").Value,
				Token = token
			};

			ExpectPunctuation("{");
			while (!TakeIf("}"))
			{
				Token entry = Expect(TokenKind.Identifier, "init, main or exit");
				EntryTarget target;

				if (Current.Is(TokenKind.Punctuation, "{"))
				{
					target = new EntryTarget { Body = ParseBlock() };
				}
				else
				{
					ExpectPunctuation("=");
					target = ParseTarget();
					ExpectPunctuation(";");
				}

				switch (entry.Text)
				{
					case "init": cast.Init = target; break;
					case "main": cast.Main = target; break;
					case "exit": cast.Exit = target; break;
					default:
						throw new ScriptSyntaxException(
							"a cast has init, main and exit, not '" + entry.Text + "'",
							entry.Line, entry.Column);
				}
			}
			return cast;
		}

		private FunctionDeclaration ParseFunctionPointer()
		{
			Token token = Take();
			FunctionDeclaration function = new FunctionDeclaration
			{
				Id = Expect(TokenKind.Number, "the function id").Value,
				HasId = true,
				Token = token
			};
			ExpectPunctuation("=");
			function.Target = ParseTarget();
			ExpectPunctuation(";");
			return function;
		}

		/// <summary>func name() { ... } - the compiler gives it an id.</summary>
		private FunctionDeclaration ParseFunctionBody()
		{
			Token token = Take();
			Token name = Expect(TokenKind.Identifier, "the function's name");
			ExpectPunctuation("(");
			ExpectPunctuation(")");

			return new FunctionDeclaration
			{
				Name = name.Text,
				Token = token,
				Target = new EntryTarget { Body = ParseBlock() }
			};
		}

		private ExternDeclaration ParseExtern()
		{
			Token token = Take();
			Token name = Expect(TokenKind.Identifier, "the name to use for it");
			ExpectPunctuation("(");
			ExpectPunctuation(")");
			ExpectPunctuation("=");
			long library = Expect(TokenKind.Number, "the library number").Value;
			ExpectPunctuation(",");
			long id = Expect(TokenKind.Number, "the function id").Value;
			ExpectPunctuation(";");

			return new ExternDeclaration
			{
				Name = name.Text,
				Library = library,
				Id = id,
				Token = token
			};
		}

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

		// --------------------------------------------------------------- statements

		private BlockStatement ParseBlock()
		{
			Token token = Current;
			ExpectPunctuation("{");
			BlockStatement block = new BlockStatement { Token = token };
			while (!TakeIf("}"))
			{
				if (Current.Kind == TokenKind.End)
				{
					throw Error("this block is never closed");
				}
				block.Statements.Add(ParseStatement());
			}
			return block;
		}

		private Statement ParseStatement()
		{
			Token token = Current;

			if (token.Is(TokenKind.Punctuation, "{"))
			{
				return ParseBlock();
			}

			if (token.Kind != TokenKind.Identifier)
			{
				throw Error("expected a statement, found " + token);
			}

			// A label: an identifier followed by a colon, and never a keyword.
			if (Next.Is(TokenKind.Punctuation, ":") && !Keywords.Contains(token.Text))
			{
				_at += 2;
				return new LabelItem { Name = token.Text, Token = token };
			}

			switch (token.Text)
			{
				case "if": return ParseIf();
				case "while": return ParseWhile();
				case "do": return ParseDoWhile();
				case "for": return ParseFor();
				case "goto": return ParseGoto();
				case "break":
				case "continue":
					_at++;
					ExpectPunctuation(";");
					return new LoopJump { IsBreak = token.Text == "break", Token = token };
				case "data": return ParseData();
				default: return ParseCallOrInstruction(true);
			}
		}

		private Statement ParseIf()
		{
			Token token = Take();
			ExpectPunctuation("(");
			Condition condition = ParseCondition();
			ExpectPunctuation(")");

			IfStatement statement = new IfStatement
			{
				Token = token,
				Condition = condition,
				Then = ParseBlock()
			};

			if (TakeWord("else"))
			{
				statement.Else = Current.Is(TokenKind.Punctuation, "{")
					? (Statement)ParseBlock()
					: ParseIf();
			}
			return statement;
		}

		private Statement ParseWhile()
		{
			Token token = Take();
			ExpectPunctuation("(");
			Condition condition = ParseCondition();
			ExpectPunctuation(")");
			return new WhileStatement
			{
				Token = token,
				Condition = condition,
				Body = ParseBlock()
			};
		}

		private Statement ParseDoWhile()
		{
			Token token = Take();
			BlockStatement body = ParseBlock();
			if (!TakeWord("while"))
			{
				throw Error("expected 'while' after the body of a do, found " + Current);
			}
			ExpectPunctuation("(");
			Condition condition = ParseCondition();
			ExpectPunctuation(")");
			ExpectPunctuation(";");
			return new DoWhileStatement { Token = token, Condition = condition, Body = body };
		}

		private Statement ParseFor()
		{
			Token token = Take();
			ExpectPunctuation("(");

			ForStatement statement = new ForStatement { Token = token };
			if (!TakeIf(";"))
			{
				statement.Initialiser = ParseCallOrInstruction(true);
			}
			if (!Current.Is(TokenKind.Punctuation, ";"))
			{
				statement.Condition = ParseCondition();
			}
			ExpectPunctuation(";");
			if (!Current.Is(TokenKind.Punctuation, ")"))
			{
				statement.Step = ParseCallOrInstruction(false);
			}
			ExpectPunctuation(")");

			statement.Body = ParseBlock();
			return statement;
		}

		private Statement ParseGoto()
		{
			Token token = Take();
			Token label = Expect(TokenKind.Identifier, "a label to go to");
			ExpectPunctuation(";");
			return new GotoStatement { Label = label.Text, Token = token };
		}

		private Statement ParseData()
		{
			Token token = Take();
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
			while (TakeIf(","));
			ExpectPunctuation(";");
			return data;
		}

		/// <summary>
		/// An instruction, or a call to a function this file declares. Which one it is
		/// cannot be decided here - a name is only known to be a function once every
		/// declaration has been seen - so both come out as an instruction and lowering
		/// sorts it out.
		/// </summary>
		private Statement ParseCallOrInstruction(bool semicolon)
		{
			Token token = Take();
			InstructionItem instruction = new InstructionItem
			{
				Mnemonic = token.Text,
				Token = token
			};

			// op(123) names an opcode by number, for the ones with no handler.
			if (token.Text == "op" && Current.Is(TokenKind.Punctuation, "("))
			{
				_at++;
				instruction.Opcode = (int)Expect(TokenKind.Number, "an opcode number").Value;
				ExpectPunctuation(")");
				if (semicolon)
				{
					ExpectPunctuation(";");
				}
				return instruction;
			}

			ExpectPunctuation("(");
			if (!Current.Is(TokenKind.Punctuation, ")"))
			{
				do
				{
					instruction.Arguments.Add(ParseArgument());
				}
				while (TakeIf(","));
			}
			ExpectPunctuation(")");
			if (semicolon)
			{
				ExpectPunctuation(";");
			}
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

		// --------------------------------------------------------------- conditions

		private Condition ParseCondition()
		{
			Token token = Current;

			if (TakeIf("!"))
			{
				return new NotCondition { Inner = ParseCondition(), Token = token };
			}

			Token name = Expect(TokenKind.Identifier, "a condition");

			if (string.Equals(name.Text, Conditions.ValueName, StringComparison.Ordinal))
			{
				ExpectPunctuation("(");
				Argument group = ParseArgument();
				ExpectPunctuation(",");
				Argument index = ParseArgument();
				ExpectPunctuation(")");

				string comparison = Current.Kind == TokenKind.Punctuation
					&& Conditions.Comparisons.ContainsKey(Current.Text)
					? Take().Text
					: throw Error("expected a comparison after value(...), found " + Current);

				return new ValueCondition
				{
					Token = name,
					Group = group,
					Index = index,
					Comparison = comparison,
					Value = ParseArgument()
				};
			}

			FormCondition form = new FormCondition { Name = name.Text, Token = name };
			ExpectPunctuation("(");
			if (!Current.Is(TokenKind.Punctuation, ")"))
			{
				do
				{
					form.Arguments.Add(ParseArgument());
				}
				while (TakeIf(","));
			}
			ExpectPunctuation(")");
			return form;
		}
	}
}
