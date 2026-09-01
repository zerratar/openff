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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class btl
	{
		public class BattlePlayer : BaseBattleCharacter
		{
			public enum IDLE_TYPE
			{
				IDLE,
				POISE,
				IDLE_TYPE_MAX
			}

			public enum ITEM_TYPE
			{
				WEAPON,
				PROTECTION,
				ITEM_TYPE_MAX
			}

			public const IDLE_TYPE IDLE = IDLE_TYPE.IDLE;

			public const IDLE_TYPE POISE = IDLE_TYPE.POISE;

			public const IDLE_TYPE IDLE_TYPE_MAX = IDLE_TYPE.IDLE_TYPE_MAX;

			public const ITEM_TYPE WEAPON = ITEM_TYPE.WEAPON;

			public const ITEM_TYPE PROTECTION = ITEM_TYPE.PROTECTION;

			public const ITEM_TYPE ITEM_TYPE_MAX = ITEM_TYPE.ITEM_TYPE_MAX;

			public const int ROOT = 0;

			public const int BODY = 1;

			public const int FACE = 2;

			public const int HEAD = 3;

			public const int OVER_HEAD = 4;

			public const int NOT_REGISTER = 0;

			public const int READY_MODEL = 1;

			public const int END_MODEL = 2;

			public const int READY_COMMON_MOTION = 3;

			public const int END_COMMON_MOTION = 4;

			public const int READY_LV_UP_MOTION = 5;

			public const int END_LV_UP_MOTION = 6;

			public const int READY_RW_MODEL = 7;

			public const int END_RW_MODEL = 8;

			public const int READY_COLOR_RW = 9;

			public const int END_COLOR_RW = 10;

			public const int READY_LW_MODEL = 11;

			public const int END_LW_MODEL = 12;

			public const int READY_COLOR_LW = 13;

			public const int END_COLOR_LW = 14;

			public const int END_DATA = 15;

			public const int REGISTER_DATA_STATE_MAX = 16;

			private static sbyte[] EquipWeaponMotionFileId = new sbyte[19]
			{
				11, 12, 13, 15, 15, 17, 17, 18, 18, 24,
				11, 15, 15, 16, 21, 23, 22, 14, 0
			};

			private static int[] JobMotionFileId = new int[23]
			{
				72, 50, 51, 52, 54, 56, 58, 59, 60, 61,
				62, 63, 64, 65, 66, 67, 69, 53, 55, 57,
				68, 71, 70
			};

			private byte playerId_;

			private int orderId_;

			private int playerActionId_;

			private int nextPlayerActionId_;

			private bool isPlayerActionEnd_;

			private int[] attackMotionNumber_ = new int[2];

			private int[] effectNumber_ = new int[2];

			private bool[] missAttackFlag_ = new bool[2];

			private int speed_;

			private int distance_;

			private short stealItemId_;

			private bool[] hiddenWeaponFlag_ = new bool[2];

			private int dataState_;

			private StoneInfo stoneInfo_ = new StoneInfo();

			private pl.ItemInfo[] itemInfo_ = new pl.ItemInfo[2];

			private pl.HAND_TYPE nowAttackHand_;

			private sbyte idleType_;

			private pl.Player player_;

			private eld.IObject conditionEffect_;

			private BattleActionBase[] actionBase_ = new BattleActionBase[38];

			private BattleActionMoveFront moveFront_ = new BattleActionMoveFront();

			private BattleActionMoveBack moveBack_ = new BattleActionMoveBack();

			private BattleActionNormalAttack normalAttack_ = new BattleActionNormalAttack();

			private BattleActionDamage damage_ = new BattleActionDamage();

			private BattleActionEscape escape_ = new BattleActionEscape();

			private BattleActionFinish finish_ = new BattleActionFinish();

			private BattleActionIdleToPoise idleToPoise_ = new BattleActionIdleToPoise();

			private BattleActionPoiseToIdle poiseToIdle_ = new BattleActionPoiseToIdle();

			private BattleActionAttackMoveFront attackMoveFront_ = new BattleActionAttackMoveFront();

			private BattleActionAttackMoveBack attackMoveBack_ = new BattleActionAttackMoveBack();

			private BattleActionChangeFormationMoveFront changeFormationMoveFront_ = new BattleActionChangeFormationMoveFront();

			private BattleActionChangeformationMoveBack changeFormationMoveBack_ = new BattleActionChangeformationMoveBack();

			private BattleActionGuardStart guardStart_ = new BattleActionGuardStart();

			private BattleActionGuard guard_ = new BattleActionGuard();

			private BattleActionMagic magic_ = new BattleActionMagic();

			private BattleActionMagicLoop magicLoop_ = new BattleActionMagicLoop();

			private BattleActionSteal steal_ = new BattleActionSteal();

			private BattleActionTakeAPowder takeAPowder_ = new BattleActionTakeAPowder();

			private BattleActionProtect protect_ = new BattleActionProtect();

			private BattleActionItem item_ = new BattleActionItem();

			private BattleActionItemLoop itemLoop_ = new BattleActionItemLoop();

			private BattleActionKnockOverMoveFront knockOverMoveFront_ = new BattleActionKnockOverMoveFront();

			private BattleActionKnockOverMoveBack knockOverMoveBack_ = new BattleActionKnockOverMoveBack();

			private BattleActionPoise poise_ = new BattleActionPoise();

			private BattleActionCoverStart coverStart_ = new BattleActionCoverStart();

			private BattleActionCheck check_ = new BattleActionCheck();

			private BattleActionDetect detect_ = new BattleActionDetect();

			private BattleActionJumpStart jumpStart_ = new BattleActionJumpStart();

			private BattleActionJumpEnd jumpEnd_ = new BattleActionJumpEnd();

			private BattleActionDark dark_ = new BattleActionDark();

			private BattleActionRollUp rollUp_ = new BattleActionRollUp();

			private BattleActionProvocation provocation_ = new BattleActionProvocation();

			private BattleActionCover cover_ = new BattleActionCover();

			private BattleActionPitch pitch_ = new BattleActionPitch();

			private BattleActionGeography geography_ = new BattleActionGeography();

			private BattleActionSong song_ = new BattleActionSong();

			private BattleActionIdle idle_ = new BattleActionIdle();

			private BattleActionCondition condition_2 = new BattleActionCondition();

			public BattlePlayer()
			{
				for (int i = 0; i < itemInfo_.Length; i++)
				{
					itemInfo_[i] = new pl.ItemInfo();
				}
				player_ = null;
				conditionEffect_ = null;
				for (int j = 0; j < 38; j++)
				{
					actionBase_[j] = null;
				}
				stoneInfo_.motionIndex_ = 0;
				stoneInfo_.motionFrame_ = 0;
			}

			public new void initialize()
			{
				base.initialize();
				playerId_ = 0;
				playerActionId_ = -1;
				nextPlayerActionId_ = -1;
				isPlayerActionEnd_ = false;
				idleType_ = 0;
				nowAttackHand_ = pl.HAND_TYPE.NO_HAND;
				player_ = null;
				deleteConditionEffect();
				for (int i = 0; i < 2; i++)
				{
					attackMotionNumber_[i] = 0;
				}
				changeColorFrame_ = 0;
				itemInfo_[0].characterMngId_ = -1;
				itemInfo_[0].itemType_ = -1;
				itemInfo_[1].characterMngId_ = -1;
				itemInfo_[1].itemType_ = -1;
				dataState_ = 0;
				clearStoneInfo();
				setHiddenWeaponFlag(pl.HAND_TYPE.RIGHT_HAND, flag: false);
				setHiddenWeaponFlag(pl.HAND_TYPE.LEFT_HAND, flag: false);
				actionBase_[0] = moveFront_;
				actionBase_[1] = moveBack_;
				actionBase_[2] = normalAttack_;
				actionBase_[3] = damage_;
				actionBase_[4] = escape_;
				actionBase_[5] = finish_;
				actionBase_[6] = idleToPoise_;
				actionBase_[7] = poiseToIdle_;
				actionBase_[8] = attackMoveFront_;
				actionBase_[9] = attackMoveBack_;
				actionBase_[10] = changeFormationMoveFront_;
				actionBase_[11] = changeFormationMoveBack_;
				actionBase_[12] = guardStart_;
				actionBase_[13] = guard_;
				actionBase_[14] = magic_;
				actionBase_[15] = magicLoop_;
				actionBase_[16] = steal_;
				actionBase_[17] = takeAPowder_;
				actionBase_[18] = protect_;
				actionBase_[19] = item_;
				actionBase_[20] = itemLoop_;
				actionBase_[21] = knockOverMoveFront_;
				actionBase_[22] = knockOverMoveBack_;
				actionBase_[23] = poise_;
				actionBase_[24] = coverStart_;
				actionBase_[25] = check_;
				actionBase_[26] = detect_;
				actionBase_[27] = jumpStart_;
				actionBase_[28] = jumpEnd_;
				actionBase_[29] = dark_;
				actionBase_[32] = rollUp_;
				actionBase_[34] = provocation_;
				actionBase_[35] = cover_;
				actionBase_[33] = pitch_;
				actionBase_[30] = geography_;
				actionBase_[31] = song_;
				actionBase_[36] = idle_;
				actionBase_[37] = condition_2;
			}

			public void terminate()
			{
				deleteConditionEffect();
			}

			public void phaseInitialize()
			{
				nowAttackHand_ = pl.HAND_TYPE.NO_HAND;
				for (int i = 0; i < 2; i++)
				{
					missAttackFlag_[i] = false;
				}
			}

			public void preExecute()
			{
				if (characterMngId() >= 0)
				{
					characterMng.initJntMtx(characterMngId());
					reserveBoneMatrix(pl.HAND_TYPE.RIGHT_HAND);
					reserveBoneMatrix(pl.HAND_TYPE.LEFT_HAND);
					characterMng.reserveToGetJntMtx(characterMngId(), PlayerBoneName[4]);
					characterMng.reserveToGetJntMtx(characterMngId(), PlayerBoneName[5]);
				}
			}

			public void setNextPlayerActionId(BATTLE_ACTION_TYPE next_id)
			{
				nextPlayerActionId_ = (int)next_id;
				offIsPlayerActionEnd();
			}

			public void setAttackMotionNumber(int number, int i)
			{
				int num;
				if (number == 0)
				{
					num = 1;
					setMissAttackFlag(flag: true, i);
				}
				else if (number >= 6)
				{
					num = 6;
					setMissAttackFlag(flag: false, i);
				}
				else if (number >= 1)
				{
					num = number;
					setMissAttackFlag(flag: false, i);
				}
				else
				{
					num = number;
					setMissAttackFlag(flag: true, i);
				}
				attackMotionNumber_[i] = num;
			}

			public void setEffectNumber(int number, int i)
			{
				int num = ((number != 0) ? ((number >= 6) ? 6 : ((number < 1) ? number : number)) : 0);
				effectNumber_[i] = num;
			}

			public void calcDistance(VecFx32 targetPos)
			{
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
				characterMng.getPosition(characterMngId(), fnd_reuse_pos);
				fnd_reuse_pos.y = targetPos.y;
				int num = VEC_Distance(fnd_reuse_pos, targetPos);
				setDistance(num);
			}

			public bool isPlayerActionEnd()
			{
				bool result = isPlayerActionEnd_;
				offIsPlayerActionEnd();
				return result;
			}

			public void clearMissAttackFlag()
			{
				for (int i = 0; i < 2; i++)
				{
					setMissAttackFlag(flag: false, i);
				}
			}

			public void clearStoneInfo()
			{
				stoneInfo_.motionIndex_ = 0;
				stoneInfo_.motionFrame_ = 0;
			}

			public void act()
			{
				if (playerActionId() != nextPlayerActionId())
				{
					playerActionId_set(nextPlayerActionId());
					if (playerActionId() != -1 && action() != null)
					{
						action().initialize(this);
					}
				}
				else if (playerActionId() != -1 && action() != null && action().execute(this))
				{
					action().terminate(this);
					setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_NON_ACTION);
					onIsPlayerActionEnd();
				}
			}

			public VecFx32 rootPosition()
			{
				byte b = player().formationType();
				byte b2 = (byte)pl.PlayerParty.instance().playerOrder(playerId());
				return PlayerPosition[b][b2];
			}

			public void updateCondition()
			{
				condition().offDeath();
				condition().offNearDeath();
				if (hp().getNow() == 0)
				{
					condition().onDeath();
				}
				else if (hp().getNow() <= hp().getLimit() * 25 / 100)
				{
					condition().onNearDeath();
				}
				if (condition().isConfusion() || condition().isParalysis() || condition().isDeath())
				{
					clearFlag(PLAYER_FLAG.PF_GUARD);
					clearFlag(PLAYER_FLAG.PF_MORE_GUARD);
				}
			}

			public bool setConditionMotion(int frame)
			{
				int motionIndex = characterMng.getMotionIndex(characterMngId());
				characterMng.setMotionPause(characterMngId(), b: false);
				bool flag = false;
				if (condition().isDeath())
				{
					return checkMotionDeath(motionIndex, frame);
				}
				if (condition().isStone())
				{
					return checkMotionStone(motionIndex, frame);
				}
				if (condition().isPoisonMotion())
				{
					return checkMotionPoison(motionIndex, frame);
				}
				if (condition().isNearDeath())
				{
					return checkMotionNearDeath(motionIndex, frame);
				}
				if (base.flag(PLAYER_FLAG.PF_GUARD))
				{
					return checkMotionDefense(motionIndex, frame);
				}
				return checkMotionHealth(motionIndex, idleType(), frame);
			}

			public bool registerHuman(bool flag)
			{
				string arg = "";
				sprintf(out arg, "j%d%02d", playerId() + 1, player().jobManager().nowJob() + 1);
				if (!condition().isFrog())
				{
					characterMngId_set(characterMng.setCharacter(arg, CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST));
					characterMng.releaseMdlTexRes(characterMngId());
					characterMng.addMotion(characterMngId(), "b_b01");
					characterMng.addMotion(characterMngId(), "b_b04_002");
					registerWeapon(pl.HAND_TYPE.RIGHT_HAND);
					registerWeapon(pl.HAND_TYPE.LEFT_HAND);
					characterMng.setShadowType(characterMngId(), 1);
					characterMng.setShadowScale(characterMngId(), PlayerShadowScale);
					characterMng.setShadowHeight(characterMngId(), 2048);
					characterMng.setShadowAlphaRate(characterMngId(), SHADOW_ALPHA_RATE_MAX);
					characterMng.startMotion(characterMngId(), 101, fLoop: true, 0u);
					if (!flag)
					{
						changeConditionEffect();
					}
				}
				return true;
			}

			public bool registerHumanAsync(bool flag)
			{
				if (condition().isFrog())
				{
					return true;
				}
				string arg = "";
				switch (dataState_)
				{
				case 0:
					dataState_ = 1;
					sprintf(out arg, "j%d%02d", playerId() + 1, player().jobManager().nowJob() + 1);
					setCharacterMngId(characterMng.setCharacterAsync(arg, CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST));
					characterMng.setHidden(characterMngId(), b: true);
					dataState_ = 2;
					break;
				case 1:
					sprintf(out arg, "j%d%02d", playerId() + 1, player().jobManager().nowJob() + 1);
					setCharacterMngId(characterMng.setCharacterAsync(arg, CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST));
					characterMng.setHidden(characterMngId(), b: true);
					dataState_ = 2;
					break;
				case 2:
					if (!characterMng.isLoadingCharaAsync())
					{
						characterMng.releaseMdlTexRes(characterMngId());
						dataState_ = 3;
					}
					break;
				case 3:
					sprintf(out arg, "b_b01");
					characterMng.addMotionAsync(characterMngId(), arg);
					dataState_ = 4;
					break;
				case 4:
					if (!characterMng.isLoadingMotionAsync())
					{
						dataState_ = 5;
					}
					break;
				case 5:
					sprintf(out arg, "b_b01");
					characterMng.addMotionAsync(characterMngId(), arg);
					dataState_ = 6;
					break;
				case 6:
					if (!characterMng.isLoadingMotionAsync())
					{
						dataState_ = 7;
					}
					break;
				case 7:
				{
					itemInfo_[0].characterMngId_ = -1;
					itemInfo_[0].itemType_ = -1;
					short num = equipItemId(pl.HAND_TYPE.RIGHT_HAND);
					if (num > 0)
					{
						sprintf(out arg, "w%03d", itm.ItemManager.instance().itemParameter(num).graphId());
						itemInfo_[0].characterMngId_ = characterMng.setCharacter(arg, CCharacterMng.PRI_SCENE.PRI_SCENE_SECOND);
						characterMng.setHidden(itemInfo_[0].characterMngId_, b: true);
						dataState_ = 8;
					}
					else
					{
						dataState_ = 11;
					}
					break;
				}
				case 8:
					if (!characterMng.isLoadingCharaAsync())
					{
						characterMng.setShadowType(itemInfo_[0].characterMngId_, 2);
						characterMng.releaseMdlTexRes(itemInfo_[0].characterMngId_);
						dataState_ = 9;
					}
					break;
				case 9:
				{
					short num4 = equipItemId(pl.HAND_TYPE.RIGHT_HAND);
					sprintf(out arg, "w%03d_%04d", itm.ItemManager.instance().itemParameter(num4).graphId(), num4);
					characterMng.bindReplacePlttAsync(itemInfo_[0].characterMngId_, arg);
					dataState_ = 10;
					break;
				}
				case 10:
					if (characterMng.isLoadedReplacePltt(itemInfo_[0].characterMngId_))
					{
						dataState_ = 11;
					}
					break;
				case 11:
				{
					itemInfo_[1].characterMngId_ = -1;
					itemInfo_[1].itemType_ = -1;
					short num3 = equipItemId(pl.HAND_TYPE.LEFT_HAND);
					if (num3 > 0)
					{
						sprintf(out arg, "w%03d", itm.ItemManager.instance().itemParameter(num3).graphId());
						itemInfo_[1].characterMngId_ = characterMng.setCharacter(arg, CCharacterMng.PRI_SCENE.PRI_SCENE_SECOND);
						characterMng.setHidden(itemInfo_[1].characterMngId_, b: true);
						dataState_ = 12;
					}
					else
					{
						dataState_ = 15;
					}
					break;
				}
				case 12:
					if (!characterMng.isLoadingCharaAsync())
					{
						characterMng.setShadowType(itemInfo_[1].characterMngId_, 2);
						characterMng.releaseMdlTexRes(itemInfo_[1].characterMngId_);
						dataState_ = 13;
					}
					break;
				case 13:
				{
					short num2 = equipItemId(pl.HAND_TYPE.LEFT_HAND);
					sprintf(out arg, "w%03d_%04d", itm.ItemManager.instance().itemParameter(num2).graphId(), num2);
					characterMng.bindReplacePlttAsync(itemInfo_[1].characterMngId_, arg);
					dataState_ = 14;
					break;
				}
				case 14:
					if (characterMng.isLoadedReplacePltt(itemInfo_[1].characterMngId_))
					{
						dataState_ = 15;
					}
					break;
				case 15:
					characterMng.setShadowType(characterMngId(), 1);
					characterMng.setShadowScale(characterMngId(), PlayerShadowScale);
					characterMng.setShadowHeight(characterMngId(), 2048);
					characterMng.setShadowAlphaRate(characterMngId(), SHADOW_ALPHA_RATE_MAX);
					characterMng.startMotion(characterMngId(), 101, fLoop: true, 0u);
					if (!flag)
					{
						changeConditionEffect();
					}
					return true;
				}
				return false;
			}

			public bool changeModel(bool flag)
			{
				bool flag2 = false;
				if (condition().isFrog())
				{
					flag2 = changeFrog(flag);
				}
				if (!flag2 && condition().isLilliput())
				{
					flag2 = changeLilliput(flag);
				}
				return flag2;
			}

			public bool changeLilliput(bool flag)
			{
				if (!flag && !changeCondition().isLilliput())
				{
					return false;
				}
				VecFx32 fnd_reuse_pos = fnd_reuse_pos2;
				fnd_reuse_pos.x = 2048;
				fnd_reuse_pos.y = 2048;
				fnd_reuse_pos.z = 2048;
				characterMng.setScale(characterMngId(), fnd_reuse_pos);
				fnd_reuse_pos.x = 2730;
				fnd_reuse_pos.y = 4096;
				fnd_reuse_pos.z = 2730;
				characterMng.setShadowScale(characterMngId(), fnd_reuse_pos);
				if (itemInfo_[0].characterMngId_ >= 0)
				{
					characterMng.setHidden(itemInfo_[0].characterMngId_, b: true);
				}
				if (itemInfo_[1].characterMngId_ >= 0)
				{
					characterMng.setHidden(itemInfo_[1].characterMngId_, b: true);
				}
				condition().onLilliput();
				changeCondition().onLilliput();
				return true;
			}

			public bool changeFrog(bool flag)
			{
				if (!flag && !changeCondition().isFrog())
				{
					return false;
				}
				if (characterMngId() >= 0)
				{
					characterMng.delCharacter(characterMngId());
				}
				setCharacterMngId(-1);
				string arg = "";
				if (playerId() > 0)
				{
					sprintf(out arg, "n43%d", playerId() + 4);
				}
				else
				{
					sprintf(out arg, "n431");
				}
				setCharacterMngId(characterMng.setCharacter(arg, CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST));
				characterMng.releaseMdlTexRes(characterMngId());
				characterMng.addMotion(characterMngId(), "b_b01_431");
				characterMng.startMotion(characterMngId(), 101, fLoop: true, 0u);
				characterMng.setPosition(characterMngId(), rootPosition());
				characterMng.setRotation(characterMngId(), 0, PlayerRotationY, 0);
				VecFx32 fnd_reuse_pos = fnd_reuse_pos2;
				fnd_reuse_pos.x = 2048;
				fnd_reuse_pos.y = 2048;
				fnd_reuse_pos.z = 2048;
				characterMng.setScale(characterMngId(), fnd_reuse_pos);
				fnd_reuse_pos.x = 2730;
				fnd_reuse_pos.y = 4096;
				fnd_reuse_pos.z = 2730;
				characterMng.setShadowScale(characterMngId(), fnd_reuse_pos);
				if (itemInfo_[0].characterMngId_ >= 0)
				{
					characterMng.setHidden(itemInfo_[0].characterMngId_, b: false);
				}
				if (itemInfo_[1].characterMngId_ >= 0)
				{
					characterMng.setHidden(itemInfo_[1].characterMngId_, b: false);
				}
				condition().onFrog();
				changeCondition().offFrog();
				setConditionMotion(0);
				return true;
			}

			public bool returnHuman()
			{
				if (changeCondition().isFrog())
				{
					VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos2;
					characterMng.getPosition(characterMngId(), fnd_reuse_pos);
					characterMng.delCharacter(characterMngId());
					setCharacterMngId(-1);
					condition().offFrog();
					changeCondition().offFrog();
					registerHuman(flag: false);
					resetParameterMagicFlag();
					characterMng.setPosition(characterMngId(), fnd_reuse_pos);
				}
				else if (changeCondition().isLilliput())
				{
					VecFx32 fnd_reuse_pos2 = GlobalScope.fnd_reuse_pos2;
					fnd_reuse_pos2.x = 4096;
					fnd_reuse_pos2.y = 4096;
					fnd_reuse_pos2.z = 4096;
					characterMng.setScale(characterMngId(), fnd_reuse_pos2);
					fnd_reuse_pos2.x = 5461;
					fnd_reuse_pos2.y = 4096;
					fnd_reuse_pos2.z = 5461;
					characterMng.setShadowScale(characterMngId(), fnd_reuse_pos2);
					condition().offLilliput();
					changeCondition().offLilliput();
					resetParameterMagicFlag();
				}
				characterMng.setRotation(characterMngId(), 0, PlayerRotationY, 0);
				return true;
			}

			public void changeDeath()
			{
				if (condition().isDeath())
				{
					characterMng.startMotion(characterMngId(), 706, fLoop: false, 0u);
					int maxFrame = (int)characterMng.getMaxFrame(characterMngId());
					characterMng.setCurrentFrame(characterMngId(), (uint)maxFrame);
				}
			}

			public override bool isBattle()
			{
				if (!isEnable())
				{
					return false;
				}
				if (condition().isNotBattleCondition())
				{
					return false;
				}
				return true;
			}

			public bool isHealth()
			{
				if (!isBattle())
				{
					return false;
				}
				if (!condition().isHealth())
				{
					return false;
				}
				return true;
			}

			public bool checkMotionDeath(int index, int frame)
			{
				if (index != 706)
				{
					characterMng.startMotion(characterMngId(), 706, fLoop: false, (uint)frame);
				}
				return true;
			}

			public bool checkMotionStone(int index, int frame)
			{
				characterMng.setMotionPause(characterMngId(), b: true);
				return true;
			}

			public bool checkMotionPoison(int index, int frame)
			{
				if (condition().isFrog())
				{
					if (characterMng.getMotionIndex(characterMngId()) != 101)
					{
						characterMng.startMotion(characterMngId(), 101, fLoop: true, (uint)frame);
					}
					return true;
				}
				int num = -1;
				switch (player().equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND).weaponSystem())
				{
				case itm.WEAPON_SYSTEM.WEAPON_CLUB:
				case itm.WEAPON_SYSTEM.WEAPON_MACE:
				case itm.WEAPON_SYSTEM.WEAPON_HAMMER:
				case itm.WEAPON_SYSTEM.WEAPON_AXE:
					num = 415;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_HARP:
					num = 422;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_BOOK:
					num = 424;
					break;
				default:
					num = 401;
					break;
				}
				if (index != num)
				{
					characterMng.startMotion(characterMngId(), num, fLoop: true, (uint)frame);
				}
				return true;
			}

			public bool checkMotionNearDeath(int index, int frame)
			{
				if (condition().isFrog())
				{
					if (characterMng.getMotionIndex(characterMngId()) != 101)
					{
						characterMng.startMotion(characterMngId(), 101, fLoop: true, (uint)frame);
					}
					return true;
				}
				int num = -1;
				switch (player().equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND).weaponSystem())
				{
				case itm.WEAPON_SYSTEM.WEAPON_CLUB:
				case itm.WEAPON_SYSTEM.WEAPON_MACE:
				case itm.WEAPON_SYSTEM.WEAPON_HAMMER:
				case itm.WEAPON_SYSTEM.WEAPON_AXE:
					num = 315;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_BOW:
				case itm.WEAPON_SYSTEM.WEAPON_THROW:
					num = 321;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_BELL:
					num = 323;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_BOOK:
					num = 324;
					break;
				default:
					num = 301;
					break;
				}
				if (index == 706)
				{
					characterMng.startMotion(characterMngId(), num, fLoop: true, 6u);
					return false;
				}
				if (index != num)
				{
					characterMng.startMotion(characterMngId(), num, fLoop: true, (uint)frame);
				}
				return true;
			}

			public bool checkMotionHealth(int index, sbyte idle, int frame)
			{
				if (condition().isFrog())
				{
					if (characterMng.getMotionIndex(characterMngId()) != 101)
					{
						characterMng.startMotion(characterMngId(), 101, fLoop: true, (uint)frame);
					}
					return true;
				}
				int blendFrame = frame;
				switch (index)
				{
				case 706:
					blendFrame = 6;
					break;
				case 301:
				case 315:
				case 321:
				case 323:
				case 324:
					blendFrame = 6;
					break;
				}
				if (idle == 0)
				{
					if (index != 101)
					{
						characterMng.startMotion(characterMngId(), 101, fLoop: true, (uint)blendFrame);
					}
					return true;
				}
				if (actionId() == 5 || actionId() == 6)
				{
					characterMng.startMotion(characterMngId(), 501, fLoop: true, (uint)blendFrame);
				}
				else
				{
					characterMng.startMotion(characterMngId(), 201, fLoop: true, (uint)blendFrame);
				}
				return true;
			}

			public bool checkMotionDefense(int index, int frame)
			{
				if (condition().isFrog())
				{
					if (characterMng.getMotionIndex(characterMngId()) != 101)
					{
						characterMng.startMotion(characterMngId(), 101, fLoop: true, (uint)frame);
					}
					return true;
				}
				if (isHealth() && index != 704)
				{
					characterMng.startMotion(characterMngId(), 704, fLoop: true, 0u);
				}
				return true;
			}

			public void addEquipWeaponMotion(pl.HAND_TYPE handType)
			{
				string arg = "";
				short itemId = player().equipParameter().equipHand(handType).itemId();
				if (itm.ItemManager.instance().itemCategory(itemId) != itm.CATEGORY.CATEGORY_WEAPON)
				{
					sprintf(out arg, "b_b02_011");
				}
				else
				{
					int num = (int)player().equipParameter().equipHand(handType).weaponSystem();
					if (num == 20)
					{
						sprintf(out arg, "b_b02_011");
					}
					else
					{
						sprintf(out arg, "b_b02_%03d", EquipWeaponMotionFileId[num]);
					}
				}
				characterMng.addMotion(characterMngId(), arg);
				OS_Printf("[YS]   ADD PLAYER MOTION NAME %s\n", arg);
				if (player().equipParameter().isEquipBow() || player().equipParameter().isEquipArrow())
				{
					sprintf(out arg, "b_b02_011");
					characterMng.addMotion(characterMngId(), arg);
				}
			}

			public void addEquipWeaponMotionForNpc(pl.HAND_TYPE handType)
			{
				string arg = "";
				short num = weaponId(handType);
				if (itm.ItemManager.instance().itemCategory(num) != itm.CATEGORY.CATEGORY_WEAPON)
				{
					sprintf(out arg, "b_b02_011");
				}
				else
				{
					itm.WEAPON_SYSTEM wEAPON_SYSTEM = (itm.WEAPON_SYSTEM)((num >= 0) ? itm.ItemManager.instance().itemParameter(num).system() : 20);
					if (wEAPON_SYSTEM == itm.WEAPON_SYSTEM.NON_WEAPON)
					{
						sprintf(out arg, "b_b02_011");
					}
					else
					{
						sprintf(out arg, "b_b02_%03d", EquipWeaponMotionFileId[(int)wEAPON_SYSTEM]);
					}
				}
				characterMng.addMotion(characterMngId(), arg);
				OS_Printf("[YS]   ADD PLAYER MOTION NAME %s\n", arg);
			}

			public void addMagicMotion()
			{
				characterMng.addMotion(characterMngId(), "b_b02_040");
				OS_Printf("[YS]   ADD PLAYER MAGIC MOTION\n");
			}

			public void addJobMotion()
			{
				int num = player().jobManager().nowJob();
				string arg = "";
				sprintf(out arg, "b_b03_%03d", JobMotionFileId[num]);
				characterMng.addMotion(characterMngId(), arg);
				OS_Printf("[YS]   ADD JOB MOTION %d\n", JobMotionFileId[num]);
			}

			public void addPitchMotion()
			{
				string arg = "";
				sprintf(out arg, "b_b02_021");
				characterMng.addMotion(characterMngId(), arg);
			}

			public void removeEquipWeaponMotion(pl.HAND_TYPE handType)
			{
				string arg = "";
				short itemId = player().equipParameter().equipHand(handType).itemId();
				if (itm.ItemManager.instance().itemCategory(itemId) != itm.CATEGORY.CATEGORY_WEAPON)
				{
					sprintf(out arg, "b_b02_011");
				}
				else
				{
					int num = (int)player().equipParameter().equipHand(handType).weaponSystem();
					if (num == 20)
					{
						sprintf(out arg, "b_b02_011");
					}
					else
					{
						sprintf(out arg, "b_b02_%03d", EquipWeaponMotionFileId[num]);
					}
				}
				characterMng.removeMotion(characterMngId(), arg);
				OS_Printf("[YS]   REMOVE PLAYER MOTION NAME %s\n", arg);
				sprintf(out arg, "b_b02_011");
				characterMng.removeMotion(characterMngId(), arg);
				OS_Printf("[YS]   REMOVE PLAYER MOTION NAME %s\n", arg);
			}

			public void removeMagicMotion()
			{
				characterMng.removeMotion(characterMngId(), "b_b02_040");
				OS_Printf("[YS]   REMOVE PLAYER MAGIC MOTION\n");
			}

			public void removeJobMotion()
			{
				int num = player().jobManager().nowJob();
				string arg = "";
				sprintf(out arg, "b_b03_%03d", JobMotionFileId[num]);
				characterMng.removeMotion(characterMngId(), arg);
				OS_Printf("[YS]   REMOVE JOB MOTION %d\n", JobMotionFileId[num]);
			}

			public void removePitchMotion()
			{
				characterMng.removeMotion(characterMngId(), "b_b02_021");
			}

			public int equipWeaponMotionIndex(itm.WEAPON_SYSTEM weapon)
			{
				int num = -1;
				switch (weapon)
				{
				case itm.WEAPON_SYSTEM.WEAPON_KNIFE:
					num = 1200;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_DARKSWORD:
					num = 1400;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_SWORD:
					num = 1300;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_CLUB:
				case itm.WEAPON_SYSTEM.WEAPON_MACE:
				case itm.WEAPON_SYSTEM.WEAPON_HAMMER:
				case itm.WEAPON_SYSTEM.WEAPON_AXE:
					num = 1500;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_STICK:
				case itm.WEAPON_SYSTEM.WEAPON_ROD:
					num = 1700;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_BOW:
				case itm.WEAPON_SYSTEM.WEAPON_ARROW:
					num = 1800;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_BOOK:
					num = 2400;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_CLAW:
					num = 1100;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_SPEAR:
					num = 1600;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_THROW:
					num = 2100;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_BELL:
					num = 2300;
					break;
				case itm.WEAPON_SYSTEM.WEAPON_HARP:
					num = 2200;
					break;
				default:
					num = 1100;
					break;
				}
				if (breed() == 0 && player().equipParameter().isBareHands())
				{
					num = 1100;
				}
				return num;
			}

			public void showWeapon(pl.HAND_TYPE hand)
			{
				if (itemInfo_[(int)hand].characterMngId_ >= 0)
				{
					bool flag = false;
					flag = condition().isFrog() || idleType() == 0 || actionId() == 5 || actionId() == 6 || actionId() == 7 || actionId() == 22 || (hiddenWeaponFlag(hand) ? true : false);
					characterMng.setHidden(itemInfo_[(int)hand].characterMngId_, flag);
				}
			}

			public void rebornItemOrMagicNumber(bool deleteItem)
			{
				if (actionId() == 7)
				{
					if (itm.ItemManager.instance().consumptionParameter((short)useItemId()) != null)
					{
						pl.PlayerParty.instance().item().serchNormalItem((short)useItemId())?.itemNumber_inc();
					}
					if (!deleteItem)
					{
						clearUseItemId();
					}
				}
				else if (actionId() == 22)
				{
					if (itm.ItemManager.instance().itemParameter((short)useItemId()) != null)
					{
						pl.PlayerParty.instance().item().storeItem((short)useItemId(), 1);
					}
					if (!deleteItem)
					{
						clearUseItemId();
					}
				}
				else if (actionId() == 5 || actionId() == 6)
				{
					itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(useMagicId());
					if (magicParameter != null)
					{
						player().mp(magicParameter.magicClass()).addNow(1);
						player().setJobChangeMp(magicParameter.magicClass(), (byte)player().mp(magicParameter.magicClass()).getNow());
					}
					if (!deleteItem)
					{
						clearUseMagicId();
					}
				}
			}

			public void deleteItemOrMagicNumber()
			{
				if (actionId() == 7)
				{
					if (itm.ItemManager.instance().consumptionParameter((short)useItemId()) != null)
					{
						itm.PossessionItem possessionItem = pl.PlayerParty.instance().item().serchNormalItem((short)useItemId());
						if (possessionItem != null)
						{
							byte itemNumber = (byte)(possessionItem.itemNumber() - 1);
							pl.PlayerParty.instance().item().serchNormalItem((short)useItemId())
								.setItemNumber(itemNumber);
						}
					}
				}
				else if (actionId() == 22)
				{
					if (itm.ItemManager.instance().itemParameter((short)useItemId()) != null)
					{
						itm.PossessionItem possessionItem2 = pl.PlayerParty.instance().item().serchNormalItem((short)useItemId());
						if (possessionItem2 != null)
						{
							byte itemNumber2 = (byte)(possessionItem2.itemNumber() - 1);
							pl.PlayerParty.instance().item().serchNormalItem((short)useItemId())
								.setItemNumber(itemNumber2);
						}
					}
				}
				else if (actionId() == 5 || actionId() == 6)
				{
					itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(useMagicId());
					if (magicParameter != null)
					{
						player().mp(magicParameter.magicClass()).subNow(1);
						player().setJobChangeMp(magicParameter.magicClass(), (byte)player().mp(magicParameter.magicClass()).getNow());
					}
				}
			}

			public bool checkMotion(int index)
			{
				if (characterMng.getMotionIndex(characterMngId()) != index)
				{
					return false;
				}
				return true;
			}

			public bool checkMotionFrame(int frame)
			{
				if (characterMng.getCurrentFrame(characterMngId()) != frame)
				{
					return false;
				}
				return true;
			}

			public bool checkMotionAndFrame(int index, int frame)
			{
				if (!checkMotion(index))
				{
					return false;
				}
				if (!checkMotionFrame(frame))
				{
					return false;
				}
				return true;
			}

			public bool checkMotionAndEnd(int index)
			{
				if (!checkMotion(index))
				{
					return false;
				}
				return characterMng.isEndOfMotion(characterMngId());
			}

			public bool checkExecuteCover()
			{
				if (!player().jobManager().checkRegisterPassiveAbility(10))
				{
					return false;
				}
				if (!isBattle())
				{
					return false;
				}
				if (!condition().isCanCover())
				{
					return false;
				}
				return true;
			}

			public bool isFinishAttack()
			{
				if (breed() == 2)
				{
					return true;
				}
				if (player() == null)
				{
					return false;
				}
				return player().isFinishAttack();
			}

			public int getWinMotionIndex()
			{
				return 4101 + playerId();
			}

			public void setWinMotion()
			{
				if (!condition().isFrog())
				{
					characterMng.startMotion(characterMngId(), getWinMotionIndex(), fLoop: true, 0u);
				}
			}

			public int getHappyMotionIndex()
			{
				return 4201 + playerId();
			}

			public void setHappyMotion()
			{
				if (!condition().isFrog())
				{
					characterMng.startMotion(characterMngId(), getHappyMotionIndex(), fLoop: false, 0u);
				}
			}

			public bool restartWinMotion()
			{
				if (condition().isFrog())
				{
					return true;
				}
				if (checkMotion(getHappyMotionIndex()))
				{
					if (!checkMotionAndEnd(getHappyMotionIndex()))
					{
						return false;
					}
					setWinMotion();
				}
				return true;
			}

			public int selectSummonType()
			{
				if (player().jobManager().nowJob() == 20)
				{
					return 2;
				}
				int num = (int)ds.RandomNumber.rand32(101u);
				if (num <= 50)
				{
					return 1;
				}
				return 0;
			}

			public int magicLevel()
			{
				return ((int?)itm.ItemManager.instance().magicParameter(useMagicId())?.magicClass()) ?? (-1);
			}

			public bool disappear(int frame)
			{
				int t = characterMng.getTransparencyRate(characterMngId()) - ALPHA_RATE_MAX / frame;
				t = ds.max(t, 0);
				characterMng.setTransparencyRate(characterMngId(), t);
				if (itemInfo_[0].characterMngId_ >= 0)
				{
					characterMng.setTransparencyRate(itemInfo_[0].characterMngId_, t);
				}
				if (itemInfo_[1].characterMngId_ >= 0)
				{
					characterMng.setTransparencyRate(itemInfo_[1].characterMngId_, t);
				}
				int t2 = characterMng.getShadowAlphaRate(characterMngId()) - SHADOW_ALPHA_RATE_MAX / frame;
				t2 = ds.max(t2, 0);
				characterMng.setShadowAlphaRate(characterMngId(), t2);
				deleteConditionEffect();
				if (t != 0)
				{
					return false;
				}
				return true;
			}

			public bool appear(int frame)
			{
				int t = characterMng.getTransparencyRate(characterMngId()) + ALPHA_RATE_MAX / frame;
				t = ds.min(t, ALPHA_RATE_MAX);
				characterMng.setTransparencyRate(characterMngId(), t);
				if (itemInfo_[0].characterMngId_ >= 0)
				{
					characterMng.setTransparencyRate(itemInfo_[0].characterMngId_, t);
				}
				if (itemInfo_[1].characterMngId_ >= 0)
				{
					characterMng.setTransparencyRate(itemInfo_[1].characterMngId_, t);
				}
				int t2 = characterMng.getShadowAlphaRate(characterMngId()) + SHADOW_ALPHA_RATE_MAX / frame;
				t2 = ds.min(t2, SHADOW_ALPHA_RATE_MAX);
				characterMng.setShadowAlphaRate(characterMngId(), t2);
				if (t == ALPHA_RATE_MAX)
				{
					changeConditionEffect();
				}
				if (t != ALPHA_RATE_MAX)
				{
					return false;
				}
				return true;
			}

			public void setAlpha(int alpha, int shadow)
			{
				characterMng.setTransparencyRate(characterMngId(), alpha);
				if (itemInfo_[0].characterMngId_ >= 0)
				{
					characterMng.setTransparencyRate(itemInfo_[0].characterMngId_, alpha);
				}
				if (itemInfo_[1].characterMngId_ >= 0)
				{
					characterMng.setTransparencyRate(itemInfo_[1].characterMngId_, alpha);
				}
				characterMng.setShadowAlphaRate(characterMngId(), shadow);
			}

			public override void resetParameterMagicFlag()
			{
				if (player() != null)
				{
					player().updateParameter();
				}
			}

			public bool isSetAbility()
			{
				if (actionId() == 8)
				{
					return true;
				}
				if (actionId() == 11)
				{
					return true;
				}
				if (actionId() == 16)
				{
					return true;
				}
				if (actionId() == 18)
				{
					return true;
				}
				if (actionId() == 22)
				{
					return true;
				}
				if (actionId() == 23)
				{
					return true;
				}
				return false;
			}

			public void moveConditionEffect()
			{
				if (condition() == null)
				{
					return;
				}
				if (condition().isParalysis())
				{
					if (equalCategoryAndMember(201, 13))
					{
						setPositionConditionEffect(1);
					}
				}
				else if (condition().isSleep())
				{
					if (equalCategoryAndMember(201, 12))
					{
						setPositionConditionEffect(3);
					}
				}
				else if (condition().isNearStone())
				{
					if (equalCategoryAndMember(201, 10))
					{
						setPositionConditionEffect(3);
					}
				}
				else if (condition().isConfusion())
				{
					if (equalCategoryAndMember(201, 11))
					{
						setPositionConditionEffect(3);
					}
				}
				else if (condition().isSilence())
				{
					if (equalCategoryAndMember(201, 6))
					{
						setPositionConditionEffect(4);
					}
				}
				else if (condition().isPoison())
				{
					if (equalCategoryAndMember(201, 3))
					{
						setPositionConditionEffect(3);
					}
				}
				else if (condition().isDarkness() && equalCategoryAndMember(201, 4))
				{
					setPositionConditionEffect(2);
				}
			}

			public void changeConditionEffect()
			{
				if (flag(PLAYER_FLAG.PF_JUMP))
				{
					deleteConditionEffect();
					return;
				}
				changePlayerColor();
				if (condition().isStone())
				{
					if (!condition().isDeath())
					{
						deleteConditionEffect();
						if (characterMng.getReplacePlttId(characterMngId()) >= 0)
						{
							return;
						}
						string arg = "";
						if (condition().isFrog())
						{
							if (playerId() > 0)
							{
								sprintf(out arg, "n43%0d_stone", playerId() + 4);
							}
							else
							{
								sprintf(out arg, "n431_stone");
							}
						}
						else
						{
							sprintf(out arg, "j%d%02d_stone", playerId() + 1, player().jobManager().nowJob() + 1);
						}
						characterMng.bindReplacePltt(characterMngId(), arg);
						if (stoneInfo_.motionIndex_ == 0)
						{
							stoneInfo_.motionIndex_ = (short)characterMng.getMotionIndex(characterMngId());
						}
						else
						{
							characterMng.startMotion(characterMngId(), stoneInfo_.motionIndex_, fLoop: true, 0u);
							characterMng.setMotionPause(characterMngId(), b: true);
						}
						if (stoneInfo_.motionFrame_ == 0)
						{
							stoneInfo_.motionFrame_ = (short)characterMng.getCurrentFrame(characterMngId());
						}
						else
						{
							characterMng.setCurrentFrame(characterMngId(), (uint)stoneInfo_.motionFrame_);
						}
						return;
					}
				}
				else if (characterMng.getReplacePlttId(characterMngId()) >= 0)
				{
					characterMng.bindMdlPltt(characterMngId());
				}
				if (condition().isDeath())
				{
					deleteConditionEffect();
				}
				else if (condition().isParalysis())
				{
					createConditionEffect(201, 13, 1);
				}
				else if (condition().isSleep())
				{
					createConditionEffect(201, 12, 3);
				}
				else if (condition().isNearStone())
				{
					createConditionEffect(201, 10, 3);
				}
				else if (condition().isConfusion())
				{
					createConditionEffect(201, 11, 3);
				}
				else if (condition().isSilence())
				{
					createConditionEffect(201, 6, 4);
				}
				else if (condition().isPoison())
				{
					createConditionEffect(201, 3, 3);
				}
				else if (condition().isDarkness())
				{
					createConditionEffect(201, 4, 2);
				}
				else
				{
					deleteConditionEffect();
				}
			}

			public void deleteConditionEffect()
			{
				if (conditionEffect_ != null)
				{
					conditionEffect_.DeleteObject();
					eld.g_elsvr.deleteObject(conditionEffect_);
					conditionEffect_ = null;
				}
			}

			public void createConditionEffect(int category, int member, int position)
			{
				if (conditionEffect_ != null)
				{
					if (equalCategoryAndMember(category, member))
					{
						return;
					}
					deleteConditionEffect();
				}
				conditionEffect_ = eld.g_elsvr.createObject((uint)category, (uint)member);
				setPositionConditionEffect(position);
			}

			public void setPositionConditionEffect(int position)
			{
				if (conditionEffect_ == null)
				{
					return;
				}
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
				characterMng.getPosition(characterMngId(), fnd_reuse_pos);
				switch (position)
				{
				case 0:
					conditionEffect_.SetPosition(fnd_reuse_pos.x, fnd_reuse_pos.y, fnd_reuse_pos.z);
					break;
				case 1:
					if (condition().isFrog() || condition().isLilliput())
					{
						fnd_reuse_pos.y += 18432;
					}
					else
					{
						fnd_reuse_pos.y += 36864;
					}
					conditionEffect_.SetPosition(fnd_reuse_pos.x, fnd_reuse_pos.y, fnd_reuse_pos.z);
					break;
				case 2:
				{
					MtxFx43 btl_reuse_temp = btl.btl_reuse_temp;
					MtxFx43 btl_reuse_move = btl.btl_reuse_move;
					MtxFx43 btl_reuse_mtx = btl.btl_reuse_mtx;
					MTX_Identity43(btl_reuse_temp);
					MTX_Identity43(btl_reuse_move);
					if (characterMng.getJntMtx(characterMngId(), "atama", btl_reuse_mtx))
					{
						ds.Vector3<int> btl_reuse_vec = btl.btl_reuse_vec;
						btl_reuse_vec.set(0, 4096, 0);
						ds.CpuMatrix.setTranslate(btl_reuse_move, btl_reuse_vec);
						MTX_Concat43(btl_reuse_temp, btl_reuse_move, btl_reuse_temp);
						MTX_Concat43(btl_reuse_temp, btl_reuse_mtx, btl_reuse_temp);
						fnd_reuse_pos.x = btl_reuse_temp._30;
						fnd_reuse_pos.y = btl_reuse_temp._31;
						fnd_reuse_pos.z = btl_reuse_temp._32;
						conditionEffect_.SetPosition(fnd_reuse_pos.x, fnd_reuse_pos.y, fnd_reuse_pos.z);
					}
					break;
				}
				case 3:
					if (condition().isFrog() || condition().isLilliput())
					{
						fnd_reuse_pos.y += 30720;
					}
					else
					{
						fnd_reuse_pos.y += 61440;
					}
					conditionEffect_.SetPosition(fnd_reuse_pos.x, fnd_reuse_pos.y, fnd_reuse_pos.z);
					break;
				case 4:
					if (condition().isFrog() || condition().isLilliput())
					{
						fnd_reuse_pos.y += 40960;
					}
					else
					{
						fnd_reuse_pos.y += 81920;
					}
					conditionEffect_.SetPosition(fnd_reuse_pos.x, fnd_reuse_pos.y, fnd_reuse_pos.z);
					break;
				}
			}

			public bool equalCategoryAndMember(int category, int member)
			{
				if (conditionEffect_ == null)
				{
					return false;
				}
				if (category == conditionEffect_.getCategoryNo() && member == conditionEffect_.getMemberNo())
				{
					return true;
				}
				return false;
			}

			public short equipItemId(pl.HAND_TYPE hand_type)
			{
				short num = 0;
				if (breed() == 0)
				{
					if (player().equipParameter().equipHand(hand_type).equipNumber()
						.get() == 0)
					{
						return -1;
					}
					num = player().equipParameter().equipHand(hand_type).itemId();
				}
				else if (breed() == 2)
				{
					num = weaponId(hand_type);
				}
				if (num < 0)
				{
					return -1;
				}
				if (itm.ItemManager.instance().weaponParameter(num) == null && itm.ItemManager.instance().protectionParameter(num) == null)
				{
					return -1;
				}
				switch (itm.ItemManager.instance().itemCategory(num))
				{
				case itm.CATEGORY.CATEGORY_WEAPON:
					if (itm.ItemManager.instance().weaponParameter(num) != null)
					{
						if (itm.ItemManager.instance().weaponParameter(num).system() == 8)
						{
							return -1;
						}
						itemInfo_[(int)hand_type].itemType_ = 0;
					}
					break;
				case itm.CATEGORY.CATEGORY_PROTECTION:
					if (itm.ItemManager.instance().protectionParameter(num) != null)
					{
						itemInfo_[(int)hand_type].itemType_ = 1;
					}
					break;
				default:
					return -1;
				}
				return num;
			}

			public int registerWeapon(pl.HAND_TYPE hand_type)
			{
				itemInfo_[(int)hand_type].characterMngId_ = -1;
				itemInfo_[(int)hand_type].itemType_ = -1;
				short num = equipItemId(hand_type);
				if (num == -1)
				{
					return -1;
				}
				string arg = "";
				sprintf(out arg, "w%03d", itm.ItemManager.instance().itemParameter(num).graphId());
				OS_Printf("アイテムモデル名 %s\n", arg);
				itemInfo_[(int)hand_type].characterMngId_ = characterMng.setCharacter(arg, CCharacterMng.PRI_SCENE.PRI_SCENE_SECOND);
				characterMng.releaseMdlTexRes(itemInfo_[(int)hand_type].characterMngId_);
				sprintf(out arg, "w%03d_%04d", itm.ItemManager.instance().itemParameter(num).graphId(), num);
				OS_Printf("アイテムパレット名 %s\n", arg);
				characterMng.bindReplacePltt(itemInfo_[(int)hand_type].characterMngId_, arg);
				characterMng.setShadowType(itemInfo_[(int)hand_type].characterMngId_, 2);
				characterMng.setHidden(itemInfo_[(int)hand_type].characterMngId_, b: true);
				return itemInfo_[(int)hand_type].characterMngId_;
			}

			public void unregisterWeapon(pl.HAND_TYPE hand_type)
			{
				if (itemInfo_[(int)hand_type].characterMngId_ >= 0)
				{
					characterMng.delCharacter(itemInfo_[(int)hand_type].characterMngId_);
					itemInfo_[(int)hand_type].characterMngId_ = -1;
				}
			}

			public string boneName(pl.HAND_TYPE hand_type)
			{
				int itemType_ = itemInfo_[(int)hand_type].itemType_;
				int num = 0;
				if (itemType_ == 1)
				{
					num = 2;
				}
				return PlayerBoneName[(int)(hand_type + num)];
			}

			public void haveWeapon(pl.HAND_TYPE hand_type)
			{
				if (itemInfo_[(int)hand_type].characterMngId_ < 0)
				{
					return;
				}
				MtxFx43 btl_reuse_mtx = btl.btl_reuse_mtx;
				MtxFx43 btl_reuse_temp = btl.btl_reuse_temp;
				MtxFx43 btl_reuse_move = btl.btl_reuse_move;
				if (!characterMng.getJntMtx(characterMngId(), boneName(hand_type), btl_reuse_mtx))
				{
					characterMng.setHidden(itemInfo_[(int)hand_type].characterMngId_, b: true);
					return;
				}
				characterMng.setHidden(itemInfo_[(int)hand_type].characterMngId_, b: false);
				MTX_Identity43(btl_reuse_temp);
				MTX_Identity43(btl_reuse_move);
				ds.Vector3<int> btl_reuse_vec = btl.btl_reuse_vec;
				if (itemInfo_[(int)hand_type].itemType_ == 0)
				{
					if (hand_type == pl.HAND_TYPE.RIGHT_HAND)
					{
						ds.CpuMatrix.setRotateZ(btl_reuse_temp, ds.DEGto65536(270));
						if (breed() == 0 && player().equipParameter().equipHand(hand_type).checkClaw())
						{
							ds.CpuMatrix.setRotateY(btl_reuse_temp, ds.DEGto65536(270));
						}
						btl_reuse_vec.set(-2457, 0, 0);
					}
					if (hand_type == pl.HAND_TYPE.LEFT_HAND)
					{
						ds.CpuMatrix.setRotateZ(btl_reuse_temp, ds.DEGto65536(90));
						if (breed() == 0 && player().equipParameter().equipHand(hand_type).checkClaw())
						{
							ds.CpuMatrix.setRotateY(btl_reuse_temp, ds.DEGto65536(90));
						}
						btl_reuse_vec.set(2457, 0, 0);
					}
				}
				if (itemInfo_[(int)hand_type].itemType_ == 1)
				{
					if (hand_type == pl.HAND_TYPE.RIGHT_HAND)
					{
						ds.CpuMatrix.setRotateX(btl_reuse_temp, ds.DEGto65536(345));
						btl_reuse_vec.set(-2457, 1024, -3276);
					}
					if (hand_type == pl.HAND_TYPE.LEFT_HAND)
					{
						ds.CpuMatrix.setRotateX(btl_reuse_temp, ds.DEGto65536(15));
						btl_reuse_vec.set(2457, 1024, -3276);
					}
				}
				ds.CpuMatrix.setTranslate(btl_reuse_move, btl_reuse_vec);
				MTX_Concat43(btl_reuse_temp, btl_reuse_move, btl_reuse_temp);
				MTX_Concat43(btl_reuse_temp, btl_reuse_mtx, btl_reuse_temp);
				characterMng.setPoseMtx(itemInfo_[(int)hand_type].characterMngId_, btl_reuse_temp);
			}

			public void reserveBoneMatrix(pl.HAND_TYPE hand_type)
			{
				if (itemInfo_[(int)hand_type].characterMngId_ >= 0)
				{
					characterMng.reserveToGetJntMtx(characterMngId(), boneName(hand_type));
				}
			}

			public byte playerId()
			{
				return playerId_;
			}

			public void setPlayerId(byte _id)
			{
				playerId_ = _id;
			}

			public int orderId()
			{
				return orderId_;
			}

			public void setOrderId(int _id)
			{
				orderId_ = _id;
			}

			public int playerActionId()
			{
				return playerActionId_;
			}

			public void playerActionId_set(int arg0)
			{
				playerActionId_ = arg0;
			}

			public void setPlayerActionId(int _id)
			{
				playerActionId_ = _id;
			}

			public int nextPlayerActionId()
			{
				return nextPlayerActionId_;
			}

			public void onIsPlayerActionEnd()
			{
				isPlayerActionEnd_ = true;
			}

			public void offIsPlayerActionEnd()
			{
				isPlayerActionEnd_ = false;
			}

			public int attackMotionNumber(int i)
			{
				return attackMotionNumber_[i];
			}

			public int effectNumber(int i)
			{
				return effectNumber_[i];
			}

			public void subEffectNumber(int i)
			{
				effectNumber_[i]--;
			}

			public void setPlayer(pl.Player player)
			{
				player_ = player;
			}

			public pl.Player player()
			{
				return player_;
			}

			public sbyte idleType()
			{
				return idleType_;
			}

			public void setIdleType(sbyte idle)
			{
				idleType_ = idle;
			}

			public int speed()
			{
				return speed_;
			}

			public void speed_set(int arg0)
			{
				speed_ = arg0;
			}

			public void setSpeed(int speed)
			{
				speed_ = speed;
			}

			public int distance()
			{
				return distance_;
			}

			public void setDistance(int distance)
			{
				distance_ = distance;
			}

			public short stealItemId()
			{
				return stealItemId_;
			}

			public void setStealItemId(short itemId)
			{
				stealItemId_ = itemId;
			}

			public pl.HAND_TYPE nowAttackHand()
			{
				return nowAttackHand_;
			}

			public void setNowAttackHand(pl.HAND_TYPE hand)
			{
				nowAttackHand_ = hand;
			}

			public bool missAttackFlag(pl.HAND_TYPE hand)
			{
				return missAttackFlag_[(int)hand];
			}

			public void setMissAttackFlag(bool flag, int i)
			{
				missAttackFlag_[i] = flag;
			}

			public bool hiddenWeaponFlag(pl.HAND_TYPE hand)
			{
				return hiddenWeaponFlag_[(int)hand];
			}

			public void setHiddenWeaponFlag(pl.HAND_TYPE hand, bool flag)
			{
				hiddenWeaponFlag_[(int)hand] = flag;
			}

			public BattleActionBase action()
			{
				return actionBase_[playerActionId_];
			}

			public pl.ItemInfo itemInfo(pl.HAND_TYPE hand_type)
			{
				return itemInfo_[(int)hand_type];
			}
		}
	}
}
