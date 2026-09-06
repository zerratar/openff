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
	public static partial class map
	{
							public enum MAP_JUMP_PLAYER_CONDITION_FLAG
							{
								MAP_JUMP_PLAYER_CONDITION_FLAG_ALL = 1,
								MAP_JUMP_PLAYER_CONDITION_FLAG_SMALL = 2,
								MAP_JUMP_PLAYER_CONDITION_FLAG_FROG = 4,
								MAP_JUMP_PLAYER_CONDITION_FLAG_MAX = 5
							}
	}
}
