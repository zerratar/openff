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
	public static partial class btl
	{
		public class BasicBattleWindow
		{
			public const int ERR_MESSAGE_ID = -1;

			protected menu.BasicWindow window_ = new menu.BasicWindow();

			protected bool created_;

			public virtual void setup()
			{
			}

			public virtual void cleanup()
			{
			}

			public virtual void execute()
			{
			}

			public virtual void create()
			{
			}

			public virtual void release()
			{
			}

			public virtual void show(bool flag)
			{
			}
		}
	}
}
