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
	public class TexBank
	{
		public int w;

		public int h;

		public byte[] data;

		public uint[] tex = new uint[1];

		public uint wrap;

		public uint filter;

		public uint format;

		public void setDefault()
		{
			w = 0;
			h = 0;
			data = null;
			tex[0] = 0u;
			wrap = 0u;
			filter = 0u;
			format = 0u;
		}
	}
}
