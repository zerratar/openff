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
	public static partial class btl
	{
		public enum BATTLE_ACTION_TYPE
		{
			DBA_FRONT = 0,
			DBA_BACK = 1,
			DBA_NORMAL_ATTACK = 2,
			DBA_DAMAGE = 3,
			DBA_ESCAPE = 4,
			DBA_FINISH = 5,
			DBA_IDLE_TO_POISE = 6,
			DBA_POISE_TO_IDLE = 7,
			DBA_FRONT_ATTACK = 8,
			DBA_BACK_ATTACK = 9,
			DBA_CHANGE_FRONT = 10,
			DBA_CHANGE_BACK = 11,
			DBA_GUARD_START = 12,
			DBA_GUARD = 13,
			DBA_MAGIC = 14,
			DBA_MAGIC_LOOP = 15,
			DBA_STEAL = 16,
			DBA_TAKE_A_POWDER = 17,
			DBA_PROTECT = 18,
			DBA_ITEM = 19,
			DBA_ITEM_LOOP = 20,
			DBA_KNOCK_OVER_FRONT = 21,
			DBA_KNOCK_OVER_BACK = 22,
			DBA_POISE = 23,
			DBA_COVER_START = 24,
			DBA_CHECK = 25,
			DBA_DETECT = 26,
			DBA_JUMP_START = 27,
			DBA_JUMP_END = 28,
			DBA_DARK = 29,
			DBA_GEOGRAPHY = 30,
			DBA_SONG = 31,
			DBA_ROLL_UP = 32,
			DBA_PITCH = 33,
			DBA_PROVOCATION = 34,
			DBA_COVER = 35,
			DBA_IDLE = 36,
			DBA_CONDITION_MOTION = 37,
			BATTLE_ACTION_TYPE_MAX = 38,
			DBA_NON_ACTION = -1
		}
	}
}
