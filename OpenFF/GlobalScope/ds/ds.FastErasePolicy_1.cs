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
	public static partial class ds
	{
		public class FastErasePolicy<T>
		{
			public void erase(int pos, T[] vector, int size)
			{
				if (pos < size - 1)
				{
					vector[pos] = vector[size - 1];
				}
				vector[size - 1] = default(T);
			}
		}
	}
}
