// The DS geometry engine's command set, as far as a display list walker needs it.
//
// FF3's models only ever put vertex-run commands inside a shape's display list, so the
// phone port's decoder terminated on anything else. FF4's models carry matrix,
// polygon-attribute and texture-parameter commands between vertex runs (the editor's
// Mdl0 found the same); those are stepped past by their fixed parameter counts.

using System.Collections.Generic;

namespace FF3
{
	internal static class GxCommands
	{
		private static readonly HashSet<int> _reported = new HashSet<int>();

		/// <summary>Parameter words a GX command takes, or -1 for a byte that is not a command.</summary>
		public static int ParameterWords(int command)
		{
			switch (command)
			{
				case 0x00: return 0;                     // NOP
				case 0x10: return 1;                     // MTX_MODE
				case 0x11: return 0;                     // MTX_PUSH
				case 0x12: return 1;                     // MTX_POP
				case 0x13: return 1;                     // MTX_STORE
				case 0x14: return 1;                     // MTX_RESTORE
				case 0x15: return 0;                     // MTX_IDENTITY
				case 0x16: return 16;                    // MTX_LOAD_4x4
				case 0x17: return 12;                    // MTX_LOAD_4x3
				case 0x18: return 16;                    // MTX_MULT_4x4
				case 0x19: return 12;                    // MTX_MULT_4x3
				case 0x1A: return 9;                     // MTX_MULT_3x3
				case 0x1B: return 3;                     // MTX_SCALE
				case 0x1C: return 3;                     // MTX_TRANS
				case 0x20: return 1;                     // COLOR
				case 0x21: return 1;                     // NORMAL
				case 0x22: return 1;                     // TEXCOORD
				case 0x23: return 2;                     // VTX_16
				case 0x24: return 1;                     // VTX_10
				case 0x25: return 1;                     // VTX_XY
				case 0x26: return 1;                     // VTX_XZ
				case 0x27: return 1;                     // VTX_YZ
				case 0x28: return 1;                     // VTX_DIFF
				case 0x29: return 1;                     // POLYGON_ATTR
				case 0x2A: return 1;                     // TEXIMAGE_PARAM
				case 0x2B: return 1;                     // PLTT_BASE
				case 0x2C: return 2;                     // FF4's 16.16 texture coordinate
				case 0x30: return 1;                     // DIF_AMB
				case 0x31: return 1;                     // SPE_EMI
				case 0x32: return 1;                     // LIGHT_VECTOR
				case 0x33: return 1;                     // LIGHT_COLOR
				case 0x34: return 32;                    // SHININESS
				case 0x40: return 1;                     // BEGIN_VTXS
				case 0x41: return 0;                     // END_VTXS
				case 0x50: return 1;                     // SWAP_BUFFERS
				case 0x60: return 1;                     // VIEWPORT
				case 0x70: return 3;                     // BOX_TEST
				case 0x71: return 2;                     // POS_TEST
				case 0x72: return 1;                     // VEC_TEST
				default: return -1;
			}
		}

		/// <summary>
		/// Words to step past for a command the shape decoder has no use for: its parameter
		/// count, logged the first time each command turns up. A byte that is no command
		/// at all is reported and treated as taking no parameters, so the walk still ends.
		/// </summary>
		public static int Skip(int command)
		{
			int words = ParameterWords(command);
			lock (_reported)
			{
				if (_reported.Add(command))
				{
					Log.Write(LogChannel.General, words >= 0
						? "model: display list command 0x" + command.ToString("X2") + " stepped past (" + words + " words)"
						: "model: display list byte 0x" + command.ToString("X2") + " is not a GX command");
				}
			}
			return words < 0 ? 0 : words;
		}
	}
}
