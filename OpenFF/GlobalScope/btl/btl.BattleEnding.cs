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
		public class BattleEnding : BaseBattle
		{
			private BATTLE_RESULT result_;

			private BaseBattle[] ending_ = new BaseBattle[3];

			private BattleWin win_ = new BattleWin();

			private BattleLose lose_ = new BattleLose();

			private BattlePlayerEscape playerEscape_ = new BattlePlayerEscape();

			public BattleEnding()
			{
				for (int i = 0; i < 3; i++)
				{
					ending_[i] = null;
				}
			}

			public override void initialize(BattleSystem B)
			{
				registerBattleEnding();
				initializePhase();
				getBattleResult();
				for (int i = 0; i < 4; i++)
				{
					if (B.characterManager().playerParty().battlePlayer(i)
						.isEnable() && !B.characterManager().playerParty().battlePlayer(i)
						.condition()
						.isStone())
					{
						B.characterManager().playerParty().battlePlayer(i)
							.deleteConditionEffect();
					}
				}
			}

			public override void terminate(BattleSystem B)
			{
			}

			public override void execute(BattleSystem B)
			{
				if (ending() != null)
				{
					if (ending().phase() == Phase.Initialize)
					{
						ending().initialize(B);
						ending().setPhase(Phase.Execute);
					}
					if (ending().phase() == Phase.Execute)
					{
						ending().execute(B);
					}
					if (ending().phase() == Phase.Terminate)
					{
						ending().terminate(B);
						ending().setPhase(Phase.Initialize);
						setPhase(Phase.Initialize);
					}
				}
			}

			public void registerBattleEnding()
			{
				ending_[0] = win_;
				ending_[1] = lose_;
				ending_[2] = playerEscape_;
			}

			public void initializePhase()
			{
				ending_[0].setPhase(Phase.Initialize);
				ending_[1].setPhase(Phase.Initialize);
				ending_[2].setPhase(Phase.Initialize);
			}

			public void getBattleResult()
			{
				result_ = BattleToOutside.getInstance().battleResult();
			}

			private BaseBattle ending()
			{
				return ending_[(int)result_];
			}
		}
	}
}
