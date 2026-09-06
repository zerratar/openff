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
							public class CWorldSystem : CBaseSystem
							{
								protected sbyte playerMngSleepTime_;

								protected ds.Vector<CParallelWorldSystem, ds.FastErasePolicy<CParallelWorldSystem>> vecPWorldSys = new ds.Vector<CParallelWorldSystem, ds.FastErasePolicy<CParallelWorldSystem>>(4);

								protected bool talkIconVisible_;

								public void destruct()
								{
								}

								public void appendPWS(CParallelWorldSystem pPWS)
								{
									vecPWorldSys.push_back(pPWS);
									pPWS.initialize(this);
								}

								public void removePWS(CParallelWorldSystem pPWS)
								{
									for (int num = vecPWorldSys.size() - 1; num >= 0; num--)
									{
										if (vecPWorldSys[num] == pPWS)
										{
											vecPWorldSys.erase(num);
											pPWS.terminate(this);
										}
									}
								}

								public bool isPWSEmpty()
								{
									return vecPWorldSys.empty();
								}

								public override void initialize()
								{
									setState(WORLD_STATE.WORLD_STATE_START);
									CrtState().phase_set(CBaseState.PHASE.START);
									if (!IsShop() && !IsTalk())
									{
										setMapJump(b: false);
										CBaseSystem.setBattle(b: false);
										setShop(b: false);
										setMenu(b: false);
										setTalk(b: false);
										setMogNet(b: false);
										CBaseSystem.setTitle(b: false);
										setEnd(_End: false);
										setSave(b: false);
										setSpecial(b: false);
										setInn(b: false);
									}
									vecPWorldSys.clear();
									CCastCommandTransit.getInstance().initialize();
									CCastCommandTransit.getInstance().setCast_BaseSystem(this);
									setEnd(_End: false);
									playerMngSleepTime_ = 0;
									talkIconVisible_ = false;
								}

								public override void execute()
								{
									Mode();
									_ = -1;
									bool flag = CrtState().canExecuteEvent(this);
									bool flag2 = false;
									if (CrtState().phase() == CBaseState.PHASE.START)
									{
										CrtState().start(this);
										CrtState().setPhase(CBaseState.PHASE.UPDATE);
										flag2 = true;
									}
									if ((Mode() == WORLD_MODE.WORLD_MODE_FIELD || Mode() == WORLD_MODE.WORLD_MODE_TOWN || Mode() == WORLD_MODE.WORLD_MODE_TALK || Mode() == WORLD_MODE.WORLD_MODE_SITE || Mode() == WORLD_MODE.WORLD_MODE_MENU) && flag)
									{
										evt.CEventManager.getInstance().execute();
									}
									if (CrtState().phase() == CBaseState.PHASE.UPDATE && !flag2)
									{
										CrtState().update(this);
									}
									else if (CrtState().phase() == CBaseState.PHASE.END)
									{
										CrtState().end(this);
										CrtState().setPhase(CBaseState.PHASE.START);
									}
									for (int num = vecPWorldSys.size() - 1; num >= 0; num--)
									{
										if (!vecPWorldSys[num].execute(this))
										{
											vecPWorldSys[num].terminate(this);
											vecPWorldSys.erase(num);
										}
									}
									dv.CDeviceManager.getInstance().execute();
									if (Mode() == WORLD_MODE.WORLD_MODE_FIELD || Mode() == WORLD_MODE.WORLD_MODE_TOWN || Mode() == WORLD_MODE.WORLD_MODE_TALK || Mode() == WORLD_MODE.WORLD_MODE_MENU || Mode() == WORLD_MODE.WORLD_MODE_SITE || Mode() == WORLD_MODE.WORLD_MODE_INN)
									{
										World2DMng().execute();
										if (canRunPlayerMng())
										{
											PlayerMng().execute();
										}
										wbc_.wbcExecute();
										woc_.wocExecute();
										swMng_.execute(PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getPosition());
										if (canRunPlayerMng() && !evt.CEventManager.getInstance().isEvent())
										{
											OpenFF.Client.Ff4Exits.Update(PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getPosition(), this);
										}
										if ((WorldCamera().Mode() == cmr.CWorldCamera.MODE.MODE_AUTOFOLLOW_DEFAULT || WorldCamera().Mode() == cmr.CWorldCamera.MODE.MODE_AUTOFOLLOW) && PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getCharacterId() != -1)
										{
											WorldCamera().setSucTrg(PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getPosition());
										}
									}
									TPData dispPoint = dv.CDeviceManager.getInstance().Tp().getDispPoint();
									sys2d.Sprite3d[] array = World2DMng().PadButton();
									array[0].SetShow(show: false);
									array[1].SetShow(show: false);
									if ((Mode() == WORLD_MODE.WORLD_MODE_FIELD || Mode() == WORLD_MODE.WORLD_MODE_TOWN) && canRunPlayerMng() && !evt.CEventManager.getInstance().isEvent() && dispPoint.drag != 0 && PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getNowAct() != 11 && PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getNowAct() != 12)
									{
										int num2 = dispPoint.x - dispPoint.dragX;
										int num3 = dispPoint.y - dispPoint.dragY;
										int num4 = (int)sqrt(num2 * num2 + num3 * num3);
										if (num4 > 32)
										{
											num2 = num2 * 32 / num4;
											num3 = num3 * 32 / num4;
										}
										array[0].SetShow(show: true);
										array[0].SetPositionI(dispPoint.dragX, dispPoint.dragY);
										array[1].SetShow(show: true);
										array[1].SetPositionI(dispPoint.dragX + num2, dispPoint.dragY + num3);
									}
									int num5 = 0;
									pl.CBasePlayer cBasePlayer = null;
									if ((Mode() == WORLD_MODE.WORLD_MODE_FIELD || Mode() == WORLD_MODE.WORLD_MODE_TOWN) && canRunPlayerMng() && !evt.CEventManager.getInstance().isEvent())
									{
										cBasePlayer = PlayerMng().Player(chr.CBaseCharacter.getLookIndex());
										if (cBasePlayer.CharaKind() != chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
										{
											int num6 = CWorldOutSideData.getInstance().MapData().MapJumpIndex();
											if (0 <= num6 && pl.canKeyDoorByDirection(cBasePlayer))
											{
												num6--;
												map.CMapJumpParameter cMapJumpParameter = map.CMapParameterManager.Instance().MapJumpParameter(num6);
												if (cMapJumpParameter != null && cMapJumpParameter.Kind() >= 800)
												{
													num5 = 1;
												}
											}
											chr.CCharacterEureka target = cBasePlayer.getTarget();
											if ((cBasePlayer.getColType() & 2) != 0 && target != null && target.getCharacterId() != -1)
											{
												num5 = 1;
												lastLogic_set((int)target.LogicIndex());
												if (target.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_MAP_OBJECT)
												{
													map.CMapObject cMapObject = (map.CMapObject)target;
													if (cMapObject.MapObjType() == map.MAP_OBJECT_TYPE.TREASURE_BOX && FlagManager.singleton().get(cMapObject.getFlagGroup(), cMapObject.getFlagIndex()) != 0)
													{
														num5 = 0;
													}
													map.MapSignEffect signEffect = cMapObject.getSignEffect();
													if (signEffect != null && !signEffect.checkVisible())
													{
														num5 = 0;
													}
												}
												if (target.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
												{
													pl.CPlayerVehicle cPlayerVehicle = (pl.CPlayerVehicle)target;
													switch (cPlayerVehicle.getVehicleType())
													{
													case pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CHOKOBO:
													case pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP:
													case pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM:
														num5 = 0;
														break;
													}
												}
												string nowMapName = CWorldOutSideData.getInstance().MapData().getNowMapName();
												int num7 = (int)target.LogicIndex();
												if (num7 == 15 && strcmp(nowMapName, "d06_01") == 0 && FlagManager.singleton().get(0u, 85u) == 0)
												{
													num5 = 0;
												}
												if (num7 == 14 && strcmp(nowMapName, "t14_02") == 0)
												{
													num5 = 0;
												}
												if (num7 == 11 && strcmp(nowMapName, "d11_01") == 0 && FlagManager.singleton().get(0u, 212u) != 0)
												{
													num5 = 0;
												}
												if (num7 == 32 && strcmp(nowMapName, "f03_01") == 0 && FlagManager.singleton().get(0u, 263u) == 0)
												{
													num5 = 0;
												}
												if (num7 == 31 && strcmp(nowMapName, "f03_01") == 0)
												{
													num5 = 0;
												}
												if (num7 == 25 && strcmp(nowMapName, "d18_01") == 0 && FlagManager.singleton().get(0u, 320u) == 0)
												{
													num5 = 0;
												}
											}
										}
										else if (cBasePlayer.checkJump12())
										{
											num5 = 1;
										}
									}
									sys2d.Sprite3d sprite3d = World2DMng().refTalkIcon();
									if (num5 != 0 && talkIconVisible_)
									{
										VecFx32 wld_reuse_v = wld_reuse_v0;
										wld_reuse_v.copy(cBasePlayer.getPosition());
										wld_reuse_v.y += 65536;
										NNS_G3dWorldPosToScrPos(wld_reuse_v, out var px, out var py);
										sprite3d.SetShow(show: true);
										sprite3d.SetPositionI(px, py);
									}
									else
									{
										sprite3d.SetShow(show: false);
									}
									talkIconVisible_ = num5 != 0;
									bool operateZoom = false;
									if (isEscape())
									{
										WorldCamera().composit.setZoomEnable(b: false);
									}
									if ((Mode() == WORLD_MODE.WORLD_MODE_FIELD || Mode() == WORLD_MODE.WORLD_MODE_TOWN) && canRunPlayerMng() && !evt.CEventManager.getInstance().isEvent() && dispPoint.state == TPState.TP_PINCH && PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getNowAct() != 11 && PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getNowAct() != 12)
									{
										int num8 = WorldCamera().composit.ZoomMin();
										int num9 = WorldCamera().composit.ZoomMax();
										WorldCamera().composit.Pinch_add(dispPoint.pinch * (num9 - num8) / (isIPad() ? 64 : 128));
										WorldCamera().composit.ZoomState_set(cmr.CCameraZoom.ZOOM_STATE.ZOOM_PINCH);
									}
									else
									{
										WorldCamera().composit.Pinch_set(WorldCamera().composit.Zoom());
									}
									WorldCamera().setOperateZoom(operateZoom);
									WorldCamera().execute();
									if ((ds.g_Pad.edge() & 0x2000) != 0)
									{
										NNS_GfdDumpLnkTexVramManager();
										NNS_GfdDumpLnkPlttVramManager();
									}
								}

								public override void terminate()
								{
									if (Mode() == WORLD_MODE.WORLD_MODE_ERR)
									{
									}
									while (!vecPWorldSys.empty())
									{
										vecPWorldSys[0].terminate(this);
										vecPWorldSys.erase(0);
									}
									dv.CDeviceManager.getInstance().terminate();
									evt.CEventManager.getInstance().terminate();
									PlayerMng().terminate();
									WorldCamera().terminate();
									World2DMng().terminate();
									stageMng.delStage();
									cleanup();
									CCastCommandTransit.getInstance().terminate();
								}

								public override void update()
								{
									Mode();
									_ = -1;
									if (Mode() == WORLD_MODE.WORLD_MODE_FIELD || Mode() == WORLD_MODE.WORLD_MODE_TOWN || Mode() == WORLD_MODE.WORLD_MODE_TALK || Mode() == WORLD_MODE.WORLD_MODE_MENU || Mode() == WORLD_MODE.WORLD_MODE_SITE || Mode() == WORLD_MODE.WORLD_MODE_INN)
									{
										if (canRunPlayerMng())
										{
											PlayerMng().update();
										}
										if (!evt.CEventManager.getInstance().isEvent())
										{
											PlayerMng().checkPCCollision();
										}
										if (stageMng.getLoopEnable())
										{
											VecFx32 wld_reuse_v = wld_reuse_v0;
											VecFx32 wld_reuse_v2 = wld_reuse_v1;
											VecFx32 wld_reuse_v3 = wld.wld_reuse_v2;
											wld_reuse_v.copy(PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getPosition());
											wld_reuse_v2.copy(WorldCamera().Pos());
											wld_reuse_v3.copy(WorldCamera().Trg());
											VecFx32 edgeMax = stageMng.getEdgeMax();
											VecFx32 edgeMin = stageMng.getEdgeMin();
											VecFx32 size = stageMng.getSize();
											VecFx32 wld_reuse_v4 = wld.wld_reuse_v3;
											wld_reuse_v4.set(0, 0, 0);
											if (edgeMax.x < wld_reuse_v.x)
											{
												wld_reuse_v4.x = -size.x;
											}
											else if (edgeMin.x > wld_reuse_v.x)
											{
												wld_reuse_v4.x = size.x;
											}
											if (edgeMax.z < wld_reuse_v.z)
											{
												wld_reuse_v4.z = -size.z;
											}
											else if (edgeMin.z > wld_reuse_v.z)
											{
												wld_reuse_v4.z = size.z;
											}
											wld_reuse_v.x += wld_reuse_v4.x;
											wld_reuse_v.z += wld_reuse_v4.z;
											wld_reuse_v2.x += wld_reuse_v4.x;
											wld_reuse_v2.z += wld_reuse_v4.z;
											wld_reuse_v3.x += wld_reuse_v4.x;
											wld_reuse_v3.z += wld_reuse_v4.z;
											PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).setPosition(wld_reuse_v);
											WorldCamera().setPos(wld_reuse_v2);
											WorldCamera().setTrg(wld_reuse_v3);
										}
										if (stg.STAGE_TYPE.STAGE_TYPE_FIELD01 == stageMng.getStageType() || stg.STAGE_TYPE.STAGE_TYPE_FIELD02 == stageMng.getStageType() || stg.STAGE_TYPE.STAGE_TYPE_FIELD03 == stageMng.getStageType() || stg.STAGE_TYPE.STAGE_TYPE_FIELD04 == stageMng.getStageType())
										{
											if (!evt.CEventManager.getInstance().isEvent() || PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getCharacterId() >= 0)
											{
												PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).setGrv(_GrvFlag: false);
												VecFx32 position = PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getPosition();
												stageMng.setFootPos(position);
											}
											if (stageMng.isChipChanged())
											{
												map.CMapParameterManager.Instance().fieldBlockParameter(null);
											}
										}
										dgs.Restrict();
									}
									WorldCamera().update();
								}

								public bool canRunPlayerMng()
								{
									if (playerMngSleepTime_ > 0)
									{
										if (--playerMngSleepTime_ <= 0)
										{
											return true;
										}
										return false;
									}
									if (isEscape())
									{
										return false;
									}
									if (State() == WORLD_STATE.WORLD_STATE_START)
									{
										return true;
									}
									if (State() == WORLD_STATE.WORLD_STATE_MOVE && (CrtState().phase() == CBaseState.PHASE.START || CrtState().phase() == CBaseState.PHASE.UPDATE))
									{
										return true;
									}
									return false;
								}

								public void setplayerMngSleepTime(sbyte time)
								{
									playerMngSleepTime_ = time;
								}
							}
	}
}
