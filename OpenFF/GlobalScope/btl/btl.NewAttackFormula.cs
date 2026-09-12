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
		public class NewAttackFormula
		{
			public int calcDamage(BaseBattleCharacter user, BaseBattleCharacter target)
			{
				int result = 0;
				if (user.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(user);
					if (battlePlayer.player().equipParameter().isEquipBow() && battlePlayer.player().equipParameter().isEquipArrow())
					{
						OS_Printf("\n//////////////////////////////////////////////////////////////////////\n");
						OS_Printf("弓矢で攻撃します\n");
						result = calcTotalBowDamage(user, target);
						OS_Printf("\n弓矢で攻撃しました\n");
						OS_Printf("//////////////////////////////////////////////////////////////////////\n");
					}
					else if (battlePlayer.player().equipParameter().isEquipHarp())
					{
						OS_Printf("\n//////////////////////////////////////////////////////////////////////\n");
						OS_Printf("竪琴で攻撃します\n");
						result = calcTotalHarpDamage(user, target);
						OS_Printf("\n竪琴で攻撃しました\n");
						OS_Printf("//////////////////////////////////////////////////////////////////////\n");
					}
					else if (battlePlayer.player().equipParameter().isBareHands())
					{
						OS_Printf("\n//////////////////////////////////////////////////////////////////////\n");
						OS_Printf("素手で攻撃します\n");
						result = calcTotalBareHandsDamage(user, target);
						OS_Printf("\n素手で攻撃しました\n");
						OS_Printf("//////////////////////////////////////////////////////////////////////\n");
					}
					else
					{
						OS_Printf("\n//////////////////////////////////////////////////////////////////////\n");
						OS_Printf("武器で攻撃します\n");
						result = calcTotalWeaponDamage(user, target);
						OS_Printf("\n武器で攻撃しました\n");
						OS_Printf("//////////////////////////////////////////////////////////////////////\n");
					}
				}
				else if (user.breed() == 1)
				{
					OS_Printf("\n//////////////////////////////////////////////////////////////////////\n");
					OS_Printf("モンスターとかが攻撃します\n");
					result = calcTotalBareHandsDamage(user, target);
					OS_Printf("\nモンスターとかが攻撃しました\n");
					OS_Printf("//////////////////////////////////////////////////////////////////////\n");
				}
				else if (user.breed() == 2)
				{
					OS_Printf("\n//////////////////////////////////////////////////////////////////////\n");
					OS_Printf("NPCとかが攻撃します\n");
					result = calcTotalWeaponDamage(user, target);
					OS_Printf("\nNPCとかが攻撃しました\n");
					OS_Printf("//////////////////////////////////////////////////////////////////////\n");
				}
				return result;
			}

			public int calcBareHandsDamage(BaseBattleCharacter user, BaseBattleCharacter target, pl.HAND_TYPE hand)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("素手基礎攻撃ダメージ算出します\n\n");
				CommonFormula commonFormula = new CommonFormula();
				int num = commonFormula.calcJobSkill(user);
				if (user.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(user);
					pl.JOB_TYPE jOB_TYPE = static_cast<pl.JOB_TYPE>(battlePlayer.player().jobManager().nowJob());
					// PORT: FF5's Barehanded (a mastery passive) lets any job strike unarmed as a Monk does.
					if (jOB_TYPE != pl.JOB_TYPE.MONK && jOB_TYPE != pl.JOB_TYPE.KARATE_MASTER && !OpenFF.Client.ProgressionLayer.HasInBattle(battlePlayer, OpenFF.Data.Ff3Abilities.Barehanded))
					{
						num = 1;
					}
				}
				OS_Printf("熟練度ボーナス %d\n", num);
				int num2 = user.bodyAndBonus().strength().get();
				OS_Printf("自分のSTR %d\n", num2);
				int num3 = (target.physicsDefense().phylacticPower().get() + target.bodyAndBonus().vitality().get()) / 2;
				if (target.flag(PLAYER_FLAG.PF_PROVOCATION) && target.provocationCharacter() != null)
				{
					int num4 = commonFormula.calcJobSkill(target.provocationCharacter());
					num3 -= num3 * num4 / 2 / 100;
				}
				OS_Printf("相手のVIT %d\n", num3);
				int num5 = commonFormula.calcHandSkill(user, hand);
				OS_Printf("腕の熟練度 %d\n", num5);
				int num6 = user.level();
				OS_Printf("自分のレベル %d\n", num6);
				int num7 = offenseAndDefense(user, target);
				OS_Printf("攻防比 %d\n", num7 / 4096);
				OS_Printf("攻防比 %d\n", num7);
				OS_Printf("攻防比 %f\n", num7 / 4096);
				short num8 = user.handAttack(hand).attackType();
				if (user.breed() == 0)
				{
					num8 = 0;
				}
				if (user.flag(PLAYER_FLAG.PF_JUMP))
				{
					num8 |= 0x200;
				}
				int num9 = commonFormula.calcAttribute(num8, target.magicDefense().weakType(), target.physicsDefense().antiType());
				OS_Printf("魔法属性効果 %d\n", num9);
				int num10 = num + num2;
				if (user.magicFlag(MAGIC_FLAG.MF_BAHAMUT))
				{
					num10 = num10 * 150 / 100;
				}
				int t = 0;
				if (user.breed() == 0)
				{
					t = (num10 - num3 + num5 + num6 / 9) * num7 / 4096;
					t = ((num9 != 0) ? (t * num9) : (t / 2));
				}
				else if (user.breed() == 1)
				{
					t = num10 - num3 + num5 + num6 / 9 * num7 / 4096;
					t = ((num9 != 0) ? (t * num9) : (t / 2));
				}
				return ds.max(t, 0);
			}

			public int calcBareHandsAttackNumber(BaseBattleCharacter user)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("素手攻撃回数算出します\n\n");
				CommonFormula commonFormula = new CommonFormula();
				int num = commonFormula.calcHandSkill(user, pl.HAND_TYPE.RIGHT_HAND);
				int num2 = commonFormula.calcHandSkill(user, pl.HAND_TYPE.LEFT_HAND);
				OS_Printf("右腕の熟練度 %d\n", num);
				OS_Printf("左腕の熟練度 %d\n", num2);
				int num3 = user.bodyAndBonus().dexterity().get();
				OS_Printf("自分のDEX %d\n", num3);
				int num4 = commonFormula.calcJobSkill(user);
				if (user.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(user);
					pl.JOB_TYPE jOB_TYPE = static_cast<pl.JOB_TYPE>(battlePlayer.player().jobManager().nowJob());
					if (jOB_TYPE != pl.JOB_TYPE.MONK && jOB_TYPE != pl.JOB_TYPE.KARATE_MASTER)
					{
						num4 = 1;
					}
				}
				OS_Printf("ジョブ熟練度 %d\n", num4);
				int num5 = commonFormula.calcWeight(user);
				OS_Printf("装備重量 %d\n", num5);
				int t = 1 + ((num - 1) / 16 + (num2 - 1)) / 16 + (num3 / 4 + num4 / 4 - num5 / 4);
				t = ds.clamp(t, 0, 99);
				if (user.breed() == 0)
				{
					BattlePlayer battlePlayer2 = static_cast<BattlePlayer>(user);
					int num6 = t;
					num6 = ds.clamp(t, 0, 16);
					battlePlayer2.setAttackMotionNumber(num6, 0);
					battlePlayer2.setAttackMotionNumber(num6, 1);
					if (t > 0)
					{
						battlePlayer2.player().skillManager().skill(pl.GET_SKILL_TYPE.GET_RIGHT_HAND)
							.addPoolSkillExp(3);
						battlePlayer2.player().skillManager().skill(pl.GET_SKILL_TYPE.GET_LEFT_HAND)
							.addPoolSkillExp(3);
					}
				}
				return t;
			}

			public int calcBareHandsRollNumber(BaseBattleCharacter character, BaseBattleCharacter target, int attackNumber, int defenseNumber)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("攻撃ロール回数算出します\n\n");
				return 1 + attackNumber - defenseNumber;
			}

			public int calcBareHandsAttackSuccessNumber(BaseBattleCharacter user, BaseBattleCharacter target, int attackNumber, int defenseNumber)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("素手攻撃成功回数算出します\n\n");
				int num = calcHit(user);
				OS_Printf("自分の命中力 %d\n", num);
				int num2 = calcAvoidance(target);
				OS_Printf("相手の回避力 %d\n", num2);
				int t = 1 + attackNumber - defenseNumber;
				t = ds.max(t, 0);
				return 2 + t * (num / num2) / 4;
			}

			public int calcTotalBareHandsDamage(BaseBattleCharacter user, BaseBattleCharacter target)
			{
				int num = calcBareHandsDamage(user, target, pl.HAND_TYPE.RIGHT_HAND);
				OS_Printf("GET! 右手素手ダメージ %d\n", num);
				int num2 = calcBareHandsDamage(user, target, pl.HAND_TYPE.LEFT_HAND);
				OS_Printf("GET! 左手素手ダメージ %d\n", num2);
				int num3 = calcWeaponAttackNumber(user, pl.HAND_TYPE.RIGHT_HAND);
				OS_Printf("GET! 右手攻撃回数 %d\n", num3);
				int num4 = calcWeaponAttackNumber(user, pl.HAND_TYPE.LEFT_HAND);
				OS_Printf("GET! 左手攻撃回数 %d\n", num4);
				if (num3 + num4 == 0)
				{
					user.setAttackNumber(0);
					target.setFlag(PLAYER_FLAG.PF_MISS);
					OS_Printf("\nおまえはミスった\n");
					return 0;
				}
				int num5 = calcWeaponHitOdds(user, target, pl.HAND_TYPE.RIGHT_HAND);
				OS_Printf("GET! 右手攻撃命中率 %d\n", num5);
				int num6 = calcWeaponHitOdds(user, target, pl.HAND_TYPE.LEFT_HAND);
				OS_Printf("GET! 左手攻撃命中率 %d\n", num6);
				int t = calcWeaponAttackSuccessNumber(num3, num5);
				t = ds.clamp(t, 0, 99);
				if (t > 0 && !user.flag(PLAYER_FLAG.PF_DARK))
				{
					addCondition(user, target, pl.HAND_TYPE.RIGHT_HAND);
				}
				OS_Printf("GET! 右手攻撃成功回数 %d\n", t);
				int t2 = calcWeaponAttackSuccessNumber(num4, num6);
				t2 = ds.clamp(t2, 0, 99);
				if (t2 > 0 && !user.flag(PLAYER_FLAG.PF_DARK))
				{
					addCondition(user, target, pl.HAND_TYPE.LEFT_HAND);
				}
				OS_Printf("GET! 左手攻撃成功回数 %d\n", t2);
				int num7 = t + t2;
				if (num7 == 0)
				{
					user.setAttackNumber(0);
					target.setFlag(PLAYER_FLAG.PF_MISS);
					OS_Printf("\nおまえはミスった\n");
					return 0;
				}
				if (t > 0)
				{
					user.setAttackSuccess(0, flag: true);
				}
				if (t2 > 0)
				{
					user.setAttackSuccess(1, flag: true);
				}
				if (user.breed() == 0 || user.breed() == 2)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(user);
					battlePlayer.setEffectNumber(t, 0);
					battlePlayer.setEffectNumber(t2, 1);
				}
				int attackNumber = ds.clamp(num7, 0, 32);
				user.setAttackNumber(attackNumber);
				if (user.breed() == 0)
				{
					pl.PlayerParty.instance().mania().setMaxHitNumber(user.attackNumber());
				}
				if (calcCritical(user, target))
				{
					user.setFlag(PLAYER_FLAG.PF_CRITICAL);
					OS_Printf("GET! クリティカル! \n");
				}
				int num8 = num + num2;
				float num9 = (float)(ds.RandomNumber.rand32(3u) + 5) / 10f;
				int num10 = (int)((float)num8 * num9 * (float)num7);
				if (user.flag(PLAYER_FLAG.PF_CRITICAL))
				{
					num10 *= 160;
					num10 /= 100;
				}
				if (user.breed() == 0)
				{
					BattlePlayer battlePlayer2 = static_cast<BattlePlayer>(user);
					if (battlePlayer2 != null && battlePlayer2.player() != null && battlePlayer2.player().formationType() == 1)
					{
						num10 /= 2;
					}
				}
				num10 = ds.clamp(num10, 1, 99999);
				if (num7 == 1)
				{
					num10 *= 150;
					num10 /= 100;
				}
				OS_Printf("GET! ダメージ %d\n", num10);
				return num10;
			}

			public int calcWeaponDamage(BaseBattleCharacter character, BaseBattleCharacter target, pl.HAND_TYPE handType)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				CommonFormula commonFormula = new CommonFormula();
				if (character.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(character);
					if (battlePlayer.player().equipParameter().isEquipWeapon() == 1)
					{
						pl.HAND_TYPE hAND_TYPE = battlePlayer.player().equipParameter().checkEquipWeaponHand();
						if (hAND_TYPE != handType)
						{
							battlePlayer.setAttackMotionNumber(-1, (int)handType);
							OS_Printf("装備してないよ\n");
							return 0;
						}
					}
				}
				else if (character.breed() == 2)
				{
					BattlePlayer battlePlayer2 = static_cast<BattlePlayer>(character);
					if (battlePlayer2.weaponId(handType) < 0)
					{
						battlePlayer2.setAttackMotionNumber(-1, (int)handType);
						OS_Printf("装備してないよ\n");
						return 0;
					}
				}
				int num = character.handAttack(handType).aggressivity().get();
				OS_Printf("武器攻撃力 %d\n", num);
				int num2 = character.bodyAndBonus().strength().get();
				OS_Printf("自分のSTR %d\n", num2);
				int num3 = (target.physicsDefense().phylacticPower().get() + target.bodyAndBonus().vitality().get()) / 2;
				if (target.flag(PLAYER_FLAG.PF_PROVOCATION) && target.provocationCharacter() != null)
				{
					int num4 = commonFormula.calcJobSkill(target.provocationCharacter());
					num3 -= num3 * num4 / 2 / 100;
				}
				OS_Printf("相手のVIT %d\n", num3);
				int num5 = commonFormula.calcHandSkill(character, handType);
				int num6 = commonFormula.calcJobSkill(character);
				OS_Printf("ジョブ熟練度 %d\n", num6);
				int num7 = offenseAndDefense(character, target);
				OS_Printf("攻防比 %d\n", num7 / 4096);
				OS_Printf("攻防比 %d\n", num7);
				OS_Printf("攻防比 %f\n", num7 / 4096);
				int num8 = commonFormula.calcAttribute(character.handAttack(handType).armsAttribute(), target.physicsDefense().armsWeakAttribute(), target.physicsDefense().armsAttribute());
				OS_Printf("物理属性効果 %d\n", num8);
				short num9 = character.handAttack(handType).attackType();
				if (character.flag(PLAYER_FLAG.PF_JUMP))
				{
					num9 |= 0x200;
				}
				int num10 = commonFormula.calcAttribute(num9, target.magicDefense().weakType(), target.physicsDefense().antiType());
				OS_Printf("魔法属性効果 %d\n", num10);
				int num11 = 0;
				num11 = (num + num2 - num3 + num5 / 9 + num6 / 11) * num7 / 4096;
				num11 = ((num10 != 0) ? (num11 * num10) : (num11 / 2));
				num11 = ((num8 != 0) ? (num11 * num8) : (num11 / 2));
				if (character.breed() == 0)
				{
					BattlePlayer battlePlayer3 = static_cast<BattlePlayer>(character);
					if (battlePlayer3 != null && battlePlayer3.player() != null && !battlePlayer3.player().equipParameter().equipHand(handType)
						.isEquipPitch() && battlePlayer3.player().formationType() == 1)
					{
						num11 /= 2;
					}
				}
				return ds.max(num11, 0);
			}

			public int calcWeaponAttackNumber(BaseBattleCharacter character, pl.HAND_TYPE handType)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				if (character.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(character);
					if (battlePlayer.player().equipParameter().isEquipWeapon() == 1)
					{
						pl.HAND_TYPE hAND_TYPE = battlePlayer.player().equipParameter().checkEquipWeaponHand();
						if (hAND_TYPE != handType)
						{
							battlePlayer.setAttackMotionNumber(-1, (int)handType);
							OS_Printf("装備してないよ\n");
							return 0;
						}
					}
				}
				else if (character.breed() == 2)
				{
					BattlePlayer battlePlayer2 = static_cast<BattlePlayer>(character);
					if (battlePlayer2.weaponId(handType) < 0)
					{
						battlePlayer2.setAttackMotionNumber(-1, (int)handType);
						OS_Printf("装備してないよ\n");
						return 0;
					}
				}
				CommonFormula commonFormula = new CommonFormula();
				int num = commonFormula.calcHandSkill(character, handType);
				OS_Printf("腕の熟練度 %d\n", num);
				int num2 = character.bodyAndBonus().dexterity().get();
				OS_Printf("自分のDEX %d\n", num2);
				int num3 = commonFormula.calcJobSkill(character);
				OS_Printf("ジョブ熟練度 %d\n", num3);
				int num4 = commonFormula.calcWeight(character);
				OS_Printf("装備重量 %d\n", num4);
				int t = 1 + (num - 1) / 7 + num2 / 7 + num3 / 14 - num4 / 6;
				t = ds.clamp(t, 0, 99);
				if (character.breed() == 0 || character.breed() == 2)
				{
					BattlePlayer battlePlayer3 = static_cast<BattlePlayer>(character);
					int number = ds.clamp(t, 0, 16);
					if (character.breed() == 0)
					{
						if (!battlePlayer3.player().equipParameter().equipHand(handType)
							.isEquipPitch())
						{
							battlePlayer3.setAttackMotionNumber(number, (int)handType);
						}
						else
						{
							battlePlayer3.setAttackMotionNumber(1, (int)handType);
						}
						if (t > 0)
						{
							battlePlayer3.player().skillManager().skill(static_cast<pl.GET_SKILL_TYPE>(handType))
								.addPoolSkillExp(3);
						}
					}
					else if (character.breed() == 2)
					{
						battlePlayer3.setAttackMotionNumber(number, (int)handType);
					}
				}
				return t;
			}

			public int calcWeaponRollNumber(BaseBattleCharacter character, BaseBattleCharacter target, int rightNumber, int leftNumber, int defenseNumber)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("攻撃ロール回数算出します\n\n");
				int t = 1 + (rightNumber + leftNumber - defenseNumber);
				return ds.max(t, 0);
			}

			public int calcWeaponHitOdds(BaseBattleCharacter character, BaseBattleCharacter target, pl.HAND_TYPE hand)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("攻撃命中率算出します\n\n");
				int num = character.bodyAndBonus().dexterity().get();
				OS_Printf("自分のDEX %d\n", num);
				CommonFormula commonFormula = new CommonFormula();
				int num2 = commonFormula.calcJobSkill(character);
				OS_Printf("ジョブ熟練度 %d\n", num2);
				int num3 = commonFormula.calcWeight(character);
				OS_Printf("装備重量 %d\n", num3);
				int num4 = target.bodyAndBonus().dexterity().get();
				OS_Printf("相手のAGL %d\n", num4);
				int num5 = commonFormula.calcWeight(target);
				OS_Printf("相手の重量 %d\n", num5);
				int num6 = 80 + (num / 10 + num2 / 10 - num4 / 20 - num3 / 6);
				int num7 = character.handAttack(hand).hitProbability();
				OS_Printf("武器命中率 %d\n", num7);
				num6 += num7 / 2;
				if (character.condition().isDarkness())
				{
					num6 /= 2;
				}
				return ds.clamp(num6, 1, 95);
			}

			public int calcWeaponAttackSuccessNumber(int rollNumber, int hitOdds)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("攻撃成功回数算出します\n\n");
				if (rollNumber == 1)
				{
					hitOdds = 95;
				}
				int num = 0;
				for (int i = 0; i < rollNumber; i++)
				{
					int num2 = (int)ds.RandomNumber.rand32(101u);
					OS_Printf("攻撃命中率は %2d\u3000\u3000ランダム値は %2d\u3000です\n", hitOdds, num2);
					if (num2 < hitOdds)
					{
						num++;
					}
				}
				return num;
			}

			public int calcTotalWeaponDamage(BaseBattleCharacter character, BaseBattleCharacter target)
			{
				int num = calcWeaponDamage(character, target, pl.HAND_TYPE.RIGHT_HAND);
				OS_Printf("GET! 右手ダメージ %d\n", num);
				int num2 = calcWeaponDamage(character, target, pl.HAND_TYPE.LEFT_HAND);
				OS_Printf("GET! 左手ダメージ %d\n", num2);
				int num3 = calcWeaponAttackNumber(character, pl.HAND_TYPE.RIGHT_HAND);
				OS_Printf("GET! 右手攻撃回数 %d\n", num3);
				int num4 = calcWeaponAttackNumber(character, pl.HAND_TYPE.LEFT_HAND);
				OS_Printf("GET! 左手攻撃回数 %d\n", num4);
				if (num3 == 0 && num4 == 0)
				{
					character.setAttackNumber(0);
					target.setFlag(PLAYER_FLAG.PF_MISS);
					OS_Printf("\nおまえはミスった\n");
					return 0;
				}
				int num5 = calcWeaponHitOdds(character, target, pl.HAND_TYPE.RIGHT_HAND);
				OS_Printf("GET! 右手攻撃命中率 %d\n", num5);
				int num6 = calcWeaponHitOdds(character, target, pl.HAND_TYPE.LEFT_HAND);
				OS_Printf("GET! 左手攻撃命中率 %d\n", num6);
				int t = calcWeaponAttackSuccessNumber(num3, num5);
				t = ds.clamp(t, 0, 99);
				if (t > 0 && !character.flag(PLAYER_FLAG.PF_DARK))
				{
					addCondition(character, target, pl.HAND_TYPE.RIGHT_HAND);
				}
				OS_Printf("GET! 右手攻撃成功回数 %d\n", t);
				int t2 = calcWeaponAttackSuccessNumber(num4, num6);
				t2 = ds.clamp(t2, 0, 99);
				if (t2 > 0 && !character.flag(PLAYER_FLAG.PF_DARK))
				{
					addCondition(character, target, pl.HAND_TYPE.LEFT_HAND);
				}
				OS_Printf("GET! 左手攻撃成功回数 %d\n", t2);
				int num7 = t + t2;
				if (num7 == 0)
				{
					character.setAttackNumber(0);
					target.setFlag(PLAYER_FLAG.PF_MISS);
					OS_Printf("\nおまえはミスった\n");
					return 0;
				}
				if (t > 0)
				{
					character.setAttackSuccess(0, flag: true);
				}
				if (t2 > 0)
				{
					character.setAttackSuccess(1, flag: true);
				}
				if (character.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(character);
					if (battlePlayer.player().equipParameter().isEquipWeapon() == 2)
					{
						int attackNumber = ds.clamp(num7, 0, 32);
						character.setAttackNumber(attackNumber);
					}
					else
					{
						int attackNumber2 = ds.clamp(num7, 0, 16);
						character.setAttackNumber(attackNumber2);
					}
				}
				else if (character.breed() == 2)
				{
					int attackNumber3 = ds.clamp(num7, 0, 16);
					character.setAttackNumber(attackNumber3);
				}
				if (character.breed() == 0)
				{
					pl.PlayerParty.instance().mania().setMaxHitNumber(character.attackNumber());
				}
				if (character.breed() == 0)
				{
					BattlePlayer battlePlayer2 = static_cast<BattlePlayer>(character);
					if (!battlePlayer2.player().equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND)
						.isEquipPitch())
					{
						battlePlayer2.setEffectNumber(t, 0);
					}
					else
					{
						battlePlayer2.setEffectNumber(1, 0);
					}
					if (!battlePlayer2.player().equipParameter().equipHand(pl.HAND_TYPE.LEFT_HAND)
						.isEquipPitch())
					{
						battlePlayer2.setEffectNumber(t2, 1);
					}
					else
					{
						battlePlayer2.setEffectNumber(1, 1);
					}
				}
				else if (character.breed() == 2)
				{
					BattlePlayer battlePlayer3 = static_cast<BattlePlayer>(character);
					battlePlayer3.setEffectNumber(t, 0);
					battlePlayer3.setEffectNumber(t2, 1);
				}
				if (calcCritical(character, target))
				{
					character.setFlag(PLAYER_FLAG.PF_CRITICAL);
					OS_Printf("GET! クリティカル! \n");
				}
				int num8 = num + num2;
				float num9 = (float)(ds.RandomNumber.rand32(3u) + 5) / 10f;
				int num10 = (int)((float)num8 * num9 * (float)num7);
				if (character.flag(PLAYER_FLAG.PF_CRITICAL))
				{
					num10 *= 160;
					num10 /= 100;
				}
				num10 = ds.clamp(num10, 1, 99999);
				if (num7 == 1)
				{
					OS_Printf("1HIT時 補正前 武器ダメージ %d\n", num10);
					num10 *= 150;
					num10 /= 100;
					OS_Printf("1HIT時 補正後 武器ダメージ %d\n", num10);
				}
				if (character.breed() == 0)
				{
					BattlePlayer battlePlayer4 = static_cast<BattlePlayer>(character);
					if (battlePlayer4.player().equipParameter().isEquipWeapon() == 2)
					{
						OS_Printf("二刀流補正前 %d\n", num10);
						num10 *= 60;
						num10 /= 100;
						OS_Printf("二刀流補正後 %d\n", num10);
					}
				}
				OS_Printf("GET! 武器ダメージ %d\n", num10);
				return num10;
			}

			public int calcBowDamage(BaseBattleCharacter attacker, BaseBattleCharacter target)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("弓矢基礎攻撃ダメージ算出します\n\n");
				CommonFormula commonFormula = new CommonFormula();
				int num = attacker.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
				OS_Printf("右手攻撃力 %d\n", num);
				int num2 = attacker.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get();
				OS_Printf("左手攻撃力 %d\n", num2);
				int num3 = attacker.bodyAndBonus().strength().get();
				OS_Printf("自分のSTR %d\n", num3);
				int num4 = (target.physicsDefense().phylacticPower().get() + target.bodyAndBonus().vitality().get()) / 2;
				if (target.flag(PLAYER_FLAG.PF_PROVOCATION) && target.provocationCharacter() != null)
				{
					int num5 = commonFormula.calcJobSkill(target.provocationCharacter());
					num4 -= num4 * num5 / 2 / 100;
				}
				OS_Printf("相手のVIT %d\n", num4);
				int num6 = commonFormula.calcHandSkill(attacker, pl.HAND_TYPE.RIGHT_HAND);
				OS_Printf("右手熟練度 %d\n", num6);
				int num7 = commonFormula.calcHandSkill(attacker, pl.HAND_TYPE.LEFT_HAND);
				OS_Printf("左手熟練度 %d\n", num7);
				int num8 = offenseAndDefense(attacker, target);
				OS_Printf("攻防比 %d\n", num8 / 4096);
				OS_Printf("攻防比 %d\n", num8);
				OS_Printf("攻防比 %f\n", num8 / 4096);
				short num9 = 0;
				num9 |= attacker.handAttack(pl.HAND_TYPE.RIGHT_HAND).armsAttribute();
				num9 |= attacker.handAttack(pl.HAND_TYPE.LEFT_HAND).armsAttribute();
				int num10 = commonFormula.calcAttribute(num9, target.physicsDefense().armsWeakAttribute(), target.physicsDefense().armsAttribute());
				OS_Printf("物理属性効果 %d\n", num10);
				short num11 = 0;
				num11 |= attacker.handAttack(pl.HAND_TYPE.RIGHT_HAND).attackType();
				num11 |= attacker.handAttack(pl.HAND_TYPE.LEFT_HAND).attackType();
				int num12 = commonFormula.calcAttribute(num11, target.magicDefense().weakType(), target.physicsDefense().antiType());
				OS_Printf("魔法属性効果 %d\n", num12);
				int num13 = (num + num2 + num3 - num4 + (num6 + num7) / 2) * num8 / 4096;
				num13 = ((num12 != 0) ? (num13 * num12) : (num13 / 2));
				num13 = ((num10 != 0) ? (num13 * num10) : (num13 / 2));
				if (num13 <= 0)
				{
					num13 = 0;
				}
				return num13;
			}

			public int calcBowAttackNumber(BaseBattleCharacter attacker)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("弓矢攻撃回数算出\n\n");
				CommonFormula commonFormula = new CommonFormula();
				int num = commonFormula.calcHandSkill(attacker, pl.HAND_TYPE.RIGHT_HAND);
				OS_Printf("右手熟練度 %d\n", num);
				int num2 = commonFormula.calcHandSkill(attacker, pl.HAND_TYPE.LEFT_HAND);
				OS_Printf("左手熟練度 %d\n", num2);
				int num3 = attacker.bodyAndBonus().dexterity().get();
				OS_Printf("自分のDEX %d\n", num3);
				int num4 = commonFormula.calcJobSkill(attacker);
				OS_Printf("ジョブ熟練度 %d\n", num4);
				int num5 = commonFormula.calcWeight(attacker);
				OS_Printf("装備重量 %d\n", num5);
				int t = 1 + (num + num2 - 2) / 14 + num3 / 9 + num4 / 14 - num5 / 6;
				t = ds.clamp(t, 0, 99);
				if (attacker.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(attacker);
					if (battlePlayer.player().equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND)
						.isEquipArrow())
					{
						battlePlayer.setAttackMotionNumber(t, 0);
						battlePlayer.setAttackMotionNumber(-1, 1);
					}
					else
					{
						battlePlayer.setAttackMotionNumber(-1, 0);
						battlePlayer.setAttackMotionNumber(t, 1);
					}
					if (attacker.breed() == 0 && t > 0)
					{
						battlePlayer.player().skillManager().skill(static_cast<pl.GET_SKILL_TYPE>(pl.HAND_TYPE.RIGHT_HAND))
							.addPoolSkillExp(3);
						battlePlayer.player().skillManager().skill(static_cast<pl.GET_SKILL_TYPE>(pl.HAND_TYPE.LEFT_HAND))
							.addPoolSkillExp(3);
					}
				}
				return t;
			}

			public int calcBowRollNumber(BaseBattleCharacter attacker, BaseBattleCharacter target, int attackNumber, int defenseNumber)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("攻撃ロール回数算出します\n\n");
				int t = 1 + (attackNumber * 2 - defenseNumber);
				return ds.max(t, 0);
			}

			public int calcBowHitOdds(BaseBattleCharacter attacker, BaseBattleCharacter target)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("攻撃命中率算出します\n\n");
				int num = attacker.bodyAndBonus().dexterity().get();
				OS_Printf("自分のDEX %d\n", num);
				CommonFormula commonFormula = new CommonFormula();
				int num2 = commonFormula.calcJobSkill(attacker);
				int num3 = commonFormula.calcWeight(attacker);
				OS_Printf("装備重量 %d\n", num3);
				int num4 = target.bodyAndBonus().dexterity().get();
				OS_Printf("相手のAGL %d\n", num4);
				int num5 = commonFormula.calcWeight(target);
				OS_Printf("相手の重量 %d\n", num5);
				int num6 = 80 + num / 10 + num2 / 10 - num3 / 6 - num4 / 20;
				int num7 = attacker.handAttack(pl.HAND_TYPE.RIGHT_HAND).hitProbability();
				int num8 = attacker.handAttack(pl.HAND_TYPE.LEFT_HAND).hitProbability();
				num6 = (num6 + (num7 + num8) / 2) / 2;
				if (attacker.condition().isDarkness())
				{
					num6 /= 2;
				}
				return ds.clamp(num6, 1, 95);
			}

			public int calcBowAttackSuccessNumber(int rollNumber, int hitOdds)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("攻撃成功回数算出します\n\n");
				int num = 0;
				for (int i = 0; i < rollNumber; i++)
				{
					int num2 = (int)ds.RandomNumber.rand32(101u);
					OS_Printf("攻撃命中率は %2d\u3000\u3000ランダム値は %2d\u3000です\n", hitOdds, num2);
					if (num2 < hitOdds)
					{
						num++;
					}
				}
				return num;
			}

			public int calcTotalBowDamage(BaseBattleCharacter attacker, BaseBattleCharacter target)
			{
				int num = calcBowDamage(attacker, target);
				OS_Printf("GET! 弓矢基礎攻撃 %d\n", num);
				int num2 = calcBowAttackNumber(attacker);
				OS_Printf("GET! 攻撃回数 %d\n", num2);
				if (num2 == 0)
				{
					attacker.setAttackNumber(0);
					target.setFlag(PLAYER_FLAG.PF_MISS);
					OS_Printf("\nおまえはミスった\n");
					return 0;
				}
				int num3 = calcWeaponHitOdds(attacker, target, pl.HAND_TYPE.RIGHT_HAND);
				OS_Printf("GET! 右手攻撃命中率 %d\n", num3);
				int num4 = calcWeaponHitOdds(attacker, target, pl.HAND_TYPE.LEFT_HAND);
				OS_Printf("GET! 左手攻撃命中率 %d\n", num4);
				int num5 = calcWeaponAttackSuccessNumber(num2, num3);
				int num6 = calcWeaponAttackSuccessNumber(num2, num4);
				int t = num5 + num6;
				t = ds.clamp(t, 0, 99);
				OS_Printf("GET! 攻撃成功回数 %d\n", t);
				if (t == 0)
				{
					attacker.setAttackNumber(0);
					target.setFlag(PLAYER_FLAG.PF_MISS);
					OS_Printf("\nおまえはミスった\n");
					return 0;
				}
				int num7 = ds.clamp(t, 0, 32);
				attacker.setAttackNumber(num7);
				if (attacker.breed() == 0)
				{
					pl.PlayerParty.instance().mania().setMaxHitNumber(attacker.attackNumber());
				}
				if (attacker.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(attacker);
					if (battlePlayer.player().equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND)
						.isEquipArrow())
					{
						battlePlayer.setEffectNumber(num7, 0);
						battlePlayer.setEffectNumber(-1, 1);
						attacker.setAttackSuccess(0, flag: true);
						attacker.setAttackSuccess(1, flag: false);
					}
					else
					{
						battlePlayer.setEffectNumber(-1, 0);
						battlePlayer.setEffectNumber(num7, 1);
						attacker.setAttackSuccess(0, flag: false);
						attacker.setAttackSuccess(1, flag: true);
					}
				}
				addCondition(attacker, target, pl.HAND_TYPE.RIGHT_HAND);
				addCondition(attacker, target, pl.HAND_TYPE.LEFT_HAND);
				if (calcCritical(attacker, target))
				{
					attacker.setFlag(PLAYER_FLAG.PF_CRITICAL);
					OS_Printf("GET! クリティカル! \n");
				}
				float num8 = (float)(ds.RandomNumber.rand32(3u) + 5) / 10f;
				int num9 = (int)((float)num * num8 * (float)t);
				if (attacker.flag(PLAYER_FLAG.PF_CRITICAL))
				{
					num9 *= 160;
					num9 /= 100;
				}
				num9 = ds.clamp(num9, 1, 99999);
				if (t == 1)
				{
					OS_Printf("1HIT時 補正前 武器ダメージ %d\n", num9);
					num9 *= 150;
					num9 /= 100;
					OS_Printf("1HIT時 補正後 武器ダメージ %d\n", num9);
				}
				OS_Printf("GET! 武器ダメージ %d\n", num9);
				return num9;
			}

			public int calcHarpDamage(BaseBattleCharacter attacker, BaseBattleCharacter target)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("竪琴基礎攻撃ダメージ算出します\n\n");
				int num = attacker.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get() + attacker.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get();
				OS_Printf("武器攻撃力 %d\n", num);
				CommonFormula commonFormula = new CommonFormula();
				int num2 = attacker.bodyAndBonus().mind().get();
				OS_Printf("自分の精神 %d\n", num2);
				int num3 = target.bodyAndBonus().mind().get();
				OS_Printf("相手の精神 %d\n", num3);
				int num4 = target.magicDefense().magicPhylacticPower();
				short num5 = 0;
				num5 |= attacker.handAttack(pl.HAND_TYPE.RIGHT_HAND).attackType();
				num5 |= attacker.handAttack(pl.HAND_TYPE.LEFT_HAND).attackType();
				int num6 = commonFormula.calcAttribute(num5, target.magicDefense().weakType(), target.physicsDefense().antiType());
				OS_Printf("魔法属性効果 %d\n", num6);
				int num7 = num + num2 - num3 - num4;
				num7 = ((num6 != 0) ? (num7 * num6) : (num7 / 2));
				if (num7 <= 0)
				{
					num7 = 0;
				}
				return num7;
			}

			public int calcHarpAttackNumber(BaseBattleCharacter attacker)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("攻撃回数算出します\n\n");
				CommonFormula commonFormula = new CommonFormula();
				int num = commonFormula.calcHandSkill(attacker, pl.HAND_TYPE.RIGHT_HAND);
				int num2 = commonFormula.calcHandSkill(attacker, pl.HAND_TYPE.LEFT_HAND);
				OS_Printf("右腕の熟練度 %d\n", num);
				OS_Printf("左腕の熟練度 %d\n", num2);
				int num3 = attacker.bodyAndBonus().dexterity().get();
				OS_Printf("自分DEX %d\n", num3);
				int num4 = commonFormula.calcJobSkill(attacker);
				if (attacker.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(attacker);
					pl.JOB_TYPE jOB_TYPE = static_cast<pl.JOB_TYPE>(battlePlayer.player().jobManager().nowJob());
					if (jOB_TYPE != pl.JOB_TYPE.BARD)
					{
						num4 = 1;
					}
				}
				OS_Printf("ジョブ熟練度 %d\n", num4);
				int t = 1 + (num - 1 + num2 - 1) / 14 + (num3 / 6 + num4 / 22);
				t = ds.clamp(t, 0, 99);
				if (attacker.breed() == 0 || attacker.breed() == 2)
				{
					BattlePlayer battlePlayer2 = static_cast<BattlePlayer>(attacker);
					if (battlePlayer2.player().equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND)
						.isEquipHarp())
					{
						battlePlayer2.setAttackMotionNumber(1, 0);
						battlePlayer2.setAttackMotionNumber(-1, 1);
					}
					else
					{
						battlePlayer2.setAttackMotionNumber(-1, 0);
						battlePlayer2.setAttackMotionNumber(1, 1);
					}
					if (attacker.breed() == 0 && t > 0)
					{
						battlePlayer2.player().skillManager().skill(static_cast<pl.GET_SKILL_TYPE>(pl.HAND_TYPE.RIGHT_HAND))
							.addPoolSkillExp(3);
						battlePlayer2.player().skillManager().skill(static_cast<pl.GET_SKILL_TYPE>(pl.HAND_TYPE.LEFT_HAND))
							.addPoolSkillExp(3);
					}
				}
				return t;
			}

			public int calcHarpHitOdds(int myMND, int targetMND)
			{
				int t = 40 + myMND - targetMND;
				return ds.clamp(t, 1, 95);
			}

			public int calcHarpAttackSuccessNumber(int number, int hit)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("攻撃成功回数算出します\n\n");
				int num = 0;
				for (int i = 0; i < number; i++)
				{
					int num2 = (int)ds.RandomNumber.rand32(101u);
					OS_Printf("攻撃命中率は %2d\u3000\u3000ランダム値は %2d\u3000です\n", hit, num2);
					if (num2 < hit)
					{
						num++;
					}
				}
				return num;
			}

			public int calcTotalHarpDamage(BaseBattleCharacter attacker, BaseBattleCharacter target)
			{
				int num = calcHarpDamage(attacker, target);
				OS_Printf("GET! 基礎ダメージ %d\n", num);
				int num2 = calcHarpAttackNumber(attacker);
				OS_Printf("GET! 攻撃回数 %d\n", num2);
				if (num2 == 0)
				{
					attacker.setAttackNumber(0);
					target.setFlag(PLAYER_FLAG.PF_MISS);
					OS_Printf("\nおまえはミスった\n");
					return 0;
				}
				int hit = calcHarpHitOdds(attacker.bodyAndBonus().mind().get(), target.bodyAndBonus().mind().get());
				int t = calcHarpAttackSuccessNumber(num2, hit);
				t = ds.clamp(t, 0, 99);
				OS_Printf("GET! 攻撃成功回数 %d\n", t);
				if (t == 0)
				{
					attacker.setAttackNumber(0);
					target.setFlag(PLAYER_FLAG.PF_MISS);
					OS_Printf("\nおまえはミスった\n");
					return 0;
				}
				int attackNumber = ds.clamp(t, 0, 32);
				attacker.setAttackNumber(attackNumber);
				if (attacker.breed() == 0)
				{
					pl.PlayerParty.instance().mania().setMaxHitNumber(attacker.attackNumber());
				}
				if (attacker.breed() == 0 || attacker.breed() == 2)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(attacker);
					if (battlePlayer.player().equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND)
						.isEquipHarp())
					{
						battlePlayer.setEffectNumber(1, 0);
						battlePlayer.setEffectNumber(-1, 1);
					}
					else
					{
						battlePlayer.setEffectNumber(-1, 0);
						battlePlayer.setEffectNumber(1, 1);
					}
				}
				addCondition(attacker, target, pl.HAND_TYPE.RIGHT_HAND);
				addCondition(attacker, target, pl.HAND_TYPE.LEFT_HAND);
				attacker.setAttackSuccess(0, flag: true);
				attacker.setAttackSuccess(1, flag: false);
				float num3 = (float)(ds.RandomNumber.rand32(3u) + 5) / 10f;
				int t2 = (int)((float)num * num3 * (float)t);
				if (attacker.breed() == 0)
				{
					BattlePlayer battlePlayer2 = static_cast<BattlePlayer>(attacker);
					if (battlePlayer2 != null && battlePlayer2.player() != null && battlePlayer2.player().formationType() == 1)
					{
						num /= 2;
					}
				}
				t2 = ds.clamp(t2, 1, 99999);
				if (t == 1)
				{
					t2 *= 150;
					t2 /= 100;
				}
				OS_Printf("GET! ダメージ %d\n", t2);
				return t2;
			}

			public int offenseAndDefense(BaseBattleCharacter character, BaseBattleCharacter target)
			{
				int num = 0;
				int num2 = 0;
				if (character.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get() > 0)
				{
					if (character.breed() == 0)
					{
						BattlePlayer battlePlayer = static_cast<BattlePlayer>(character);
						if (battlePlayer.player().equipParameter().isEquipWeapon() == 1)
						{
							if (battlePlayer.player().equipParameter().checkEquipWeaponHand() == pl.HAND_TYPE.RIGHT_HAND)
							{
								num += character.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
								num2++;
							}
						}
						else if (battlePlayer.player().equipParameter().isEquipWeapon() == 2)
						{
							num += character.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
							num2++;
						}
					}
					else
					{
						num += character.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
						num2++;
					}
				}
				if (character.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get() > 0)
				{
					if (character.breed() == 0)
					{
						BattlePlayer battlePlayer2 = static_cast<BattlePlayer>(character);
						if (battlePlayer2.player().equipParameter().isEquipWeapon() == 1)
						{
							if (battlePlayer2.player().equipParameter().checkEquipWeaponHand() == pl.HAND_TYPE.LEFT_HAND)
							{
								num += character.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get();
								num2++;
							}
						}
						else if (battlePlayer2.player().equipParameter().isEquipWeapon() == 2)
						{
							num += character.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get();
							num2++;
						}
					}
					else
					{
						num += character.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get();
						num2++;
					}
				}
				if (num2 == 2)
				{
					num /= 2;
				}
				if (num2 == 0 && num == 0)
				{
					num = character.bodyAndBonus().strength().get();
					OS_Printf("自分のSTRを攻撃力に！！！ %d\n", num);
				}
				int num3 = target.physicsDefense().phylacticPower().get();
				OS_Printf("防御力%d\n", num3);
				if ((target.flag(PLAYER_FLAG.PF_COUNTER) || target.flag(PLAYER_FLAG.PF_COVER)) && target.breed() == 0)
				{
					BattlePlayer battlePlayer3 = static_cast<BattlePlayer>(target);
					num3 += battlePlayer3.player().jobManager().nowJobParameter()
						.skill()
						.skillLevel()
						.get();
				}
				int t = ((num3 == 0) ? (4096 * num) : (4096 * num / num3));
				return ds.clamp(t, 0, 10240);
			}

			public int calcDefenseNumber(BaseBattleCharacter character)
			{
				OS_Printf("\n//-------------------------------------------------------\n");
				OS_Printf("防御回数を算出します\n\n");
				if (character.flag(PLAYER_FLAG.PF_ROLL_UP))
				{
					return 0;
				}
				int num = character.bodyAndBonus().dexterity().get() * 4096;
				OS_Printf("素早さ %d\n", num / 4096);
				int num2 = 0;
				int num3 = 4096;
				CommonFormula commonFormula = new CommonFormula();
				int num4 = commonFormula.calcWeight(character);
				return (num / 4096 / 4 + num2 / 4096 - num4 / 8) * num3 / 4096;
			}

			public int calcHit(BaseBattleCharacter character)
			{
				CommonFormula commonFormula = new CommonFormula();
				int num = commonFormula.calcJobSkill(character);
				int num2 = (commonFormula.calcHandSkill(character, pl.HAND_TYPE.RIGHT_HAND) + commonFormula.calcHandSkill(character, pl.HAND_TYPE.LEFT_HAND)) / 2;
				int num3 = character.bodyAndBonus().dexterity().get();
				return num + num2 + num3;
			}

			public int calcAvoidance(BaseBattleCharacter character)
			{
				CommonFormula commonFormula = new CommonFormula();
				int num = commonFormula.calcJobSkill(character);
				int num2 = character.bodyAndBonus().dexterity().get();
				int num3 = commonFormula.calcWeight(character);
				int t = num + num2 - num3;
				return ds.max(t, 1);
			}

			public bool calcCritical(BaseBattleCharacter character, BaseBattleCharacter target)
			{
				int t = character.bodyAndBonus().dexterity().get() - target.bodyAndBonus().dexterity().get() + 5;
				t = ds.clamp(t, 0, 10);
				int num = (int)ds.RandomNumber.rand32(101u);
				if (t <= num)
				{
					return false;
				}
				return true;
			}

			public bool addCondition(BaseBattleCharacter attacker, BaseBattleCharacter target, pl.HAND_TYPE handType)
			{
				int num = (int)ds.RandomNumber.rand32(101u);
				short num2 = attacker.handAttack(handType).attackOption();
				if (num2 == 0)
				{
					return false;
				}
				bool nearStone = false;
				if (attacker.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(attacker);
					if (battlePlayer != null && battlePlayer.player() != null)
					{
						short num3 = battlePlayer.player().equipParameter().equipHand(handType)
							.itemId();
						if (num3 == 1125 || num3 == 1209 || num3 == 1311 || num3 == 1517)
						{
							nearStone = true;
						}
					}
				}
				int num4 = attacker.handAttack(handType).optionProbability();
				if (!OutsideToBattle.getInstance().condition() && num > num4)
				{
					return false;
				}
				short anti = target.physicsDefense().antiOption();
				NewMagicFormula newMagicFormula = new NewMagicFormula();
				newMagicFormula.setCondition(attacker, target, num2, anti, nearStone);
				return true;
			}
		}
	}
}
