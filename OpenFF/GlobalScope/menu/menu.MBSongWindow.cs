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
		public class MBSongWindow : MenuBehavior
		{
			public const int MESSAGE_SONG_MAX = 5;

			public static dgs.UniqueNumber MBSongWindow_UN = new dgs.UniqueNumber();

			private bool savedVisibility;

			private bool[] bEnable = new bool[5];

			private dgs.DGSMessage[] pMsg = new dgs.DGSMessage[5];

			public MBSongWindow()
			{
				for (int i = 0; i < 5; i++)
				{
					bEnable[i] = false;
					pMsg[i] = null;
				}
			}

			~MBSongWindow()
			{
				clearAllMessage();
			}

			public override void bmInitialize(Medget M)
			{
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((M.display() != 0) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				dgs.msg.CMessageMng.MSF_HANDLE_KIND msfHandle = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
				createSongMessage(dGSMessageManager, msfHandle, M);
				if (opt.COptionManager.getSingleton().cursorOption().cursorPosition() == opt.CURSOR_POSITION.CURSOR_POSITION_MEMORY && MenuManager.getSingleton().battleMode())
				{
					MenuManager.getSingleton().SetTrialInitFocuseFlag(val: false);
					MenuManager.getSingleton().initFocus(MenuManager.getSingleton().saveSongTarget(pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).playerId()));
				}
			}

			public override void bmBehave(Medget M)
			{
				Medget focuseMedget = MenuManager.getSingleton().getFocuseMedget();
				if (focuseMedget != null)
				{
					MenuManager.getSingleton().saveSongTarget_set(pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).playerId(), MenuManager.getSingleton().getFocuseMedget().myTag());
					int num = UseSongList[focuseMedget.myTag()];
					if (num < 0)
					{
						MenuManager.getSingleton().SetTargetItemNo(-1);
					}
					else
					{
						MenuManager.getSingleton().SetTargetItemNo(num);
					}
				}
				clearColorSongName(M);
				if (focuseMedget != null && GET_MSG_IDX(focuseMedget) != -1 && pMsg[GET_MSG_IDX(focuseMedget)] != null)
				{
					pMsg[GET_MSG_IDX(focuseMedget)].setMessageColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
				}
			}

			public override void bmFinalize(Medget M)
			{
				clearAllMessage();
			}

			public override bool bmDecide(Medget M)
			{
				MenuManager.getSingleton().SetDecideButtonState(0);
				return false;
			}

			public override bool bmCancel(Medget M)
			{
				MenuManager.getSingleton().SetCancelButtonState(0);
				return false;
			}

			public override bool bmDirection(Medget M, int dir)
			{
				return MenuManager.getSingleton().MoveCursor(M, PlaySe: true);
			}

			public override void bmActivate(Medget M)
			{
				MenuManager.getSingleton().SetActivateButtonState(0);
			}

			public override void bmSuspend(Medget M)
			{
				for (int i = 0; i < 5; i++)
				{
					if (bEnable[i] && pMsg[i] != null)
					{
						savedVisibility = pMsg[i].activity();
						pMsg[i].setActivity(b: false);
					}
				}
			}

			public override void bmResume(Medget M)
			{
				for (int i = 0; i < 5; i++)
				{
					if (bEnable[i] && pMsg[i] != null)
					{
						pMsg[i].setActivity(savedVisibility);
					}
				}
			}

			public void createSongMessage(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND msfHandle, Medget pParent)
			{
				for (Medget medget = pParent.childNode(); medget != null; medget = medget.nextSibling())
				{
					MenuManager.getSingleton().joinFocusList(medget);
					int num = UseSongList[medget.myTag()];
					if (num <= 0)
					{
						SET_MSG_IDX(medget, -1);
					}
					else
					{
						int nameId = itm.ItemManager.instance().itemParameter((short)num).nameId();
						createSongNameString(pm, msfHandle, medget.x(), medget.y(), nameId, medget);
					}
				}
			}

			public void createSongNameString(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND msfHandle, int x, int y, int nameId, Medget pTarget)
			{
				int num = searchUseEnableIndexNo();
				if (num >= -1)
				{
					pMsg[num] = pm.createMessage((uint)nameId, MenuManager.getSingleton().GetItemDataTextNo(), (int)msfHandle);
					if (pMsg[num] != null)
					{
						pMsg[num].setPosition((short)x, (short)y, erase: true);
						pMsg[num].setDisplaySpeed(byte.MaxValue);
						pMsg[num].setDisplayWait(0);
						SET_MSG_IDX(pTarget, num);
					}
				}
			}

			public int searchUseEnableIndexNo()
			{
				for (int i = 0; i < 5; i++)
				{
					if (!bEnable[i])
					{
						bEnable[i] = true;
						return i;
					}
				}
				return -1;
			}

			public void clearColorSongName(Medget pMedget)
			{
				for (Medget medget = pMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (GET_MSG_IDX(medget) != -1 && pMsg[GET_MSG_IDX(medget)] != null)
					{
						pMsg[GET_MSG_IDX(medget)].setMessageColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
					}
				}
			}

			public void clearAllMessage()
			{
				for (int i = 0; i < 5; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].release();
						pMsg[i] = null;
					}
				}
			}

			public new static int classIdentifier()
			{
				return MBSongWindow_UN.number();
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
