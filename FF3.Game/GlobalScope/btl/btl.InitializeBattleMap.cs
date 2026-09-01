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
	public static partial class btl
	{
		public class InitializeBattleMap
		{
			private int battleMapId_;

			public void initialize()
			{
				setBattleMapId(INITIALIZE_BATTLE_MAP_ID);
			}

			public void setBattleMapId(int _id)
			{
				battleMapId_ = _id;
			}

			public int battleMapId()
			{
				return battleMapId_;
			}
		}
	}
}
