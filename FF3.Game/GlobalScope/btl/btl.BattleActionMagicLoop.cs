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
		public class BattleActionMagicLoop : BattleActionBase
		{
			private int loopFrame_;

			public override void initialize(BattlePlayer player)
			{
				loopFrame_ = MAGIC_LOOP_FRAME_MAX;
				if (!player.condition().isFrog())
				{
					int motionIndex = characterMng.getMotionIndex(player.characterMngId());
					if (motionIndex != 4003)
					{
						characterMng.startMotion(player.characterMngId(), 4003, fLoop: true, 0u);
					}
				}
			}

			public override void terminate(BattlePlayer player)
			{
				player.setIdleType(0);
			}

			public override bool execute(BattlePlayer player)
			{
				loopFrame_--;
				if (loopFrame_ == 0)
				{
					return true;
				}
				return false;
			}
		}
	}
}
