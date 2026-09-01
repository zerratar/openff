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
							public class CStateMenuStart : CBaseState
							{
								private int step;

								private int counter;

								public override void start(CBaseSystem _sys)
								{
									step = 0;
									counter = SCROLL_CONSTANT;
									G2_SetBG2Offset(0, 0);
									G2_SetWnd0Position(0, 0, 255, 192);
									G2_SetWnd0InsidePlane(1, 0);
									G2_SetWndOutsidePlane(0, 0);
									GX_SetVisibleWnd(0);
									savedCamera.copy(_sys.WorldCamera());
									_sys.WorldCamera().setProjMode(1);
									_sys.WorldCamera().composit.setZoomEnable(b: false);
									dgs.CFade.Main().fadeOut(SCROLL_COUNT, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
									dv.CDeviceManager.getInstance().Pad().setActivity(b: false);
									ds.g_Pad.disable();
									ds.g_TouchPanel.disable();
									_sys.resetSite();
									MatrixSound.MtxSENDS_Play(1, 1, 192, 127);
								}

								public override void update(CBaseSystem _sys)
								{
									OS_AssignBackButton(1);
									switch (step)
									{
									case 0:
									{
										counter -= SCROLL_SPEED;
										if (counter >= 0)
										{
											break;
										}
										if (!TexDivideLoader.getSingleton().tdlIsEmpty())
										{
											TexDivideLoader.getSingleton().tdlForceLoad();
										}
										SVC_WaitVBlankIntr();
										GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_MAIN_SUB);
										GX_Power3D(0);
										changeGlobalDirectory();
										wmenu.CWMenuManager.Instance().GetPcFace().pcfmSetup(NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB2);
										wmenu.CWMenuManager.Instance().GetPcFace().pcfmCleanup();
										for (int i = 0; i < 4; i++)
										{
											if (pl.PlayerParty.instance().player((byte)i).isEnable())
											{
												wmenu.CWMenuManager.Instance().GetPcFace().pcfmSetJob(pl.PlayerParty.instance().player((byte)i).playerId(), (uint)pl.PlayerParty.instance().player((byte)i).jobManager()
													.nowJob());
											}
										}
										wmenu.CWMenuManager.Instance().swapCharFirstPosition();
										menu.MenuManager.getSingleton().Set2d3dMode(2);
										changeCompanyDirectory();
										menu.MenuManager.getSingleton().LoadXbnFile("MenuDefine.xbn");
										SVC_WaitVBlankIntr();
										menu.MenuManager.getSingleton().CreateNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_2D);
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
										menu.MenuManager.getSingleton().SetUsingMenuType(1);
										menu.MenuManager.getSingleton().CreateItemDataText();
										menu.MenuManager.getSingleton().CreateMenuDataText(0);
										SVC_WaitVBlankIntr();
										wmenu.CWMenuManager.Instance().initialize();
										wmenu.CWMenuManager.Instance().SetPrimaryBG(9);
										counter = SCROLL_CONSTANT;
										G2_SetWnd0Position(0, 0, 0, 0);
										ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: false, bg2: true, bg3: true, obj: true);
										dgs.CFade.Main().fadeIn(SCROLL_COUNT);
										step = 2;
										break;
									}
									case 2:
										counter -= SCROLL_SPEED;
										if (counter < 0)
										{
											G2_SetWnd0Position(0, 0, 255, 192);
											G3X_SetHOffset(0);
											GX_SetVisibleWnd(0);
											step = 3;
										}
										break;
									case 3:
										setPhase(PHASE.END);
										break;
									case 1:
										break;
									}
								}

								public override void end(CBaseSystem _sys)
								{
									OS_AssignBackButton(1);
									MatrixSound.MtxSENDS_Unload();
									MatrixSound.MtxSENDS_Load(98);
									_sys.WorldCamera().setProjMode(0);
									_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_MOVE);
									ds.g_Pad.enable();
									ds.g_TouchPanel.enable();
								}

								public override bool canExecuteEvent(CBaseSystem arg0)
								{
									return false;
								}
							}
	}
}
