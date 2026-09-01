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
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class map
	{
							public enum MAP_LANDFORM_TYPE
							{
								MAP_LANDFORM_TYPE_ERR = -1,
								MAP_LANDFORM_TYPE_GRS = 0,
								MAP_LANDFORM_TYPE_FRT = 1,
								MAP_LANDFORM_TYPE_DES = 2,
								MAP_LANDFORM_TYPE_LAK = 3,
								MAP_LANDFORM_TYPE_SEA = 4,
								MAP_LANDFORM_TYPE_MOR = 5,
								MAP_LANDFORM_TYPE_SKY = 6,
								MAP_LANDFORM_TYPE_SBD = 7,
								MAP_LANDFORM_TYPE_CWL = 8,
								MAP_LANDFORM_TYPE_SBW = 9,
								MAP_LANDFORM_FIELD_TYPE_MAX = 10,
								MAP_LANDFORM_TYPE_HOR = MAP_LANDFORM_FIELD_TYPE_MAX,
								MAP_LANDFORM_TYPE_INR = 11,
								MAP_LANDFORM_TYPE_WAT = 12,
								MAP_LANDFORM_TYPE_STL = 13,
								MAP_LANDFORM_TYPE_WOD = 14,
								MAP_LANDFORM_TYPE_MAX = 15
							}
	}
}
