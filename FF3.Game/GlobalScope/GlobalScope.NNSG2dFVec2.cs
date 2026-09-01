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
	public class NNSG2dFVec2
	{
		public int x;

		public int y;

		public NNSG2dFVec2()
		{
		}

		public NNSG2dFVec2(NNSG2dFVec2 src)
		{
			copy(src);
		}

		public NNSG2dFVec2(int arg0, int arg1)
		{
			x = arg0;
			y = arg1;
		}

		public void copy(NNSG2dFVec2 src)
		{
			x = src.x;
			y = src.y;
		}
	}
}
