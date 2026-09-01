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
							public class CWMenuItem : CWMenuMemberBase
							{
								public class ITEM_SEITON
								{
									public int itemID;

									public int itemNum;
								}

								public const int ITEM_CONSUMPTION = 0;

								public const int ITEM_MAGIC = 1;

								public const int ITEM_ERR = 2;

								public const int APPEND_RIGHT = 0;

								public const int APPEND_LEFT = 1;

								public const int ITEM_WINDOW_LIST = 0;

								public const int ITEM_WINDOW_HELP = 1;

								public const int ITEM_WINDOW_USE = 2;

								public const int ITEM_WINDOW_MAX = 3;

								public const int SEITON_TOP_ITEM = 0;

								public const int SEITON_TOP_WEAPON = 1;

								public const int SEITON_TOP_ARMER = 2;

								public const int LOCAL_MAGIC = 0;

								public const int LOCAL_NORMAL = 1;

								private int helpNo;

								private dgs.DGSMessage pHelpMsg;

								private menu.Medget pAppendAddr;

								private menu.Medget pMainMedget;

								private menu.Medget dDecidePrevMedget;

								private menu.MBItemWindow pItemWindow;

								private int myProcess;

								private int localState;

								private int seitonPatern;

								private int sortCount;

								private int selectItemNo;

								private int dummyInBoxNo;

								private int dummyLine;

								private int dummyItemNo;

								private ds.Vector2<short> dummyCursorPos = new ds.Vector2<short>();

								private sys2d.Cell dummyCursor = new sys2d.Cell();

								private ITEM_SEITON[] nItem = new ITEM_SEITON[384];

								private ITEM_SEITON[] wItem = new ITEM_SEITON[384];

								private ITEM_SEITON[] aItem = new ITEM_SEITON[384];

								private ITEM_SEITON[] mItem = new ITEM_SEITON[384];

								private ITEM_SEITON[] iItem = new ITEM_SEITON[384];

								private int nCount;

								private int wCount;

								private int aCount;

								private int mCount;

								private int iCount;

								private int err_no;

								private int effectItemNo;

								private bool itemNoneState;

								private int used_item_no;

								private int used_item_num;

								private int localUState;

								private int appendType;

								private int prev_focus_group;

								private int prevCharNo_;

								private bool m_state;

								private int[] windowNo = new int[3];

								private sys2d.Cell[] masterCard = new sys2d.Cell[2];

								private ds.sys3d.CCamera Camera_ = new ds.sys3d.CCamera();

								private int wait;

								private static int use_type;

								public override bool cSelectInitialize()
								{
									return false;
								}

								public override void initialize()
								{
									setFinalize(val: false);
									SVC_WaitVBlankIntr();
									CWMenuManager.Instance().SetPrimaryBG(0);
									CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
									menu.MenuManager.getSingleton().SetTargetCharNo(0);
									menu.MenuManager.getSingleton().SetItemListPatern(0);
									menu.MenuManager.getSingleton().buildMenu("menu_item");
									for (int i = 0; i < 4; i++)
									{
										CWMenuManager.Instance().SetShowPcFace(i, show: false);
									}
									CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
									menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
									sys2d.DS2DManager.d2dGetInstance().d2dUpdate();
									dummyCursor.copy(menu.MenuManager.getSingleton().GetCursor2d());
									dummyCursor.SetCell(3);
									dummyCursor.SetAnimation(anm: false);
									dummyCursor.SetPriority(1);
									dummyCursor.SetShow(show: false);
									dummyCursor.SetPositionI(256, 192);
									sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(dummyCursor);
									CWMenuManager.Instance().GetDummyCursor().SetCell(0);
									CWMenuManager.Instance().GetDummyCursor().PlayAnimation(0, NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD);
									CWMenuManager.Instance().GetDummyCursor().SetAnimation(anm: true);
									menu.MenuManager.getSingleton().GetCursor2d().PlayAnimation(0, NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD);
									pMainMedget = menu.MenuManager.getSingleton().GetBaseMedget();
									m_state = true;
									helpNo = -1;
									localState = 0;
									localUState = 0;
									seitonPatern = 0;
									pHelpMsg = null;
									effectItemNo = 0;
									nCount = (aCount = (wCount = (mCount = (iCount = 0))));
									sortCount = 0;
									itemNoneState = false;
									prev_focus_group = 0;
									pAppendAddr = null;
									dDecidePrevMedget = null;
									pItemWindow = null;
									pItemWindow = (menu.MBItemWindow)pMainMedget.getNodeByID(TRANSCODE("item_list")).behavior().queryInterface(menu.MBItemWindow.classIdentifier());
									if (pItemWindow != null)
									{
										pItemWindow.mbPause();
									}
									menu.MenuManager.getSingleton().SetActivateButtonState(1);
									CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_RUN);
									myProcess = 0;
									ItemListNextProcessToNormal();
									prev_focus_group = menu.MenuManager.getSingleton().getFocuseMedget().myTag();
									if (pItemWindow != null)
									{
										pItemWindow.mbRestart();
									}
									CWMenuManager.Instance().GetDummyCursor().SetPositionI(menu.MenuManager.getSingleton().getFocuseMedget().cursorX(), menu.MenuManager.getSingleton().getFocuseMedget().cursorY());
									CWMenuManager.Instance().GetDummyCursor().SetShow(show: true);
									ChangeFocuseComToList();
									menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: false);
									helpNo = 0;
									m_state = false;
									localState = 0;
									prevCharNo_ = -1;
								}

								public override void run()
								{
									menu.MenuManager.getSingleton().execute();
									switch (myProcess)
									{
									case 0:
										ItemUsing();
										break;
									case 1:
										ItemSeiton();
										break;
									case 2:
										ItemImportant();
										break;
									}
									if (menu.MenuManager.getSingleton().GetActivateButtonState() == 0 && menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
										._id(TRANSCODE("mm_command")))
									{
										ProcessReturnFirst();
										CWMenuManager.Instance().GetDummyCursor().SetShow(show: false);
										switch (menu.MenuManager.getSingleton().getFocuseMedget().myTag())
										{
										case 0:
											ItemListNextProcessToNormal();
											break;
										case 1:
											myProcess = 1;
											break;
										case 2:
											ItemListNextProcessToImportant();
											break;
										}
										if (myProcess != 1)
										{
											prev_focus_group = menu.MenuManager.getSingleton().getFocuseMedget().myTag();
											if (pItemWindow != null)
											{
												pItemWindow.mbRestart();
											}
											CWMenuManager.Instance().GetDummyCursor().SetPositionI(menu.MenuManager.getSingleton().getFocuseMedget().cursorX(), menu.MenuManager.getSingleton().getFocuseMedget().cursorY());
											CWMenuManager.Instance().GetDummyCursor().SetShow(show: true);
											ChangeFocuseComToList();
											menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: false);
										}
										if (myProcess == 1)
										{
											menu.MenuManager.getSingleton().playSEDecide();
										}
										helpNo = 0;
										m_state = false;
										localState = 0;
									}
									menu.MenuManager.getSingleton().SetActivateButtonState(1);
									menu.MenuManager.getSingleton().ClearBehaviorButton();
								}

								public override void terminate()
								{
									if (!isFinalize())
									{
										if (pHelpMsg != null)
										{
											pHelpMsg.release();
											pHelpMsg = null;
										}
										if (pAppendAddr != null)
										{
											menu.MenuManager.getSingleton().Remove(pAppendAddr);
											pAppendAddr = null;
										}
										dummyCursor.Release();
										sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(dummyCursor);
										CWMenuManager.Instance().GetDummyCursor().SetShow(show: false);
										CWMenuManager.Instance().GetDummyCursor().SetAnimation(anm: false);
										CWMenuManager.Instance().GetDummyCursor().SetCell(3);
										for (int i = 0; i < 2; i++)
										{
											NNS_G2dReleaseImageProxy(masterCard[i].GetImageProxy());
											masterCard[i].Release();
											sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(masterCard[i]);
										}
										CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
										menu.MenuManager.getSingleton().releaseWindowAll();
										menu.MenuManager.getSingleton().release();
										setFinalize(val: true);
									}
								}

								public void ProcessReturnFirst()
								{
									int focusedCursor = menu.MenuManager.getSingleton().getFocuseMedget().myTag();
									m_state = true;
									helpNo = -1;
									localState = 0;
									localUState = 0;
									if (pHelpMsg != null)
									{
										pHelpMsg.release();
										pHelpMsg = null;
									}
									dummyCursor.SetShow(show: false);
									effectItemNo = 0;
									sortCount = 0;
									itemNoneState = false;
									menu.Medget nodeByID = pMainMedget.getNodeByID(TRANSCODE("item_list"));
									if (nodeByID != null && pItemWindow != null)
									{
										pItemWindow.bmDeactivate(nodeByID);
										pItemWindow.mbPause();
									}
									TouchAreaToRemoveRect();
									menu.MenuManager.getSingleton().releaseWindowAll();
									menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: true);
									menu.Medget nodeByID2 = pMainMedget.getNodeByID(TRANSCODE("item_list"));
									if (nodeByID2 != null)
									{
										CWMenuManager.Instance().ChainLeaveFocuseList(nodeByID2.childNode());
									}
									menu.MenuManager.getSingleton().initFocus(focusedCursor);
									CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
									GX_Power3D(0);
									GX_SetPriority3D(0);
									stageMng.setHidden(flag: false);
									wld.WorldPart.getInstance().getScene().setCamera(wld.WorldPart.getInstance().getWorldSystem().WorldCamera());
									wld.WorldPart.getInstance().getWorldSystem().WorldCamera()
										.setActivity(b: true);
								}

								public void ItemUsing()
								{
									switch (localState)
									{
									case 0:
										if (ds.g_TouchPanel.getDispPoint().hold != 0)
										{
											ds.g_TouchPanel.getPoint(out var x2, out var y2);
											menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("item_list"));
											menu.MBItemWindow mBItemWindow = (menu.MBItemWindow)nodeByID.behavior().queryInterface(menu.MBItemWindow.classIdentifier());
											for (menu.Medget medget = nodeByID.childNode(); medget != null; medget = medget.nextSibling())
											{
												if (medget.x() < x2 && x2 <= medget.x() + medget.width() && medget.y() < y2 && y2 <= medget.y() + medget.height())
												{
													menu.MenuManager.getSingleton().initFocus(medget.myTag());
													menu.MenuManager.getSingleton().playSEDecide();
													if (mBItemWindow.SetDrag(drag: true))
													{
														CreateDummyCursor();
														localState = 1;
													}
													break;
												}
											}
										}
										if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
										{
											if (menu.MenuManager.getSingleton().GetTargetItemNo() <= 0)
											{
												menu.MenuManager.getSingleton().playSEBeep();
												break;
											}
											CreateDummyCursor();
											dummyCursor.SetShow(show: false);
											bool flag = CheckPushItemUsePosible();
											if ((evt.CEventRestriction.getSingleton().check(dummyItemNo) || !flag) && itm.CATEGORY.CATEGORY_MAGIC != itm.ItemManager.instance().itemCategory((short)menu.MenuManager.getSingleton().GetTargetItemNo()))
											{
												menu.MenuManager.getSingleton().playSEBeep();
											}
											else if (menu.MenuManager.getSingleton().GetTargetItemNo() == 5009)
											{
												if (wld.WorldPart.getInstance().getWorldSystem().canEscape())
												{
													wld.WorldPart.getInstance().getWorldSystem().doEscape();
													CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_END);
													int num = pl.PlayerParty.instance().item().normalItem((int)menu.MenuManager.getSingleton().getFocuseMedget().work())
														.itemNumber();
													pl.PlayerParty.instance().item().normalItem((int)menu.MenuManager.getSingleton().getFocuseMedget().work())
														.setItemNumber(num - 1);
													pl.PlayerParty.instance().item().resetItemId();
													return;
												}
												menu.MenuManager.getSingleton().playSEBeep();
											}
											else if (menu.MenuManager.getSingleton().GetTargetItemNo() == 5012)
											{
												if (wld.WorldPart.getInstance().getWorldSystem().canSite())
												{
													wld.WorldPart.getInstance().getWorldSystem().doSite();
													CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_END);
													int num2 = pl.PlayerParty.instance().item().normalItem((int)menu.MenuManager.getSingleton().getFocuseMedget().work())
														.itemNumber();
													pl.PlayerParty.instance().item().normalItem((int)menu.MenuManager.getSingleton().getFocuseMedget().work())
														.setItemNumber(num2 - 1);
													pl.PlayerParty.instance().item().resetItemId();
													return;
												}
												menu.MenuManager.getSingleton().playSEBeep();
											}
											else
											{
												SetUpConsumptionItem();
												localState = 2;
												localUState = 0;
												dDecidePrevMedget = menu.MenuManager.getSingleton().getFocuseMedget();
												menu.MenuManager.getSingleton().playSEDecide();
												menu.MenuManager.getSingleton().inputPermission(b: false);
											}
										}
										else if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
										{
											menu.MenuManager.getSingleton().playSECancel();
											CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
											CWMenuManager.Instance().SetNextKind(WMENU_KIND.WMENU_KIND_MAIN_MENU);
										}
										break;
									case 1:
										if (ds.g_TouchPanel.isRelease())
										{
											ds.g_TouchPanel.getPoint(out var x, out var y);
											pItemWindow.SetDrag(drag: false);
											menu.Medget focuseMedget = menu.MenuManager.getSingleton().getFocuseMedget();
											if (dummyItemNo != menu.MenuManager.getSingleton().GetTargetItemNo() && focuseMedget.x() < x && x <= focuseMedget.x() + focuseMedget.width() && focuseMedget.y() < y && y <= focuseMedget.y() + focuseMedget.height())
											{
												ChangeItemPatern();
											}
											else
											{
												pItemWindow.RefreshTargetMsg(dummyInBoxNo, dummyInBoxNo);
											}
											menu.MenuManager.getSingleton().playSEDecide();
											DeleteDummyCursor();
											localState = 0;
										}
										else
										{
											CheckDispAreaDummyCursor();
										}
										break;
									case 2:
										UsingItemPatern();
										break;
									}
									if (localState != 2)
									{
										ProcessHelpWindow();
									}
								}

								public void SetUpConsumptionItem()
								{
									if ((int)menu.MenuManager.getSingleton().getFocuseMedget().work() % 2 == 0)
									{
										appendType = 0;
									}
									else
									{
										appendType = 1;
									}
									CWMenuManager.Instance().SetSecondlyBG(appendType);
									CWMenuManager.Instance().SetSecondlyBGVisibility(b: true);
								}

								public void ItemSeiton()
								{
									switch (localState)
									{
									case 0:
										sortCount = 0;
										ProcessHelpWindowVerSeiton();
										SeitonItemSort();
										localState = 1;
										break;
									case 1:
										SeitonItemListRefresh();
										seitonPatern++;
										if (seitonPatern > 2)
										{
											seitonPatern = 0;
										}
										localState = 2;
										wait = 15;
										break;
									case 2:
										wait--;
										if (wait == 0)
										{
											menu.MenuManager.getSingleton().setFocuseMedget(0);
											ProcessReturnFirst();
											ItemListNextProcessToNormal();
											prev_focus_group = menu.MenuManager.getSingleton().getFocuseMedget().myTag();
											if (pItemWindow != null)
											{
												pItemWindow.mbRestart();
											}
											CWMenuManager.Instance().GetDummyCursor().SetPositionI(menu.MenuManager.getSingleton().getFocuseMedget().cursorX(), menu.MenuManager.getSingleton().getFocuseMedget().cursorY());
											CWMenuManager.Instance().GetDummyCursor().SetShow(show: true);
											ChangeFocuseComToList();
											menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: false);
											helpNo = 0;
											m_state = false;
											localState = 0;
										}
										break;
									}
								}

								public void ItemImportant()
								{
									ProcessHelpWindow();
									switch (localState)
									{
									case 0:
										if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
										{
											if (menu.MenuManager.getSingleton().GetTargetItemNo() >= 5218 && menu.MenuManager.getSingleton().GetTargetItemNo() <= 5241)
											{
												if (pItemWindow != null)
												{
													pItemWindow.mbPause();
												}
												menu.MenuManager.getSingleton().inputPermission(b: false);
												menu.MenuManager.getSingleton().playSEDecide();
												dgs.CFade.Main().fadeOut(10, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
												localState = 3;
											}
											else
											{
												menu.MenuManager.getSingleton().playSEBeep();
											}
										}
										else if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
										{
											menu.MenuManager.getSingleton().playSECancel();
											CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
											CWMenuManager.Instance().SetNextKind(WMENU_KIND.WMENU_KIND_MAIN_MENU);
										}
										break;
									case 3:
										if (dgs.CFade.Main().isFaded())
										{
											sprintf(out var arg, "ca_text_%.2d.NCGR", menu.MenuManager.getSingleton().GetTargetItemNo() - 5218 + 1);
											masterCard[1].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "mastercard.NCER", null, arg, null);
											sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(masterCard[1]);
											sprintf(out arg, "ca_%.2d.NCGR", menu.MenuManager.getSingleton().GetTargetItemNo() - 5218 + 1);
											masterCard[0].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "mastercard.NCER", null, arg, null);
											masterCard[0].SetDepth(ds.S32toFX32(10));
											sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(masterCard[0]);
											menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
											CWMenuManager.Instance().GetDummyCursor().SetShow(show: false);
											dgs.CFade.Main().fadeIn(10);
											localState = 4;
										}
										break;
									case 4:
										if (dgs.CFade.Main().isCleared())
										{
											localState = 5;
										}
										break;
									case 5:
										if (ds.g_TouchPanel.isTap() || (ds.g_Pad.edge() & 2) != 0)
										{
											menu.MenuManager.getSingleton().playSECancel();
											dgs.CFade.Main().fadeOut(10, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
											localState = 6;
										}
										break;
									case 6:
										if (dgs.CFade.Main().isFaded())
										{
											for (int i = 0; i < 2; i++)
											{
												NNS_G2dReleaseImageProxy(masterCard[i].GetImageProxy());
												masterCard[i].Release();
												sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(masterCard[i]);
											}
											menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
											CWMenuManager.Instance().GetDummyCursor().SetShow(show: true);
											dgs.CFade.Main().fadeIn(10);
											localState = 7;
										}
										break;
									case 7:
										if (dgs.CFade.Main().isCleared())
										{
											if (pItemWindow != null)
											{
												pItemWindow.mbRestart();
											}
											menu.MenuManager.getSingleton().inputPermission(b: true);
											menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: false);
											localState = 0;
										}
										break;
									case 1:
									case 2:
										break;
									}
								}

								public void ProcessHelpWindow()
								{
									int targetItemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
									if (0 > targetItemNo || itm.ItemManager.instance().itemParameter((short)targetItemNo) == null)
									{
										if (pHelpMsg != null)
										{
											pHelpMsg.release();
											pHelpMsg = null;
											helpNo = -1;
										}
									}
									else if (itm.ItemManager.instance().itemParameter((short)targetItemNo).captionId() != helpNo)
									{
										helpNo = itm.ItemManager.instance().itemParameter((short)targetItemNo).captionId();
										if (pHelpMsg != null)
										{
											pHelpMsg.release();
											pHelpMsg = null;
										}
										dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
										pHelpMsg = dGSMessageManager.createMessage((uint)helpNo, menu.MenuManager.getSingleton().GetItemDataTextNo(), 1);
										if (pHelpMsg != null)
										{
											menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("caption"));
											pHelpMsg.setPosition(nodeByID.x(), (short)(nodeByID.y() + (nodeByID.height() - 12) / 2), erase: true);
											pHelpMsg.setDisplaySpeed(byte.MaxValue);
											pHelpMsg.setDisplayWait(0);
										}
									}
								}

								public void ProcessHelpWindowVerSeiton()
								{
									if (pHelpMsg != null)
									{
										pHelpMsg.release();
										pHelpMsg = null;
									}
									int[] array = new int[3] { 50210, 50211, 50212 };
									int num = 0;
									switch (seitonPatern)
									{
									case 0:
										num = 0;
										break;
									case 1:
										num = 1;
										break;
									case 2:
										num = 2;
										break;
									}
									helpNo = -100;
									dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
									pHelpMsg = dGSMessageManager.createMessage((uint)array[num], dgs.INVALID_MSDHANDLE, 1);
									if (pHelpMsg != null)
									{
										menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("caption"));
										pHelpMsg.setPosition(nodeByID.x(), (short)(nodeByID.y() + (nodeByID.height() - 12) / 2), erase: true);
										pHelpMsg.setDisplaySpeed(byte.MaxValue);
										pHelpMsg.setDisplayWait(0);
									}
								}

								public void ProcessHelpWindowVerErr(int msdNo)
								{
									if (pHelpMsg != null)
									{
										pHelpMsg.release();
										pHelpMsg = null;
									}
									dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
									pHelpMsg = dGSMessageManager.createMessage((uint)msdNo, dgs.INVALID_MSDHANDLE, 1);
									if (pHelpMsg != null)
									{
										menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("caption"));
										pHelpMsg.setPosition(nodeByID.x(), (short)(nodeByID.y() + (nodeByID.height() - 12) / 2), erase: true);
										pHelpMsg.setDisplaySpeed(byte.MaxValue);
										pHelpMsg.setDisplayWait(0);
									}
									helpNo = -100;
								}

								public void ItemListNextProcessToImportant()
								{
									if (myProcess != 2)
									{
										menu.MenuManager.getSingleton().SetItemListPatern(1);
										if (pItemWindow != null)
										{
											pItemWindow.RefreshChangeList(pItemWindow.mbGetOwner());
										}
									}
									myProcess = 2;
								}

								public void ItemListNextProcessToNormal()
								{
									if (myProcess == 2)
									{
										menu.MenuManager.getSingleton().SetItemListPatern(0);
										if (pItemWindow != null)
										{
											pItemWindow.RefreshChangeList(pItemWindow.mbGetOwner());
										}
									}
									myProcess = 0;
								}

								public void CreateDummyCursor()
								{
									dummyInBoxNo = (int)menu.MenuManager.getSingleton().getFocuseMedget().work();
									dummyLine = (int)menu.MenuManager.getSingleton().getFocuseMedget().work() / 2;
									dummyItemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
									dummyCursorPos.vx = (short)menu.MenuManager.getSingleton().getFocuseMedget().cursorX();
									dummyCursorPos.vy = (short)menu.MenuManager.getSingleton().getFocuseMedget().cursorY();
									dummyCursor.SetPositionI(dummyCursorPos.vx, dummyCursorPos.vy + 2);
									dummyCursor.SetShow(show: true);
								}

								public void DeleteDummyCursor()
								{
									dummyCursor.SetShow(show: false);
								}

								public void CheckDispAreaDummyCursor()
								{
									for (menu.Medget medget = menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
										.childNode(); medget != null; medget = medget.nextSibling())
									{
										int num = (int)medget.work();
										if (myProcess == 0)
										{
											num = pl.PlayerParty.instance().item().normalItem(num)
												.itemId();
										}
										else if (myProcess == 2)
										{
											num = pl.PlayerParty.instance().item().importantItem(num)
												.itemId();
										}
										if (dummyItemNo == num)
										{
											dummyCursorPos.vy = (short)medget.cursorY();
											dummyCursor.SetShow(show: true);
											dummyCursor.SetPositionI(dummyCursorPos.vx, dummyCursorPos.vy + 2);
											break;
										}
										if (medget.nextSibling() == null)
										{
											dummyCursor.SetShow(show: false);
										}
									}
								}

								public void UsingItemPatern()
								{
									switch (localUState)
									{
									case 0:
									{
										localUState = 1;
										effectItemNo = (int)menu.MenuManager.getSingleton().getFocuseMedget().work();
										selectItemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
										int num = selectItemNo;
										itm.CATEGORY cATEGORY = itm.ItemManager.instance().itemCategory((short)num);
										if (cATEGORY == itm.CATEGORY.CATEGORY_MAGIC)
										{
											use_type = 0;
										}
										else
										{
											use_type = 1;
										}
										menu.MenuManager.getSingleton().inputPermission(b: true);
										TouchAreaToAppendRect();
										menu.MenuManager.getSingleton().SetActivateButtonState(1);
										GX_Power3D(1);
										GX_SetPriority3D(1);
										stageMng.setHidden(flag: true);
										VecFx32 vecFx = new VecFx32((appendType == 0) ? (-72817) : 382293, 40960000, 0);
										VecFx32 position = new VecFx32(vecFx.x, vecFx.y + 524288, vecFx.z + 1048576);
										Camera_.initialize();
										Camera_.setMoveMode(1);
										Camera_.setPosition(position);
										Camera_.setTarget(vecFx);
										Camera_.setAngle(0, 32768, 0);
										Camera_.setCamUp(0, 4096, 0);
										Camera_.setDistance(65536);
										Camera_.setClip(40960, 2048000);
										Camera_.setFOV(1060, 3956);
										Camera_.execute();
										wld.WorldPart.getInstance().getWorldSystem().WorldCamera()
											.setActivity(b: false);
										break;
									}
									case 1:
										Camera_.execute();
										if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
										{
											menu.MenuManager.getSingleton().playSECancel();
											localUState = 2;
										}
										else if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
										{
											if (use_type == 1)
											{
												if (!ProcessItemEffect())
												{
													localUState = 2;
												}
											}
											else if (use_type == 0)
											{
												menu.MenuManager.getSingleton().playSEDecide();
												if (ProcessItemMagicLearningEffect() == 0)
												{
													localUState = 2;
												}
											}
										}
										else if (menu.MenuManager.getSingleton().GetDirectionKeyState() != 0 || menu.MenuManager.getSingleton().checkTouchState() != menu.MenuManager.TOUCH_STATE.NOT_TOUCH)
										{
											ProcessHelpWindow();
										}
										break;
									case 2:
										localUState = 0;
										localState = 0;
										menu.MenuManager.getSingleton().releaseWindow(windowNo[2]);
										TouchAreaToRemoveRect();
										if (itemNoneState)
										{
											if (pItemWindow != null)
											{
												pItemWindow.TargetOneMsgDelete(pItemWindow.mbGetOwner(), used_item_no);
												pItemWindow.ResetTargetItemID();
											}
											itemNoneState = false;
											ProcessHelpWindow();
										}
										CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
										menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: false);
										GX_Power3D(0);
										GX_SetPriority3D(0);
										stageMng.setHidden(flag: false);
										wld.WorldPart.getInstance().getScene().setCamera(wld.WorldPart.getInstance().getWorldSystem().WorldCamera());
										wld.WorldPart.getInstance().getWorldSystem().WorldCamera()
											.setActivity(b: true);
										break;
									}
								}

								public bool CheckPushItemUsePosible()
								{
									if (itm.ItemManager.instance().itemParameter((short)menu.MenuManager.getSingleton().GetTargetItemNo()) == null)
									{
										return false;
									}
									if (itm.ItemManager.instance().itemParameter((short)menu.MenuManager.getSingleton().GetTargetItemNo()).useField() != 0)
									{
										itm.CATEGORY cATEGORY = itm.ItemManager.instance().itemCategory((short)menu.MenuManager.getSingleton().GetTargetItemNo());
										if (cATEGORY == itm.CATEGORY.CATEGORY_IMPORTANT)
										{
											return false;
										}
										return true;
									}
									itm.CATEGORY cATEGORY2 = itm.ItemManager.instance().itemCategory((short)menu.MenuManager.getSingleton().GetTargetItemNo());
									if (cATEGORY2 == itm.CATEGORY.CATEGORY_MAGIC)
									{
										return true;
									}
									return false;
								}

								public int PushItemCategory()
								{
									return itm.ItemManager.instance().itemCategory((short)menu.MenuManager.getSingleton().GetTargetItemNo()) switch
									{
										itm.CATEGORY.CATEGORY_CONSUMPTION => 0, 
										itm.CATEGORY.CATEGORY_MAGIC => 1, 
										_ => 2, 
									};
								}

								public bool ProcessItemEffect()
								{
									int num = selectItemNo;
									int targetCharNo = menu.MenuManager.getSingleton().GetTargetCharNo();
									itm.ItemUse itemUse = new itm.ItemUse();
									OS_Printf(" 回復前：プレイヤーの体力: %d \n ", pl.PlayerParty.instance().player(0).hp()
										.getNow());
									if (!itemUse.useItemInField(selectItemNo, targetCharNo))
									{
										menu.MenuManager.getSingleton().playSEBeep();
										return true;
									}
									menu.Medget medget = menu.MenuManager.getSingleton().getFocuseMedget().parentNode();
									if (medget != null)
									{
										((menu.MBItemUse)medget.behavior().queryInterface(menu.MBItemUse.classIdentifier()))?.UpdateConditionLife(medget);
									}
									OS_Printf(" 回復後：プレイヤーの体力: %d \n ", pl.PlayerParty.instance().player(0).hp()
										.getNow());
									int num2 = pl.PlayerParty.instance().item().normalItem(effectItemNo)
										.itemNumber();
									pl.PlayerParty.instance().item().normalItem(effectItemNo)
										.setItemNumber(--num2);
									used_item_no = num;
									used_item_num = num2;
									if (dDecidePrevMedget != null && pItemWindow != null)
									{
										pItemWindow.TargetMsgNumReset(dDecidePrevMedget.parentNode(), used_item_no, 0);
									}
									if (num2 <= 0)
									{
										itemNoneState = true;
										return false;
									}
									return true;
								}

								public int ProcessItemMagicLearningEffect()
								{
									int num = selectItemNo;
									int level = itm.ItemManager.instance().magicParameter((short)num).magicClass();
									int targetCharNo = menu.MenuManager.getSingleton().GetTargetCharNo();
									for (int i = 0; i < 3; i++)
									{
										int num2 = pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
											.equipMagic((pl.MAGIC_LEVEL)level)
											.magicId(i);
										if (num == num2)
										{
											menu.MenuManager.getSingleton().playSEBeep();
											ProcessHelpWindowVerErr(50070);
											prevCharNo_ = targetCharNo;
											return 2;
										}
									}
									int num3 = pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
										.equipMagic((pl.MAGIC_LEVEL)level)
										.equip(num);
									if (num3 == -999)
									{
										menu.MenuManager.getSingleton().playSEBeep();
										ProcessHelpWindowVerErr(50071);
										prevCharNo_ = targetCharNo;
										return 2;
									}
									menu.MenuManager.getSingleton().playSEDecide();
									int num4 = pl.PlayerParty.instance().item().normalItem(effectItemNo)
										.itemNumber();
									pl.PlayerParty.instance().item().normalItem(effectItemNo)
										.setItemNumber(--num4);
									used_item_no = num;
									used_item_num = num4;
									if (dDecidePrevMedget != null && pItemWindow != null)
									{
										pItemWindow.TargetMsgNumReset(dDecidePrevMedget.parentNode(), used_item_no, 0);
									}
									if (num4 <= 0)
									{
										itemNoneState = true;
										return 0;
									}
									return 2;
								}

								public void TouchAreaToAppendRect()
								{
									menu.Medget medget = null;
									if (pItemWindow != null)
									{
										pItemWindow.mbPause();
									}
									menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: false);
									if (appendType == 0)
									{
										dgs.msg.CMessageSys.getInstance().Sub().dgsMMAreaErase(240, 24, 240, 248);
										if (pItemWindow != null)
										{
											pItemWindow.bmAreaSuspend(menu.MenuManager.getSingleton().getFocuseMedget().parentNode(), 1);
										}
										medget = pMainMedget.getNodeByID(TRANSCODE("item_list"));
										if (medget != null)
										{
											CWMenuManager.Instance().ChainLeaveFocuseList(medget.childNode());
											pAppendAddr = menu.MenuManager.getSingleton().Append("UseItemCommand_Right");
										}
									}
									else if (appendType == 1)
									{
										dgs.msg.CMessageSys.getInstance().Sub().dgsMMAreaErase(0, 24, 240, 248);
										if (pItemWindow != null)
										{
											pItemWindow.bmAreaSuspend(menu.MenuManager.getSingleton().getFocuseMedget().parentNode(), 0);
										}
										medget = pMainMedget.getNodeByID(TRANSCODE("item_list"));
										if (medget != null)
										{
											CWMenuManager.Instance().ChainLeaveFocuseList(medget.childNode());
											pAppendAddr = menu.MenuManager.getSingleton().Append("UseItemCommand_Left");
										}
									}
								}

								public void TouchAreaToRemoveRect()
								{
									dummyCursor.SetShow(show: false);
									if (dDecidePrevMedget != null)
									{
										menu.MenuManager.getSingleton().initFocus(dDecidePrevMedget.myTag());
										dDecidePrevMedget = null;
									}
									if (pAppendAddr != null)
									{
										menu.MenuManager.getSingleton().Remove(pAppendAddr);
										pAppendAddr = null;
									}
									menu.Medget nodeByID = pMainMedget.getNodeByID(TRANSCODE("item_list"));
									if (nodeByID != null)
									{
										CWMenuManager.Instance().ChainLeaveFocuseList(nodeByID.childNode());
										CWMenuManager.Instance().ChainJoinFocuseList(nodeByID.childNode());
									}
									menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: false);
									if (pItemWindow != null)
									{
										pItemWindow.mbRestart();
									}
									if (appendType == 0)
									{
										if (pItemWindow != null)
										{
											pItemWindow.bmAreaResume(nodeByID, 1);
										}
									}
									else if (pItemWindow != null)
									{
										pItemWindow.bmAreaResume(nodeByID, 0);
									}
								}

								public void ChangeItemPatern()
								{
									int num = 0;
									int num2 = 0;
									num = dummyInBoxNo;
									num2 = (int)menu.MenuManager.getSingleton().getFocuseMedget().work();
									itm.PossessionItem arg = pl.PlayerParty.instance().item().normalItem(num);
									pl.PlayerParty.instance().item().normalItem_set(num, pl.PlayerParty.instance().item().normalItem(num2));
									pl.PlayerParty.instance().item().normalItem_set(num2, arg);
									if (pItemWindow != null)
									{
										pItemWindow.RefreshTargetMsg(num, num2);
									}
								}

								public void SeitonItemListRefresh()
								{
									menu.MenuManager.getSingleton().SetItemListPatern(0);
									if (pItemWindow != null)
									{
										pItemWindow.RefreshList();
										pItemWindow.mbPause();
									}
								}

								public void SeitonItemSort()
								{
									aCount = (nCount = (wCount = (mCount = (iCount = 0))));
									for (int i = 0; i < 384; i++)
									{
										int num = pl.PlayerParty.instance().item().normalItem(i)
											.itemId();
										if (num > 0)
										{
											switch (itm.ItemManager.instance().itemCategory((short)num))
											{
											case itm.CATEGORY.CATEGORY_CONSUMPTION:
												nItem[nCount].itemID = num;
												nItem[nCount].itemNum = pl.PlayerParty.instance().item().normalItem(i)
													.itemNumber();
												nCount++;
												break;
											case itm.CATEGORY.CATEGORY_IMPORTANT:
												iItem[iCount].itemID = num;
												iItem[iCount].itemNum = pl.PlayerParty.instance().item().normalItem(i)
													.itemNumber();
												iCount++;
												break;
											case itm.CATEGORY.CATEGORY_WEAPON:
												wItem[wCount].itemID = num;
												wItem[wCount].itemNum = pl.PlayerParty.instance().item().normalItem(i)
													.itemNumber();
												wCount++;
												break;
											case itm.CATEGORY.CATEGORY_PROTECTION:
												aItem[aCount].itemID = num;
												aItem[aCount].itemNum = pl.PlayerParty.instance().item().normalItem(i)
													.itemNumber();
												aCount++;
												break;
											case itm.CATEGORY.CATEGORY_MAGIC:
												mItem[mCount].itemID = num;
												mItem[mCount].itemNum = pl.PlayerParty.instance().item().normalItem(i)
													.itemNumber();
												mCount++;
												break;
											}
										}
									}
									for (int j = 0; j < 384; j++)
									{
										pl.PlayerParty.instance().item().normalItem(j)
											.setItemId(-1);
										pl.PlayerParty.instance().item().normalItem(j)
											.setItemNumber(0);
									}
									if (nCount > 1)
									{
										Sort(nItem, 0, nCount - 1);
									}
									if (wCount > 1)
									{
										Sort(wItem, 0, wCount - 1);
									}
									if (aCount > 1)
									{
										Sort(aItem, 0, aCount - 1);
									}
									if (mCount > 1)
									{
										Sort(mItem, 0, mCount - 1);
									}
									if (iCount > 1)
									{
										Sort(iItem, 0, iCount - 1);
									}
									switch (seitonPatern)
									{
									case 0:
										SeitonTopItem();
										break;
									case 1:
										SeitonTopWeapon();
										break;
									case 2:
										SeitonTopArmer();
										break;
									}
								}

								public void SeitonTopItem()
								{
									if (nCount > 0)
									{
										for (int i = 0; i < nCount; i++)
										{
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemId((short)nItem[i].itemID);
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemNumber(nItem[i].itemNum);
											sortCount++;
										}
									}
									if (iCount > 0)
									{
										for (int j = 0; j < iCount; j++)
										{
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemId((short)iItem[j].itemID);
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemNumber(iItem[j].itemNum);
											sortCount++;
										}
									}
									if (mCount > 0)
									{
										for (int k = 0; k < mCount; k++)
										{
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemId((short)mItem[k].itemID);
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemNumber(mItem[k].itemNum);
											sortCount++;
										}
									}
									if (wCount > 0)
									{
										for (int l = 0; l < wCount; l++)
										{
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemId((short)wItem[l].itemID);
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemNumber(wItem[l].itemNum);
											sortCount++;
										}
									}
									if (aCount > 0)
									{
										for (int m = 0; m < aCount; m++)
										{
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemId((short)aItem[m].itemID);
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemNumber(aItem[m].itemNum);
											sortCount++;
										}
									}
								}

								public void SeitonTopWeapon()
								{
									if (wCount > 0)
									{
										for (int i = 0; i < wCount; i++)
										{
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemId((short)wItem[i].itemID);
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemNumber(wItem[i].itemNum);
											sortCount++;
										}
									}
									if (aCount > 0)
									{
										for (int j = 0; j < aCount; j++)
										{
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemId((short)aItem[j].itemID);
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemNumber(aItem[j].itemNum);
											sortCount++;
										}
									}
									if (nCount > 0)
									{
										for (int k = 0; k < nCount; k++)
										{
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemId((short)nItem[k].itemID);
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemNumber(nItem[k].itemNum);
											sortCount++;
										}
									}
									if (iCount > 0)
									{
										for (int l = 0; l < iCount; l++)
										{
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemId((short)iItem[l].itemID);
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemNumber(iItem[l].itemNum);
											sortCount++;
										}
									}
									if (mCount > 0)
									{
										for (int m = 0; m < mCount; m++)
										{
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemId((short)mItem[m].itemID);
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemNumber(mItem[m].itemNum);
											sortCount++;
										}
									}
								}

								public void SeitonTopArmer()
								{
									if (aCount > 0)
									{
										for (int i = 0; i < aCount; i++)
										{
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemId((short)aItem[i].itemID);
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemNumber(aItem[i].itemNum);
											sortCount++;
										}
									}
									if (nCount > 0)
									{
										for (int j = 0; j < nCount; j++)
										{
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemId((short)nItem[j].itemID);
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemNumber(nItem[j].itemNum);
											sortCount++;
										}
									}
									if (iCount > 0)
									{
										for (int k = 0; k < iCount; k++)
										{
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemId((short)iItem[k].itemID);
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemNumber(iItem[k].itemNum);
											sortCount++;
										}
									}
									if (mCount > 0)
									{
										for (int l = 0; l < mCount; l++)
										{
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemId((short)mItem[l].itemID);
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemNumber(mItem[l].itemNum);
											sortCount++;
										}
									}
									if (wCount > 0)
									{
										for (int m = 0; m < wCount; m++)
										{
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemId((short)wItem[m].itemID);
											pl.PlayerParty.instance().item().normalItem(sortCount)
												.setItemNumber(wItem[m].itemNum);
											sortCount++;
										}
									}
								}

								public void Sort(ITEM_SEITON[] pTarget, int left, int right)
								{
									if (left >= right)
									{
										return;
									}
									int num = (left + right) / 2;
									ITEM_SEITON iTEM_SEITON = pTarget[num];
									pTarget[num] = pTarget[left];
									int num2 = left;
									for (int i = left + 1; i <= right; i++)
									{
										if (pTarget[i].itemID < iTEM_SEITON.itemID)
										{
											num2++;
											Swap(pTarget, num2, i);
										}
									}
									pTarget[left] = pTarget[num2];
									pTarget[num2] = iTEM_SEITON;
									Sort(pTarget, left, num2 - 1);
									Sort(pTarget, num2 + 1, right);
								}

								public void Swap(ITEM_SEITON[] pTarget, int i, int j)
								{
									ITEM_SEITON iTEM_SEITON = pTarget[i];
									pTarget[i] = pTarget[j];
									pTarget[j] = iTEM_SEITON;
								}

								public void ChangeFocuseComToList()
								{
									menu.Medget nodeByID = pMainMedget.getNodeByID(TRANSCODE("item_list"));
									for (menu.Medget medget = nodeByID.childNode(); medget != null; medget = medget.nextSibling())
									{
										menu.MenuManager.getSingleton().joinFocusList(medget);
									}
									menu.MenuManager.getSingleton().initFocus(3);
									if (nodeByID.behavior() != null)
									{
										nodeByID.behavior().bmActivate(nodeByID);
									}
								}

								public void ChangeFocuseListToCom()
								{
									menu.Medget medget = menu.MenuManager.getSingleton().getFocuseMedget().parentNode();
									for (menu.Medget medget2 = medget.childNode(); medget2 != null; medget2 = medget2.nextSibling())
									{
										menu.MenuManager.getSingleton().leaveFocusList(medget2);
									}
									menu.MenuManager.getSingleton().initFocus(prev_focus_group);
									if (medget.behavior() != null)
									{
										medget.behavior().bmDeactivate(medget);
									}
								}

								public CWMenuItem()
								{
									for (int i = 0; i < masterCard.Length; i++)
									{
										masterCard[i] = new sys2d.Cell();
									}
									for (int i = 0; i < nItem.Length; i++)
									{
										nItem[i] = new ITEM_SEITON();
									}
									for (int i = 0; i < wItem.Length; i++)
									{
										wItem[i] = new ITEM_SEITON();
									}
									for (int i = 0; i < aItem.Length; i++)
									{
										aItem[i] = new ITEM_SEITON();
									}
									for (int i = 0; i < mItem.Length; i++)
									{
										mItem[i] = new ITEM_SEITON();
									}
									for (int i = 0; i < iItem.Length; i++)
									{
										iItem[i] = new ITEM_SEITON();
									}
								}

								public int GetEffectNo()
								{
									return effectItemNo;
								}
							}
	}
}
