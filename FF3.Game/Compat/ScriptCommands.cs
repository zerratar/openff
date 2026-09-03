// The script command table for the content in front.
//
// FF3's 298 handlers are the decompiled game (GlobalScope.commandTable). FF4's scripts
// were compiled against a 500-entry table (Shared/Script/ScriptOpsFf4, read out of
// libff4.so): 201 commands are the same command at the same number reading the same
// operands, and those run FF3's handler as they are. The rest - FF4-only commands, and
// the 26 that share a number and a name but read different operands - get a stub that
// reads exactly the operands the table says and does nothing else, so the script stays
// in step and the next command is decoded correctly. Each stubbed command is logged
// once; that log is the to-do list for FF4.

using System;
using System.Collections.Generic;

namespace FF3
{
	internal static class ScriptCommands
	{
		private static GlobalScope.SCRIPT_COMMAND[] _ff4;
		private static readonly HashSet<int> _reported = new HashSet<int>();
		private static int _reusedCount;

		/// <summary>Runs one command on the engine, from whichever table the content needs.</summary>
		private static readonly bool _noScripts = Options.Get("noscript") != null;

		public static void Dispatch(GlobalScope.ScriptEngine engine, uint opcode)
		{
			if (_noScripts)
			{
				// --noscript: every script ends at its first command, for telling a map's own
				// problems from what its scripts do to it.
				engine.end();
				return;
			}
			GlobalScope.SCRIPT_COMMAND[] table = GameProfile.IsFf4 ? Ff4Table() : GlobalScope.commandTable;
			if (opcode < table.Length && table[opcode] != null)
			{
				table[opcode](engine);
				return;
			}
			lock (_reported)
			{
				if (_reported.Add((int)opcode + 100000))
				{
					Log.Write(LogChannel.General, "script: opcode " + opcode + " is outside the " + GameProfile.Game + " table; ending the script");
				}
			}
			engine.end();
		}

		private static GlobalScope.SCRIPT_COMMAND[] Ff4Table()
		{
			if (_ff4 != null)
			{
				return _ff4;
			}
			ScriptOpTable ff4 = ScriptOpTable.Ff4;
			ScriptOpTable ff3 = ScriptOpTable.Ff3;
			GlobalScope.SCRIPT_COMMAND[] table = new GlobalScope.SCRIPT_COMMAND[ff4.Count];
			for (int i = 0; i < table.Length; i++)
			{
				ScriptOp op = ff4.Get(i);
				ScriptOp theirs = ff3.Get(i);
				if (op != null && theirs != null && i < GlobalScope.commandTable.Length
					&& SameCommand(op, theirs))
				{
					table[i] = GlobalScope.commandTable[i];
					_reusedCount++;
				}
				else
				{
					int opcode = i;
					table[i] = engine => Skip(engine, opcode, op);
				}
			}
			Log.Write(LogChannel.General, "script: FF4 table built - " + _reusedCount + " of " + table.Length + " commands run FF3's handler, the rest are skipped with their operands");
			_ff4 = table;
			return table;
		}

		private static bool SameCommand(ScriptOp mine, ScriptOp theirs)
		{
			if (ScriptOpTable.Simplify(mine.Name) != ScriptOpTable.Simplify(theirs.Name))
			{
				return false;
			}
			Operand[] a = mine.Operands ?? Array.Empty<Operand>();
			Operand[] b = theirs.Operands ?? Array.Empty<Operand>();
			if (a.Length != b.Length || mine.Variable != theirs.Variable)
			{
				return false;
			}
			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] != b[i])
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>Reads the command's operands and moves on, logging the first time.</summary>
		private static void Skip(GlobalScope.ScriptEngine engine, int opcode, ScriptOp op)
		{
			if (op?.Operands != null)
			{
				foreach (Operand operand in op.Operands)
				{
					switch (operand)
					{
						case Operand.Byte: engine.getByte(); break;
						case Operand.Word: engine.getWord(); break;
						case Operand.Dword: engine.getDword(); break;
						case Operand.String: engine.getString(); break;
					}
				}
			}
			lock (_reported)
			{
				if (_reported.Add(opcode))
				{
					Log.Write(LogChannel.General, "script: FF4 command " + opcode + " " + (op != null ? ScriptOpTable.Simplify(op.Name) : "?") + " not implemented - skipped");
				}
			}
		}
	}
}
