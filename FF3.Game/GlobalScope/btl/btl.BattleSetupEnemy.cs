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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class btl
	{
		public class BattleSetupEnemy : BaseBattle
		{
			public override void initialize(BattleSystem B)
			{
				BattleMonsterParty battleMonsterParty = B.characterManager().monsterParty();
				for (int i = 0; i < 6; i++)
				{
					BattleMonster battleMonster = battleMonsterParty.battleMonster(i);
					if (battleMonster != null && battleMonster.isEnable())
					{
						battleMonster.calcConditionTime();
						battleMonster.clearBattleFlag();
						battleMonster.clearSongFlag();
						battleMonster.resetParameterMagicFlag();
						CommonFormula commonFormula = new CommonFormula();
						battleMonster.reupdateParameter(commonFormula.calcJobSkill(battleMonster));
						battleMonster.clearMonsterFlag(MONSTER_FLAG.MF_SPECIAL_ATTACKED);
						if (battleMonster.isBattle())
						{
							battleMonster.setActionNumber(battleMonster.monster().actionNumber());
							battleMonster.clearTarget();
						}
					}
				}
				B.characterManager().changeMagicColor();
			}

			public override void terminate(BattleSystem B)
			{
			}

			public override void execute(BattleSystem B)
			{
				BattleMonsterParty battleMonsterParty = B.characterManager().monsterParty();
				for (int i = 0; i < 6; i++)
				{
					BattleMonster battleMonster = battleMonsterParty.battleMonster(i);
					if (battleMonster != null && battleMonster.isBattle())
					{
						if (OutsideToBattle.getInstance().battleOpeningType() == BATTLE_OPENING_TYPE.INITIATLVE_ATTACK)
						{
							battleMonster.setActionId(0);
							battleMonster.onIsActionEnd();
						}
						else if (battleMonster.condition().isConfusion())
						{
							battleMonster.setActionId(1);
						}
						else if (!battleMonster.condition().isCanTargetSelect())
						{
							battleMonster.setActionId(0);
						}
						else
						{
							selectAction(battleMonster);
						}
					}
				}
				setPhase(Phase.Terminate);
				static_cast<BattleMain>(B.battle()).setNextBattleMainState(BATTLE_MAIN_STATE.TURN_EXECUTE);
			}

			public void selectAction(BattleMonster monster)
			{
				monster.clearFlag(PLAYER_FLAG.PF_CRITICAL);
				if (!monster.monster().isSpecial())
				{
					monster.setActionId(1);
					return;
				}
				if (monster.monsterFlag(MONSTER_FLAG.MF_SPECIAL_ATTACKED))
				{
					monster.setActionId(1);
					return;
				}
				if (monster.condition().isFrog())
				{
					monster.setActionId(1);
					return;
				}
				for (int i = 0; i < mon.MONSTER_SPECIAL_ACTION_MAX; i++)
				{
					int num = (int)ds.RandomNumber.rand32(101u);
					if (num > monster.monster().specialAction(i).specialActionProbability())
					{
						continue;
					}
					int num2 = monster.hp().getLimit() * monster.monster().specialAction(i).actStartHP() / 100;
					if (monster.hp().getNow() > num2)
					{
						continue;
					}
					short num3 = monster.monster().specialAction(i).specialActionId();
					itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(num3);
					if (magicParameter != null && !monster.condition().isSilence() && (magicParameter.system() == 0 || magicParameter.system() == 1) && monster.checkUseMagic(num3))
					{
						monster.setUseMagicId(num3);
						monster.setActionId(3);
						monster.setMonsterFlag(MONSTER_FLAG.MF_SPECIAL_ATTACKED);
						return;
					}
					mon.MonsterSpecialAttackParameter monsterSpecialAttackParameter = mon.MonsterManager.instance().specialAttack(num3);
					if (monsterSpecialAttackParameter == null)
					{
						continue;
					}
					if (monsterSpecialAttackParameter.command() == 1)
					{
						short num4 = selectTableMagic(monster, num3);
						if (monster.checkUseMagic(num4))
						{
							monster.setUseMagicId(num4);
							monster.setActionId(3);
							monster.setMonsterFlag(MONSTER_FLAG.MF_SPECIAL_ATTACKED);
							return;
						}
					}
					else if (magicParameter.system() != 0 && magicParameter.system() != 1 && monster.checkUseMagic(num3))
					{
						monster.setUseMagicId(num3);
						monster.setActionId(2);
						monster.setMonsterFlag(MONSTER_FLAG.MF_SPECIAL_ATTACKED);
						return;
					}
				}
				monster.setUseMagicId(0);
				monster.setActionId(1);
			}

			public short selectTableMagic(BattleMonster monster, short special_id)
			{
				mon.MonsterSpecialAttackParameter monsterSpecialAttackParameter = mon.MonsterManager.instance().specialAttack(special_id);
				int num = 0;
				for (num = 0; num < 6 && monsterSpecialAttackParameter.param(num) >= 0; num++)
				{
				}
				int num2 = 100 / num + 1;
				int num3 = (int)ds.RandomNumber.rand32(101u);
				int num4 = num2;
				for (int i = 0; i < num; i++)
				{
					if (num3 < num4)
					{
						return monsterSpecialAttackParameter.param(i);
					}
					num4 += num2;
				}
				return monsterSpecialAttackParameter.param(0);
			}
		}
	}
}
