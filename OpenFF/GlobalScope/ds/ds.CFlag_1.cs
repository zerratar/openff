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
		public class CFlag<T>
		{
			private byte __Flag;

			public void onFlag(byte flag)
			{
				__Flag |= flag;
			}

			public void offFlag(byte flag)
			{
				__Flag &= (byte)(~flag);
			}

			public void clrFlag()
			{
				__Flag = 0;
			}

			public bool isFlag(byte flag)
			{
				if ((__Flag & flag) == 0)
				{
					return false;
				}
				return true;
			}

			public bool isAllFlag(byte flag)
			{
				if ((__Flag & flag) != flag)
				{
					return false;
				}
				return true;
			}
		}
	}
}
