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
		public class BattleActionTakeAPowder : BattleActionBase
		{
			public override void initialize(BattlePlayer player)
			{
				if (!player.condition().isFrog())
				{
					characterMng.startMotion(player.characterMngId(), 702, fLoop: false, 0u);
				}
				else
				{
					characterMng.startMotion(player.characterMngId(), 731, fLoop: false, 0u);
				}
				characterMng.setMotionSpeed(player.characterMngId(), 8192);
			}

			public override void terminate(BattlePlayer player)
			{
			}

			public override bool execute(BattlePlayer player)
			{
				if (characterMng.isEndOfMotion(player.characterMngId()))
				{
					return true;
				}
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
				fnd_reuse_pos.y = MonsterRotationY;
				VecFx32 fnd_reuse_pos2 = GlobalScope.fnd_reuse_pos;
				characterMng.getPosition(player.characterMngId(), fnd_reuse_pos2);
				int currentFrame = (int)characterMng.getCurrentFrame(player.characterMngId());
				int num = 24576;
				if (currentFrame >= 2)
				{
					fnd_reuse_pos2.x -= -(FX_SinIdx(fnd_reuse_pos.y) * num) / 4096;
					fnd_reuse_pos2.z -= FX_CosIdx(fnd_reuse_pos.y) * num / 4096;
					characterMng.setPosition(player.characterMngId(), fnd_reuse_pos2);
				}
				return false;
			}
		}
	}
}
