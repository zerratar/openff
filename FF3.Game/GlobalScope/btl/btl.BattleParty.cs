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
		public class BattleParty
		{
			private BattlePlayer[] player_ = new BattlePlayer[4];

			private BattlePlayer npc_ = new BattlePlayer();

			private BattleMonster summon_ = new BattleMonster();

			private short memberNumber_;

			private byte escapeActionNumber_;

			private int endAsyncPlayer_;

			private bool deleteEffectFlag_;

			public void initialize()
			{
				for (int i = 0; i < 4; i++)
				{
					player_[i].initialize();
				}
				battleNpc().initialize();
				summon().initialize();
				memberNumber_ = 0;
				escapeActionNumber_ = 0;
				endAsyncPlayer_ = 0;
			}

			public void terminate()
			{
				for (int i = 0; i < 4; i++)
				{
					player_[i].terminate();
				}
				initialize();
			}

			public void execute()
			{
				for (byte b = 0; b < 4; b++)
				{
					battlePlayer(b).checkClearEffectId();
					battlePlayer(b).calcFrameCounter();
					battlePlayer(b).moveConditionEffect();
					if (battlePlayer(b).isEnable())
					{
						battlePlayer(b).updateCondition();
						battlePlayer(b).act();
						battlePlayer(b).haveWeapon(pl.HAND_TYPE.RIGHT_HAND);
						battlePlayer(b).haveWeapon(pl.HAND_TYPE.LEFT_HAND);
						battlePlayer(b).showWeapon(pl.HAND_TYPE.RIGHT_HAND);
						battlePlayer(b).showWeapon(pl.HAND_TYPE.LEFT_HAND);
					}
				}
				battleNpc().haveWeapon(pl.HAND_TYPE.RIGHT_HAND);
				battleNpc().haveWeapon(pl.HAND_TYPE.LEFT_HAND);
				battleNpc().showWeapon(pl.HAND_TYPE.RIGHT_HAND);
				battleNpc().showWeapon(pl.HAND_TYPE.LEFT_HAND);
				summon().checkClearEffectId();
			}

			public void preExecute()
			{
				for (byte b = 0; b < 4; b++)
				{
					if (battlePlayer(b).isEnable())
					{
						battlePlayer(b).preExecute();
					}
				}
				battleNpc().preExecute();
			}

			public void registerParty(ref short charNum)
			{
				for (byte b = 0; b < 4; b++)
				{
					if (pl.PlayerParty.instance().player(b).isEnable())
					{
						battlePlayer(b).onIsEnable();
						battlePlayer(b).setOrderId(b);
						battlePlayer(b).setBattleCharacterId(charNum);
						battlePlayer(b).setActionNumber(1);
						battlePlayer(b).setBreed(0);
						battlePlayer(b).setPlayerId(pl.PlayerParty.instance().player(b).playerId());
						battlePlayer(b).setPlayer(pl.PlayerParty.instance().player(b));
						battlePlayer(b).setLevel((byte)pl.PlayerParty.instance().player(b).level()
							.get());
						battlePlayer(b).setHp(pl.PlayerParty.instance().player(b).hp());
						battlePlayer(b).setCondition(pl.PlayerParty.instance().player(b).condition());
						battlePlayer(b).condition().clearConditionTime();
						battlePlayer(b).setBody(pl.PlayerParty.instance().player(b).body());
						battlePlayer(b).setBodyAndBonus(pl.PlayerParty.instance().player(b).bodyAndBonus());
						battlePlayer(b).setHandAttack(pl.HAND_TYPE.RIGHT_HAND, pl.PlayerParty.instance().player(b).handAttack(pl.HAND_TYPE.RIGHT_HAND));
						battlePlayer(b).setHandAttack(pl.HAND_TYPE.LEFT_HAND, pl.PlayerParty.instance().player(b).handAttack(pl.HAND_TYPE.LEFT_HAND));
						battlePlayer(b).setPhysicsDefense(pl.PlayerParty.instance().player(b).physicsDefense());
						battlePlayer(b).setMagicDefense(pl.PlayerParty.instance().player(b).magicDefense());
						memberNumber_++;
						charNum++;
					}
				}
			}

			public void registerCharacterMng()
			{
				for (byte b = 0; b < 4; b++)
				{
					if (battlePlayer(b).isEnable())
					{
						battlePlayer(b).registerHuman(deleteEffectFlag());
						battlePlayer(b).changeModel(flag: true);
						battlePlayer(b).changeDeath();
						if (!deleteEffectFlag())
						{
							battlePlayer(b).changeConditionEffect();
						}
						battlePlayer(b).setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION);
					}
				}
				setDeleteEffectFlag(flag: false);
			}

			public bool registerCharacterMngAsync()
			{
				while (endAsyncPlayer_ < 4)
				{
					if (battlePlayer(endAsyncPlayer_).isEnable())
					{
						if (!battlePlayer(endAsyncPlayer_).registerHumanAsync(deleteEffectFlag()))
						{
							return false;
						}
						battlePlayer(endAsyncPlayer_).changeModel(flag: true);
						battlePlayer(endAsyncPlayer_).changeDeath();
						if (!deleteEffectFlag())
						{
							battlePlayer(endAsyncPlayer_).changeConditionEffect();
						}
						battlePlayer(endAsyncPlayer_).setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION);
					}
					endAsyncPlayer_++;
				}
				if (endAsyncPlayer_ == 4)
				{
					for (int i = 0; i < 4; i++)
					{
						if (battlePlayer(i).isEnable())
						{
							characterMng.setHidden(battlePlayer(i).characterMngId(), b: false);
							if (battlePlayer(i).itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_ >= 0)
							{
								characterMng.setHidden(battlePlayer(i).itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_, b: false);
							}
							if (battlePlayer(i).itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_ >= 0)
							{
								characterMng.setHidden(battlePlayer(i).itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_, b: false);
							}
						}
					}
					setDeleteEffectFlag(flag: false);
					endAsyncPlayer_ = 0;
					return true;
				}
				return false;
			}

			public void unregisterCharacterMng()
			{
				for (byte b = 0; b < 4; b++)
				{
					if (battlePlayer(b).characterMngId() != -1)
					{
						characterMng.delCharacter(battlePlayer(b).characterMngId());
						battlePlayer(b).setCharacterMngId(-1);
						battlePlayer(b).unregisterWeapon(pl.HAND_TYPE.RIGHT_HAND);
						battlePlayer(b).unregisterWeapon(pl.HAND_TYPE.LEFT_HAND);
					}
				}
				if (battleNpc().characterMngId() != -1)
				{
					characterMng.delCharacter(battleNpc().characterMngId());
					battleNpc().setCharacterMngId(-1);
					battleNpc().unregisterWeapon(pl.HAND_TYPE.RIGHT_HAND);
					battleNpc().unregisterWeapon(pl.HAND_TYPE.LEFT_HAND);
				}
			}

			public void initializePlayerPosition()
			{
				for (byte b = 0; b < 4; b++)
				{
					if (battlePlayer(b).isEnable())
					{
						byte b2 = battlePlayer(b).player().formationType();
						byte b3 = (byte)pl.PlayerParty.instance().playerOrder(battlePlayer(b).playerId());
						if (battlePlayer(b).flag(PLAYER_FLAG.PF_JUMP))
						{
							VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
							characterMng.getPosition(battlePlayer(b).characterMngId(), fnd_reuse_pos);
							fnd_reuse_pos.y = 40960 * JUMP_END_FRAME;
							characterMng.setPosition(battlePlayer(b).characterMngId(), fnd_reuse_pos);
							if (battlePlayer(b).itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_ >= 0)
							{
								characterMng.setPosition(battlePlayer(b).itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_, fnd_reuse_pos);
							}
							if (battlePlayer(b).itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_ >= 0)
							{
								characterMng.setPosition(battlePlayer(b).itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_, fnd_reuse_pos);
							}
						}
						else
						{
							characterMng.setPosition(battlePlayer(b).characterMngId(), PlayerPosition[b2][b3]);
						}
						characterMng.setRotation(battlePlayer(b).characterMngId(), 0, PlayerRotationY, 0);
						if (battlePlayer(b).condition().isFrog())
						{
							battlePlayer(b).changeFrog(flag: false);
						}
						else if (battlePlayer(b).condition().isLilliput())
						{
							battlePlayer(b).changeLilliput(flag: false);
						}
					}
				}
			}

			public void clearPoolSkillExp()
			{
				for (int i = 0; i < 4; i++)
				{
					if (battlePlayer(i).isEnable())
					{
						battlePlayer(i).player().jobManager().clearJobPoolSkillExp();
						battlePlayer(i).player().skillManager().skill(pl.GET_SKILL_TYPE.GET_RIGHT_HAND)
							.clearPoolSkillExp();
						battlePlayer(i).player().skillManager().skill(pl.GET_SKILL_TYPE.GET_LEFT_HAND)
							.clearPoolSkillExp();
					}
				}
			}

			public bool disappear(int frame)
			{
				bool result = false;
				for (int i = 0; i < 4; i++)
				{
					if (battlePlayer(i).isEnable())
					{
						result = battlePlayer(i).disappear(frame);
					}
				}
				return result;
			}

			public bool appear(int frame)
			{
				bool result = false;
				for (int i = 0; i < 4; i++)
				{
					if (battlePlayer(i).isEnable() && !battlePlayer(i).flag(PLAYER_FLAG.PF_JUMP))
					{
						result = battlePlayer(i).appear(frame);
					}
				}
				return result;
			}

			public void setAlpha(int alpha, int shadow)
			{
				for (int i = 0; i < 4; i++)
				{
					if (battlePlayer(i).isEnable() && !battlePlayer(i).flag(PLAYER_FLAG.PF_JUMP))
					{
						battlePlayer(i).setAlpha(alpha, shadow);
					}
				}
			}

			public int battlePlayerId(BattlePlayer player)
			{
				for (int i = 0; i < 4; i++)
				{
					if (battlePlayer(i) == player)
					{
						return i;
					}
				}
				return -1;
			}

			public byte getMinBattlePlayerId()
			{
				for (byte b = 0; b < 4; b++)
				{
					if (battlePlayer(b).isBattle() && !battlePlayer(b).flag(PLAYER_FLAG.PF_JUMP))
					{
						return b;
					}
				}
				return byte.MaxValue;
			}

			public BattlePlayer getbattleCharacterIdPlayer(short battleCharacterId)
			{
				for (byte b = 0; b < 4; b++)
				{
					if (battlePlayer(b).battleCharacterId() == battleCharacterId)
					{
						return battlePlayer(b);
					}
				}
				return null;
			}

			public short getbattleCharacterIdBattlePlayerId(short battleCharacterId)
			{
				for (byte b = 0; b < 4; b++)
				{
					if (battlePlayer(b).battleCharacterId() == battleCharacterId)
					{
						return b;
					}
				}
				return -1;
			}

			public int aliveNumber()
			{
				int num = 0;
				for (byte b = 0; b < 4; b++)
				{
					if (battlePlayer(b).isBattle())
					{
						num++;
					}
				}
				return num;
			}

			public byte getMinLevel()
			{
				byte b = (byte)pl.PLAYER_LEVEL_MAX;
				for (byte b2 = 0; b2 < 4; b2++)
				{
					if (battlePlayer(b2).isEnable())
					{
						byte b3 = (byte)battlePlayer(b2).player().level().get();
						if (b3 < b)
						{
							b = b3;
						}
					}
				}
				return b;
			}

			public BattlePlayer serchExecuteCoverMan(BaseBattleCharacter target)
			{
				if (target == null)
				{
					return null;
				}
				if (target.breed() != 0)
				{
					return null;
				}
				BattlePlayer battlePlayer = static_cast<BattlePlayer>(target);
				if (!battlePlayer.isBattle())
				{
					return null;
				}
				if (!battlePlayer.condition().isNearDeath())
				{
					return null;
				}
				int[] btl_reuse_player_id = btl.btl_reuse_player_id;
				int num = 0;
				for (int i = 0; i < 4; i++)
				{
					btl_reuse_player_id[i] = -1;
					if (this.battlePlayer(i).battleCharacterId() != target.battleCharacterId() && this.battlePlayer(i).isEnable() && this.battlePlayer(i).checkExecuteCover())
					{
						btl_reuse_player_id[num] = i;
						num++;
					}
				}
				if (num == 0)
				{
					return null;
				}
				int num2 = (int)ds.RandomNumber.rand32((uint)num);
				return this.battlePlayer(btl_reuse_player_id[num2]);
			}

			public bool isTargetDrain(BaseBattleCharacter target)
			{
				if (!target.isDrain())
				{
					return false;
				}
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer = this.battlePlayer(i);
					if (battlePlayer.isBattle() && !battlePlayer.flag(PLAYER_FLAG.PF_JUMP) && target.battleCharacterId() != battlePlayer.battleCharacterId())
					{
						return false;
					}
				}
				return true;
			}

			public bool isPartyJump()
			{
				for (int i = 0; i < 4; i++)
				{
					if (battlePlayer(i).isBattle() && !battlePlayer(i).flag(PLAYER_FLAG.PF_JUMP))
					{
						return true;
					}
				}
				return false;
			}

			public BattleParty()
			{
				for (int i = 0; i < player_.Length; i++)
				{
					player_[i] = new BattlePlayer();
				}
				deleteEffectFlag_ = false;
				endAsyncPlayer_ = 0;
			}

			public BattlePlayer battlePlayer(int i)
			{
				return player_[i];
			}

			public short memberNumber()
			{
				return memberNumber_;
			}

			public byte escapeActionNumber()
			{
				return escapeActionNumber_;
			}

			public void addEscapeActionNumber()
			{
				escapeActionNumber_++;
			}

			public bool deleteEffectFlag()
			{
				return deleteEffectFlag_;
			}

			public void setDeleteEffectFlag(bool flag)
			{
				deleteEffectFlag_ = flag;
			}

			public BattlePlayer battleNpc()
			{
				return npc_;
			}

			public BattleMonster summon()
			{
				return summon_;
			}
		}
	}
}
