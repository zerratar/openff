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
	public class RTCDate
	{
		public int year;

		public int month;

		public int day;

		public int week;

		public long absoluteTime;

		public void parse(ArrayReader reader)
		{
			year = reader.readInt32();
			month = reader.readInt32();
			day = reader.readInt32();
			week = reader.readInt32();
			absoluteTime = reader.readInt64();
		}

		public void copy(RTCDate src)
		{
			year = src.year;
			month = src.month;
			day = src.day;
			week = src.week;
			absoluteTime = src.absoluteTime;
		}

		public void setDefault()
		{
			year = 0;
			month = 0;
			day = 0;
			week = 0;
			absoluteTime = 0L;
		}

		public byte[] toByteArray()
		{
			ArrayWriter arrayWriter = new ArrayWriter();
			arrayWriter.writeInt32(year);
			arrayWriter.writeInt32(month);
			arrayWriter.writeInt32(day);
			arrayWriter.writeInt32(week);
			arrayWriter.writeInt64(absoluteTime);
			return arrayWriter.getBytes();
		}
	}
}
