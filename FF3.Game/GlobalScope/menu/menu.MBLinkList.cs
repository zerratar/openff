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
		public class MBLinkList : MenuBehavior, SBEventHandler
		{
			public static dgs.UniqueNumber MBLinkList_UN = new dgs.UniqueNumber();

			public static int LINK_ITEM_MAX = 32;

			public static int LINES_PAR_PAGE = 4;

			private ScrollBar sb = new ScrollBar();

			private bool sbFlag;

			private int sbLine;

			private int maxMonsterLine;

			private bool refresh_;

			private int item_id;

			private bool savedVisibility;

			private bool suspend_;

			private int item_count;

			private dgs.DGSMessage[,] pMsg = new dgs.DGSMessage[LINES_PAR_PAGE, 3];

			private sys2d.Cell[] icon = new sys2d.Cell[LINK_ITEM_MAX];

			public MBLinkList()
			{
				for (int i = 0; i < icon.Length; i++)
				{
					icon[i] = new sys2d.Cell();
				}
				sbFlag = false;
				sbLine ^= sbLine;
				item_id = -1;
			}

			~MBLinkList()
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
				item_count = LENGTH(item_list);
				for (int i = 0; i < item_count; i++)
				{
					item_list[i] = UserInfo.GetAchievementInfo(i);
				}
				for (int j = 0; j < item_count; j++)
				{
					if (item_list[j] != null)
					{
						icon[j].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "link_icon.NCER", null, (byte[])item_list[j].m_abyIconData, "link_icon.NCLR");
						icon[j].SetCell(0);
						icon[j].SetShow(show: false);
						icon[j].SetPriority(0);
						sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(icon[j]);
					}
				}
				ClearAllObj();
				sbFlag = false;
				sbLine ^= sbLine;
				sb.sbSetUseCenter(bUse: true);
				sb.sbCreate();
				sb.sbSetPosition((short)(M.x() + M.width() - (ScrollBar.PARTS_W >> 12) - 4), M.y());
				sb.sbSetHeight(M.height());
				sb.sbSetCapacity((short)LINES_PAR_PAGE, (short)item_count);
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
				for (int i = 0; i < item_count; i++)
				{
					icon[i].Release();
					NNS_G2dReleaseImageProxy(icon[i].GetImageProxy());
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(icon[i]);
				}
				if (sbFlag)
				{
					sb.sbDestroy();
					sb.sbSetHandler(null);
					sbFlag = false;
				}
			}

			public override void bmBehave(Medget M)
			{
				if (!suspend_ && sbFlag)
				{
					if (MenuManager.getSingleton().GetScrollType() == MenuManager.SCROLL_TYPE.TYPE_DOWN && sbLine < item_count - LINES_PAR_PAGE - 1)
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
				}
				return true;
			}

			public override void bmActivate(Medget M)
			{
			}

			public override void bmDeactivate(Medget M)
			{
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
					bmRefreshList(MenuManager.getSingleton().getFocuseMedget().parentNode(), dgs.msg.CMessageSys.getInstance().Sub(), dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, currentLine);
					MenuManager.getSingleton().playSEMoveCursor();
					MenuManager.getSingleton().SetScrollType(MenuManager.SCROLL_TYPE.TYPE_WAIT);
				}
			}

			public void bmRefreshList(Medget pParent, dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND msfHandle, int targetLine)
			{
				ClearAllObj();
				int num = 0;
				for (Medget medget = pParent.childNode(); medget != null; medget = medget.nextSibling())
				{
					int num2 = medget.x();
					int num3 = medget.y() + medget.height() / 2;
					if (item_list[targetLine] != null)
					{
						int num4 = 0;
						pMsg[num, num4] = pm.createMessage(item_list[targetLine].m_strName, 1);
						pMsg[num, num4].setPosition((short)(num2 + 64), (short)(num3 - 12 + num4 * 8), erase: true);
						pMsg[num, num4].setDisplaySpeed(byte.MaxValue);
						pMsg[num, num4].setDisplayWait(0);
						num4 = 2;
						pMsg[num, num4] = pm.createMessage(item_list[targetLine].m_strDesc, 1);
						pMsg[num, num4].setPosition((short)(num2 + 64), (short)(num3 - 12 + num4 * 8), erase: true);
						pMsg[num, num4].setDisplaySpeed(byte.MaxValue);
						pMsg[num, num4].setDisplayWait(0);
					}
					if (item_list[targetLine] != null)
					{
						icon[targetLine].SetShow(item_list[targetLine].m_bShow);
						icon[targetLine].SetPositionI(num2 + 16, num3 - 28 + 8);
					}
					targetLine++;
					num++;
				}
			}

			public void ClearAllObj()
			{
				for (int i = 0; i < LINES_PAR_PAGE; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						if (pMsg[i, j] != null)
						{
							pMsg[i, j].release();
						}
						pMsg[i, j] = null;
					}
				}
				for (int k = 0; k < item_count; k++)
				{
					icon[k].SetShow(show: false);
				}
			}

			public new static int classIdentifier()
			{
				return MBLinkList_UN.number();
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
