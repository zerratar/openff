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
	public class VecFx32
	{
		public int x;

		public int y;

		public int z;

		public int vx
		{
			get
			{
				return x;
			}
			set
			{
				x = value;
			}
		}

		public int vy
		{
			get
			{
				return y;
			}
			set
			{
				y = value;
			}
		}

		public int vz
		{
			get
			{
				return z;
			}
			set
			{
				z = value;
			}
		}

		public void copy(VecFx32 src)
		{
			x = src.x;
			y = src.y;
			z = src.z;
		}

		public void copy(ds.Vector3<int> src)
		{
			x = src.vx;
			y = src.vy;
			z = src.vz;
		}

		public VecFx32()
		{
		}

		public VecFx32(VecFx32 src)
		{
			copy(src);
		}

		public VecFx32(int arg0, int arg1, int arg2)
		{
			x = arg0;
			y = arg1;
			z = arg2;
		}

		public void parse(ArrayReader reader)
		{
			x = reader.readInt32();
			y = reader.readInt32();
			z = reader.readInt32();
		}

		public void store(ArrayWriter writer)
		{
			writer.writeInt32(x);
			writer.writeInt32(y);
			writer.writeInt32(z);
		}

		public void setDefault()
		{
			x = 0;
			y = 0;
			z = 0;
		}

		public void set(int arg0, int arg1, int arg2)
		{
			x = arg0;
			y = arg1;
			z = arg2;
		}
	}
}
