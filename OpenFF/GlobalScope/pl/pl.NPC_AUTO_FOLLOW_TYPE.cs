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
		public enum NPC_AUTO_FOLLOW_TYPE
		{
			NPC_AUTO_FOLLOW_TYPE_ERR = -1,
			NPC_AUTO_FOLLOW_TYPE_DEFAULT,
			NPC_AUTO_FOLLOW_TYPE_UNE,
			NPC_AUTO_FOLLOW_TYPE_DORGA,
			NPC_AUTO_FOLLOW_TYPE_MAX
		}
	}
}
