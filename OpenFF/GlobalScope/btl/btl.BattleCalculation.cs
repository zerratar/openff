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
		public class BattleCalculation
		{
			private int[] damage_ = new int[12];

			public void clearDamageAll()
			{
				for (int i = 0; i < 12; i++)
				{
					clearDamage(i);
				}
			}

			public void calcMagic(BattleCharacterManager characterManager, BaseBattleCharacter user)
			{
				for (int i = 0; i < 12; i++)
				{
					if (user.targetId(i) < 0)
					{
						continue;
					}
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager.getBaseBattleCharacterFromBreed(user.targetId(i));
					if (baseBattleCharacterFromBreed == null || baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_ABSORB) || baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_JUMP))
					{
						continue;
					}
					itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(user.useMagicId());
					if (magicParameter.isReflect() != 0 && baseBattleCharacterFromBreed.magicFlag(MAGIC_FLAG.MF_REFLECT) && !OutsideToBattle.getInstance().transfix())
					{
						BaseBattleCharacter baseBattleCharacter = null;
						TurnSystem turnSystem = new TurnSystem();
						BaseBattleCharacter baseBattleCharacter2 = new BaseBattleCharacter(baseBattleCharacterFromBreed);
						baseBattleCharacter2.setUseMagicId(user.useMagicId());
						baseBattleCharacter2.clearFlagAll();
						baseBattleCharacter2.setReflectFlag(flag: true);
						if (baseBattleCharacterFromBreed.breed() == 0)
						{
							turnSystem.setTargetRandam(baseBattleCharacter2, characterManager.monsterParty(), reflect: true);
						}
						else
						{
							turnSystem.setTargetRandam(baseBattleCharacter2, characterManager.playerParty(), reflect: true);
						}
						baseBattleCharacter = characterManager.getBaseBattleCharacterFromBreed(baseBattleCharacter2.targetId(0));
						if (baseBattleCharacter == null)
						{
							continue;
						}
						baseBattleCharacterFromBreed.setReflectTargetId(baseBattleCharacter.battleCharacterId());
						calcMagicDamage(user, baseBattleCharacter, user.useMagicId());
						if (baseBattleCharacter.changeCondition().normalCondition() != 0 || baseBattleCharacter.changeCondition().battleCondition() != 0)
						{
							if (user.useMagicId() == 4105 || user.useMagicId() == 4110)
							{
								if (baseBattleCharacter.flag(PLAYER_FLAG.PF_MISS))
								{
									baseBattleCharacter.clearFlag(PLAYER_FLAG.PF_2D);
									baseBattleCharacter.clearFlag(PLAYER_FLAG.PF_MISS);
								}
							}
							else if (!baseBattleCharacter.flag(PLAYER_FLAG.PF_2D) && baseBattleCharacter.flag(PLAYER_FLAG.PF_MISS))
							{
								baseBattleCharacter.setFlag(PLAYER_FLAG.PF_2D);
								baseBattleCharacter.clearFlag(PLAYER_FLAG.PF_MISS);
							}
						}
						CommonFormula commonFormula = new CommonFormula();
						baseBattleCharacter.updateParameterMagicFlag(user.useMagicId(), commonFormula.calcJobSkill(user));
						damageCharacter(baseBattleCharacter);
					}
					else
					{
						calcMagicDamage(user, baseBattleCharacterFromBreed, user.useMagicId());
						CommonFormula commonFormula2 = new CommonFormula();
						baseBattleCharacterFromBreed.updateParameterMagicFlag(user.useMagicId(), commonFormula2.calcJobSkill(user));
						damageCharacter(baseBattleCharacterFromBreed);
					}
				}
			}

			public void calcMagicDamage(BaseBattleCharacter user, BaseBattleCharacter target, short _id)
			{
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(_id);
				switch ((itm.MAGIC_USE_KIND)magicParameter.magicUseKind())
				{
				case itm.MAGIC_USE_KIND.MAGIC_USE_KIND_ATTACK:
					calcAttackMagic(user, target, magicParameter);
					break;
				case itm.MAGIC_USE_KIND.MAGIC_USE_KIND_RECOVERY:
					calcRecoveryMagic(user, target, magicParameter);
					break;
				case itm.MAGIC_USE_KIND.MAGIC_USE_KIND_ASSIST:
					calcAssistMagic(user, target, magicParameter);
					break;
				case itm.MAGIC_USE_KIND.MAGIC_USE_KIND_SPECIAL:
					calcSpecialMagic(user, target, magicParameter);
					break;
				}
			}

			public void calcAttackMagic(BaseBattleCharacter user, BaseBattleCharacter target, itm.MagicParameter magic)
			{
				NewMagicFormula newMagicFormula = new NewMagicFormula();
				damage_[target.battleCharacterId()] += newMagicFormula.attackMagicDamage(magic.itemId(), user, target, user.targetNumber());
				if (user.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(user);
					if (battlePlayer.actionId() == 7 && battlePlayer.player().jobManager().checkRegisterPassiveAbility(28))
					{
						damage_[target.battleCharacterId()] *= 2;
					}
				}
				if (target.flag(PLAYER_FLAG.PF_MORE_GUARD))
				{
					CommonFormula commonFormula = new CommonFormula();
					int num = commonFormula.calcJobSkill(target);
					int num2 = damage_[target.battleCharacterId()] - damage_[target.battleCharacterId()] * (40 + num / 5) / 100;
					damage_[target.battleCharacterId()] = num2;
				}
				if (target.magicFlag(MAGIC_FLAG.MF_SONG5))
				{
					CommonFormula commonFormula2 = new CommonFormula();
					int num3 = commonFormula2.calcJobSkill(target);
					int num4 = damage_[target.battleCharacterId()] - damage_[target.battleCharacterId()] * (20 + num3 * 10 / 110) / 100;
					damage_[target.battleCharacterId()] = num4;
				}
				if ((magic.magicType() & 4) != 0)
				{
					damage_[user.battleCharacterId()] = damage_[target.battleCharacterId()];
					user.setTargetIdMyself();
					user.setFlag(PLAYER_FLAG.PF_ABSORB);
					if ((target.physicsDefense().antiType() & 0x400) != 0)
					{
						if (target.battleCharacterId() != user.battleCharacterId())
						{
							target.setFlag(PLAYER_FLAG.PF_RECOVER);
							target.hp().addNow(damage_[target.battleCharacterId()]);
						}
						user.hp().subNow(damage_[user.battleCharacterId()]);
						return;
					}
					if (target.battleCharacterId() != user.battleCharacterId())
					{
						user.setFlag(PLAYER_FLAG.PF_RECOVER);
						user.hp().addNow(damage_[user.battleCharacterId()]);
					}
					target.hp().subNow(damage_[target.battleCharacterId()]);
					if (user.breed() == 0)
					{
						pl.PlayerParty.instance().mania().setMaxDamage(damage_[target.battleCharacterId()]);
					}
				}
				else
				{
					target.hp().subNow(damage_[target.battleCharacterId()]);
					if (user.breed() == 0)
					{
						pl.PlayerParty.instance().mania().setMaxDamage(damage_[target.battleCharacterId()]);
					}
				}
			}

			public void calcRecoveryMagic(BaseBattleCharacter user, BaseBattleCharacter target, itm.MagicParameter magic)
			{
				NewMagicFormula newMagicFormula = new NewMagicFormula();
				healingCondition(user, target, magic.changeCondition(), magic.itemId());
				healingDeath(target, magic.changeCondition(), magic.itemId());
				if ((magic.magicType() & 1) == 0 || (magic.changeCondition() & 0x200) != 0)
				{
					return;
				}
				target.clearFlag(PLAYER_FLAG.PF_2D);
				if (damage_[target.battleCharacterId()] == 0)
				{
					damage_[target.battleCharacterId()] += newMagicFormula.healingMagicValue(magic.itemId(), user, target, user.targetNumber());
					if (target.flag(PLAYER_FLAG.PF_RECOVER))
					{
						target.hp().addNow(damage_[target.battleCharacterId()]);
						return;
					}
					if (target.flag(PLAYER_FLAG.PF_MORE_GUARD))
					{
						CommonFormula commonFormula = new CommonFormula();
						int num = commonFormula.calcJobSkill(target);
						int num2 = damage_[target.battleCharacterId()] - damage_[target.battleCharacterId()] * (10 + num * 10 / 110) / 100;
						damage_[target.battleCharacterId()] = num2;
					}
					target.hp().subNow(damage_[target.battleCharacterId()]);
				}
				else if (target.isGhost())
				{
					newMagicFormula.healingMagicValue(magic.itemId(), user, target, user.targetNumber());
				}
			}

			public void calcAssistMagic(BaseBattleCharacter user, BaseBattleCharacter target, itm.MagicParameter magic)
			{
				NewMagicFormula newMagicFormula = new NewMagicFormula();
				newMagicFormula.conditionMagicOdds(magic.itemId(), user, target, user.targetNumber());
				if (target.isGhost() && magic.itemId() == 4123 && target.isBattle())
				{
					damage_[target.battleCharacterId()] = 9999;
					target.setFlag(PLAYER_FLAG.PF_RECOVER);
					target.hp().addNow(damage_[target.battleCharacterId()]);
				}
			}

			public void calcSpecialMagic(BaseBattleCharacter user, BaseBattleCharacter target, itm.MagicParameter magic)
			{
				short num = magic.itemId();
				if (num == 4010)
				{
					target.setFlag(PLAYER_FLAG.PF_2D);
				}
			}

			public void calcItem(BattleCharacterManager characterManager, BaseBattleCharacter user)
			{
				itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)user.useItemId());
				if (itemBaseParameter.useItemId() > 0)
				{
					for (int i = 0; i < 12; i++)
					{
						if (user.targetId(i) < 0)
						{
							continue;
						}
						BaseBattleCharacter baseBattleCharacterFromBreed = characterManager.getBaseBattleCharacterFromBreed(user.targetId(i));
						if (baseBattleCharacterFromBreed == null || baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_ABSORB) || baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_JUMP))
						{
							continue;
						}
						itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(itemBaseParameter.useItemId());
						if (magicParameter.isReflect() != 0 && baseBattleCharacterFromBreed.magicFlag(MAGIC_FLAG.MF_REFLECT) && !OutsideToBattle.getInstance().transfix())
						{
							BaseBattleCharacter baseBattleCharacter = null;
							TurnSystem turnSystem = new TurnSystem();
							BaseBattleCharacter baseBattleCharacter2 = baseBattleCharacterFromBreed;
							baseBattleCharacter2.setUseMagicId(user.useMagicId());
							baseBattleCharacter2.clearFlagAll();
							baseBattleCharacter2.setReflectFlag(flag: true);
							if (baseBattleCharacterFromBreed.breed() == 0)
							{
								turnSystem.setTargetRandam(baseBattleCharacter2, characterManager.monsterParty(), reflect: true);
							}
							else
							{
								turnSystem.setTargetRandam(baseBattleCharacter2, characterManager.playerParty(), reflect: true);
							}
							baseBattleCharacter = characterManager.getBaseBattleCharacterFromBreed(baseBattleCharacter2.targetId(0));
							if (baseBattleCharacter != null)
							{
								baseBattleCharacterFromBreed.setReflectTargetId(baseBattleCharacter.battleCharacterId());
								calcMagicDamage(user, baseBattleCharacter, itemBaseParameter.useItemId());
								CommonFormula commonFormula = new CommonFormula();
								baseBattleCharacter.updateParameterMagicFlag(itemBaseParameter.useItemId(), commonFormula.calcJobSkill(user));
								damageCharacter(baseBattleCharacter);
							}
						}
						else
						{
							calcMagicDamage(user, baseBattleCharacterFromBreed, itemBaseParameter.useItemId());
							CommonFormula commonFormula2 = new CommonFormula();
							baseBattleCharacterFromBreed.updateParameterMagicFlag(itemBaseParameter.useItemId(), commonFormula2.calcJobSkill(user));
							damageCharacter(baseBattleCharacterFromBreed);
						}
					}
					return;
				}
				for (int j = 0; j < 12; j++)
				{
					if (user.targetId(j) < 0)
					{
						continue;
					}
					BaseBattleCharacter baseBattleCharacterFromBreed2 = characterManager.getBaseBattleCharacterFromBreed(user.targetId(j));
					if (baseBattleCharacterFromBreed2 != null && !baseBattleCharacterFromBreed2.flag(PLAYER_FLAG.PF_ABSORB) && !baseBattleCharacterFromBreed2.flag(PLAYER_FLAG.PF_JUMP))
					{
						bool ability = false;
						if (user.breed() == 0)
						{
							BattlePlayer battlePlayer = static_cast<BattlePlayer>(user);
							ability = battlePlayer.player().jobManager().checkRegisterPassiveAbility(28);
						}
						calcItemDamage(user, baseBattleCharacterFromBreed2, (short)user.useItemId(), ability);
						CommonFormula commonFormula3 = new CommonFormula();
						baseBattleCharacterFromBreed2.updateParameterMagicFlag((short)user.useItemId(), commonFormula3.calcJobSkill(user));
						damageCharacter(baseBattleCharacterFromBreed2);
					}
				}
			}

			public void calcItemDamage(BaseBattleCharacter user, BaseBattleCharacter target, short _id, bool ability)
			{
				itm.ConsumptionParameter consumptionParameter = itm.ItemManager.instance().consumptionParameter(_id);
				healingCondition(user, target, consumptionParameter.changeCondition(), _id);
				healingDeath(target, consumptionParameter.changeCondition(), _id);
				if (consumptionParameter.usedPower() <= 0)
				{
					return;
				}
				target.clearFlag(PLAYER_FLAG.PF_2D);
				if (consumptionParameter.usedPower() == itm.FINE_POWER)
				{
					if (target.isGhost())
					{
						target.clearFlag(PLAYER_FLAG.PF_RECOVER);
						damage_[target.battleCharacterId()] = target.hp().getLimit() * 60 / 100;
						if (ability)
						{
							damage_[target.battleCharacterId()] *= 2;
						}
						target.hp().subNow(damage_[target.battleCharacterId()]);
						if (user.breed() == 0)
						{
							pl.PlayerParty.instance().mania().setMaxDamage(damage_[target.battleCharacterId()]);
						}
					}
					else
					{
						target.setFlag(PLAYER_FLAG.PF_RECOVER);
						damage_[target.battleCharacterId()] = 9999;
						if (target.breed() == 0)
						{
							BattlePlayer battlePlayer = static_cast<BattlePlayer>(target);
							battlePlayer.player().recoverHPandMP();
						}
						else
						{
							target.hp().maxNow();
						}
					}
				}
				else if (target.isGhost())
				{
					target.clearFlag(PLAYER_FLAG.PF_RECOVER);
					damage_[target.battleCharacterId()] = consumptionParameter.usedPower();
					if (ability)
					{
						damage_[target.battleCharacterId()] *= 2;
					}
					target.hp().subNow(damage_[target.battleCharacterId()]);
					if (user.breed() == 0)
					{
						pl.PlayerParty.instance().mania().setMaxDamage(damage_[target.battleCharacterId()]);
					}
				}
				else
				{
					target.setFlag(PLAYER_FLAG.PF_RECOVER);
					damage_[target.battleCharacterId()] = consumptionParameter.usedPower();
					if (ability)
					{
						damage_[target.battleCharacterId()] *= 2;
					}
					target.hp().addNow(damage_[target.battleCharacterId()]);
				}
			}

			public bool calcPoison(BattleCharacterManager characterManager, BaseBattleCharacter attacker)
			{
				bool result = false;
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager.getBaseBattleCharacterFromBreed((short)i);
					if (baseBattleCharacterFromBreed != null && baseBattleCharacterFromBreed.isEnable() && baseBattleCharacterFromBreed.condition().isPoison() && !baseBattleCharacterFromBreed.condition().isStone() && !baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_JUMP))
					{
						damage_[baseBattleCharacterFromBreed.battleCharacterId()] = baseBattleCharacterFromBreed.hp().getLimit() / 30;
						damage_[baseBattleCharacterFromBreed.battleCharacterId()] = ds.max(damage_[baseBattleCharacterFromBreed.battleCharacterId()], 1);
						baseBattleCharacterFromBreed.hp().subNow(damage_[baseBattleCharacterFromBreed.battleCharacterId()]);
						damageCharacter(baseBattleCharacterFromBreed);
						attacker.setTargetId(i, baseBattleCharacterFromBreed.battleCharacterId());
						result = true;
					}
				}
				return result;
			}

			public void healingCondition(BaseBattleCharacter user, BaseBattleCharacter target, short condition, short _id)
			{
				target.setFlag(PLAYER_FLAG.PF_2D);
				target.setFlag(PLAYER_FLAG.PF_RECOVER);
				if ((condition & 1) != 0)
				{
					target.condition().offParalysis();
				}
				if ((condition & 2) != 0)
				{
					target.condition().offSleep();
				}
				if ((condition & 4) != 0)
				{
					target.condition().offConfusion();
				}
				if ((condition & 0x20) != 0)
				{
					target.condition().offSilence();
				}
				if ((condition & 0x100) != 0)
				{
					target.condition().offPoison();
				}
				if ((condition & 0x80) != 0)
				{
					target.condition().offDarkness();
				}
				if ((condition & 0x40) != 0)
				{
					if (calcLilliput(user, target, _id))
					{
						target.changeCondition().onLilliput();
					}
					else
					{
						target.changeCondition().offLilliput();
						if (_id != 4020)
						{
							target.clearFlag(PLAYER_FLAG.PF_2D);
							target.setFlag(PLAYER_FLAG.PF_MISS);
						}
					}
				}
				if ((condition & 0x10) != 0)
				{
					if (calcFrog(user, target, _id))
					{
						target.changeCondition().onFrog();
					}
					else
					{
						target.changeCondition().offFrog();
						if (_id != 4020 && _id != 5007)
						{
							target.clearFlag(PLAYER_FLAG.PF_2D);
							target.setFlag(PLAYER_FLAG.PF_MISS);
						}
					}
				}
				if ((condition & 8) != 0)
				{
					target.condition().offStone();
					target.condition().offNearStone();
					target.condition().clearNearStoneCounter();
				}
			}

			public void healingDeath(BaseBattleCharacter target, short condition, short _id)
			{
				if ((condition & 0x200) == 0)
				{
					return;
				}
				target.clearFlag(PLAYER_FLAG.PF_2D);
				if (_id == 4023)
				{
					if (target.isGhost())
					{
						target.clearFlag(PLAYER_FLAG.PF_RECOVER);
						target.setFlag(PLAYER_FLAG.PF_2D);
						damage_[target.battleCharacterId()] = 0;
						target.hp().minNow();
					}
					else if (target.condition().isDeath())
					{
						target.setFlag(PLAYER_FLAG.PF_RECOVER);
						target.condition().offDeath();
						damage_[target.battleCharacterId()] = 9999;
						target.hp().maxNow();
						if (target.breed() == 0)
						{
							BattlePlayer battlePlayer = static_cast<BattlePlayer>(target);
							battlePlayer.setIdleType(0);
						}
					}
					else
					{
						target.setFlag(PLAYER_FLAG.PF_2D);
					}
				}
				else if (target.isGhost())
				{
					target.clearFlag(PLAYER_FLAG.PF_RECOVER);
					target.setFlag(PLAYER_FLAG.PF_2D);
					damage_[target.battleCharacterId()] = 0;
					target.hp().minNow();
				}
				else if (target.condition().isDeath())
				{
					target.setFlag(PLAYER_FLAG.PF_RECOVER);
					target.condition().offDeath();
					damage_[target.battleCharacterId()] = target.hp().getLimit() * 10 / 100;
					target.hp().setNow(damage_[target.battleCharacterId()]);
					if (target.breed() == 0)
					{
						BattlePlayer battlePlayer2 = static_cast<BattlePlayer>(target);
						battlePlayer2.setIdleType(0);
					}
				}
				else
				{
					target.setFlag(PLAYER_FLAG.PF_2D);
				}
			}

			public bool calcFrog(BaseBattleCharacter user, BaseBattleCharacter target, short _id)
			{
				if (!target.isBattle())
				{
					return false;
				}
				if (_id == 5007 && !target.condition().isFrog())
				{
					return false;
				}
				if ((target.physicsDefense().antiOption() & 0x10) != 0 && !target.condition().isFrog())
				{
					return false;
				}
				if (_id == 4020 && !target.condition().isFrog())
				{
					return false;
				}
				if (target.condition().isLilliput())
				{
					return false;
				}
				if (user.breed() == target.breed())
				{
					return true;
				}
				int num = (int)ds.RandomNumber.rand32(101u);
				NewMagicFormula newMagicFormula = new NewMagicFormula();
				int num2 = newMagicFormula.calcCommonConditionOdds(user, 7, target, 30, 1);
				if (num2 <= num)
				{
					return false;
				}
				return true;
			}

			public bool calcLilliput(BaseBattleCharacter user, BaseBattleCharacter target, short _id)
			{
				if (!target.isBattle())
				{
					return false;
				}
				if ((target.physicsDefense().antiOption() & 0x40) != 0 && !target.condition().isLilliput())
				{
					return false;
				}
				if (_id == 4020 && !target.condition().isLilliput())
				{
					return false;
				}
				if (target.condition().isFrog())
				{
					return false;
				}
				if (user.breed() == target.breed())
				{
					return true;
				}
				int num = (int)ds.RandomNumber.rand32(101u);
				NewMagicFormula newMagicFormula = new NewMagicFormula();
				int num2 = newMagicFormula.calcCommonConditionOdds(user, 7, target, 30, 1);
				if (num2 <= num)
				{
					return false;
				}
				return true;
			}

			public void calcDark(BattlePlayer player)
			{
				DarkFormula darkFormula = new DarkFormula();
				damage_[player.battleCharacterId()] = darkFormula.calcDarkSubHp(player);
				player.hp().subNow(damage_[player.battleCharacterId()]);
			}

			public void calcRollUp(BattlePlayer player)
			{
				damage_[player.battleCharacterId()] = player.hp().getNow() / 2;
				player.hp().subNow(damage_[player.battleCharacterId()]);
			}

			public void calcGeography(BattleCharacterManager characterManager, BattlePlayer player)
			{
				for (int i = 0; i < 12; i++)
				{
					if (player.targetId(i) < 0)
					{
						continue;
					}
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager.getBaseBattleCharacterFromBreed(player.targetId(i));
					int num = player.useMagicId();
					itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter((short)num);
					if (magicParameter == null)
					{
						continue;
					}
					GeographyFormula geographyFormula = new GeographyFormula();
					switch (num)
					{
					case 6506:
						if (geographyFormula.calcGeographyDeath1(player) && (baseBattleCharacterFromBreed.physicsDefense().antiOption() & 0x200) == 0)
						{
							baseBattleCharacterFromBreed.setConditionDeath();
							baseBattleCharacterFromBreed.setFlag(PLAYER_FLAG.PF_2D);
							baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_MISS);
						}
						else
						{
							baseBattleCharacterFromBreed.setFlag(PLAYER_FLAG.PF_MISS);
							baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_2D);
						}
						break;
					case 6509:
						if (geographyFormula.calcGeographyDeath2(player) && (baseBattleCharacterFromBreed.physicsDefense().antiOption() & 0x200) == 0)
						{
							baseBattleCharacterFromBreed.setConditionDeath();
							baseBattleCharacterFromBreed.setFlag(PLAYER_FLAG.PF_2D);
							baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_MISS);
						}
						else
						{
							baseBattleCharacterFromBreed.setFlag(PLAYER_FLAG.PF_MISS);
							baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_2D);
						}
						break;
					case 6510:
						damage_[player.targetId(i)] = geographyFormula.calcGeographyDamage3(player);
						baseBattleCharacterFromBreed.hp().subNow(damage_[player.targetId(i)]);
						break;
					default:
						if (magicParameter.targetPossible() == 2)
						{
							damage_[player.targetId(i)] = geographyFormula.calcGeographyDamage1(player, magicParameter.magicType());
							baseBattleCharacterFromBreed.hp().subNow(damage_[player.targetId(i)]);
						}
						else
						{
							damage_[player.targetId(i)] = geographyFormula.calcGeographyDamage2(player, magicParameter.magicType());
							baseBattleCharacterFromBreed.hp().subNow(damage_[player.targetId(i)]);
						}
						break;
					}
					damageCharacter(baseBattleCharacterFromBreed);
					pl.PlayerParty.instance().mania().setMaxDamage(damage_[baseBattleCharacterFromBreed.battleCharacterId()]);
				}
			}

			public bool calcCoverFire(BattleCharacterManager characterManager, BaseBattleCharacter attacker)
			{
				bool result = false;
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager.getBaseBattleCharacterFromBreed(attacker.targetId(i));
					if (baseBattleCharacterFromBreed != null && baseBattleCharacterFromBreed.isBattle())
					{
						damage_[baseBattleCharacterFromBreed.battleCharacterId()] = baseBattleCharacterFromBreed.hp().getLimit() * 2 / 3;
						damage_[baseBattleCharacterFromBreed.battleCharacterId()] = ds.max(damage_[baseBattleCharacterFromBreed.battleCharacterId()], 1);
						baseBattleCharacterFromBreed.hp().subNow(damage_[baseBattleCharacterFromBreed.battleCharacterId()]);
						damageCharacter(baseBattleCharacterFromBreed);
						result = true;
					}
				}
				return result;
			}

			public void damageCharacter(BaseBattleCharacter target)
			{
				if (target.hp().getNow() == 0)
				{
					target.condition().onDeath();
					target.condition().clearConditionTime();
					target.onIsActionEnd();
				}
				else if (target.hp().getNow() <= target.hp().getLimit() * 25 / 100)
				{
					target.condition().onNearDeath();
				}
			}

			public int damage(int i)
			{
				return damage_[i];
			}

			public void damage_add(int i, int arg0)
			{
				damage_[i] += arg0;
			}

			public void setDamage(int i, int value)
			{
				damage_[i] = value;
			}

			public void clearDamage(int i)
			{
				damage_[i] = 0;
			}
		}
	}
}
