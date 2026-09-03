// One script engine, two command tables.
//
// FF3 and FF4 3D run the same bytecode interpreter, but each game hands it its own
// dispatch table: FF3's has 298 entries (ScriptOps.cs, from the decompiled source),
// FF4's has 500 (ScriptOpsFf4.cs, from libff4.so). 222 commands sit at the same number
// in both, and 48 of those read different operands - so a script can only be decoded,
// named or compiled against the table of the game it belongs to. Everything that
// touches bytecode takes one of these rather than reaching for ScriptOps directly.

using System;

namespace FF3.ContentTool
{
	internal sealed class ScriptOpTable
	{
		public static readonly ScriptOpTable Ff3 = new ScriptOpTable("ff3", ScriptOps.Table);
		public static readonly ScriptOpTable Ff4 = new ScriptOpTable("ff4", ScriptOpsFf4.Table);

		/// <summary>"ff3" or "ff4" - what Workspace.Game says.</summary>
		public readonly string Game;

		public readonly ScriptOp[] Ops;

		private Ffs.Mnemonics _names;

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
		public Ffs.Mnemonics Names
		{
			get
			{
				if (_names == null)
				{
					_names = new Ffs.Mnemonics(Ops);
				}
				return _names;
			}
		}

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
				&& Ffs.Mnemonics.Simplify(mine.Name) == Ffs.Mnemonics.Simplify(theirs.Name);
		}
	}
}
