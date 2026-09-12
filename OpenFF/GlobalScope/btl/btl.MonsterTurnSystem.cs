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
		public class MonsterTurnSystem
		{
			public enum MONSTER_ACTION_STATE
			{
				MAS_IS_DATA_INITIALIZE,
				MAS_NORMAL_ATTACK,
				MAS_SPECIAL_ATTACK,
				MAS_MAGIC,
				MAS_CREATE_HELP_WINDOW,
				MAS_END_HELP_WINDOW,
				MAS_BREAK_INITIALIZE,
				MAS_SHOW_MONSTER,
				MONSTER_ACTION_STATE_MAX
			}

			public const MONSTER_ACTION_STATE MAS_IS_DATA_INITIALIZE = MONSTER_ACTION_STATE.MAS_IS_DATA_INITIALIZE;

			public const MONSTER_ACTION_STATE MAS_NORMAL_ATTACK = MONSTER_ACTION_STATE.MAS_NORMAL_ATTACK;

			public const MONSTER_ACTION_STATE MAS_SPECIAL_ATTACK = MONSTER_ACTION_STATE.MAS_SPECIAL_ATTACK;

			public const MONSTER_ACTION_STATE MAS_MAGIC = MONSTER_ACTION_STATE.MAS_MAGIC;

			public const MONSTER_ACTION_STATE MAS_CREATE_HELP_WINDOW = MONSTER_ACTION_STATE.MAS_CREATE_HELP_WINDOW;

			public const MONSTER_ACTION_STATE MAS_END_HELP_WINDOW = MONSTER_ACTION_STATE.MAS_END_HELP_WINDOW;

			public const MONSTER_ACTION_STATE MAS_BREAK_INITIALIZE = MONSTER_ACTION_STATE.MAS_BREAK_INITIALIZE;

			public const MONSTER_ACTION_STATE MAS_SHOW_MONSTER = MONSTER_ACTION_STATE.MAS_SHOW_MONSTER;

			public const MONSTER_ACTION_STATE MONSTER_ACTION_STATE_MAX = MONSTER_ACTION_STATE.MONSTER_ACTION_STATE_MAX;

			public const int MONSTER_MAGIC_WAIT_FRAME = 30;

			private BattleMonster nowMonster_;

			private BattleMonster breakAfterMonster_;

			private int workCounter_;

			private int breakState_;

			public void initializeAll()
			{
				nowMonster_ = null;
				breakAfterMonster_ = null;
				workCounter_ = 0;
				breakState_ = 0;
			}

			public void initialize(TurnSystem T)
			{
				OS_Printf("//----------------------------------------------------------------------------------\n");
				OS_Printf("// モンスターの行動開始\n");
				setNowMnster(T.nowCharacter());
				if (nowMonster() == null)
				{
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				if (!nowMonster().isEnable() || !nowMonster().condition().isCanAction() || !T.isTarget(nowMonster()))
				{
					nowMonster().setActionId(0);
					return;
				}
				if (T.characterManager().playerParty().aliveNumber() == 0)
				{
					nowMonster().setActionId(0);
					return;
				}
				if (nowMonster().actionId() == 3 && nowMonster().useMagicId() == 4115 && T.characterManager().useAssistPlayer() == -1)
				{
					nowMonster().setActionId(1);
					T.setTargetRandam(nowMonster(), T.characterManager().playerParty(), reflect: false);
				}
				if (nowMonster().condition().isConfusion())
				{
					nowMonster().setActionId(1);
					T.setTargetRandam(nowMonster(), T.characterManager().monsterParty(), reflect: false);
				}
				else if (nowMonster().condition().isSilence())
				{
					nowMonster().setActionId(1);
					T.setTargetRandam(nowMonster(), T.characterManager().playerParty(), reflect: false);
				}
				else
				{
					T.setTargetRandam(nowMonster(), T.characterManager().playerParty(), reflect: false);
				}
				if (nowMonster().actionId() == 2)
				{
					mon.MonsterSpecialAttackParameter monsterSpecialAttackParameter = mon.MonsterManager.instance().specialAttack(nowMonster().useMagicId());
					if (monsterSpecialAttackParameter != null)
					{
						if (monsterSpecialAttackParameter.command() == 4 && !T.isAugmentMonster(nowMonster()))
						{
							nowMonster().setActionId(1);
							T.setTargetRandam(nowMonster(), T.characterManager().playerParty(), reflect: false);
						}
						if (monsterSpecialAttackParameter.command() == 5 && !T.isSummonMonster(nowMonster()))
						{
							nowMonster().setActionId(1);
							T.setTargetRandam(nowMonster(), T.characterManager().playerParty(), reflect: false);
						}
					}
				}
				switch (nowMonster().actionId())
				{
				case 1:
					initializeNormalAttack(T);
					break;
				case 2:
					initializeSpecialAttack(T);
					break;
				case 3:
					initializeMagic(T);
					break;
				default:
					T.setPhase(TurnSystem.Phase.Terminate);
					break;
				}
			}

			public void terminate(TurnSystem T)
			{
				if (T.coverPlayer() != null)
				{
					T.coverPlayer().removeJobMotion();
				}
				BattleSE.instance().free();
			}

			public void execute(TurnSystem T)
			{
				if (nowMonster() == null)
				{
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				switch (nowMonster().actionId())
				{
				case 1:
					stateNormalAttack(T);
					break;
				case 2:
					stateSpecialAttack(T);
					break;
				case 3:
					stateMagic(T);
					break;
				default:
					T.setPhase(TurnSystem.Phase.Terminate);
					break;
				}
			}

			public void executeBreakMonster(TurnSystem T)
			{
				switch (breakState_)
				{
				case 6:
					initializeBreakMonster(T);
					break;
				case 4:
					createHelpWindow(T, MONSTER_ACTION_STATE.MAS_END_HELP_WINDOW);
					break;
				case 5:
					endHelpWindow(T, MONSTER_ACTION_STATE.MAS_IS_DATA_INITIALIZE);
					break;
				case 0:
					if (T.breakAfterMonster().registerMonsterAsync())
					{
						BattleSE.instance().play(0, 40);
						breakState_ = 7;
					}
					break;
				case 7:
					if (T.breakRootMonster() != null && 6611 == T.breakRootMonster().useMagicId())
					{
						T.breakRootMonster().setUseMagicId(T.breakRootMonster().prevMagicId());
					}
					T.setPhase(TurnSystem.Phase.Terminate);
					break;
				case 1:
				case 2:
				case 3:
					break;
				}
			}

			public void executeSummonMonster(TurnSystem T)
			{
			}

			public void setNowMnster(BaseBattleCharacter nowCharacter)
			{
				if (nowCharacter.breed() == 1)
				{
					BattleMonster battleMonster = static_cast<BattleMonster>(nowCharacter);
					if (battleMonster != null)
					{
						nowMonster_ = battleMonster;
					}
				}
			}

			public void createHelpWindow(TurnSystem T, MONSTER_ACTION_STATE next)
			{
				workCounter_ = 0;
				Battle2DManager.instance().helpWindow().createHelpWindow(itm.ItemManager.instance().magicParameter(nowMonster().useMagicId()).nameId(), 0, 0);
				Battle2DManager.instance().helpWindow().setMsdHandle(1);
				T.setState((int)next);
				breakState_ = (int)next;
			}

			public void endHelpWindow(TurnSystem T, MONSTER_ACTION_STATE next)
			{
				if (++workCounter_ == 30)
				{
					workCounter_ = 0;
					Battle2DManager.instance().helpWindow().releaseHelpWindow();
					T.setState((int)next);
					breakState_ = (int)next;
				}
			}

			public void initializeNormalAttack(TurnSystem T)
			{
				if (nowMonster().condition().isFrog())
				{
					pl.PlayerNormalAttackParameter playerNormalAttackParameter = pl.PlayerParty.instance().normalAttack(118);
					int category_ = playerNormalAttackParameter.effect(0).category_;
					BattleEffect.instance().addEfp(category_);
				}
				else
				{
					mon.MonsterNormalAttackParameter monsterNormalAttackParameter = mon.MonsterManager.instance().normalAttack(nowMonster().monsterId());
					BattleEffect.instance().addEfp(monsterNormalAttackParameter.effects(0).category_);
				}
				if (nowMonster().condition().isConfusion())
				{
					T.setTargetRandam(nowMonster(), T.characterManager().monsterParty(), reflect: false);
				}
				else
				{
					T.setTargetRandam(nowMonster(), T.characterManager().playerParty(), reflect: false);
				}
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowMonster().targetId(0));
				if (baseBattleCharacterFromBreed == null || baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_JUMP))
				{
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				nowMonster().clearFlag(PLAYER_FLAG.PF_CRITICAL);
				T.clearCheckFlag(TurnSystem.ExecuteCoverProcess);
				T.setCoverPlayer(T.characterManager().playerParty().serchExecuteCoverMan(baseBattleCharacterFromBreed));
				if (T.coverPlayer() != null)
				{
					T.coverPlayer().setFlag(PLAYER_FLAG.PF_COVER);
					T.coverPlayer().addJobMotion();
					BattleEffect.instance().addEfp(247);
					nowMonster().setLastTargetId();
					nowMonster().setTargetId(0, T.coverPlayer().battleCharacterId());
					T.calcNormalAttackDamage(nowMonster());
					T.setCheckFlag(TurnSystem.ExecuteCoverProcess);
					T.setNormalAttackDamage(nowMonster());
				}
				else
				{
					T.calcNormalAttackDamage(T.nowCharacter());
					T.setCounterMan(nowMonster(), baseBattleCharacterFromBreed);
					T.setNormalAttackDamage(T.nowCharacter());
				}
				T.clearCheckFlag(TurnSystem.EndEnemyMotionProcess);
				BattleSE.instance().load(201);
				T.setState(0);
			}

			public void stateNormalAttack(TurnSystem T)
			{
				switch (T.state())
				{
				case 0:
					isNormalAttackData(T);
					break;
				case 1:
					executeNormalAttack(T);
					break;
				}
			}

			public void isNormalAttackData(TurnSystem T)
			{
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowMonster().targetId(0));
				if ((TexDivideLoader.getSingleton().tdlIsEmpty() || baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_GUARD)) && !BattleSE.instance().isLoadAsync())
				{
					if (nowMonster().condition().isFrog())
					{
						characterMng.startMotion(nowMonster().characterMngId(), 118, fLoop: false, 0u);
					}
					else
					{
						characterMng.startMotion(nowMonster().characterMngId(), 201, fLoop: false, 0u);
					}
					T.setState(1);
					workCounter_ = 0;
				}
			}

			public void executeNormalAttack(TurnSystem T)
			{
				BaseBattleCharacter baseBattleCharacterFromBreed = T.characterManager().getBaseBattleCharacterFromBreed(nowMonster().targetId(0));
				mon.MonsterNormalAttackParameter monsterNormalAttackParameter = mon.MonsterManager.instance().normalAttack(nowMonster().monsterId());
				if (nowMonster().condition().isFrog())
				{
					if (characterMng.getMotionIndex(nowMonster().characterMngId()) == 118 && characterMng.isEndOfMotion(nowMonster().characterMngId()))
					{
						characterMng.startMotion(nowMonster().characterMngId(), 101, fLoop: true, 0u);
						T.setCheckFlag(TurnSystem.EndPlayerProcess);
					}
				}
				else if (characterMng.getMotionIndex(nowMonster().characterMngId()) == 201 && characterMng.isEndOfMotion(nowMonster().characterMngId()))
				{
					characterMng.startMotion(nowMonster().characterMngId(), 101, fLoop: true, 5u);
					T.setCheckFlag(TurnSystem.EndPlayerProcess);
				}
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && !T.checkFlag(TurnSystem.End2DProcess))
				{
					bool flag = true;
					for (int i = 0; i < 12; i++)
					{
						if (Battle2DManager.instance().damage().isExist(i))
						{
							flag = false;
						}
						if (Battle2DManager.instance().hit(i).isExist())
						{
							flag = false;
						}
					}
					if (flag)
					{
						T.setCheckFlag(TurnSystem.End2DProcess);
					}
				}
				int draw = 0;
				if (baseBattleCharacterFromBreed != null)
				{
					draw = ((!baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_MISS)) ? 1 : 0);
				}
				pl.PlayerNormalAttackParameter playerNormalAttackParameter = pl.PlayerParty.instance().normalAttack(118);
				bool flag2 = false;
				if (baseBattleCharacterFromBreed != null)
				{
					if (nowMonster().condition().isFrog())
					{
						if (T.checkFlag(TurnSystem.ExecuteCoverProcess) && workCounter_ == playerNormalAttackParameter.effect(0).frameCounter_ && T.coverPlayer() != null)
						{
							characterMng.setPosition(T.coverPlayer().characterMngId(), CoverManPosition[nowMonster().lastTargetId()]);
							T.coverPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_COVER);
						}
						if (T.createEffect(workCounter_, playerNormalAttackParameter.effect(0), baseBattleCharacterFromBreed, 0, 0, draw))
						{
							flag2 = true;
						}
					}
					else
					{
						if (T.checkFlag(TurnSystem.ExecuteCoverProcess) && workCounter_ == monsterNormalAttackParameter.effects(0).frameCounter_ && T.coverPlayer() != null)
						{
							characterMng.setPosition(T.coverPlayer().characterMngId(), CoverManPosition[nowMonster().lastTargetId()]);
							T.coverPlayer().setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_COVER);
						}
						if (T.createEffect(workCounter_, monsterNormalAttackParameter.effects(0), baseBattleCharacterFromBreed, 0, 0, draw))
						{
							flag2 = true;
						}
					}
				}
				if (baseBattleCharacterFromBreed != null && flag2)
				{
					if (baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_MISS))
					{
						T.createHit(nowMonster(), baseBattleCharacterFromBreed);
					}
					else
					{
						T.createDamage(baseBattleCharacterFromBreed, null);
						if (nowMonster().flag(PLAYER_FLAG.PF_CRITICAL))
						{
							T.createCritical(baseBattleCharacterFromBreed);
							T.createCriticalFlash();
						}
					}
					T.setCheckFlag(TurnSystem.EndEffectProcess);
				}
				if (nowMonster().condition().isFrog())
				{
					T.playSE(workCounter_, playerNormalAttackParameter.se(0), 0, 0, 0);
				}
				else if (baseBattleCharacterFromBreed != null)
				{
					if (!baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_MISS))
					{
						int guard = 0;
						if (baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_GUARD))
						{
							guard = 1;
						}
						else if (baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_MORE_GUARD))
						{
							guard = 2;
						}
						else if (baseBattleCharacterFromBreed.flag(PLAYER_FLAG.PF_COVER))
						{
							guard = 1;
						}
						T.playSE(workCounter_, monsterNormalAttackParameter.effects(1), guard, 0, 0);
					}
					else
					{
						T.playSE(workCounter_, monsterNormalAttackParameter.effects(1), 0, 1, 0);
					}
				}
				if (!T.checkFlag(TurnSystem.EndEnemyMotionProcess))
				{
					if (!T.checkFlag(TurnSystem.ExecuteCoverProcess))
					{
						if (baseBattleCharacterFromBreed != null)
						{
							if (nowMonster().condition().isFrog())
							{
								if (T.startDamageAction(workCounter_, playerNormalAttackParameter.targetMotionStartFrame(0), baseBattleCharacterFromBreed))
								{
									T.setCheckFlag(TurnSystem.EndEnemyMotionProcess);
								}
							}
							else if (T.startDamageAction(workCounter_, monsterNormalAttackParameter.damageMotion(), baseBattleCharacterFromBreed))
							{
								T.setCheckFlag(TurnSystem.EndEnemyMotionProcess);
							}
						}
					}
					else if (T.coverPlayer().playerActionId() == 35 && T.coverPlayer().isPlayerActionEnd())
					{
						characterMng.setPosition(T.coverPlayer().characterMngId(), T.coverPlayer().rootPosition());
						T.setCheckFlag(TurnSystem.EndEnemyMotionProcess);
					}
				}
				else
				{
					T.deadCharacters(nowMonster());
				}
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && T.checkFlag(TurnSystem.EndEnemyProcess) && T.checkFlag(TurnSystem.End2DProcess) && T.checkFlag(TurnSystem.EndEffectProcess))
				{
					if (workCounter_ > 0)
					{
						workCounter_ = -10;
					}
					if (workCounter_ == 0)
					{
						nowMonster().clearTargetId();
						T.setPhase(TurnSystem.Phase.MonsterExecute);
					}
				}
				workCounter_++;
			}

			public void initializeMagic(TurnSystem T)
			{
				short num = nowMonster().useMagicId();
				if (nowMonster().condition().isSilence())
				{
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				if (nowMonster().condition().isFrog() && num != 4005)
				{
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				setTargetSpecialAttack(T);
				if (!nowMonster().isTargetId())
				{
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				T.calcMagicDamage(nowMonster());
				pl.PlayerNormalMagicParameter playerNormalMagicParameter = pl.PlayerParty.instance().normalMagic(num);
				int category_ = playerNormalMagicParameter.effect().category_;
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(num);
				int num2 = ((magicParameter.system() != 0) ? 260 : 250);
				int se_group = num2 + magicParameter.magicClass();
				BattleSE.instance().load(se_group);
				BattleEffect.instance().addEfp(category_);
				BattleEffect.instance().addEfp(406);
				T.addEfpReflect();
				ys.Effects effects = new ys.Effects();
				effects.frameCounter_ = 0;
				effects.category_ = 406;
				effects.member_ = 1;
				T.createEffect(0, effects, nowMonster(), 0, 0, 1);
				Battle2DManager.instance().helpWindow().createHelpWindow(itm.ItemManager.instance().magicParameter(num).nameId(), 0, 0);
				Battle2DManager.instance().helpWindow().setMsdHandle(1);
				T.setState(0);
			}

			public void stateMagic(TurnSystem T)
			{
				if (nowMonster().condition().isFrog() && nowMonster().useMagicId() != 4005)
				{
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				switch (T.state())
				{
				case 0:
					isMagicData(T);
					break;
				case 3:
					executeMagic(T);
					break;
				}
			}

			public void isMagicData(TurnSystem T)
			{
				if (TexDivideLoader.getSingleton().tdlIsEmpty())
				{
					workCounter_ = 0;
					T.setState(3);
					BattleSE.instance().play(200, 9);
				}
			}

			public void executeMagic(TurnSystem T)
			{
				workCounter_++;
				if (workCounter_ < 30)
				{
					return;
				}
				if (workCounter_ == 30)
				{
					Battle2DManager.instance().helpWindow().releaseHelpWindow();
					nowMonster().setShakeScreen();
					T.setCheckFlag(TurnSystem.PlayEffect);
					T.setCheckFlag(TurnSystem.EndPlayerProcess);
					T.setCheckFlag(TurnSystem.END_MAGIC_EFFECT);
					return;
				}
				if (T.checkFlag(TurnSystem.END_MAGIC_EFFECT))
				{
					T.executeCommonMagic();
				}
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && T.checkFlag(TurnSystem.EndEnemyProcess))
				{
					T.setPhase(TurnSystem.Phase.MonsterExecute);
				}
			}

			public void initializeSpecialAttack(TurnSystem T)
			{
				if (nowMonster().condition().isFrog())
				{
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				short num = nowMonster().useMagicId();
				setTargetSpecialAttack(T);
				if (!nowMonster().isTargetId())
				{
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				calcDamageSpecialAttack(T);
				if (num == 6606)
				{
					nowMonster().setTargetIdMyself();
					nowMonster().setConditionDeath();
					nowMonster().setFlag(PLAYER_FLAG.PF_2D);
				}
				bool flag = true;
				mon.MonsterSpecialAttackEffects monsterSpecialAttackEffects = mon.MonsterManager.instance().effectsInfo(num);
				mon.MonsterSpecialAttackParameter monsterSpecialAttackParameter = mon.MonsterManager.instance().specialAttack(num);
				if (monsterSpecialAttackEffects != null)
				{
					if (monsterSpecialAttackEffects.effectInfo(0).category_ > 0)
					{
						BattleEffect.instance().addEfp(monsterSpecialAttackEffects.effectInfo(0).category_);
					}
					else if (monsterSpecialAttackParameter.command() != 2)
					{
						T.setCheckFlag(TurnSystem.StartEffectProcess);
						T.setCheckFlag(TurnSystem.EndEffectProcess);
					}
					if (monsterSpecialAttackParameter.command() == 5)
					{
						if (T.isSummonMonster(nowMonster()))
						{
							flag = false;
						}
					}
					else if (monsterSpecialAttackParameter.command() == 4 && T.isAugmentMonster(nowMonster()))
					{
						flag = false;
					}
				}
				if (flag)
				{
					BattleSE.instance().load(202);
				}
				T.setState(0);
			}

			public void stateSpecialAttack(TurnSystem T)
			{
				if (nowMonster().condition().isFrog() && nowMonster().useMagicId() != 4005)
				{
					T.setPhase(TurnSystem.Phase.Terminate);
					return;
				}
				switch (T.state())
				{
				case 0:
					isSpecialAttackData(T);
					break;
				case 4:
					createHelpWindow(T, MONSTER_ACTION_STATE.MAS_END_HELP_WINDOW);
					break;
				case 5:
					endHelpWindow(T, MONSTER_ACTION_STATE.MAS_SPECIAL_ATTACK);
					break;
				case 2:
					executeSpecialAttack(T);
					break;
				case 1:
				case 3:
					break;
				}
			}

			public void isSpecialAttackData(TurnSystem T)
			{
				if (!TexDivideLoader.getSingleton().tdlIsEmpty() || BattleSE.instance().isLoadAsync())
				{
					return;
				}
				workCounter_ = 0;
				mon.MonsterSpecialAttackParameter monsterSpecialAttackParameter = mon.MonsterManager.instance().specialAttack(nowMonster().useMagicId());
				if (monsterSpecialAttackParameter != null)
				{
					if (monsterSpecialAttackParameter.command() == 5)
					{
						T.setPhase(TurnSystem.Phase.MonsterExecute);
						return;
					}
					if (monsterSpecialAttackParameter.command() == 4)
					{
						T.setPhase(TurnSystem.Phase.MonsterExecute);
						return;
					}
				}
				T.setState(4);
			}

			public void executeSpecialAttack(TurnSystem T)
			{
				checkEffectStart(T);
				T.executeMonsterSpecial();
				monsterSpecialAttackAction(T);
				if (T.checkFlag(TurnSystem.EndPlayerProcess) && T.checkFlag(TurnSystem.EndEnemyProcess) && T.checkFlag(TurnSystem.End2DProcess))
				{
					T.setPhase(TurnSystem.Phase.MonsterExecute);
				}
			}

			public void monsterSpecialAttackAction(TurnSystem T)
			{
				if (T.checkFlag(TurnSystem.EndPlayerProcess))
				{
					return;
				}
				mon.MonsterSpecialAttackEffects monsterSpecialAttackEffects = mon.MonsterManager.instance().effectsInfo(nowMonster().useMagicId());
				if (monsterSpecialAttackEffects != null)
				{
					if (monsterSpecialAttackEffects.changeMotionIndex() > 0)
					{
						if (characterMng.getMotionIndex(nowMonster().characterMngId()) == monsterSpecialAttackEffects.changeMotionIndex())
						{
							if (characterMng.isEndOfMotion(nowMonster().characterMngId()))
							{
								characterMng.startMotion(nowMonster().characterMngId(), 101, fLoop: true, 10u);
								T.setCheckFlag(TurnSystem.EndPlayerProcess);
							}
						}
						else
						{
							characterMng.startMotion(nowMonster().characterMngId(), monsterSpecialAttackEffects.changeMotionIndex(), fLoop: false, 10u);
						}
					}
					else
					{
						T.setCheckFlag(TurnSystem.EndPlayerProcess);
					}
				}
				else
				{
					T.setCheckFlag(TurnSystem.EndPlayerProcess);
				}
			}

			public void setTargetSpecialAttack(TurnSystem T)
			{
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(nowMonster().useMagicId());
				nowMonster().clearTargetId();
				if ((magicParameter.targetPossible() & 0x40) != 0)
				{
					nowMonster().setTargetIdMyself();
				}
				else if (magicParameter.targetPosition() == 0 || magicParameter.itemId() == 4005 || magicParameter.itemId() == 4006)
				{
					if (T.isOnlyAllMagic(nowMonster().useMagicId()) || magicParameter.itemId() == 4115)
					{
						T.characterManager().setPlayerAllTarget(nowMonster(), 0);
					}
					else if (T.isOnlySingleMagic(nowMonster().useMagicId()))
					{
						T.setTargetRandam(nowMonster(), T.characterManager().playerParty(), reflect: false);
					}
					else if (ds.RandomNumber.logic32(2u) != 0)
					{
						T.characterManager().setPlayerAllTarget(nowMonster(), 0);
					}
					else
					{
						T.setTargetRandam(nowMonster(), T.characterManager().playerParty(), reflect: false);
					}
				}
				else if (magicParameter.targetPosition() == 3 || magicParameter.targetPosition() == 2 || magicParameter.targetPosition() == 4)
				{
					if (T.isOnlyAllMagic(nowMonster().useMagicId()))
					{
						T.characterManager().setMonsterAllTarget(nowMonster());
					}
					else if (T.isOnlySingleMagic(nowMonster().useMagicId()))
					{
						T.setTargetRandam(nowMonster(), T.characterManager().monsterParty(), reflect: false);
					}
					else if (ds.RandomNumber.logic32(2u) != 0)
					{
						T.characterManager().setMonsterAllTarget(nowMonster());
					}
					else
					{
						T.setTargetRandam(nowMonster(), T.characterManager().monsterParty(), reflect: false);
					}
				}
			}

			public void calcDamageSpecialAttack(TurnSystem T)
			{
				mon.MonsterSpecialAttackParameter monsterSpecialAttackParameter = mon.MonsterManager.instance().specialAttack(nowMonster().useMagicId());
				if (monsterSpecialAttackParameter.command() == 0)
				{
					short num = monsterSpecialAttackParameter.specialAttackId();
					if (num == 6602)
					{
						T.setCheckFlag(TurnSystem.End2DProcess);
						changeWeakType(T);
						nowMonster().setMonsterFlag(MONSTER_FLAG.MF_WEAK_CHANGE);
						return;
					}
				}
				T.calcMagicDamage(nowMonster());
			}

			public void checkEffectStart(TurnSystem T)
			{
				if (T.checkFlag(TurnSystem.PlayEffect))
				{
					return;
				}
				mon.MonsterSpecialAttackEffects monsterSpecialAttackEffects = mon.MonsterManager.instance().effectsInfo(nowMonster().useMagicId());
				int motionIndex = characterMng.getMotionIndex(nowMonster().characterMngId());
				int currentFrame = (int)characterMng.getCurrentFrame(nowMonster().characterMngId());
				for (int i = 0; i < mon.EFFECTS_MAX; i++)
				{
					if (monsterSpecialAttackEffects.effectInfo(i).timingInfo_.motionIndex_ == motionIndex && monsterSpecialAttackEffects.effectInfo(i).timingInfo_.frame_ == currentFrame)
					{
						T.setCheckFlag(TurnSystem.PlayEffect);
						nowMonster().setShakeScreen();
						return;
					}
				}
				if (monsterSpecialAttackEffects.effectInfo(0).timingInfo_.motionIndex_ < 0 && monsterSpecialAttackEffects.effectInfo(0).timingInfo_.frame_ < 0)
				{
					T.setCheckFlag(TurnSystem.PlayEffect);
					nowMonster().setShakeScreen();
				}
			}

			public void changeWeakType(TurnSystem T)
			{
				mon.MonsterSpecialAttackParameter monsterSpecialAttackParameter = mon.MonsterManager.instance().specialAttack(nowMonster().useMagicId());
				int i;
				for (i = 0; i < 6 && monsterSpecialAttackParameter.param(i) >= 0; i++)
				{
				}
				int i2 = (int)ds.RandomNumber.logic32((uint)i);
				short num = monsterSpecialAttackParameter.param(i2);
				nowMonster().magicDefense().weakType_set(num);
				int num2 = 2047 - num;
				nowMonster().physicsDefense().antiType_set((short)num2);
			}

			public void initializeBreakMonster(TurnSystem T)
			{
				nowMonster_ = T.breakRootMonster();
				int num = T.characterManager().monsterParty().targetBreakMonsterId();
				int char_id = T.characterManager().breakMonsterCharatcerId(num);
				mon.MonsterSpecialAttackParameter monsterSpecialAttackParameter = mon.MonsterManager.instance().specialAttack(nowMonster_.useMagicId());
				if (nowMonster_.monsterFlag(MONSTER_FLAG.MF_BREAK))
				{
					nowMonster_.setPrevMagicId(nowMonster_.useMagicId());
					nowMonster_.setUseMagicId(6611);
				}
				nowMonster_.clearMonsterFlag(MONSTER_FLAG.MF_BREAK);
				if (monsterSpecialAttackParameter != null && monsterSpecialAttackParameter.command() == 5)
				{
					T.characterManager().monsterParty().battleMonster(num)
						.setNewMonster(char_id, num, monsterSpecialAttackParameter.param(0));
				}
				else
				{
					T.characterManager().monsterParty().battleMonster(num)
						.setNewMonster(char_id, num, nowMonster().monsterId());
				}
				BattleMonster battleMonster = T.characterManager().monsterParty().battleMonster(num);
				T.setBreakAfterMonster(battleMonster);
				if (nowMonster_.useMagicId() == 6611)
				{
					int t = nowMonster_.hp().getNow() / 2;
					t = ds.max(t, 1);
					battleMonster.hp().setNow(t);
				}
				else if (monsterSpecialAttackParameter != null && monsterSpecialAttackParameter.command() == 4)
				{
					battleMonster.hp().setNow(nowMonster_.hp().getNow());
				}
				T.characterManager().monsterParty().memberNumber_inc();
				breakState_ = 4;
			}

			public void initializeSummonMonster(TurnSystem T)
			{
			}

			public MonsterTurnSystem()
			{
				nowMonster_ = null;
				breakAfterMonster_ = null;
			}

			public void setBreakState(int state)
			{
				breakState_ = state;
			}

			private BattleMonster nowMonster()
			{
				return nowMonster_;
			}
		}
	}
}
