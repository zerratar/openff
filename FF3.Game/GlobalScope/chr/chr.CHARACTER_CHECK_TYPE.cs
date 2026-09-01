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
	public static partial class chr
	{
		public enum CHARACTER_CHECK_TYPE
		{
			CHARACTER_CHECK_TYPE_ERR = -1,
			CHARACTER_CHECK_TYPE_TALK,
			CHARACTER_CHECK_TYPE_CHECK,
			CHARACTER_CHECK_TYPE_MAX
		}
	}
}
