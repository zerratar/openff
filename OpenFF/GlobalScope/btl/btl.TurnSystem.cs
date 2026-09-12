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
		public class TurnSystem
		{
			public enum Phase
			{
				Initialize,
				Execute,
				MonsterExecute,
				CommonExecute,
				Terminate,
				PhaseMax
			}

			public enum CHECK_END_POINT
			{
				TARGET_NAME = 1,
				TARGET_HP = 2,
				TARGET_WEAK = 4,
				TARGET_END = 8
			}

			public const int ERR_ID = -1;

			public const Phase Initialize = Phase.Initialize;

			public const Phase Execute = Phase.Execute;

			public const Phase MonsterExecute = Phase.MonsterExecute;

			public const Phase CommonExecute = Phase.CommonExecute;

			public const Phase Terminate = Phase.Terminate;

			public const Phase PhaseMax = Phase.PhaseMax;

			public const int COMMON_START = 0;

			public const int CONDITION_CHECK = 1;

			public const int COMMON_POISON_START = 2;

			public const int COMMON_POISON = 3;

			public const int COMMON_POISON_END = 4;

			public const int COMMON_STONE_START = 5;

			public const int COMMON_CLEAR_CONDTION = 6;

			public const int COMMON_END = 7;

			public const int COMMON_MAX = 8;

			public const int COVER_FIRE_START = 0;

			public const int COVER_FIRE_HELP_END = 1;

			public const int COVER_FIRE_LOAD_EFFECT = 2;

			public const int COVER_FIRE_IS_LOAD_EFFECT = 3;

			public const int COVER_FIRE_EFFECT_START = 4;

			public const int COVER_FIRE_IS_EFFECT = 5;

			public const int COVER_FIRE_2D_START = 6;

			public const int COVER_FIRE_IS_2D = 7;

			public const int COVER_FIRE_EXECUTE = 8;

			public const int COVER_FIRE_END = 9;

			public const int COVER_FIRE_MAX = 10;

			public const CHECK_END_POINT TARGET_NAME = CHECK_END_POINT.TARGET_NAME;

			public const CHECK_END_POINT TARGET_HP = CHECK_END_POINT.TARGET_HP;

			public const CHECK_END_POINT TARGET_WEAK = CHECK_END_POINT.TARGET_WEAK;

			public const CHECK_END_POINT TARGET_END = CHECK_END_POINT.TARGET_END;

			private static int[] DROP_ITEM_ODDS = new int[8] { 1900, 1900, 1900, 1900, 900, 900, 450, 150 };

			private static int MAGIC_WAIT_FRAME = 10;

			public static int GUARD_WAIT_FRAME = 15;

			public static int DRAW_HELP_WINDOW_FRAME = 40;

			public static int IDLE_FRAME = 10;

			private static int WAIT_COMMON_START_FRAME = 10;

			public static uint EndPlayerProcess = 1u;

			public static uint EndEnemyProcess = 2u;

			public static uint End2DProcess = 4u;

			public static uint EndEffectProcess = 8u;

			public static uint EndEnemyMotionProcess = 16u;

			public static uint StartEffectProcess = 32u;

			public static uint Start2DProcess = 64u;

			private static uint StartEnemyDeadProcess = 128u;

			private static uint EndEnemyDeadProcess = 256u;

			public static uint StartSEProcess = 512u;

			public static uint ExecuteCoverProcess = 1024u;

			public static uint PlayEffect = 2048u;

			private static uint FlashProcess = 4096u;

			public static uint CHECK_SUCCESS = 8192u;

			private static uint CHECK_START = 16384u;

			private static uint CHECK_END = 32768u;

			private static uint DRAIN_START = 65536u;

			private static uint DRAIN_END = 131072u;

			private static uint REFLECT_TARGET = 262144u;

			public static uint START_MAGIC_EFFECT = 524288u;

			public static uint END_MAGIC_EFFECT = 1048576u;

			private static int EFPID_MAX = 5;

			private int state_;

			private Phase phase_;

			private int frameCounter_;

			private int workCounter_;

			private int monsterCounter_;

			private int effectCamera_;

			private bool isPlayerEscape_;

			private uint checkFlag_;

			private BaseBattleCharacter nowCharacter_;

			private BattleCharacterManager characterManager_;

			private SummonDataManager summonDataManager_;

			private GeographyManager geographyManager_;

			private BaseBattleCharacter counterCharacter_;

			private BattlePlayer coverPlayer_;

			private BattleMonster breakRootMonster_;

			private BattleMonster breakAfterMonster_;

			private int attackNumber_;

			private int effectCounter_;

			private int deadFlashCounter_;

			private int deadStartCounter_;

			private int commonState_;

			private PlayerWindow playerWindow_;

			public BattleCalculation calc_ = new BattleCalculation();

			private BattleCalculation overissueCalc_ = new BattleCalculation();

			public int[] overissueTargetId_ = new int[12];

			private BaseBattleCharacter commonAttacker_ = new BaseBattleCharacter();

			public dgs.ScreenFlash flash_ = new dgs.ScreenFlash();

			public PlayerTurnSystem playerTurn_ = new PlayerTurnSystem();

			private MonsterTurnSystem monsterTurn_ = new MonsterTurnSystem();

			private static int WEAK_MESSAGE_ID_MAX = 10;

			private short weakType_;

			private int checkState_;

			private int checkActionType_;

			private ds.Vector<int, ds.OrderSavedErasePolicy<int>> weakList_ = new ds.Vector<int, ds.OrderSavedErasePolicy<int>>(WEAK_MESSAGE_ID_MAX);

			public void initializeAll()
			{
				state_ = 0;
				phase_ = Phase.Initialize;
				frameCounter_ = 0;
				workCounter_ = 0;
				monsterCounter_ = 0;
				effectCamera_ = 0;
				isPlayerEscape_ = false;
				checkFlag_ = 0u;
				nowCharacter_ = null;
				characterManager_ = null;
				summonDataManager_ = null;
				geographyManager_ = null;
				counterCharacter_ = null;
				coverPlayer_ = null;
				breakRootMonster_ = null;
				breakAfterMonster_ = null;
				attackNumber_ = 0;
				effectCounter_ = 0;
				deadFlashCounter_ = 0;
				deadStartCounter_ = 0;
				commonState_ = 0;
				playerWindow_ = null;
				calc_.clearDamageAll();
				overissueCalc_.clearDamageAll();
				for (int i = 0; i < 12; i++)
				{
					overissueTargetId_[i] = -1;
				}
				commonAttacker_.initialize();
				flash_.initialize();
				playerTurn_.initializeAll();
				monsterTurn_.initializeAll();
			}

			public void initializeTurn()
			{
				phase_ = Phase.Initialize;
				frameCounter_ = 0;
				effectCounter_ = 0;
				state_ = 0;
				isPlayerEscape_ = false;
				effectCamera_ = 0;
				deadFlashCounter_ = 0;
				deadStartCounter_ = 0;
				checkActionType_ = 0;
				monsterCounter_ = 0;
				breakRootMonster_ = null;
				breakAfterMonster_ = null;
				flash_.initialize();
				flash_.endFlash();
				overissueCalc_.clearDamageAll();
				for (int i = 0; i < 12; i++)
				{
					overissueTargetId_[i] = -1;
				}
				commonState_ = 0;
				clearCheckFlagAll();
				if (nowCharacter() != null)
				{
					nowCharacter().setAttackSuccess(0, flag: false);
					nowCharacter().setAttackSuccess(1, flag: false);
				}
				clearFlagInitializeTurn();
				clearResult();
				if (nowCharacter() == null || nowCharacter().isActionEnd())
				{
					setPhase(Phase.Terminate);
					return;
				}
				nowCharacter().actionNumber_dec();
				if (nowCharacter().actionNumber() <= 0)
				{
					nowCharacter().onIsActionEnd();
				}
				for (int j = 0; j < 12; j++)
				{
					for (int k = 0; k < 12; k++)
					{
						BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed((short)k);
						if (baseBattleCharacterFromBreed != null)
						{
							baseBattleCharacterFromBreed.setReflectTargetId(-1);
							if (nowCharacter().targetId(j) == baseBattleCharacterFromBreed.battleCharacterId() && !isSelectTarget(nowCharacter(), baseBattleCharacterFromBreed))
							{
								nowCharacter().targetId_set(j, -1);
								break;
							}
						}
					}
				}
				switch (nowCharacter().breed())
				{
				case 0:
					playerTurn_.initialize(this);
					break;
				case 1:
					monsterTurn_.initialize(this);
					break;
				}
			}

			public void clearFlagInitializeTurn()
			{
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed((short)i);
					if (baseBattleCharacterFromBreed != null)
					{
						baseBattleCharacterFromBreed.setAttackNumber(0);
						baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_MISS);
						baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_EFFECT);
						baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_RECOVER);
						baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_2D);
						baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_CREATE_2D);
						baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_COVER);
						baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_OVERISSUE);
						baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_PITCH);
						baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_ABSORB);
						baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_EXPLOSION);
						baseBattleCharacterFromBreed.clearMagicFlag(MAGIC_FLAG.MF_SLEEP);
						baseBattleCharacterFromBreed.clearMagicFlag(MAGIC_FLAG.MF_RECOVER_SLEEP);
						baseBattleCharacterFromBreed.clearMagicFlag(MAGIC_FLAG.MF_CONFUSION);
						baseBattleCharacterFromBreed.clearMagicFlag(MAGIC_FLAG.MF_RECOVER_CONFUSION);
						baseBattleCharacterFromBreed.changeCondition().clearCondition();
					}
				}
			}

			public bool terminateTurn()
			{
				clearFlagTerminateTurn();
				if (nowCharacter() != null)
				{
					switch (nowCharacter().breed())
					{
					case 0:
						playerTurn_.terminate(this);
						break;
					case 1:
						monsterTurn_.terminate(this);
						break;
					}
				}
				BattleEffect.instance().deleteAll();
				BattleEffect.instance().endEfp();
				OutsideToBattle.getInstance().offMagicDefenseInvalidation();
				flash_.terminate();
				return true;
			}

			public void clearFlagTerminateTurn()
			{
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed((short)i);
					if (baseBattleCharacterFromBreed != null)
					{
						baseBattleCharacterFromBreed.setAttackNumber(0);
						baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_MISS);
						baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_EFFECT);
						baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_RECOVER);
						baseBattleCharacterFromBreed.changeCondition().clearCondition();
					}
				}
			}

			public void executeTurn()
			{
				if (nowCharacter() == null)
				{
					setPhase(Phase.Terminate);
					return;
				}
				frameCounter_++;
				switch (nowCharacter().breed())
				{
				case 0:
					playerTurn_.execute(this);
					break;
				case 1:
					monsterTurn_.execute(this);
					break;
				}
				flash_.draw();
			}

			public void monsterExecute()
			{
				if (breakRootMonster_ != null)
				{
					monsterTurn_.executeBreakMonster(this);
				}
				else if (++monsterCounter_ > 10)
				{
					monsterCounter_ = 0;
					setPhase(Phase.Terminate);
				}
			}

			public bool commonExecute()
			{
				switch (commonState_)
				{
				case 0:
					startCommon();
					return false;
				case 1:
					checkCondition();
					return false;
				case 2:
					poisonCommonStart();
					return false;
				case 3:
					poisonCommon();
					return false;
				case 4:
					poisonCommonEnd();
					return false;
				case 5:
					stoneCommon();
					return false;
				case 6:
					clearConditionCommon();
					return false;
				case 7:
					endCommon();
					return true;
				default:
					return true;
				}
			}

			public bool coverFire()
			{
				switch (state_)
				{
				case 0:
					if (OutsideToBattle.getInstance().battleOpeningType() == BATTLE_OPENING_TYPE.BACK_ATTACK)
					{
						state_ = 9;
					}
					else if (wld.CWorldOutSideData.getInstance().VehicleData().getPreRidingOnVehicleNo() == pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_INVINSIBLE)
					{
						commonAttacker_.clearTargetId();
						commonAttacker_.clearEffectIdAll();
						clearFlagInitializeTurn();
						characterManager().setMonsterAllTarget(commonAttacker_);
						calc_.clearDamageAll();
						calc_.calcCoverFire(characterManager_, commonAttacker_);
						clearFlagInitializeTurn();
						Battle2DManager.instance().helpWindow().createHelpWindow(64, 0, 0);
						Battle2DManager.instance().helpWindow().setMsdHandle(0);
						BattleSE.instance().load(206);
						workCounter_ = DRAW_HELP_WINDOW_FRAME;
						state_ = 1;
					}
					else
					{
						state_ = 9;
					}
					break;
				case 1:
					if (--workCounter_ < 0)
					{
						Battle2DManager.instance().helpWindow().releaseHelpWindow();
						BattleEffect.instance().addEfp(434);
						workCounter_ = 0;
						state_ = 3;
					}
					break;
				case 3:
					if (TexDivideLoader.getSingleton().tdlIsEmpty())
					{
						state_ = 4;
					}
					break;
				case 4:
				{
					ys.Effects effects = new ys.Effects(0, 0, 434, 1, arg4: false);
					createAllMagicEffect(effects, commonAttacker_);
					BattleSE.instance().play(206, 0);
					state_ = 5;
					break;
				}
				case 5:
					if (commonAttacker_.isClearAllEffect())
					{
						state_ = 6;
					}
					break;
				case 6:
				{
					for (int i = 0; i < 12; i++)
					{
						BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed(commonAttacker_.targetId(i));
						if (baseBattleCharacterFromBreed != null)
						{
							createDamage(baseBattleCharacterFromBreed, null);
						}
					}
					setCheckFlag(Start2DProcess);
					clearCheckFlag(End2DProcess);
					state_ = 7;
					break;
				}
				case 7:
					if (checkEnd2D())
					{
						setCheckFlag(End2DProcess);
						setCheckFlag(EndPlayerProcess);
						setCheckFlag(EndEffectProcess);
						clearCheckFlag(EndEnemyProcess);
						clearCheckFlag(CHECK_START);
						state_ = 8;
					}
					break;
				case 8:
					if (deadCharacters(commonAttacker_))
					{
						state_ = 9;
					}
					flash_.draw();
					break;
				case 9:
					BattleEffect.instance().deleteAll();
					BattleEffect.instance().endEfp();
					BattleSE.instance().free();
					state_ = 0;
					return true;
				}
				return false;
			}

			public void setState(int state)
			{
				if (state >= 0)
				{
					state_ = state;
				}
			}

			public bool createEffect(int currentFrame, ys.Effects effect, BaseBattleCharacter target, short offset, int randam, int draw)
			{
				if (target == null)
				{
					return false;
				}
				if (currentFrame == effect.frameCounter_)
				{
					if (effect.category_ == 0 && effect.member_ == 0)
					{
						return true;
					}
					if (draw == 0)
					{
						return true;
					}
					int num = -1;
					num = ((target.flag(PLAYER_FLAG.PF_GUARD) || target.flag(PLAYER_FLAG.PF_MORE_GUARD)) ? BattleEffect.instance().create(201, 1) : ((!target.flag(PLAYER_FLAG.PF_COVER)) ? BattleEffect.instance().create(effect.category_, effect.member_) : BattleEffect.instance().create(247, 1)));
					if (num != -1)
					{
						setHitEffectPosition(target, num, offset, randam);
						return true;
					}
				}
				return false;
			}

			public bool createMagicEffect(int currentFrame, ys.Effects effect, BaseBattleCharacter target, short offset)
			{
				if (target == null)
				{
					return false;
				}
				if (currentFrame == effect.frameCounter_)
				{
					if (effect.category_ == 0 && effect.member_ == 0)
					{
						return true;
					}
					int num = -1;
					num = BattleEffect.instance().create(effect.category_, effect.member_);
					if (num != -1)
					{
						setHitEffectPosition(target, num, offset, 0);
						return true;
					}
				}
				return false;
			}

			public bool createAllMagicEffect(ys.Effects effects, BaseBattleCharacter attacker)
			{
				if (effects.category_ == 0 && effects.member_ == 0)
				{
					return true;
				}
				int num = -1;
				num = BattleEffect.instance().create(effects.category_, effects.member_);
				if (num != -1)
				{
					for (int i = 0; i < 12; i++)
					{
						BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed(attacker.targetId(i));
						if (baseBattleCharacterFromBreed != null)
						{
							if (baseBattleCharacterFromBreed.breed() == 1)
							{
								setEffectPosition(attacker, num, AllEnemyMagicPosition);
							}
							else
							{
								setEffectPosition(attacker, num, AllPlayerMagicPosition);
							}
							return true;
						}
					}
				}
				return false;
			}

			public bool createEffectAndSetPosition(BaseBattleCharacter character, int category, int member, VecFx32 position)
			{
				int num = BattleEffect.instance().create(category, member);
				if (num == -1)
				{
					return false;
				}
				setEffectPosition(character, num, position);
				return true;
			}

			public bool playSE(int currentFrame, ys.Effects se, int guard, int miss, int critical)
			{
				if (currentFrame == se.frameCounter_)
				{
					if (miss == 0)
					{
						if (critical != 0)
						{
							BattleSE.instance().play(200, 5);
						}
						else
						{
							switch (guard)
							{
							case 0:
								BattleSE.instance().play(se.category_, se.member_);
								break;
							case 1:
								BattleSE.instance().play(200, 10);
								break;
							case 2:
								BattleSE.instance().play(200, 10);
								break;
							}
						}
					}
					else
					{
						BattleSE.instance().playMissSE();
					}
					return true;
				}
				return false;
			}

			public bool playFlash(int currentFrame, int flashFrame, BaseBattleCharacter target)
			{
				if (target == null)
				{
					return false;
				}
				if (currentFrame == flashFrame)
				{
					characterMng.setFlash(target.characterMngId());
					return true;
				}
				return false;
			}

			public bool startDamageAction(int currentFrame, int startFrame, BaseBattleCharacter target)
			{
				if (target == null)
				{
					return false;
				}
				if (target.flag(PLAYER_FLAG.PF_ERASE))
				{
					return false;
				}
				if (currentFrame == startFrame)
				{
					if (target.breed() == 0)
					{
						if (target.battleCharacterId() != nowCharacter().battleCharacterId())
						{
							BattlePlayer battlePlayer = static_cast<BattlePlayer>(target);
							if (battlePlayer.flag(PLAYER_FLAG.PF_MISS))
							{
								return true;
							}
							if (battlePlayer.flag(PLAYER_FLAG.PF_GUARD) || battlePlayer.flag(PLAYER_FLAG.PF_MORE_GUARD))
							{
								battlePlayer.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_GUARD);
							}
							else if (target.flag(PLAYER_FLAG.PF_RECOVER))
							{
								battlePlayer.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION);
							}
							else
							{
								battlePlayer.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_DAMAGE);
							}
							return true;
						}
					}
					else if (target.breed() == 1)
					{
						return true;
					}
				}
				return false;
			}

			public void createChangeConditionEffect()
			{
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed((short)i);
					if (baseBattleCharacterFromBreed != null && baseBattleCharacterFromBreed.isBattle() && baseBattleCharacterFromBreed.breed() != 0 && baseBattleCharacterFromBreed.breed() != 2 && baseBattleCharacterFromBreed.breed() != 3)
					{
						ys.Effects effects = selectChangeConditionEffect(baseBattleCharacterFromBreed);
						if (effects.member_ != 0)
						{
							createMagicEffect(0, effects, baseBattleCharacterFromBreed, 0);
						}
					}
				}
			}

			public bool isEndChangeConditionEffect()
			{
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed((short)i);
					if (baseBattleCharacterFromBreed != null && baseBattleCharacterFromBreed.isBattle() && baseBattleCharacterFromBreed.breed() != 0 && baseBattleCharacterFromBreed.breed() != 2 && baseBattleCharacterFromBreed.breed() != 3 && !baseBattleCharacterFromBreed.isClearAllEffect())
					{
						return false;
					}
				}
				return true;
			}

			public ys.Effects selectChangeConditionEffect(BaseBattleCharacter target)
			{
				ys.Effects effects = new ys.Effects(0, 0, 435, 0, arg4: false);
				if (target.changeCondition().isPoison())
				{
					effects.member_ = 3;
				}
				else if (target.changeCondition().isDarkness())
				{
					effects.member_ = 4;
				}
				else if (target.changeCondition().isSilence())
				{
					effects.member_ = 6;
				}
				else if (target.changeCondition().isNearStone())
				{
					effects.member_ = 10;
				}
				else if (target.changeCondition().isSleep())
				{
					effects.member_ = 12;
				}
				else if (target.changeCondition().isParalysis())
				{
					effects.member_ = 13;
				}
				else if (target.changeCondition().isConfusion())
				{
					effects.member_ = 11;
				}
				return effects;
			}

			public void setMagicStartEffect(int magicId)
			{
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter((short)magicId);
				switch ((itm.MAGIC_SYSTEM)magicParameter.system())
				{
				case itm.MAGIC_SYSTEM.MAGIC_WHITE:
					BattleEffect.instance().addEfp(408);
					break;
				case itm.MAGIC_SYSTEM.MAGIC_BLACK:
					BattleEffect.instance().addEfp(407);
					break;
				case itm.MAGIC_SYSTEM.MAGIC_SUMMONS:
					BattleEffect.instance().addEfp(243);
					break;
				}
			}

			public int magicStartEffect(int magicId)
			{
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter((short)magicId);
				return (itm.MAGIC_SYSTEM)magicParameter.system() switch
				{
					itm.MAGIC_SYSTEM.MAGIC_WHITE => 408, 
					itm.MAGIC_SYSTEM.MAGIC_BLACK => 407, 
					itm.MAGIC_SYSTEM.MAGIC_SUMMONS => 243, 
					_ => -1, 
				};
			}

			public void setCounterMan(BaseBattleCharacter attacker, BaseBattleCharacter target)
			{
				// PORT: a hero with the Counterattack passive in play (the mods' mastery progression, FF5's
				// Counter) strikes back at every physical attack, as the Retaliate stance does for a turn -
				// a plain attack, without the stance's doubled damage (that follows PF_COUNTER).
				bool passive = target != null && target.isBattle() && target.breed() == 0 && OpenFF.Client.ProgressionLayer.HasInBattle(target, 15);
				if (target != null && target.isBattle() && ((target.condition().isCounter() && target.flag(PLAYER_FLAG.PF_COUNTER)) || passive))
				{
					counterCharacter_ = target;
					counterCharacter_.setActionNumber(1);
					counterCharacter_.offIsActionEnd();
					counterCharacter_.clearTargetId();
					counterCharacter_.setTargetId(0, attacker.battleCharacterId());
					if (counterCharacter_.breed() == 0)
					{
						counterCharacter_.setActionId(25);
					}
				}
			}

			public void setTargetAllEnemyAfterDeadLastBoss(BaseBattleCharacter attacker)
			{
				bool flag = false;
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed(attacker.targetId(i));
					if (baseBattleCharacterFromBreed != null && !baseBattleCharacterFromBreed.isBattle() && baseBattleCharacterFromBreed.breed() == 1)
					{
						BattleMonster battleMonster = static_cast<BattleMonster>(baseBattleCharacterFromBreed);
						if (battleMonster != null && battleMonster.monster() != null && (battleMonster.monsterId() == 225 || battleMonster.monsterId() == 226))
						{
							flag = true;
						}
					}
				}
				if (!flag)
				{
					return;
				}
				for (int j = 0; j < 12; j++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed2 = characterManager().getBaseBattleCharacterFromBreed((short)j);
					if (baseBattleCharacterFromBreed2 == null || baseBattleCharacterFromBreed2.breed() != 1)
					{
						continue;
					}
					BattleMonster battleMonster2 = static_cast<BattleMonster>(baseBattleCharacterFromBreed2);
					if (battleMonster2 == null || battleMonster2.monster() == null || battleMonster2.monsterId() == 225 || battleMonster2.monsterId() == 226)
					{
						continue;
					}
					if (attacker.checkTargetId(battleMonster2.battleCharacterId()))
					{
						if (battleMonster2.isBattle())
						{
							battleMonster2.condition().onDeath();
							battleMonster2.condition().clearConditionTime();
							battleMonster2.onIsActionEnd();
						}
						continue;
					}
					int num = attacker.unusedTargetId();
					if (num >= 0)
					{
						attacker.setTargetId(num, battleMonster2.battleCharacterId());
						if (battleMonster2.isBattle())
						{
							battleMonster2.condition().onDeath();
							battleMonster2.condition().clearConditionTime();
							battleMonster2.onIsActionEnd();
						}
					}
				}
			}

			public bool deadCharacters(BaseBattleCharacter attacker)
			{
				if (checkFlag(EndEnemyProcess))
				{
					return false;
				}
				if (checkFlag(CHECK_START) && !checkFlag(CHECK_END))
				{
					return false;
				}
				if (checkFlag(EndPlayerProcess) && checkFlag(EndEffectProcess) && checkFlag(End2DProcess))
				{
					int[] array = new int[12];
					for (int i = 0; i < 12; i++)
					{
						array[i] = -1;
					}
					bool flag = false;
					if (isDeadMonster(attacker))
					{
						flag = true;
						if (!preDead())
						{
							return false;
						}
					}
					if (!checkFlag(StartEnemyDeadProcess))
					{
						bool flag2 = false;
						if (flag)
						{
							setTargetAllEnemyAfterDeadLastBoss(attacker);
							selectDeadMonster();
						}
						characterManager().changeMagicColor();
						for (int j = 0; j < 12; j++)
						{
							BaseBattleCharacter baseBattleCharacter = characterManager().getBaseBattleCharacterFromBreed(attacker.targetId(j));
							if (baseBattleCharacter == null)
							{
								continue;
							}
							if (reflectCharacter(baseBattleCharacter) != null)
							{
								BaseBattleCharacter baseBattleCharacter2 = reflectCharacter(baseBattleCharacter);
								if (array[baseBattleCharacter2.battleCharacterId()] != -1)
								{
									continue;
								}
								array[baseBattleCharacter2.battleCharacterId()] = baseBattleCharacter2.battleCharacterId();
								baseBattleCharacter = baseBattleCharacter2;
							}
							if (baseBattleCharacter.breed() == 1)
							{
								BattleMonster battleMonster = static_cast<BattleMonster>(baseBattleCharacter);
								if (battleMonster.condition().isDeath() || battleMonster.condition().isStone())
								{
									deadMonster(battleMonster);
									setDeadMonster(battleMonster);
									baseBattleCharacter.condition().clearDeadCondition();
									flag2 = true;
								}
							}
							else
							{
								if (baseBattleCharacter.breed() != 0)
								{
									continue;
								}
								BattlePlayer battlePlayer = static_cast<BattlePlayer>(baseBattleCharacter);
								battlePlayer.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION);
								if (baseBattleCharacter.condition().isDeath())
								{
									baseBattleCharacter.clearMagicFlagAll();
									baseBattleCharacter.resetParameterMagicFlag();
									baseBattleCharacter.condition().clearDeadCondition();
									baseBattleCharacter.condition().offStone();
									battlePlayer.setIdleType(0);
									if (battlePlayer.actionId() != 1 && battlePlayer.actionId() != 17 && battlePlayer.actionId() != 11)
									{
										battlePlayer.setActionId(0);
									}
									battlePlayer.clearRollUpLevel();
								}
								else if (baseBattleCharacter.condition().isStone())
								{
									baseBattleCharacter.clearMagicFlagAll();
									baseBattleCharacter.resetParameterMagicFlag();
									baseBattleCharacter.condition().clearDeadCondition();
								}
								else if (!baseBattleCharacter.condition().isStone())
								{
									battlePlayer.clearStoneInfo();
								}
								if (!battlePlayer.flag(PLAYER_FLAG.PF_ERASE))
								{
									battlePlayer.changeConditionEffect();
								}
								playerWindow().updateHp(battlePlayer.orderId());
							}
						}
						if (!flag2)
						{
							setCheckFlag(EndEnemyProcess);
							return true;
						}
						startMonsterDead();
						setCheckFlag(StartEnemyDeadProcess);
					}
					else
					{
						bool flag3 = false;
						for (int k = 0; k < 12; k++)
						{
							BaseBattleCharacter baseBattleCharacter3 = characterManager().getBaseBattleCharacterFromBreed(attacker.targetId(k));
							if (baseBattleCharacter3 == null)
							{
								continue;
							}
							if (reflectCharacter(baseBattleCharacter3) != null)
							{
								BaseBattleCharacter baseBattleCharacter4 = reflectCharacter(baseBattleCharacter3);
								if (array[baseBattleCharacter4.battleCharacterId()] != -1)
								{
									continue;
								}
								array[baseBattleCharacter4.battleCharacterId()] = baseBattleCharacter4.battleCharacterId();
								baseBattleCharacter3 = baseBattleCharacter4;
							}
							if (baseBattleCharacter3.breed() == 1)
							{
								BattleMonster battleMonster2 = static_cast<BattleMonster>(baseBattleCharacter3);
								if (!checkFlag(EndEnemyProcess) && isEndMonsterDead() && (battleMonster2.condition().isDeath() || battleMonster2.condition().isStone()))
								{
									OS_Printf("敵処理終了\n");
									characterMng.setTransparency(battleMonster2.characterMngId(), 0);
									characterMng.setShadowAlpha(battleMonster2.characterMngId(), 0);
									battleMonster2.unregisterMonster();
									flag3 = true;
								}
							}
							else if (baseBattleCharacter3.breed() == 0 && isEndMonsterDead())
							{
								flag3 = true;
							}
						}
						if (flag3)
						{
							endMonsterDead();
							setCheckFlag(EndEnemyProcess);
							return true;
						}
					}
				}
				return false;
			}

			public BaseBattleCharacter reflectCharacter(BaseBattleCharacter target)
			{
				if (target.reflectTargetId() > -1)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed((short)target.reflectTargetId());
					if (baseBattleCharacterFromBreed == null)
					{
						return null;
					}
					if (baseBattleCharacterFromBreed.battleCharacterId() < 0)
					{
						return null;
					}
					return baseBattleCharacterFromBreed;
				}
				return null;
			}

			public bool isTarget(BattlePlayer player)
			{
				if (!player.flag(PLAYER_FLAG.PF_TARGET_PLAYER) && !player.flag(PLAYER_FLAG.PF_TARGET_MONSTER))
				{
					return true;
				}
				if (setTargetProvocation(player))
				{
					return true;
				}
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed(player.targetId(i));
					if (baseBattleCharacterFromBreed != null && isSelectTarget(nowCharacter(), baseBattleCharacterFromBreed))
					{
						return true;
					}
				}
				if (player.flag(PLAYER_FLAG.PF_TARGET_MONSTER))
				{
					BattleMonsterParty battleMonsterParty = characterManager().monsterParty();
					for (byte b = 0; b < 6; b++)
					{
						if (isSelectTarget(nowCharacter(), battleMonsterParty.battleMonster(b)))
						{
							player.targetId_set(0, battleMonsterParty.battleMonster(b).battleCharacterId());
							return true;
						}
					}
				}
				else if (player.flag(PLAYER_FLAG.PF_TARGET_PLAYER))
				{
					BattleParty battleParty = characterManager().playerParty();
					for (int j = 0; j < 4; j++)
					{
						if (isSelectTarget(nowCharacter(), battleParty.battlePlayer(j)))
						{
							player.targetId_set(0, battleParty.battlePlayer(j).battleCharacterId());
							return true;
						}
					}
				}
				return false;
			}

			public void setTargetRandam(BaseBattleCharacter attacker, BattleMonsterParty monsterParty, bool reflect)
			{
				if (!reflect && setTargetProvocation(attacker))
				{
					return;
				}
				attacker.clearTargetId();
				ds.Vector<int, ds.FastErasePolicy<int>> vector = new ds.Vector<int, ds.FastErasePolicy<int>>(6);
				for (int i = 0; i < 6; i++)
				{
					if (isSelectTarget(attacker, monsterParty.battleMonster(i)))
					{
						vector.push_back(monsterParty.battleMonster(i).battleCharacterId());
					}
				}
				if (vector.size() == 0)
				{
					attacker.setTargetId(0, -1);
					return;
				}
				int pos = (int)ds.RandomNumber.rand32((uint)vector.size());
				attacker.setTargetId(0, (short)vector.at(pos));
			}

			public void setTargetRandam(BaseBattleCharacter attacker, BattleParty party, bool reflect)
			{
				if (!reflect && setTargetProvocation(attacker))
				{
					return;
				}
				attacker.clearTargetId();
				ds.Vector<int, ds.FastErasePolicy<int>> vector = new ds.Vector<int, ds.FastErasePolicy<int>>(4);
				for (int i = 0; i < 4; i++)
				{
					if (isSelectTarget(attacker, party.battlePlayer(i)))
					{
						vector.push_back(party.battlePlayer(i).battleCharacterId());
					}
				}
				if (vector.size() == 0)
				{
					attacker.setTargetId(0, -1);
					return;
				}
				int pos = (int)ds.RandomNumber.rand32((uint)vector.size());
				attacker.setTargetId(0, (short)vector.at(pos));
			}

			public bool setTargetProvocation(BaseBattleCharacter attacker)
			{
				if (attacker.flag(PLAYER_FLAG.PF_PROVOCATION))
				{
					itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(attacker.useMagicId());
					if (magicParameter != null && (magicParameter.magicUseKind() == 1 || magicParameter.magicUseKind() == 2))
					{
						return false;
					}
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed((short)attacker.provocationCharacterId());
					if (baseBattleCharacterFromBreed != null && baseBattleCharacterFromBreed.isBattle())
					{
						attacker.clearTargetId();
						attacker.setTargetId(0, (short)attacker.provocationCharacterId());
						return true;
					}
				}
				return false;
			}

			public bool doCondition(BaseBattleCharacter target)
			{
				if (target.flag(PLAYER_FLAG.PF_ERASE) && target.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(target);
					battlePlayer.disappear(1);
				}
				if (target.changeCondition().isFrog())
				{
					if (target.condition().isFrog())
					{
						returnCharacter(target);
					}
					else if (!target.condition().isFrog())
					{
						changeFrog(target);
					}
				}
				else if (target.changeCondition().isLilliput())
				{
					if (target.condition().isLilliput())
					{
						returnCharacter(target);
					}
					else if (!target.condition().isLilliput())
					{
						changeLilliput(target);
					}
				}
				target.initializeFrameCounter();
				return true;
			}

			public void executeCommonMagic()
			{
				checkEnd2D();
				drawMagic2D();
				checkEndEffect();
				drawMagicEffect();
				doCheck();
				deadCharacters(nowCharacter());
			}

			public void executeMonsterSpecial()
			{
				checkEnd2D();
				drawMagic2D();
				checkEndEffect();
				drawMonsterEffect();
				deadCharacters(nowCharacter());
			}

			public void drawMagicEffect()
			{
				if (checkFlag(PlayEffect) && !checkFlag(StartEffectProcess))
				{
					if (isOnlyAllMagic(nowCharacter().useMagicId()))
					{
						drawAllMagicEffect();
					}
					else
					{
						drawOnceMagicEffect(nowCharacter().useMagicId());
					}
				}
			}

			public bool drawOnceMagicEffect(int magic_id)
			{
				pl.PlayerNormalMagicParameter playerNormalMagicParameter = pl.PlayerParty.instance().normalMagic(magic_id);
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter((short)magic_id);
				if (effectCounter_ > 0)
				{
					effectCounter_--;
				}
				bool flag = true;
				for (int i = 0; i < 12; i++)
				{
					if (nowCharacter().targetId(i) < 0)
					{
						continue;
					}
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed(nowCharacter().targetId(i));
					if (baseBattleCharacterFromBreed == null || (nowCharacter().isDrain() && nowCharacter().battleCharacterId() == baseBattleCharacterFromBreed.battleCharacterId()) || baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_EFFECT))
					{
						continue;
					}
					if (effectCounter_ > 0)
					{
						flag = false;
						continue;
					}
					if (checkFlag(REFLECT_TARGET))
					{
						BaseBattleCharacter baseBattleCharacterFromBreed2 = characterManager().getBaseBattleCharacterFromBreed((short)baseBattleCharacterFromBreed.reflectTargetId());
						if (createMagicEffect(playerNormalMagicParameter.effect().frameCounter_, playerNormalMagicParameter.effect(), baseBattleCharacterFromBreed2, playerNormalMagicParameter.offset()))
						{
							effectCounter_ = playerNormalMagicParameter.effectPlayFrame();
							baseBattleCharacterFromBreed2.zeroClearFrameCounter();
							baseBattleCharacterFromBreed.setFlag(PLAYER_FLAG.PF_EFFECT);
							baseBattleCharacterFromBreed.clearMagicFlag(MAGIC_FLAG.MF_REFLECT);
							if (nowCharacter().useMagicId() == REFLECT_ID)
							{
								baseBattleCharacterFromBreed2.setMagicFlag(MAGIC_FLAG.MF_REFLECT);
								baseBattleCharacterFromBreed2.setNowColor(64);
							}
							clearCheckFlag(REFLECT_TARGET);
						}
						playSE(playerNormalMagicParameter.se().frameCounter_, playerNormalMagicParameter.se(), 0, 0, 0);
					}
					else if (baseBattleCharacterFromBreed.magicFlag(MAGIC_FLAG.MF_REFLECT) && magicParameter != null && magicParameter.isReflect() != 0 && !OutsideToBattle.getInstance().transfix())
					{
						if (nowCharacter().breed() == 0 && nowCharacter().actionId() == 6)
						{
							if (createMagicEffect(playerNormalMagicParameter.effect().frameCounter_, playerNormalMagicParameter.effect(), baseBattleCharacterFromBreed, playerNormalMagicParameter.offset()))
							{
								effectCounter_ = playerNormalMagicParameter.effectPlayFrame() / 2;
								baseBattleCharacterFromBreed.zeroClearFrameCounter();
								baseBattleCharacterFromBreed.setFlag(PLAYER_FLAG.PF_EFFECT);
								if (nowCharacter().useMagicId() == REFLECT_ID || nowCharacter().useMagicId() == 4206)
								{
									baseBattleCharacterFromBreed.setMagicFlag(MAGIC_FLAG.MF_REFLECT);
									if (!baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_MISS))
									{
										baseBattleCharacterFromBreed.setNowColor(64);
									}
								}
								if (nowCharacter().useMagicId() == 4208 && !baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_MISS))
								{
									baseBattleCharacterFromBreed.setNowColor(4);
								}
							}
							playSE(playerNormalMagicParameter.se().frameCounter_, playerNormalMagicParameter.se(), 0, 0, 0);
							flag = false;
							continue;
						}
						pl.PlayerNormalMagicParameter playerNormalMagicParameter2 = pl.PlayerParty.instance().normalMagic(REFLECT_ID);
						if (createMagicEffect(playerNormalMagicParameter2.effect().frameCounter_, playerNormalMagicParameter2.effect(), baseBattleCharacterFromBreed, playerNormalMagicParameter2.offset()))
						{
							effectCounter_ = playerNormalMagicParameter2.effectPlayFrame();
							baseBattleCharacterFromBreed.zeroClearFrameCounter();
							setCheckFlag(REFLECT_TARGET);
							if (baseBattleCharacterFromBreed.breed() == 0)
							{
								BattlePlayer battlePlayer = static_cast<BattlePlayer>(baseBattleCharacterFromBreed);
								battlePlayer.changePlayerColor();
							}
						}
						playSE(playerNormalMagicParameter2.se().frameCounter_, playerNormalMagicParameter2.se(), 0, 0, 0);
					}
					else
					{
						if (createMagicEffect(playerNormalMagicParameter.effect().frameCounter_, playerNormalMagicParameter.effect(), baseBattleCharacterFromBreed, playerNormalMagicParameter.offset()))
						{
							effectCounter_ = playerNormalMagicParameter.effectPlayFrame() / 2;
							baseBattleCharacterFromBreed.zeroClearFrameCounter();
							baseBattleCharacterFromBreed.setFlag(PLAYER_FLAG.PF_EFFECT);
							if (nowCharacter().useMagicId() == REFLECT_ID || nowCharacter().useMagicId() == 4206)
							{
								baseBattleCharacterFromBreed.setMagicFlag(MAGIC_FLAG.MF_REFLECT);
								if (!baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_MISS))
								{
									baseBattleCharacterFromBreed.setNowColor(64);
								}
							}
							if (nowCharacter().useMagicId() == 4208 && !baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_MISS))
							{
								baseBattleCharacterFromBreed.setNowColor(4);
							}
						}
						playSE(playerNormalMagicParameter.se().frameCounter_, playerNormalMagicParameter.se(), 0, 0, 0);
					}
					flag = false;
				}
				for (int j = 0; j < 12; j++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed3 = characterManager().getBaseBattleCharacterFromBreed((short)j);
					if (baseBattleCharacterFromBreed3 != null && baseBattleCharacterFromBreed3.frameCounter() >= 0)
					{
						if (baseBattleCharacterFromBreed3.frameCounter() == playerNormalMagicParameter.effectPlayFrame() / 2)
						{
							doCondition(baseBattleCharacterFromBreed3);
						}
						else
						{
							flag = false;
						}
					}
				}
				if (flag)
				{
					OS_Printf("エフェクト開始終了\n");
					setCheckFlag(StartEffectProcess);
				}
				return flag;
			}

			public void drawAllMagicEffect()
			{
				int num = nowCharacter().useMagicId();
				pl.PlayerNormalMagicParameter playerNormalMagicParameter = pl.PlayerParty.instance().normalMagic(num);
				itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().magicParameter((short)num);
				if (itemBaseParameter != null && itemBaseParameter.system() == 3)
				{
					createEffect(playerNormalMagicParameter.effect().frameCounter_, playerNormalMagicParameter.effect(), nowCharacter(), 0, 0, 1);
					playSE(playerNormalMagicParameter.se().frameCounter_, playerNormalMagicParameter.se(), 0, 0, 0);
				}
				else
				{
					createAllMagicEffect(playerNormalMagicParameter.effect(), nowCharacter());
					playSE(playerNormalMagicParameter.se().frameCounter_, playerNormalMagicParameter.se(), 0, 0, 0);
				}
				setCheckFlag(StartEffectProcess);
			}

			public void drawMonsterEffect()
			{
				if (checkFlag(PlayEffect) && !checkFlag(StartEffectProcess))
				{
					mon.MonsterSpecialAttackParameter monsterSpecialAttackParameter = mon.MonsterManager.instance().specialAttack(nowCharacter().useMagicId());
					if (monsterSpecialAttackParameter.command() == 2)
					{
						setFlash(2, 2, 1);
						BattleSE.instance().play(202, 0);
						setCheckFlag(StartEffectProcess);
					}
					else if (isOnlyAllMagic(nowCharacter().useMagicId()))
					{
						drawAllMonsterEffect();
					}
					else
					{
						drawOnceMonsterEffect();
					}
				}
			}

			public void drawOnceMonsterEffect()
			{
				mon.MonsterSpecialAttackEffects monsterSpecialAttackEffects = mon.MonsterManager.instance().effectsInfo(nowCharacter().useMagicId());
				ys.Effects effects = new ys.Effects(0, 0, monsterSpecialAttackEffects.effectInfo(0).category_, monsterSpecialAttackEffects.effectInfo(0).member_, arg4: false);
				ys.Effects effects2 = new ys.Effects(0, 0, monsterSpecialAttackEffects.seInfo(0).category_, monsterSpecialAttackEffects.seInfo(0).member_, arg4: false);
				if (effectCounter_ > 0)
				{
					effectCounter_--;
				}
				bool flag = true;
				for (int i = 0; i < 12; i++)
				{
					if (nowCharacter().targetId(i) < 0)
					{
						continue;
					}
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed(nowCharacter().targetId(i));
					if (baseBattleCharacterFromBreed == null || (nowCharacter().useMagicId() == 6606 && nowCharacter().targetId(i) == nowCharacter().battleCharacterId()))
					{
						continue;
					}
					if (!baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_EFFECT))
					{
						if (effectCounter_ > 0)
						{
							flag = false;
						}
						else
						{
							if (createMagicEffect(effects.frameCounter_, effects, baseBattleCharacterFromBreed, monsterSpecialAttackEffects.effectInfo(0).positionType_))
							{
								effectCounter_ = monsterSpecialAttackEffects.effectInfo(0).playFrame_ / 2;
								baseBattleCharacterFromBreed.zeroClearFrameCounter();
								baseBattleCharacterFromBreed.setFlag(PLAYER_FLAG.PF_EFFECT);
							}
							playSE(effects2.frameCounter_, effects2, 0, 0, 0);
							flag = false;
						}
					}
					flag = baseBattleCharacterFromBreed.frameCounter() == monsterSpecialAttackEffects.effectInfo(0).playFrame_ / 2 && doCondition(baseBattleCharacterFromBreed);
				}
				if (flag)
				{
					OS_Printf("エフェクト開始終了\n");
					setCheckFlag(StartEffectProcess);
				}
			}

			public void drawAllMonsterEffect()
			{
				mon.MonsterSpecialAttackEffects monsterSpecialAttackEffects = mon.MonsterManager.instance().effectsInfo(nowCharacter().useMagicId());
				int motionIndex = characterMng.getMotionIndex(nowCharacter().characterMngId());
				int currentFrame = (int)characterMng.getCurrentFrame(nowCharacter().characterMngId());
				if (monsterSpecialAttackEffects.effectInfo(0).timingInfo_.motionIndex_ == motionIndex && monsterSpecialAttackEffects.effectInfo(0).timingInfo_.frame_ == currentFrame)
				{
					ys.Effects effects = new ys.Effects(0, 0, monsterSpecialAttackEffects.effectInfo(0).category_, monsterSpecialAttackEffects.effectInfo(0).member_, arg4: false);
					createAllMagicEffect(effects, nowCharacter());
					if (nowCharacter().useMagicId() == 6618 || nowCharacter().useMagicId() == 6652)
					{
						BattleSE.instance().stop();
					}
					BattleSE.instance().play(monsterSpecialAttackEffects.seInfo(0).category_, monsterSpecialAttackEffects.seInfo(0).member_);
					setCheckFlag(StartEffectProcess);
				}
				if (!checkFlag(StartEffectProcess) && monsterSpecialAttackEffects.effectInfo(0).timingInfo_.motionIndex_ < 0 && monsterSpecialAttackEffects.effectInfo(0).timingInfo_.frame_ < 0)
				{
					ys.Effects effects2 = new ys.Effects(0, 0, monsterSpecialAttackEffects.effectInfo(0).category_, monsterSpecialAttackEffects.effectInfo(0).member_, arg4: false);
					createAllMagicEffect(effects2, nowCharacter());
					BattleSE.instance().play(monsterSpecialAttackEffects.seInfo(0).category_, monsterSpecialAttackEffects.seInfo(0).member_);
					setCheckFlag(StartEffectProcess);
				}
				if (monsterSpecialAttackEffects.effectInfo(1).timingInfo_.motionIndex_ != motionIndex || monsterSpecialAttackEffects.effectInfo(1).timingInfo_.frame_ != currentFrame)
				{
					return;
				}
				ys.Effects effects3 = new ys.Effects(0, 0, monsterSpecialAttackEffects.effectInfo(1).category_, monsterSpecialAttackEffects.effectInfo(1).member_, arg4: false);
				int effectId = BattleEffect.instance().create(effects3.category_, effects3.member_);
				if (nowCharacter().useMagicId() == 6618 || nowCharacter().useMagicId() == 6652)
				{
					ushort x = 0;
					ushort y = 0;
					ushort z = 0;
					characterMng.getRotation(nowCharacter().characterMngId(), ref x, ref y, ref z);
					MtxFx43 mtxFx = new MtxFx43();
					MtxFx43 mtxFx2 = new MtxFx43();
					MtxFx43 mtxFx3 = new MtxFx43();
					MTX_Identity43(mtxFx);
					MTX_Identity43(mtxFx2);
					characterMng.getPoseMtx(nowCharacter().characterMngId(), mtxFx3);
					ds.CpuMatrix.setRotateY(mtxFx, y);
					ds.Vector3<int> vector = new ds.Vector3<int>();
					vector.set(-44335, 86315, 130707);
					ds.CpuMatrix.setTranslate(mtxFx2, vector);
					MTX_Concat43(mtxFx, mtxFx2, mtxFx);
					MTX_Concat43(mtxFx, mtxFx3, mtxFx);
					VecFx32 vecFx = new VecFx32();
					vecFx.x = mtxFx._30;
					vecFx.y = mtxFx._31;
					vecFx.z = mtxFx._32;
					setEffectPosition(nowCharacter(), effectId, vecFx);
				}
				else
				{
					MtxFx43 mtxFx4 = new MtxFx43();
					if (characterMng.getJntMtx(nowCharacter().characterMngId(), "kuti", mtxFx4))
					{
						VecFx32 pos = new VecFx32(mtxFx4._30, mtxFx4._31, mtxFx4._32);
						setEffectPosition(nowCharacter(), effectId, pos);
					}
				}
				BattleSE.instance().play(monsterSpecialAttackEffects.seInfo(1).category_, monsterSpecialAttackEffects.seInfo(1).member_);
			}

			public void checkEndEffect()
			{
				if (!checkFlag(StartEffectProcess) || checkFlag(EndEffectProcess))
				{
					return;
				}
				bool flag = true;
				mon.MonsterSpecialAttackParameter monsterSpecialAttackParameter = mon.MonsterManager.instance().specialAttack(nowCharacter().useMagicId());
				if (monsterSpecialAttackParameter != null && monsterSpecialAttackParameter.command() == 2 && flash_.isFlash())
				{
					setCheckFlag(EndEffectProcess);
					OS_Printf("エフェクト終了\n");
					return;
				}
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed(nowCharacter().targetId(i));
					if (baseBattleCharacterFromBreed != null && !baseBattleCharacterFromBreed.isClearAllEffect())
					{
						flag = false;
					}
				}
				if (!nowCharacter().isClearAllEffect())
				{
					flag = false;
				}
				if (!flag)
				{
					return;
				}
				if (nowCharacter().isDrain())
				{
					if (!checkFlag(DRAIN_START))
					{
						pl.PlayerNormalMagicParameter playerNormalMagicParameter = pl.PlayerParty.instance().normalMagic(nowCharacter().useMagicId());
						ys.Effects effects = playerNormalMagicParameter.effect();
						effects.member_ = 2;
						createMagicEffect(playerNormalMagicParameter.effect().frameCounter_, effects, nowCharacter(), playerNormalMagicParameter.offset());
						setCheckFlag(DRAIN_START);
					}
					else
					{
						setCheckFlag(EndEffectProcess);
					}
				}
				else
				{
					setCheckFlag(EndEffectProcess);
				}
			}

			public void drawMagic2D()
			{
				if (checkFlag(Start2DProcess) || !checkFlag(EndEffectProcess))
				{
					return;
				}
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed(nowCharacter().targetId(i));
					if (baseBattleCharacterFromBreed == null)
					{
						continue;
					}
					if (baseBattleCharacterFromBreed.reflectTargetId() > -1)
					{
						BaseBattleCharacter baseBattleCharacterFromBreed2 = characterManager().getBaseBattleCharacterFromBreed((short)baseBattleCharacterFromBreed.reflectTargetId());
						if (baseBattleCharacterFromBreed2 != null && !baseBattleCharacterFromBreed2.flag(PLAYER_FLAG.PF_2D))
						{
							if (baseBattleCharacterFromBreed2.flag(PLAYER_FLAG.PF_MISS))
							{
								createHit(nowCharacter(), baseBattleCharacterFromBreed2);
							}
							else
							{
								createDamage(baseBattleCharacterFromBreed2, null);
							}
						}
					}
					else if (!baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_2D))
					{
						if (baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_MISS))
						{
							createHit(nowCharacter(), baseBattleCharacterFromBreed);
						}
						else
						{
							createDamage(baseBattleCharacterFromBreed, null);
						}
					}
				}
				createChangeConditionEffect();
				setCheckFlag(Start2DProcess);
			}

			public void draw2D2()
			{
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed((short)i);
					if (baseBattleCharacterFromBreed != null && baseBattleCharacterFromBreed.condition().isPoison())
					{
						createDamage(baseBattleCharacterFromBreed, null);
					}
				}
			}

			public bool checkEnd2D()
			{
				if (!checkFlag(Start2DProcess))
				{
					return false;
				}
				if (checkFlag(End2DProcess))
				{
					return false;
				}
				for (int i = 0; i < 12; i++)
				{
					if (Battle2DManager.instance().damage().isExist(i))
					{
						return false;
					}
					if (Battle2DManager.instance().hit(i).isExist())
					{
						return false;
					}
				}
				if (!isEndChangeConditionEffect())
				{
					return false;
				}
				OS_Printf("2D再生終了\n");
				setCheckFlag(End2DProcess);
				return true;
			}

			public bool checkEnd2DNoTarget()
			{
				if (!checkFlag(Start2DProcess))
				{
					return false;
				}
				if (checkFlag(End2DProcess))
				{
					return false;
				}
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed((short)i);
					if (baseBattleCharacterFromBreed != null && baseBattleCharacterFromBreed.condition().isPoison() && Battle2DManager.instance().damage().isExist(i))
					{
						return false;
					}
				}
				OS_Printf("2D再生終了\n");
				setCheckFlag(End2DProcess);
				return true;
			}

			public bool isSelectTarget(BaseBattleCharacter attacker, BaseBattleCharacter target)
			{
				if (target == null)
				{
					return false;
				}
				if (attacker.isSelectDeadOrStoneTarget(target))
				{
					if (!target.isEnable())
					{
						return false;
					}
				}
				else if (!target.isBattle())
				{
					return false;
				}
				if (target.breed() == 0 && target != null && target.flag(PLAYER_FLAG.PF_JUMP))
				{
					return false;
				}
				return true;
			}

			public void addEfpReflect()
			{
				if (characterManager().isReflected())
				{
					BattleEffect.instance().addEfp(REFLECT_EFP);
					MatrixSound.MtxSENDS_Load(256);
				}
			}

			public bool isTarget(BattleMonster battleMonster)
			{
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed(battleMonster.targetId(i));
					if (baseBattleCharacterFromBreed != null && isSelectTarget(battleMonster, baseBattleCharacterFromBreed))
					{
						return true;
					}
				}
				setTargetRandam(battleMonster, characterManager().monsterParty(), reflect: false);
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed2 = characterManager().getBaseBattleCharacterFromBreed(battleMonster.targetId(i));
					if (baseBattleCharacterFromBreed2 != null && isSelectTarget(battleMonster, baseBattleCharacterFromBreed2))
					{
						return true;
					}
				}
				return false;
			}

			public void addGiftExp(BattleMonster monster, BattleMonsterParty monsterParty)
			{
				int value = mon.MonsterManager.instance().monsterParameter(monster.monsterId()).droppingParameter()
					.exp();
				monsterParty.giftExp().add(value);
				OS_Printf("経験値 %d 取得しました\n", monsterParty.giftExp().get());
			}

			public void addGiftGold(BattleMonster monster, BattleMonsterParty monsterParty)
			{
				int value = mon.MonsterManager.instance().monsterParameter(monster.monsterId()).droppingParameter()
					.gold();
				monsterParty.giftGold().add(value);
				OS_Printf("かね %d 取得しました\n", monsterParty.giftGold().get());
			}

			public void addGiftItem(BattleMonster monster, BattleMonsterParty monsterParty)
			{
				OS_Printf("\n//////////////////////////////////////////////////////////////////////\n");
				OS_Printf("// アイテム落とす？\n");
				int num = monster.monster().droppingParameter().droppingItemProbability() * 100;
				OS_Printf("アイテム落とす確率 %d\n", num);
				int num2 = (int)(ds.RandomNumber.rand32(101u) * 100);
				OS_Printf("ランダム確率 %d\n", num2);
				if (num2 <= num)
				{
					OS_Printf("何かしら拾えそう\n");
					num2 = (int)(ds.RandomNumber.rand32(101u) * 100);
					int num3 = 0;
					for (int i = 0; i < 8; i++)
					{
						if (num2 <= DROP_ITEM_ODDS[i] + num3)
						{
							int num4 = monster.monster().droppingParameter().droppingItemTableId();
							if (num4 < 0)
							{
								return;
							}
							int num5 = mon.MonsterManager.instance().dropItem(num4).itemId(i);
							OS_Printf("アイテムID %d を拾いました\n", num5);
							if (itm.ItemManager.instance().itemParameter((short)num5) != null)
							{
								monsterParty.setDropItemId((short)num5);
								return;
							}
						}
						num3 += DROP_ITEM_ODDS[i];
					}
				}
				OS_Printf("\n// アイテム落とした？\n");
				OS_Printf("//////////////////////////////////////////////////////////////////////\n");
			}

			public bool isBreakMonster(BaseBattleCharacter target)
			{
				if (target == null)
				{
					return false;
				}
				if (characterManager().monsterParty().aliveNumber() > 2)
				{
					return false;
				}
				if (!target.isBattle())
				{
					return false;
				}
				if (!target.condition().isBreak())
				{
					return false;
				}
				if (target.breed() != 1)
				{
					return false;
				}
				BattleMonster battleMonster = static_cast<BattleMonster>(target);
				if (battleMonster.monster().devide() == 0)
				{
					return false;
				}
				if (calc_.damage(target.battleCharacterId()) <= 0)
				{
					return false;
				}
				bool flag = false;
				if (nowCharacter().attackSuccess(0) && (nowCharacter().handAttack(pl.HAND_TYPE.RIGHT_HAND).attackType() & 0x400) == 0)
				{
					flag = true;
				}
				if (nowCharacter().attackSuccess(1) && (nowCharacter().handAttack(pl.HAND_TYPE.LEFT_HAND).attackType() & 0x400) == 0)
				{
					flag = true;
				}
				if (!flag)
				{
					return false;
				}
				breakRootMonster_ = battleMonster;
				breakRootMonster_.setMonsterFlag(MONSTER_FLAG.MF_BREAK);
				monsterTurn_.setBreakState(6);
				return true;
			}

			public bool isSummonMonster(BattleMonster monster)
			{
				breakRootMonster_ = null;
				if (monster == null)
				{
					return false;
				}
				if (characterManager().monsterParty().aliveNumber() > 2)
				{
					return false;
				}
				if (!monster.isBattle())
				{
					return false;
				}
				if (!monster.condition().isBreak())
				{
					return false;
				}
				if (monster.breed() != 1)
				{
					return false;
				}
				breakRootMonster_ = monster;
				monsterTurn_.setBreakState(6);
				return true;
			}

			public bool isAugmentMonster(BattleMonster monster)
			{
				breakRootMonster_ = null;
				if (monster == null)
				{
					return false;
				}
				if (characterManager().monsterParty().aliveNumber() > 2)
				{
					return false;
				}
				if (!monster.isBattle())
				{
					return false;
				}
				if (!monster.condition().isBreak())
				{
					return false;
				}
				breakRootMonster_ = monster;
				monsterTurn_.setBreakState(6);
				return true;
			}

			public void clearResult()
			{
				calc_.clearDamageAll();
				attackNumber_ = 0;
			}

			public int getAttackNumber(BaseBattleCharacter character)
			{
				if (attackNumber_ == 0)
				{
					character.setFlag(PLAYER_FLAG.PF_MISS);
				}
				return attackNumber_;
			}

			public void calcNormalAttackDamage(BaseBattleCharacter attacker)
			{
				attacker.setAttackSuccess(0, flag: false);
				attacker.setAttackSuccess(1, flag: false);
				calc_.clearDamageAll();
				for (int i = 0; i < 12; i++)
				{
					if (attacker.targetId(i) < 0)
					{
						continue;
					}
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed(attacker.targetId(i));
					if (baseBattleCharacterFromBreed == null || baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_ABSORB))
					{
						continue;
					}
					if (baseBattleCharacterFromBreed.condition().isSleep())
					{
						baseBattleCharacterFromBreed.setMagicFlag(MAGIC_FLAG.MF_RECOVER_SLEEP);
					}
					if (baseBattleCharacterFromBreed.condition().isConfusion())
					{
						baseBattleCharacterFromBreed.setMagicFlag(MAGIC_FLAG.MF_RECOVER_CONFUSION);
					}
					NewAttackFormula newAttackFormula = new NewAttackFormula();
					calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), newAttackFormula.calcDamage(attacker, baseBattleCharacterFromBreed));
					baseBattleCharacterFromBreed.setAttackNumber(attacker.attackNumber());
					if (baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_ESCAPE) && !baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_TAKE_A_POWDER))
					{
						int num = calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) * 180;
						num /= 100;
						calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), num);
					}
					if (baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_GUARD))
					{
						int t = calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) / 2;
						t = ds.max(t, 1);
						calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), t);
					}
					if (attacker.flag(PLAYER_FLAG.PF_JUMP))
					{
						BattlePlayer battlePlayer = static_cast<BattlePlayer>(attacker);
						int num2 = battlePlayer.player().jobManager().nowJobParameter()
							.skill()
							.skillLevel()
							.get();
						int num3 = calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) * (6144 + 4096 * num2 / 110) / 4096;
						if ((baseBattleCharacterFromBreed.magicDefense().weakType() & 0x200) != 0)
						{
							num3 *= 2;
						}
						calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), num3);
					}
					if (attacker.flag(PLAYER_FLAG.PF_DARK) && attacker.battleCharacterId() != baseBattleCharacterFromBreed.battleCharacterId())
					{
						DarkFormula darkFormula = new DarkFormula();
						int value = darkFormula.calcDamageDark(attacker);
						calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), value);
						baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_MISS);
					}
					if (attacker.rollUpLevel() > 0)
					{
						int value2 = calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) * (attacker.rollUpLevel() + 1);
						calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), value2);
					}
					if (baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_MORE_GUARD))
					{
						CommonFormula commonFormula = new CommonFormula();
						int num4 = commonFormula.calcJobSkill(baseBattleCharacterFromBreed);
						int value3 = calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) - calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) * (40 + num4 / 5) / 100;
						calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), value3);
					}
					if (attacker.flag(PLAYER_FLAG.PF_OVERISSUE))
					{
						CommonFormula commonFormula2 = new CommonFormula();
						int num5 = commonFormula2.calcJobSkill(attacker);
						int num6 = 0;
						num6 = ((num5 < 21) ? 3 : ((num5 >= 71) ? 6 : 4));
						int value4 = calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) * num6 / 10;
						calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), value4);
					}
					if (attacker.flag(PLAYER_FLAG.PF_KNOCK_OVER))
					{
						CommonFormula commonFormula3 = new CommonFormula();
						int num7 = commonFormula3.calcJobSkill(attacker);
						int value5 = calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) * (6144 + 4096 * num7 / 33 / 10) / 4096;
						calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), value5);
					}
					if (baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_KNOCK_OVER))
					{
						CommonFormula commonFormula4 = new CommonFormula();
						int num8 = commonFormula4.calcJobSkill(baseBattleCharacterFromBreed);
						int value6 = calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) * (6144 + 4096 * num8 / 33 / 10) / 4096;
						calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), value6);
					}
					if (attacker.flag(PLAYER_FLAG.PF_COUNTER))
					{
						int value7 = calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) * 2;
						calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), value7);
					}
					// PORT: FF5's Two-Handed (a mastery passive): one weapon and a free other hand strike twice as hard.
					if (attacker.breed() == 0 && OpenFF.Client.ProgressionLayer.TwoHandedStrike(static_cast<BattlePlayer>(attacker)))
					{
						calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) * 2);
					}
					if (baseBattleCharacterFromBreed.breed() == 0)
					{
						BattlePlayer battlePlayer2 = static_cast<BattlePlayer>(baseBattleCharacterFromBreed);
						if (battlePlayer2.player().formationType() == 1)
						{
							int value8 = calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) * 50 / 100;
							calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), value8);
						}
					}
					if (baseBattleCharacterFromBreed.magicFlag(MAGIC_FLAG.MF_SONG5))
					{
						CommonFormula commonFormula5 = new CommonFormula();
						int num9 = commonFormula5.calcJobSkill(baseBattleCharacterFromBreed);
						int value9 = calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) - calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) * (20 + num9 * 10 / 110) / 100;
						calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), value9);
					}
					if (attacker.isAbsorb() <= 0 || attacker.flag(PLAYER_FLAG.PF_ABSORB) || attacker.flag(PLAYER_FLAG.PF_DARK))
					{
						continue;
					}
					bool flag = true;
					for (int j = 0; j < 12; j++)
					{
						BaseBattleCharacter baseBattleCharacterFromBreed2 = characterManager().getBaseBattleCharacterFromBreed(attacker.targetId(j));
						if (baseBattleCharacterFromBreed2 != null && baseBattleCharacterFromBreed2.isBattle() && baseBattleCharacterFromBreed2.battleCharacterId() != attacker.battleCharacterId())
						{
							flag = false;
						}
					}
					if (flag)
					{
						continue;
					}
					attacker.setFlag(PLAYER_FLAG.PF_ABSORB);
					attacker.setTargetIdMyself();
					if (baseBattleCharacterFromBreed.isGhost())
					{
						int value10 = calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) * attacker.isAbsorb() / 6;
						calc_.setDamage(attacker.battleCharacterId(), value10);
						attacker.clearFlag(PLAYER_FLAG.PF_2D);
						bool flag2 = true;
						if (attacker.breed() == 0)
						{
							BattlePlayer battlePlayer3 = static_cast<BattlePlayer>(attacker);
							itm.WeaponParameter weaponParameter = null;
							if (battlePlayer3 != null)
							{
								weaponParameter = itm.ItemManager.instance().weaponParameter(battlePlayer3.player().equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND)
									.itemId());
								if (weaponParameter != null && weaponParameter.itemId() != 1000 && (weaponParameter.atckType() & 4) == 0)
								{
									flag2 = false;
								}
								weaponParameter = null;
								weaponParameter = itm.ItemManager.instance().weaponParameter(battlePlayer3.player().equipParameter().equipHand(pl.HAND_TYPE.LEFT_HAND)
									.itemId());
								if (weaponParameter != null && weaponParameter.itemId() != 1000 && (weaponParameter.atckType() & 4) == 0)
								{
									flag2 = false;
								}
							}
						}
						if (flag2)
						{
							value10 = calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) * attacker.isAbsorb();
							calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), value10);
							baseBattleCharacterFromBreed.setFlag(PLAYER_FLAG.PF_RECOVER);
						}
						else
						{
							value10 = calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) * attacker.isAbsorb();
							calc_.setDamage(baseBattleCharacterFromBreed.battleCharacterId(), value10);
						}
					}
					else
					{
						int value11 = calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()) * attacker.isAbsorb() / 6;
						calc_.setDamage(attacker.battleCharacterId(), value11);
						attacker.setFlag(PLAYER_FLAG.PF_RECOVER);
						attacker.clearFlag(PLAYER_FLAG.PF_2D);
					}
				}
			}

			public void setNormalAttackDamage(BaseBattleCharacter attacker)
			{
				for (int i = 0; i < 12; i++)
				{
					if (attacker.targetId(i) < 0)
					{
						continue;
					}
					BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed(attacker.targetId(i));
					if (baseBattleCharacterFromBreed == null)
					{
						continue;
					}
					if (baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_RECOVER))
					{
						baseBattleCharacterFromBreed.hp().addNow(calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()));
					}
					else
					{
						if (baseBattleCharacterFromBreed.condition().isConfusion() || baseBattleCharacterFromBreed.condition().isSleep())
						{
							baseBattleCharacterFromBreed.onIsActionEnd();
						}
						baseBattleCharacterFromBreed.hp().subNow(calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()));
						if (attacker.breed() == 0)
						{
							pl.PlayerParty.instance().mania().setMaxDamage(calc_.damage(baseBattleCharacterFromBreed.battleCharacterId()));
						}
					}
					damageCharacter(baseBattleCharacterFromBreed);
					if (attacker.overissueNumber() <= 1)
					{
						isBreakMonster(baseBattleCharacterFromBreed);
					}
					if (!baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_RECOVER))
					{
						if (baseBattleCharacterFromBreed.magicFlag(MAGIC_FLAG.MF_RECOVER_SLEEP))
						{
							baseBattleCharacterFromBreed.condition().offSleep();
							baseBattleCharacterFromBreed.changeCondition().offSleep();
						}
						if (baseBattleCharacterFromBreed.magicFlag(MAGIC_FLAG.MF_RECOVER_CONFUSION))
						{
							baseBattleCharacterFromBreed.condition().offConfusion();
							baseBattleCharacterFromBreed.changeCondition().offConfusion();
						}
					}
				}
			}

			public void calcMagicDamage(BaseBattleCharacter user)
			{
				calc_.calcMagic(characterManager(), user);
			}

			public void calcItemDamage(BaseBattleCharacter user)
			{
				calc_.calcItem(characterManager(), user);
			}

			public void damageCharacter(BaseBattleCharacter target)
			{
				calc_.damageCharacter(target);
			}

			public void setOverissueDamage(BaseBattleCharacter attacker)
			{
				for (int i = 0; i < 12; i++)
				{
					overissueCalc_.damage_add(i, calc_.damage(i));
				}
				overissueTargetId_[attacker.targetId(0)] = attacker.targetId(0);
			}

			public void doCheck()
			{
				if (nowCharacter().useMagicId() != 4010 || !checkFlag(EndEffectProcess))
				{
					return;
				}
				BaseBattleCharacter baseBattleCharacterFromBreed = characterManager().getBaseBattleCharacterFromBreed(nowCharacter().targetId(0));
				if (!checkFlag(CHECK_START))
				{
					checkWeak(baseBattleCharacterFromBreed);
					clearCheckState();
					setCheckFlag(CHECK_START);
					if (baseBattleCharacterFromBreed.breed() == 1)
					{
						BattleMonster battleMonster = static_cast<BattleMonster>(baseBattleCharacterFromBreed);
						mon.MonsterMania monsterMania = mon.MonsterManager.instance().monsterManiaManager().monsterManiaForMonsterID(battleMonster.monster().monsterId());
						monsterMania.onChecked();
					}
				}
				else
				{
					outputWeak(baseBattleCharacterFromBreed);
				}
			}

			public void checkWeak(BaseBattleCharacter target)
			{
				weakType_ = target.magicDefense().weakType();
				weakType_ &= -2;
				weakType_ &= -3;
				weakType_ &= -5;
				if ((weakType_ & 8) != 0)
				{
					weakList_.push_back(128);
				}
				if ((weakType_ & 0x10) != 0)
				{
					weakList_.push_back(129);
				}
				if ((weakType_ & 0x20) != 0)
				{
					weakList_.push_back(130);
				}
				if ((weakType_ & 0x40) != 0)
				{
					weakList_.push_back(131);
				}
				if ((weakType_ & 0x80) != 0)
				{
					weakList_.push_back(132);
				}
				if ((weakType_ & 0x100) != 0)
				{
					weakList_.push_back(133);
				}
				if ((weakType_ & 0x200) != 0)
				{
					weakList_.push_back(134);
				}
				if ((weakType_ & 0x400) != 0)
				{
					weakList_.push_back(135);
				}
				if (weakList_.size() == 0)
				{
					weakList_.push_back(116);
				}
			}

			public void outputWeak(BaseBattleCharacter target)
			{
				switch (checkState())
				{
				case 0:
					firstStep(target);
					break;
				case 1:
					secondStep(target);
					break;
				case 2:
					thirdStep(target);
					break;
				case 4:
					lastStep(target);
					break;
				case 3:
					break;
				}
			}

			public void firstStep(BaseBattleCharacter target)
			{
				if (target.breed() == 1)
				{
					BattleMonster battleMonster = static_cast<BattleMonster>(target);
					Battle2DManager.instance().helpWindow().createHelpWindow(battleMonster.monster().nameId(), 1, 0);
				}
				else if (target.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(target);
					dgs.CCtrlCodeInterface.instance().setPlayerId(battlePlayer.player().playerId());
					Battle2DManager.instance().helpWindow().createHelpWindow(100, 1, 0);
				}
				Battle2DManager.instance().helpWindow().setMsdHandle(0);
				checkState_ = 1;
				if (checkActionType_ == 15)
				{
					checkState_ = 2;
				}
			}

			public void secondStep(BaseBattleCharacter target)
			{
				if ((ds.g_Pad.edge() & 1) != 0 || ds.g_TouchPanel.isEdge())
				{
					menu.MenuManager.getSingleton().playSEDecide();
					if (OutsideToBattle.getInstance().battleType() == BATTLE_TYPE.NORMAL_BATTLE || OutsideToBattle.getInstance().battleType() == BATTLE_TYPE.EVENT_BATTLE || target.breed() == 0)
					{
						dgs.CCtrlCodeInterface.instance().setMaxHp(target.hp().getLimit());
						dgs.CCtrlCodeInterface.instance().setHp(target.hp().getNow());
						Battle2DManager.instance().helpWindow().updateMessage(127, 0);
					}
					else
					{
						Battle2DManager.instance().helpWindow().updateMessage(137, 0);
					}
					checkState_ = 2;
				}
			}

			public void thirdStep(BaseBattleCharacter target)
			{
				if ((ds.g_Pad.edge() & 1) != 0 || ds.g_TouchPanel.isEdge())
				{
					menu.MenuManager.getSingleton().playSEDecide();
					if (weakList_.size() == 0)
					{
						checkState_ = 4;
						return;
					}
					Battle2DManager.instance().helpWindow().updateMessage(weakList_.at(0), 0);
					weakList_.erase(0);
				}
			}

			public void lastStep(BaseBattleCharacter target)
			{
				bool flag = false;
				if (true)
				{
					Battle2DManager.instance().helpWindow().releaseHelpWindow();
					checkState_ = 8;
					setCheckFlag(CHECK_END);
				}
			}

			public bool deadMonster(BattleMonster monster)
			{
				addGiftExp(monster, characterManager().monsterParty());
				addGiftGold(monster, characterManager().monsterParty());
				addGiftItem(monster, characterManager().monsterParty());
				mon.MonsterMania monsterMania = mon.MonsterManager.instance().monsterManiaManager().monsterManiaForMonsterID(monster.monsterId());
				monsterMania.setEntryState(mon.MonsterMania.NEW_ENTRY);
				monsterMania.deadCount().add(1);
				int num = monster.monsterId();
				int monsterRate = spl.MonsterBook.getMonsterRate();
				switch (num)
				{
				case 227:
					UserInfo.AwardAchievement(15);
					break;
				}
				if (monsterRate >= 50)
				{
					UserInfo.AwardAchievement(12);
					if (monsterRate >= 100)
					{
						UserInfo.AwardAchievement(13);
					}
				}
				pl.PlayerParty.instance().mania().enemyBreakNumber()
					.add(1);
				return true;
			}

			public void selectDeadMonster()
			{
				switch (OutsideToBattle.getInstance().battleType())
				{
				case BATTLE_TYPE.NORMAL_BATTLE:
				case BATTLE_TYPE.EVENT_BATTLE:
					BPTranslucence.setting(30);
					BattlePerformer.getInstance().selectPerformer(BATTLEPERFORM_TYPE.BPT_TRANSLUCENCE);
					break;
				case BATTLE_TYPE.BOSS_BATTLE:
				case BATTLE_TYPE.LAST_BOSS_BATTLE:
				{
					VecFx32 width = new VecFx32(1024, 1024, 1024);
					int frame = 100;
					battleDisplay.readyShakeCamera(frame, width);
					BPTranslucence.setting(frame);
					BattlePerformer.getInstance().selectPerformer(BATTLEPERFORM_TYPE.BPT_TRANSLUCENCE);
					break;
				}
				}
			}

			public void startMonsterDead()
			{
				switch (OutsideToBattle.getInstance().battleType())
				{
				case BATTLE_TYPE.NORMAL_BATTLE:
				case BATTLE_TYPE.EVENT_BATTLE:
					BattleSE.instance().play(200, 1);
					break;
				case BATTLE_TYPE.BOSS_BATTLE:
				case BATTLE_TYPE.LAST_BOSS_BATTLE:
					BattleSE.instance().play(200, 2);
					break;
				}
				BattlePerformer.getInstance().start();
			}

			public void setDeadMonster(BattleMonster monster)
			{
				BattlePerformer.getInstance().setTarget(monster.characterMngId());
			}

			public bool isEndMonsterDead()
			{
				if (BattlePerformer.getInstance().isPerforming())
				{
					return false;
				}
				return true;
			}

			public void endMonsterDead()
			{
				BattlePerformer.getInstance().end();
			}

			public void setDeadFlash()
			{
				switch (OutsideToBattle.getInstance().battleType())
				{
				case BATTLE_TYPE.BOSS_BATTLE:
				case BATTLE_TYPE.LAST_BOSS_BATTLE:
					flash_.setFlashEx(2, 3, 13, ds.setGXRgb(31, 31, 31));
					BattleSE.instance().play(200, 17);
					break;
				}
				setCheckFlag(FlashProcess);
			}

			public bool preDead()
			{
				if (OutsideToBattle.getInstance().battleType() != BATTLE_TYPE.BOSS_BATTLE && OutsideToBattle.getInstance().battleType() != BATTLE_TYPE.LAST_BOSS_BATTLE)
				{
					return true;
				}
				if (!checkFlag(FlashProcess))
				{
					if (++deadFlashCounter_ > 10)
					{
						flash_.endFlash();
						setDeadFlash();
					}
					return false;
				}
				if (flash_.isFlash())
				{
					return false;
				}
				if (++deadStartCounter_ < 10)
				{
					return false;
				}
				return true;
			}

			public bool isDeadMonster(BaseBattleCharacter attacker)
			{
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacter = characterManager().getBaseBattleCharacterFromBreed(attacker.targetId(i));
					if (baseBattleCharacter == null || !baseBattleCharacter.isEnable())
					{
						continue;
					}
					if (reflectCharacter(baseBattleCharacter) != null)
					{
						baseBattleCharacter = reflectCharacter(baseBattleCharacter);
					}
					if (baseBattleCharacter.breed() == 1)
					{
						BattleMonster battleMonster = static_cast<BattleMonster>(baseBattleCharacter);
						if (battleMonster.condition().isDeath())
						{
							return true;
						}
						if (battleMonster.condition().isStone())
						{
							return true;
						}
					}
				}
				return false;
			}

			public void setHitEffectPosition(BaseBattleCharacter target, int effectId, short offset, int randamFlag)
			{
				int num = target.unUsedEffectId();
				if (num != -1)
				{
					target.effectId_set(num, effectId);
					VecFx32 vecFx = new VecFx32();
					characterMng.getPosition(target.characterMngId(), vecFx);
					VecFx32 vecFx2 = new VecFx32(battleDisplay.getBattleCamera().getPosition());
					int num2 = 0;
					int num3 = 0;
					if (target.breed() != 2 && target.condition().isFrog())
					{
						num2 = 9;
						num3 = 20480;
						vecFx.y = 0;
					}
					else if (target.breed() == 0 || target.breed() == 2)
					{
						num2 = 9;
						num3 = 20480;
					}
					else if (target.breed() == 1)
					{
						BattleMonster battleMonster = static_cast<BattleMonster>(target);
						mon.EffectOffset effectOffset = mon.MonsterManager.instance().offset(battleMonster.monsterId()).hitEffect();
						_ = effectOffset.bone_;
						_ = 0;
						num2 = effectOffset.cameraDistance_;
						num3 = 4096 * effectOffset.offsetY_;
					}
					if (randamFlag == 1)
					{
						vecFx.x += (int)((ds.RandomNumber.rand32(8u) - 4) * 4096);
						vecFx.y += (int)(ds.RandomNumber.rand32(4u) * 4096);
						vecFx.z += (int)((ds.RandomNumber.rand32(8u) - 4) * 4096);
					}
					vecFx2.x -= vecFx.x;
					vecFx2.y -= vecFx.y;
					vecFx2.z -= vecFx.z;
					VEC_Normalize(vecFx2, vecFx2);
					vecFx2.x *= num2;
					vecFx2.y *= num2;
					vecFx2.z *= num2;
					if (offset == 1)
					{
						num3 = 0;
						vecFx2.y = 0;
					}
					vecFx.x += vecFx2.x;
					vecFx.y += vecFx2.y + num3;
					vecFx.z += vecFx2.z;
					BattleEffect.instance().setPosition(effectId, vecFx);
				}
			}

			public void setEffectPosition(BaseBattleCharacter target, int effectId, VecFx32 pos)
			{
				if (target.unUsedEffectId() != -1)
				{
					target.setEffectId(target.unUsedEffectId(), effectId);
					BattleEffect.instance().setPosition(effectId, pos);
				}
			}

			public bool createDamage(BaseBattleCharacter target, BaseBattleCharacter attacker)
			{
				bool result = false;
				if (target.flag(PLAYER_FLAG.PF_MISS))
				{
					return false;
				}
				VecFx32 vecFx = new VecFx32();
				VecFx32 vecFx2 = new VecFx32();
				if (target.breed() == 0)
				{
					if (target.flag(PLAYER_FLAG.PF_DARK) && calc_.damage(target.battleCharacterId()) == 0)
					{
						return false;
					}
					if (target.flag(PLAYER_FLAG.PF_EXPLOSION) && calc_.damage(target.battleCharacterId()) == 0)
					{
						return false;
					}
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(target);
					characterMng.getPosition(target.characterMngId(), vecFx);
					vecFx2.y = 24576;
					vecFx.y += vecFx2.y;
					if (target.flag(PLAYER_FLAG.PF_RECOVER) && target.isAbsorb() != 0)
					{
						vecFx2.x = 32768;
						vecFx.x += vecFx2.x;
					}
					result = Battle2DManager.instance().damage().createPlayers(target.battleCharacterId(), calc_.damage(target.battleCharacterId()), vecFx, target.flag(PLAYER_FLAG.PF_RECOVER) ? 1 : 0);
					target.setFlag(PLAYER_FLAG.PF_CREATE_2D);
					playerWindow().updateHp(battlePlayer.orderId());
				}
				else if (target.breed() == 1)
				{
					BattleMonster battleMonster = static_cast<BattleMonster>(target);
					characterMng.getPosition(battleMonster.characterMngId(), vecFx);
					vecFx2 = mon.MonsterManager.instance().offset(battleMonster.monsterId()).damagePosition();
					vecFx.x += 4096 * vecFx2.x;
					vecFx.y += 4096 * vecFx2.y;
					vecFx.z += 4096 * vecFx2.z;
					result = ((attacker == null) ? Battle2DManager.instance().damage().create(target.battleCharacterId(), calc_.damage(battleMonster.battleCharacterId()), vecFx, target.flag(PLAYER_FLAG.PF_RECOVER) ? 1 : 0) : ((!attacker.flag(PLAYER_FLAG.PF_OVERISSUE)) ? Battle2DManager.instance().damage().create(target.battleCharacterId(), calc_.damage(battleMonster.battleCharacterId()), vecFx, target.flag(PLAYER_FLAG.PF_RECOVER) ? 1 : 0) : Battle2DManager.instance().damage().create(target.battleCharacterId(), overissueCalc_.damage(battleMonster.battleCharacterId()), vecFx, target.flag(PLAYER_FLAG.PF_RECOVER) ? 1 : 0)));
					target.setFlag(PLAYER_FLAG.PF_CREATE_2D);
				}
				return result;
			}

			public bool createHit(BaseBattleCharacter character, BaseBattleCharacter target)
			{
				bool result = false;
				if (effectCamera_ != 0)
				{
					return result;
				}
				VecFx32 vecFx = new VecFx32();
				VecFx32 vecFx2 = new VecFx32();
				u2d.PopUpHitNumber.puhnKIND puhnKIND = (target.flag(PLAYER_FLAG.PF_MISS) ? u2d.PopUpHitNumber.puhnKIND.puhnkMISS : u2d.PopUpHitNumber.puhnKIND.puhnkNORMAL);
				if (puhnKIND == u2d.PopUpHitNumber.puhnKIND.puhnkMISS)
				{
					if (target.breed() == 1)
					{
						BattleMonster battleMonster = static_cast<BattleMonster>(target);
						characterMng.getPosition(battleMonster.characterMngId(), vecFx);
						vecFx2 = mon.MonsterManager.instance().offset(battleMonster.monsterId()).damagePosition();
						vecFx.x += 4096 * vecFx2.x;
						vecFx.y += 4096 * vecFx2.y;
						vecFx.z += 4096 * vecFx2.z;
						result = Battle2DManager.instance().hit(target.battleCharacterId()).create(target.attackNumber(), vecFx, puhnKIND);
						target.setFlag(PLAYER_FLAG.PF_CREATE_2D);
					}
					else if (target.breed() == 0 || target.breed() == 2)
					{
						BattlePlayer battlePlayer = static_cast<BattlePlayer>(target);
						characterMng.getPosition(battlePlayer.characterMngId(), vecFx);
						vecFx2.y = 49152;
						vecFx.y += vecFx2.y;
						result = Battle2DManager.instance().hit(target.battleCharacterId()).create(target.attackNumber(), vecFx, puhnKIND);
						target.setFlag(PLAYER_FLAG.PF_CREATE_2D);
					}
				}
				else if (character.breed() == 1)
				{
					BattleMonster battleMonster2 = static_cast<BattleMonster>(character);
					characterMng.getPosition(battleMonster2.characterMngId(), vecFx);
					vecFx2 = mon.MonsterManager.instance().offset(battleMonster2.monsterId()).damagePosition();
					vecFx.x += 4096 * vecFx2.x;
					vecFx.y += 4096 * vecFx2.y;
					vecFx.z += 4096 * vecFx2.z;
					result = Battle2DManager.instance().hit(character.battleCharacterId()).create(character.attackNumber(), vecFx, puhnKIND);
					character.setFlag(PLAYER_FLAG.PF_CREATE_2D);
				}
				else if (character.breed() == 0)
				{
					BattlePlayer battlePlayer2 = static_cast<BattlePlayer>(character);
					vecFx.copy(battlePlayer2.rootPosition());
					vecFx2.y = 49152;
					vecFx.y += vecFx2.y;
					if (!character.flag(PLAYER_FLAG.PF_JUMP) && !character.flag(PLAYER_FLAG.PF_OVERISSUE) && !character.flag(PLAYER_FLAG.PF_DARK) && !character.flag(PLAYER_FLAG.PF_EXPLOSION) && !character.flag(PLAYER_FLAG.PF_PITCH))
					{
						result = Battle2DManager.instance().hit(character.battleCharacterId()).createPlayers(character.attackNumber(), vecFx, puhnKIND);
					}
					character.setFlag(PLAYER_FLAG.PF_CREATE_2D);
				}
				else if (character.breed() == 2)
				{
					BattlePlayer battlePlayer3 = static_cast<BattlePlayer>(character);
					characterMng.getPosition(battlePlayer3.characterMngId(), vecFx);
					vecFx2.y = 49152;
					vecFx.y += vecFx2.y;
					result = Battle2DManager.instance().hit(battlePlayer3.battleCharacterId()).create(battlePlayer3.attackNumber(), vecFx, puhnKIND);
					battlePlayer3.setFlag(PLAYER_FLAG.PF_CREATE_2D);
				}
				return result;
			}

			public bool createCritical(BaseBattleCharacter character)
			{
				bool result = false;
				if (effectCamera_ != 0)
				{
					return result;
				}
				if (character.flag(PLAYER_FLAG.PF_MISS))
				{
					return false;
				}
				VecFx32 vecFx = new VecFx32();
				VecFx32 vecFx2 = new VecFx32();
				u2d.PopUpHitNumber.puhnKIND kind = u2d.PopUpHitNumber.puhnKIND.puhnkCRITICAL;
				if (character.breed() == 1)
				{
					BattleMonster battleMonster = static_cast<BattleMonster>(character);
					characterMng.getPosition(battleMonster.characterMngId(), vecFx);
					vecFx2 = mon.MonsterManager.instance().offset(battleMonster.monsterId()).damagePosition();
					vecFx.x += 4096 * vecFx2.x;
					vecFx.y += 4096 * vecFx2.y;
					vecFx.z += 4096 * vecFx2.z;
					vecFx.y += 16384;
					result = Battle2DManager.instance().critical().create(character.attackNumber(), vecFx, kind);
				}
				else if (character.breed() == 0 || character.breed() == 2)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(character);
					characterMng.getPosition(battlePlayer.characterMngId(), vecFx);
					vecFx2.y = 36864;
					vecFx.y += vecFx2.y;
					result = ((character.breed() != 0) ? Battle2DManager.instance().critical().create(character.attackNumber(), vecFx, kind) : Battle2DManager.instance().critical().createPlayers(character.attackNumber(), vecFx, kind));
				}
				return result;
			}

			public void createCriticalFlash()
			{
				flash_.setFlash(3, 3, ds.setGXRgb(31, 31, 31));
			}

			public void setFlash(byte flashNum, short flashFrame, short intervalFrame)
			{
				flash_.setFlashEx(flashNum, flashFrame, intervalFrame, ds.setGXRgb(31, 31, 31));
			}

			public bool isOnlyAllMagic(int _id)
			{
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter((short)_id);
				if (magicParameter == null)
				{
					return false;
				}
				if (magicParameter.itemId() == 4008)
				{
					return false;
				}
				short num = magicParameter.targetPossible();
				if ((num & 2) != 0)
				{
					return false;
				}
				if ((num & 4) != 0)
				{
					return false;
				}
				if ((num & 0x80) != 0)
				{
					return false;
				}
				if ((num & 0x100) != 0)
				{
					return false;
				}
				if ((num & 8) != 0 || (num & 0x200) != 0)
				{
					return true;
				}
				return false;
			}

			public bool isOnlySingleMagic(int _id)
			{
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter((short)_id);
				if (magicParameter == null)
				{
					return false;
				}
				if (magicParameter.itemId() == 4008)
				{
					return false;
				}
				short num = magicParameter.targetPossible();
				if ((num & 8) != 0)
				{
					return false;
				}
				if ((num & 4) != 0)
				{
					return false;
				}
				if ((num & 0x200) != 0)
				{
					return false;
				}
				if ((num & 0x100) != 0)
				{
					return false;
				}
				if ((num & 2) != 0 || (num & 0x80) != 0)
				{
					return true;
				}
				return false;
			}

			public bool calcEscape()
			{
				BattlePlayer battlePlayer = static_cast<BattlePlayer>(nowCharacter());
				bool flag = false;
				EscapeFormula escapeFormula = new EscapeFormula();
				if (battlePlayer.actionId() == 9)
				{
					return escapeFormula.calcTakeAPowder(battlePlayer, characterManager_.playerParty(), characterManager_.monsterParty());
				}
				return escapeFormula.calcEscapePlayer(characterManager_.playerParty(), characterManager_.monsterParty());
			}

			public bool changeFrog(BaseBattleCharacter target)
			{
				if (target.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(target);
					battlePlayer.changeFrog(flag: false);
				}
				else if (target.breed() == 1)
				{
					BattleMonster battleMonster = static_cast<BattleMonster>(target);
					battleMonster.changeFrog();
				}
				return true;
			}

			public bool changeLilliput(BaseBattleCharacter target)
			{
				if (target.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(target);
					battlePlayer.changeLilliput(flag: false);
				}
				else if (target.breed() == 1)
				{
					BattleMonster battleMonster = static_cast<BattleMonster>(target);
					battleMonster.changeLilliput();
				}
				return true;
			}

			public bool returnCharacter(BaseBattleCharacter target)
			{
				if (target.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(target);
					battlePlayer.returnHuman();
				}
				else if (target.breed() == 1)
				{
					BattleMonster battleMonster = static_cast<BattleMonster>(target);
					battleMonster.returnMonster();
				}
				return true;
			}

			public void startCommon()
			{
				if (characterManager().monsterParty().aliveNumber() == 0)
				{
					commonState_ = 7;
					return;
				}
				clearResult();
				workCounter_ = 0;
				commonState_ = 1;
			}

			public void endCommon()
			{
				commonState_ = 0;
			}

			public void checkCondition()
			{
				int num = 0;
				for (num = 0; num < 6; num++)
				{
					BattleMonster battleMonster = characterManager().monsterParty().battleMonster(num);
					if (battleMonster != null && battleMonster.isEnable())
					{
						battleMonster.condition().calcConditionTime();
					}
				}
				for (num = 0; num < 4; num++)
				{
					BattlePlayer battlePlayer = characterManager().playerParty().battlePlayer(num);
					if (battlePlayer != null && battlePlayer.isEnable())
					{
						battlePlayer.condition().calcConditionTime();
						battlePlayer.changeConditionEffect();
					}
				}
				commonState_ = 2;
			}

			public void poisonCommonStart()
			{
				if (++workCounter_ >= WAIT_COMMON_START_FRAME)
				{
					workCounter_ = 0;
					commonAttacker_.clearTargetId();
					if (calc_.calcPoison(characterManager(), commonAttacker_))
					{
						setCheckFlag(Start2DProcess);
						setCheckFlag(EndPlayerProcess);
						setCheckFlag(EndEffectProcess);
						draw2D2();
						commonState_ = 3;
					}
					else
					{
						commonState_ = 5;
					}
				}
			}

			public void poisonCommon()
			{
				if (checkEnd2DNoTarget())
				{
					commonState_ = 4;
				}
			}

			public void poisonCommonEnd()
			{
				if (deadCharacters(commonAttacker_))
				{
					commonState_ = 5;
				}
				flash_.draw();
			}

			public void stoneCommon()
			{
				commonState_ = 6;
			}

			public void clearConditionCommon()
			{
				commonState_ = 7;
			}

			public void setPoisonDamage()
			{
			}

			public TurnSystem()
			{
				phase_ = Phase.Initialize;
				effectCamera_ = 0;
				nowCharacter_ = null;
				counterCharacter_ = null;
				coverPlayer_ = null;
				characterManager_ = null;
				summonDataManager_ = null;
				geographyManager_ = null;
				breakRootMonster_ = null;
				breakAfterMonster_ = null;
			}

			public void setPhase(Phase phase)
			{
				phase_ = phase;
			}

			public Phase phase()
			{
				return phase_;
			}

			public void setNowCharacter(BaseBattleCharacter character)
			{
				nowCharacter_ = character;
			}

			public BaseBattleCharacter nowCharacter()
			{
				return nowCharacter_;
			}

			public void setCharacterManager(BattleCharacterManager characterManager)
			{
				characterManager_ = characterManager;
			}

			public BattleCharacterManager characterManager()
			{
				return characterManager_;
			}

			public void setSummonDataManager(SummonDataManager data)
			{
				summonDataManager_ = data;
			}

			public SummonDataManager summonDataManager()
			{
				return summonDataManager_;
			}

			public void setGeographyManager(GeographyManager data)
			{
				geographyManager_ = data;
			}

			public GeographyManager geographyManager()
			{
				return geographyManager_;
			}

			public bool isPlayerEscape()
			{
				return isPlayerEscape_;
			}

			public void onPlayerEscape()
			{
				isPlayerEscape_ = true;
			}

			public void offPlayerEscape()
			{
				isPlayerEscape_ = false;
			}

			public BaseBattleCharacter counterCharacter()
			{
				return counterCharacter_;
			}

			public void setCounterCharacter(BaseBattleCharacter character)
			{
				counterCharacter_ = character;
			}

			public BattlePlayer coverPlayer()
			{
				return coverPlayer_;
			}

			public void setCoverPlayer(BattlePlayer player)
			{
				coverPlayer_ = player;
			}

			public BattleMonster breakRootMonster()
			{
				return breakRootMonster_;
			}

			public void setBreakRootMonster(BattleMonster monster)
			{
				breakRootMonster_ = monster;
			}

			public BattleMonster breakAfterMonster()
			{
				return breakAfterMonster_;
			}

			public void setBreakAfterMonster(BattleMonster monster)
			{
				breakAfterMonster_ = monster;
			}

			public int checkState()
			{
				return checkState_;
			}

			public void clearCheckState()
			{
				checkState_ = 0;
			}

			public void setCheckFlag(uint flag)
			{
				checkFlag_ |= flag;
			}

			public void clearCheckFlag(uint flag)
			{
				checkFlag_ &= ~flag;
			}

			public bool checkFlag(uint flag)
			{
				if ((checkFlag_ & flag) == 0)
				{
					return false;
				}
				return true;
			}

			public void clearCheckFlagAll()
			{
				checkFlag_ = 0u;
			}

			public int state()
			{
				return state_;
			}

			public void setPlayerWindow(PlayerWindow window)
			{
				playerWindow_ = window;
			}

			public PlayerWindow playerWindow()
			{
				return playerWindow_;
			}

			public void setCheckActionType(int type)
			{
				checkActionType_ = type;
			}
		}
	}
}
