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
		public class CPlayerManager
		{
			public static CPlayerVehicle m_dummyVehicle = new CPlayerVehicle();

			public static CPlayerCharacter m_dummyPlayer = new CPlayerCharacter();

			private CBasePlayer[] m_Player = new CBasePlayer[FIELD_CHARACTER_NUM];

			private CPlayerHuman[] m_PlayerHuman = new CPlayerHuman[24];

			private CPlayerVehicle[] m_PlayerVehicle = new CPlayerVehicle[4];

			private map.CMapObject[] m_MapObject = new map.CMapObject[MAP_OBJECT_NUM];

			private int m_MainNpcNo;

			private int m_CanoeId;

			public void setPlayerStart(int _Index)
			{
				if (m_Player[_Index] != null && m_Player[_Index].getCharacterId() != -1)
				{
					m_Player[_Index].getParamMove().init();
					m_Player[_Index].getParamTurn().init();
					if (strcmp(sceneMng.getStage(), "debug01") != 0 && map.CMapParameterManager.Instance().isLoaded())
					{
						m_Player[_Index].isGrv_set(arg0: true);
					}
					m_Player[_Index].setAutoPilot(_AutoPilot: false);
					m_Player[_Index].setNowAct(0);
					m_Player[_Index].setNextAct(0);
					if ((long)_Index < 24L)
					{
						m_PlayerHuman[_Index].setAction(CPlayerHuman.ACTION_ID.ACTION_ID_WAIT);
						m_PlayerHuman[_Index].InputPermission_set(arg0: true);
					}
					else
					{
						m_PlayerVehicle[(long)_Index - 24L].setAction(CPlayerVehicle.ACTION_ID.ACTION_ID_WAIT);
						m_PlayerVehicle[(long)_Index - 24L].InputPermission_set(arg0: true);
					}
				}
			}

			public void setPlayerStop(int _Index)
			{
				if (m_Player[_Index] != null && m_Player[_Index].getCharacterId() != -1)
				{
					m_Player[_Index].getParamMove().init();
					m_Player[_Index].getParamTurn().init();
					m_Player[_Index].getParamGrv().init();
					m_Player[_Index].setAutoPilot(_AutoPilot: true);
					VecFx32 vecFx = new VecFx32(0, 0, 0);
					m_Player[_Index].setTargetDirection(vecFx);
					m_Player[_Index].MoveSys().setTargetPoint(0, vecFx);
					m_Player[_Index].MoveSys().setTargetPoint(1, vecFx);
					if ((long)_Index < 24L)
					{
						m_PlayerHuman[_Index].InputPermission_set(arg0: false);
					}
					else
					{
						m_PlayerVehicle[(long)_Index - 24L].InputPermission_set(arg0: false);
					}
				}
			}

			public void setAllPlayerAutoPilot(bool _Flag)
			{
				for (int i = 0; (long)i < 28L; i++)
				{
					if (m_Player[i] == null || m_Player[i].getCharacterId() == -1)
					{
						continue;
					}
					if (m_Player[i].CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
					{
						CPlayerVehicle cPlayerVehicle = (CPlayerVehicle)m_Player[i];
						if (cPlayerVehicle.getBoardPlayer() == null)
						{
							continue;
						}
					}
					else
					{
						CPlayerHuman cPlayerHuman = (CPlayerHuman)m_Player[i];
						if (CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW == cPlayerHuman.NPCAiManager().AiKind())
						{
							continue;
						}
					}
					m_Player[i].setAutoPilot(_Flag);
				}
			}

			public int searchNullPlayerHumanIndex()
			{
				for (int i = 0; (long)i < 24L; i++)
				{
					if (m_PlayerHuman[i].getCharacterId() == -1)
					{
						return i;
					}
				}
				return -1;
			}

			public int searchNullPlayerVehicleIndex()
			{
				for (int i = 0; (long)i < 4L; i++)
				{
					if (m_PlayerVehicle[i] == null)
					{
						return i;
					}
				}
				return -1;
			}

			public int searchNullMapObjectIndex()
			{
				for (int i = 0; i < MAP_OBJECT_NUM; i++)
				{
					if (m_MapObject[i].getCharacterId() == -1)
					{
						return i;
					}
				}
				return -1;
			}

			public int setUpWorldCharacter(VecFx32 _Position, VecFx32 _Rotation, VecFx32 _Scale, VecFx32 _ShadowScale, string _ChrName, bool _AutoPilot, bool _Operater)
			{
				if (_ChrName == null)
				{
					return -1;
				}
				strcpy(out var arg, _ChrName);
				int num = -1;
				sprintf(out var _, "%s.nmdp.lz", arg);
				uint num2 = 1u;
				switch (_ChrName[0])
				{
				case 'j':
					if (num2 == 0)
					{
						strcpy(out arg, "j101");
					}
					num = setUpPlayerHuman(arg, _AutoPilot, _Operater);
					m_PlayerHuman[num].HumanType_set(PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_HUMAN);
					m_PlayerHuman[num].PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_TOWN);
					break;
				case 'f':
				case 'n':
				{
					PLAYER_HUMAN_TYPE arg3 = PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_HUMAN;
					PLAYER_MOVE_TYPE arg4 = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_DEFAULT_NPC;
					if (strcmp(arg, "n451") == 0 || strcmp(arg, "n461") == 0 || strcmp(arg, "n471") == 0 || strcmp(arg, "n481") == 0 || strcmp(arg, "n491") == 0 || strcmp(arg, "n491") == 0 || strcmp(arg, "n511") == 0 || strcmp(arg, "n442") == 0)
					{
						num = setUpPlayerVehicle(arg, _AutoPilot, _Operater);
						num += 24;
						break;
					}
					if (strcmp(_ChrName, "n441") == 0)
					{
						arg3 = PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_CHOKOBO;
						arg4 = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_TOWN;
					}
					else if (strcmp(arg, "n551") == 0)
					{
						VEC_Set(_Scale, 5324, 5324, 5324);
					}
					else if (strcmp(arg, "n031") == 0 || strcmp(arg, "n041") == 0)
					{
						VEC_Set(_Scale, 3317, 3317, 3317);
						VEC_Set(_ShadowScale, 2730, 2048, 2730);
					}
					else if (strcmp(arg, "n431") == 0 || strcmp(arg, "n435") == 0 || strcmp(arg, "n436") == 0 || strcmp(arg, "n437") == 0)
					{
						arg3 = PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_FROG;
						arg4 = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_FROG;
						VEC_Set(_Scale, 1228, 1228, 1228);
						VEC_Set(_ShadowScale, 2730, 1024, 2730);
					}
					else if (strcmp(arg, "n251") == 0)
					{
						VEC_Set(_Scale, 3317, 3317, 3317);
						VEC_Set(_ShadowScale, 2730, 2048, 2730);
					}
					else if (strcmp(arg, "n261") == 0)
					{
						arg3 = PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_SHEEP;
						arg4 = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_SHEEP;
						VEC_Set(_Scale, 3072, 3072, 3072);
						VEC_Set(_ShadowScale, 2730, 2048, 2730);
					}
					else if (strcmp(arg, "n221") == 0)
					{
						arg3 = PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_FAIRY;
						arg4 = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_FAIRY;
						VEC_Set(_Scale, 2048, 2048, 2048);
						VEC_Set(_Scale, 1638, 1638, 1638);
						VEC_Set(_ShadowScale, 2730, 4096, 2730);
					}
					else if (strcmp(arg, "n351") == 0)
					{
						int num3 = 2867;
						VEC_Set(_Scale, num3, num3, num3);
						VEC_Set(_ShadowScale, num3, num3, num3);
					}
					else if (strcmp(arg, "n561") == 0)
					{
						int num4 = 4096;
						VEC_Set(_Scale, num4, num4, num4);
						VEC_Set(_ShadowScale, num4, num4, num4);
					}
					else if (strcmp(arg, "n571") == 0)
					{
						int num5 = 4096;
						VEC_Set(_Scale, num5, num5, num5);
						VEC_Set(_ShadowScale, num5, num5, num5);
					}
					if (num2 == 0)
					{
						strcpy(out arg, "n011");
					}
					num = setUpPlayerHuman(arg, _AutoPilot, _Operater);
					m_PlayerHuman[num].HumanType_set(arg3);
					m_PlayerHuman[num].PlayerMoveType_set(arg4);
					m_Player[num].NPCRandomMoveType_set(NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_DEFAULT);
					m_Player[num].NPCAutoFollowType_set(NPC_AUTO_FOLLOW_TYPE.NPC_AUTO_FOLLOW_TYPE_DEFAULT);
					break;
				}
				case 'o':
				case 'w':
					if (num2 == 0)
					{
						strcpy(out arg, "o001");
					}
					num = setUpMapObject(arg);
					num += (int)(FIELD_CHARACTER_NUM - MAP_OBJECT_NUM);
					m_Player[num].PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_FIELD);
					break;
				}
				m_Player[num].setModelName(arg);
				m_Player[num].setPosition(_Position);
				m_Player[num].setRotation(_Rotation);
				m_Player[num].setScale(_Scale);
				m_Player[num].setShadowScale(_ShadowScale);
				m_Player[num].setShadowAlpha(10);
				m_Player[num].setTargetDirectionFromRotation();
				m_Player[num].getPreParamObj_set(m_Player[num].getParamObj());
				m_Player[num].setMainPos(m_Player[num].getPosition());
				FS_ChangeDir("/");
				return num;
			}

			public int setUpPlayerHuman(string _ChrName, bool _AutoPilot, bool _Operater)
			{
				if (_ChrName == null)
				{
					return -1;
				}
				bool flag = true;
				bool flag2 = false;
				bool flag3 = false;
				string arg = "";
				int num = -1;
				int num2 = -1;
				if (_ChrName[0] != 'j' && strcmp(_ChrName, "n441") != 0)
				{
					strcmp(_ChrName, "n431");
				}
				strcpy(out var arg2, _ChrName);
				num = characterMng.setCharacter(arg2, CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST);
				TexDivideLoader.getSingleton().tdlForceLoad();
				characterMng.releaseMdlTexRes(num);
				if (num == -1)
				{
					return -1;
				}
				bool flag4 = false;
				if ((CCastCommandTransit.getInstance().cast_BaseSystem().Mode() != wld.CBaseSystem.WORLD_MODE.WORLD_MODE_MENU && CCastCommandTransit.getInstance().cast_BaseSystem().Mode() == wld.CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD) || (CCastCommandTransit.getInstance().cast_BaseSystem().Mode() == wld.CBaseSystem.WORLD_MODE.WORLD_MODE_MENU && CCastCommandTransit.getInstance().cast_BaseSystem().PreviousMode() == wld.CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD))
				{
					flag4 = true;
				}
				if (_ChrName[0] == 'f' || strcmp(_ChrName, "n441") == 0 || strcmp(_ChrName, "n221") == 0 || strcmp(_ChrName, "n261") == 0 || strcmp(_ChrName, "n141") == 0 || strcmp(_ChrName, "n211") == 0 || strcmp(_ChrName, "n351") == 0 || strcmp(_ChrName, "n291") == 0 || strcmp(_ChrName, "n661") == 0 || strcmp(_ChrName, "n251") == 0 || strcmp(_ChrName, "n401") == 0 || strcmp(_ChrName, "n321") == 0 || strcmp(_ChrName, "n161") == 0)
				{
					sprintf(out arg, "w_act_%s", _ChrName);
				}
				else if (strcmp(_ChrName, "n431") == 0 || strcmp(_ChrName, "n435") == 0 || strcmp(_ChrName, "n436") == 0 || strcmp(_ChrName, "n437") == 0)
				{
					sprintf(out arg, "w_act_n431");
				}
				else if (strcmp(_ChrName, "n071") == 0 || strcmp(_ChrName, "n072") == 0 || strcmp(_ChrName, "n073") == 0 || strcmp(_ChrName, "n081") == 0 || strcmp(_ChrName, "n082") == 0 || strcmp(_ChrName, "n083") == 0 || strcmp(_ChrName, "n311") == 0 || strcmp(_ChrName, "n371") == 0)
				{
					strcpy(out arg, "w_field_old");
					flag2 = true;
					flag3 = true;
				}
				else if (strcmp(_ChrName, "n551") == 0 || strcmp(_ChrName, "n531") == 0 || strcmp(_ChrName, "n551") == 0)
				{
					strcpy(out arg, "w_field_fat");
				}
				else if (strcmp(_ChrName, "n711") == 0 || strcmp(_ChrName, "n721") == 0)
				{
					flag = false;
				}
				else if (!flag4)
				{
					strcpy(out arg, "w_act_man");
					flag2 = true;
					flag3 = true;
				}
				else
				{
					strcpy(out arg, "w_field_man");
				}
				if (flag)
				{
					characterMng.addMotion(num, arg);
				}
				if (flag3)
				{
					characterMng.addMotion(num, "w_sleep");
				}
				if (flag2 && (evt.CEventManager.getInstance().isEvent() || CCastCommandTransit.getInstance().cast_BaseSystem().Mode() == wld.CBaseSystem.WORLD_MODE.WORLD_MODE_TALK))
				{
					addEventMotion(num);
				}
				if (_ChrName[0] != 'j' && strcmp(_ChrName, "n441") != 0)
				{
					strcmp(_ChrName, "n431");
				}
				num2 = searchNullPlayerHumanIndex();
				if (num2 == -1)
				{
					return -1;
				}
				m_PlayerHuman[num2].setCharacterId(num);
				m_PlayerHuman[num2].setAutoPilot(_AutoPilot);
				m_PlayerHuman[num2].setOperater(_Operater);
				m_PlayerHuman[num2].setLight(g_Light);
				return num2;
			}

			public int setUpPlayerVehicle(string _ChrName, bool _AutoPilot, bool _Operater)
			{
				if (_ChrName == null)
				{
					return -1;
				}
				int num = -1;
				int num2 = -1;
				strcpy(out var arg, _ChrName);
				if (strcmp(arg, "n471") == 0 || strcmp(arg, "n481") == 0)
				{
					sprintf(out arg, "n451");
				}
				if (strcmp(arg, "n442") == 0)
				{
					sprintf(out arg, "n441");
				}
				num = characterMng.setCharacter(arg, CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST);
				TexDivideLoader.getSingleton().tdlForceLoad();
				characterMng.releaseMdlTexRes(num);
				if (num == -1)
				{
					return -1;
				}
				num2 = searchNullPlayerVehicleIndex();
				if (num2 == -1)
				{
					return -1;
				}
				if (strcmp(_ChrName, "n461") == 0)
				{
					m_PlayerVehicle[num2] = (VehicleCanoe)ds.CHeap.alloc_app(typeof(VehicleCanoe));
					m_CanoeId = num2;
				}
				if (strcmp(_ChrName, "n451") == 0)
				{
					m_PlayerVehicle[num2] = (VehicleShido)ds.CHeap.alloc_app(typeof(VehicleShido));
				}
				if (strcmp(_ChrName, "n471") == 0 || strcmp(_ChrName, "n481") == 0)
				{
					m_PlayerVehicle[num2] = (VehicleEnterp)ds.CHeap.alloc_app(typeof(VehicleEnterp));
				}
				if (strcmp(_ChrName, "n491") == 0)
				{
					m_PlayerVehicle[num2] = (VehicleNorchi)ds.CHeap.alloc_app(typeof(VehicleNorchi));
				}
				if (strcmp(_ChrName, "n511") == 0)
				{
					m_PlayerVehicle[num2] = (VehicleInvinsible)ds.CHeap.alloc_app(typeof(VehicleInvinsible));
				}
				if (strcmp(_ChrName, "n442") == 0)
				{
					m_PlayerVehicle[num2] = (VehicleChokobo)ds.CHeap.alloc_app(typeof(VehicleChokobo));
				}
				if (m_PlayerVehicle[num2] == null)
				{
					m_PlayerVehicle[num2] = (CPlayerVehicle)ds.CHeap.alloc_app(typeof(CPlayerVehicle));
				}
				m_PlayerVehicle[num2].initialize();
				m_Player[24L + (long)num2] = m_PlayerVehicle[num2];
				m_PlayerVehicle[num2].setCharacterId(num);
				m_PlayerVehicle[num2].setAutoPilot(_AutoPilot);
				m_PlayerVehicle[num2].setOperater(_Operater);
				m_PlayerVehicle[num2].setLight(g_Light);
				VecFx32 scale = new VecFx32(4096, 4096, 4096);
				m_PlayerVehicle[num2].setScale(scale);
				return num2;
			}

			public int setUpMapObject(string _ChrName)
			{
				if (_ChrName == null)
				{
					return -1;
				}
				int num = -1;
				int num2 = -1;
				strcpy(out var arg, _ChrName);
				sprintf(out var arg2, "w_%s", _ChrName);
				num = characterMng.setCharacter(arg, CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST);
				TexDivideLoader.getSingleton().tdlForceLoad();
				characterMng.releaseMdlTexRes(num);
				if (num == -1)
				{
					return -1;
				}
				sprintf(out var _, "%s.ncap.lz", arg2);
				characterMng.addMotion(num, arg2);
				num2 = searchNullMapObjectIndex();
				if (num2 == -1)
				{
					return -1;
				}
				m_MapObject[num2].setCharacterId(num);
				sbyte[] array = new sbyte[4];
				int num3 = -1;
				array[0] = (sbyte)_ChrName[1];
				array[1] = (sbyte)_ChrName[2];
				array[2] = (sbyte)_ChrName[3];
				num3 = atoi(StringUtil.createString(array));
				m_MapObject[num2].setMapObjType(static_cast<map.MAP_OBJECT_TYPE>(num3));
				m_MapObject[num2].setLight(g_Light);
				VecFx32 scale = new VecFx32(4096, 4096, 4096);
				m_MapObject[num2].setScale(scale);
				return num2;
			}

			public void addEventMotion(int _ChrIndex)
			{
				_ = -1;
				int num = 1;
				if ((CCastCommandTransit.getInstance().cast_BaseSystem().Mode() != wld.CBaseSystem.WORLD_MODE.WORLD_MODE_MENU && CCastCommandTransit.getInstance().cast_BaseSystem().Mode() != wld.CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD) || (CCastCommandTransit.getInstance().cast_BaseSystem().Mode() == wld.CBaseSystem.WORLD_MODE.WORLD_MODE_MENU && CCastCommandTransit.getInstance().cast_BaseSystem().PreviousMode() != wld.CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD))
				{
					characterMng.addMotion(_ChrIndex, "w_event_common");
				}
				for (int i = 2; i < 8 && evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_CHAPTER_FLAG[i - 2]) != 0; i++)
				{
					num = i;
				}
				sprintf(out var arg, "w_event%02d", num);
				sprintf(out var _, "%s.ncap.lz", arg);
				characterMng.addMotion(_ChrIndex, arg);
			}

			public void removeEventMotion(int _ChrIndex)
			{
				_ = -1;
				int num = 1;
				if ((CCastCommandTransit.getInstance().cast_BaseSystem().Mode() != wld.CBaseSystem.WORLD_MODE.WORLD_MODE_MENU && CCastCommandTransit.getInstance().cast_BaseSystem().Mode() != wld.CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD) || (CCastCommandTransit.getInstance().cast_BaseSystem().Mode() == wld.CBaseSystem.WORLD_MODE.WORLD_MODE_MENU && CCastCommandTransit.getInstance().cast_BaseSystem().PreviousMode() != wld.CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD))
				{
					characterMng.removeMotion(_ChrIndex, "w_event_common");
				}
				for (int i = 2; i < 8 && evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_CHAPTER_FLAG[i - 2]) != 0; i++)
				{
					num = i;
				}
				sprintf(out var arg, "w_event%02d", num);
				sprintf(out var arg2, "%s.ncap.lz", arg);
				if (ds.g_File.getSize(arg2) == 0)
				{
					num = 1;
					sprintf(out arg, "w_event%02d", num);
				}
				characterMng.removeMotion(_ChrIndex, arg);
			}

			public void initialize()
			{
				for (int i = 0; i < FIELD_CHARACTER_NUM; i++)
				{
					m_Player[i] = null;
				}
				byte b = 0;
				while ((uint)b < 4u)
				{
					m_PlayerVehicle[b] = null;
					b++;
				}
				setPlayerRegister();
				setUserAuto();
				for (int j = 0; j < FIELD_CHARACTER_NUM; j++)
				{
					if (m_Player[j] != null)
					{
						m_Player[j].reset();
						m_Player[j].initialize();
					}
				}
				sbyte b2 = 0;
				while ((long)b2 < 4L)
				{
					m_PlayerVehicle[b2] = null;
					b2++;
				}
				g_Light.setDiffuze(20, 10, 10);
				g_Light.setAmbient(6, 6, 6);
				g_Light.setSpecular(10, 16, 0);
				g_Light.setEmission(6, 6, 14);
				g_Light.setLightVector(0, 3786, -1379, -728);
				g_Light.setLightVector(1, 0, 0, 0);
				g_Light.setLightVector(2, 0, 0, 0);
				g_Light.setLightVector(3, 0, 0, 0);
				g_Light.setLightColor(0, 31, 31, 27);
				m_dummyVehicle.initialize();
				m_dummyPlayer.initialize();
				m_MainNpcNo = -1;
				m_CanoeId = -1;
			}

			public void execute()
			{
				for (int i = 0; i < FIELD_CHARACTER_NUM; i++)
				{
					if (m_Player[i] != null && m_Player[i].getCharacterId() != -1)
					{
						m_Player[i].execute();
					}
				}
			}

			public void terminate()
			{
				for (int i = 0; i < FIELD_CHARACTER_NUM; i++)
				{
					if (m_Player[i] != null && m_Player[i].getCharacterId() != -1)
					{
						m_Player[i].terminate();
					}
				}
				sbyte b = 0;
				while ((long)b < 4L)
				{
					if (m_PlayerVehicle[b] != null)
					{
						m_PlayerVehicle[b].destruct();
						ds.CHeap.free_app(m_PlayerVehicle[b]);
						m_PlayerVehicle[b] = null;
					}
					b++;
				}
			}

			public void deletePlayerVehicle(int chrIdx)
			{
				if (chrIdx >= 0 && FIELD_CHARACTER_NUM > chrIdx)
				{
					int num = (int)((long)chrIdx - 24L);
					if (num >= 0 && 4L > (long)num && m_Player[chrIdx] != null && m_PlayerVehicle[num] != null)
					{
						m_Player[chrIdx].terminate();
						m_PlayerVehicle[num].destruct();
						ds.CHeap.free_app(m_PlayerVehicle[num]);
						m_PlayerVehicle[num] = null;
						m_Player[chrIdx] = null;
					}
				}
			}

			public void into()
			{
				for (int i = 0; i < FIELD_CHARACTER_NUM; i++)
				{
					if (m_Player[i] != null && m_Player[i].getCharacterId() != -1)
					{
						m_Player[i].into();
					}
				}
			}

			public void update()
			{
				for (int i = 0; i < FIELD_CHARACTER_NUM; i++)
				{
					if (m_Player[i] != null && m_Player[i].getCharacterId() != -1)
					{
						m_Player[i].update();
					}
				}
			}

			public void setPlayerRegister()
			{
				uint num = 0u;
				num += 24;
				for (uint num2 = num - 24; num2 < num; num2++)
				{
					m_Player[num2] = m_PlayerHuman[num2 - (num - 24)];
				}
				num += 4;
				for (uint num3 = num - 4; num3 < num; num3++)
				{
					m_Player[num3] = m_PlayerVehicle[num3 - (num - 4)];
				}
				num += MAP_OBJECT_NUM;
				for (uint num4 = num - MAP_OBJECT_NUM; num4 < num; num4++)
				{
					m_Player[num4] = m_MapObject[num4 - (num - MAP_OBJECT_NUM)];
				}
			}

			public void setUserAuto()
			{
				uint num = 0u;
				for (num = 0u; num < FIELD_CHARACTER_NUM; num++)
				{
					if (m_Player[num] != null && m_Player[num].getCharacterId() != -1)
					{
						m_Player[num].setOperater(_Operater: false);
					}
				}
			}

			public void checkPCCollision()
			{
				short num = 0;
				short num2 = 0;
				ds.pri.DSSphere[] pl_reuse__ColSphere = pl.pl_reuse__ColSphere;
				ds.pri.DSSphere[] pl_reuse__CckSphere = pl.pl_reuse__CckSphere;
				ds.pri.DSAABB[] pl_reuse__ColAABB = pl.pl_reuse__ColAABB;
				for (num = (short)(FIELD_CHARACTER_NUM - 1); num >= 0; num--)
				{
					bool flag = false;
					bool flag2 = false;
					if (m_Player[num] != null && m_Player[num].getCharacterId() != -1 && (m_Player[num].getColFlag() & 4) != 0 && m_Player[num].CharaKind() != chr.CHARACTER_KIND.CHARACTER_KIND_MAP_OBJECT && (num >= m_PlayerHuman.Length || m_PlayerHuman[num].NPCAiManager().AiKind() != CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW))
					{
						m_Player[num].getColType_not_and(1);
						m_Player[num].getColType_not_and(2);
						pl_reuse__ColSphere[0].r = m_Player[num].getColRadius();
						pl_reuse__ColSphere[0].c.copy(m_Player[num].getPosition());
						VEC_Add(pl_reuse__ColSphere[0].c, m_Player[num].getColOffset(), pl_reuse__ColSphere[0].c);
						pl_reuse__CckSphere[0].r = m_Player[num].getCckRadius();
						pl_reuse__CckSphere[0].c.copy(m_Player[num].getPosition());
						VEC_Add(pl_reuse__CckSphere[0].c, m_Player[num].getCckOffset(), pl_reuse__CckSphere[0].c);
						if (m_Player[num].CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN)
						{
							int nowAct = m_Player[num].getNowAct();
							if (nowAct == 1 || nowAct == 2)
							{
								pl_reuse__CckSphere[0].r -= 6144;
							}
						}
						for (num2 = (short)(FIELD_CHARACTER_NUM - 1); num2 >= 0; num2--)
						{
							if (num == num2 || m_Player[num2] == null || m_Player[num2].getCharacterId() == -1 || VEC_Distance(m_Player[num].getPosition(), m_Player[num2].getPosition()) >= 409600)
							{
								continue;
							}
							if (m_Player[num2].CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN || m_Player[num2].CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
							{
								if (num2 < m_PlayerHuman.Length && m_PlayerHuman[num2].NPCAiManager().AiKind() == CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW)
								{
									continue;
								}
								pl_reuse__ColSphere[1].r = m_Player[num2].getColRadius();
								pl_reuse__ColSphere[1].c.copy(m_Player[num2].getPosition());
								VEC_Add(pl_reuse__ColSphere[1].c, m_Player[num2].getColOffset(), pl_reuse__ColSphere[1].c);
								pl_reuse__CckSphere[1].r = m_Player[num2].getCckRadius();
								pl_reuse__CckSphere[1].c.copy(m_Player[num2].getPosition());
								VEC_Add(pl_reuse__CckSphere[1].c, m_Player[num2].getCckOffset(), pl_reuse__CckSphere[1].c);
								if (ds.pri.PrimitiveTest.testSphereSphere(pl_reuse__ColSphere[0], pl_reuse__ColSphere[1]))
								{
									m_Player[num].getColType_or(1);
									flag = true;
								}
							}
							else if (m_Player[num2].CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_MAP_OBJECT)
							{
								pl_reuse__ColAABB[1].r.copy(m_Player[num2].getColAabbRadius());
								pl_reuse__ColAABB[1].c.copy(m_Player[num2].getPosition());
								VEC_Add(pl_reuse__ColAABB[1].c, m_Player[num2].getColOffset(), pl_reuse__ColAABB[1].c);
								pl_reuse__CckSphere[1].r = m_Player[num2].getCckRadius();
								pl_reuse__CckSphere[1].c.copy(m_Player[num2].getPosition());
								VEC_Add(pl_reuse__CckSphere[1].c, m_Player[num2].getCckOffset(), pl_reuse__CckSphere[1].c);
								if (ds.pri.PrimitiveTest.testSphereAABB(pl_reuse__ColSphere[0], pl_reuse__ColAABB[1]))
								{
									m_Player[num].getColType_or(1);
									flag = true;
								}
							}
							if (ds.pri.PrimitiveTest.testSphereSphere(pl_reuse__CckSphere[0], pl_reuse__CckSphere[1]))
							{
								bool flag3 = false;
								if (chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE == m_Player[num2].CharaKind())
								{
									CPlayerVehicle cPlayerVehicle = (CPlayerVehicle)m_Player[num2];
									if (PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP == cPlayerVehicle.getVehicleType() || PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM == cPlayerVehicle.getVehicleType())
									{
										if (m_Player[num].checkSightAngle(m_Player[num2]))
										{
											flag3 = true;
										}
									}
									else
									{
										flag3 = true;
									}
								}
								else if (m_Player[num].checkSightAngle(m_Player[num2]))
								{
									flag3 = true;
								}
								if (flag3)
								{
									m_Player[num].getColType_or(2);
									flag2 = true;
								}
							}
							if (m_Player[num].getColType() != 0)
							{
								m_Player[num].checkCollisionCharacter(static_cast<chr.CCharacterEureka>(m_Player[num2]));
								m_Player[num].getColType_not_and(1);
								m_Player[num].getColType_not_and(2);
							}
						}
						if (flag)
						{
							m_Player[num].getColType_or(1);
						}
						if (flag2)
						{
							m_Player[num].getColType_or(2);
						}
					}
				}
			}

			public void checkTouchCollision()
			{
				if (!ds.g_TouchPanel.isEdge())
				{
					return;
				}
				ds.g_TouchPanel.getPoint(out var x, out var y);
				VecFx32 vecFx = new VecFx32(0, 0, 0);
				VecFx32 vecFx2 = new VecFx32(0, 0, 0);
				NNS_G3dScrPosToWorldLine(x, y, vecFx, null);
				vecFx2.copy(CCastCommandTransit.getInstance().cast_FieldCamera().getPosition());
				ds.pri.DSLine l = new ds.pri.DSLine(vecFx2, vecFx);
				int playCharacterIndex = wld.CWorldOutSideData.getInstance().PlayerData().getPlayCharacterIndex();
				if (m_Player[playCharacterIndex] == null || m_Player[playCharacterIndex].getCharacterId() == -1 || (m_Player[playCharacterIndex].getColFlag() & 2) == 0)
				{
					return;
				}
				CBasePlayer cBasePlayer = m_Player[playCharacterIndex];
				CBasePlayer cBasePlayer2 = null;
				m_Player[playCharacterIndex].getColType_not_and(4);
				int num = 0;
				for (num = 0; num < FIELD_CHARACTER_NUM; num++)
				{
					if (m_Player[num] == null || m_Player[num].getCharacterId() == -1 || (m_Player[num].getColFlag() & 2) == 0)
					{
						continue;
					}
					cBasePlayer2 = m_Player[num];
					if (cBasePlayer == cBasePlayer2 && cBasePlayer.CharaKind() != chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
					{
						continue;
					}
					int num2 = VEC_Distance(cBasePlayer.getPosition(), cBasePlayer2.getPosition());
					if (num2 < 1048576)
					{
						ds.pri.DSSphere pl_reuse_sphere = pl.pl_reuse_sphere;
						pl_reuse_sphere.set(cBasePlayer2.getPosition(), cBasePlayer2.getTchRadius());
						if (ds.pri.PrimitiveTest.testRaySphere(l, pl_reuse_sphere, null, null))
						{
							cBasePlayer.getColType_or(4);
							break;
						}
					}
				}
				if ((cBasePlayer.getColType() & 4) != 0)
				{
					if (strcmp(sceneMng.getStage(), "d01_03") == 0 && m_MapObject[num - (FIELD_CHARACTER_NUM - MAP_OBJECT_NUM)].MapObjType() == map.MAP_OBJECT_TYPE.WIND_CRYSTAL)
					{
						num = (int)(FIELD_CHARACTER_NUM - MAP_OBJECT_NUM + 1);
					}
					cBasePlayer.setTarget(static_cast<chr.CCharacterEureka>(cBasePlayer2));
				}
			}

			public bool checkPlayerType(PLAYER_TYPE player_type)
			{
				bool result = false;
				CBasePlayer cBasePlayer = m_Player[wld.CWorldOutSideData.getInstance().PlayerData().getPlayCharacterIndex()];
				switch (player_type)
				{
				case PLAYER_TYPE.PLAYER_TYPE_HUMAN:
					if (cBasePlayer.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN)
					{
						result = true;
					}
					break;
				case PLAYER_TYPE.PLAYER_TYPE_FROG:
					if (cBasePlayer.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN && cBasePlayer.getPlayerMoveType() == PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_FROG)
					{
						result = true;
					}
					break;
				case PLAYER_TYPE.PLAYER_TYPE_MINI:
					if (cBasePlayer.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN && cBasePlayer.getPlayerMoveType() == PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_LILLIPUT)
					{
						result = true;
					}
					break;
				case PLAYER_TYPE.PLAYER_TYPE_CHOKOBO:
					if (cBasePlayer.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
					{
						CPlayerVehicle cPlayerVehicle7 = static_cast<CPlayerVehicle>(cBasePlayer);
						if (cPlayerVehicle7.getVehicleType() == PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CHOKOBO)
						{
							result = true;
						}
					}
					break;
				case PLAYER_TYPE.PLAYER_TYPE_SHIDO_H:
					if (cBasePlayer.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
					{
						CPlayerVehicle cPlayerVehicle2 = static_cast<CPlayerVehicle>(cBasePlayer);
						if (cPlayerVehicle2.getVehicleType() == PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_SHIDO_H)
						{
							result = true;
						}
					}
					break;
				case PLAYER_TYPE.PLAYER_TYPE_CANOE:
					if (cBasePlayer.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
					{
						CPlayerVehicle cPlayerVehicle6 = static_cast<CPlayerVehicle>(cBasePlayer);
						if (cPlayerVehicle6.getVehicleType() == PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CANOE)
						{
							result = true;
						}
					}
					break;
				case PLAYER_TYPE.PLAYER_TYPE_ENTERP:
					if (cBasePlayer.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
					{
						CPlayerVehicle cPlayerVehicle3 = static_cast<CPlayerVehicle>(cBasePlayer);
						if (cPlayerVehicle3.getVehicleType() == PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP)
						{
							result = true;
						}
					}
					break;
				case PLAYER_TYPE.PLAYER_TYPE_ENTERP_CTM:
					if (cBasePlayer.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
					{
						CPlayerVehicle cPlayerVehicle8 = static_cast<CPlayerVehicle>(cBasePlayer);
						if (cPlayerVehicle8.getVehicleType() == PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM)
						{
							result = true;
						}
					}
					break;
				case PLAYER_TYPE.PLAYER_TYPE_NORCHI:
					if (cBasePlayer.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
					{
						CPlayerVehicle cPlayerVehicle5 = static_cast<CPlayerVehicle>(cBasePlayer);
						if (cPlayerVehicle5.getVehicleType() == PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_NORCHI)
						{
							result = true;
						}
					}
					break;
				case PLAYER_TYPE.PLAYER_TYPE_NORCHI_CTM:
					if (cBasePlayer.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
					{
						CPlayerVehicle cPlayerVehicle4 = static_cast<CPlayerVehicle>(cBasePlayer);
						if (cPlayerVehicle4.getVehicleType() == PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_NORCHI_CTM)
						{
							result = true;
						}
					}
					break;
				case PLAYER_TYPE.PLAYER_TYPE_INVINSIBLE:
					if (cBasePlayer.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
					{
						CPlayerVehicle cPlayerVehicle = static_cast<CPlayerVehicle>(cBasePlayer);
						if (cPlayerVehicle.getVehicleType() == PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_INVINSIBLE)
						{
							result = true;
						}
					}
					break;
				}
				return result;
			}

			public CPlayerVehicle PlayerVehicle(int index)
			{
				if (index < 0 || 4L <= (long)index)
				{
					return m_dummyVehicle;
				}
				if (m_PlayerVehicle[index] == null)
				{
					return m_dummyVehicle;
				}
				return m_PlayerVehicle[index];
			}

			public CBasePlayer Player(int index)
			{
				if (index < 0 || FIELD_CHARACTER_NUM <= index)
				{
					return m_dummyPlayer;
				}
				if (m_Player[index] == null)
				{
					return m_dummyPlayer;
				}
				return m_Player[index];
			}

			public int setupPlainCharacter(string chrname, VecFx32 scl, VecFx32 shadowScl)
			{
				OS_GetTick();
				int num = characterMng.setCharacter(const_cast<string>(chrname), CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST);
				TexDivideLoader.getSingleton().tdlForceLoad();
				characterMng.releaseMdlTexRes(num);
				int num2 = searchNullPlayerHumanIndex();
				CPlayerHuman cPlayerHuman = m_PlayerHuman[num2];
				cPlayerHuman.setCharacterId(num);
				cPlayerHuman.setAutoPilot(_AutoPilot: false);
				cPlayerHuman.setOperater(_Operater: false);
				cPlayerHuman.HumanType_set(PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_HUMAN);
				cPlayerHuman.PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_TOWN);
				cPlayerHuman.setModelName(const_cast<string>(chrname));
				cPlayerHuman.setScale(const_cast<VecFx32>(scl));
				cPlayerHuman.setShadowScale(const_cast<VecFx32>(shadowScl));
				return num2;
			}

			public CPlayerHuman getNpc()
			{
				byte b = 0;
				while ((uint)b < 24u)
				{
					CPlayerHuman cPlayerHuman = m_PlayerHuman[b];
					if (cPlayerHuman.getCharacterId() >= 0 && cPlayerHuman.NPCAiManager().AiKind() == CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW)
					{
						return cPlayerHuman;
					}
					b++;
				}
				return null;
			}

			public CPlayerManager()
			{
				for (int i = 0; i < m_PlayerHuman.Length; i++)
				{
					m_PlayerHuman[i] = new CPlayerHuman();
				}
				for (int i = 0; i < m_MapObject.Length; i++)
				{
					m_MapObject[i] = new map.CMapObject();
				}
			}

			public CPlayerHuman PlayerHuman(int index)
			{
				if (index < 0)
				{
					return null;
				}
				return m_PlayerHuman[index];
			}

			public map.CMapObject MapObject(int index)
			{
				return m_MapObject[index];
			}

			public int getMainNpcNo()
			{
				return m_MainNpcNo;
			}

			public int getCanoeId()
			{
				return m_CanoeId;
			}

			public CBasePlayer[] getPlayerList()
			{
				return m_Player;
			}
		}
	}
}
