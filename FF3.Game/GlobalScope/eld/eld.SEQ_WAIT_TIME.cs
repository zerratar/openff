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
		public class SEQ_WAIT_TIME
		{
			public uint command;

			public uint time;

			public uint[] res = new uint[2];

			public static explicit operator SEQ_WAIT_TIME(ArrayReader src)
			{
				SEQ_WAIT_TIME sEQ_WAIT_TIME = new SEQ_WAIT_TIME();
				sEQ_WAIT_TIME.command = src.readUInt32();
				sEQ_WAIT_TIME.time = src.readUInt32();
				sEQ_WAIT_TIME.res[0] = src.readUInt32();
				sEQ_WAIT_TIME.res[1] = src.readUInt32();
				return sEQ_WAIT_TIME;
			}
		}
	}
}
