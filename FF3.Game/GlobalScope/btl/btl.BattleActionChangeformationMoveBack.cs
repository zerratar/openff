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
		public class BattleActionChangeformationMoveBack : BattleActionBase
		{
			public override void initialize(BattlePlayer player)
			{
				int num = -1;
				num = (player.condition().isFrog() ? 604 : ((!player.condition().isNearDeath() && !player.condition().isPoison()) ? 604 : 606));
				characterMng.startMotion(player.characterMngId(), num, fLoop: false, 0u);
				BattleSE.instance().play(200, 4);
				int num2;
				int num3;
				if (player.condition().isFrog())
				{
					num2 = 4;
					num3 = 8;
				}
				else
				{
					num2 = 4;
					num3 = 5;
				}
				player.speed_set(PLAYER_MOVE_DISTANCE / (num3 - num2 + 1));
				rotateBack(player);
			}

			public override void terminate(BattlePlayer player)
			{
				player.setConditionMotion(0);
			}

			public override bool execute(BattlePlayer player)
			{
				return back(player, player.rootPosition());
			}
		}
	}
}
