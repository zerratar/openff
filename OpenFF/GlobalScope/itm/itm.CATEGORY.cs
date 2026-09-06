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
	public static partial class itm
	{
		public enum CATEGORY
		{
			CATEGORY_ERR = -1,
			CATEGORY_CONSUMPTION,
			CATEGORY_WEAPON,
			CATEGORY_PROTECTION,
			CATEGORY_MAGIC,
			CATEGORY_IMPORTANT,
			CATEGORY_MAX
		}
	}
}
