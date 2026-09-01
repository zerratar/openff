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
	public static partial class chr
	{
		public enum CHARACTER_KIND
		{
			CHARACTER_KIND_ERR = -1,
			CHARACTER_KIND_PLAYER_HUMAN,
			CHARACTER_KIND_PLAYER_VEHICLE,
			CHARACTER_KIND_MAP_OBJECT,
			CHARACTER_KIND_MAX
		}
	}
}
