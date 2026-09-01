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
		public enum DS2D_OBJ_PLANE
		{
			DS2D_OBJ_PLANE_MAIN3D,
			DS2D_OBJ_PLANE_MAIN2D,
			DS2D_OBJ_PLANE_SUB2D,
			DS2D_OBJ_PLANE_MAX
		}
	}
}
