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
	public enum GXPlaneMask
	{
		GX_PLANEMASK_NONE = 0,
		GX_PLANEMASK_BG0 = 1,
		GX_PLANEMASK_BG1 = 2,
		GX_PLANEMASK_BG2 = 4,
		GX_PLANEMASK_BG3 = 8,
		GX_PLANEMASK_OBJ = 0x10
	}
}
