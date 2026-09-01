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
		public class DelayedEraseArea
		{
			public NNSG2dCharCanvas canvas;

			public NNSG2dFont font;

			public DGSMessage.EraseArea area = new DGSMessage.EraseArea();

			public DelayedEraseArea(NNSG2dCharCanvas arg0, NNSG2dFont arg1, DGSMessage.EraseArea arg2)
			{
				canvas = arg0;
				font = arg1;
				area.copy(arg2);
			}
		}
	}
}
