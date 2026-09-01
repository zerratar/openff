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
	public static partial class btl
	{
		public class BattleActionDamage : BattleActionBase
		{
			public override void initialize(BattlePlayer player)
			{
				if (!player.condition().isFrog())
				{
					characterMng.startMotion(player.characterMngId(), 705, fLoop: false, 0u);
				}
			}

			public override void terminate(BattlePlayer player)
			{
				player.setPlayerActionId(-1);
				player.setConditionMotion(0);
			}

			public override bool execute(BattlePlayer player)
			{
				if (player.condition().isFrog())
				{
					return false;
				}
				if (characterMng.isEndOfMotion(player.characterMngId()))
				{
					return true;
				}
				return false;
			}
		}
	}
}
