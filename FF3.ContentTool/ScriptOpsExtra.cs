// Opcodes past the end of FF3's table, from the game that shares its engine.
//
// ScriptOps.cs is generated from FF3's decompiled dispatch table and must not be edited
// by hand. This file is the hand-written part: what the other game adds. FF4 has 451
// game commands to FF3's 264 and numbers its own past FF3's range, so nothing here can
// collide with an FF3 script - FF3 has no instruction 333, and never will.
//
// Each entry was measured rather than read. There is no FF4 source to decompile; its
// engine is native, and while libff4.so keeps its symbol names, an operand list is not
// a symbol. So the shape comes from the scripts themselves: 658 instances of 0x014D
// across 369 FF4 maps all carry two strings and then exactly ten S32s, and the ten
// decode as an arrival position, a facing in eighths of a turn, and the two corners of
// the door's trigger box on the current map. The name is the handler's, from the
// symbol table: babilCommand_SetInsideMapJump.
//
// Add to this as more of FF4's opcodes are pinned down the same way.

using System.Collections.Generic;

namespace FF3.ContentTool
{
	internal static class ScriptOpsExtra
	{
		/// <summary>FF4's exit declaration: trigger, map, arrival x y z, facing, box corners.</summary>
		public const int SetInsideMapJump = 333;

		public static readonly Dictionary<int, ScriptOp> All = new Dictionary<int, ScriptOp>
		{
			[SetInsideMapJump] = new ScriptOp("babilCommand_SetInsideMapJump", new[]
			{
				Operand.String, Operand.String,
				Operand.Dword, Operand.Dword, Operand.Dword,
				Operand.Dword,
				Operand.Dword, Operand.Dword, Operand.Dword,
				Operand.Dword, Operand.Dword, Operand.Dword
			}, false, -1),
		};

		public static bool Known(int opcode)
		{
			return All.ContainsKey(opcode);
		}

		public static ScriptOp Get(int opcode)
		{
			return All.TryGetValue(opcode, out ScriptOp op) ? op : null;
		}
	}
}
