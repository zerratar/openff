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
		public enum PLAYER_BATTLE_ACTION
		{
			PBA_IDLE,
			PBA_NORMAL_ATTACK,
			PBA_ESCAPE,
			PBA_GUARD_START,
			PBA_SPECIAL_ATTACK,
			PBA_NORMAL_MAGIC,
			PBA_SUMMON_MAGIC,
			PBA_ITEM,
			PBA_STEAL,
			PBA_TAKE_A_POWDER,
			PBA_BERSERK,
			PBA_KNOCK_OVER,
			PBA_POISE,
			PBA_COVER_START,
			PBA_CHECK,
			PBA_DETECT,
			PBA_JUMP_START,
			PBA_JUMP_END,
			PBA_DARK,
			PBA_GEOGRAPHY,
			PBA_SONG,
			PBA_ROLL_UP,
			PBA_PITCH,
			PBA_PROVOCATION,
			PBA_OVERISSUE,
			PBA_COUNTER,
			PBA_CHANGE_FORMATION,
			PLAYER_BATTLE_ACTION_MAX
		}
	}
}
