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
		public enum PLAYER_MOVE_TYPE
		{
			PLAYER_MOVE_TYPE_ERR = -1,
			PLAYER_MOVE_TYPE_HERO_FIELD,
			PLAYER_MOVE_TYPE_HERO_TOWN,
			PLAYER_MOVE_TYPE_FROG,
			PLAYER_MOVE_TYPE_LILLIPUT,
			PLAYER_MOVE_TYPE_CHOKOBO,
			PLAYER_MOVE_TYPE_FAIRY,
			PLAYER_MOVE_TYPE_SHEEP,
			PLAYER_MOVE_TYPE_SHIDO_H,
			PLAYER_MOVE_TYPE_CANOE,
			PLAYER_MOVE_TYPE_ENTERP,
			PLAYER_MOVE_TYPE_ENTERP_CTM,
			PLAYER_MOVE_TYPE_NORCHI,
			PLAYER_MOVE_TYPE_NORCHI_CTM,
			PLAYER_MOVE_TYPE_INVINSIBLE,
			PLAYER_MOVE_TYPE_DEFAULT_NPC,
			PLAYER_MOVE_TYPE_FRIEND_NPC_FIELD,
			PLAYER_MOVE_TYPE_FRIEND_NPC_TOWN,
			PLAYER_MOVE_TYPE_MAX
		}
	}
}
