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
		public class CPlayerHumanLeave : CPlayerHumanAction
		{
			public int change_wait;

			public override void start()
			{
				Player().setAutoPilot(_AutoPilot: true);
				Player().InputPermission_set(arg0: false);
				Player().setMCLCol(b: true);
				byte b = 4;
				Player().setSucAlpha(100);
				Player().setAutoAlphaFrame(b);
				Player().setWorkAutoAlphaFrame(0);
				Player().setSucShadowAlpha(15);
				Player().setAutoShadowAlphaFrame(b);
				Player().setWorkAutoShadowAlphaFrame(0);
				CPlayerHuman cPlayerHuman = (CPlayerHuman)Player();
				Player().setShadowType((uint)cPlayerHuman.getShadowType());
			}

			public override void update()
			{
				Player().setNextAct(0);
			}

			public override void end()
			{
				if (Player().isOperater())
				{
					Player().setTarget(null);
					CPlayerHuman cPlayerHuman = (CPlayerHuman)Player();
					CPlayerHuman cPlayerHuman2 = cPlayerHuman.getNpc();
					if (cPlayerHuman2 != null)
					{
						cPlayerHuman2.setNextAct(0);
						cPlayerHuman2.setPosition(cPlayerHuman.getPosition());
						cPlayerHuman2.setTarget(Player());
						cPlayerHuman2.setAutoPilot(_AutoPilot: false);
						cPlayerHuman2.setShadowType((uint)cPlayerHuman2.getShadowType());
					}
					Player().setAutoPilot(_AutoPilot: false);
					Player().InputPermission_set(arg0: true);
					Player().setMCLCol(b: true);
					Player().getColFlag_or(2);
					Player().getColFlag_or(4096);
					Player().getColFlag_or(16);
				}
			}
		}
	}
}
