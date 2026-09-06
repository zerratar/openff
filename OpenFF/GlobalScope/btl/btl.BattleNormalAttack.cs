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
		public class BattleNormalAttack : BaseBattle
		{
			public override void initialize(BattleSystem B)
			{
			}

			public override void terminate(BattleSystem B)
			{
			}

			public override void execute(BattleSystem B)
			{
				if (dgs.CFade.Main().isFaded() && battleDisplay.openingState() == 1)
				{
					battleDisplay.setOpeningState(2);
					B.characterManager().playerParty().appear(1);
				}
				if (OutsideToBattle.getInstance().battleCamera() == BATTLE_CAMERA.COMMAND_CAMERA)
				{
					B.setNextState(BATTLE_SYSTEM_STATE.MAIN);
					setPhase(Phase.Terminate);
				}
			}
		}
	}
}
