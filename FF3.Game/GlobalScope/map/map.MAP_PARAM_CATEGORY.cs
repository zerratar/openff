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
	public static partial class map
	{
							public enum MAP_PARAM_CATEGORY
							{
								MAP_PARAM_CATEGORY_MAPJUMP,
								MAP_PARAM_CATEGORY_LANDFORM,
								MAP_PARAM_CATEGORY_MONSTER_PARTY,
								MAP_PARAM_CATEGORY_SOUND,
								MAP_PARAM_CATEGORY_ENCOUNT,
								MAP_PARAM_CATEGORY_CAMERA,
								MAP_PARAM_CATEGORY_SECRETWAY,
								MAP_PARAM_CATEGORY_MAX
							}
	}
}
