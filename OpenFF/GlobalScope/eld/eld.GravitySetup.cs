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
		public class GravitySetup
		{
			public ds.Vector3<int> vGravityDir;

			public int nGravityPow;

			public int nGravityRand;

			public static explicit operator GravitySetup(ArrayReader src)
			{
				GravitySetup gravitySetup = new GravitySetup();
				gravitySetup.vGravityDir = new ds.Vector3<int>();
				gravitySetup.vGravityDir.vx = src.readInt32();
				gravitySetup.vGravityDir.vy = src.readInt32();
				gravitySetup.vGravityDir.vz = src.readInt32();
				gravitySetup.nGravityPow = src.readInt32();
				gravitySetup.nGravityRand = src.readInt32();
				return gravitySetup;
			}
		}
	}
}
