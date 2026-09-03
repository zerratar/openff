// One script engine, two command tables.
//
// FF3 and FF4 3D run the same bytecode interpreter, but each game hands it its own
// dispatch table: FF3's has 298 entries (ScriptOps.cs, from the decompiled source),
// FF4's has 500 (ScriptOpsFf4.cs, from libff4.so). 222 commands sit at the same number
// in both, and 48 of those read different operands - so a script can only be decoded,
// named or compiled against the table of the game it belongs to. Everything that
// touches bytecode takes one of these rather than reaching for ScriptOps directly.

using System;

namespace FF3.Script
{
	internal sealed partial class ScriptOpTable
	{
		public static readonly ScriptOpTable Ff3 = new ScriptOpTable("ff3", ScriptOps.Table);
		public static readonly ScriptOpTable Ff4 = new ScriptOpTable("ff4", ScriptOpsFf4.Table);

		/// <summary>"ff3" or "ff4" - what Workspace.Game says.</summary>
		public readonly string Game;

		public readonly ScriptOp[] Ops;


		private ScriptOpTable(string game, ScriptOp[] ops)
		{
			Game = game;
			Ops = ops;
		}

		/// <summary>The table for a game; anything that is not FF4 is FF3.</summary>
		public static ScriptOpTable For(string game)
		{
			return string.Equals(game, "ff4", StringComparison.OrdinalIgnoreCase) ? Ff4 : Ff3;
		}

		public int Count => Ops.Length;

		/// <summary>Whether the engine has a handler at this number at all.</summary>
		public bool Known(int opcode)
		{
			return opcode >= 0 && opcode < Ops.Length;
		}

		public ScriptOp Get(int opcode)
		{
			return Known(opcode) ? Ops[opcode] : null;
		}

		/// <summary>The mnemonics for this table, built once.</summary>

		/// <summary>
		/// What an operand is called, where that is known. The names were followed
		/// through FF3's source, so they apply to FF4 only where FF4 has the same
		/// command at the same number reading the same number of operands - the 48
		/// that differ would otherwise be labelled with another command's meaning.
		/// </summary>
		public string OperandName(int opcode, int operand)
		{
			return SharesFf3Operands(opcode) ? ScriptOperands.Name(opcode, operand) : null;
		}

		/// <summary>Whether an operand is NDS fixed point, on the same terms as OperandName.</summary>
		public bool IsFixed(int opcode, int operand)
		{
			return SharesFf3Operands(opcode) && ScriptOperands.IsFixed(opcode, operand);
		}

		private bool SharesFf3Operands(int opcode)
		{
			if (this == Ff3)
			{
				return true;
			}
			ScriptOp mine = Get(opcode);
			ScriptOp theirs = Ff3.Get(opcode);
			if (mine == null || theirs == null || mine.Operands == null || theirs.Operands == null)
			{
				return false;
			}
			return mine.Operands.Length == theirs.Operands.Length
				&& Simplify(mine.Name) == Simplify(theirs.Name);
		}

		/// <summary>
		/// handler name -> mnemonic: drop the ff3Command_ (or babilCommand_) prefix and
		/// the Command suffix, then lower the leading run of capitals so NOPCommand
		/// becomes nop and StartMessage2 becomes startMessage2. babilCommand_3DSSetup
		/// keeps its underscore (_3DSSetup) because no lexer reads a name that starts
		/// with a digit.
		/// </summary>
		public static string Simplify(string handler)
		{
			string name = handler;
			// Two games, two prefixes: ours is ff3Command_, FF4's is babilCommand_ -
			// Babil being what the FF4 team called it internally. One FF4 handler is
			// spelt babilCommands_.
			foreach (string prefix in new[] { "ff3Command_", "babilCommands_", "babilCommand_" })
			{
				if (name.StartsWith(prefix, StringComparison.Ordinal))
				{
					name = name.Substring(prefix.Length);
					break;
				}
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
			if (char.IsDigit(name[0]))
			{
				name = "_" + name;
			}
			System.Text.StringBuilder text = new System.Text.StringBuilder(name);
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
