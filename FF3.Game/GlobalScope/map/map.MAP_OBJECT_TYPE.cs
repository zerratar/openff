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
	public static partial class map
	{
							public enum MAP_OBJECT_TYPE
							{
								MAP_OBJECT_TYPE_ERR = -1,
								INVISIBLE,
								TREASURE_BOX,
								ROCK,
								SKELETON,
								CANNON,
								DWARF_HORN,
								FUYUUGUSA_SHOES,
								GOLDR_KEY,
								NOAH_LUTE,
								ADAMANTITE,
								ENGETURIN,
								MASAMUNE,
								EXCALIBUR,
								RAGNAROK,
								ELDER_STICK,
								WIND_CRYSTAL,
								JAR,
								CANDLE_STICK,
								CRUMBLE_WALL,
								MOVE_WALL,
								SWITCH_ROCK,
								LIGHT_PILLAR,
								BLACK_BOARD,
								URU_DOOR,
								BLACK_BOARD2,
								INVISIBLE_TREASURE,
								NEPT_EYE,
								OBSTACLE_WALL,
								NERV_ROCK,
								CASTLE_GATE,
								RING,
								OBSTACLE_WALL2,
								OBSTACLE_WALL3,
								OBSTACLE_WALL4,
								CANNON_SHELL,
								OBSTACLE_WALL5,
								OBSTACLE_WALL6,
								HINE_CASTLE,
								TRESURE_ROCK,
								OBSTACLE_FIRE,
								OBSTACLE_WALL7,
								FIRE_CRYSTAL,
								CRYSTAL_DUST,
								JAIL_CELL,
								LEVIATHAN,
								WATER_CRYSTAL,
								CASTLE_GATE2,
								WATERWAY_FENCE,
								GOLD_CRYSTAL,
								OBSTACLE_WALL8,
								OBSTACLE_WALL9,
								EARTH_FUNG,
								STONE_FIGURE,
								EARTH_CRYSTAL,
								OBSTACLE_WALL10,
								DARK_WIND_CRYSTAL,
								DARK_FIRE_CRYSTAL,
								DARK_WATER_CRYSTAL,
								DARK_EARTH_CRYSTAL,
								MAGIC_SQUARE,
								SILKS_GATE,
								CLOUD,
								FORCE,
								WATER_TEMPLE_GATE,
								UNKNOWN_64,
								UNKNOWN_65,
								UNKNOWN_66,
								UNKNOWN_67,
								UNKNOWN_68,
								UNKNOWN_69,
								UNKNOWN_70,
								UNKNOWN_71,
								UNKNOWN_72,
								UNKNOWN_73,
								UNKNOWN_74,
								UNKNOWN_75,
								UNKNOWN_76,
								UNKNOWN_77,
								MAP_OBJECT_MAX
							}
	}
}
