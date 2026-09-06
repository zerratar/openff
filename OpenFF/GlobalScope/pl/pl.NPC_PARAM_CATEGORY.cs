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
		public enum NPC_PARAM_CATEGORY
		{
			NPC_PARAM_CATEGORY_RANDOM_MOVE,
			NPC_PARAM_CATEGORY_AUTO_FOLLOW,
			NPC_PARAM_CATEGORY_MAX
		}
	}
}
