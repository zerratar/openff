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
		public static class ut
		{
			internal static void setColorToPrimitive(ds.pt.Particle prim, spr.Eff_FRGBA color)
			{
				prim.Color[0] = (short)color.red;
				prim.Color[1] = (short)color.green;
				prim.Color[2] = (short)color.blue;
				prim.Color[3] = (short)color.alpha;
			}

			internal static void setColorToPrimitive(ds.pt.LargeParticle prim, spr.Eff_FRGBA color)
			{
				prim.Color[0] = (short)color.red;
				prim.Color[1] = (short)color.green;
				prim.Color[2] = (short)color.blue;
				prim.Color[3] = (short)color.alpha;
			}
		}
	}
}
