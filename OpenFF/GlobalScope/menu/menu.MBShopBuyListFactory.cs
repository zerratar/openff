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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class menu
	{
		public class MBShopBuyListFactory : MenuBehaviorFactory
		{
			public MBShopBuyListFactory()
				: base("ShopBuyList")
			{
			}

			public override MenuBehavior mbfCreate()
			{
				return new MBShopBuyList();
			}
		}
	}
}
