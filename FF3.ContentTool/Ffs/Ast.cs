// The syntax tree for the script language.
//
// Deliberately close to what the machine does: a script is a map number, a table of
// casts, a table of functions, and a flat run of labels and instructions. Casts and
// functions point at labels rather than owning blocks, because that is the only shape
// that can express every shipped script - entry points share addresses, and code runs
// on past the end of the block that "owns" it.
//
// Structured sugar can be lowered onto this later without changing the compiler: an
// if/else or a loop is a matter of generating labels and jumps, and this is what those
// would generate.

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

	/// <summary>Anything that occupies a position in the code stream.</summary>
	internal abstract class CodeItem
	{
		public Token Token;
	}

	internal sealed class LabelItem : CodeItem
	{
		public string Name;
	}

	internal sealed class InstructionItem : CodeItem
	{
		public string Mnemonic;
		public int Opcode = -1;                  // resolved by the compiler
		public List<Argument> Arguments = new List<Argument>();
	}

	/// <summary>Raw bytes: the few places where the stream is not instructions.</summary>
	internal sealed class DataItem : CodeItem
	{
		public List<byte> Bytes = new List<byte>();
	}

	/// <summary>
	/// Where an entry point goes: a label, nothing at all, or a bare address.
	/// The last one exists because global.script points its only cast at address 0,
	/// which is outside the code and so has no label to refer to.
	/// </summary>
	internal sealed class EntryTarget
	{
		public string Label;
		public long? Address;

		public static readonly EntryTarget None = new EntryTarget();

		public bool IsNone => Label == null && Address == null;
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
		public EntryTarget Target = EntryTarget.None;
		public Token Token;
	}

	internal sealed class ScriptDocument
	{
		public int Map;
		public List<CastDeclaration> Casts = new List<CastDeclaration>();
		public List<FunctionDeclaration> Functions = new List<FunctionDeclaration>();
		public List<CodeItem> Code = new List<CodeItem>();
	}
}
