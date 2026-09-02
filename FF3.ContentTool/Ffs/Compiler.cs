// Compiler for the script language: syntax tree in, a .script file out.
//
// Two passes. The first walks the code assigning an address to every instruction and
// label, which it can do because every instruction has a fixed length. The second
// emits the bytes with label references resolved. The tables are then written around
// the code: casts before it, functions after.
//
// Problems are collected rather than thrown at the first one. Someone editing a
// script wants to hear about all six mistakes, not to fix one and run again.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FF3.ContentTool.Ffs
{
	internal sealed class Diagnostic
	{
		public readonly string Message;
		public readonly int Line;
		public readonly int Column;

		public Diagnostic(string message, Token token)
		{
			Message = message;
			Line = token.Line;
			Column = token.Column;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture,
				"line {0}, column {1}: {2}", Line, Column, Message);
		}
	}

	internal sealed class ScriptCompileException : Exception
	{
		public IReadOnlyList<Diagnostic> Diagnostics { get; }

		public ScriptCompileException(IReadOnlyList<Diagnostic> diagnostics)
			: base(string.Format(CultureInfo.InvariantCulture,
				"{0} problem(s):{1}{2}", diagnostics.Count, Environment.NewLine,
				string.Join(Environment.NewLine, diagnostics.Select(d => "  " + d))))
		{
			Diagnostics = diagnostics;
		}
	}

	internal static class Compiler
	{
		private const int HeaderSize = 16;
		private const uint NoScript = 0xFFFFFFFF;

		public static byte[] Compile(ScriptDocument document)
		{
			List<Diagnostic> problems = new List<Diagnostic>();

			// if, while, for and named functions stop existing here. Everything below
			// this line sees nothing but labels, instructions and bytes.
			Lowering.Flatten(document, problems);

			uint codeStart = (uint)(HeaderSize + document.Casts.Count * 16);

			Dictionary<string, uint> labels = Layout(document, codeStart, problems);
			byte[] code = Emit(document, labels, problems);

			uint functionTable = codeStart + (uint)code.Length;
			foreach (CastDeclaration cast in document.Casts)
			{
				Resolve(cast.Init, labels, cast.Token, problems);
				Resolve(cast.Main, labels, cast.Token, problems);
				Resolve(cast.Exit, labels, cast.Token, problems);
			}
			foreach (FunctionDeclaration function in document.Functions)
			{
				Resolve(function.Target, labels, function.Token, problems);
			}

			if (problems.Count > 0)
			{
				throw new ScriptCompileException(problems);
			}

			using MemoryStream stream = new MemoryStream();
			Write(stream, (byte)'M', (byte)'H', (byte)'C', (byte)'S');
			WriteUInt16(stream, 1);
			WriteUInt16(stream, 1);
			WriteUInt32(stream, functionTable);
			WriteUInt16(stream, (ushort)document.Map);
			WriteUInt16(stream, (ushort)document.Casts.Count);

			foreach (CastDeclaration cast in document.Casts)
			{
				WriteUInt32(stream, (uint)cast.Number);
				WriteUInt32(stream, Resolve(cast.Init, labels, cast.Token, problems));
				WriteUInt32(stream, Resolve(cast.Main, labels, cast.Token, problems));
				WriteUInt32(stream, Resolve(cast.Exit, labels, cast.Token, problems));
			}

			stream.Write(code, 0, code.Length);

			WriteUInt32(stream, (uint)document.Functions.Count);
			foreach (FunctionDeclaration function in document.Functions)
			{
				WriteUInt32(stream, (uint)function.Id);
				WriteUInt32(stream, Resolve(function.Target, labels, function.Token, problems));
			}

			return stream.ToArray();
		}

		/// <summary>First pass: where everything lands.</summary>
		private static Dictionary<string, uint> Layout(ScriptDocument document,
			uint codeStart, List<Diagnostic> problems)
		{
			Dictionary<string, uint> labels = new Dictionary<string, uint>(StringComparer.Ordinal);
			uint at = codeStart;

			foreach (Statement item in document.Code)
			{
				switch (item)
				{
					case LabelItem label:
						if (labels.ContainsKey(label.Name))
						{
							problems.Add(new Diagnostic(
								"label '" + label.Name + "' is defined twice", label.Token));
						}
						else
						{
							labels[label.Name] = at;
						}
						break;

					case DataItem data:
						at += (uint)data.Bytes.Count;
						break;

					case InstructionItem instruction:
						at += Size(instruction, problems);
						break;
				}
			}
			return labels;
		}

		/// <summary>How many bytes an instruction occupies, resolving its opcode.</summary>
		private static uint Size(InstructionItem instruction, List<Diagnostic> problems)
		{
			if (instruction.Opcode < 0)
			{
				instruction.Opcode = Mnemonics.Opcode(instruction.Mnemonic);
			}
			if (instruction.Opcode < 0)
			{
				problems.Add(new Diagnostic(
					"unknown instruction '" + instruction.Mnemonic + "'", instruction.Token));
				return 0;
			}
			if (instruction.Opcode > ushort.MaxValue)
			{
				problems.Add(new Diagnostic(
					"an opcode is a 16 bit number", instruction.Token));
				return 0;
			}
			if (instruction.Opcode >= ScriptOps.Count)
			{
				// Past the dispatch table: no handler, so no operands either. These
				// only turn up in regions nothing reaches, where a linear sweep is
				// reading whatever happens to be there.
				if (instruction.Arguments.Count > 0)
				{
					problems.Add(new Diagnostic(string.Format(CultureInfo.InvariantCulture,
						"opcode {0} has no handler, so it takes no arguments",
						instruction.Opcode), instruction.Token));
				}
				return 2;
			}

			ScriptOp op = ScriptOps.Get(instruction.Opcode);
			Operand[] operands = op.Operands ?? Array.Empty<Operand>();
			if (instruction.Arguments.Count != operands.Length)
			{
				problems.Add(new Diagnostic(string.Format(CultureInfo.InvariantCulture,
					"{0} takes {1} argument(s), not {2}",
					instruction.Mnemonic, operands.Length, instruction.Arguments.Count),
					instruction.Token));
				return 2;
			}

			uint size = 2;
			for (int i = 0; i < operands.Length; i++)
			{
				size += Width(operands[i], instruction.Arguments[i]);
			}
			return size;
		}

		private static uint Width(Operand operand, Argument argument)
		{
			switch (operand)
			{
				case Operand.Byte:
					return 1;
				case Operand.Word:
					return 2;
				case Operand.Dword:
					return 4;
				default:
					return (uint)(argument.Kind == ArgumentKind.String
						? Encoding.UTF8.GetByteCount(argument.Text) + 1
						: 1);
			}
		}

		/// <summary>Second pass: the bytes.</summary>
		private static byte[] Emit(ScriptDocument document,
			Dictionary<string, uint> labels, List<Diagnostic> problems)
		{
			using MemoryStream stream = new MemoryStream();
			foreach (Statement item in document.Code)
			{
				switch (item)
				{
					case DataItem data:
						foreach (byte value in data.Bytes)
						{
							stream.WriteByte(value);
						}
						break;

					case InstructionItem instruction:
						EmitInstruction(stream, instruction, labels, problems);
						break;
				}
			}
			return stream.ToArray();
		}

		private static void EmitInstruction(Stream stream, InstructionItem instruction,
			Dictionary<string, uint> labels, List<Diagnostic> problems)
		{
			if (instruction.Opcode < 0 || instruction.Opcode > ushort.MaxValue)
			{
				return;                                  // already reported
			}

			WriteUInt16(stream, (ushort)instruction.Opcode);
			if (instruction.Opcode >= ScriptOps.Count)
			{
				return;
			}

			ScriptOp op = ScriptOps.Get(instruction.Opcode);
			Operand[] operands = op.Operands ?? Array.Empty<Operand>();

			for (int i = 0; i < operands.Length && i < instruction.Arguments.Count; i++)
			{
				Argument argument = instruction.Arguments[i];
				Operand operand = operands[i];

				if (operand == Operand.String)
				{
					if (argument.Kind != ArgumentKind.String)
					{
						problems.Add(new Diagnostic(
							"this argument is a string", argument.Token));
						stream.WriteByte(0);
						continue;
					}
					byte[] text = Encoding.UTF8.GetBytes(argument.Text);
					stream.Write(text, 0, text.Length);
					stream.WriteByte(0);
					continue;
				}

				long value;
				switch (argument.Kind)
				{
					case ArgumentKind.Label:
						if (!labels.TryGetValue(argument.Text, out uint address))
						{
							problems.Add(new Diagnostic(
								"no label called '" + argument.Text + "'", argument.Token));
							address = 0;
						}
						else if (operand != Operand.Dword)
						{
							problems.Add(new Diagnostic(
								"an address needs four bytes, and this argument has "
								+ (operand == Operand.Byte ? "one" : "two"), argument.Token));
						}
						value = address;
						break;

					case ArgumentKind.String:
						problems.Add(new Diagnostic(
							"this argument is a number, not a string", argument.Token));
						value = 0;
						break;

					default:
						value = argument.Number;
						break;
				}

				switch (operand)
				{
					case Operand.Byte:
						Check(value, byte.MinValue, byte.MaxValue, argument, problems);
						stream.WriteByte((byte)value);
						break;
					case Operand.Word:
						Check(value, ushort.MinValue, ushort.MaxValue, argument, problems);
						WriteUInt16(stream, (ushort)value);
						break;
					default:
						Check(value, uint.MinValue, uint.MaxValue, argument, problems);
						WriteUInt32(stream, (uint)value);
						break;
				}
			}
		}

		private static void Check(long value, long low, long high, Argument argument,
			List<Diagnostic> problems)
		{
			// Negative numbers are written as their two's complement, which is what
			// the game reads back, so only genuinely out of range values are errors.
			if (value < 0)
			{
				value += high + 1;
			}
			if (value < low || value > high)
			{
				problems.Add(new Diagnostic(string.Format(CultureInfo.InvariantCulture,
					"{0} does not fit in this argument, which holds {1} to {2}",
					argument.Number, low, high), argument.Token));
			}
		}

		private static uint Resolve(EntryTarget target, Dictionary<string, uint> labels,
			Token token, List<Diagnostic> problems)
		{
			if (target == null || target.IsNone)
			{
				return NoScript;
			}
			if (target.Address.HasValue)
			{
				return (uint)target.Address.Value;
			}
			if (labels.TryGetValue(target.Label, out uint address))
			{
				return address;
			}
			problems.Add(new Diagnostic("no label called '" + target.Label + "'", token));
			return NoScript;
		}

		private static void Write(Stream stream, params byte[] bytes)
		{
			stream.Write(bytes, 0, bytes.Length);
		}

		private static void WriteUInt16(Stream stream, ushort value)
		{
			stream.WriteByte((byte)value);
			stream.WriteByte((byte)(value >> 8));
		}

		private static void WriteUInt32(Stream stream, uint value)
		{
			stream.WriteByte((byte)value);
			stream.WriteByte((byte)(value >> 8));
			stream.WriteByte((byte)(value >> 16));
			stream.WriteByte((byte)(value >> 24));
		}
	}
}
