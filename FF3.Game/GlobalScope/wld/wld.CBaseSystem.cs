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
	public static partial class wld
	{
		public class CBaseSystem
		{
			public enum WORLD_MODE
			{
				WORLD_MODE_ERR = -1,
				WORLD_MODE_FIELD,
				WORLD_MODE_TOWN,
				WORLD_MODE_SHOP,
				WORLD_MODE_MENU,
				WORLD_MODE_TALK,
				WORLD_MODE_SITE,
				WORLD_MODE_SAVE,
				WORLD_MODE_INN,
				WORLD_MODE_MAX
			}

			public enum WORLD_STATE
			{
				WORLD_STATE_ERR = -1,
				WORLD_STATE_START,
				WORLD_STATE_MOVE,
				WORLD_STATE_END,
				WORLD_STATE_MAX
			}

			public class CAST_FILE
			{
				public Array m_GlobalAddr;

				public Array m_MapAddr;
			}

			public class WorldMode
			{
				public CBaseState[] m_CurrentState = new CBaseState[3];

				public CBaseWorldVram m_Vram;

				public WorldMode()
				{
					for (int i = 0; i < m_CurrentState.Length; i++)
					{
						m_CurrentState[i] = new CBaseState();
					}
				}

				public WorldMode(CBaseState arg0, CBaseState arg1, CBaseState arg2, CBaseWorldVram arg3)
					: this()
				{
					m_CurrentState[0] = arg0;
					m_CurrentState[1] = arg1;
					m_CurrentState[2] = arg2;
					m_Vram = arg3;
				}
			}

			public class CONTENT
			{
				public class _DATA
				{
					private byte[] S = new byte[4];

					public uint L
					{
						get
						{
							return (uint)(S[0] | (S[1] << 8) | (S[2] << 16) | (S[3] << 24));
						}
						set
						{
							S[0] = (byte)(value & 0xFF);
							S[1] = (byte)((value >> 8) & 0xFF);
							S[2] = (byte)((value >> 16) & 0xFF);
							S[3] = (byte)((value >> 24) & 0xFF);
						}
					}
				}

				public uint ID;

				public _DATA DATA = new _DATA();

				public CONTENT()
				{
				}

				public CONTENT(uint arg0, uint arg1)
				{
					ID = arg0;
					DATA.L = arg1;
				}

				public void copy(CONTENT src)
				{
					ID = src.ID;
					DATA.L = src.DATA.L;
				}
			}

			public enum NEXT_MODE
			{
				NEXT_ERROR = -1,
				NEXT_MAPJUMP,
				NEXT_BATTLE,
				NEXT_SHOP,
				NEXT_MENU,
				NEXT_TALK,
				NEXT_MOGNET,
				NEXT_TITLE,
				NEXT_SAVE,
				NEXT_SPL,
				NEXT_INN,
				NEXT_AREAMAP,
				NEXT_MAX
			}

			public const WORLD_MODE WORLD_MODE_ERR = WORLD_MODE.WORLD_MODE_ERR;

			public const WORLD_MODE WORLD_MODE_FIELD = WORLD_MODE.WORLD_MODE_FIELD;

			public const WORLD_MODE WORLD_MODE_TOWN = WORLD_MODE.WORLD_MODE_TOWN;

			public const WORLD_MODE WORLD_MODE_SHOP = WORLD_MODE.WORLD_MODE_SHOP;

			public const WORLD_MODE WORLD_MODE_MENU = WORLD_MODE.WORLD_MODE_MENU;

			public const WORLD_MODE WORLD_MODE_TALK = WORLD_MODE.WORLD_MODE_TALK;

			public const WORLD_MODE WORLD_MODE_SITE = WORLD_MODE.WORLD_MODE_SITE;

			public const WORLD_MODE WORLD_MODE_SAVE = WORLD_MODE.WORLD_MODE_SAVE;

			public const WORLD_MODE WORLD_MODE_INN = WORLD_MODE.WORLD_MODE_INN;

			public const WORLD_MODE WORLD_MODE_MAX = WORLD_MODE.WORLD_MODE_MAX;

			public const WORLD_STATE WORLD_STATE_ERR = WORLD_STATE.WORLD_STATE_ERR;

			public const WORLD_STATE WORLD_STATE_START = WORLD_STATE.WORLD_STATE_START;

			public const WORLD_STATE WORLD_STATE_MOVE = WORLD_STATE.WORLD_STATE_MOVE;

			public const WORLD_STATE WORLD_STATE_END = WORLD_STATE.WORLD_STATE_END;

			public const WORLD_STATE WORLD_STATE_MAX = WORLD_STATE.WORLD_STATE_MAX;

			protected const int LIMIT_OF_TEMPORARY = 4;

			public const NEXT_MODE NEXT_ERROR = NEXT_MODE.NEXT_ERROR;

			public const NEXT_MODE NEXT_MAPJUMP = NEXT_MODE.NEXT_MAPJUMP;

			public const NEXT_MODE NEXT_BATTLE = NEXT_MODE.NEXT_BATTLE;

			public const NEXT_MODE NEXT_SHOP = NEXT_MODE.NEXT_SHOP;

			public const NEXT_MODE NEXT_MENU = NEXT_MODE.NEXT_MENU;

			public const NEXT_MODE NEXT_TALK = NEXT_MODE.NEXT_TALK;

			public const NEXT_MODE NEXT_MOGNET = NEXT_MODE.NEXT_MOGNET;

			public const NEXT_MODE NEXT_TITLE = NEXT_MODE.NEXT_TITLE;

			public const NEXT_MODE NEXT_SAVE = NEXT_MODE.NEXT_SAVE;

			public const NEXT_MODE NEXT_SPL = NEXT_MODE.NEXT_SPL;

			public const NEXT_MODE NEXT_INN = NEXT_MODE.NEXT_INN;

			public const NEXT_MODE NEXT_AREAMAP = NEXT_MODE.NEXT_AREAMAP;

			public const NEXT_MODE NEXT_MAX = NEXT_MODE.NEXT_MAX;

			public static NEXT_MODE m_Next = NEXT_MODE.NEXT_ERROR;

			public static WORLD_MODE m_Mode = WORLD_MODE.WORLD_MODE_ERR;

			public static WORLD_MODE m_PreviousMode = WORLD_MODE.WORLD_MODE_ERR;

			public static bool m_ChangePlayerDraw = false;

			public static ds.Vector<CONTENT, ds.FastErasePolicy<CONTENT>> m_TemporaryContainer = new ds.Vector<CONTENT, ds.FastErasePolicy<CONTENT>>(4);

			public static bool areaChangeShutterFlag_ = true;

			public static bool customFadeSettingFlag_ = false;

			public static dgs.CFade.FADE_TYPE fadeColor_;

			public static short fadeFrame_;

			public static bool setUpMapSound_ = true;

			protected WORLD_STATE m_State;

			protected WORLD_STATE m_PreviousState;

			protected WorldMode[] m_CurrentMode = new WorldMode[8];

			protected CStateFieldStart m_StateFieldStart = new CStateFieldStart();

			protected CStateTownStart m_StateTownStart = new CStateTownStart();

			protected CStateShopStart m_StateShopStart = new CStateShopStart();

			protected CStateMenuStart m_StateMenuStart = new CStateMenuStart();

			protected CStateTalkStart m_StateTalkStart = new CStateTalkStart();

			protected CStateSiteStart m_StateSiteStart = new CStateSiteStart();

			protected CStateSaveStart m_StateSaveStart = new CStateSaveStart();

			protected CStateInnStart m_StateInnStart = new CStateInnStart();

			protected CStateWorldMove m_StateWorldMove = new CStateWorldMove();

			protected CStateShopMove m_StateShopMove = new CStateShopMove();

			protected CStateMenuMove m_StateMenuMove = new CStateMenuMove();

			protected CStateTalkMove m_StateTalkMove = new CStateTalkMove();

			protected CStateSiteMove m_StateSiteMove = new CStateSiteMove();

			protected CStateInnMove m_StateInnMove = new CStateInnMove();

			protected CStateFieldEnd m_StateFieldEnd = new CStateFieldEnd();

			protected CStateTownEnd m_StateTownEnd = new CStateTownEnd();

			protected CStateShopEnd m_StateShopEnd = new CStateShopEnd();

			protected CStateMenuEnd m_StateMenuEnd = new CStateMenuEnd();

			protected CStateTalkEnd m_StateTalkEnd = new CStateTalkEnd();

			protected CStateSiteEnd m_StateSiteEnd = new CStateSiteEnd();

			protected CStateSaveEnd m_StateSaveEnd = new CStateSaveEnd();

			protected CStateInnEnd m_StateInnEnd = new CStateInnEnd();

			protected CWorldFieldVram m_WorldFieldVram = new CWorldFieldVram();

			protected CWorldTownVram m_WorldTownVram = new CWorldTownVram();

			protected CWorldShopVram m_WorldShopVram = new CWorldShopVram();

			protected CWorldMenuVram m_WorldMenuVram = new CWorldMenuVram();

			protected CWorldTalkVram m_WorldTalkVram = new CWorldTalkVram();

			protected bool m_End;

			protected pl.CPlayerManager m_PlayerMng = new pl.CPlayerManager();

			protected cmr.CWorldCamera m_WldCamera = new cmr.CWorldCamera();

			protected map.CEnCountManager m_EnCountMng = new map.CEnCountManager();

			protected CWorld2DManager m_Wld2DMng = new CWorld2DManager();

			protected dgs.ScreenFlash m_ScreenFlash = new dgs.ScreenFlash();

			protected dgs.MSDINFO m_ScenarioMsdAddr;

			protected dgs.MSDINFO m_PermanentMsdAddr;

			protected CAST_FILE m_CastFile = new CAST_FILE();

			protected int npcEntryId_;

			protected int npcId_;

			protected bool isEscape_;

			protected bool isSite_;

			protected int m_LastLogic;

			protected int m_LastMessage;

			protected WorldBGControl wbc_ = new WorldBGControl();

			protected WorldOBJControl woc_ = new WorldOBJControl();

			protected map.SecretWayMng swMng_ = new map.SecretWayMng();

			public static void resetOverlay()
			{
				m_Next = NEXT_MODE.NEXT_ERROR;
				m_Mode = WORLD_MODE.WORLD_MODE_ERR;
				m_PreviousMode = WORLD_MODE.WORLD_MODE_ERR;
				m_ChangePlayerDraw = false;
				areaChangeShutterFlag_ = true;
				customFadeSettingFlag_ = false;
				fadeColor_ = dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK;
				fadeFrame_ = 0;
				setUpMapSound_ = true;
			}

			public CBaseSystem()
			{
				m_State = WORLD_STATE.WORLD_STATE_ERR;
				for (int i = 0; i < m_CurrentMode.Length; i++)
				{
					m_CurrentMode[i] = new WorldMode();
				}
				m_ScenarioMsdAddr = null;
				m_PermanentMsdAddr = null;
			}

			public void putContent(CONTENT C)
			{
				for (int num = m_TemporaryContainer.size() - 1; num >= 0; num--)
				{
					if (m_TemporaryContainer[num].ID == C.ID)
					{
						return;
					}
				}
				m_TemporaryContainer.push_back(C);
			}

			public bool getContent(uint ID, CONTENT C)
			{
				for (int num = m_TemporaryContainer.size() - 1; num >= 0; num--)
				{
					if (m_TemporaryContainer[num].ID == ID)
					{
						C.copy(m_TemporaryContainer[num]);
						m_TemporaryContainer.erase(num);
						return true;
					}
				}
				return false;
			}

			public void setupStage(string _StageName, WORLD_MODE mode)
			{
				MapJump mapJump = new MapJump();
				mapJump.setupStage(this, _StageName, mode);
				setAutoSave(b: true);
			}

			public bool canChangeLilliput()
			{
				strncpy(out var arg, sceneMng.getStage(), 3);
				if (strcmp(arg, "t06") != 0 && strcmp(arg, "d05") != 0 && (strcmp(arg, "d06") != 0 || '1' == sceneMng.getStage()[5]))
				{
					return strcmp(arg, "d14") != 0;
				}
				return false;
			}

			public int setupHero()
			{
				string arg = "";
				int frontPlayerID = CWorldOutSideData.getInstance().PlayerData().getFrontPlayerID();
				int num = pl.PlayerParty.instance().playerForId((byte)frontPlayerID).playerId();
				if (pl.PlayerParty.instance().playerForId((byte)frontPlayerID).isEnable())
				{
					sprintf(out arg, "j%d%02d", num + 1, pl.PlayerParty.instance().playerForId((byte)frontPlayerID).jobManager()
						.nowJob() + 1);
				}
				VecFx32 position = new VecFx32(0, 0, 0);
				VecFx32 rotation = new VecFx32(0, 0, 0);
				VecFx32 scale = new VecFx32(4096, 4096, 4096);
				VecFx32 vecFx = new VecFx32(4096, 1024, 4096);
				if (strncmp(sceneMng.getStage(), "d09", 3) == 0 || strncmp(sceneMng.getStage(), "d13", 3) == 0 || strncmp(sceneMng.getStage(), "d26", 3) == 0)
				{
					vecFx.y = 256;
				}
				int num2 = 0;
				num2 = m_PlayerMng.setUpWorldCharacter(position, rotation, scale, vecFx, arg, _AutoPilot: true, static_cast<bool>(true));
				m_PlayerMng.PlayerHuman(num2).setShadowType(0u);
				TexDivideLoader.getSingleton().tdlForceLoad();
				characterMng.setupOrgTex(m_PlayerMng.PlayerHuman(num2).getCharacterId());
				chr.CBaseCharacter.setLookIndex(num2);
				CWorldOutSideData.getInstance().PlayerData().setPlayCharacterIndex(num2);
				string arg2 = "n431";
				if (num != 0)
				{
					sprintf(out arg2, "n43%d", num + 4);
				}
				m_PlayerMng.PlayerHuman(num2).createFrogMdl(arg2);
				if (Mode() == WORLD_MODE.WORLD_MODE_FIELD)
				{
					m_PlayerMng.PlayerHuman(num2).setInPutMode(pl.INPUT_MODE.INPUT_MODE_FIELD);
					m_PlayerMng.PlayerHuman(num2).PlayerMoveType_set(pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_FIELD);
				}
				else if (Mode() == WORLD_MODE.WORLD_MODE_TOWN)
				{
					m_PlayerMng.PlayerHuman(num2).setInPutMode(pl.INPUT_MODE.INPUT_MODE_TOWN);
					m_PlayerMng.PlayerHuman(num2).PlayerMoveType_set(pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_TOWN);
				}
				if (pl.PlayerParty.instance().playerForId((byte)frontPlayerID).condition()
					.isFrog())
				{
					m_PlayerMng.PlayerHuman(num2).changeFrog(frontPlayerID, -1);
				}
				else if (pl.PlayerParty.instance().playerForId((byte)frontPlayerID).condition()
					.isLilliput() && canChangeLilliput())
				{
					m_PlayerMng.PlayerHuman(num2).changeLilliput(frontPlayerID, -1);
				}
				m_PlayerMng.PlayerHuman(num2).setMenuIcon(World2DMng().MenuStartButton());
				m_PlayerMng.PlayerHuman(num2).setCameraIcon(World2DMng().visibleMap() ? World2DMng().CameraButton() : null);
				m_PlayerMng.PlayerHuman(num2).setTalkIcon(World2DMng().TalkButton());
				m_PlayerMng.Player(num2).into();
				if (Mode() == WORLD_MODE.WORLD_MODE_FIELD)
				{
					m_PlayerMng.Player(num2).setGrv(_GrvFlag: false);
				}
				return num2;
			}

			public void setupComradeNPC()
			{
				npcEntryId_ = -1;
				if (!pl.PlayerParty.instance().npc().isEnable())
				{
					return;
				}
				string[] array = new string[8] { "n531", "n541", "n591", "n272", "n561", "n571", "n361", "n371" };
				sprintf(out var arg, "%s", array[pl.PlayerParty.instance().npc().npcId()]);
				VecFx32 vecFx = new VecFx32(8192, 8192, 16384);
				VecFx32 vecFx2 = new VecFx32(0, 0, 0);
				VecFx32 vecFx3 = new VecFx32(0, 0, 0);
				VecFx32 scl = new VecFx32(4096, 4096, 4096);
				VecFx32 shadowScl = new VecFx32(4096, 1024, 4096);
				int num = -1;
				num = m_PlayerMng.setupPlainCharacter(arg, scl, shadowScl);
				TexDivideLoader.getSingleton().tdlForceLoad();
				pl.CPlayerHuman cPlayerHuman = m_PlayerMng.PlayerHuman(num);
				characterMng.setupOrgTex(cPlayerHuman.getCharacterId());
				cPlayerHuman.HumanType_set(pl.PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_HUMAN);
				cPlayerHuman.PlayerMoveType_set(pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_DEFAULT_NPC);
				string arg2 = "w_light_man";
				if (pl.PlayerParty.instance().npc().npcId() == 0)
				{
					strcpy(out arg2, "w_field_fat");
				}
				else if (pl.PlayerParty.instance().npc().npcId() == 7)
				{
					strcpy(out arg2, "w_field_old");
				}
				characterMng.addMotion(cPlayerHuman.getCharacterId(), arg2);
				cPlayerHuman.createFrogMdl("n431");
				pl.PlayerParty.instance().npc().setBadStateNormal();
				if (3 != pl.PlayerParty.instance().npc().npcId())
				{
					if (pl.PlayerParty.instance().isLilliputAll() && canChangeLilliput())
					{
						m_PlayerMng.PlayerHuman(num).changeLilliputForNpc();
					}
					else if (pl.PlayerParty.instance().isFrogAll())
					{
						m_PlayerMng.PlayerHuman(num).changeFrogForNpc();
					}
				}
				npcEntryId_ = num;
				if (Mode() == WORLD_MODE.WORLD_MODE_FIELD || (Mode() == WORLD_MODE.WORLD_MODE_MENU && PreviousMode() == WORLD_MODE.WORLD_MODE_FIELD))
				{
					m_PlayerMng.PlayerHuman(num).setInPutMode(pl.INPUT_MODE.INPUT_MODE_FIELD);
					m_PlayerMng.PlayerHuman(num).PlayerMoveType_set(pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_FIELD);
				}
				else if (Mode() == WORLD_MODE.WORLD_MODE_TOWN || (Mode() == WORLD_MODE.WORLD_MODE_MENU && PreviousMode() == WORLD_MODE.WORLD_MODE_TOWN))
				{
					m_PlayerMng.PlayerHuman(num).setInPutMode(pl.INPUT_MODE.INPUT_MODE_TOWN);
					m_PlayerMng.PlayerHuman(num).PlayerMoveType_set(pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_TOWN);
				}
				m_PlayerMng.PlayerHuman(num).NPCAutoFollowType_set(pl.NPC_AUTO_FOLLOW_TYPE.NPC_AUTO_FOLLOW_TYPE_UNE);
				m_PlayerMng.PlayerHuman(num).NPCAiManager().AiKind_set(pl.CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW);
				m_PlayerMng.PlayerHuman(num).NPCAiManager().NPC()
					.setLookPlayer(PlayerMng().Player(0));
				pl.CPlayerHuman cPlayerHuman2 = m_PlayerMng.PlayerHuman(num);
				m_PlayerMng.PlayerHuman(0).setNpc(cPlayerHuman2);
				vecFx.copy(m_PlayerMng.Player(0).getPosition());
				vecFx2.copy(m_PlayerMng.Player(0).getRotation());
				VEC_Set(vecFx3, FX_SinIdx(vecFx2.y), 0, FX_CosIdx(vecFx2.y));
				if (!IS_ZERO_NORM(vecFx3))
				{
					VEC_Normalize(vecFx3, vecFx3);
				}
				vecFx3.x *= -1;
				vecFx3.y *= -1;
				vecFx3.z *= -1;
				vecFx3.x = FX_Mul(vecFx3.x, 4096);
				vecFx3.z = FX_Mul(vecFx3.z, 4096);
				VEC_Add(vecFx, vecFx3, vecFx);
				m_PlayerMng.PlayerHuman(num).setPosition(vecFx);
				m_PlayerMng.PlayerHuman(num).setRotation(vecFx2);
				m_PlayerMng.PlayerHuman(num).setTargetDirectionFromRotation();
				m_PlayerMng.PlayerHuman(num).setTransparencyRate(0);
				m_PlayerMng.PlayerHuman(num).setShadowAlpha(0);
				m_PlayerMng.Player(num).into();
				m_PlayerMng.Player(num).getColFlag_not_and(4096);
				m_PlayerMng.Player(num).getColFlag_not_and(2);
				m_PlayerMng.Player(num).getColFlag_not_and(4);
				if (Mode() == WORLD_MODE.WORLD_MODE_FIELD)
				{
					m_PlayerMng.Player(num).setGrv(_GrvFlag: false);
				}
			}

			public void setupVehicle()
			{
				int fieldNo = sceneMng.getFieldNo();
				if (-1 == fieldNo)
				{
					return;
				}
				pl.PLAYER_VEHICLE_TYPE[] array = new pl.PLAYER_VEHICLE_TYPE[8]
				{
					pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CHOKOBO,
					pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_SHIDO_H,
					pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CANOE,
					pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP,
					pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM,
					pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_NORCHI,
					pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_NORCHI_CTM,
					pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_INVINSIBLE
				};
				pl.PLAYER_MOVE_TYPE[] array2 = new pl.PLAYER_MOVE_TYPE[8]
				{
					pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_CHOKOBO,
					pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_SHIDO_H,
					pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_CANOE,
					pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_ENTERP,
					pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_ENTERP_CTM,
					pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_NORCHI,
					pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_NORCHI_CTM,
					pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_INVINSIBLE
				};
				VecFx32[] array3 = new VecFx32[8]
				{
					new VecFx32(0, 0, 0),
					new VecFx32(20480, VEHICLE_HEIGHT_GROUND, 20480),
					new VecFx32(-20480, -409600, -20480),
					new VecFx32(0, VEHICLE_HEIGHT_ONSEA, -20480),
					new VecFx32(20480, VEHICLE_HEIGHT_ONSEA, -20480),
					new VecFx32(-20480, VEHICLE_HEIGHT_GROUND, 20480),
					new VecFx32(0, VEHICLE_HEIGHT_GROUND, 20480),
					new VecFx32(-20480, VEHICLE_HEIGHT_AIR, -20480)
				};
				bool[] array4 = new bool[8];
				bool[] array5 = array4;
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[1]) == 1)
				{
					array5[1] = true;
				}
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[2]) == 1)
				{
					array5[1] = false;
				}
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[0]) == 1)
				{
					array5[2] = true;
				}
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[3]) == 1)
				{
					array5[3] = true;
				}
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[4]) == 1)
				{
					array5[3] = false;
				}
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[5]) == 1)
				{
					array5[3] = false;
				}
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[6]) == 1)
				{
					array5[3] = false;
				}
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[7]) == 1)
				{
					array5[3] = false;
				}
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[4]) == 1)
				{
					array5[4] = true;
				}
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[5]) == 1)
				{
					array5[4] = true;
				}
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[6]) == 1)
				{
					array5[4] = true;
				}
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[7]) == 1)
				{
					array5[4] = false;
				}
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[8]) == 1)
				{
					array5[5] = true;
				}
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[9]) == 1)
				{
					array5[5] = false;
				}
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[9]) == 1)
				{
					array5[6] = true;
				}
				if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[10]) == 1)
				{
					array5[7] = true;
				}
				if (CWorldOutSideData.getInstance().MapData().getRideOnChokobo())
				{
					array5[0] = true;
				}
				VecFx32 vecFx = new VecFx32();
				VecFx32 vecFx2 = new VecFx32();
				VecFx32 vecFx3 = new VecFx32();
				VecFx32 vecFx4 = new VecFx32();
				pl.PLAYER_VEHICLE_TYPE preRidingOnVehicleNo = CWorldOutSideData.getInstance().VehicleData().getPreRidingOnVehicleNo();
				for (int i = 0; i < 8; i++)
				{
					if (!array5[i])
					{
						continue;
					}
					SHoldVehicleData holdData = CWorldOutSideData.getInstance().VehicleData().getHoldData(i);
					if (i == 0 || i == 2)
					{
						holdData.m_FieldNo = (sbyte)fieldNo;
					}
					if (fieldNo != holdData.m_FieldNo && preRidingOnVehicleNo != static_cast<pl.PLAYER_VEHICLE_TYPE>(i))
					{
						continue;
					}
					strcpy(out var arg, pl.PlayerVehicleFileName[i]);
					VEC_Set(vecFx, 0, 0, 0);
					VEC_Set(vecFx2, 0, 0, 0);
					VEC_Set(vecFx3, 4096, 4096, 4096);
					VEC_Set(vecFx4, 4096, 4096, 4096);
					int num = PlayerMng().setUpWorldCharacter(vecFx, vecFx2, vecFx3, vecFx4, arg, _AutoPilot: true, _Operater: false);
					TexDivideLoader.getSingleton().tdlForceLoad();
					characterMng.setupOrgTex(PlayerMng().Player(num).getCharacterId());
					PlayerMng().Player(num).into();
					num -= 24;
					PlayerMng().PlayerVehicle(num).VehicleType_set(array[i]);
					PlayerMng().PlayerVehicle(num).PlayerMoveType_set(array2[i]);
					if (array[i] == pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP || array[i] == pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM)
					{
						chr.CBaseCharacter cBaseCharacter = PlayerMng().PlayerVehicle(num);
						cBaseCharacter.getCckRadius_set(73728);
						PlayerMng().PlayerVehicle(num).PlayerMoveType_set(pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_ENTERP);
					}
					if (array[i] == pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_INVINSIBLE)
					{
						PlayerMng().PlayerVehicle(num).getCckRadius_set(28672);
					}
					if (array[i] == pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_NORCHI_CTM)
					{
						PlayerMng().PlayerVehicle(num).PlayerMoveType_set(pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_NORCHI);
					}
					PlayerMng().PlayerVehicle(num).setCanBoard(holdData.m_CanBoard);
					if (!PlayerMng().PlayerVehicle(num).canBoard())
					{
						PlayerMng().PlayerVehicle(num).getCckRadius_set(0);
						PlayerMng().PlayerVehicle(num).getColFlag_not_and(2);
					}
					if (sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_DEBUG_MENU)
					{
						if (array[i] == pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP || array[i] == pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM)
						{
							vecFx.copy(holdData.m_Position);
							vecFx2.copy(holdData.m_Rotation);
						}
						else
						{
							vecFx.copy(PlayerMng().Player(0).getPosition());
							VEC_Add(vecFx, array3[i], vecFx);
						}
					}
					else
					{
						vecFx.copy(holdData.m_Position);
						vecFx2.copy(holdData.m_Rotation);
					}
					PlayerMng().PlayerVehicle(num).setPosition(vecFx);
					PlayerMng().PlayerVehicle(num).getPosition().y = array3[i].y;
					PlayerMng().PlayerVehicle(num).setRotation(vecFx2);
					PlayerMng().PlayerVehicle(num).setTargetDirectionFromRotation();
				}
			}

			public void setupCamera()
			{
				bool mCLCollision = false;
				int near = 40960;
				int far = 2048000;
				cmr.CWorldCamera.MODE mode = cmr.CWorldCamera.MODE.MODE_AUTOFOLLOW_DEFAULT;
				VecFx32 vecFx = new VecFx32(0, 450560, -450560);
				VecFx32 vecFx2 = new VecFx32(0, 40960, 0);
				bool zoomEnable = true;
				cmr.CWorldCamera.TYPE type = cmr.CWorldCamera.TYPE.TYPE_DUNGEON;
				int zoomMax = -245760;
				int zoomMin = 0;
				int zoomSpd = 65536;
				WorldCamera().initialize();
				if (map.CMapParameterManager.Instance().isLoaded())
				{
					mCLCollision = map.CMapParameterManager.Instance().MapCameraParameter(0).Collision() != 0;
					near = 4096 * map.CMapParameterManager.Instance().MapCameraParameter(0).ClipNear();
					far = 4096 * map.CMapParameterManager.Instance().MapCameraParameter(0).ClipFar();
					mode = (cmr.CWorldCamera.MODE)map.CMapParameterManager.Instance().MapCameraParameter(0).Mode();
					VEC_Set(vecFx, 4096 * map.CMapParameterManager.Instance().MapCameraParameter(0).PositionOffset(0), 4096 * map.CMapParameterManager.Instance().MapCameraParameter(0).PositionOffset(1), 4096 * map.CMapParameterManager.Instance().MapCameraParameter(0).PositionOffset(2));
					VEC_Set(vecFx2, 4096 * map.CMapParameterManager.Instance().MapCameraParameter(0).TargetOffset(0), 4096 * map.CMapParameterManager.Instance().MapCameraParameter(0).TargetOffset(1), 4096 * map.CMapParameterManager.Instance().MapCameraParameter(0).TargetOffset(2));
					zoomEnable = ((map.CMapParameterManager.Instance().MapCameraParameter(0).ZoomOnOff() != 0) ? true : false);
					type = (cmr.CWorldCamera.TYPE)map.CMapParameterManager.Instance().MapCameraParameter(0).ZoomType();
					zoomMax = 4096 * map.CMapParameterManager.Instance().MapCameraParameter(0).ZoomMax();
					zoomMin = 4096 * map.CMapParameterManager.Instance().MapCameraParameter(0).ZoomMin();
					zoomSpd = 4096 * map.CMapParameterManager.Instance().MapCameraParameter(0).ZoomSpeed();
				}
				vecFx.z *= -1;
				if (Mode() == WORLD_MODE.WORLD_MODE_FIELD)
				{
					zoomEnable = false;
				}
				WorldCamera().setFOV(1060, 3956);
				WorldCamera().setMCLCollision(mCLCollision);
				WorldCamera().setClip(near, far);
				WorldCamera().setMode(mode);
				WorldCamera().setPosOffset(vecFx);
				WorldCamera().setTrgOffset(vecFx2);
				WorldCamera().composit.setZoomEnable(zoomEnable);
				WorldCamera().setType(type);
				WorldCamera().composit.setZoomMax(zoomMax);
				WorldCamera().composit.setZoomMin(zoomMin);
				WorldCamera().composit.setZoom(WorldCamera().composit.ZoomMin());
				WorldCamera().composit.setZoomSpd(zoomSpd);
				WorldCamera().composit.ZoomState_set(cmr.CCameraZoom.ZOOM_STATE.ZOOM_WAIT);
				WorldCamera().setPos(PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getPosition());
				WorldCamera().setTrg(PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getPosition());
			}

			public void setupMapJumpPosition()
			{
				MapJump mapJump = new MapJump();
				mapJump.setupMapJumpPosition(this);
			}

			public void setupBackUpPosition()
			{
				MapJump mapJump = new MapJump();
				mapJump.setupBackupPosition(this);
			}

			public void MapJumpPosition()
			{
				MapJump mapJump = new MapJump();
				mapJump.mapJumpPosition(this);
			}

			public void BackUpPosition()
			{
				MapJump mapJump = new MapJump();
				mapJump.backupPosition(this);
			}

			public void BackUpVehiclePosition()
			{
				MapJump mapJump = new MapJump();
				mapJump.backupVehiclePosition(this);
			}

			public void RidePlayerOnVehicle()
			{
				pl.PLAYER_VEHICLE_TYPE pLAYER_VEHICLE_TYPE = CWorldOutSideData.getInstance().VehicleData().getPreRidingOnVehicleNo();
				string arg = "";
				sprintf(out arg, "%s", CWorldOutSideData.getInstance().MapData().getBeforeTownMapName());
				if (arg.Length >= 3 && arg[0] == 'd' && arg[1] == '1' && arg[2] == '9' && GAMEPART.GAMEPART_WORLD == sys.GGlobal.getPreviousPart() && WORLD_MODE.WORLD_MODE_TOWN == PreviousMode())
				{
					pLAYER_VEHICLE_TYPE = pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CANOE;
				}
				if (pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ERR == pLAYER_VEHICLE_TYPE)
				{
					if (4 == sceneMng.getFieldNo())
					{
						pLAYER_VEHICLE_TYPE = pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_NORCHI_CTM;
					}
					else
					{
						if (!CWorldOutSideData.getInstance().MapData().getRideOnChokobo())
						{
							return;
						}
						pLAYER_VEHICLE_TYPE = pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CHOKOBO;
					}
				}
				pl.CPlayerVehicle cPlayerVehicle = null;
				byte b = 0;
				b = 0;
				while ((uint)b < 4u)
				{
					cPlayerVehicle = m_PlayerMng.PlayerVehicle(b);
					pl.PLAYER_VEHICLE_TYPE vehicleType = cPlayerVehicle.getVehicleType();
					if (vehicleType != pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ERR && cPlayerVehicle.getVehicleType() == pLAYER_VEHICLE_TYPE)
					{
						break;
					}
					b++;
				}
				if ((uint)b >= 4u)
				{
					return;
				}
				pl.CPlayerCharacter cPlayerCharacter = static_cast<pl.CPlayerCharacter>(m_PlayerMng.Player(0));
				cPlayerCharacter.setMCLCol(b: false);
				cPlayerCharacter.getColFlag_not_and(2);
				cPlayerCharacter.setTransparency(0);
				cPlayerCharacter.setShadowAlpha(0);
				int num = npcEntryId();
				pl.CPlayerCharacter cPlayerCharacter2 = null;
				if (-1 != num)
				{
					cPlayerCharacter2 = static_cast<pl.CPlayerCharacter>(m_PlayerMng.PlayerHuman(num));
					if (cPlayerCharacter2 != null)
					{
						cPlayerCharacter2.setMCLCol(b: false);
						cPlayerCharacter2.getColFlag_not_and(2);
						cPlayerCharacter2.setTransparency(0);
						cPlayerCharacter2.setShadowAlpha(0);
						cPlayerCharacter2.setAutoPilot(_AutoPilot: true);
						cPlayerCharacter2.InputPermission_set(arg0: false);
					}
				}
				int fieldNo = sceneMng.getFieldNo();
				int preFieldNo = sceneMng.getPreFieldNo();
				bool flag = (fieldNo == 3 && preFieldNo == 1) || (fieldNo == 1 && preFieldNo == 3);
				VecFx32 vecFx = new VecFx32(cPlayerCharacter.getPosition());
				if (pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_INVINSIBLE == cPlayerVehicle.getVehicleType() && !flag)
				{
					cPlayerCharacter.getPosition().x = cPlayerVehicle.getPosition().x;
					cPlayerCharacter.getPosition().z = cPlayerVehicle.getPosition().z;
				}
				else
				{
					cPlayerVehicle.getPosition().x = vecFx.x;
					cPlayerVehicle.getPosition().z = vecFx.z;
				}
				cPlayerVehicle.setAutoPilot(_AutoPilot: false);
				cPlayerVehicle.setOperater(_Operater: true);
				cPlayerVehicle.setBoardSetting(cPlayerCharacter);
				switch ((int)sceneMng.getFieldNo())
				{
				case 1:
				case 2:
				case 3:
					if (pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM == cPlayerVehicle.getVehicleType())
					{
						if (CWorldOutSideData.getInstance().VehicleData().getEnterpOnAir())
						{
							cPlayerVehicle.setConditionOfAir();
						}
					}
					else if (pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP != cPlayerVehicle.getVehicleType())
					{
						cPlayerVehicle.setConditionOfAir();
					}
					break;
				case 4:
					cPlayerVehicle.setConditionOfDeepSea();
					break;
				}
			}

			public void registerWorldMode()
			{
				WorldMode[] array = new WorldMode[8]
				{
					new WorldMode(m_StateFieldStart, m_StateWorldMove, m_StateFieldEnd, m_WorldFieldVram),
					new WorldMode(m_StateTownStart, m_StateWorldMove, m_StateTownEnd, m_WorldTownVram),
					new WorldMode(m_StateShopStart, m_StateShopMove, m_StateShopEnd, m_WorldShopVram),
					new WorldMode(m_StateMenuStart, m_StateMenuMove, m_StateMenuEnd, m_WorldTownVram),
					new WorldMode(m_StateTalkStart, m_StateTalkMove, m_StateTalkEnd, m_WorldTalkVram),
					new WorldMode(m_StateSiteStart, m_StateSiteMove, m_StateSiteEnd, m_WorldFieldVram),
					new WorldMode(m_StateSaveStart, m_StateMenuMove, m_StateSaveEnd, m_WorldTownVram),
					new WorldMode(m_StateInnStart, m_StateInnMove, m_StateInnEnd, m_WorldTownVram)
				};
				m_CurrentMode[0] = array[0];
				m_CurrentMode[1] = array[1];
				m_CurrentMode[2] = array[2];
				m_CurrentMode[3] = array[3];
				m_CurrentMode[4] = array[4];
				m_CurrentMode[5] = array[5];
				m_CurrentMode[6] = array[6];
				m_CurrentMode[7] = array[7];
			}

			public CBaseWorldVram PreVram()
			{
				if (m_PreviousMode <= WORLD_MODE.WORLD_MODE_ERR || WORLD_MODE.WORLD_MODE_MAX <= m_PreviousMode)
				{
					return m_CurrentMode[0].m_Vram;
				}
				return m_CurrentMode[(int)m_PreviousMode].m_Vram;
			}

			public CBaseWorldVram CrtVram()
			{
				if (m_Mode <= WORLD_MODE.WORLD_MODE_ERR || WORLD_MODE.WORLD_MODE_MAX <= m_Mode)
				{
					return m_CurrentMode[0].m_Vram;
				}
				return m_CurrentMode[(int)m_Mode].m_Vram;
			}

			public void setup()
			{
				setUpMapParameter();
				setUpPcParameter();
				setUpNpcParameter();
				dgs.msg.CMessageSys.getInstance().Main().assignBG(3, 0, 0, 32, 24);
				G2S_SetBG0Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x6000, GXBGCharBase.GX_BG_CHARBASE_0x00000, 0);
				dgs.msg.CMessageSys.getInstance().Sub().assignBG(0, 0, 0, 32, 24);
				setUpEventData();
				setUpEffectData();
				m_ScreenFlash.endFlash();
				isEscape_ = false;
				wbc_.wbcInitialize();
				woc_.wocInitialize();
			}

			public void cleanup()
			{
				cleanUpMapParameter();
				cleanUpPcParameter();
				cleanUpNpcParameter();
				cleanUpEventData();
				cleanUpEffectData();
				cleanUpMapSound();
				cleanUpMessageData();
				m_ScreenFlash.endFlash();
				woc_.wocTerminate();
				swMng_.terminate();
				menu.MenuManager.getSingleton().Set2d3dMode(3);
				menu.MenuManager.getSingleton().ReleaseItemDataText();
				setMenu(b: false);
			}

			public bool canEscape()
			{
				return PreviousMode() == WORLD_MODE.WORLD_MODE_TOWN;
			}

			public void doEscape()
			{
				if (canEscape())
				{
					isEscape_ = true;
					CWorldOutSideData.getInstance().SoundData().setSoundFlag(CWorldOutSideData.getInstance().SoundData().getSoundFlag() & -3);
				}
			}

			public bool isEscape()
			{
				return isEscape_;
			}

			public bool executeEscapeEffect()
			{
				return false;
			}

			public void setUpMapParameter()
			{
				string arg;
				if (Mode() == WORLD_MODE.WORLD_MODE_FIELD)
				{
					strncpy(out arg, sceneMng.getStage(), 3);
					sprintf(out arg, "%s%s", arg, ".pak");
				}
				else
				{
					sprintf(out arg, "%s%s", sceneMng.getStage(), ".pak");
				}
				map.CMapParameterManager.Instance().Initialize();
				map.CMapParameterManager.Instance().Load(arg);
			}

			public void setUpPcParameter()
			{
				sprintf(out var arg, "%s%s", "player_world_move_parameter", ".pak");
				pl.CPlayerWorldParameterManager.Instance().Initialize();
				pl.CPlayerWorldParameterManager.Instance().Load(arg);
			}

			public void setUpNpcParameter()
			{
				sprintf(out var arg, "%s%s", "npc_world_move_parameter", ".pak");
				pl.CNPCWorldParameterManager.Instance().Initialize();
				pl.CNPCWorldParameterManager.Instance().Load(arg);
			}

			public void setUpEventData()
			{
				CastFile().m_GlobalAddr = null;
				CastFile().m_MapAddr = null;
				string text = CWorldOutSideData.getInstance().MapData().getNowMapName();
				if ('f' == text[0] && '2' == text[2] && evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_CHAPTER_FLAG[3]) == 1)
				{
					char[] array = text.ToCharArray();
					array[2] = '3';
					text = new string(array);
				}
				string arg = "";
				switch (text[0])
				{
				case 'd':
					sprintf(out arg, "/MAP/DUNGEON");
					break;
				case 't':
					sprintf(out arg, "/MAP/TOWN");
					break;
				case 's':
					sprintf(out arg, "/MAP/SHOP");
					break;
				case 'w':
					sprintf(out arg, "/MAP/BATTLE");
					break;
				case 'f':
					sprintf(out arg, "/MAP/FIELD/F0%c", text[2]);
					break;
				}
				evt.CHichParameterManager.getInstance().initialize();
				uint num = 0u;
				Array array2 = null;
				sprintf(out var arg2, "%s/HICH/%s.hich", arg, text);
				num = ds.g_File.getSize(arg2);
				if (num != 0)
				{
					array2 = ds.CHeap.alloc_app(num);
					if (array2 != null)
					{
						ds.g_File.load(array2, arg2);
						evt.CHichParameterManager.getInstance().setUp(0u, array2);
						ds.CHeap.free_app(array2);
					}
				}
				uint num2 = 0u;
				string text2 = ".script";
				sprintf(out var arg3, "/%s%s", "global", text2);
				num2 = ds.g_File.getSize(arg3);
				if (num2 != 0)
				{
					CastFile().m_GlobalAddr = ds.CHeap.alloc_app(num2);
					if (CastFile().m_GlobalAddr != null)
					{
						ds.g_File.load(CastFile().m_GlobalAddr, arg3);
					}
				}
				sprintf(out arg3, "%s/SCRIPT/%s%s", arg, text, text2);
				num2 = ds.g_File.getSize(arg3);
				if (num2 != 0)
				{
					CastFile().m_MapAddr = ds.CHeap.alloc_app(num2);
					if (CastFile().m_MapAddr != null)
					{
						ds.g_File.load(CastFile().m_MapAddr, arg3);
					}
				}
				evt.CEventManager.getInstance().into(CastFile().m_GlobalAddr, CastFile().m_MapAddr);
				cleanUpMessageData();
				m_ScenarioMsdAddr = null;
				uint num3 = 0u;
				string arg4 = "";
				string text3 = "";
				string text4 = ".msd";
				getCompanyDirectory(text3);
				Array array3 = null;
				sprintf(out arg4, "/%s%s%s", text3, text, text4);
				num3 = ds.g_File.getSize(arg4);
				if (num3 != 0)
				{
					array3 = ds.CHeap.alloc_app(num3);
					if (array3 != null)
					{
						OS_GetTick();
						ds.g_File.load(array3, arg4);
					}
					m_ScenarioMsdAddr = (dgs.MSDINFO)array3;
				}
				dgs.msg.CMessageSys.getInstance().Main().setUpMSD(m_ScenarioMsdAddr, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON);
				dgs.msg.CMessageSys.getInstance().Sub().setUpMSD(m_ScenarioMsdAddr, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON);
				changeCompanyDirectory();
				m_PermanentMsdAddr = null;
				Array array4 = null;
				string filename = "eureka_permanent.msd";
				uint size = ds.g_File.getSize(filename);
				if (size != 0)
				{
					array4 = ds.CHeap.alloc_app(size);
					if (array4 != null)
					{
						ds.g_File.load(array4, filename);
					}
					m_PermanentMsdAddr = (dgs.MSDINFO)array4;
				}
				dgs.msg.CMessageSys.getInstance().Main().setUpMSD(m_PermanentMsdAddr, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_PERMANENT);
				dgs.msg.CMessageSys.getInstance().Sub().setUpMSD(m_PermanentMsdAddr, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_PERMANENT);
				changeGlobalDirectory();
				if (CastFile().m_MapAddr != null)
				{
					evt.CEventManager.getInstance().startAllMapLogic();
				}
			}

			public void setUpEffectData()
			{
				eff.CEffectMng.instance().loadEfi("/EFFECT/effect.efi");
				eff.CEffectMng.instance().loadEfp("/EFFECT/w_common.efp");
				TexDivideLoader.getSingleton().tdlForceLoad();
				int num = 1;
				for (int i = 2; i < 8 && evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_CHAPTER_FLAG[i - 2]) != 0; i++)
				{
					num = i;
				}
				strncpy(out var arg, sceneMng.getStage(), 3);
				sprintf(out var arg2, "/EFFECT/event%02d_%s.efp", num, arg);
				if (ds.g_File.getSize(arg2) == 0)
				{
					num = 1;
					sprintf(out arg2, "/EFFECT/event%02d_%s.efp", num, arg);
				}
				eff.CEffectMng.instance().loadEfp(arg2);
				TexDivideLoader.getSingleton().tdlForceLoad();
				if (!map.CMapParameterManager.Instance().isLoaded())
				{
					return;
				}
				for (int j = 0; j < map.MAP_LANDFORM_PARAM_MAX; j++)
				{
					int num2 = map.CMapParameterManager.Instance().MapLandFormParameter(0).LandAttr(j);
					if (num2 != 0)
					{
						sprintf(out arg2, "/EFFECT/w_landform_%d.efp", num2);
						if (ds.g_File.getSize(arg2) != 0)
						{
							eff.CEffectMng.instance().loadEfp(arg2);
							TexDivideLoader.getSingleton().tdlForceLoad();
						}
					}
				}
			}

			public void setUpMapSound()
			{
				if (setUpMapSound_)
				{
					MapSound mapSound = new MapSound();
					mapSound.setup(this);
				}
				else
				{
					setUpMapSound_ = true;
				}
			}

			public void setUpMapSecretWay()
			{
				swMng_.initialize(map.CMapParameterManager.Instance().MapSecretWayParameter());
			}

			public void cleanUpMapParameter()
			{
				map.CMapParameterManager.Instance().Free();
			}

			public void cleanUpPcParameter()
			{
				pl.CPlayerWorldParameterManager.Instance().Free();
			}

			public void cleanUpNpcParameter()
			{
				pl.CNPCWorldParameterManager.Instance().Free();
			}

			public void cleanUpEventData()
			{
				if (CastFile().m_GlobalAddr != null)
				{
					evt.CEventManager.getInstance().cleanUpGlobalData();
					ds.CHeap.free_app(CastFile().m_GlobalAddr);
					CastFile().m_GlobalAddr = null;
				}
				if (CastFile().m_MapAddr != null)
				{
					evt.CEventManager.getInstance().cleanUpScriptData();
					ds.CHeap.free_app(CastFile().m_MapAddr);
					CastFile().m_MapAddr = null;
				}
			}

			public void cleanUpMessageData()
			{
				if (m_ScenarioMsdAddr != null)
				{
					ds.CHeap.free_app(m_ScenarioMsdAddr);
					m_ScenarioMsdAddr = null;
				}
				if (m_PermanentMsdAddr != null)
				{
					ds.CHeap.free_app(m_PermanentMsdAddr);
					m_PermanentMsdAddr = null;
				}
			}

			public void cleanUpEffectData()
			{
			}

			public void cleanUpMapSound()
			{
				if (setUpMapSound_)
				{
					MapSound mapSound = new MapSound();
					mapSound.cleanup(this);
				}
			}

			public void setMode(WORLD_MODE _Mode)
			{
				setPreviousMode(m_Mode);
				m_Mode = _Mode;
			}

			public WORLD_MODE Mode()
			{
				return m_Mode;
			}

			public void setMapJump(bool b)
			{
				OS_Printf("setMapJump %d\n", b);
				if (b)
				{
					m_Next = NEXT_MODE.NEXT_MAPJUMP;
				}
			}

			public bool IsMapJump()
			{
				if (m_Next == NEXT_MODE.NEXT_MAPJUMP)
				{
					return true;
				}
				return false;
			}

			public static void setBattle(bool b)
			{
				OS_Printf("setBattle %d\n", b);
				if (b)
				{
					m_Next = NEXT_MODE.NEXT_BATTLE;
				}
			}

			public bool IsBattle()
			{
				if (NEXT_MODE.NEXT_BATTLE == m_Next)
				{
					return true;
				}
				return false;
			}

			public void setShop(bool b)
			{
				OS_Printf("setShop %d\n", b);
				if (b)
				{
					m_Next = NEXT_MODE.NEXT_SHOP;
				}
			}

			public bool IsShop()
			{
				if (NEXT_MODE.NEXT_SHOP == m_Next)
				{
					return true;
				}
				return false;
			}

			public void setMenu(bool b)
			{
				OS_Printf("setMenu %d\n", b);
				if (b)
				{
					m_Next = NEXT_MODE.NEXT_MENU;
				}
			}

			public bool IsMenu()
			{
				if (NEXT_MODE.NEXT_MENU == m_Next)
				{
					return true;
				}
				return false;
			}

			public void setTalk(bool b)
			{
				OS_Printf("setTalk %d\n", b);
				if (b)
				{
					m_Next = NEXT_MODE.NEXT_TALK;
				}
			}

			public bool IsTalk()
			{
				if (NEXT_MODE.NEXT_TALK == m_Next)
				{
					return true;
				}
				return false;
			}

			public void setMogNet(bool b)
			{
				OS_Printf("setMogNet %d\n", b);
				if (b)
				{
					m_Next = NEXT_MODE.NEXT_MOGNET;
				}
			}

			public bool IsMogNet()
			{
				if (NEXT_MODE.NEXT_MOGNET == m_Next)
				{
					return true;
				}
				return false;
			}

			public void setSave(bool b)
			{
				if (b)
				{
					m_Next = NEXT_MODE.NEXT_SAVE;
				}
			}

			public bool IsSave()
			{
				if (NEXT_MODE.NEXT_SAVE == m_Next)
				{
					return true;
				}
				return false;
			}

			public void setSpecial(bool b)
			{
				if (b)
				{
					m_Next = NEXT_MODE.NEXT_SPL;
				}
			}

			public bool IsSpecial()
			{
				if (NEXT_MODE.NEXT_SPL == m_Next)
				{
					return true;
				}
				return false;
			}

			public void setInn(bool b)
			{
				if (b)
				{
					m_Next = NEXT_MODE.NEXT_INN;
				}
			}

			public bool IsInn()
			{
				if (NEXT_MODE.NEXT_INN == m_Next)
				{
					return true;
				}
				return false;
			}

			public void setAreaMap(bool b)
			{
				if (b)
				{
					m_Next = NEXT_MODE.NEXT_AREAMAP;
				}
			}

			public bool IsAreaMap()
			{
				if (NEXT_MODE.NEXT_AREAMAP == m_Next)
				{
					return true;
				}
				return false;
			}

			public static void setTitle(bool b)
			{
				OS_Printf("setTitle %d\n", b);
				if (b)
				{
					m_Next = NEXT_MODE.NEXT_TITLE;
				}
			}

			public bool IsTitle()
			{
				if (NEXT_MODE.NEXT_TITLE == m_Next)
				{
					return true;
				}
				return false;
			}

			public void setState(WORLD_STATE _State)
			{
				setPreviousState(m_State);
				m_State = _State;
			}

			public WORLD_STATE State()
			{
				return m_State;
			}

			public bool getAreaChangeShutterFlag()
			{
				return areaChangeShutterFlag_;
			}

			public void setAreaChangeShutterFlag(bool flag)
			{
				areaChangeShutterFlag_ = flag;
			}

			public void customFadeSetting(short frame, dgs.CFade.FADE_TYPE color)
			{
				customFadeSettingFlag_ = true;
				fadeColor_ = color;
				fadeFrame_ = frame;
			}

			public void cancelCustomFadeSetting()
			{
				customFadeSettingFlag_ = false;
			}

			public bool getCustomFadeSetting(out short frame, out dgs.CFade.FADE_TYPE color)
			{
				color = fadeColor_;
				frame = fadeFrame_;
				return customFadeSettingFlag_;
			}

			public void changePlayerCharDisplay()
			{
				ChangePlayerDisplay changePlayerDisplay = new ChangePlayerDisplay();
				changePlayerDisplay.execute(this);
				if (sceneMng.getStage()[0] == 'f')
				{
					MapMarkerUpdater.getSingleton().deregisterAccepter(PlayerMng().PlayerHuman(0).composit2);
					MapMarkerUpdater.getSingleton().registerAccepter(PlayerMng().PlayerHuman(0).composit2, 0);
				}
			}

			public bool canSite()
			{
				bool result = false;
				if (PreviousMode() == WORLD_MODE.WORLD_MODE_FIELD)
				{
					result = true;
				}
				return result;
			}

			public void doSite()
			{
				if (canSite())
				{
					isSite_ = true;
				}
			}

			public bool isSite()
			{
				return isSite_;
			}

			public void resetSite()
			{
				isSite_ = false;
			}

			public void setPreviousMode(WORLD_MODE _PreviousMode)
			{
				m_PreviousMode = _PreviousMode;
			}

			public WORLD_MODE PreviousMode()
			{
				return m_PreviousMode;
			}

			public void setPreviousState(WORLD_STATE _PreviousState)
			{
				m_PreviousState = _PreviousState;
			}

			public WORLD_STATE PreviousState()
			{
				return m_PreviousState;
			}

			public void setEnd(bool _End)
			{
				m_End = _End;
			}

			public bool isEnd()
			{
				return m_End;
			}

			public static void initNextMode()
			{
				m_Next = NEXT_MODE.NEXT_ERROR;
			}

			public static bool isChangePlayerDraw()
			{
				return m_ChangePlayerDraw;
			}

			public static void onChangePlayerDraw()
			{
				m_ChangePlayerDraw = true;
			}

			public static void offChangePlayerDraw()
			{
				m_ChangePlayerDraw = false;
			}

			public pl.CPlayerManager PlayerMng()
			{
				return m_PlayerMng;
			}

			public cmr.CWorldCamera WorldCamera()
			{
				return m_WldCamera;
			}

			public map.CEnCountManager EnCountManager()
			{
				return m_EnCountMng;
			}

			public CWorld2DManager World2DMng()
			{
				return m_Wld2DMng;
			}

			public dgs.ScreenFlash ScrFlash()
			{
				return m_ScreenFlash;
			}

			public WorldBGControl WorldBGCtrl()
			{
				return wbc_;
			}

			public WorldOBJControl WorldOBJCtrl()
			{
				return woc_;
			}

			public WorldMode CrtMode()
			{
				return m_CurrentMode[(int)m_Mode];
			}

			public CBaseState CrtState()
			{
				_ = m_State;
				_ = 3;
				return m_CurrentMode[(int)m_Mode].m_CurrentState[(int)m_State];
			}

			public CAST_FILE CastFile()
			{
				return m_CastFile;
			}

			public virtual void initialize()
			{
			}

			public virtual void execute()
			{
			}

			public virtual void terminate()
			{
			}

			public virtual void update()
			{
			}

			public int npcEntryId()
			{
				return npcEntryId_;
			}

			public void setNpcEntryId(int _id)
			{
				npcEntryId_ = _id;
			}

			public int npcId()
			{
				return npcId_;
			}

			public void setNpcId(int _id)
			{
				npcId_ = _id;
			}

			public int lastLogic()
			{
				return m_LastLogic;
			}

			public void lastLogic_set(int arg0)
			{
				m_LastLogic = arg0;
			}

			public int lastMessage()
			{
				return m_LastMessage;
			}

			public void lastMessage_set(int arg0)
			{
				m_LastMessage = arg0;
			}

			public void setMapSoundSetting(bool b)
			{
				setUpMapSound_ = b;
			}

			public bool getMapSoundSetting()
			{
				return setUpMapSound_;
			}
		}
	}
}
