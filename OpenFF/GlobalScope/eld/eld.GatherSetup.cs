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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	public static partial class eld
	{
		public class GatherSetup
		{
			public int nSpeedPow;

			public int nSpeedAdd;

			public ds.Vector3<int> vRotate;

			public static explicit operator GatherSetup(ArrayReader src)
			{
				GatherSetup gatherSetup = new GatherSetup();
				gatherSetup.nSpeedPow = src.readInt32();
				gatherSetup.nSpeedAdd = src.readInt32();
				gatherSetup.vRotate = new ds.Vector3<int>();
				gatherSetup.vRotate.vx = src.readInt32();
				gatherSetup.vRotate.vy = src.readInt32();
				gatherSetup.vRotate.vz = src.readInt32();
				return gatherSetup;
			}
		}
	}
}
