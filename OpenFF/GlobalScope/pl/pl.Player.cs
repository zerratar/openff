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
	public static partial class pl
	{
		public class Player
		{
			public enum ITEM_TYPE
			{
				WEAPON,
				PROTECTION,
				ITEM_TYPE_MAX
			}

			public const int PENALTY_X = 0;

			public const int PENALTY_Y = 1;

			public const int PENALTY_TYPE_MAX = 2;

			public const int INVALID_VALUE = -1;

			public const int RED_ZONE = 0;

			public const int YELLOW_ZONE = 25;

			private static int[][] JobPenaltyTimeCount = new int[2][]
			{
				new int[23]
				{
					0, 0, -1, -1, 1, 1, 0, -1, 0, -1,
					2, 3, -3, -3, 0, 3, 3, -4, 4, 4,
					4, 4, -2
				},
				new int[23]
				{
					0, 0, 1, -1, 1, -1, 2, 3, 4, -3,
					2, -3, 3, -3, -4, 0, 3, 0, 4, -4,
					0, 2, -4
				}
			};

			private bool isEnable_;

			private string name_ = "";

			private byte playerId_;

			private ys.ParameterPoint<int> level_ = new ys.ParameterPoint<int>(1, 99);

			private ys.ParameterPoint<int> exp_ = new ys.ParameterPoint<int>(0, 9999999);

			private ys.ParameterPoint<int> capacity_ = new ys.ParameterPoint<int>(0, 99);

			private ys.MPoint<int> hp_ = new ys.MPoint<int>(0, 999999);

			private ys.MPoint<int>[] mp_ = new ys.MPoint<int>[8];

			private byte[] jobChangeMp_ = new byte[8];

			private ys.BodyParameter body_ = new ys.BodyParameter();

			private ys.BodyParameter bodyAndBonus_ = new ys.BodyParameter();

			private byte formationType_;

			private ys.Condition condition_ = new ys.Condition();

			private ys.PhysicsAttackParameter[] handAttack_ = new ys.PhysicsAttackParameter[2];

			private ys.PhysicsDefenseParameter physicsDefense_ = new ys.PhysicsDefenseParameter();

			private ys.MagicDefenseParameter magicDefense_ = new ys.MagicDefenseParameter();

			private PlayerEquipParameter equipParameter_ = new PlayerEquipParameter();

			private PlayerJobManager jobManager_ = new PlayerJobManager();

			private PlayerSkillManager skillManager_ = new PlayerSkillManager();

			private int jobPenaltyTime_;

			public Player()
			{
				for (int i = 0; i < handAttack_.Length; i++)
				{
					handAttack_[i] = new ys.PhysicsAttackParameter();
				}
				for (int i = 0; i < mp_.Length; i++)
				{
					mp_[i] = new ys.MPoint<int>(0, 99);
				}
			}

			public Player(Player src)
				: this()
			{
				copy(src);
			}

			public void initialize(int playerId)
			{
				offIsEnable();
				getPlayerInitialName(ref name_, playerId);
				playerId_ = (byte)playerId;
				level_.min();
				exp_.min();
				capacity_.min();
				hp_.setLimit(DefaultHp);
				hp_.maxNow();
				body_.initialize();
				bodyAndBonus_.initialize();
				formationType_ = 0;
				condition_.clearCondition();
				for (int i = 0; i < 2; i++)
				{
					handAttack_[i].initialize();
				}
				physicsDefense_.initialize();
				magicDefense_.initialize();
				equipParameter_.initialize();
				jobManager_.initialize();
				skillManager_.initialize();
				jobPenaltyTime_ = 0;
				if (OpenFF.Client.GameProfile.Ff3Party)
				{
					changeJob(JOB_TYPE.SUPPINN);
				}
				for (int j = 0; j < 8; j++)
				{
					mp_[j].maxNow();
					jobChangeMp_[j] = (byte)mp_[j].getNow();
				}
			}

			public ys.BodyParameter bodyAndBonus()
			{
				if (condition().isFrog() || condition().isLilliput())
				{
					bodyAndBonus_.strength().set(1);
					bodyAndBonus_.vitality().set(1);
				}
				return bodyAndBonus_;
			}

			public void setExp(byte level)
			{
				exp_.set(PlayerParty.instance().playerExp()[0].exp(level));
			}

			public bool levelUp(int exp)
			{
				byte b = (byte)level_.get();
				if (b == PLAYER_LEVEL_MAX)
				{
					return false;
				}
				exp_.add(exp);
				bool result = false;
				while (b <= PLAYER_LEVEL_MAX && exp_.get() >= PlayerParty.instance().playerExp()[0].exp(b))
				{
					setParameter((byte)(b - 1));
					setMp((byte)(b - 1));
					setHp(b);
					result = true;
					b++;
				}
				level_.set(b);
				return result;
			}

			public void setParameter(byte lv)
			{
				int job = jobManager().nowJob();
				byte growType = PlayerParty.instance().jobGrowUpType()[0].growUpType(job, 0);
				body_.strength().set(PlayerParty.instance().growUp()[0].parameter(growType, lv));
				growType = PlayerParty.instance().jobGrowUpType()[0].growUpType(job, 1);
				body_.vitality().set(PlayerParty.instance().growUp()[0].parameter(growType, lv));
				growType = PlayerParty.instance().jobGrowUpType()[0].growUpType(job, 2);
				body_.dexterity().set(PlayerParty.instance().growUp()[0].parameter(growType, lv));
				growType = PlayerParty.instance().jobGrowUpType()[0].growUpType(job, 3);
				body_.intelligence().set(PlayerParty.instance().growUp()[0].parameter(growType, lv));
				growType = PlayerParty.instance().jobGrowUpType()[0].growUpType(job, 4);
				body_.mind().set(PlayerParty.instance().growUp()[0].parameter(growType, lv));
			}

			public void setHp(byte lv)
			{
				int num = (int)ds.RandomNumber.rand32((uint)(body_.vitality().get() / 2));
				OS_Printf("HP LIMIT %d LEVEL %d VITALITY %d + ds.RandomNumber.rand32 %d\n", hp_.getLimit(), lv, body_.vitality().get(), num);
				int num2 = lv + body_.vitality().get() + num;
				hp_.setLimit(hp_.getLimit() + num2);
				hp_.setLimit(ds.min(hp_.getLimit(), PLAYER_HP_MAX));
				OS_Printf("HP LIMIT %d\n", hp_.getLimit());
			}

			public void setMp(byte lv)
			{
				int job = jobManager().nowJob();
				byte b = PlayerParty.instance().jobGrowUpType()[0].growUpType(job, 5);
				if (b == 0)
				{
					for (int i = 0; i < 8; i++)
					{
						mp(i).setLimit(0);
						mp(i).setNow(0);
					}
				}
				else
				{
					for (int j = 0; j < 8; j++)
					{
						mp(j).setLimit(PlayerParty.instance().growUpMp(b - 1)[0].mp(lv, j));
						mp(j).addNow(0);
					}
				}
				// PORT: the limits are the tables' again; a mastery hero's MP boost (FF5's MP +n) goes back on.
				OpenFF.Client.ProgressionLayer.OnMpReset(this);
			}

			public void growParameter(byte level)
			{
				int num = level_.get();
				if (num == level)
				{
					return;
				}
				num++;
				if (num > level)
				{
					num = 1;
					hp_.setLimit(DefaultHp);
					hp_.maxNow();
				}
				for (; num <= level; num++)
				{
					setExp((byte)(num - 1));
					setParameter((byte)(num - 1));
					setMp((byte)(num - 1));
					if (num > 1)
					{
						setHp((byte)num);
					}
				}
				if (num != level)
				{
					num = level;
				}
				level_.set((byte)num);
			}

			public byte deftness()
			{
				return (byte)body_.dexterity().get();
			}

			public byte avoidance()
			{
				return (byte)body_.dexterity().get();
			}

			public void setHandAttack(HAND_TYPE hand)
			{
				itm.WeaponParameter weaponParameter = null;
				if ((equipParameter().equipHand(HAND_TYPE.RIGHT_HAND).itemId() == -99 || equipParameter().equipHand(HAND_TYPE.RIGHT_HAND).itemId() <= 0) && (equipParameter().equipHand(HAND_TYPE.LEFT_HAND).itemId() == -99 || equipParameter().equipHand(HAND_TYPE.LEFT_HAND).itemId() <= 0))
				{
					handAttack_[(int)hand].initialize();
					handAttack_[(int)hand].aggressivity().set(bodyAndBonus_.strength().get());
					JOB_TYPE jOB_TYPE = static_cast<JOB_TYPE>(jobManager_.nowJob());
					if (jOB_TYPE == JOB_TYPE.MONK || jOB_TYPE == JOB_TYPE.KARATE_MASTER)
					{
						handAttack_[(int)hand].aggressivity().add(jobManager_.nowJobParameter().skill().skillLevel()
							.get());
					}
					return;
				}
				weaponParameter = ((equipParameter().equipHand(hand).itemId() != -99 && equipParameter().equipHand(hand).itemId() > 0 && equipParameter().equipHand(hand).checkCategory() == itm.CATEGORY.CATEGORY_WEAPON) ? itm.ItemManager.instance().weaponParameter(equipParameter().equipHand(hand).itemId()) : itm.ItemManager.instance().weaponParameter(1000));
				if (weaponParameter != null)
				{
					handAttack_[(int)hand].aggressivity().set(weaponParameter.aggressivity());
					handAttack_[(int)hand].hitProbability_set(weaponParameter.hitProbability());
					handAttack_[(int)hand].optionProbability_set(weaponParameter.optionProbability());
					handAttack_[(int)hand].optionMagicId_set(weaponParameter.optionMagicItemId());
					handAttack_[(int)hand].armsAttribute_set(weaponParameter.armsAttribute());
					handAttack_[(int)hand].attackType_set(weaponParameter.atckType());
					handAttack_[(int)hand].attackOption_set(weaponParameter.atckOption());
					handAttack_[(int)hand].equipOption_set(weaponParameter.equipOption());
				}
				else
				{
					handAttack_[(int)hand].initialize();
				}
			}

			public void setPhysicsDefense(itm.ProtectionParameter protect)
			{
				if (protect != null)
				{
					physicsDefense_.phylacticPower().add(protect.phylacticPower());
					physicsDefense_.avoidanceNumber_add(protect.evasionNum());
					physicsDefense_.armsWeakAttribute_or(protect.armsWeakAttribute());
					physicsDefense_.armsAttribute_or(protect.armsAttribute());
					physicsDefense_.antiType_or(protect.antiType());
					physicsDefense_.antiOption_or(protect.antiOption());
				}
			}

			public void setMagicDefense(itm.ProtectionParameter protect)
			{
				if (protect != null)
				{
					magicDefense_.weakType_or(protect.weakType());
					magicDefense_.magicPhylacticPower_add(protect.magicPhylacticPower());
				}
			}

			public void calcPhysicsDefense()
			{
				setPhysicsDefense(itm.ItemManager.instance().protectionParameter(equipParameter().equipHead().itemId()));
				setPhysicsDefense(itm.ItemManager.instance().protectionParameter(equipParameter().equipBody().itemId()));
				setPhysicsDefense(itm.ItemManager.instance().protectionParameter(equipParameter().equipArm().itemId()));
				setPhysicsDefense(itm.ItemManager.instance().protectionParameter(equipParameter().equipHand(HAND_TYPE.RIGHT_HAND).itemId()));
				setPhysicsDefense(itm.ItemManager.instance().protectionParameter(equipParameter().equipHand(HAND_TYPE.LEFT_HAND).itemId()));
			}

			public void calcMagicDefense()
			{
				setMagicDefense(itm.ItemManager.instance().protectionParameter(equipParameter().equipHead().itemId()));
				setMagicDefense(itm.ItemManager.instance().protectionParameter(equipParameter().equipBody().itemId()));
				setMagicDefense(itm.ItemManager.instance().protectionParameter(equipParameter().equipArm().itemId()));
				setMagicDefense(itm.ItemManager.instance().protectionParameter(equipParameter().equipHand(HAND_TYPE.RIGHT_HAND).itemId()));
				setMagicDefense(itm.ItemManager.instance().protectionParameter(equipParameter().equipHand(HAND_TYPE.LEFT_HAND).itemId()));
			}

			public void setBonus(itm.ItemBaseParameter item)
			{
				if (item != null)
				{
					bodyAndBonus().strength().add(item.strength());
					bodyAndBonus().vitality().add(item.vitality());
					bodyAndBonus().dexterity().add(item.dexterity());
					bodyAndBonus().intelligence().add(item.intellect());
					bodyAndBonus().mind().add(item.mind());
				}
			}

			public void calcBonus()
			{
				bodyAndBonus_.copy(body_);
				setBonus(itm.ItemManager.instance().itemParameter(equipParameter().equipHead().itemId()));
				setBonus(itm.ItemManager.instance().itemParameter(equipParameter().equipBody().itemId()));
				setBonus(itm.ItemManager.instance().itemParameter(equipParameter().equipArm().itemId()));
				setBonus(itm.ItemManager.instance().itemParameter(equipParameter().equipHand(HAND_TYPE.RIGHT_HAND).itemId()));
				setBonus(itm.ItemManager.instance().itemParameter(equipParameter().equipHand(HAND_TYPE.LEFT_HAND).itemId()));
			}

			public void setPenaltyBonus()
			{
				if (jobPenaltyTime() > 0)
				{
					bodyAndBonus_.strength().set((byte)(bodyAndBonus_.strength().get() * 90 / 100));
					bodyAndBonus_.vitality().set((byte)(bodyAndBonus_.vitality().get() * 90 / 100));
					bodyAndBonus_.dexterity().set((byte)(bodyAndBonus_.dexterity().get() * 90 / 100));
					bodyAndBonus_.intelligence().set((byte)(bodyAndBonus_.intelligence().get() * 90 / 100));
					bodyAndBonus_.mind().set((byte)(bodyAndBonus_.mind().get() * 90 / 100));
				}
			}

			public void updateParameter()
			{
				if (!OpenFF.Client.GameProfile.Ff3Party)
				{
					// PORT: no growth tables to read; the FF4 party keeps its defaults.
					return;
				}
				setParameter((byte)(level_.get() - 1));
				bodyAndBonus_.copy(body_);
				setMp((byte)(level_.get() - 1));
				handAttack_[0].initialize();
				handAttack_[1].initialize();
				setHandAttack(HAND_TYPE.RIGHT_HAND);
				setHandAttack(HAND_TYPE.LEFT_HAND);
				physicsDefense_.initialize();
				magicDefense_.initialize();
				calcPhysicsDefense();
				calcMagicDefense();
				calcBonus();
				setPenaltyBonus();
				// PORT: the mods' mastery progression (FF5's way) adds its job ladder's stat modifiers.
				OpenFF.Client.ProgressionLayer.ApplyStats(this);
				bodyAndBonus();
				updateCondition();
			}

			public void updateCondition()
			{
				if (hp().getNow() < hp().getLimit() * 25 / 100)
				{
					condition().onNearDeath();
				}
			}

			public bool isHealing()
			{
				if (!isEnable())
				{
					return false;
				}
				if (condition().isDeath())
				{
					return false;
				}
				if (condition().isStone())
				{
					return false;
				}
				return true;
			}

			public dgs.TXT_COLOR checkHpColor()
			{
				int num = 4096 * hp().getNow();
				hp().getLimit();
				if (num <= 0)
				{
					return dgs.TXT_COLOR.TXT_COLOR_RED;
				}
				if (4096 * hp().getNow() <= 4096 * hp().getLimit() * 25 / 100)
				{
					return dgs.TXT_COLOR.TXT_COLOR_YELLOW;
				}
				return dgs.TXT_COLOR.TXT_COLOR_WHITE;
			}

			public void calcJobPenaltyTime(JOB_TYPE nextJob)
			{
				jobPenaltyTime_ = formulaJobPenaltyTime(nextJob);
			}

			public void subJobPenaltyTime()
			{
				if (jobPenaltyTime_ > 0)
				{
					jobPenaltyTime_--;
				}
			}

			public int formulaJobPenaltyTime(JOB_TYPE nextJob)
			{
				int now = JobPenaltyTimeCount[0][jobManager().nowJob()];
				int now2 = JobPenaltyTimeCount[1][jobManager().nowJob()];
				int next = JobPenaltyTimeCount[0][(int)nextJob];
				int next2 = JobPenaltyTimeCount[1][(int)nextJob];
				int num = jobManager().job(nextJob).skill().skillLevel()
					.get();
				int t = jobPenaltyTime_ + difference(now, next) + difference(now2, next2) - num / 10;
				t = ds.clamp(t, 0, JOB_PENALTY_TIME_MAX);
				return ds.max(jobPenaltyTime_, t);
			}

			public int difference(int now, int next)
			{
				if (now < next)
				{
					return ds.abs(next - now);
				}
				return ds.abs(now - next);
			}

			public void setJobChangeMP()
			{
				for (int i = 0; i < 8; i++)
				{
					if (jobChangeMp_[i] < mp_[i].getNow())
					{
						jobChangeMp_[i] = (byte)mp_[i].getNow();
					}
				}
			}

			public void setJobChangeMPtoMP()
			{
				for (int i = 0; i < 8; i++)
				{
					if (jobChangeMp_[i] > mp_[i].getNow())
					{
						mp_[i].setNow(jobChangeMp_[i]);
					}
				}
			}

			public void changeFormationType()
			{
				if (formationType_ == 0)
				{
					formationType_ = 1;
				}
				else if (formationType_ == 1)
				{
					formationType_ = 0;
				}
			}

			public void recoverHPandMP()
			{
				hp().maxNow();
				for (int i = 0; i < 8; i++)
				{
					mp(i).maxNow();
					jobChangeMp_[i] = (byte)mp(i).getNow();
				}
			}

			public void fine()
			{
				recoverHPandMP();
				condition().clearCondition();
			}

			public int magicDefensePower()
			{
				return magicDefense_.magicPhylacticPower();
			}

			public void changeJob(JOB_TYPE nextJob)
			{
				calcJobPenaltyTime(nextJob);
				jobManager().setNowJob(nextJob);
				JOB_TYPE jOB_TYPE = (JOB_TYPE)jobManager().nowJob();
				for (int i = 0; i < COMMAND_ABILITY_MAX; i++)
				{
					jobManager().job(jOB_TYPE).ability().playerAbility()
						.command_[i] = PlayerParty.instance().initializeJobAbility((int)jOB_TYPE).command_[i];
				}
				for (int i = 0; i < PASSIVE_ABILITY_MAX; i++)
				{
					jobManager().job(jOB_TYPE).ability().playerAbility()
						.passive_[i] = PlayerParty.instance().initializeJobAbility((int)jOB_TYPE).passive_[i];
				}
				for (int i = 0; i < COMMAND_ABILITY_MAX; i++)
				{
					jobManager().command().setCommandId(i, (sbyte)jobManager().job(jOB_TYPE).ability().playerAbility()
						.command_[i]);
				}
				setJobChangeMP();
				updateParameter();
				setJobChangeMPtoMP();
				// PORT: a hero on the mods' mastery progression (FF5's way) takes its commands from
				// its job ladder and pays no penalty time; the others are as the game made them.
				OpenFF.Client.ProgressionLayer.OnJobChanged(this);
			}

			/// <summary>PORT: the penalty time set outright (the mastery progression clears it).</summary>
			public void jobPenaltyTime_set(int value)
			{
				jobPenaltyTime_ = value;
			}

			public bool doEquip(int points, short item_id, bool sort)
			{
				itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter(item_id);
				itm.WeaponParameter weaponParameter = itm.ItemManager.instance().weaponParameter(item_id);
				itm.ProtectionParameter protection = itm.ItemManager.instance().protectionParameter(item_id);
				EquipItemInfo equipItemInfo = new EquipItemInfo();
				if (itemBaseParameter == null)
				{
					equipItemInfo.itemNumber_ = 0;
					isHand(points, sort);
				}
				else
				{
					itm.CATEGORY cATEGORY = itm.ItemManager.instance().itemCategory(item_id);
					if (cATEGORY != itm.CATEGORY.CATEGORY_WEAPON && cATEGORY != itm.CATEGORY.CATEGORY_PROTECTION)
					{
						return false;
					}
					if (PlayerParty.instance().item().serchNormalItem(item_id) != null && PlayerParty.instance().item().serchNormalItem(item_id)
						.itemNumber() == 0)
					{
						return false;
					}
					switch (cATEGORY)
					{
					case itm.CATEGORY.CATEGORY_WEAPON:
						if (!isCanEquipWeapon(points, weaponParameter))
						{
							return false;
						}
						if (weaponParameter.system() == 7)
						{
							if (!isCanEquipBow(points, weaponParameter, sort))
							{
								return false;
							}
						}
						else if (weaponParameter.system() == 8)
						{
							if (!isCanEquipArrow(points, weaponParameter))
							{
								return false;
							}
						}
						else if (weaponParameter.system() == 16 && !isCanEquipHarp(points, weaponParameter, sort))
						{
							return false;
						}
						break;
					case itm.CATEGORY.CATEGORY_PROTECTION:
						if (!isCanEquipProtection(points, protection))
						{
							return false;
						}
						break;
					}
					if (!isHandCheck(points, weaponParameter, protection, sort))
					{
						return false;
					}
				}
				equipItemInfo.itemId_ = item_id;
				if (sort)
				{
					if (weaponParameter != null && weaponParameter.system() == 8)
					{
						if (PlayerParty.instance().item().serchNormalItem(item_id) == null)
						{
							return false;
						}
						equipItemInfo.itemNumber_ = PlayerParty.instance().item().serchNormalItem(item_id)
							.itemNumber();
						PlayerParty.instance().item().serchNormalItem(item_id)
							.setItemNumber(0);
					}
					else if (itemBaseParameter != null)
					{
						equipItemInfo.itemNumber_ = 1;
						int itemNumber = PlayerParty.instance().item().serchNormalItem(item_id)
							.itemNumber() - 1;
						PlayerParty.instance().item().serchNormalItem(item_id)
							.setItemNumber(itemNumber);
					}
				}
				else
				{
					equipItemInfo.itemNumber_ = 1;
				}
				if (PlayerParty.instance().item().serchNormalItem(item_id)
					.itemNumber() == 0)
				{
					PlayerParty.instance().item().serchNormalItem(item_id)
						.setItemId(-1);
				}
				EquipItemInfo equipItemInfo2 = equipParameter().doEquipItem(points, equipItemInfo);
				if (sort)
				{
					PlayerParty.instance().item().storeItem(equipItemInfo2.itemId_, equipItemInfo2.itemNumber_);
				}
				updateParameter();
				return true;
			}

			public bool isCanEquipWeapon(int points, itm.WeaponParameter weapon)
			{
				if (weapon == null)
				{
					return false;
				}
				if (!isEquipItem(weapon.equipJob()))
				{
					return false;
				}
				if (points != 0 && points != 1)
				{
					return false;
				}
				return true;
			}

			public bool isCanEquipProtection(int points, itm.ProtectionParameter protection)
			{
				if (protection == null)
				{
					return false;
				}
				if (!isEquipItem(protection.equipJob()))
				{
					return false;
				}
				if (protection.system() == 0)
				{
					if (points != 0 && points != 1)
					{
						return false;
					}
				}
				else if (protection.system() == 1)
				{
					if (points != 2)
					{
						return false;
					}
				}
				else if (protection.system() == 2)
				{
					if (points != 3)
					{
						return false;
					}
				}
				else if (protection.system() == 3 && points != 4)
				{
					return false;
				}
				return true;
			}

			public bool isCanEquipBow(int points, itm.WeaponParameter bow, bool sort)
			{
				if (points != 0 && points != 1)
				{
					return false;
				}
				if (equipParameter().equipPoint(points).isEquipBow())
				{
					return true;
				}
				switch (points)
				{
				case 0:
					releaseEquipItem(1, sort);
					break;
				case 1:
					releaseEquipItem(0, sort);
					break;
				}
				return true;
			}

			public bool isCanEquipArrow(int points, itm.WeaponParameter arrow)
			{
				if (points != 0 && points != 1)
				{
					return false;
				}
				switch (points)
				{
				case 0:
					if (!equipParameter().equipPoint(1).isEquipBow())
					{
						return false;
					}
					break;
				case 1:
					if (!equipParameter().equipPoint(0).isEquipBow())
					{
						return false;
					}
					break;
				}
				return true;
			}

			public bool isCanEquipHarp(int points, itm.WeaponParameter harp, bool sort)
			{
				if (points != 0 && points != 1)
				{
					return false;
				}
				if (equipParameter().equipPoint(points).isEquipHarp())
				{
					return true;
				}
				switch (points)
				{
				case 0:
					releaseEquipItem(1, sort);
					break;
				case 1:
					releaseEquipItem(0, sort);
					break;
				}
				return true;
			}

			public bool isHand(int points, bool sort)
			{
				if (points != 0 && points != 1)
				{
					return true;
				}
				switch (points)
				{
				case 0:
					if (equipParameter().equipPoint(1).isEquipArrow())
					{
						releaseEquipItem(1, sort);
					}
					break;
				case 1:
					if (equipParameter().equipPoint(0).isEquipArrow())
					{
						releaseEquipItem(0, sort);
					}
					break;
				}
				return true;
			}

			public bool isHandCheck(int points, itm.WeaponParameter weapon, itm.ProtectionParameter protection, bool sort)
			{
				if (points != 0 && points != 1)
				{
					return true;
				}
				if (weapon != null && weapon.system() == 8)
				{
					return true;
				}
				switch (points)
				{
				case 0:
					if (equipParameter().equipPoint(1).isEquipBow())
					{
						return false;
					}
					if (equipParameter().equipPoint(1).isEquipHarp())
					{
						return false;
					}
					if (equipParameter().equipPoint(1).isEquipArrow())
					{
						if (weapon != null && weapon.system() != 7)
						{
							releaseEquipItem(1, sort);
							return true;
						}
						if (protection != null && protection.system() == 0)
						{
							releaseEquipItem(1, sort);
							return true;
						}
					}
					break;
				case 1:
					if (equipParameter().equipPoint(0).isEquipBow())
					{
						return false;
					}
					if (equipParameter().equipPoint(0).isEquipHarp())
					{
						return false;
					}
					if (equipParameter().equipPoint(0).isEquipArrow())
					{
						if (weapon != null && weapon.system() != 7)
						{
							releaseEquipItem(0, sort);
							return true;
						}
						if (protection != null && protection.system() == 0)
						{
							releaseEquipItem(0, sort);
							return true;
						}
					}
					break;
				}
				return true;
			}

			public void releaseEquipItem(int points, bool sort)
			{
				EquipItemInfo equipItemInfo = equipParameter().equipPoint(points).release();
				if (sort)
				{
					PlayerParty.instance().item().storeItem(equipItemInfo.itemId_, equipItemInfo.itemNumber_);
				}
			}

			public bool isEquipItem(int jobFlag)
			{
				int num = jobManager().nowJob();
				num = 1 << num;
				// PORT: the mastery progression's grants - the jobs a set or innate ability lends
				// the hero the permissions of (FF5's "Equip Swords"; a set white magic casting).
				num |= OpenFF.Client.ProgressionLayer.GrantBits(this);
				if ((jobFlag & num) == 0)
				{
					return false;
				}
				return true;
			}

			public bool isUseMagic(int magicId, int part)
			{
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter((short)magicId);
				if (magicParameter == null)
				{
					return false;
				}
				if (part != 0)
				{
					if (magicParameter.useBattle() == 0)
					{
						return false;
					}
				}
				else if (magicParameter.useField() == 0)
				{
					return false;
				}
				if (magicParameter.system() == 3)
				{
					return true;
				}
				if (mp(magicParameter.magicClass()).getNow() == 0)
				{
					return false;
				}
				if (!isEquipItem(magicParameter.equipJob()))
				{
					return false;
				}
				if (condition().isDeath() | condition().isSilence() | condition().isStone())
				{
					return false;
				}
				return true;
			}

			public bool isUseMagicCharacter()
			{
				if (mp_[0].getLimit() == 0)
				{
					return false;
				}
				return true;
			}

			public bool isFinishAttack()
			{
				if (equipParameter().isEquipWeapon() == 0)
				{
					return true;
				}
				if (equipParameter().isEquipWeapon() == 1 && equipParameter().checkEquipWeaponHand() == HAND_TYPE.LEFT_HAND)
				{
					return false;
				}
				if (equipParameter().equipHand(HAND_TYPE.RIGHT_HAND).isEquipBow())
				{
					return false;
				}
				if (equipParameter().equipHand(HAND_TYPE.LEFT_HAND).isEquipHarp())
				{
					return false;
				}
				return true;
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

			public string name()
			{
				return name_;
			}

			public void setName(string name)
			{
				strcpy(out name_, name);
			}

			public byte playerId()
			{
				return playerId_;
			}

			public ys.ParameterPoint<int> level()
			{
				return level_;
			}

			public ys.ParameterPoint<int> exp()
			{
				return exp_;
			}

			public ys.ParameterPoint<int> capacity()
			{
				return capacity_;
			}

			public ys.MPoint<int> hp()
			{
				return hp_;
			}

			public ys.MPoint<int> mp(int level)
			{
				return mp_[level];
			}

			public byte jobChangeMp(int level)
			{
				return jobChangeMp_[level];
			}

			public void setJobChangeMp(int level, byte mp)
			{
				jobChangeMp_[level] = mp;
			}

			public ys.BodyParameter body()
			{
				return body_;
			}

			public byte formationType()
			{
				return formationType_;
			}

			public void formationType_set(byte arg0)
			{
				formationType_ = arg0;
			}

			public void setFormationType(byte type)
			{
				formationType_ = type;
			}

			public ys.Condition condition()
			{
				return condition_;
			}

			public ys.PhysicsAttackParameter handAttack(HAND_TYPE type)
			{
				return handAttack_[(int)type];
			}

			public ys.PhysicsDefenseParameter physicsDefense()
			{
				return physicsDefense_;
			}

			public ys.MagicDefenseParameter magicDefense()
			{
				return magicDefense_;
			}

			public PlayerEquipParameter equipParameter()
			{
				return equipParameter_;
			}

			public PlayerJobManager jobManager()
			{
				return jobManager_;
			}

			public PlayerSkillManager skillManager()
			{
				return skillManager_;
			}

			public int jobPenaltyTime()
			{
				return jobPenaltyTime_;
			}

			public void setDefault()
			{
				isEnable_ = false;
				name_ = "";
				playerId_ = 0;
				level_.set(0);
				exp_.set(0);
				capacity_.set(0);
				hp_.setLimit(0);
				hp_.setNow(0);
				for (int i = 0; i < 8; i++)
				{
					mp_[i].setLimit(0);
					mp_[i].setNow(0);
				}
				memset(jobChangeMp_, 0, 8);
				body_.setDefault();
				bodyAndBonus_.setDefault();
				formationType_ = 0;
				condition_.setDefault();
				for (int i = 0; i < 2; i++)
				{
					handAttack_[i].setDefault();
				}
				physicsDefense_.setDefault();
				magicDefense_.setDefault();
				equipParameter_.setDefault();
				jobManager_.setDefault();
				skillManager_.setDefault();
				jobPenaltyTime_ = 0;
			}

			public void copy(Player src)
			{
				isEnable_ = src.isEnable_;
				name_ = src.name_;
				playerId_ = src.playerId_;
				level_.set(src.level_.get());
				exp_.set(src.exp_.get());
				capacity_.set(src.capacity_.get());
				hp_.setLimit(src.hp_.getLimit());
				hp_.setNow(src.hp_.getNow());
				for (int i = 0; i < 8; i++)
				{
					mp_[i].setLimit(src.mp_[i].getLimit());
					mp_[i].setNow(src.mp_[i].getNow());
				}
				memcpy(jobChangeMp_, src.jobChangeMp_, 8);
				body_.copy(src.body_);
				bodyAndBonus_.copy(src.bodyAndBonus_);
				formationType_ = src.formationType_;
				condition_.copy(src.condition_);
				for (int i = 0; i < 2; i++)
				{
					handAttack_[i].copy(src.handAttack_[i]);
				}
				physicsDefense_.copy(src.physicsDefense_);
				magicDefense_.copy(src.magicDefense_);
				equipParameter_.copy(src.equipParameter_);
				jobManager_.copy(src.jobManager_);
				skillManager_.copy(src.skillManager_);
				jobPenaltyTime_ = src.jobPenaltyTime_;
			}

			public void parse(ArrayReader reader)
			{
				byte[] array = new byte[25];
				isEnable_ = reader.readByte() != 0;
				reader.read(array, 0, 25);
				name_ = StringUtil.createString(array);
				playerId_ = reader.readByte();
				level_.set(reader.readInt32());
				exp_.set(reader.readInt32());
				capacity_.set(reader.readInt32());
				hp_.setLimit(reader.readInt32());
				hp_.setNow(reader.readInt32());
				for (int i = 0; i < 8; i++)
				{
					mp_[i].setLimit(reader.readInt32());
					mp_[i].setNow(reader.readInt32());
				}
				reader.read(jobChangeMp_, 0, 8);
				body_.parse(reader);
				bodyAndBonus_.parse(reader);
				formationType_ = reader.readByte();
				condition_.parse(reader);
				for (int i = 0; i < 2; i++)
				{
					handAttack_[i].parse(reader);
				}
				physicsDefense_.parse(reader);
				magicDefense_.parse(reader);
				equipParameter_.parse(reader);
				jobManager_.parse(reader);
				skillManager_.parse(reader);
				jobPenaltyTime_ = reader.readInt32();
			}

			public void store(ArrayWriter writer)
			{
				byte[] array = new byte[25];
				byte[] bytes = StringUtil.getBytes(name_);
				memcpy(array, bytes, bytes.Length);
				writer.writeByte((byte)(isEnable_ ? 1u : 0u));
				writer.write(array, 0, 25);
				writer.writeByte(playerId_);
				writer.writeInt32(level_.get());
				writer.writeInt32(exp_.get());
				writer.writeInt32(capacity_.get());
				writer.writeInt32(hp_.getLimit());
				writer.writeInt32(hp_.getNow());
				for (int i = 0; i < 8; i++)
				{
					writer.writeInt32(mp_[i].getLimit());
					writer.writeInt32(mp_[i].getNow());
				}
				writer.write(jobChangeMp_, 0, 8);
				body_.store(writer);
				bodyAndBonus_.store(writer);
				writer.writeByte(formationType_);
				condition_.store(writer);
				for (int i = 0; i < 2; i++)
				{
					handAttack_[i].store(writer);
				}
				physicsDefense_.store(writer);
				magicDefense_.store(writer);
				equipParameter_.store(writer);
				jobManager_.store(writer);
				skillManager_.store(writer);
				writer.writeInt32(jobPenaltyTime_);
			}
		}
	}
}
