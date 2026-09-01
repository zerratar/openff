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
	public static partial class wld
	{
							public const MapMarkerType MAP_MARKER_INVALID = MapMarkerType.MAP_MARKER_INVALID;

							public const MapMarkerType MAP_MARKER_PLAYER = MapMarkerType.MAP_MARKER_PLAYER;

							public const MapMarkerType MAP_MARKER_VEHICLE = MapMarkerType.MAP_MARKER_VEHICLE;

							public const MapMarkerType MAP_MARKER_TOWN = MapMarkerType.MAP_MARKER_TOWN;

							public const MapMarkerType MAP_MARKER_DUNGEON = MapMarkerType.MAP_MARKER_DUNGEON;

							public const MapMarkerType MAP_MARKER_CHOCOBO = MapMarkerType.MAP_MARKER_CHOCOBO;

							public const MapMarkerType MAP_MARKER_FIELD = MapMarkerType.MAP_MARKER_FIELD;

							public const MapMarkerType MAP_MARKER_DOGA = MapMarkerType.MAP_MARKER_DOGA;

							public const MapMarkerType MAP_MARKER_TOWN2 = MapMarkerType.MAP_MARKER_TOWN2;

							public const MapMarkerType MAP_MARKER_SHIRINE = MapMarkerType.MAP_MARKER_SHIRINE;

							public const MapMarkerType MAP_MARKER_TOWN3 = MapMarkerType.MAP_MARKER_TOWN3;

							public const MapMarkerType MAP_MARKER_CASTLE = MapMarkerType.MAP_MARKER_CASTLE;

							public const MapMarkerType MAP_MARKER_TOWER = MapMarkerType.MAP_MARKER_TOWER;

							public const MapMarkerType MAP_MARKER_VEHICLE1 = MapMarkerType.MAP_MARKER_VEHICLE1;

							public const MapMarkerType MAP_MARKER_VEHICLE2 = MapMarkerType.MAP_MARKER_VEHICLE2;

							public const MapMarkerType MAP_MARKER_VEHICLE3 = MapMarkerType.MAP_MARKER_VEHICLE3;

							public const MapMarkerType MAP_MARKER_MAX = MapMarkerType.MAP_MARKER_MAX;

							public const int MS_STEP_CURTAIN = 0;

							public const int MS_STEP_CHANGE_CHR = 1;

							public const int MS_STEP_ROLLUP = 2;

							public const int MS_STEP_BUILD = 3;

							public static int BATTLE_FADEOUT_TIME = 5;

							public static int ENCOUNT_MOTSTART_FRAME = 10;

							public static int ENCOUNT_MOTION_INDEX = 2012;

							public static int ENCOUNT_LOOP_MOTION_INDEX = 2014;

							public static int g_encountWorkFrame = 0;

							public static byte g_encountState = 0;

							public static string LS_FILENAME = "locate_of_smith.dat";

							public static uint LS_FLAG_GROUP = 0u;

							public static uint LS_FLAG_INDEX = 981u;

							private static byte[] BGM_TABEL_INDEX = new byte[60]
							{
								0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
								10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
								20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
								30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
								40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
								50, 51, 52, 53, 54, 55, 56, 57, 58, 59
							};

							private static int BASIC_MSG_POS_X = 16;

							private static int BASIC_MSG_POS_Y = 246;

							public static TownInfo[] g_TownInfoTable = new TownInfo[26]
							{
								new TownInfo("t01", 0, 900),
								new TownInfo("t02", 0, 901),
								new TownInfo("t03", 0, 902),
								new TownInfo("t04", 0, 903),
								new TownInfo("t06", 0, 905),
								new TownInfo("t07", 0, 906),
								new TownInfo("t08", 0, 912),
								new TownInfo("t09", 0, 909),
								new TownInfo("t10", 0, 910),
								new TownInfo("t12", 0, 908),
								new TownInfo("t13", 0, 914),
								new TownInfo("t14", 0, 911),
								new TownInfo("t15", 1, 927),
								new TownInfo("t16", 1, 928),
								new TownInfo("t17", 2, 934),
								new TownInfo("t18", 2, 942),
								new TownInfo("t19", 2, 940),
								new TownInfo("t20", 2, 936),
								new TownInfo("t21", 2, 939),
								new TownInfo("t22", 2, 935),
								new TownInfo("t23", 2, 937),
								new TownInfo("t24", 2, 938),
								new TownInfo("t25", 2, 943),
								new TownInfo("t26", 2, 944),
								new TownInfo("t27", 2, 955),
								new TownInfo("t29", 2, 941)
							};

							private static VecFx32 wld_reuse_v0 = new VecFx32();

							private static VecFx32 wld_reuse_v1 = new VecFx32();

							private static VecFx32 wld_reuse_v2 = new VecFx32();

							private static VecFx32 wld_reuse_v3 = new VecFx32();

							private static VecFx32 wld_reuse_v4 = new VecFx32();

							private static VecFx32 wld_reuse_v5 = new VecFx32();

							private static VecFx32 wld_reuse_v6 = new VecFx32();

							public static int SCROLL_COUNT = 16;

							public static int SCROLL_CONSTANT = 192;

							public static int SCROLL_SPEED = SCROLL_CONSTANT / SCROLL_COUNT;

							public static cmr.CWorldCamera savedCamera = new cmr.CWorldCamera();

							public static int old_job = -1;

							public static bool old_lillput = false;

							public static bool old_frog = false;

							internal static void executeEncountMotion(CBaseSystem _sys)
							{
								pl.CBasePlayer cBasePlayer = _sys.PlayerMng().Player(0);
								if (cBasePlayer == null)
								{
									return;
								}
								switch (g_encountState)
								{
								case 0:
									if (++g_encountWorkFrame >= ENCOUNT_MOTSTART_FRAME)
									{
										cBasePlayer.startMotion(ENCOUNT_MOTION_INDEX, _Loop: true, 5u);
										cBasePlayer.setMotionLoop(loop: false);
										g_encountState = 1;
									}
									break;
								case 1:
									if (cBasePlayer.isEndOfMotion())
									{
										cBasePlayer.startMotion(ENCOUNT_LOOP_MOTION_INDEX, _Loop: true, 5u);
										g_encountState = 2;
									}
									break;
								case 2:
									break;
								}
							}

							internal static void stopNaviSE(CBaseSystem sys)
							{
								byte b = 0;
								while ((uint)b < 4u)
								{
									if (sys.PlayerMng().PlayerVehicle(b).getBoardPlayer() != null)
									{
										sys.PlayerMng().PlayerVehicle(b).stopNaviSE(0);
									}
									b++;
								}
							}

							internal static void setUpMapSoundDummy()
							{
								if (sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_WORLD)
								{
									MatrixSound.MtxSENDS_Unload();
									MatrixSound.MtxSoundBGM.getSingleton().stop(0, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT1);
									MatrixSound.MtxBGMNDS_Unload();
									MatrixSound.MtxBGMNDS_Unload();
								}
								MatrixSound.MtxBGMNDS_LoadEx(0, 0);
								MatrixSound.MtxBGMNDS_LoadEx(0, 0);
								MatrixSound.MtxSoundBGM.getSingleton().play(0, 192, 0, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT1);
								MatrixSound.MtxSENDS_Load(1);
							}

							public static MatrixSound.enMtxBGMSlot getSlot(int bgmNo)
							{
								switch (bgmNo)
								{
								case 7:
								case 8:
								case 9:
								case 10:
								case 20:
									return MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT3;
								default:
									return MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT1;
								}
							}

							internal static void releaseMessage(ref int msgID)
							{
								if (-1 != msgID)
								{
									dgs.msg.CMessageSys.getInstance().Main().releaseMessage(msgID);
									msgID = -1;
								}
							}

							public static ds.Vector2<float> transCoordWorldToAreaF(VecFx32 WorldPos, VecFx32 AreaOrg, VecFx32 AreaWH)
							{
								VecFx32 vecFx = new VecFx32(0, 0, 0);
								VEC_Subtract(WorldPos, AreaOrg, vecFx);
								ds.Vector2<float> vector = new ds.Vector2<float>(0f, 0f);
								vector.vx = FX_FX32_TO_F32(FX_Mul(4096, FX_Div(vecFx.x, AreaWH.x)));
								vector.vy = FX_FX32_TO_F32(FX_Mul(4096, FX_Div(vecFx.z, AreaWH.z)));
								return vector;
							}

							public static ds.Vector2<int> transCoordWorldToAreaFx32(VecFx32 WorldPos, VecFx32 AreaOrg, VecFx32 AreaWH)
							{
								VecFx32 vecFx = new VecFx32(0, 0, 0);
								VEC_Subtract(WorldPos, AreaOrg, vecFx);
								ds.Vector2<int> vector = new ds.Vector2<int>();
								vector.vx = FX_Mul(FX_F32_TO_FX32(1f), FX_Div(vecFx.x, AreaWH.x));
								vector.vy = FX_Mul(FX_F32_TO_FX32(1f), FX_Div(vecFx.z, AreaWH.z));
								return vector;
							}

							internal static int evaluteProgress()
							{
								int num = 0;
								for (int i = 850; 859 > i; i++)
								{
									if (1 == FlagManager.singleton().get(0u, (uint)i))
									{
										num++;
									}
								}
								return num;
							}

							internal static void releaseAllMapMarkerFlag()
							{
								for (int i = 900; 964 > i; i++)
								{
									FlagManager.singleton().set(1u, (uint)i);
								}
							}

							internal static void checkFrontPlayerCondition()
							{
								int frontPlayerID = CWorldOutSideData.getInstance().PlayerData().getFrontPlayerID();
								old_job = pl.PlayerParty.instance().playerForId((byte)frontPlayerID).jobManager()
									.nowJob() + 1;
								old_lillput = pl.PlayerParty.instance().playerForId((byte)frontPlayerID).condition()
									.isLilliput();
								old_frog = pl.PlayerParty.instance().playerForId((byte)frontPlayerID).condition()
									.isFrog();
							}

	}
}
