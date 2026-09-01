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
	public static partial class ds
	{
		public class OrderSavedErasePolicy<T>
		{
			public void erase(int pos, T[] vector, int size)
			{
				int num = size - 1;
				for (int i = pos; i < num; i++)
				{
					vector[i] = vector[i + 1];
				}
				vector[size - 1] = default(T);
			}
		}
	}
}
