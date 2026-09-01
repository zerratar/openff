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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public class NNSG3dAnmObj
	{
		public int frame;

		public int ratio;

		public NNSG3dResAnmHeader resAnm;

		public Array funcAnm;

		public NNSG3dAnmObj next;

		public NNSG3dResTex resTex;

		public byte priority;

		public byte numMapData;

		public ushort[] mapData = new ushort[1];

		public void destruct()
		{
		}

		public void setDefault()
		{
			frame = 0;
			ratio = 0;
			resAnm = null;
			funcAnm = null;
			next = null;
			resTex = null;
			priority = 0;
			numMapData = 0;
			mapData[0] = 0;
		}
	}
}
