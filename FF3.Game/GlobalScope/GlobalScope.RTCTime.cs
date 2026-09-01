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
	public class RTCTime
	{
		public int hour;

		public int minute;

		public int second;

		public void parse(ArrayReader reader)
		{
			hour = reader.readInt32();
			minute = reader.readInt32();
			second = reader.readInt32();
		}

		public void copy(RTCTime src)
		{
			hour = src.hour;
			minute = src.minute;
			second = src.second;
		}

		public void setDefault()
		{
			hour = 0;
			minute = 0;
			second = 0;
		}
	}
}
