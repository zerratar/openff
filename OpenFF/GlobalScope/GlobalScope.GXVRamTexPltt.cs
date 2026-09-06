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
	public enum GXVRamTexPltt
	{
		GX_VRAM_TEXPLTT_NONE = 0,
		GX_VRAM_TEXPLTT_0_F = 32,
		GX_VRAM_TEXPLTT_0_G = 64,
		GX_VRAM_TEXPLTT_01_FG = 96,
		GX_VRAM_TEXPLTT_0123_E = 16,
		GX_VRAM_TEXPLTT_01234_EF = 48,
		GX_VRAM_TEXPLTT_012345_EFG = 112
	}
}
