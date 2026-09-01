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
							public class CStateMenuEnd : CBaseState
							{
								private int step;

								private int counter;

								public override void start(CBaseSystem _sys)
								{
									OS_AssignBackButton(1);
									menu.MenuManager.getSingleton().ReleaseItemDataText();
									menu.MenuManager.getSingleton().ReleaseMenuDataText();
									wmenu.CWMenuManager.Instance().terminate();
									menu.MenuManager.getSingleton().ReleaseXbnFile();
									menu.MenuManager.getSingleton().DeleteNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
									menu.MenuManager.getSingleton().Set2d3dMode(3);
									menu.MenuManager.getSingleton().release();
									step = 0;
									counter = SCROLL_CONSTANT;
									_sys.WorldCamera().setProjMode(1);
									G2_SetWnd0Position(0, 0, 255, 192);
									G2_SetWnd0InsidePlane(1, 0);
									G2_SetWndOutsidePlane(0, 0);
									GX_SetVisibleWnd(0);
									dgs.CFade.Main().fadeOut(SCROLL_COUNT, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
									ds.g_Pad.disable();
									ds.g_TouchPanel.disable();
									MatrixSound.MtxSENDS_Unload();
									MatrixSound.MtxSENDS_Load(1);
									MatrixSound.MtxSENDS_Play(1, 0, 192, 127);
								}

								public override void update(CBaseSystem _sys)
								{
									OS_AssignBackButton(1);
									if (_sys.isEscape() || _sys.IsTitle())
									{
										if (dgs.CFade.Main().isFaded())
										{
											setPhase(PHASE.END);
										}
										return;
									}
									switch (step)
									{
									case 0:
										counter -= SCROLL_SPEED;
										if (counter < 0)
										{
											SVC_WaitVBlankIntr();
											NNS_G2dBGClear();
											_sys.World2DMng().initialize();
											if (!_sys.isSite())
											{
												dgs.CFade.Main().fadeIn(SCROLL_COUNT);
											}
											GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_SUB_MAIN);
											GX_Power3D(1);
											counter = SCROLL_CONSTANT;
											step = 1;
										}
										break;
									case 1:
										_sys.changePlayerCharDisplay();
										step = 2;
										break;
									case 2:
										counter -= SCROLL_SPEED;
										if (counter < 0)
										{
											SVC_WaitVBlankIntr();
											VecFx32 offset = new VecFx32(0, 0, 0);
											_sys.WorldCamera().setOffset(offset);
											G2_SetWnd0Position(0, 0, 255, 192);
											GX_SetVisibleWnd(0);
											setPhase(PHASE.END);
										}
										break;
									}
								}

								public override void end(CBaseSystem _sys)
								{
									GX_Power3D(1);
									dv.CDeviceManager.getInstance().Pad().setActivity(b: true);
									opt.MENU_ZOOM_SETTING setting = opt.COptionManager.getSingleton().gameOption().menuZoomSetting();
									_sys.World2DMng().switchMenuCameraButton(setting);
									if (_sys.isEscape())
									{
										GX_SetVisibleWnd(0);
										CWorldOutSideData.getInstance().MapData().setCommonMdlNo(0);
										string arg = "";
										sprintf(out arg, "%s%s", sceneMng.getStage(), ".pak");
										char[] array = arg.ToCharArray();
										array[4] = '0';
										array[5] = '1';
										arg = new string(array);
										map.CMapParameterManager.Instance().Free();
										map.CMapParameterManager.Instance().Initialize();
										map.CMapParameterManager.Instance().Load(arg);
										CWorldOutSideData.getInstance().MapData().MapJumpIndex_set(1);
										CWorldOutSideData.getInstance().MapData().isColFlag_not_and(2048);
										_sys.setMapJump(b: true);
										_sys.setMode(_sys.PreviousMode());
										_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_MOVE);
										_sys.CrtState().phase_set(PHASE.END);
										_sys.World2DMng().setButtonShow(show: false);
										_sys.setMenu(b: false);
									}
									else if (_sys.isSite())
									{
										_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_SITE);
										_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_START);
										_sys.setMenu(b: false);
										_sys.resetSite();
										_sys.World2DMng().setButtonShow(show: false);
									}
									else if (_sys.IsTitle())
									{
										_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN);
										sys.GGlobal.setNextPart(GAMEPART.GAMEPART_TITLE);
										ds.g_Pad.enable();
										ds.g_TouchPanel.enable();
									}
									else
									{
										_sys.WorldCamera().setProjMode(0);
										_sys.setMode(_sys.PreviousMode());
										_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_MOVE);
										_sys.CrtState().phase_set(PHASE.UPDATE);
										_sys.setMenu(b: false);
										ds.g_Pad.enable();
										ds.g_TouchPanel.enable();
									}
								}

								public override bool canExecuteEvent(CBaseSystem arg0)
								{
									return false;
								}
							}
	}
}
