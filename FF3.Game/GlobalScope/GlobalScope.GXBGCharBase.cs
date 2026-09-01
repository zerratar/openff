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
	public enum GXBGCharBase
	{
		GX_BG_CHARBASE_0x00000,
		GX_BG_CHARBASE_0x04000,
		GX_BG_CHARBASE_0x08000,
		GX_BG_CHARBASE_0x0c000,
		GX_BG_CHARBASE_0x10000,
		GX_BG_CHARBASE_0x14000,
		GX_BG_CHARBASE_0x18000,
		GX_BG_CHARBASE_0x1c000,
		GX_BG_CHARBASE_0x20000,
		GX_BG_CHARBASE_0x24000,
		GX_BG_CHARBASE_0x28000,
		GX_BG_CHARBASE_0x2c000,
		GX_BG_CHARBASE_0x30000,
		GX_BG_CHARBASE_0x34000,
		GX_BG_CHARBASE_0x38000,
		GX_BG_CHARBASE_0x3c000
	}
}
