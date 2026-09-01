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
	public static partial class menu
	{
		public class MBItemWindow : MenuBehavior, SBEventHandler
		{
			public class ITEM_SMALL_ICON
			{
				public sys2d.Cell mgIcon = new sys2d.Cell();

				public sys2d.Sprite3d mgIcon3d = new sys2d.Sprite3d();

				public bool bEnable;

				public int type;

				public ds.Vector2<short> pos = new ds.Vector2<short>();
			}

			public class TEMPBOX
			{
				private int _id;

				private int num;
			}

			public const int ITEM_MESSAGE_MAX = 32;

			public const int NOTIFY_SELECTED = 0;

			public const int NOTIFY_CANCELED = 1;

			public const int NOTIFY_CHANGED = 2;

			public static dgs.UniqueNumber MBItemWindow_UN = new dgs.UniqueNumber();

			private ScrollBar sb = new ScrollBar();

			private bool sbFlag;

			private int sbLine;

			private int maxItemLine;

			private int maxItemNum;

			private itm.PossessionItem dummyItem = new itm.PossessionItem();

			private itm.PossessionItem[] pBox = new itm.PossessionItem[384];

			private itm.PossessionItem[] buffer_ = new itm.PossessionItem[384];

			private int l_width_num;

			private int l_height_num;

			private int tItemBoxNo;

			private bool vanishFlag_;

			private bool noScrollFlag_;

			private bool[] bEnable = new bool[32];

			private ITEM_SMALL_ICON[] mIType = new ITEM_SMALL_ICON[32];

			private dgs.DGSMessage[] pMsg = new dgs.DGSMessage[32];

			private dgs.DGSMessage[] pMsgItemNum = new dgs.DGSMessage[32];

			private dgs.DGSMessage pMsgDrag;

			private bool drag_;

			public MBItemWindow()
			{
				for (int i = 0; i < mIType.Length; i++)
				{
					mIType[i] = new ITEM_SMALL_ICON();
				}
				for (int j = 0; j < 32; j++)
				{
					pMsg[j] = null;
					pMsgItemNum[j] = null;
					bEnable[j] = false;
					mIType[j].bEnable = false;
					mIType[j].pos.vx = 256;
					mIType[j].pos.vy = 192;
				}
			}

			public override void bmInitialize(Medget M)
			{
				vanishFlag_ = false;
				noScrollFlag_ = true;
				dummyItem.setItemId(-1);
				dummyItem.setItemNumber(0);
				XbnNode firstNodeByTagNameFromChildren = M.node().getFirstNodeByTagNameFromChildren(TRANSCODE("behavior"));
				if (firstNodeByTagNameFromChildren == null)
				{
					return;
				}
				XbnNodeList xbnNodeList = new XbnNodeList();
				firstNodeByTagNameFromChildren.getNodesByTagNameFromChildren(TRANSCODE("parameter"), xbnNodeList);
				l_height_num = GET_DECIMAL_PARAMETER(xbnNodeList, 1);
				l_width_num = GET_DECIMAL_PARAMETER(xbnNodeList, 2);
				sbFlag = false;
				sbLine = 0;
				for (int i = 0; i < 32; i++)
				{
					mIType[i].bEnable = false;
				}
				int num = 384;
				maxItemLine = num >> 1;
				for (int j = 0; j < 32; j++)
				{
					bEnable[j] = false;
					pMsg[j] = null;
					pMsgItemNum[j] = null;
				}
				CreateItemListBox();
				CreateExclusiveUseScrollBar(M);
				if (GET_DECIMAL_PARAMETER(xbnNodeList, 3) != 0)
				{
					MenuManager.getSingleton().initFocus(0);
				}
				if (opt.COptionManager.getSingleton().cursorOption().cursorPosition() == opt.CURSOR_POSITION.CURSOR_POSITION_MEMORY && MenuManager.getSingleton().battleMode())
				{
					vanishFlag_ = true;
					MenuManager.getSingleton().SetTrialInitFocuseFlag(val: false);
					int id = pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).playerId();
					if (MenuManager.getSingleton().GetItemListPatern() == 3)
					{
						if (MenuManager.getSingleton().saveBattleEquipLine(id) >= 0)
						{
							sb.sbFixedMove(0);
						}
						MenuManager.getSingleton().initFocus(0);
					}
					else if (MenuManager.getSingleton().GetItemListPatern() == 5)
					{
						MenuManager.getSingleton().initFocus(MenuManager.getSingleton().saveBattlePitchTarget(id));
						if (MenuManager.getSingleton().saveBattlePitchLine(id) >= 0)
						{
							sb.sbFixedMove((short)MenuManager.getSingleton().saveBattlePitchLine(id));
						}
					}
					else
					{
						MenuManager.getSingleton().initFocus(MenuManager.getSingleton().saveBattleItemTarget(id));
						if (MenuManager.getSingleton().saveBattleItemLine(id) >= 0)
						{
							sb.sbFixedMove((short)MenuManager.getSingleton().saveBattleItemLine(id));
						}
					}
				}
				vanishFlag_ = false;
				noScrollFlag_ = false;
				drag_ = false;
				if (MenuManager.getSingleton().battleMode() && MenuManager.getSingleton().GetItemListPatern() == 3)
				{
					if (MenuBattleItem.getSingleton().localType() == MenuBattleItem.ITEM_SELECT_STATE.W_LOCAL_CHOICE)
					{
						sb.sbRestrainCheck((ScrollBar.AREA_FLAG)3);
					}
					else
					{
						sb.sbRestrainCheck();
					}
				}
			}

			public void sbehScrolled(short currentLine)
			{
				if (!sbFlag || maxItemLine < 3 || sbLine == currentLine || currentLine > maxItemLine || (MenuManager.getSingleton().battleMode() && MenuManager.getSingleton().GetItemListPatern() == 3 && MenuBattleItem.getSingleton().localType() == MenuBattleItem.ITEM_SELECT_STATE.W_LOCAL_CHOICE))
				{
					return;
				}
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((ownerMedget.display() != 0) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				dgs.msg.CMessageMng.MSF_HANDLE_KIND msfHandle = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
				int num = currentLine - sbLine;
				sbLine = currentLine;
				ClearMsg();
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					int num2 = GET_ITEM_NUMBER(medget);
					num2 += num * 2;
					SET_ITEM_NUMBER(medget, num2);
					int num3 = pBox[num2].itemId();
					if (num3 <= 0)
					{
						SET_MSG_IDX(medget, -1);
					}
					else if (pBox[num2].itemNumber() <= 0)
					{
						SET_MSG_IDX(medget, -1);
					}
					else if (itm.ItemManager.instance().itemParameter((short)num3) == null)
					{
						SET_MSG_IDX(medget, -1);
					}
					else
					{
						int num4 = CheckEnableMessageNo();
						int msg_number = itm.ItemManager.instance().itemParameter((short)num3).nameId();
						pMsg[num4] = dGSMessageManager.createMessage((uint)msg_number, MenuManager.getSingleton().GetItemDataTextNo(), 1);
						if (pMsg[num4] == null)
						{
							bEnable[num4] = false;
							SET_MSG_IDX(medget, -1);
						}
						else
						{
							int num5 = 0;
							if (medget.height() > 0)
							{
								num5 = (medget.height() - 12) / 2;
							}
							if (MenuManager.getSingleton().battleMode())
							{
								pMsg[num4].setPosition((short)(medget.x() + 12), (short)(medget.y() + num5), erase: true);
							}
							else
							{
								pMsg[num4].setPosition((short)(medget.x() + 16), (short)(medget.y() + num5), erase: true);
							}
							pMsg[num4].setDisplaySpeed(byte.MaxValue);
							pMsg[num4].setDisplayWait(0);
							CreateItemTypeIcon(dGSMessageManager, msfHandle, medget.x(), medget.y() + num5 + -2, num3, num4);
							SET_MSG_IDX(medget, num4);
							CreateItemNumMessage(num2, medget, num4);
						}
					}
				}
				if (MenuManager.getSingleton().battleMode() && !noScrollFlag_)
				{
					int id = pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).playerId();
					if (MenuManager.getSingleton().GetItemListPatern() == 5)
					{
						MenuManager.getSingleton().saveBattlePitchLine_add(id, num);
					}
					else if (MenuManager.getSingleton().GetItemListPatern() != 3)
					{
						MenuManager.getSingleton().saveBattleItemLine_add(id, num);
					}
				}
				UpdateMsgColor(0);
				if (!vanishFlag_)
				{
					MenuManager.getSingleton().playSEMoveCursor();
				}
				else
				{
					vanishFlag_ = false;
				}
				MenuManager.getSingleton().SetScrollType(MenuManager.SCROLL_TYPE.TYPE_WAIT);
				if (mbNotifier != null)
				{
					int param = pBox[GET_ITEM_NUMBER(MenuManager.getSingleton().getFocuseMedget())].itemId();
					mbNotifier.mbnNotify(this, 2u, (uint)param);
				}
			}

			public override void bmBehave(Medget M)
			{
				if (drag_)
				{
					ds.g_TouchPanel.getPoint(out var x, out var y);
					for (Medget medget = M.childNode(); medget != null; medget = medget.nextSibling())
					{
						if (medget.x() < x && x <= medget.x() + medget.width() && medget.y() < y && y <= medget.y() + medget.height())
						{
							MenuManager.getSingleton().initFocus(medget.myTag());
						}
					}
				}
				if (MenuManager.getSingleton().updateMessageFlag())
				{
					ResetAreaMessage(M, 1);
					MenuManager.getSingleton().setUpdateMessageFlag(flag: false);
				}
				int id = pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).playerId();
				if (MenuManager.getSingleton().battleMode())
				{
					if (MenuManager.getSingleton().GetItemListPatern() == 3)
					{
						if (MenuBattleItem.getSingleton().localType() == MenuBattleItem.ITEM_SELECT_STATE.W_LOCAL_DECIDE)
						{
							MenuManager.getSingleton().saveBattleEquipTarget_set(id, MenuManager.getSingleton().getFocuseMedget().myTag());
						}
					}
					else if (MenuManager.getSingleton().GetItemListPatern() == 5)
					{
						MenuManager.getSingleton().saveBattlePitchTarget_set(id, MenuManager.getSingleton().getFocuseMedget().myTag());
					}
					else
					{
						MenuManager.getSingleton().saveBattleItemTarget_set(id, MenuManager.getSingleton().getFocuseMedget().myTag());
					}
				}
				if (MenuManager.getSingleton().battleMode() && MenuManager.getSingleton().GetItemListPatern() == 3)
				{
					if (MenuBattleItem.getSingleton().localType() == MenuBattleItem.ITEM_SELECT_STATE.W_LOCAL_CHOICE)
					{
						sb.sbRestrainCheck((ScrollBar.AREA_FLAG)3);
						return;
					}
					sb.sbRestrainCheck();
				}
				if (maxItemLine != -1 && maxItemLine >= 3 && MenuManager.getSingleton().GetScrollType() != MenuManager.SCROLL_TYPE.TYPE_WAIT)
				{
					if (MenuManager.getSingleton().GetScrollType() == MenuManager.SCROLL_TYPE.TYPE_DOWN)
					{
						sb.sbFixedMove(1);
					}
					else if (MenuManager.getSingleton().GetScrollType() == MenuManager.SCROLL_TYPE.TYPE_UP)
					{
						sb.sbFixedMove(-1);
					}
				}
			}

			public override void bmFinalize(Medget M)
			{
				ClearMsg();
				if (pMsgDrag != null)
				{
					pMsgDrag.release();
					pMsgDrag = null;
				}
				if (sbFlag)
				{
					sb.sbDestroy();
					sb.sbSetHandler(null);
				}
			}

			public override bool bmDecide(Medget M)
			{
				MenuManager.getSingleton().SetDecideButtonState(0);
				if (mbNotifier != null && MenuManager.getSingleton().GetTargetItemNo() > 0)
				{
					mbNotifier.mbnNotify(this, 0u, (uint)MenuManager.getSingleton().GetTargetItemNo());
				}
				return false;
			}

			public override bool bmCancel(Medget M)
			{
				MenuManager.getSingleton().SetCancelButtonState(0);
				if (mbNotifier != null)
				{
					mbNotifier.mbnNotify(this, 1u, 0u);
				}
				return false;
			}

			public override bool bmDirection(Medget M, int key)
			{
				bool flag = MenuManager.getSingleton().MoveCursor(M, PlaySe: true);
				if (flag)
				{
					UpdateMsgColor(0);
					if (mbNotifier != null)
					{
						int param = pBox[GET_ITEM_NUMBER(MenuManager.getSingleton().getFocuseMedget())].itemId();
						mbNotifier.mbnNotify(this, 2u, (uint)param);
					}
				}
				return flag;
			}

			public void bmSuspendBase(int indexNo)
			{
				if (indexNo >= 0 && bEnable[indexNo])
				{
					if (pMsg[indexNo] != null)
					{
						pMsg[indexNo].setVisibility(b: false);
						pMsgItemNum[indexNo].setVisibility(b: false);
					}
					if (MenuManager.getSingleton().Get2d3dMode() == 2)
					{
						mIType[indexNo].mgIcon.SetShow(show: false);
					}
					else
					{
						mIType[indexNo].mgIcon3d.SetShow(show: false);
					}
				}
			}

			public override void bmSuspend(Medget M)
			{
				for (int i = 0; i < 32; i++)
				{
					bmSuspendBase(i);
				}
			}

			public void bmAreaSuspend(Medget pM, int direct)
			{
				for (Medget medget = pM.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (direct == 0)
					{
						if (GET_ITEM_NUMBER(medget) % 2 == 0)
						{
							bmSuspendBase(GET_MSG_IDX(medget));
						}
					}
					else if (GET_ITEM_NUMBER(medget) % 2 == 1)
					{
						bmSuspendBase(GET_MSG_IDX(medget));
					}
				}
			}

			public void bmResumeBase(int indexNo)
			{
				if (indexNo >= 0 && bEnable[indexNo])
				{
					if (pMsg[indexNo] != null)
					{
						pMsg[indexNo].setVisibility(b: true);
						pMsgItemNum[indexNo].setVisibility(b: true);
					}
					if (MenuManager.getSingleton().Get2d3dMode() == 2)
					{
						mIType[indexNo].mgIcon.SetShow(show: true);
					}
					else
					{
						mIType[indexNo].mgIcon3d.SetShow(show: true);
					}
				}
			}

			public override void bmResume(Medget M)
			{
				for (int i = 0; i < 32; i++)
				{
					bmResumeBase(i);
				}
			}

			public void bmAreaResume(Medget pM, int direct)
			{
				for (Medget medget = pM.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (direct == 0)
					{
						if (GET_ITEM_NUMBER(medget) % 2 == 0)
						{
							bmResumeBase(GET_MSG_IDX(medget));
						}
					}
					else if (GET_ITEM_NUMBER(medget) % 2 == 1)
					{
						bmResumeBase(GET_MSG_IDX(medget));
						_ = sbFlag;
					}
				}
			}

			public override void mbPause()
			{
				sb.sbRestrainCheck((ScrollBar.AREA_FLAG)3);
				base.mbPause();
			}

			public override void mbRestart()
			{
				sb.sbRestrainCheck();
				base.mbRestart();
			}

			public void bmItemVisibility(bool v)
			{
				for (int i = 0; i < 32; i++)
				{
					if (bEnable[i] && pMsg[i] != null)
					{
						pMsg[i].setVisibility(v);
						pMsgItemNum[i].setVisibility(v);
					}
				}
			}

			public int GetTargetItemID()
			{
				return pBox[GET_ITEM_NUMBER(MenuManager.getSingleton().getFocuseMedget())].itemId();
			}

			public override void bmActivate(Medget M)
			{
				if (!ownerMedget._id(M._id()))
				{
					int num = pBox[GET_ITEM_NUMBER(M)].itemId();
					if (num <= 0)
					{
						MenuManager.getSingleton().SetTargetItemNo(-1);
					}
					else
					{
						MenuManager.getSingleton().SetTargetItemNo(num);
					}
					if (mbNotifier != null)
					{
						int param = pBox[GET_ITEM_NUMBER(MenuManager.getSingleton().getFocuseMedget())].itemId();
						mbNotifier.mbnNotify(this, 2u, (uint)param);
					}
				}
				UpdateMsgColor(0);
			}

			public override void bmDeactivate(Medget M)
			{
				UpdateMsgColor(0);
			}

			public int CheckEnableMessageNo()
			{
				for (int i = 0; i < 32; i++)
				{
					if (!bEnable[i])
					{
						bEnable[i] = true;
						return i;
					}
				}
				return -1;
			}

			public void CreateItemListBox()
			{
				SetTargetItemList();
				maxItemLine = 0;
				if (MenuManager.getSingleton().GetItemListPatern() == 0 || MenuManager.getSingleton().GetItemListPatern() == 11)
				{
					for (int i = 0; i < 384; i++)
					{
						if (pl.PlayerParty.instance().item().normalItem(i)
							.itemNumber() > 0)
						{
							maxItemLine = i;
						}
					}
				}
				else if (MenuManager.getSingleton().GetItemListPatern() == 10)
				{
					for (int j = 0; j < 384; j++)
					{
						if (pl.PlayerParty.instance().storedItem().item((short)j)
							.itemNumber() > 0)
						{
							maxItemLine = j;
						}
					}
				}
				else if (MenuManager.getSingleton().GetItemListPatern() == 1)
				{
					for (int k = 0; k < 64; k++)
					{
						if (pl.PlayerParty.instance().item().importantItem(k)
							.itemNumber() > 0)
						{
							maxItemLine = k;
						}
					}
				}
				else
				{
					for (int l = 0; l < 384; l++)
					{
						if (pBox[l] != null && pBox[l] != dummyItem)
						{
							maxItemLine++;
						}
					}
				}
				OS_Printf("CreateItemListBox() maxItemLine = %d\n", maxItemLine);
				if (MenuManager.getSingleton().battleMode() && MenuManager.getSingleton().GetItemListPatern() == 3)
				{
					maxItemLine += 3;
				}
				else
				{
					maxItemLine++;
				}
				maxItemLine = maxItemLine % 2 + (maxItemLine >> 1) + 1;
				if (maxItemLine > 192)
				{
					maxItemLine = 192;
				}
				sb.sbSetCapacity((short)l_height_num, (short)maxItemLine);
				BuildMsg();
			}

			public void CreateItemTypeIcon(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND _msfHandle, int x, int y, int tItemNo, int indexNo)
			{
				short num = itm.ItemManager.instance().itemParameter((short)tItemNo).system();
				itm.CATEGORY cATEGORY = itm.ItemManager.instance().itemCategory((short)tItemNo);
				mIType[indexNo].type = num;
				mIType[indexNo].bEnable = true;
				mIType[indexNo].pos.vx = (short)x;
				mIType[indexNo].pos.vy = (short)y;
				if (MenuManager.getSingleton().Get2d3dMode() == 2)
				{
					mIType[indexNo].mgIcon.copy(MenuManager.getSingleton().GetSmallIcon2d());
					switch (cATEGORY)
					{
					case itm.CATEGORY.CATEGORY_WEAPON:
						mIType[indexNo].mgIcon.SetCell((ushort)convertIDXWeaponSysToIcon(num));
						break;
					case itm.CATEGORY.CATEGORY_PROTECTION:
						mIType[indexNo].mgIcon.SetCell((ushort)convertIDXProtectionSysToIcon(num));
						break;
					case itm.CATEGORY.CATEGORY_MAGIC:
						mIType[indexNo].mgIcon.SetCell((ushort)convertIDXMagicSysToIcon(num));
						break;
					default:
						mIType[indexNo].mgIcon.SetCell(45);
						break;
					}
					mIType[indexNo].mgIcon.SetPositionI(mIType[indexNo].pos.vx, mIType[indexNo].pos.vy);
					mIType[indexNo].mgIcon.SetShow(show: true);
					mIType[indexNo].mgIcon.SetPriority(1);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(mIType[indexNo].mgIcon);
					return;
				}
				mIType[indexNo].mgIcon3d.copy(MenuManager.getSingleton().GetSmallIcon3d());
				switch (cATEGORY)
				{
				case itm.CATEGORY.CATEGORY_WEAPON:
					mIType[indexNo].mgIcon3d.SetCell((ushort)convertIDXWeaponSysToIcon(num));
					break;
				case itm.CATEGORY.CATEGORY_PROTECTION:
					mIType[indexNo].mgIcon3d.SetCell((ushort)convertIDXProtectionSysToIcon(num));
					break;
				case itm.CATEGORY.CATEGORY_MAGIC:
					mIType[indexNo].mgIcon3d.SetCell((ushort)convertIDXMagicSysToIcon(num));
					break;
				default:
					mIType[indexNo].mgIcon3d.SetCell(45);
					break;
				}
				if (MenuManager.getSingleton().battleMode())
				{
					mIType[indexNo].mgIcon3d.SetPositionI(mIType[indexNo].pos.vx - 4, mIType[indexNo].pos.vy);
				}
				else
				{
					mIType[indexNo].mgIcon3d.SetPositionI(mIType[indexNo].pos.vx, mIType[indexNo].pos.vy);
				}
				mIType[indexNo].mgIcon3d.SetShow(show: true);
				mIType[indexNo].mgIcon3d.SetDepth(1);
				sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(mIType[indexNo].mgIcon3d);
			}

			public void CreateItemNumMessage(int work_list, Medget pMy, int enableNo)
			{
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((ownerMedget.display() != 1) ? dgs.msg.CMessageSys.getInstance().Sub() : dgs.msg.CMessageSys.getInstance().Main());
				string str = TRANSCODE(((int)pBox[work_list].itemNumber()).ToString());
				pMsgItemNum[enableNo] = dGSMessageManager.createMessage(str, 1);
				int num = 0;
				if (pMy.height() > 0)
				{
					num = (pMy.height() - 12) / 2;
				}
				ds.Vector2<short> vector = new ds.Vector2<short>();
				pMsgItemNum[enableNo].getTextSize(vector);
				if (MenuManager.getSingleton().battleMode())
				{
					pMsgItemNum[enableNo].setPosition((short)(pMy.x() + 12 + 160 - vector.vx), (short)(pMy.y() + num), erase: true);
				}
				else
				{
					pMsgItemNum[enableNo].setPosition((short)(pMy.x() + 16 + 160 - vector.vx), (short)(pMy.y() + num), erase: true);
				}
				pMsgItemNum[enableNo].setDisplaySpeed(byte.MaxValue);
				pMsgItemNum[enableNo].setDisplayWait(0);
			}

			public void CreateExclusiveUseScrollBar(Medget M)
			{
				XbnNode firstNodeByTagNameFromChildren = M.node().getFirstNodeByTagNameFromChildren(TRANSCODE("behavior"));
				if (firstNodeByTagNameFromChildren != null)
				{
					XbnNodeList xbnNodeList = new XbnNodeList();
					firstNodeByTagNameFromChildren.getNodesByTagNameFromChildren(TRANSCODE("parameter"), xbnNodeList);
					int num = 0;
					if (xbnNodeList.size() > 0)
					{
						num = xbnNodeList[0].nodeValueInt();
					}
					if (num != 0)
					{
						sb.sbCreate();
						sb.sbSetPosition((short)(M.x() + M.width() - (ScrollBar.PARTS_W >> 12) - SCROLL_BAR_MARGIN), M.y());
						sb.sbSetHeight(M.height());
						sb.sbPartsSetPriority(1);
						sb.sbSetCapacity((short)l_height_num, (short)maxItemLine);
						sb.sbSetHandler(this);
						sbLine ^= sbLine;
						sbFlag = true;
					}
				}
			}

			public void ClearMsg()
			{
				for (int i = 0; i < 32; i++)
				{
					if (bEnable[i])
					{
						if (pMsg[i] != null)
						{
							pMsg[i].release();
							pMsg[i] = null;
						}
						if (pMsgItemNum[i] != null)
						{
							pMsgItemNum[i].release();
							pMsgItemNum[i] = null;
						}
						bEnable[i] = false;
					}
					if (mIType[i].bEnable)
					{
						if (MenuManager.getSingleton().Get2d3dMode() == 2)
						{
							sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIType[i].mgIcon);
							mIType[i].mgIcon.Release();
						}
						else
						{
							sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIType[i].mgIcon3d);
							mIType[i].mgIcon3d.Release();
						}
						mIType[i].bEnable = false;
					}
				}
			}

			public void BuildMsg()
			{
				dgs.msg.CMessageMng.MSF_HANDLE_KIND mSF_HANDLE_KIND = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((ownerMedget.display() != 1) ? dgs.msg.CMessageSys.getInstance().Sub() : dgs.msg.CMessageSys.getInstance().Main());
				int num = 0;
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					int num2 = pBox[num].itemId();
					SET_ITEM_NUMBER(medget, num);
					num++;
					if (num2 <= 0)
					{
						SET_MSG_IDX(medget, -1);
					}
					else if (pBox[GET_ITEM_NUMBER(medget)].itemNumber() <= 0)
					{
						SET_MSG_IDX(medget, -1);
					}
					else
					{
						itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)num2);
						if (itemBaseParameter == null)
						{
							SET_MSG_IDX(medget, -1);
						}
						else if (itm.ItemManager.instance().itemParameter((short)num2) == null)
						{
							SET_MSG_IDX(medget, -1);
						}
						else
						{
							int num3 = CheckEnableMessageNo();
							int msg_number = itm.ItemManager.instance().itemParameter((short)num2).nameId();
							pMsg[num3] = dGSMessageManager.createMessage((uint)msg_number, dgs.INVALID_MSDHANDLE, (int)mSF_HANDLE_KIND);
							if (pMsg[num3] == null)
							{
								bEnable[num3] = false;
								SET_MSG_IDX(medget, -1);
							}
							else
							{
								int num4 = 0;
								if (medget.height() > 0)
								{
									num4 = (medget.height() - 12) / 2;
								}
								if (MenuManager.getSingleton().battleMode())
								{
									pMsg[num3].setPosition((short)(medget.x() + 12), (short)(medget.y() + num4), erase: true);
								}
								else
								{
									pMsg[num3].setPosition((short)(medget.x() + 16), (short)(medget.y() + num4), erase: true);
								}
								pMsg[num3].setDisplaySpeed(byte.MaxValue);
								pMsg[num3].setDisplayWait(0);
								SET_MSG_IDX(medget, num3);
								CreateItemTypeIcon(dGSMessageManager, mSF_HANDLE_KIND, medget.x(), medget.y() + num4 + -2, num2, num3);
								CreateItemNumMessage(num - 1, medget, num3);
							}
						}
					}
				}
				UpdateMsgColor(0);
			}

			public void UpdateMsgColor(int no_set_item_id)
			{
				Medget focuseMedget = MenuManager.getSingleton().getFocuseMedget();
				if (focuseMedget == null || GET_ITEM_NUMBER(focuseMedget) < 0)
				{
					return;
				}
				int num = pBox[GET_ITEM_NUMBER(focuseMedget)].itemId();
				if (no_set_item_id == 0)
				{
					if (num <= 0)
					{
						MenuManager.getSingleton().SetTargetItemNo(-1);
					}
					else
					{
						MenuManager.getSingleton().SetTargetItemNo(num);
					}
					if (num == 1000)
					{
						MenuManager.getSingleton().SetTargetItemNo(-99);
					}
				}
				if (GET_MSG_IDX(focuseMedget) >= 0 && no_set_item_id == 0)
				{
					if (pMsg[GET_MSG_IDX(focuseMedget)] != null)
					{
						pMsg[GET_MSG_IDX(focuseMedget)].setMessageColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
					}
					if (pMsgItemNum[GET_MSG_IDX(focuseMedget)] != null)
					{
						pMsgItemNum[GET_MSG_IDX(focuseMedget)].setMessageColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
					}
				}
				if (pMsgDrag != null)
				{
					pMsgDrag.setMessageColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
				}
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (medget != focuseMedget && GET_MSG_IDX(medget) >= 0 && GET_ITEM_NUMBER(medget) >= 0 && pBox[GET_ITEM_NUMBER(medget)] != null && pBox[GET_ITEM_NUMBER(medget)].itemNumber() > 0)
					{
						itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter(pBox[GET_ITEM_NUMBER(medget)].itemId());
						if (itemBaseParameter == null)
						{
							if (pMsg[GET_MSG_IDX(medget)] != null)
							{
								pMsg[GET_MSG_IDX(medget)].setMessageColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
							}
							if (pMsgItemNum[GET_MSG_IDX(medget)] != null)
							{
								pMsgItemNum[GET_MSG_IDX(medget)].setMessageColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
							}
						}
						else
						{
							bool flag = false;
							itm.CATEGORY cATEGORY = itm.ItemManager.instance().itemCategory(pBox[GET_ITEM_NUMBER(medget)].itemId());
							switch (cATEGORY)
							{
							case itm.CATEGORY.CATEGORY_MAGIC:
								flag = !MenuManager.getSingleton().battleMode();
								break;
							case itm.CATEGORY.CATEGORY_CONSUMPTION:
								flag = !evt.CEventRestriction.getSingleton().check(pBox[GET_ITEM_NUMBER(medget)].itemId()) && ((MenuManager.getSingleton().GetUsingMenuType() != 0) ? ((pBox[GET_ITEM_NUMBER(medget)].itemId() != 5013) ? ((pBox[GET_ITEM_NUMBER(medget)].itemId() != 5014) ? ((itemBaseParameter.useField() != 2 || wld.WorldPart.getInstance().getWorldSystem().PreviousMode() == wld.CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD) && (itemBaseParameter.useField() != 3 || wld.WorldPart.getInstance().getWorldSystem().PreviousMode() == wld.CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN) && itemBaseParameter.useField() != 0) : ((evt.CEventManager.getInstance().FlagMng().get(0u, 979u) != 0) ? true : false)) : (MenuManager.getSingleton().openDoorFlag() ? true : false)) : (itemBaseParameter.useBattle() != 0));
								break;
							case itm.CATEGORY.CATEGORY_IMPORTANT:
							{
								flag = false;
								int num2 = pBox[GET_ITEM_NUMBER(medget)].itemId();
								if (num2 >= 5218 && num2 <= 5241)
								{
									flag = true;
								}
								break;
							}
							default:
							{
								int targetCharNo = MenuManager.getSingleton().GetTargetCharNo();
								if (MenuManager.getSingleton().battleMode())
								{
									pl.Command command = pl.PlayerParty.instance().player((byte)targetCharNo).jobManager()
										.command();
									if (command.commandId(command.nowCommand()) == 40)
									{
										flag = true;
										break;
									}
									switch (cATEGORY)
									{
									case itm.CATEGORY.CATEGORY_WEAPON:
									{
										if (MenuManager.getSingleton().GetItemListPatern() == 0)
										{
											flag = false;
											break;
										}
										if (MenuManager.getSingleton().GetItemListPatern() == 11)
										{
											itm.WeaponParameter weaponParameter = itm.ItemManager.instance().weaponParameter(pBox[GET_ITEM_NUMBER(medget)].itemId());
											flag = weaponParameter != null && ((pl.PlayerParty.instance().player((byte)targetCharNo).isEquipItem(weaponParameter.equipJob()) && weaponParameter.useItemId() > 0) ? true : false);
											break;
										}
										itm.WeaponParameter weaponParameter2 = itm.ItemManager.instance().weaponParameter(pBox[GET_ITEM_NUMBER(medget)].itemId());
										if (weaponParameter2 == null)
										{
											flag = false;
										}
										flag = pl.PlayerParty.instance().player((byte)targetCharNo).isEquipItem(weaponParameter2.equipJob());
										break;
									}
									case itm.CATEGORY.CATEGORY_PROTECTION:
									{
										if (MenuManager.getSingleton().GetItemListPatern() == 0 || MenuManager.getSingleton().GetItemListPatern() == 11)
										{
											flag = false;
											break;
										}
										itm.ProtectionParameter protectionParameter = itm.ItemManager.instance().protectionParameter(pBox[GET_ITEM_NUMBER(medget)].itemId());
										if (protectionParameter == null)
										{
											flag = false;
										}
										flag = pl.PlayerParty.instance().player((byte)targetCharNo).isEquipItem(protectionParameter.equipJob());
										break;
									}
									default:
										flag = false;
										break;
									}
									break;
								}
								switch (cATEGORY)
								{
								case itm.CATEGORY.CATEGORY_WEAPON:
								{
									if (MenuManager.getSingleton().GetItemListPatern() == 0)
									{
										flag = false;
										break;
									}
									if (MenuManager.getSingleton().GetItemListPatern() == 11)
									{
										itm.WeaponParameter weaponParameter3 = itm.ItemManager.instance().weaponParameter(pBox[GET_ITEM_NUMBER(medget)].itemId());
										flag = weaponParameter3 != null && ((pl.PlayerParty.instance().player((byte)targetCharNo).isEquipItem(weaponParameter3.equipJob()) && weaponParameter3.useItemId() > 0) ? true : false);
										break;
									}
									itm.WeaponParameter weaponParameter4 = itm.ItemManager.instance().weaponParameter(pBox[GET_ITEM_NUMBER(medget)].itemId());
									if (weaponParameter4 == null)
									{
										flag = false;
									}
									flag = pl.PlayerParty.instance().player((byte)targetCharNo).isEquipItem(weaponParameter4.equipJob());
									break;
								}
								case itm.CATEGORY.CATEGORY_PROTECTION:
								{
									if (MenuManager.getSingleton().GetItemListPatern() == 0 || MenuManager.getSingleton().GetItemListPatern() == 11)
									{
										flag = false;
										break;
									}
									itm.ProtectionParameter protectionParameter2 = itm.ItemManager.instance().protectionParameter(pBox[GET_ITEM_NUMBER(medget)].itemId());
									if (protectionParameter2 == null)
									{
										flag = false;
									}
									flag = pl.PlayerParty.instance().player((byte)targetCharNo).isEquipItem(protectionParameter2.equipJob());
									break;
								}
								default:
									flag = false;
									break;
								}
								break;
							}
							}
							if (pMsg[GET_MSG_IDX(medget)] != null)
							{
								if (!flag)
								{
									pMsg[GET_MSG_IDX(medget)].setMessageColor(dgs.TXT_COLOR.TXT_UCOLOR_4);
								}
								else
								{
									pMsg[GET_MSG_IDX(medget)].setMessageColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
								}
								if (!MenuManager.getSingleton().battleMode() && MenuManager.getSingleton().GetMagicMenuType() == 2)
								{
									pMsg[GET_MSG_IDX(medget)].setMessageColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
								}
							}
							if (pMsgItemNum[GET_MSG_IDX(medget)] != null)
							{
								if (!flag)
								{
									pMsgItemNum[GET_MSG_IDX(medget)].setMessageColor(dgs.TXT_COLOR.TXT_UCOLOR_4);
								}
								else
								{
									pMsgItemNum[GET_MSG_IDX(medget)].setMessageColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
								}
								if (!MenuManager.getSingleton().battleMode() && MenuManager.getSingleton().GetMagicMenuType() == 2)
								{
									pMsgItemNum[GET_MSG_IDX(medget)].setMessageColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
								}
							}
						}
					}
				}
			}

			public void RefreshChangeList(Medget pMedget)
			{
				ClearMsg();
				for (int i = 0; i < 32; i++)
				{
					bEnable[i] = false;
					pMsg[i] = null;
					pMsgItemNum[i] = null;
					mIType[i].bEnable = false;
				}
				sbLine ^= sbLine;
				CreateItemListBox();
				sb.sbSetLine(0);
			}

			public void RefreshTargetMsg(int prevBoxNo, int afterBoxNo)
			{
				dgs.DGSMessageManager dGSMessageManager = ((ownerMedget.display() == 1) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (prevBoxNo == GET_ITEM_NUMBER(medget) || afterBoxNo == GET_ITEM_NUMBER(medget))
					{
						int num = 0;
						if (prevBoxNo == GET_ITEM_NUMBER(medget))
						{
							num = prevBoxNo;
						}
						else if (afterBoxNo == GET_ITEM_NUMBER(medget))
						{
							num = afterBoxNo;
						}
						if (GET_MSG_IDX(medget) != -1)
						{
							pMsg[GET_MSG_IDX(medget)].release();
							pMsg[GET_MSG_IDX(medget)] = null;
							if (mIType[GET_MSG_IDX(medget)].bEnable)
							{
								if (MenuManager.getSingleton().Get2d3dMode() == 2)
								{
									mIType[GET_MSG_IDX(medget)].mgIcon.Release();
									sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIType[GET_MSG_IDX(medget)].mgIcon);
								}
								else
								{
									mIType[GET_MSG_IDX(medget)].mgIcon3d.Release();
									sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIType[GET_MSG_IDX(medget)].mgIcon3d);
								}
								mIType[GET_MSG_IDX(medget)].bEnable = false;
							}
							pMsgItemNum[GET_MSG_IDX(medget)].release();
							pMsgItemNum[GET_MSG_IDX(medget)] = null;
							bEnable[GET_MSG_IDX(medget)] = false;
						}
						pBox[num] = pl.PlayerParty.instance().item().normalItem(num);
						int num2 = pBox[num].itemId();
						if (num2 <= 0)
						{
							SET_MSG_IDX(medget, -1);
						}
						else if (pBox[num].itemNumber() <= 0)
						{
							SET_MSG_IDX(medget, -1);
						}
						else if (itm.ItemManager.instance().itemParameter((short)num2) == null)
						{
							SET_MSG_IDX(medget, -1);
						}
						else
						{
							int num3 = CheckEnableMessageNo();
							int msg_number = itm.ItemManager.instance().itemParameter((short)num2).nameId();
							pMsg[num3] = dGSMessageManager.createMessage((uint)msg_number, MenuManager.getSingleton().GetItemDataTextNo(), 1);
							if (pMsg[num3] == null)
							{
								bEnable[num3] = false;
								SET_MSG_IDX(medget, -1);
							}
							else
							{
								int num4 = 0;
								if (medget.height() > 0)
								{
									num4 = (medget.height() - 12) / 2;
								}
								if (MenuManager.getSingleton().battleMode())
								{
									pMsg[num3].setPosition((short)(medget.x() + 12), (short)(medget.y() + num4), erase: true);
								}
								else
								{
									pMsg[num3].setPosition((short)(medget.x() + 16), (short)(medget.y() + num4), erase: true);
								}
								pMsg[num3].setDisplaySpeed(byte.MaxValue);
								pMsg[num3].setDisplayWait(0);
								CreateItemTypeIcon(dGSMessageManager, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, medget.x(), medget.y() + num4 + -2, num2, num3);
								SET_MSG_IDX(medget, num3);
								CreateItemNumMessage(num, medget, num3);
							}
						}
					}
				}
				UpdateMsgColor(0);
			}

			public void RefreshList()
			{
				ClearMsg();
				for (int i = 0; i < 32; i++)
				{
					bEnable[i] = false;
					pMsg[i] = null;
					pMsgItemNum[i] = null;
				}
				sbLine ^= sbLine;
				CreateItemListBox();
				sb.sbSetLine(0);
			}

			public void TargetOneMsgDelete(Medget pMedget, int t_item_no)
			{
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (t_item_no == pBox[GET_ITEM_NUMBER(medget)].itemId() && GET_MSG_IDX(medget) != -1 && pMsg[GET_MSG_IDX(medget)] != null)
					{
						pMsg[GET_MSG_IDX(medget)].release();
						pMsg[GET_MSG_IDX(medget)] = null;
						if (pMsgItemNum[GET_MSG_IDX(medget)] != null)
						{
							pMsgItemNum[GET_MSG_IDX(medget)].release();
							pMsgItemNum[GET_MSG_IDX(medget)] = null;
						}
						bEnable[GET_MSG_IDX(medget)] = false;
						if (mIType[GET_MSG_IDX(medget)].bEnable)
						{
							if (MenuManager.getSingleton().Get2d3dMode() == 2)
							{
								mIType[GET_MSG_IDX(medget)].mgIcon.Release();
								sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIType[GET_MSG_IDX(medget)].mgIcon);
							}
							else
							{
								mIType[GET_MSG_IDX(medget)].mgIcon3d.Release();
								sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIType[GET_MSG_IDX(medget)].mgIcon3d);
							}
							mIType[GET_MSG_IDX(medget)].bEnable = false;
						}
						SET_MSG_IDX(medget, -1);
						pBox[GET_ITEM_NUMBER(medget)].setItemId(-1);
					}
				}
			}

			public void TargetMsgNumReset(Medget pMedget, int t_item_no, int effectNo)
			{
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (t_item_no == pBox[GET_ITEM_NUMBER(medget)].itemId() && GET_MSG_IDX(medget) != -1)
					{
						pMsgItemNum[GET_MSG_IDX(medget)].release();
						pMsgItemNum[GET_MSG_IDX(medget)] = null;
						CreateItemNumMessage(GET_ITEM_NUMBER(medget), medget, GET_MSG_IDX(medget));
						if (GET_MSG_IDX(medget) != -1)
						{
							pMsgItemNum[GET_MSG_IDX(medget)].setMessageColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
						}
					}
				}
			}

			public void ResetAreaMessage(Medget pMedget, int no_set_item_id)
			{
				SetTargetItemList();
				for (Medget medget = pMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (GET_MSG_IDX(medget) != -1 && pMsg[GET_MSG_IDX(medget)] != null)
					{
						pMsg[GET_MSG_IDX(medget)].release();
						pMsg[GET_MSG_IDX(medget)] = null;
						if (pMsgItemNum[GET_MSG_IDX(medget)] != null)
						{
							pMsgItemNum[GET_MSG_IDX(medget)].release();
							pMsgItemNum[GET_MSG_IDX(medget)] = null;
						}
						bEnable[GET_MSG_IDX(medget)] = false;
						if (mIType[GET_MSG_IDX(medget)].bEnable)
						{
							if (pMedget.display() != 0)
							{
								sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIType[GET_MSG_IDX(medget)].mgIcon3d);
								mIType[GET_MSG_IDX(medget)].mgIcon3d.Release();
							}
							else
							{
								sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIType[GET_MSG_IDX(medget)].mgIcon);
								mIType[GET_MSG_IDX(medget)].mgIcon.Release();
							}
							mIType[GET_MSG_IDX(medget)].bEnable = false;
						}
						SET_MSG_IDX(medget, -1);
					}
				}
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((pMedget.display() != 0) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				for (Medget medget2 = pMedget.childNode(); medget2 != null; medget2 = medget2.nextSibling())
				{
					int num = pBox[GET_ITEM_NUMBER(medget2)].itemId();
					if (num <= 0)
					{
						SET_MSG_IDX(medget2, -1);
					}
					else if (pBox[GET_ITEM_NUMBER(medget2)].itemNumber() <= 0)
					{
						SET_MSG_IDX(medget2, -1);
					}
					else if (itm.ItemManager.instance().itemParameter((short)num) == null)
					{
						SET_MSG_IDX(medget2, -1);
					}
					else
					{
						int msg_number = itm.ItemManager.instance().itemParameter((short)num).nameId();
						int num2 = CheckEnableMessageNo();
						pMsg[num2] = dGSMessageManager.createMessage((uint)msg_number, MenuManager.getSingleton().GetItemDataTextNo(), 1);
						if (pMsg[num2] == null)
						{
							bEnable[num2] = false;
							SET_MSG_IDX(medget2, -1);
						}
						else
						{
							int num3 = 0;
							if (medget2.height() > 0)
							{
								num3 = (medget2.height() - 12) / 2;
							}
							if (MenuManager.getSingleton().battleMode())
							{
								pMsg[num2].setPosition((short)(medget2.x() + 12), (short)(medget2.y() + num3), erase: true);
							}
							else
							{
								pMsg[num2].setPosition((short)(medget2.x() + 16), (short)(medget2.y() + num3), erase: true);
							}
							pMsg[num2].setDisplaySpeed(byte.MaxValue);
							pMsg[num2].setDisplayWait(0);
							SET_MSG_IDX(medget2, num2);
							CreateItemTypeIcon(dGSMessageManager, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, medget2.x(), medget2.y() + num3 + -2, num, num2);
							CreateItemNumMessage(GET_ITEM_NUMBER(medget2), medget2, num2);
						}
					}
				}
				UpdateMsgColor(no_set_item_id);
			}

			public void ChangeColorAllString(Medget pMedget, int color)
			{
				if (GET_MSG_IDX(pMedget) != -1 && pMsg[GET_MSG_IDX(pMedget)] != null)
				{
					pMsg[GET_MSG_IDX(pMedget)].setMessageColor((dgs.TXT_COLOR)color);
					pMsgItemNum[GET_MSG_IDX(pMedget)].setMessageColor((dgs.TXT_COLOR)color);
				}
			}

			public void SetTargetItemList()
			{
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				for (int i = 0; i < 384; i++)
				{
					pBox[i] = dummyItem;
				}
				int num4 = 0;
				if (MenuManager.getSingleton().GetItemListPatern() != 1)
				{
					for (int j = 0; j < 384; j++)
					{
						int num5 = ((MenuManager.getSingleton().GetItemListPatern() != 10) ? pl.PlayerParty.instance().item().normalItem(j)
							.itemId() : pl.PlayerParty.instance().storedItem().item((short)j)
							.itemId());
						if (num5 <= 0 || itm.ItemManager.instance().itemParameter((short)num5) == null)
						{
							continue;
						}
						itm.CATEGORY cATEGORY = itm.ItemManager.instance().itemCategory((short)num5);
						int num6 = itm.ItemManager.instance().itemParameter((short)num5).system();
						if (MenuManager.getSingleton().GetItemListPatern() != 0 && MenuManager.getSingleton().GetItemListPatern() != 11 && MenuManager.getSingleton().GetItemListPatern() != 10 && MenuManager.getSingleton().GetItemListPatern() != 1 && MenuManager.getSingleton().GetItemListPatern() != 2 && MenuManager.getSingleton().GetItemListPatern() != 5)
						{
							int num7 = 0;
							switch (cATEGORY)
							{
							case itm.CATEGORY.CATEGORY_WEAPON:
								num7 = itm.ItemManager.instance().weaponParameter((short)num5).equipJob();
								if (!pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).isEquipItem(num7))
								{
									continue;
								}
								break;
							case itm.CATEGORY.CATEGORY_PROTECTION:
								num7 = itm.ItemManager.instance().protectionParameter((short)num5).equipJob();
								if (!pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).isEquipItem(num7))
								{
									continue;
								}
								break;
							}
						}
						switch (MenuManager.getSingleton().GetItemListPatern())
						{
						case 0:
							pBox[j] = pl.PlayerParty.instance().item().normalItem(j);
							break;
						case 10:
							pBox[j] = pl.PlayerParty.instance().storedItem().item((short)j);
							break;
						case 1:
							pBox[j] = pl.PlayerParty.instance().item().importantItem(j);
							break;
						case 2:
							if (cATEGORY == itm.CATEGORY.CATEGORY_MAGIC)
							{
								byte b = itm.ItemManager.instance().itemParameter((short)num5).system();
								if (b != 3 && b != 4)
								{
									pBox[num] = pl.PlayerParty.instance().item().normalItem(j);
									num++;
								}
							}
							break;
						case 3:
							if (num5 < 0)
							{
								break;
							}
							switch (cATEGORY)
							{
							case itm.CATEGORY.CATEGORY_WEAPON:
								pBox[num2] = pl.PlayerParty.instance().item().normalItem(j);
								num2++;
								break;
							case itm.CATEGORY.CATEGORY_PROTECTION:
								if (num6 == 0)
								{
									pBox[num2] = pl.PlayerParty.instance().item().normalItem(j);
									num2++;
								}
								break;
							}
							break;
						case 5:
							if (cATEGORY == itm.CATEGORY.CATEGORY_WEAPON)
							{
								pBox[num2] = pl.PlayerParty.instance().item().normalItem(j);
								num2++;
							}
							break;
						case 7:
							if (cATEGORY == itm.CATEGORY.CATEGORY_PROTECTION && num6 == 1)
							{
								pBox[num3] = pl.PlayerParty.instance().item().normalItem(j);
								num3++;
							}
							break;
						case 8:
							if (cATEGORY == itm.CATEGORY.CATEGORY_PROTECTION && num6 == 2)
							{
								pBox[num3] = pl.PlayerParty.instance().item().normalItem(j);
								num3++;
							}
							break;
						case 9:
							if (cATEGORY == itm.CATEGORY.CATEGORY_PROTECTION && num6 == 3)
							{
								pBox[num3] = pl.PlayerParty.instance().item().normalItem(j);
								num3++;
							}
							break;
						case 11:
							if (pl.PlayerParty.instance().item().normalItem(j)
								.itemId() > 0 && pl.PlayerParty.instance().item().normalItem(j)
								.itemNumber() > 0)
							{
								pBox[num4] = pl.PlayerParty.instance().item().normalItem(j);
								num4++;
							}
							break;
						}
					}
				}
				else
				{
					for (int k = 0; k < 64; k++)
					{
						if (pl.PlayerParty.instance().item().importantItem(k)
							.itemNumber() == 0)
						{
							continue;
						}
						int num5 = pl.PlayerParty.instance().item().importantItem(k)
							.itemId();
						if (num5 > 0)
						{
							itm.CATEGORY cATEGORY2 = itm.ItemManager.instance().itemCategory((short)num5);
							if (cATEGORY2 == itm.CATEGORY.CATEGORY_IMPORTANT)
							{
								pBox[num4] = pl.PlayerParty.instance().item().importantItem(k);
								num4++;
							}
						}
					}
				}
				for (int l = 0; l < 384; l++)
				{
					buffer_[l] = null;
				}
				sort(num4, pBox, 0);
			}

			public void sort(int n, itm.PossessionItem[] x, int iOffset)
			{
				if (n <= 1)
				{
					return;
				}
				int num = n / 2;
				sort(num, x, iOffset);
				sort(n - num, x, iOffset + num);
				int i;
				for (i = 0; i < num; i++)
				{
					buffer_[i] = x[iOffset + i];
				}
				int num2 = num;
				int num3;
				i = (num3 = 0);
				while (i < num && num2 < n)
				{
					if (buffer_[i].itemId() <= x[iOffset + num2].itemId())
					{
						x[iOffset + num3++] = buffer_[i++];
					}
					else
					{
						x[iOffset + num3++] = x[iOffset + i++];
					}
				}
				while (i < num)
				{
					x[iOffset + num3++] = buffer_[i++];
				}
			}

			public void ResetTargetItemID()
			{
				int num = pBox[GET_ITEM_NUMBER(MenuManager.getSingleton().getFocuseMedget())].itemId();
				if (num <= 0)
				{
					MenuManager.getSingleton().SetTargetItemNo(-1);
				}
				else
				{
					MenuManager.getSingleton().SetTargetItemNo(num);
				}
			}

			public bool SetDrag(bool drag)
			{
				if (pMsgDrag != null)
				{
					pMsgDrag.release();
					pMsgDrag = null;
				}
				if (drag)
				{
					dgs.msg.CMessageMng.MSF_HANDLE_KIND font = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
					dgs.DGSMessageManager dGSMessageManager = null;
					dGSMessageManager = ((ownerMedget.display() != 1) ? dgs.msg.CMessageSys.getInstance().Sub() : dgs.msg.CMessageSys.getInstance().Main());
					Medget focuseMedget = MenuManager.getSingleton().getFocuseMedget();
					int num = pBox[GET_ITEM_NUMBER(focuseMedget)].itemId();
					if (num == -1)
					{
						return false;
					}
					int msg_number = itm.ItemManager.instance().itemParameter((short)num).nameId();
					pMsgDrag = dGSMessageManager.createMessage((uint)msg_number, dgs.INVALID_MSDHANDLE, (int)font);
					pMsgDrag.setPosition((short)(focuseMedget.x() + 16 + 2), (short)(focuseMedget.y() + (focuseMedget.height() - 12) / 2 + 2), erase: true);
					pMsgDrag.setDisplaySpeed(byte.MaxValue);
					pMsgDrag.setDisplayWait(0);
					pMsgDrag.setStyle(pMsgDrag.getStyle() | 0x1000);
				}
				sb.sbSetDrag(drag);
				drag_ = drag;
				return true;
			}

			public new static int classIdentifier()
			{
				return MBItemWindow_UN.number();
			}

			public override object queryInterface(int class_id)
			{
				if (class_id == classIdentifier())
				{
					return this;
				}
				return null;
			}

			public override bool bmUseTap(Medget M)
			{
				return true;
			}

			public bool CheckNowList(int val)
			{
				if (val != tItemBoxNo)
				{
					return false;
				}
				return true;
			}

			public ScrollBar getScrollBar()
			{
				return sb;
			}
		}
	}
}
