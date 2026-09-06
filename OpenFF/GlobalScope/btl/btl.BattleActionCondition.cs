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
	public static partial class btl
	{
		public class BattleActionCondition : BattleActionBase
		{
			public override void initialize(BattlePlayer player)
			{
			}

			public override void terminate(BattlePlayer player)
			{
			}

			public override bool execute(BattlePlayer player)
			{
				return player.setConditionMotion(0);
			}
		}
	}
}
