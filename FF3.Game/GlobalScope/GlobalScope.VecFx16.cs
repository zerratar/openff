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
	public class VecFx16
	{
		public short x;

		public short y;

		public short z;

		public VecFx16()
		{
		}

		public VecFx16(short arg0, short arg1, short arg2)
		{
			x = arg0;
			y = arg1;
			z = arg2;
		}
	}
}
