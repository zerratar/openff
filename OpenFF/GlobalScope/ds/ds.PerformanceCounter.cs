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
	public static partial class ds
	{
		public class PerformanceCounter
		{
			public static ushort NUM_COUNTER_LINES = 8;

			private TickCounter[] _objPfc = new TickCounter[NUM_COUNTER_LINES];

			public PerformanceCounter()
			{
				for (int i = 0; i < _objPfc.Length; i++)
				{
					_objPfc[i] = new TickCounter();
				}
				reset();
			}

			public void reset()
			{
				for (int i = 0; i < NUM_COUNTER_LINES; i++)
				{
					_objPfc[i].reset();
				}
			}

			public void start(ushort lineNum)
			{
				_objPfc[lineNum].start();
			}

			public ulong stop(ushort lineNum)
			{
				return _objPfc[lineNum].stop();
			}

			public ulong get(ushort lineNum)
			{
				return _objPfc[lineNum].get();
			}
		}
	}
}
