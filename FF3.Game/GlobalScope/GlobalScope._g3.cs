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
	public class _g3
	{
		public GXBegin primitive;

		public VecFx32 pos = new VecFx32();

		public float[] tex = new float[2];

		public byte[] clr = new byte[4];

		public Vertex[] vertex = new Vertex[4];

		public int index;

		public _g3()
		{
			for (int i = 0; i < vertex.Length; i++)
			{
				vertex[i] = new Vertex();
			}
		}
	}
}
