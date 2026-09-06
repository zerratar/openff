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
		public class MapJump
		{
			public void setupMapJumpPosition(CBaseSystem sys)
			{
				VecFx32 vecFx = new VecFx32(0, 0, 0);
				VecFx32 vecFx2 = new VecFx32(0, 0, 0);
				CWorldOutSideData.getInstance().MapData().MapJumpIndex();
				short id = (short)(CWorldOutSideData.getInstance().MapData().NextMapIndex() - 1);
				if (!map.CMapParameterManager.Instance().isLoaded() && !CCastCommandTransit.getInstance().castParam_MapJump().m_Flag)
				{
					VEC_Set(vecFx, 0, 0, 40960);
					sys.PlayerMng().Player(0).setPosition(vecFx);
					sys.PlayerMng().Player(0).setRotation(vecFx2);
					sys.PlayerMng().Player(0).setTargetDirectionFromRotation();
					sys.PlayerMng().Player(0).getPrePosition_set(sys.PlayerMng().Player(0).getPosition());
					sys.PlayerMng().Player(0).getPreRotation_set(sys.PlayerMng().Player(0).getRotation());
					return;
				}
				if (GlobalScope.sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_DEBUG_MENU)
				{
					_ = CCastCommandTransit.getInstance().castParam_MapJump().m_Flag;
					id = 0;
				}
				if (CCastCommandTransit.getInstance().castParam_MapJump().m_Flag)
				{
					vecFx.copy(CCastCommandTransit.getInstance().castParam_MapJump().m_Pos);
					vecFx2.copy(CCastCommandTransit.getInstance().castParam_MapJump().m_Rot);
					CCastCommandTransit.getInstance().castParam_MapJump().initialize();
				}
				else if (CCastCommandTransit.getInstance().castParam_MapJump().m_Flag2)
				{
					id = (short)CCastCommandTransit.getInstance().castParam_MapJump().m_MapJumpIndex;
					vecFx.x = 4096 * map.CMapParameterManager.Instance().MapJumpParameter(id).PlPos(0);
					vecFx.y = 4096 * map.CMapParameterManager.Instance().MapJumpParameter(id).PlPos(1);
					vecFx.z = 4096 * map.CMapParameterManager.Instance().MapJumpParameter(id).PlPos(2);
					vecFx2.y = map.CMapParameterManager.Instance().MapJumpParameter(id).PlRot();
				}
				else
				{
					string preStage = sceneMng.getPreStage();
					if (strcmp("t28_01", preStage) == 0)
					{
						SHoldVehicleData holdData = CWorldOutSideData.getInstance().VehicleData().getHoldData(7);
						vecFx.x = holdData.m_Position.x;
						vecFx.z = holdData.m_Position.z;
					}
					else if (CWorldOutSideData.getInstance().MapData().getBackupPosJump())
					{
						vecFx.copy(CWorldOutSideData.getInstance().PlayerData().getHoldData(0)
							.m_Position);
							vecFx2.copy(CWorldOutSideData.getInstance().PlayerData().getHoldData(0)
								.m_Rotation);
								vecFx.y = 0;
								CWorldOutSideData.getInstance().MapData().setBackupPosJump(b: false);
							}
							else if (!FF3.GameProfile.Ff3MapParameters || map.CMapParameterManager.Instance().MapJumpParameter(id) == null)
							{
								// PORT: no jump table to take the arrival from - the jump part's position.
								vecFx.copy(FF3.JumpPart.StartPosition);
								vecFx2.y = FF3.JumpPart.StartRotation;
							}
							else
							{
								vecFx.x = 4096 * static_cast<int>(map.CMapParameterManager.Instance().MapJumpParameter(id).PlPos(0));
								vecFx.y = 4096 * static_cast<int>(map.CMapParameterManager.Instance().MapJumpParameter(id).PlPos(1));
								vecFx.z = 4096 * static_cast<int>(map.CMapParameterManager.Instance().MapJumpParameter(id).PlPos(2));
								vecFx2.y = static_cast<int>(map.CMapParameterManager.Instance().MapJumpParameter(id).PlRot());
							}
						}
						sys.PlayerMng().Player(0).setPosition(vecFx);
						sys.PlayerMng().Player(0).setRotation(vecFx2);
						sys.PlayerMng().Player(0).setTargetDirectionFromRotation();
						sys.PlayerMng().Player(0).getPrePosition_set(sys.PlayerMng().Player(0).getPosition());
						sys.PlayerMng().Player(0).getPreRotation_set(sys.PlayerMng().Player(0).getRotation());
						CCastCommandTransit.getInstance().castParam_MapJump().initialize();
					}

					public void setupBackupPosition(CBaseSystem sys)
					{
						VecFx32 vecFx = new VecFx32(0, 0, 0);
						VecFx32 vecFx2 = new VecFx32(0, 0, 0);
						for (int i = 0; (long)i < 28L; i++)
						{
							if (sys.PlayerMng().Player(i).getCharacterId() != -1)
							{
								vecFx.copy(CWorldOutSideData.getInstance().PlayerData().getHoldData(i)
									.m_Position);
									vecFx2.copy(CWorldOutSideData.getInstance().PlayerData().getHoldData(i)
										.m_Rotation);
										sys.PlayerMng().Player(i).setPosition(vecFx);
										sys.PlayerMng().Player(i).setRotation(vecFx2);
										sys.PlayerMng().Player(i).setTargetDirectionFromRotation();
										sys.PlayerMng().Player(i).getPrePosition_set(vecFx);
										sys.PlayerMng().Player(i).getPreRotation_set(vecFx2);
									}
								}
							}

							public void mapJumpPosition(CBaseSystem sys)
							{
								bool flag = false;
								string arg = "";
								if (map.CMapParameterManager.Instance().isLoaded())
								{
									flag = ((CWorldOutSideData.getInstance().MapData().MapJumpIndex() != 0) ? true : false);
								}
								if (CCastCommandTransit.getInstance().castParam_MapJump().m_Flag)
								{
									arg = CCastCommandTransit.getInstance().castParam_MapJump().m_MapName;
								}
								else if (CCastCommandTransit.getInstance().castParam_MapJump().m_Flag2)
								{
									arg = CCastCommandTransit.getInstance().castParam_MapJump().m_MapName;
								}
								else if (flag)
								{
									if (CWorldOutSideData.getInstance().MapData().getSpMapType() == CMapData.SP_MAP_TYPE.SP_MAP_DEEPSEA)
									{
										char[] array = stageMng.getChipName().ToCharArray();
										array[2] = '4';
										arg = new string(array);
										CWorldOutSideData.getInstance().MapData().NextMapIndex_set(0);
									}
									else if (CWorldOutSideData.getInstance().MapData().getSpMapType() == CMapData.SP_MAP_TYPE.SP_MAP_AIR)
									{
										char[] array2 = stageMng.getChipName().ToCharArray();
										array2[2] = '3';
										arg = new string(array2);
										CWorldOutSideData.getInstance().MapData().NextMapIndex_set(0);
									}
									else if (CWorldOutSideData.getInstance().MapData().getSpMapType() == CMapData.SP_MAP_TYPE.SP_MAP_INVINSIBLE)
									{
										sprintf(out arg, "t28_01");
										CWorldOutSideData.getInstance().MapData().NextMapIndex_set(1);
									}
									else if (sys.isEscape() && -1 != CWorldOutSideData.getInstance().MapData().getBeforeFieldMapJumpIndex())
									{
										strcpy(out arg, CWorldOutSideData.getInstance().MapData().getBeforeFieldMapName());
										CWorldOutSideData.getInstance().MapData().NextMapIndex_set(CWorldOutSideData.getInstance().MapData().getBeforeFieldMapJumpIndex());
									}
									else
									{
										int id = CWorldOutSideData.getInstance().MapData().MapJumpIndex() - 1;
										arg = map.CMapParameterManager.Instance().MapJumpParameter(id).NextMapName();
										CWorldOutSideData.getInstance().MapData().NextMapIndex_set((sbyte)map.CMapParameterManager.Instance().MapJumpParameter(id).NextMapIndex());
										int commonMdlNo = map.CMapParameterManager.Instance().MapJumpParameter(id).ModelNo();
										CWorldOutSideData.getInstance().MapData().setCommonMdlNo(commonMdlNo);
									}
								}
								else
								{
									strcpy(out arg, "debug01");
								}
								if (strcmp(arg, "back_field_map") == 0)
								{
									arg = CWorldOutSideData.getInstance().MapData().getBeforeFieldMapName();
									CWorldOutSideData.getInstance().MapData().NextMapIndex_set(CWorldOutSideData.getInstance().MapData().getBeforeFieldMapJumpIndex());
									CCastCommandTransit.getInstance().castParam_MapJump().m_Flag = false;
								}
								else if (strcmp(arg, "back_town_map") == 0)
								{
									arg = CWorldOutSideData.getInstance().MapData().getBeforeTownMapName();
									CWorldOutSideData.getInstance().MapData().NextMapIndex_set(CWorldOutSideData.getInstance().MapData().getBeforeTownMapJumpIndex());
									CCastCommandTransit.getInstance().castParam_MapJump().initialize();
								}
								else if (strcmp(arg, "back_from_inv") == 0)
								{
									CWorldOutSideData.getInstance().VehicleData().getHoldData(7);
									arg = CWorldOutSideData.getInstance().MapData().getBeforeFieldMapName();
									CCastCommandTransit.getInstance().castParam_MapJump().m_Flag = false;
								}
								sceneMng.gotoStage(arg);
							}

							public void backupPosition(CBaseSystem sys)
							{
								VecFx32 vecFx = new VecFx32(0, 0, 0);
								VecFx32 vecFx2 = new VecFx32(0, 0, 0);
								for (int i = 0; (long)i < 24L; i++)
								{
									if (sys.PlayerMng().Player(i).getCharacterId() != -1)
									{
										vecFx.copy(sys.PlayerMng().Player(i).getPosition());
										vecFx2.copy(sys.PlayerMng().Player(i).getRotation());
										CWorldOutSideData.getInstance().PlayerData().setHoldData(i, vecFx, vecFx2);
									}
								}
							}

							public void backupVehiclePosition(CBaseSystem sys)
							{
								CWorldOutSideData.getInstance().VehicleData().setPreRidingOnVehicleNo(pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ERR);
								for (int i = 0; (long)i < 4L; i++)
								{
									if (sys.PlayerMng().PlayerVehicle(i).getCharacterId() == -1)
									{
										continue;
									}
									VecFx32 position = new VecFx32(sys.PlayerMng().PlayerVehicle(i).getPosition());
									VecFx32 rotation = new VecFx32(sys.PlayerMng().PlayerVehicle(i).getRotation());
									bool canBoard = sys.PlayerMng().PlayerVehicle(i).canBoard();
									int num = static_cast<int>(sys.PlayerMng().PlayerVehicle(i).getVehicleType());
									CWorldOutSideData.getInstance().VehicleData().setHoldData(num, sceneMng.getFieldNo(), position, rotation, canBoard);
									if (sys.PlayerMng().PlayerVehicle(i).getBoardPlayer() != null)
									{
										CWorldOutSideData.getInstance().VehicleData().setPreRidingOnVehicleNo(static_cast<pl.PLAYER_VEHICLE_TYPE>(num));
										pl.CPlayerVehicle cPlayerVehicle = sys.PlayerMng().PlayerVehicle(i);
										if (pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM == cPlayerVehicle.getVehicleType())
										{
											CWorldOutSideData.getInstance().VehicleData().setEnterpOnAir(cPlayerVehicle.isOnAir());
										}
										if (cPlayerVehicle.getVehicleType() == pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CHOKOBO)
										{
											CWorldOutSideData.getInstance().MapData().setRideOnChokobo(b: true);
										}
									}
								}
							}

							public void setupStage(CBaseSystem sys, string pStageName, CBaseSystem.WORLD_MODE mode)
							{
								int fieldNo = sceneMng.getFieldNo();
								if (2 == fieldNo && evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_CHAPTER_FLAG[3]) == 1)
								{
									string text = "";
									text = sceneMng.getPreStage();
									string text2 = "";
									char[] array = sceneMng.getStage().ToCharArray();
									array[2] = '3';
									text2 = new string(array);
									sceneMng.gotoStage(text);
									sceneMng.gotoStage(text2);
								}
								string arg;
								switch (mode)
								{
								case CBaseSystem.WORLD_MODE.WORLD_MODE_SHOP:
								case CBaseSystem.WORLD_MODE.WORLD_MODE_TALK:
									strcpy(out arg, pStageName);
									break;
								default:
									strcpy(out arg, sceneMng.getStage());
									sceneMng.setCommonMdlNo((sbyte)CWorldOutSideData.getInstance().MapData().getCommonMdlNo());
									if (strcmp("\0", sceneMng.getCommonMdl()) != 0 && strcmp("prev", sceneMng.getCommonMdl()) != 0)
									{
										strcpy(out arg, sceneMng.getCommonMdl());
										sceneMng.setCommonMdlNo(99);
									}
									break;
								}
								stageMng.setStage(arg);
								if (!map.CMapParameterManager.Instance().isLoaded() || sys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
								{
									return;
								}
								int num = map.CMapParameterManager.Instance().mapJumpNum();
								for (int i = 0; i < num; i++)
								{
									int num2 = map.CMapParameterManager.Instance().MapJumpParameter(i).Kind();
									if (800 <= num2 && FlagManager.singleton().get(0u, (uint)num2) == 1)
									{
										map.CMapParameterManager.Instance().MapJumpParameter(i).Kind_set(0);
									}
								}
								string arg2 = "";
								bool flag = false;
								short num3 = CWorldOutSideData.getInstance().MapData().NextMapIndex();
								int num4 = map.CMapParameterManager.Instance().MapJumpParameter(num3 - 1).Kind();
								if (num4 >= 0)
								{
									CWorldOutSideData.getInstance().MapData().setHoldDoorData(arg, _IsOpen: true, (sbyte)num3);
									sprintf(out arg2, "O%02d", num3);
									flag = true;
								}
								else
								{
									SHoldDoorData holdDoorData = CWorldOutSideData.getInstance().MapData().getHoldDoorData();
									if (holdDoorData.m_IsOpen && strcmp(arg, holdDoorData.m_MapName) == 0)
									{
										sprintf(out arg2, "O%02d", holdDoorData.m_MaterialIndex);
										flag = true;
									}
								}
								if (flag)
								{
									stageMng.setMaterialAlpha(arg2, 0u);
								}
							}
						}
	}
}
