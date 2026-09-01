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
	public static partial class mognet
	{
		public class NPCMailEntry
		{
			public int no_;

			public int title_;

			public int body_;

			public int state_;

			public int name_;

			public NPCMailEntry(int arg0, int arg1, int arg2, int arg3, int arg4)
			{
				no_ = arg0;
				title_ = arg1;
				body_ = arg2;
				state_ = arg3;
				name_ = arg4;
			}
		}
	}
}
