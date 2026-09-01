// The names the script language uses for the 298 opcodes.
//
// Derived from the handler names rather than invented, so anyone reading the game's
// sources and anyone reading a .ffs file are looking at the same vocabulary:
//
//   waitCommand                     -> wait
//   ff3Command_StartMessage2        -> startMessage2
//   flagOnJumpCommand               -> flagOnJump
//
// Five opcodes share the name NOPCommand, so those get their opcode number appended
// (nop0 .. nop4). Anything else can still be written as op(<number>), which is what
// the disassembler falls back to for an opcode with no handler.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FF3.ContentTool.Ffs
{
	internal static class Mnemonics
	{
		private static readonly string[] _names = new string[ScriptOps.Count];
		private static readonly Dictionary<string, int> _opcodes =
			new Dictionary<string, int>(StringComparer.Ordinal);

		static Mnemonics()
		{
			// Count first: a name shared by several opcodes has to be disambiguated,
			// and that can only be decided once every name is known.
			Dictionary<string, int> uses = new Dictionary<string, int>(StringComparer.Ordinal);
			string[] plain = new string[ScriptOps.Count];
			for (int opcode = 0; opcode < ScriptOps.Count; opcode++)
			{
				plain[opcode] = Simplify(ScriptOps.Get(opcode).Name);
				uses.TryGetValue(plain[opcode], out int count);
				uses[plain[opcode]] = count + 1;
			}

			for (int opcode = 0; opcode < ScriptOps.Count; opcode++)
			{
				string name = uses[plain[opcode]] > 1
					? plain[opcode] + opcode.ToString(CultureInfo.InvariantCulture)
					: plain[opcode];
				_names[opcode] = name;
				_opcodes[name] = opcode;
			}
		}

		/// <summary>The mnemonic for an opcode, or op(n) when there is no handler.</summary>
		public static string Name(int opcode)
		{
			return opcode >= 0 && opcode < _names.Length
				? _names[opcode]
				: string.Format(CultureInfo.InvariantCulture, "op({0})", opcode);
		}

		/// <summary>The opcode for a mnemonic, or -1.</summary>
		public static int Opcode(string name)
		{
			return _opcodes.TryGetValue(name, out int opcode) ? opcode : -1;
		}

		public static IEnumerable<KeyValuePair<string, int>> All => _opcodes;

		/// <summary>
		/// handler name -> mnemonic: drop the ff3Command_ prefix and the Command
		/// suffix, then lower the leading run of capitals so NOPCommand becomes nop
		/// and StartMessage2 becomes startMessage2.
		/// </summary>
		private static string Simplify(string handler)
		{
			string name = handler;
			if (name.StartsWith("ff3Command_", StringComparison.Ordinal))
			{
				name = name.Substring("ff3Command_".Length);
			}
			if (name.Length > "Command".Length
				&& name.EndsWith("Command", StringComparison.Ordinal))
			{
				name = name.Substring(0, name.Length - "Command".Length);
			}
			if (name.Length == 0)
			{
				return handler;
			}

			StringBuilder text = new StringBuilder(name);
			int i = 0;
			while (i < text.Length && char.IsUpper(text[i])
				&& (i + 1 >= text.Length || !char.IsLower(text[i + 1]) || i == 0))
			{
				text[i] = char.ToLowerInvariant(text[i]);
				i++;
			}
			return text.ToString();
		}
	}
}
