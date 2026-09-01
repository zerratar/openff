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
		public class MBMonsterList : MenuBehavior, SBEventHandler
		{
			public static dgs.UniqueNumber MBMonsterList_UN = new dgs.UniqueNumber();

			public static int MonsterListMsgMax = 8;

			public static int MOBLIST_LINES_PAR_PAGE = 5;

			private ScrollBar sb = new ScrollBar();

			private bool sbFlag;

			private int sbLine;

			private int maxMonsterLine;

			private int sbPageLine;

			private bool refresh_;

			private int item_id;

			private bool savedVisibility;

			private bool suspend_;

			private MLIST_MSG_TYPE[] mName = new MLIST_MSG_TYPE[MonsterListMsgMax];

			private MLIST_MSG_TYPE[] mNoStr = new MLIST_MSG_TYPE[MonsterListMsgMax];

			private MLIST_MSG_TYPE[] mNoNum = new MLIST_MSG_TYPE[MonsterListMsgMax];

			private MLIST_MSG_TYPE[] mKnock = new MLIST_MSG_TYPE[MonsterListMsgMax];

			public MBMonsterList()
			{
				for (int i = 0; i < mName.Length; i++)
				{
					mName[i] = new MLIST_MSG_TYPE();
				}
				for (int i = 0; i < mNoStr.Length; i++)
				{
					mNoStr[i] = new MLIST_MSG_TYPE();
				}
				for (int i = 0; i < mNoNum.Length; i++)
				{
					mNoNum[i] = new MLIST_MSG_TYPE();
				}
				for (int i = 0; i < mKnock.Length; i++)
				{
					mKnock[i] = new MLIST_MSG_TYPE();
				}
				InitMlistMsgTypeArray(mName);
				InitMlistMsgTypeArray(mNoStr);
				InitMlistMsgTypeArray(mNoNum);
				InitMlistMsgTypeArray(mKnock);
				sbFlag = false;
				sbLine ^= sbLine;
				item_id = -1;
			}

			~MBMonsterList()
			{
				ReleaseMlistMsgTypeArray(mName);
				ReleaseMlistMsgTypeArray(mNoStr);
				ReleaseMlistMsgTypeArray(mNoNum);
				ReleaseMlistMsgTypeArray(mKnock);
				if (sbFlag)
				{
					sb.sbDestroy();
					sb.sbSetHandler(null);
					sbFlag = false;
				}
				suspend_ = false;
			}

			public void InitMlistMsgTypeArray(MLIST_MSG_TYPE[] pArray)
			{
				for (int i = 0; i < MonsterListMsgMax; i++)
				{
					pArray[i].bEnable = false;
					pArray[i].pMsg = null;
				}
			}

			public void ReleaseMlistMsgTypeArray(MLIST_MSG_TYPE[] pArray)
			{
				for (int i = 0; i < MonsterListMsgMax; i++)
				{
					if (pArray[i].bEnable)
					{
						pArray[i].bEnable = false;
					}
					if (pArray[i].pMsg != null)
					{
						pArray[i].pMsg.release();
						pArray[i].pMsg = null;
					}
				}
			}

			public static bool isMobItemVisible(int i)
			{
				if (i < 0 || i >= MONSTER_ID_MAX)
				{
					return false;
				}
				if (i == 0 || i == 225 || i == 228 || i == 229 || i == 230 || i == 231 || i == 232 || i == 233 || i == 234 || i == 235 || i == 236 || i == 237 || i == 238 || i == 239 || i == 240 || i == 241 || i == 242 || i == 243 || i == 244 || i == 245 || i == 246 || i == 247 || i == 248 || i == 249 || i == 250 || i == 251 || i == 252 || i == 253 || i == 254 || i == 255)
				{
					return false;
				}
				return true;
			}

			public override void bmInitialize(Medget M)
			{
				InitMlistMsgTypeArray(mName);
				InitMlistMsgTypeArray(mNoStr);
				InitMlistMsgTypeArray(mNoNum);
				InitMlistMsgTypeArray(mKnock);
				int num = 0;
				for (int i = 0; MONSTER_ID_MAX >= i; i++)
				{
					if (isMobItemVisible(i) && MONSTER_MAX > num)
					{
						TempMonsterIdBox[num++] = i;
					}
				}
				sbFlag = false;
				sbLine ^= sbLine;
				CreateExclusiveScrollBar(M);
			}

			public override void bmPostInitialize(Medget M)
			{
				bmRefreshMonsterList(M, dgs.msg.CMessageSys.getInstance().Sub(), dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, 0);
			}

			public override void bmFinalize(Medget M)
			{
				ReleaseMlistMsgTypeArray(mName);
				ReleaseMlistMsgTypeArray(mNoStr);
				ReleaseMlistMsgTypeArray(mNoNum);
				ReleaseMlistMsgTypeArray(mKnock);
				if (sbFlag)
				{
					sb.sbDestroy();
					sb.sbSetHandler(null);
					sbFlag = false;
				}
			}

			public override void bmBehave(Medget M)
			{
				if (suspend_)
				{
					return;
				}
				if (CheckScroll(M))
				{
					int targetLine = 0;
					bool flag = false;
					if (MenuManager.getSingleton().GetScrollType() == MenuManager.SCROLL_TYPE.TYPE_DOWN && !ScrolledEnd(M))
					{
						targetLine = (int)M.childNode().work() + 1;
						flag = true;
					}
					else if (MenuManager.getSingleton().GetScrollType() == MenuManager.SCROLL_TYPE.TYPE_UP && !ScrolledLead(M))
					{
						targetLine = (int)M.childNode().work() - 1;
						flag = true;
					}
					if (flag)
					{
						bmRefreshMonsterList(M, dgs.msg.CMessageSys.getInstance().Sub(), dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, targetLine);
					}
					if (sbFlag)
					{
						sbLine = (int)M.childNode().work();
						sb.sbSetLine((short)sbLine);
					}
				}
				else if (refresh_)
				{
					bmRefreshMonsterList(M, dgs.msg.CMessageSys.getInstance().Sub(), dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, (int)M.childNode().work());
					refresh_ = false;
				}
				int num = (int)MenuManager.getSingleton().getFocuseMedget().work();
				if (-1 < num && 256 > num)
				{
					MenuManager.getSingleton().SetTargetItemNo(TempMonsterIdBox[num]);
				}
			}

			public override bool bmDecide(Medget M)
			{
				if (!suspend_)
				{
					MenuManager.getSingleton().SetDecideButtonState(0);
				}
				return true;
			}

			public override bool bmCancel(Medget M)
			{
				if (!suspend_)
				{
					MenuManager.getSingleton().SetCancelButtonState(0);
				}
				return true;
			}

			public override bool bmDirection(Medget M, int key)
			{
				if (!suspend_)
				{
					MenuManager.getSingleton().MoveCursor(M, PlaySe: true);
					ChangeColorTargetMsg(ownerMedget);
				}
				return true;
			}

			public override void bmActivate(Medget M)
			{
				ChangeColorTargetMsg(ownerMedget);
			}

			public override void bmDeactivate(Medget M)
			{
				ChangeColorTargetMsg(ownerMedget);
			}

			public override void bmSuspend(Medget M)
			{
				suspend_ = true;
			}

			public override void bmResume(Medget M)
			{
				suspend_ = false;
				MenuManager.getSingleton().inputPermission(b: true);
			}

			public void bmMListVisibility(bool v)
			{
			}

			public void sbehScrolled(short currentLine)
			{
				if (sbFlag && !MenuManager.getSingleton().GetImposibleScrollFlag())
				{
					if (sbLine != currentLine)
					{
						MenuManager.getSingleton().playSEMoveCursor();
					}
					sbLine = currentLine;
					bmRefreshMonsterList(MenuManager.getSingleton().getFocuseMedget().parentNode(), dgs.msg.CMessageSys.getInstance().Sub(), dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, currentLine);
					MenuManager.getSingleton().playSEMoveCursor();
					MenuManager.getSingleton().SetScrollType(MenuManager.SCROLL_TYPE.TYPE_WAIT);
				}
			}

			public void ChangeColorTargetMsg(Medget pParent)
			{
				Medget focuseMedget = MenuManager.getSingleton().getFocuseMedget();
				int num = 0;
				for (Medget medget = pParent.childNode(); medget != null; medget = medget.nextSibling())
				{
					dgs.TXT_COLOR messageColor = ((medget != focuseMedget) ? dgs.TXT_COLOR.TXT_COLOR_WHITE : dgs.TXT_COLOR.TXT_COLOR_YELLOW);
					if (mName[num].pMsg != null)
					{
						mName[num].pMsg.setMessageColor(messageColor);
					}
					if (mNoStr[num].pMsg != null)
					{
						mNoStr[num].pMsg.setMessageColor(messageColor);
					}
					if (mNoNum[num].pMsg != null)
					{
						mNoNum[num].pMsg.setMessageColor(messageColor);
					}
					if (mKnock[num].pMsg != null)
					{
						mKnock[num].pMsg.setMessageColor(messageColor);
					}
					num++;
				}
			}

			public void bmRefreshMonsterList(Medget pParent, dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND msfHandle, int targetLine)
			{
				ClearAllObj();
				for (Medget medget = pParent.childNode(); medget != null; medget = medget.nextSibling())
				{
					int num = TempMonsterIdBox[targetLine];
					int num2 = SearchUseMsgNo(mNoStr);
					int indexNo = SearchUseMsgNo(mName);
					int indexNo2 = SearchUseMsgNo(mKnock);
					bmRefreshMonsterIdNo(medget, num, num2, pm, msfHandle, targetLine + 1);
					bmRefreshMonsterName(medget, num, indexNo, pm, msfHandle);
					bmRefreshMonsterDownCount(medget, num, indexNo2, pm, msfHandle);
					bmRefreshMonsterAddNewType(medget, num);
					bmRefreshMonsterBossMark(medget, num);
					medget.setWork(targetLine);
					medget.setWork2(num2);
					targetLine++;
				}
				ChangeColorTargetMsg(pParent);
			}

			public void bmRefreshMonsterDownCount(Medget pCurrent, int nameID, int indexNo, dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND msfHandle)
			{
				pCurrent = pCurrent.childNode().nextSibling().nextSibling();
				if (spl.MonsterBook.isMobItemEnable(nameID) != 0)
				{
					string after = "";
					int num = mon.MonsterManager.instance().monsterManiaManager().monsterManiaForMonsterID(nameID)
						.deadCount()
						.get();
					if (0 > num)
					{
						num = 0;
					}
					else if (255 < num)
					{
						num = 255;
					}
					dgs.msg.CMessageSys.getInstance().changeValueFont(num, out after);
					mKnock[indexNo].bEnable = true;
					mKnock[indexNo].pMsg = pm.createMessage(after, (int)msfHandle);
				}
				else
				{
					mKnock[indexNo].bEnable = true;
					mKnock[indexNo].pMsg = pm.createMessage(140u, dgs.INVALID_MSDHANDLE, (int)msfHandle);
				}
				if (mKnock[indexNo].pMsg != null)
				{
					ds.Vector2<short> vector = new ds.Vector2<short>(0, 0);
					mKnock[indexNo].pMsg.getTextSize(vector);
					mKnock[indexNo].pMsg.setPosition((short)(pCurrent.x() - vector.vx), (short)(pCurrent.y() + (pCurrent.height() - 12) / 2), erase: true);
					mKnock[indexNo].pMsg.setDisplaySpeed(byte.MaxValue);
					mKnock[indexNo].pMsg.setDisplayWait(0);
				}
			}

			public void bmRefreshMonsterAddNewType(Medget pCurrent, int indexNo, dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND _msfHandle)
			{
			}

			public void bmRefreshMonsterAddNewType(Medget pCurrent, int mobID)
			{
				pCurrent = pCurrent.childNode().nextSibling().nextSibling()
					.nextSibling();
				if (spl.MonsterBook.isMobItemEnable(mobID) == 0)
				{
					MenuManager.getSingleton().MedgetsSuspend(pCurrent);
				}
				else if (spl.SCManager.getSingleton().GetMonsterBook().isMobEntryFinish(mobID))
				{
					MenuManager.getSingleton().MedgetsSuspend(pCurrent);
				}
				else
				{
					MenuManager.getSingleton().MedgetsResume(pCurrent);
				}
			}

			public void bmRefreshMonsterName(Medget pCurrent, int nameID, int indexNo, dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND _msfHandle)
			{
				pCurrent = pCurrent.childNode().nextSibling();
				if (spl.MonsterBook.isMobItemEnable(nameID) != 0)
				{
					mName[indexNo].bEnable = true;
					mName[indexNo].pMsg = pm.createMessage((uint)(nameID + 1000), spl.SCManager.getSingleton().GetSpecialBattleMsdNo(), (int)_msfHandle);
				}
				else
				{
					mName[indexNo].bEnable = true;
					mName[indexNo].pMsg = pm.createMessage(142u, dgs.INVALID_MSDHANDLE, (int)_msfHandle);
				}
				if (mName[indexNo].pMsg != null)
				{
					mName[indexNo].pMsg.setPosition(pCurrent.x(), (short)(pCurrent.y() + (pCurrent.height() - 12) / 2), erase: true);
					mName[indexNo].pMsg.setDisplaySpeed(byte.MaxValue);
					mName[indexNo].pMsg.setDisplayWait(0);
				}
			}

			public void bmRefreshMonsterIdNo(Medget pCurrent, int nameID, int indexNo, dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND _msfHandle, int No)
			{
				pCurrent = pCurrent.childNode();
				dgs.DGSMessage dGSMessage = pm.createMessage(60102u, dgs.INVALID_MSDHANDLE, (int)_msfHandle);
				string arg = "";
				sprintf(out arg, "%s%3d", dGSMessage.getString(), No);
				mNoStr[indexNo].bEnable = true;
				mNoStr[indexNo].pMsg = pm.createMessage(arg, (int)_msfHandle);
				if (mNoStr[indexNo].pMsg != null)
				{
					mNoStr[indexNo].pMsg.setPosition(pCurrent.x(), (short)(pCurrent.y() + (pCurrent.height() - 12) / 2), erase: true);
					mNoStr[indexNo].pMsg.setDisplaySpeed(byte.MaxValue);
					mNoStr[indexNo].pMsg.setDisplayWait(0);
				}
				dGSMessage.release();
			}

			internal static bool bossCheck(int nMobID)
			{
				bool result = false;
				if (220 == nMobID)
				{
					result = false;
				}
				else if (191 == nMobID)
				{
					result = true;
				}
				else if (255 >= nMobID && 196 <= nMobID)
				{
					result = true;
				}
				return result;
			}

			public void bmRefreshMonsterBossMark(Medget pCurrent, int mobID)
			{
				pCurrent = pCurrent.childNode().nextSibling().nextSibling()
					.nextSibling()
					.nextSibling();
				if (spl.MonsterBook.isMobItemEnable(mobID) == 0)
				{
					MenuManager.getSingleton().MedgetsSuspend(pCurrent);
				}
				else if (!bossCheck(mobID))
				{
					MenuManager.getSingleton().MedgetsSuspend(pCurrent);
				}
				else
				{
					MenuManager.getSingleton().MedgetsResume(pCurrent);
				}
			}

			public void CreateExclusiveScrollBar(Medget M)
			{
				XbnNode firstNodeByTagNameFromChildren = M.node().getFirstNodeByTagNameFromChildren(TRANSCODE("behavior"));
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
				if (num != 0)
				{
					int num2 = 0;
					if (xbnNodeList.size() > 1)
					{
						num2 = xbnNodeList[1].nodeValueInt();
					}
					int num3 = 0;
					if (xbnNodeList.size() > 2)
					{
						num3 = xbnNodeList[2].nodeValueInt();
					}
					int num4 = 0;
					if (xbnNodeList.size() > 3)
					{
						num4 = xbnNodeList[3].nodeValueInt();
					}
					int num5 = 0;
					if (xbnNodeList.size() > 4)
					{
						num5 = xbnNodeList[4].nodeValueInt();
					}
					sbPageLine = num5;
					int num6 = 0;
					if (xbnNodeList.size() > 5)
					{
						num6 = xbnNodeList[5].nodeValueInt();
					}
					sb.sbCreate();
					sb.sbSetPosition((short)num2, (short)num3);
					sb.sbSetHeight((short)num4);
					sb.sbSetCapacity((short)num5, (short)num6);
					sb.sbRestrainCheck();
					sb.sbSetHandler(this);
					sbLine ^= sbLine;
					sbFlag = true;
				}
			}

			public void ClearAllObj()
			{
				ReleaseMlistMsgTypeArray(mName);
				ReleaseMlistMsgTypeArray(mNoStr);
				ReleaseMlistMsgTypeArray(mNoNum);
				ReleaseMlistMsgTypeArray(mKnock);
			}

			public int SearchUseMsgNo(MLIST_MSG_TYPE[] pArray)
			{
				for (int i = 0; i < MonsterListMsgMax; i++)
				{
					if (!pArray[i].bEnable)
					{
						pArray[i].bEnable = true;
						if (pArray[i].pMsg != null)
						{
							pArray[i].pMsg.release();
						}
						return i;
					}
				}
				return -1;
			}

			public bool CheckScroll(Medget pM)
			{
				if (MenuManager.getSingleton().GetScrollType() != MenuManager.SCROLL_TYPE.TYPE_WAIT)
				{
					if (MenuManager.getSingleton().GetScrollType() == MenuManager.SCROLL_TYPE.TYPE_DOWN)
					{
						if ((int)MenuManager.getSingleton().root().getNodeByID("five")
							.work() < MONSTER_MAX - 1)
						{
							return true;
						}
					}
					else if (MenuManager.getSingleton().GetScrollType() == MenuManager.SCROLL_TYPE.TYPE_UP && (int)MenuManager.getSingleton().root().getNodeByID("one")
						.work() > 0)
					{
						return true;
					}
				}
				return false;
			}

			public bool ScrolledLead(Medget pM)
			{
				if (pM != null && (int)pM.getNodeByID("one").work() == 0)
				{
					return true;
				}
				return false;
			}

			public bool ScrolledEnd(Medget pM)
			{
				if (pM != null && (int)pM.getNodeByID("five").work() >= MONSTER_MAX)
				{
					return true;
				}
				return false;
			}

			public ScrollBar getScrollBar()
			{
				return sb;
			}

			public void setCursor(Medget M, int index)
			{
				int num = MATH_MIN(index, 226 - sbPageLine);
				sb.sbSetHandler(null);
				sb.sbSetLine((short)num);
				bmRefreshMonsterList(M, dgs.msg.CMessageSys.getInstance().Sub(), dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, num);
				sb.sbSetHandler(this);
				Medget medget = M.childNode();
				for (int i = 0; i < index - num; i++)
				{
					medget = medget.nextSibling();
				}
				MenuManager.getSingleton().setFocuseMedget(medget);
			}

			public new static int classIdentifier()
			{
				return MBMonsterList_UN.number();
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

			public void setRefresh(bool b)
			{
				refresh_ = b;
			}

			public bool checkRefresh()
			{
				return refresh_;
			}
		}
	}
}
