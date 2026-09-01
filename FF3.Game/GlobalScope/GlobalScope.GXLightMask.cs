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
	public enum GXLightMask
	{
		GX_LIGHTMASK_NONE,
		GX_LIGHTMASK_0,
		GX_LIGHTMASK_1,
		GX_LIGHTMASK_01,
		GX_LIGHTMASK_2,
		GX_LIGHTMASK_02,
		GX_LIGHTMASK_12,
		GX_LIGHTMASK_012,
		GX_LIGHTMASK_3,
		GX_LIGHTMASK_03,
		GX_LIGHTMASK_13,
		GX_LIGHTMASK_013,
		GX_LIGHTMASK_23,
		GX_LIGHTMASK_023,
		GX_LIGHTMASK_123,
		GX_LIGHTMASK_0123
	}
}
