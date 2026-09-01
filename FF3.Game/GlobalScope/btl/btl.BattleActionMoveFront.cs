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
	public static partial class btl
	{
		public class BattleActionMoveFront : BattleActionBase
		{
			public override void initialize(BattlePlayer player)
			{
				int num = -1;
				num = (player.condition().isFrog() ? 601 : ((!player.condition().isNearDeath() && !player.condition().isPoison()) ? 601 : 603));
				characterMng.startMotion(player.characterMngId(), num, fLoop: false, 0u);
				int num2;
				int num3;
				if (player.condition().isFrog())
				{
					num2 = 5;
					num3 = 9;
				}
				else
				{
					num2 = 2;
					num3 = 3;
				}
				player.speed_set(PLAYER_MOVE_DISTANCE / (num3 - num2 + 1));
				rotateFront(player);
			}

			public override void terminate(BattlePlayer player)
			{
				player.setConditionMotion(0);
			}

			public override bool execute(BattlePlayer player)
			{
				VecFx32 vecFx = player.rootPosition();
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
				fnd_reuse_pos.x = vecFx.x - -FX_SinIdx(player.moveYaw()) * PLAYER_MOVE_DISTANCE / 4096;
				fnd_reuse_pos.y = 0;
				fnd_reuse_pos.z = vecFx.z - FX_CosIdx(player.moveYaw()) * PLAYER_MOVE_DISTANCE / 4096;
				return front(player, fnd_reuse_pos);
			}
		}
	}
}
