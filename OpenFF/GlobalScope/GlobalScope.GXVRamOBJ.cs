using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using android.content;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public enum GXVRamOBJ
	{
		GX_VRAM_OBJ_NONE = 0,
		GX_VRAM_OBJ_16_F = 32,
		GX_VRAM_OBJ_16_G = 64,
		GX_VRAM_OBJ_32_FG = 96,
		GX_VRAM_OBJ_64_E = 16,
		GX_VRAM_OBJ_80_EF = 48,
		GX_VRAM_OBJ_80_EG = 80,
		GX_VRAM_OBJ_96_EFG = 112,
		GX_VRAM_OBJ_128_A = 1,
		GX_VRAM_OBJ_128_B = 2,
		GX_VRAM_OBJ_256_AB = 3
	}
}
