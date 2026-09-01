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
	public static partial class mognet
	{
		public class DWCUtilCaller
		{
			protected byte[] work = new byte[32];

			public void dwcucExecute()
			{
				int a = ds.CHeap.align_app();
				ds.CHeap.realign_app(32);
				byte[] arg = work;
				SWC_Init(arg);
				SWC_SetMemFunc(AllocatorForDWC, DeallocatorForDWC);
				DWC_SetAuthServer(0);
				ds.CHeap.realign_app(a);
			}

			~DWCUtilCaller()
			{
			}
		}
	}
}
