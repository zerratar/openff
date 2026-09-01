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
		public class BattleActionJumpStart : BattleActionBase
		{
			public override void initialize(BattlePlayer player)
			{
				characterMng.startMotion(player.characterMngId(), 6401, fLoop: false, 0u);
				player.speed_set(4096);
			}

			public override void terminate(BattlePlayer player)
			{
				characterMng.setTransparencyRate(player.characterMngId(), 0);
				if (player.itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_ >= 0)
				{
					characterMng.setTransparencyRate(player.itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_, 0);
				}
				if (player.itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_ >= 0)
				{
					characterMng.setTransparencyRate(player.itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_, 0);
				}
				player.setConditionMotion(0);
			}

			public override bool execute(BattlePlayer player)
			{
				if (player.checkMotionFrame(JUMP_START_FRAME))
				{
					BattleSE.instance().play(203, 6);
				}
				if (characterMng.getCurrentFrame(player.characterMngId()) >= JUMP_START_FRAME)
				{
					VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
					characterMng.getPosition(player.characterMngId(), fnd_reuse_pos);
					fnd_reuse_pos.x -= -(FX_SinIdx(player.moveYaw()) * player.speed()) / 4096;
					fnd_reuse_pos.z -= FX_CosIdx(player.moveYaw()) * player.speed() / 4096;
					fnd_reuse_pos.y += 40960;
					characterMng.setPosition(player.characterMngId(), fnd_reuse_pos);
					VecFx32 fnd_reuse_pos2 = GlobalScope.fnd_reuse_pos2;
					characterMng.getShadowScale(player.characterMngId(), fnd_reuse_pos2);
					fnd_reuse_pos2.x -= PlayerShadowScale.x / 4;
					fnd_reuse_pos2.z -= PlayerShadowScale.z / 4;
					if (fnd_reuse_pos2.x < 0)
					{
						fnd_reuse_pos2.x = 0;
					}
					if (fnd_reuse_pos2.z < 0)
					{
						fnd_reuse_pos2.z = 0;
					}
					characterMng.setShadowScale(player.characterMngId(), fnd_reuse_pos2);
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
