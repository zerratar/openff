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
		public class BattleActionMagic : BattleActionBase
		{
			public override void initialize(BattlePlayer player)
			{
				if (!player.condition().isFrog())
				{
					characterMng.startMotion(player.characterMngId(), 4002, fLoop: false, 5u);
				}
			}

			public override void terminate(BattlePlayer player)
			{
			}

			public override bool execute(BattlePlayer player)
			{
				if (player.condition().isFrog())
				{
					return true;
				}
				int motionIndex = characterMng.getMotionIndex(player.characterMngId());
				if (motionIndex == 4002 && characterMng.isEndOfMotion(player.characterMngId()))
				{
					return true;
				}
				return false;
			}
		}
	}
}
