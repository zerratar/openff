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
							public enum MAP_OBJECT_O001_ACTION_ID
							{
								MAP_OBJECT_O001_ACTION_ID_OPEN = 1001,
								MAP_OBJECT_O001_ACTION_ID_OPEN_WAIT,
								MAP_OBJECT_O001_ACTION_ID_CLOSE,
								MAP_OBJECT_O001_ACTION_ID_MAX
							}
	}
}
