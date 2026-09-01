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
	public static partial class pl
	{
		public enum PLAYER_HUMAN_TYPE
		{
			PLAYER_HUMAN_TYPE_ERR = -1,
			PLAYER_HUMAN_TYPE_HUMAN,
			PLAYER_HUMAN_TYPE_MINI,
			PLAYER_HUMAN_TYPE_FROG,
			PLAYER_HUMAN_TYPE_CHOKOBO,
			PLAYER_HUMAN_TYPE_FAIRY,
			PLAYER_HUMAN_TYPE_SHEEP,
			PLAYER_HUMAN_TYPE_MAX
		}
	}
}
