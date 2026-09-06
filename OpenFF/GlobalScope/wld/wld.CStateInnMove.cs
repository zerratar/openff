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
							public class CStateInnMove : CBaseState, menu.MenuBehavedNotifier
							{
								public enum INN_PHASE
								{
									INN_PHASE_START,
									INN_PHASE_START_FB,
									INN_PHASE_MSG,
									INN_PHASE_WND_OPEN,
									INN_PHASE_WND_OPEN_FB,
									INN_PHASE_START_CONFIRM,
									INN_PHASE_START_CONFIRM_FB,
									INN_PHASE_CONFIRM,
									INN_PHASE_RECEPTION,
									INN_PHASE_RECEPTION_FB,
									INN_PHASE_WAIT_SE,
									INN_PHASE_WND_WAIT_MSG2,
									INN_PHASE_WND_CLOSE,
									INN_PHASE_END,
									INN_PHASE_MAX
								}

								public const INN_PHASE INN_PHASE_START = INN_PHASE.INN_PHASE_START;

								public const INN_PHASE INN_PHASE_START_FB = INN_PHASE.INN_PHASE_START_FB;

								public const INN_PHASE INN_PHASE_MSG = INN_PHASE.INN_PHASE_MSG;

								public const INN_PHASE INN_PHASE_WND_OPEN = INN_PHASE.INN_PHASE_WND_OPEN;

								public const INN_PHASE INN_PHASE_WND_OPEN_FB = INN_PHASE.INN_PHASE_WND_OPEN_FB;

								public const INN_PHASE INN_PHASE_START_CONFIRM = INN_PHASE.INN_PHASE_START_CONFIRM;

								public const INN_PHASE INN_PHASE_START_CONFIRM_FB = INN_PHASE.INN_PHASE_START_CONFIRM_FB;

								public const INN_PHASE INN_PHASE_CONFIRM = INN_PHASE.INN_PHASE_CONFIRM;

								public const INN_PHASE INN_PHASE_RECEPTION = INN_PHASE.INN_PHASE_RECEPTION;

								public const INN_PHASE INN_PHASE_RECEPTION_FB = INN_PHASE.INN_PHASE_RECEPTION_FB;

								public const INN_PHASE INN_PHASE_WAIT_SE = INN_PHASE.INN_PHASE_WAIT_SE;

								public const INN_PHASE INN_PHASE_WND_WAIT_MSG2 = INN_PHASE.INN_PHASE_WND_WAIT_MSG2;

								public const INN_PHASE INN_PHASE_WND_CLOSE = INN_PHASE.INN_PHASE_WND_CLOSE;

								public const INN_PHASE INN_PHASE_END = INN_PHASE.INN_PHASE_END;

								public const INN_PHASE INN_PHASE_MAX = INN_PHASE.INN_PHASE_MAX;

								private int phase_;

								private bool mode_;

								private bool confirm_;

								private int confirmWindowID_;

								private int moneyWindowID_;

								private int seCounter_;

								public override void start(CBaseSystem _sys)
								{
									changeCompanyDirectory();
									menu.MenuManager.getSingleton().CreateMenuDataText(0);
									changeGlobalDirectory();
									if (CCastCommandTransit.getInstance().cast_getInnValue() <= 0)
									{
										mode_ = false;
										phase_ = 1;
									}
									else
									{
										mode_ = true;
										phase_ = 0;
									}
									confirmWindowID_ = (moneyWindowID_ = -1);
									confirm_ = false;
								}

								public override void update(CBaseSystem _sys)
								{
									OS_AssignBackButton(1);
									menu.MenuManager.getSingleton().execute();
									switch (phase_)
									{
									case 0:
										_sys.World2DMng().MessageWindow().setup();
										_sys.World2DMng().MessageWindow().createMessageWindow(0, 1000008, -1);
										dgs.CCtrlCodeInterface.instance().setInnPrice(CCastCommandTransit.getInstance().cast_getInnValue());
										phase_ = 2;
										break;
									case 1:
										phase_ = 6;
										break;
									case 2:
										if (_sys.World2DMng().MessageWindow().isFinished() && ((ds.g_Pad.edge() & 1) != 0 || (ds.g_Pad.edge() & 2) != 0 || (ds.g_Pad.edge() & 0x400) != 0 || (ds.g_Pad.edge() & 0x800) != 0 || ds.g_TouchPanel.isTouch()))
										{
											phase_ = 5;
										}
										break;
									case 5:
										if ((ds.g_Pad.edge() & 1) != 0 || CCastCommandTransit.getInstance().cast_getInnValue() != 0)
										{
											_sys.World2DMng().MessageWindow().release();
											menu.MenuManager.getSingleton().Set2d3dMode(3);
											menu.MenuManager.getSingleton().CreateNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
											menu.MenuManager.getSingleton().GetCursor3d().SetShow(show: false);
											confirmWindowID_ = menu.MenuManager.getSingleton().buildWindow(TRANSCODE("inn_question"), TRANSCODE("inn_question_window"));
											moneyWindowID_ = menu.MenuManager.getSingleton().buildWindow(TRANSCODE("inn_question"), TRANSCODE("inn_money_window"));
											phase_ = 3;
										}
										break;
									case 3:
									{
										bool flag = menu.MenuManager.getSingleton().UpdateWindowState(confirmWindowID_, menu.MenuWindow.MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_LARGE);
										bool flag2 = menu.MenuManager.getSingleton().UpdateWindowState(moneyWindowID_, menu.MenuWindow.MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_LARGE);
										if (!flag && !flag2)
										{
											menu.MenuManager.getSingleton().buildMenu(TRANSCODE("inn_question"));
											menu.MenuManager.getSingleton().GetCursor3d().SetShow(show: true);
											menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("inn_confirm"))?.behavior().mbSetNotifier(this);
											phase_ = 7;
										}
										break;
									}
									case 6:
										menu.MenuManager.getSingleton().Set2d3dMode(3);
										menu.MenuManager.getSingleton().CreateNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
										menu.MenuManager.getSingleton().GetCursor3d().SetShow(show: false);
										confirmWindowID_ = menu.MenuManager.getSingleton().buildWindow(TRANSCODE("fb_question"), TRANSCODE("fb_question_window"));
										phase_ = 4;
										break;
									case 4:
										if (!menu.MenuManager.getSingleton().UpdateWindowState(confirmWindowID_, menu.MenuWindow.MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_LARGE))
										{
											menu.MenuManager.getSingleton().buildMenu(TRANSCODE("fb_question"));
											menu.MenuManager.getSingleton().GetCursor3d().SetShow(show: true);
											menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("fb_confirm"))?.behavior().mbSetNotifier(this);
											phase_ = 7;
										}
										break;
									case 8:
										menu.MenuManager.getSingleton().release();
										menu.MenuManager.getSingleton().releaseWindowAll();
										menu.MenuManager.getSingleton().DeleteNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
										if (confirm_)
										{
											if (CCastCommandTransit.getInstance().cast_getInnValue() > pl.PlayerParty.instance().gold().get())
											{
												_sys.World2DMng().MessageWindow().createMessageWindow(0, 1000011, 2);
												confirm_ = false;
												seCounter_ = 15;
											}
											else
											{
												_sys.World2DMng().MessageWindow().createMessageWindow(0, 1000010, 2);
												pl.PlayerParty.instance().gold().sub(CCastCommandTransit.getInstance().cast_getInnValue());
												dgs.CCtrlCodeInterface.instance().setGold(pl.PlayerParty.instance().gold().get());
												MatrixSound.MtxSENDS_Play(0, 6, 192, 127);
												seCounter_ = 15;
											}
											phase_ = 10;
										}
										else
										{
											phase_ = 13;
										}
										break;
									case 9:
										menu.MenuManager.getSingleton().release();
										menu.MenuManager.getSingleton().releaseWindowAll();
										menu.MenuManager.getSingleton().DeleteNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
										phase_ = 13;
										break;
									case 10:
										if (0 > --seCounter_)
										{
											phase_ = 11;
										}
										break;
									case 11:
										if (_sys.World2DMng().MessageWindow().isFinished() && ((ds.g_Pad.edge() & 1) != 0 || (ds.g_Pad.edge() & 2) != 0 || (ds.g_Pad.edge() & 0x400) != 0 || (ds.g_Pad.edge() & 0x800) != 0 || ds.g_TouchPanel.isTouch()))
										{
											phase_ = 13;
										}
										break;
									case 13:
										_sys.World2DMng().MessageWindow().release();
										setPhase(PHASE.END);
										CCastCommandTransit.getInstance().cast_setInnConfirm(confirm_);
										break;
									case 7:
									case 12:
										break;
									}
								}

								public override void end(CBaseSystem _sys)
								{
									menu.MenuManager.getSingleton().ReleaseMenuDataText();
									_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_END);
								}

								public bool mbnNotify(menu.MenuBehavior notifier, uint number, uint param)
								{
									if (7 == phase_)
									{
										if (number == 0)
										{
											confirm_ = true;
											menu.MenuManager.getSingleton().playSEDecide();
										}
										else if (1 == number)
										{
											confirm_ = false;
											menu.MenuManager.getSingleton().playSECancel();
										}
										else if (-1 == number)
										{
											confirm_ = false;
											menu.MenuManager.getSingleton().playSECancel();
										}
										if (mode_)
										{
											phase_ = 8;
										}
										else if (!mode_)
										{
											phase_ = 9;
										}
									}
									return true;
								}

								public override bool canExecuteEvent(CBaseSystem arg0)
								{
									return true;
								}
							}
	}
}
