// Writes a .script back out as script language source.
//
// The output is meant to be compiled again, so everything the format carries has to
// survive the trip: the map number, the cast table, the function table, every
// instruction, and any byte in the code region that is not one. What it deliberately
// does not carry is addresses - those become labels, which is the whole point of
// having a language rather than a hex editor.
//
// Dialogue is written in as a comment beside the instruction that shows it. Comments
// are free: the compiler ignores them, so the text can be wrong or stale without
// breaking anything, and it makes a script readable at a glance.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FF3.ContentTool.Ffs
{
	internal static class SourceWriter
	{
		private const int StartMessage2 = 100;

		public static void Write(TextWriter writer, ScriptFile file, string name,
			Func<uint, string> lookupMessage)
		{
			(List<ScriptInstruction> code, Dictionary<uint, List<string>> labels) =
				ScriptDisassembler.Disassemble(file);
			Dictionary<uint, ScriptInstruction> byAddress = code.ToDictionary(i => i.At);

			writer.WriteLine("// {0}", name);
			writer.WriteLine("// Decompiled by crystal. Compile with:");
			writer.WriteLine("//   crystal script-build {0}",
				Path.ChangeExtension(name, ".ffs"));
			writer.WriteLine();
			writer.WriteLine("map {0};", file.MapNumber);
			writer.WriteLine();

			foreach (ScriptCast cast in file.Casts)
			{
				writer.WriteLine("cast {0} {{", cast.Number);
				writer.WriteLine("    init = {0};", Target(cast.Constructor, labels));
				writer.WriteLine("    main = {0};", Target(cast.Normal, labels));
				writer.WriteLine("    exit = {0};", Target(cast.Destructor, labels));
				writer.WriteLine("}");
			}

			if (file.Functions.Count > 0)
			{
				writer.WriteLine();
				foreach (ScriptFunction function in file.Functions)
				{
					writer.WriteLine("function 0x{0:X8} = {1};",
						function.Id, Target(function.Offset, labels));
				}
			}

			writer.WriteLine();
			bool reached = true;
			uint at = file.CodeStart;

			while (at < file.CodeEnd)
			{
				if (labels.TryGetValue(at, out List<string> names))
				{
					writer.WriteLine();
					foreach (string label in names)
					{
						writer.WriteLine("{0}:", label);
					}
				}

				if (!byAddress.TryGetValue(at, out ScriptInstruction instruction))
				{
					// Not an instruction: emit the run of bytes up to whatever is.
					uint end = at;
					while (end < file.CodeEnd && !byAddress.ContainsKey(end)
						&& !labels.ContainsKey(end))
					{
						end++;
					}
					WriteData(writer, file.Data, at, end - at);
					at = end;
					continue;
				}

				if (at + instruction.Length > file.CodeEnd)
				{
					// The last few bytes before the function table are not a whole
					// instruction. A linear sweep will happily decode one anyway and
					// read past the end, so what is actually there is written out as
					// bytes instead.
					WriteData(writer, file.Data, at, file.CodeEnd - at);
					break;
				}

				if (instruction.Reached != reached)
				{
					reached = instruction.Reached;
					writer.WriteLine();
					writer.WriteLine(reached
						? "// ---- reachable again"
						: "// ---- not reached from any entry point in this file");
				}

				WriteInstruction(writer, instruction, labels, lookupMessage, file.Ops);
				at += Math.Max(instruction.Length, 1);
			}
		}

		private static void WriteInstruction(TextWriter writer, ScriptInstruction instruction,
			Dictionary<uint, List<string>> labels, Func<uint, string> lookupMessage, ScriptOpTable ops)
		{
			string mnemonic = instruction.Op == null
				? string.Format(CultureInfo.InvariantCulture, "op({0})", instruction.Opcode)
				: ops.Names.Name(instruction.Opcode);

			StringBuilder line = new StringBuilder("    ").Append(mnemonic);

			// op(512) already carries its own brackets and never has operands.
			if (instruction.Op != null)
			{
				line.Append('(');
				for (int i = 0; i < instruction.Operands.Count; i++)
				{
					if (i > 0)
					{
						line.Append(", ");
					}
					line.Append(Format(instruction, i, labels));
				}
				line.Append(')');
			}
			line.Append(';');

			string comment = Comment(instruction, lookupMessage, ops);
			if (comment != null)
			{
				// Line up where it can, but never run into the comment marker.
				do
				{
					line.Append(' ');
				}
				while (line.Length < 60);
				line.Append("// ").Append(comment);
			}
			writer.WriteLine(line.ToString());
		}

		private static string Format(ScriptInstruction instruction, int index,
			Dictionary<uint, List<string>> labels)
		{
			object value = instruction.Operands[index];
			if (value is string text)
			{
				return Quote(text);
			}

			uint number = (uint)value;
			if (instruction.Targets.Contains(number)
				&& labels.TryGetValue(number, out List<string> names))
			{
				return names[0];
			}

			// Small numbers read better as decimal; ids and masks as hex.
			return number > 9999
				? "0x" + number.ToString("X", CultureInfo.InvariantCulture)
				: number.ToString(CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// What the numbers on this line mean, where that is known: the dialogue behind
		/// a message id, and any operand held in NDS fixed point. 0x64000 is 100.0, and
		/// working that out in your head while reading a cutscene is no way to live.
		/// </summary>
		private static string Comment(ScriptInstruction instruction,
			Func<uint, string> lookupMessage, ScriptOpTable ops)
		{
			if (lookupMessage != null && instruction.Opcode == StartMessage2
				&& instruction.Operands.Count >= 2)
			{
				string text = lookupMessage((uint)instruction.Operands[1]);
				if (text != null)
				{
					string flat = text.Replace(CR, string.Empty).Replace(LF, " / ");
					return Q + (flat.Length > 90 ? flat.Substring(0, 90) + "..." : flat) + Q;
				}
			}

			List<string> parts = new List<string>();
			for (int i = 0; i < instruction.Operands.Count; i++)
			{
				if (!ops.IsFixed(instruction.Opcode, i)
					|| !(instruction.Operands[i] is uint raw))
				{
					continue;
				}
				string name = ops.OperandName(instruction.Opcode, i)
					?? ("arg" + i.ToString(CultureInfo.InvariantCulture));
				parts.Add(string.Format(CultureInfo.InvariantCulture, "{0} {1}",
					name, FixedPoint(raw)));
			}
			return parts.Count > 0 ? string.Join("  ", parts) : null;
		}

		/// <summary>NDS fixed point: a position is held in 1/4096ths of a unit.</summary>
		private static string FixedPoint(uint raw)
		{
			return ((int)raw / 4096.0).ToString("0.###", CultureInfo.InvariantCulture);
		}

		private const char Q = '\"';
		private const string CR = "\r";
		private const string LF = "\n";

		private static void WriteData(TextWriter writer, byte[] data, uint at, uint length)
		{
			const int PerLine = 16;
			for (uint start = 0; start < length; start += PerLine)
			{
				uint count = Math.Min(PerLine, length - start);
				writer.WriteLine("    data {0};", string.Join(", ",
					Enumerable.Range(0, (int)count)
						.Select(i => "0x" + data[at + start + i]
							.ToString("X2", CultureInfo.InvariantCulture))));
			}
		}

		private static string Target(uint address, Dictionary<uint, List<string>> labels)
		{
			if (address == ScriptFile.NoScript)
			{
				return "none";
			}
			if (labels.TryGetValue(address, out List<string> names))
			{
				return names[0];
			}
			// An entry pointing outside the code has no label to name it. global.script
			// aims its only cast at address 0, so the address is written as it stands.
			return "0x" + address.ToString("X", CultureInfo.InvariantCulture);
		}

		private static string Quote(string text)
		{
			StringBuilder quoted = new StringBuilder("\"");
			foreach (char c in text)
			{
				switch (c)
				{
					case '"': quoted.Append("\\\""); break;
					case '\\': quoted.Append("\\\\"); break;
					case '\n': quoted.Append("\\n"); break;
					case '\r': quoted.Append("\\r"); break;
					case '\t': quoted.Append("\\t"); break;
					default: quoted.Append(c); break;
				}
			}
			return quoted.Append('"').ToString();
		}
	}
}
