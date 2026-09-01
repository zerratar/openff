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
	public static partial class menu
	{
		public enum OPTION_LINE
		{
			OL_MES,
			OL_CUR,
			OL_BGM,
			OL_SE,
			OL_SMD,
			OL_MOV,
			OL_MENU,
			OL_FIX,
			OL_TITLE,
			OL_CONFIRM
		}
	}
}
