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
	public class Vertex
	{
		public float tex0;

		public float tex1;

		public float pos0;

		public float pos1;

		public float pos2;

		public byte clr0;

		public byte clr1;

		public byte clr2;

		public byte clr3;

		public void copy(Vertex src)
		{
			pos0 = src.pos0;
			pos1 = src.pos1;
			pos2 = src.pos2;
			tex0 = src.tex0;
			tex1 = src.tex1;
			clr0 = src.clr0;
			clr1 = src.clr1;
			clr2 = src.clr2;
			clr3 = src.clr3;
		}
	}
}
