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
	public static partial class menu
	{
		public class MenuBehaviorFactory : dgs.DGSLinkedList<MenuBehaviorFactory>
		{
			private static int LENGTH_OF_NAME = 64;

			private string behaviorName = "";

			public static MenuBehavior createMenuBehavior(string behavior_name)
			{
				for (MenuBehaviorFactory menuBehaviorFactory = (MenuBehaviorFactory)dgs.DGSLinkedList<MenuBehaviorFactory>.dgsllBase(); menuBehaviorFactory != null; menuBehaviorFactory = (MenuBehaviorFactory)menuBehaviorFactory.dgsllNext())
				{
					if (strcmp(behavior_name, menuBehaviorFactory.behaviorName) == 0)
					{
						return menuBehaviorFactory.mbfCreate();
					}
				}
				return null;
			}

			public MenuBehaviorFactory(string behavior_name)
			{
				strncpy(out behaviorName, behavior_name, LENGTH_OF_NAME);
				if (behaviorName.Length > 0)
				{
					dgsllLink();
				}
			}

			~MenuBehaviorFactory()
			{
				if (behaviorName.Length > 0)
				{
					dgsllUnlink();
				}
			}

			public virtual MenuBehavior mbfCreate()
			{
				return null;
			}
		}
	}
}
