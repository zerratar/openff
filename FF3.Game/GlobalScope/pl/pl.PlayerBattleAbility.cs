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
		public class PlayerBattleAbility
		{
			private int abilityId_;

			private ys.MotionEffects effect_;

			private ys.MotionEffects se_;

			private int motionStartFrame_;

			public int abilityId()
			{
				return abilityId_;
			}

			public ys.MotionEffects effect()
			{
				return effect_;
			}

			public ys.MotionEffects se()
			{
				return se_;
			}

			public int motionStartFrame()
			{
				return motionStartFrame_;
			}
		}
	}
}
