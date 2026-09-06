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
		public class TickCounter
		{
			public class PfcObject
			{
				public ulong unCounter;

				public bool bRun;
			}

			private PfcObject _objPfc = new PfcObject();

			public TickCounter()
			{
				reset();
			}

			public void reset()
			{
				_objPfc.unCounter = 0uL;
				_objPfc.bRun = false;
			}

			public void start()
			{
				_objPfc.unCounter = (ulong)OS_GetTick();
				_objPfc.bRun = true;
			}

			public ulong stop()
			{
				if (_objPfc.bRun)
				{
					_objPfc.unCounter = (ulong)OS_GetTick() - _objPfc.unCounter;
					_objPfc.bRun = false;
				}
				return _objPfc.unCounter;
			}

			public ulong get()
			{
				if (_objPfc.bRun)
				{
					return (ulong)OS_GetTick() - _objPfc.unCounter;
				}
				return _objPfc.unCounter;
			}
		}
	}
}
