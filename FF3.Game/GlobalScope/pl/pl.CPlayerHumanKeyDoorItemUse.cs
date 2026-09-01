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
		public class CPlayerHumanKeyDoorItemUse : CPlayerHumanAction, menu.MenuBehavedNotifier
		{
			public int Phase_;

			public bool decide_;

			public sbyte counter_;

			public override void start()
			{
				Player().TurnSys().setStop(b: true);
				menu.MenuManager.getSingleton().SetUsingMenuType(1);
				menu.MenuManager.getSingleton().setOpenDoorFlag(flag: true);
				Player().InputPermission_set(arg0: false);
				CCastCommandTransit.getInstance().cast_Field2D().ItemUseMenuManager()
					.create();
				setTouchIconVisibility(Visibility: false);
				Phase_ = 4;
				decide_ = false;
				counter_ = 0;
			}

			public override void update()
			{
				sbyte b = 5;
				if (counter_ > 0)
				{
					counter_--;
				}
				switch (Phase_)
				{
				case 0:
				{
					if ((ds.g_Pad.edge() & 1) != 0 || decide_)
					{
						menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("item_use_window"))?.behavior().mbSetNotifier(null);
						CCastCommandTransit.getInstance().cast_Field2D().ItemUseMenuManager()
							.erase();
						Phase_ = 1;
						break;
					}
					int x = 0;
					int y = 0;
					ds.g_TouchPanel.getPoint(out x, out y);
					if ((ds.g_Pad.edge() & 2) != 0 || (ds.g_TouchPanel.isTap() && menu.MenuManager.getSingleton().TouchWindowOutArea(x, y)))
					{
						menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("item_use_window"))?.behavior().mbSetNotifier(null);
						CCastCommandTransit.getInstance().cast_Field2D().ItemUseMenuManager()
							.erase();
						counter_ = b;
						Phase_ = 5;
					}
					break;
				}
				case 1:
					if (5013 == menu.MenuManager.getSingleton().GetTargetItemNo())
					{
						cancelKeyDoor();
						PlayerParty.instance().addItem(5013, -1);
						Phase_ = 2;
					}
					else
					{
						counter_ = b;
						Phase_ = 5;
					}
					break;
				case 2:
					if (!CCastCommandTransit.getInstance().cast_Field2D().ItemUseMenuManager()
						.isItemMenu())
					{
						wld.WorldPart.getInstance().getWorldSystem().World2DMng()
							.MessageWindow()
							.createWindow(-1);
						wld.WorldPart.getInstance().getWorldSystem().World2DMng()
							.MessageWindow()
							.setMessageColor(9);
						wld.WorldPart.getInstance().getWorldSystem().World2DMng()
							.MessageWindow()
							.createMessage(1000152, -1, 0);
						Phase_ = 3;
					}
					break;
				case 3:
					if (wld.WorldPart.getInstance().getWorldSystem().World2DMng()
						.MessageWindow()
						.isFinished() && wld.WorldPart.getInstance().getWorldSystem().World2DMng()
						.MessageWindow()
						.isNextPageButton())
					{
						Player().InputPermission_set(arg0: true);
						Player().getColFlag_or(4096);
						Player().setNextAct(0);
					}
					break;
				case 4:
					if (CCastCommandTransit.getInstance().cast_Field2D().ItemUseMenuManager()
						.isItemMenu())
					{
						menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("item_use_window"))?.behavior().mbSetNotifier(this);
						Phase_ = 0;
					}
					break;
				case 5:
					if (counter_ <= 0)
					{
						Player().InputPermission_set(arg0: true);
						Player().getColFlag_or(4096);
						Player().setNextAct(0);
						setTouchIconVisibility(Visibility: true);
					}
					break;
				}
			}

			public override void end()
			{
				wld.WorldPart.getInstance().getWorldSystem().World2DMng()
					.MessageWindow()
					.release();
				Player().getColFlag_or(4096);
				Player().TurnSys().setStop(b: false);
				Player().MoveSys().setStop(b: true);
				menu.MenuManager.getSingleton().setOpenDoorFlag(flag: false);
			}

			public bool mbnNotify(menu.MenuBehavior notifier, uint number, uint param)
			{
				if (number == 0)
				{
					decide_ = true;
					menu.MenuManager.getSingleton().playSEDecide();
				}
				return false;
			}
		}
	}
}
