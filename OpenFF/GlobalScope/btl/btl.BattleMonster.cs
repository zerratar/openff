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
		public class BattleMonster : BaseBattleCharacter
		{
			public enum REGISTER_DATA_STATE
			{
				NOT_REGISTER,
				READY_MODEL,
				END_MODEL,
				READY_MOTION,
				END_MOTION,
				READY_COLOR,
				END_COLOR,
				END_DATA,
				REGISTER_DATA_STATE_MAX
			}

			public const int MONSTER_BONE_MAX = 1;

			public const REGISTER_DATA_STATE NOT_REGISTER = REGISTER_DATA_STATE.NOT_REGISTER;

			public const REGISTER_DATA_STATE READY_MODEL = REGISTER_DATA_STATE.READY_MODEL;

			public const REGISTER_DATA_STATE END_MODEL = REGISTER_DATA_STATE.END_MODEL;

			public const REGISTER_DATA_STATE READY_MOTION = REGISTER_DATA_STATE.READY_MOTION;

			public const REGISTER_DATA_STATE END_MOTION = REGISTER_DATA_STATE.END_MOTION;

			public const REGISTER_DATA_STATE READY_COLOR = REGISTER_DATA_STATE.READY_COLOR;

			public const REGISTER_DATA_STATE END_COLOR = REGISTER_DATA_STATE.END_COLOR;

			public const REGISTER_DATA_STATE END_DATA = REGISTER_DATA_STATE.END_DATA;

			public const REGISTER_DATA_STATE REGISTER_DATA_STATE_MAX = REGISTER_DATA_STATE.REGISTER_DATA_STATE_MAX;

			private short monsterId_;

			private short stolenItemId_;

			private int alpha_;

			private int battleMonsterId_;

			private int dataState_;

			private uint monsterFlag_;

			private ys.MPoint<int> m_hp_ = new ys.MPoint<int>(0, 999999);

			private ys.Condition m_condition_ = new ys.Condition();

			private ys.BodyParameter m_body_ = new ys.BodyParameter();

			private ys.BodyParameter m_bodyAndBonus_ = new ys.BodyParameter();

			private ys.PhysicsAttackParameter[] m_handAttack_ = new ys.PhysicsAttackParameter[2];

			private ys.PhysicsDefenseParameter m_physicsDefense_ = new ys.PhysicsDefenseParameter();

			private ys.MagicDefenseParameter m_magicDefense_ = new ys.MagicDefenseParameter();

			private mon.MonsterParameter monster_;

			public BattleMonster()
			{
				for (int i = 0; i < m_handAttack_.Length; i++)
				{
					m_handAttack_[i] = new ys.PhysicsAttackParameter();
				}
				monster_ = null;
			}

			public new void initialize()
			{
				base.initialize();
				monsterId_ = 0;
				stolenItemId_ = -1;
				dataState_ = 0;
				monster_ = null;
				m_hp_.minLimit();
				m_hp_.minNow();
				m_condition_.clearCondition();
				alpha_ = 31;
				clearMonsterFlagAll();
			}

			public void terminate()
			{
			}

			public void preExecute()
			{
				if (characterMngId() >= 0)
				{
					characterMng.initJntMtx(characterMngId());
					for (int i = 0; i < 1; i++)
					{
						characterMng.reserveToGetJntMtx(characterMngId(), MonsterBoneName[i]);
					}
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

			public bool registerMonster()
			{
				string arg = "";
				string arg2 = "";
				sprintf(out arg, "f%03d", monster().familyId());
				sprintf(out arg2, "f%03d.nmdp.lz", monster().familyId());
				characterMngId_set(characterMng.setCharacter(arg, CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST));
				characterMng.releaseMdlTexRes(characterMngId());
				string arg3 = "";
				sprintf(out arg3, "b_f%03d", monster().familyId());
				sprintf(out arg2, "b_f%03d.ncap.lz", monster().familyId());
				characterMng.addMotion(characterMngId(), arg3);
				characterMng.startMotion(characterMngId(), 101, fLoop: true, 0u);
				characterMng.setShadowType(characterMngId(), 1);
				characterMng.setShadowHeight(characterMngId(), 2048);
				characterMng.setShadowAlphaRate(characterMngId(), SHADOW_ALPHA_RATE_MAX);
				string arg4 = "";
				sprintf(out arg4, "f%03d_%03d", monster().familyId(), monster().monsterId());
				sprintf(out arg2, "/OBJ/mon.MonsterManager.instance()/f%03d_%03d.ntxp.lz", monster().familyId(), monster().monsterId());
				if (ds.g_File.getSize(arg2) != 0)
				{
					characterMng.bindReplaceTex(characterMngId(), arg4);
				}
				return true;
			}

			public bool registerMonsterAsync()
			{
				string arg = "";
				switch (dataState_)
				{
				case 0:
					dataState_ = 1;
					sprintf(out arg, "f%03d", monster().familyId());
					setCharacterMngId(characterMng.setCharacterAsync(arg, CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST));
					characterMng.setHidden(characterMngId(), b: true);
					dataState_ = 2;
					break;
				case 1:
					sprintf(out arg, "f%03d", monster().familyId());
					setCharacterMngId(characterMng.setCharacterAsync(arg, CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST));
					characterMng.setHidden(characterMngId(), b: true);
					dataState_ = 2;
					break;
				case 2:
					if (!characterMng.isLoadingCharaAsync())
					{
						dataState_ = 3;
					}
					break;
				case 3:
					sprintf(out arg, "b_f%03d", monster().familyId());
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
				{
					string arg2 = "";
					sprintf(out arg, "f%03d_%03d", monster().familyId(), monster().monsterId());
					sprintf(out arg2, "/OBJ/mon.MonsterManager.instance()/f%03d_%03d.ntxp.lz", monster().familyId(), monster().monsterId());
					if (ds.g_File.getSize(arg2) != 0)
					{
						characterMng.bindReplaceTexAsync(characterMngId(), arg);
						dataState_ = 6;
					}
					else
					{
						dataState_ = 7;
					}
					break;
				}
				case 6:
					if (characterMng.isLoadedReplaceTex(characterMngId()))
					{
						dataState_ = 7;
					}
					break;
				case 7:
					characterMng.startMotion(characterMngId(), 101, fLoop: true, 0u);
					characterMng.setShadowType(characterMngId(), 1);
					characterMng.setShadowHeight(characterMngId(), 2048);
					characterMng.setShadowAlphaRate(characterMngId(), SHADOW_ALPHA_RATE_MAX);
					characterMng.setHidden(characterMngId(), b: false);
					initializeData();
					dataState_ = 0;
					return true;
				}
				return false;
			}

			public void unregisterMonster()
			{
				characterMng.delCharacter(characterMngId());
				setCharacterMngId(-1);
				terminate();
				offIsEnable();
				dataState_ = 0;
			}

			public bool changeLilliput()
			{
				if (!changeCondition().isLilliput())
				{
					return false;
				}
				VecFx32 fnd_reuse_pos = fnd_reuse_pos2;
				fnd_reuse_pos.x = mon.MonsterManager.instance().offset(monsterId()).scale() / 2;
				fnd_reuse_pos.y = mon.MonsterManager.instance().offset(monsterId()).scale() / 2;
				fnd_reuse_pos.z = mon.MonsterManager.instance().offset(monsterId()).scale() / 2;
				characterMng.setScale(characterMngId(), fnd_reuse_pos);
				fnd_reuse_pos.x = mon.MonsterManager.instance().offset(monsterId()).shadowX() / 2;
				fnd_reuse_pos.y = 4096;
				fnd_reuse_pos.z = mon.MonsterManager.instance().offset(monsterId()).shadowZ() / 2;
				characterMng.setShadowScale(characterMngId(), fnd_reuse_pos);
				condition().onLilliput();
				changeCondition().onLilliput();
				return true;
			}

			public bool changeFrog()
			{
				if (!changeCondition().isFrog())
				{
					return false;
				}
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos2;
				characterMng.getPosition(characterMngId(), fnd_reuse_pos);
				characterMng.delCharacter(characterMngId());
				setCharacterMngId(characterMng.setCharacter("n431", CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST));
				characterMng.releaseMdlTexRes(characterMngId());
				characterMng.addMotion(characterMngId(), "b_b01_431");
				characterMng.setPosition(characterMngId(), fnd_reuse_pos);
				VecFx32 fnd_reuse_pos2 = GlobalScope.fnd_reuse_pos2;
				fnd_reuse_pos2.x = 2048;
				fnd_reuse_pos2.y = 2048;
				fnd_reuse_pos2.z = 2048;
				characterMng.setScale(characterMngId(), fnd_reuse_pos2);
				fnd_reuse_pos2.x = 2730;
				fnd_reuse_pos2.y = 4096;
				fnd_reuse_pos2.z = 2730;
				characterMng.setShadowScale(characterMngId(), fnd_reuse_pos2);
				characterMng.setRotation(characterMngId(), 0, MonsterRotationY, 0);
				if (condition().isDeath())
				{
					characterMng.startMotion(characterMngId(), 706, fLoop: false, 0u);
					int maxFrame = (int)characterMng.getMaxFrame(characterMngId());
					characterMng.setCurrentFrame(characterMngId(), (uint)maxFrame);
				}
				else
				{
					characterMng.startMotion(characterMngId(), 101, fLoop: true, 0u);
				}
				condition().onFrog();
				changeCondition().offFrog();
				return true;
			}

			public bool returnMonster()
			{
				if (changeCondition().isFrog())
				{
					characterMng.delCharacter(characterMngId());
					setCharacterMngId(-1);
					registerMonster();
					initializeData();
					changeCondition();
					condition().offFrog();
					changeCondition().offFrog();
				}
				else if (changeCondition().isLilliput())
				{
					VecFx32 fnd_reuse_pos = fnd_reuse_pos2;
					fnd_reuse_pos.x = mon.MonsterManager.instance().offset(monsterId()).scale();
					fnd_reuse_pos.y = mon.MonsterManager.instance().offset(monsterId()).scale();
					fnd_reuse_pos.z = mon.MonsterManager.instance().offset(monsterId()).scale();
					characterMng.setScale(characterMngId(), fnd_reuse_pos);
					fnd_reuse_pos.x = mon.MonsterManager.instance().offset(monsterId()).shadowX();
					fnd_reuse_pos.y = 4096;
					fnd_reuse_pos.z = mon.MonsterManager.instance().offset(monsterId()).shadowZ();
					characterMng.setShadowScale(characterMngId(), fnd_reuse_pos);
					condition().offLilliput();
					changeCondition().offLilliput();
				}
				return true;
			}

			public bool initializeData()
			{
				VecFx32 nitro_reuse_pos = GlobalScope.nitro_reuse_pos;
				nitro_reuse_pos.copy(mon.MonsterManager.instance().offset(monsterId()).initializePosition());
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
				if (nitro_reuse_pos.x == 0 && nitro_reuse_pos.y == 0 && nitro_reuse_pos.z == 0)
				{
					fnd_reuse_pos.x = MonsterPosition[battleMonsterId()].x;
					fnd_reuse_pos.y = MonsterPosition[battleMonsterId()].y;
					fnd_reuse_pos.z = MonsterPosition[battleMonsterId()].z;
					characterMng.setPosition(characterMngId(), fnd_reuse_pos);
				}
				else
				{
					nitro_reuse_pos.y += 4096 * mon.MonsterManager.instance().offset(monsterId()).height();
					characterMng.setPosition(characterMngId(), nitro_reuse_pos);
				}
				int i = mon.MonsterManager.instance().offset(monsterId()).rotate();
				characterMng.setRotation(characterMngId(), 0, ds.DEGto65536(i), 0);
				VecFx32 fnd_reuse_pos2 = GlobalScope.fnd_reuse_pos2;
				fnd_reuse_pos2.x = mon.MonsterManager.instance().offset(monsterId()).scale();
				fnd_reuse_pos2.y = mon.MonsterManager.instance().offset(monsterId()).scale();
				fnd_reuse_pos2.z = mon.MonsterManager.instance().offset(monsterId()).scale();
				characterMng.setScale(characterMngId(), fnd_reuse_pos2);
				fnd_reuse_pos2.x = mon.MonsterManager.instance().offset(monsterId()).shadowX();
				fnd_reuse_pos2.y = 4096;
				fnd_reuse_pos2.z = mon.MonsterManager.instance().offset(monsterId()).shadowZ();
				characterMng.setShadowScale(characterMngId(), fnd_reuse_pos2);
				return true;
			}

			public void setNewMonster(int char_id, int monster_id, int root_id)
			{
				onIsEnable();
				setBattleCharacterId((sbyte)char_id);
				setBreed(1);
				setMonsterId((short)root_id);
				setCondition(monsterCondition());
				condition().clearCondition();
				setMonster(mon.MonsterManager.instance().monsterParameter(monsterId()));
				setActionNumber(monster().actionNumber());
				monsterHp().setLimit(monster().maxHp());
				monsterHp().maxNow();
				setMonsterBody(monster().body());
				setMonstermBodyAndBonus(monster().body());
				setMonsterHandAttack(0, monster().physicsAttack());
				setMonsterHandAttack(1, monster().physicsAttack());
				setMonsterPhysicsDefense(monster().physicsDefense());
				setMonsterMagicDefense(monster().magicDefense());
				setLevel(monster().level());
				setHp(monsterHp());
				setBody(monsterBody());
				setBodyAndBonus(monstermBodyAndBonus());
				setHandAttack(pl.HAND_TYPE.RIGHT_HAND, monsterHandAttack(0));
				setHandAttack(pl.HAND_TYPE.LEFT_HAND, monsterHandAttack(1));
				setPhysicsDefense(monsterPhysicsDefense());
				setMagicDefense(monsterMagicDefense());
				setBattleMonsterId(monster_id);
				clearFlagAll();
				clearMagicFlagAll();
				clearMonsterFlagAll();
			}

			public bool disappear(int frame)
			{
				int t = characterMng.getTransparencyRate(characterMngId()) - ALPHA_RATE_MAX / frame;
				t = ds.max(t, 0);
				characterMng.setTransparencyRate(characterMngId(), t);
				int t2 = characterMng.getShadowAlphaRate(characterMngId()) - SHADOW_ALPHA_RATE_MAX / frame;
				t2 = ds.max(t2, 0);
				characterMng.setShadowAlphaRate(characterMngId(), t2);
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
				int t2 = characterMng.getShadowAlphaRate(characterMngId()) + SHADOW_ALPHA_RATE_MAX / frame;
				t2 = ds.min(t2, SHADOW_ALPHA_RATE_MAX);
				characterMng.setShadowAlphaRate(characterMngId(), t2);
				if (t != ALPHA_RATE_MAX)
				{
					return false;
				}
				return true;
			}

			public void setAlpha(int alpha, int shadow)
			{
				characterMng.setTransparencyRate(characterMngId(), alpha);
				characterMng.setShadowAlphaRate(characterMngId(), shadow);
			}

			public override void resetParameterMagicFlag()
			{
				if (monster() != null)
				{
					setMonsterBody(monster().body());
					setMonstermBodyAndBonus(monster().body());
					setMonsterHandAttack(0, monster().physicsAttack());
					setMonsterHandAttack(1, monster().physicsAttack());
					setMonsterPhysicsDefense(monster().physicsDefense());
					if (monsterFlag(MONSTER_FLAG.MF_WEAK_CHANGE))
					{
						short arg = monsterMagicDefense().weakType();
						setMonsterMagicDefense(monster().magicDefense());
						monsterMagicDefense().weakType_set(arg);
					}
					else
					{
						setMonsterMagicDefense(monster().magicDefense());
					}
					setBody(monsterBody());
					setBodyAndBonus(monstermBodyAndBonus());
					setHandAttack(pl.HAND_TYPE.RIGHT_HAND, monsterHandAttack(0));
					setHandAttack(pl.HAND_TYPE.LEFT_HAND, monsterHandAttack(1));
					setPhysicsDefense(monsterPhysicsDefense());
					setMagicDefense(monsterMagicDefense());
				}
			}

			public bool checkUseMagic(short magic_id)
			{
				switch (magic_id)
				{
				case 4015:
					if (magicFlag(MAGIC_FLAG.MF_PROTECT))
					{
						return false;
					}
					break;
				case 4018:
					if (magicFlag(MAGIC_FLAG.MF_HASTE))
					{
						return false;
					}
					break;
				default:
					if (magic_id == REFLECT_ID && magicFlag(MAGIC_FLAG.MF_REFLECT))
					{
						return false;
					}
					break;
				}
				return true;
			}

			public short monsterId()
			{
				return monsterId_;
			}

			public void monsterId_set(short arg0)
			{
				monsterId_ = arg0;
			}

			public void setMonsterId(short _id)
			{
				monsterId_ = _id;
			}

			public short stolenItemId()
			{
				return stolenItemId_;
			}

			public void setStolenItemId(short itemId)
			{
				stolenItemId_ = itemId;
			}

			public int alpha()
			{
				return alpha_;
			}

			public ys.MPoint<int> monsterHp()
			{
				return m_hp_;
			}

			public ys.Condition monsterCondition()
			{
				return m_condition_;
			}

			public ys.BodyParameter monsterBody()
			{
				return m_body_;
			}

			public void setMonsterBody(ys.BodyParameter b)
			{
				m_body_.copy(b);
			}

			public ys.BodyParameter monstermBodyAndBonus()
			{
				return m_bodyAndBonus_;
			}

			public void setMonstermBodyAndBonus(ys.BodyParameter b)
			{
				m_bodyAndBonus_.copy(b);
			}

			public ys.PhysicsAttackParameter monsterHandAttack(int i)
			{
				return m_handAttack_[i];
			}

			public void setMonsterHandAttack(int i, ys.PhysicsAttackParameter p)
			{
				m_handAttack_[i].copy(p);
			}

			public ys.PhysicsDefenseParameter monsterPhysicsDefense()
			{
				return m_physicsDefense_;
			}

			public void setMonsterPhysicsDefense(ys.PhysicsDefenseParameter p)
			{
				m_physicsDefense_.copy(p);
			}

			public ys.MagicDefenseParameter monsterMagicDefense()
			{
				return m_magicDefense_;
			}

			public void setMonsterMagicDefense(ys.MagicDefenseParameter m)
			{
				m_magicDefense_.copy(m);
			}

			public void setMonster(mon.MonsterParameter monster)
			{
				monster_ = monster;
			}

			public mon.MonsterParameter monster()
			{
				return monster_;
			}

			public int battleMonsterId()
			{
				return battleMonsterId_;
			}

			public void battleMonsterId_set(int arg0)
			{
				battleMonsterId_ = arg0;
			}

			public void setBattleMonsterId(int monster_id)
			{
				battleMonsterId_ = monster_id;
			}

			public void setMonsterFlag(MONSTER_FLAG flag)
			{
				monsterFlag_ |= (uint)flag;
			}

			public void clearMonsterFlag(MONSTER_FLAG flag)
			{
				monsterFlag_ &= (uint)(~flag);
			}

			public bool monsterFlag(MONSTER_FLAG flag)
			{
				if ((monsterFlag_ & (uint)flag) == 0)
				{
					return false;
				}
				return true;
			}

			public void clearMonsterFlagAll()
			{
				monsterFlag_ = 0u;
			}
		}
	}
}
