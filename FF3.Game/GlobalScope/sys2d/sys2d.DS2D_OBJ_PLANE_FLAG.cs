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
	public static partial class sys2d
	{
		public enum DS2D_OBJ_PLANE_FLAG
		{
			DS2D_OBJ_PLANE_FLAG_MAIN3D = 1,
			DS2D_OBJ_PLANE_FLAG_MAIN2D = 2,
			DS2D_OBJ_PLANE_FLAG_SUB2D = 4
		}
	}
}
