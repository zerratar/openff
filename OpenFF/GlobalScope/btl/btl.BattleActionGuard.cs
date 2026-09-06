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
		public class BattleActionGuard : BattleActionBase
		{
			public override void initialize(BattlePlayer player)
			{
				if (!player.condition().isFrog())
				{
					characterMng.startMotion(player.characterMngId(), 704, fLoop: true, 0u);
				}
			}

			public override void terminate(BattlePlayer player)
			{
				player.playerActionId_set(-1);
				player.setConditionMotion(11);
			}

			public override bool execute(BattlePlayer player)
			{
				if (player.condition().isFrog())
				{
					return true;
				}
				if (characterMng.getMotionIndex(player.characterMngId()) == 704 && characterMng.isEndOfMotion(player.characterMngId()))
				{
					return true;
				}
				return false;
			}
		}
	}
}
