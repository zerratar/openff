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
	public enum GXPolygonAttrMisc
	{
		GX_POLYGON_ATTR_MISC_NONE = 0,
		GX_POLYGON_ATTR_MISC_XLU_DEPTH_UPDATE = 0x800,
		GX_POLYGON_ATTR_MISC_FAR_CLIPPING = 0x1000,
		GX_POLYGON_ATTR_MISC_DISP_1DOT = 0x2000,
		GX_POLYGON_ATTR_MISC_DEPTHTEST_DECAL = 0x4000,
		GX_POLYGON_ATTR_MISC_FOG = 0x8000
	}
}
