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
							public class CWMenuMagic : CWMenuMemberBase
							{
								public const int MAGIC_WINDOW_LIST = 0;

								public const int MAGIC_WINDOW_HELP = 1;

								public const int MAGIC_WINDOW_USE = 2;

								public const int MAGIC_WINDOW_STRING = 3;

								public const int MAGIC_WINDOW_MAX = 4;

								public const int APPEND_RIGHT = 0;

								public const int APPEND_LEFT = 1;

								private int helpNo;

								private dgs.DGSMessage pHelpMsg;

								private int appendType;

								private menu.Medget pAppendAddr;

								private menu.Medget pMainMedget;

								private menu.Medget pPrevDecideMedget;

								private sys2d.Cell dummyCursor = new sys2d.Cell();

								private int myProcess;

								private int localState;

								private int localSubState;

								private int prev_focuse_group;

								private int command_cursor_memory;

								private int buildMode;

								private int itemNo;

								private int prevMagicLv;

								private int prevMagicKoumoku;

								private int sMagicLv;

								private int width_no;

								private int err_no;

								private int playerIndex;

								private int playerMCount;

								private int[] playerBox = new int[4];

								private bool m_state;

								private int[] windowNo = new int[4];

								private int slideTime;

								private int slideDir;

								private ds.sys3d.CCamera Camera_ = new ds.sys3d.CCamera();

								public override bool cSelectInitialize()
								{
									return true;
								}

								public override void initialize()
								{
									setFinalize(val: false);
									SVC_WaitVBlankIntr();
									menu.MenuManager.getSingleton().SetTargetItemNo(-1);
									menu.MenuManager.getSingleton().SetItemListPatern(2);
									menu.MenuManager.getSingleton().buildMenu("magic");
									for (int i = 0; i < 4; i++)
									{
										CWMenuManager.Instance().SetShowPcFace(i, show: false);
									}
									CWMenuManager.Instance().SetPrimaryBG(1);
									CWMenuManager.Instance().SetSecondlyBG(0);
									CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
									menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
									dummyCursor.copy(menu.MenuManager.getSingleton().GetCursor2d());
									dummyCursor.SetShow(show: false);
									dummyCursor.SetPriority(1);
									dummyCursor.SetCell(1);
									dummyCursor.SetPositionI(256, 192);
									dummyCursor.SetAnimation(anm: false);
									sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(dummyCursor);
									CWMenuManager.Instance().GetDummyCursor().SetCell(0);
									CWMenuManager.Instance().GetDummyCursor().PlayAnimation(0, NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD);
									CWMenuManager.Instance().GetDummyCursor().SetAnimation(anm: true);
									menu.MenuManager.getSingleton().GetCursor2d().PlayAnimation(0, NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD);
									pMainMedget = menu.MenuManager.getSingleton().GetBaseMedget();
									m_state = true;
									helpNo = -1;
									localSubState = 0;
									localState = 0;
									pHelpMsg = null;
									playerMCount = 0;
									playerIndex = 0;
									for (int j = 0; j < 4; j++)
									{
										if (pl.PlayerParty.instance().player((byte)j).isEnable())
										{
											playerBox[j] = pl.PlayerParty.instance().player((byte)j).playerId();
											playerMCount++;
										}
										else
										{
											playerBox[j] = -1;
										}
									}
									playerIndex = menu.MenuManager.getSingleton().GetTargetCharNo();
									if (playerMCount <= 1)
									{
										CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
									}
									else
									{
										CWMenuManager.Instance().GetMenuButton().SetUpNormalVer();
									}
									slideTime = 0;
									slideDir = 0;
									bmRefresh(playerIndex);
									command_cursor_memory = 0;
									localState = 0;
									myProcess = 0;
									menu.MenuManager.getSingleton().SetMagicMenuType(myProcess);
									menu.Medget nodeByID = pMainMedget.getNodeByID(TRANSCODE("m_use"));
									CWMenuManager.Instance().GetDummyCursor().SetPositionI(nodeByID.cursorX(), nodeByID.cursorY());
									CWMenuManager.Instance().GetDummyCursor().SetShow(show: true);
									command_cursor_memory = 0;
									ChangeFocuseComToList();
									menu.MBMagicPram mBMagicPram = (menu.MBMagicPram)pMainMedget.getNodeByID(TRANSCODE("magic_list")).behavior().queryInterface(menu.MBMagicPram.classIdentifier());
									if (mBMagicPram != null)
									{
										mBMagicPram.SetCurrentPlayer(playerIndex);
										mBMagicPram.mbRestart();
									}
									helpNo = 0;
									m_state = false;
									menu.MenuManager.getSingleton().setNotSEFlag(flag: false);
									menu.MenuManager.getSingleton().SetActivateButtonState(1);
									CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_RUN);
								}

								public override void run()
								{
									prev_focuse_group = menu.MenuManager.getSingleton().getFocuseMedget().myTag();
									menu.MenuManager.getSingleton().setNotSEFlag(flag: true);
									menu.MenuManager.getSingleton().execute();
									menu.MenuManager.getSingleton().setNotSEFlag(flag: false);
									if (slideDir != 0)
									{
										slideTime++;
										G2_SetScreenOffset(((slideTime < 4) ? slideTime : (slideTime - 8)) * slideDir * 120, 0);
										if (slideTime == 4)
										{
											menu.MenuManager.getSingleton().SetTargetCharNo(playerIndex);
											menu.MenuManager.getSingleton().releaseWindowAll();
											menu.Medget nodeByID = pMainMedget.getNodeByID(TRANSCODE("magic_list"));
											if (nodeByID != null)
											{
												menu.MBMagicPram mBMagicPram = (menu.MBMagicPram)nodeByID.behavior().queryInterface(menu.MBMagicPram.classIdentifier());
												if (mBMagicPram != null)
												{
													mBMagicPram.SetCurrentPlayer(playerIndex);
													mBMagicPram.ResettingMagic(nodeByID, playerIndex);
												}
											}
											if (playerMCount <= 1)
											{
												CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
											}
											else
											{
												CWMenuManager.Instance().GetMenuButton().SetUpNormalVer();
											}
											bmRefresh(playerIndex);
										}
										if (slideTime == 8)
										{
											menu.MenuManager.getSingleton().inputPermission(b: true);
											slideTime = 0;
											slideDir = 0;
										}
										return;
									}
									switch (myProcess)
									{
									case 0:
										MagicUse();
										break;
									case 1:
										MagicLearning();
										break;
									case 2:
										MagicRemove();
										break;
									case 3:
										MagicChange();
										break;
									}
									// PORT: X / Y turn the Use / Learn / Remove / Change tabs for a pad or a keyboard (L / R
									// turn the character here, as on the DS), in the states where a tap on a tab is taken.
									if (!m_state && (myProcess == 0 || myProcess == 2 || (myProcess == 1 && localState == 3) || (myProcess == 3 && localState == 2)))
									{
										TurnTabs(myProcess, TAB_PREV_BUTTON, TAB_NEXT_BUTTON);
									}
									if (myProcess == 1)
									{
										if (localState == 3 && (m_state || CheckTouchSlotSelect()))
										{
											CWMenuManager.Instance().GetDummyCursor().SetShow(show: false);
											ProcessTouchSlotSelect();
										}
									}
									else if (myProcess == 3)
									{
										if (localState == 2 && (m_state || CheckTouchSlotSelect()))
										{
											CWMenuManager.Instance().GetDummyCursor().SetShow(show: false);
											ProcessTouchSlotSelect();
										}
									}
									else if (m_state || CheckTouchSlotSelect())
									{
										CWMenuManager.Instance().GetDummyCursor().SetShow(show: false);
										ProcessTouchSlotSelect();
									}
									if (m_state)
									{
										myProcess = 100;
									}
									menu.MenuManager.getSingleton().SetMagicMenuType(myProcess);
									if (!m_state && myProcess != 1 && myProcess != 3 && ((ds.g_Pad.edge() & 0x200) != 0 || (ds.g_Pad.edge() & 0x100) != 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonL() || CWMenuManager.Instance().GetMenuButton().TouchButtonR()))
									{
										ProcessLorRButtonPush();
									}
									if (m_state)
									{
										switch (menu.MenuManager.getSingleton().getFocuseMedget().myTag())
										{
										case 0:
											myProcess = 0;
											localState = 0;
											break;
										case 1:
											myProcess = 1;
											localState = 0;
											break;
										case 2:
											myProcess = 2;
											localState = 0;
											break;
										case 3:
											myProcess = 3;
											localState = 0;
											break;
										}
										menu.MenuManager.getSingleton().SetMagicMenuType(myProcess);
										CWMenuManager.Instance().GetDummyCursor().SetPositionI(menu.MenuManager.getSingleton().getFocuseMedget().cursorX(), menu.MenuManager.getSingleton().getFocuseMedget().cursorY());
										CWMenuManager.Instance().GetDummyCursor().SetPriority(0);
										CWMenuManager.Instance().GetDummyCursor().SetShow(show: true);
										command_cursor_memory = menu.MenuManager.getSingleton().getFocuseMedget().myTag();
										if (myProcess != 3 && myProcess != 1)
										{
											ChangeFocuseComToList();
											((menu.MBMagicPram)pMainMedget.getNodeByID(TRANSCODE("magic_list")).behavior().queryInterface(menu.MBMagicPram.classIdentifier()))?.mbRestart();
										}
										else
										{
											menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
											CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
										}
										helpNo = 0;
										m_state = false;
									}
									menu.MenuManager.getSingleton().SetActivateButtonState(1);
									menu.MenuManager.getSingleton().ClearBehaviorButton();
								}

								public override void terminate()
								{
									menu.MenuManager.getSingleton().setNotSEFlag(flag: false);
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
										sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(dummyCursor);
										dummyCursor.Release();
										CWMenuManager.Instance().GetDummyCursor().SetShow(show: false);
										CWMenuManager.Instance().GetDummyCursor().SetAnimation(anm: false);
										CWMenuManager.Instance().GetDummyCursor().SetCell(3);
										menu.MenuManager.getSingleton().releaseWindowAll();
										menu.MenuManager.getSingleton().release();
										menu.MenuManager.getSingleton().SetMagicMenuType(-1);
										setFinalize(val: true);
									}
								}

								public bool CheckTouchSlotSelect()
								{
									if (menu.MenuManager.getSingleton().GetActivateButtonState() == 0 && strcmp(menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
										._id(), "mm_command") == 0)
									{
										int num = -1;
										switch (myProcess)
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
										case 3:
											num = 3;
											break;
										}
										if (menu.MenuManager.getSingleton().getFocuseMedget().myTag() == num)
										{
											menu.MenuManager.getSingleton().initFocus(prev_focuse_group);
											return false;
										}
										return true;
									}
									return false;
								}

								public bool CheckTouchSlotMagicList()
								{
									return false;
								}

								public void ProcessTouchSlotSelect()
								{
									int focusedCursor = menu.MenuManager.getSingleton().getFocuseMedget().myTag();
									m_state = true;
									helpNo = -1;
									localState = 0;
									localSubState = 0;
									if (pHelpMsg != null)
									{
										pHelpMsg.release();
										pHelpMsg = null;
									}
									menu.MBMagicPram mBMagicPram = (menu.MBMagicPram)pMainMedget.getNodeByID(TRANSCODE("magic_list")).behavior().queryInterface(menu.MBMagicPram.classIdentifier());
									TouchAreaToRemoveRect();
									menu.MenuManager.getSingleton().releaseWindowAll();
									dummyCursor.SetShow(show: false);
									ds.Stack<menu.MenuManager.MENU_BACKUP> buildMenuStackLevel = menu.MenuManager.getSingleton().GetBuildMenuStackLevel();
									if (buildMenuStackLevel.size() > 0)
									{
										menu.MenuManager.getSingleton().Pop();
									}
									if (mBMagicPram != null)
									{
										mBMagicPram.SelectAreaResettingData(pMainMedget.getNodeByID(TRANSCODE("magic_list")), playerIndex);
										mBMagicPram.SelectAreaUseCountResetting(pMainMedget.getNodeByID(TRANSCODE("magic_list")));
									}
									mBMagicPram?.mbPause();
									for (menu.Medget medget = pMainMedget.getNodeByID(TRANSCODE("magic_list")).childNode(); medget != null; medget = medget.nextSibling())
									{
										menu.MenuManager.getSingleton().leaveFocusList(medget);
									}
									if (playerMCount <= 1)
									{
										CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
									}
									else
									{
										CWMenuManager.Instance().GetMenuButton().SetUpNormalVer();
									}
									menu.MenuManager.getSingleton().initFocus(focusedCursor);
									CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
									CWMenuManager.Instance().SetPrimaryBG(1);
									GX_Power3D(0);
									GX_SetPriority3D(0);
									stageMng.setHidden(flag: false);
									wld.WorldPart.getInstance().getScene().setCamera(wld.WorldPart.getInstance().getWorldSystem().WorldCamera());
									wld.WorldPart.getInstance().getWorldSystem().WorldCamera()
										.setActivity(b: true);
								}

								public void ProcessLorRButtonPush()
								{
									if (playerMCount == 0 || playerMCount == 1)
									{
										menu.MenuManager.getSingleton().playSEBeep();
									}
									else
									{
										if (!m_state && (myProcess == 3 || myProcess == 1 || (myProcess == 0 && localState != 0)))
										{
											return;
										}
										int num = playerIndex;
										if (CWMenuManager.Instance().GetMenuButton().TouchButtonL() || (ds.g_Pad.edge() & 0x200) != 0)
										{
											do
											{
												if (--playerIndex < 0)
												{
													playerIndex = 3;
												}
											}
											while (-1 == playerBox[playerIndex]);
										}
										else if (CWMenuManager.Instance().GetMenuButton().TouchButtonR() || (ds.g_Pad.edge() & 0x100) != 0)
										{
											do
											{
												if (++playerIndex >= 4)
												{
													playerIndex = 0;
												}
											}
											while (-1 == playerBox[playerIndex]);
										}
										if (num == playerIndex)
										{
											menu.MenuManager.getSingleton().playSEBeep();
											return;
										}
										menu.MenuManager.getSingleton().playSEMoveCursor();
										menu.MenuManager.getSingleton().inputPermission(b: false);
										slideTime = 0;
										slideDir = ((!CWMenuManager.Instance().GetMenuButton().TouchButtonL() && (ds.g_Pad.edge() & 0x200) == 0) ? 1 : (-1));
									}
								}

								public void MagicUse()
								{
									switch (localState)
									{
									case 0:
										if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
										{
											menu.MenuManager.getSingleton().playSECancel();
											CWMenuManager.Instance().SetNextKind(WMENU_KIND.WMENU_KIND_MAIN_MENU);
											CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
										}
										else
										{
											if (menu.MenuManager.getSingleton().GetDecideButtonState() != 0)
											{
												break;
											}
											if (menu.MenuManager.getSingleton().GetTargetItemNo() <= 0)
											{
												OS_Printf("GetTargetItemNo GetTargetItemNo %d\n", menu.MenuManager.getSingleton().GetTargetItemNo());
												menu.MenuManager.getSingleton().playSEBeep();
												return;
											}
											if (evt.CEventRestriction.getSingleton().check(menu.MenuManager.getSingleton().GetTargetItemNo()))
											{
												menu.MenuManager.getSingleton().playSEBeep();
												return;
											}
											if (pl.PlayerParty.instance().player((byte)playerIndex).condition()
												.isFrog() && menu.MenuManager.getSingleton().GetTargetItemNo() != 4005)
											{
												menu.MenuManager.getSingleton().playSEBeep();
												return;
											}
											if (!pl.PlayerParty.instance().player((byte)playerIndex).isUseMagic(menu.MenuManager.getSingleton().GetTargetItemNo(), 0))
											{
												menu.MenuManager.getSingleton().playSEBeep();
												return;
											}
											int level = itm.ItemManager.instance().magicParameter((short)menu.MenuManager.getSingleton().GetTargetItemNo()).magicClass();
											if (pl.PlayerParty.instance().player((byte)playerIndex).mp(level)
												.getNow() <= 0)
											{
												menu.MenuManager.getSingleton().playSEBeep();
												return;
											}
											if (itm.ItemManager.instance().itemParameter((short)menu.MenuManager.getSingleton().GetTargetItemNo()).useField() == 0)
											{
												break;
											}
											if (menu.MenuManager.getSingleton().GetTargetItemNo() == 4008 || menu.MenuManager.getSingleton().GetTargetItemNo() == 4118)
											{
												if (wld.WorldPart.getInstance().getWorldSystem().canEscape())
												{
													wld.WorldPart.getInstance().getWorldSystem().doEscape();
													CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_END);
													pl.PlayerParty.instance().player((byte)playerIndex).mp(level)
														.subNow(1);
													pl.PlayerParty.instance().player((byte)playerIndex).setJobChangeMp(level, (byte)pl.PlayerParty.instance().player((byte)playerIndex).mp(level)
														.getNow());
												}
												else
												{
													menu.MenuManager.getSingleton().playSEBeep();
												}
												return;
											}
											if (menu.MenuManager.getSingleton().GetTargetItemNo() == 4003)
											{
												if (wld.WorldPart.getInstance().getWorldSystem().canSite())
												{
													wld.WorldPart.getInstance().getWorldSystem().doSite();
													CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_END);
													pl.PlayerParty.instance().player((byte)playerIndex).mp(level)
														.subNow(1);
													pl.PlayerParty.instance().player((byte)playerIndex).setJobChangeMp(level, (byte)pl.PlayerParty.instance().player((byte)playerIndex).mp(level)
														.getNow());
												}
												else
												{
													menu.MenuManager.getSingleton().playSEBeep();
												}
												return;
											}
											localState = 1;
											itemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
											dummyCursor.SetPositionI(menu.MenuManager.getSingleton().getFocuseMedget().x(), menu.MenuManager.getSingleton().getFocuseMedget().y() + 4);
											dummyCursor.SetShow(show: true);
											SetUpWindowType();
											pPrevDecideMedget = menu.MenuManager.getSingleton().getFocuseMedget();
											menu.MenuManager.getSingleton().playSEDecide();
											menu.MenuManager.getSingleton().inputPermission(b: false);
										}
										break;
									case 1:
									{
										localState = 2;
										if (pHelpMsg != null)
										{
											pHelpMsg.release();
											pHelpMsg = null;
											helpNo = -100;
										}
										menu.MenuManager.getSingleton().inputPermission(b: true);
										TouchAreaToAppendRect();
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
									case 2:
										Camera_.execute();
										if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
										{
											menu.MenuManager.getSingleton().playSECancel();
											dummyCursor.SetShow(show: false);
											localState = 3;
											TouchAreaToRemoveRect();
										}
										else if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
										{
											if (!ProcessMagicAction())
											{
												dummyCursor.SetShow(show: false);
												TouchAreaToRemoveRect();
												localState = 3;
											}
										}
										else if (strcmp(menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
											._id(), "magic_list") == 0)
										{
											dummyCursor.SetShow(show: false);
											menu.Medget focuseMedget = menu.MenuManager.getSingleton().getFocuseMedget();
											TouchAreaToRemoveRect();
											menu.Medget medget2 = menu.MenuManager.getSingleton().getFocuseMedget().parentNode();
											menu.MBMagicPram mBMagicPram2 = null;
											if (medget2 != null)
											{
												mBMagicPram2 = (menu.MBMagicPram)medget2.behavior().queryInterface(menu.MBMagicPram.classIdentifier());
											}
											((menu.MBMagicPram)pMainMedget.getNodeByID(TRANSCODE("magic_list")).behavior().queryInterface(menu.MBMagicPram.classIdentifier()))?.SelectAreaResettingData(pMainMedget.getNodeByID(TRANSCODE("magic_list")), playerIndex);
											mBMagicPram2.SetCurrentPlayer(playerIndex);
											mBMagicPram2.SelectAreaUseCountResetting(medget2);
											localState = 0;
											menu.MenuManager.getSingleton().initFocus(focuseMedget.myTag());
											CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
											GX_Power3D(0);
											GX_SetPriority3D(0);
											stageMng.setHidden(flag: false);
											wld.WorldPart.getInstance().getScene().setCamera(wld.WorldPart.getInstance().getWorldSystem().WorldCamera());
											wld.WorldPart.getInstance().getWorldSystem().WorldCamera()
												.setActivity(b: true);
										}
										break;
									case 3:
									{
										localState = 0;
										menu.Medget medget = menu.MenuManager.getSingleton().getFocuseMedget().parentNode();
										menu.MBMagicPram mBMagicPram = null;
										if (medget != null)
										{
											mBMagicPram = (menu.MBMagicPram)medget.behavior().queryInterface(menu.MBMagicPram.classIdentifier());
										}
										((menu.MBMagicPram)pMainMedget.getNodeByID(TRANSCODE("magic_list")).behavior().queryInterface(menu.MBMagicPram.classIdentifier()))?.SelectAreaResettingData(pMainMedget.getNodeByID(TRANSCODE("magic_list")), playerIndex);
										mBMagicPram.SetCurrentPlayer(playerIndex);
										mBMagicPram.SelectAreaUseCountResetting(medget);
										CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
										menu.MenuManager.getSingleton().releaseWindow(windowNo[2]);
										GX_Power3D(0);
										GX_SetPriority3D(0);
										stageMng.setHidden(flag: false);
										wld.WorldPart.getInstance().getScene().setCamera(wld.WorldPart.getInstance().getWorldSystem().WorldCamera());
										wld.WorldPart.getInstance().getWorldSystem().WorldCamera()
											.setActivity(b: true);
										break;
									}
									}
									ProcessHelpWindow();
								}

								public bool ProcessMagicAction()
								{
									int num = itm.ItemManager.instance().magicParameter((short)itemNo).magicClass();
									int targetCharNo = menu.MenuManager.getSingleton().GetTargetCharNo();
									itm.ItemUse itemUse = new itm.ItemUse();
									SVC_WaitVBlankIntr();
									if (targetCharNo != 4)
									{
										if (!itemUse.useMagicInField(itemNo, playerIndex, targetCharNo, all: false))
										{
											menu.MenuManager.getSingleton().playSEBeep();
											return true;
										}
										menu.Medget medget = menu.MenuManager.getSingleton().getFocuseMedget().parentNode();
										if (medget != null)
										{
											((menu.MBItemUse)medget.behavior().queryInterface(menu.MBItemUse.classIdentifier()))?.UpdateConditionLife(medget);
										}
									}
									else if (targetCharNo == 4)
									{
										OS_Printf("Magic User %d, Target %d\n", playerIndex, targetCharNo);
										if (!itemUse.useMagicInField(itemNo, playerIndex, targetCharNo, all: true))
										{
											menu.MenuManager.getSingleton().playSEBeep();
											return true;
										}
										menu.Medget medget2 = menu.MenuManager.getSingleton().getFocuseMedget().parentNode();
										if (medget2 != null)
										{
											((menu.MBItemUse)medget2.behavior().queryInterface(menu.MBItemUse.classIdentifier()))?.UpdateConditionLife(medget2);
										}
									}
									pl.PlayerParty.instance().player((byte)playerIndex).mp(num)
										.subNow(1);
									pl.PlayerParty.instance().player((byte)playerIndex).setJobChangeMp(num, (byte)pl.PlayerParty.instance().player((byte)playerIndex).mp(num)
										.getNow());
									OS_Printf("Magic User %d, Target %d [ %d ]\n", playerIndex, targetCharNo, pl.PlayerParty.instance().player((byte)playerIndex).mp(num)
										.getNow());
									if (appendType == 0)
									{
										((menu.MBMagicPram)pMainMedget.getNodeByID(TRANSCODE("magic_list")).behavior().queryInterface(menu.MBMagicPram.classIdentifier()))?.ChangeUseCountMessage(pPrevDecideMedget, num, playerIndex);
									}
									if (pl.PlayerParty.instance().player((byte)playerIndex).mp(num)
										.getNow() <= 0)
									{
										return false;
									}
									return true;
								}

								public void MagicLearning()
								{
									switch (localState)
									{
									case 0:
										prevMagicKoumoku = -1;
										dgs.msg.CMessageSys.getInstance().Sub().dgsMMAreaErase(0, 64, 480, 192);
										CWMenuManager.Instance().SetPrimaryBG(11);
										menu.MenuManager.getSingleton().Push("magic_item_list");
										menu.MenuManager.getSingleton().initFocus(4);
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
										localState = 3;
										break;
									case 3:
										if (menu.MenuManager.getSingleton().GetTargetItemNo() != prevMagicKoumoku)
										{
											ProcessHelpWindow();
										}
										prevMagicKoumoku = menu.MenuManager.getSingleton().GetTargetItemNo();
										if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
										{
											itm.CATEGORY cATEGORY = itm.ItemManager.instance().itemCategory((short)menu.MenuManager.getSingleton().GetTargetItemNo());
											if (cATEGORY != itm.CATEGORY.CATEGORY_MAGIC)
											{
												menu.MenuManager.getSingleton().playSEBeep();
												break;
											}
											switch (ProcessLearningMagic())
											{
											case 0:
											{
												menu.MenuManager.getSingleton().Pop();
												CWMenuManager.Instance().GetDummyCursor().SetShow(show: false);
												menu.MenuManager.getSingleton().initFocus(command_cursor_memory);
												CWMenuManager.Instance().SetPrimaryBG(1);
												localState = 0;
												menu.Medget nodeByID = menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
													.parentNode()
													.getNodeByID(TRANSCODE("magic_list"));
												menu.MBMagicPram mBMagicPram = null;
												if (nodeByID != null)
												{
													mBMagicPram = (menu.MBMagicPram)nodeByID.behavior().queryInterface(menu.MBMagicPram.classIdentifier());
												}
												mBMagicPram.SelectAreaResettingData(nodeByID, playerIndex);
												if (pHelpMsg != null)
												{
													pHelpMsg.release();
													pHelpMsg = null;
												}
												if (playerMCount <= 1)
												{
													CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
												}
												else
												{
													CWMenuManager.Instance().GetMenuButton().SetUpNormalVer();
												}
												menu.MenuManager.getSingleton().setFocuseMedget(0);
												ProcessTouchSlotSelect();
												m_state = true;
												break;
											}
											case 1:
												localState = 5;
												break;
											}
										}
										else if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
										{
											menu.MenuManager.getSingleton().playSECancel();
											CWMenuManager.Instance().SetNextKind(WMENU_KIND.WMENU_KIND_MAIN_MENU);
											CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
										}
										break;
									}
								}

								public int ProcessLearningMagic()
								{
									int targetItemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
									if (targetItemNo <= 0)
									{
										return 2;
									}
									sMagicLv = itm.ItemManager.instance().magicParameter((short)targetItemNo).magicClass();
									for (int i = 0; i < 3; i++)
									{
										int num = pl.PlayerParty.instance().player((byte)playerIndex).equipParameter()
											.equipMagic((pl.MAGIC_LEVEL)sMagicLv)
											.magicId(i);
										if (targetItemNo == num)
										{
											menu.MenuManager.getSingleton().playSEBeep();
											ProcessHelpWindowVerErr(50070);
											return 2;
										}
									}
									width_no = pl.PlayerParty.instance().player((byte)playerIndex).equipParameter()
										.equipMagic((pl.MAGIC_LEVEL)sMagicLv)
										.equip(targetItemNo);
									if (width_no == -999)
									{
										menu.MenuManager.getSingleton().playSEBeep();
										ProcessHelpWindowVerErr(50071);
										return 2;
									}
									MatrixSound.MtxSENDS_Play(98, 3, 192, 127);
									for (int j = 0; j < 384; j++)
									{
										int num2 = pl.PlayerParty.instance().item().normalItem(j)
											.itemId();
										if (targetItemNo == num2)
										{
											menu.Medget medget = menu.MenuManager.getSingleton().getFocuseMedget().parentNode();
											menu.MBItemWindow mBItemWindow = null;
											if (medget != null)
											{
												mBItemWindow = (menu.MBItemWindow)medget.behavior().queryInterface(menu.MBItemWindow.classIdentifier());
											}
											int num3 = pl.PlayerParty.instance().item().normalItem(j)
												.itemNumber();
											pl.PlayerParty.instance().item().normalItem(j)
												.setItemNumber(--num3);
											if (num3 == 0)
											{
												pl.PlayerParty.instance().item().normalItem(j)
													.setItemId(-1);
												mBItemWindow.TargetOneMsgDelete(medget, targetItemNo);
												return 0;
											}
											mBItemWindow.TargetMsgNumReset(medget, targetItemNo, j);
										}
									}
									return 2;
								}

								public void MagicRemove()
								{
									if (localState != 2)
									{
										ProcessHelpWindow();
									}
									switch (localState)
									{
									case 0:
										if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
										{
											if (menu.MenuManager.getSingleton().GetTargetItemNo() <= 0)
											{
												menu.MenuManager.getSingleton().playSEBeep();
												break;
											}
											int level = (int)menu.MenuManager.getSingleton().getFocuseMedget().work();
											int i = (sbyte)menu.MenuManager.getSingleton().getFocuseMedget().work1();
											pl.PlayerParty.instance().player((byte)playerIndex).equipParameter()
												.equipMagic((pl.MAGIC_LEVEL)level)
												.release(i);
											menu.Medget medget = menu.MenuManager.getSingleton().getFocuseMedget().parentNode();
											if (medget != null)
											{
												((menu.MBMagicPram)medget.behavior().queryInterface(menu.MBMagicPram.classIdentifier()))?.SelectAreaResettingData(medget, playerIndex);
											}
											int num = 0;
											while (true)
											{
												if (num < 384)
												{
													if (menu.MenuManager.getSingleton().GetTargetItemNo() == pl.PlayerParty.instance().item().normalItem(num)
														.itemId())
													{
														int num2 = pl.PlayerParty.instance().item().normalItem(num)
															.itemNumber();
														pl.PlayerParty.instance().item().normalItem(num)
															.setItemNumber(++num2);
														break;
													}
													num++;
													continue;
												}
												for (int j = 0; j < 384; j++)
												{
													int num3 = pl.PlayerParty.instance().item().normalItem(j)
														.itemId();
													if (num3 <= 0)
													{
														pl.PlayerParty.instance().item().normalItem(j)
															.setItemId((short)menu.MenuManager.getSingleton().GetTargetItemNo());
														pl.PlayerParty.instance().item().normalItem(j)
															.setItemNumber(1);
														break;
													}
												}
												break;
											}
											MatrixSound.MtxSENDS_Play(98, 6, 192, 127);
										}
										else if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
										{
											menu.MenuManager.getSingleton().playSECancel();
											CWMenuManager.Instance().SetNextKind(WMENU_KIND.WMENU_KIND_MAIN_MENU);
											CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
										}
										break;
									case 2:
										if ((ds.g_Pad.edge() & 1) != 0 || (ds.g_Pad.edge() & 2) != 0 || ds.g_TouchPanel.isRelease())
										{
											menu.MenuManager.getSingleton().playSECancel();
											if (pHelpMsg != null)
											{
												pHelpMsg.release();
												pHelpMsg = null;
											}
											localState = 0;
										}
										break;
									case 1:
										break;
									}
								}

								public void MagicChange()
								{
									switch (localState)
									{
									case 0:
									{
										if (pHelpMsg != null)
										{
											pHelpMsg.release();
											pHelpMsg = null;
											helpNo = -100;
										}
										ProcessHelpWindowVerErr(50072);
										dgs.msg.CMessageSys.getInstance().Sub().dgsMMAreaErase(240, 32, 240, 224);
										menu.Medget medget = menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
											.nextSibling()
											.childNode();
										if (medget != null)
										{
											((menu.MBMagicPram)medget.behavior().queryInterface(menu.MBMagicPram.classIdentifier()))?.bmAreaSuspend(medget, 1);
										}
										menu.MenuManager.getSingleton().SetTargetItemNo(-1);
										SetUpWindowType();
										pAppendAddr = menu.MenuManager.getSingleton().Append("UseItemCommand_Right2");
										menu.MenuManager.getSingleton().initFocus(SearchFirstTargetPlayerTag());
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
										localState = 2;
										break;
									}
									case 2:
										if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
										{
											if (menu.MenuManager.getSingleton().GetTargetCharNo() == playerIndex)
											{
												menu.MenuManager.getSingleton().playSEBeep();
												break;
											}
											CWMenuManager.Instance().GetDummyCursor().SetShow(show: false);
											menu.MenuManager.getSingleton().initFocus(command_cursor_memory);
											menu.MenuManager.getSingleton().playSEDecide();
											menu.MenuManager.getSingleton().Remove(pAppendAddr);
											pAppendAddr = null;
											menu.Medget nodeByID = pMainMedget.getNodeByID(TRANSCODE("magic_list"));
											if (nodeByID != null)
											{
												((menu.MBMagicPram)nodeByID.behavior().queryInterface(menu.MBMagicPram.classIdentifier()))?.bmAreaResume(nodeByID, 1);
											}
											menu.Medget nodeByID2 = pMainMedget.getNodeByID(TRANSCODE("magic_list"));
											if (nodeByID2 != null)
											{
												menu.MBMagicPram mBMagicPram = (menu.MBMagicPram)nodeByID2.behavior().queryInterface(menu.MBMagicPram.classIdentifier());
												if (mBMagicPram != null)
												{
													mBMagicPram.SetCurrentPlayer(playerIndex);
													mBMagicPram.ChangeMagic(nodeByID2);
												}
											}
											menu.MenuManager.getSingleton().SetTargetCharNo(playerIndex);
											menu.MenuManager.getSingleton().releaseWindowAll();
											nodeByID2 = pMainMedget.getNodeByID(TRANSCODE("magic_list"));
											if (nodeByID2 != null)
											{
												menu.MBMagicPram mBMagicPram2 = (menu.MBMagicPram)nodeByID.behavior().queryInterface(menu.MBMagicPram.classIdentifier());
												if (mBMagicPram2 != null)
												{
													mBMagicPram2.SetCurrentPlayer(playerIndex);
													mBMagicPram2.ResettingMagic(nodeByID2, playerIndex);
												}
											}
											ProcessHelpWindowVerErr(50073);
											localState = 3;
										}
										else if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
										{
											menu.MenuManager.getSingleton().playSECancel();
											CWMenuManager.Instance().SetNextKind(WMENU_KIND.WMENU_KIND_MAIN_MENU);
											CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
										}
										break;
									case 3:
										localState = 0;
										m_state = true;
										CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
										menu.MenuManager.getSingleton().releaseWindow(windowNo[2]);
										if (playerMCount <= 1)
										{
											CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
										}
										else
										{
											CWMenuManager.Instance().GetMenuButton().SetUpNormalVer();
										}
										menu.MenuManager.getSingleton().setFocuseMedget(0);
										break;
									case 1:
										break;
									}
								}

								public void ChangeFocuseComToList()
								{
									menu.Medget medget = menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
										.parentNode();
									medget = medget.getNodeByID(TRANSCODE("magic_list"));
									for (menu.Medget medget2 = medget.childNode(); medget2 != null; medget2 = medget2.nextSibling())
									{
										menu.MenuManager.getSingleton().joinFocusList(medget2);
									}
									menu.MenuManager.getSingleton().initFocus(medget.childNode().myTag());
								}

								public void ChangeFocuseListToCom()
								{
									menu.Medget medget = menu.MenuManager.getSingleton().getFocuseMedget().parentNode();
									for (menu.Medget medget2 = medget.childNode(); medget2 != null; medget2 = medget2.nextSibling())
									{
										menu.MenuManager.getSingleton().leaveFocusList(medget2);
									}
									menu.MenuManager.getSingleton().initFocus(command_cursor_memory);
								}

								public void ProcessHelpWindow()
								{
									menu.Medget focuseMedget = menu.MenuManager.getSingleton().getFocuseMedget();
									int num = menu.MenuManager.getSingleton().GetTargetItemNo();
									if (myProcess != 1 && focuseMedget != null && strlen(focuseMedget._id()) == 3)
									{
										num = pl.PlayerParty.instance().player((byte)menu.MenuManager.getSingleton().GetTargetCharNo()).equipParameter()
											.equipMagic((pl.MAGIC_LEVEL)focuseMedget.work())
											.magicId((sbyte)focuseMedget.work1());
									}
									itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)num);
									if (num <= 0 || itemBaseParameter == null)
									{
										if (pHelpMsg != null)
										{
											pHelpMsg.release();
											pHelpMsg = null;
											helpNo = -1;
										}
										return;
									}
									num = itemBaseParameter.captionId();
									if (num != helpNo)
									{
										helpNo = num;
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

								public void SetUpWindowType()
								{
									if (atoi(menu.MenuManager.getSingleton().getFocuseMedget().work1()
										.ToString()) == 0)
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

								public void TouchAreaToAppendRect()
								{
									pPrevDecideMedget = menu.MenuManager.getSingleton().getFocuseMedget();
									menu.Medget medget = null;
									menu.MBMagicPram mBMagicPram = (menu.MBMagicPram)pMainMedget.getNodeByID(TRANSCODE("magic_list")).behavior().queryInterface(menu.MBMagicPram.classIdentifier());
									mBMagicPram?.mbPause();
									if (appendType == 0)
									{
										dgs.msg.CMessageSys.getInstance().Sub().dgsMMAreaErase(240, 32, 240, 224);
										if (mBMagicPram != null)
										{
											mBMagicPram.mbPause();
											mBMagicPram.bmAreaSuspend(menu.MenuManager.getSingleton().getFocuseMedget().parentNode(), 1);
										}
										medget = pMainMedget.getNodeByID(TRANSCODE("magic_list"));
										if (medget != null)
										{
											CWMenuManager.Instance().ChainLeaveFocuseList(medget.childNode());
										}
										pAppendAddr = menu.MenuManager.getSingleton().Append("UseItemCommand_Right");
										menu.MenuManager.getSingleton().initFocus(SearchFirstTargetPlayerTag());
									}
									else
									{
										dgs.msg.CMessageSys.getInstance().Sub().dgsMMAreaErase(0, 32, 240, 224);
										AnotherMessageSuspend();
										mBMagicPram?.bmAreaSuspend(menu.MenuManager.getSingleton().getFocuseMedget().parentNode(), 0);
										medget = pMainMedget.getNodeByID(TRANSCODE("magic_list"));
										if (medget != null)
										{
											CWMenuManager.Instance().ChainLeaveFocuseList(medget.childNode());
										}
										pAppendAddr = menu.MenuManager.getSingleton().Append("UseItemCommand_Left");
										menu.MenuManager.getSingleton().initFocus(SearchFirstTargetPlayerTag());
									}
									CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
								}

								public void TouchAreaToRemoveRect()
								{
									menu.Medget nodeByID = pMainMedget.getNodeByID(TRANSCODE("magic_list"));
									if (nodeByID != null)
									{
										CWMenuManager.Instance().ChainLeaveFocuseList(nodeByID.childNode());
										CWMenuManager.Instance().ChainJoinFocuseList(nodeByID.childNode());
									}
									if (pPrevDecideMedget != null)
									{
										menu.MenuManager.getSingleton().initFocus(pPrevDecideMedget.myTag());
										pPrevDecideMedget = null;
									}
									if (pAppendAddr != null)
									{
										menu.MenuManager.getSingleton().Remove(pAppendAddr);
										pAppendAddr = null;
									}
									menu.MenuManager.getSingleton().releaseWindowAll();
									menu.MBMagicPram mBMagicPram = (menu.MBMagicPram)nodeByID.behavior().queryInterface(menu.MBMagicPram.classIdentifier());
									mBMagicPram?.mbRestart();
									if (appendType == 0)
									{
										mBMagicPram?.bmAreaResume(nodeByID, 1);
									}
									else
									{
										AnotherMessageResume();
										mBMagicPram?.bmAreaResume(nodeByID, 0);
									}
									menu.MenuManager.getSingleton().SetTargetCharNo(playerIndex);
									CWMenuManager.Instance().GetMenuButton().SetUpNormalVer();
								}

								public void AnotherMessageSuspend()
								{
									menu.Medget nodeByID = pMainMedget.getNodeByID(TRANSCODE("char_name"));
									if (nodeByID != null)
									{
										((menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier()))?.bmSuspend(nodeByID);
									}
									nodeByID = pMainMedget.getNodeByID(TRANSCODE("char_job"));
									if (nodeByID != null)
									{
										((menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier()))?.bmSuspend(nodeByID);
									}
								}

								public void AnotherMessageResume()
								{
									menu.Medget nodeByID = pMainMedget.getNodeByID(TRANSCODE("char_name"));
									if (nodeByID != null)
									{
										((menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier()))?.bmResume(nodeByID);
									}
									nodeByID = pMainMedget.getNodeByID(TRANSCODE("char_job"));
									if (nodeByID != null)
									{
										((menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier()))?.bmResume(nodeByID);
									}
								}

								public void bmRefresh(int ncNo)
								{
									menu.MBText mBText = null;
									menu.Medget medget = null;
									medget = pMainMedget.getNodeByID(TRANSCODE("char_name"));
									mBText = (menu.MBText)medget.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										strcpy(out var arg, pl.PlayerParty.instance().player((byte)ncNo).name());
										mBText.mbSetBufferMsg(arg, decWidth: false);
									}
									medget = medget.nextSibling();
									// PORT: a job of the mods' own has a name of its own (ProgressionLayer).
									string ownJob = OpenFF.Client.ProgressionLayer.JobNameOverride(pl.PlayerParty.instance().player((byte)ncNo).playerId(), pl.PlayerParty.instance().player((byte)ncNo).jobManager().nowJob());
									menu.MBText jobText = (menu.MBText)medget.behavior().queryInterface(menu.MBText.classIdentifier());
									if (ownJob != null) jobText?.mbSetBufferMsg(ownJob, decWidth: false);
									else jobText?.mbSetTextMsgNo(50105 + pl.PlayerParty.instance().player((byte)ncNo).jobManager()
										.nowJob());
								}

								public int SearchFirstTargetPlayerTag()
								{
									int result = -1;
									menu.Medget medget = menu.MenuManager.getSingleton().root().getNodeByID("use_main");
									if (medget != null && medget.childNode() != null)
									{
										medget = medget.childNode();
									}
									if (medget != null)
									{
										int num = 0;
										while (medget.nextSibling() != null && 4 > num)
										{
											if (pl.PlayerParty.instance().player((byte)num).isEnable())
											{
												result = medget.myTag();
												break;
											}
											medget = medget.nextSibling();
											num++;
										}
									}
									return result;
								}
							}
	}
}
