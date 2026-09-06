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
	public static partial class wld
	{
							public class MapMarker
							{
								public MapMarkerType Type_;

								public int x_;

								public int y_;

								public sys2d.Cell Cell_ = new sys2d.Cell();

								public bool m_bCell_YSeparate;

								public sys2d.Cell Cell_YSeparate = new sys2d.Cell();

								public int dir;
							}
	}
}
