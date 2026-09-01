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
						public enum MapMarkerDirection
						{
							MARK_DIR_W,
							MARK_DIR_E,
							MARK_DIR_NW,
							MARK_DIR_NE,
							MARK_DIR_SW,
							MARK_DIR_SE,
							MARK_DIR_N,
							MARK_DIR_S
						}
}
