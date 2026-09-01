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
		public enum BATTLE_EFFECT_TYPE
		{
			BET_GUARD = 201,
			BET_ABILITY = 231,
			BET_ROLL_UP = 233,
			BET_TAP = 237,
			BET_BOOKMAN = 238,
			BET_JUMP_HIT = 240,
			BET_PROVOCATION = 241,
			BET_DARK = 242,
			BET_SONG = 245,
			BET_PITCH = 246,
			BET_COVER = 247,
			BET_STEAL = 248,
			BET_KNOCK_OVER = 431,
			BET_COVER_START = 432,
			BET_POISE = 433
		}
	}
}
