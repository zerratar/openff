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
		public enum PLAYER_PARAM_CATEGORY
		{
			PLAYER_PARAM_CATEGORY_MOVE,
			PLAYER_PARAM_CATEGORY_ENTER,
			PLAYER_PARAM_CATEGORY_VEHICLE_MOVE,
			PLAYER_PARAM_CATEGORY_VEHICLE_ENTER,
			PLAYER_PARAM_CATEGORY_SE_EFFECT_PLAY,
			PLAYER_PARAM_CATEGORY_SE_EFFECT_MAP,
			PLAYER_PARAM_CATEGORY_MAX
		}
	}
}
