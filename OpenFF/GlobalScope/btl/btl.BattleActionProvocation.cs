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
		public class BattleActionProvocation : BattleActionBase
		{
			public override void initialize(BattlePlayer player)
			{
				characterMng.startMotion(player.characterMngId(), 6501, fLoop: false, 0u);
			}

			public override void terminate(BattlePlayer player)
			{
			}

			public override bool execute(BattlePlayer player)
			{
				if (player.checkMotion(6501) && characterMng.isEndOfMotion(player.characterMngId()))
				{
					return true;
				}
				return false;
			}
		}
	}
}
