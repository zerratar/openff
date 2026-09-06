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
	public static partial class wld
	{
						public class CStateWorldMove : CBaseState
						{
							private static uint PrintFrame;

							private sbyte jumpStopTime_;

							public override void start(CBaseSystem _sys)
							{
								changeCompanyDirectory();
								menu.MenuManager.getSingleton().CreateItemDataText();
								CBaseSystem.initNextMode();
								int playCharacterIndex = CWorldOutSideData.getInstance().PlayerData().getPlayCharacterIndex();
								pl.CPlayerCharacter cPlayerCharacter = static_cast<pl.CPlayerCharacter>(_sys.PlayerMng().Player(playCharacterIndex));
								if (_sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_MENU)
								{
									changeCompanyDirectory();
									menu.MenuManager.getSingleton().LoadXbnFile("WorldDefine.xbn");
									changeGlobalDirectory();
									cPlayerCharacter.InputPermission_set(arg0: true);
									_sys.PlayerMng().setPlayerStart(playCharacterIndex);
									G3X_SetClearColor(GX_RGB(0, 0, 0), 31, 32767, 1, 0);
									_sys.EnCountManager().SetFlag(b: true);
								}
								else if (_sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_SITE)
								{
									cPlayerCharacter.InputPermission_set(arg0: true);
								}
								else
								{
									_sys.EnCountManager().initialize();
									_sys.EnCountManager().SetFlag(b: true);
								}
								if (!evt.CEventManager.getInstance().isEvent())
								{
									pl.CBasePlayer cBasePlayer = _sys.PlayerMng().Player(chr.CBaseCharacter.getLookIndex());
									if (cBasePlayer.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN)
									{
										if (cBasePlayer.getNowAct() == 0 || cBasePlayer.getNowAct() == 1 || cBasePlayer.getNowAct() == 2)
										{
											cBasePlayer.isAutoPilot_set(arg0: false);
										}
									}
									else if (cBasePlayer.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE && (cBasePlayer.getNowAct() == 0 || cBasePlayer.getNowAct() == 1))
									{
										cBasePlayer.isAutoPilot_set(arg0: false);
									}
									if (!_sys.isEscape())
									{
										_sys.World2DMng().MenuStartButton().create();
										_sys.World2DMng().CameraButton().create();
										_sys.World2DMng().TalkButton().create();
										_sys.World2DMng().MenuStartButton().setStateShow();
										if (_sys.World2DMng().visibleMap())
										{
											_sys.World2DMng().CameraButton().setStateShow();
										}
										if (_sys.PlayerMng().PlayerHuman(0).getTalkIcon() != null)
										{
											_sys.World2DMng().TalkButton().setStateShow();
										}
									}
								}
								ds.g_TouchPanel.enable();
								ds.g_TouchPanel.update();
								dv.CDeviceManager.getInstance().Tp().initialize();
								dv.CDeviceManager.getInstance().Tp().setCamera(_sys.WorldCamera());
								ds.g_Pad.enable();
								ds.g_Pad.read();
								dv.CDeviceManager.getInstance().Pad().initialize();
								dv.CDeviceManager.getInstance().Pad().registerPad(ds.g_Pad);
								CWorldSystem cWorldSystem = (CWorldSystem)_sys;
								cWorldSystem.setplayerMngSleepTime(2);
								jumpStopTime_ = 1;
							}

							public override void update(CBaseSystem _sys)
							{
								if (!evt.CEventManager.getInstance().isEvent() && !evt.CEventManager.getInstance().isPartyTalkEvent() && !evt.CEventManager.getInstance().isItemEvent())
								{
									_sys.PlayerMng().checkTouchCollision();
								}
								if (map.CMapParameterManager.Instance().isLoaded() && !evt.CEventManager.getInstance().isEvent() && !evt.CEventManager.getInstance().isPartyTalkEvent() && !evt.CEventManager.getInstance().isItemEvent())
								{
									bool zoomEnable = false;
									if (!_sys.WorldCamera().IsCollision())
									{
										zoomEnable = map.CMapParameterManager.Instance().MapCameraParameter(0).ZoomOnOff() != 0;
									}
									_sys.WorldCamera().composit.setZoomEnable(zoomEnable);
								}
								if (jumpStopTime_ > 0)
								{
									jumpStopTime_--;
								}
								else
								{
									sendMapJump(_sys);
								}
								sendBattle(_sys);
								sendMenu(_sys);
								if (_sys.IsMapJump() | _sys.IsBattle() | _sys.IsMogNet() | _sys.IsTitle() | _sys.IsShop() | _sys.IsMenu() | _sys.IsTalk() | _sys.IsSave() | _sys.IsSpecial() | _sys.IsInn() | _sys.IsAreaMap())
								{
									setPhase(PHASE.END);
								}
							}

							public override void end(CBaseSystem _sys)
							{
								_sys.BackUpVehiclePosition();
								menu.MenuManager.getSingleton().ReleaseItemDataText();
								int playCharacterIndex = CWorldOutSideData.getInstance().PlayerData().getPlayCharacterIndex();
								if (!_sys.PlayerMng().Player(playCharacterIndex).isAutoPilot())
								{
									if (_sys.PlayerMng().Player(playCharacterIndex).CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN)
									{
										if (_sys.PlayerMng().Player(playCharacterIndex).getMotionIndex() != 1001)
										{
											_sys.PlayerMng().Player(playCharacterIndex).startMotion(1001, _Loop: true, 5u);
										}
									}
									else
									{
										_sys.PlayerMng().Player(playCharacterIndex).CharaKind();
										_ = 1;
									}
								}
								_sys.PlayerMng().setPlayerStop(playCharacterIndex);
								if (_sys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD && (_sys.IsMenu() || _sys.IsSave() || _sys.IsBattle() || _sys.IsTalk()))
								{
									string name = const_cast<string>(stageMng.getChipName());
									sceneMng.gotoStage(name);
								}
								if (_sys.IsMenu() || _sys.IsSave())
								{
									if (_sys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
									{
										string beforeFieldMapName = const_cast<string>(stageMng.getChipName());
										CWorldOutSideData.getInstance().MapData().setBeforeFieldMapName(beforeFieldMapName);
									}
									menu.MenuManager.getSingleton().ReleaseXbnFile();
									_sys.World2DMng().refWorldMap().hideMapMarker();
									_sys.World2DMng().terminate();
								}
								else if (!_sys.IsInn() && !_sys.IsAreaMap())
								{
									int playCharacterIndex2 = CWorldOutSideData.getInstance().PlayerData().getPlayCharacterIndex();
									int index = _sys.PlayerMng().Player(playCharacterIndex2).getLandFormIndex() - 1;
									int num = map.CMapParameterManager.Instance().MapLandFormParameter(0).BattleFieldIndex(index);
									CWorldOutSideData.getInstance().MapData().BattleMapIndex_set((sbyte)num);
									if (pl.PlayerParty.instance().npc().isEnable())
									{
										if (!_sys.PlayerMng().Player(1).isAutoPilot())
										{
											_sys.PlayerMng().Player(1).startMotion(1001, _Loop: true, 5u);
										}
										_sys.PlayerMng().setPlayerStop(1);
									}
									_sys.PlayerMng().setAllPlayerAutoPilot(_Flag: true);
									_sys.EnCountManager().terminate();
									short num2 = CWorldOutSideData.getInstance().MapData().MapJumpIndex();
									if (_sys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
									{
										if (CWorldOutSideData.getInstance().MapData().getSpMapType() != CMapData.SP_MAP_TYPE.SP_MAP_DEEPSEA && CWorldOutSideData.getInstance().MapData().getSpMapType() != CMapData.SP_MAP_TYPE.SP_MAP_AIR)
										{
											string beforeFieldMapName2 = const_cast<string>(stageMng.getChipName());
											CWorldOutSideData.getInstance().MapData().setBeforeFieldMapName(beforeFieldMapName2);
											CWorldOutSideData.getInstance().MapData().setBeforeFieldMapJumpIndex((sbyte)num2);
										}
									}
									else if (_sys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN)
									{
										string stage = sceneMng.getStage();
										CWorldOutSideData.getInstance().MapData().setBeforeTownMapName(stage);
										CWorldOutSideData.getInstance().MapData().setBeforeTownMapJumpIndex((sbyte)num2);
									}
								}
								_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_END);
								ds.g_Pad.enable();
							}

							public void sendMapJump(CBaseSystem _sys)
							{
								int playCharacterIndex = CWorldOutSideData.getInstance().PlayerData().getPlayCharacterIndex();
								if ((_sys.PlayerMng().Player(playCharacterIndex).CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN && (_sys.PlayerMng().Player(playCharacterIndex).getNowAct() == 11 || _sys.PlayerMng().Player(playCharacterIndex).getNowAct() == 12 || _sys.PlayerMng().Player(playCharacterIndex).getNextAct() == 11 || _sys.PlayerMng().Player(playCharacterIndex).getNextAct() == 12)) || evt.CEventManager.getInstance().isEvent())
								{
									return;
								}
								short num = CWorldOutSideData.getInstance().MapData().MapJumpIndex();
								if (num < 0)
								{
									return;
								}
								num--;
								int lookIndex = chr.CBaseCharacter.getLookIndex();
								if (_sys.PlayerMng().Player(lookIndex).CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
								{
									if (CMapData.SP_MAP_TYPE.SP_MAP_DEEPSEA == CWorldOutSideData.getInstance().MapData().getSpMapType() || CMapData.SP_MAP_TYPE.SP_MAP_AIR == CWorldOutSideData.getInstance().MapData().getSpMapType() || CMapData.SP_MAP_TYPE.SP_MAP_INVINSIBLE == CWorldOutSideData.getInstance().MapData().getSpMapType())
									{
										goto IL_020a;
									}
									if (num != 4 && num != 8 && num != 9 && num != 10)
									{
										return;
									}
								}
								if (CWorldOutSideData.getInstance().MapData().getSpMapType() != CMapData.SP_MAP_TYPE.SP_MAP_INVINSIBLE)
								{
									if (_sys.Mode() != CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
									{
										short num2 = (short)map.CMapParameterManager.Instance().MapJumpParameter(num).Kind();
										if (800 <= num2)
										{
											return;
										}
									}
									bool flag = true;
									short num3 = (short)map.CMapParameterManager.Instance().MapJumpParameter(num).ConditionFlag();
									if ((num3 & 1) != 0)
									{
										flag = false;
									}
									else if (flag)
									{
										if ((num3 & 2) != 0)
										{
											for (int i = 0; i < 4; i++)
											{
												if (pl.PlayerParty.instance().player((byte)i).isEnable())
												{
													if (!pl.PlayerParty.instance().player((byte)i).condition()
														.isLilliput())
													{
														flag = true;
														break;
													}
													flag = false;
												}
											}
										}
										else if ((num3 & 4) != 0)
										{
											for (int j = 0; j < 4; j++)
											{
												if (pl.PlayerParty.instance().player((byte)j).isEnable())
												{
													if (!pl.PlayerParty.instance().player((byte)j).condition()
														.isFrog())
													{
														flag = true;
														break;
													}
													flag = false;
												}
											}
										}
									}
									if (flag)
									{
										return;
									}
									openTheDoor(_sys);
								}
								goto IL_020a;
								IL_020a:
								_sys.setMapJump(b: true);
								if ((long)chr.CBaseCharacter.getLookIndex() < 24L)
								{
									pl.CPlayerHuman cPlayerHuman = _sys.PlayerMng().PlayerHuman(chr.CBaseCharacter.getLookIndex());
									cPlayerHuman.setNextAct(0);
									cPlayerHuman.setNowAct(0);
									cPlayerHuman.InputPermission_set(arg0: false);
								}
								else
								{
									pl.CPlayerVehicle cPlayerVehicle = _sys.PlayerMng().PlayerVehicle((int)((long)chr.CBaseCharacter.getLookIndex() - 24L));
									cPlayerVehicle.setNextAct(0);
									cPlayerVehicle.setNowAct(0);
									cPlayerVehicle.InputPermission_set(arg0: false);
								}
								_sys.World2DMng().refMapNameWindow().close();
								CWorldOutSideData.getInstance().MapData().isColFlag_not_and(2048);
							}

							public void sendBattle(CBaseSystem _sys)
							{
								int playCharacterIndex = CWorldOutSideData.getInstance().PlayerData().getPlayCharacterIndex();
								pl.CPlayerCharacter cPlayerCharacter = (pl.CPlayerCharacter)_sys.PlayerMng().Player(playCharacterIndex);
								chr.CHARACTER_KIND cHARACTER_KIND = cPlayerCharacter.CharaKind();
								if (playCharacterIndex < 0 || 28L <= (long)playCharacterIndex || chr.CHARACTER_KIND.CHARACTER_KIND_ERR == cHARACTER_KIND || (0 <= playCharacterIndex && (long)playCharacterIndex < 24L && cHARACTER_KIND != chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN) || (24L <= (long)playCharacterIndex && (long)playCharacterIndex < 28L && chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE != cHARACTER_KIND) || _sys.IsShop() || _sys.IsTalk() || _sys.IsMenu() || _sys.IsAreaMap() || _sys.IsMapJump() || _sys.IsTitle() || _sys.IsSave() || _sys.IsSpecial() || evt.CEventManager.getInstance().isEvent() || _sys.World2DMng().MessageWindow().isMadeWindow())
								{
									return;
								}
								if (cPlayerCharacter.isAutoPilot())
								{
									_sys.EnCountManager().SetFlag(b: false);
								}
								else
								{
									if (!cPlayerCharacter.canEncount() || !CWorldOutSideData.getInstance().canEncount())
									{
										return;
									}
									_sys.EnCountManager().SetFlag(CWorldOutSideData.getInstance().canEncount());
									_sys.EnCountManager().setForceEncount(CWorldOutSideData.getInstance().isForceEncount());
									if (_sys.EnCountManager().checkEncount(cPlayerCharacter) && _sys.EnCountManager().setMonsterPartyId(cPlayerCharacter))
									{
										if (_sys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
										{
											sceneMng.gotoStage(stageMng.getChipName());
										}
										card.SaveSuspend();
										_sys.EnCountManager().setBattleField(cPlayerCharacter);
										CBaseSystem.setBattle(b: true);
										cPlayerCharacter.InputPermission_set(arg0: false);
										cPlayerCharacter.getParamMove().init();
										cPlayerCharacter.getParamTurn().init();
										cPlayerCharacter.MoveSys().setStop(b: true);
										cPlayerCharacter.MoveSys().setTargetPoint(0, 0, 0, 0);
										cPlayerCharacter.MoveSys().setTargetPoint(1, 0, 0, 0);
										cPlayerCharacter.TurnSys().setStop(b: true);
										evt.CEventManager.getInstance().FlagMng().set(0u, 986u);
										_sys.World2DMng().refMapNameWindow().close();
									}
								}
							}

							public void sendMenu(CBaseSystem _sys)
							{
								int playCharacterIndex = CWorldOutSideData.getInstance().PlayerData().getPlayCharacterIndex();
								if (_sys.IsBattle() || _sys.IsShop() || _sys.IsTalk() || _sys.IsTitle() || _sys.IsMapJump() || _sys.IsSave() || _sys.IsSpecial() || evt.CEventManager.getInstance().isEvent() || _sys.World2DMng().ItemUseMenuManager().isItemMenu() || _sys.PlayerMng().Player(playCharacterIndex).isAutoPilot() || !_sys.PlayerMng().Player(playCharacterIndex).canOpenMenu())
								{
									return;
								}
								ushort num = 0;
								num = (ushort)((opt.COptionManager.getSingleton().gameOption().menuZoomSetting() != opt.MENU_ZOOM_SETTING.MENU_R_ZOOM_L) ? 512 : 256);
								if (isAutoSave())
								{
									if (_sys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
									{
										sceneMng.gotoStage(stageMng.getChipName());
									}
									card.SaveSuspend();
									setAutoSave(b: false);
								}
								// PORT: FF3's menu reads FF3's party, jobs and face cells; on FF4 the engine draws
								// its own status menu (Ff4Menu) from the unified party instead.
								if (!OpenFF.Client.GameProfile.IsFf4 && ((ds.g_Pad.edge() & 0x400) != 0 || (ds.g_Pad.edge() & num) != 0 || _sys.World2DMng().MenuStartButton().isTouch()))
								{
									_sys.World2DMng().refMapNameWindow().close();
									MatrixSound.MtxSENDS_Play(0, 1, 192, 127);
									ds.g_Pad.disable();
									_sys.setMenu(b: true);
									if ((long)chr.CBaseCharacter.getLookIndex() < 24L)
									{
										pl.CPlayerHuman cPlayerHuman = _sys.PlayerMng().PlayerHuman(chr.CBaseCharacter.getLookIndex());
										cPlayerHuman.setNextAct(0);
										cPlayerHuman.setNowAct(0);
										cPlayerHuman.InputPermission_set(arg0: false);
									}
									else
									{
										pl.CPlayerVehicle cPlayerVehicle = _sys.PlayerMng().PlayerVehicle((int)((long)chr.CBaseCharacter.getLookIndex() - 24L));
										cPlayerVehicle.setNextAct(0);
										cPlayerVehicle.InputPermission_set(arg0: false);
										cPlayerVehicle.startMotion(cPlayerVehicle.getWaitMotionIndex(), _Loop: true, 5u);
									}
								}
								if (_sys.World2DMng().CameraButton().isTouch())
								{
									_sys.World2DMng().refMapNameWindow().close();
									MatrixSound.MtxSENDS_Play(0, 1, 192, 127);
									_sys.setAreaMap(b: true);
									_sys.World2DMng().setButtonShow(show: false);
									if ((long)chr.CBaseCharacter.getLookIndex() < 24L)
									{
										pl.CPlayerHuman cPlayerHuman2 = _sys.PlayerMng().PlayerHuman(chr.CBaseCharacter.getLookIndex());
										cPlayerHuman2.setNextAct(0);
										cPlayerHuman2.setNowAct(0);
										cPlayerHuman2.InputPermission_set(arg0: false);
									}
									else
									{
										pl.CPlayerVehicle cPlayerVehicle2 = _sys.PlayerMng().PlayerVehicle((int)((long)chr.CBaseCharacter.getLookIndex() - 24L));
										cPlayerVehicle2.setNextAct(0);
										cPlayerVehicle2.InputPermission_set(arg0: false);
										cPlayerVehicle2.startMotion(cPlayerVehicle2.getWaitMotionIndex(), _Loop: true, 5u);
									}
								}
							}

							public void openTheDoor(CBaseSystem _sys)
							{
								if (_sys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
								{
									return;
								}
								short num = CWorldOutSideData.getInstance().MapData().MapJumpIndex();
								num--;
								int num2 = map.CMapParameterManager.Instance().MapJumpParameter(num).Kind();
								if (num2 < 0)
								{
									return;
								}
								string arg = "";
								string arg2 = "";
								strcpy(out arg, CWorldOutSideData.getInstance().MapData().getHoldDoorData()
									.m_MapName);
									strcpy(out arg2, sceneMng.getStage());
									if (strcmp(arg, arg2) == 0 && CWorldOutSideData.getInstance().MapData().getHoldDoorData()
										.m_MaterialIndex == num + 1 && CWorldOutSideData.getInstance().MapData().getHoldDoorData()
										.m_IsOpen)
									{
										return;
									}
									CWorldOutSideData.getInstance().MapData().setHoldDoorData(sceneMng.getStage(), _IsOpen: true, (sbyte)(num + 1));
									short[] array = new short[1] { 24 };
									sprintf(out var arg3, "O%02d", num + 1);
									stageMng.setMaterialAlpha(arg3, 0u);
									if (num2 >= 0 && 2 >= num2)
									{
										switch (num2)
										{
										case 2:
											MatrixSound.MtxSENDS_Play(1, 29, 192, 127);
											break;
										case 1:
											MatrixSound.MtxSENDS_Play(1, 28, 192, 127);
											break;
										default:
											MatrixSound.MtxSENDS_Play(1, array[num2], 192, 127);
											break;
										}
									}
								}

								public void changePartyMember(CBaseSystem _sys)
								{
									int playCharacterIndex = CWorldOutSideData.getInstance().PlayerData().getPlayCharacterIndex();
									pl.CBasePlayer cBasePlayer = _sys.PlayerMng().Player(playCharacterIndex);
									if (cBasePlayer == null || (-1 < cBasePlayer.getTransparency() && cBasePlayer.getTransparency() < 31) || evt.CEventManager.getInstance().isEvent() || pl.PlayerParty.instance().aliveNumber() == 1 || cBasePlayer.CharaKind() != chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN || (ds.g_Pad.edge() & 4) == 0)
									{
										return;
									}
									int i = CWorldOutSideData.getInstance().PlayerData().getFrontPlayerID() + 1;
									if (pl.PlayerParty.instance().aliveNumber() <= i)
									{
										i = 0;
									}
									for (; i < 4 && !pl.PlayerParty.instance().player((byte)i).isEnable(); i++)
									{
									}
									if (i < 4)
									{
										CWorldOutSideData.getInstance().PlayerData().setFrontPlayerID(i);
										int nowAct = cBasePlayer.getNowAct();
										chr.CHARA_OBJECT arg = new chr.CHARA_OBJECT(cBasePlayer.getParamObj());
										characterMng.delCharacter(cBasePlayer.getCharacterId());
										cBasePlayer.setCharacterId(-1);
										int num = _sys.setupHero();
										pl.CBasePlayer cBasePlayer2 = _sys.PlayerMng().Player(num);
										cBasePlayer2.into();
										cBasePlayer2.setMCLCol(b: true);
										cBasePlayer2.getColFlag_set(cBasePlayer.getColFlag());
										cBasePlayer2.getColType_set(cBasePlayer.getColType());
										cBasePlayer2.setTransparencyRate(0);
										cBasePlayer2.setShadowAlpha(0);
										cBasePlayer2.setSucAlpha(100);
										cBasePlayer2.setAutoAlphaFrame(6);
										cBasePlayer2.setWorkAutoAlphaFrame(0);
										cBasePlayer2.setSucShadowAlpha(8);
										cBasePlayer2.setAutoShadowAlphaFrame(6);
										cBasePlayer2.setWorkAutoShadowAlphaFrame(0);
										cBasePlayer2.setAutoPilot(_AutoPilot: false);
										cBasePlayer2.setNextAct(nowAct);
										cBasePlayer2.getParamObj_set(arg);
										chr.CBaseCharacter.setLookIndex(num);
										CWorldOutSideData.getInstance().PlayerData().setPlayCharacterIndex(num);
										if (pl.PlayerParty.instance().npc().isEnable() && pl.PlayerParty.instance().npc().npcId() == _sys.npcId())
										{
											_sys.PlayerMng().PlayerHuman(_sys.npcEntryId()).NPCAiManager()
												.NPC()
												.setLookPlayer(_sys.PlayerMng().Player(num));
										}
									}
								}

								public override bool canExecuteEvent(CBaseSystem sys)
								{
									if (m_phase == PHASE.UPDATE && !sys.IsMenu() && !sys.IsAreaMap() && !sys.IsTalk())
									{
										return !sys.IsBattle();
									}
									return false;
								}

								public void testCommand(CBaseSystem _sys)
								{
								}

								public void debugCommand(CBaseSystem _sys)
								{
									if ((ds.g_Pad.edge() & 4) != 0)
									{
										CWorldOutSideData.getInstance().setCanEncount(!CWorldOutSideData.getInstance().canEncount());
									}
								}

								public void debugPrintf(CBaseSystem _sys)
								{
									PrintFrame++;
									if (PrintFrame >= 30)
									{
										PrintFrame = 0u;
									}
								}
							}
	}
}
