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
	public static partial class eld
	{
		public class PrimParticle
		{
			public int[] Center = new int[3];

			public short[] Size = new short[2];

			public short[] Color = new short[4];

			public int[,] St = new int[2, 2];

			public short Disp;

			public ushort PolygonID;
		}
	}
}
