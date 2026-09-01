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
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
						public class ChocoboBank : wld.CParallelWorldSystem, menu.MenuBehavedNotifier
						{
							public static ChocoboBank instance_ = new ChocoboBank();

							private int helpNo;

							private dgs.DGSMessage pHelpMsg;

							private sys2d.Bg bg_ = new sys2d.Bg();

							private sys2d.Cell dummyCursor_ = new sys2d.Cell();

							private bool endFlag_;

							private bool change_;

							private int step_;

							private int topcursor_;

							private menu.MBItemWindow itemWindow_;

							public ChocoboBank()
							{
								endFlag_ = false;
								step_ = 0;
							}

							public override void initialize(wld.CBaseSystem BS)
							{
								evt.CEventManager.getInstance().setEventStop(_EventStop: true);
								endFlag_ = false;
								change_ = false;
								step_ = 0;
								ds.g_Pad.disable();
								ds.g_TouchPanel.disable();
								dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
								dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
								menu.MenuManager.getSingleton().SetMagicMenuType(2);
								wld.WorldPart.getInstance().getWorldSystem().World2DMng()
									.refWorldMap()
									.hideMapMarker();
								dummyCursor_.copy(menu.MenuManager.getSingleton().GetCursor2d());
								dummyCursor_.SetCell(0);
								dummyCursor_.SetAnimation(anm: true);
								dummyCursor_.SetPriority(0);
								dummyCursor_.SetPositionI(256, 192);
								dummyCursor_.SetShow(show: false);
								sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(dummyCursor_);
							}

							public override void terminate(wld.CBaseSystem BS)
							{
								if (pHelpMsg != null)
								{
									pHelpMsg.release();
									pHelpMsg = null;
									helpNo = -1;
								}
								dummyCursor_.Release();
								sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(dummyCursor_);
								menu.MenuManager.getSingleton().SetMagicMenuType(-1);
								menu.MenuManager.getSingleton().DeleteNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
								menu.MenuManager.getSingleton().release();
								menu.MenuManager.getSingleton().ReleaseMenuDataText();
								menu.MenuManager.getSingleton().ReleaseXbnFile();
								wmenu.CWMenuManager.Instance().terminate();
								evt.CEventManager.getInstance().setEventStop(_EventStop: false);
								menu.MenuManager.getSingleton().inputPermission(b: true);
								changeCompanyDirectory();
								menu.MenuManager.getSingleton().LoadXbnFile("WorldDefine.xbn");
								wld.WorldPart.getInstance().getWorldSystem().World2DMng()
									.refWorldMap()
									.showMapMarker();
								ds.g_Pad.enable();
								ds.g_TouchPanel.enable();
								GX_Power3D(1);
							}

							public bool mbnNotify(menu.MenuBehavior notifier, uint number, uint param)
							{
								if (number != 0)
								{
									return false;
								}
								switch (step_)
								{
								case 3:
								{
									itm.PossessionItem possessionItem3 = pl.PlayerParty.instance().item().serchNormalItem((short)param);
									if (possessionItem3 == null)
									{
										break;
									}
									itm.PossessionItem possessionItem4 = pl.PlayerParty.instance().storedItem().searchItem((short)param);
									if (possessionItem4 != null && possessionItem4.itemNumber() >= 99)
									{
										menu.MenuManager.getSingleton().playSEBeep();
										break;
									}
									if (possessionItem4 == null)
									{
										int itemNumber3 = possessionItem3.itemNumber();
										pl.PlayerParty.instance().storedItem().storeItem((short)param, 1);
										pl.PlayerParty.instance().storedItem().searchItem((short)param)
											.setItemNumber(itemNumber3);
										possessionItem3.setItemNumber(0);
									}
									else
									{
										int num3 = possessionItem3.itemNumber();
										int num4 = possessionItem4.itemNumber();
										if (num3 + num4 > 99)
										{
											int itemNumber4 = num3 + num4 - 99;
											possessionItem4.setItemNumber(99);
											possessionItem3.setItemNumber(itemNumber4);
										}
										else
										{
											possessionItem4.setItemNumber(num3 + num4);
											possessionItem3.setItemNumber(0);
										}
									}
									if (possessionItem3.itemNumber() <= 0)
									{
										itemWindow_.TargetOneMsgDelete(null, (int)param);
										itemWindow_.ResetTargetItemID();
									}
									else
									{
										itemWindow_.TargetMsgNumReset(null, (int)param, 0);
									}
									menu.MenuManager.getSingleton().playSEDecide();
									change_ = true;
									break;
								}
								case 4:
								{
									itm.PossessionItem possessionItem = pl.PlayerParty.instance().storedItem().searchItem((short)param);
									if (possessionItem == null)
									{
										break;
									}
									itm.PossessionItem possessionItem2 = pl.PlayerParty.instance().item().serchNormalItem((short)param);
									if (possessionItem2 != null && possessionItem2.itemNumber() >= 99)
									{
										menu.MenuManager.getSingleton().playSEBeep();
										break;
									}
									if (possessionItem2 == null)
									{
										int itemNumber = possessionItem.itemNumber();
										pl.PlayerParty.instance().item().storeItem((short)param, 1);
										pl.PlayerParty.instance().item().serchNormalItem((short)param)
											.setItemNumber(itemNumber);
										possessionItem.setItemNumber(0);
									}
									else
									{
										int num = possessionItem2.itemNumber();
										int num2 = possessionItem.itemNumber();
										if (num + num2 > 99)
										{
											int itemNumber2 = num + num2 - 99;
											possessionItem2.setItemNumber(99);
											possessionItem.setItemNumber(itemNumber2);
										}
										else
										{
											possessionItem2.setItemNumber(num + num2);
											possessionItem.setItemNumber(0);
										}
									}
									if (possessionItem.itemNumber() <= 0)
									{
										itemWindow_.TargetOneMsgDelete(null, (int)param);
										itemWindow_.ResetTargetItemID();
									}
									else
									{
										itemWindow_.TargetMsgNumReset(null, (int)param, 0);
									}
									menu.MenuManager.getSingleton().playSEDecide();
									change_ = true;
									break;
								}
								case 2:
									menu.MenuManager.getSingleton().playSEDecide();
									break;
								}
								return true;
							}

							public void focusSelection()
							{
								menu.Medget nodeByID = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID(TRANSCODE("item_list"));
								for (menu.Medget medget = nodeByID.childNode(); medget != null; medget = medget.nextSibling())
								{
									itemWindow_.ChangeColorAllString(medget, 1);
								}
								menu.MenuManager.getSingleton().changeFocusGroup(0);
								itemWindow_.mbPause();
								step_ = 2;
								menu.MenuManager.getSingleton().initFocus(topcursor_);
								if (pHelpMsg != null)
								{
									pHelpMsg.release();
									pHelpMsg = null;
									helpNo = -1;
								}
								dummyCursor_.SetShow(show: false);
								itemWindow_.getScrollBar().sbSetCapacity(1, 1);
							}

							public void focusSelectionTouchPen()
							{
								menu.Medget medget = null;
								medget = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID(TRANSCODE("deposit"));
								if (medget != null)
								{
									menu.MenuManager.getSingleton().leaveFocusList(medget);
								}
								medget = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID(TRANSCODE("withdraw"));
								if (medget != null)
								{
									menu.MenuManager.getSingleton().leaveFocusList(medget);
								}
								focusSelection();
							}

							public void focusPossessionList()
							{
								menu.MenuManager.getSingleton().changeFocusGroup(1);
								menu.MenuManager.getSingleton().SetItemListPatern(0);
								if (itemWindow_ != null)
								{
									itemWindow_.RefreshChangeList(null);
								}
								step_ = 3;
								menu.Medget medget = null;
								medget = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID(TRANSCODE("deposit"));
								if (medget != null)
								{
									menu.MenuManager.getSingleton().joinFocusList(medget);
								}
								medget = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID(TRANSCODE("withdraw"));
								if (medget != null)
								{
									menu.MenuManager.getSingleton().joinFocusList(medget);
								}
							}

							public void focusStockList()
							{
								menu.MenuManager.getSingleton().changeFocusGroup(1);
								menu.MenuManager.getSingleton().SetItemListPatern(10);
								if (itemWindow_ != null)
								{
									itemWindow_.RefreshChangeList(null);
								}
								step_ = 4;
								menu.Medget medget = null;
								medget = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID(TRANSCODE("deposit"));
								if (medget != null)
								{
									menu.MenuManager.getSingleton().joinFocusList(medget);
								}
								medget = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID(TRANSCODE("withdraw"));
								if (medget != null)
								{
									menu.MenuManager.getSingleton().joinFocusList(medget);
								}
							}

							public override bool execute(wld.CBaseSystem BS)
							{
								switch (step_)
								{
								case 0:
								{
									if (!dgs.CFade.Main().isFaded() || !dgs.CFade.Sub().isFaded())
									{
										break;
									}
									TexDivideLoader.getSingleton().tdlCancel();
									eld.g_elsvr.eraseObjects();
									GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_MAIN_SUB);
									GX_Power3D(0);
									BS.World2DMng().terminate();
									changeGlobalDirectory();
									bg_.bgLoad("chocobo_bank.NSCR", "menu_bg_01.NCGR", "new_menu_bg.NCLR");
									bg_.bgSetUp(NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB3);
									bg_.bgRelease();
									bg_.bgSetShow(show: true);
									changeCompanyDirectory();
									menu.MenuManager.getSingleton().SetItemListPatern(0);
									changeGlobalDirectory();
									menu.MenuManager.getSingleton().LoadXbnFile("ChocoboBank.xbn");
									menu.MenuManager.getSingleton().Set2d3dMode(2);
									changeCompanyDirectory();
									menu.MenuManager.getSingleton().CreateMenuDataText(0);
									changeGlobalDirectory();
									menu.MenuManager.getSingleton().CreateNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_2D);
									menu.MenuManager.getSingleton().SetUsingMenuType(1);
									menu.MenuManager.getSingleton().buildMenu("chocobo_bank");
									wmenu.CWMenuManager.Instance().GetMenuButton().initialize();
									wmenu.CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: true);
									menu.MenuManager.getSingleton().SetDecideButtonState(1);
									menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
									wmenu.CWMenuManager.Instance().SetUpDummyCursor(256, 192, act: false);
									sys2d.DS2DManager.d2dGetInstance().d2dUpdate();
									menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("item_list"));
									if (nodeByID != null)
									{
										itemWindow_ = (menu.MBItemWindow)nodeByID.behavior().queryInterface(menu.MBItemWindow.classIdentifier());
										if (itemWindow_ != null)
										{
											itemWindow_.mbSetNotifier(this);
											itemWindow_.mbPause();
										}
									}
									ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: true, obj: true);
									dgs.CFade.Main().fadeIn(15);
									dgs.CFade.Sub().fadeIn(15);
									dummyCursor_.SetPositionI(menu.MenuManager.getSingleton().getFocuseMedget().cursorX(), menu.MenuManager.getSingleton().getFocuseMedget().cursorY());
									focusPossessionList();
									topcursor_ = 0;
									dummyCursor_.SetShow(show: true);
									itemWindow_.mbRestart();
									step_ = 1;
									break;
								}
								case 1:
									if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
									{
										ds.g_Pad.enable();
										ds.g_TouchPanel.enable();
										step_ = 3;
									}
									break;
								case 2:
									menu.MenuManager.getSingleton().execute();
									if ((ds.g_Pad.edge() & 1) != 0 || menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
									{
										dummyCursor_.SetPositionI(menu.MenuManager.getSingleton().getFocuseMedget().cursorX(), menu.MenuManager.getSingleton().getFocuseMedget().cursorY());
										switch (menu.MenuManager.getSingleton().getFocuseMedget().myTag())
										{
										case 0:
											focusPossessionList();
											topcursor_ = 0;
											break;
										case 1:
											focusStockList();
											topcursor_ = 1;
											break;
										}
										dummyCursor_.SetShow(show: true);
										itemWindow_.mbRestart();
										menu.MenuManager.getSingleton().playSEDecide();
									}
									else if ((ds.g_Pad.edge() & 2) != 0 || wmenu.CWMenuManager.Instance().GetMenuButton().TouchButtonB())
									{
										ds.g_Pad.disable();
										ds.g_TouchPanel.disable();
										dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
										dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
										step_ = 5;
										menu.MenuManager.getSingleton().playSECancel();
									}
									menu.MenuManager.getSingleton().SetDecideButtonState(1);
									menu.MenuManager.getSingleton().SetCancelButtonState(1);
									break;
								case 3:
								case 4:
								{
									bool flag = true;
									OS_AssignBackButton(1);
									menu.MenuManager.getSingleton().execute();
									if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
									{
										if (!change_)
										{
											menu.MenuManager.getSingleton().playSEBeep();
										}
									}
									else if (menu.MenuManager.getSingleton().GetActivateButtonState() == 0)
									{
										if (menu.MenuManager.getSingleton().getFocuseMedget()._id(TRANSCODE("deposit")))
										{
											dummyCursor_.SetPositionI(menu.MenuManager.getSingleton().getFocuseMedget().cursorX(), menu.MenuManager.getSingleton().getFocuseMedget().cursorY());
											topcursor_ = 0;
											focusPossessionList();
											flag = false;
										}
										else if (menu.MenuManager.getSingleton().getFocuseMedget()._id(TRANSCODE("withdraw")))
										{
											dummyCursor_.SetPositionI(menu.MenuManager.getSingleton().getFocuseMedget().cursorX(), menu.MenuManager.getSingleton().getFocuseMedget().cursorY());
											topcursor_ = 1;
											focusStockList();
											flag = false;
										}
									}
									if ((ds.g_Pad.edge() & 2) != 0 || menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || wmenu.CWMenuManager.Instance().GetMenuButton().TouchButtonB())
									{
										ds.g_Pad.disable();
										ds.g_TouchPanel.disable();
										dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
										dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
										step_ = 5;
										menu.MenuManager.getSingleton().playSECancel();
										flag = false;
									}
									if (flag)
									{
										ProcessHelpWindow();
									}
									menu.MenuManager.getSingleton().SetDecideButtonState(1);
									menu.MenuManager.getSingleton().SetCancelButtonState(1);
									change_ = false;
									break;
								}
								case 5:
									if (dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
									{
										if (pHelpMsg != null)
										{
											pHelpMsg.release();
											pHelpMsg = null;
											helpNo = -1;
										}
										dummyCursor_.Release();
										sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(dummyCursor_);
										bg_.bgSetShow(show: false);
										wmenu.CWMenuManager.Instance().GetMenuButton().terminate();
										menu.MenuManager.getSingleton().DeleteNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
										menu.MenuManager.getSingleton().release();
										menu.MenuManager.getSingleton().ReleaseMenuDataText();
										menu.MenuManager.getSingleton().ReleaseXbnFile();
										wmenu.CWMenuManager.Instance().terminate();
										GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_SUB_MAIN);
										GX_Power3D(1);
										dgs.CFade.Main().fadeIn(15);
										dgs.CFade.Sub().fadeIn(15);
										BS.World2DMng().initialize();
										BS.World2DMng().setShowOnePicture(b: false);
										step_ = 6;
									}
									break;
								case 6:
									if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
									{
										return false;
									}
									break;
								}
								return true;
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

							public static ChocoboBank getSingleton()
							{
								return instance_;
							}
						}
}
