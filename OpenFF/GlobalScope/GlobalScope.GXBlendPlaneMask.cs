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
	public enum GXBlendPlaneMask
	{
		GX_BLEND_PLANEMASK_NONE = 0,
		GX_BLEND_PLANEMASK_BG0 = 1,
		GX_BLEND_PLANEMASK_BG1 = 2,
		GX_BLEND_PLANEMASK_BG2 = 4,
		GX_BLEND_PLANEMASK_BG3 = 8,
		GX_BLEND_PLANEMASK_OBJ = 0x10,
		GX_BLEND_PLANEMASK_BD = 0x20
	}
}
