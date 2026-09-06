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
	public enum GXVRamSubBG
	{
		GX_VRAM_SUB_BG_NONE = 0,
		GX_VRAM_SUB_BG_128_C = 4,
		GX_VRAM_SUB_BG_32_H = 128,
		GX_VRAM_SUB_BG_48_HI = 384
	}
}
