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
	public static partial class eld
	{
		public class RangeSetup
		{
			public ds.Vector3<int> vRangeBirth;

			public static explicit operator RangeSetup(ArrayReader src)
			{
				RangeSetup rangeSetup = new RangeSetup();
				rangeSetup.vRangeBirth = new ds.Vector3<int>();
				rangeSetup.vRangeBirth.vx = src.readInt32();
				rangeSetup.vRangeBirth.vy = src.readInt32();
				rangeSetup.vRangeBirth.vz = src.readInt32();
				return rangeSetup;
			}
		}
	}
}
