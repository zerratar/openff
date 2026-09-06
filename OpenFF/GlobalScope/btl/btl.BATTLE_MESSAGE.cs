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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	public static partial class btl
	{
		public enum BATTLE_MESSAGE
		{
			BM_COUNTER = 21,
			BM_BACK_ATTACK = 61,
			BM_COVER_FIRE = 64,
			BM_PLAYER_NAME = 100,
			BM_NOT_ESCAPE = 104,
			BM_ESCAPE = 105,
			BM_GET_GILL = 106,
			BM_GET_CAPACITY = 107,
			BM_GET_EXP = 108,
			BM_LEVEL_UP = 109,
			BM_SKILL_UP = 110,
			BM_GET_TREASURE = 111,
			BM_ANNIHILATED = 112,
			BM_CHANGE = 113,
			BM_INITIATLVE_ATTACK = 114,
			BM_NOT_STEAL = 115,
			BM_NO_WEAK = 116,
			BM_NO_ACTION = 117,
			BM_NO_RESULT = 118,
			BM_FINE = 119,
			BM_REBIRTH = 120,
			BM_COME_BACK = 121,
			BM_POISON = 122,
			BM_DARKNESS = 123,
			BM_POISON_DAMAGE = 124,
			BM_TARGET_ALL = 125,
			BM_TARGET_GROUP = 126,
			BM_MAX_HP_AND_HP = 127,
			BM_WEAK_THUNDER = 128,
			BM_WEAK_ICE = 129,
			BM_WEAK_FIRE = 130,
			BM_WEAK_WATER = 131,
			BM_WEAK_EARTH = 132,
			BM_WEAK_HOLY = 133,
			BM_WEAK_SKY = 134,
			BM_WEAK_DARK = 135,
			BM_INVALIDATION = 136,
			BM_NOT_KNOW_HP = 137,
			BM_NOT_KNOW_WEAK = 138,
			BM_SELF_EXPLOSION = 139,
			BM_NO_ITEM = 143,
			BM_BREAK = 144
		}
	}
}
