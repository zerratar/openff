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
	public static partial class eld
	{
		public static class path
		{
			internal static uint typeLine(uint f)
			{
				return f & 1;
			}

			internal static uint typeMove(uint f)
			{
				return f & 0xE;
			}

			internal static uint typeCoord(uint f)
			{
				return f & 0x30;
			}

			internal static uint flagFigure(uint f)
			{
				return f & 0x40;
			}

			internal static uint flagAddFigure(uint f)
			{
				return f & 0x80;
			}
		}
	}
}
