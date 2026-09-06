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
	public static partial class map
	{
							public enum MAP_COLLISION_FLAG
							{
								MAP_COL_FLAG_NONE = 1,
								MAP_COL_FLAG_CHARACTER = 2,
								MAP_COL_FLAG_GROUND = 4,
								MAP_COL_FLAG_WALL = 8,
								MAP_COL_FLAG_WALL_01 = 16,
								MAP_COL_FLAG_WALL_02 = 32,
								MAP_COL_FLAG_WALL_03 = 64,
								MAP_COL_FLAG_WALL_04 = 128,
								MAP_COL_FLAG_WALL_05 = 256,
								MAP_COL_FLAG_LANDFORM = 512,
								MAP_COL_FLAG_MONSTER = 1024,
								MAP_COL_FLAG_MAPJUMP = 2048,
								MAP_COL_FLAG_MAPJUMP_01 = 4096,
								MAP_COL_FLAG_MAPJUMP_02 = 8192,
								MAP_COL_FLAG_MAPJUMP_03 = 16384,
								MAP_COL_FLAG_MAPJUMP_04 = 32768,
								MAP_COL_FLAG_MAPJUMP_05 = 65536,
								MAP_COL_FLAG_MAPJUMP_06 = 131072,
								MAP_COL_FLAG_MAPJUMP_07 = 262144,
								MAP_COL_FLAG_MAPJUMP_08 = 524288,
								MAP_COL_FLAG_MAPJUMP_09 = 1048576,
								MAP_COL_FLAG_MAPJUMP_10 = 2097152,
								MAP_COL_FLAG_MAPJUMP_11 = 4194304,
								MAP_COL_FLAG_MAPJUMP_12 = 8388608,
								MAP_COL_FLAG_DAMAGE = 16777216,
								MAP_COL_FLAG_DAMAGE_01 = 33554432,
								MAP_COL_FLAG_DAMAGE_02 = 67108864,
								MAP_COL_FLAG_DAMAGE_03 = 134217728,
								MAP_COL_FLAG_DAMAGE_04 = 268435456,
								MAP_COL_FLAG_DAMAGE_05 = 536870912,
								MAP_COL_FLAG_MAX = 536870913
							}
	}
}
