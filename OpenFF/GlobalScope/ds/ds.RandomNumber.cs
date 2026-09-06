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
	public static partial class ds
	{
		public class RandomNumber
		{
			public static MATHRandContext16 _s_ctxt16;

			public static MATHRandContext32 _s_ctxt32;

			public static void init(uint seed16, ulong seed32)
			{
				MATH_InitRand16(_s_ctxt16, seed16);
				MATH_InitRand32(_s_ctxt32, seed32);
			}

			public static ushort rand16(ushort max)
			{
				return MATH_Rand16(_s_ctxt16, max);
			}

			public static uint rand32(uint max)
			{
				return MATH_Rand32(_s_ctxt32, max);
			}
		}
	}
}
