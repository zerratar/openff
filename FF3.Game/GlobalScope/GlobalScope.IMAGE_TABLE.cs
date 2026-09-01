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
	public class IMAGE_TABLE
	{
		public uint tex;

		public float texScaleU;

		public float texScaleV;

		public static explicit operator IMAGE_TABLE(Array src)
		{
			IMAGE_TABLE iMAGE_TABLE = new IMAGE_TABLE();
			ArrayReader arrayReader = new ArrayReader(src);
			iMAGE_TABLE.tex = arrayReader.readUInt32();
			iMAGE_TABLE.texScaleU = arrayReader.readUInt32();
			iMAGE_TABLE.texScaleV = arrayReader.readUInt32();
			arrayReader.dispose();
			return iMAGE_TABLE;
		}

		public void setDefault()
		{
			tex = 0u;
			texScaleU = 0f;
			texScaleV = 0f;
		}
	}
}
