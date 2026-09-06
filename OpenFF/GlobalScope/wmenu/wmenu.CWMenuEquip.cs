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
	public static partial class wmenu
	{
							public class CWMenuEquip : CWMenuMemberBase, menu.MenuBehavedNotifier
							{
								public class WEAPONPOWER
								{
									public bool bEnable;

									public sys2d.Cell cell = new sys2d.Cell();
								}

								public const int PRAM_ATTACK = 0;

								public const int PRAM_DEFENCE = 1;

								public const int PRAM_MAX = 2;

								private int currentMode;

								private int currentFocusGroup;

								private int currentPlayer;

								private int currentSlot;

								private int prevComFocusNo;

								private menu.Medget pAppendAddr;

								private sys2d.Cell weaponDummyCursor = new sys2d.Cell();

								private int playerMCount;

								private int playerIndex;

								private int[] playerBox = new int[4];

								private int prevItemID;

								private WEAPONPOWER[] weaponIcon = new WEAPONPOWER[2];

								private bool se_;

								private menu.MBText caption_;

								private int slideTime;

								private int slideDir;

								private int wait;

								public override bool cSelectInitialize()
								{
									return true;
								}

								public override void initialize()
								{
									setFinalize(val: false);
									SVC_WaitVBlankIntr();
									currentMode = 0;
									currentFocusGroup = 0;
									currentPlayer = menu.MenuManager.getSingleton().GetTargetCharNo();
									drawCounter_ = 0;
									itemNum_[0] = 0;
									itemNum_[1] = 0;
									CWMenuManager.Instance().SetPrimaryBG(2);
									for (int i = 0; i < 4; i++)
									{
										CWMenuManager.Instance().SetShowPcFace(i, show: false);
										CWMenuManager.Instance().SetCharScrMovement(2, 8, i);
									}
									CWMenuManager.Instance().SetShowPcFace(pl.PlayerParty.instance().player((byte)menu.MenuManager.getSingleton().GetTargetCharNo()).playerId(), show: true);
									menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: true);
									for (int j = 0; j < 2; j++)
									{
										weaponIcon[j].bEnable = false;
									}
									menu.MenuManager.getSingleton().buildMenu("equip");
									shiftToSlotSelect();
									menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
									weaponDummyCursor.copy(menu.MenuManager.getSingleton().GetCursor2d());
									weaponDummyCursor.SetCell(3);
									weaponDummyCursor.SetAnimation(anm: false);
									weaponDummyCursor.SetPriority(0);
									weaponDummyCursor.SetPositionI(256, 192);
									weaponDummyCursor.SetShow(show: false);
									sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(weaponDummyCursor);
									CWMenuManager.Instance().GetDummyCursor().SetCell(0);
									CWMenuManager.Instance().GetDummyCursor().PlayAnimation(0, NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD);
									CWMenuManager.Instance().GetDummyCursor().SetAnimation(anm: true);
									menu.MenuManager.getSingleton().GetCursor2d().PlayAnimation(0, NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD);
									CreateParameterIcon();
									pAppendAddr = null;
									prevComFocusNo = 0;
									prevItemID = 0;
									playerIndex = 0;
									playerMCount = 0;
									for (int k = 0; k < 4; k++)
									{
										if (pl.PlayerParty.instance().player((byte)k).isEnable())
										{
											playerBox[playerMCount] = k;
											playerMCount++;
										}
									}
									for (int l = 0; l < playerMCount; l++)
									{
										if (menu.MenuManager.getSingleton().GetTargetCharNo() == playerBox[l])
										{
											playerIndex = l;
											currentPlayer = playerBox[l];
											break;
										}
									}
									se_ = false;
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
									wait = 0;
									CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_RUN);
								}

								public override void run()
								{
									string arg = "";
									bool flag = ds.g_TouchPanel.isEdge();
									if (flag)
									{
										if (strcmp(menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
											._id(), "select_equip_command") == 0)
										{
											prevComFocusNo = menu.MenuManager.getSingleton().getFocuseMedget().myTag();
										}
										strcpy(out arg, menu.MenuManager.getSingleton().getFocuseMedget()._id());
									}
									if (se_)
									{
										menu.MenuManager.getSingleton().setNotSEFlag(flag: true);
									}
									menu.MenuManager.getSingleton().execute();
									if (currentFocusGroup == 3 && prevItemID != menu.MenuManager.getSingleton().GetTargetItemNo())
									{
										bmRefreshItemPower(ownerMedget);
									}
									if (wait != 0)
									{
										wait--;
										if (wait == 0)
										{
											menu.MenuManager.getSingleton().inputPermission(b: true);
											menu.MenuManager.getSingleton().setFocuseMedget(0);
											shiftToSlotSelect();
										}
										return;
									}
									if (slideDir != 0)
									{
										slideTime++;
										G2_SetScreenOffset(((slideTime < 4) ? slideTime : (slideTime - 8)) * slideDir * 120, 0);
										if (slideTime == 4)
										{
											playerIndex = (playerIndex + slideDir + playerMCount) % playerMCount;
											currentPlayer = playerBox[playerIndex];
											menu.MenuManager.getSingleton().SetTargetCharNo(currentPlayer);
											wmsRefresh(ownerMedget);
											for (int i = 0; i < 4; i++)
											{
												CWMenuManager.Instance().SetShowPcFace(i, show: false);
											}
											CWMenuManager.Instance().SetCharScrMovement(2, 8, pl.PlayerParty.instance().player((byte)menu.MenuManager.getSingleton().GetTargetCharNo()).playerId());
											CWMenuManager.Instance().SetShowPcFace(pl.PlayerParty.instance().player((byte)menu.MenuManager.getSingleton().GetTargetCharNo()).playerId(), show: true);
											bmRefreshChangeValue(ownerMedget);
										}
										if (slideTime == 8)
										{
											menu.MenuManager.getSingleton().inputPermission(b: true);
											slideTime = 0;
											slideDir = 0;
										}
										return;
									}
									if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
									{
										bmCancel(ownerMedget);
									}
									else if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
									{
										if (currentFocusGroup == 3)
										{
											bmDecide(ownerMedget);
											menu.MenuManager.getSingleton().setNotSEFlag(flag: false);
										}
									}
									else if (menu.MenuManager.getSingleton().GetActivateButtonState() == 0)
									{
										if (strcmp(menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
											._id(), "select_equip_command") == 0)
										{
											if (currentFocusGroup == 1)
											{
												menu.Medget nodeByID = ownerMedget.getNodeByID(TRANSCODE("slots"));
												if (nodeByID != null)
												{
													CWMenuManager.Instance().ChainLeaveFocuseList(nodeByID.childNode());
												}
												bmRefreshChangeValue(ownerMedget);
												menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: true);
												CWMenuManager.Instance().GetDummyCursor().SetShow(show: false);
												menu.MenuManager.getSingleton().initFocus(menu.MenuManager.getSingleton().getFocuseMedget().myTag());
												currentMode = 0;
												currentFocusGroup = 0;
											}
											else if (currentFocusGroup == 3)
											{
												if (pAppendAddr != null)
												{
													menu.MenuManager.getSingleton().Remove(pAppendAddr);
													pAppendAddr = null;
												}
												menu.Medget nodeByID2 = ownerMedget.getNodeByID(TRANSCODE("slots"));
												if (nodeByID2 != null)
												{
													CWMenuManager.Instance().ChainLeaveFocuseList(nodeByID2.childNode());
												}
												menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: true);
												weaponDummyCursor.SetShow(show: false);
												CWMenuManager.Instance().GetDummyCursor().SetShow(show: false);
												bmRefreshChangeValue(ownerMedget);
												menu.MenuManager.getSingleton().initFocus(menu.MenuManager.getSingleton().getFocuseMedget().myTag());
												if (playerMCount <= 1)
												{
													CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
												}
												else
												{
													CWMenuManager.Instance().GetMenuButton().SetUpNormalVer();
												}
												currentMode = 0;
												currentFocusGroup = 0;
											}
											updateCaption();
											menu.Medget medget = null;
											menu.Medget medget2 = null;
											se_ = false;
											menu.MenuManager.getSingleton().setNotSEFlag(flag: false);
											if (menu.MenuManager.getSingleton().getFocuseMedget()._id(TRANSCODE("m_equip")))
											{
												shiftToSlotSelect();
											}
											else if (menu.MenuManager.getSingleton().getFocuseMedget()._id(TRANSCODE("m_nouse")))
											{
												currentMode = 1;
												currentFocusGroup = 1;
												medget = ownerMedget.getNodeByID(TRANSCODE("slots"));
												medget2 = medget.childNode();
												CWMenuManager.Instance().ChainJoinFocuseList(medget2);
												CWMenuManager.Instance().SetUpDummyCursor(menu.MenuManager.getSingleton().getFocuseMedget().cursorX(), menu.MenuManager.getSingleton().getFocuseMedget().cursorY(), act: true);
												menu.MenuManager.getSingleton().initFocus(medget.childNode().myTag());
												updateCaption();
												bmRefreshStolePower();
											}
											else if (menu.MenuManager.getSingleton().getFocuseMedget()._id(TRANSCODE("m_removeall")))
											{
												disarmament();
												wait = 15;
											}
										}
										else if (strcmp(menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
											._id(), "slots") == 0)
										{
											if (currentFocusGroup == 3)
											{
												if (pAppendAddr != null)
												{
													menu.MenuManager.getSingleton().Remove(pAppendAddr);
													pAppendAddr = null;
												}
												bmRefreshChangeValue(ownerMedget);
												menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: true);
												weaponDummyCursor.SetShow(show: false);
												menu.MenuManager.getSingleton().initFocus(menu.MenuManager.getSingleton().getFocuseMedget().myTag());
												if (playerMCount <= 1)
												{
													CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
												}
												else
												{
													CWMenuManager.Instance().GetMenuButton().SetUpNormalVer();
												}
												currentMode = 0;
												currentFocusGroup = 1;
											}
											updateCaption();
										}
									}
									if (currentFocusGroup == 3)
									{
										prevItemID = menu.MenuManager.getSingleton().GetTargetItemNo();
									}
									else if (currentFocusGroup == 1 && currentMode == 1 && flag && strcmp(arg, menu.MenuManager.getSingleton().getFocuseMedget()._id()) != 0)
									{
										bmRefreshStolePower();
									}
									if (drawCounter_ > 0)
									{
										drawCounter_--;
										for (int j = 0; j < 2; j++)
										{
											if (itemNum_[j] != 0)
											{
												setWeaponNumber(j, itemNum_[j], isDisp: true);
											}
											else
											{
												setWeaponNumber(j, itemNum_[j], isDisp: false);
											}
										}
									}
									menu.MenuManager.getSingleton().SetActivateButtonState(1);
									menu.MenuManager.getSingleton().ClearBehaviorButton();
								}

								public override void terminate()
								{
									if (!isFinalize())
									{
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
										sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(weaponDummyCursor);
										weaponDummyCursor.Release();
										CWMenuManager.Instance().GetDummyCursor().SetShow(show: false);
										CWMenuManager.Instance().GetDummyCursor().SetAnimation(anm: false);
										CWMenuManager.Instance().GetDummyCursor().SetCell(3);
										setWeaponNumber(0, 0, isDisp: false);
										setWeaponNumber(1, 0, isDisp: false);
										drawCounter_ = 0;
										if (pAppendAddr != null)
										{
											menu.MenuManager.getSingleton().Remove(pAppendAddr);
											pAppendAddr = null;
										}
										for (int i = 0; i < 2; i++)
										{
											if (weaponIcon[i].bEnable)
											{
												sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(weaponIcon[i].cell);
												weaponIcon[i].cell.Release();
												weaponIcon[i].bEnable = false;
											}
										}
										DeleteParameterIcon();
										menu.MenuManager.getSingleton().releaseWindowAll();
										menu.MenuManager.getSingleton().release();
										setFinalize(val: true);
									}
									se_ = false;
									menu.MenuManager.getSingleton().setNotSEFlag(flag: false);
								}

								public void CreateParameterIcon()
								{
									menu.Medget medget = null;
									medget = ownerMedget.getNodeByID(TRANSCODE("attack_max"));
									if (medget != null)
									{
										weaponIcon[0].bEnable = true;
										weaponIcon[0].cell.copy(menu.MenuManager.getSingleton().GetSmallIcon2d());
										weaponIcon[0].cell.SetCell(11);
										weaponIcon[0].cell.SetShow(show: false);
										weaponIcon[0].cell.SetPositionI(medget.x() - 8, medget.y());
										sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(weaponIcon[0].cell);
									}
									medget = ownerMedget.getNodeByID(TRANSCODE("difence_max"));
									if (medget != null)
									{
										weaponIcon[1].bEnable = true;
										weaponIcon[1].cell.copy(menu.MenuManager.getSingleton().GetSmallIcon2d());
										weaponIcon[1].cell.SetCell(11);
										weaponIcon[1].cell.SetShow(show: false);
										weaponIcon[1].cell.SetPositionI(medget.x() - 8, medget.y());
										sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(weaponIcon[1].cell);
									}
								}

								public void DeleteParameterIcon()
								{
									for (int i = 0; i < 2; i++)
									{
										if (weaponIcon[i].bEnable)
										{
											sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(weaponIcon[i].cell);
											weaponIcon[i].cell.Release();
											weaponIcon[i].bEnable = false;
										}
									}
								}

								public CWMenuEquip()
									: base("CWMenuEquip")
								{
									for (int i = 0; i < weaponIcon.Length; i++)
									{
										weaponIcon[i] = new WEAPONPOWER();
									}
								}

								public override void mbDelete()
								{
								}

								public override menu.MenuBehavior mbfCreate()
								{
									return CWMenuManager.Instance().GetWMenuEquip();
								}

								public override void bmInitialize(menu.Medget M)
								{
								}

								public bool mbnNotify(menu.MenuBehavior notifier, uint number, uint param)
								{
									if (caption_ == null || number != 2)
									{
										return false;
									}
									if (param != 0)
									{
										itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)param);
										if (itemBaseParameter != null)
										{
											caption_.mbSetTextMsgNo(itemBaseParameter.captionId());
											return true;
										}
									}
									caption_.mbSetBufferMsg("", decWidth: false);
									return true;
								}

								public override bool bmDirection(menu.Medget M, int key)
								{
									bool result = menu.MenuManager.getSingleton().MoveCursor(M, PlaySe: true);
									updateCaption();
									return result;
								}

								public override void bmPostInitialize(menu.Medget M)
								{
									pl.PlayerParty.instance().player(0).updateParameter();
									pl.PlayerParty.instance().player(1).updateParameter();
									pl.PlayerParty.instance().player(2).updateParameter();
									pl.PlayerParty.instance().player(3).updateParameter();
									menu.Medget nodeByID = ownerMedget.parentNode().getNodeByID(TRANSCODE("caption"));
									if (nodeByID != null)
									{
										caption_ = (menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier());
									}
									wmsRefresh(M);
								}

								public override void bmSuspend(menu.Medget M)
								{
								}

								public override void bmResume(menu.Medget M)
								{
									flagOn(1);
								}

								public void updateCaption()
								{
									if (caption_ == null)
									{
										return;
									}
									menu.Medget focuseMedget = menu.MenuManager.getSingleton().getFocuseMedget();
									if (focuseMedget != null && focuseMedget.behavior() != null)
									{
										menu.MBItemName mBItemName = (menu.MBItemName)focuseMedget.behavior().queryInterface(menu.MBItemName.classIdentifier());
										if (mBItemName != null && mBItemName.mbiGetItemNumber() > 0)
										{
											itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)mBItemName.mbiGetItemNumber());
											if (itemBaseParameter != null)
											{
												caption_.mbSetTextMsgNo(itemBaseParameter.captionId());
												return;
											}
										}
									}
									caption_.mbSetBufferMsg("", decWidth: false);
								}

								public void wmsRefresh(menu.Medget M)
								{
									menu.Medget nodeByIDFromChildren;
									if ((nodeByIDFromChildren = M.getNodeByIDFromChildren(TRANSCODE("char_name"))) != null)
									{
										((menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier()))?.mbSetBufferMsg(pl.PlayerParty.instance().player((byte)currentPlayer).name(), decWidth: false);
									}
									string after;
									if ((nodeByIDFromChildren = M.getNodeByIDFromChildren(TRANSCODE("slots"))) != null)
									{
										pl.PlayerEquipParameter playerEquipParameter = pl.PlayerParty.instance().player((byte)currentPlayer).equipParameter();
										menu.MBItemName mBItemName = null;
										if ((nodeByIDFromChildren = nodeByIDFromChildren.childNode()) != null)
										{
											mBItemName = (menu.MBItemName)nodeByIDFromChildren.behavior().queryInterface(menu.MBItemName.classIdentifier());
											if (mBItemName != null)
											{
												int num = playerEquipParameter.equipHand(pl.HAND_TYPE.RIGHT_HAND).itemId();
												int num2 = playerEquipParameter.equipHand(pl.HAND_TYPE.RIGHT_HAND).equipNumber().get();
												if (num <= 0 || num == 1000)
												{
													after = "";
													mBItemName.mbiSetBuffer(after, val: false);
													mBItemName.mbiSetItemNumber(-1);
													itemNum_[0] = 0;
												}
												else
												{
													mBItemName.mbiSetItemNumber((short)num);
													itemNum_[0] = num2;
												}
											}
										}
										if ((nodeByIDFromChildren = nodeByIDFromChildren.nextSibling()) != null)
										{
											mBItemName = (menu.MBItemName)nodeByIDFromChildren.behavior().queryInterface(menu.MBItemName.classIdentifier());
											if (mBItemName != null)
											{
												int num3 = playerEquipParameter.equipHand(pl.HAND_TYPE.LEFT_HAND).itemId();
												int num4 = playerEquipParameter.equipHand(pl.HAND_TYPE.LEFT_HAND).equipNumber().get();
												if (num3 <= 0 || num3 == 1000)
												{
													after = "";
													mBItemName.mbiSetBuffer(after, val: false);
													mBItemName.mbiSetItemNumber(-1);
													itemNum_[1] = 0;
												}
												else
												{
													mBItemName.mbiSetItemNumber((short)num3);
													itemNum_[1] = num4;
												}
											}
										}
										if ((nodeByIDFromChildren = nodeByIDFromChildren.nextSibling()) != null)
										{
											mBItemName = (menu.MBItemName)nodeByIDFromChildren.behavior().queryInterface(menu.MBItemName.classIdentifier());
											if (mBItemName != null)
											{
												if (playerEquipParameter.equipHead().itemId() > 0)
												{
													mBItemName.mbiSetItemNumber(playerEquipParameter.equipHead().itemId());
												}
												else
												{
													after = "";
													mBItemName.mbiSetBuffer(after, val: false);
													mBItemName.mbiSetItemNumber(-1);
												}
											}
										}
										if ((nodeByIDFromChildren = nodeByIDFromChildren.nextSibling()) != null)
										{
											mBItemName = (menu.MBItemName)nodeByIDFromChildren.behavior().queryInterface(menu.MBItemName.classIdentifier());
											if (mBItemName != null)
											{
												if (playerEquipParameter.equipBody().itemId() > 0)
												{
													mBItemName.mbiSetItemNumber(playerEquipParameter.equipBody().itemId());
												}
												else
												{
													after = "";
													mBItemName.mbiSetBuffer(after, val: false);
													mBItemName.mbiSetItemNumber(-1);
												}
											}
										}
										if ((nodeByIDFromChildren = nodeByIDFromChildren.nextSibling()) != null)
										{
											mBItemName = (menu.MBItemName)nodeByIDFromChildren.behavior().queryInterface(menu.MBItemName.classIdentifier());
											if (mBItemName != null)
											{
												if (playerEquipParameter.equipArm().itemId() > 0)
												{
													mBItemName.mbiSetItemNumber(playerEquipParameter.equipArm().itemId());
												}
												else
												{
													after = "";
													mBItemName.mbiSetBuffer(after, val: false);
													mBItemName.mbiSetItemNumber(-1);
												}
											}
										}
									}
									menu.MBText mBText = null;
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("attack_now"));
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										int num5 = pl.PlayerParty.instance().player((byte)currentPlayer).handAttack(pl.HAND_TYPE.RIGHT_HAND)
											.aggressivity()
											.get();
										num5 += pl.PlayerParty.instance().player((byte)currentPlayer).handAttack(pl.HAND_TYPE.LEFT_HAND)
											.aggressivity()
											.get();
										if (pl.PlayerParty.instance().player((byte)currentPlayer).condition()
											.isFrog() || pl.PlayerParty.instance().player((byte)currentPlayer).condition()
											.isLilliput())
										{
											num5 = ((num5 > 0) ? 1 : 0);
										}
										dgs.msg.CMessageSys.getInstance().changeValueFont(num5, out after);
										mBText.mbSetBufferMsg(after, decWidth: true);
									}
									nodeByIDFromChildren = nodeByIDFromChildren.nextSibling();
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										after = "";
										mBText.mbSetBufferMsg(after, decWidth: false);
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("difence_now"));
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										int num5 = pl.PlayerParty.instance().player((byte)currentPlayer).physicsDefense()
											.phylacticPower()
											.get();
										if (pl.PlayerParty.instance().player((byte)currentPlayer).condition()
											.isFrog() || pl.PlayerParty.instance().player((byte)currentPlayer).condition()
											.isLilliput())
										{
											num5 = ((num5 > 0) ? 1 : 0);
										}
										dgs.msg.CMessageSys.getInstance().changeValueFont(num5, out after);
										mBText.mbSetBufferMsg(after, decWidth: true);
									}
									nodeByIDFromChildren = nodeByIDFromChildren.nextSibling();
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										after = "";
										mBText.mbSetBufferMsg(after, decWidth: false);
									}
									updateSlotName();
									updateCaption();
									for (int i = 0; i < 2; i++)
									{
										if (itemNum_[i] != 0)
										{
											setWeaponNumber(i, itemNum_[i], isDisp: true);
										}
										else
										{
											setWeaponNumber(i, itemNum_[i], isDisp: false);
										}
									}
									drawCounter_ = 5;
								}

								public void bmRefreshItemPower(menu.Medget M)
								{
									menu.Medget nodeByID = M.getNodeByID(TRANSCODE("attack_now"));
									nodeByID = nodeByID.nextSibling();
									menu.Medget nodeByID2 = M.getNodeByID(TRANSCODE("difence_now"));
									nodeByID2 = nodeByID2.nextSibling();
									menu.MBText mBText = (menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier());
									menu.MBText mBText2 = (menu.MBText)nodeByID2.behavior().queryInterface(menu.MBText.classIdentifier());
									pl.Player player = pl.PlayerParty.instance().player((byte)currentPlayer);
									int num = player.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get() + player.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get();
									int num2 = player.physicsDefense().phylacticPower().get();
									if (pl.PlayerParty.instance().player((byte)currentPlayer).condition()
										.isFrog() || pl.PlayerParty.instance().player((byte)currentPlayer).condition()
										.isLilliput())
									{
										num = ((num > 0) ? 1 : 0);
										num2 = ((num2 > 0) ? 1 : 0);
									}
									int num3 = num;
									int num4 = num2;
									short num5 = (short)menu.MenuManager.getSingleton().GetTargetItemNo();
									pl.EquipItemInfo equipItemInfo = new pl.EquipItemInfo();
									equipItemInfo.itemNumber_ = 1;
									equipItemInfo.itemId_ = num5;
									if (currentSlot == 0)
									{
										if (num5 > 0 && num5 != 1000)
										{
											pl.EquipItemInfo itemInfo = player.equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND).equipItemInfo();
											pl.EquipItemInfo itemInfo2 = player.equipParameter().equipHand(pl.HAND_TYPE.LEFT_HAND).equipItemInfo();
											player.doEquip(0, num5, sort: false);
											player.updateParameter();
											num3 = player.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get() + player.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
											num4 = player.physicsDefense().phylacticPower().get();
											player.equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND).equip(itemInfo);
											player.equipParameter().equipHand(pl.HAND_TYPE.LEFT_HAND).equip(itemInfo2);
											player.updateParameter();
										}
									}
									else if (currentSlot == 1)
									{
										if (num5 > 0 && num5 != 1000)
										{
											pl.EquipItemInfo itemInfo = player.equipParameter().equipHand(pl.HAND_TYPE.LEFT_HAND).equipItemInfo();
											pl.EquipItemInfo itemInfo2 = player.equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND).equipItemInfo();
											player.doEquip(1, num5, sort: false);
											player.updateParameter();
											num3 = player.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get() + player.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
											num4 = player.physicsDefense().phylacticPower().get();
											player.equipParameter().equipHand(pl.HAND_TYPE.LEFT_HAND).equip(itemInfo);
											player.equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND).equip(itemInfo2);
											player.updateParameter();
										}
									}
									else if (currentSlot == 2)
									{
										if (num5 > 0)
										{
											pl.EquipItemInfo itemInfo = player.equipParameter().equipHead().equip(equipItemInfo);
											player.updateParameter();
											num3 = player.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get() + player.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
											num4 = player.physicsDefense().phylacticPower().get();
											player.equipParameter().equipHead().equip(itemInfo);
											player.updateParameter();
										}
									}
									else if (currentSlot == 3)
									{
										if (num5 > 0)
										{
											pl.EquipItemInfo itemInfo = player.equipParameter().equipBody().equip(equipItemInfo);
											player.updateParameter();
											num3 = player.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get() + player.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
											num4 = player.physicsDefense().phylacticPower().get();
											player.equipParameter().equipBody().equip(itemInfo);
											player.updateParameter();
										}
									}
									else if (currentSlot == 4 && num5 > 0)
									{
										pl.EquipItemInfo itemInfo = player.equipParameter().equipArm().equip(equipItemInfo);
										player.updateParameter();
										num3 = player.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get() + player.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
										num4 = player.physicsDefense().phylacticPower().get();
										player.equipParameter().equipArm().equip(itemInfo);
										player.updateParameter();
									}
									if (pl.PlayerParty.instance().player((byte)currentPlayer).condition()
										.isFrog() || pl.PlayerParty.instance().player((byte)currentPlayer).condition()
										.isLilliput())
									{
										num3 = ((num3 > 0) ? 1 : 0);
										num4 = ((num4 > 0) ? 1 : 0);
									}
									string after;
									if (num4 < num2)
									{
										weaponIcon[1].cell.SetCell(12);
										weaponIcon[1].cell.SetShow(show: true);
										dgs.msg.CMessageSys.getInstance().changeValueFont(num4, out after);
										mBText2.mbSetBufferMsg(after, decWidth: false);
									}
									else if (num4 > num2)
									{
										weaponIcon[1].cell.SetCell(11);
										weaponIcon[1].cell.SetShow(show: true);
										dgs.msg.CMessageSys.getInstance().changeValueFont(num4, out after);
										mBText2.mbSetBufferMsg(after, decWidth: false);
									}
									else
									{
										after = "";
										mBText2.mbSetBufferMsg(after, decWidth: false);
										weaponIcon[1].cell.SetShow(show: false);
									}
									if (num3 < num)
									{
										weaponIcon[0].cell.SetCell(12);
										weaponIcon[0].cell.SetShow(show: true);
										dgs.msg.CMessageSys.getInstance().changeValueFont(num3, out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
									}
									else if (num3 > num)
									{
										weaponIcon[0].cell.SetCell(11);
										weaponIcon[0].cell.SetShow(show: true);
										dgs.msg.CMessageSys.getInstance().changeValueFont(num3, out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
									}
									else
									{
										after = "";
										mBText.mbSetBufferMsg(after, decWidth: false);
										weaponIcon[0].cell.SetShow(show: false);
									}
								}

								public void bmRefreshStolePower()
								{
									menu.Medget nodeByID = ownerMedget.getNodeByID(TRANSCODE("attack_now"));
									nodeByID = nodeByID.nextSibling();
									menu.Medget nodeByID2 = ownerMedget.getNodeByID(TRANSCODE("difence_now"));
									nodeByID2 = nodeByID2.nextSibling();
									menu.MBText mBText = (menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier());
									menu.MBText mBText2 = (menu.MBText)nodeByID2.behavior().queryInterface(menu.MBText.classIdentifier());
									pl.Player player = pl.PlayerParty.instance().player((byte)currentPlayer);
									int num = player.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get() + player.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
									int num2 = player.physicsDefense().phylacticPower().get();
									int num3 = num;
									int num4 = num2;
									if (strcmp(menu.MenuManager.getSingleton().getFocuseMedget()._id(), "right") == 0)
									{
										int num5 = player.equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND).itemId();
										if (num5 > 0 && num5 != 1000)
										{
											pl.EquipItemInfo itemInfo = player.equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND).release();
											player.updateParameter();
											num = player.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get() + player.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
											num2 = player.physicsDefense().phylacticPower().get();
											player.equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND).equip(itemInfo);
											player.updateParameter();
										}
									}
									else if (strcmp(menu.MenuManager.getSingleton().getFocuseMedget()._id(), "left") == 0)
									{
										int num5 = player.equipParameter().equipHand(pl.HAND_TYPE.LEFT_HAND).itemId();
										if (num5 > 0 && num5 != 1000)
										{
											pl.EquipItemInfo itemInfo = player.equipParameter().equipHand(pl.HAND_TYPE.LEFT_HAND).release();
											player.updateParameter();
											num = player.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get() + player.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
											num2 = player.physicsDefense().phylacticPower().get();
											player.equipParameter().equipHand(pl.HAND_TYPE.LEFT_HAND).equip(itemInfo);
											player.updateParameter();
										}
									}
									else if (strcmp(menu.MenuManager.getSingleton().getFocuseMedget()._id(), "head") == 0)
									{
										int num5 = player.equipParameter().equipHead().itemId();
										if (num5 > 0)
										{
											pl.EquipItemInfo itemInfo = player.equipParameter().equipHead().release();
											player.updateParameter();
											num = player.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get() + player.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
											num2 = player.physicsDefense().phylacticPower().get();
											player.equipParameter().equipHead().equip(itemInfo);
											player.updateParameter();
										}
									}
									else if (strcmp(menu.MenuManager.getSingleton().getFocuseMedget()._id(), "body") == 0)
									{
										int num5 = player.equipParameter().equipBody().itemId();
										if (num5 > 0)
										{
											pl.EquipItemInfo itemInfo = player.equipParameter().equipBody().release();
											player.updateParameter();
											num = player.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get() + player.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
											num2 = player.physicsDefense().phylacticPower().get();
											player.equipParameter().equipBody().equip(itemInfo);
											player.updateParameter();
										}
									}
									else if (strcmp(menu.MenuManager.getSingleton().getFocuseMedget()._id(), "arm") == 0)
									{
										int num5 = player.equipParameter().equipArm().itemId();
										if (num5 > 0)
										{
											pl.EquipItemInfo itemInfo = player.equipParameter().equipArm().release();
											player.updateParameter();
											num = player.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get() + player.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
											num2 = player.physicsDefense().phylacticPower().get();
											player.equipParameter().equipArm().equip(itemInfo);
											player.updateParameter();
										}
									}
									string after;
									if (num4 < num2)
									{
										weaponIcon[1].cell.SetCell(11);
										weaponIcon[1].cell.SetShow(show: true);
										dgs.msg.CMessageSys.getInstance().changeValueFont(num2, out after);
										mBText2.mbSetBufferMsg(after, decWidth: false);
									}
									else if (num4 > num2)
									{
										weaponIcon[1].cell.SetCell(12);
										weaponIcon[1].cell.SetShow(show: true);
										dgs.msg.CMessageSys.getInstance().changeValueFont(num2, out after);
										mBText2.mbSetBufferMsg(after, decWidth: false);
									}
									else
									{
										after = "";
										mBText2.mbSetBufferMsg(after, decWidth: false);
										weaponIcon[1].cell.SetShow(show: false);
									}
									if (num3 < num)
									{
										weaponIcon[0].cell.SetCell(11);
										weaponIcon[0].cell.SetShow(show: true);
										dgs.msg.CMessageSys.getInstance().changeValueFont(num, out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
									}
									else if (num3 > num)
									{
										weaponIcon[0].cell.SetCell(12);
										weaponIcon[0].cell.SetShow(show: true);
										dgs.msg.CMessageSys.getInstance().changeValueFont(num, out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
									}
									else
									{
										after = "";
										mBText.mbSetBufferMsg(after, decWidth: false);
										weaponIcon[0].cell.SetShow(show: false);
									}
								}

								public void bmRefreshChangeValue(menu.Medget M)
								{
									menu.Medget nodeByID = M.getNodeByID(TRANSCODE("attack_now"));
									nodeByID = nodeByID.nextSibling();
									menu.Medget nodeByID2 = M.getNodeByID(TRANSCODE("difence_now"));
									nodeByID2 = nodeByID2.nextSibling();
									menu.MBText mBText = (menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier());
									menu.MBText mBText2 = (menu.MBText)nodeByID2.behavior().queryInterface(menu.MBText.classIdentifier());
									string pBuf = "";
									mBText.mbSetBufferMsg(pBuf, decWidth: false);
									mBText2.mbSetBufferMsg(pBuf, decWidth: false);
									for (int i = 0; i < 2; i++)
									{
										weaponIcon[i].cell.SetShow(show: false);
									}
								}

								public override bool bmDecide(menu.Medget M)
								{
									switch (currentFocusGroup)
									{
									case 1:
									{
										if (currentMode == 0)
										{
											weaponDummyCursor.SetPositionI(M.cursorX(), M.cursorY() + 2);
											weaponDummyCursor.SetShow(show: true);
											if (M._id(TRANSCODE("right")))
											{
												currentSlot = 0;
												menu.MenuManager.getSingleton().SetItemListPatern(3);
											}
											else if (M._id(TRANSCODE("left")))
											{
												currentSlot = 1;
												menu.MenuManager.getSingleton().SetItemListPatern(3);
											}
											else if (M._id(TRANSCODE("head")))
											{
												currentSlot = 2;
												menu.MenuManager.getSingleton().SetItemListPatern(7);
											}
											else if (M._id(TRANSCODE("body")))
											{
												currentSlot = 3;
												menu.MenuManager.getSingleton().SetItemListPatern(8);
											}
											else
											{
												if (!M._id(TRANSCODE("arm")))
												{
													return true;
												}
												currentSlot = 4;
												menu.MenuManager.getSingleton().SetItemListPatern(9);
											}
											menu.MenuManager.getSingleton().setNotSEFlag(flag: false);
											menu.MenuManager.getSingleton().playSEDecide();
											menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: false);
											currentFocusGroup = 3;
											CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
											prevItemID = 0;
											pAppendAddr = menu.MenuManager.getSingleton().Append("equip_under_list");
											menu.Medget nodeByID = pAppendAddr.getNodeByID(TRANSCODE("item_list"));
											if (nodeByID == null)
											{
												break;
											}
											menu.MenuManager.getSingleton().initFocus(nodeByID.childNode().myTag());
											if (nodeByID.behavior() == null)
											{
												break;
											}
											menu.MBItemWindow mBItemWindow = (menu.MBItemWindow)nodeByID.behavior().queryInterface(menu.MBItemWindow.classIdentifier());
											if (mBItemWindow == null)
											{
												break;
											}
											mBItemWindow.mbSetNotifier(this);
											int targetItemID = mBItemWindow.GetTargetItemID();
											if (targetItemID > 0)
											{
												itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)targetItemID);
												if (itemBaseParameter != null)
												{
													caption_.mbSetTextMsgNo(itemBaseParameter.captionId());
													return true;
												}
											}
											break;
										}
										pl.PlayerEquipParameter playerEquipParameter = pl.PlayerParty.instance().player((byte)currentPlayer).equipParameter();
										pl.EquipItemInfo equipItemInfo = new pl.EquipItemInfo();
										bool flag = false;
										if (M._id(TRANSCODE("right")) && playerEquipParameter.equipHand(pl.HAND_TYPE.RIGHT_HAND).itemId() > 0 && playerEquipParameter.equipHand(pl.HAND_TYPE.RIGHT_HAND).itemId() != 1000)
										{
											int num = playerEquipParameter.equipHand(pl.HAND_TYPE.RIGHT_HAND).itemId();
											if (itm.ItemManager.instance().weaponParameter((short)num) != null && itm.ItemManager.instance().weaponParameter((short)num).system() == 7)
											{
												pl.EquipItemInfo equipItemInfo2 = playerEquipParameter.equipHand(pl.HAND_TYPE.LEFT_HAND).release();
												pl.PlayerParty.instance().item().storeItem(equipItemInfo2.itemId_, equipItemInfo2.itemNumber_);
											}
											pl.PlayerParty.instance().item().storeItem(playerEquipParameter.equipHand(pl.HAND_TYPE.RIGHT_HAND).itemId(), playerEquipParameter.equipHand(pl.HAND_TYPE.RIGHT_HAND).equipNumber().get());
											playerEquipParameter.equipHand(pl.HAND_TYPE.RIGHT_HAND).release();
											equipItemInfo.itemId_ = -1;
											equipItemInfo.itemNumber_ = 0;
											playerEquipParameter.equipHand(pl.HAND_TYPE.RIGHT_HAND).equip(equipItemInfo);
											flag = true;
										}
										else if (M._id(TRANSCODE("left")) && playerEquipParameter.equipHand(pl.HAND_TYPE.LEFT_HAND).itemId() > 0 && playerEquipParameter.equipHand(pl.HAND_TYPE.LEFT_HAND).itemId() != 1000)
										{
											int num2 = playerEquipParameter.equipHand(pl.HAND_TYPE.LEFT_HAND).itemId();
											if (itm.ItemManager.instance().weaponParameter((short)num2) != null && itm.ItemManager.instance().weaponParameter((short)num2).system() == 7)
											{
												pl.EquipItemInfo equipItemInfo2 = playerEquipParameter.equipHand(pl.HAND_TYPE.RIGHT_HAND).release();
												pl.PlayerParty.instance().item().storeItem(equipItemInfo2.itemId_, equipItemInfo2.itemNumber_);
											}
											pl.PlayerParty.instance().item().storeItem(playerEquipParameter.equipHand(pl.HAND_TYPE.LEFT_HAND).itemId(), playerEquipParameter.equipHand(pl.HAND_TYPE.LEFT_HAND).equipNumber().get());
											playerEquipParameter.equipHand(pl.HAND_TYPE.LEFT_HAND).release();
											equipItemInfo.itemId_ = -1;
											equipItemInfo.itemNumber_ = 0;
											playerEquipParameter.equipHand(pl.HAND_TYPE.LEFT_HAND).equip(equipItemInfo);
											flag = true;
										}
										else if (M._id(TRANSCODE("head")) && playerEquipParameter.equipHead().itemId() > 0)
										{
											pl.PlayerParty.instance().item().storeItem(playerEquipParameter.equipHead().itemId(), 1);
											playerEquipParameter.equipHead().release();
											flag = true;
										}
										else if (M._id(TRANSCODE("body")) && playerEquipParameter.equipBody().itemId() > 0)
										{
											pl.PlayerParty.instance().item().storeItem(playerEquipParameter.equipBody().itemId(), 1);
											playerEquipParameter.equipBody().release();
											flag = true;
										}
										else if (M._id(TRANSCODE("arm")) && playerEquipParameter.equipArm().itemId() > 0)
										{
											pl.PlayerParty.instance().item().storeItem(playerEquipParameter.equipArm().itemId(), 1);
											playerEquipParameter.equipArm().release();
											flag = true;
										}
										if (!flag)
										{
											menu.MenuManager.getSingleton().playSEBeep();
											return true;
										}
										pl.PlayerParty.instance().player((byte)currentPlayer).updateParameter();
										MatrixSound.MtxSENDS_Play(98, 2, 192, 127);
										bmRefreshChangeValue(ownerMedget);
										wmsRefresh(ownerMedget);
										return true;
									}
									case 3:
										se_ = true;
										if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
										{
											if (menu.MenuManager.getSingleton().GetTargetItemNo() <= 0)
											{
												menu.MenuManager.getSingleton().playSEBeep();
												return true;
											}
											if (!pl.PlayerParty.instance().player((byte)currentPlayer).doEquip(currentSlot, (short)menu.MenuManager.getSingleton().GetTargetItemNo(), sort: true))
											{
												menu.MenuManager.getSingleton().playSEBeep();
												return true;
											}
											currentFocusGroup = 1;
											MatrixSound.MtxSENDS_Play(98, 1, 192, 127);
											weaponDummyCursor.SetShow(show: false);
											menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: true);
											if (pAppendAddr != null)
											{
												menu.MenuManager.getSingleton().initFocus(0);
												menu.MenuManager.getSingleton().Remove(pAppendAddr);
												pAppendAddr = null;
											}
											if (playerMCount <= 1)
											{
												CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
											}
											else
											{
												CWMenuManager.Instance().GetMenuButton().SetUpNormalVer();
											}
											menu.MenuManager.getSingleton().initFocus(currentSlot + 4);
											bmRefreshChangeValue(ownerMedget);
											wmsRefresh(M);
										}
										break;
									}
									return false;
								}

								public override bool bmCancel(menu.Medget M)
								{
									se_ = false;
									switch (currentFocusGroup)
									{
									case 3:
										menu.MenuManager.getSingleton().initFocus(0);
										menu.MenuManager.getSingleton().Remove(pAppendAddr);
										pAppendAddr = null;
										weaponDummyCursor.SetShow(show: false);
										menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: true);
										menu.MenuManager.getSingleton().initFocus(currentSlot + 4);
										if (playerMCount <= 1)
										{
											CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
										}
										else
										{
											CWMenuManager.Instance().GetMenuButton().SetUpNormalVer();
										}
										updateCaption();
										bmRefreshChangeValue(ownerMedget);
										currentFocusGroup = 1;
										break;
									case 1:
										CWMenuManager.Instance().SetNextKind(WMENU_KIND.WMENU_KIND_MAIN_MENU);
										CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
										break;
									default:
										return false;
									}
									menu.MenuManager.getSingleton().playSECancel();
									return true;
								}

								public override void bmBehave(menu.Medget M)
								{
									switch (currentFocusGroup)
									{
									case 2:
										currentFocusGroup = 3;
										break;
									}
									if (currentFocusGroup == 1)
									{
										if (playerMCount != 1)
										{
											if ((ds.g_Pad.edge() & 0x200) != 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonL())
											{
												slideDir = -1;
												slideTime = 0;
												menu.MenuManager.getSingleton().playSEMoveCursor();
												menu.MenuManager.getSingleton().inputPermission(b: false);
											}
											else if ((ds.g_Pad.edge() & 0x100) != 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonR())
											{
												slideDir = 1;
												slideTime = 0;
												menu.MenuManager.getSingleton().playSEMoveCursor();
												menu.MenuManager.getSingleton().inputPermission(b: false);
											}
										}
										else if ((ds.g_Pad.edge() & 0x200) != 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonL())
										{
											menu.MenuManager.getSingleton().playSEBeep();
										}
										else if ((ds.g_Pad.edge() & 0x100) != 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonR())
										{
											menu.MenuManager.getSingleton().playSEBeep();
										}
									}
									if (flagCheck(1))
									{
										flagOff(1);
										wmsRefresh(ownerMedget);
									}
								}

								public override void bmFinalize(menu.Medget M)
								{
									sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(weaponDummyCursor);
									weaponDummyCursor.Release();
									if (pAppendAddr != null)
									{
										menu.MenuManager.getSingleton().Remove(pAppendAddr);
										pAppendAddr = null;
									}
									for (int i = 0; i < 2; i++)
									{
										if (weaponIcon[i].bEnable)
										{
											sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(weaponIcon[i].cell);
											weaponIcon[i].cell.Release();
											weaponIcon[i].bEnable = false;
										}
									}
								}

								public void GetNowEquipItem(out int pLHand, out int pLNum, out int pRHand, out int pRNum, out int pHead, out int pArmor, out int pGunt)
								{
									pLHand = pl.PlayerParty.instance().player((byte)currentPlayer).equipParameter()
										.equipHand(pl.HAND_TYPE.LEFT_HAND)
										.itemId();
									pLNum = pl.PlayerParty.instance().player((byte)currentPlayer).equipParameter()
										.equipHand(pl.HAND_TYPE.LEFT_HAND)
										.equipNumber()
										.get();
									pRHand = pl.PlayerParty.instance().player((byte)currentPlayer).equipParameter()
										.equipHand(pl.HAND_TYPE.RIGHT_HAND)
										.itemId();
									pRNum = pl.PlayerParty.instance().player((byte)currentPlayer).equipParameter()
										.equipHand(pl.HAND_TYPE.RIGHT_HAND)
										.equipNumber()
										.get();
									pHead = pl.PlayerParty.instance().player((byte)currentPlayer).equipParameter()
										.equipHead()
										.itemId();
									pArmor = pl.PlayerParty.instance().player((byte)currentPlayer).equipParameter()
										.equipBody()
										.itemId();
									pGunt = pl.PlayerParty.instance().player((byte)currentPlayer).equipParameter()
										.equipArm()
										.itemId();
								}

								public void disarmament()
								{
									GetNowEquipItem(out var pLHand, out var pLNum, out var pRHand, out var pRNum, out var pHead, out var pArmor, out var pGunt);
									pl.PlayerEquipParameter playerEquipParameter = pl.PlayerParty.instance().player((byte)currentPlayer).equipParameter();
									playerEquipParameter.equipHand(pl.HAND_TYPE.RIGHT_HAND).release();
									playerEquipParameter.equipHand(pl.HAND_TYPE.LEFT_HAND).release();
									playerEquipParameter.equipHead().release();
									playerEquipParameter.equipBody().release();
									playerEquipParameter.equipArm().release();
									pl.EquipItemInfo equipItemInfo = new pl.EquipItemInfo();
									equipItemInfo.itemId_ = -1;
									equipItemInfo.itemNumber_ = 0;
									playerEquipParameter.equipHand(pl.HAND_TYPE.RIGHT_HAND).equip(equipItemInfo);
									playerEquipParameter.equipHand(pl.HAND_TYPE.LEFT_HAND).equip(equipItemInfo);
									if (pLHand > 0)
									{
										pl.PlayerParty.instance().item().storeItem((short)pLHand, pLNum);
									}
									if (pRHand > 0)
									{
										pl.PlayerParty.instance().item().storeItem((short)pRHand, pRNum);
									}
									if (pHead > 0)
									{
										pl.PlayerParty.instance().item().storeItem((short)pHead, 1);
									}
									if (pArmor > 0)
									{
										pl.PlayerParty.instance().item().storeItem((short)pArmor, 1);
									}
									if (pGunt > 0)
									{
										pl.PlayerParty.instance().item().storeItem((short)pGunt, 1);
									}
									pl.PlayerParty.instance().player((byte)currentPlayer).updateParameter();
									MatrixSound.MtxSENDS_Play(98, 2, 192, 127);
									wmsRefresh(ownerMedget);
								}

								public void updateSlotName()
								{
									int[] array = new int[5];
									GetNowEquipItem(out array[1], out var _, out array[0], out var _, out array[2], out array[3], out array[4]);
									menu.Medget nodeByIDFromChildren = ownerMedget.getNodeByIDFromChildren(TRANSCODE("equip_slot_all"));
									nodeByIDFromChildren = nodeByIDFromChildren.childNode();
									int num = 0;
									while (num < 5 && nodeByIDFromChildren != null)
									{
										if (nodeByIDFromChildren.behavior() != null)
										{
											menu.MBText mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
											if (mBText != null)
											{
												if (array[num] < 0)
												{
													mBText.bmTextVisibility(v: true);
												}
												else
												{
													mBText.bmTextVisibility(v: false);
												}
											}
										}
										num++;
										nodeByIDFromChildren = nodeByIDFromChildren.nextSibling();
									}
								}

								public void shiftToSlotSelect()
								{
									menu.Medget nodeByID = ownerMedget.getNodeByID(TRANSCODE("slots"));
									currentMode = 0;
									currentFocusGroup = 1;
									CWMenuManager.Instance().ChainJoinFocuseList(nodeByID.childNode());
									menu.MenuManager.getSingleton().initFocus(nodeByID.childNode().myTag());
									updateCaption();
									nodeByID = ownerMedget.childNode().childNode();
									CWMenuManager.Instance().SetUpDummyCursor(nodeByID.cursorX(), nodeByID.cursorY(), act: true);
								}

								public void setWeaponNumber(int type, int num, bool isDisp)
								{
									if (type < 2)
									{
										dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
										menu.Medget nodeByID = ownerMedget.getNodeByID((type == 0) ? TRANSCODE("migite") : TRANSCODE("hidarite"));
										int num2 = nodeByID.x() + 120;
										int num3 = nodeByID.y() + nodeByID.height() / 2 - 6;
										sprintf(out var arg, "%2d", num);
										dGSMessageManager.dgsMMAreaErase((short)num2, (short)num3, 16, 8);
										if (isDisp)
										{
											dGSMessageManager.writeCharacterString((short)num2, (short)num3, 0, 0, dgs.TXT_COLOR.TXT_COLOR_WHITE, 9u, arg, shadow: true, 1);
										}
									}
								}

								public new static int classIdentifier()
								{
									dgs.UniqueNumber uniqueNumber = new dgs.UniqueNumber();
									return uniqueNumber.number();
								}

								public override object queryInterface(int class_id)
								{
									if (class_id == classIdentifier())
									{
										return this;
									}
									return null;
								}
							}
	}
}
