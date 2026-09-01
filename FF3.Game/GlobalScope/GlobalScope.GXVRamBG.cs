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
using android.text;
using android.widget;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public enum GXVRamBG
	{
		GX_VRAM_BG_NONE = 0,
		GX_VRAM_BG_16_F = 32,
		GX_VRAM_BG_16_G = 64,
		GX_VRAM_BG_32_FG = 96,
		GX_VRAM_BG_64_E = 16,
		GX_VRAM_BG_80_EF = 48,
		GX_VRAM_BG_96_EFG = 112,
		GX_VRAM_BG_128_A = 1,
		GX_VRAM_BG_128_B = 2,
		GX_VRAM_BG_128_C = 4,
		GX_VRAM_BG_128_D = 8,
		GX_VRAM_BG_256_AB = 3,
		GX_VRAM_BG_256_BC = 6,
		GX_VRAM_BG_256_CD = 12,
		GX_VRAM_BG_384_ABC = 7,
		GX_VRAM_BG_384_BCD = 14,
		GX_VRAM_BG_512_ABCD = 15,
		GX_VRAM_BG_80_EG = 80,
		GX_VRAM_BG_256_AC = 5,
		GX_VRAM_BG_256_AD = 9,
		GX_VRAM_BG_256_BD = 10,
		GX_VRAM_BG_384_ABD = 11,
		GX_VRAM_BG_384_ACD = 13
	}
}
