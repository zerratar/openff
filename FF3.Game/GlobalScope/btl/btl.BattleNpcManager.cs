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
		public class BattleNpcManager
		{
			public enum NPC_STATE
			{
				NPC_START,
				NPC_WINDOW_CREATING,
				PLAYER_DISAPPEAR,
				IS_NPC_DATA,
				NPC_APPEAR,
				NPC_MAGIC_ACTION,
				NPC_DISAPPEAR,
				IS_PLAYER_DATA,
				PLAYER_APPEAR,
				NPC_MAGIC_ATTACK_START,
				NPC_MAGIC_ATTACK,
				NPC_NORMAL_ATTACK_START,
				NPC_NORMAL_ATTACK,
				NPC_END,
				NPC_STATE_MAX
			}

			public enum NPC_ATTACK_TYPE
			{
				NORMAL_ATTACK,
				MAGIC_ATTACK,
				NPC_ATTACK_TYPE_MAX
			}

			public enum NPC_TARGET_TYPE
			{
				PLAYER,
				ENEMY,
				NPC_TARGET_TYPE_MAX
			}

			public class NpcState
			{
				public delegate void _npcState(BattleSystem B);

				public _npcState npcState;

				public NpcState(_npcState arg0)
				{
					npcState = arg0;
				}
			}

			public const NPC_STATE NPC_START = NPC_STATE.NPC_START;

			public const NPC_STATE NPC_WINDOW_CREATING = NPC_STATE.NPC_WINDOW_CREATING;

			public const NPC_STATE PLAYER_DISAPPEAR = NPC_STATE.PLAYER_DISAPPEAR;

			public const NPC_STATE IS_NPC_DATA = NPC_STATE.IS_NPC_DATA;

			public const NPC_STATE NPC_APPEAR = NPC_STATE.NPC_APPEAR;

			public const NPC_STATE NPC_MAGIC_ACTION = NPC_STATE.NPC_MAGIC_ACTION;

			public const NPC_STATE NPC_DISAPPEAR = NPC_STATE.NPC_DISAPPEAR;

			public const NPC_STATE IS_PLAYER_DATA = NPC_STATE.IS_PLAYER_DATA;

			public const NPC_STATE PLAYER_APPEAR = NPC_STATE.PLAYER_APPEAR;

			public const NPC_STATE NPC_MAGIC_ATTACK_START = NPC_STATE.NPC_MAGIC_ATTACK_START;

			public const NPC_STATE NPC_MAGIC_ATTACK = NPC_STATE.NPC_MAGIC_ATTACK;

			public const NPC_STATE NPC_NORMAL_ATTACK_START = NPC_STATE.NPC_NORMAL_ATTACK_START;

			public const NPC_STATE NPC_NORMAL_ATTACK = NPC_STATE.NPC_NORMAL_ATTACK;

			public const NPC_STATE NPC_END = NPC_STATE.NPC_END;

			public const NPC_STATE NPC_STATE_MAX = NPC_STATE.NPC_STATE_MAX;

			public const NPC_TARGET_TYPE PLAYER = NPC_TARGET_TYPE.PLAYER;

			public const NPC_TARGET_TYPE ENEMY = NPC_TARGET_TYPE.ENEMY;

			public const NPC_TARGET_TYPE NPC_TARGET_TYPE_MAX = NPC_TARGET_TYPE.NPC_TARGET_TYPE_MAX;

			public NpcState[] npcState_;

			private static int WINDOW_DRAW_FRAME_MAX = 60;

			private static int NPC_DISAPPEAR_FRAME_MAX = 10;

			private static int ALPHA_RATE_MAX = 100;

			private static int SHADOW_ALPHA_RATE_MAX = 31;

			private static int NORMAL_ATTACK_ID = 1;

			private NPC_STATE state_;

			private bool end_;

			private int counter_;

			private int alphaRate_;

			private int shadowAlphaRate_;

			private BattlePlayer npc_;

			private int nowNpcId_;

			private int attackType_;

			private int attackArea_;

			private byte target_;

			private short effectWaitCounter_;

			private short helpWindowCounter_;

			private ys.PhysicsAttackParameter invalidParameter = new ys.PhysicsAttackParameter();

			private ys.PhysicsAttackParameter[] attackParameter_ = new ys.PhysicsAttackParameter[2];

			private ys.Condition condition_ = new ys.Condition();

			private int[] weaponMngId_ = new int[2];

			private TurnSystem turnSystem_ = new TurnSystem();

			public void setup()
			{
				turnSystem_.initializeAll();
				offEnd();
				setupState();
				npc_ = null;
				helpWindowCounter_ = 0;
				counter_ = 0;
				shadowAlphaRate_ = 0;
				attackType_ = 0;
				attackArea_ = 0;
				target_ = 0;
				effectWaitCounter_ = 0;
				nowNpcId_ = -1;
				invalidParameter.aggressivity().set(0);
				invalidParameter.hitProbability_set(0);
				invalidParameter.optionProbability_set(0);
				invalidParameter.optionMagicId_set(0);
				invalidParameter.armsAttribute_set(0);
				invalidParameter.attackType_set(0);
				invalidParameter.attackOption_set(0);
				invalidParameter.equipOption_set(0);
				attackParameter_[0] = invalidParameter;
				attackParameter_[1] = invalidParameter;
				weaponMngId_[0] = -1;
				weaponMngId_[1] = -1;
				condition_.clearCondition();
			}

			public void cleanup()
			{
			}

			public void execute(BattleSystem B)
			{
				npcState_[(int)state_].npcState(B);
				if (npc_ != null)
				{
					npc_.checkClearEffectId();
				}
			}

			public void setNpcParameter(BattleSystem B)
			{
				npc_ = B.characterManager().playerParty().battleNpc();
				npc_.initialize();
				npc_.onIsEnable();
				npc_.setBattleCharacterId(B.characterManager().characterNumber());
				npc_.setActionNumber(1);
				BattleNpcData battleNpcData = B.npcDataManager().npcData(nowNpcId());
				npc_.setLevel(battleNpcData.level());
				npc_.setWeight(battleNpcData.weight());
				npc_.setBreed(battleNpcData.breed());
				npc_.setBody(battleNpcData.body());
				npc_.setBodyAndBonus(battleNpcData.body());
				npc_.setWeaponId(pl.HAND_TYPE.RIGHT_HAND, battleNpcData.weaponId(0));
				OS_Printf("NPC右手装備番号 %d\n", npc_.weaponId(pl.HAND_TYPE.RIGHT_HAND));
				npc_.setWeaponId(pl.HAND_TYPE.LEFT_HAND, battleNpcData.weaponId(1));
				OS_Printf("NPC左手装備番号 %d\n", npc_.weaponId(pl.HAND_TYPE.LEFT_HAND));
				setAttack(pl.HAND_TYPE.RIGHT_HAND);
				setAttack(pl.HAND_TYPE.LEFT_HAND);
				npc_.setCondition(condition_);
				npc_.phaseInitialize();
				npc_.clearFlagAll();
				npc_.setReflectTargetId(-1);
			}

			public void setAttack(pl.HAND_TYPE type)
			{
				itm.WeaponParameter weaponParameter = itm.ItemManager.instance().weaponParameter(npc_.weaponId(type));
				if (weaponParameter != null)
				{
					attackParameter_[(int)type].aggressivity().set(weaponParameter.aggressivity());
					attackParameter_[(int)type].hitProbability_set(weaponParameter.hitProbability());
					attackParameter_[(int)type].optionProbability_set(weaponParameter.optionProbability());
					attackParameter_[(int)type].optionMagicId_set(weaponParameter.optionMagicItemId());
					attackParameter_[(int)type].armsAttribute_set(weaponParameter.armsAttribute());
					attackParameter_[(int)type].attackType_set(weaponParameter.atckType());
					attackParameter_[(int)type].attackOption_set(weaponParameter.atckOption());
					attackParameter_[(int)type].equipOption_set(weaponParameter.equipOption());
					npc_.setHandAttack(type, attackParameter_[(int)type]);
				}
				else
				{
					npc_.setHandAttack(type, invalidParameter);
				}
			}

			public void setAttackType(BattleNpcData data)
			{
				if (data == null)
				{
					return;
				}
				int num = (int)ds.RandomNumber.rand32(101u);
				short num2 = 0;
				int i;
				for (i = 0; i < NPC_ATTACK_MAX; i++)
				{
					if (data.npcAttack(i).attackId_ > 0)
					{
						if (num <= data.npcAttack(i).attackOdds_ + num2)
						{
							break;
						}
						num2 += data.npcAttack(i).attackOdds_;
					}
				}
				if (pl.PlayerParty.instance().isLilliputAll())
				{
					attackType_ = data.npcAttack(1).attackId_;
					attackArea_ = data.npcAttack(1).attackArea_;
					target_ = data.npcAttack(1).target_;
				}
				else
				{
					attackType_ = data.npcAttack(i).attackId_;
					attackArea_ = data.npcAttack(i).attackArea_;
					target_ = data.npcAttack(i).target_;
				}
			}

			public void setNpcTarget(BattleSystem B)
			{
				if (attackType_ == NORMAL_ATTACK_ID)
				{
					setNpcTargetNormalAttack(B);
				}
				else
				{
					setNpcTargetMagic(B);
				}
			}

			public void setNpcTargetNormalAttack(BattleSystem B)
			{
				turnSystem_.setTargetRandam(npc_, B.characterManager().monsterParty(), reflect: true);
			}

			public void setNpcTargetMagic(BattleSystem B)
			{
				if (target_ == 0)
				{
					if (attackArea_ != 0)
					{
						B.characterManager().setPlayerAllTarget(npc_, 0);
					}
					else
					{
						B.characterManager().setPlayerAllTarget(npc_, 0);
					}
				}
				else if (attackArea_ != 0)
				{
					B.characterManager().setMonsterAllTarget(npc_);
				}
				else
				{
					turnSystem_.setTargetRandam(npc_, B.characterManager().monsterParty(), reflect: true);
				}
			}

			public void initializeNormalAttack(BattleSystem B)
			{
				npc_.setAttackSuccess(0, flag: false);
				npc_.setAttackSuccess(1, flag: false);
				turnSystem_.calcNormalAttackDamage(npc_);
				turnSystem_.setNormalAttackDamage(npc_);
				npc_.addEquipWeaponMotionForNpc(pl.HAND_TYPE.RIGHT_HAND);
				npc_.addEquipWeaponMotionForNpc(pl.HAND_TYPE.LEFT_HAND);
				characterMng.startMotion(npc_.characterMngId(), 201, fLoop: true, 0u);
				if (B.characterManager().monsterParty().aliveNumber() != 0)
				{
					npc_.clearFlag(PLAYER_FLAG.PF_FINISH);
				}
				else
				{
					npc_.setFlag(PLAYER_FLAG.PF_FINISH);
				}
				BattleSE.instance().load(201);
				registerNormalAttackEfp(B, pl.HAND_TYPE.RIGHT_HAND);
				registerNormalAttackEfp(B, pl.HAND_TYPE.LEFT_HAND);
			}

			public void registerNormalAttackEfp(BattleSystem B, pl.HAND_TYPE handType)
			{
				BattleNpcData battleNpcData = B.npcDataManager().npcData(nowNpcId());
				itm.WEAPON_SYSTEM weapon = (itm.WEAPON_SYSTEM)(itm.ItemManager.instance().weaponParameter(battleNpcData.weaponId((int)handType))?.system() ?? 20);
				int motionId = npc_.equipWeaponMotionIndex(weapon) + 1;
				pl.PlayerNormalAttackParameter playerNormalAttackParameter = pl.PlayerParty.instance().normalAttack(motionId);
				int category_ = playerNormalAttackParameter.effect(0).category_;
				BattleEffect.instance().addEfp(category_);
			}

			public void initializeNormalMagic(BattleSystem B)
			{
				OutsideToBattle.getInstance().onTransfix();
				npc_.setUseMagicId((short)attackType());
				characterMng.addMotion(npc_.characterMngId(), "b_b02_040");
				characterMng.startMotion(npc_.characterMngId(), 4001, fLoop: true, 0u);
				pl.PlayerNormalMagicParameter playerNormalMagicParameter = pl.PlayerParty.instance().normalMagic(attackType());
				if (playerNormalMagicParameter != null)
				{
					int category_ = playerNormalMagicParameter.effect().category_;
					BattleEffect.instance().addEfp(category_);
				}
				if (playerNormalMagicParameter != null)
				{
					int category_2 = playerNormalMagicParameter.se().category_;
					BattleSE.instance().load(category_2);
				}
				turnSystem_.setMagicStartEffect(attackType());
			}

			public void npcStart(BattleSystem B)
			{
				bool flag = false;
				if (pl.PlayerParty.instance().isFrogAll())
				{
					setState(NPC_STATE.NPC_END);
					return;
				}
				if (OutsideToBattle.getInstance().battleOpeningType() == BATTLE_OPENING_TYPE.BACK_ATTACK)
				{
					setState(NPC_STATE.NPC_END);
					return;
				}
				if (B.turnCount_ % 2 == 0 && pl.PlayerParty.instance().npc().isEnable() && B.npcDataManager().npcData(pl.PlayerParty.instance().npc().npcId()) != null)
				{
					int num = (int)ds.RandomNumber.rand32(101u);
					if (num <= NPC_APPEAR_PROBABILITY)
					{
						flag = true;
						setNowNpcId(pl.PlayerParty.instance().npc().npcId());
					}
				}
				if (!flag)
				{
					int num2 = OutsideToBattle.getInstance().initializeNPC().npcId();
					if (num2 > -1 && B.npcDataManager().npcData(num2) != null)
					{
						flag = true;
						setNowNpcId(num2);
					}
				}
				if (flag)
				{
					setNpcParameter(B);
					alphaRate_ = ALPHA_RATE_MAX;
					shadowAlphaRate_ = SHADOW_ALPHA_RATE_MAX;
					setState(NPC_STATE.PLAYER_DISAPPEAR);
				}
				else
				{
					setState(NPC_STATE.NPC_END);
				}
			}

			public void npcCreatingWindow(BattleSystem B)
			{
				counter_--;
				if (counter_ <= 0)
				{
					counter_ = 0;
					Battle2DManager.instance().helpWindow().releaseHelpWindow();
					setState(NPC_STATE.NPC_APPEAR);
				}
			}

			public void playerDisappear(BattleSystem B)
			{
				alphaRate_ -= ALPHA_RATE_MAX / NPC_DISAPPEAR_FRAME_MAX;
				shadowAlphaRate_ -= SHADOW_ALPHA_RATE_MAX / NPC_DISAPPEAR_FRAME_MAX;
				if (alphaRate_ < 0)
				{
					alphaRate_ = 0;
				}
				if (shadowAlphaRate_ < 0)
				{
					shadowAlphaRate_ = 0;
				}
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(i);
					if (battlePlayer.isEnable())
					{
						characterMng.setTransparencyRate(battlePlayer.characterMngId(), alphaRate_);
						if (battlePlayer.itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_ >= 0)
						{
							characterMng.setTransparencyRate(battlePlayer.itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_, alphaRate_);
						}
						if (battlePlayer.itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_ >= 0)
						{
							characterMng.setTransparencyRate(battlePlayer.itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_, alphaRate_);
						}
						characterMng.setShadowAlphaRate(battlePlayer.characterMngId(), shadowAlphaRate_);
					}
				}
				if (alphaRate_ != 0)
				{
					return;
				}
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer2 = B.characterManager().playerParty().battlePlayer(i);
					if (battlePlayer2.isEnable())
					{
						battlePlayer2.deleteConditionEffect();
					}
				}
				string arg = "";
				sprintf(out arg, "n%d", B.npcDataManager().npcData(nowNpcId()).modelId());
				npc_.setCharacterMngId(characterMng.setCharacter(arg, CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST));
				characterMng.releaseMdlTexRes(npc_.characterMngId());
				characterMng.setShadowType(npc_.characterMngId(), 1);
				characterMng.setTransparencyRate(npc_.characterMngId(), 0);
				characterMng.setShadowAlphaRate(npc_.characterMngId(), 0);
				characterMng.setRotation(npc_.characterMngId(), 0, PlayerRotationY, 0);
				characterMng.setPosition(npc_.characterMngId(), EffectedNpcPosition);
				npc_.setIdleType(1);
				npc_.registerWeapon(pl.HAND_TYPE.RIGHT_HAND);
				npc_.registerWeapon(pl.HAND_TYPE.LEFT_HAND);
				if (npc_.itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_ >= 0)
				{
					characterMng.setTransparencyRate(npc_.itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_, 0);
				}
				if (npc_.itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_ >= 0)
				{
					characterMng.setTransparencyRate(npc_.itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_, 0);
				}
				if (pl.PlayerParty.instance().isLilliputAll())
				{
					npc_.changeLilliput(flag: true);
				}
				setAttackType(B.npcDataManager().npcData(nowNpcId()));
				setNpcTarget(B);
				turnSystem_.setNowCharacter(npc_);
				turnSystem_.setCharacterManager(B.characterManager());
				turnSystem_.initializeTurn();
				turnSystem_.playerTurn_.clearFlagAll();
				characterMng.addMotion(npc_.characterMngId(), "b_b01");
				if (attackType() == NORMAL_ATTACK_ID)
				{
					initializeNormalAttack(B);
				}
				else
				{
					initializeNormalMagic(B);
				}
				setState(NPC_STATE.IS_NPC_DATA);
			}

			public void npcIsData(BattleSystem B)
			{
				if (!TexDivideLoader.getSingleton().tdlIsEmpty())
				{
					return;
				}
				int num = 0;
				for (int i = 0; i < NPC_MESSAGE_MAX; i++)
				{
					if (B.npcDataManager().npcData(nowNpcId()).messageId(i) > 0)
					{
						num++;
					}
				}
				num = (int)ds.RandomNumber.rand32((uint)num);
				int id = B.npcDataManager().npcData(nowNpcId()).messageId(num);
				Battle2DManager.instance().helpWindow().setMsdHandle(0);
				Battle2DManager.instance().helpWindow().createHelpWindow(id, 0, 0);
				counter_ = 30;
				setState(NPC_STATE.NPC_WINDOW_CREATING);
			}

			public void npcAppear(BattleSystem B)
			{
				alphaRate_ += ALPHA_RATE_MAX / NPC_DISAPPEAR_FRAME_MAX;
				shadowAlphaRate_ += SHADOW_ALPHA_RATE_MAX / NPC_DISAPPEAR_FRAME_MAX;
				if (alphaRate_ > ALPHA_RATE_MAX)
				{
					alphaRate_ = ALPHA_RATE_MAX;
				}
				if (shadowAlphaRate_ > SHADOW_ALPHA_RATE_MAX)
				{
					shadowAlphaRate_ = SHADOW_ALPHA_RATE_MAX;
				}
				characterMng.setTransparencyRate(npc_.characterMngId(), alphaRate_);
				characterMng.setShadowAlphaRate(npc_.characterMngId(), shadowAlphaRate_);
				if (alphaRate_ != ALPHA_RATE_MAX || shadowAlphaRate_ != SHADOW_ALPHA_RATE_MAX || ++counter_ <= 20)
				{
					return;
				}
				VecFx32 btl_reuse_pos = btl.btl_reuse_pos;
				characterMng.getPosition(npc_.characterMngId(), btl_reuse_pos);
				battleDisplay.getBattleCamera().setTarget(EffectedNpcTarget);
				battleDisplay.getBattleCamera().setPosition(EffectedNpcCamera);
				stageMng.setHidden(flag: true);
				B.playerWindow().show(flag: false);
				if (attackType() == NORMAL_ATTACK_ID)
				{
					Battle2DManager.instance().helpWindow().releaseHelpWindow();
					setState(NPC_STATE.NPC_NORMAL_ATTACK_START);
					return;
				}
				int effectId = BattleEffect.instance().create(turnSystem_.magicStartEffect(attackType()), 1);
				turnSystem_.setHitEffectPosition(npc_, effectId, 0, 0);
				int id = itm.ItemManager.instance().magicParameter((short)attackType()).nameId();
				Battle2DManager.instance().helpWindow().setMsdHandle(1);
				Battle2DManager.instance().helpWindow().createHelpWindow(id, 0, 0);
				if (turnSystem_.magicStartEffect(attackType()) == 407)
				{
					BattleSE.instance().play(200, 15);
				}
				else
				{
					BattleSE.instance().play(200, 14);
				}
				counter_ = 0;
				setState(NPC_STATE.NPC_MAGIC_ACTION);
			}

			public void npcMagicAction(BattleSystem B)
			{
				counter_++;
				if (counter_ < WINDOW_DRAW_FRAME_MAX)
				{
					return;
				}
				if (counter_ == WINDOW_DRAW_FRAME_MAX)
				{
					Battle2DManager.instance().helpWindow().releaseHelpWindow();
					VecFx32 btl_reuse_pos = btl.btl_reuse_pos;
					characterMng.getPosition(npc_.characterMngId(), btl_reuse_pos);
					npc_.effectId_set(1, BattleEffect.instance().create(turnSystem_.magicStartEffect(attackType()), 2));
					if (turnSystem_.magicStartEffect(attackType()) == 407)
					{
						BattleEffect.instance().setPosition(npc_.effectId(1), btl_reuse_pos);
					}
					else
					{
						turnSystem_.setHitEffectPosition(npc_, npc_.effectId(1), 0, 0);
					}
					BattleEffect.instance().deleteEffect(npc_.effectId(0));
					npc_.effectId_set(0, -1);
					characterMng.startMotion(npc_.characterMngId(), 4002, fLoop: false, 0u);
				}
				else
				{
					if (characterMng.getMotionIndex(npc_.characterMngId()) == 4002 && characterMng.isEndOfMotion(npc_.characterMngId()))
					{
						characterMng.startMotion(npc_.characterMngId(), 4003, fLoop: true, 0u);
					}
					if (npc_.isClearAllEffect())
					{
						alphaRate_ = ALPHA_RATE_MAX;
						shadowAlphaRate_ = SHADOW_ALPHA_RATE_MAX;
						counter_ = 0;
						setState(NPC_STATE.NPC_DISAPPEAR);
					}
				}
			}

			public void npcDisappear(BattleSystem B)
			{
				if (++counter_ >= WINDOW_DRAW_FRAME_MAX / 6)
				{
					counter_ = 0;
					dgs.CFade.Main().fadeOut(10, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
					setState(NPC_STATE.IS_PLAYER_DATA);
				}
			}

			public void playerIsData(BattleSystem B)
			{
				if (!dgs.CFade.Main().isFaded())
				{
					return;
				}
				characterMng.setTransparencyRate(npc_.characterMngId(), 0);
				characterMng.setShadowAlphaRate(npc_.characterMngId(), 0);
				characterMng.delCharacter(npc_.characterMngId());
				npc_.setCharacterMngId(-1);
				npc_.unregisterWeapon(pl.HAND_TYPE.RIGHT_HAND);
				npc_.unregisterWeapon(pl.HAND_TYPE.LEFT_HAND);
				battleDisplay.stateBattleCamera();
				stageMng.setHidden(flag: false);
				B.playerWindow().show(flag: true);
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(i);
					if (battlePlayer.isEnable())
					{
						characterMng.setTransparencyRate(battlePlayer.characterMngId(), 100);
						if (battlePlayer.itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_ >= 0)
						{
							characterMng.setTransparencyRate(battlePlayer.itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_, 100);
						}
						if (battlePlayer.itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_ >= 0)
						{
							characterMng.setTransparencyRate(battlePlayer.itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_, 100);
						}
						characterMng.setShadowAlphaRate(battlePlayer.characterMngId(), SHADOW_ALPHA_RATE_MAX);
						battlePlayer.changeConditionEffect();
					}
				}
				dgs.CFade.Main().fadeIn(10);
				setState(NPC_STATE.PLAYER_APPEAR);
			}

			public void playerAppear(BattleSystem B)
			{
				if (dgs.CFade.Main().isCleared())
				{
					if (attackType_ == NORMAL_ATTACK_ID)
					{
						setState(NPC_STATE.NPC_END);
						return;
					}
					turnSystem_.calcMagicDamage(npc_);
					setState(NPC_STATE.NPC_MAGIC_ATTACK_START);
				}
			}

			public void npcMagicAttackStart(BattleSystem B)
			{
				if (!TexDivideLoader.getSingleton().tdlIsEmpty())
				{
					return;
				}
				turnSystem_.setCheckFlag(TurnSystem.PlayEffect);
				turnSystem_.setCheckFlag(TurnSystem.EndPlayerProcess);
				turnSystem_.setPlayerWindow(B.playerWindow());
				for (int i = 0; i < 12; i++)
				{
					BaseBattleCharacter baseBattleCharacterFromBreed = turnSystem_.characterManager().getBaseBattleCharacterFromBreed((short)i);
					if (baseBattleCharacterFromBreed != null && baseBattleCharacterFromBreed.battleCharacterId() != npc_.battleCharacterId())
					{
						baseBattleCharacterFromBreed.setReflectTargetId(-1);
					}
				}
				setState(NPC_STATE.NPC_MAGIC_ATTACK);
			}

			public void npcMagicAttack(BattleSystem B)
			{
				turnSystem_.executeCommonMagic();
				turnSystem_.flash_.draw();
				if (turnSystem_.checkFlag(TurnSystem.EndPlayerProcess) && turnSystem_.checkFlag(TurnSystem.EndEnemyProcess))
				{
					setState(NPC_STATE.NPC_END);
				}
			}

			public void npcNormalAttackStart(BattleSystem B)
			{
				if (npc_.itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_ >= 0)
				{
					characterMng.setTransparencyRate(npc_.itemInfo(pl.HAND_TYPE.RIGHT_HAND).characterMngId_, alphaRate_);
				}
				if (npc_.itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_ >= 0)
				{
					characterMng.setTransparencyRate(npc_.itemInfo(pl.HAND_TYPE.LEFT_HAND).characterMngId_, alphaRate_);
				}
				if (++helpWindowCounter_ > TurnSystem.DRAW_HELP_WINDOW_FRAME)
				{
					battleDisplay.stateBattleCamera();
					stageMng.setHidden(flag: false);
					B.playerWindow().show(flag: true);
					npc_.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_NORMAL_ATTACK);
					setState(NPC_STATE.NPC_NORMAL_ATTACK);
					turnSystem_.setPhase(TurnSystem.Phase.Execute);
					turnSystem_.setState(7);
					turnSystem_.clearCheckFlagAll();
					turnSystem_.playerTurn_.setNowPlayer(turnSystem_.nowCharacter());
				}
			}

			public void npcNormalAttack(BattleSystem B)
			{
				turnSystem_.playerTurn_.stateNormalAttack(turnSystem_);
				turnSystem_.flash_.draw();
				npc_.act();
				if (turnSystem_.phase() == TurnSystem.Phase.MonsterExecute)
				{
					setState(NPC_STATE.NPC_DISAPPEAR);
				}
			}

			public void npcEnd(BattleSystem B)
			{
				OutsideToBattle.getInstance().offTransfix();
				BattleSE.instance().free();
				BattleEffect.instance().deleteAll();
				BattleEffect.instance().endEfp();
				OutsideToBattle.getInstance().initializeNPC().setNpcId(-1);
				setState(NPC_STATE.NPC_START);
				stageMng.setHidden(flag: false);
				B.playerWindow().show(flag: true);
				onEnd();
			}

			public BattleNpcManager()
			{
				npc_ = null;
				npcState_ = new NpcState[14]
				{
					new NpcState(npcStart),
					new NpcState(npcCreatingWindow),
					new NpcState(playerDisappear),
					new NpcState(npcIsData),
					new NpcState(npcAppear),
					new NpcState(npcMagicAction),
					new NpcState(npcDisappear),
					new NpcState(playerIsData),
					new NpcState(playerAppear),
					new NpcState(npcMagicAttackStart),
					new NpcState(npcMagicAttack),
					new NpcState(npcNormalAttackStart),
					new NpcState(npcNormalAttack),
					new NpcState(npcEnd)
				};
			}

			public bool isEnd()
			{
				return end_;
			}

			public void setupState()
			{
				state_ = NPC_STATE.NPC_START;
			}

			public int nowNpcId()
			{
				return nowNpcId_;
			}

			public void setNowNpcId(int _id)
			{
				nowNpcId_ = _id;
			}

			private void setState(NPC_STATE state)
			{
				state_ = state;
			}

			private void onEnd()
			{
				end_ = true;
			}

			private void offEnd()
			{
				end_ = false;
			}

			private int attackType()
			{
				return attackType_;
			}
		}
	}
}
