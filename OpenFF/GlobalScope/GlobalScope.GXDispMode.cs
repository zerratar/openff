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
	public enum GXDispMode
	{
		GX_DISPMODE_GRAPHICS = 1,
		GX_DISPMODE_VRAM_A = 2,
		GX_DISPMODE_VRAM_B = 6,
		GX_DISPMODE_VRAM_C = 10,
		GX_DISPMODE_VRAM_D = 14,
		GX_DISPMODE_MMEM = 3
	}
}
