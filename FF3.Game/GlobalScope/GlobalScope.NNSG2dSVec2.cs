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
	public class NNSG2dSVec2
	{
		public short x;

		public short y;

		public NNSG2dSVec2()
		{
		}

		public NNSG2dSVec2(NNSG2dSVec2 src)
		{
			copy(src);
		}

		public NNSG2dSVec2(short arg0, short arg1)
		{
			x = arg0;
			y = arg1;
		}

		public void copy(NNSG2dSVec2 src)
		{
			x = src.x;
			y = src.y;
		}
	}
}
