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
	public static partial class dgs
	{
		public class C_C_SET
		{
			public string code;

			public CtrlCodeProcessor processor;

			public C_C_SET(string arg0, CtrlCodeProcessor arg1)
			{
				code = arg0;
				processor = arg1;
			}
		}
	}
}
