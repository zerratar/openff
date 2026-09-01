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
	public static partial class eld
	{
		public enum enPATH_FLAG
		{
			enPATH_FLAG_TYPE_LINE = 0,
			enPATH_FLAG_TYPE_CURVE = 1,
			enPATH_FLAG_TYPE_ITURN = 2,
			enPATH_FLAG_TYPE_UTURN = 4,
			enPATH_FLAG_TYPE_LOOP = 8,
			enPATH_FLAG_TYPE_LOCAL = 0x10,
			enPATH_FLAG_TYPE_WORLD = 0x20,
			enPATH_FLAG_SET_FIGURE = 0x40,
			enPATH_FLAG_ADD_FIGURE = 0x80
		}
	}
}
