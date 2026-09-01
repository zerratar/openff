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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class wld
	{
							public class TownInfo
							{
								public string namePrerfix_;

								public sbyte belongWorld_;

								public int flag_;

								public TownInfo(string arg0, sbyte arg1, int arg2)
								{
									namePrerfix_ = arg0;
									belongWorld_ = arg1;
									flag_ = arg2;
								}
							}
	}
}
