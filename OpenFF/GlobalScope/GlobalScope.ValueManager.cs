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
	public class ValueManager
	{
		public static ValueManager instance_ = new ValueManager();

		public static ValueManager singleton()
		{
			return instance_;
		}

		public int get(uint group, uint index)
		{
			return values[group * 3 + index];
		}

		public void set(uint group, uint index, int value)
		{
			values[group * 3 + index] = value;
		}

		public void inc(uint group, uint index)
		{
			values[group * 3 + index]++;
		}

		public void dec(uint group, uint index)
		{
			values[group * 3 + index]--;
		}

		~ValueManager()
		{
		}
	}
}
