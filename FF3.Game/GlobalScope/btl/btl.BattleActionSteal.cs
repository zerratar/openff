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
		public class BattleActionSteal : BattleActionBase
		{
			public override void initialize(BattlePlayer player)
			{
				characterMng.startMotion(player.characterMngId(), 6101, fLoop: false, 0u);
				BattleSE.instance().play(203, 1);
			}

			public override void terminate(BattlePlayer player)
			{
			}

			public override bool execute(BattlePlayer player)
			{
				int motionIndex = characterMng.getMotionIndex(player.characterMngId());
				if (motionIndex == 6101 && characterMng.isEndOfMotion(player.characterMngId()))
				{
					return true;
				}
				return false;
			}
		}
	}
}
