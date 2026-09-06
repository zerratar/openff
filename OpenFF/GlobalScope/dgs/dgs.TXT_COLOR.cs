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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class dgs
	{
		public enum TXT_COLOR
		{
			TXT_COLOR_NULL = 0,
			TXT_COLOR_WHITE = 1,
			TXT_COLOR_BLACK = 2,
			TXT_COLOR_RED = 3,
			TXT_COLOR_GREEN = 4,
			TXT_COLOR_BLUE = 5,
			TXT_COLOR_CYAN = 6,
			TXT_COLOR_MAGENTA = 7,
			TXT_COLOR_YELLOW = 8,
			TXT_COLOR_PALLID_YELLOW = 9,
			TXT_COLOR_PALLID_BLUE = 10,
			TXT_COLOR_PALLID_RED = 11,
			TXT_UCOLOR_3 = 12,
			TXT_UCOLOR_4 = 13,
			TXT_UCOLOR_5 = 14,
			TXT_UCOLOR_6 = 15,
			NUMBER_OF_TXT_COLOR = 16,
			TXT_COLOR_DISABLE = TXT_UCOLOR_4
		}
	}
}
