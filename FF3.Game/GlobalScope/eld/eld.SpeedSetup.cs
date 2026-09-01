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
		public class SpeedSetup
		{
			public ds.Vector3<int> vSpeedDir;

			public int nSpeedPow;

			public int nSpeedRand;

			public static explicit operator SpeedSetup(ArrayReader src)
			{
				SpeedSetup speedSetup = new SpeedSetup();
				speedSetup.vSpeedDir = new ds.Vector3<int>();
				speedSetup.vSpeedDir.vx = src.readInt32();
				speedSetup.vSpeedDir.vy = src.readInt32();
				speedSetup.vSpeedDir.vz = src.readInt32();
				speedSetup.nSpeedPow = src.readInt32();
				speedSetup.nSpeedRand = src.readInt32();
				return speedSetup;
			}
		}
	}
}
