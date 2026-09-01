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
	public class CastInfo
	{
		public static uint INVALID_SCRIPT = uint.MaxValue;

		private uint castNo_;

		private uint constructor_;

		private uint normal_;

		private uint destructor_;

		public uint getCastNo()
		{
			return castNo_;
		}

		public uint getConstructor()
		{
			return constructor_;
		}

		public uint getNormal()
		{
			return normal_;
		}

		public uint getDestructor()
		{
			return destructor_;
		}

		public void parse(ArrayReader reader)
		{
			castNo_ = reader.readUInt32();
			constructor_ = reader.readUInt32();
			normal_ = reader.readUInt32();
			destructor_ = reader.readUInt32();
		}
	}
}
