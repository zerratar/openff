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
	public static partial class dgs
	{
		public class MSFINFO
		{
			private static int NUMBER_OF_FONTS = 10;

			private static int NUMBER_OF_COLORS = 16;

			private uint type;

			private uint reserved1;

			private uint reserved2;

			private uint converted;

			private FONTHEADER[] font_header = new FONTHEADER[NUMBER_OF_FONTS];

			private uint[,] palette = new uint[NUMBER_OF_FONTS, NUMBER_OF_COLORS];

			private FONTINFO[] f_infos = new FONTINFO[0];
		}
	}
}
