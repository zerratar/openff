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
							public class CStateTownStart : CBaseState
							{
								public override void start(CBaseSystem _sys)
								{
									if (!UserInfo.isTrial() && FlagManager.singleton().get(0u, 903u) == 1)
									{
										UserInfo.AwardAchievement(0);
									}
									ds.g_Pad.disable();
									ds.g_TouchPanel.disable();
									int num = 0;
									GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_SUB_MAIN);
									G3X_SetClearColor(GX_RGB(0, 0, 0), 1, 32767, 1, 0);
									num = OS_GetTick();
									evt.CEventManager.getInstance().initializeLogic();
									OS_GetTick();
									num = OS_GetTick();
									_sys.PlayerMng().initialize();
									OS_GetTick();
									num = OS_GetTick();
									_sys.World2DMng().setWldMode((int)_sys.Mode());
									_sys.World2DMng().initialize();
									OS_GetTick();
									dv.CDeviceManager.getInstance().Tp().setCamera(_sys.WorldCamera());
									CWorldOutSideData.getInstance().MapData().setNowMapName(sceneMng.getStage());
									num = OS_GetTick();
									_sys.setup();
									OS_GetTick();
									num = OS_GetTick();
									_sys.setupStage(sceneMng.getStage(), CBaseSystem.WORLD_MODE.WORLD_MODE_ERR);
									_sys.setUpMapSecretWay();
									OS_GetTick();
									LegendarySmith legendarySmith = new LegendarySmith();
									legendarySmith.lottery(sceneMng.getStage(), sys.GGlobal.getPreviousPart());
									num = OS_GetTick();
									_sys.setupHero();
									OS_GetTick();
									if (sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_BATTLE || sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_MOG_NET || sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_LOAD || sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_SUSPEND_LOAD || sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_SPECIAL || _sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_SHOP || _sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_MENU || (_sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_TALK && !CCastCommandTransit.getInstance().castParam_MapJump().m_Flag))
									{
										_sys.setupBackUpPosition();
									}
									else
									{
										_sys.setupMapJumpPosition();
									}
									_sys.setupComradeNPC();
									_sys.setupCamera();
									evt.CEventManager.getInstance().initializeValue();
									TexDivideLoader.getSingleton().tdlForceLoad();
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
										AreaChange.getInstance().setOpen(15, AreaChange.SWITCH_TYPE.SWITCH_TYPE_OUTER);
										dgs.CFade.Main().fadeIn(1);
										dgs.CFade.Sub().fadeIn(1);
									}
									else if (_sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN)
									{
										AreaChange.getInstance().setCloseStrong();
										AreaChange.getInstance().setOpen(15, AreaChange.SWITCH_TYPE.SWITCH_TYPE_OUTER);
										dgs.CFade.Main().fadeIn(1);
										dgs.CFade.Sub().fadeIn(1);
									}
									else
									{
										AreaChange.getInstance().setOpenStrong();
										dgs.CFade.Main().fadeIn(15);
									}
									dgs.CFade.Sub().fadeIn(15);
									CWorldOutSideData.getInstance().initialize2();
									_sys.PlayerMng().into();
									_sys.changePlayerCharDisplay();
									if (sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_MOG_NET)
									{
										_sys.setMapSoundSetting(MapSound.loadSetupMapSoundSetting());
										MapSound.storeSetupMapSoundSetting(b: false);
									}
									_sys.setUpMapSound();
									if (sys.GGlobal.getPreviousPart() != GAMEPART.GAMEPART_BATTLE && sys.GGlobal.getPreviousPart() != GAMEPART.GAMEPART_MOG_NET)
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
