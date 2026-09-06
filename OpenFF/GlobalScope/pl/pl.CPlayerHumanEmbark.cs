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
	public static partial class pl
	{
		public class CPlayerHumanEmbark : CPlayerHumanAction
		{
			public int change_wait;

			public override void start()
			{
				Player().setMCLCol(b: false);
				Player().getColFlag_not_and(2);
				if (Player().isOperater())
				{
					Player().setAutoPilot(_AutoPilot: true);
					Player().InputPermission_set(arg0: false);
					Player().getColFlag_not_and(4096);
					CPlayerHuman cPlayerHuman = ((CPlayerHuman)Player()).getNpc();
					if (cPlayerHuman != null)
					{
						cPlayerHuman.setNextAct(9);
						cPlayerHuman.setAutoRun(_AutoRun: true);
					}
				}
				else
				{
					CPlayerHumanAction.m_WaitNPCFlag = true;
					Player().setAutoRun(_AutoRun: true);
				}
				byte b = 4;
				Player().setSucAlpha(0);
				Player().setAutoAlphaFrame(b);
				Player().setWorkAutoAlphaFrame(b);
				Player().setSucShadowAlpha(0);
				Player().setAutoShadowAlphaFrame(b);
				Player().setWorkAutoShadowAlphaFrame(b);
				Player().setShadowType(2u);
			}

			public override void update()
			{
				if (!Player().isOperater())
				{
					if (Player().getTransparencyRate() <= 0)
					{
						CPlayerHumanAction.m_WaitNPCFlag = false;
					}
				}
				else if (!CPlayerHumanAction.m_WaitNPCFlag)
				{
					Player().setNextAct(0);
				}
			}

			public override void end()
			{
				if (Player().isOperater())
				{
					int canoeId = CCastCommandTransit.getInstance().cast_PlayerMng().getCanoeId();
					CPlayerVehicle cPlayerVehicle = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerVehicle(canoeId);
					cPlayerVehicle.getParamObj_set(Player().getParamObj());
					cPlayerVehicle.setBoardSetting(Player());
				}
			}
		}
	}
}
