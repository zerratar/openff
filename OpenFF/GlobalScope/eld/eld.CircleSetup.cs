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
		public class CircleSetup
		{
			public int nRadius;

			public int nRadiusAdd;

			public int nAngleAdd;

			public static explicit operator CircleSetup(ArrayReader src)
			{
				CircleSetup circleSetup = new CircleSetup();
				circleSetup.nRadius = src.readInt32();
				circleSetup.nRadiusAdd = src.readInt32();
				circleSetup.nAngleAdd = src.readInt32();
				return circleSetup;
			}
		}
	}
}
