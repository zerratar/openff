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
	public static partial class btl
	{
		public enum PLAYER_FLAG
		{
			PF_ESCAPE = 1,
			PF_GUARD = 2,
			PF_DEAD = 4,
			PF_MISS = 8,
			PF_BERSERK = 0x10,
			PF_EFFECT = 0x20,
			PF_CREATE_EFFECT = 0x40,
			PF_RECOVER = 0x80,
			PF_FINISH = 0x100,
			PF_2D = 0x200,
			PF_MAGIC_LOOP_END = 0x400,
			PF_CRITICAL = 0x800,
			PF_CREATE_2D = 0x1000,
			PF_TARGET_PLAYER = 0x2000,
			PF_TARGET_MONSTER = 0x4000,
			PF_COUNTER = 0x8000,
			PF_ATTACKED = 0x10000,
			PF_MORE_GUARD = 0x20000,
			PF_JUMP = 0x40000,
			PF_DARK = 0x80000,
			PF_ROLL_UP = 0x100000,
			PF_EXPLOSION = 0x200000,
			PF_COVER = 0x400000,
			PF_OVERISSUE = 0x800000,
			PF_PITCH = 0x1000000,
			PF_PROVOCATION = 0x2000000,
			PF_ERASE = 0x4000000,
			PF_ABSORB = 0x8000000,
			PF_KNOCK_OVER = 0x10000000,
			PF_TAKE_A_POWDER = 0x20000000,
			PF_POISON_NOW = 0x40000000
		}
	}
}
