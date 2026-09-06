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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	public enum GXVRam
	{
		GX_VRAM_A = 1,
		GX_VRAM_B = 2,
		GX_VRAM_C = 4,
		GX_VRAM_D = 8,
		GX_VRAM_E = 16,
		GX_VRAM_F = 32,
		GX_VRAM_G = 64,
		GX_VRAM_H = 128,
		GX_VRAM_I = 256,
		GX_VRAM_ALL = 511
	}
}
