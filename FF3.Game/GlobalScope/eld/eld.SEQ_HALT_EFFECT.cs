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
		public class SEQ_HALT_EFFECT
		{
			public uint command;

			public uint index;

			public uint _id;

			public uint res;

			public static explicit operator SEQ_HALT_EFFECT(ArrayReader src)
			{
				SEQ_HALT_EFFECT sEQ_HALT_EFFECT = new SEQ_HALT_EFFECT();
				sEQ_HALT_EFFECT.command = src.readUInt32();
				sEQ_HALT_EFFECT.index = src.readUInt32();
				sEQ_HALT_EFFECT._id = src.readUInt32();
				sEQ_HALT_EFFECT.res = src.readUInt32();
				return sEQ_HALT_EFFECT;
			}
		}
	}
}
