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
	public static partial class pl
	{
		public class CPlayerVehicleAction : act.CBaseAction
		{
			protected int m_Counter;

			public CPlayerCharacter GetPlayer()
			{
				return static_cast<CPlayerCharacter>(getCharacter());
			}

			public CPlayerCharacter Player()
			{
				return static_cast<CPlayerCharacter>(character());
			}
		}
	}
}
