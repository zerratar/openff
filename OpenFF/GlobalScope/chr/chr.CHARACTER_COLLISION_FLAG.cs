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
	public static partial class chr
	{
		public enum CHARACTER_COLLISION_FLAG
		{
			COL_FLAG_NONE = 1,
			COL_FLAG_TOUCH = 2,
			COL_FLAG_CHARACTER = 4,
			COL_FLAG_GROUND = 8,
			COL_FLAG_WALL = 16,
			COL_FLAG_WALL_01 = 32,
			COL_FLAG_WALL_02 = 64,
			COL_FLAG_WALL_03 = 128,
			COL_FLAG_WALL_04 = 256,
			COL_FLAG_WALL_05 = 512,
			COL_FLAG_LANDFORM = 1024,
			COL_FLAG_MONSTER = 2048,
			COL_FLAG_MAPJUMP = 4096,
			COL_FLAG_MAPJUMP_01 = 8192,
			COL_FLAG_MAPJUMP_02 = 16384,
			COL_FLAG_MAPJUMP_03 = 32768,
			COL_FLAG_MAPJUMP_04 = 65536,
			COL_FLAG_MAPJUMP_05 = 131072,
			COL_FLAG_MAPJUMP_06 = 262144,
			COL_FLAG_MAPJUMP_07 = 524288,
			COL_FLAG_MAPJUMP_08 = 1048576,
			COL_FLAG_MAPJUMP_09 = 2097152,
			COL_FLAG_MAPJUMP_10 = 4194304,
			COL_FLAG_MAPJUMP_11 = 8388608,
			COL_FLAG_MAPJUMP_12 = 16777216,
			COL_FLAG_DAMAGE = 33554432,
			COL_FLAG_DAMAGE_01 = 67108864,
			COL_FLAG_DAMAGE_02 = 134217728,
			COL_FLAG_DAMAGE_03 = 268435456,
			COL_FLAG_DAMAGE_04 = 536870912,
			COL_FLAG_DAMAGE_05 = 1073741824,
			COL_FLAG_MAX = 1073741825
		}
	}
}
