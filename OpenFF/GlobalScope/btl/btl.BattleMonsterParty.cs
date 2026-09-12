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
		public class BattleMonsterParty
		{
			private BattleMonster[] monster_ = new BattleMonster[6];

			private short memberNumber_;

			private short dropItemId_;

			private ys.ParameterPoint<int> dropItemNumber_ = new ys.ParameterPoint<int>(0, 99);

			private ys.ParameterPoint<int> giftGold_ = new ys.ParameterPoint<int>(0, 9999999);

			private ys.ParameterPoint<int> giftExp_ = new ys.ParameterPoint<int>(0, 9999999);

			private ys.ParameterPoint<byte> giftCapacity_ = new ys.ParameterPoint<byte>(0, 99);

			public void initialize()
			{
				for (int i = 0; i < 6; i++)
				{
					monster_[i].initialize();
				}
				dropItemId_ = -1;
				dropItemNumber_.min();
				memberNumber_ = 0;
				giftGold_.min();
				giftExp_.min();
				giftCapacity_.min();
			}

			public void terminate()
			{
				for (int i = 0; i < 6; i++)
				{
					monster_[i].terminate();
				}
				initialize();
			}

			public void execute()
			{
				for (int i = 0; i < 6; i++)
				{
					battleMonster(i).checkClearEffectId();
					battleMonster(i).calcFrameCounter();
				}
			}

			public void preExecute()
			{
				for (int i = 0; i < 6; i++)
				{
					if (battleMonster(i).isEnable())
					{
						battleMonster(i).preExecute();
					}
				}
			}

			public void setDropItemId(short itemId)
			{
				if (dropItemId_ < 0)
				{
					dropItemId_ = itemId;
					OS_Printf("アイテムID %d をもらえるリストに登録しました\n", dropItemId_);
					addDropItemNumber();
				}
				else if (dropItemId_ == itemId)
				{
					addDropItemNumber();
				}
				else
				{
					OS_Printf("すでに登録されていましたので残念ですがアイテムはもらえません\n");
				}
			}

			public void registerParty(ref short charNum)
			{
				int num = OutsideToBattle.getInstance().initializeMonster().monsterId();
				if (num >= 0)
				{
					registerMonster(ref charNum, (short)num);
					setBattleCharacterIdMonsterParty(ref charNum);
					return;
				}
				int id = OutsideToBattle.getInstance().initializeMonster().monsterPartyId();
				int num2 = 0;
				mon.MonsterParty monsterParty = mon.MonsterPartyManager.instance().monsterParty(id);
				for (int i = 0; i < mon.MONSTERS_MAX; i++)
				{
					mon.Monsters monsters = monsterParty.monsters(i);
					if (monsters.monsterId() < 0 || memberNumber_ == 6)
					{
						continue;
					}
					ushort num3 = monsters.min();
					ushort num4 = monsters.max();
					int t = (int)ds.RandomNumber.logic32(num4);
					if (OutsideToBattle.getInstance().initializeMonster().monsterCountType() == 1)
					{
						t = num3;
					}
					if (OutsideToBattle.getInstance().initializeMonster().monsterCountType() == 2)
					{
						t = num4 - 1;
					}
					t = ds.max(t, num3);
					if (t > 6 - memberNumber_)
					{
						t = 6 - memberNumber_;
					}
					t += memberNumber_;
					while (memberNumber_ < t)
					{
						mon.MonsterParameter monsterParameter = mon.MonsterManager.instance().monsterParameter(monsters.monsterId());
						if (monsterParameter != null)
						{
							if (monsterParameter.size() == 0)
							{
								if (memberNumber_ == 6)
								{
									return;
								}
							}
							else if (monsterParameter.size() == 1)
							{
								if (memberNumber_ == 3)
								{
									return;
								}
							}
							else if (monsterParameter.size() == 2 && memberNumber_ == 3)
							{
								return;
							}
						}
						battleMonster(memberNumber_).onIsEnable();
						battleMonster(memberNumber_).setBattleCharacterId(charNum);
						battleMonster(memberNumber_).breed_set(1);
						battleMonster(memberNumber_).monsterId_set(monsters.monsterId());
						battleMonster(memberNumber_).setCondition(battleMonster(memberNumber_).monsterCondition());
						battleMonster(memberNumber_).setMonster(mon.MonsterManager.instance().monsterParameter(monsters.monsterId()));
						battleMonster(memberNumber_).actionNumber_set(battleMonster(memberNumber_).monster().actionNumber());
						battleMonster(memberNumber_).monsterHp().setLimit(battleMonster(memberNumber_).monster().maxHp());
						battleMonster(memberNumber_).monsterHp().maxNow();
						battleMonster(memberNumber_).setMonsterBody(battleMonster(memberNumber_).monster().body());
						battleMonster(memberNumber_).setMonstermBodyAndBonus(battleMonster(memberNumber_).monster().body());
						battleMonster(memberNumber_).setMonsterHandAttack(0, battleMonster(memberNumber_).monster().physicsAttack());
						battleMonster(memberNumber_).setMonsterHandAttack(1, battleMonster(memberNumber_).monster().physicsAttack());
						battleMonster(memberNumber_).setMonsterPhysicsDefense(battleMonster(memberNumber_).monster().physicsDefense());
						battleMonster(memberNumber_).setMonsterMagicDefense(battleMonster(memberNumber_).monster().magicDefense());
						battleMonster(memberNumber_).setLevel(battleMonster(memberNumber_).monster().level());
						battleMonster(memberNumber_).setHp(battleMonster(memberNumber_).monsterHp());
						battleMonster(memberNumber_).setBody(battleMonster(memberNumber_).monsterBody());
						battleMonster(memberNumber_).setBodyAndBonus(battleMonster(memberNumber_).monstermBodyAndBonus());
						battleMonster(memberNumber_).setHandAttack(pl.HAND_TYPE.RIGHT_HAND, battleMonster(memberNumber_).monsterHandAttack(0));
						battleMonster(memberNumber_).setHandAttack(pl.HAND_TYPE.LEFT_HAND, battleMonster(memberNumber_).monsterHandAttack(1));
						battleMonster(memberNumber_).setPhysicsDefense(battleMonster(memberNumber_).monsterPhysicsDefense());
						battleMonster(memberNumber_).setMagicDefense(battleMonster(memberNumber_).monsterMagicDefense());
						battleMonster(memberNumber_).battleMonsterId_set(num2);
						num2++;
						charNum++;
						memberNumber_++;
					}
				}
				setBattleCharacterIdMonsterParty(ref charNum);
				if (memberNumber_ != 2)
				{
					return;
				}
				BattleMonster arg = battleMonster(2);
				battleMonster_set(2, battleMonster(0));
				battleMonster_set(0, battleMonster(1));
				battleMonster_set(1, arg);
				for (int j = 0; j < 3; j++)
				{
					if (j != 1)
					{
						battleMonster(j).setCondition(battleMonster(j).monsterCondition());
						battleMonster(j).setHp(battleMonster(j).monsterHp());
						battleMonster(j).setBody(battleMonster(j).monsterBody());
						battleMonster(j).setBodyAndBonus(battleMonster(j).monstermBodyAndBonus());
						battleMonster(j).setHandAttack(pl.HAND_TYPE.RIGHT_HAND, battleMonster(j).monsterHandAttack(0));
						battleMonster(j).setHandAttack(pl.HAND_TYPE.LEFT_HAND, battleMonster(j).monsterHandAttack(1));
						battleMonster(j).setPhysicsDefense(battleMonster(j).monsterPhysicsDefense());
						battleMonster(j).setMagicDefense(battleMonster(j).monsterMagicDefense());
						battleMonster(j).battleMonsterId_set(j);
					}
				}
			}

			public void registerMonster(ref short charNum, short monsterId)
			{
				battleMonster(0).onIsEnable();
				battleMonster(0).setBattleCharacterId(charNum);
				battleMonster(0).breed_set(1);
				battleMonster(0).monsterId_set(monsterId);
				battleMonster(0).setCondition(battleMonster(0).monsterCondition());
				battleMonster(0).setMonster(mon.MonsterManager.instance().monsterParameter(monsterId));
				battleMonster(0).actionNumber_set(battleMonster(0).monster().actionNumber());
				battleMonster(0).monsterHp().setLimit(battleMonster(0).monster().maxHp());
				battleMonster(0).monsterHp().maxNow();
				battleMonster(0).setMonsterBody(battleMonster(0).monster().body());
				battleMonster(0).setMonstermBodyAndBonus(battleMonster(0).monster().body());
				battleMonster(0).setMonsterHandAttack(0, battleMonster(0).monster().physicsAttack());
				battleMonster(0).setMonsterHandAttack(1, battleMonster(0).monster().physicsAttack());
				battleMonster(0).setMonsterPhysicsDefense(battleMonster(0).monster().physicsDefense());
				battleMonster(0).setMonsterMagicDefense(battleMonster(0).monster().magicDefense());
				battleMonster(0).setLevel(battleMonster(0).monster().level());
				battleMonster(0).setHp(battleMonster(0).monsterHp());
				battleMonster(0).setBody(battleMonster(0).monsterBody());
				battleMonster(0).setBodyAndBonus(battleMonster(0).monstermBodyAndBonus());
				battleMonster(0).setHandAttack(pl.HAND_TYPE.RIGHT_HAND, battleMonster(0).monsterHandAttack(0));
				battleMonster(0).setHandAttack(pl.HAND_TYPE.LEFT_HAND, battleMonster(0).monsterHandAttack(1));
				battleMonster(0).setPhysicsDefense(battleMonster(0).monsterPhysicsDefense());
				battleMonster(0).setMagicDefense(battleMonster(0).monsterMagicDefense());
				battleMonster(0).battleMonsterId_set(0);
				memberNumber_++;
				charNum++;
			}

			public void setBattleCharacterIdMonsterParty(ref short character_number)
			{
				int i;
				for (i = 0; i < 6 && battleMonster(i).battleCharacterId() != -1; i++)
				{
				}
				while (i < 6)
				{
					battleMonster(i).setBattleCharacterId(character_number);
					i++;
					character_number++;
				}
			}

			public void registerCharacterMng()
			{
				for (int i = 0; i < 6; i++)
				{
					if (battleMonster(i).isEnable())
					{
						battleMonster(i).registerMonster();
					}
				}
			}

			public void unregisterCharacterMng()
			{
				for (int i = 0; i < 6; i++)
				{
					if (battleMonster(i).characterMngId() != -1)
					{
						characterMng.delCharacter(battleMonster(i).characterMngId());
						battleMonster(i).setCharacterMngId(-1);
					}
				}
			}

			public void initializePlayerPosition()
			{
				for (int i = 0; i < 6; i++)
				{
					if (battleMonster(i).isEnable())
					{
						battleMonster(i).initializeData();
					}
				}
			}

			public int aliveNumber()
			{
				int num = 0;
				for (int i = 0; i < 6; i++)
				{
					if (battleMonster(i).isBattle())
					{
						num++;
					}
				}
				return num;
			}

			public BattleMonster getbattleCharacterIdMonster(short _id)
			{
				for (byte b = 0; b < 6; b++)
				{
					if (battleMonster(b).battleCharacterId() == _id)
					{
						return battleMonster(b);
					}
				}
				return null;
			}

			public sbyte getbattleCharacterIdBattleMonsterId(short _id)
			{
				for (byte b = 0; b < 6; b++)
				{
					if (battleMonster(b).battleCharacterId() == _id)
					{
						return (sbyte)b;
					}
				}
				return -1;
			}

			public byte getMinBattleMonsterId()
			{
				for (byte b = 0; b < 6; b++)
				{
					if (battleMonster(b).isBattle())
					{
						return b;
					}
				}
				return byte.MaxValue;
			}

			public byte getTopBattleMonsterId()
			{
				int[] array = new int[6] { 4, 3, 5, 1, 0, 2 };
				for (byte b = 0; b < 6; b++)
				{
					if (battleMonster(array[b]).isBattle())
					{
						return (byte)array[b];
					}
				}
				return byte.MaxValue;
			}

			public sbyte isBattleMonsterFront()
			{
				sbyte b = 3;
				sbyte b2 = 6;
				for (sbyte b3 = b; b3 < b2; b3++)
				{
					if (battleMonster(b3).isBattle())
					{
						return b3;
					}
				}
				return -1;
			}

			public sbyte isBattleMonsterBack()
			{
				sbyte b = 0;
				sbyte b2 = 3;
				for (sbyte b3 = b; b3 < b2; b3++)
				{
					if (battleMonster(b3).isBattle())
					{
						return b3;
					}
				}
				return -1;
			}

			public byte getMaxLevel()
			{
				byte b = 0;
				for (byte b2 = 0; b2 < 6; b2++)
				{
					if (battleMonster(b2).isBattle())
					{
						byte b3 = battleMonster(b2).monster().level();
						if (b3 > b)
						{
							b = b3;
						}
					}
				}
				return b;
			}

			public bool checkSameMonster(int _id)
			{
				for (int i = 0; i < 6; i++)
				{
					if (battleMonster(i).isBattle() && battleMonster(i).monsterId() != _id)
					{
						return true;
					}
				}
				return false;
			}

			public void hideMonster(bool hide)
			{
				for (int i = 0; i < 6; i++)
				{
					if (battleMonster(i).isEnable() && battleMonster(i).characterMngId() >= 0)
					{
						characterMng.setHidden(battleMonster(i).characterMngId(), hide);
					}
				}
			}

			public int targetBreakMonsterId()
			{
				for (int i = 0; i < 6; i++)
				{
					if (!battleMonster(i).isEnable())
					{
						return i;
					}
				}
				return -1;
			}

			public bool disappear(int frame)
			{
				bool result = false;
				for (int i = 0; i < 6; i++)
				{
					if (battleMonster(i).isEnable())
					{
						result = battleMonster(i).disappear(frame);
					}
				}
				return result;
			}

			public bool appear(int frame)
			{
				bool result = false;
				for (int i = 0; i < 6; i++)
				{
					if (battleMonster(i).isEnable())
					{
						result = battleMonster(i).appear(frame);
					}
				}
				return result;
			}

			public void setAlpha(int alpha, int shadow)
			{
				for (int i = 0; i < 6; i++)
				{
					if (battleMonster(i).isEnable())
					{
						battleMonster(i).setAlpha(alpha, shadow);
					}
				}
			}

			public BattleMonsterParty()
			{
				for (int i = 0; i < monster_.Length; i++)
				{
					monster_[i] = new BattleMonster();
				}
			}

			public BattleMonster battleMonster(int i)
			{
				return monster_[i];
			}

			public void battleMonster_set(int i, BattleMonster arg0)
			{
				monster_[i] = arg0;
			}

			public short memberNumber()
			{
				return memberNumber_;
			}

			public void memberNumber_inc()
			{
				memberNumber_++;
			}

			public ys.ParameterPoint<int> giftGold()
			{
				return giftGold_;
			}

			public ys.ParameterPoint<int> giftExp()
			{
				return giftExp_;
			}

			public ys.ParameterPoint<byte> giftCapacity()
			{
				return giftCapacity_;
			}

			public short dropItemId()
			{
				return dropItemId_;
			}

			public ys.ParameterPoint<int> dropItemNumber()
			{
				return dropItemNumber_;
			}

			public void addDropItemNumber()
			{
				dropItemNumber_.add(1);
			}
		}
	}
}
