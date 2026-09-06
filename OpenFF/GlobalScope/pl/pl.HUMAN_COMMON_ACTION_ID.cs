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
		public enum HUMAN_COMMON_ACTION_ID
		{
			HUMAN_COMMON_ACTION_ID_WAIT = 1001,
			HUMAN_COMMON_ACTION_ID_LEAVE_WAIT = 1002,
			HUMAN_COMMON_ACTION_ID_LEAVE_WAIT_LOOP = 1003,
			HUMAN_COMMON_ACTION_ID_WALK = 1004,
			HUMAN_COMMON_ACTION_ID_RUN = 1005,
			HUMAN_COMMON_ACTION_ID_TALK = 1006,
			HUMAN_COMMON_ACTION_ID_USE_ITEM = 1007,
			HUMAN_COMMON_ACTION_ID_SLEEP = 1008,
			HUMAN_COMMON_ACTION_ID_PLAYING_PIANO = 1009,
			HUMAN_COMMON_ACTION_ID_STEPPING = 1011,
			HUMAN_COMMON_ACTION_ID_MAX = 1012
		}
	}
}
