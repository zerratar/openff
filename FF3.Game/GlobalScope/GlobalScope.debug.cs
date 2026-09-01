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
	public static class debug
	{
		public class CMenuMain
		{
			public const int DEBUG_MAIN_MODE_ERR = -1;

			public const int DEBUG_MAIN_MODE_CAMPANY_LOGO = 0;

			public const int DEBUG_MAIN_MODE_BATTLE = 1;

			public const int DEBUG_MAIN_MODE_WORLD = 2;

			public const int DEBUG_MAIN_MODE_EVENT = 3;

			public const int DEBUG_MAIN_MODE_TITLE = 4;

			public const int DEBUG_MAIN_MODE_SPECIAL = 5;

			public const int DEBUG_MAIN_MODE_LOAD = 6;

			public const int DEBUG_MAIN_MODE_MOGNET = 7;

			public const int DEBUG_MAIN_MODE_MOVIE = 9;

			public const int DEBUG_MAIN_MODE_MAX = 15;

			private bool m_EndFlag;

			private ds.CDevice.enFPS m_TownFps;

			private int playerId_;

			private int w_playerId_;

			private int w_npcId_;

			private int[] level_ = new int[4];

			private int skill_;

			private int job_;

			private int magicType_;

			public ds.CDevice.enFPS getTownFps()
			{
				return m_TownFps;
			}
		}

		public class MenuPart : sys.FF3GamePart
		{
			private int partID_;
		}
	}
}
