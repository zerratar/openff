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
	public class NNSG2dCharCanvas
	{
		public byte[] charBase;

		public int areaWidth;

		public int areaHeight;

		public byte dstBpp;

		public byte[] reserved = new byte[3];

		public uint param;

		public NNSiG2dCharCanvasVTable vtable;

		public int lcd;

		public int charBase_idx;
	}
}
