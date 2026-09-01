// .script - the event bytecode. 357 files: what an NPC does when you talk to it,
// when a cutscene fires, what a chest holds, how a quest moves forward.
//
//   ff3content script <file.script | directory> [out] [--text=<dir>]
//
// The engine (GlobalScope.ScriptEngine) fetches a 16 bit opcode and dispatches it
// through a 298 entry table. Every handler reads its operands straight off the
// instruction stream and - checked across all 298 - none of them reads inside a
// branch, so every instruction has a fixed length and a linear walk is exact.
// ScriptOps.cs is that table, generated from the handlers themselves.
//
// Layout (little endian)
//   +0  "MHCS"
//   +4  major, minor            1, 1
//   +8  offset of the function table
//   +12 map number
//   +14 cast count
//   +16 casts, 16 bytes each: cast number, then the offsets of its constructor,
//       its per frame body and its destructor. FFFFFFFF means it has none.
//   then the bytecode, and at the function table offset: a count followed by
//   (id, offset) pairs, which is what call reaches by id.
//
// This disassembles. It does not assemble: the output is for reading and for
// finding where something happens, not yet a source format to build from.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FF3.ContentTool
{
	internal sealed class ScriptCast
	{
		public uint Number;
		public uint Constructor;
		public uint Normal;
		public uint Destructor;
	}

	internal sealed class ScriptFunction
	{
		public uint Id;
		public uint Offset;
	}

	internal sealed class ScriptInstruction
	{
		public uint At;
		public int Opcode;
		public ScriptOp Op;
		public List<object> Operands = new List<object>();
		public uint Length;
		public uint? Target;                        // where a branch goes
		public List<uint> Targets = new List<uint>();  // or all of them, if it picks
		public bool Reached = true;                 // false: swept, not walked into
	}

	internal sealed class ScriptFile
	{
		public const uint NoScript = 0xFFFFFFFF;

		public ushort Major;
		public ushort Minor;
		public ushort MapNumber;
		public uint FunctionTableOffset;
		public List<ScriptCast> Casts = new List<ScriptCast>();
		public List<ScriptFunction> Functions = new List<ScriptFunction>();
		public byte[] Data;

		public static ScriptFile Read(byte[] data)
		{
			if (data == null || data.Length < 16
				|| data[0] != (byte)'M' || data[1] != (byte)'H'
				|| data[2] != (byte)'C' || data[3] != (byte)'S')
			{
				throw new InvalidDataException("not a script file");
			}

			ScriptFile file = new ScriptFile
			{
				Data = data,
				Major = ReadUInt16(data, 4),
				Minor = ReadUInt16(data, 6),
				FunctionTableOffset = ReadUInt32(data, 8),
				MapNumber = ReadUInt16(data, 12)
			};
			if (file.Major != 1 || file.Minor != 1)
			{
				throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture,
					"unsupported script version {0}.{1}", file.Major, file.Minor));
			}

			int castCount = ReadUInt16(data, 14);
			for (int i = 0; i < castCount; i++)
			{
				int at = 16 + i * 16;
				file.Casts.Add(new ScriptCast
				{
					Number = ReadUInt32(data, at),
					Constructor = ReadUInt32(data, at + 4),
					Normal = ReadUInt32(data, at + 8),
					Destructor = ReadUInt32(data, at + 12)
				});
			}

			uint table = file.FunctionTableOffset;
			if (table + 4 <= data.Length)
			{
				uint count = ReadUInt32(data, (int)table);
				for (uint i = 0; i < count && table + 4 + (i + 1) * 8 <= data.Length; i++)
				{
					int at = (int)(table + 4 + i * 8);
					file.Functions.Add(new ScriptFunction
					{
						Id = ReadUInt32(data, at),
						Offset = ReadUInt32(data, at + 4)
					});
				}
			}
			return file;
		}

		/// <summary>Where the bytecode lives: after the cast table, before the functions.</summary>
		public uint CodeStart => (uint)(16 + Casts.Count * 16);

		public uint CodeEnd => Math.Min(FunctionTableOffset, (uint)Data.Length);

		private static ushort ReadUInt16(byte[] d, int at)
		{
			return (ushort)(d[at] | (d[at + 1] << 8));
		}

		private static uint ReadUInt32(byte[] d, int at)
		{
			return (uint)(d[at] | (d[at + 1] << 8) | (d[at + 2] << 16) | (d[at + 3] << 24));
		}
	}

	internal static class ScriptDisassembler
	{
		// Flow stops dead after these; everything else falls through to the next
		// instruction, including the conditional ends and conditional jumps. NOP is
		// not among them - its handler is empty, so execution simply carries on.
		private static readonly HashSet<string> Terminators = new HashSet<string>
		{
			"endCommand", "jumpCommand", "returnCommand"
		};

		private const int StartMessage2 = 100;

		/// <summary>
		/// Walks every entry point, following calls and jumps. Returns the instructions
		/// in address order, plus the addresses worth naming.
		/// </summary>
		public static (List<ScriptInstruction> Code, Dictionary<uint, List<string>> Labels)
			Disassemble(ScriptFile file)
		{
			Dictionary<uint, ScriptInstruction> found = new Dictionary<uint, ScriptInstruction>();
			Dictionary<uint, List<string>> labels = new Dictionary<uint, List<string>>();
			Queue<uint> pending = new Queue<uint>();

			// One address can have several names: two function ids share an offset in
			// global.script, and a cast entry can also be a call target. Keeping all of
			// them means the listing does not quietly lose one.
			void Name(uint at, string name)
			{
				if (!labels.TryGetValue(at, out List<string> names))
				{
					labels[at] = names = new List<string>();
				}
				if (!names.Contains(name))
				{
					names.Add(name);
				}
			}

			void Entry(uint at, string name)
			{
				if (at == ScriptFile.NoScript || at < file.CodeStart || at >= file.CodeEnd)
				{
					return;
				}
				Name(at, name);
				pending.Enqueue(at);
			}

			foreach (ScriptCast cast in file.Casts)
			{
				Entry(cast.Constructor, string.Format(CultureInfo.InvariantCulture,
					"cast{0}_init", cast.Number));
				Entry(cast.Normal, string.Format(CultureInfo.InvariantCulture,
					"cast{0}_main", cast.Number));
				Entry(cast.Destructor, string.Format(CultureInfo.InvariantCulture,
					"cast{0}_exit", cast.Number));
			}
			foreach (ScriptFunction function in file.Functions)
			{
				Entry(function.Offset, string.Format(CultureInfo.InvariantCulture,
					"func_{0}", function.Id));
			}

			while (pending.Count > 0)
			{
				uint pc = pending.Dequeue();
				while (pc >= file.CodeStart && pc < file.CodeEnd && !found.ContainsKey(pc))
				{
					ScriptInstruction instruction = Decode(file, pc);
					found[pc] = instruction;
					if (instruction.Op == null)
					{
						break;              // unknown opcode: stop rather than guess
					}
					foreach (uint target in instruction.Targets)
					{
						if (target >= file.CodeStart && target < file.CodeEnd)
						{
							if (!labels.ContainsKey(target))
							{
								Name(target, string.Format(CultureInfo.InvariantCulture,
									"loc_{0:X4}", target));
							}
							pending.Enqueue(target);
						}
					}
					if (Terminators.Contains(instruction.Op.Name))
					{
						break;
					}
					pc += instruction.Length;
				}
			}

			SweepGaps(file, found, labels);
			return (found.Values.OrderBy(i => i.At).ToList(), labels);
		}

		/// <summary>
		/// Decodes whatever the walk did not reach. About an eighth of the bytecode is
		/// not reachable from any cast entry or function in its own file, and nothing
		/// in the file refers to it - dead or driven from somewhere not yet understood.
		/// It decodes cleanly, so it is shown rather than hidden, but marked, because
		/// a linear sweep starting at an arbitrary byte is a guess in a way that
		/// following the entry points is not.
		/// </summary>
		private static void SweepGaps(ScriptFile file,
			Dictionary<uint, ScriptInstruction> found,
			Dictionary<uint, List<string>> labels)
		{
			uint pc = file.CodeStart;
			while (pc < file.CodeEnd)
			{
				if (found.TryGetValue(pc, out ScriptInstruction known))
				{
					pc += Math.Max(known.Length, 2);
					continue;
				}

				ScriptInstruction instruction = Decode(file, pc);
				uint length = Math.Max(instruction.Length, 2);

				// The sweep is reading bytes nothing reaches, so its alignment is a
				// guess. Where that guess would swallow an address something else
				// points at, the bytes are left undecoded instead - otherwise the
				// label disappears inside an instruction and the listing can no
				// longer be compiled back.
				uint boundary = NextKnown(pc + 1, pc + length, found, labels);
				if (boundary != 0)
				{
					pc = boundary;
					continue;
				}

				instruction.Reached = false;
				found[pc] = instruction;
				pc += length;
			}
		}

		/// <summary>The first address in (from, until) that something already claims.</summary>
		private static uint NextKnown(uint from, uint until,
			Dictionary<uint, ScriptInstruction> found,
			Dictionary<uint, List<string>> labels)
		{
			for (uint at = from; at < until; at++)
			{
				if (found.ContainsKey(at) || labels.ContainsKey(at))
				{
					return at;
				}
			}
			return 0;
		}

		private static ScriptInstruction Decode(ScriptFile file, uint pc)
		{
			byte[] data = file.Data;
			ScriptInstruction instruction = new ScriptInstruction { At = pc };
			int opcode = data[pc] | (data[pc + 1] << 8);
			instruction.Opcode = opcode;
			instruction.Op = ScriptOps.Get(opcode);

			uint at = pc + 2;
			if (instruction.Op?.Operands == null)
			{
				instruction.Length = 2;
				return instruction;
			}

			foreach (Operand kind in instruction.Op.Operands)
			{
				switch (kind)
				{
					case Operand.Byte:
						instruction.Operands.Add((uint)data[at]);
						at += 1;
						break;
					case Operand.Word:
						instruction.Operands.Add((uint)(data[at] | (data[at + 1] << 8)));
						at += 2;
						break;
					case Operand.Dword:
						uint value = (uint)(data[at] | (data[at + 1] << 8)
							| (data[at + 2] << 16) | (data[at + 3] << 24));
						instruction.Operands.Add(value);
						at += 4;
						break;
					case Operand.String:
						int end = Array.IndexOf(data, (byte)0, (int)at);
						if (end < 0)
						{
							end = data.Length;
						}
						instruction.Operands.Add(
							Encoding.UTF8.GetString(data, (int)at, end - (int)at));
						at = (uint)end + 1;
						break;
				}
			}

			instruction.Length = at - pc;
			instruction.Target = BranchTarget(instruction);
			if (instruction.Target.HasValue)
			{
				instruction.Targets.Add(instruction.Target.Value);
			}
			else if (instruction.Op.JumpsToAny)
			{
				for (int i = 0; i < instruction.Op.Operands.Length; i++)
				{
					if (instruction.Op.Operands[i] == Operand.Dword)
					{
						instruction.Targets.Add((uint)instruction.Operands[i]);
					}
				}
			}
			return instruction;
		}

		/// <summary>
		/// Where a branch goes, taken from the operand the handler actually passes to
		/// engine.jump(). Guessing "the first Dword" gets it wrong: a conditional jump
		/// reads its flags first, and WithOutCharacterJump reads a whole coordinate box
		/// before the destination.
		/// </summary>
		private static uint? BranchTarget(ScriptInstruction instruction)
		{
			int index = instruction.Op.JumpOperand;
			return index >= 0 && index < instruction.Operands.Count
				&& instruction.Operands[index] is uint target
				? target : (uint?)null;
		}

		/// <summary>
		/// Instructions that overlap the next one. Every instruction has a fixed
		/// length, so a walk that decoded one operand list wrongly would land
		/// mid-instruction somewhere and show up here. Zero across every shipped
		/// script is the evidence that the operand table is right.
		/// </summary>
		public static int CountOverlaps(List<ScriptInstruction> code)
		{
			int overlaps = 0;
			for (int i = 0; i + 1 < code.Count; i++)
			{
				if (code[i].At + code[i].Length > code[i + 1].At)
				{
					overlaps++;
				}
			}
			return overlaps;
		}

		public static void Write(TextWriter writer, ScriptFile file, string name,
			Func<uint, string> lookupMessage)
		{
			(List<ScriptInstruction> code, Dictionary<uint, List<string>> labels) = Disassemble(file);

			writer.WriteLine("; {0}", name);
			int walked = code.Count(i => i.Reached);
			int overlaps = CountOverlaps(code);
			writer.WriteLine("; map {0}   {1} casts   {2} functions   {3} instructions"
				+ " ({4} reached from an entry point)",
				file.MapNumber, file.Casts.Count, file.Functions.Count, code.Count, walked);
			if (overlaps > 0)
			{
				writer.WriteLine("; WARNING: {0} instruction(s) overlap the next one -"
					+ " a decode landed mid instruction", overlaps);
			}
			foreach (ScriptCast cast in file.Casts)
			{
				writer.WriteLine(";   cast {0,-4} init {1}  main {2}  exit {3}",
					cast.Number, Address(cast.Constructor), Address(cast.Normal),
					Address(cast.Destructor));
			}
			writer.WriteLine();

			bool reached = true;
			foreach (ScriptInstruction instruction in code)
			{
				if (instruction.Reached != reached)
				{
					reached = instruction.Reached;
					writer.WriteLine();
					writer.WriteLine(reached
						? "; ---- reachable again"
						: "; ---- not reached from any entry point in this file");
				}

				if (labels.TryGetValue(instruction.At, out List<string> names))
				{
					writer.WriteLine();
					foreach (string name2 in names)
					{
						writer.WriteLine("{0}:", name2);
					}
				}

				string operands = string.Join(" ", instruction.Operands.Select((value, i) =>
					Format(instruction, i, value, labels)));
				string line = string.Format(CultureInfo.InvariantCulture,
					"  {0:X4}  {1} {2}", instruction.At,
					instruction.Op?.Name ?? string.Format(CultureInfo.InvariantCulture,
						"unknown_{0}", instruction.Opcode), operands);

				string comment = Comment(instruction, lookupMessage);
				writer.WriteLine(comment == null ? line.TrimEnd()
					: string.Format(CultureInfo.InvariantCulture, "{0,-64}; {1}",
						line.TrimEnd(), comment));
			}
		}

		private static string Format(ScriptInstruction instruction, int index, object value,
			Dictionary<uint, List<string>> labels)
		{
			if (value is string text)
			{
				return "\"" + text + "\"";
			}
			uint number = (uint)value;
			if (instruction.Targets.Contains(number)
				&& labels.TryGetValue(number, out List<string> names))
			{
				return names[0];
			}
			return number > 9
				? "0x" + number.ToString("X", CultureInfo.InvariantCulture)
				: number.ToString(CultureInfo.InvariantCulture);
		}

		private static string Comment(ScriptInstruction instruction, Func<uint, string> lookupMessage)
		{
			if (lookupMessage == null || instruction.Opcode != StartMessage2
				|| instruction.Operands.Count < 2)
			{
				return null;
			}
			string text = lookupMessage((uint)instruction.Operands[1]);
			return text == null ? null : Quote(text);
		}

		private static string Quote(string text)
		{
			string flat = text.Replace("\r", string.Empty).Replace("\n", " / ");
			return "\"" + (flat.Length > 90 ? flat.Substring(0, 90) + "..." : flat) + "\"";
		}

		private static string Address(uint at)
		{
			return at == ScriptFile.NoScript
				? "----" : at.ToString("X4", CultureInfo.InvariantCulture);
		}
	}
}
