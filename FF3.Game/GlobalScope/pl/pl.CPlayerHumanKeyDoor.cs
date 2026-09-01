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
		public class CPlayerHumanKeyDoor : CPlayerHumanAction
		{
			private bool flag_;

			public override void start()
			{
				Player().startMotion(1001, _Loop: true, 5u);
				Player().TurnSys().setStop(b: true);
				int num = 0;
				if (checkThief())
				{
					cancelKeyDoor();
					num = 1000151;
					flag_ = true;
				}
				else
				{
					num = 1000150;
					flag_ = false;
				}
				wld.WorldPart.getInstance().getWorldSystem().World2DMng()
					.MessageWindow()
					.setMessageColor(9);
				wld.WorldPart.getInstance().getWorldSystem().World2DMng()
					.MessageWindow()
					.createMessageWindow(0, num, -1);
				wld.WorldPart.getInstance().getWorldSystem().World2DMng()
					.setButtonShow(show: false);
			}

			public override void update()
			{
				if (wld.WorldPart.getInstance().getWorldSystem().World2DMng()
					.MessageWindow()
					.isFinished() && wld.WorldPart.getInstance().getWorldSystem().World2DMng()
					.MessageWindow()
					.isNextPageButton())
				{
					if (flag_)
					{
						Player().setNextAct(0);
					}
					else if (!flag_)
					{
						Player().setNextAct(12);
					}
				}
			}

			public override void end()
			{
				Player().TurnSys().setStop(b: false);
				wld.WorldPart.getInstance().getWorldSystem().World2DMng()
					.MessageWindow()
					.release();
			}
		}
	}
}
