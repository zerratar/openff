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
	public static partial class dgs
	{
		public class SmartPtr<type_value> where type_value : DGSMessage
		{
			private type_value ptr;

			public SmartPtr()
			{
				ptr = null;
			}

			public SmartPtr(type_value p)
			{
				ptr = p;
			}

			~SmartPtr()
			{
				release();
			}

			public type_value get()
			{
				return ptr;
			}

			public void release()
			{
				if (ptr != null)
				{
					ptr.release();
				}
				ptr = null;
			}
		}
	}
}
