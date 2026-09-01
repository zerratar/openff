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
	public static partial class btl
	{
		public class BattleToOutside
		{
			public static BattleToOutside instance_ = new BattleToOutside();

			private BATTLE_RESULT result_;

			public void initialize()
			{
				initializeBattleResult();
			}

			public static BattleToOutside getInstance()
			{
				return instance_;
			}

			public void initializeBattleResult()
			{
				result_ = BATTLE_RESULT.ERR_RESULT;
			}

			public void setBattleResult(BATTLE_RESULT result)
			{
				result_ = result;
			}

			public BATTLE_RESULT battleResult()
			{
				return result_;
			}
		}
	}
}
