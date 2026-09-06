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
		public class SizeSpreadSetup
		{
			private int nSizeAdd;

			private int[] nReserve = new int[3];

			public static explicit operator SizeSpreadSetup(ArrayReader src)
			{
				SizeSpreadSetup sizeSpreadSetup = new SizeSpreadSetup();
				sizeSpreadSetup.nSizeAdd = src.readInt32();
				sizeSpreadSetup.nReserve[0] = src.readInt32();
				sizeSpreadSetup.nReserve[1] = src.readInt32();
				sizeSpreadSetup.nReserve[2] = src.readInt32();
				return sizeSpreadSetup;
			}
		}
	}
}
