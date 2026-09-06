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
		public class PlayerTurnSystem
		{
			public enum FLAG_TYPE
			{
				FLAG_HIT = 1,
				FLAG_RANDAM = 2,
				FLAG_FINISH = 4,
				FLAG_STEAL = 8,
				FLAG_CRITICAL = 0x10,
				FLAG_STEAL_SUCCESS = 0x20,
				FLAG_DRAK = 0x80
			}

			public enum PLAYER_ACTION_STATE
			{
				PAS_IS_DATA_INITIALIZE = 0,
				PAS_IDLE = 1,
				PAS_FRONT_READY = 2,
				PAS_FRONT = 3,
				PAS_BACK = 4,
				PAS_CREATE_HELP_WINDOW = 5,
				PAS_END_HELP_WINDOW = 6,
				PAS_NORMAL_ATTACK = 7,
				PAS_NORMAL_MAGIC = 8,
				PAS_ITEM = 9,
				PAS_BACK_ATTACK = 10,
				PAS_GUARD_START = 11,
				PAS_GUARD = 12,
				PAS_TAKE_A_POWDER = 13,
				PAS_TAKE_A_POWDER_SUCCESS = 14,
				PAS_STEAL = 15,
				PAS_SUMMON_MAGIC = 16,
				PAS_MORE_MOVE_READY = 17,
				PAS_MORE_MOVE_FRONT = 18,
				PAS_MORE_MOVE_BACK = 19,
				PAS_POISE = 20,
				PAS_COVER_START = 21,
				PAS_CHECK = 22,
				PAS_DETECT = 23,
				PAS_JUMP_START = 24,
				PAS_JUMP_END = 25,
				PAS_DARK = 26,
				PAS_ROLL_UP = 27,
				PAS_EXPLOSION = 28,
				PAS_PROVOCATION = 29,
				PAS_SET_TARGET_RANDAM = 30,
				PAS_PITCH = 31,
				PAS_GEOGRAPHY = 32,
				PAS_SONG = 33,
				PAS_ABILITY_CAMERA_START = 34,
				PAS_ABILITY_CAMERA_MOVE = 35,
				PAS_ABILITY_CAMERA_END = 36,
				PAS_ABILITY_EFFECT_START = 37,
				PLAYER_ACTION_STATE_MAX = 38,
				NON_STATE = -1
			}

			public enum SUMMON_STATE
			{
				PLAYER_ACTION,
				CHARACTER_DISAPPEAR,
				SUMMON_EXECUTE,
				SUMMON_ESCAPE,
				SUMMON_STATE_MAX
			}

			public const FLAG_TYPE FLAG_HIT = FLAG_TYPE.FLAG_HIT;

			public const FLAG_TYPE FLAG_RANDAM = FLAG_TYPE.FLAG_RANDAM;

			public const FLAG_TYPE FLAG_FINISH = FLAG_TYPE.FLAG_FINISH;

			public const FLAG_TYPE FLAG_STEAL = FLAG_TYPE.FLAG_STEAL;

			public const FLAG_TYPE FLAG_CRITICAL = FLAG_TYPE.FLAG_CRITICAL;

			public const FLAG_TYPE FLAG_STEAL_SUCCESS = FLAG_TYPE.FLAG_STEAL_SUCCESS;

			public const FLAG_TYPE FLAG_DRAK = FLAG_TYPE.FLAG_DRAK;

			public const PLAYER_ACTION_STATE PAS_IS_DATA_INITIALIZE = PLAYER_ACTION_STATE.PAS_IS_DATA_INITIALIZE;

			public const PLAYER_ACTION_STATE PAS_IDLE = PLAYER_ACTION_STATE.PAS_IDLE;

			public const PLAYER_ACTION_STATE PAS_FRONT_READY = PLAYER_ACTION_STATE.PAS_FRONT_READY;

			public const PLAYER_ACTION_STATE PAS_FRONT = PLAYER_ACTION_STATE.PAS_FRONT;

			public const PLAYER_ACTION_STATE PAS_BACK = PLAYER_ACTION_STATE.PAS_BACK;

			public const PLAYER_ACTION_STATE PAS_CREATE_HELP_WINDOW = PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW;

			public const PLAYER_ACTION_STATE PAS_END_HELP_WINDOW = PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW;

			public const PLAYER_ACTION_STATE PAS_NORMAL_ATTACK = PLAYER_ACTION_STATE.PAS_NORMAL_ATTACK;

			public const PLAYER_ACTION_STATE PAS_NORMAL_MAGIC = PLAYER_ACTION_STATE.PAS_NORMAL_MAGIC;

			public const PLAYER_ACTION_STATE PAS_ITEM = PLAYER_ACTION_STATE.PAS_ITEM;

			public const PLAYER_ACTION_STATE PAS_BACK_ATTACK = PLAYER_ACTION_STATE.PAS_BACK_ATTACK;

			public const PLAYER_ACTION_STATE PAS_GUARD_START = PLAYER_ACTION_STATE.PAS_GUARD_START;

			public const PLAYER_ACTION_STATE PAS_GUARD = PLAYER_ACTION_STATE.PAS_GUARD;

			public const PLAYER_ACTION_STATE PAS_TAKE_A_POWDER = PLAYER_ACTION_STATE.PAS_TAKE_A_POWDER;

			public const PLAYER_ACTION_STATE PAS_TAKE_A_POWDER_SUCCESS = PLAYER_ACTION_STATE.PAS_TAKE_A_POWDER_SUCCESS;

			public const PLAYER_ACTION_STATE PAS_STEAL = PLAYER_ACTION_STATE.PAS_STEAL;

			public const PLAYER_ACTION_STATE PAS_SUMMON_MAGIC = PLAYER_ACTION_STATE.PAS_SUMMON_MAGIC;

			public const PLAYER_ACTION_STATE PAS_MORE_MOVE_READY = PLAYER_ACTION_STATE.PAS_MORE_MOVE_READY;

			public const PLAYER_ACTION_STATE PAS_MORE_MOVE_FRONT = PLAYER_ACTION_STATE.PAS_MORE_MOVE_FRONT;

			public const PLAYER_ACTION_STATE PAS_MORE_MOVE_BACK = PLAYER_ACTION_STATE.PAS_MORE_MOVE_BACK;

			public const PLAYER_ACTION_STATE PAS_POISE = PLAYER_ACTION_STATE.PAS_POISE;

			public const PLAYER_ACTION_STATE PAS_COVER_START = PLAYER_ACTION_STATE.PAS_COVER_START;

			public const PLAYER_ACTION_STATE PAS_CHECK = PLAYER_ACTION_STATE.PAS_CHECK;

			public const PLAYER_ACTION_STATE PAS_DETECT = PLAYER_ACTION_STATE.PAS_DETECT;

			public const PLAYER_ACTION_STATE PAS_JUMP_START = PLAYER_ACTION_STATE.PAS_JUMP_START;

			public const PLAYER_ACTION_STATE PAS_JUMP_END = PLAYER_ACTION_STATE.PAS_JUMP_END;

			public const PLAYER_ACTION_STATE PAS_DARK = PLAYER_ACTION_STATE.PAS_DARK;

			public const PLAYER_ACTION_STATE PAS_ROLL_UP = PLAYER_ACTION_STATE.PAS_ROLL_UP;

			public const PLAYER_ACTION_STATE PAS_EXPLOSION = PLAYER_ACTION_STATE.PAS_EXPLOSION;

			public const PLAYER_ACTION_STATE PAS_PROVOCATION = PLAYER_ACTION_STATE.PAS_PROVOCATION;

			public const PLAYER_ACTION_STATE PAS_SET_TARGET_RANDAM = PLAYER_ACTION_STATE.PAS_SET_TARGET_RANDAM;

			public const PLAYER_ACTION_STATE PAS_PITCH = PLAYER_ACTION_STATE.PAS_PITCH;

			public const PLAYER_ACTION_STATE PAS_GEOGRAPHY = PLAYER_ACTION_STATE.PAS_GEOGRAPHY;

			public const PLAYER_ACTION_STATE PAS_SONG = PLAYER_ACTION_STATE.PAS_SONG;

			public const PLAYER_ACTION_STATE PAS_ABILITY_CAMERA_START = PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START;

			public const PLAYER_ACTION_STATE PAS_ABILITY_CAMERA_MOVE = PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_MOVE;

			public const PLAYER_ACTION_STATE PAS_ABILITY_CAMERA_END = PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END;

			public const PLAYER_ACTION_STATE PAS_ABILITY_EFFECT_START = PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START;

			public const PLAYER_ACTION_STATE PLAYER_ACTION_STATE_MAX = PLAYER_ACTION_STATE.PLAYER_ACTION_STATE_MAX;

			public const PLAYER_ACTION_STATE NON_STATE = PLAYER_ACTION_STATE.NON_STATE;

			public const SUMMON_STATE PLAYER_ACTION = SUMMON_STATE.PLAYER_ACTION;

			public const SUMMON_STATE CHARACTER_DISAPPEAR = SUMMON_STATE.CHARACTER_DISAPPEAR;

			public const SUMMON_STATE SUMMON_EXECUTE = SUMMON_STATE.SUMMON_EXECUTE;

			public const SUMMON_STATE SUMMON_ESCAPE = SUMMON_STATE.SUMMON_ESCAPE;

			public const SUMMON_STATE SUMMON_STATE_MAX = SUMMON_STATE.SUMMON_STATE_MAX;

			public const int DISAPPEAR_FRAME_MAX = 10;

			private int workCounter_;

			private uint flag_;

			private SummonManager summonManager_ = new SummonManager();

			private BattlePlayer nowPlayer_;

			private SUMMON_STATE summonState_;

			public void initializeAll()
			{
				workCounter_ = 0;
				flag_ = 0u;
				nowPlayer_ = null;
				summonState_ = SUMMON_STATE.PLAYER_ACTION;
			}

			public void initialize(TurnSystem T)
			{
				setNowPlayer(T.nowCharacter());
				OS_Printf("//----------------------------------------------------------------------------------\n");
				OS_Printf("// プレイヤー %d の行動開始\n", nowPlayer().playerId());
				if (!nowPlayer().isEnable() || T.characterManager().monsterParty().aliveNumber() == 0 || !nowPlayer().condition().isCanAction() || !T.isTarget(nowPlayer()))
				{
					setIdle();
					return;
				}
				nowPlayer().phaseInitialize();
				clearFlagAll();
				if (nowPlayer().actionId() != 2 && nowPlayer().actionId() != 9 && nowPlayer().actionId() != 26)
				{
					nowPlayer().player().jobManager().addJobSkillExp();
				}
				if (nowPlayer().prevActionId() != 21)
				{
					nowPlayer().clearRollUpLevel();
				}
				nowPlayer().setEffectNumber(-1, 0);
				nowPlayer().setEffectNumber(-1, 1);
				if (nowPlayer().condition().isConfusion())
				{
					nowPlayer().clearTargetId();
					T.setTargetRandam(nowPlayer(), T.characterManager().playerParty(), reflect: false);
					nowPlayer().setActionId(1);
				}
				switch (nowPlayer().actionId())
				{
				case 1:
				case 25:
					initializeNormalAttack(T);
					break;
				case 2:
					initializeEscape(T);
					break;
				case 3:
					initializeGuard(T);
					break;
				case 5:
				case 6:
					initializeMagic(T);
					break;
				case 7:
					initializeItem(T);
					break;
				case 9:
					initializeTakeAPowder(T);
					break;
				case 8:
					initializeSteal(T);
					break;
				case 10:
					initializeNormalAttack(T);
					break;
				case 11:
					initializeNormalAttack(T);
					break;
				case 12:
					initializePoise(T);
					break;
				case 13:
					initializeCoverStart(T);
					break;
				case 14:
					initializeCheck(T);
					break;
				case 15:
					initializeDetect(T);
					break;
				case 16:
					initializeJumpStart(T);
					break;
				case 17:
					initializeJumpEnd(T);
					break;
				case 18:
					initializeDark(T);
					break;
				case 21:
					initializeRollUp(T);
					break;
				case 23:
					initializeProvocation(T);
					break;
				case 24:
					initializeOverissue(T);
					break;
				case 22:
					initializePitch(T);
					break;
				case 19:
					initializeGeography(T);
					break;
				case 20:
					initializeSong(T);
					break;
				case 26:
					initializeChangeFormation(T);
					break;
				default:
					T.setPhase(TurnSystem.Phase.Terminate);
					break;
				}
				nowPlayer().setPrevActionId();
			}

			public void terminate(TurnSystem T)
			{
				if (nowPlayer() != null && !nowPlayer().condition().isFrog())
				{
					nowPlayer().removeEquipWeaponMotion(pl.HAND_TYPE.RIGHT_HAND);
					nowPlayer().removeEquipWeaponMotion(pl.HAND_TYPE.LEFT_HAND);
					nowPlayer().removeMagicMotion();
					nowPlayer().removeJobMotion();
					nowPlayer().removePitchMotion();
				}
				BattleSE.instance().free();
			}

			public void execute(TurnSystem T)
			{
				switch (T.nowCharacter().actionId())
				{
				case 1:
				case 25:
					stateNormalAttack(T);
					break;
				case 2:
					executeEscape(T);
					break;
				case 3:
					stateGuard(T);
					break;
				case 5:
				case 6:
					stateMagic(T);
					break;
				case 7:
					stateItem(T);
					break;
				case 9:
					stateTakeAPowder(T);
					break;
				case 8:
					stateSteal(T);
					break;
				case 10:
					stateNormalAttack(T);
					break;
				case 11:
					stateKnockOver(T);
					break;
				case 12:
					statePoise(T);
					break;
				case 13:
					stateCoverStart(T);
					break;
				case 14:
					stateCheck(T);
					break;
				case 15:
					stateDetect(T);
					break;
				case 16:
					stateJumpStart(T);
					break;
				case 17:
					stateJumpEnd(T);
					break;
				case 18:
					stateDark(T);
					break;
				case 21:
					stateRollUp(T);
					break;
				case 23:
					stateProvocation(T);
					break;
				case 24:
					stateOverissue(T);
					break;
				case 22:
					statePitch(T);
					break;
				case 19:
					stateGeography(T);
					break;
				case 20:
					stateSong(T);
					break;
				case 26:
					executeChangeFormation(T);
					break;
				default:
					T.setPhase(TurnSystem.Phase.Terminate);
					break;
				}
			}

			public void setNowPlayer(BaseBattleCharacter nowCharacter)
			{
				if (nowCharacter != null && (nowCharacter.breed() == 0 || nowCharacter.breed() == 2))
				{
					nowPlayer_ = static_cast<BattlePlayer>(nowCharacter);
				}
				else
				{
					nowPlayer_ = null;
				}
			}

			public void setIdle()
			{
				if (nowPlayer().actionId() == 5 || nowPlayer().actionId() == 6)
				{
					nowPlayer().setIdleType(0);
				}
				if (nowPlayer().actionId() == 5 || nowPlayer().actionId() == 6 || nowPlayer().actionId() == 7 || nowPlayer().actionId() == 22)
				{
					nowPlayer().setHiddenWeaponFlag(pl.HAND_TYPE.RIGHT_HAND, flag: true);
					nowPlayer().setHiddenWeaponFlag(pl.HAND_TYPE.LEFT_HAND, flag: true);
				}
				nowPlayer().setActionId(0);
			}

			public void isData(TurnSystem T, PLAYER_ACTION_STATE nextState)
			{
				if (TexDivideLoader.getSingleton().tdlIsEmpty() && !BattleSE.instance().isLoadAsync())
				{
					workCounter_ = 0;
					T.setState((int)nextState);
				}
			}

			public void idle(TurnSystem T, int frame, PLAYER_ACTION_STATE nextState)
			{
				if (frame == ++workCounter_)
				{
					workCounter_ = 0;
					T.setState((int)nextState);
				}
			}

			public void moveFrontReady(TurnSystem T)
			{
				if (nowPlayer().isPlayerActionEnd() || nowPlayer().playerActionId() == -1)
				{
					nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_FRONT_ATTACK);
					T.setState(3);
				}
			}

			public void moveFront(TurnSystem T, BATTLE_ACTION_TYPE type, PLAYER_ACTION_STATE state)
			{
				if (nowPlayer().isPlayerActionEnd())
				{
					workCounter_ = 0;
					nowPlayer().setNextPlayerActionId(type);
					T.setState((int)state);
				}
			}

			public void moveBack(TurnSystem T, BATTLE_ACTION_TYPE type, sbyte idle)
			{
				if (nowPlayer().isPlayerActionEnd())
				{
					nowPlayer().setIdleType(idle);
					nowPlayer().setNextPlayerActionId(type);
					T.setPhase(TurnSystem.Phase.MonsterExecute);
				}
			}

			public void createHelpWindow(TurnSystem T, int messageId, PLAYER_ACTION_STATE state)
			{
				Battle2DManager.instance().helpWindow().createHelpWindow(messageId, 0, 0);
				T.setState((int)state);
			}

			public void endHelpWindow(TurnSystem T, int frame, BATTLE_ACTION_TYPE type, PLAYER_ACTION_STATE state, int hidden)
			{
				if (frame == ++workCounter_)
				{
					if (hidden != 0)
					{
						stageMng.setHidden(flag: false);
					}
					workCounter_ = 0;
					Battle2DManager.instance().helpWindow().releaseHelpWindow();
					nowPlayer().setNextPlayerActionId(type);
					T.setState((int)state);
				}
			}

			public void createAbilityName(TurnSystem T, PLAYER_ACTION_STATE state)
			{
				pl.Command command = nowPlayer().player().jobManager().command();
				int nameId_ = pl.PlayerParty.instance().abilityList(command.commandId(command.nowCommand())).nameId_;
				createHelpWindow(T, nameId_, state);
				Battle2DManager.instance().helpWindow().setMsdHandle(0);
			}

			public void startAbilityCamera(TurnSystem T, PLAYER_ACTION_STATE nextState)
			{
				if (!nowPlayer().condition().isFrog() && !nowPlayer().condition().isLilliput() && ds.RandomNumber.rand32(101u) < 30)
				{
					battleDisplay.setAbilityCamera(nowPlayer().player());
					stageMng.setHidden(flag: true);
					T.playerWindow().show(flag: false);
				}
				T.setState((int)nextState);
			}

			public void moveAbilityCamera(TurnSystem T, PLAYER_ACTION_STATE nextState)
			{
			}

			public void endAbilityCamera(TurnSystem T, PLAYER_ACTION_STATE nextState)
			{
				T.playerWindow().show(flag: true);
				battleDisplay.stateBattleCamera();
				T.setState((int)nextState);
			}

			public void startAbilityEffect(TurnSystem T, PLAYER_ACTION_STATE nextState)
			{
				int num = BattleEffect.instance().create(231, 1);
				int num2 = nowPlayer().unUsedEffectId();
				if (num2 != -1)
				{
					nowPlayer().effectId_set(num2, num);
					VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
					characterMng.getPosition(nowPlayer().characterMngId(), fnd_reuse_pos);
					BattleEffect.instance().setPosition(num, fnd_reuse_pos);
					BattleSE.instance().play(200, 6);
					T.setState((int)nextState);
				}
			}

			public void createBaseHit2D(TurnSystem T, BattlePlayer player, BaseBattleCharacter target)
			{
				T.createHit(player, target);
				T.createDamage(target, null);
			}

			public void initializeNormalAttack(TurnSystem T)
			{
				if (nowPlayer().condition().isFrog() && nowPlayer().actionId() != 1)
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				if (!nowPlayer().condition().isConfusion() && T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0)) == null)
				{
					nowPlayer().clearTargetId();
					T.setTargetRandam(nowPlayer(), T.characterManager().monsterParty(), reflect: false);
				}
				nowPlayer().setIdleType(1);
				nowPlayer().clearFlag(PLAYER_FLAG.PF_CRITICAL);
				if (nowPlayer().actionId() == 11)
				{
					nowPlayer().setFlag(PLAYER_FLAG.PF_KNOCK_OVER);
				}
				T.calcNormalAttackDamage(T.nowCharacter());
				T.setOverissueDamage(nowPlayer());
				T.setNormalAttackDamage(T.nowCharacter());
				if (!nowPlayer().condition().isFrog())
				{
					nowPlayer().addEquipWeaponMotion(pl.HAND_TYPE.RIGHT_HAND);
					nowPlayer().addEquipWeaponMotion(pl.HAND_TYPE.LEFT_HAND);
				}
				if (T.characterManager().monsterParty().aliveNumber() != 0 || !nowPlayer().isFinishAttack() || nowPlayer().condition().isFrog())
				{
					nowPlayer().clearFlag(PLAYER_FLAG.PF_FINISH);
				}
				else
				{
					nowPlayer().setFlag(PLAYER_FLAG.PF_FINISH);
				}
				nowPlayer().clearOverissueNumber();
				setNormalAttackEfp(pl.HAND_TYPE.RIGHT_HAND);
				setNormalAttackEfp(pl.HAND_TYPE.LEFT_HAND);
				BattleEffect.instance().addEfp(231);
				BattleSE.instance().load(201);
				if (nowPlayer().actionId() == 11)
				{
					BattleEffect.instance().addEfp(431);
				}
				T.setState(0);
			}

			public void stateNormalAttack(TurnSystem T)
			{
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
				switch (T.state())
				{
				case 0:
					if (nowPlayer().flag(PLAYER_FLAG.PF_COUNTER))
					{
						isData(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					}
					else
					{
						isData(T, PLAYER_ACTION_STATE.PAS_FRONT_READY);
					}
					break;
				case 5:
					createHelpWindow(T, 21, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, PLAYER_ACTION_STATE.PAS_FRONT_READY, 1);
					break;
				case 2:
					moveFrontReady(T);
					break;
				case 3:
					moveFront(T, BATTLE_ACTION_TYPE.DBA_NORMAL_ATTACK, PLAYER_ACTION_STATE.PAS_NORMAL_ATTACK);
					break;
				case 7:
					executeNormalAttack(T, baseBattleCharacterFromBreed);
					break;
				case 10:
					moveBackAttack(T, baseBattleCharacterFromBreed);
					break;
				case 1:
				case 4:
				case 8:
				case 9:
					break;
				}
			}

			public void executeNormalAttack(TurnSystem T, BaseBattleCharacter target)
			{
				int motionIndex = characterMng.getMotionIndex(nowPlayer().characterMngId());
				int currentFrame = (int)characterMng.getCurrentFrame(nowPlayer().characterMngId());
				pl.PlayerNormalAttackParameter playerNormalAttackParameter = pl.PlayerParty.instance().normalAttack(motionIndex);
				for (int i = 0; i < 2; i++)
				{
					if (playerNormalAttackParameter == null)
					{
						break;
					}
					int draw = 0;
					if (!target.flag(PLAYER_FLAG.PF_MISS))
					{
						draw = 1;
					}
					if (nowPlayer().condition().isFrog())
					{
						draw = 1;
					}
					pl.HAND_TYPE hAND_TYPE = nowPlayer().nowAttackHand();
					int num = nowPlayer().effectNumber((int)hAND_TYPE);
					if (num > 0)
					{
						ys.Effects effects = playerNormalAttackParameter.effect(i);
						if (effects.category_ == 221)
						{
							effects = selectPitchEffect(T, (int)hAND_TYPE, effects);
						}
						if (!flag(FLAG_TYPE.FLAG_RANDAM))
						{
							if (T.createEffect(currentFrame, effects, target, 0, 0, draw))
							{
								hiddenPitchWeapon(T, hAND_TYPE);
								nowPlayer().subEffectNumber((int)hAND_TYPE);
								criticalFlash(T);
								setFlag(FLAG_TYPE.FLAG_RANDAM);
							}
						}
						else if (T.createEffect(currentFrame, effects, target, 0, playerNormalAttackParameter.randamFlag(), draw))
						{
							hiddenPitchWeapon(T, hAND_TYPE);
							nowPlayer().subEffectNumber((int)hAND_TYPE);
							criticalFlash(T);
						}
						if (!target.flag(PLAYER_FLAG.PF_MISS))
						{
							int critical = (nowPlayer().flag(PLAYER_FLAG.PF_CRITICAL) ? 1 : 0);
							if (nowPlayer().actionId() == 24)
							{
								critical = 0;
							}
							ys.Effects effects2 = playerNormalAttackParameter.se(i);
							if (effects2.category_ == 201 && effects2.member_ == 19)
							{
								effects2 = selectAxeSE((int)hAND_TYPE, effects2);
							}
							BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
							if (baseBattleCharacterFromBreed != null && baseBattleCharacterFromBreed.breed() == 0 && effects2.category_ == 201 && effects2.member_ == 22)
							{
								effects2.member_ = 8;
							}
							int guard = 0;
							if (baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_GUARD))
							{
								guard = 1;
							}
							else if (baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_MORE_GUARD))
							{
								guard = 2;
							}
							T.playSE(currentFrame, effects2, guard, 0, critical);
							T.playFlash(currentFrame, playerNormalAttackParameter.effect(i).frameCounter_, baseBattleCharacterFromBreed);
							T.startDamageAction(currentFrame, playerNormalAttackParameter.targetMotionStartFrame(i), baseBattleCharacterFromBreed);
						}
						else
						{
							T.playSE(currentFrame, playerNormalAttackParameter.se(i), 0, 1, 0);
							hiddenPitchWeapon(T, hAND_TYPE);
						}
					}
					else
					{
						T.playSE(currentFrame, playerNormalAttackParameter.se(i), 0, 1, 0);
						hiddenPitchWeapon(T, hAND_TYPE);
					}
					if (target.flag(PLAYER_FLAG.PF_MISS))
					{
						T.playSE(currentFrame, playerNormalAttackParameter.se(i), 0, 1, 0);
						hiddenPitchWeapon(T, hAND_TYPE);
					}
				}
				if (!flag(FLAG_TYPE.FLAG_FINISH))
				{
					if (nowPlayer().isPlayerActionEnd())
					{
						if (T.characterManager().monsterParty().aliveNumber() != 0 || !nowPlayer().isFinishAttack() || nowPlayer().condition().isFrog())
						{
							endNormalAttack(T, target);
							return;
						}
						nowPlayer().clearMissAttackFlag();
						nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_FINISH);
						setFlag(FLAG_TYPE.FLAG_FINISH);
					}
				}
				else if (nowPlayer().isPlayerActionEnd())
				{
					endNormalAttack(T, target);
				}
			}

			public void endNormalAttack(TurnSystem T, BaseBattleCharacter target)
			{
				nowPlayer().decOverissueNumber();
				clearFlag(FLAG_TYPE.FLAG_FINISH);
				createHit2D(T, target);
				setNextActionNowPlayer(T);
				T.setState(10);
				T.setCheckFlag(TurnSystem.EndEffectProcess);
			}

			public void moveBackAttack(TurnSystem T, BaseBattleCharacter target)
			{
				isEndNormalAttack2D(T, target);
				if (nowPlayer().breed() == 0)
				{
					checkAttackPlayer(T);
				}
				else
				{
					checkAttackNpc(T);
				}
				if (nowPlayer().breed() == 0)
				{
					if (nowPlayer().flag(PLAYER_FLAG.PF_OVERISSUE))
					{
						if (T.characterManager().monsterParty().aliveNumber() == 0 || !nowPlayer().isEndOverissueNumber())
						{
							nowPlayer().clearTargetId();
							for (int i = 0; i < 12; i++)
							{
								nowPlayer().setTargetId(i, (short)T.overissueTargetId_[i]);
							}
							T.deadCharacters(nowPlayer());
						}
						else
						{
							T.setCheckFlag(TurnSystem.EndEnemyProcess);
						}
					}
					else
					{
						T.deadCharacters(nowPlayer());
					}
				}
				else
				{
					T.deadCharacters(nowPlayer());
				}
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && T.checkFlag(TurnSystem.EndEnemyProcess) && T.checkFlag(TurnSystem.End2DProcess))
				{
					if (nowPlayer().breed() == 0 && nowPlayer().player().equipParameter().isEquipBow() && nowPlayer().player().equipParameter().isEquipArrow())
					{
						nowPlayer().player().equipParameter().decArrow();
					}
					if (isOverissue(T))
					{
						setOverissue(T);
					}
					else
					{
						T.setPhase(TurnSystem.Phase.MonsterExecute);
					}
				}
			}

			public void setNormalAttackEfp(pl.HAND_TYPE hand)
			{
				int motionId;
				if (nowPlayer().condition().isFrog())
				{
					motionId = 118;
				}
				else
				{
					itm.WEAPON_SYSTEM weapon = nowPlayer().player().equipParameter().equipHand(hand)
						.weaponSystem();
					motionId = nowPlayer().equipWeaponMotionIndex(weapon) + 1;
				}
				pl.PlayerNormalAttackParameter playerNormalAttackParameter = pl.PlayerParty.instance().normalAttack(motionId);
				int num = playerNormalAttackParameter.effect(0).category_;
				if (num == 221)
				{
					num = 224;
				}
				BattleEffect.instance().addEfp(num);
			}

			public void createHit2D(TurnSystem T, BaseBattleCharacter target)
			{
				if ((nowPlayer().flag(PLAYER_FLAG.PF_OVERISSUE) && T.characterManager().monsterParty().aliveNumber() != 0 && nowPlayer().isEndOverissueNumber()) || flag(FLAG_TYPE.FLAG_HIT))
				{
					return;
				}
				T.createHit(nowPlayer(), target);
				if (nowPlayer().flag(PLAYER_FLAG.PF_OVERISSUE))
				{
					for (int i = 0; i < 12; i++)
					{
						BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed((short)T.overissueTargetId_[i]);
						if (baseBattleCharacterFromBreed != null)
						{
							T.createDamage(baseBattleCharacterFromBreed, nowPlayer());
						}
					}
				}
				else
				{
					for (int j = 0; j < 12; j++)
					{
						BaseBattleCharacter baseBattleCharacterFromBreed2 = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(j));
						if (baseBattleCharacterFromBreed2 != null)
						{
							T.createDamage(baseBattleCharacterFromBreed2, nowPlayer());
						}
					}
				}
				setFlag(FLAG_TYPE.FLAG_HIT);
				if (nowPlayer().flag(PLAYER_FLAG.PF_CRITICAL) && nowPlayer().actionId() != 24)
				{
					T.createCritical(target);
				}
			}

			public void criticalFlash(TurnSystem T)
			{
				if (!flag(FLAG_TYPE.FLAG_CRITICAL))
				{
					if (nowPlayer().flag(PLAYER_FLAG.PF_CRITICAL) && nowPlayer().actionId() != 24)
					{
						T.createCriticalFlash();
					}
					setFlag(FLAG_TYPE.FLAG_CRITICAL);
				}
			}

			public void isEndNormalAttack2D(TurnSystem T, BaseBattleCharacter target)
			{
				if (T.checkFlag(TurnSystem.End2DProcess))
				{
					return;
				}
				if (!T.checkFlag(TurnSystem.Start2DProcess))
				{
					if (!Battle2DManager.instance().hit(nowPlayer().battleCharacterId()).isExist())
					{
						T.setCheckFlag(TurnSystem.Start2DProcess);
					}
					return;
				}
				bool flag = true;
				for (int i = 0; i < 12; i++)
				{
					if (Battle2DManager.instance().damage().isExist(i))
					{
						flag = false;
					}
				}
				if (flag)
				{
					T.setCheckFlag(TurnSystem.End2DProcess);
				}
			}

			public void checkAttackPlayer(TurnSystem T)
			{
				if (T.checkFlag(TurnSystem.EndPlayerProcess))
				{
					return;
				}
				switch (nowPlayer().playerActionId())
				{
				case 6:
					if (nowPlayer().isPlayerActionEnd())
					{
						T.setCheckFlag(TurnSystem.EndPlayerProcess);
					}
					break;
				case 7:
					if (nowPlayer().isPlayerActionEnd())
					{
						T.setCheckFlag(TurnSystem.EndPlayerProcess);
					}
					break;
				case 9:
					if (!nowPlayer().isPlayerActionEnd())
					{
						break;
					}
					if (nowPlayer().isActionEnd())
					{
						if (nowPlayer().flag(PLAYER_FLAG.PF_COUNTER))
						{
							nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_IDLE_TO_POISE);
							T.setCheckFlag(TurnSystem.EndPlayerProcess);
						}
						else
						{
							nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_POISE_TO_IDLE);
						}
					}
					else
					{
						T.setCheckFlag(TurnSystem.EndPlayerProcess);
					}
					break;
				case 22:
					if (nowPlayer().isPlayerActionEnd())
					{
						if (nowPlayer().isActionEnd())
						{
							nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_POISE_TO_IDLE);
						}
						else
						{
							T.setCheckFlag(TurnSystem.EndPlayerProcess);
						}
					}
					break;
				}
			}

			public void setNextActionNowPlayer(TurnSystem T)
			{
				if (nowPlayer().breed() == 2)
				{
					nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_IDLE_TO_POISE);
				}
				else if (nowPlayer().flag(PLAYER_FLAG.PF_OVERISSUE))
				{
					if (T.characterManager().monsterParty().aliveNumber() == 0 || !nowPlayer().isEndOverissueNumber())
					{
						nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_BACK_ATTACK);
					}
					else
					{
						nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_IDLE_TO_POISE);
					}
				}
				else
				{
					nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_BACK_ATTACK);
				}
			}

			public void checkAttackNpc(TurnSystem T)
			{
				if (!T.checkFlag(TurnSystem.EndPlayerProcess) && nowPlayer().isPlayerActionEnd())
				{
					nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_IDLE_TO_POISE);
					T.setCheckFlag(TurnSystem.EndPlayerProcess);
				}
			}

			public bool isOverissue(TurnSystem T)
			{
				if (T.characterManager().monsterParty().aliveNumber() == 0)
				{
					return false;
				}
				if (nowPlayer().flag(PLAYER_FLAG.PF_OVERISSUE) && nowPlayer().isEndOverissueNumber())
				{
					return true;
				}
				return false;
			}

			public void setOverissue(TurnSystem T)
			{
				clearFlagAll();
				T.clearFlagTerminateTurn();
				T.clearCheckFlagAll();
				nowPlayer().clearFlag(PLAYER_FLAG.PF_CRITICAL);
				nowPlayer().setHiddenWeaponFlag(pl.HAND_TYPE.RIGHT_HAND, flag: false);
				nowPlayer().setHiddenWeaponFlag(pl.HAND_TYPE.LEFT_HAND, flag: false);
				T.setTargetRandam(nowPlayer(), T.characterManager().monsterParty(), reflect: false);
				T.calcNormalAttackDamage(T.nowCharacter());
				T.setOverissueDamage(nowPlayer());
				T.setNormalAttackDamage(T.nowCharacter());
				if (T.characterManager().monsterParty().aliveNumber() != 0)
				{
					nowPlayer().clearFlag(PLAYER_FLAG.PF_FINISH);
				}
				else
				{
					nowPlayer().setFlag(PLAYER_FLAG.PF_FINISH);
				}
				nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_NORMAL_ATTACK);
				T.setState(7);
			}

			public ys.Effects selectPitchEffect(TurnSystem T, int hand, ys.Effects effects)
			{
				if (nowPlayer().breed() == 2)
				{
					return effects;
				}
				int num = nowPlayer().player().equipParameter().equipHand((pl.HAND_TYPE)hand)
					.itemId();
				effects.category_ = 224;
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
				if (baseBattleCharacterFromBreed != null && baseBattleCharacterFromBreed.breed() == 0)
				{
					effects.member_ = 5;
					return effects;
				}
				switch (num)
				{
				case 2101:
					effects.member_ = 2;
					break;
				case 2103:
					effects.member_ = 1;
					break;
				case 2105:
					effects.member_ = 4;
					break;
				case 2107:
					effects.member_ = 3;
					break;
				}
				return effects;
			}

			public void hiddenPitchWeapon(TurnSystem T, pl.HAND_TYPE hand)
			{
				if (nowPlayer().breed() == 2)
				{
					return;
				}
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
				if (baseBattleCharacterFromBreed == null || baseBattleCharacterFromBreed.breed() != 0)
				{
					int num = nowPlayer().player().equipParameter().equipHand(hand)
						.itemId();
					if (itm.ItemManager.instance().weaponParameter((short)num) != null && itm.ItemManager.instance().weaponParameter((short)num).system() == 14)
					{
						nowPlayer().setHiddenWeaponFlag(hand, flag: true);
					}
				}
			}

			public ys.Effects selectAxeSE(int hand, ys.Effects effects)
			{
				if (nowPlayer().breed() == 2)
				{
					return effects;
				}
				int num = nowPlayer().player().equipParameter().equipHand((pl.HAND_TYPE)hand)
					.itemId();
				itm.WeaponParameter weaponParameter = itm.ItemManager.instance().weaponParameter((short)num);
				if (weaponParameter != null && weaponParameter.system() == 12)
				{
					effects.member_ = 20;
				}
				return effects;
			}

			public void initializeEscape(TurnSystem T)
			{
				if (OutsideToBattle.getInstance().escape())
				{
					T.onPlayerEscape();
				}
				else if (T.calcEscape())
				{
					T.onPlayerEscape();
				}
				else
				{
					T.offPlayerEscape();
				}
				T.nowCharacter().setFlag(PLAYER_FLAG.PF_ESCAPE);
				if (T.isPlayerEscape())
				{
					for (int i = 0; i < 4; i++)
					{
						BattlePlayer battlePlayer = T.characterManager().playerParty().battlePlayer(i);
						if (battlePlayer.isEnable() && battlePlayer.condition().isCanEscape())
						{
							battlePlayer.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_ESCAPE);
						}
					}
					Battle2DManager.instance().helpWindow().createHelpWindow(105, 0, 0);
				}
				else
				{
					nowPlayer().setIdleType(0);
					nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION);
					Battle2DManager.instance().helpWindow().createHelpWindow(104, 0, 0);
				}
				Battle2DManager.instance().helpWindow().setMsdHandle(0);
				workCounter_ = 0;
			}

			public void executeEscape(TurnSystem T)
			{
				if (++workCounter_ > TurnSystem.DRAW_HELP_WINDOW_FRAME)
				{
					T.setPhase(TurnSystem.Phase.Terminate);
					if (!T.isPlayerEscape())
					{
						Battle2DManager.instance().helpWindow().releaseHelpWindow();
					}
				}
			}

			public void initializeGuard(TurnSystem T)
			{
				T.nowCharacter().setFlag(PLAYER_FLAG.PF_GUARD);
				T.setState(11);
			}

			public void stateGuard(TurnSystem T)
			{
				switch (T.state())
				{
				case 11:
					startGuard(T);
					break;
				case 12:
					executeGuard(T);
					break;
				}
			}

			public void startGuard(TurnSystem T)
			{
				if (nowPlayer().isPlayerActionEnd())
				{
					nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_GUARD);
					workCounter_ = 0;
					T.setState(12);
				}
			}

			public void executeGuard(TurnSystem T)
			{
				if (++workCounter_ > TurnSystem.GUARD_WAIT_FRAME)
				{
					T.setPhase(TurnSystem.Phase.MonsterExecute);
				}
			}

			public void initializeMagic(TurnSystem T)
			{
				if (nowPlayer().condition().isSilence())
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				if (nowPlayer().condition().isFrog() && nowPlayer().useMagicId() != 4005)
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				summonState_ = SUMMON_STATE.PLAYER_ACTION;
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(nowPlayer().useMagicId());
				if (!nowPlayer().condition().isFrog())
				{
					nowPlayer().addMagicMotion();
				}
				if (nowPlayer().actionId() == 6)
				{
					OutsideToBattle.getInstance().onMagicDefenseInvalidation();
					summonManager_.setSummonType(static_cast<SummonManager.SUMMON_TYPE>(nowPlayer().selectSummonType()));
					summonManager_.setSummonLevel(nowPlayer().magicLevel());
					int num = nowPlayer().useMagicId() + summonManager_.summonType() * 10;
					nowPlayer().setUseMagicId((short)num);
					setTargetGeography(T);
					int se_group = 270 + magicParameter.magicClass();
					BattleSE.instance().load(se_group);
				}
				else
				{
					int num2 = ((magicParameter.system() != 0) ? 260 : 250);
					int se_group2 = num2 + magicParameter.magicClass();
					BattleSE.instance().load(se_group2);
				}
				T.calcMagicDamage(nowPlayer());
				if (OutsideToBattle.getInstance().battleType() == BATTLE_TYPE.NORMAL_BATTLE || OutsideToBattle.getInstance().escape())
				{
					if (nowPlayer().useMagicId() == 4201)
					{
						T.onPlayerEscape();
						nowPlayer().setFlag(PLAYER_FLAG.PF_ESCAPE);
						for (int i = 0; i < 12; i++)
						{
							BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(i));
							if (baseBattleCharacterFromBreed != null && baseBattleCharacterFromBreed.isEnable())
							{
								baseBattleCharacterFromBreed.setFlag(PLAYER_FLAG.PF_2D);
								baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_MISS);
							}
						}
					}
					else
					{
						T.offPlayerEscape();
					}
				}
				else
				{
					T.offPlayerEscape();
				}
				if (nowPlayer().flag(PLAYER_FLAG.PF_ERASE))
				{
					for (int j = 0; j < 4; j++)
					{
						T.characterManager().playerParty().battlePlayer(j)?.isEnable();
					}
					T.onPlayerEscape();
					T.nowCharacter().setFlag(PLAYER_FLAG.PF_ESCAPE);
				}
				pl.PlayerNormalMagicParameter playerNormalMagicParameter = pl.PlayerParty.instance().normalMagic(nowPlayer().useMagicId());
				if (playerNormalMagicParameter != null)
				{
					int category_ = playerNormalMagicParameter.effect().category_;
					BattleEffect.instance().addEfp(category_);
				}
				nowPlayer().deleteItemOrMagicNumber();
				T.setMagicStartEffect(nowPlayer().useMagicId());
				T.addEfpReflect();
				Battle2DManager.instance().helpWindow().setMsdHandle(1);
				T.setState(0);
			}

			public void stateMagic(TurnSystem T)
			{
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					if (T.magicStartEffect(nowPlayer().useMagicId()) == 407)
					{
						BattleSE.instance().play(200, 8);
					}
					else if (T.magicStartEffect(nowPlayer().useMagicId()) == 408)
					{
						BattleSE.instance().play(200, 13);
					}
					else
					{
						BattleSE.instance().play(200, 12);
					}
					startMagicEffect(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW, 1);
					break;
				case 5:
				{
					int num = 0;
					if (nowPlayer().actionId() == 6)
					{
						num = 1300;
						num += summonManager_.summonType() + 1;
						num += summonManager_.summonLevel() * 10;
					}
					else
					{
						num = itm.ItemManager.instance().magicParameter(nowPlayer().useMagicId()).nameId();
					}
					createHelpWindow(T, num, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				}
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_MAGIC, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 0);
					break;
				case 36:
					if (nowPlayer().actionId() == 6)
					{
						T.setState(16);
					}
					else
					{
						T.setState(8);
					}
					break;
				case 8:
					executeMagic(T);
					break;
				case 16:
					executeSummonMagic(T);
					break;
				}
				controlMagicStartEffect(T);
			}

			public void executeMagic(TurnSystem T)
			{
				playerMagicAction(T);
				if (T.checkFlag(TurnSystem.END_MAGIC_EFFECT))
				{
					T.executeCommonMagic();
				}
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && T.checkFlag(TurnSystem.EndEnemyProcess))
				{
					nowPlayer().setConditionMotion(6);
					T.setPhase(TurnSystem.Phase.MonsterExecute);
				}
			}

			public void executeSummonMagic(TurnSystem T)
			{
				switch (summonState_)
				{
				case SUMMON_STATE.PLAYER_ACTION:
					playerMagicAction(T);
					if (T.checkFlag(TurnSystem.EndPlayerProcess))
					{
						summonState_ = SUMMON_STATE.CHARACTER_DISAPPEAR;
					}
					break;
				case SUMMON_STATE.CHARACTER_DISAPPEAR:
					if (T.characterManager().playerParty().disappear(10))
					{
						ds.CHeap.getAllocNum();
						T.characterManager().playerParty().unregisterCharacterMng();
						summonManager_.initialize(T);
						T.clearCheckFlagAll();
						summonState_ = SUMMON_STATE.SUMMON_EXECUTE;
					}
					break;
				case SUMMON_STATE.SUMMON_EXECUTE:
					if (!summonManager_.execute(T))
					{
						break;
					}
					if (nowPlayer().useMagicId() == 4201 && T.isPlayerEscape())
					{
						for (int i = 0; i < 4; i++)
						{
							T.characterManager().playerParty().battlePlayer(i)?.isEnable();
						}
						workCounter_ = 0;
						Battle2DManager.instance().helpWindow().createHelpWindow(105, 0, 0);
						Battle2DManager.instance().helpWindow().setMsdHandle(0);
						MatrixSound.MtxSENDS_Load(270);
						MatrixSound.MtxSENDS_Play(270, 1, 192, 127);
						summonState_ = SUMMON_STATE.SUMMON_ESCAPE;
					}
					else
					{
						summonManager_.terminate(T);
						ds.CHeap.getAllocNum();
						T.setPhase(TurnSystem.Phase.MonsterExecute);
					}
					break;
				case SUMMON_STATE.SUMMON_ESCAPE:
					if (++workCounter_ > TurnSystem.DRAW_HELP_WINDOW_FRAME + 10)
					{
						workCounter_ = 0;
						Battle2DManager.instance().helpWindow().releaseHelpWindow();
						MatrixSound.MtxSENDS_Unload();
						summonManager_.terminate(T);
						ds.CHeap.getAllocNum();
						T.setPhase(TurnSystem.Phase.MonsterExecute);
					}
					break;
				}
			}

			public void playerMagicAction(TurnSystem T)
			{
				if (nowPlayer().playerActionId() == 14)
				{
					if (nowPlayer().isPlayerActionEnd())
					{
						nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_MAGIC_LOOP);
					}
				}
				else if (nowPlayer().playerActionId() == 15)
				{
					if (nowPlayer().isPlayerActionEnd())
					{
						nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_IDLE);
						battleDisplay.stateBattleCamera();
						T.playerWindow().show(flag: true);
						nowPlayer().setShakeScreen();
						stageMng.setHidden(flag: false);
						T.setCheckFlag(TurnSystem.PlayEffect);
					}
				}
				else if (nowPlayer().playerActionId() == 36)
				{
					T.setCheckFlag(TurnSystem.EndPlayerProcess);
				}
			}

			public void startMagicEffect(TurnSystem T, PLAYER_ACTION_STATE nextState, int member)
			{
				int magicId = nowPlayer().useMagicId();
				int num = BattleEffect.instance().create(T.magicStartEffect(magicId), member);
				int num2 = nowPlayer().unUsedEffectId();
				if (num2 != -1)
				{
					nowPlayer().effectId_set(num2, num);
					VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
					characterMng.getPosition(nowPlayer().characterMngId(), fnd_reuse_pos);
					if (T.magicStartEffect(magicId) == 407 && member == 2)
					{
						BattleEffect.instance().setPosition(num, fnd_reuse_pos);
					}
					else
					{
						T.setHitEffectPosition(nowPlayer(), num, 0, 0);
					}
					T.setState((int)nextState);
				}
			}

			public void controlMagicStartEffect(TurnSystem T)
			{
				if (T.checkFlag(TurnSystem.END_MAGIC_EFFECT))
				{
					return;
				}
				if (!T.checkFlag(TurnSystem.START_MAGIC_EFFECT))
				{
					bool flag = false;
					if (nowPlayer().condition().isFrog())
					{
						if (T.checkFlag(TurnSystem.EndPlayerProcess))
						{
							flag = true;
						}
					}
					else if (nowPlayer().checkMotionAndFrame(4002, 1))
					{
						flag = true;
					}
					if (flag)
					{
						BattleEffect.instance().deleteEffect(nowPlayer().effectId(0));
						nowPlayer().setEffectId(0, -1);
						startMagicEffect(T, PLAYER_ACTION_STATE.NON_STATE, 2);
						T.setCheckFlag(TurnSystem.START_MAGIC_EFFECT);
					}
				}
				if (T.checkFlag(TurnSystem.START_MAGIC_EFFECT) && nowPlayer().isClearAllEffect())
				{
					T.setCheckFlag(TurnSystem.END_MAGIC_EFFECT);
				}
			}

			public void initializeItem(TurnSystem T)
			{
				T.calcItemDamage(nowPlayer());
				short num = (short)nowPlayer().useItemId();
				itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter(num);
				if (itemBaseParameter.useItemId() > 0)
				{
					num = itemBaseParameter.useItemId();
				}
				pl.PlayerNormalMagicParameter playerNormalMagicParameter = pl.PlayerParty.instance().normalMagic(num);
				int category_ = playerNormalMagicParameter.effect().category_;
				BattleEffect.instance().addEfp(category_);
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(num);
				if (magicParameter != null)
				{
					int num2 = ((magicParameter.system() != 0) ? 260 : 250);
					int se_group = num2 + magicParameter.magicClass();
					BattleSE.instance().load(se_group);
				}
				else
				{
					BattleSE.instance().load(210);
				}
				nowPlayer().setUseMagicId(num);
				T.addEfpReflect();
				nowPlayer().deleteItemOrMagicNumber();
				Battle2DManager.instance().helpWindow().createHelpWindow(itm.ItemManager.instance().itemParameter((short)nowPlayer().useItemId()).nameId(), 0, 0);
				Battle2DManager.instance().helpWindow().setMsdHandle(1);
				T.setState(0);
			}

			public void stateItem(TurnSystem T)
			{
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_ITEM, PLAYER_ACTION_STATE.PAS_ITEM, 1);
					break;
				case 9:
					executeItem(T);
					break;
				}
			}

			public void executeItem(TurnSystem T)
			{
				playerItemAction(T);
				T.executeCommonMagic();
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && T.checkFlag(TurnSystem.EndEnemyProcess))
				{
					nowPlayer().setConditionMotion(6);
					T.setPhase(TurnSystem.Phase.MonsterExecute);
				}
			}

			public void playerItemAction(TurnSystem T)
			{
				if (nowPlayer().playerActionId() == 19)
				{
					if (nowPlayer().isPlayerActionEnd())
					{
						nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_ITEM_LOOP);
						nowPlayer().setShakeScreen();
						T.setCheckFlag(TurnSystem.PlayEffect);
					}
				}
				else if (nowPlayer().playerActionId() == 20)
				{
					if (nowPlayer().isPlayerActionEnd())
					{
						nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_IDLE);
					}
				}
				else if (nowPlayer().playerActionId() == 36)
				{
					T.setCheckFlag(TurnSystem.EndPlayerProcess);
				}
			}

			public void initializeTakeAPowder(TurnSystem T)
			{
				if (nowPlayer().condition().isFrog())
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				if (OutsideToBattle.getInstance().escape())
				{
					T.onPlayerEscape();
				}
				else if (T.calcEscape())
				{
					T.onPlayerEscape();
				}
				else
				{
					T.offPlayerEscape();
				}
				nowPlayer().setFlag(PLAYER_FLAG.PF_ESCAPE);
				nowPlayer().setFlag(PLAYER_FLAG.PF_TAKE_A_POWDER);
				BattleEffect.instance().addEfp(237);
				BattleEffect.instance().addEfp(231);
				BattleSE.instance().load(203);
				T.setState(0);
			}

			public void stateTakeAPowder(TurnSystem T)
			{
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					startAbilityEffect(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					break;
				case 5:
					createAbilityName(T, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 1);
					break;
				case 36:
					endAbilityCamera(T, PLAYER_ACTION_STATE.PAS_FRONT_READY);
					break;
				case 2:
					moveFrontReady(T);
					break;
				case 3:
					moveFront(T, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, PLAYER_ACTION_STATE.PAS_TAKE_A_POWDER);
					break;
				case 13:
					executeTakeAPowder(T, T.characterManager().playerParty());
					break;
				case 4:
					moveBack(T, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, 0);
					break;
				case 14:
					successTakeAPowder(T);
					break;
				}
			}

			public void executeTakeAPowder(TurnSystem T, BattleParty party)
			{
				if (T.isPlayerEscape())
				{
					if (nowPlayer().playerActionId() != 17)
					{
						for (int i = 0; i < 4; i++)
						{
							if (party.battlePlayer(i).isBattle())
							{
								party.battlePlayer(i).setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_TAKE_A_POWDER);
								ys.Effects effects = new ys.Effects();
								effects.frameCounter_ = 0;
								effects.category_ = 237;
								effects.member_ = 1;
								T.createEffect(0, effects, nowPlayer(), 1, 0, 1);
								T.setCheckFlag(TurnSystem.PlayEffect);
							}
						}
						BattleSE.instance().play(203, 2);
						Battle2DManager.instance().helpWindow().createHelpWindow(105, 0, 0);
						workCounter_ = 0;
						T.setState(14);
					}
				}
				else if (workCounter_ == TurnSystem.DRAW_HELP_WINDOW_FRAME)
				{
					Battle2DManager.instance().helpWindow().createHelpWindow(104, 0, 0);
				}
				else if (workCounter_ == TurnSystem.DRAW_HELP_WINDOW_FRAME + TurnSystem.DRAW_HELP_WINDOW_FRAME)
				{
					nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_BACK_ATTACK);
					Battle2DManager.instance().helpWindow().releaseHelpWindow();
					T.setState(4);
				}
				workCounter_++;
			}

			public void successTakeAPowder(TurnSystem T)
			{
				if (++workCounter_ > TurnSystem.DRAW_HELP_WINDOW_FRAME)
				{
					T.setPhase(TurnSystem.Phase.Terminate);
				}
			}

			public void initializeSteal(TurnSystem T)
			{
				if (nowPlayer().condition().isFrog())
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
				BattleMonster battleMonster = static_cast<BattleMonster>(baseBattleCharacterFromBreed);
				clearFlag(FLAG_TYPE.FLAG_STEAL_SUCCESS);
				setFlag(FLAG_TYPE.FLAG_STEAL);
				nowPlayer().setStealItemId(-1);
				if (battleMonster.stolenItemId() < 0)
				{
					StealFormula stealFormula = new StealFormula();
					int num = stealFormula.calcSteal(nowPlayer(), battleMonster);
					if (itm.ItemManager.instance().itemParameter((short)num) != null)
					{
						nowPlayer().setStealItemId((short)num);
						battleMonster.setStolenItemId((short)num);
						pl.PlayerParty.instance().addItem(num, 1);
						setFlag(FLAG_TYPE.FLAG_STEAL_SUCCESS);
					}
					else
					{
						nowPlayer().setStealItemId((short)num);
						battleMonster.setStolenItemId((short)num);
					}
				}
				BattleEffect.instance().addEfp(248);
				BattleEffect.instance().addEfp(231);
				BattleSE.instance().load(203);
				nowPlayer().addJobMotion();
				T.setState(0);
			}

			public void stateSteal(TurnSystem T)
			{
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					startAbilityEffect(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					break;
				case 5:
					createAbilityName(T, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 1);
					break;
				case 36:
					endAbilityCamera(T, PLAYER_ACTION_STATE.PAS_FRONT_READY);
					break;
				case 2:
					moveFrontReady(T);
					break;
				case 3:
					moveFront(T, BATTLE_ACTION_TYPE.DBA_STEAL, PLAYER_ACTION_STATE.PAS_STEAL);
					break;
				case 15:
					executeSteal(T, baseBattleCharacterFromBreed);
					break;
				case 4:
					moveBack(T, BATTLE_ACTION_TYPE.DBA_POISE_TO_IDLE, 0);
					break;
				}
			}

			public void executeSteal(TurnSystem T, BaseBattleCharacter target)
			{
				deleteStealMessage(T);
				createStealMessage(T);
				createStealEffect(T);
				playerStealAction(T);
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && T.checkFlag(TurnSystem.End2DProcess))
				{
					T.setPhase(TurnSystem.Phase.MonsterExecute);
				}
			}

			public void playerStealAction(TurnSystem T)
			{
				if (T.checkFlag(TurnSystem.EndPlayerProcess))
				{
					return;
				}
				if (nowPlayer().playerActionId() == 16)
				{
					if (nowPlayer().isPlayerActionEnd())
					{
						nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_BACK);
					}
				}
				else if (nowPlayer().playerActionId() == 1 && nowPlayer().isPlayerActionEnd())
				{
					nowPlayer().setIdleType(0);
					nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION);
					T.setCheckFlag(TurnSystem.EndPlayerProcess);
				}
			}

			public void createStealMessage(TurnSystem T)
			{
				if (!T.checkFlag(TurnSystem.EndPlayerProcess) || T.checkFlag(TurnSystem.Start2DProcess))
				{
					return;
				}
				if (nowPlayer().stealItemId() > 0)
				{
					itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter(nowPlayer().stealItemId());
					if (itemBaseParameter != null)
					{
						dgs.CCtrlCodeInterface.instance().setItemId(itemBaseParameter.nameId());
						Battle2DManager.instance().helpWindow().createHelpWindow(111, 0, 0);
					}
				}
				else
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
					BattleMonster battleMonster = static_cast<BattleMonster>(baseBattleCharacterFromBreed);
					if (battleMonster.stolenItemId() > 0)
					{
						Battle2DManager.instance().helpWindow().createHelpWindow(143, 0, 0);
					}
					else if (battleMonster.stolenItemId() == -2)
					{
						Battle2DManager.instance().helpWindow().createHelpWindow(143, 0, 0);
					}
					else
					{
						Battle2DManager.instance().helpWindow().createHelpWindow(115, 0, 0);
					}
				}
				workCounter_ = 0;
				T.setCheckFlag(TurnSystem.Start2DProcess);
			}

			public void deleteStealMessage(TurnSystem T)
			{
				if (T.checkFlag(TurnSystem.Start2DProcess) && !T.checkFlag(TurnSystem.End2DProcess) && ++workCounter_ >= TurnSystem.DRAW_HELP_WINDOW_FRAME)
				{
					Battle2DManager.instance().helpWindow().releaseHelpWindow();
					workCounter_ = 0;
					T.setCheckFlag(TurnSystem.End2DProcess);
				}
			}

			public void createStealEffect(TurnSystem T)
			{
				if (!T.checkFlag(TurnSystem.StartEffectProcess))
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
					OS_Printf("エフェクト再生開始\n");
					ys.Effects effects = new ys.Effects();
					effects.frameCounter_ = 0;
					effects.category_ = 248;
					effects.member_ = 1;
					T.createEffect(0, effects, baseBattleCharacterFromBreed, 0, 0, 1);
					T.setCheckFlag(TurnSystem.StartEffectProcess);
				}
			}

			public void stateKnockOver(TurnSystem T)
			{
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					startAbilityEffect(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					break;
				case 5:
					createAbilityName(T, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 1);
					break;
				case 36:
					endAbilityCamera(T, PLAYER_ACTION_STATE.PAS_FRONT_READY);
					break;
				case 2:
					moveFrontReady(T);
					break;
				case 3:
					moveFront(T, BATTLE_ACTION_TYPE.DBA_IDLE_TO_POISE, PLAYER_ACTION_STATE.PAS_IDLE);
					break;
				case 1:
					idle(T, TurnSystem.IDLE_FRAME, PLAYER_ACTION_STATE.PAS_MORE_MOVE_READY);
					break;
				case 17:
					moreMoveFrontReady(T);
					BattleSE.instance().play(200, 16);
					break;
				case 18:
					moveFront(T, BATTLE_ACTION_TYPE.DBA_NORMAL_ATTACK, PLAYER_ACTION_STATE.PAS_NORMAL_ATTACK);
					break;
				case 7:
					executeNormalAttack(T, baseBattleCharacterFromBreed);
					break;
				case 10:
					moveBackAttack(T, baseBattleCharacterFromBreed);
					break;
				}
			}

			public void moreMoveFrontReady(TurnSystem T)
			{
				if (nowPlayer().isPlayerActionEnd())
				{
					nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_KNOCK_OVER_FRONT);
					T.setState(18);
					ys.Effects effects = new ys.Effects();
					effects.frameCounter_ = 0;
					effects.category_ = 431;
					effects.member_ = 1;
					T.createEffect(0, effects, nowPlayer(), 1, 0, 1);
				}
			}

			public void initializePoise(TurnSystem T)
			{
				if (nowPlayer().condition().isFrog())
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				T.nowCharacter().setFlag(PLAYER_FLAG.PF_COUNTER);
				BattleEffect.instance().addEfp(231);
				BattleEffect.instance().addEfp(433);
				BattleSE.instance().load(203);
				T.setState(0);
			}

			public void statePoise(TurnSystem T)
			{
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					startAbilityEffect(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					break;
				case 5:
					createAbilityName(T, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 1);
					break;
				case 36:
					endAbilityCamera(T, PLAYER_ACTION_STATE.PAS_FRONT_READY);
					break;
				case 2:
					moveFrontReady(T);
					break;
				case 3:
					moveFront(T, BATTLE_ACTION_TYPE.DBA_POISE, PLAYER_ACTION_STATE.PAS_POISE);
					break;
				case 20:
					executePoise(T, nowPlayer());
					break;
				case 4:
					moveBack(T, BATTLE_ACTION_TYPE.DBA_IDLE_TO_POISE, 1);
					break;
				}
			}

			public void executePoise(TurnSystem T, BattlePlayer player)
			{
				if (!T.checkFlag(TurnSystem.PlayEffect))
				{
					ys.Effects effects = new ys.Effects();
					effects.frameCounter_ = 0;
					effects.category_ = 433;
					effects.member_ = 1;
					T.createEffect(0, effects, player, 0, 0, 1);
					T.setCheckFlag(TurnSystem.PlayEffect);
					BattleSE.instance().play(203, 10);
				}
				if (player.isPlayerActionEnd())
				{
					player.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_BACK);
					T.setState(4);
				}
			}

			public void initializeCoverStart(TurnSystem T)
			{
				if (nowPlayer().condition().isFrog())
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				BattleEffect.instance().addEfp(231);
				BattleEffect.instance().addEfp(432);
				BattleSE.instance().load(203);
				T.setState(0);
			}

			public void stateCoverStart(TurnSystem T)
			{
				BattlePlayer player = static_cast<BattlePlayer>(T.nowCharacter());
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					startAbilityEffect(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					break;
				case 5:
					createAbilityName(T, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 1);
					break;
				case 36:
					endAbilityCamera(T, PLAYER_ACTION_STATE.PAS_FRONT_READY);
					break;
				case 2:
					moveFrontReady(T);
					break;
				case 3:
					moveFront(T, BATTLE_ACTION_TYPE.DBA_COVER_START, PLAYER_ACTION_STATE.PAS_COVER_START);
					break;
				case 21:
					executeCoverStart(T, player);
					break;
				case 4:
					moveBack(T, BATTLE_ACTION_TYPE.DBA_IDLE_TO_POISE, 1);
					break;
				}
			}

			public void executeCoverStart(TurnSystem T, BattlePlayer player)
			{
				if (!T.checkFlag(TurnSystem.PlayEffect))
				{
					ys.Effects effects = new ys.Effects();
					effects.frameCounter_ = 0;
					effects.category_ = 432;
					effects.member_ = 1;
					T.createEffect(0, effects, player, 0, 0, 1);
					T.setCheckFlag(TurnSystem.PlayEffect);
					T.nowCharacter().setFlag(PLAYER_FLAG.PF_MORE_GUARD);
					BattleSE.instance().play(203, 3);
				}
				if (player.isPlayerActionEnd())
				{
					player.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_BACK);
					T.setState(4);
				}
			}

			public void initializeCheck(TurnSystem T)
			{
				if (nowPlayer().condition().isFrog())
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
				if (!nowPlayer().condition().isFrog())
				{
					nowPlayer().addJobMotion();
				}
				T.setCheckActionType(nowPlayer().actionId());
				T.clearCheckState();
				T.setCheckFlag(TurnSystem.CHECK_SUCCESS);
				T.checkWeak(baseBattleCharacterFromBreed);
				baseBattleCharacterFromBreed.clearMagicFlagAll();
				baseBattleCharacterFromBreed.resetParameterMagicFlag();
				BattleEffect.instance().addEfp(238);
				BattleEffect.instance().addEfp(231);
				BattleSE.instance().load(203);
				T.setState(0);
			}

			public void stateCheck(TurnSystem T)
			{
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					startAbilityEffect(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					break;
				case 5:
					createAbilityName(T, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 1);
					break;
				case 36:
					endAbilityCamera(T, PLAYER_ACTION_STATE.PAS_FRONT_READY);
					break;
				case 2:
					moveFrontReady(T);
					BattleSE.instance().play(203, 5);
					break;
				case 3:
					moveFront(T, BATTLE_ACTION_TYPE.DBA_CHECK, PLAYER_ACTION_STATE.PAS_CHECK);
					break;
				case 22:
					executeCheck(T, nowPlayer(), baseBattleCharacterFromBreed);
					break;
				case 4:
					moveBack(T, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, 0);
					break;
				}
			}

			public void executeCheck(TurnSystem T, BattlePlayer player, BaseBattleCharacter target)
			{
				T.checkFlag(TurnSystem.PlayEffect);
				if (T.checkState() == 8)
				{
					target.changePlayerColor();
					player.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_BACK);
					T.setState(4);
				}
				T.outputWeak(target);
			}

			public void initializeDetect(TurnSystem T)
			{
				if (nowPlayer().condition().isFrog())
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				BattlePlayer battlePlayer = static_cast<BattlePlayer>(T.nowCharacter());
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(battlePlayer.targetId(0));
				if (!battlePlayer.condition().isFrog())
				{
					battlePlayer.addJobMotion();
				}
				T.setCheckActionType(nowPlayer().actionId());
				T.checkWeak(baseBattleCharacterFromBreed);
				T.clearCheckState();
				baseBattleCharacterFromBreed.clearMagicFlagAll();
				baseBattleCharacterFromBreed.resetParameterMagicFlag();
				BattleEffect.instance().addEfp(238);
				BattleEffect.instance().addEfp(231);
				BattleSE.instance().load(203);
				T.setState(0);
			}

			public void stateDetect(TurnSystem T)
			{
				BattlePlayer battlePlayer = static_cast<BattlePlayer>(T.nowCharacter());
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(battlePlayer.targetId(0));
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					startAbilityEffect(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					break;
				case 5:
					createAbilityName(T, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 1);
					break;
				case 36:
					endAbilityCamera(T, PLAYER_ACTION_STATE.PAS_FRONT_READY);
					break;
				case 2:
					moveFrontReady(T);
					break;
				case 3:
					moveFront(T, BATTLE_ACTION_TYPE.DBA_CHECK, PLAYER_ACTION_STATE.PAS_CHECK);
					break;
				case 22:
					executeCheck(T, battlePlayer, baseBattleCharacterFromBreed);
					break;
				case 4:
					moveBack(T, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, 0);
					break;
				}
			}

			public void initializeJumpStart(TurnSystem T)
			{
				if (nowPlayer().condition().isFrog())
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				BattleEffect.instance().addEfp(231);
				BattleSE.instance().load(203);
				nowPlayer().addJobMotion();
				T.setState(0);
			}

			public void stateJumpStart(TurnSystem T)
			{
				BattlePlayer player = static_cast<BattlePlayer>(T.nowCharacter());
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					startAbilityEffect(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					break;
				case 5:
					createAbilityName(T, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 1);
					break;
				case 36:
					endAbilityCamera(T, PLAYER_ACTION_STATE.PAS_FRONT_READY);
					break;
				case 2:
					moveFrontReady(T);
					break;
				case 3:
					moveFront(T, BATTLE_ACTION_TYPE.DBA_JUMP_START, PLAYER_ACTION_STATE.PAS_JUMP_START);
					break;
				case 24:
					executeJumpStart(T, player);
					break;
				}
			}

			public void executeJumpStart(TurnSystem T, BattlePlayer player)
			{
				if (player.isPlayerActionEnd())
				{
					player.setActionId(17);
					player.setFlag(PLAYER_FLAG.PF_JUMP);
					T.setPhase(TurnSystem.Phase.MonsterExecute);
				}
			}

			public void initializeJumpEnd(TurnSystem T)
			{
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
				if (baseBattleCharacterFromBreed == null || !baseBattleCharacterFromBreed.isBattle())
				{
					T.setTargetRandam(nowPlayer(), T.characterManager().monsterParty(), reflect: false);
					baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
				}
				VecFx32 vecFx = new VecFx32();
				VecFx32 vecFx2 = new VecFx32();
				characterMng.getPosition(nowPlayer().characterMngId(), vecFx);
				characterMng.getPosition(baseBattleCharacterFromBreed.characterMngId(), vecFx2);
				vecFx.x = vecFx2.x;
				vecFx.z = vecFx2.z;
				vecFx.y = 40960 * JUMP_END_FRAME;
				characterMng.setPosition(nowPlayer().characterMngId(), vecFx);
				VecFx32 vecFx3 = new VecFx32(nowPlayer().rootPosition());
				vecFx3.y = vecFx2.y;
				int distance = VEC_Distance(vecFx, vecFx3);
				nowPlayer().setDistance(distance);
				int num = FX_Atan2Idx(vecFx3.x - vecFx.x, -(vecFx3.z - vecFx.z));
				nowPlayer().setMoveYaw(static_cast<int>(num));
				BattleEffect.instance().addEfp(240);
				BattleSE.instance().load(203);
				T.calcNormalAttackDamage(nowPlayer());
				T.setNormalAttackDamage(nowPlayer());
				nowPlayer().addJobMotion();
				T.setState(0);
			}

			public void stateJumpEnd(TurnSystem T)
			{
				BattlePlayer battlePlayer = static_cast<BattlePlayer>(T.nowCharacter());
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(battlePlayer.targetId(0));
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, 1, BATTLE_ACTION_TYPE.DBA_JUMP_END, PLAYER_ACTION_STATE.PAS_JUMP_END, 1);
					break;
				case 25:
					executeJumpEnd(T, battlePlayer, baseBattleCharacterFromBreed);
					break;
				}
			}

			public void executeJumpEnd(TurnSystem T, BattlePlayer player, BaseBattleCharacter target)
			{
				isJumpEffectEnd(T, target);
				createJumpEffect(T, player, target);
				playJumpSE(T, player);
				isJump2DEnd(T, player, target);
				createJump2D(T, player, target);
				playJumpScreenEffect(T, player);
				targetJumpDamageAction(T, player, target);
				isTargetJumpDamageActionEnd(T, player, target);
				isPlayerJumpEnd(T, player);
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && T.checkFlag(TurnSystem.EndEffectProcess) && T.checkFlag(TurnSystem.End2DProcess) && T.checkFlag(TurnSystem.EndEnemyProcess))
				{
					T.setPhase(TurnSystem.Phase.MonsterExecute);
				}
			}

			public void createJumpEffect(TurnSystem T, BattlePlayer player, BaseBattleCharacter target)
			{
				if (!T.checkFlag(TurnSystem.StartEffectProcess) && player.playerActionId() == 28)
				{
					if (player.flag(PLAYER_FLAG.PF_MISS))
					{
						OS_Printf("エフェクト再生開始\n");
						T.setCheckFlag(TurnSystem.StartEffectProcess);
					}
					else if (characterMng.getCurrentFrame(player.characterMngId()) == JUMP_END_FRAME)
					{
						OS_Printf("エフェクト再生開始\n");
						ys.Effects effects = new ys.Effects();
						effects.frameCounter_ = 0;
						effects.category_ = 240;
						effects.member_ = 1;
						T.createEffect(0, effects, target, 1, 0, 1);
						T.setCheckFlag(TurnSystem.StartEffectProcess);
					}
				}
			}

			public bool isJumpEffectEnd(TurnSystem T, BaseBattleCharacter target)
			{
				if (!T.checkFlag(TurnSystem.StartEffectProcess))
				{
					return false;
				}
				if (T.checkFlag(TurnSystem.EndEffectProcess))
				{
					return false;
				}
				if (target.isClearAllEffect())
				{
					OS_Printf("エフェクト終了\n");
					T.setCheckFlag(TurnSystem.EndEffectProcess);
					return true;
				}
				return false;
			}

			public void playJumpSE(TurnSystem T, BattlePlayer player)
			{
				if (player.playerActionId() == 28 && characterMng.getCurrentFrame(player.characterMngId()) == JUMP_END_FRAME)
				{
					ys.Effects effects = new ys.Effects();
					effects.frameCounter_ = 0;
					if (player.flag(PLAYER_FLAG.PF_MISS))
					{
						BattleSE.instance().playMissSE();
						return;
					}
					effects.category_ = 203;
					effects.member_ = 7;
					T.playSE(0, effects, 0, 0, 0);
				}
			}

			public void createJump2D(TurnSystem T, BattlePlayer player, BaseBattleCharacter target)
			{
				if (!T.checkFlag(TurnSystem.Start2DProcess) && player.playerActionId() == 28 && characterMng.getCurrentFrame(player.characterMngId()) == JUMP_END_FRAME)
				{
					OS_Printf("2D再生開始\n");
					createHit2D(T, target);
					T.setCheckFlag(TurnSystem.Start2DProcess);
				}
			}

			public bool isJump2DEnd(TurnSystem T, BattlePlayer player, BaseBattleCharacter target)
			{
				if (!T.checkFlag(TurnSystem.Start2DProcess))
				{
					return false;
				}
				if (T.checkFlag(TurnSystem.End2DProcess))
				{
					return false;
				}
				if (Battle2DManager.instance().hit(nowPlayer().battleCharacterId()).isExist())
				{
					return false;
				}
				for (int i = 0; i < 12; i++)
				{
					if (Battle2DManager.instance().damage().isExist(i))
					{
						return false;
					}
				}
				OS_Printf("2D再生終了\n");
				T.setCheckFlag(TurnSystem.End2DProcess);
				return true;
			}

			public void playJumpScreenEffect(TurnSystem T, BattlePlayer player)
			{
				if (!player.flag(PLAYER_FLAG.PF_MISS) && player.playerActionId() == 28 && characterMng.getCurrentFrame(player.characterMngId()) == JUMP_END_FRAME && player.flag(PLAYER_FLAG.PF_CRITICAL))
				{
					T.createCriticalFlash();
				}
			}

			public void targetJumpDamageAction(TurnSystem T, BattlePlayer player, BaseBattleCharacter target)
			{
				if (!player.flag(PLAYER_FLAG.PF_MISS) && player.playerActionId() == 28 && characterMng.getCurrentFrame(player.characterMngId()) == JUMP_END_FRAME)
				{
					T.playFlash(0, 0, target);
					T.startDamageAction(0, 0, target);
				}
			}

			public bool isTargetJumpDamageActionEnd(TurnSystem T, BattlePlayer player, BaseBattleCharacter target)
			{
				return T.deadCharacters(player);
			}

			public bool isPlayerJumpEnd(TurnSystem T, BattlePlayer player)
			{
				if (player.isPlayerActionEnd())
				{
					OS_Printf("プレイヤー処理終了\n");
					player.clearFlag(PLAYER_FLAG.PF_JUMP);
					player.changeConditionEffect();
					T.setCheckFlag(TurnSystem.EndPlayerProcess);
					return true;
				}
				return false;
			}

			public void initializeDark(TurnSystem T)
			{
				if (nowPlayer().condition().isFrog())
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				nowPlayer().setFlag(PLAYER_FLAG.PF_DARK);
				BattleEffect.instance().addEfp(242);
				BattleEffect.instance().addEfp(231);
				T.calcNormalAttackDamage(nowPlayer());
				T.setNormalAttackDamage(nowPlayer());
				nowPlayer().setTargetIdMyself();
				T.calc_.calcDark(nowPlayer());
				BattleSE.instance().load(203);
				nowPlayer().addJobMotion();
				T.setState(0);
			}

			public void stateDark(TurnSystem T)
			{
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					startAbilityEffect(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					break;
				case 5:
					createAbilityName(T, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_NON_ACTION, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 1);
					break;
				case 36:
					endAbilityCamera(T, PLAYER_ACTION_STATE.PAS_FRONT_READY);
					break;
				case 2:
					moveFrontReady(T);
					break;
				case 3:
					moveFront(T, BATTLE_ACTION_TYPE.DBA_DARK, PLAYER_ACTION_STATE.PAS_DARK);
					break;
				case 26:
					executeDark(T, nowPlayer());
					break;
				}
			}

			public void executeDark(TurnSystem T, BattlePlayer player)
			{
				isDarkEffectEnd(T, player);
				createDarkEffect(T, player);
				playDarkSE(T, player);
				isDark2DEnd(T, player);
				createDark2D(T, player);
				isTargetDarkDamageActionEnd(T, player);
				playerActionDark(T, player);
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && T.checkFlag(TurnSystem.EndEffectProcess) && T.checkFlag(TurnSystem.End2DProcess) && T.checkFlag(TurnSystem.EndEnemyProcess))
				{
					player.clearFlag(PLAYER_FLAG.PF_DARK);
					T.setPhase(TurnSystem.Phase.MonsterExecute);
				}
			}

			public void createDarkEffect(TurnSystem T, BattlePlayer player)
			{
				if (!T.checkFlag(TurnSystem.StartEffectProcess) && player.checkMotionAndFrame(6601, DARK_HIT_FRAME))
				{
					if (player.flag(PLAYER_FLAG.PF_MISS))
					{
						OS_Printf("エフェクト再生開始\n");
						T.setCheckFlag(TurnSystem.StartEffectProcess);
						return;
					}
					OS_Printf("エフェクト再生開始\n");
					T.createEffectAndSetPosition(player, 242, 1, AllEnemyMagicPosition);
					BattleSE.instance().play(203, 8);
					T.setCheckFlag(TurnSystem.StartEffectProcess);
				}
			}

			public bool isDarkEffectEnd(TurnSystem T, BattlePlayer player)
			{
				if (!T.checkFlag(TurnSystem.StartEffectProcess))
				{
					return false;
				}
				if (T.checkFlag(TurnSystem.EndEffectProcess))
				{
					return false;
				}
				if (player.isClearAllEffect())
				{
					OS_Printf("エフェクト終了\n");
					T.setCheckFlag(TurnSystem.EndEffectProcess);
					return true;
				}
				return false;
			}

			public void playDarkSE(TurnSystem T, BattlePlayer player)
			{
				if (player.checkMotionAndFrame(6601, DARK_HIT_FRAME))
				{
					ys.Effects effects = new ys.Effects();
					effects.frameCounter_ = 0;
					if (player.flag(PLAYER_FLAG.PF_MISS))
					{
						BattleSE.instance().playMissSE();
						return;
					}
					effects.category_ = 203;
					effects.member_ = 8;
				}
			}

			public void createDarkHit2D(TurnSystem T, BattlePlayer player, BaseBattleCharacter target)
			{
				T.createHit(player, target);
				T.createDamage(target, null);
			}

			public void createDark2D(TurnSystem T, BattlePlayer player)
			{
				if (!T.checkFlag(TurnSystem.EndEffectProcess) || T.checkFlag(TurnSystem.Start2DProcess))
				{
					return;
				}
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(player.targetId(i));
					if (baseBattleCharacterFromBreed != null)
					{
						OS_Printf("2D再生開始\n");
						createDarkHit2D(T, player, baseBattleCharacterFromBreed);
					}
				}
				T.setCheckFlag(TurnSystem.Start2DProcess);
			}

			public bool isDark2DEnd(TurnSystem T, BattlePlayer player)
			{
				if (!T.checkFlag(TurnSystem.Start2DProcess))
				{
					return false;
				}
				if (T.checkFlag(TurnSystem.End2DProcess))
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
				OS_Printf("2D再生終了\n");
				T.setCheckFlag(TurnSystem.End2DProcess);
				return true;
			}

			public void playerActionDark(TurnSystem T, BattlePlayer player)
			{
				if (player.playerActionId() == 29)
				{
					if (player.isPlayerActionEnd())
					{
						player.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_BACK);
					}
				}
				else if (player.playerActionId() == 1 && player.isPlayerActionEnd())
				{
					player.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION);
					T.setCheckFlag(TurnSystem.EndPlayerProcess);
				}
			}

			public bool isTargetDarkDamageActionEnd(TurnSystem T, BattlePlayer player)
			{
				return T.deadCharacters(player);
			}

			public void initializeRollUp(TurnSystem T)
			{
				if (nowPlayer().condition().isFrog())
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				nowPlayer().setFlag(PLAYER_FLAG.PF_ROLL_UP);
				BattleEffect.instance().addEfp(233);
				BattleEffect.instance().addEfp(231);
				BattleSE.instance().load(203);
				nowPlayer().clearTargetId();
				if (nowPlayer().isLimitBreakRollUpLevelUp())
				{
					nowPlayer().setFlag(PLAYER_FLAG.PF_EXPLOSION);
					nowPlayer().setTargetIdMyself();
					T.calc_.calcRollUp(nowPlayer());
				}
				nowPlayer().addJobMotion();
				T.setState(0);
			}

			public void stateRollUp(TurnSystem T)
			{
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					startAbilityEffect(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					break;
				case 5:
					createAbilityName(T, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_NON_ACTION, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 1);
					break;
				case 36:
					endAbilityCamera(T, PLAYER_ACTION_STATE.PAS_FRONT_READY);
					break;
				case 2:
					moveFrontReady(T);
					break;
				case 3:
					moveFront(T, BATTLE_ACTION_TYPE.DBA_ROLL_UP, PLAYER_ACTION_STATE.PAS_ROLL_UP);
					break;
				case 27:
					executeRollUp(T);
					break;
				case 4:
					moveBack(T, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, 0);
					break;
				case 28:
					executeExplosion(T);
					break;
				}
			}

			public void executeRollUp(TurnSystem T)
			{
				isRollUpEffectEnd(T);
				createRollUpEffect(T, 1);
				playRollUpSE(T);
				playerActionRollUp(T);
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && T.checkFlag(TurnSystem.EndEffectProcess))
				{
					if (!nowPlayer().flag(PLAYER_FLAG.PF_EXPLOSION))
					{
						nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_BACK);
						T.setState(4);
						return;
					}
					Battle2DManager.instance().helpWindow().createHelpWindow(139, 0, 0);
					Battle2DManager.instance().helpWindow().setMsdHandle(0);
					T.clearCheckFlagAll();
					nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_DAMAGE);
					T.setState(28);
				}
			}

			public void createRollUpEffect(TurnSystem T, int member)
			{
				if (!T.checkFlag(TurnSystem.StartEffectProcess))
				{
					OS_Printf("エフェクト再生開始\n");
					ys.Effects effects = new ys.Effects();
					effects.frameCounter_ = 0;
					effects.category_ = 233;
					effects.member_ = (short)member;
					T.createEffect(0, effects, nowPlayer(), 0, 0, 1);
					T.setCheckFlag(TurnSystem.StartEffectProcess);
				}
			}

			public void isRollUpEffectEnd(TurnSystem T)
			{
				if (T.checkFlag(TurnSystem.StartEffectProcess) && !T.checkFlag(TurnSystem.EndEffectProcess) && nowPlayer().isClearAllEffect())
				{
					OS_Printf("エフェクト終了\n");
					T.setCheckFlag(TurnSystem.EndEffectProcess);
				}
			}

			public void playRollUpSE(TurnSystem T)
			{
				if (!T.checkFlag(TurnSystem.StartSEProcess))
				{
					BattleSE.instance().play(203, 9);
					T.setCheckFlag(TurnSystem.StartSEProcess);
				}
			}

			public void playerActionRollUp(TurnSystem T)
			{
				if (!T.checkFlag(TurnSystem.EndPlayerProcess) && nowPlayer().playerActionId() == 32 && T.checkFlag(TurnSystem.EndEffectProcess))
				{
					T.setCheckFlag(TurnSystem.EndPlayerProcess);
				}
			}

			public void executeExplosion(TurnSystem T)
			{
				isRollUpEffectEnd(T);
				createRollUpEffect(T, 2);
				isExplosion2DEnd(T);
				createExplosion2D(T);
				playExplosionSE(T);
				playerActionExplosion(T);
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && T.checkFlag(TurnSystem.EndEffectProcess) && T.checkFlag(TurnSystem.End2DProcess))
				{
					Battle2DManager.instance().helpWindow().releaseHelpWindow();
					nowPlayer().clearRollUpLevel();
					nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_BACK);
					T.setState(4);
				}
			}

			public void playerActionExplosion(TurnSystem T)
			{
				if (!T.checkFlag(TurnSystem.EndPlayerProcess) && nowPlayer().isPlayerActionEnd())
				{
					nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION);
					T.setCheckFlag(TurnSystem.EndPlayerProcess);
				}
			}

			public void createExplosion2D(TurnSystem T)
			{
				if (T.checkFlag(TurnSystem.Start2DProcess))
				{
					return;
				}
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(i));
					if (baseBattleCharacterFromBreed != null)
					{
						OS_Printf("2D再生開始\n");
						createBaseHit2D(T, nowPlayer(), baseBattleCharacterFromBreed);
					}
				}
				T.setCheckFlag(TurnSystem.Start2DProcess);
			}

			public void isExplosion2DEnd(TurnSystem T)
			{
				if (!T.checkFlag(TurnSystem.Start2DProcess) || T.checkFlag(TurnSystem.End2DProcess))
				{
					return;
				}
				for (int i = 0; i < 12; i++)
				{
					if (Battle2DManager.instance().damage().isExist(i) || Battle2DManager.instance().hit(i).isExist())
					{
						return;
					}
				}
				OS_Printf("2D再生終了\n");
				T.setCheckFlag(TurnSystem.End2DProcess);
			}

			public void playExplosionSE(TurnSystem T)
			{
				if (!T.checkFlag(TurnSystem.StartSEProcess))
				{
					BattleSE.instance().play(203, 14);
					T.setCheckFlag(TurnSystem.StartSEProcess);
				}
			}

			public void initializeProvocation(TurnSystem T)
			{
				if (nowPlayer().condition().isFrog())
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
				if (baseBattleCharacterFromBreed != null)
				{
					if (calcProvacation())
					{
						baseBattleCharacterFromBreed.setFlag(PLAYER_FLAG.PF_PROVOCATION);
						baseBattleCharacterFromBreed.setProvocationCharacterId(nowPlayer().battleCharacterId());
						baseBattleCharacterFromBreed.setProvocationCharacter(nowPlayer());
						baseBattleCharacterFromBreed.setFlag(PLAYER_FLAG.PF_2D);
					}
					else
					{
						baseBattleCharacterFromBreed.setProvocationCharacterId(-1);
						baseBattleCharacterFromBreed.setProvocationCharacter(null);
						baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_2D);
						baseBattleCharacterFromBreed.setFlag(PLAYER_FLAG.PF_MISS);
					}
				}
				nowPlayer().setFlag(PLAYER_FLAG.PF_2D);
				nowPlayer().addJobMotion();
				BattleEffect.instance().addEfp(241);
				BattleEffect.instance().addEfp(231);
				BattleSE.instance().load(203);
				T.setState(0);
			}

			public void stateProvocation(TurnSystem T)
			{
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					startAbilityEffect(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					break;
				case 5:
					createAbilityName(T, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 1);
					break;
				case 36:
					endAbilityCamera(T, PLAYER_ACTION_STATE.PAS_FRONT_READY);
					break;
				case 2:
					moveFrontReady(T);
					break;
				case 3:
					moveFront(T, BATTLE_ACTION_TYPE.DBA_PROVOCATION, PLAYER_ACTION_STATE.PAS_PROVOCATION);
					break;
				case 29:
					executeProvocation(T);
					break;
				}
			}

			public void executeProvocation(TurnSystem T)
			{
				isProvacationEffectEnd(T);
				createProvacationEffect(T);
				playProvacationSE(T);
				playerActionProvacation(T);
				checkProvacation2D(T);
				startProvacation2D(T);
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && T.checkFlag(TurnSystem.EndEffectProcess) && T.checkFlag(TurnSystem.End2DProcess))
				{
					T.setPhase(TurnSystem.Phase.MonsterExecute);
				}
			}

			public bool calcProvacation()
			{
				int num = (int)ds.RandomNumber.rand32(101u);
				CommonFormula commonFormula = new CommonFormula();
				int num2 = commonFormula.calcJobSkill(nowPlayer());
				bool result = false;
				if (num2 < 21)
				{
					if (num <= 60)
					{
						result = true;
					}
				}
				else if (num2 < 41)
				{
					if (num <= 70)
					{
						result = true;
					}
				}
				else if (num2 < 81)
				{
					if (num <= 80)
					{
						result = true;
					}
				}
				else if (num2 < 99)
				{
					if (num <= 90)
					{
						result = true;
					}
				}
				else
				{
					result = true;
				}
				return result;
			}

			public void createProvacationEffect(TurnSystem T)
			{
				if (T.checkFlag(TurnSystem.StartEffectProcess))
				{
					return;
				}
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(i));
					if (baseBattleCharacterFromBreed != null)
					{
						ys.Effects effects = new ys.Effects();
						effects.frameCounter_ = 0;
						effects.category_ = 241;
						effects.member_ = 1;
						T.createEffect(0, effects, nowPlayer(), 0, 0, 1);
					}
				}
				OS_Printf("エフェクト再生開始\n");
				T.setCheckFlag(TurnSystem.StartEffectProcess);
			}

			public void isProvacationEffectEnd(TurnSystem T)
			{
				if (T.checkFlag(TurnSystem.StartEffectProcess) && !T.checkFlag(TurnSystem.EndEffectProcess))
				{
					bool flag = true;
					if (!nowPlayer().isClearAllEffect())
					{
						flag = false;
					}
					if (flag)
					{
						T.setCheckFlag(TurnSystem.EndEffectProcess);
						OS_Printf("エフェクト終了\n");
					}
				}
			}

			public void playProvacationSE(TurnSystem T)
			{
				if (!T.checkFlag(TurnSystem.StartSEProcess))
				{
					BattleSE.instance().play(203, 11);
					T.setCheckFlag(TurnSystem.StartSEProcess);
				}
			}

			public void playerActionProvacation(TurnSystem T)
			{
				if (T.checkFlag(TurnSystem.EndPlayerProcess))
				{
					return;
				}
				if (nowPlayer().playerActionId() == 34)
				{
					if (nowPlayer().isPlayerActionEnd())
					{
						nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_BACK);
					}
				}
				else if (nowPlayer().playerActionId() == 1 && nowPlayer().isPlayerActionEnd())
				{
					nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION);
					T.setCheckFlag(TurnSystem.EndPlayerProcess);
				}
			}

			public void startProvacation2D(TurnSystem T)
			{
				if (T.checkFlag(TurnSystem.Start2DProcess) || !T.checkFlag(TurnSystem.EndEffectProcess))
				{
					return;
				}
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(i));
					if (baseBattleCharacterFromBreed != null)
					{
						OS_Printf("2D再生開始\n");
						if (!baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_2D))
						{
							T.createHit(nowPlayer(), baseBattleCharacterFromBreed);
						}
					}
				}
				T.setCheckFlag(TurnSystem.Start2DProcess);
			}

			public void checkProvacation2D(TurnSystem T)
			{
				if (!T.checkFlag(TurnSystem.Start2DProcess) || T.checkFlag(TurnSystem.End2DProcess))
				{
					return;
				}
				for (int i = 0; i < 12; i++)
				{
					if (Battle2DManager.instance().damage().isExist(i) || Battle2DManager.instance().hit(i).isExist())
					{
						return;
					}
				}
				OS_Printf("2D再生終了\n");
				T.setCheckFlag(TurnSystem.End2DProcess);
			}

			public void initializeOverissue(TurnSystem T)
			{
				if (nowPlayer().condition().isFrog())
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				nowPlayer().setFlag(PLAYER_FLAG.PF_OVERISSUE);
				if (!nowPlayer().condition().isFrog())
				{
					nowPlayer().addEquipWeaponMotion(pl.HAND_TYPE.RIGHT_HAND);
					nowPlayer().addEquipWeaponMotion(pl.HAND_TYPE.LEFT_HAND);
					characterMng.addMotion(nowPlayer().characterMngId(), "b_b02_011");
				}
				if (T.characterManager().monsterParty().aliveNumber() != 0)
				{
					nowPlayer().clearFlag(PLAYER_FLAG.PF_FINISH);
				}
				else
				{
					nowPlayer().setFlag(PLAYER_FLAG.PF_FINISH);
				}
				nowPlayer().setOverissueNumber(OVERISSUE_MAX);
				T.setTargetRandam(nowPlayer(), T.characterManager().monsterParty(), reflect: false);
				T.calcNormalAttackDamage(T.nowCharacter());
				T.setOverissueDamage(nowPlayer());
				T.setNormalAttackDamage(T.nowCharacter());
				setNormalAttackEfp(pl.HAND_TYPE.RIGHT_HAND);
				setNormalAttackEfp(pl.HAND_TYPE.LEFT_HAND);
				BattleEffect.instance().addEfp(211);
				BattleEffect.instance().addEfp(231);
				BattleSE.instance().load(201);
				T.setState(0);
			}

			public void stateOverissue(TurnSystem T)
			{
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					startAbilityEffect(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					break;
				case 5:
					createAbilityName(T, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 1);
					break;
				case 36:
					endAbilityCamera(T, PLAYER_ACTION_STATE.PAS_FRONT_READY);
					break;
				case 2:
					moveFrontReady(T);
					break;
				case 3:
					moveFront(T, BATTLE_ACTION_TYPE.DBA_NORMAL_ATTACK, PLAYER_ACTION_STATE.PAS_NORMAL_ATTACK);
					break;
				case 7:
					executeNormalAttack(T, baseBattleCharacterFromBreed);
					break;
				case 10:
					moveBackAttack(T, baseBattleCharacterFromBreed);
					break;
				}
			}

			public void initializePitch(TurnSystem T)
			{
				if (nowPlayer().condition().isFrog())
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				nowPlayer().setFlag(PLAYER_FLAG.PF_PITCH);
				nowPlayer().addPitchMotion();
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(0));
				if (baseBattleCharacterFromBreed == null)
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				PitchFormula pitchFormula = new PitchFormula();
				if (pitchFormula.calcPitchHitOdds(nowPlayer()))
				{
					int value = pitchFormula.calcPitchDamage(nowPlayer());
					T.calc_.setDamage(nowPlayer().targetId(0), value);
					T.setNormalAttackDamage(nowPlayer());
					baseBattleCharacterFromBreed.setFlag(PLAYER_FLAG.PF_2D);
				}
				else
				{
					baseBattleCharacterFromBreed.clearFlag(PLAYER_FLAG.PF_2D);
					baseBattleCharacterFromBreed.setFlag(PLAYER_FLAG.PF_MISS);
				}
				nowPlayer().deleteItemOrMagicNumber();
				BattleEffect.instance().addEfp(221);
				BattleEffect.instance().addEfp(246);
				BattleEffect.instance().addEfp(231);
				BattleSE.instance().load(203);
				T.setState(0);
			}

			public void statePitch(TurnSystem T)
			{
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					startAbilityEffect(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					break;
				case 5:
					createAbilityName(T, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 1);
					break;
				case 36:
					endAbilityCamera(T, PLAYER_ACTION_STATE.PAS_FRONT_READY);
					break;
				case 2:
					moveFrontReady(T);
					break;
				case 3:
					moveFront(T, BATTLE_ACTION_TYPE.DBA_PITCH, PLAYER_ACTION_STATE.PAS_PITCH);
					break;
				case 31:
					executePitch(T);
					break;
				}
			}

			public void executePitch(TurnSystem T)
			{
				isPitchEffectEnd(T);
				createPitchEffect(T);
				playPitchSE(T);
				isPitch2DEnd(T);
				createPitch2D(T);
				targetPitchDamageAction(T);
				isTargetPitchDamageActionEnd(T);
				isPlayerPitchEnd(T);
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && T.checkFlag(TurnSystem.EndEffectProcess) && T.checkFlag(TurnSystem.End2DProcess) && T.checkFlag(TurnSystem.EndEnemyProcess))
				{
					T.setPhase(TurnSystem.Phase.MonsterExecute);
				}
			}

			public void isPitchEffectEnd(TurnSystem T)
			{
				if (!T.checkFlag(TurnSystem.StartEffectProcess) || T.checkFlag(TurnSystem.EndEffectProcess))
				{
					return;
				}
				bool flag = true;
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(i));
					if (baseBattleCharacterFromBreed != null && !baseBattleCharacterFromBreed.isClearAllEffect())
					{
						flag = false;
					}
				}
				if (!nowPlayer().isClearAllEffect())
				{
					flag = false;
				}
				if (flag)
				{
					T.setCheckFlag(TurnSystem.EndEffectProcess);
					OS_Printf("エフェクト終了\n");
				}
			}

			public void createPitchEffect(TurnSystem T)
			{
				if (T.checkFlag(TurnSystem.StartEffectProcess) || !nowPlayer().checkMotionAndFrame(2101, PITCH_HIT_FRAME))
				{
					return;
				}
				int motionIndex = characterMng.getMotionIndex(nowPlayer().characterMngId());
				pl.PlayerNormalAttackParameter playerNormalAttackParameter = pl.PlayerParty.instance().normalAttack(motionIndex);
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(i));
					if (baseBattleCharacterFromBreed != null && !baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_MISS))
					{
						int frameCounter_ = playerNormalAttackParameter.effect(i).frameCounter_;
						T.createEffect(frameCounter_, playerNormalAttackParameter.effect(i), baseBattleCharacterFromBreed, 0, playerNormalAttackParameter.randamFlag(), 1);
					}
				}
				OS_Printf("エフェクト再生開始\n");
				T.setCheckFlag(TurnSystem.StartEffectProcess);
			}

			public void playPitchSE(TurnSystem T)
			{
				if (nowPlayer().checkMotionAndFrame(2101, PITCH_HIT_FRAME + 3))
				{
					ys.Effects effects = new ys.Effects();
					effects.frameCounter_ = 0;
					if (nowPlayer().flag(PLAYER_FLAG.PF_MISS))
					{
						BattleSE.instance().playMissSE();
					}
					else
					{
						BattleSE.instance().play(203, 13);
					}
				}
			}

			public void isPitch2DEnd(TurnSystem T)
			{
				if (!T.checkFlag(TurnSystem.Start2DProcess) || T.checkFlag(TurnSystem.End2DProcess))
				{
					return;
				}
				for (int i = 0; i < 12; i++)
				{
					if (Battle2DManager.instance().damage().isExist(i) || Battle2DManager.instance().hit(i).isExist())
					{
						return;
					}
				}
				OS_Printf("2D再生終了\n");
				T.setCheckFlag(TurnSystem.End2DProcess);
			}

			public void createPitch2D(TurnSystem T)
			{
				if (!T.checkFlag(TurnSystem.EndEffectProcess) || T.checkFlag(TurnSystem.Start2DProcess))
				{
					return;
				}
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(i));
					if (baseBattleCharacterFromBreed != null)
					{
						OS_Printf("2D再生開始\n");
						createDarkHit2D(T, nowPlayer(), baseBattleCharacterFromBreed);
					}
				}
				T.setCheckFlag(TurnSystem.Start2DProcess);
			}

			public void targetPitchDamageAction(TurnSystem T)
			{
				if (nowPlayer().flag(PLAYER_FLAG.PF_MISS) || nowPlayer().playerActionId() != 33)
				{
					return;
				}
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowPlayer().targetId(i));
					if (baseBattleCharacterFromBreed != null && !baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_MISS) && nowPlayer().checkMotionAndFrame(2101, PITCH_HIT_FRAME))
					{
						T.playFlash(0, 0, baseBattleCharacterFromBreed);
						T.startDamageAction(0, 0, baseBattleCharacterFromBreed);
					}
				}
			}

			public void isTargetPitchDamageActionEnd(TurnSystem T)
			{
				T.deadCharacters(nowPlayer());
			}

			public void isPlayerPitchEnd(TurnSystem T)
			{
				if (T.checkFlag(TurnSystem.EndPlayerProcess))
				{
					return;
				}
				if (nowPlayer().playerActionId() == 33)
				{
					if (nowPlayer().isPlayerActionEnd())
					{
						nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_BACK);
					}
				}
				else if (nowPlayer().playerActionId() == 1 && nowPlayer().isPlayerActionEnd())
				{
					nowPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION);
					T.setCheckFlag(TurnSystem.EndPlayerProcess);
				}
			}

			public void initializeGeography(TurnSystem T)
			{
				if (nowPlayer().condition().isFrog())
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				nowPlayer().addJobMotion();
				selectGeography(T);
				setTargetGeography(T);
				T.calc_.calcGeography(T.characterManager(), nowPlayer());
				BattleEffect.instance().addEfp(231);
				entryEfectGeography(T);
				BattleSE.instance().load(204);
				T.setState(0);
			}

			public void stateGeography(TurnSystem T)
			{
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					startAbilityEffect(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					break;
				case 5:
				{
					int num = nowPlayer().useMagicId();
					int messageId = itm.ItemManager.instance().magicParameter((short)num).nameId();
					createHelpWindow(T, messageId, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				}
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_GEOGRAPHY, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 1);
					break;
				case 36:
					endAbilityCamera(T, PLAYER_ACTION_STATE.PAS_GEOGRAPHY);
					break;
				case 32:
					executeGeography(T);
					break;
				}
			}

			public void executeGeography(TurnSystem T)
			{
				playerGeographyAction(T);
				T.executeCommonMagic();
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && T.checkFlag(TurnSystem.EndEnemyProcess))
				{
					T.setPhase(TurnSystem.Phase.MonsterExecute);
				}
			}

			public void playerGeographyAction(TurnSystem T)
			{
				if (nowPlayer().playerActionId() == 30)
				{
					if (nowPlayer().checkMotionAndFrame(6301, GEOGRAPHY_HIT_FRAME))
					{
						nowPlayer().setShakeScreen();
						T.setCheckFlag(TurnSystem.PlayEffect);
					}
					if (nowPlayer().isPlayerActionEnd())
					{
						T.setCheckFlag(TurnSystem.EndPlayerProcess);
					}
				}
			}

			public void selectGeography(TurnSystem T)
			{
				int map_id = OutsideToBattle.getInstance().initializeBattleMap().battleMapId();
				GeographyData geographyData = const_cast<GeographyData>(T.geographyManager().geographyData(map_id));
				ds.Vector<GeographyInfo, ds.FastErasePolicy<GeographyInfo>> vector = new ds.Vector<GeographyInfo, ds.FastErasePolicy<GeographyInfo>>(GEOGRAPHY_MAX);
				for (int i = 0; i < GEOGRAPHY_MAX; i++)
				{
					if (geographyData.geographyInfo(i).geographyId_ > 0)
					{
						vector.push_back(geographyData.geographyInfo(i));
					}
				}
				int num = (int)ds.RandomNumber.rand32(101u);
				int num2 = 0;
				for (int j = 0; j < vector.size(); j++)
				{
					if (num <= vector.at(j).odds_ + num2)
					{
						nowPlayer().setUseMagicId(vector.at(j).geographyId_);
						break;
					}
					num2 += vector.at(j).odds_;
				}
			}

			public void setTargetGeography(TurnSystem T)
			{
				nowPlayer().clearTargetId();
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(nowPlayer().useMagicId());
				if (magicParameter.targetPossible() == 2)
				{
					T.setTargetRandam(nowPlayer(), T.characterManager().monsterParty(), reflect: false);
				}
				else if (magicParameter.targetPossible() == 128)
				{
					T.setTargetRandam(nowPlayer(), T.characterManager().playerParty(), reflect: false);
				}
				else if (magicParameter.targetPossible() == 512)
				{
					T.characterManager().setPlayerAllTarget(nowPlayer(), 0);
				}
				else
				{
					T.characterManager().setMonsterAllTarget(nowPlayer());
				}
			}

			public void entryEfectGeography(TurnSystem T)
			{
				pl.PlayerNormalMagicParameter playerNormalMagicParameter = pl.PlayerParty.instance().normalMagic(nowPlayer().useMagicId());
				if (playerNormalMagicParameter != null)
				{
					int category_ = playerNormalMagicParameter.effect().category_;
					BattleEffect.instance().addEfp(category_);
				}
			}

			public void initializeSong(TurnSystem T)
			{
				if (nowPlayer().condition().isFrog())
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				nowPlayer().addJobMotion();
				setSong(T);
				if (nowPlayer().useMagicId() == -1)
				{
					nowPlayer().setConditionMotion(0);
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				setSongTarget(T);
				T.calcMagicDamage(nowPlayer());
				BattleEffect.instance().addEfp(231);
				entryEfectGeography(T);
				BattleSE.instance().load(205);
				T.setState(0);
			}

			public void stateSong(TurnSystem T)
			{
				switch (T.state())
				{
				case 0:
					isData(T, PLAYER_ACTION_STATE.PAS_ABILITY_EFFECT_START);
					break;
				case 37:
					startAbilityEffect(T, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_START);
					break;
				case 34:
					startAbilityCamera(T, PLAYER_ACTION_STATE.PAS_CREATE_HELP_WINDOW);
					break;
				case 5:
				{
					int messageId = itm.ItemManager.instance().magicParameter(nowPlayer().useMagicId()).nameId();
					createHelpWindow(T, messageId, PLAYER_ACTION_STATE.PAS_END_HELP_WINDOW);
					break;
				}
				case 6:
					endHelpWindow(T, TurnSystem.DRAW_HELP_WINDOW_FRAME, BATTLE_ACTION_TYPE.DBA_SONG, PLAYER_ACTION_STATE.PAS_ABILITY_CAMERA_END, 1);
					break;
				case 36:
					endAbilityCamera(T, PLAYER_ACTION_STATE.PAS_SONG);
					break;
				case 33:
					executeSong(T);
					break;
				}
			}

			public void executeSong(TurnSystem T)
			{
				playerSongAction(T);
				T.executeCommonMagic();
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && T.checkFlag(TurnSystem.EndEnemyProcess))
				{
					T.setPhase(TurnSystem.Phase.MonsterExecute);
				}
			}

			public void playerSongAction(TurnSystem T)
			{
				if (!T.checkFlag(TurnSystem.EndPlayerProcess) && nowPlayer().playerActionId() == 31)
				{
					if (!T.checkFlag(TurnSystem.PlayEffect))
					{
						T.setCheckFlag(TurnSystem.PlayEffect);
					}
					else if (nowPlayer().isPlayerActionEnd())
					{
						T.setCheckFlag(TurnSystem.EndPlayerProcess);
					}
				}
			}

			public void setSong(TurnSystem T)
			{
				if (nowPlayer().player().equipParameter().isEquipHarp())
				{
					short itemId = 0;
					if (nowPlayer().player().equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND)
						.isEquipHarp())
					{
						itemId = nowPlayer().player().equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND)
							.itemId();
					}
					else if (nowPlayer().player().equipParameter().equipHand(pl.HAND_TYPE.LEFT_HAND)
						.isEquipHarp())
					{
						itemId = nowPlayer().player().equipParameter().equipHand(pl.HAND_TYPE.LEFT_HAND)
							.itemId();
					}
					itm.WeaponParameter weaponParameter = itm.ItemManager.instance().weaponParameter(itemId);
					if (weaponParameter != null && weaponParameter.optionMagicItemId() > 0)
					{
						nowPlayer().setUseMagicId(weaponParameter.optionMagicItemId());
						return;
					}
				}
				nowPlayer().setUseMagicId(-1);
			}

			public bool setSongTarget(TurnSystem T)
			{
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(nowPlayer().useMagicId());
				if (magicParameter == null)
				{
					return false;
				}
				if (magicParameter.system() != 3)
				{
					return false;
				}
				if (magicParameter.targetPossible() == 8)
				{
					T.characterManager().setMonsterAllTarget(nowPlayer());
				}
				else if (magicParameter.targetPossible() == 512)
				{
					T.characterManager().setPlayerAllTarget(nowPlayer(), 0);
				}
				return true;
			}

			public void initializeChangeFormation(TurnSystem T)
			{
				nowPlayer().player().changeFormationType();
				BATTLE_ACTION_TYPE nextPlayerActionId = ((nowPlayer().player().formationType() != 0) ? BATTLE_ACTION_TYPE.DBA_CHANGE_BACK : BATTLE_ACTION_TYPE.DBA_CHANGE_FRONT);
				nowPlayer().setNextPlayerActionId(nextPlayerActionId);
			}

			public void executeChangeFormation(TurnSystem T)
			{
				if (nowPlayer().isPlayerActionEnd())
				{
					T.setPhase(TurnSystem.Phase.MonsterExecute);
				}
			}

			public PlayerTurnSystem()
			{
				nowPlayer_ = null;
			}

			public void setFlag(FLAG_TYPE flag)
			{
				flag_ |= (uint)flag;
			}

			public void clearFlag(FLAG_TYPE flag)
			{
				flag_ &= (uint)(~flag);
			}

			public bool flag(FLAG_TYPE flag)
			{
				if ((flag_ & (uint)flag) == 0)
				{
					return false;
				}
				return true;
			}

			public void clearFlagAll()
			{
				flag_ = 0u;
			}

			private BattlePlayer nowPlayer()
			{
				return nowPlayer_;
			}
		}
	}
}
