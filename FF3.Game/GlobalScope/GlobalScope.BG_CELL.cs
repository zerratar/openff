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
	public class BG_CELL
	{
		public uint image;

		public float texScaleU;

		public float texScaleV;

		public float scale;

		public byte[] color = new byte[4];

		public int x;

		public int y;

		public short[] oam;

		public int numOAM;

		public NNSG2dBGSelect bg;
	}
}
