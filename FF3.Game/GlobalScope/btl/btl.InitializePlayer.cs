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
	public static partial class btl
	{
		public class InitializePlayer
		{
			private bool[] isEnable_ = new bool[4];

			public void initialize()
			{
				for (byte b = 0; b < 4; b++)
				{
					setIsEnable(b, flag: false);
				}
			}

			public void setIsEnable(int i, bool flag)
			{
				isEnable_[i] = flag;
			}

			public bool isEnable(int i)
			{
				return isEnable_[i];
			}
		}
	}
}
