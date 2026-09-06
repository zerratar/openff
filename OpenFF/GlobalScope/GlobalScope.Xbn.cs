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
	public class Xbn
	{
		protected XbnFile xbnFile;

		protected string[] massString;

		public Xbn()
		{
			xbnFile = null;
		}

		public void xbnInitilaize(XbnFile data)
		{
			xbnFile = data;
		}

		public XbnFile xbnFinalize()
		{
			XbnFile result = xbnFile;
			xbnFile = null;
			return result;
		}

		public XbnNode root()
		{
			if (xbnFile == null)
			{
				return null;
			}
			return xbnFile.xbnNode[0];
		}
	}
}
