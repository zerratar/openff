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
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public enum GXVRamTex
	{
		GX_VRAM_TEX_NONE = 0,
		GX_VRAM_TEX_0_A = 1,
		GX_VRAM_TEX_0_B = 2,
		GX_VRAM_TEX_0_C = 4,
		GX_VRAM_TEX_0_D = 8,
		GX_VRAM_TEX_01_AB = 3,
		GX_VRAM_TEX_01_BC = 6,
		GX_VRAM_TEX_01_CD = 12,
		GX_VRAM_TEX_012_ABC = 7,
		GX_VRAM_TEX_012_BCD = 14,
		GX_VRAM_TEX_0123_ABCD = 15,
		GX_VRAM_TEX_01_AC = 5,
		GX_VRAM_TEX_01_AD = 9,
		GX_VRAM_TEX_01_BD = 10,
		GX_VRAM_TEX_012_ABD = 11,
		GX_VRAM_TEX_012_ACD = 13
	}
}
