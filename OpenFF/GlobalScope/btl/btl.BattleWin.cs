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
		public class BattleWin : BaseBattle
		{
			public enum GET_PHASE
			{
				FADE_WAIT,
				CHANGE_BGM,
				FADE_START,
				ENJOY,
				WINDOW_OPEN,
				GET_GOLD,
				GET_EXP,
				LEVEL_UP,
				SKILL_UP,
				GET_ITEM,
				GET_PHASE_END,
				END,
				GET_PHASE_MAX
			}

			public class GetPhaseExecute
			{
				public delegate bool _GetPhaseExecute(BattleSystem B);

				public _GetPhaseExecute __GetPhaseExecute;

				public GetPhaseExecute(_GetPhaseExecute arg0)
				{
					__GetPhaseExecute = arg0;
				}
			}

			public const GET_PHASE FADE_WAIT = GET_PHASE.FADE_WAIT;

			public const GET_PHASE CHANGE_BGM = GET_PHASE.CHANGE_BGM;

			public const GET_PHASE FADE_START = GET_PHASE.FADE_START;

			public const GET_PHASE ENJOY = GET_PHASE.ENJOY;

			public const GET_PHASE WINDOW_OPEN = GET_PHASE.WINDOW_OPEN;

			public const GET_PHASE GET_GOLD = GET_PHASE.GET_GOLD;

			public const GET_PHASE GET_EXP = GET_PHASE.GET_EXP;

			public const GET_PHASE LEVEL_UP = GET_PHASE.LEVEL_UP;

			public const GET_PHASE SKILL_UP = GET_PHASE.SKILL_UP;

			public const GET_PHASE GET_ITEM = GET_PHASE.GET_ITEM;

			public const GET_PHASE GET_PHASE_END = GET_PHASE.GET_PHASE_END;

			public const GET_PHASE END = GET_PHASE.END;

			public const GET_PHASE GET_PHASE_MAX = GET_PHASE.GET_PHASE_MAX;

			public const int FADE_FRAME = 6;

			public GetPhaseExecute[] GetPhaseExecute_;

			private GET_PHASE getPhase_;

			private GET_PHASE prevGetPhase_;

			private byte nowPlayerId_;

			private int changeBGMCounter_;

			public override void initialize(BattleSystem B)
			{
				BattleSE.instance().free();
				MatrixSound.MtxSoundBGM.getSingleton().stop(CHANGE_BGM_COUNT_MAX, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
				Battle2DManager.instance().helpWindow().setMsdHandle(0);
				setGetPhase(GET_PHASE.FADE_WAIT);
				nowPlayerId_ = 0;
				dgs.CFade.Main().fadeOut(6, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
				dgs.CFade.Sub().fadeOut(6, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			}

			public override void terminate(BattleSystem B)
			{
			}

			public override void execute(BattleSystem B)
			{
				GetPhaseExecute_[(int)getPhase_].__GetPhaseExecute(B);
			}

			public bool waitFadePhase(BattleSystem B)
			{
				if (dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
				{
					setGetPhase(GET_PHASE.CHANGE_BGM);
					return true;
				}
				return false;
			}

			public bool changeBGMPhase(BattleSystem B)
			{
				if (MatrixSound.MtxSoundBGM.getSingleton().getState(MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0) == MatrixSound.enMtxBGMState.enMTX_BGM_STOP)
				{
					BattleBGM.instance().free();
					BattleBGM.instance().loadAndPlay(58, 0);
					BattleParty battleParty = B.characterManager().playerParty();
					for (byte b = 0; b < 4; b++)
					{
						BattlePlayer battlePlayer = battleParty.battlePlayer(b);
						if (battlePlayer != null)
						{
							if (battlePlayer.isEnable())
							{
								if (battlePlayer.isBattle())
								{
									battlePlayer.clearFlagAll();
									battlePlayer.clearMagicFlagAll();
									battlePlayer.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_NON_ACTION);
								}
								battlePlayer.setAlpha(ALPHA_RATE_MAX, SHADOW_ALPHA_RATE_MAX);
								if (battlePlayer.condition().isFrog() || battlePlayer.condition().isLilliput())
								{
									VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
									fnd_reuse_pos.x = PlayerShadowScale.x / 2;
									fnd_reuse_pos.y = PlayerShadowScale.y;
									fnd_reuse_pos.z = PlayerShadowScale.z / 2;
									characterMng.setShadowScale(battlePlayer.characterMngId(), fnd_reuse_pos);
								}
								else
								{
									characterMng.setShadowScale(battlePlayer.characterMngId(), PlayerShadowScale);
								}
								characterMng.setPosition(battlePlayer.characterMngId(), PlayerWinPosition[b]);
								characterMng.setRotation(battlePlayer.characterMngId(), 0, PlayerWinRotateY[battlePlayer.playerId()], 0);
							}
							battlePlayer.setIdleType(0);
							if (battlePlayer.isBattle())
							{
								battlePlayer.player().skillManager().skill(pl.GET_SKILL_TYPE.GET_RIGHT_HAND)
									.skillExpPlusPoolSkillExp();
								battlePlayer.player().skillManager().skill(pl.GET_SKILL_TYPE.GET_LEFT_HAND)
									.skillExpPlusPoolSkillExp();
								if (!battlePlayer.condition().isFrog())
								{
									battlePlayer.setWinMotion();
								}
							}
						}
					}
					changeBGMCounter_ = 90;
					setGetPhase(GET_PHASE.FADE_START);
					battleDisplay.readyEndingCamera();
					B.playerWindow().release();
					return true;
				}
				return false;
			}

			public bool startFadeInPhase(BattleSystem B)
			{
				dgs.CFade.Main().fadeIn(6);
				dgs.CFade.Sub().fadeIn(6);
				setGetPhase(GET_PHASE.ENJOY);
				return true;
			}

			public bool enjoyPhase(BattleSystem B)
			{
				if (!dgs.CFade.Main().isCleared() && !dgs.CFade.Sub().isCleared())
				{
					return false;
				}
				changeBGMCounter_--;
				if (changeBGMCounter_ <= 0)
				{
					setGetPhase(GET_PHASE.WINDOW_OPEN);
					return true;
				}
				return false;
			}

			public bool windowOpenPhase(BattleSystem B)
			{
				dgs.CCtrlCodeInterface.instance().setFontSize(_FontType: true);
				int num = B.characterManager().monsterParty().giftGold()
					.get();
				dgs.CCtrlCodeInterface.instance().setGold(num);
				Battle2DManager.instance().helpWindow().createHelpWindow(106, 1, 0);
				pl.PlayerParty.instance().gold().add(num);
				if (pl.PlayerParty.instance().gold().get() >= 50000)
				{
					UserInfo.AwardAchievement(6);
					if (pl.PlayerParty.instance().gold().get() >= 500000)
					{
						UserInfo.AwardAchievement(7);
					}
				}
				setGetPhase(GET_PHASE.GET_GOLD);
				return true;
			}

			public bool getGoldPhase(BattleSystem B)
			{
				if (Battle2DManager.instance().helpWindow().isCreate() && B.isEdgeAButtonAndTouchEdge())
				{
					menu.MenuManager.getSingleton().playSEDecide();
					dgs.CCtrlCodeInterface.instance().setExp(B.characterManager().getTrueExp());
					Battle2DManager.instance().helpWindow().updateMessage(108, 0);
					setGetPhase(GET_PHASE.GET_EXP);
					return true;
				}
				return false;
			}

			public bool getExpPhase(BattleSystem B)
			{
				if (B.isEdgeAButtonAndTouchEdge() || prevGetPhase_ == GET_PHASE.LEVEL_UP || prevGetPhase_ == GET_PHASE.SKILL_UP)
				{
					menu.MenuManager.getSingleton().playSEDecide();
					while (nowPlayerId_ < 4)
					{
						pl.Player player = pl.PlayerParty.instance().player(nowPlayerId_);
						if (player.isEnable() && !player.condition().isNotBattleCondition())
						{
							if (player.levelUp(B.characterManager().getTrueExp()))
							{
								dgs.CCtrlCodeInterface.instance().setPlayerId(player.playerId());
								Battle2DManager.instance().helpWindow().updateMessage(109, 0);
								B.characterManager().playerParty().battlePlayer(nowPlayerId_)
									.setHappyMotion();
								setGetPhase(GET_PHASE.LEVEL_UP);
								return true;
							}
							if (player.jobManager().jobSkillExpPlusPoolSkillExp())
							{
								dgs.CCtrlCodeInterface.instance().setPlayerId(player.playerId());
								Battle2DManager.instance().helpWindow().updateMessage(110, 0);
								B.characterManager().playerParty().battlePlayer(nowPlayerId_)
									.setHappyMotion();
								setGetPhase(GET_PHASE.SKILL_UP);
								return true;
							}
						}
						nowPlayerId_++;
					}
					BattleMonsterParty battleMonsterParty = B.characterManager().monsterParty();
					if (itm.ItemManager.instance().itemParameter(battleMonsterParty.dropItemId()) != null)
					{
						pl.PlayerParty.instance().addItem(battleMonsterParty.dropItemId(), battleMonsterParty.dropItemNumber().get());
						dgs.CCtrlCodeInterface.instance().setItemId(itm.ItemManager.instance().itemParameter(battleMonsterParty.dropItemId()).nameId());
						Battle2DManager.instance().helpWindow().updateMessage(111, 0);
						setGetPhase(GET_PHASE.GET_ITEM);
						return true;
					}
					setGetPhase(GET_PHASE.GET_PHASE_END);
					return true;
				}
				return false;
			}

			public bool levelUpPhase(BattleSystem B)
			{
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayerId_);
				if (!battlePlayer.restartWinMotion())
				{
					return false;
				}
				if (B.isEdgeAButtonAndTouchEdge())
				{
					if (pl.PlayerParty.instance().player(nowPlayerId_).jobManager()
						.jobSkillExpPlusPoolSkillExp())
					{
						menu.MenuManager.getSingleton().playSEDecide();
						dgs.CCtrlCodeInterface.instance().setPlayerId(pl.PlayerParty.instance().player(nowPlayerId_).playerId());
						Battle2DManager.instance().helpWindow().updateMessage(110, 0);
						setGetPhase(GET_PHASE.SKILL_UP);
						return true;
					}
					nowPlayerId_++;
					setGetPhase(GET_PHASE.GET_EXP);
					return true;
				}
				return false;
			}

			public bool skillUpPhase(BattleSystem B)
			{
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayerId_);
				if (!battlePlayer.restartWinMotion())
				{
					return false;
				}
				if (B.isEdgeAButtonAndTouchEdge())
				{
					nowPlayerId_++;
					setGetPhase(GET_PHASE.GET_EXP);
					return true;
				}
				return false;
			}

			public bool getItemPahse(BattleSystem B)
			{
				if (B.isEdgeAButtonAndTouchEdge())
				{
					menu.MenuManager.getSingleton().playSEDecide();
					setGetPhase(GET_PHASE.GET_PHASE_END);
					return true;
				}
				return false;
			}

			public bool getPhaseEnd(BattleSystem B)
			{
				BattleBGM.instance().stop(15);
				setGetPhase(GET_PHASE.END);
				return true;
			}

			public bool end(BattleSystem B)
			{
				if (MatrixSound.MtxSoundBGM.getSingleton().getState(MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0) == MatrixSound.enMtxBGMState.enMTX_BGM_STOP)
				{
					B.onEnd();
					setPhase(Phase.Terminate);
					return true;
				}
				return false;
			}

			public void setGetPhase(GET_PHASE phase)
			{
				prevGetPhase_ = getPhase_;
				getPhase_ = phase;
			}

			public BattleWin()
			{
				GetPhaseExecute_ = new GetPhaseExecute[12]
				{
					new GetPhaseExecute(waitFadePhase),
					new GetPhaseExecute(changeBGMPhase),
					new GetPhaseExecute(startFadeInPhase),
					new GetPhaseExecute(enjoyPhase),
					new GetPhaseExecute(windowOpenPhase),
					new GetPhaseExecute(getGoldPhase),
					new GetPhaseExecute(getExpPhase),
					new GetPhaseExecute(levelUpPhase),
					new GetPhaseExecute(skillUpPhase),
					new GetPhaseExecute(getItemPahse),
					new GetPhaseExecute(getPhaseEnd),
					new GetPhaseExecute(end)
				};
			}
		}
	}
}
