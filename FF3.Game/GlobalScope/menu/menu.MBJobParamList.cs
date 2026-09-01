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
		public class MBJobParamList : MenuBehavior
		{
			public const int MESSAGE_NUM = 23;

			public const int ITEM_NUM = 22;

			public static dgs.UniqueNumber MBJobParamList_UN = new dgs.UniqueNumber();

			~MBJobParamList()
			{
			}

			public override void bmInitialize(Medget M)
			{
				reset();
				updataJobList();
				createItemMessage();
				MenuManager.getSingleton().initFocus(0);
			}

			public override void bmBehave(Medget M)
			{
				changeJobColor();
			}

			public override void bmFinalize(Medget M)
			{
				releaseItemMessage();
			}

			public void wmsRefresh()
			{
				releaseItemMessage();
				reset();
				updataJobList();
				createItemMessage();
			}

			public override void bmSuspend(Medget M)
			{
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					dgs.DGSMessage dGSMessage = null;
					dGSMessage = (dgs.DGSMessage)GET_MSG_JOB(medget);
					if (dGSMessage != null)
					{
						SET_ACTIVITY(medget, dGSMessage.activity() ? 1 : 0);
						dGSMessage.setActivity(b: false);
					}
					dGSMessage = (dgs.DGSMessage)GET_MSG_SKILL(medget);
					if (dGSMessage != null)
					{
						SET_ACTIVITY(medget, dGSMessage.activity() ? 1 : 0);
						dGSMessage.setActivity(b: false);
					}
				}
			}

			public override void bmResume(Medget M)
			{
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					dgs.DGSMessage dGSMessage = null;
					((dgs.DGSMessage)GET_MSG_JOB(medget))?.setActivity(GET_ACTIVITY(medget) != 0);
					((dgs.DGSMessage)GET_MSG_SKILL(medget))?.setActivity(GET_ACTIVITY(medget) != 0);
				}
				wmsRefresh();
			}

			public override bool bmDecide(Medget M)
			{
				int num = GET_JOB_NUMBER(M);
				OS_Printf("job_num %d\n", num);
				if (num < 0 || 23 <= num)
				{
					MenuManager.getSingleton().playSEBeep();
					return true;
				}
				MenuManager.getSingleton().SetTargetItemNo(num);
				for (Medget medget = M.childNode(); medget != null; medget = medget.nextSibling())
				{
					MenuManager.getSingleton().leaveFocusList(medget);
				}
				MenuManager.getSingleton().SetDecideButtonState(0);
				return true;
			}

			public override bool bmCancel(Medget M)
			{
				bool result = false;
				MenuManager.getSingleton().SetCancelButtonState(0);
				return result;
			}

			public override bool bmDirection(Medget M, int key)
			{
				return MenuManager.getSingleton().MoveCursor(M, PlaySe: true);
			}

			public override void bmActivate(Medget M)
			{
				if (M == ownerMedget)
				{
					MenuManager.getSingleton().SetTargetItemNo(0);
				}
			}

			public override void bmDeactivate(Medget M)
			{
			}

			public void reset()
			{
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					SET_JOB_NUMBER(medget, -1);
					SET_MSG_JOB(medget, null);
					SET_MSG_SKILL(medget, null);
				}
			}

			public void createItemMessage()
			{
				string after = "";
				int num = 0;
				dgs.msg.CMessageMng.MSF_HANDLE_KIND font = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
				dgs.DGSMessageManager dGSMessageManager = null;
				int targetCharNo = MenuManager.getSingleton().GetTargetCharNo();
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (GET_JOB_NUMBER(medget) >= 0)
					{
						dGSMessageManager = ((medget.display() != 0) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
						dgs.DGSMessage dGSMessage = dGSMessageManager.createMessage((uint)(50105 + GET_JOB_NUMBER(medget)), MenuManager.getSingleton().GetMenuDataTextNo(), (int)font);
						if (dGSMessage != null)
						{
							dGSMessage.setPosition((short)(medget.x() + 4), (short)(medget.y() + (medget.height() - 12) / 2), erase: true);
							dGSMessage.setDisplaySpeed(byte.MaxValue);
							dGSMessage.setDisplayWait(0);
							SET_MSG_JOB(medget, dGSMessage);
							num = pl.PlayerParty.instance().player((byte)targetCharNo).jobManager()
								.job(static_cast<pl.JOB_TYPE>(GET_JOB_NUMBER(medget)))
								.skill()
								.skillLevel()
								.get();
							dgs.msg.CMessageSys.getInstance().changeValueFont(num, out after);
							dGSMessage = dGSMessageManager.createMessage(after, (int)font);
							if (dGSMessage != null)
							{
								dGSMessage.setPosition((short)(medget.x() + medget.width() - 4), (short)(medget.y() + (medget.height() - 12) / 2), erase: true);
								dGSMessage.setDisplaySpeed(byte.MaxValue);
								dGSMessage.setDisplayWait(0);
								dGSMessage.setStyle(dGSMessage.getStyle() | 0x20);
								SET_MSG_SKILL(medget, dGSMessage);
								MBText mBText = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
								mBText.bmTextVisibility(v: true);
								mBText.bmSetPriority(3);
								MenuManager.getSingleton().joinFocusList(medget);
							}
						}
					}
				}
			}

			public void releaseItemMessage()
			{
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					dgs.DGSMessage dGSMessage = null;
					((dgs.DGSMessage)GET_MSG_JOB(medget))?.release();
					SET_MSG_JOB(medget, null);
					((dgs.DGSMessage)GET_MSG_SKILL(medget))?.release();
					SET_MSG_SKILL(medget, null);
					if (medget.behavior() != null)
					{
						MBText mBText = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
						mBText.bmTextVisibility(v: false);
					}
				}
			}

			public void updataJobList()
			{
				evt.CEventManager.getInstance().FlagMng().set(0u, (uint)evt.EVENT_JOB_FLAG[(int)job_menu_itemu_priority[0]]);
				int num = 0;
				int num2 = 0;
				Medget medget = ownerMedget.childNode();
				while (medget != null && num < 23)
				{
					while ((job_menu_itemu_priority[num] == (pl.JOB_TYPE)pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).jobManager()
						.nowJob() || evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_JOB_FLAG[(int)job_menu_itemu_priority[num]]) == 0) && ++num < 23)
					{
					}
					if (23 > num)
					{
						SET_JOB_NUMBER(medget, static_cast<int>(job_menu_itemu_priority[num]));
						num2++;
					}
					num++;
					medget = medget.nextSibling();
				}
				if (num2 == 0)
				{
					MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
				}
				else
				{
					MenuManager.getSingleton().SetTargetItemNo(0);
				}
			}

			public void changeJobColor()
			{
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					MBText mBText = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
					int num = (mBText.bmIsPushed() ? 1 : 0);
					int num2 = medget.y() + (medget.height() - 12) / 2 + num;
					dgs.DGSMessage dGSMessage = null;
					dGSMessage = (dgs.DGSMessage)GET_MSG_JOB(medget);
					if (dGSMessage != null)
					{
						dGSMessage.position(out var _, out var y);
						if (y != num2)
						{
							dGSMessage.setPosition((short)(medget.x() + 4 + num), (short)num2, erase: true);
							((dgs.DGSMessage)GET_MSG_SKILL(medget))?.setPosition((short)(medget.x() + medget.width() - 4 + num), (short)num2, erase: true);
						}
					}
				}
			}

			public new static int classIdentifier()
			{
				return MBJobParamList_UN.number();
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
