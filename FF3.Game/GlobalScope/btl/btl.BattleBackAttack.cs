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
		public class BattleBackAttack : BaseBattle
		{
			public const int BACK_ATTACK_EFFECT = 0;

			public const int IS_END_EFFECT = 1;

			public const int TURN_MOTION = 2;

			public const int IS_END_MOTION = 3;

			public const int BACK_ATTACK_STATE_MAX = 4;

			private int backAttackState_;

			public override void initialize(BattleSystem B)
			{
				Battle2DManager.instance().helpWindow().createHelpWindow(61, 0, 0);
				Battle2DManager.instance().helpWindow().setMsdHandle(0);
				BattleEffect.instance().addEfp(446);
				backAttackState_ = 0;
			}

			public override void terminate(BattleSystem B)
			{
				BattleEffect.instance().deleteAll();
				BattleEffect.instance().endEfp();
			}

			public override void execute(BattleSystem B)
			{
				if (dgs.CFade.Main().isFaded() && battleDisplay.openingState() == 1)
				{
					battleDisplay.setOpeningState(2);
					B.characterManager().playerParty().appear(1);
					for (int i = 0; i < 4; i++)
					{
						BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(i);
						if (battlePlayer != null && battlePlayer.isBattle())
						{
							characterMng.setRotation(battlePlayer.characterMngId(), 0, (ushort)(-PlayerRotationY), 0);
						}
					}
					Battle2DManager.instance().helpWindow().releaseHelpWindow();
				}
				if (OutsideToBattle.getInstance().battleCamera() == BATTLE_CAMERA.COMMAND_CAMERA && backAttack(B))
				{
					B.setNextState(BATTLE_SYSTEM_STATE.MAIN);
					setPhase(Phase.Terminate);
				}
			}

			public bool backAttack(BattleSystem B)
			{
				switch (backAttackState_)
				{
				case 0:
					drawBackAttackEffect(B);
					break;
				case 1:
					isEndBackAttackEffect(B);
					break;
				case 2:
					playTurnMotion(B);
					break;
				case 3:
					return isEndTurnMotion(B);
				}
				return false;
			}

			public void drawBackAttackEffect(BattleSystem B)
			{
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(i);
					if (battlePlayer != null && battlePlayer.isBattle())
					{
						int num = BattleEffect.instance().create(446, 1);
						int i2 = battlePlayer.unUsedEffectId();
						battlePlayer.setEffectId(i2, num);
						VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
						characterMng.getPosition(battlePlayer.characterMngId(), fnd_reuse_pos);
						fnd_reuse_pos.y += 73728;
						BattleEffect.instance().setPosition(num, fnd_reuse_pos);
					}
				}
				backAttackState_ = 1;
			}

			public void isEndBackAttackEffect(BattleSystem B)
			{
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(i);
					if (battlePlayer != null && battlePlayer.isBattle() && !battlePlayer.isClearAllEffect())
					{
						return;
					}
				}
				backAttackState_ = 2;
			}

			public void playTurnMotion(BattleSystem B)
			{
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(i);
					if (battlePlayer != null && battlePlayer.isBattle())
					{
						int index = 732;
						if (battlePlayer.condition().isFrog())
						{
							index = 733;
						}
						characterMng.startMotion(battlePlayer.characterMngId(), index, fLoop: false, 0u);
					}
				}
				backAttackState_ = 3;
			}

			public bool isEndTurnMotion(BattleSystem B)
			{
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(i);
					if (battlePlayer != null && battlePlayer.isBattle())
					{
						if (!characterMng.isEndOfMotion(battlePlayer.characterMngId()))
						{
							return false;
						}
						characterMng.setRotation(battlePlayer.characterMngId(), 0, PlayerRotationY, 0);
					}
				}
				return true;
			}
		}
	}
}
