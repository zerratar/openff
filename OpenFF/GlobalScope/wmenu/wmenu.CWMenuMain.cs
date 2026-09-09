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
	public static partial class wmenu
	{
							public class CWMenuMain : CWMenuMemberBase
							{
								private int prev_focus;

								private WMENU_KIND auto_shift_menu;

								private NNSG2dSVec2[] charPos = new NNSG2dSVec2[4];

								public CWMenuMain()
								{
									for (int i = 0; i < charPos.Length; i++)
									{
										charPos[i] = new NNSG2dSVec2();
									}
									auto_shift_menu = WMENU_KIND.WMENU_KIND_MAX;
								}

								public override bool cSelectInitialize()
								{
									return false;
								}

								public override void initialize()
								{
									setFinalize(val: false);
									wld.WorldPart.getInstance().getWorldSystem().World2DMng()
										.refWorldMap()
										.hideMapMarker();
									ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: false, bg2: true, bg3: true, obj: true);
									menu.MenuManager.getSingleton().buildMenu("main_menu");
									if (wld.WorldPart.getInstance().getWorldSystem().PreviousMode() != wld.CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
									{
										menu.Medget nodeByID = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID(TRANSCODE("com_save"));
										if (nodeByID != null && nodeByID.behavior() != null)
										{
											((menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier()))?.changeTextColor(dgs.TXT_COLOR.TXT_UCOLOR_4);
										}
									}
									if (FlagManager.singleton().get(0u, 36u) == 0)
									{
										menu.Medget nodeByID2 = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID(TRANSCODE("com_job"));
										if (nodeByID2 != null && nodeByID2.behavior() != null)
										{
											((menu.MBText)nodeByID2.behavior().queryInterface(menu.MBText.classIdentifier()))?.changeTextColor(dgs.TXT_COLOR.TXT_UCOLOR_4);
										}
									}
									CWMenuManager.Instance().SetPrimaryBG(9);
									CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
									CWMenuManager.Instance().swapCharFirstPosition();
									CWMenuManager.Instance().SetActiveCharShow();
									menu.MenuManager.getSingleton().initFocus(CWMenuManager.Instance().GetMainMenuMemoryCursor());
									if (auto_shift_menu >= WMENU_KIND.WMENU_KIND_MAX)
									{
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
									}
									else
									{
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
									}
									CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_RUN);
								}

								public override void run()
								{
									menu.MenuManager.getSingleton().execute();
									ushort num = 0;
									num = (ushort)((opt.COptionManager.getSingleton().gameOption().menuZoomSetting() != opt.MENU_ZOOM_SETTING.MENU_R_ZOOM_L) ? 512 : 256);
									if (auto_shift_menu < WMENU_KIND.WMENU_KIND_MAX)
									{
										CWMenuManager.Instance().SetNextKind(auto_shift_menu);
										CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
										auto_shift_menu = WMENU_KIND.WMENU_KIND_MAX;
										return;
									}
									if (menu.MenuManager.getSingleton().GetActivateButtonState() != 0)
									{
										if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
										{
											if (strcmp(menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
												._id(), TRANSCODE("char_select")) == 0)
											{
												return;
											}
											int num2 = (sbyte)menu.MenuManager.getSingleton().getFocuseMedget().work();
											if (num2 == 8 && wld.WorldPart.getInstance().getWorldSystem().PreviousMode() != wld.CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
											{
												menu.MenuManager.getSingleton().playSEBeep();
												return;
											}
											if (num2 == 5 && FlagManager.singleton().get(0u, 36u) == 0)
											{
												menu.MenuManager.getSingleton().playSEBeep();
												return;
											}
											menu.MenuManager.getSingleton().playSEDecide();
											CWMenuManager.Instance().SetMainMenuMemoryCursor(num2);
											CWMenuManager.Instance().SetNextKind((WMENU_KIND)num2);
											CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
										}
										// PORT: the menu-zoom button (L or R, by the config) closes the menu; the recreated
										// source had this as (edge() != 0) & (num != 0), which any button satisfied - a
										// pad's Left or Right shut the menu.
										else if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB() || (ds.g_Pad.edge() & num) != 0)
										{
											CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_END);
										}
									}
									menu.MenuManager.getSingleton().ClearBehaviorButton();
								}

								public override void terminate()
								{
									if (!isFinalize())
									{
										menu.MenuManager.getSingleton().releaseWindowAll();
										setFinalize(val: true);
									}
									auto_shift_menu = WMENU_KIND.WMENU_KIND_MAX;
								}

								public void setUpCharScrPos()
								{
									CWMenuManager.Instance().swapCharFirstPosition();
									CWMenuManager.Instance().GetPcFace().pcfmSetStatusDefault(pl.PlayerParty.instance().playerForId(0).formationType() == 0, pl.PlayerParty.instance().playerForId(1).formationType() == 0, pl.PlayerParty.instance().playerForId(2).formationType() == 0, pl.PlayerParty.instance().playerForId(3).formationType() == 0);
									for (int i = 0; i < 4; i++)
									{
										charPos[i].copy(CWMenuManager.Instance().GetPcFace().pcfmGetPosition(pl.PlayerParty.instance().player((byte)i).playerId()));
									}
								}

								public void bmRefresh()
								{
								}

								public NNSG2dSVec2 GetCharPosI(int val)
								{
									return charPos[val];
								}

								public void autoShift(WMENU_KIND wmk)
								{
									auto_shift_menu = wmk;
								}
							}
	}
}
