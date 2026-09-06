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
	public static partial class wld
	{
							public enum MapMarkerType
							{
								MAP_MARKER_INVALID = -1,
								MAP_MARKER_PLAYER,
								MAP_MARKER_VEHICLE,
								MAP_MARKER_TOWN,
								MAP_MARKER_DUNGEON,
								MAP_MARKER_CHOCOBO,
								MAP_MARKER_FIELD,
								MAP_MARKER_DOGA,
								MAP_MARKER_TOWN2,
								MAP_MARKER_SHIRINE,
								MAP_MARKER_TOWN3,
								MAP_MARKER_CASTLE,
								MAP_MARKER_TOWER,
								MAP_MARKER_VEHICLE1,
								MAP_MARKER_VEHICLE2,
								MAP_MARKER_VEHICLE3,
								MAP_MARKER_MAX
							}
	}
}
