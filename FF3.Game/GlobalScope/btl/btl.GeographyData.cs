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
		public class GeographyData
		{
			private int battleMapId_;

			private GeographyInfo[] geographyInfo_ = new GeographyInfo[GEOGRAPHY_MAX];

			public int battleMapId()
			{
				return battleMapId_;
			}

			public GeographyInfo geographyInfo(int i)
			{
				return geographyInfo_[i];
			}

			public static GeographyData[] castArray(Array src)
			{
				GeographyData[] array = new GeographyData[src.Length / 44];
				ArrayReader arrayReader = new ArrayReader(src);
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = (GeographyData)arrayReader;
				}
				arrayReader.dispose();
				return array;
			}

			public static explicit operator GeographyData(ArrayReader src)
			{
				GeographyData geographyData = new GeographyData();
				geographyData.battleMapId_ = src.readInt32();
				for (int i = 0; i < GEOGRAPHY_MAX; i++)
				{
					geographyData.geographyInfo_[i] = (GeographyInfo)src;
				}
				return geographyData;
			}
		}
	}
}
