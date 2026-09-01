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
	public enum GXBGMode
	{
		GX_BGMODE_0,
		GX_BGMODE_1,
		GX_BGMODE_2,
		GX_BGMODE_3,
		GX_BGMODE_4,
		GX_BGMODE_5,
		GX_BGMODE_6
	}
}
