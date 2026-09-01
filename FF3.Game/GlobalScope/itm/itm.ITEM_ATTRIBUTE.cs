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
		public enum ITEM_ATTRIBUTE
		{
			ATTRIBUTE_RECOVERY = 1,
			ATTRIBUTE_POISON = 2,
			ATTRIBUTE_ABSORPTION = 4,
			ATTRIBUTE_THUNDER = 8,
			ATTRIBUTE_ICE = 16,
			ATTRIBUTE_BLAZE = 32,
			ATTRIBUTE_WATER = 64,
			ATTRIBUTE_MUD = 128,
			ATTRIBUTE_HOLY = 256,
			ATTRIBUTE_SKY = 512,
			ATTRIBUTE_DARK = 1024,
			ATTRIBUTE_GHOST = 1025,
			ATTRIBUTE_HP_RECOVERY = 1026,
			ATTRIBUTE_CURE = 1027,
			ATTRIBUTE_REVIVAL = 1028,
			ATTRIBUTE_MP_RECOVERY = 1029,
			ITEM_ATTRIBUTE_MAX = 2048
		}
	}
}
