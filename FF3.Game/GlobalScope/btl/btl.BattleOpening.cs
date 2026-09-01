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
		public class BattleOpening : BaseBattle
		{
			public const int BACK_ATTACK_RATE = 5;

			public const int INITIATIVE_ATTACK_RATE = 10;

			private BATTLE_OPENING_TYPE type_;

			private BaseBattle[] opening_ = new BaseBattle[3];

			private BattleNormalAttack normalAttack_ = new BattleNormalAttack();

			private BattleInitiativeAttack initiativeAttack_ = new BattleInitiativeAttack();

			private BattleBackAttack backAttack_ = new BattleBackAttack();

			public BattleOpening()
			{
				for (int i = 0; i < 3; i++)
				{
					opening_[i] = null;
				}
			}

			public override void initialize(BattleSystem B)
			{
				registerBattleOpening();
				initializePhase();
				getBattleOpeningType();
				selectBattleOpeningType();
				if (type_ != BATTLE_OPENING_TYPE.BACK_ATTACK)
				{
					return;
				}
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(i);
					if (battlePlayer != null && battlePlayer.isEnable())
					{
						battlePlayer.player().changeFormationType();
					}
				}
				B.characterManager().playerParty().initializePlayerPosition();
			}

			public override void terminate(BattleSystem B)
			{
			}

			public override void execute(BattleSystem B)
			{
				if (opening() != null)
				{
					if (opening().phase() == Phase.Initialize)
					{
						opening().initialize(B);
						opening().setPhase(Phase.Execute);
					}
					if (opening().phase() == Phase.Execute)
					{
						opening().execute(B);
					}
					if (opening().phase() == Phase.Terminate)
					{
						B.playerWindow().create();
						opening().terminate(B);
						opening().setPhase(Phase.Initialize);
					}
				}
			}

			public void registerBattleOpening()
			{
				opening_[0] = normalAttack_;
				opening_[1] = initiativeAttack_;
				opening_[2] = backAttack_;
			}

			public void initializePhase()
			{
				opening_[0].setPhase(Phase.Initialize);
				opening_[1].setPhase(Phase.Initialize);
				opening_[2].setPhase(Phase.Initialize);
			}

			public void getBattleOpeningType()
			{
				type_ = OutsideToBattle.getInstance().battleOpeningType();
			}

			public void selectBattleOpeningType()
			{
				if (OutsideToBattle.getInstance().battleType() != BATTLE_TYPE.NORMAL_BATTLE)
				{
					type_ = BATTLE_OPENING_TYPE.NORMAL_ATTACK;
				}
				else if (OutsideToBattle.getInstance().battleOpeningType() == BATTLE_OPENING_TYPE.CALC_BATTLE_OPENING_TYPE)
				{
					int num = (int)ds.RandomNumber.rand32(101u);
					if (num < 5)
					{
						type_ = BATTLE_OPENING_TYPE.BACK_ATTACK;
					}
					else if (num + 5 < 15)
					{
						type_ = BATTLE_OPENING_TYPE.INITIATLVE_ATTACK;
					}
					else
					{
						type_ = BATTLE_OPENING_TYPE.NORMAL_ATTACK;
					}
				}
				else
				{
					type_ = OutsideToBattle.getInstance().battleOpeningType();
				}
				OutsideToBattle.getInstance().setBattleOpeningType(type_);
			}

			private BaseBattle opening()
			{
				return opening_[(int)type_];
			}
		}
	}
}
