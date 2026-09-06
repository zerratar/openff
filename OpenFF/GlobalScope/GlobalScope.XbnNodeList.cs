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
	public class XbnNodeList : ds.Vector<XbnNode, ds.FastErasePolicy<XbnNode>>
	{
		public int prevSearchCursor;

		public XbnNodeList()
			: base(LIMIT_OF_NODELIST)
		{
			prevSearchCursor = 0;
		}

		public override void clear()
		{
			prevSearchCursor = 0;
			base.clear();
		}
	}
}
