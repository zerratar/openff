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
		public class BattleActionJumpEnd : BattleActionBase
		{
			private static int JUMP_HEIGHT_MAX = 5;

			public override void initialize(BattlePlayer player)
			{
				characterMng.setShadowScale(player.characterMngId(), PlayerShadowScale);
				characterMng.setTransparencyRate(player.characterMngId(), 100);
				if (player.itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_ >= 0)
				{
					characterMng.setTransparencyRate(player.itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_, 100);
				}
				if (player.itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_ >= 0)
				{
					characterMng.setTransparencyRate(player.itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_, 100);
				}
				characterMng.startMotion(player.characterMngId(), 6403, fLoop: false, 0u);
				player.setSpeed(40960);
				player.setIdleType(1);
			}

			public override void terminate(BattlePlayer player)
			{
				player.setIdleType(0);
				player.setConditionMotion(8);
			}

			public override bool execute(BattlePlayer player)
			{
				int currentFrame = (int)characterMng.getCurrentFrame(player.characterMngId());
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
				characterMng.getPosition(player.characterMngId(), fnd_reuse_pos);
				if (currentFrame < JUMP_END_FRAME)
				{
					fnd_reuse_pos.y -= player.speed();
					characterMng.setPosition(player.characterMngId(), fnd_reuse_pos);
					return false;
				}
				if (currentFrame == JUMP_END_FRAME)
				{
					int speed = player.distance() / (ROOT_POSITION_FRAME - JUMP_END_FRAME + 1 + 1);
					player.setSpeed(speed);
					player.setMovePitch(0);
					return false;
				}
				if (currentFrame < ROOT_POSITION_FRAME)
				{
					OS_Printf("PITCH %d\n", player.movePitch());
					fnd_reuse_pos.x -= -(FX_SinIdx(static_cast<int>(player.moveYaw())) * player.speed()) / 4096;
					fnd_reuse_pos.z -= FX_CosIdx(static_cast<int>(player.moveYaw())) * player.speed() / 4096;
					fnd_reuse_pos.y += FX_SinIdx(player.movePitch()) * JUMP_HEIGHT_MAX * 4096 / 4096;
					player.setMovePitch(player.movePitch() + 32768 / (ROOT_POSITION_FRAME - JUMP_END_FRAME + 1 + 1) * 2);
					characterMng.setPosition(player.characterMngId(), fnd_reuse_pos);
					return false;
				}
				if (currentFrame == ROOT_POSITION_FRAME)
				{
					player.setConditionMotion(0);
					characterMng.setPosition(player.characterMngId(), player.rootPosition());
					return true;
				}
				return false;
			}
		}
	}
}
