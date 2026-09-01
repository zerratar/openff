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
	public static partial class itm
	{
		public enum ITEM_EQUIP_JOB
		{
			EQUIP_SUPPIN = 1,
			EQUIP_ONION_SWORDER = 2,
			EQUIP_FIGHTER = 4,
			EQUIP_MONK = 8,
			EQUIP_WHITE_MAGICIAN = 16,
			EQUIP_BLACK_MAGICIAN = 32,
			EQUIP_RED_MAGICIAN = 64,
			EQUIP_HUNTER = 128,
			EQUIP_KNIGHT = 256,
			EQUIP_THIEF = 512,
			EQUIP_BOOK_MAN = 1024,
			EQUIP_GEOMANCER = 2048,
			EQUIP_DRAGON_KNIGHT = 4096,
			EQUIP_VIKING = 8192,
			EQUIP_EVIL_SWORDER = 16384,
			EQUIP_PHANTOMER = 32768,
			EQUIP_BARD = 65536,
			EQUIP_KARATE_MASTER = 131072,
			EQUIP_IMAN = 262144,
			EQUIP_DEVIL_MAN = 524288,
			EQUIP_DEVILDOM_PHANTOMER = 1048576,
			EQUIP_SAGE = 2097152,
			EQUIP_NINJA = 4194304,
			ITEM_EQUIP_JOB_MAX = 4194305
		}
	}
}
