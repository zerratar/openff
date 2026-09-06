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
	public static partial class menu
	{
		public class MBTipsList : MenuBehavior, SBEventHandler
		{
			public static dgs.UniqueNumber MBTipsList_UN = new dgs.UniqueNumber();

			public static int TIPS_ITEM_MAX = 6;

			public static int LINES_PAR_PAGE = 4;

			private ScrollBar sb = new ScrollBar();

			private bool sbFlag;

			private int sbLine;

			private bool refresh_;

			private int item_id;

			private bool savedVisibility;

			private bool suspend_;

			private dgs.DGSMessage[] pMsg = new dgs.DGSMessage[LINES_PAR_PAGE];

			public MBTipsList()
			{
				sbFlag = false;
				sbLine ^= sbLine;
				item_id = -1;
			}

			~MBTipsList()
			{
				ClearAllObj();
				if (sbFlag)
				{
					sb.sbDestroy();
					sb.sbSetHandler(null);
					sbFlag = false;
				}
				suspend_ = false;
			}

			public override void bmInitialize(Medget M)
			{
				ClearAllObj();
				sbFlag = false;
				sbLine ^= sbLine;
				sb.sbCreate();
				sb.sbSetPosition((short)(M.x() + M.width() - (ScrollBar.PARTS_W >> 12) - 4), M.y());
				sb.sbSetHeight(M.height());
				sb.sbSetCapacity((short)LINES_PAR_PAGE, (short)TIPS_ITEM_MAX);
				sb.sbRestrainCheck();
				sb.sbSetHandler(this);
				sbLine ^= sbLine;
				sbFlag = true;
			}

			public override void bmPostInitialize(Medget M)
			{
				bmRefreshList(M, dgs.msg.CMessageSys.getInstance().Sub(), dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, 0);
			}

			public override void bmFinalize(Medget M)
			{
				ClearAllObj();
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
				if (sbFlag)
				{
					if (MenuManager.getSingleton().GetScrollType() == MenuManager.SCROLL_TYPE.TYPE_DOWN && sbLine < TIPS_ITEM_MAX - LINES_PAR_PAGE - 1)
					{
						sbLine++;
						bmRefreshList(M, dgs.msg.CMessageSys.getInstance().Sub(), dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, sbLine);
					}
					else if (MenuManager.getSingleton().GetScrollType() == MenuManager.SCROLL_TYPE.TYPE_UP && sbLine > 0)
					{
						sbLine--;
						bmRefreshList(M, dgs.msg.CMessageSys.getInstance().Sub(), dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, sbLine);
					}
					sb.sbSetLine((short)sbLine);
				}
				MenuManager.getSingleton().SetTargetItemNo((int)MenuManager.getSingleton().getFocuseMedget().work());
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
				for (int i = 0; i < LINES_PAR_PAGE; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].setVisibility(b: false);
					}
				}
				sb.sbRestrainCheck((ScrollBar.AREA_FLAG)3);
			}

			public override void bmResume(Medget M)
			{
				suspend_ = false;
				for (int i = 0; i < LINES_PAR_PAGE; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].setVisibility(b: true);
					}
				}
				sbFlag = false;
				int targetItemNo = MenuManager.getSingleton().GetTargetItemNo();
				if (targetItemNo < sbLine)
				{
					sbLine = targetItemNo;
				}
				if (targetItemNo >= sbLine + LINES_PAR_PAGE)
				{
					sbLine = targetItemNo - LINES_PAR_PAGE + 1;
				}
				sb.sbSetLine((short)sbLine);
				MenuManager.getSingleton().setFocuseMedget(targetItemNo - sbLine);
				bmRefreshList(ownerMedget, dgs.msg.CMessageSys.getInstance().Sub(), dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, sbLine);
				sbFlag = true;
				sb.sbRestrainCheck();
				MenuManager.getSingleton().inputPermission(b: true);
			}

			public override void mbPause()
			{
				base.mbPause();
			}

			public override void mbRestart()
			{
				sb.sbRestrainCheck();
				base.mbRestart();
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
					bmRefreshList(MenuManager.getSingleton().getFocuseMedget().parentNode(), dgs.msg.CMessageSys.getInstance().Sub(), dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, currentLine);
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
					if (pMsg[num] != null)
					{
						pMsg[num].setMessageColor(messageColor);
					}
					num++;
				}
			}

			public void bmRefreshList(Medget pParent, dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND msfHandle, int targetLine)
			{
				ClearAllObj();
				int num = 0;
				for (Medget medget = pParent.childNode(); medget != null; medget = medget.nextSibling())
				{
					int num2 = medget.x() + medget.width() / 2;
					int num3 = medget.y() + medget.height() / 2;
					pMsg[num] = pm.createMessage((uint)(52102 + targetLine * 2), dgs.INVALID_MSDHANDLE, 1);
					pMsg[num].setPosition((short)num2, (short)num3, erase: true);
					pMsg[num].setDisplaySpeed(byte.MaxValue);
					pMsg[num].setDisplayWait(0);
					pMsg[num].setStyle(18u);
					medget.setWork(targetLine);
					targetLine++;
					num++;
				}
				ChangeColorTargetMsg(pParent);
			}

			public void ClearAllObj()
			{
				for (int i = 0; i < LINES_PAR_PAGE; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].release();
					}
					pMsg[i] = null;
				}
			}

			public new static int classIdentifier()
			{
				return MBTipsList_UN.number();
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
		}
	}
}
