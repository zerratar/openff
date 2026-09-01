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
	public static partial class pl
	{
		public class AbilityManager
		{
			private PlayerAbility playerAbility_ = new PlayerAbility();

			public PlayerAbility playerAbility()
			{
				return playerAbility_;
			}

			public void setDefault()
			{
				playerAbility_.setDefault();
			}

			public void copy(AbilityManager src)
			{
				playerAbility_.copy(src.playerAbility_);
			}

			public void parse(ArrayReader reader)
			{
				playerAbility_.parse(reader);
			}

			public void store(ArrayWriter writer)
			{
				playerAbility_.store(writer);
			}
		}
	}
}
