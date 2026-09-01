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
	public static partial class dgs
	{
		public class UniqueNumber
		{
			public static int INVALID_NUMBER = -1;

			private int number_;

			public UniqueNumber()
			{
				g_UNCurrentNumber++;
				number_ = g_UNCurrentNumber;
			}

			public int number()
			{
				return number_;
			}
		}
	}
}
