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
	public enum GXVRamBGExtPltt
	{
		GX_VRAM_BGEXTPLTT_NONE = 0,
		GX_VRAM_BGEXTPLTT_01_F = 32,
		GX_VRAM_BGEXTPLTT_23_G = 64,
		GX_VRAM_BGEXTPLTT_0123_E = 16,
		GX_VRAM_BGEXTPLTT_0123_FG = 96
	}
}
