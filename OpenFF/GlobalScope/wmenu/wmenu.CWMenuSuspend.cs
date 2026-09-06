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
							public class CWMenuSuspend : CWMenuMemberBase, menu.MenuBehavedNotifier
							{
								public const int CWMS_CONFIRM = 0;

								public const int CWMS_SETUPMESSAGE = 1;

								public const int CWMS_PREPARE_SUSPEND = 2;

								public const int CWMS_SUSPEND = 3;

								public const int CWMS_SUSPEND_FIN = 4;

								public const int CWMS_SHUTDOWN = 5;

								public const int CWMS_TERMINATE = 6;

								public const int CWMS_STATES = 7;

								private int cwmsState_;

								private card.SaveDataAddress SuspendData_ = new card.SaveDataAddress();

								private dgs.DGSMessage pMessage_;

								public CWMenuSuspend()
								{
									cwmsState_ = 0;
								}

								public override bool cSelectInitialize()
								{
									return false;
								}

								public override void initialize()
								{
									setFinalize(val: false);
									menu.MenuManager.getSingleton().buildMenu("suspend_");
									menu.MenuManager.getSingleton().ClearBehaviorButton();
									menu.MenuManager.getSingleton().initFocus(1);
									menu.MenuManager.getSingleton().root().getNodeByID("suspend_menu")?.behavior().mbSetNotifier(this);
									SVC_WaitVBlankIntr();
									CWMenuManager.Instance().SetPrimaryBG(7);
									CWMenuManager.Instance().GetPcFace().pcfmClear();
									CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
									cwmsState_ = 0;
								}

								public override void run()
								{
									menu.MenuManager.getSingleton().execute();
									if (cwmsState_ == 0)
									{
										cwmsConfirm();
									}
									else if (1 == cwmsState_)
									{
										cwmsSetupMessage();
									}
									else if (2 == cwmsState_)
									{
										cwmsPrepareSuspend();
									}
									else if (3 == cwmsState_)
									{
										cwmsSuspend();
									}
									else if (4 == cwmsState_)
									{
										cwmsSuspendFin();
									}
									else if (5 == cwmsState_)
									{
										cwmsShutdown();
									}
									else if (6 == cwmsState_)
									{
										cwmsTerminate();
									}
								}

								public override void terminate()
								{
								}

								internal static void stateEnd_()
								{
									CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
									CWMenuManager.Instance().SetNextKind(WMENU_KIND.WMENU_KIND_MAIN_MENU);
								}

								public void cwmsConfirm()
								{
									if (CWMenuManager.Instance().GetMenuButton().TouchButtonB())
									{
										stateEnd_();
										menu.MenuManager.getSingleton().playSECancel();
									}
								}

								public void cwmsSetupMessage()
								{
									if (!dgs.CFade.Sub().isFaded())
									{
										return;
									}
									menu.MenuManager.getSingleton().release();
									byte[] pData = new byte[4];
									card.Manager.GetInstance().LoadData(pData, 4u, 0u);
									if (card.RESULT.RESULT_LOST_CARD != card.Manager.GetInstance().GetResult())
									{
										menu.MenuManager.getSingleton().buildMenu("now_suspending");
										cwmsState_ = 2;
									}
									else
									{
										menu.MenuManager.getSingleton().buildMenu("suspend_write_failed");
										menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID("suspend_message_2");
										if (nodeByID != null && nodeByID.behavior() != null)
										{
											menu.MBText mBText = (menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier());
											if (mBText != null && mBText.getMessage() != null)
											{
												mBText.getMessage().setStyle(1024u);
												mBText.getMessage().setVSpace(4);
											}
										}
										ds.Sound.Stop();
										cwmsState_ = 6;
									}
									CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: false);
									ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: false, obj: false);
									dgs.CFade.Sub().fadeIn(15);
								}

								public void cwmsPrepareSuspend()
								{
									if (dgs.CFade.Sub().isCleared())
									{
										SuspendData_.sdaSetValidity((int)card.SSD_SIGN);
										SuspendData_.sdCreate();
										SuspendData_.sdaSave(41568u);
										cwmsState_ = 3;
									}
								}

								public void cwmsSuspend()
								{
									if (SuspendData_.sdExecute())
									{
										dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
										cwmsState_ = 4;
									}
								}

								public void cwmsSuspendFin()
								{
									if (dgs.CFade.Sub().isFaded())
									{
										if (!SuspendData_.sdCheck())
										{
											menu.MenuManager.getSingleton().release();
											menu.MenuManager.getSingleton().buildMenu("suspend_write_failed");
											messageCentering("suspend_message_2");
											ds.Sound.Stop();
											cwmsState_ = 6;
										}
										else
										{
											menu.MenuManager.getSingleton().release();
											menu.MenuManager.getSingleton().buildMenu("suspend_end");
											messageCentering("suspend_message_2");
											MatrixSound.MtxSENDS_Play(98, 5, 192, 127);
											cwmsState_ = 5;
										}
										dgs.CFade.Sub().fadeIn(15);
										dgs.CFade.Main().fadeIn(15);
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
									}
								}

								public void cwmsShutdown()
								{
									OS_AssignBackButton(0);
									if (dgs.CFade.Sub().isCleared() && ds.g_TouchPanel.isTap())
									{
										wld.CBaseSystem.setTitle(b: true);
										CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_END);
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
										wld.MapSound.stopBGM(15);
									}
								}

								public void cwmsTerminate()
								{
									if (dgs.CFade.Sub().isCleared())
									{
										OS_Terminate();
									}
								}

								public bool mbnNotify(menu.MenuBehavior notifier, uint number, uint param)
								{
									if (cwmsState_ == 0)
									{
										switch (number)
										{
										case 0u:
											if (menu.MenuManager.getSingleton().getFocuseMedget()._id(TRANSCODE("yes")))
											{
												dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
												dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
												cwmsState_ = 1;
												menu.MenuManager.getSingleton().inputPermission(b: false);
												menu.MenuManager.getSingleton().playSEDecide();
											}
											else if (menu.MenuManager.getSingleton().getFocuseMedget()._id(TRANSCODE("no")))
											{
												menu.MenuManager.getSingleton().playSECancel();
												stateEnd_();
											}
											break;
										case 1u:
											menu.MenuManager.getSingleton().playSECancel();
											stateEnd_();
											break;
										}
									}
									return true;
								}
							}
	}
}
