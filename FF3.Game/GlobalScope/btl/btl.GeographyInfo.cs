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
	public static partial class btl
	{
		public class GeographyInfo
		{
			public short geographyId_;

			public short odds_;

			public static explicit operator GeographyInfo(ArrayReader src)
			{
				GeographyInfo geographyInfo = new GeographyInfo();
				geographyInfo.geographyId_ = src.readInt16();
				geographyInfo.odds_ = src.readInt16();
				return geographyInfo;
			}
		}
	}
}
