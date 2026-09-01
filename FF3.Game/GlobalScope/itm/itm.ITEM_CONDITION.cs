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
	public static partial class itm
	{
		public enum ITEM_CONDITION
		{
			CONDITION_PARALYSIS = 1,
			CONDITION_SLEEP = 2,
			CONDITION_CONFUSION = 4,
			CONDITION_STONY = 8,
			CONDITION_FROG = 16,
			CONDITION_SILENCE = 32,
			CONDITION_LILLIPUT = 64,
			CONDITION_DARKNESS = 128,
			CONDITION_POISON = 256,
			CONDITION_DEATH = 512,
			CONDITION_NEAR_DEATH = 1024,
			ITEM_CONDITION_MAX = 11
		}
	}
}
