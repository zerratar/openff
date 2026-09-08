// The names the script language uses for the opcodes of one command table.
//
// Derived from the handler names rather than invented, so anyone reading the game's
// sources and anyone reading a .ffs file are looking at the same vocabulary:
//
//   waitCommand                     -> wait
//   ff3Command_StartMessage2        -> startMessage2
//   babilCommand_SetInsideMapJump   -> setInsideMapJump
//   flagOnJumpCommand               -> flagOnJump
//
// Five opcodes share the name NOPCommand, so those get their opcode number appended
// (nop0 .. nop4); FF4's eighteen UnUseCommand slots become unUse56 and so on. Anything
// else can still be written as op(<number>), which is what the disassembler falls back
// to for an opcode with no handler.
//
// One instance per table - FF3's and FF4's vocabularies overlap but are not the same,
// and a name must resolve to the number the game being edited dispatches on.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Crystal.Ffs
{
	internal sealed class Mnemonics
	{
		private readonly string[] _names;
		private readonly Dictionary<string, int> _opcodes =
			new Dictionary<string, int>(StringComparer.Ordinal);

		public Mnemonics(ScriptOp[] table)
		{
			_names = new string[table.Length];

			// Count first: a name shared by several opcodes has to be disambiguated,
			// and that can only be decided once every name is known.
			Dictionary<string, int> uses = new Dictionary<string, int>(StringComparer.Ordinal);
			string[] plain = new string[table.Length];
			for (int opcode = 0; opcode < table.Length; opcode++)
			{
				plain[opcode] = Simplify(table[opcode].Name);
				uses.TryGetValue(plain[opcode], out int count);
				uses[plain[opcode]] = count + 1;
			}

			for (int opcode = 0; opcode < table.Length; opcode++)
			{
				string name = uses[plain[opcode]] > 1
					? plain[opcode] + opcode.ToString(CultureInfo.InvariantCulture)
					: plain[opcode];
				_names[opcode] = name;
				_opcodes[name] = opcode;
			}
		}

		/// <summary>The mnemonic for an opcode, or op(n) when there is no handler.</summary>
		public string Name(int opcode)
		{
			return opcode >= 0 && opcode < _names.Length
				? _names[opcode]
				: string.Format(CultureInfo.InvariantCulture, "op({0})", opcode);
		}

		/// <summary>The opcode for a mnemonic, or -1.</summary>
		public int Opcode(string name)
		{
			return _opcodes.TryGetValue(name, out int opcode) ? opcode : -1;
		}

		public IEnumerable<KeyValuePair<string, int>> All => _opcodes;

		/// <summary>
		/// handler name -> mnemonic: drop the ff3Command_ (or babilCommand_) prefix and
		/// the Command suffix, then lower the leading run of capitals so NOPCommand
		/// becomes nop and StartMessage2 becomes startMessage2.
		/// </summary>
		public static string Simplify(string handler)
		{
			return ScriptOpTable.Simplify(handler);
		}
	}
}
