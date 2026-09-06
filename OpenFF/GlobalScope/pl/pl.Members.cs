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
	public static partial class pl
	{
		public const int PARTY_MEMBER_MAX = 4;

		public const int SONG_MAX = 5;

		public const PLAYER_ID PLAYER_1 = PLAYER_ID.PLAYER_1;

		public const PLAYER_ID PLAYER_2 = PLAYER_ID.PLAYER_2;

		public const PLAYER_ID PLAYER_3 = PLAYER_ID.PLAYER_3;

		public const PLAYER_ID PLAYER_4 = PLAYER_ID.PLAYER_4;

		public const PLAYER_ID PLAYER_ID_MAX = PLAYER_ID.PLAYER_ID_MAX;

		public const _PLAYER_CHAINDATA_INDEX INDEX_EXP = _PLAYER_CHAINDATA_INDEX.INDEX_EXP;

		public const _PLAYER_CHAINDATA_INDEX INDEX_JOB_GROW_UP_TYPE = _PLAYER_CHAINDATA_INDEX.INDEX_JOB_GROW_UP_TYPE;

		public const _PLAYER_CHAINDATA_INDEX INDEX_GROW_UP = _PLAYER_CHAINDATA_INDEX.INDEX_GROW_UP;

		public const _PLAYER_CHAINDATA_INDEX INDEX_NORMAL_ATTACK = _PLAYER_CHAINDATA_INDEX.INDEX_NORMAL_ATTACK;

		public const _PLAYER_CHAINDATA_INDEX INDEX_MP01 = _PLAYER_CHAINDATA_INDEX.INDEX_MP01;

		public const _PLAYER_CHAINDATA_INDEX INDEX_MP02 = _PLAYER_CHAINDATA_INDEX.INDEX_MP02;

		public const _PLAYER_CHAINDATA_INDEX INDEX_MP03 = _PLAYER_CHAINDATA_INDEX.INDEX_MP03;

		public const _PLAYER_CHAINDATA_INDEX INDEX_MP04 = _PLAYER_CHAINDATA_INDEX.INDEX_MP04;

		public const _PLAYER_CHAINDATA_INDEX INDEX_MP05 = _PLAYER_CHAINDATA_INDEX.INDEX_MP05;

		public const _PLAYER_CHAINDATA_INDEX INDEX_MP06 = _PLAYER_CHAINDATA_INDEX.INDEX_MP06;

		public const _PLAYER_CHAINDATA_INDEX INDEX_MP07 = _PLAYER_CHAINDATA_INDEX.INDEX_MP07;

		public const _PLAYER_CHAINDATA_INDEX INDEX_JOB_EQUIP_INFO = _PLAYER_CHAINDATA_INDEX.INDEX_JOB_EQUIP_INFO;

		public const _PLAYER_CHAINDATA_INDEX INDEX_NORMAL_MAGIC = _PLAYER_CHAINDATA_INDEX.INDEX_NORMAL_MAGIC;

		public const _PLAYER_CHAINDATA_INDEX INDEX_ABILITY = _PLAYER_CHAINDATA_INDEX.INDEX_ABILITY;

		public const _PLAYER_CHAINDATA_INDEX INDEX_INITIALIZE_ABILITY = _PLAYER_CHAINDATA_INDEX.INDEX_INITIALIZE_ABILITY;

		public const _PLAYER_CHAINDATA_INDEX _PLAYER_CHAINDATA_INDEX_MAX = _PLAYER_CHAINDATA_INDEX._PLAYER_CHAINDATA_INDEX_MAX;

		public const GROW_UP_TYPE GROW_UP_S = GROW_UP_TYPE.GROW_UP_S;

		public const GROW_UP_TYPE GROW_UP_A = GROW_UP_TYPE.GROW_UP_A;

		public const GROW_UP_TYPE GROW_UP_B = GROW_UP_TYPE.GROW_UP_B;

		public const GROW_UP_TYPE GROW_UP_C = GROW_UP_TYPE.GROW_UP_C;

		public const GROW_UP_TYPE GROW_UP_D = GROW_UP_TYPE.GROW_UP_D;

		public const GROW_UP_TYPE GROW_UP_E = GROW_UP_TYPE.GROW_UP_E;

		public const GROW_UP_TYPE GROW_UP_F = GROW_UP_TYPE.GROW_UP_F;

		public const GROW_UP_TYPE GROW_UP_Z = GROW_UP_TYPE.GROW_UP_Z;

		public const GROW_UP_TYPE GROW_UP_TYPE_MAX = GROW_UP_TYPE.GROW_UP_TYPE_MAX;

		public const GROW_UP_PARAMETER GROW_UP_STR = GROW_UP_PARAMETER.GROW_UP_STR;

		public const GROW_UP_PARAMETER GROW_UP_VIT = GROW_UP_PARAMETER.GROW_UP_VIT;

		public const GROW_UP_PARAMETER GROW_UP_DEX = GROW_UP_PARAMETER.GROW_UP_DEX;

		public const GROW_UP_PARAMETER GORW_UP_INT = GROW_UP_PARAMETER.GORW_UP_INT;

		public const GROW_UP_PARAMETER GROW_UP_MIN = GROW_UP_PARAMETER.GROW_UP_MIN;

		public const GROW_UP_PARAMETER GROW_UP_MP = GROW_UP_PARAMETER.GROW_UP_MP;

		public const GROW_UP_PARAMETER GROW_UP_PARAMETER_MAX = GROW_UP_PARAMETER.GROW_UP_PARAMETER_MAX;

		public const ATTACK_MOTION ATTACK_MOTION_MAX = ATTACK_MOTION.ATTACK_MOTION_MAX;

		public const FORMATION_TYPE FRONT = FORMATION_TYPE.FRONT;

		public const FORMATION_TYPE BACK = FORMATION_TYPE.BACK;

		public const FORMATION_TYPE FORMATION_TYPE_MAX = FORMATION_TYPE.FORMATION_TYPE_MAX;

		public const HAND_TYPE RIGHT_HAND = HAND_TYPE.RIGHT_HAND;

		public const HAND_TYPE LEFT_HAND = HAND_TYPE.LEFT_HAND;

		public const HAND_TYPE HAND_TYPE_MAX = HAND_TYPE.HAND_TYPE_MAX;

		public const HAND_TYPE NO_HAND = HAND_TYPE.NO_HAND;

		public const MAGIC_LEVEL MAGIC_LEVEL1 = MAGIC_LEVEL.MAGIC_LEVEL1;

		public const MAGIC_LEVEL MAGIC_LEVEL2 = MAGIC_LEVEL.MAGIC_LEVEL2;

		public const MAGIC_LEVEL MAGIC_LEVEL3 = MAGIC_LEVEL.MAGIC_LEVEL3;

		public const MAGIC_LEVEL MAGIC_LEVEL4 = MAGIC_LEVEL.MAGIC_LEVEL4;

		public const MAGIC_LEVEL MAGIC_LEVEL5 = MAGIC_LEVEL.MAGIC_LEVEL5;

		public const MAGIC_LEVEL MAGIC_LEVEL6 = MAGIC_LEVEL.MAGIC_LEVEL6;

		public const MAGIC_LEVEL MAGIC_LEVEL7 = MAGIC_LEVEL.MAGIC_LEVEL7;

		public const MAGIC_LEVEL MAGIC_LEVEL8 = MAGIC_LEVEL.MAGIC_LEVEL8;

		public const MAGIC_LEVEL MAGIC_LEVEL_MAX = MAGIC_LEVEL.MAGIC_LEVEL_MAX;

		public const GET_SKILL_TYPE GET_RIGHT_HAND = GET_SKILL_TYPE.GET_RIGHT_HAND;

		public const GET_SKILL_TYPE GET_LEFT_HAND = GET_SKILL_TYPE.GET_LEFT_HAND;

		public const GET_SKILL_TYPE GET_MAGIC = GET_SKILL_TYPE.GET_MAGIC;

		public const GET_SKILL_TYPE GET_SKILL_TYPE_MAX = GET_SKILL_TYPE.GET_SKILL_TYPE_MAX;

		public const ABILITY_ID ABILITY_ERR_ID = ABILITY_ID.ABILITY_ERR_ID;

		public const ABILITY_ID ABILITY_ATTACK = ABILITY_ID.ABILITY_ATTACK;

		public const ABILITY_ID ABILITY_ESCAPE = ABILITY_ID.ABILITY_ESCAPE;

		public const ABILITY_ID ABILITY_GUARD = ABILITY_ID.ABILITY_GUARD;

		public const ABILITY_ID ABILITY_ITEM = ABILITY_ID.ABILITY_ITEM;

		public const ABILITY_ID ABILITY_BLACK_MAGIC = ABILITY_ID.ABILITY_BLACK_MAGIC;

		public const ABILITY_ID ABILITY_WHITE_MAGIC = ABILITY_ID.ABILITY_WHITE_MAGIC;

		public const ABILITY_ID ABILITY_STEAL = ABILITY_ID.ABILITY_STEAL;

		public const ABILITY_ID ABILITY_TAKE_A_POWDER = ABILITY_ID.ABILITY_TAKE_A_POWDER;

		public const ABILITY_ID ABILITY_BERSERK = ABILITY_ID.ABILITY_BERSERK;

		public const ABILITY_ID ABILITY_COVER_UP = ABILITY_ID.ABILITY_COVER_UP;

		public const ABILITY_ID ABILITY_KNOCK_OVER = ABILITY_ID.ABILITY_KNOCK_OVER;

		public const ABILITY_ID ABILITY_BLOW = ABILITY_ID.ABILITY_BLOW;

		public const ABILITY_ID ABILITY_SUMMON = ABILITY_ID.ABILITY_SUMMON;

		public const ABILITY_ID ABILITY_CHAKRA = ABILITY_ID.ABILITY_CHAKRA;

		public const ABILITY_ID ABILITY_COUNTER = ABILITY_ID.ABILITY_COUNTER;

		public const ABILITY_ID ABILITY_PILLAGE = ABILITY_ID.ABILITY_PILLAGE;

		public const ABILITY_ID ABILITY_SECOND_MAGIC = ABILITY_ID.ABILITY_SECOND_MAGIC;

		public const ABILITY_ID ABILITY_SONG = ABILITY_ID.ABILITY_SONG;

		public const ABILITY_ID ABILITY_HIDE = ABILITY_ID.ABILITY_HIDE;

		public const ABILITY_ID ABILITY_BACKUP = ABILITY_ID.ABILITY_BACKUP;

		public const ABILITY_ID ABILITY_COVER = ABILITY_ID.ABILITY_COVER;

		public const ABILITY_ID ABILITY_SHIELD = ABILITY_ID.ABILITY_SHIELD;

		public const ABILITY_ID ABILITY_AIM = ABILITY_ID.ABILITY_AIM;

		public const ABILITY_ID ABILITY_SHADOW = ABILITY_ID.ABILITY_SHADOW;

		public const ABILITY_ID ABILITY_OVERISSUE = ABILITY_ID.ABILITY_OVERISSUE;

		public const ABILITY_ID ABILITY_CHECK = ABILITY_ID.ABILITY_CHECK;

		public const ABILITY_ID ABILITY_DETECT = ABILITY_ID.ABILITY_DETECT;

		public const ABILITY_ID ABILITY_KNOWLEDGE = ABILITY_ID.ABILITY_KNOWLEDGE;

		public const ABILITY_ID ABILITY_JUMP = ABILITY_ID.ABILITY_JUMP;

		public const ABILITY_ID ABILITY_DARK = ABILITY_ID.ABILITY_DARK;

		public const ABILITY_ID ABILITY_DESPERATE = ABILITY_ID.ABILITY_DESPERATE;

		public const ABILITY_ID ABILITY_GEOGRAPHY = ABILITY_ID.ABILITY_GEOGRAPHY;

		public const ABILITY_ID ABILITY_DEMONS_GATE = ABILITY_ID.ABILITY_DEMONS_GATE;

		public const ABILITY_ID ABILITY_DRAGON = ABILITY_ID.ABILITY_DRAGON;

		public const ABILITY_ID ABILITY_ROLL_UP = ABILITY_ID.ABILITY_ROLL_UP;

		public const ABILITY_ID ABILITY_POISE = ABILITY_ID.ABILITY_POISE;

		public const ABILITY_ID ABILITY_MEDITATION = ABILITY_ID.ABILITY_MEDITATION;

		public const ABILITY_ID ABILITY_TRANS = ABILITY_ID.ABILITY_TRANS;

		public const ABILITY_ID ABILITY_PUPPETEER = ABILITY_ID.ABILITY_PUPPETEER;

		public const ABILITY_ID ABILITY_PITCH = ABILITY_ID.ABILITY_PITCH;

		public const ABILITY_ID ABILITY_TWIN = ABILITY_ID.ABILITY_TWIN;

		public const ABILITY_ID ABILITY_NINJYA_DROP = ABILITY_ID.ABILITY_NINJYA_DROP;

		public const ABILITY_ID ABILITY_CANCEL = ABILITY_ID.ABILITY_CANCEL;

		public const ABILITY_ID ABILITY_CHANGE = ABILITY_ID.ABILITY_CHANGE;

		public const ABILITY_ID ABILITY_PROVOCATION = ABILITY_ID.ABILITY_PROVOCATION;

		public const ABILITY_ID ABILITY_MAGIC = ABILITY_ID.ABILITY_MAGIC;

		public const ABILITY_ID ABILITY_EQUIP = ABILITY_ID.ABILITY_EQUIP;

		public const ABILITY_ID ABILITY_FRONT = ABILITY_ID.ABILITY_FRONT;

		public const ABILITY_ID ABILITY_BACK = ABILITY_ID.ABILITY_BACK;

		public const ABILITY_ID ABILITY_LIST_MAX = ABILITY_ID.ABILITY_LIST_MAX;

		public const JOB_TYPE SUPPINN = JOB_TYPE.SUPPINN;

		public const JOB_TYPE ONION_SWORDER = JOB_TYPE.ONION_SWORDER;

		public const JOB_TYPE FIGHTER = JOB_TYPE.FIGHTER;

		public const JOB_TYPE MONK = JOB_TYPE.MONK;

		public const JOB_TYPE WHITE_MAGICIAN = JOB_TYPE.WHITE_MAGICIAN;

		public const JOB_TYPE BLACK_MAGICIAN = JOB_TYPE.BLACK_MAGICIAN;

		public const JOB_TYPE RED_MAGICIAN = JOB_TYPE.RED_MAGICIAN;

		public const JOB_TYPE HUNTER = JOB_TYPE.HUNTER;

		public const JOB_TYPE KNIGHT = JOB_TYPE.KNIGHT;

		public const JOB_TYPE THIEF = JOB_TYPE.THIEF;

		public const JOB_TYPE BOOK_MAN = JOB_TYPE.BOOK_MAN;

		public const JOB_TYPE GEOMANCER = JOB_TYPE.GEOMANCER;

		public const JOB_TYPE DRAGON_KNIGHT = JOB_TYPE.DRAGON_KNIGHT;

		public const JOB_TYPE VIKING = JOB_TYPE.VIKING;

		public const JOB_TYPE EVIL_SOWRDER = JOB_TYPE.EVIL_SOWRDER;

		public const JOB_TYPE PHANTOMER = JOB_TYPE.PHANTOMER;

		public const JOB_TYPE BARD = JOB_TYPE.BARD;

		public const JOB_TYPE KARATE_MASTER = JOB_TYPE.KARATE_MASTER;

		public const JOB_TYPE IMAM = JOB_TYPE.IMAM;

		public const JOB_TYPE DEVIL_MAN = JOB_TYPE.DEVIL_MAN;

		public const JOB_TYPE DEVILDOM_PHANTOMER = JOB_TYPE.DEVILDOM_PHANTOMER;

		public const JOB_TYPE SAGE = JOB_TYPE.SAGE;

		public const JOB_TYPE NINJA = JOB_TYPE.NINJA;

		public const JOB_TYPE JOB_TYPE_MAX = JOB_TYPE.JOB_TYPE_MAX;

		public const uint PLAYER_CHARACTER_HUMAN_NUM = 24u;

		public const uint PLAYER_CHARACTER_VEHICLE_NUM = 4u;

		public const uint PLAYER_CHARACTER_NUM = 28u;

		public const PLAYER_INDEX PLAYER_INDEX_HERO = PLAYER_INDEX.PLAYER_INDEX_HERO;

		public const PLAYER_INDEX PLAYER_INDEX_VEHICLE = PLAYER_INDEX.PLAYER_INDEX_VEHICLE;

		public const PLAYER_INDEX PLAYER_INDEX_MAP_OBJECT = PLAYER_INDEX.PLAYER_INDEX_MAP_OBJECT;

		public const PLAYER_INDEX PLAYER_INDEX_MAX = PLAYER_INDEX.PLAYER_INDEX_MAX;

		public const PLAYER_PARAM_CATEGORY PLAYER_PARAM_CATEGORY_MOVE = PLAYER_PARAM_CATEGORY.PLAYER_PARAM_CATEGORY_MOVE;

		public const PLAYER_PARAM_CATEGORY PLAYER_PARAM_CATEGORY_ENTER = PLAYER_PARAM_CATEGORY.PLAYER_PARAM_CATEGORY_ENTER;

		public const PLAYER_PARAM_CATEGORY PLAYER_PARAM_CATEGORY_VEHICLE_MOVE = PLAYER_PARAM_CATEGORY.PLAYER_PARAM_CATEGORY_VEHICLE_MOVE;

		public const PLAYER_PARAM_CATEGORY PLAYER_PARAM_CATEGORY_VEHICLE_ENTER = PLAYER_PARAM_CATEGORY.PLAYER_PARAM_CATEGORY_VEHICLE_ENTER;

		public const PLAYER_PARAM_CATEGORY PLAYER_PARAM_CATEGORY_SE_EFFECT_PLAY = PLAYER_PARAM_CATEGORY.PLAYER_PARAM_CATEGORY_SE_EFFECT_PLAY;

		public const PLAYER_PARAM_CATEGORY PLAYER_PARAM_CATEGORY_SE_EFFECT_MAP = PLAYER_PARAM_CATEGORY.PLAYER_PARAM_CATEGORY_SE_EFFECT_MAP;

		public const PLAYER_PARAM_CATEGORY PLAYER_PARAM_CATEGORY_MAX = PLAYER_PARAM_CATEGORY.PLAYER_PARAM_CATEGORY_MAX;

		public const PLAYER_TYPE PLAYER_TYPE_ERR = PLAYER_TYPE.PLAYER_TYPE_ERR;

		public const PLAYER_TYPE PLAYER_TYPE_HUMAN = PLAYER_TYPE.PLAYER_TYPE_HUMAN;

		public const PLAYER_TYPE PLAYER_TYPE_FROG = PLAYER_TYPE.PLAYER_TYPE_FROG;

		public const PLAYER_TYPE PLAYER_TYPE_MINI = PLAYER_TYPE.PLAYER_TYPE_MINI;

		public const PLAYER_TYPE PLAYER_TYPE_CHOKOBO = PLAYER_TYPE.PLAYER_TYPE_CHOKOBO;

		public const PLAYER_TYPE PLAYER_TYPE_SHIDO_H = PLAYER_TYPE.PLAYER_TYPE_SHIDO_H;

		public const PLAYER_TYPE PLAYER_TYPE_CANOE = PLAYER_TYPE.PLAYER_TYPE_CANOE;

		public const PLAYER_TYPE PLAYER_TYPE_ENTERP = PLAYER_TYPE.PLAYER_TYPE_ENTERP;

		public const PLAYER_TYPE PLAYER_TYPE_ENTERP_CTM = PLAYER_TYPE.PLAYER_TYPE_ENTERP_CTM;

		public const PLAYER_TYPE PLAYER_TYPE_NORCHI = PLAYER_TYPE.PLAYER_TYPE_NORCHI;

		public const PLAYER_TYPE PLAYER_TYPE_NORCHI_CTM = PLAYER_TYPE.PLAYER_TYPE_NORCHI_CTM;

		public const PLAYER_TYPE PLAYER_TYPE_INVINSIBLE = PLAYER_TYPE.PLAYER_TYPE_INVINSIBLE;

		public const PLAYER_TYPE PLAYER_TYPE_MAX = PLAYER_TYPE.PLAYER_TYPE_MAX;

		public const PLAYER_HUMAN_TYPE PLAYER_HUMAN_TYPE_ERR = PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_ERR;

		public const PLAYER_HUMAN_TYPE PLAYER_HUMAN_TYPE_HUMAN = PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_HUMAN;

		public const PLAYER_HUMAN_TYPE PLAYER_HUMAN_TYPE_MINI = PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_MINI;

		public const PLAYER_HUMAN_TYPE PLAYER_HUMAN_TYPE_FROG = PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_FROG;

		public const PLAYER_HUMAN_TYPE PLAYER_HUMAN_TYPE_CHOKOBO = PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_CHOKOBO;

		public const PLAYER_HUMAN_TYPE PLAYER_HUMAN_TYPE_FAIRY = PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_FAIRY;

		public const PLAYER_HUMAN_TYPE PLAYER_HUMAN_TYPE_SHEEP = PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_SHEEP;

		public const PLAYER_HUMAN_TYPE PLAYER_HUMAN_TYPE_MAX = PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_MAX;

		public const PLAYER_VEHICLE_TYPE PLAYER_VEHICLE_TYPE_ERR = PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ERR;

		public const PLAYER_VEHICLE_TYPE PLAYER_VEHICLE_TYPE_CHOKOBO = PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CHOKOBO;

		public const PLAYER_VEHICLE_TYPE PLAYER_VEHICLE_TYPE_SHIDO_H = PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_SHIDO_H;

		public const PLAYER_VEHICLE_TYPE PLAYER_VEHICLE_TYPE_CANOE = PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CANOE;

		public const PLAYER_VEHICLE_TYPE PLAYER_VEHICLE_TYPE_ENTERP = PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP;

		public const PLAYER_VEHICLE_TYPE PLAYER_VEHICLE_TYPE_ENTERP_CTM = PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM;

		public const PLAYER_VEHICLE_TYPE PLAYER_VEHICLE_TYPE_NORCHI = PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_NORCHI;

		public const PLAYER_VEHICLE_TYPE PLAYER_VEHICLE_TYPE_NORCHI_CTM = PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_NORCHI_CTM;

		public const PLAYER_VEHICLE_TYPE PLAYER_VEHICLE_TYPE_INVINSIBLE = PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_INVINSIBLE;

		public const PLAYER_VEHICLE_TYPE PLAYER_VEHICLE_TYPE_MAX = PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_MAX;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_ERR = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_ERR;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_HERO_FIELD = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_FIELD;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_HERO_TOWN = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_TOWN;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_FROG = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_FROG;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_LILLIPUT = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_LILLIPUT;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_CHOKOBO = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_CHOKOBO;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_FAIRY = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_FAIRY;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_SHEEP = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_SHEEP;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_SHIDO_H = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_SHIDO_H;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_CANOE = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_CANOE;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_ENTERP = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_ENTERP;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_ENTERP_CTM = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_ENTERP_CTM;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_NORCHI = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_NORCHI;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_NORCHI_CTM = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_NORCHI_CTM;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_INVINSIBLE = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_INVINSIBLE;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_DEFAULT_NPC = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_DEFAULT_NPC;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_FRIEND_NPC_FIELD = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_FRIEND_NPC_FIELD;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_FRIEND_NPC_TOWN = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_FRIEND_NPC_TOWN;

		public const PLAYER_MOVE_TYPE PLAYER_MOVE_TYPE_MAX = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_MAX;

		public const NPC_PARAM_CATEGORY NPC_PARAM_CATEGORY_RANDOM_MOVE = NPC_PARAM_CATEGORY.NPC_PARAM_CATEGORY_RANDOM_MOVE;

		public const NPC_PARAM_CATEGORY NPC_PARAM_CATEGORY_AUTO_FOLLOW = NPC_PARAM_CATEGORY.NPC_PARAM_CATEGORY_AUTO_FOLLOW;

		public const NPC_PARAM_CATEGORY NPC_PARAM_CATEGORY_MAX = NPC_PARAM_CATEGORY.NPC_PARAM_CATEGORY_MAX;

		public const NPC_RANDOM_MOVE_TYPE NPC_RANDOM_MOVE_TYPE_ERR = NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_ERR;

		public const NPC_RANDOM_MOVE_TYPE NPC_RANDOM_MOVE_TYPE_DEFAULT = NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_DEFAULT;

		public const NPC_RANDOM_MOVE_TYPE NPC_RANDOM_MOVE_TYPE_MAN = NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_MAN;

		public const NPC_RANDOM_MOVE_TYPE NPC_RANDOM_MOVE_TYPE_WOMAN = NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_WOMAN;

		public const NPC_RANDOM_MOVE_TYPE NPC_RANDOM_MOVE_TYPE_BOY = NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_BOY;

		public const NPC_RANDOM_MOVE_TYPE NPC_RANDOM_MOVE_TYPE_GIRL = NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_GIRL;

		public const NPC_RANDOM_MOVE_TYPE NPC_RANDOM_MOVE_TYPE_UNCLE = NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_UNCLE;

		public const NPC_RANDOM_MOVE_TYPE NPC_RANDOM_MOVE_TYPE_AUNT = NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_AUNT;

		public const NPC_RANDOM_MOVE_TYPE NPC_RANDOM_MOVE_TYPE_OLD_MAN = NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_OLD_MAN;

		public const NPC_RANDOM_MOVE_TYPE NPC_RANDOM_MOVE_TYPE_OLD_WOMAN = NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_OLD_WOMAN;

		public const NPC_RANDOM_MOVE_TYPE NPC_RANDOM_MOVE_TYPE_MAX = NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_MAX;

		public const NPC_AUTO_FOLLOW_TYPE NPC_AUTO_FOLLOW_TYPE_ERR = NPC_AUTO_FOLLOW_TYPE.NPC_AUTO_FOLLOW_TYPE_ERR;

		public const NPC_AUTO_FOLLOW_TYPE NPC_AUTO_FOLLOW_TYPE_DEFAULT = NPC_AUTO_FOLLOW_TYPE.NPC_AUTO_FOLLOW_TYPE_DEFAULT;

		public const NPC_AUTO_FOLLOW_TYPE NPC_AUTO_FOLLOW_TYPE_UNE = NPC_AUTO_FOLLOW_TYPE.NPC_AUTO_FOLLOW_TYPE_UNE;

		public const NPC_AUTO_FOLLOW_TYPE NPC_AUTO_FOLLOW_TYPE_DORGA = NPC_AUTO_FOLLOW_TYPE.NPC_AUTO_FOLLOW_TYPE_DORGA;

		public const NPC_AUTO_FOLLOW_TYPE NPC_AUTO_FOLLOW_TYPE_MAX = NPC_AUTO_FOLLOW_TYPE.NPC_AUTO_FOLLOW_TYPE_MAX;

		public const INPUT_MODE INPUT_MODE_FIELD = INPUT_MODE.INPUT_MODE_FIELD;

		public const INPUT_MODE INPUT_MODE_TOWN = INPUT_MODE.INPUT_MODE_TOWN;

		public const INPUT_MODE INPUT_MODE_MAX = INPUT_MODE.INPUT_MODE_MAX;

		public const HUMAN_COMMON_ACTION_ID HUMAN_COMMON_ACTION_ID_WAIT = HUMAN_COMMON_ACTION_ID.HUMAN_COMMON_ACTION_ID_WAIT;

		public const HUMAN_COMMON_ACTION_ID HUMAN_COMMON_ACTION_ID_LEAVE_WAIT = HUMAN_COMMON_ACTION_ID.HUMAN_COMMON_ACTION_ID_LEAVE_WAIT;

		public const HUMAN_COMMON_ACTION_ID HUMAN_COMMON_ACTION_ID_LEAVE_WAIT_LOOP = HUMAN_COMMON_ACTION_ID.HUMAN_COMMON_ACTION_ID_LEAVE_WAIT_LOOP;

		public const HUMAN_COMMON_ACTION_ID HUMAN_COMMON_ACTION_ID_WALK = HUMAN_COMMON_ACTION_ID.HUMAN_COMMON_ACTION_ID_WALK;

		public const HUMAN_COMMON_ACTION_ID HUMAN_COMMON_ACTION_ID_RUN = HUMAN_COMMON_ACTION_ID.HUMAN_COMMON_ACTION_ID_RUN;

		public const HUMAN_COMMON_ACTION_ID HUMAN_COMMON_ACTION_ID_TALK = HUMAN_COMMON_ACTION_ID.HUMAN_COMMON_ACTION_ID_TALK;

		public const HUMAN_COMMON_ACTION_ID HUMAN_COMMON_ACTION_ID_USE_ITEM = HUMAN_COMMON_ACTION_ID.HUMAN_COMMON_ACTION_ID_USE_ITEM;

		public const HUMAN_COMMON_ACTION_ID HUMAN_COMMON_ACTION_ID_SLEEP = HUMAN_COMMON_ACTION_ID.HUMAN_COMMON_ACTION_ID_SLEEP;

		public const HUMAN_COMMON_ACTION_ID HUMAN_COMMON_ACTION_ID_PLAYING_PIANO = HUMAN_COMMON_ACTION_ID.HUMAN_COMMON_ACTION_ID_PLAYING_PIANO;

		public const HUMAN_COMMON_ACTION_ID HUMAN_COMMON_ACTION_ID_STEPPING = HUMAN_COMMON_ACTION_ID.HUMAN_COMMON_ACTION_ID_STEPPING;

		public const HUMAN_COMMON_ACTION_ID HUMAN_COMMON_ACTION_ID_MAX = HUMAN_COMMON_ACTION_ID.HUMAN_COMMON_ACTION_ID_MAX;

		public const CANOE_MOTION_ID CANOE_MOTION_ID_WAIT = CANOE_MOTION_ID.CANOE_MOTION_ID_WAIT;

		public const CANOE_MOTION_ID CANOE_MOTION_ID_MOVE = CANOE_MOTION_ID.CANOE_MOTION_ID_MOVE;

		public const CANOE_MOTION_ID CANOE_MOTION_ID_MAX = CANOE_MOTION_ID.CANOE_MOTION_ID_MAX;

		private static int COMMAND_ABILITY_MAX = 4;

		private static int PASSIVE_ABILITY_MAX = 2;

		private static byte[] JobSkillExp = new byte[23]
		{
			20, 8, 14, 14, 10, 10, 12, 14, 12, 18,
			24, 14, 16, 14, 14, 10, 18, 14, 10, 10,
			12, 10, 12
		};

		public static short DefaultHp = 32;

		public static int PLAYER_LEVEL_MAX = 99;

		public static int PLAYER_HP_MAX = 9999;

		private static int PLAYER_SKILL_MAX = 99;

		public static int GROW_UP_MP_TYPE_MAX = 7;

		public static int PLAYER_NAME_MAX = 25;

		public static int MAGIC_ONCE_LEVEL_EQUIP_MAX = 3;

		public static int COMMAND_MAX = 7;

		private static int COMMON_COMMAND_MAX = 2;

		public static int BATTLE_COMMAND_MAX = 4;

		public static int PLAYER_EFFEXTS_MAX = 2;

		private static int JOB_PENALTY_TIME_MAX = 10;

		public static string[] PlayerVehicleFileName = new string[8] { "n442", "n451", "n461", "n471", "n481", "n491", "n491", "n511" };

		private static string[] LOCATE_NAME = new string[2] { "L_te", "R_te" };

		public static int CAMERAHEIGHT_PARAM = -16384;

		public static int SHADOWSCALE_PARAM = -204;

		public static int g_cameraHeightOrg = 0;

		public static VecFx32 g_shadowScaleOrg = new VecFx32(6144, 8192, 6144);

		public static int CHOKOBO_MOTIONNO_WAIT = 1001;

		public static int CHOKOBO_MOTIONNO_WALK = 1004;

		public static int CHOKOBO_MOTIONNO_RUN = 1005;

		public static dv.pad.KEY_COMPARISON CHOKOBO_KEY_RUN = dv.pad.KEY_COMPARISON.KEY_BRIGHT;

		public static dv.pad.KEY_COMPARISON CHOKOBO_KEY_DROP = dv.pad.KEY_COMPARISON.KEY_BDOWN;

		public static int ENTERP_MOTIONNO_AIR_WAIT = 1001;

		public static int ENTERP_MOTIONNO_INPROPELLER = 1002;

		public static int ENTERP_MOTIONNO_OUTMAST = 1003;

		public static int ENTERP_MOTIONNO_SHIP_WAIT = 2001;

		public static int ENTERP_MOTIONNO_SHIP_MOVE = 2002;

		public static int ENTERP_MOTIONNO_INMAST = 2003;

		public static int ENTERP_MOTIONNO_OUTPROPELLER = 2004;

		public static int ACTION_ID_HIGHNAVIGATE = 4;

		public static int ACTION_ID_ENTERINSIDE = 5;

		private static VecFx32 pl_reuse_v0 = new VecFx32();

		private static VecFx32 pl_reuse_v1 = new VecFx32();

		private static VecFx32 pl_reuse_v2 = new VecFx32();

		private static VecFx32 pl_reuse_v3 = new VecFx32();

		private static VecFx32 pl_reuse_v4 = new VecFx32();

		private static mcl.CollisionResult pl_reuse_result = new mcl.CollisionResult();

		private static mcl.CollisionResult pl_reuse_result2 = new mcl.CollisionResult();

		public static uint MAP_OBJECT_NUM = 24u;

		public static uint FIELD_CHARACTER_NUM = 28 + MAP_OBJECT_NUM;

		private static uint ENVIRONMENT_DAMAGE_INTERVAL = 30u;

		private static uint POISON_DAMAGE_INTERVAL = 150u;

		private static ds.pri.DSSphere pl_reuse_sphere = new ds.pri.DSSphere();

		private static ds.pri.DSAABB pl_reuse_aabb = new ds.pri.DSAABB();

		private static ds.pri.DSSphere pl_reuse_plCheckSphere = new ds.pri.DSSphere();

		private static ds.pri.DSSphere pl_reuse_taCheckSphere = new ds.pri.DSSphere();

		private static ds.pri.DSLine pl_reuse_line = new ds.pri.DSLine();

		private static ds.pri.DSSphere[] pl_reuse__ColSphere = new ds.pri.DSSphere[2]
		{
			new ds.pri.DSSphere(),
			new ds.pri.DSSphere()
		};

		private static ds.pri.DSSphere[] pl_reuse__CckSphere = new ds.pri.DSSphere[2]
		{
			new ds.pri.DSSphere(),
			new ds.pri.DSSphere()
		};

		private static ds.pri.DSAABB[] pl_reuse__ColAABB = new ds.pri.DSAABB[2]
		{
			new ds.pri.DSAABB(),
			new ds.pri.DSAABB()
		};

		internal static void getPlayerInitialName(ref string Name, int playerID)
		{
			if (playerID < 0 || playerID >= 4)
			{
				strcpy(out Name, "error");
				return;
			}
			string[] array = new string[4] { "Luneth", "Arc", "Refia", "Ingus" };
			string[] array2 = new string[4] { "ルーネス", "アルクゥ", "レフィア", "イングズ" };
			string[] array3 = new string[4] { "鲁内斯", "阿尔克", "蕾菲亚", "因古斯" };
			string[] array4 = new string[4] { "魯內斯", "阿爾克", "蕾菲亞", "因古斯" };
			switch (OS_GetLanguage())
			{
			case 0:
				strcpy(out Name, array2[playerID]);
				break;
			case 6:
				strcpy(out Name, array3[playerID]);
				break;
			case 7:
				strcpy(out Name, array4[playerID]);
				break;
			default:
				strcpy(out Name, array[playerID]);
				break;
			}
		}

		internal static bool canCheckAction(chr.CBaseCharacter pPlayer, chr.CBaseCharacter pTarget)
		{
			ds.pri.DSSphere dSSphere = pl_reuse_plCheckSphere;
			ds.pri.DSSphere dSSphere2 = pl_reuse_taCheckSphere;
			dSSphere.r = const_cast<chr.CBaseCharacter>(pPlayer).getCckRadius();
			dSSphere.c.copy(const_cast<chr.CBaseCharacter>(pPlayer).getPosition());
			dSSphere2.r = const_cast<chr.CBaseCharacter>(pTarget).getCckRadius();
			dSSphere2.c.copy(const_cast<chr.CBaseCharacter>(pTarget).getPosition());
			return ds.pri.PrimitiveTest.testSphereSphere(dSSphere, dSSphere2);
		}

		internal static bool canBoardVehicle(CPlayerCharacter plchara)
		{
			CPlayerVehicle cPlayerVehicle = (CPlayerVehicle)plchara.getTarget();
			if (cPlayerVehicle == null)
			{
				return false;
			}
			if (cPlayerVehicle.canBoard())
			{
				if (PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP == cPlayerVehicle.getVehicleType() || PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM == cPlayerVehicle.getVehicleType())
				{
					if (plchara.getColResultWall(3).hit == 0)
					{
						return false;
					}
					VecFx32 vecFx = new VecFx32(cPlayerVehicle.getPosition().x, 0, cPlayerVehicle.getPosition().z);
					VecFx32 vecFx2 = new VecFx32(plchara.getPosition().x, 0, plchara.getPosition().z);
					VecFx32 vecFx3 = new VecFx32(0, 0, 0);
					VEC_Subtract(vecFx, vecFx2, vecFx3);
					int num = VEC_Mag(vecFx3);
					if (num > 61440)
					{
						return false;
					}
					VecFx32 vecFx4 = new VecFx32(plchara.getTargetDirection());
					vecFx4.y = 0;
					VEC_Normalize(vecFx4, vecFx4);
					ds.pri.DSLine dSLine = pl_reuse_line;
					ds.pri.DSSphere dSSphere = pl_reuse_sphere;
					dSLine.setDSLineForPointDir(vecFx2, vecFx4);
					dSSphere.set(vecFx, 12288);
					if (ds.pri.PrimitiveTest.testRaySphere(dSLine, dSSphere, null, null))
					{
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		internal static bool isWalk()
		{
			if ((dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 0xF) == 0)
			{
				return false;
			}
			bool flag = (dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 0x20) != 0;
			if (opt.COptionManager.getSingleton().gameOption().worldMoveType() == opt.WORLD_MOVE_TYPE.WORLD_MOVE_TYPE_WALK)
			{
				if (!flag)
				{
					return true;
				}
			}
			else if (opt.WORLD_MOVE_TYPE.WORLD_MOVE_TYPE_RUN == opt.COptionManager.getSingleton().gameOption().worldMoveType() && flag)
			{
				return true;
			}
			return false;
		}

		internal static bool isRun()
		{
			if ((dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 0xF) == 0)
			{
				return false;
			}
			bool flag = (dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 0x20) != 0;
			if (opt.COptionManager.getSingleton().gameOption().worldMoveType() == opt.WORLD_MOVE_TYPE.WORLD_MOVE_TYPE_WALK)
			{
				if (flag)
				{
					return true;
				}
			}
			else if (opt.WORLD_MOVE_TYPE.WORLD_MOVE_TYPE_RUN == opt.COptionManager.getSingleton().gameOption().worldMoveType() && !flag)
			{
				return true;
			}
			return false;
		}

		internal static void gotoWorldTalk(CPlayerCharacter pl)
		{
			CPlayerHuman cPlayerHuman = (CPlayerHuman)pl;
			cPlayerHuman.AutoRun_set(arg0: false);
			cPlayerHuman.setTarget(cPlayerHuman.getNpc());
			cPlayerHuman.setNextAct(4);
		}

		internal static bool canWorldTalk(CPlayerCharacter pl)
		{
			CPlayerHuman cPlayerHuman = (CPlayerHuman)pl;
			if ((dv.CDeviceManager.getInstance().Pad().edge_trs(0) & 0x40) == 0 && !CCastCommandTransit.getInstance().cast_Field2D().TalkButton()
				.isTouch())
			{
				return false;
			}
			if (!cPlayerHuman.hasTalkNpc())
			{
				return false;
			}
			if (!cPlayerHuman.InputPermission())
			{
				return false;
			}
			if (!cPlayerHuman.isOperater())
			{
				return false;
			}
			if (cPlayerHuman.getNpc() == null)
			{
				return false;
			}
			if (!dgs.CFade.Main().isCleared())
			{
				return false;
			}
			if (cPlayerHuman.getNpc().CharaCheckType() != chr.CHARACTER_CHECK_TYPE.CHARACTER_CHECK_TYPE_TALK)
			{
				return false;
			}
			if (CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW != cPlayerHuman.getNpc().NPCAiManager().AiKind())
			{
				return false;
			}
			return true;
		}

		internal static bool canKeyDoorByDirection(CBasePlayer pl)
		{
			VecFx32 a = new VecFx32(0, 0, -4096);
			int num = 4014;
			VecFx32 vecFx = new VecFx32(pl.getDirection());
			VEC_Normalize(vecFx, vecFx);
			int num2 = VEC_DotProduct(a, vecFx);
			return num <= num2;
		}

		internal static CPlayerVehicle getVehicle(CPlayerCharacter pl)
		{
			if (pl.isOperater())
			{
				return (CPlayerVehicle)pl.getTarget();
			}
			return (CPlayerVehicle)pl.getTarget().getTarget();
		}

		public static bool checkThief()
		{
			int frontPlayerID = wld.CWorldOutSideData.getInstance().PlayerData().getFrontPlayerID();
			if (PlayerParty.instance().playerForId((byte)frontPlayerID).jobManager()
				.nowJob() == 9)
			{
				return true;
			}
			return false;
		}

		public static void cancelKeyDoor()
		{
			int num = wld.CWorldOutSideData.getInstance().MapData().MapJumpIndex();
			if (0 <= num)
			{
				num--;
				map.CMapJumpParameter cMapJumpParameter = map.CMapParameterManager.Instance().MapJumpParameter(num);
				if (cMapJumpParameter != null)
				{
					FlagManager.singleton().set(0u, (uint)cMapJumpParameter.Kind());
					cMapJumpParameter.Kind_set(0);
				}
			}
		}

		internal static void setTouchIconVisibility(bool Visibility)
		{
			wld.WorldPart.getInstance().getWorldSystem().World2DMng()
				.setButtonShow(Visibility);
		}

		internal static bool checkActionTrigger()
		{
			if ((dv.CDeviceManager.getInstance().Pad().edge_trs(0) & 0x80) == 0)
			{
				return ds.g_TouchPanel.isTap();
			}
			return true;
		}

	}
}
