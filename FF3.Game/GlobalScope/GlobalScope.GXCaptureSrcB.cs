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
	public enum GXCaptureSrcB
	{
		GX_CAPTURE_SRCB_VRAM_0x00000 = 0,
		GX_CAPTURE_SRCB_MRAM = 1,
		GX_CAPTURE_SRCB_VRAM_0x08000 = 2,
		GX_CAPTURE_SRCB_VRAM_0x10000 = 4,
		GX_CAPTURE_SRCB_VRAM_0x18000 = 6
	}
}
