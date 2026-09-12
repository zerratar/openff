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
		public class NewMagicFormula
		{
			public int attackMagicDamage(short magicId, BaseBattleCharacter character, BaseBattleCharacter target, int targetNumber)
			{
				if (magicId == 4105)
				{
					short condition = itm.ItemManager.instance().magicParameter(magicId).changeCondition();
					short num = target.physicsDefense().antiOption();
					int num2 = (int)ds.RandomNumber.logic32(101u);
					if (calcAntiOption(condition, num))
					{
						target.setFlag(PLAYER_FLAG.PF_MISS);
						target.clearFlag(PLAYER_FLAG.PF_2D);
						return 0;
					}
					int num3 = calcCommonConditionOdds(character, 7, target, 30, 1);
					if (num3 > num2)
					{
						setCondition(character, target, condition, num, nearStone: false);
						OS_Printf("毒になりました\n");
					}
					else if (!target.changeCondition().isPoison())
					{
						target.setFlag(PLAYER_FLAG.PF_MISS);
						target.clearFlag(PLAYER_FLAG.PF_2D);
						return 0;
					}
				}
				if (magicId == 4110)
				{
					short condition2 = itm.ItemManager.instance().magicParameter(magicId).changeCondition();
					short antiOption = target.physicsDefense().antiOption();
					int num4 = (int)ds.RandomNumber.logic32(101u);
					if (calcAntiOption(condition2, antiOption))
					{
						target.setFlag(PLAYER_FLAG.PF_MISS);
						target.clearFlag(PLAYER_FLAG.PF_2D);
						return 0;
					}
					int num5 = calcCommonConditionOdds(character, 7, target, 30, 1);
					if (num5 > num4)
					{
						target.condition().onNearStone();
						target.condition().goStone();
						OS_Printf("石になりつつあります\n");
					}
					else if (!target.changeCondition().isNearStone())
					{
						target.setFlag(PLAYER_FLAG.PF_MISS);
						target.clearFlag(PLAYER_FLAG.PF_2D);
						return 0;
					}
				}
				if (magicId == 4119 && target.breed() == 1 && ((target.magicDefense().weakType() & 0x200) != 0 || (target.physicsDefense().antiType() & 0x80) != 0))
				{
					target.setFlag(PLAYER_FLAG.PF_MISS);
					target.clearFlag(PLAYER_FLAG.PF_2D);
					return 0;
				}
				switch (magicId)
				{
				case 6004:
				{
					int now = target.hp().getNow();
					CommonFormula commonFormula = new CommonFormula();
					int t = now * (10 + commonFormula.calcJobSkill(character) * 10 / 110) / 100;
					return ds.clamp(t, 1, 9999);
				}
				case 4221:
					return calcKick(character);
				default:
				{
					int num6 = calcAttackMagicDamage(magicId, character, target);
					OS_Printf("魔法ダメージ算出 %d\n", num6 / 4096);
					int num7 = calcMagicSuccessValue(calcMagicSuccess(character, target));
					OS_Printf("魔法成功判定 %d\n", num7 / 4096);
					int num8 = static_cast<int>(calcAttributeValue(magicId, target)) * 4096;
					OS_Printf("属性効果 %d\n", num8 / 4096);
					int num9 = calcTargetNumberValue(targetNumber, 80, magicId);
					OS_Printf("複数体効果 %d\n", num9 / 4096);
					int num10 = num6 / 4096;
					uint num11 = (uint)(num10 * num7 / 4096 * num9 / 4096);
					num11 = ((num8 != 0) ? (num11 * (uint)(num8 / 4096)) : (num11 / 2));
					OS_Printf("実際のダメージ算出 %d\n", num11);
					return (int)num11;
				}
				}
			}

			public int healingMagicValue(short magicId, BaseBattleCharacter character, BaseBattleCharacter target, int targetNumber)
			{
				if (magicId == 6003)
				{
					int limit = target.hp().getLimit();
					CommonFormula commonFormula = new CommonFormula();
					int t = limit * (10 + commonFormula.calcJobSkill(character) * 10 / 110) / 100;
					return ds.clamp(t, 1, 9999);
				}
				int num = static_cast<int>(calcHealingValue(magicId, character, target)) * 4096;
				OS_Printf("回復量計算 %d\n", num);
				int num2 = static_cast<int>(calcAttributeValue(magicId, target)) * 4096;
				OS_Printf("属性効果 %d\n", num2);
				if (num2 != 8192)
				{
					target.setFlag(PLAYER_FLAG.PF_RECOVER);
				}
				else
				{
					target.clearFlag(PLAYER_FLAG.PF_RECOVER);
				}
				int num3 = calcTargetNumberValue(targetNumber, 90, magicId);
				OS_Printf("複数体効果 %d\n", num3);
				int num4 = num / 4096;
				num4 = (int)(num4 * (100 - ds.RandomNumber.logic32(10u)) / 100);
				OS_Printf("実際の回復量算出 %d\n", num4 * num2 / 4096 * num3 / 4096);
				return num4 * num2 / 4096 * num3 / 4096;
			}

			public int conditionMagicOdds(short magicId, BaseBattleCharacter user, BaseBattleCharacter target, int targetNumber)
			{
				return calcConditionOdds(magicId, user, target);
			}

			public int calcAttackMagicDamage(short magicId, BaseBattleCharacter character, BaseBattleCharacter target)
			{
				int num = itm.ItemManager.instance().magicParameter(magicId).magicAggressivity();
				CommonFormula commonFormula = new CommonFormula();
				int num2 = commonFormula.calcJobSkill(character);
				int num3 = target.magicDefense().magicPhylacticPower();
				int num4 = character.bodyAndBonus().intelligence().get();
				int num5 = target.bodyAndBonus().mind().get();
				int num6 = 4096 * ((num + num2 - num3 - num5) * num4 / 3);
				if (num6 < 0)
				{
					num6 = 0;
				}
				return num6;
			}

			public bool calcMagicSuccess(BaseBattleCharacter character, BaseBattleCharacter target)
			{
				int num = character.bodyAndBonus().intelligence().get() - (target.bodyAndBonus().mind().get() + 30);
				if (character.condition().isDarkness())
				{
					num /= 2;
				}
				if (num < ds.RandomNumber.logic32(101u))
				{
					return false;
				}
				return true;
			}

			public int calcKick(BaseBattleCharacter attacker)
			{
				int num = attacker.bodyAndBonus().mind().get();
				int num2 = attacker.bodyAndBonus().intelligence().get();
				CommonFormula commonFormula = new CommonFormula();
				int num3 = commonFormula.calcJobSkill(attacker);
				return (num + num2) / 4 * ((50 + num3) / 3) / 4;
			}

			public int calcHealingValue(short magicId, BaseBattleCharacter character, BaseBattleCharacter target)
			{
				int num = character.bodyAndBonus().mind().get();
				CommonFormula commonFormula = new CommonFormula();
				int num2 = commonFormula.calcJobSkill(character);
				int num3 = character.bodyAndBonus().vitality().get();
				int num4 = itm.ItemManager.instance().magicParameter(magicId).magicAggressivity();
				return (num / 2 + num2 / 4 + num3 / 8) * num4;
			}

			public int calcConditionOdds(short magicId, BaseBattleCharacter character, BaseBattleCharacter target)
			{
				int num = (int)ds.RandomNumber.logic32(101u);
				OS_Printf("ランダム値は\u3000%d\u3000です\n", num);
				int num2 = 0;
				bool flag = false;
				short condition = itm.ItemManager.instance().magicParameter(magicId).changeCondition();
				short num3 = target.physicsDefense().antiOption();
				switch (magicId)
				{
				case 4008:
					if (OutsideToBattle.getInstance().battleType() == BATTLE_TYPE.NORMAL_BATTLE)
					{
						target.setFlag(PLAYER_FLAG.PF_ERASE);
						flag = true;
					}
					OS_Printf("テレポ確認\n");
					break;
				case 4011:
				case 6604:
				case 6645:
					if (!calcAntiOption(condition, num3))
					{
						num2 = calcCommonConditionOdds(character, 7, target, 25, 1);
						if (num2 > num)
						{
							flag = true;
							setCondition(character, target, condition, num3, nearStone: false);
							OS_Printf("混乱しました\n");
						}
						else if (target.changeCondition().isConfusion())
						{
							flag = true;
						}
					}
					break;
				case 4012:
					if (!calcAntiOption(condition, num3))
					{
						num2 = calcCommonConditionOdds(character, 7, target, 40, 2);
						if (num2 > num)
						{
							flag = true;
							setCondition(character, target, condition, num3, nearStone: false);
							OS_Printf("沈黙になりました\n");
						}
						else if (target.changeCondition().isSilence())
						{
							flag = true;
						}
					}
					break;
				case 4021:
				case 4206:
					if (!target.magicFlag(MAGIC_FLAG.MF_REFLECT))
					{
						flag = true;
						OS_Printf("リフレクかけました\n");
					}
					break;
				case 4022:
					if (!calcAntiOption(condition, num3))
					{
						num2 = calcCommonConditionOdds(character, 7, target, 40, 2);
						if (num2 > num)
						{
							flag = true;
							setCondition(character, target, condition, num3, nearStone: false);
							OS_Printf("瀕死になりました\n");
						}
					}
					break;
				case 4103:
				case 4202:
				case 6603:
				case 6644:
					if (!calcAntiOption(condition, num3))
					{
						num2 = calcCommonConditionOdds(character, 7, target, 25, 1);
						if (num2 > num)
						{
							flag = true;
							setCondition(character, target, condition, num3, nearStone: false);
							OS_Printf("眠りました\n");
						}
						else if (target.changeCondition().isSleep())
						{
							flag = true;
						}
					}
					break;
				case 4105:
					if (!calcAntiOption(condition, num3))
					{
						num2 = calcCommonConditionOdds(character, 7, target, 30, 1);
						if (num2 > num)
						{
							flag = true;
							setCondition(character, target, condition, num3, nearStone: false);
							OS_Printf("毒になりました\n");
						}
						else if (target.changeCondition().isPoison())
						{
							flag = true;
						}
					}
					break;
				case 4106:
					if (!calcAntiOption(condition, num3))
					{
						num2 = calcCommonConditionOdds(character, 7, target, 30, 1);
						if (num2 > num)
						{
							flag = true;
							setCondition(character, target, condition, num3, nearStone: false);
							OS_Printf("暗闇になりました\n");
						}
						else if (target.changeCondition().isDarkness())
						{
							flag = true;
						}
					}
					break;
				case 4110:
					if (!calcAntiOption(condition, num3))
					{
						num2 = calcCommonConditionOdds(character, 7, target, 30, 1);
						if (num2 > num)
						{
							flag = true;
							target.condition().onNearStone();
							target.condition().goStone();
							OS_Printf("石になりつつあります\n");
						}
					}
					break;
				case 4112:
				case 4203:
					if (!calcAntiOption(condition, num3))
					{
						num2 = calcCommonConditionOdds(character, 7, target, 20, 1);
						if (num2 > num)
						{
							flag = true;
							setCondition(character, target, condition, num3, nearStone: false);
							OS_Printf("麻痺になりました\n");
						}
						else if (target.changeCondition().isParalysis())
						{
							flag = true;
						}
					}
					break;
				case 4114:
					if (!calcAntiOption(condition, num3))
					{
						int num4 = character.level();
						int num5 = target.level();
						if (num4 * 2 / 3 >= num5)
						{
							flag = true;
							setCondition(character, target, condition, num3, nearStone: false);
							OS_Printf("死にました\n");
						}
					}
					break;
				case 4115:
					if (target.magicFlagAll() != 0)
					{
						flag = true;
						target.clearMagicFlagAll();
						target.clearConditionTimeAll();
						target.resetParameterMagicFlag();
						OS_Printf("魔法が解けました\n");
					}
					break;
				case 4118:
					if (!calcAntiOption(condition, num3))
					{
						num2 = calcCommonConditionOdds(character, 7, target, 20, 1);
						if (num2 > num)
						{
							flag = true;
							setCondition(character, target, condition, num3, nearStone: false);
							OS_Printf("死にました\n");
						}
					}
					break;
				case 4120:
				case 4207:
				case 6607:
					if (!calcAntiOption(condition, num3))
					{
						num2 = calcCommonConditionOdds(character, 7, target, 10, 1);
						if (num2 > num)
						{
							flag = true;
							setCondition(character, target, condition, num3, nearStone: false);
							OS_Printf("石になりました\n");
						}
						else if (target.changeCondition().isStone())
						{
							flag = true;
						}
					}
					break;
				case 4123:
				case 4218:
				case 4226:
				case 6615:
					if (magicId == 4123 && target.isGhost())
					{
						flag = true;
						target.clearFlag(PLAYER_FLAG.PF_2D);
						target.clearFlag(PLAYER_FLAG.PF_MISS);
						OS_Printf("アンデッドにデスをかけました\n");
						if (!flag)
						{
							return 0;
						}
						return 1;
					}
					if (!calcAntiOption(condition, num3))
					{
						num2 = calcCommonConditionOdds(character, 7, target, 10, 1);
						if (num2 > num)
						{
							flag = true;
							setCondition(character, target, condition, num3, nearStone: false);
							OS_Printf("死にました\n");
						}
						else if (target.changeCondition().isDeath())
						{
							flag = true;
						}
					}
					break;
				case 6605:
					if (!calcAntiOption(condition, num3))
					{
						num2 = calcCommonConditionOdds(character, 7, target, 10, 1);
						if (num2 > num)
						{
							flag = true;
							setCondition(character, target, condition, num3, nearStone: false);
							OS_Printf("いろいろかかりました\n");
						}
					}
					break;
				}
				if (flag)
				{
					target.setFlag(PLAYER_FLAG.PF_2D);
					target.clearFlag(PLAYER_FLAG.PF_MISS);
				}
				else
				{
					target.setFlag(PLAYER_FLAG.PF_MISS);
					target.clearFlag(PLAYER_FLAG.PF_2D);
				}
				if (!flag)
				{
					return 0;
				}
				return 1;
			}

			public int calcCommonConditionOdds(BaseBattleCharacter character, int userOffset, BaseBattleCharacter target, int targetOffset, int lowest)
			{
				CommonFormula commonFormula = new CommonFormula();
				int num = commonFormula.calcJobSkill(character);
				OS_Printf("ジョブ熟練度は\u3000%d\u3000です\n", num);
				int num2 = character.bodyAndBonus().intelligence().get();
				OS_Printf("自分の知性は\u3000%d\u3000です\n", num2);
				int num3 = target.bodyAndBonus().mind().get();
				OS_Printf("相手の精神は\u3000%d\u3000です\n", num3);
				int num4 = targetOffset + num / userOffset + (num2 - num3) / lowest;
				if (character.condition().isDarkness())
				{
					num4 /= 2;
				}
				if (OutsideToBattle.getInstance().condition())
				{
					num4 = 100;
				}
				OS_Printf("成功確率は\u3000%d\u3000です\n", num4);
				return num4;
			}

			public bool calcAntiOption(short condition, short antiOption)
			{
				bool result = false;
				for (int i = 0; i < 11; i++)
				{
					short num = (short)(1 << i);
					if ((condition & num) == 0)
					{
						result = true;
						continue;
					}
					if ((antiOption & num) != 0)
					{
						result = true;
						continue;
					}
					result = false;
					break;
				}
				return result;
			}

			public void setCondition(BaseBattleCharacter attacker, BaseBattleCharacter target, short condition, short anti, bool nearStone)
			{
				int num = attacker.bodyAndBonus().intelligence().get();
				int num2 = target.bodyAndBonus().mind().get();
				int num3 = num / num2;
				for (int i = 0; i < 11; i++)
				{
					short num4 = (short)(1 << i);
					if ((num4 & condition) == 0 || (num4 & anti) != 0)
					{
						continue;
					}
					if ((num4 & 0x100) != 0)
					{
						target.condition().onPoison();
						target.changeCondition().onPoison();
					}
					if ((num4 & 1) != 0)
					{
						if (target.condition().paralysisTime() == 0)
						{
							byte t = (byte)(2 + num3);
							t = (byte)ds.clamp(t, 0, 3);
							target.condition().setParalysisTime(t);
						}
						target.condition().onParalysis();
						target.changeCondition().onParalysis();
						target.clearFlag(PLAYER_FLAG.PF_GUARD);
						target.clearFlag(PLAYER_FLAG.PF_MORE_GUARD);
					}
					if ((num4 & 2) != 0)
					{
						if (target.condition().sleepTime() == 0)
						{
							byte t2 = (byte)(2 + num3);
							t2 = (byte)ds.clamp(t2, 0, 3);
							target.condition().setSleepTime(t2);
						}
						target.condition().onSleep();
						target.changeCondition().onSleep();
						target.clearFlag(PLAYER_FLAG.PF_GUARD);
						target.clearFlag(PLAYER_FLAG.PF_MORE_GUARD);
					}
					if ((num4 & 4) != 0)
					{
						if (target.condition().confusionTime() == 0)
						{
							byte t3 = (byte)(2 + num3);
							t3 = (byte)ds.clamp(t3, 0, 3);
							target.condition().setConfusionTime(t3);
						}
						target.condition().onConfusion();
						target.changeCondition().onConfusion();
						target.clearFlag(PLAYER_FLAG.PF_GUARD);
						target.clearFlag(PLAYER_FLAG.PF_MORE_GUARD);
					}
					if ((num4 & 8) != 0)
					{
						if (nearStone)
						{
							target.condition().onNearStone();
							target.condition().goStone();
						}
						else
						{
							target.condition().onStone();
							target.clearFlag(PLAYER_FLAG.PF_GUARD);
							target.clearFlag(PLAYER_FLAG.PF_MORE_GUARD);
						}
					}
					if ((num4 & 0x10) != 0 && !target.condition().isLilliput())
					{
						target.changeCondition().onFrog();
					}
					if ((num4 & 0x20) != 0)
					{
						target.condition().onSilence();
						target.changeCondition().onSilence();
					}
					if ((num4 & 0x40) != 0 && !target.condition().isFrog())
					{
						target.changeCondition().onLilliput();
					}
					if ((num4 & 0x80) != 0)
					{
						target.condition().onDarkness();
						target.changeCondition().onDarkness();
					}
					if ((num4 & 0x400) != 0)
					{
						target.setConditionNearDeath();
					}
					if ((num4 & 0x200) != 0)
					{
						target.setConditionDeath();
					}
				}
			}

			public int calcAttributeValue(short magicId, BaseBattleCharacter target)
			{
				short attack = itm.ItemManager.instance().magicParameter(magicId).magicType();
				short weak = target.magicDefense().weakType();
				short anti = target.physicsDefense().antiType();
				CommonFormula commonFormula = new CommonFormula();
				return commonFormula.calcAttribute(attack, weak, anti);
			}

			public int calcMagicSuccessValue(bool success)
			{
				int num = 0;
				num = (int)((!success) ? (4096 * (ds.RandomNumber.logic32(11u) + 50) / 100) : (4096 * (ds.RandomNumber.logic32(21u) + 90) / 100));
				OS_Printf("魔法の成否による補正値 %f \n", FX_FX32_TO_F32(num));
				return num;
			}

			public int calcTargetNumberValue(int targetNumber, int rate, int magic_id)
			{
				int num = 0;
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter((short)magic_id);
				if (magicParameter != null && ((magicParameter.targetPossible() & 8) != 0 || (magicParameter.targetPossible() & 0x200) != 0) && (magicParameter.targetPossible() & 1) == 0 && (magicParameter.targetPossible() & 2) == 0 && (magicParameter.targetPossible() & 4) == 0 && (magicParameter.targetPossible() & 0x10) == 0 && (magicParameter.targetPossible() & 0x20) == 0 && (magicParameter.targetPossible() & 0x40) == 0 && (magicParameter.targetPossible() & 0x80) == 0 && (magicParameter.targetPossible() & 0x100) == 0 && (magicParameter.targetPossible() & 0x400) == 0 && (magicParameter.targetPossible() & 0x800) == 0)
				{
					return 4096;
				}
				if (targetNumber == 1)
				{
					return 4096;
				}
				return 4096 * (rate - targetNumber * 10) / 100;
			}
		}
	}
}
