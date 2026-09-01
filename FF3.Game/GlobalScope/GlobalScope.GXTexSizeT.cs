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
	public enum GXTexSizeT
	{
		GX_TEXSIZE_T8,
		GX_TEXSIZE_T16,
		GX_TEXSIZE_T32,
		GX_TEXSIZE_T64,
		GX_TEXSIZE_T128,
		GX_TEXSIZE_T256,
		GX_TEXSIZE_T512,
		GX_TEXSIZE_T1024
	}
}
