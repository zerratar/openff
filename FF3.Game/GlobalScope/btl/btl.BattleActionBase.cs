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
		public class BattleActionBase
		{
			public bool front(BattlePlayer player, VecFx32 goal)
			{
				int num;
				int num2;
				if (player.condition().isFrog())
				{
					num = 5;
					num2 = 9;
				}
				else
				{
					num = 2;
					num2 = 3;
				}
				VecFx32 btl_reuse_pos = btl.btl_reuse_pos;
				characterMng.getPosition(player.characterMngId(), btl_reuse_pos);
				int currentFrame = (int)characterMng.getCurrentFrame(player.characterMngId());
				if (num <= currentFrame && currentFrame <= num2)
				{
					btl_reuse_pos.x -= -(FX_SinIdx(player.moveYaw()) * player.speed()) / 4096;
					btl_reuse_pos.z -= FX_CosIdx(player.moveYaw()) * player.speed() / 4096;
				}
				if (characterMng.isEndOfMotion(player.characterMngId()))
				{
					characterMng.setPosition(player.characterMngId(), goal);
					return true;
				}
				characterMng.setPosition(player.characterMngId(), btl_reuse_pos);
				return false;
			}

			public bool back(BattlePlayer player, VecFx32 goal)
			{
				int num;
				int num2;
				if (player.condition().isFrog())
				{
					num = 4;
					num2 = 8;
				}
				else
				{
					num = 4;
					num2 = 5;
				}
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
				characterMng.getPosition(player.characterMngId(), fnd_reuse_pos);
				int currentFrame = (int)characterMng.getCurrentFrame(player.characterMngId());
				if (num <= currentFrame && currentFrame <= num2)
				{
					fnd_reuse_pos.x -= -(FX_SinIdx(player.moveYaw()) * player.speed()) / 4096;
					fnd_reuse_pos.z -= FX_CosIdx(player.moveYaw()) * player.speed() / 4096;
				}
				if (characterMng.isEndOfMotion(player.characterMngId()))
				{
					characterMng.setPosition(player.characterMngId(), goal);
					return true;
				}
				characterMng.setPosition(player.characterMngId(), fnd_reuse_pos);
				return false;
			}

			public int rotateFront(BattlePlayer player)
			{
				ushort x = 0;
				ushort y = 0;
				ushort z = 0;
				characterMng.getRotation(player.characterMngId(), ref x, ref y, ref z);
				player.moveYaw_set(y);
				if (player.moveYaw() < 0)
				{
					player.moveYaw_add(65536);
				}
				return player.moveYaw();
			}

			public int rotateBack(BattlePlayer player)
			{
				ushort x = 0;
				ushort y = 0;
				ushort z = 0;
				characterMng.getRotation(player.characterMngId(), ref x, ref y, ref z);
				player.moveYaw_set(-y);
				if (player.moveYaw() < 0)
				{
					player.moveYaw_add(65536);
				}
				return player.moveYaw();
			}

			public virtual void initialize(BattlePlayer player)
			{
			}

			public virtual void terminate(BattlePlayer player)
			{
			}

			public virtual bool execute(BattlePlayer player)
			{
				return false;
			}
		}
	}
}
