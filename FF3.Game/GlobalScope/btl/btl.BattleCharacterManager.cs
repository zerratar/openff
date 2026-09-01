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
		public class BattleCharacterManager
		{
			private BattleParty playerParty_ = new BattleParty();

			private BattleMonsterParty monsterParty_ = new BattleMonsterParty();

			private short characterNumber_;

			private BaseBattleCharacter[] order_ = new BaseBattleCharacter[12];

			public BattleCharacterManager()
			{
				for (int i = 0; i < 12; i++)
				{
					order_[i] = null;
				}
			}

			public void initialize()
			{
				characterNumber_ = 0;
				clearOrder();
				playerParty_.initialize();
				monsterParty_.initialize();
				setBattleCharacter();
				registerCharacterMng();
				setInitPosition();
			}

			public void terminate()
			{
				unregisterCharacterMng();
				characterNumber_ = 0;
				clearOrder();
				playerParty_.terminate();
				monsterParty_.terminate();
			}

			public void execute()
			{
				playerParty_.execute();
				monsterParty_.execute();
			}

			public void preExecute()
			{
				playerParty_.preExecute();
				monsterParty_.preExecute();
			}

			public void setBattleCharacter()
			{
				playerParty_.registerParty(ref characterNumber_);
				monsterParty_.registerParty(ref characterNumber_);
			}

			public void registerCharacterMng()
			{
				playerParty_.registerCharacterMng();
				monsterParty_.registerCharacterMng();
				playerParty_.disappear(1);
			}

			public void unregisterCharacterMng()
			{
				playerParty_.unregisterCharacterMng();
				monsterParty_.unregisterCharacterMng();
			}

			public void setInitPosition()
			{
				playerParty_.initializePlayerPosition();
				monsterParty_.initializePlayerPosition();
			}

			public BaseBattleCharacter getBaseBattleCharacterFromBreed(short battleCharacterId)
			{
				if (battleCharacterId < 0)
				{
					return null;
				}
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer = playerParty().battlePlayer(i);
					if (battlePlayer != null && battlePlayer.isEnable() && battlePlayer.battleCharacterId() == battleCharacterId)
					{
						return static_cast<BaseBattleCharacter>(battlePlayer);
					}
				}
				for (int i = 0; i < 6; i++)
				{
					BattleMonster battleMonster = monsterParty().battleMonster(i);
					if (battleMonster != null && battleMonster.isEnable() && battleMonster.battleCharacterId() == battleCharacterId)
					{
						return static_cast<BaseBattleCharacter>(battleMonster);
					}
				}
				return null;
			}

			public void setMonsterGroupTarget(BaseBattleCharacter character)
			{
				int num = character.targetId(0);
				character.clearTargetId();
				BaseBattleCharacter baseBattleCharacterFromBreed = getBaseBattleCharacterFromBreed((short)num);
				BattleMonster battleMonster = static_cast<BattleMonster>(baseBattleCharacterFromBreed);
				short num2 = battleMonster.monsterId();
				for (int i = 0; i < 6; i++)
				{
					if (monsterParty().battleMonster(i).isBattle() && num2 == monsterParty().battleMonster(i).monsterId())
					{
						character.setTargetId(i, monsterParty().battleMonster(i).battleCharacterId());
					}
				}
			}

			public void setMonsterAllTarget(BaseBattleCharacter character)
			{
				character.clearTargetId();
				for (int i = 0; i < 6; i++)
				{
					if (monsterParty().battleMonster(i).isBattle())
					{
						character.setTargetId(i, monsterParty().battleMonster(i).battleCharacterId());
					}
				}
			}

			public void setPlayerAllTarget(BaseBattleCharacter character, int recover)
			{
				character.clearTargetId();
				for (int i = 0; i < 4; i++)
				{
					if (recover == 0)
					{
						if (!playerParty().battlePlayer(i).isBattle())
						{
							continue;
						}
					}
					else if (!playerParty().battlePlayer(i).isEnable())
					{
						continue;
					}
					if (!playerParty().battlePlayer(i).flag(PLAYER_FLAG.PF_JUMP))
					{
						character.setTargetId(i, playerParty().battlePlayer(i).battleCharacterId());
					}
				}
			}

			public int getTargetType(BaseBattleCharacter character)
			{
				for (int i = 0; i < 12; i++)
				{
					if (character.targetId(i) >= 0)
					{
						BaseBattleCharacter baseBattleCharacterFromBreed = getBaseBattleCharacterFromBreed(character.targetId(i));
						if (baseBattleCharacterFromBreed != null)
						{
							return baseBattleCharacterFromBreed.breed();
						}
					}
				}
				return -1;
			}

			public int getTrueExp()
			{
				int num = monsterParty().giftExp().get() / playerParty().aliveNumber();
				if (num == 0)
				{
					num = 1;
				}
				return num;
			}

			public int breakMonsterCharatcerId(int monster_id)
			{
				BattleMonster battleMonster = monsterParty().battleMonster(monster_id);
				if (battleMonster != null && battleMonster.battleCharacterId() != -1)
				{
					return battleMonster.battleCharacterId();
				}
				characterNumber_++;
				return characterNumber_;
			}

			public bool isReflected()
			{
				for (int i = 0; i < 6; i++)
				{
					if (monsterParty_.battleMonster(i).isBattle() && monsterParty_.battleMonster(i).magicFlag(MAGIC_FLAG.MF_REFLECT))
					{
						return true;
					}
				}
				for (int j = 0; j < 4; j++)
				{
					if (playerParty_.battlePlayer(j).isEnable() && playerParty_.battlePlayer(j).magicFlag(MAGIC_FLAG.MF_REFLECT))
					{
						return true;
					}
				}
				return false;
			}

			public void changeMagicColor()
			{
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = getBaseBattleCharacterFromBreed((short)i);
					if (baseBattleCharacterFromBreed != null && baseBattleCharacterFromBreed.isEnable() && baseBattleCharacterFromBreed.breed() != 2)
					{
						baseBattleCharacterFromBreed.changePlayerColor();
					}
				}
			}

			public int useAssistPlayer()
			{
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer = playerParty().battlePlayer(i);
					if (battlePlayer != null && battlePlayer.isBattle() && !battlePlayer.flag(PLAYER_FLAG.PF_JUMP) && battlePlayer.magicFlagAll() != 0)
					{
						return battlePlayer.battleCharacterId();
					}
				}
				return -1;
			}

			public BattleParty playerParty()
			{
				return playerParty_;
			}

			public BattleMonsterParty monsterParty()
			{
				return monsterParty_;
			}

			public short characterNumber()
			{
				return characterNumber_;
			}

			public BaseBattleCharacter order(int i)
			{
				return order_[i];
			}

			public void setOrder(int i, BaseBattleCharacter character)
			{
				order_[i] = character;
			}

			public void clearOrder()
			{
				for (int i = 0; i < 12; i++)
				{
					order_[i] = null;
				}
			}
		}
	}
}
