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
	public static partial class pl
	{
		public enum PLAYER_TYPE
		{
			PLAYER_TYPE_ERR = -1,
			PLAYER_TYPE_HUMAN,
			PLAYER_TYPE_FROG,
			PLAYER_TYPE_MINI,
			PLAYER_TYPE_CHOKOBO,
			PLAYER_TYPE_SHIDO_H,
			PLAYER_TYPE_CANOE,
			PLAYER_TYPE_ENTERP,
			PLAYER_TYPE_ENTERP_CTM,
			PLAYER_TYPE_NORCHI,
			PLAYER_TYPE_NORCHI_CTM,
			PLAYER_TYPE_INVINSIBLE,
			PLAYER_TYPE_MAX
		}
	}
}
