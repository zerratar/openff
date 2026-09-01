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
	public static partial class itm
	{
		public class ItemUse
		{
			private static int[,] membersItem = new int[15, 3]
			{
				{ 5001, 10, 15 },
				{ 5002, 11, 15 },
				{ 5003, 12, 15 },
				{ 5004, 13, 15 },
				{ 5005, 14, 15 },
				{ 5006, 15, 15 },
				{ 5007, 16, 16 },
				{ 5008, 17, 17 },
				{ 5009, -1, -1 },
				{ 5010, 19, 15 },
				{ 5011, 20, 15 },
				{ 5012, -1, -1 },
				{ 5014, -1, -1 },
				{ 0, 0, 0 },
				{ 0, 0, 0 }
			};

			private static int[,] membersMagic = new int[15, 3]
			{
				{ 4001, 30, 15 },
				{ 4002, 31, 15 },
				{ 4003, -1, 15 },
				{ 4005, 33, 16 },
				{ 4006, 34, 17 },
				{ 4007, 35, 15 },
				{ 4008, -1, 15 },
				{ 4009, 37, 15 },
				{ 4010, -1, 15 },
				{ 4013, 39, 15 },
				{ 4014, 40, 15 },
				{ 4017, 41, 15 },
				{ 4019, 42, 15 },
				{ 4020, 43, 15 },
				{ 4023, 44, 15 }
			};

			public bool isUseInBattle(int itemId)
			{
				ItemBaseParameter itemBaseParameter = ItemManager.instance().itemParameter((short)itemId);
				if (itemBaseParameter == null)
				{
					return false;
				}
				if (evt.CEventRestriction.getSingleton().check(itemBaseParameter.itemId()))
				{
					return false;
				}
				if (ItemManager.instance().weaponParameter((short)itemId) != null)
				{
					short itemId2 = ItemManager.instance().weaponParameter((short)itemId).useItemId();
					if (ItemManager.instance().magicParameter(itemId2) == null)
					{
						return false;
					}
				}
				if (itemBaseParameter.useBattle() == 0)
				{
					return false;
				}
				return true;
			}

			public bool useItemInField(int itemId, int playerId)
			{
				ConsumptionParameter consumptionParameter = ItemManager.instance().consumptionParameter((short)itemId);
				if (consumptionParameter != null && !useItem(consumptionParameter, itemId, playerId))
				{
					return false;
				}
				pl.PlayerParty.instance().player((byte)playerId).updateCondition();
				int[,] array = membersItem;
				pl.CBasePlayer cBasePlayer = wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
					.Player(0);
				if (cBasePlayer != null)
				{
					int num = -1;
					for (int i = 0; i < 15; i++)
					{
						if (array[i, 0] == itemId && array[i, 2] > 0)
						{
							num = eff.CEffectMng.instance().create(102, array[i, 2]);
						}
					}
					if (num != -1)
					{
						int num2 = playerId * 4096 * 9 - 69632;
						VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
						fnd_reuse_pos.x = 0;
						fnd_reuse_pos.y = 40960000 - num2 * 2;
						fnd_reuse_pos.z = num2;
						eff.CEffectMng.instance().setPosition(num, fnd_reuse_pos);
					}
				}
				for (int j = 0; j < 15; j++)
				{
					if (array[j, 0] == itemId)
					{
						MatrixSound.MtxSENDS_Play(98, array[j, 1], 192, 127);
						return true;
					}
				}
				MatrixSound.MtxSENDS_Play(0, 1, 192, 127);
				return true;
			}

			public bool useMagicInField(int magicId, int userId, int targetId, bool all)
			{
				MagicParameter magicParameter = ItemManager.instance().magicParameter((short)magicId);
				if (magicParameter == null)
				{
					return false;
				}
				if (!useMagic(magicId, userId, targetId, all))
				{
					return false;
				}
				for (int i = 0; i < 4; i++)
				{
					pl.PlayerParty.instance().player((byte)i).updateCondition();
				}
				pl.PlayerNormalMagicParameter playerNormalMagicParameter = pl.PlayerParty.instance().normalMagic(magicId);
				if (playerNormalMagicParameter != null)
				{
					int[,] array = membersMagic;
					for (int j = 0; j < 4; j++)
					{
						if (!all && j != targetId)
						{
							continue;
						}
						pl.CBasePlayer cBasePlayer = wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
							.Player(0);
						if (cBasePlayer == null)
						{
							continue;
						}
						int num = -1;
						for (int k = 0; k < 15; k++)
						{
							if (array[k, 0] == magicId)
							{
								num = eff.CEffectMng.instance().create(102, array[k, 2]);
							}
						}
						if (num != -1)
						{
							int num2 = j * 4096 * 9 - 69632;
							VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
							fnd_reuse_pos.x = 0;
							fnd_reuse_pos.y = 40960000 - num2 * 2;
							fnd_reuse_pos.z = num2;
							eff.CEffectMng.instance().setPosition(num, fnd_reuse_pos);
						}
					}
					for (int l = 0; l < 15; l++)
					{
						if (array[l, 0] == magicId)
						{
							MatrixSound.MtxSENDS_Play(98, array[l, 1], 192, 127);
							return true;
						}
					}
					MatrixSound.MtxSENDS_Play(0, 1, 192, 127);
				}
				return true;
			}

			public bool useItem(ConsumptionParameter item, int itemId, int playerId)
			{
				bool result = false;
				if ((item.itemType() & 1) != 0 && useRebirthItem(itemId, playerId))
				{
					result = true;
				}
				if (useConditionItem(itemId, playerId))
				{
					result = true;
				}
				if ((item.itemType() & 1) != 0)
				{
					if (useHpRecoverItem(itemId, playerId))
					{
						result = true;
					}
					if (useMpRecoverItem(itemId, playerId))
					{
						result = true;
					}
				}
				return result;
			}

			public bool useHpRecoverItem(int itemId, int playerId)
			{
				ConsumptionParameter consumptionParameter = ItemManager.instance().consumptionParameter((short)itemId);
				pl.Player player = pl.PlayerParty.instance().player((byte)playerId);
				if (consumptionParameter.usedPower() == 0 || player.hp().getNow() >= player.hp().getLimit())
				{
					return false;
				}
				if (player.condition().isNotBattleCondition())
				{
					return false;
				}
				player.hp().addNow(consumptionParameter.usedPower());
				player.hp().setNow(ds.clamp(player.hp().getNow(), 0, player.hp().getLimit()));
				return true;
			}

			public bool useConditionItem(int itemId, int playerId)
			{
				bool result = false;
				ConsumptionParameter consumptionParameter = ItemManager.instance().consumptionParameter((short)itemId);
				pl.Player player = pl.PlayerParty.instance().player((byte)playerId);
				int num = consumptionParameter.changeCondition();
				if (num == 0)
				{
					return false;
				}
				if ((consumptionParameter.itemType() & 1) != 0)
				{
					int num2 = player.condition().normalCondition();
					num2 &= -2;
					if ((num2 & -129) == 0)
					{
						return false;
					}
					if ((num & 0x10) != 0 && player.condition().isFrog())
					{
						player.condition().offFrog();
						result = true;
					}
					if ((num & 0x20) != 0 && player.condition().isSilence())
					{
						player.condition().offSilence();
						result = true;
					}
					if ((num & 0x40) != 0)
					{
						player.condition().offLilliput();
						result = true;
					}
					if ((num & 0x100) != 0 && player.condition().isPoison())
					{
						player.condition().offPoison();
						result = true;
					}
					if ((num & 8) != 0 && player.condition().isStone())
					{
						player.condition().offStone();
						player.condition().offNearStone();
						player.condition().clearNearStoneCounter();
						result = true;
					}
					if ((num & 0x80) != 0 && player.condition().isDarkness())
					{
						player.condition().offDarkness();
						result = true;
					}
				}
				else
				{
					if ((num & 0x10) != 0)
					{
						if (player.condition().isFrog())
						{
							player.condition().offFrog();
							result = true;
						}
						else if (!player.condition().isLilliput())
						{
							player.condition().onFrog();
							result = true;
						}
					}
					if ((num & 0x20) != 0)
					{
						if (player.condition().isSilence())
						{
							player.condition().offSilence();
						}
						else
						{
							player.condition().onSilence();
						}
						result = true;
					}
					if ((num & 0x40) != 0)
					{
						if (player.condition().isLilliput())
						{
							player.condition().offLilliput();
							result = true;
						}
						else if (!player.condition().isFrog())
						{
							player.condition().onLilliput();
							result = true;
						}
					}
					if ((num & 0x100) != 0)
					{
						if (player.condition().isPoison())
						{
							player.condition().offPoison();
						}
						else
						{
							player.condition().onPoison();
						}
						result = true;
					}
					if ((num & 8) != 0)
					{
						if (player.condition().isStone())
						{
							player.condition().offStone();
							player.condition().offNearStone();
							player.condition().clearNearStoneCounter();
							result = true;
						}
						else if (player.condition().isStone())
						{
							player.condition().offNearStone();
							player.condition().clearNearStoneCounter();
							result = true;
						}
					}
					if ((num & 0x80) != 0)
					{
						if (player.condition().isDarkness())
						{
							player.condition().offDarkness();
						}
						else
						{
							player.condition().onDarkness();
						}
						result = true;
					}
				}
				return result;
			}

			public bool useRebirthItem(int itemId, int playerId)
			{
				if (itemId != 5010)
				{
					return false;
				}
				ConsumptionParameter consumptionParameter = ItemManager.instance().consumptionParameter((short)itemId);
				pl.Player player = pl.PlayerParty.instance().player((byte)playerId);
				if (!player.condition().isDeath())
				{
					return false;
				}
				player.condition().offDeath();
				if (consumptionParameter.usedPower() < 100)
				{
					int value = player.hp().getLimit() * 10 / 100;
					player.hp().addNow(value);
				}
				else
				{
					player.hp().addNow(consumptionParameter.usedPower());
				}
				return true;
			}

			public bool useMpRecoverItem(int itemId, int playerId)
			{
				if (itemId != 5011)
				{
					return false;
				}
				pl.Player player = pl.PlayerParty.instance().player((byte)playerId);
				if (player.condition().isNotBattleCondition())
				{
					return false;
				}
				bool result = false;
				for (int i = 0; i < 8; i++)
				{
					if (player.mp(i).getNow() != player.mp(i).getLimit())
					{
						result = true;
					}
					player.mp(i).maxNow();
					player.setJobChangeMp(i, (byte)player.mp(i).getNow());
				}
				return result;
			}

			public bool useMagic(int magicId, int userId, int targetId, bool all)
			{
				MagicParameter magicParameter = ItemManager.instance().magicParameter((short)magicId);
				if (magicParameter == null)
				{
					return false;
				}
				if (magicParameter.magicUseKind() != 1)
				{
					return false;
				}
				if (((magicParameter.magicType() & 1) == 0 || ((magicParameter.magicType() & 1) != 0 && (magicParameter.changeCondition() & 0x200) != 0)) && !useConditionMagic(magicId, targetId, all))
				{
					return false;
				}
				if ((magicParameter.magicType() & 1) != 0 && (magicParameter.changeCondition() & 0x200) == 0 && !useHpRecoverMagic(magicId, userId, targetId, all))
				{
					return false;
				}
				return true;
			}

			public bool useHpRecoverMagic(int magicId, int userId, int targetId, bool all)
			{
				pl.Player user = pl.PlayerParty.instance().player((byte)userId);
				if (all)
				{
					int num = 0;
					for (int i = 0; i < 4; i++)
					{
						pl.Player player = pl.PlayerParty.instance().player((byte)i);
						if (player.isEnable())
						{
							if (!player.isHealing() || player.hp().getNow() >= player.hp().getLimit())
							{
								num++;
								continue;
							}
							int value = healingMagicValueInField((short)magicId, user, player, pl.PlayerParty.instance().aliveNumber());
							player.hp().addNow(value);
						}
					}
					if (num >= pl.PlayerParty.instance().partyMemberEnableNumber())
					{
						return false;
					}
				}
				else
				{
					pl.Player player2 = pl.PlayerParty.instance().player((byte)targetId);
					if (!player2.isHealing() || player2.hp().getNow() >= player2.hp().getLimit())
					{
						return false;
					}
					int value2 = healingMagicValueInField((short)magicId, user, player2, 1);
					player2.hp().addNow(value2);
				}
				return true;
			}

			public int healingMagicValueInField(short magicId, pl.Player user, pl.Player target, int targetNumber)
			{
				int num = (user.bodyAndBonus().mind().get() / 2 + user.jobManager().job(static_cast<pl.JOB_TYPE>(user.jobManager().nowJob())).skill()
					.skillLevel()
					.get() / 4 + target.bodyAndBonus().vitality().get() / 8) * ItemManager.instance().magicParameter(magicId).magicAggressivity();
				int num2 = static_cast<int>(num) * 4096;
				int num3 = 4096;
				int num4 = 0;
				num4 = ((targetNumber != 1) ? (4096 * (90 - targetNumber * 5) / 100) : 4096);
				OS_Printf("実際の回復量算出 %d\n", num2 / 4096 * num3 / 4096 * num4 / 4096);
				return num2 / 4096 * num3 / 4096 * num4 / 4096;
			}

			public bool useConditionMagic(int magicId, int targetId, bool all)
			{
				MagicParameter magicParameter = ItemManager.instance().magicParameter((short)magicId);
				if (all)
				{
					bool flag = false;
					for (int i = 0; i < 4; i++)
					{
						if (pl.PlayerParty.instance().player((byte)i).isEnable() && (magicId == 4005 || magicId == 4006 || isHealCondition(magicParameter.changeCondition(), pl.PlayerParty.instance().player((byte)i).condition())))
						{
							flag = true;
							healConditionMagic(magicParameter.changeCondition(), i, magicId);
						}
					}
					if (!flag)
					{
						return false;
					}
				}
				else
				{
					if (magicId != 4005 && magicId != 4006 && !isHealCondition(magicParameter.changeCondition(), pl.PlayerParty.instance().player((byte)targetId).condition()))
					{
						return false;
					}
					healConditionMagic(magicParameter.changeCondition(), targetId, magicId);
				}
				return true;
			}

			public void healConditionMagic(short healCondition, int targetId, int magicId)
			{
				pl.Player player = pl.PlayerParty.instance().player((byte)targetId);
				if ((healCondition & 1) != 0)
				{
					player.condition().offParalysis();
				}
				if ((healCondition & 2) != 0)
				{
					player.condition().offSleep();
				}
				if ((healCondition & 4) != 0)
				{
					player.condition().offConfusion();
				}
				if ((healCondition & 0x20) != 0)
				{
					player.condition().offSilence();
				}
				if ((healCondition & 0x100) != 0)
				{
					player.condition().offPoison();
				}
				if ((healCondition & 0x80) != 0)
				{
					player.condition().offDarkness();
				}
				if ((healCondition & 0x40) != 0)
				{
					if (player.condition().isLilliput())
					{
						player.condition().offLilliput();
					}
					else if (magicId == 4006 && !player.condition().isFrog())
					{
						player.condition().onLilliput();
					}
				}
				if ((healCondition & 0x10) != 0)
				{
					if (player.condition().isFrog())
					{
						player.condition().offFrog();
					}
					else if (magicId == 4005 && !player.condition().isLilliput())
					{
						player.condition().onFrog();
					}
				}
				if ((healCondition & 8) != 0)
				{
					player.condition().offStone();
					player.condition().offNearStone();
					player.condition().clearNearStoneCounter();
				}
				if ((healCondition & 0x200) == 0)
				{
					return;
				}
				if (magicId == 4023)
				{
					if (player.condition().isDeath())
					{
						player.condition().offDeath();
						player.hp().maxNow();
					}
				}
				else if (player.condition().isDeath())
				{
					player.condition().offDeath();
					player.hp().setNow(player.hp().getLimit() * 10 / 100);
				}
			}

			public bool isHealCondition(short heal_condition, ys.Condition target_condition)
			{
				if ((heal_condition & 0x80) != 0 && target_condition.isDarkness())
				{
					return true;
				}
				if ((heal_condition & 8) != 0 && target_condition.isStone())
				{
					return true;
				}
				if ((heal_condition & 0x10) != 0 && target_condition.isFrog())
				{
					return true;
				}
				if ((heal_condition & 0x20) != 0 && target_condition.isSilence())
				{
					return true;
				}
				if ((heal_condition & 0x40) != 0 && target_condition.isLilliput())
				{
					return true;
				}
				if ((heal_condition & 0x100) != 0 && target_condition.isPoison())
				{
					return true;
				}
				if ((heal_condition & 0x200) != 0 && target_condition.isDeath())
				{
					return true;
				}
				return false;
			}
		}
	}
}
