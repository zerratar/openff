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
	public static partial class eld
	{
		public class EmmitSetup
		{
			public ds.Vector3<int> vEmmitAngle;

			public static explicit operator EmmitSetup(ArrayReader src)
			{
				EmmitSetup emmitSetup = new EmmitSetup();
				emmitSetup.vEmmitAngle = new ds.Vector3<int>();
				emmitSetup.vEmmitAngle.vx = src.readInt32();
				emmitSetup.vEmmitAngle.vy = src.readInt32();
				emmitSetup.vEmmitAngle.vz = src.readInt32();
				return emmitSetup;
			}
		}
	}
}
