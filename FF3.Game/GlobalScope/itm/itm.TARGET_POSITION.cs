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
	public static partial class itm
	{
		public enum TARGET_POSITION
		{
			TARGET_POSITION_ENEMY,
			TARGET_POSITION_ENEMY_ABNORMAL,
			TARGET_POSITION_SELF,
			TARGET_POSITION_FRIEND,
			TARGET_POSITION_FRIEND_ABNORMAL
		}
	}
}
