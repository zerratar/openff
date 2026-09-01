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
	public static partial class pl
	{
		public class CPlayerHumanLeaveWait : CPlayerHumanAction
		{
			public override void start()
			{
				if (Player().getMotionIndex() != 1002)
				{
					Player().startMotion(1002, _Loop: false, 5u);
				}
			}

			public override void update()
			{
				if (Player().isEndOfMotion() && Player().getMotionIndex() != 1003)
				{
					Player().startMotion(1003, _Loop: true, 5u);
				}
				if (!Player().isOperater() || !Player().InputPermission() || touchPanelAction())
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
		}
	}
}
