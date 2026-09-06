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
	public static partial class map
	{
							public const int MAP_JUMP_ATTRIBUTE_NUM = 12;

							public const MAP_JUMP_PLAYER_CONDITION_FLAG MAP_JUMP_PLAYER_CONDITION_FLAG_ALL = MAP_JUMP_PLAYER_CONDITION_FLAG.MAP_JUMP_PLAYER_CONDITION_FLAG_ALL;

							public const MAP_JUMP_PLAYER_CONDITION_FLAG MAP_JUMP_PLAYER_CONDITION_FLAG_SMALL = MAP_JUMP_PLAYER_CONDITION_FLAG.MAP_JUMP_PLAYER_CONDITION_FLAG_SMALL;

							public const MAP_JUMP_PLAYER_CONDITION_FLAG MAP_JUMP_PLAYER_CONDITION_FLAG_FROG = MAP_JUMP_PLAYER_CONDITION_FLAG.MAP_JUMP_PLAYER_CONDITION_FLAG_FROG;

							public const MAP_JUMP_PLAYER_CONDITION_FLAG MAP_JUMP_PLAYER_CONDITION_FLAG_MAX = MAP_JUMP_PLAYER_CONDITION_FLAG.MAP_JUMP_PLAYER_CONDITION_FLAG_MAX;

							public const MAP_PARAM_CATEGORY MAP_PARAM_CATEGORY_MAPJUMP = MAP_PARAM_CATEGORY.MAP_PARAM_CATEGORY_MAPJUMP;

							public const MAP_PARAM_CATEGORY MAP_PARAM_CATEGORY_LANDFORM = MAP_PARAM_CATEGORY.MAP_PARAM_CATEGORY_LANDFORM;

							public const MAP_PARAM_CATEGORY MAP_PARAM_CATEGORY_MONSTER_PARTY = MAP_PARAM_CATEGORY.MAP_PARAM_CATEGORY_MONSTER_PARTY;

							public const MAP_PARAM_CATEGORY MAP_PARAM_CATEGORY_SOUND = MAP_PARAM_CATEGORY.MAP_PARAM_CATEGORY_SOUND;

							public const MAP_PARAM_CATEGORY MAP_PARAM_CATEGORY_ENCOUNT = MAP_PARAM_CATEGORY.MAP_PARAM_CATEGORY_ENCOUNT;

							public const MAP_PARAM_CATEGORY MAP_PARAM_CATEGORY_CAMERA = MAP_PARAM_CATEGORY.MAP_PARAM_CATEGORY_CAMERA;

							public const MAP_PARAM_CATEGORY MAP_PARAM_CATEGORY_SECRETWAY = MAP_PARAM_CATEGORY.MAP_PARAM_CATEGORY_SECRETWAY;

							public const MAP_PARAM_CATEGORY MAP_PARAM_CATEGORY_MAX = MAP_PARAM_CATEGORY.MAP_PARAM_CATEGORY_MAX;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_ERR = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_ERR;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_GRS = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_GRS;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_FRT = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_FRT;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_DES = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_DES;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_LAK = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_LAK;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_SEA = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_SEA;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_MOR = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_MOR;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_SKY = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_SKY;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_SBD = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_SBD;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_CWL = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_CWL;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_SBW = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_SBW;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_FIELD_TYPE_MAX = MAP_LANDFORM_TYPE.MAP_LANDFORM_FIELD_TYPE_MAX;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_HOR = MAP_LANDFORM_TYPE.MAP_LANDFORM_FIELD_TYPE_MAX;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_INR = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_INR;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_WAT = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_WAT;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_STL = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_STL;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_WOD = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_WOD;

							public const MAP_LANDFORM_TYPE MAP_LANDFORM_TYPE_MAX = MAP_LANDFORM_TYPE.MAP_LANDFORM_TYPE_MAX;

							public const MAP_OBJECT_TYPE MAP_OBJECT_TYPE_ERR = MAP_OBJECT_TYPE.MAP_OBJECT_TYPE_ERR;

							public const MAP_OBJECT_TYPE INVISIBLE = MAP_OBJECT_TYPE.INVISIBLE;

							public const MAP_OBJECT_TYPE TREASURE_BOX = MAP_OBJECT_TYPE.TREASURE_BOX;

							public const MAP_OBJECT_TYPE ROCK = MAP_OBJECT_TYPE.ROCK;

							public const MAP_OBJECT_TYPE SKELETON = MAP_OBJECT_TYPE.SKELETON;

							public const MAP_OBJECT_TYPE CANNON = MAP_OBJECT_TYPE.CANNON;

							public const MAP_OBJECT_TYPE DWARF_HORN = MAP_OBJECT_TYPE.DWARF_HORN;

							public const MAP_OBJECT_TYPE FUYUUGUSA_SHOES = MAP_OBJECT_TYPE.FUYUUGUSA_SHOES;

							public const MAP_OBJECT_TYPE GOLDR_KEY = MAP_OBJECT_TYPE.GOLDR_KEY;

							public const MAP_OBJECT_TYPE NOAH_LUTE = MAP_OBJECT_TYPE.NOAH_LUTE;

							public const MAP_OBJECT_TYPE ADAMANTITE = MAP_OBJECT_TYPE.ADAMANTITE;

							public const MAP_OBJECT_TYPE ENGETURIN = MAP_OBJECT_TYPE.ENGETURIN;

							public const MAP_OBJECT_TYPE MASAMUNE = MAP_OBJECT_TYPE.MASAMUNE;

							public const MAP_OBJECT_TYPE EXCALIBUR = MAP_OBJECT_TYPE.EXCALIBUR;

							public const MAP_OBJECT_TYPE RAGNAROK = MAP_OBJECT_TYPE.RAGNAROK;

							public const MAP_OBJECT_TYPE ELDER_STICK = MAP_OBJECT_TYPE.ELDER_STICK;

							public const MAP_OBJECT_TYPE WIND_CRYSTAL = MAP_OBJECT_TYPE.WIND_CRYSTAL;

							public const MAP_OBJECT_TYPE JAR = MAP_OBJECT_TYPE.JAR;

							public const MAP_OBJECT_TYPE CANDLE_STICK = MAP_OBJECT_TYPE.CANDLE_STICK;

							public const MAP_OBJECT_TYPE CRUMBLE_WALL = MAP_OBJECT_TYPE.CRUMBLE_WALL;

							public const MAP_OBJECT_TYPE MOVE_WALL = MAP_OBJECT_TYPE.MOVE_WALL;

							public const MAP_OBJECT_TYPE SWITCH_ROCK = MAP_OBJECT_TYPE.SWITCH_ROCK;

							public const MAP_OBJECT_TYPE LIGHT_PILLAR = MAP_OBJECT_TYPE.LIGHT_PILLAR;

							public const MAP_OBJECT_TYPE BLACK_BOARD = MAP_OBJECT_TYPE.BLACK_BOARD;

							public const MAP_OBJECT_TYPE URU_DOOR = MAP_OBJECT_TYPE.URU_DOOR;

							public const MAP_OBJECT_TYPE BLACK_BOARD2 = MAP_OBJECT_TYPE.BLACK_BOARD2;

							public const MAP_OBJECT_TYPE INVISIBLE_TREASURE = MAP_OBJECT_TYPE.INVISIBLE_TREASURE;

							public const MAP_OBJECT_TYPE NEPT_EYE = MAP_OBJECT_TYPE.NEPT_EYE;

							public const MAP_OBJECT_TYPE OBSTACLE_WALL = MAP_OBJECT_TYPE.OBSTACLE_WALL;

							public const MAP_OBJECT_TYPE NERV_ROCK = MAP_OBJECT_TYPE.NERV_ROCK;

							public const MAP_OBJECT_TYPE CASTLE_GATE = MAP_OBJECT_TYPE.CASTLE_GATE;

							public const MAP_OBJECT_TYPE RING = MAP_OBJECT_TYPE.RING;

							public const MAP_OBJECT_TYPE OBSTACLE_WALL2 = MAP_OBJECT_TYPE.OBSTACLE_WALL2;

							public const MAP_OBJECT_TYPE OBSTACLE_WALL3 = MAP_OBJECT_TYPE.OBSTACLE_WALL3;

							public const MAP_OBJECT_TYPE OBSTACLE_WALL4 = MAP_OBJECT_TYPE.OBSTACLE_WALL4;

							public const MAP_OBJECT_TYPE CANNON_SHELL = MAP_OBJECT_TYPE.CANNON_SHELL;

							public const MAP_OBJECT_TYPE OBSTACLE_WALL5 = MAP_OBJECT_TYPE.OBSTACLE_WALL5;

							public const MAP_OBJECT_TYPE OBSTACLE_WALL6 = MAP_OBJECT_TYPE.OBSTACLE_WALL6;

							public const MAP_OBJECT_TYPE HINE_CASTLE = MAP_OBJECT_TYPE.HINE_CASTLE;

							public const MAP_OBJECT_TYPE TRESURE_ROCK = MAP_OBJECT_TYPE.TRESURE_ROCK;

							public const MAP_OBJECT_TYPE OBSTACLE_FIRE = MAP_OBJECT_TYPE.OBSTACLE_FIRE;

							public const MAP_OBJECT_TYPE OBSTACLE_WALL7 = MAP_OBJECT_TYPE.OBSTACLE_WALL7;

							public const MAP_OBJECT_TYPE FIRE_CRYSTAL = MAP_OBJECT_TYPE.FIRE_CRYSTAL;

							public const MAP_OBJECT_TYPE CRYSTAL_DUST = MAP_OBJECT_TYPE.CRYSTAL_DUST;

							public const MAP_OBJECT_TYPE JAIL_CELL = MAP_OBJECT_TYPE.JAIL_CELL;

							public const MAP_OBJECT_TYPE LEVIATHAN = MAP_OBJECT_TYPE.LEVIATHAN;

							public const MAP_OBJECT_TYPE WATER_CRYSTAL = MAP_OBJECT_TYPE.WATER_CRYSTAL;

							public const MAP_OBJECT_TYPE CASTLE_GATE2 = MAP_OBJECT_TYPE.CASTLE_GATE2;

							public const MAP_OBJECT_TYPE WATERWAY_FENCE = MAP_OBJECT_TYPE.WATERWAY_FENCE;

							public const MAP_OBJECT_TYPE GOLD_CRYSTAL = MAP_OBJECT_TYPE.GOLD_CRYSTAL;

							public const MAP_OBJECT_TYPE OBSTACLE_WALL8 = MAP_OBJECT_TYPE.OBSTACLE_WALL8;

							public const MAP_OBJECT_TYPE OBSTACLE_WALL9 = MAP_OBJECT_TYPE.OBSTACLE_WALL9;

							public const MAP_OBJECT_TYPE EARTH_FUNG = MAP_OBJECT_TYPE.EARTH_FUNG;

							public const MAP_OBJECT_TYPE STONE_FIGURE = MAP_OBJECT_TYPE.STONE_FIGURE;

							public const MAP_OBJECT_TYPE EARTH_CRYSTAL = MAP_OBJECT_TYPE.EARTH_CRYSTAL;

							public const MAP_OBJECT_TYPE OBSTACLE_WALL10 = MAP_OBJECT_TYPE.OBSTACLE_WALL10;

							public const MAP_OBJECT_TYPE DARK_WIND_CRYSTAL = MAP_OBJECT_TYPE.DARK_WIND_CRYSTAL;

							public const MAP_OBJECT_TYPE DARK_FIRE_CRYSTAL = MAP_OBJECT_TYPE.DARK_FIRE_CRYSTAL;

							public const MAP_OBJECT_TYPE DARK_WATER_CRYSTAL = MAP_OBJECT_TYPE.DARK_WATER_CRYSTAL;

							public const MAP_OBJECT_TYPE DARK_EARTH_CRYSTAL = MAP_OBJECT_TYPE.DARK_EARTH_CRYSTAL;

							public const MAP_OBJECT_TYPE MAGIC_SQUARE = MAP_OBJECT_TYPE.MAGIC_SQUARE;

							public const MAP_OBJECT_TYPE SILKS_GATE = MAP_OBJECT_TYPE.SILKS_GATE;

							public const MAP_OBJECT_TYPE CLOUD = MAP_OBJECT_TYPE.CLOUD;

							public const MAP_OBJECT_TYPE FORCE = MAP_OBJECT_TYPE.FORCE;

							public const MAP_OBJECT_TYPE WATER_TEMPLE_GATE = MAP_OBJECT_TYPE.WATER_TEMPLE_GATE;

							public const MAP_OBJECT_TYPE UNKNOWN_64 = MAP_OBJECT_TYPE.UNKNOWN_64;

							public const MAP_OBJECT_TYPE UNKNOWN_65 = MAP_OBJECT_TYPE.UNKNOWN_65;

							public const MAP_OBJECT_TYPE UNKNOWN_66 = MAP_OBJECT_TYPE.UNKNOWN_66;

							public const MAP_OBJECT_TYPE UNKNOWN_67 = MAP_OBJECT_TYPE.UNKNOWN_67;

							public const MAP_OBJECT_TYPE UNKNOWN_68 = MAP_OBJECT_TYPE.UNKNOWN_68;

							public const MAP_OBJECT_TYPE UNKNOWN_69 = MAP_OBJECT_TYPE.UNKNOWN_69;

							public const MAP_OBJECT_TYPE UNKNOWN_70 = MAP_OBJECT_TYPE.UNKNOWN_70;

							public const MAP_OBJECT_TYPE UNKNOWN_71 = MAP_OBJECT_TYPE.UNKNOWN_71;

							public const MAP_OBJECT_TYPE UNKNOWN_72 = MAP_OBJECT_TYPE.UNKNOWN_72;

							public const MAP_OBJECT_TYPE UNKNOWN_73 = MAP_OBJECT_TYPE.UNKNOWN_73;

							public const MAP_OBJECT_TYPE UNKNOWN_74 = MAP_OBJECT_TYPE.UNKNOWN_74;

							public const MAP_OBJECT_TYPE UNKNOWN_75 = MAP_OBJECT_TYPE.UNKNOWN_75;

							public const MAP_OBJECT_TYPE UNKNOWN_76 = MAP_OBJECT_TYPE.UNKNOWN_76;

							public const MAP_OBJECT_TYPE UNKNOWN_77 = MAP_OBJECT_TYPE.UNKNOWN_77;

							public const MAP_OBJECT_TYPE MAP_OBJECT_MAX = MAP_OBJECT_TYPE.MAP_OBJECT_MAX;

							public const MAP_OBJECT_O001_ACTION_ID MAP_OBJECT_O001_ACTION_ID_OPEN = MAP_OBJECT_O001_ACTION_ID.MAP_OBJECT_O001_ACTION_ID_OPEN;

							public const MAP_OBJECT_O001_ACTION_ID MAP_OBJECT_O001_ACTION_ID_OPEN_WAIT = MAP_OBJECT_O001_ACTION_ID.MAP_OBJECT_O001_ACTION_ID_OPEN_WAIT;

							public const MAP_OBJECT_O001_ACTION_ID MAP_OBJECT_O001_ACTION_ID_CLOSE = MAP_OBJECT_O001_ACTION_ID.MAP_OBJECT_O001_ACTION_ID_CLOSE;

							public const MAP_OBJECT_O001_ACTION_ID MAP_OBJECT_O001_ACTION_ID_MAX = MAP_OBJECT_O001_ACTION_ID.MAP_OBJECT_O001_ACTION_ID_MAX;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_NONE = MAP_COLLISION_FLAG.MAP_COL_FLAG_NONE;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_CHARACTER = MAP_COLLISION_FLAG.MAP_COL_FLAG_CHARACTER;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_GROUND = MAP_COLLISION_FLAG.MAP_COL_FLAG_GROUND;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_WALL = MAP_COLLISION_FLAG.MAP_COL_FLAG_WALL;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_WALL_01 = MAP_COLLISION_FLAG.MAP_COL_FLAG_WALL_01;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_WALL_02 = MAP_COLLISION_FLAG.MAP_COL_FLAG_WALL_02;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_WALL_03 = MAP_COLLISION_FLAG.MAP_COL_FLAG_WALL_03;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_WALL_04 = MAP_COLLISION_FLAG.MAP_COL_FLAG_WALL_04;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_WALL_05 = MAP_COLLISION_FLAG.MAP_COL_FLAG_WALL_05;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_LANDFORM = MAP_COLLISION_FLAG.MAP_COL_FLAG_LANDFORM;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_MONSTER = MAP_COLLISION_FLAG.MAP_COL_FLAG_MONSTER;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_MAPJUMP = MAP_COLLISION_FLAG.MAP_COL_FLAG_MAPJUMP;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_MAPJUMP_01 = MAP_COLLISION_FLAG.MAP_COL_FLAG_MAPJUMP_01;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_MAPJUMP_02 = MAP_COLLISION_FLAG.MAP_COL_FLAG_MAPJUMP_02;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_MAPJUMP_03 = MAP_COLLISION_FLAG.MAP_COL_FLAG_MAPJUMP_03;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_MAPJUMP_04 = MAP_COLLISION_FLAG.MAP_COL_FLAG_MAPJUMP_04;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_MAPJUMP_05 = MAP_COLLISION_FLAG.MAP_COL_FLAG_MAPJUMP_05;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_MAPJUMP_06 = MAP_COLLISION_FLAG.MAP_COL_FLAG_MAPJUMP_06;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_MAPJUMP_07 = MAP_COLLISION_FLAG.MAP_COL_FLAG_MAPJUMP_07;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_MAPJUMP_08 = MAP_COLLISION_FLAG.MAP_COL_FLAG_MAPJUMP_08;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_MAPJUMP_09 = MAP_COLLISION_FLAG.MAP_COL_FLAG_MAPJUMP_09;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_MAPJUMP_10 = MAP_COLLISION_FLAG.MAP_COL_FLAG_MAPJUMP_10;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_MAPJUMP_11 = MAP_COLLISION_FLAG.MAP_COL_FLAG_MAPJUMP_11;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_MAPJUMP_12 = MAP_COLLISION_FLAG.MAP_COL_FLAG_MAPJUMP_12;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_DAMAGE = MAP_COLLISION_FLAG.MAP_COL_FLAG_DAMAGE;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_DAMAGE_01 = MAP_COLLISION_FLAG.MAP_COL_FLAG_DAMAGE_01;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_DAMAGE_02 = MAP_COLLISION_FLAG.MAP_COL_FLAG_DAMAGE_02;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_DAMAGE_03 = MAP_COLLISION_FLAG.MAP_COL_FLAG_DAMAGE_03;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_DAMAGE_04 = MAP_COLLISION_FLAG.MAP_COL_FLAG_DAMAGE_04;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_DAMAGE_05 = MAP_COLLISION_FLAG.MAP_COL_FLAG_DAMAGE_05;

							public const MAP_COLLISION_FLAG MAP_COL_FLAG_MAX = MAP_COLLISION_FLAG.MAP_COL_FLAG_MAX;

							public static int MAP_HEIGHT_GROUND = 0;

							public static int MAP_HEIGHT_SEA = -18108;

							public static int MAPNAME_BUFMAX = 16;

							public static int MapJumpParameterMax = 10;

							public static int MapLandFormParameterMax = 1;

							public static int MapMonsterIDParameterMax = 1;

							public static int MapSoundParameterMax = 1;

							public static int MAP_LANDFORM_PARAM_MAX = 12;

							public static int MAP_MONSTER_PARTY_GROUP_MAX = 5;

							public static int MAP_MONSTER_PARTY_PARAM_MAX = 4;

							public static int MAP_ENCOUNT_BTLFIELD_PARAM_MAX = MAP_LANDFORM_PARAM_MAX;

							public static int MAP_ENCOUNT_REVISE_PARAM_MAX = 30;

							private static readonly uint[] MapObjectShadowType = new uint[78]
							{
								2u, 2u, 1u, 2u, 2u, 2u, 2u, 2u, 2u, 2u,
								2u, 2u, 2u, 2u, 2u, 2u, 2u, 2u, 2u, 2u,
								1u, 2u, 2u, 2u, 2u, 2u, 2u, 2u, 2u, 2u,
								2u, 2u, 2u, 2u, 2u, 2u, 2u, 2u, 2u, 2u,
								2u, 2u, 2u, 2u, 2u, 2u, 2u, 2u, 2u, 2u,
								2u, 2u, 2u, 2u, 2u, 2u, 2u, 2u, 2u, 2u,
								2u, 2u, 2u, 2u, 0u, 0u, 0u, 0u, 0u, 0u,
								0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u
							};

							private static readonly VecFx32[] MapObjectShadowScale = new VecFx32[78]
							{
								new VecFx32(0, 0, 0),
								new VecFx32(8192, 4096, 8192),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(0, 0, 0),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(0, 0, 0),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(8192, 4096, 4096),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(4096, 4096, 4096),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0),
								new VecFx32(0, 0, 0)
							};

							private static readonly int[] MapObjectCollisionRadius = new int[78]
							{
								0, 12288, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096,
								4096, 4096, 4096, 4096, 4096, 49152, 4096, 4096, 49152, 4096,
								20480, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096,
								4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096,
								4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096,
								4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096,
								4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096,
								4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096
							};

							private static readonly int[] MapObjectCheckRadius = new int[78]
							{
								24576, 24576, 8192, 12288, 40960, 8192, 8192, 8192, 8192, 8192,
								8192, 8192, 8192, 8192, 8192, 81920, 8192, 8192, 57344, 8192,
								28672, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192,
								8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192,
								8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192,
								8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192,
								8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192,
								8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192
							};

							public static int[][] MapObjectCollisionAABB = new int[78][]
							{
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 18841, 20480, 11468 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 40960, 20480, 24576 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 20480, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 98304, 20480, 49152 },
								new int[3] { 32768, 20480, 32768 },
								new int[3] { 12288, 20480, 12288 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 65536, 20480, 32768 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 65536, 20480, 32768 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 227737, 20480, 114688 },
								new int[3] { 159744, 20480, 45056 },
								new int[3] { 106496, 20480, 114688 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 28672, 20480, 65536 },
								new int[3] { 24576, 20480, 57344 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 73728, 20480, 73728 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 65536, 20480, 24576 },
								new int[3] { 8192, 20480, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 57344, 20480, 24576 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 },
								new int[3] { 4096, 4096, 4096 }
							};

							private static readonly int[] MapObjectTouchRadius = new int[78]
							{
								32768, 32768, 0, 0, 32768, 0, 0, 0, 0, 0,
								0, 0, 0, 0, 0, 81920, 0, 0, 0, 0,
								32768, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								0, 0, 0, 0, 0, 0, 0, 0
							};

	}
}
