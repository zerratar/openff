// The syntax tree for the script language.
//
// Two layers live here. The bottom one is what the machine does: labels, instructions
// and raw bytes, in the order they will be written. The top one is what people write:
// if, while, for, blocks, functions. Lowering.cs turns the second into the first, and
// the compiler only ever sees the first - which is why adding a structured statement
// never means touching code generation.
//
// Casts and functions can be written either way. A pointer form - `main = someLabel` -
// is what the disassembler emits, because it is the only shape that fits every shipped
// script: entry points share addresses and code runs past the end of the block that
// appears to own it. A block form - `main { ... }` - is what a person writes.

using System.Collections.Generic;

namespace FF3.ContentTool.Ffs
{
	internal enum ArgumentKind
	{
		Number,
		String,
		Label
	}

	internal sealed class Argument
	{
		public ArgumentKind Kind;
		public long Number;
		public string Text;
		public Token Token;

		public static Argument FromNumber(long value, Token token)
		{
			return new Argument { Kind = ArgumentKind.Number, Number = value, Token = token };
		}

		public static Argument FromString(string text, Token token)
		{
			return new Argument { Kind = ArgumentKind.String, Text = text, Token = token };
		}

		public static Argument FromLabel(string name, Token token)
		{
			return new Argument { Kind = ArgumentKind.Label, Text = name, Token = token };
		}
	}

	// ---------------------------------------------------------------- statements

	internal abstract class Statement
	{
		public Token Token;
	}

	/// <summary>A label, which anything can jump to.</summary>
	internal sealed class LabelItem : Statement
	{
		public string Name;
	}

	/// <summary>One instruction: wait(15);</summary>
	internal sealed class InstructionItem : Statement
	{
		public string Mnemonic;
		public int Opcode = -1;
		public List<Argument> Arguments = new List<Argument>();
	}

	/// <summary>Raw bytes, for the places the stream is not instructions.</summary>
	internal sealed class DataItem : Statement
	{
		public List<byte> Bytes = new List<byte>();
	}

	internal sealed class BlockStatement : Statement
	{
		public List<Statement> Statements = new List<Statement>();
	}

	internal sealed class IfStatement : Statement
	{
		public Condition Condition;
		public BlockStatement Then;
		public Statement Else;                   // a block, or another if
	}

	internal sealed class WhileStatement : Statement
	{
		public Condition Condition;
		public BlockStatement Body;
	}

	internal sealed class DoWhileStatement : Statement
	{
		public Condition Condition;
		public BlockStatement Body;
	}

	internal sealed class ForStatement : Statement
	{
		public Statement Initialiser;            // an instruction, or null
		public Condition Condition;              // or null, meaning forever
		public Statement Step;                   // an instruction, or null
		public BlockStatement Body;
	}

	internal sealed class GotoStatement : Statement
	{
		public string Label;
	}

	/// <summary>break or continue - which one is in Token.Text.</summary>
	internal sealed class LoopJump : Statement
	{
		public bool IsBreak;
	}

	/// <summary>A call to a function declared in this file or named by extern.</summary>
	internal sealed class CallStatement : Statement
	{
		public string Name;
	}

	// ---------------------------------------------------------------- conditions

	internal abstract class Condition
	{
		public Token Token;
	}

	/// <summary>flag(0, 986), touch(), button() - one of the On/Off jump pairs.</summary>
	internal sealed class FormCondition : Condition
	{
		public string Name;
		public List<Argument> Arguments = new List<Argument>();
	}

	/// <summary>value(0, 3) &lt; 4</summary>
	internal sealed class ValueCondition : Condition
	{
		public Argument Group;
		public Argument Index;
		public string Comparison;
		public Argument Value;
	}

	internal sealed class NotCondition : Condition
	{
		public Condition Inner;
	}

	// -------------------------------------------------------------- declarations

	/// <summary>
	/// Where an entry point goes: a label, nothing, a bare address, or a block whose
	/// code the compiler places and labels itself.
	/// </summary>
	internal sealed class EntryTarget
	{
		public string Label;
		public long? Address;
		public BlockStatement Body;

		public static readonly EntryTarget None = new EntryTarget();

		public bool IsNone => Label == null && Address == null && Body == null;
	}

	internal sealed class CastDeclaration
	{
		public long Number;
		public EntryTarget Init = EntryTarget.None;
		public EntryTarget Main = EntryTarget.None;
		public EntryTarget Exit = EntryTarget.None;
		public Token Token;
	}

	internal sealed class FunctionDeclaration
	{
		public long Id;
		public bool HasId;                       // false when the compiler assigns one
		public string Name;                      // for `func name() { }`
		public EntryTarget Target = EntryTarget.None;
		public Token Token;
	}

	/// <summary>A name for a function that lives somewhere else, usually the global script.</summary>
	internal sealed class ExternDeclaration
	{
		public string Name;
		public long Library;
		public long Id;
		public Token Token;
	}

	internal sealed class ScriptDocument
	{
		public int Map;
		public List<CastDeclaration> Casts = new List<CastDeclaration>();
		public List<FunctionDeclaration> Functions = new List<FunctionDeclaration>();
		public List<ExternDeclaration> Externs = new List<ExternDeclaration>();
		public List<Statement> Code = new List<Statement>();
	}
}
