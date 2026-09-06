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
		public class BattleMain : BaseBattle
		{
			private BATTLE_MAIN_STATE state_;

			private BATTLE_MAIN_STATE nextState_;

			private ys.ParameterPoint<int> turnCount_ = new ys.ParameterPoint<int>(0, 9999);

			private BaseBattle[] main_ = new BaseBattle[3];

			private BattleSetupPlayer setupPlayer_ = new BattleSetupPlayer();

			private BattleSetupEnemy setupEnemy_ = new BattleSetupEnemy();

			private BattleTurnExecute turnExecute_ = new BattleTurnExecute();

			public BattleMain()
			{
				for (int i = 0; i < 3; i++)
				{
					main_[i] = null;
				}
			}

			public override void initialize(BattleSystem B)
			{
				registerBattleMain();
				initializePhase();
				setBattleMainState(BATTLE_MAIN_STATE.SETUP_PLAYER);
				B.turnCount_ = 0u;
			}

			public override void terminate(BattleSystem B)
			{
				if (BattleToOutside.getInstance().battleResult() != BATTLE_RESULT.WIN)
				{
					_ = 1;
				}
			}

			public override void execute(BattleSystem B)
			{
				if (main() != null)
				{
					if (main().phase() == Phase.Initialize)
					{
						main().initialize(B);
						main().setPhase(Phase.Execute);
					}
					if (main().phase() == Phase.Execute)
					{
						main().execute(B);
					}
					if (main().phase() == Phase.Terminate)
					{
						main().terminate(B);
						main().setPhase(Phase.Initialize);
						setBattleMainState(nextBattleMainState());
					}
				}
			}

			public void releaseData()
			{
				setupPlayer_.releaseData();
				turnExecute_.releaseData();
			}

			public void registerBattleMain()
			{
				main_[0] = setupPlayer_;
				main_[1] = setupEnemy_;
				main_[2] = turnExecute_;
			}

			public void initializePhase()
			{
				main_[0].setPhase(Phase.Initialize);
				main_[1].setPhase(Phase.Initialize);
				main_[2].setPhase(Phase.Initialize);
			}

			private BaseBattle main()
			{
				return main_[(int)state_];
			}

			private void setBattleMainState(BATTLE_MAIN_STATE state)
			{
				state_ = state;
			}

			private BATTLE_MAIN_STATE battleMainState()
			{
				return state_;
			}

			public void setNextBattleMainState(BATTLE_MAIN_STATE state)
			{
				nextState_ = state;
			}

			private BATTLE_MAIN_STATE nextBattleMainState()
			{
				return nextState_;
			}
		}
	}
}
