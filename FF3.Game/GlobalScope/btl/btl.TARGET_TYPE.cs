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
	public static partial class btl
	{
		public enum TARGET_TYPE
		{
			TARGET_NONE = 0,
			TARGET_ENEMY = 1,
			TARGET_ENEMY_GROUP = 2,
			TARGET_ENEMY_ALL = 4,
			TARGET_SELF = 8,
			TARGET_FRIEND = 0x10,
			TARGET_FRIEND_ALL = 0x20,
			TARGET_ALL = 0x40
		}
	}
}
