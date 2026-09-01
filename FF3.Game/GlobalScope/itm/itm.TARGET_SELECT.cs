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
	public static partial class itm
	{
		public enum TARGET_SELECT
		{
			TARGET_SELECT_NONE = 1,
			TARGET_SELECT_ENEMY = 2,
			TARGET_SELECT_ENEMY_GROUP = 4,
			TARGET_SELECT_ENEMY_ALL = 8,
			TARGET_SELECT_RANDOM_ENEMY = 0x10,
			TARGET_SELECT_RANDOM_ENEMY_GROUP = 0x20,
			TARGET_SELECT_SELF = 0x40,
			TARGET_SELECT_FRIEND = 0x80,
			TARGET_SELECT_FRIEND_GROUP = 0x100,
			TARGET_SELECT_FRIEND_ALL = 0x200,
			TARGET_SELECT_RANDOM_FRIEND = 0x400,
			TARGET_SELECT_RANDOM_FRIEND_GROUP = 0x800,
			TARGET_SELECT_ALL = 0x1000
		}
	}
}
