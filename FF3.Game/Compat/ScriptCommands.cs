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

		/// <summary>
		/// Same-numbered commands whose FF4 operands are FF3's followed by more (or, for the
		/// two camera moves, whose extra word lands in operands FF3's handler reads but
		/// ignores): bytes to step past after FF3's handler has read its own.
		/// </summary>
		private static readonly Dictionary<int, uint> _extraOperandBytes = new Dictionary<int, uint>
		{
			{ 66, 3 },   // bootEventBattle: W,B + B,B,B
			{ 72, 2 },   // moveCamera_AbsoluteCoordination: D,D,D,W,[W],D - FF3 ignores the W and D
			{ 73, 2 },   // moveCamera_RelativeCoordination: same
			{ 91, 4 },   // playBGM: W,B,W + W,W
			{ 92, 4 },   // stopBGM: W + W,W
			{ 113, 8 },  // bootInn: W + D,D
			{ 124, 16 }, // selectEndWait: D + D,D,D,D
			{ 126, 4 },  // moveCamera_LookPlayer2: W,W,D + W,W
			{ 200, 6 },  // setHalfWayBGM_Play: W,W + W,W,W
			{ 203, 4 },  // setBGM_Volume: W,W,W + W,W
			{ 217, 2 },  // setCamera_FOV: W,W + W
		};
		private static readonly HashSet<int> _reported = new HashSet<int>();
		private static readonly int[] _recent = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
		private static int _recentAt;
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
				_recent[_recentAt] = (int)opcode;
				_recentAt = (_recentAt + 1) % _recent.Length;
				table[opcode](engine);
				return;
			}
			lock (_reported)
			{
				if (_reported.Add((int)opcode + 100000))
				{
					// The commands before it, oldest first: the one that read too little or too much is among them.
					List<string> recent = new List<string>();
					for (int i = 0; i < _recent.Length; i++)
					{
						int op = _recent[(_recentAt + i) % _recent.Length];
						if (op < 0) continue;
						ScriptOp known = GameProfile.IsFf4 ? ScriptOpTable.Ff4.Get(op) : ScriptOpTable.Ff3.Get(op);
						recent.Add(op + (known != null ? " " + ScriptOpTable.Simplify(known.Name) : ""));
					}
					Log.Write(LogChannel.General, "script: opcode " + opcode + " is outside the " + GameProfile.Game + " table; ending the script (after: " + string.Join(" > ", recent) + ")");
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
			// FF4 renamed a few commands it kept: the same operands, FF3's handler.
			Dictionary<string, string> aliases = new Dictionary<string, string>(StringComparer.Ordinal)
			{
				{ "startMessage", "startMessage2" },
				{ "deleteMessage", "deleteMessage2" },
			};
			Dictionary<string, int> ff3ByName = new Dictionary<string, int>(StringComparer.Ordinal);
			for (int i = 0; i < ff3.Count && i < GlobalScope.commandTable.Length; i++)
			{
				ScriptOp op = ff3.Get(i);
				if (op != null)
				{
					ff3ByName[ScriptOpTable.Simplify(op.Name)] = i;
				}
			}
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
				else if (op != null && Ff4Commands.ByName.TryGetValue(ScriptOpTable.Simplify(op.Name), out GlobalScope.SCRIPT_COMMAND own))
				{
					// An FF4 command with an implementation of its own (Ff4Commands), by name.
					table[i] = own;
					_reusedCount++;
				}
				else if (op != null && Ff4Cutscene.ByName.TryGetValue(ScriptOpTable.Simplify(op.Name), out GlobalScope.SCRIPT_COMMAND scene))
				{
					// The cutscene engine's commands, by name.
					table[i] = scene;
					_reusedCount++;
				}
				else if (op != null && theirs != null && i < GlobalScope.commandTable.Length
					&& _extraOperandBytes.TryGetValue(i, out uint extra))
				{
					// FF4 appended operands FF3's handler never reads: run it, step past the rest.
					GlobalScope.SCRIPT_COMMAND handler = GlobalScope.commandTable[i];
					table[i] = engine => { handler(engine); engine.skip(extra); };
					_reusedCount++;
				}
				else if (op != null && aliases.TryGetValue(ScriptOpTable.Simplify(op.Name), out string alias)
					&& ff3ByName.TryGetValue(alias, out int ff3Index)
					&& SameOperands(op, ff3.Get(ff3Index)))
				{
					table[i] = GlobalScope.commandTable[ff3Index];
					_reusedCount++;
				}
				else if (op != null && ff3ByName.TryGetValue(ScriptOpTable.Simplify(op.Name), out int sameName)
					&& sameName != i && SameOperands(op, ff3.Get(sameName)))
				{
					// The same command under another number: FF3's handler, FF4's place in the table.
					table[i] = GlobalScope.commandTable[sameName];
					_reusedCount++;
				}
				else
				{
					int opcode = i;
					table[i] = engine => Skip(engine, opcode, op);
				}
			}
			Log.Write(LogChannel.General, "script: FF4 table built - " + _reusedCount + " of " + table.Length + " commands run FF3's handler, the rest are skipped with their operands");
			if (Options.Get("ff4table") != null)
			{
				// --ff4table: every command that is only skipped, by number and name, for the porting list.
				List<string> skipped = new List<string>();
				for (int i = 0; i < table.Length; i++)
				{
					ScriptOp op = ff4.Get(i);
					if (op == null) continue;
					int opcode = i;
					bool runs = (ff3.Get(i) != null && i < GlobalScope.commandTable.Length && SameCommand(op, ff3.Get(i)))
						|| Ff4Commands.ByName.ContainsKey(ScriptOpTable.Simplify(op.Name)) || Ff4Cutscene.ByName.ContainsKey(ScriptOpTable.Simplify(op.Name)) || _extraOperandBytes.ContainsKey(i)
						|| (aliases.TryGetValue(ScriptOpTable.Simplify(op.Name), out string a) && ff3ByName.ContainsKey(a));
					if (!runs) skipped.Add(i + " " + ScriptOpTable.Simplify(op.Name));
				}
				Log.Write(LogChannel.General, "script: FF4 skipped: " + string.Join(", ", skipped));
			}
			_ff4 = table;
			return table;
		}

		private static bool SameCommand(ScriptOp mine, ScriptOp theirs)
		{
			// flagON is FF3's flagOn: the same command, another hand on the keyboard.
			if (!string.Equals(ScriptOpTable.Simplify(mine.Name), ScriptOpTable.Simplify(theirs.Name), StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			return SameOperands(mine, theirs);
		}

		private static bool SameOperands(ScriptOp mine, ScriptOp theirs)
		{
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
			bool quiet = op != null && (Ff4Commands.Cosmetic.Contains(ScriptOpTable.Simplify(op.Name)) || Ff4Cutscene.Quiet.Contains(ScriptOpTable.Simplify(op.Name)));
			lock (_reported)
			{
				if (!quiet && _reported.Add(opcode))
				{
					Log.Write(LogChannel.General, "script: FF4 command " + opcode + " " + (op != null ? ScriptOpTable.Simplify(op.Name) : "?") + " not implemented - skipped");
				}
			}
		}
	}
}
