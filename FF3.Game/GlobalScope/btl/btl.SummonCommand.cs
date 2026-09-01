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
		public class SummonCommand
		{
			public class SummonCommandState
			{
				public delegate bool _summonCommandState(CommandParameter param);

				public _summonCommandState summonCommandState;

				public SummonCommandState(_summonCommandState arg0)
				{
					summonCommandState = arg0;
				}
			}

			public static SummonCommand instance_ = new SummonCommand();

			public SummonCommandState[] summonCommandState_;

			private TurnSystem turnSystem_;

			private int frame_;

			private int moveFrame_;

			private VecFx32 beforePosition_ = new VecFx32();

			private VecFx32 afterPosition_ = new VecFx32();

			private VecFx32 beforeTarget_ = new VecFx32();

			private VecFx32 afterTarget_ = new VecFx32();

			private bool autoCamera_;

			private int autoCameraFrame_;

			private int autoCameraMoveFrame_;

			public bool setDarkScreen(CommandParameter param)
			{
				stageMng.enableFakeMaterialColor(enable: true, stg.CStageMng.FAKEMATERIAL_TYPE.TYPE_TOON);
				stageMng.setFakeMaterialColor(param.param1_, GX_RGB(param.param2_, param.param3_, param.param4_));
				return true;
			}

			public bool isDarkScreen(CommandParameter param)
			{
				if (stageMng.isChangedFakeMaterialColor())
				{
					return true;
				}
				return false;
			}

			public bool setNormalScreen(CommandParameter param)
			{
				stageMng.setFakeMaterialColor(param.param1_, ds.DS_COLOR_WHITE);
				return true;
			}

			public bool isNormalScreen(CommandParameter param)
			{
				if (stageMng.isChangedFakeMaterialColor())
				{
					stageMng.enableFakeMaterialColor(enable: false, stg.CStageMng.FAKEMATERIAL_TYPE.TYPE_TOON);
					return true;
				}
				return false;
			}

			public bool setFadeOut(CommandParameter param)
			{
				dgs.CFade.Main().fadeOut(param.param2_, static_cast<dgs.CFade.FADE_TYPE>(param.param1_));
				dgs.CFade.Sub().fadeOut(param.param2_, static_cast<dgs.CFade.FADE_TYPE>(param.param1_));
				return true;
			}

			public bool isFadeOut(CommandParameter param)
			{
				if (dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
				{
					return true;
				}
				return false;
			}

			public bool setFadeIn(CommandParameter param)
			{
				dgs.CFade.Main().fadeIn(param.param1_);
				dgs.CFade.Sub().fadeIn(param.param1_);
				return true;
			}

			public bool isFadeIn(CommandParameter param)
			{
				if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
				{
					return true;
				}
				return false;
			}

			public bool setModel(CommandParameter param)
			{
				string arg = "";
				sprintf(out arg, "f%03d", param.param1_);
				int num = characterMng.setCharacterAsync(arg, CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST);
				if (turnSystem() != null)
				{
					turnSystem().characterManager().playerParty().summon()
						.setCharacterMngId(num);
					characterMng.setHidden(num, b: true);
					return true;
				}
				return false;
			}

			public bool isModel(CommandParameter param)
			{
				BattleMonster battleMonster = turnSystem().characterManager().playerParty().summon();
				int ctrl = battleMonster.characterMngId();
				if (characterMng.isLoadedOrgTex(ctrl))
				{
					characterMng.setupOrgTex(ctrl);
					if (turnSystem() != null)
					{
						characterMng.setTransparencyRate(ctrl, 0);
						VecFx32 fnd_reuse_pos = fnd_reuse_pos2;
						fnd_reuse_pos.x = mon.MonsterManager.instance().offset(battleMonster.monsterId()).scale();
						fnd_reuse_pos.y = mon.MonsterManager.instance().offset(battleMonster.monsterId()).scale();
						fnd_reuse_pos.z = mon.MonsterManager.instance().offset(battleMonster.monsterId()).scale();
						characterMng.setScale(ctrl, fnd_reuse_pos);
						return true;
					}
				}
				return false;
			}

			public bool setMotion(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					int ctrl = turnSystem().characterManager().playerParty().summon()
						.characterMngId();
					string arg = "";
					sprintf(out arg, "b_sm%03d", param.param1_);
					characterMng.addMotionAsync(ctrl, arg);
					return true;
				}
				return false;
			}

			public bool isMotion(CommandParameter param)
			{
				if (!characterMng.isLoadingMotionAsync())
				{
					return true;
				}
				return false;
			}

			public bool startMotion(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					int ctrl = turnSystem().characterManager().playerParty().summon()
						.characterMngId();
					characterMng.startMotion(ctrl, param.param1_, param.param2_ != 0, (uint)param.param3_);
					return true;
				}
				return false;
			}

			public bool isMotionFrame(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					int ctrl = turnSystem().characterManager().playerParty().summon()
						.characterMngId();
					if (characterMng.getCurrentFrame(ctrl) == param.param1_)
					{
						return true;
					}
				}
				return false;
			}

			public bool delSummon(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					int ctrl = turnSystem().characterManager().playerParty().summon()
						.characterMngId();
					characterMng.delCharacter(ctrl);
					turnSystem().characterManager().playerParty().summon()
						.setCharacterMngId(-1);
					return true;
				}
				return false;
			}

			public bool registerPlayers(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					if (turnSystem().isPlayerEscape())
					{
						turnSystem().characterManager().playerParty().setDeleteEffectFlag(flag: true);
					}
					turnSystem().characterManager().playerParty().registerCharacterMng();
					turnSystem().characterManager().playerParty().initializePlayerPosition();
					for (int i = 0; i < 4; i++)
					{
						turnSystem().characterManager().playerParty().battlePlayer(i)
							.changeCondition()
							.clearCondition();
					}
					return true;
				}
				return false;
			}

			public bool setEffect(CommandParameter param)
			{
				BattleEffect.instance().addEfp(param.param1_);
				return true;
			}

			public bool clearEffect(CommandParameter param)
			{
				BattleEffect.instance().endEfp();
				return true;
			}

			public bool isEffect(CommandParameter param)
			{
				if (TexDivideLoader.getSingleton().tdlIsEmpty())
				{
					return true;
				}
				return false;
			}

			public bool drawSummonEffect(CommandParameter param)
			{
				int num = BattleEffect.instance().create(param.param1_, param.param2_);
				if (num == -1)
				{
					return false;
				}
				BattleMonster target = turnSystem().characterManager().playerParty().summon();
				turnSystem().setHitEffectPosition(target, num, (short)param.param3_, param.param4_);
				return true;
			}

			public bool drawSummonEffectTargetAll(CommandParameter param)
			{
				int num = BattleEffect.instance().create(param.param1_, param.param2_);
				if (num == -1)
				{
					return false;
				}
				BattleMonster target = turnSystem().characterManager().playerParty().summon();
				turnSystem().setEffectPosition(target, num, TargetAllMagicPosition[param.param3_]);
				return true;
			}

			public bool isEndSummonEffect(CommandParameter param)
			{
				BattleMonster battleMonster = turnSystem().characterManager().playerParty().summon();
				return battleMonster.isClearAllEffect();
			}

			public bool chengeCamera(CommandParameter param)
			{
				battleDisplay.getBattleCamera().setPosition(param.param1_, param.param2_, param.param3_);
				battleDisplay.getBattleCamera().setTarget(param.param4_, param.param5_, param.param6_);
				return true;
			}

			public bool setCameraPosition(CommandParameter param)
			{
				beforePosition_.x = param.param1_;
				beforePosition_.y = param.param2_;
				beforePosition_.z = param.param3_;
				afterPosition_.x = param.param4_;
				afterPosition_.y = param.param5_;
				afterPosition_.z = param.param6_;
				return true;
			}

			public bool setCameraTarget(CommandParameter param)
			{
				beforeTarget_.x = param.param1_;
				beforeTarget_.y = param.param2_;
				beforeTarget_.z = param.param3_;
				afterTarget_.x = param.param4_;
				afterTarget_.y = param.param5_;
				afterTarget_.z = param.param6_;
				return true;
			}

			public bool setMoveCameraFrame(CommandParameter param)
			{
				frame_ = param.param1_;
				return true;
			}

			public bool moveCamera(CommandParameter param)
			{
				frame_--;
				battleDisplay.getBattleCamera().setPosition(battleDisplay.calcCamera(battleDisplay.getBattleCamera().getPosition(), afterPosition_, beforePosition_, param.param1_));
				battleDisplay.getBattleCamera().setTarget(battleDisplay.calcCamera(battleDisplay.getBattleCamera().getTarget(), afterTarget_, beforeTarget_, param.param1_));
				if (frame_ <= 0)
				{
					frame_ = 0;
					battleDisplay.getBattleCamera().setPosition(afterPosition_);
					battleDisplay.getBattleCamera().setTarget(afterTarget_);
					return true;
				}
				return false;
			}

			public bool setBattleCamera(CommandParameter param)
			{
				battleDisplay.setBattleCameraPositionAndTarget();
				return true;
			}

			public bool setSummonParameter(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					BattleMonster battleMonster = turnSystem().characterManager().playerParty().summon();
					battleMonster.initialize();
					battleMonster.setMonsterId((short)param.param1_);
					battleMonster.setBreed(1);
					battleMonster.setMonster(mon.MonsterManager.instance().monsterParameter(battleMonster.monsterId()));
					battleMonster.clearEffectIdAll();
					battleMonster.setCondition(battleMonster.monsterCondition());
					return true;
				}
				return false;
			}

			public bool setPositionAndRotation(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					int ctrl = turnSystem().characterManager().playerParty().summon()
						.characterMngId();
					characterMng.setPosition(ctrl, param.param1_, param.param2_, param.param3_);
					characterMng.setRotation(ctrl, 0, static_cast<ushort>(ds.DEGto65536(param.param4_)), 0);
					return true;
				}
				return false;
			}

			public bool showMonster(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					turnSystem().characterManager().monsterParty().hideMonster(hide: false);
					return true;
				}
				return false;
			}

			public bool hideMonster(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					turnSystem().characterManager().monsterParty().hideMonster(hide: true);
					return true;
				}
				return false;
			}

			public bool showSummon(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					int ctrl = turnSystem().characterManager().playerParty().summon()
						.characterMngId();
					characterMng.setHidden(ctrl, b: false);
					return true;
				}
				return false;
			}

			public bool hideSummon(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					int ctrl = turnSystem().characterManager().playerParty().summon()
						.characterMngId();
					characterMng.setHidden(ctrl, b: true);
					return true;
				}
				return false;
			}

			public bool calculationSummonAlphaRate(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					int ctrl = turnSystem().characterManager().playerParty().summon()
						.characterMngId();
					int transparencyRate = characterMng.getTransparencyRate(ctrl);
					transparencyRate += ((param.param2_ == 0) ? param.param1_ : (param.param1_ / param.param2_));
					transparencyRate = ds.clamp(transparencyRate, 0, 100);
					characterMng.setTransparencyRate(ctrl, transparencyRate);
					if (transparencyRate == 0 || transparencyRate == 100)
					{
						return true;
					}
					return false;
				}
				return false;
			}

			public bool setPlayersAlpha(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					turnSystem().characterManager().playerParty().setAlpha(param.param1_, param.param2_);
					return true;
				}
				return false;
			}

			public bool appearPlayers(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					if (turnSystem().isPlayerEscape())
					{
						return true;
					}
					if (turnSystem().characterManager().playerParty().appear(param.param1_))
					{
						return true;
					}
				}
				return false;
			}

			public bool setTurnFlag(CommandParameter param)
			{
				if (param.param1_ != 0)
				{
					turnSystem().setCheckFlag(TurnSystem.EndEffectProcess);
				}
				if (param.param2_ != 0)
				{
					turnSystem().setCheckFlag(TurnSystem.End2DProcess);
				}
				if (param.param3_ != 0)
				{
					turnSystem().setCheckFlag(TurnSystem.EndPlayerProcess);
				}
				if (param.param4_ != 0)
				{
					turnSystem().setCheckFlag(TurnSystem.EndEnemyProcess);
				}
				return true;
			}

			public bool isTurnFlag(CommandParameter param)
			{
				return true;
			}

			public bool moveCameraAndSummonAlpha(CommandParameter param)
			{
				bool flag = false;
				frame_--;
				battleDisplay.getBattleCamera().setPosition(battleDisplay.calcCamera(battleDisplay.getBattleCamera().getPosition(), afterPosition_, beforePosition_, param.param3_));
				battleDisplay.getBattleCamera().setTarget(battleDisplay.calcCamera(battleDisplay.getBattleCamera().getTarget(), afterTarget_, beforeTarget_, param.param3_));
				if (frame_ <= 0)
				{
					frame_ = 0;
					battleDisplay.getBattleCamera().setPosition(afterPosition_);
					battleDisplay.getBattleCamera().setTarget(afterTarget_);
					flag = true;
				}
				bool flag2 = false;
				if (turnSystem() != null)
				{
					int ctrl = turnSystem().characterManager().playerParty().summon()
						.characterMngId();
					int transparencyRate = characterMng.getTransparencyRate(ctrl);
					transparencyRate += param.param1_ / param.param2_;
					transparencyRate = ds.clamp(transparencyRate, 0, 100);
					characterMng.setTransparencyRate(ctrl, transparencyRate);
					flag2 = ((transparencyRate == 0 || transparencyRate == 100) ? true : false);
				}
				if (!flag || !flag2)
				{
					return false;
				}
				return true;
			}

			public bool frameCount(CommandParameter param)
			{
				if (frame_++ > param.param1_)
				{
					frame_ = 0;
					return true;
				}
				return false;
			}

			public bool set2D(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					turnSystem().drawMagic2D();
					return true;
				}
				return false;
			}

			public bool is2D(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					return turnSystem().checkEnd2D();
				}
				return false;
			}

			public bool deadCharacters(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					return turnSystem().deadCharacters(turnSystem().nowCharacter());
				}
				return false;
			}

			public bool summonBehaviorEnd(CommandParameter param)
			{
				return true;
			}

			public bool readyShakeCamera(CommandParameter param)
			{
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
				fnd_reuse_pos.x = param.param2_;
				fnd_reuse_pos.y = param.param3_;
				fnd_reuse_pos.z = param.param4_;
				battleDisplay.readyShakeCamera(param.param1_, fnd_reuse_pos);
				return true;
			}

			public bool rotateCharacter(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					int ctrl = turnSystem().characterManager().playerParty().summon()
						.characterMngId();
					characterMng.setRotation(ctrl, ds.DEGto65536(param.param1_), ds.DEGto65536(param.param2_), ds.DEGto65536(param.param3_));
					return true;
				}
				return false;
			}

			public bool readyMoveCharacter(CommandParameter param)
			{
				VEC_Set(afterPosition_, param.param1_, param.param2_, param.param3_);
				int ctrl = turnSystem().characterManager().playerParty().summon()
					.characterMngId();
				characterMng.getPosition(ctrl, beforePosition_);
				moveFrame_ = param.param4_;
				return true;
			}

			public bool moveCharacter(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					moveFrame_--;
					VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos2;
					VecFx32 fnd_reuse_pos2 = GlobalScope.fnd_reuse_pos;
					int ctrl = turnSystem().characterManager().playerParty().summon()
						.characterMngId();
					characterMng.getPosition(ctrl, fnd_reuse_pos2);
					VEC_Subtract(afterPosition_, beforePosition_, fnd_reuse_pos);
					fnd_reuse_pos2.x += fnd_reuse_pos.x / param.param1_;
					fnd_reuse_pos2.y += fnd_reuse_pos.y / param.param1_;
					fnd_reuse_pos2.z += fnd_reuse_pos.z / param.param1_;
					characterMng.setPosition(ctrl, fnd_reuse_pos2);
					if (moveFrame_ <= 0)
					{
						moveFrame_ = 0;
						characterMng.setPosition(ctrl, afterPosition_);
						return true;
					}
				}
				return false;
			}

			public bool setFlash(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					turnSystem().setFlash((byte)param.param1_, (short)param.param2_, (short)param.param3_);
					return true;
				}
				return false;
			}

			public bool readyAutoCamera(CommandParameter param)
			{
				autoCamera_ = true;
				autoCameraFrame_ = frame_;
				autoCameraMoveFrame_ = autoCameraFrame_;
				return true;
			}

			public bool isAutoCamera(CommandParameter param)
			{
				return autoCamera_;
			}

			public bool drawTargetEffect(CommandParameter param)
			{
				return turnSystem().drawOnceMagicEffect(param.param1_);
			}

			public bool isShakeCamera(CommandParameter param)
			{
				if (battleDisplay.shakeFrame() < 0)
				{
					return true;
				}
				return false;
			}

			public bool setShowPlayerWindow(CommandParameter param)
			{
				turnSystem().playerWindow().show(param.param1_ != 0);
				return true;
			}

			public bool createEffectAndSetPosition(CommandParameter param)
			{
				BattleMonster character = turnSystem().characterManager().playerParty().summon();
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
				fnd_reuse_pos.x = param.param3_;
				fnd_reuse_pos.y = param.param4_;
				fnd_reuse_pos.z = param.param5_;
				return turnSystem().createEffectAndSetPosition(character, param.param1_, param.param2_, fnd_reuse_pos);
			}

			public bool isEndMonsterEffect(CommandParameter param)
			{
				for (int i = 0; i < 6; i++)
				{
					BattleMonster battleMonster = turnSystem().characterManager().monsterParty().battleMonster(i);
					if (battleMonster != null && !battleMonster.isClearAllEffect())
					{
						return false;
					}
				}
				return true;
			}

			public bool isEndPlayerEffect(CommandParameter param)
			{
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer = turnSystem().characterManager().playerParty().battlePlayer(i);
					if (battlePlayer != null && !battlePlayer.isClearAllEffect())
					{
						return false;
					}
				}
				return true;
			}

			public bool setMonstersAlpha(CommandParameter param)
			{
				if (turnSystem() != null)
				{
					turnSystem().characterManager().monsterParty().setAlpha(param.param1_, param.param2_);
					return true;
				}
				return false;
			}

			public bool appearMonsters(CommandParameter param)
			{
				if (turnSystem() != null && turnSystem().characterManager().monsterParty().appear(param.param1_))
				{
					return true;
				}
				return false;
			}

			public bool disappearMonsters(CommandParameter param)
			{
				if (turnSystem() != null && turnSystem().characterManager().monsterParty().disappear(param.param1_))
				{
					return true;
				}
				return false;
			}

			public bool setSE(CommandParameter param)
			{
				BattleSE.instance().loadNew(param.param1_);
				return true;
			}

			public bool playSE(CommandParameter param)
			{
				BattleSE.instance().play(param.param1_, param.param2_);
				return true;
			}

			public bool clearSE(CommandParameter param)
			{
				BattleSE.instance().freeNew();
				return true;
			}

			public bool execute(CommandParameter param, int current)
			{
				return summonCommandState_[current].summonCommandState(param);
			}

			public void executeAutoCamera()
			{
				if (autoCamera_)
				{
					autoCameraFrame_--;
					battleDisplay.getBattleCamera().setPosition(battleDisplay.calcCamera(battleDisplay.getBattleCamera().getPosition(), afterPosition_, beforePosition_, autoCameraMoveFrame_));
					battleDisplay.getBattleCamera().setTarget(battleDisplay.calcCamera(battleDisplay.getBattleCamera().getTarget(), afterTarget_, beforeTarget_, autoCameraMoveFrame_));
					if (autoCameraFrame_ <= 0)
					{
						autoCameraFrame_ = 0;
						autoCameraMoveFrame_ = 0;
						autoCamera_ = false;
						battleDisplay.getBattleCamera().setPosition(afterPosition_);
						battleDisplay.getBattleCamera().setTarget(afterTarget_);
					}
				}
			}

			public void initializeCommand()
			{
				VEC_Set(beforePosition_, 0, 0, 0);
				VEC_Set(afterPosition_, 0, 0, 0);
				VEC_Set(beforeTarget_, 0, 0, 0);
				VEC_Set(afterTarget_, 0, 0, 0);
				frame_ = 0;
				moveFrame_ = 0;
				autoCameraFrame_ = 0;
				autoCameraMoveFrame_ = 0;
				autoCamera_ = false;
			}

			public SummonCommand()
			{
				turnSystem_ = null;
				summonCommandState_ = new SummonCommandState[64]
				{
					new SummonCommandState(setDarkScreen),
					new SummonCommandState(isDarkScreen),
					new SummonCommandState(setNormalScreen),
					new SummonCommandState(isNormalScreen),
					new SummonCommandState(setFadeOut),
					new SummonCommandState(isFadeOut),
					new SummonCommandState(setFadeIn),
					new SummonCommandState(isFadeIn),
					new SummonCommandState(setModel),
					new SummonCommandState(isModel),
					new SummonCommandState(setMotion),
					new SummonCommandState(isMotion),
					new SummonCommandState(startMotion),
					new SummonCommandState(isMotionFrame),
					new SummonCommandState(delSummon),
					new SummonCommandState(registerPlayers),
					new SummonCommandState(setEffect),
					new SummonCommandState(clearEffect),
					new SummonCommandState(isEffect),
					new SummonCommandState(drawSummonEffect),
					new SummonCommandState(drawSummonEffectTargetAll),
					new SummonCommandState(isEndSummonEffect),
					new SummonCommandState(chengeCamera),
					new SummonCommandState(setCameraPosition),
					new SummonCommandState(setCameraTarget),
					new SummonCommandState(setMoveCameraFrame),
					new SummonCommandState(moveCamera),
					new SummonCommandState(setBattleCamera),
					new SummonCommandState(setSummonParameter),
					new SummonCommandState(setPositionAndRotation),
					new SummonCommandState(showMonster),
					new SummonCommandState(hideMonster),
					new SummonCommandState(showSummon),
					new SummonCommandState(hideSummon),
					new SummonCommandState(calculationSummonAlphaRate),
					new SummonCommandState(setPlayersAlpha),
					new SummonCommandState(appearPlayers),
					new SummonCommandState(setTurnFlag),
					new SummonCommandState(isTurnFlag),
					new SummonCommandState(moveCameraAndSummonAlpha),
					new SummonCommandState(frameCount),
					new SummonCommandState(set2D),
					new SummonCommandState(is2D),
					new SummonCommandState(deadCharacters),
					new SummonCommandState(summonBehaviorEnd),
					new SummonCommandState(readyShakeCamera),
					new SummonCommandState(rotateCharacter),
					new SummonCommandState(readyMoveCharacter),
					new SummonCommandState(moveCharacter),
					new SummonCommandState(setFlash),
					new SummonCommandState(readyAutoCamera),
					new SummonCommandState(isAutoCamera),
					new SummonCommandState(drawTargetEffect),
					new SummonCommandState(isShakeCamera),
					new SummonCommandState(setShowPlayerWindow),
					new SummonCommandState(createEffectAndSetPosition),
					new SummonCommandState(isEndMonsterEffect),
					new SummonCommandState(isEndPlayerEffect),
					new SummonCommandState(setMonstersAlpha),
					new SummonCommandState(appearMonsters),
					new SummonCommandState(disappearMonsters),
					new SummonCommandState(setSE),
					new SummonCommandState(playSE),
					new SummonCommandState(clearSE)
				};
			}

			public static SummonCommand getSingleton()
			{
				return instance_;
			}

			public void setTurnSystem(TurnSystem system)
			{
				turnSystem_ = system;
			}

			public TurnSystem turnSystem()
			{
				return turnSystem_;
			}
		}
	}
}
