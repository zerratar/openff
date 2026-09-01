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
	public static partial class pl
	{
		public class CPlayerHumanWait : CPlayerHumanAction
		{
			public override void start()
			{
				if (!Player().getInvalidActionMotion() && Player().getMotionIndex() != 1001)
				{
					Player().startMotion(1001, _Loop: true, 5u);
				}
				m_Counter = 0;
				Player().AutoRun_set(arg0: false);
				CPlayerHuman cPlayerHuman = (CPlayerHuman)Player();
				if (cPlayerHuman.getMenuIcon() != null)
				{
					cPlayerHuman.getMenuIcon().setStateShow();
				}
				if (cPlayerHuman.getCameraIcon() != null)
				{
					cPlayerHuman.getCameraIcon().setStateShow();
				}
				if (cPlayerHuman.getTalkIcon() != null)
				{
					cPlayerHuman.getTalkIcon().setStateShow();
				}
				if (Player().getWaitCounter() > 0)
				{
					Player().disableMoveVector();
				}
			}

			public override void update()
			{
				sbyte waitCounter = Player().getWaitCounter();
				if (waitCounter > 0)
				{
					if (--waitCounter <= 0)
					{
						waitCounter = 0;
						Player().enableMoveVector();
					}
					Player().setWaitCounter(waitCounter);
					return;
				}
				if ((Player().getPreAct() == 9 || Player().getPreAct() == 7) && Player().getTarget() != null)
				{
					if (Player().isOperater())
					{
						Player().setPosition(Player().getTarget().getPosition());
					}
					else if (Player().NPCAiManager().getAiKind() == CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW)
					{
						Player().setPosition(Player().NPCAiManager().NPC().LookPlayer()
							.getPosition());
					}
				}
				if (!Player().isOperater())
				{
					partyNPCBoard();
					return;
				}
				if (!Player().isAutoPilot())
				{
					m_Counter++;
					if (m_Counter >= 240)
					{
						Player().setNextAct(3);
					}
				}
				if (!Player().InputPermission() || touchPanelAction())
				{
					return;
				}
				if (checkActionTrigger())
				{
					checkAction();
				}
				else if (canWorldTalk(Player()))
				{
					gotoWorldTalk(Player());
				}
				else if (isWalk())
				{
					Player().setNextAct(1);
				}
				else if (isRun())
				{
					Player().setNextAct((Player().getInPutMode() == INPUT_MODE.INPUT_MODE_FIELD) ? 1 : 2);
					if (Player().getPlayerMoveType() == PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_FROG)
					{
						Player().setNextAct(1);
					}
				}
			}

			public override void end()
			{
				m_Counter = 0;
				if (3 != Player().getNextAct())
				{
					CPlayerHuman cPlayerHuman = (CPlayerHuman)Player();
					if (cPlayerHuman.getMenuIcon() != null)
					{
						cPlayerHuman.getMenuIcon().setStateHide();
					}
					if (cPlayerHuman.getCameraIcon() != null)
					{
						cPlayerHuman.getCameraIcon().setStateHide();
					}
					if (cPlayerHuman.getTalkIcon() != null)
					{
						cPlayerHuman.getTalkIcon().setStateHide();
					}
				}
				Player().setWaitCounter(0);
				Player().enableMoveVector();
			}
		}
	}
}
