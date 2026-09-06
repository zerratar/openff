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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class itm
	{
		public enum MAGIC_USE_KIND
		{
			MAGIC_USE_KIND_ATTACK,
			MAGIC_USE_KIND_RECOVERY,
			MAGIC_USE_KIND_ASSIST,
			MAGIC_USE_KIND_SPECIAL,
			MAGIC_USE_KIND_MAX
		}
	}
}
