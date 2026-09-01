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
	public enum GXBGScrBase
	{
		GX_BG_SCRBASE_0x0000,
		GX_BG_SCRBASE_0x0800,
		GX_BG_SCRBASE_0x1000,
		GX_BG_SCRBASE_0x1800,
		GX_BG_SCRBASE_0x2000,
		GX_BG_SCRBASE_0x2800,
		GX_BG_SCRBASE_0x3000,
		GX_BG_SCRBASE_0x3800,
		GX_BG_SCRBASE_0x4000,
		GX_BG_SCRBASE_0x4800,
		GX_BG_SCRBASE_0x5000,
		GX_BG_SCRBASE_0x5800,
		GX_BG_SCRBASE_0x6000,
		GX_BG_SCRBASE_0x6800,
		GX_BG_SCRBASE_0x7000,
		GX_BG_SCRBASE_0x7800,
		GX_BG_SCRBASE_0x8000,
		GX_BG_SCRBASE_0x8800,
		GX_BG_SCRBASE_0x9000,
		GX_BG_SCRBASE_0x9800,
		GX_BG_SCRBASE_0xa000,
		GX_BG_SCRBASE_0xa800,
		GX_BG_SCRBASE_0xb000,
		GX_BG_SCRBASE_0xb800,
		GX_BG_SCRBASE_0xc000,
		GX_BG_SCRBASE_0xc800,
		GX_BG_SCRBASE_0xd000,
		GX_BG_SCRBASE_0xd800,
		GX_BG_SCRBASE_0xe000,
		GX_BG_SCRBASE_0xe800,
		GX_BG_SCRBASE_0xf000,
		GX_BG_SCRBASE_0xf800
	}
}
