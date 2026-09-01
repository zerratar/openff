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
	public class NNSG2dImageProxy
	{
		public NNSG2dImageAttr attr = new NNSG2dImageAttr();

		public ushort W;

		public ushort H;

		public uint characterFmt;

		public IMAGE_TABLE data;

		public void copy(NNSG2dImageProxy src)
		{
			attr.copy(src.attr);
			W = src.W;
			H = src.H;
			characterFmt = src.characterFmt;
			data = src.data;
		}

		public void setDefault()
		{
			attr.setDefault();
			W = 0;
			H = 0;
			characterFmt = 0u;
			data = null;
		}
	}
}
