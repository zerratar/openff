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
		public class BaseBattleCharacter
		{
			public const int ErrId = -1;

			protected bool isEnable_;

			protected bool isActionEnd_;

			protected short battleCharacterId_;

			protected int actionId_;

			protected int prevActionId_;

			protected int actionNumber_;

			protected byte breed_;

			protected int characterMngId_;

			protected short[] targetId_ = new short[12];

			protected short lastTargetId_;

			protected short rollUpLevel_;

			protected uint flag_;

			protected uint magicFlag_;

			protected int[] effectId_ = new int[13];

			protected int moveYaw_;

			protected int movePitch_;

			protected short useMagicId_;

			protected int nowMagicAttackNumber_;

			protected int useItemId_;

			protected int frameCounter_;

			protected int attackNumber_;

			protected int overissueNumber_;

			protected ys.Condition changeCondition_ = new ys.Condition();

			protected bool[] attackSuccess_ = new bool[2];

			protected int[] conditionTime_ = new int[6];

			protected int provocationCharacterId_;

			protected BaseBattleCharacter provocationCharacter_;

			protected int reflectTargetId_;

			protected short changeColorFrame_;

			protected int nowColor_;

			protected int[] prevColor_ = new int[6];

			protected bool reflect_;

			protected short prevMagicid_;

			protected byte level_;

			protected byte weight_;

			protected short[] weaponId_ = new short[2];

			protected ys.MPoint<int> hp_;

			protected ys.Condition condition_;

			protected ys.BodyParameter body_;

			protected ys.BodyParameter bodyAndBonus_;

			protected ys.PhysicsAttackParameter[] handAttack_ = new ys.PhysicsAttackParameter[2];

			protected ys.PhysicsDefenseParameter physicsDefense_;

			protected ys.MagicDefenseParameter magicDefense_;

			protected ys.BodyParameter tempBody_ = new ys.BodyParameter();

			protected ys.PhysicsAttackParameter tempAttack_ = new ys.PhysicsAttackParameter();

			protected ys.PhysicsDefenseParameter tempDefense_ = new ys.PhysicsDefenseParameter();

			protected sys2d.Window targetWindow_;

			public BaseBattleCharacter()
			{
				hp_ = null;
				condition_ = null;
				body_ = null;
				bodyAndBonus_ = null;
				physicsDefense_ = null;
				magicDefense_ = null;
				provocationCharacter_ = null;
				handAttack_[0] = null;
				handAttack_[1] = null;
			}

			public BaseBattleCharacter(BaseBattleCharacter src)
			{
				isEnable_ = src.isEnable_;
				isActionEnd_ = src.isActionEnd_;
				battleCharacterId_ = src.battleCharacterId_;
				actionId_ = src.actionId_;
				prevActionId_ = src.prevActionId_;
				actionNumber_ = src.actionNumber_;
				breed_ = src.breed_;
				characterMngId_ = src.characterMngId_;
				for (int i = 0; i < 12; i++)
				{
					targetId_[i] = src.targetId_[i];
				}
				lastTargetId_ = src.lastTargetId_;
				rollUpLevel_ = src.rollUpLevel_;
				flag_ = src.flag_;
				magicFlag_ = src.magicFlag_;
				for (int i = 0; i < 13; i++)
				{
					effectId_[i] = src.effectId_[i];
				}
				moveYaw_ = src.moveYaw_;
				movePitch_ = src.movePitch_;
				useMagicId_ = src.useMagicId_;
				nowMagicAttackNumber_ = src.nowMagicAttackNumber_;
				useItemId_ = src.useItemId_;
				frameCounter_ = src.frameCounter_;
				attackNumber_ = src.attackNumber_;
				overissueNumber_ = src.overissueNumber_;
				changeCondition_.copy(src.changeCondition_);
				for (int i = 0; i < 2; i++)
				{
					attackSuccess_[i] = src.attackSuccess_[i];
				}
				for (int i = 0; i < 6; i++)
				{
					conditionTime_[i] = src.conditionTime_[i];
				}
				provocationCharacterId_ = src.provocationCharacterId_;
				provocationCharacter_ = src.provocationCharacter_;
				reflectTargetId_ = src.reflectTargetId_;
				changeColorFrame_ = src.changeColorFrame_;
				nowColor_ = src.nowColor_;
				for (int i = 0; i < 6; i++)
				{
					prevColor_[i] = src.prevColor_[i];
				}
				reflect_ = src.reflect_;
				prevMagicid_ = src.prevMagicid_;
				level_ = src.level_;
				weight_ = src.weight_;
				for (int i = 0; i < 2; i++)
				{
					weaponId_[i] = src.weaponId_[i];
				}
				hp_ = src.hp_;
				condition_ = src.condition_;
				body_ = src.body_;
				bodyAndBonus_ = src.bodyAndBonus_;
				for (int i = 0; i < 2; i++)
				{
					handAttack_[i] = src.handAttack_[i];
				}
				physicsDefense_ = src.physicsDefense_;
				magicDefense_ = src.magicDefense_;
				tempBody_.copy(src.tempBody_);
				tempAttack_.copy(src.tempAttack_);
				tempDefense_.copy(src.tempDefense_);
				targetWindow_ = src.targetWindow_;
			}

			public void initialize()
			{
				isEnable_ = false;
				isActionEnd_ = false;
				battleCharacterId_ = -1;
				actionId_ = -1;
				actionNumber_ = 0;
				breed_ = 0;
				reflect_ = false;
				characterMngId_ = -1;
				clearTargetId();
				clearFlagAll();
				clearMagicFlagAll();
				clearEffectIdAll();
				clearUseMagicId();
				frameCounter_ = -1;
				changeCondition_.clearCondition();
				clearRollUpLevel();
				clearConditionTimeAll();
				changeColorFrame_ = 0;
				clearNowColor();
			}

			public void clearTargetId()
			{
				for (int i = 0; i < 12; i++)
				{
					targetId_[i] = -1;
				}
			}

			public int unusedTargetId()
			{
				for (int i = 0; i < 12; i++)
				{
					if (targetId(i) < 0)
					{
						return i;
					}
				}
				return -1;
			}

			public int setTargetIdMyself()
			{
				for (int i = 0; i < 12; i++)
				{
					if (targetId(i) == battleCharacterId())
					{
						return i;
					}
				}
				int num = unusedTargetId();
				setTargetId(num, battleCharacterId());
				return num;
			}

			public bool isTargetId()
			{
				for (int i = 0; i < 12; i++)
				{
					if (targetId(i) >= 0)
					{
						return true;
					}
				}
				return false;
			}

			public bool checkTargetId(int _id)
			{
				for (int i = 0; i < 12; i++)
				{
					if (targetId(i) == _id)
					{
						return true;
					}
				}
				return false;
			}

			public void clearBattleFlag()
			{
				if (isBattle())
				{
					offIsActionEnd();
				}
				else
				{
					onIsActionEnd();
				}
				clearFlag(PLAYER_FLAG.PF_ESCAPE);
				clearFlag(PLAYER_FLAG.PF_GUARD);
				clearFlag(PLAYER_FLAG.PF_DEAD);
				clearFlag(PLAYER_FLAG.PF_MORE_GUARD);
				clearFlag(PLAYER_FLAG.PF_MISS);
				clearFlag(PLAYER_FLAG.PF_EFFECT);
				clearFlag(PLAYER_FLAG.PF_CREATE_EFFECT);
				clearFlag(PLAYER_FLAG.PF_RECOVER);
				clearFlag(PLAYER_FLAG.PF_FINISH);
				clearFlag(PLAYER_FLAG.PF_2D);
				clearFlag(PLAYER_FLAG.PF_MAGIC_LOOP_END);
				clearFlag(PLAYER_FLAG.PF_CRITICAL);
				clearFlag(PLAYER_FLAG.PF_CREATE_2D);
				clearFlag(PLAYER_FLAG.PF_COUNTER);
				clearFlag(PLAYER_FLAG.PF_ROLL_UP);
				clearFlag(PLAYER_FLAG.PF_EXPLOSION);
				clearFlag(PLAYER_FLAG.PF_PROVOCATION);
				clearFlag(PLAYER_FLAG.PF_KNOCK_OVER);
				clearFlag(PLAYER_FLAG.PF_TAKE_A_POWDER);
				clearFlag(PLAYER_FLAG.PF_POISON_NOW);
				frameCounter_ = -1;
			}

			public void clearSongFlag()
			{
			}

			public void clearTarget()
			{
				clearTargetId();
			}

			public void setTargetType(PLAYER_FLAG flag)
			{
				clearTargetType();
				setFlag(flag);
			}

			public void clearTargetType()
			{
				clearFlag(PLAYER_FLAG.PF_TARGET_PLAYER);
				clearFlag(PLAYER_FLAG.PF_TARGET_MONSTER);
			}

			public int unUsedEffectId()
			{
				for (int i = 0; i < 13; i++)
				{
					if (effectId(i) == -1)
					{
						return i;
					}
				}
				return -1;
			}

			public void checkClearEffectId()
			{
				for (int i = 0; i < 13; i++)
				{
					if (effectId(i) != -1 && !BattleEffect.instance().isEffectObject(effectId(i)))
					{
						effectId_set(i, -1);
					}
				}
			}

			public bool isClearAllEffect()
			{
				for (int i = 0; i < 13; i++)
				{
					if (effectId(i) != -1 && BattleEffect.instance().isEffectObject(effectId(i)))
					{
						return false;
					}
				}
				return true;
			}

			public void clearEffectIdAll()
			{
				for (int i = 0; i < 13; i++)
				{
					effectId_[i] = -1;
				}
			}

			public void calcFrameCounter()
			{
				if (frameCounter_ != -1)
				{
					frameCounter_++;
				}
			}

			public void setAttackNumber(int number)
			{
				attackNumber_ = number;
				_ = breed_;
			}

			public bool isEndOverissueNumber()
			{
				if (overissueNumber_ <= 0)
				{
					return false;
				}
				return true;
			}

			public void clearConditionTimeAll()
			{
				for (int i = 0; i < 6; i++)
				{
					conditionTime_[i] = 0;
				}
			}

			public void calcConditionTime()
			{
				for (int i = 0; i < 6; i++)
				{
					if (conditionTime_[i] <= 0)
					{
						continue;
					}
					conditionTime_[i]--;
					if (conditionTime_[i] == 0)
					{
						switch (i)
						{
						case 0:
							clearMagicFlag(MAGIC_FLAG.MF_PROTECT);
							break;
						case 1:
							clearMagicFlag(MAGIC_FLAG.MF_HASTE);
							break;
						case 2:
							clearMagicFlag(MAGIC_FLAG.MF_BAHAMUT);
							break;
						case 3:
							clearMagicFlag(MAGIC_FLAG.MF_SONG1);
							break;
						case 4:
							clearMagicFlag(MAGIC_FLAG.MF_SONG2);
							break;
						case 5:
							clearMagicFlag(MAGIC_FLAG.MF_SONG5);
							break;
						}
					}
				}
			}

			public ys.BodyParameter bodyAndBonus()
			{
				if (breed() == 2)
				{
					return bodyAndBonus_;
				}
				if (condition().isFrog() || condition().isLilliput())
				{
					tempBody_.initialize();
					tempBody_.strength().set(1);
					tempBody_.vitality().set(1);
					tempBody_.dexterity().set(bodyAndBonus_.dexterity().get());
					tempBody_.intelligence().set(bodyAndBonus_.intelligence().get());
					tempBody_.mind().set(bodyAndBonus_.mind().get());
					return tempBody_;
				}
				return bodyAndBonus_;
			}

			public ys.PhysicsAttackParameter handAttack(pl.HAND_TYPE type)
			{
				if (breed() == 2)
				{
					return handAttack_[(int)type];
				}
				if (condition().isFrog() || condition().isLilliput())
				{
					tempAttack_.initialize();
					tempAttack_ = handAttack_[(int)type];
					tempAttack_.aggressivity().set(1);
					return tempAttack_;
				}
				return handAttack_[(int)type];
			}

			public ys.PhysicsDefenseParameter physicsDefense()
			{
				if (breed() == 2)
				{
					return physicsDefense_;
				}
				if (condition().isFrog() || condition().isLilliput())
				{
					tempDefense_.initialize();
					tempDefense_ = physicsDefense_;
					tempDefense_.phylacticPower().set(1);
					return tempDefense_;
				}
				return physicsDefense_;
			}

			public void setConditionDeath()
			{
				condition().onDeath();
				hp().minNow();
			}

			public void setConditionNearDeath()
			{
				condition().onNearDeath();
				int t = hp().getNow() / 10;
				t = ds.clamp(t, 1, 9);
				t = (int)(ds.RandomNumber.rand32((uint)t) + 1);
				hp().setNow(t);
			}

			public int targetNumber()
			{
				int num = 0;
				for (int i = 0; i < 12; i++)
				{
					if (targetId(i) >= 0)
					{
						num++;
					}
				}
				return num;
			}

			public bool isGhost()
			{
				if (magicDefense() == null)
				{
					return false;
				}
				if ((magicDefense().weakType() & 1) != 0)
				{
					return true;
				}
				return false;
			}

			public bool isDrain()
			{
				if (useMagicId() == 4121 || useItemId() == 5115)
				{
					return true;
				}
				return false;
			}

			public bool isEsuna()
			{
				if (useMagicId() == 4020 || useItemId() == 5112)
				{
					return true;
				}
				return false;
			}

			public bool setShakeScreen()
			{
				int num = 0;
				if (useMagicId() == 4119)
				{
					num = pl.PlayerParty.instance().normalMagic(useMagicId()).effectPlayFrame();
				}
				else if (useMagicId() == 6507 || useMagicId() == 6508)
				{
					num = pl.PlayerParty.instance().normalMagic(useMagicId()).effectPlayFrame();
				}
				else if (useMagicId() == 6612)
				{
					num = mon.MonsterManager.instance().effectsInfo(useMagicId()).effectInfo(0)
						.playFrame_;
				}
				if (num > 0)
				{
					VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
					fnd_reuse_pos.x = 273;
					fnd_reuse_pos.y = 273;
					fnd_reuse_pos.z = 273;
					battleDisplay.readyShakeCamera(num, fnd_reuse_pos);
					return true;
				}
				return false;
			}

			public int isAbsorb()
			{
				int num = 0;
				if ((handAttack_[0].attackType() & 4) != 0)
				{
					num++;
				}
				if ((handAttack_[1].attackType() & 4) != 0)
				{
					num++;
				}
				return num;
			}

			public bool isSelectDeadOrStoneTarget(BaseBattleCharacter target)
			{
				if (useItemId() <= 0 && useMagicId() <= 0)
				{
					return false;
				}
				if (useMagicId() > 0)
				{
					itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(useMagicId());
					if (magicParameter != null)
					{
						if ((magicParameter.magicUseKind() & 1) != 0 && ((magicParameter.changeCondition() & 8) != 0 || (magicParameter.changeCondition() & 0x200) != 0))
						{
							return true;
						}
						if ((magicParameter.magicUseKind() & 1) != 0 && (magicParameter.magicType() & 1) != 0 && (magicParameter.changeCondition() & 0x200) == 0 && target != null && !target.isGhost())
						{
							return false;
						}
					}
				}
				if (useItemId() > 0)
				{
					itm.ConsumptionParameter consumptionParameter = itm.ItemManager.instance().consumptionParameter((short)useItemId());
					if (consumptionParameter != null && (consumptionParameter.itemType() & 1) != 0 && ((consumptionParameter.changeCondition() & 8) != 0 || (consumptionParameter.changeCondition() & 0x200) != 0))
					{
						return true;
					}
				}
				return reflect_;
			}

			public bool isSelectDeadOrStoneTargetCommand()
			{
				if (useItemId() <= 0 && useMagicId() <= 0)
				{
					return false;
				}
				if (useMagicId() > 0)
				{
					itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(useMagicId());
					if (magicParameter != null && (magicParameter.magicUseKind() & 1) != 0 && ((magicParameter.changeCondition() & 8) != 0 || (magicParameter.magicType() & 1) != 0 || (magicParameter.changeCondition() & 0x200) != 0))
					{
						return true;
					}
				}
				if (useItemId() > 0)
				{
					itm.ConsumptionParameter consumptionParameter = itm.ItemManager.instance().consumptionParameter((short)useItemId());
					if (consumptionParameter != null && (consumptionParameter.itemType() & 1) != 0 && ((consumptionParameter.changeCondition() & 8) != 0 || (consumptionParameter.changeCondition() & 0x200) != 0 || consumptionParameter.usedPower() > 0))
					{
						return true;
					}
				}
				return reflect_;
			}

			public bool isSelectDeadTarget()
			{
				if (useItemId() <= 0 && useMagicId() <= 0)
				{
					return false;
				}
				if (useMagicId() > 0)
				{
					itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(useMagicId());
					if (magicParameter != null && (magicParameter.magicUseKind() & 1) != 0 && ((magicParameter.magicType() & 1) != 0 || (magicParameter.changeCondition() & 0x200) != 0))
					{
						return true;
					}
				}
				if (useItemId() > 0)
				{
					itm.ConsumptionParameter consumptionParameter = itm.ItemManager.instance().consumptionParameter((short)useItemId());
					if (consumptionParameter != null && (consumptionParameter.itemType() & 1) != 0 && ((consumptionParameter.changeCondition() & 0x200) != 0 || consumptionParameter.usedPower() > 0))
					{
						return true;
					}
				}
				return reflect_;
			}

			public bool isSelectStoneTarget()
			{
				if (useItemId() <= 0 && useMagicId() <= 0)
				{
					return false;
				}
				if (useMagicId() > 0)
				{
					itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(useMagicId());
					if (magicParameter != null && (magicParameter.magicUseKind() & 1) != 0 && ((magicParameter.magicType() & 1) != 0 || (magicParameter.changeCondition() & 8) != 0))
					{
						return true;
					}
				}
				if (useItemId() > 0)
				{
					itm.ConsumptionParameter consumptionParameter = itm.ItemManager.instance().consumptionParameter((short)useItemId());
					if (consumptionParameter != null && (consumptionParameter.itemType() & 1) != 0 && ((consumptionParameter.changeCondition() & 8) != 0 || consumptionParameter.usedPower() > 0))
					{
						return true;
					}
				}
				return reflect_;
			}

			public void updateParameterMagicFlag(short magic_id, int jobSkill)
			{
				if (magic_id == 4015 && !magicFlag(MAGIC_FLAG.MF_PROTECT))
				{
					setFlag(PLAYER_FLAG.PF_2D);
					clearFlag(PLAYER_FLAG.PF_MISS);
					int num = physicsDefense().phylacticPower().get();
					num += num * 30 / 100;
					physicsDefense().phylacticPower().set(num);
					int num2 = magicDefense().magicPhylacticPower();
					num2 += num2 * 30 / 100;
					magicDefense().magicPhylacticPower_set((short)num2);
					setMagicFlag(MAGIC_FLAG.MF_PROTECT);
					setNowColor(1);
					conditionTime_[0] = 10 + jobSkill / 12;
				}
				if (magic_id == 4018 && !magicFlag(MAGIC_FLAG.MF_HASTE))
				{
					setFlag(PLAYER_FLAG.PF_2D);
					clearFlag(PLAYER_FLAG.PF_MISS);
					byte b = (byte)bodyAndBonus().dexterity().get();
					b += (byte)(b * 40 / 100);
					bodyAndBonus().dexterity().set(b);
					setMagicFlag(MAGIC_FLAG.MF_HASTE);
					setNowColor(2);
					conditionTime_[1] = 5 + jobSkill / 24;
				}
				if (magic_id == 4208 && !magicFlag(MAGIC_FLAG.MF_BAHAMUT))
				{
					setFlag(PLAYER_FLAG.PF_2D);
					clearFlag(PLAYER_FLAG.PF_MISS);
					for (int i = 0; i < 2; i++)
					{
						int num3 = handAttack((pl.HAND_TYPE)i).aggressivity().get();
						num3 += num3 * 50 / 100;
						handAttack((pl.HAND_TYPE)i).aggressivity().set(num3);
					}
					setMagicFlag(MAGIC_FLAG.MF_BAHAMUT);
					conditionTime_[2] = 10 + jobSkill / 12;
				}
				if (magic_id == 6001 && !magicFlag(MAGIC_FLAG.MF_SONG1))
				{
					setFlag(PLAYER_FLAG.PF_2D);
					clearFlag(PLAYER_FLAG.PF_MISS);
					int num4 = physicsDefense().phylacticPower().get();
					num4 += num4 * (20 + jobSkill * 10 / 100) / 100;
					physicsDefense().phylacticPower().set(num4);
					setMagicFlag(MAGIC_FLAG.MF_SONG1);
					setNowColor(8);
					conditionTime_[3] = 2;
				}
				if (magic_id == 6002 && !magicFlag(MAGIC_FLAG.MF_SONG2))
				{
					setFlag(PLAYER_FLAG.PF_2D);
					clearFlag(PLAYER_FLAG.PF_MISS);
					for (int j = 0; j < 2; j++)
					{
						int num5 = handAttack((pl.HAND_TYPE)j).aggressivity().get();
						num5 += num5 * (20 + jobSkill * 10 / 100) / 100;
						handAttack((pl.HAND_TYPE)j).aggressivity().set(num5);
					}
					setMagicFlag(MAGIC_FLAG.MF_SONG2);
					setNowColor(16);
					conditionTime_[4] = 2;
				}
				if (magic_id == 6005 && !magicFlag(MAGIC_FLAG.MF_SONG5))
				{
					setFlag(PLAYER_FLAG.PF_2D);
					clearFlag(PLAYER_FLAG.PF_MISS);
					setMagicFlag(MAGIC_FLAG.MF_SONG5);
					setNowColor(32);
					conditionTime_[5] = 2;
				}
			}

			public void reupdateParameter(int jobSkill)
			{
				if (magicFlag(MAGIC_FLAG.MF_PROTECT))
				{
					int num = physicsDefense().phylacticPower().get();
					num += num * 30 / 100;
					physicsDefense().phylacticPower().set(num);
					int num2 = magicDefense().magicPhylacticPower();
					num2 += num2 * 30 / 100;
					magicDefense().magicPhylacticPower_set((short)num2);
				}
				if (magicFlag(MAGIC_FLAG.MF_HASTE))
				{
					byte b = (byte)bodyAndBonus().dexterity().get();
					b += (byte)(b * 40 / 100);
					bodyAndBonus().dexterity().set(b);
				}
				if (magicFlag(MAGIC_FLAG.MF_BAHAMUT))
				{
					for (int i = 0; i < 2; i++)
					{
						int num3 = handAttack((pl.HAND_TYPE)i).aggressivity().get();
						num3 += num3 * 50 / 100;
						handAttack((pl.HAND_TYPE)i).aggressivity().set(num3);
					}
				}
				if (magicFlag(MAGIC_FLAG.MF_SONG1))
				{
					int num4 = physicsDefense().phylacticPower().get();
					num4 += num4 * (20 + jobSkill * 10 / 100) / 100;
					physicsDefense().phylacticPower().set(num4);
				}
				if (magicFlag(MAGIC_FLAG.MF_SONG2))
				{
					for (int j = 0; j < 2; j++)
					{
						int num5 = handAttack((pl.HAND_TYPE)j).aggressivity().get();
						num5 += num5 * (20 + jobSkill * 10 / 100) / 100;
						handAttack((pl.HAND_TYPE)j).aggressivity().set(num5);
					}
				}
			}

			public void changePlayerColor()
			{
				if (isChangeColorMagic())
				{
					if (characterMng.isEnableLight(characterMngId()))
					{
						characterMng.disableLight(characterMngId());
					}
					setMagicColor();
					return;
				}
				if (!characterMng.isEnableLight(characterMngId()))
				{
					if (breed() == 0)
					{
						characterMng.setEmission(characterMngId(), GX_RGB(31, 31, 31));
					}
					else if (breed() == 1)
					{
						characterMng.setEmission(characterMngId(), GX_RGB(0, 0, 0));
					}
					characterMng.enableLight(characterMngId());
				}
				clearNowColor();
			}

			public bool isChangeColorMagic()
			{
				if (magicFlag(MAGIC_FLAG.MF_PROTECT) || magicFlag(MAGIC_FLAG.MF_HASTE) || magicFlag(MAGIC_FLAG.MF_REFLECT) || magicFlag(MAGIC_FLAG.MF_BAHAMUT) || magicFlag(MAGIC_FLAG.MF_SONG1) || magicFlag(MAGIC_FLAG.MF_SONG2) || magicFlag(MAGIC_FLAG.MF_SONG5))
				{
					return true;
				}
				return false;
			}

			public void setMagicColor()
			{
				if (nowColor() == 1 || nowColor() == 8)
				{
					characterMng.setEmission(characterMngId(), GX_RGB(29, 31, 10));
					changeColorFrame_ = (short)CHANGE_MAGIC_COLOR_FRAME;
				}
				else if (nowColor() == 2)
				{
					characterMng.setEmission(characterMngId(), GX_RGB(31, 15, 15));
					changeColorFrame_ = (short)CHANGE_MAGIC_COLOR_FRAME;
				}
				else if (nowColor() == 64)
				{
					characterMng.setEmission(characterMngId(), GX_RGB(16, 31, 16));
					changeColorFrame_ = (short)CHANGE_MAGIC_COLOR_FRAME;
				}
				else if (nowColor() == 4 || nowColor() == 16)
				{
					characterMng.setEmission(characterMngId(), GX_RGB(31, 19, 5));
					changeColorFrame_ = (short)CHANGE_MAGIC_COLOR_FRAME;
				}
				else if (nowColor() == 32)
				{
					characterMng.setEmission(characterMngId(), GX_RGB(16, 31, 31));
					changeColorFrame_ = (short)CHANGE_MAGIC_COLOR_FRAME;
				}
			}

			public void setNowColor(int color)
			{
				for (int num = 5; num > 0; num--)
				{
					prevColor_[num] = prevColor_[num - 1];
				}
				prevColor_[0] = nowColor_;
				nowColor_ = color;
			}

			public int nowColor()
			{
				if (magicFlag((MAGIC_FLAG)nowColor_))
				{
					return nowColor_;
				}
				for (int i = 0; i < 5; i++)
				{
					if (magicFlag((MAGIC_FLAG)prevColor_[i]))
					{
						nowColor_ = prevColor_[i];
						return nowColor_;
					}
					prevColor_[i] = 0;
				}
				nowColor_ = 0;
				return 0;
			}

			public void clearNowColor()
			{
				for (int i = 0; i < 6; i++)
				{
					prevColor_[i] = 0;
				}
				nowColor_ = 0;
			}

			public bool isCondition(short bad_condition)
			{
				if ((bad_condition & 1) != 0 && condition().isParalysis())
				{
					return true;
				}
				if ((bad_condition & 2) != 0 && condition().isSleep())
				{
					return true;
				}
				if ((bad_condition & 4) != 0 && condition().isConfusion())
				{
					return true;
				}
				if ((bad_condition & 8) != 0)
				{
					if (condition().isStone())
					{
						return true;
					}
					if (condition().isNearStone())
					{
						return true;
					}
				}
				if ((bad_condition & 0x10) != 0 && condition().isFrog() && !condition().isDeath() && !condition().isStone())
				{
					return true;
				}
				if ((bad_condition & 0x20) != 0 && condition().isSilence())
				{
					return true;
				}
				if ((bad_condition & 0x40) != 0 && condition().isLilliput() && !condition().isDeath() && !condition().isStone())
				{
					return true;
				}
				if ((bad_condition & 0x80) != 0 && condition().isDarkness())
				{
					return true;
				}
				if ((bad_condition & 0x100) != 0 && condition().isPoison())
				{
					return true;
				}
				if ((bad_condition & 0x200) != 0 && condition().isDeath())
				{
					return true;
				}
				if ((bad_condition & 0x400) != 0 && condition().isNearDeath())
				{
					return true;
				}
				return false;
			}

			public bool isEnable()
			{
				return isEnable_;
			}

			public void onIsEnable()
			{
				isEnable_ = true;
			}

			public void offIsEnable()
			{
				isEnable_ = false;
			}

			public bool isActionEnd()
			{
				return isActionEnd_;
			}

			public void onIsActionEnd()
			{
				isActionEnd_ = true;
			}

			public void offIsActionEnd()
			{
				isActionEnd_ = false;
			}

			public short battleCharacterId()
			{
				return battleCharacterId_;
			}

			public void setBattleCharacterId(short _id)
			{
				battleCharacterId_ = _id;
			}

			public int actionId()
			{
				return actionId_;
			}

			public void actionId_set(int arg0)
			{
				actionId_ = arg0;
			}

			public void setActionId(int _id)
			{
				actionId_ = _id;
			}

			public int prevActionId()
			{
				return prevActionId_;
			}

			public void setPrevActionId()
			{
				prevActionId_ = actionId_;
			}

			public int actionNumber()
			{
				return actionNumber_;
			}

			public void actionNumber_set(int arg0)
			{
				actionNumber_ = arg0;
			}

			public void actionNumber_dec()
			{
				actionNumber_--;
			}

			public void setActionNumber(int number)
			{
				actionNumber_ = number;
			}

			public byte breed()
			{
				return breed_;
			}

			public void breed_set(byte arg0)
			{
				breed_ = arg0;
			}

			public void setBreed(byte breed)
			{
				breed_ = breed;
			}

			public int characterMngId()
			{
				return characterMngId_;
			}

			public void characterMngId_set(int arg0)
			{
				characterMngId_ = arg0;
			}

			public void setCharacterMngId(int _id)
			{
				characterMngId_ = _id;
			}

			public short targetId(int i)
			{
				return targetId_[i];
			}

			public void targetId_set(int i, short arg0)
			{
				targetId_[i] = arg0;
			}

			public void setTargetId(int i, short _id)
			{
				targetId_[i] = _id;
			}

			public short lastTargetId()
			{
				return lastTargetId_;
			}

			public void setLastTargetId()
			{
				lastTargetId_ = targetId(0);
			}

			public void clearLastTargetId()
			{
				lastTargetId_ = -1;
			}

			public short rollUpLevel()
			{
				return rollUpLevel_;
			}

			public void setRollUpLevel(short level)
			{
				rollUpLevel_ = level;
			}

			public void clearRollUpLevel()
			{
				rollUpLevel_ = 0;
			}

			public short rollUpLevelUp()
			{
				return ++rollUpLevel_;
			}

			public bool isLimitBreakRollUpLevelUp()
			{
				if (rollUpLevelUp() <= ROLL_UP_LEVEL_MAX)
				{
					return false;
				}
				return true;
			}

			public void setFlag(PLAYER_FLAG flag)
			{
				flag_ |= (uint)flag;
			}

			public void clearFlag(PLAYER_FLAG flag)
			{
				flag_ &= (uint)(~flag);
			}

			public bool flag(PLAYER_FLAG flag)
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

			public void setMagicFlag(MAGIC_FLAG flag)
			{
				magicFlag_ |= (uint)flag;
			}

			public void clearMagicFlag(MAGIC_FLAG flag)
			{
				magicFlag_ &= (uint)(~flag);
			}

			public bool magicFlag(MAGIC_FLAG flag)
			{
				if ((magicFlag_ & (uint)flag) == 0)
				{
					return false;
				}
				return true;
			}

			public void clearMagicFlagAll()
			{
				magicFlag_ = 0u;
			}

			public uint magicFlagAll()
			{
				return magicFlag_;
			}

			public int effectId(int i)
			{
				return effectId_[i];
			}

			public void effectId_set(int i, int arg0)
			{
				effectId_[i] = arg0;
			}

			public void setEffectId(int i, int effectId)
			{
				effectId_[i] = effectId;
			}

			public int moveYaw()
			{
				return moveYaw_;
			}

			public void moveYaw_set(int arg0)
			{
				moveYaw_ = arg0;
			}

			public void moveYaw_add(int arg0)
			{
				moveYaw_ += arg0;
			}

			public void setMoveYaw(int yaw)
			{
				moveYaw_ = yaw;
			}

			public int movePitch()
			{
				return movePitch_;
			}

			public void setMovePitch(int pitch)
			{
				movePitch_ = pitch;
			}

			public short useMagicId()
			{
				return useMagicId_;
			}

			public void setUseMagicId(short _id)
			{
				useMagicId_ = _id;
			}

			public void clearUseMagicId()
			{
				useMagicId_ = 0;
			}

			public int nowMagicAttackNumber()
			{
				return nowMagicAttackNumber_;
			}

			public int useItemId()
			{
				return useItemId_;
			}

			public void setUseItemId(int itemId)
			{
				useItemId_ = itemId;
			}

			public void clearUseItemId()
			{
				useItemId_ = 0;
			}

			public int frameCounter()
			{
				return frameCounter_;
			}

			public void zeroClearFrameCounter()
			{
				frameCounter_ = 0;
			}

			public void initializeFrameCounter()
			{
				frameCounter_ = -1;
			}

			public ys.Condition changeCondition()
			{
				return changeCondition_;
			}

			public int attackNumber()
			{
				return attackNumber_;
			}

			public void setOverissueNumber(int number)
			{
				overissueNumber_ = number;
			}

			public int overissueNumber()
			{
				return overissueNumber_;
			}

			public void decOverissueNumber()
			{
				overissueNumber_--;
			}

			public void clearOverissueNumber()
			{
				overissueNumber_ = 0;
			}

			public bool attackSuccess(int hand_type)
			{
				return attackSuccess_[hand_type];
			}

			public void setAttackSuccess(int hand_type, bool flag)
			{
				attackSuccess_[hand_type] = flag;
			}

			public int conditionTime(int i)
			{
				return conditionTime_[i];
			}

			public int provocationCharacterId()
			{
				return provocationCharacterId_;
			}

			public void setProvocationCharacterId(int _id)
			{
				provocationCharacterId_ = _id;
			}

			public BaseBattleCharacter provocationCharacter()
			{
				return provocationCharacter_;
			}

			public void setProvocationCharacter(BaseBattleCharacter attacker)
			{
				provocationCharacter_ = attacker;
			}

			public int reflectTargetId()
			{
				return reflectTargetId_;
			}

			public void setReflectTargetId(int _id)
			{
				reflectTargetId_ = _id;
			}

			public bool reflectFlag()
			{
				return reflect_;
			}

			public void setReflectFlag(bool flag)
			{
				reflect_ = flag;
			}

			public void setPrevMagicId(short _id)
			{
				prevMagicid_ = _id;
			}

			public short prevMagicId()
			{
				return prevMagicid_;
			}

			public byte level()
			{
				return level_;
			}

			public void setLevel(byte level)
			{
				level_ = level;
			}

			public byte weight()
			{
				return weight_;
			}

			public void setWeight(byte weight)
			{
				weight_ = weight;
			}

			public short weaponId(pl.HAND_TYPE handType)
			{
				return weaponId_[(int)handType];
			}

			public void setWeaponId(pl.HAND_TYPE handType, short _id)
			{
				weaponId_[(int)handType] = _id;
			}

			public ys.MPoint<int> hp()
			{
				return hp_;
			}

			public void setHp(ys.MPoint<int> hp)
			{
				hp_ = hp;
			}

			public ys.Condition condition()
			{
				return condition_;
			}

			public void setCondition(ys.Condition con)
			{
				condition_ = con;
			}

			public ys.BodyParameter body()
			{
				return body_;
			}

			public void setBody(ys.BodyParameter body)
			{
				body_ = body;
			}

			public void setBodyAndBonus(ys.BodyParameter bonus)
			{
				bodyAndBonus_ = bonus;
			}

			public void setHandAttack(pl.HAND_TYPE type, ys.PhysicsAttackParameter atk)
			{
				handAttack_[(int)type] = atk;
			}

			public void setPhysicsDefense(ys.PhysicsDefenseParameter phy)
			{
				physicsDefense_ = phy;
			}

			public ys.MagicDefenseParameter magicDefense()
			{
				return magicDefense_;
			}

			public void setMagicDefense(ys.MagicDefenseParameter magic)
			{
				magicDefense_ = magic;
			}

			public virtual void resetParameterMagicFlag()
			{
			}

			public virtual bool isBattle()
			{
				return true;
			}

			public virtual bool isAction()
			{
				return true;
			}

			public sys2d.Window targetWindow()
			{
				return targetWindow_;
			}

			public void targetWindow_set(sys2d.Window arg0)
			{
				targetWindow_ = arg0;
			}
		}
	}
}
