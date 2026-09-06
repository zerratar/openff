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
	public static partial class mognet
	{
		public class MNSLetterBrowse : MogNetState, menu.MenuBehavedNotifier
		{
			protected int state_;

			public override void mnsInitialize(MNSMediator M)
			{
				M.changeMainBGScr(MAIN_BG_SCR.MBS_DISPLAY);
				M.changeSubBGScr(SUB_BG_SCR.SBS_MAILLIST);
				MBMogNetLetterBrowse.setMediator(M);
				menu.MenuManager.getSingleton().buildMenu("maillist");
				menu.MenuManager.getSingleton().ClearBehaviorButton();
				menu.MenuManager.getSingleton().initFocus(0);
				if (menu.MenuManager.getSingleton().root().childNode()
					.behavior() != null)
				{
					menu.MenuManager.getSingleton().root().childNode()
						.behavior()
						.mbSetNotifier(this);
				}
				M.mnsmCommonInterface(b: true, bLR: false);
				state_ = 0;
			}

			public override bool mnsProcess(MNSMediator M)
			{
				switch (state_)
				{
				case 1:
					M.shiftState(M.MNSSelectPerson_);
					break;
				}
				return true;
			}

			public override void mnsTerminate(MNSMediator M)
			{
				menu.MenuManager.getSingleton().release();
			}

			public bool mbnNotify(menu.MenuBehavior notifier, uint number, uint param)
			{
				if (number == 2)
				{
					state_ = 1;
					return true;
				}
				return false;
			}

			public static bool checkMail(int person, bool bNew)
			{
				for (int i = 0; i < NUMBER_OF_RECEIVE_NPC_MAIL; i++)
				{
					if (g_NpcMailEntry[i].name_ != g_NpcName[person])
					{
						continue;
					}
					if (bNew)
					{
						if (MNNPCMailData.getSingleton().getNPCMailState(i) == NPCMailState.NPC_MAIL_NOT_READ)
						{
							return true;
						}
					}
					else if (MNNPCMailData.getSingleton().getNPCMailState(i) != NPCMailState.NPC_MAIL_NOT_ARRIVED)
					{
						return true;
					}
				}
				return false;
			}
		}
	}
}
