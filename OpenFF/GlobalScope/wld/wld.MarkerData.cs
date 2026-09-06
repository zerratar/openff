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
							public class MarkerData
							{
								public int id_;

								public int type_;

								public int x_;

								public int z_;

								public int releaseFlag_;

								public static explicit operator MarkerData(ArrayReader src)
								{
									MarkerData markerData = new MarkerData();
									markerData.id_ = src.readInt32();
									markerData.type_ = src.readInt32();
									markerData.x_ = src.readInt32();
									markerData.z_ = src.readInt32();
									markerData.releaseFlag_ = src.readInt32();
									return markerData;
								}
							}
	}
}
