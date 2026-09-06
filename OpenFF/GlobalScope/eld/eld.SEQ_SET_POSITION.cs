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
		public class SEQ_SET_POSITION
		{
			public uint command;

			public float[] pos = new float[3];

			public static explicit operator SEQ_SET_POSITION(ArrayReader src)
			{
				SEQ_SET_POSITION sEQ_SET_POSITION = new SEQ_SET_POSITION();
				sEQ_SET_POSITION.pos[0] = src.readSingle();
				sEQ_SET_POSITION.pos[1] = src.readSingle();
				sEQ_SET_POSITION.pos[2] = src.readSingle();
				return sEQ_SET_POSITION;
			}
		}
	}
}
