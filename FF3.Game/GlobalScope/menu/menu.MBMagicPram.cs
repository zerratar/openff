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
	public static partial class menu
	{
		public class MBMagicPram : MenuBehavior, SBEventHandler
		{
			public class MAGIC_ICON_TYPE
			{
				public sys2d.Cell mgIcon = new sys2d.Cell();

				public sys2d.Sprite3d mgIcon3d = new sys2d.Sprite3d();

				public ds.Vector2<short> pos = new ds.Vector2<short>();

				public int type;

				public bool bEnable;
			}

			public const int MESSAGE_LV_MAX = 16;

			public const int MESSAGE_USE_MAX = 16;

			public const int MESSAGE_MAGIC_MAX = 32;

			public const int MAGIC_ICON_MAX = 32;

			public const int MAGIC_TYPE_WHITE = 0;

			public const int MAGIC_TYPE_BLACK = 1;

			public const int MAGIC_TYPE_SUMMON = 2;

			public static dgs.UniqueNumber MBMagicPram_UN = new dgs.UniqueNumber();

			private ScrollBar sb = new ScrollBar();

			private bool sbFlag;

			private int sLine;

			private string limitBuf;

			private int maxLv;

			private int firstFocuse;

			private int lvCount;

			private int useCount;

			private bool savedVisibility;

			private bool vanishFlag_;

			private MAGIC_ICON_TYPE[] mIType = new MAGIC_ICON_TYPE[32];

			private dgs.DGSMessage[] pMsg = new dgs.DGSMessage[32];

			private dgs.DGSMessage[] pMsgLv = new dgs.DGSMessage[16];

			private dgs.DGSMessage[] pMsgLvNo = new dgs.DGSMessage[16];

			private dgs.DGSMessage[] pMsgUse = new dgs.DGSMessage[16];

			private int currentPlayer;

			public MBMagicPram()
			{
				for (int i = 0; i < mIType.Length; i++)
				{
					mIType[i] = new MAGIC_ICON_TYPE();
				}
				for (int j = 0; j < 32; j++)
				{
					pMsg[j] = null;
					mIType[j].bEnable = false;
				}
				for (int k = 0; k < 16; k++)
				{
					pMsgLv[k] = null;
				}
				for (int l = 0; l < 16; l++)
				{
					pMsgLvNo[l] = null;
				}
				for (int m = 0; m < 16; m++)
				{
					pMsgUse[m] = null;
				}
				lvCount = 0;
				useCount = 0;
			}

			~MBMagicPram()
			{
				int i = 0;
				for (; i < 32; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].release();
						pMsg[i] = null;
					}
					if (!mIType[i].bEnable)
					{
						mIType[i].mgIcon.Release();
						sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIType[i].mgIcon);
					}
				}
				lvCount = 0;
				useCount = 0;
			}

			public override void bmInitialize(Medget M)
			{
				vanishFlag_ = false;
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((M.display() != 0) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				dgs.msg.CMessageMng.MSF_HANDLE_KIND msfHandle = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
				ClearAllString();
				lvCount = 0;
				useCount = 0;
				for (int i = 0; i < 32; i++)
				{
					pMsg[i] = null;
				}
				sbFlag = false;
				firstFocuse = 0;
				int no = (currentPlayer = MenuManager.getSingleton().GetTargetCharNo());
				maxLv = 7;
				if (MenuManager.getSingleton().GetUsingMenuType() == 0)
				{
					CreateBattleMessage(dGSMessageManager, msfHandle, M, ref no);
					MenuManager.getSingleton().initFocus(firstFocuse);
				}
				else
				{
					CreateMainMenuMessage(dGSMessageManager, msfHandle, M, ref no, 0);
				}
				if (maxLv >= 4)
				{
					CreateExclusiveUseScrollBar(M);
				}
				if (opt.COptionManager.getSingleton().cursorOption().cursorPosition() == opt.CURSOR_POSITION.CURSOR_POSITION_MEMORY && MenuManager.getSingleton().battleMode())
				{
					vanishFlag_ = true;
					MenuManager.getSingleton().SetTrialInitFocuseFlag(val: false);
					MenuManager.getSingleton().initFocus(MenuManager.getSingleton().saveBattleMagicTarget(pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).playerId()));
					if (MenuManager.getSingleton().saveBattleMagicLine(pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).playerId()) >= 0)
					{
						sb.sbFixedMove((short)MenuManager.getSingleton().saveBattleMagicLine(pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).playerId()));
					}
				}
				vanishFlag_ = false;
			}

			public void sbehScrolled(short currentLine)
			{
				if (!sbFlag || sLine == currentLine)
				{
					return;
				}
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((MenuManager.getSingleton().getFocuseMedget().parentNode()
					.display() != 0) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				dgs.msg.CMessageMng.MSF_HANDLE_KIND msfHandle = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
				int targetCharNo = MenuManager.getSingleton().GetTargetCharNo();
				int num = 0;
				if (!vanishFlag_)
				{
					MenuManager.getSingleton().playSEMoveCursor();
				}
				else
				{
					vanishFlag_ = false;
				}
				ClearAllString();
				int num2 = currentLine - sLine;
				sLine = currentLine;
				MenuManager.getSingleton().saveBattleMagicLine_set(pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).playerId(), sLine);
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					int num3 = (int)medget.work();
					num3 += num2;
					SET_LEVEL(medget, num3);
					int num4 = (medget.height() - 12) / 2;
					if (++num == 3)
					{
						CreateLvMessage(dGSMessageManager, msfHandle, MenuManager.getSingleton().getFocuseMedget().parentNode(), (int)medget.work(), medget.y() + num4);
						CreateUseMessage(dGSMessageManager, msfHandle, MenuManager.getSingleton().getFocuseMedget().parentNode(), (int)medget.work(), medget.y() + num4);
						num = 0;
					}
					int num5 = pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
						.equipMagic((pl.MAGIC_LEVEL)(int)medget.work())
						.magicId((sbyte)medget.work1());
					itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)num5);
					if (itemBaseParameter == null)
					{
						SET_MSG_IDX(medget, -1);
					}
					else
					{
						int nameId = itemBaseParameter.nameId();
						CreateMagicNameString(dGSMessageManager, msfHandle, medget.x(), medget.y() + num4, nameId, num5, medget);
						if (gcscmp(medget._id(), limitBuf) == 0)
						{
							break;
						}
					}
				}
			}

			public override void bmBehave(Medget M)
			{
				if (M.display() == 0)
				{
					dgs.msg.CMessageSys.getInstance().Sub();
				}
				else
				{
					dgs.msg.CMessageSys.getInstance().Main();
				}
				if (MenuManager.getSingleton().GetUsingMenuType() == 0)
				{
					MenuManager.getSingleton().saveBattleMagicTarget_set(pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).playerId(), MenuManager.getSingleton().getFocuseMedget().myTag());
					if (MenuManager.getSingleton().GetScrollType() != MenuManager.SCROLL_TYPE.TYPE_WAIT)
					{
						if (MenuManager.getSingleton().GetScrollType() == MenuManager.SCROLL_TYPE.TYPE_UP)
						{
							if ((int)M.childNode().work() <= 0)
							{
								sb.sbFixedMove(4);
								int focusedCursor = MenuManager.getSingleton().getFocuseMedget().myTag() + 9;
								MenuManager.getSingleton().initFocus(focusedCursor);
							}
							else
							{
								sb.sbFixedMove(-1);
							}
						}
						else if (MenuManager.getSingleton().GetScrollType() == MenuManager.SCROLL_TYPE.TYPE_DOWN)
						{
							if ((int)M.childNode().work() == 4)
							{
								sb.sbFixedMove(-4);
								int focusedCursor2 = MenuManager.getSingleton().getFocuseMedget().myTag() - 9;
								MenuManager.getSingleton().initFocus(focusedCursor2);
							}
							else
							{
								sb.sbFixedMove(1);
							}
						}
					}
				}
				else if (MenuManager.getSingleton().GetScrollType() == MenuManager.SCROLL_TYPE.TYPE_UP)
				{
					sb.sbFixedMove(-1);
				}
				else if (MenuManager.getSingleton().GetScrollType() == MenuManager.SCROLL_TYPE.TYPE_DOWN)
				{
					sb.sbFixedMove(1);
				}
				Medget focuseMedget = MenuManager.getSingleton().getFocuseMedget();
				if (focuseMedget.parentNode() != ownerMedget)
				{
					return;
				}
				int targetCharNo = MenuManager.getSingleton().GetTargetCharNo();
				int num = pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
					.equipMagic((pl.MAGIC_LEVEL)focuseMedget.work())
					.magicId((sbyte)focuseMedget.work1());
				if (num < 0 || evt.CEventRestriction.getSingleton().check(num))
				{
					if (num >= 0 && MenuManager.getSingleton().GetMagicMenuType() == 2)
					{
						MenuManager.getSingleton().SetTargetItemNo(num);
					}
					else
					{
						MenuManager.getSingleton().SetTargetItemNo(-1);
						if (MenuManager.getSingleton().GetUsingMenuType() == 0 && num >= 0)
						{
							MenuManager.getSingleton().SetTargetItemNo(num);
						}
					}
				}
				else
				{
					MenuManager.getSingleton().SetTargetItemNo(num);
				}
				ClearColorMagicName(M);
				if (GET_MSG_IDX(focuseMedget) != -1 && pMsg[GET_MSG_IDX(focuseMedget)] != null)
				{
					pMsg[GET_MSG_IDX(focuseMedget)].setMessageColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
				}
			}

			public void ClearColorMagicName(Medget pMedget)
			{
				if (MenuManager.getSingleton().GetUsingMenuType() == 0)
				{
					MenuManager.getSingleton().SetMagicMenuType(-1);
				}
				for (Medget medget = pMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (GET_MSG_IDX(medget) != -1 && pMsg[GET_MSG_IDX(medget)] != null)
					{
						if (GET_ITEM_USABILITY(medget) != 0 || MenuManager.getSingleton().GetMagicMenuType() == 2)
						{
							pMsg[GET_MSG_IDX(medget)].setMessageColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
						}
						else
						{
							pMsg[GET_MSG_IDX(medget)].setMessageColor(dgs.TXT_COLOR.TXT_UCOLOR_4);
						}
					}
				}
			}

			public override void bmFinalize(Medget M)
			{
				ClearAllString();
				if (sbFlag)
				{
					sb.sbDestroy();
					sb.sbSetHandler(null);
				}
			}

			public override bool bmDecide(Medget M)
			{
				MenuManager.getSingleton().SetDecideButtonState(0);
				return false;
			}

			public override void bmActivate(Medget M)
			{
				MenuManager.getSingleton().SetActivateButtonState(0);
			}

			public override bool bmCancel(Medget M)
			{
				MenuManager.getSingleton().SetCancelButtonState(0);
				return false;
			}

			public override bool bmDirection(Medget M, int key)
			{
				return MenuManager.getSingleton().MoveCursor(M, PlaySe: true);
			}

			public override void bmSuspend(Medget M)
			{
				for (int i = 0; i < 32; i++)
				{
					if (pMsg[i] != null)
					{
						savedVisibility = pMsg[i].activity();
						pMsg[i].setVisibility(b: false);
					}
					if (mIType[i].bEnable)
					{
						mIType[i].mgIcon.SetShow(show: false);
					}
				}
				for (int j = 0; j < lvCount; j++)
				{
					if (pMsgLv[j] != null)
					{
						savedVisibility = pMsgLv[j].activity();
						pMsgLv[j].setVisibility(b: false);
					}
					if (pMsgLvNo[j] != null)
					{
						savedVisibility = pMsgLvNo[j].activity();
						pMsgLvNo[j].setVisibility(b: false);
					}
				}
				for (int k = 0; k < useCount; k++)
				{
					if (pMsgUse[k] != null)
					{
						savedVisibility = pMsgUse[k].activity();
						pMsgUse[k].setVisibility(b: false);
					}
				}
				if (sbFlag)
				{
					sb.sbPartsActivateProcess(0);
				}
			}

			public void bmAreaSuspend(Medget pMedget, int type)
			{
				for (Medget medget = pMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (type == 0)
					{
						if ((sbyte)medget.work1() == 0 && GET_MSG_IDX(medget) >= 0)
						{
							if (pMsg[GET_MSG_IDX(medget)] != null)
							{
								pMsg[GET_MSG_IDX(medget)].setVisibility(b: false);
							}
							if (mIType[GET_MSG_IDX(medget)].bEnable)
							{
								mIType[GET_MSG_IDX(medget)].mgIcon.SetShow(show: false);
							}
						}
					}
					else if ((sbyte)medget.work1() != 0 && GET_MSG_IDX(medget) >= 0)
					{
						if (pMsg[GET_MSG_IDX(medget)] != null)
						{
							pMsg[GET_MSG_IDX(medget)].setVisibility(b: false);
						}
						if (mIType[GET_MSG_IDX(medget)].bEnable)
						{
							mIType[GET_MSG_IDX(medget)].mgIcon.SetShow(show: false);
						}
					}
				}
				if (type != 0)
				{
					return;
				}
				for (int i = 0; i < 16; i++)
				{
					if (pMsgLv[i] != null)
					{
						pMsgLv[i].setVisibility(b: false);
					}
					if (pMsgLvNo[i] != null)
					{
						pMsgLvNo[i].setVisibility(b: false);
					}
					if (pMsgUse[i] != null)
					{
						pMsgUse[i].setVisibility(b: false);
					}
				}
			}

			public override void bmResume(Medget M)
			{
				for (int i = 0; i < 32; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].setVisibility(savedVisibility);
					}
					if (mIType[i].bEnable)
					{
						mIType[i].mgIcon.SetShow(show: true);
					}
				}
				for (int j = 0; j < lvCount; j++)
				{
					if (pMsgLv[j] != null)
					{
						pMsgLv[j].setVisibility(savedVisibility);
					}
					if (pMsgLvNo[j] != null)
					{
						pMsgLvNo[j].setVisibility(savedVisibility);
					}
				}
				for (int k = 0; k < useCount; k++)
				{
					if (pMsgUse[k] != null)
					{
						pMsgUse[k].setVisibility(savedVisibility);
					}
				}
				if (sbFlag)
				{
					sb.sbPartsActivateProcess(1);
				}
			}

			public void bmAreaResume(Medget pMedget, int type)
			{
				for (Medget medget = pMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (type == 0)
					{
						if ((sbyte)medget.work1() == 0 && GET_MSG_IDX(medget) >= 0)
						{
							if (pMsg[GET_MSG_IDX(medget)] != null)
							{
								pMsg[GET_MSG_IDX(medget)].setVisibility(b: true);
							}
							if (mIType[GET_MSG_IDX(medget)].bEnable)
							{
								mIType[GET_MSG_IDX(medget)].mgIcon.SetShow(show: true);
							}
						}
					}
					else if ((sbyte)medget.work1() != 0 && GET_MSG_IDX(medget) >= 0)
					{
						if (pMsg[GET_MSG_IDX(medget)] != null)
						{
							pMsg[GET_MSG_IDX(medget)].setVisibility(b: true);
						}
						if (mIType[GET_MSG_IDX(medget)].bEnable)
						{
							mIType[GET_MSG_IDX(medget)].mgIcon.SetShow(show: true);
						}
					}
				}
				if (type != 0)
				{
					return;
				}
				for (int i = 0; i < 16; i++)
				{
					if (pMsgLv[i] != null)
					{
						pMsgLv[i].setVisibility(b: true);
					}
					if (pMsgLvNo[i] != null)
					{
						pMsgLvNo[i].setVisibility(b: true);
					}
					if (pMsgUse[i] != null)
					{
						pMsgUse[i].setVisibility(b: true);
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

			public void bmMagicVisibility(bool v)
			{
				for (int i = 0; i < 32; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].setVisibility(v);
					}
				}
			}

			public void CreateBattleMessage(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND _msfHandle, Medget pParent, ref int no)
			{
				string[] array = new string[16]
				{
					"one_thr", "two_thr", "thr_thr", "", "", "", "", "", "", "",
					"", "", "", "", "", ""
				};
				string arg;
				if (maxLv < 3)
				{
					strcpy(out arg, array[maxLv]);
				}
				else
				{
					strcpy(out arg, "for_thr");
				}
				strcpy(out limitBuf, arg);
				int num = 0;
				int num2 = 0;
				for (Medget medget = pParent.childNode(); medget != null; medget = medget.nextSibling())
				{
					MenuManager.getSingleton().joinFocusList(medget);
					if (medget != null)
					{
						int num3 = pl.PlayerParty.instance().player((byte)no).equipParameter()
							.equipMagic((pl.MAGIC_LEVEL)num)
							.magicId((sbyte)medget.work1());
						SET_LEVEL(medget, num);
						int num4 = (medget.height() - 12) / 2;
						num2++;
						if (num2 == 3)
						{
							CreateLvMessage(pm, _msfHandle, pParent, num, medget.y() + num4);
							CreateUseMessage(pm, _msfHandle, pParent, num, medget.y() + num4);
							num2 = 0;
							num++;
							if (num < 0)
							{
								medget.setDown("dummy");
								medget.prevSibling().setDown("dummy");
								medget.prevSibling().prevSibling().setDown("dummy");
								break;
							}
						}
						itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)num3);
						if (itemBaseParameter == null)
						{
							SET_MSG_IDX(medget, -1);
						}
						else
						{
							int nameId = itemBaseParameter.nameId();
							CreateMagicNameString(pm, _msfHandle, medget.x(), medget.y() + num4, nameId, num3, medget);
							if (GET_MSG_IDX(medget) == 0)
							{
								SetCursorFirstFocuse(medget);
							}
						}
					}
				}
			}

			public void CreateMainMenuMessage(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND _msfHandle, Medget pParent, ref int no, int lv)
			{
				int num = lv;
				int num2 = 0;
				for (Medget medget = pParent.childNode(); medget != null; medget = medget.nextSibling())
				{
					int num3 = pl.PlayerParty.instance().player((byte)no).equipParameter()
						.equipMagic((pl.MAGIC_LEVEL)num)
						.magicId((sbyte)medget.work1());
					itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)num3);
					SET_LEVEL(medget, num);
					int num4 = (medget.height() - 12) / 2;
					if (++num2 == 3)
					{
						CreateLvMessage(pm, _msfHandle, pParent, num, medget.y() + num4);
						CreateUseMessage(pm, _msfHandle, pParent, num, medget.y() + num4);
						num2 = 0;
						num++;
					}
					if (num3 <= 0)
					{
						SET_MSG_IDX(medget, -1);
					}
					else if (itemBaseParameter != null)
					{
						int nameId = itemBaseParameter.nameId();
						CreateMagicNameString(pm, _msfHandle, medget.x(), medget.y() + num4, nameId, num3, medget);
					}
				}
			}

			public void CreateMagicNameString(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND msfHandle, int x, int y, int nameId, int magicId, Medget pTarget)
			{
				int num = -1;
				for (int i = 0; i < 32; i++)
				{
					if (pMsg[i] == null)
					{
						num = i;
						break;
					}
				}
				if (num < 0)
				{
					return;
				}
				pMsg[num] = pm.createMessage((uint)nameId, MenuManager.getSingleton().GetItemDataTextNo(), (int)msfHandle);
				if (pMsg[num] == null)
				{
					return;
				}
				pMsg[num].setPosition((short)(x + 16), (short)y, erase: true);
				pMsg[num].setDisplaySpeed(byte.MaxValue);
				pMsg[num].setDisplayWait(0);
				if (pl.PlayerParty.instance().player((byte)currentPlayer).condition()
					.isSilence() || pl.PlayerParty.instance().player((byte)currentPlayer).condition()
					.isDeath() || pl.PlayerParty.instance().player((byte)currentPlayer).condition()
					.isStone())
				{
					SET_ITEM_USABILITY(pTarget, 0);
				}
				else
				{
					itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter((short)magicId);
					if (magicParameter == null)
					{
						SET_ITEM_USABILITY(pTarget, 0);
					}
					else if (!pl.PlayerParty.instance().player((byte)currentPlayer).isEquipItem(magicParameter.equipJob()))
					{
						SET_ITEM_USABILITY(pTarget, 0);
					}
					else if (evt.CEventRestriction.getSingleton().check(magicId))
					{
						SET_ITEM_USABILITY(pTarget, 0);
					}
					else if (pl.PlayerParty.instance().player((byte)currentPlayer).condition()
						.isFrog())
					{
						if (magicId == 4005)
						{
							SET_ITEM_USABILITY(pTarget, 1);
						}
						else
						{
							SET_ITEM_USABILITY(pTarget, 0);
						}
					}
					else if (MenuManager.getSingleton().GetUsingMenuType() == 0)
					{
						SET_ITEM_USABILITY(pTarget, magicParameter.useBattle());
					}
					else if (magicParameter.useField() == 2 && wld.WorldPart.getInstance().getWorldSystem().PreviousMode() != wld.CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
					{
						SET_ITEM_USABILITY(pTarget, 0);
					}
					else if (magicParameter.useField() == 3 && wld.WorldPart.getInstance().getWorldSystem().PreviousMode() != wld.CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN)
					{
						SET_ITEM_USABILITY(pTarget, 0);
					}
					else
					{
						SET_ITEM_USABILITY(pTarget, magicParameter.useField());
					}
				}
				SET_MSG_IDX(pTarget, num);
				CreateMagicTypeIcon(pm, msfHandle, x, y + -2, magicId, pTarget, num);
			}

			public void CreateMagicTypeIcon(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND _msfHandle, int x, int y, int magicNo, Medget pMedget, int indexNo)
			{
				itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)magicNo);
				if (itemBaseParameter == null)
				{
					return;
				}
				short num = itemBaseParameter.system();
				mIType[indexNo].type = num;
				mIType[indexNo].bEnable = true;
				mIType[indexNo].pos.vx = (short)x;
				mIType[indexNo].pos.vy = (short)y;
				if (MenuManager.getSingleton().GetUsingMenuType() == 1)
				{
					mIType[indexNo].mgIcon.copy(MenuManager.getSingleton().GetSmallIcon2d());
					switch (num)
					{
					case 0:
						mIType[indexNo].mgIcon.SetCell(30);
						break;
					case 1:
						mIType[indexNo].mgIcon.SetCell(31);
						break;
					case 2:
						mIType[indexNo].mgIcon.SetCell(32);
						break;
					}
					mIType[indexNo].mgIcon.SetPositionI(mIType[indexNo].pos.vx, mIType[indexNo].pos.vy);
					mIType[indexNo].mgIcon.SetShow(show: true);
					mIType[indexNo].mgIcon.SetPriority(2);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(mIType[indexNo].mgIcon);
				}
				else
				{
					mIType[indexNo].mgIcon3d.copy(MenuManager.getSingleton().GetSmallIcon3d());
					switch (num)
					{
					case 0:
						mIType[indexNo].mgIcon3d.SetCell(30);
						break;
					case 1:
						mIType[indexNo].mgIcon3d.SetCell(31);
						break;
					case 2:
						mIType[indexNo].mgIcon3d.SetCell(32);
						break;
					}
					mIType[indexNo].mgIcon3d.SetPositionI(mIType[indexNo].pos.vx, mIType[indexNo].pos.vy);
					mIType[indexNo].mgIcon3d.SetShow(show: true);
					mIType[indexNo].mgIcon3d.SetDepth(1);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(mIType[indexNo].mgIcon3d);
				}
			}

			public void CreateLvMessage(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND _msfHandle, Medget pM, int lv, int y)
			{
				int msg_number = 50414;
				dgs.msg.CMessageSys.getInstance().changeValueFont(lv + 1, out var after);
				if (MenuManager.getSingleton().GetUsingMenuType() == 0)
				{
					pMsgLv[lvCount] = pm.createMessage((uint)msg_number, dgs.INVALID_MSDHANDLE, (int)_msfHandle);
					pMsgLvNo[lvCount] = pm.createMessage(after, (int)_msfHandle);
					pMsgLv[lvCount].setPosition(16, (short)y, erase: true);
					pMsgLvNo[lvCount].setPosition(48, (short)y, erase: true);
				}
				else
				{
					pMsgLv[lvCount] = pm.createMessage((uint)msg_number, dgs.INVALID_MSDHANDLE, (int)_msfHandle);
					pMsgLv[lvCount].setPosition(16, (short)y, erase: true);
					pMsgLvNo[lvCount] = pm.createMessage(after, (int)_msfHandle);
					pMsgLvNo[lvCount].setPosition(48, (short)y, erase: true);
				}
				pMsgLv[lvCount].setDisplaySpeed(byte.MaxValue);
				pMsgLvNo[lvCount].setDisplaySpeed(byte.MaxValue);
				pMsgLv[lvCount].setDisplayWait(0);
				pMsgLvNo[lvCount].setDisplayWait(0);
				lvCount++;
			}

			public void CreateUseMessage(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND _msfHandle, Medget pM, int lv, int y)
			{
				OS_Printf("CreateUseMessage target[%d]\n", currentPlayer);
				int now = pl.PlayerParty.instance().player((byte)currentPlayer).mp(lv)
					.getNow();
				dgs.msg.CMessageSys.getInstance().changeValueFont(now, out var after);
				pMsgUse[useCount] = pm.createMessage(after, (int)_msfHandle);
				ds.Vector2<short> vector = new ds.Vector2<short>();
				pMsgUse[useCount].getTextSize(vector);
				if (MenuManager.getSingleton().GetUsingMenuType() == 1)
				{
					pMsgUse[useCount].setPosition((short)(104 - vector.vx), (short)y, erase: true);
				}
				else
				{
					pMsgUse[useCount].setPosition((short)(104 - vector.vx), (short)y, erase: true);
				}
				pMsgUse[useCount].setDisplaySpeed(byte.MaxValue);
				pMsgUse[useCount].setDisplayWait(0);
				useCount++;
			}

			public void CreateExclusiveUseScrollBar(Medget pT)
			{
				XbnNode firstNodeByTagNameFromChildren = pT.node().getFirstNodeByTagNameFromChildren(TRANSCODE("behavior"));
				if (firstNodeByTagNameFromChildren == null)
				{
					return;
				}
				XbnNodeList xbnNodeList = new XbnNodeList();
				firstNodeByTagNameFromChildren.getNodesByTagNameFromChildren(TRANSCODE("parameter"), xbnNodeList);
				int num = 0;
				if (xbnNodeList.size() > 0)
				{
					num = xbnNodeList[0].nodeValueInt();
				}
				int num2 = 0;
				if (xbnNodeList.size() > 1)
				{
					num2 = xbnNodeList[1].nodeValueInt();
				}
				if (num != 0)
				{
					sb.sbCreate();
					sb.sbSetPosition((short)(pT.x() + pT.width() - (ScrollBar.PARTS_W >> 12) - SCROLL_BAR_MARGIN), (short)(pT.y() + SCROLL_BAR_MARGIN));
					sb.sbSetHeight((short)(pT.height() - SCROLL_BAR_MARGIN * 2));
					sb.sbPartsSetPriority(1);
					if (MenuManager.getSingleton().GetUsingMenuType() == 0)
					{
						num2 = ((maxLv > 3) ? 4 : maxLv);
						sb.sbSetCapacity((short)num2, (short)(maxLv + 1));
					}
					else
					{
						sb.sbSetCapacity((short)num2, (short)(maxLv + 1));
					}
					sb.sbSetHandler(this);
					sLine = 0;
					sbFlag = true;
				}
			}

			public void ClearTargetMessageCount(dgs.DGSMessage[] pT, ref int pCount)
			{
				for (int i = 0; i < pCount; i++)
				{
					if (pT[i] != null)
					{
						pT[i].release();
						pT[i] = null;
					}
				}
				pCount = 0;
			}

			public void SetCursorFirstFocuse(Medget pTarget)
			{
				firstFocuse = pTarget.myTag();
				MenuManager.getSingleton().SetTrialInitFocuseFlag(val: false);
			}

			public bool SetTargetMedgetMovementDummy(Medget pTarget)
			{
				if (gcscmp(pTarget._id(), limitBuf) == 0)
				{
					pTarget.prevSibling().prevSibling().setDown("dummy");
					pTarget.prevSibling().setDown("dummy");
					pTarget.setDown("dummy");
					return false;
				}
				return true;
			}

			public void ClearAllString()
			{
				for (int i = 0; i < 32; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].release();
						pMsg[i] = null;
					}
					if (mIType[i].bEnable)
					{
						if (MenuManager.getSingleton().GetUsingMenuType() == 1)
						{
							mIType[i].mgIcon.Release();
							sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIType[i].mgIcon);
						}
						else
						{
							mIType[i].mgIcon3d.Release();
							sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIType[i].mgIcon3d);
						}
						mIType[i].bEnable = true;
					}
				}
				int num = lvCount;
				ClearTargetMessageCount(pMsgLv, ref lvCount);
				lvCount = num;
				ClearTargetMessageCount(pMsgLvNo, ref lvCount);
				ClearTargetMessageCount(pMsgUse, ref useCount);
			}

			public void ChangeMagic(Medget pCurrent)
			{
				int targetCharNo = MenuManager.getSingleton().GetTargetCharNo();
				pl.PlayerEquipParameter playerEquipParameter = pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter();
				pl.PlayerEquipParameter playerEquipParameter2 = pl.PlayerParty.instance().player((byte)currentPlayer).equipParameter();
				for (int i = 0; i < 8; i++)
				{
					pl.EquipmentMagic arg = playerEquipParameter.equipMagic((pl.MAGIC_LEVEL)i);
					playerEquipParameter.equipMagic_set((pl.MAGIC_LEVEL)i, playerEquipParameter2.equipMagic((pl.MAGIC_LEVEL)i));
					playerEquipParameter2.equipMagic_set((pl.MAGIC_LEVEL)i, arg);
				}
				ClearAllString();
				dgs.DGSMessageManager pm = dgs.msg.CMessageSys.getInstance().Sub();
				dgs.msg.CMessageMng.MSF_HANDLE_KIND msfHandle = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
				CreateMainMenuMessage(pm, msfHandle, ownerMedget, ref currentPlayer, 0);
				sLine = 0;
				sb.sbSetLine(0);
			}

			public void ResettingMagic(Medget pMedget, int cNo)
			{
				ClearAllString();
				dgs.DGSMessageManager pm = dgs.msg.CMessageSys.getInstance().Sub();
				dgs.msg.CMessageMng.MSF_HANDLE_KIND msfHandle = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
				CreateMainMenuMessage(pm, msfHandle, pMedget, ref cNo, sLine);
				ClearColorMagicName(pMedget);
			}

			public void SelectAreaResettingData(Medget pMedget, int target)
			{
				for (int i = 0; i < 32; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].release();
						pMsg[i] = null;
					}
					if (mIType[i].bEnable)
					{
						if (MenuManager.getSingleton().GetUsingMenuType() == 1)
						{
							mIType[i].mgIcon.Release();
							sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIType[i].mgIcon);
						}
						else
						{
							mIType[i].mgIcon3d.Release();
							sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIType[i].mgIcon3d);
						}
						mIType[i].bEnable = false;
					}
				}
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((pMedget.display() != 0) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				dgs.msg.CMessageMng.MSF_HANDLE_KIND msfHandle = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
				for (Medget medget = pMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					int num = pl.PlayerParty.instance().player((byte)target).equipParameter()
						.equipMagic((pl.MAGIC_LEVEL)medget.work())
						.magicId((sbyte)medget.work1());
					itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)num);
					int num2 = (medget.height() - 12) / 2;
					if (itemBaseParameter == null)
					{
						SET_MSG_IDX(medget, -1);
					}
					else
					{
						int nameId = itemBaseParameter.nameId();
						CreateMagicNameString(dGSMessageManager, msfHandle, medget.x(), medget.y() + num2, nameId, num, medget);
					}
				}
				ClearColorMagicName(ownerMedget);
			}

			public void SelectAreaUseCountResetting(Medget pMedget)
			{
				int num = lvCount;
				ClearTargetMessageCount(pMsgLv, ref lvCount);
				lvCount = num;
				ClearTargetMessageCount(pMsgLvNo, ref lvCount);
				num = useCount;
				ClearTargetMessageCount(pMsgUse, ref useCount);
				int num2 = (int)pMedget.childNode().work();
				int num3 = 0;
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((pMedget.display() != 0) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				dgs.msg.CMessageMng.MSF_HANDLE_KIND msfHandle = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
				for (Medget medget = pMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					int num4 = (medget.height() - 12) / 2;
					if (++num3 == 3)
					{
						CreateLvMessage(dGSMessageManager, msfHandle, pMedget, (int)medget.work(), medget.y() + num4);
						CreateUseMessage(dGSMessageManager, msfHandle, pMedget, (int)medget.work(), medget.y() + num4);
						num3 = 0;
						num2++;
					}
				}
			}

			public void ChangeUseCountMessage(Medget pCurrent, int lv, int currentPlayer)
			{
				int num = lv - sLine;
				if (num >= 0 && lv >= 0)
				{
					if (pMsgUse[num] != null)
					{
						pMsgUse[num].release();
						pMsgUse[num] = null;
					}
					dgs.DGSMessageManager dGSMessageManager = null;
					dGSMessageManager = ((pCurrent.parentNode().display() != 0) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
					dgs.msg.CMessageMng.MSF_HANDLE_KIND font = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
					int num2 = (pCurrent.height() - 12) / 2;
					int now = pl.PlayerParty.instance().player((byte)currentPlayer).mp(lv)
						.getNow();
					dgs.msg.CMessageSys.getInstance().changeValueFont(now, out var after);
					pMsgUse[num] = dGSMessageManager.createMessage(after, (int)font);
					ds.Vector2<short> vector = new ds.Vector2<short>();
					pMsgUse[num].getTextSize(vector);
					pMsgUse[num].setPosition((short)(104 - vector.vx), (short)(pCurrent.y() + num2), erase: true);
					pMsgUse[num].setDisplaySpeed(byte.MaxValue);
					pMsgUse[num].setDisplayWait(0);
				}
			}

			public new static int classIdentifier()
			{
				return MBMagicPram_UN.number();
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

			public void SetCurrentPlayer(int p)
			{
				currentPlayer = p;
			}

			public int GetCurrentPlayer()
			{
				return currentPlayer;
			}
		}
	}
}
