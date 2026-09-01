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
	public static partial class pl
	{
		public enum VEHICLE_MOTION_ID
		{
			VEHICLE_MOTION_ID_SAIL_OUT = 1001,
			VEHICLE_MOTION_ID_SAIL_IN = 1002,
			VEHICLE_MOTION_ID_PROPELLER_OUT = 1003,
			VEHICLE_MOTION_ID_PROPELLER_IN = 1004,
			VEHICLE_MOTION_ID_WAIT = 1005,
			VEHICLE_MOTION_ID_MOVE = VEHICLE_MOTION_ID_WAIT,
			VEHICLE_MOTION_ID_SHIP_WAIT = 1006,
			VEHICLE_MOTION_ID_SHIP_MOVE = 1007,
			VEHICLE_MOTION_ID_MAX = 1008
		}
	}
}
