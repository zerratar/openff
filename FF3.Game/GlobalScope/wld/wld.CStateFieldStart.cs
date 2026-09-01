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
							public class CStateFieldStart : CBaseState
							{
								public override void start(CBaseSystem _sys)
								{
									if (!UserInfo.isTrial() && FlagManager.singleton().get(0u, 903u) == 1)
									{
										UserInfo.AwardAchievement(0);
									}
									ds.g_Pad.disable();
									ds.g_TouchPanel.disable();
									GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_SUB_MAIN);
									G3X_SetClearColor(GX_RGB(0, 0, 0), 1, 32767, 1, 0);
									evt.CEventManager.getInstance().initializeLogic();
									_sys.PlayerMng().initialize();
									_sys.World2DMng().setWldMode((int)_sys.Mode());
									_sys.World2DMng().initialize();
									dv.CDeviceManager.getInstance().Tp().setCamera(_sys.WorldCamera());
									strncpy(out var arg, sceneMng.getStage(), 3);
									sprintf(out arg, "%s_01", arg);
									CWorldOutSideData.getInstance().MapData().setNowMapName(arg);
									if (sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_BATTLE)
									{
										string text = "";
										text = sceneMng.getPreStage();
										string text2 = "";
										text2 = CWorldOutSideData.getInstance().MapData().getBeforeFieldMapName();
										sceneMng.gotoStage(text);
										sceneMng.gotoStage(text2);
									}
									_sys.setupStage(sceneMng.getStage(), CBaseSystem.WORLD_MODE.WORLD_MODE_ERR);
									_sys.setup();
									_sys.setupHero();
									int num = 0;
									if (sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_BATTLE || sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_MOG_NET || sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_LOAD || sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_SUSPEND_LOAD || _sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_SHOP || _sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_MENU || (_sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_TALK && !CCastCommandTransit.getInstance().castParam_MapJump().m_Flag))
									{
										_sys.setupBackUpPosition();
									}
									else
									{
										num = CCastCommandTransit.getInstance().castParam_MapJump().m_CharacterType;
										if (num == 3)
										{
											CWorldOutSideData.getInstance().MapData().setRideOnChokobo(b: true);
										}
										_sys.setupMapJumpPosition();
									}
									map.CMapParameterManager.Instance().fieldBlockParameter(null);
									_sys.setupComradeNPC();
									_sys.setupVehicle();
									_sys.setupCamera();
									evt.CEventManager.getInstance().initializeValue();
									TexDivideLoader.getSingleton().tdlForceLoad();
									if (num >= 3)
									{
										for (int i = 24; i < pl.FIELD_CHARACTER_NUM; i++)
										{
											pl.CPlayerVehicle cPlayerVehicle = (pl.CPlayerVehicle)_sys.PlayerMng().Player(i);
											pl.PLAYER_VEHICLE_TYPE pLAYER_VEHICLE_TYPE = (pl.PLAYER_VEHICLE_TYPE)(num - 3);
											if (cPlayerVehicle.getVehicleType() == pLAYER_VEHICLE_TYPE)
											{
												_sys.PlayerMng().Player(0).setTarget(_sys.PlayerMng().Player(i));
												CWorldOutSideData.getInstance().VehicleData().setPreRidingOnVehicleNo(pLAYER_VEHICLE_TYPE);
												break;
											}
										}
									}
									_sys.RidePlayerOnVehicle();
									CWorldOutSideData.getInstance().initialize2();
									if (!_sys.getAreaChangeShutterFlag())
									{
										AreaChange.getInstance().setOpenStrong();
										dgs.CFade.Main().fadeIn(15);
										_sys.setAreaChangeShutterFlag(flag: true);
									}
									else if (sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_BATTLE)
									{
										AreaChange.getInstance().setOpenStrong();
										dgs.CFade.Main().fadeIn(15);
									}
									else if (_sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
									{
										AreaChange.getInstance().setCloseStrong();
										AreaChange.getInstance().setOpen(15, AreaChange.SWITCH_TYPE.SWITCH_TYPE_CENTER);
										dgs.CFade.Main().fadeIn(1);
										dgs.CFade.Sub().fadeIn(1);
									}
									else if (_sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN)
									{
										AreaChange.getInstance().setCloseStrong();
										AreaChange.getInstance().setOpen(15, AreaChange.SWITCH_TYPE.SWITCH_TYPE_CENTER);
										dgs.CFade.Main().fadeIn(1);
										dgs.CFade.Sub().fadeIn(1);
									}
									else
									{
										AreaChange.getInstance().setOpenStrong();
										dgs.CFade.Main().fadeIn(15);
									}
									dgs.CFade.Sub().fadeIn(15);
									_sys.changePlayerCharDisplay();
									stageMng.initFootPos(_sys.PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getPosition());
									map.CMapParameterManager.Instance().fieldBlockParameter(null);
									_sys.setUpMapSound();
									if (sys.GGlobal.getPreviousPart() != GAMEPART.GAMEPART_BATTLE)
									{
										int soundFlag = CWorldOutSideData.getInstance().SoundData().getSoundFlag();
										if ((soundFlag & 1) != 0)
										{
											MatrixSound.MtxSENDS_Play(1, 0, 192, 127);
											return;
										}
										soundFlag |= 1;
										CWorldOutSideData.getInstance().SoundData().setSoundFlag(soundFlag);
									}
								}

								public override void update(CBaseSystem _sys)
								{
									if (sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_BATTLE)
									{
										if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
										{
											setPhase(PHASE.END);
										}
									}
									else if (_sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD || _sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN)
									{
										if (AreaChange.getInstance().isOpened())
										{
											setPhase(PHASE.END);
										}
									}
									else if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
									{
										setPhase(PHASE.END);
									}
								}

								public override void end(CBaseSystem _sys)
								{
									_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_MOVE);
								}

								public override bool canExecuteEvent(CBaseSystem arg0)
								{
									return true;
								}
							}
	}
}
