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
	public static partial class wmenu
	{
							public class CWMenuConfig : CWMenuMemberBase, menu.MenuBehavedNotifier
							{
								public const int CONFIG_WINDOW_MAIN = 0;

								public const int CONFIG_WINDOW_SUB = 1;

								public const int CONFIG_WINDOW_MAX = 2;

								private bool m_state;

								private int[] windowNo = new int[2];

								private int page;

								private int slideTime;

								private int slideDir;

								private dgs.DGSMessage pMsg;

								public override bool cSelectInitialize()
								{
									return false;
								}

								public override void initialize()
								{
									setFinalize(val: false);
									SVC_WaitVBlankIntr();
									page = 0;
									m_state = false;
									pMsg = null;
									slideTime = 0;
									slideDir = 0;
									CWMenuManager.Instance().GetPcFace().pcfmClear();
									CWMenuManager.Instance().SetPrimaryBG(6);
									CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
									CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_RUN);
									CWMenuManager.Instance().SetSecondlyBG(2);
									CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
									menu.MenuManager.getSingleton().buildMenu("config");
									menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
									ChangeFocuseComToList();
									menu.MenuManager.getSingleton().initFocus((int)opt.COptionManager.getSingleton().messageOption().messageSpeed());
									CWMenuManager.Instance().GetDummyCursor().SetCell(0);
									CWMenuManager.Instance().GetDummyCursor().PlayAnimation(0, NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD);
									CWMenuManager.Instance().GetDummyCursor().SetAnimation(anm: true);
									menu.MenuManager.getSingleton().GetCursor2d().PlayAnimation(0, NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD);
									menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: false);
									menu.Medget focuseMedget = menu.MenuManager.getSingleton().getFocuseMedget();
									if (focuseMedget != null)
									{
										menu.MBConfig mBConfig = (menu.MBConfig)focuseMedget.behavior().queryInterface(menu.MBConfig.classIdentifier());
										if (mBConfig != null)
										{
											mBConfig.bmBehave(focuseMedget);
											mBConfig.bmActivate(focuseMedget);
										}
									}
									menu.Medget nodeByID = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID(TRANSCODE("m_setting1"));
									CWMenuManager.Instance().GetDummyCursor().SetPositionI(nodeByID.cursorX(), nodeByID.cursorY());
									CWMenuManager.Instance().GetDummyCursor().SetShow(show: true);
								}

								public void SetPage(int newPage)
								{
									int num = page;
									page = newPage;
									if (page == 0 || page == 1)
									{
										CWMenuManager.Instance().SetPrimaryBG((page != 0) ? 12 : 6);
										menu.MenuManager.getSingleton().release();
										menu.MenuManager.getSingleton().buildMenu((page != 0) ? "config2" : "config");
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
										ChangeFocuseComToList();
										if (page == 0)
										{
											menu.MenuManager.getSingleton().initFocus((int)opt.COptionManager.getSingleton().messageOption().messageSpeed());
										}
										else
										{
											menu.MenuManager.getSingleton().initFocus(0);
										}
										menu.MenuManager.getSingleton().root().getNodeByID("m_title")?.behavior().mbSetNotifier(this);
										menu.MenuManager.getSingleton().root().getNodeByID("m_fix")?.behavior().mbSetNotifier(this);
										menu.Medget focuseMedget = menu.MenuManager.getSingleton().getFocuseMedget();
										if (focuseMedget != null)
										{
											menu.MBConfig mBConfig = (menu.MBConfig)focuseMedget.behavior().queryInterface(menu.MBConfig.classIdentifier());
											if (mBConfig != null)
											{
												mBConfig.bmBehave(focuseMedget);
												mBConfig.bmActivate(focuseMedget);
											}
										}
										CWMenuManager.Instance().GetDummyCursor().SetShow(show: true);
									}
									else if (page == 2)
									{
										CWMenuManager.Instance().SetPrimaryBG(13);
										CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
										if (num == 3)
										{
											menu.MenuManager.getSingleton().Pop();
										}
										else
										{
											menu.MenuManager.getSingleton().SetTargetItemNo(0);
											menu.MenuManager.getSingleton().release();
											menu.MenuManager.getSingleton().buildMenu("tips_list");
											CWMenuManager.Instance().GetDummyCursor().SetShow(show: false);
										}
										if (pMsg != null)
										{
											pMsg.release();
										}
										pMsg = null;
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
									}
									else if (page == 3)
									{
										CWMenuManager.Instance().SetPrimaryBG(14);
										CWMenuManager.Instance().GetMenuButton().SetUpNormalVer();
										menu.MenuManager.getSingleton().Push("tips_text");
										UpdateTipsText();
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
									}
								}

								public void UpdateTipsText()
								{
									int targetItemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
									menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID("title");
									if (nodeByID != null)
									{
										((menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier())).mbSetTextMsgNo(52102 + targetItemNo * 2);
									}
									nodeByID = menu.MenuManager.getSingleton().root().getNodeByID("text");
									if (nodeByID != null)
									{
										int num = nodeByID.x() + nodeByID.width() / 2;
										int num2 = nodeByID.y() + nodeByID.height() / 2;
										dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
										if (pMsg != null)
										{
											pMsg.release();
										}
										pMsg = dGSMessageManager.createMessage((uint)(52103 + targetItemNo * 2), dgs.INVALID_MSDHANDLE, 1);
										pMsg.setPosition((short)num, (short)num2, erase: true);
										pMsg.setDisplaySpeed(byte.MaxValue);
										pMsg.setDisplayWait(0);
										pMsg.setStyle(146u);
										pMsg.setVSpace(4);
										pMsg.progress();
									}
								}

								public override void run()
								{
									menu.MenuManager.getSingleton().execute();
									if (slideDir != 0)
									{
										slideTime++;
										G2_SetScreenOffset(((slideTime < 4) ? slideTime : (slideTime - 8)) * slideDir * 120, 0);
										if (slideTime == 4)
										{
											menu.MenuManager.getSingleton().SetTargetItemNo((menu.MenuManager.getSingleton().GetTargetItemNo() + slideDir + menu.MBTipsList.TIPS_ITEM_MAX) % menu.MBTipsList.TIPS_ITEM_MAX);
											UpdateTipsText();
										}
										if (slideTime == 8)
										{
											menu.MenuManager.getSingleton().inputPermission(b: true);
											slideTime = 0;
											slideDir = 0;
										}
										return;
									}
									if (page == 1 && (menu.MenuManager.getSingleton().getFocuseMedget()._id("m_title_back") || menu.MenuManager.getSingleton().getFocuseMedget()._id("m_tips") || menu.MenuManager.getSingleton().getFocuseMedget()._id("m_fix_on")))
									{
										menu.MBText mBText = (menu.MBText)menu.MenuManager.getSingleton().getFocuseMedget().behavior();
										if (!mBText.bmIsPushed())
										{
											menu.MenuManager.getSingleton().setFocuseMedget(menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("m_bgm"))
												.myTag());
										}
									}
									if (page == 0 || page == 1)
									{
										if ((ds.g_Pad.edge() & 2) != 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
										{
											if (menu.MenuManager.getSingleton().getFocuseMedget()._id("YES") || menu.MenuManager.getSingleton().getFocuseMedget()._id("NO"))
											{
												CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: true);
												CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
												menu.MenuManager.getSingleton().Pop();
												menu.MenuManager.getSingleton().playSECancel();
												menu.MenuManager.getSingleton().setFocuseMedget(menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("m_bgm"))
													.myTag());
												menu.MenuManager.getSingleton().SetActivateButtonState(1);
												CWMenuManager.Instance().GetDummyCursor().SetShow(show: true);
											}
											else
											{
												CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
												CWMenuManager.Instance().SetNextKind(WMENU_KIND.WMENU_KIND_MAIN_MENU);
												menu.MenuManager.getSingleton().playSECancel();
											}
										}
										else if (menu.MenuManager.getSingleton().GetActivateButtonState() == 0 && strcmp(menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
											._id(), "mm_command") == 0)
										{
											CWMenuManager.Instance().GetDummyCursor().SetPositionI(menu.MenuManager.getSingleton().getFocuseMedget().cursorX(), menu.MenuManager.getSingleton().getFocuseMedget().cursorY());
											CWMenuManager.Instance().GetDummyCursor().SetPriority(0);
											CWMenuManager.Instance().GetDummyCursor().SetShow(show: true);
											switch (menu.MenuManager.getSingleton().getFocuseMedget().myTag())
											{
											case 20:
												SetPage(0);
												break;
											case 21:
												SetPage(1);
												break;
											}
										}
									}
									else if (page == 2)
									{
										if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
										{
											SetPage(1);
											menu.MenuManager.getSingleton().playSECancel();
										}
										else if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
										{
											SetPage(3);
											menu.MenuManager.getSingleton().playSEDecide();
										}
									}
									else if (page == 3)
									{
										if ((ds.g_Pad.edge() & 2) != 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
										{
											SetPage(2);
											menu.MenuManager.getSingleton().playSECancel();
										}
										else if (CWMenuManager.Instance().GetMenuButton().TouchButtonL() || (ds.g_Pad.edge() & 0x200) != 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonR() || (ds.g_Pad.edge() & 0x100) != 0)
										{
											menu.MenuManager.getSingleton().playSEMoveCursor();
											menu.MenuManager.getSingleton().inputPermission(b: false);
											slideTime = 0;
											slideDir = ((!CWMenuManager.Instance().GetMenuButton().TouchButtonL() && (ds.g_Pad.edge() & 0x200) == 0) ? 1 : (-1));
										}
									}
								}

								public override void terminate()
								{
									if (!isFinalize())
									{
										menu.MenuManager.getSingleton().releaseWindowAll();
										menu.MenuManager.getSingleton().release();
										CWMenuManager.Instance().GetDummyCursor().SetShow(show: false);
										CWMenuManager.Instance().GetDummyCursor().SetAnimation(anm: false);
										CWMenuManager.Instance().GetDummyCursor().SetCell(3);
										if (pMsg != null)
										{
											pMsg.release();
										}
										pMsg = null;
										setFinalize(val: true);
									}
								}

								public void ChangeFocuseComToList()
								{
									menu.Medget nodeByID = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID(TRANSCODE("config_list"));
									for (menu.Medget medget = nodeByID.childNode(); medget != null; medget = medget.nextSibling())
									{
										for (menu.Medget medget2 = medget.childNode(); medget2 != null; medget2 = medget2.nextSibling())
										{
											if (atoi(medget2.work2().ToString()) != 0)
											{
												menu.MenuManager.getSingleton().joinFocusList(medget2);
											}
										}
									}
								}

								public void ChangeFocuseListToCom()
								{
									menu.Medget nodeByID = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID(TRANSCODE("config_list"));
									for (menu.Medget medget = nodeByID.childNode(); medget != null; medget = medget.nextSibling())
									{
										for (menu.Medget medget2 = medget.childNode(); medget2 != null; medget2 = medget2.nextSibling())
										{
											if ((int)medget2.work2() != 0)
											{
												menu.MenuManager.getSingleton().leaveFocusList(medget2);
											}
										}
									}
								}

								public bool mbnNotify(menu.MenuBehavior notifier, uint number, uint param)
								{
									if (menu.MenuManager.getSingleton().getFocuseMedget()._id(TRANSCODE("m_title_back")))
									{
										CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: false);
										CWMenuManager.Instance().SetSecondlyBGVisibility(b: true);
										menu.MenuManager.getSingleton().Push("config_confirm");
										menu.MenuManager.getSingleton().playSEDecide();
										menu.MenuManager.getSingleton().root().getNodeByID("confirm")?.behavior().mbSetNotifier(this);
										CWMenuManager.Instance().GetDummyCursor().SetShow(show: false);
									}
									else if (menu.MenuManager.getSingleton().getFocuseMedget()._id(TRANSCODE("m_tips")))
									{
										SetPage(2);
										menu.MenuManager.getSingleton().playSEDecide();
										menu.MenuManager.getSingleton().SetDecideButtonState(1);
									}
									else if (menu.MenuManager.getSingleton().getFocuseMedget()._id(TRANSCODE("m_fix_on")))
									{
										opt.COptionManager.getSingleton().gameOption().setFixScreenSetting((opt.COptionManager.getSingleton().gameOption().fixScreenSetting() == opt.FIX_SCREEN_SETTING.FIX_SCREEN_OFF) ? opt.FIX_SCREEN_SETTING.FIX_SCREEN_ON : opt.FIX_SCREEN_SETTING.FIX_SCREEN_OFF);
										menu.MenuManager.getSingleton().playSEDecide();
										card.SaveOption();
									}
									else if (menu.MenuManager.getSingleton().getFocuseMedget()._id(TRANSCODE("YES")))
									{
										wld.CBaseSystem.setTitle(b: true);
										CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_END);
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
										wld.MapSound.stopBGM(15);
									}
									else if (menu.MenuManager.getSingleton().getFocuseMedget()._id(TRANSCODE("NO")))
									{
										CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: true);
										CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
										menu.MenuManager.getSingleton().Pop();
										menu.MenuManager.getSingleton().playSECancel();
										menu.MenuManager.getSingleton().setFocuseMedget(menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("m_bgm"))
											.myTag());
										menu.MenuManager.getSingleton().SetActivateButtonState(1);
										CWMenuManager.Instance().GetDummyCursor().SetShow(show: true);
									}
									return true;
								}
							}
	}
}
