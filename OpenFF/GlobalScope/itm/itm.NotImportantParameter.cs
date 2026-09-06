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
	public static partial class itm
	{
		public class NotImportantParameter : ItemBaseParameter
		{
			protected int buy_;

			protected int price_;

			public int buy()
			{
				return buy_;
			}

			public int price()
			{
				return price_;
			}
		}
	}
}
