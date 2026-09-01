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
	public static partial class mognet
	{
		public class SCRSTRUCT
		{
			public Array ptr;

			public NNSG2dScreenData scr;

			public NNSG2dCellDataBank cell;

			public SCRSTRUCT(Array arg0, NNSG2dScreenData arg1, NNSG2dCellDataBank arg2)
			{
				ptr = arg0;
				scr = arg1;
				cell = arg2;
			}
		}
	}
}
