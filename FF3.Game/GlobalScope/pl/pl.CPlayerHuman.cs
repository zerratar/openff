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
	public static partial class pl
	{
		public class CPlayerHuman : CPlayerCharacter
		{
			private class MapMarkerAccepterHuman : MapMarkerAccepter
			{
				private CPlayerHuman m_Owner;

				public MapMarkerAccepterHuman(CPlayerHuman owner)
				{
					m_Owner = owner;
				}

				public override VecFx32 acceptPos()
				{
					return m_Owner.getPosition();
				}

				public override int acceptDir()
				{
					return -1;
				}

				public override bool acceptVisibility()
				{
					return m_Owner.visibleMarker_;
				}
			}

			public enum ACTION_ID
			{
				ACTION_ID_ERR = -1,
				ACTION_ID_WAIT,
				ACTION_ID_WALK,
				ACTION_ID_RUN,
				ACTION_ID_LEAVE_WAIT,
				ACTION_ID_TALK,
				ACTION_ID_CHECK,
				ACTION_ID_USEITEM,
				ACTION_ID_BOARD,
				ACTION_ID_OUT,
				ACTION_ID_EMBARK,
				ACTION_ID_LEAVE,
				ACTION_ID_KEYDOOR,
				ACTION_ID_KEYDOOR_ITEMUSE,
				ACTION_ID_INN,
				ACTION_ID_MAX
			}

			public enum BIND_LOCATE
			{
				BIND_LOCATE_LHAND,
				BIND_LOCATE_RHAND,
				BIND_LOCATE_NUM
			}

			private delegate void _action();

			public const ACTION_ID ACTION_ID_ERR = ACTION_ID.ACTION_ID_ERR;

			public const ACTION_ID ACTION_ID_WAIT = ACTION_ID.ACTION_ID_WAIT;

			public const ACTION_ID ACTION_ID_WALK = ACTION_ID.ACTION_ID_WALK;

			public const ACTION_ID ACTION_ID_RUN = ACTION_ID.ACTION_ID_RUN;

			public const ACTION_ID ACTION_ID_LEAVE_WAIT = ACTION_ID.ACTION_ID_LEAVE_WAIT;

			public const ACTION_ID ACTION_ID_TALK = ACTION_ID.ACTION_ID_TALK;

			public const ACTION_ID ACTION_ID_CHECK = ACTION_ID.ACTION_ID_CHECK;

			public const ACTION_ID ACTION_ID_USEITEM = ACTION_ID.ACTION_ID_USEITEM;

			public const ACTION_ID ACTION_ID_BOARD = ACTION_ID.ACTION_ID_BOARD;

			public const ACTION_ID ACTION_ID_OUT = ACTION_ID.ACTION_ID_OUT;

			public const ACTION_ID ACTION_ID_EMBARK = ACTION_ID.ACTION_ID_EMBARK;

			public const ACTION_ID ACTION_ID_LEAVE = ACTION_ID.ACTION_ID_LEAVE;

			public const ACTION_ID ACTION_ID_KEYDOOR = ACTION_ID.ACTION_ID_KEYDOOR;

			public const ACTION_ID ACTION_ID_KEYDOOR_ITEMUSE = ACTION_ID.ACTION_ID_KEYDOOR_ITEMUSE;

			public const ACTION_ID ACTION_ID_INN = ACTION_ID.ACTION_ID_INN;

			public const ACTION_ID ACTION_ID_MAX = ACTION_ID.ACTION_ID_MAX;

			public const BIND_LOCATE BIND_LOCATE_LHAND = BIND_LOCATE.BIND_LOCATE_LHAND;

			public const BIND_LOCATE BIND_LOCATE_RHAND = BIND_LOCATE.BIND_LOCATE_RHAND;

			public const BIND_LOCATE BIND_LOCATE_NUM = BIND_LOCATE.BIND_LOCATE_NUM;

			private _action[] action = new _action[14];

			private string m_OldModelName;

			private act.CBaseAction[] m_apAction = new act.CBaseAction[14];

			private CPlayerHumanWait m_HumanWait = new CPlayerHumanWait();

			private CPlayerHumanWalk m_HumanWalk = new CPlayerHumanWalk();

			private CPlayerHumanRun m_HumanRun = new CPlayerHumanRun();

			private CPlayerHumanLeaveWait m_HumanLeaveWait = new CPlayerHumanLeaveWait();

			private CPlayerHumanTalk m_HumanTalk = new CPlayerHumanTalk();

			private CPlayerHumanCheck m_HumanCheck = new CPlayerHumanCheck();

			private CPlayerHumanUseItem m_HumanUseItem = new CPlayerHumanUseItem();

			private CPlayerHumanBoard m_HumanBoard = new CPlayerHumanBoard();

			private CPlayerHumanOut m_HumanOut = new CPlayerHumanOut();

			private CPlayerHumanEmbark m_HumanEmbark = new CPlayerHumanEmbark();

			private CPlayerHumanLeave m_HumanLeave = new CPlayerHumanLeave();

			private CPlayerHumanKeyDoor m_HumanKeyDoor = new CPlayerHumanKeyDoor();

			private CPlayerHumanKeyDoorItemUse m_HumanKeyDoorItemUse = new CPlayerHumanKeyDoorItemUse();

			private CPlayerHumanInn m_HumanInn = new CPlayerHumanInn();

			private PLAYER_HUMAN_TYPE m_HumanType;

			private CPlayerHuman m_pNpc;

			private BindObject[] pBindObjs_ = new BindObject[2];

			private wld.CMenuButton pMenuIcon_;

			private wld.CMenuButton pCameraIcon_;

			private wld.CMenuButton pTalkIcon_;

			private int shadow_type;

			private int orgMdlIdx_;

			private int frogMdlIdx_;

			private bool visibleMarker_;

			public MapMarkerAccepter composit2;

			public void changeJob(int player_id, int effect)
			{
				bool flag = false;
				VecFx32 vecFx = new VecFx32(getPosition());
				VecFx32 rotation = new VecFx32(getRotation());
				VecFx32 scale = new VecFx32(4096, 4096, 4096);
				VecFx32 shadowScale = new VecFx32(4915, 4096, 4915);
				if (effect != -1)
				{
					int num = eff.CEffectMng.instance().create(102, effect);
					if (num != -1)
					{
						eff.CEffectMng.instance().setPosition(num, vecFx);
					}
				}
				flag = getAutoPilot();
				characterMng.delCharacter(getCharacterId());
				setCharacterId(-1);
				sprintf(out var arg, "%c%c%02d", getModelName()[0], getModelName()[1], PlayerParty.instance().playerForId((byte)player_id).jobManager()
					.nowJob() + 1);
				CCastCommandTransit.getInstance().cast_PlayerMng().setUpWorldCharacter(vecFx, rotation, scale, shadowScale, arg, _AutoPilot: false, _Operater: true);
				into();
				setAutoPilot(flag);
				setNextAct(0);
				startMotion(1001, _Loop: true, 5u);
			}

			public void changeLilliput(int player_id, int effect)
			{
				if (effect != -1)
				{
					int num = eff.CEffectMng.instance().create(102, effect);
					if (num != -1)
					{
						eff.CEffectMng.instance().setPosition(num, getPosition());
					}
				}
				VecFx32 vecFx = new VecFx32(2048, 2048, 2048);
				VEC_Set(vecFx, 2048, 2048, 2048);
				setScale(vecFx);
				VEC_Set(vecFx, 2730, 128, 2730);
				setShadowScale(vecFx);
				PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_LILLIPUT);
				setNextAct(0);
				startMotion(1001, _Loop: true, 5u);
			}

			public void createFrogMdl(string mdlname)
			{
				if (-1 != frogMdlIdx_)
				{
					characterMng.delCharacter(frogMdlIdx_);
					frogMdlIdx_ = -1;
				}
				VecFx32 scale = new VecFx32(1228, 1228, 1228);
				VecFx32 scale2 = new VecFx32(2730, 128, 2730);
				frogMdlIdx_ = characterMng.setCharacter(const_cast<string>(mdlname), CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST);
				characterMng.addMotion(frogMdlIdx_, "w_act_n431");
				characterMng.setScale(frogMdlIdx_, scale);
				characterMng.setShadowType(frogMdlIdx_, 0);
				characterMng.setShadowScale(frogMdlIdx_, scale2);
				characterMng.setHidden(frogMdlIdx_, b: true);
				orgMdlIdx_ = getCharacterId();
			}

			public void changeFrog(int player_id, int effect)
			{
				if (-1 == frogMdlIdx_)
				{
					return;
				}
				bool flag = false;
				VecFx32 vecFx = new VecFx32();
				VecFx32 vecFx2 = new VecFx32();
				VecFx32 a = new VecFx32();
				VecFx32 a2 = new VecFx32();
				vecFx.copy(getPosition());
				vecFx2.copy(getRotation());
				VEC_Set(a, 2048, 2048, 2048);
				VEC_Set(a2, 2730, 1024, 2730);
				if (effect != -1)
				{
					int num = eff.CEffectMng.instance().create(102, effect);
					if (num != -1)
					{
						eff.CEffectMng.instance().setPosition(num, vecFx);
					}
				}
				flag = getAutoPilot();
				characterMng.setHidden(orgMdlIdx_, b: true);
				characterMng.setHidden(frogMdlIdx_, b: false);
				setCharacterId(frogMdlIdx_);
				setPosition(vecFx);
				setRotation(vecFx2);
				PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_FROG);
				setAutoPilot(flag);
				setNextAct(0);
				startMotion(1001, _Loop: true, 5u);
			}

			public void returnHuman(bool model_change, int player_id, int effect)
			{
				VecFx32 vecFx = new VecFx32(getPosition());
				VecFx32 rotation = new VecFx32(getRotation());
				VecFx32 scale = new VecFx32(4096, 4096, 4096);
				VecFx32 shadowScale = new VecFx32(4096, 1024, 4096);
				if (effect != -1)
				{
					int num = eff.CEffectMng.instance().create(102, effect);
					if (num != -1)
					{
						eff.CEffectMng.instance().setPosition(num, vecFx);
					}
				}
				if (-1 != orgMdlIdx_ && -1 != frogMdlIdx_)
				{
					characterMng.setHidden(orgMdlIdx_, b: false);
					characterMng.setHidden(frogMdlIdx_, b: true);
					setCharacterId(orgMdlIdx_);
				}
				setPosition(vecFx);
				setRotation(rotation);
				setScale(scale);
				setShadowScale(shadowScale);
				if (getInPutMode() == INPUT_MODE.INPUT_MODE_FIELD)
				{
					PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_FIELD);
				}
				else if (getInPutMode() == INPUT_MODE.INPUT_MODE_TOWN)
				{
					PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_TOWN);
				}
				setNextAct(0);
				startMotion(1001, _Loop: true, 5u);
			}

			public void changeFrogForNpc()
			{
				if (-1 != frogMdlIdx_)
				{
					VecFx32 position = new VecFx32(getPosition());
					VecFx32 rotation = new VecFx32(getRotation());
					characterMng.setHidden(orgMdlIdx_, b: true);
					characterMng.setHidden(frogMdlIdx_, b: false);
					setCharacterId(frogMdlIdx_);
					setPosition(position);
					setRotation(rotation);
					PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_FROG);
					setNextAct(0);
					startMotion(1001, _Loop: true, 5u);
					PlayerParty.instance().npc().setFrog();
				}
			}

			public void changeLilliputForNpc()
			{
				VecFx32 scale = new VecFx32(2048, 2048, 2048);
				VecFx32 shadowScale = new VecFx32(2730, 128, 2730);
				setScale(scale);
				setShadowScale(shadowScale);
				PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_LILLIPUT);
				setNextAct(0);
				startMotion(1001, _Loop: true, 5u);
				PlayerParty.instance().npc().setLilliput();
			}

			public void returnHumanForNpc(string pMdlname)
			{
				VecFx32 position = new VecFx32(getPosition());
				VecFx32 rotation = new VecFx32(getRotation());
				VecFx32 scale = new VecFx32(4096, 4096, 4096);
				VecFx32 shadowScale = new VecFx32(4096, 1024, 4096);
				if (-1 != orgMdlIdx_ && -1 != frogMdlIdx_)
				{
					characterMng.setHidden(orgMdlIdx_, b: false);
					characterMng.setHidden(frogMdlIdx_, b: true);
					setCharacterId(orgMdlIdx_);
				}
				setPosition(position);
				setRotation(rotation);
				setScale(scale);
				setShadowScale(shadowScale);
				if (getInPutMode() == INPUT_MODE.INPUT_MODE_FIELD)
				{
					PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_FIELD);
				}
				else if (getInPutMode() == INPUT_MODE.INPUT_MODE_TOWN)
				{
					PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_TOWN);
				}
				setNextAct(0);
				startMotion(1001, _Loop: true, 5u);
				PlayerParty.instance().npc().setBadStateNormal();
			}

			public override void initialize()
			{
				base.initialize();
				CharaKind_set(chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN);
				CharaCheckType_set(chr.CHARACTER_CHECK_TYPE.CHARACTER_CHECK_TYPE_TALK);
				m_HumanType = PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_HUMAN;
				m_pNpc = null;
				pBindObjs_[0] = null;
				pBindObjs_[1] = null;
				pMenuIcon_ = null;
				pCameraIcon_ = null;
				pTalkIcon_ = null;
				shadow_type = 1;
				orgMdlIdx_ = -1;
				frogMdlIdx_ = -1;
				visibleMarker_ = true;
			}

			public override void execute()
			{
				for (byte b = 0; b < 2; b++)
				{
					if (pBindObjs_[b] != null)
					{
						pBindObjs_[b].setJntMtx();
						characterMng.initJntMtx(getCharacterId());
						pBindObjs_[b].reserveJntMtx();
					}
				}
				base.execute();
				if (getNextAct() == 4 || getNextAct() == 5 || getNextAct() == 6 || getNextAct() == 7 || getNextAct() == 8 || getNextAct() == 9 || getNextAct() == 10 || getNowAct() == 7 || getNowAct() == 8 || getNowAct() == 9 || getNowAct() == 10 || !isAutoPilot())
				{
					action[getNowAct()]();
					if (getNowAct() != getNextAct())
					{
						setAction(static_cast<ACTION_ID>(getNextAct()));
					}
				}
			}

			public override void terminate()
			{
				for (byte b = 0; b < 2; b++)
				{
					if (pBindObjs_[b] != null)
					{
						BindObject.deleteBindObject(pBindObjs_[b]);
						pBindObjs_[b] = null;
					}
				}
				if (-1 != frogMdlIdx_ && -1 != orgMdlIdx_)
				{
					setCharacterId(orgMdlIdx_);
					characterMng.delCharacter(frogMdlIdx_);
				}
				base.terminate();
			}

			public override void into()
			{
				base.into();
				shadow_type = 1;
				if (isOperater())
				{
					shadow_type = 0;
				}
				else if (NPCAiManager().getAiKind() == CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW)
				{
					setMCLCol(b: true);
					getColFlag_not_and(1);
					getColFlag_or(8);
					isGrv_set(arg0: true);
					shadow_type = 0;
				}
				else
				{
					shadow_type = 1;
				}
				if (getInPutMode() == INPUT_MODE.INPUT_MODE_FIELD)
				{
					shadow_type = 1;
				}
				setShadowType((uint)shadow_type);
				if (shadow_type == 1)
				{
					characterMng.setShadowHeight(getCharacterId(), 2048);
				}
				setAction(ACTION_ID.ACTION_ID_WAIT);
			}

			public override void update()
			{
				base.update();
			}

			public override void reset()
			{
				base.reset();
			}

			public override void checkCollisionCharacter(chr.CCharacterEureka _Target)
			{
				if ((getColType() & 2) != 0 && !AutoRun() && (getNowAct() == 0 || getNowAct() == 1 || getNowAct() == 2 || getNowAct() == 3))
				{
					setTarget(_Target);
				}
				if ((getColType() & 1) == 0 || _Target.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
				{
					return;
				}
				if (isOperater())
				{
					if (_Target.isOperater())
					{
						return;
					}
					if (_Target.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN)
					{
						CPlayerCharacter cPlayerCharacter = static_cast<CPlayerCharacter>(_Target);
						if (cPlayerCharacter.NPCAiManager().getAiKind() != CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW)
						{
							VecFx32 nitro_reuse_v = nitro_reuse_v0;
							VEC_Subtract(getPosition(), _Target.getPosition(), nitro_reuse_v);
							int num = VEC_Mag(nitro_reuse_v);
							VEC_Normalize(nitro_reuse_v, nitro_reuse_v);
							VecFx32 nitro_reuse_pos = GlobalScope.nitro_reuse_pos;
							VEC_MultAdd(getColRadius() + _Target.getColRadius() - num, nitro_reuse_v, getPosition(), nitro_reuse_pos);
							setPosition(nitro_reuse_pos);
							if (!_Target.isOperater())
							{
								_Target.setNextAct(0);
								_Target.setPosition(_Target.getPrePosition());
								_Target.getParamMove().init();
							}
						}
					}
					else
					{
						if (_Target.CharaKind() != chr.CHARACTER_KIND.CHARACTER_KIND_MAP_OBJECT)
						{
							return;
						}
						VecFx32 nitro_reuse_pos2 = GlobalScope.nitro_reuse_pos;
						ds.pri.DSSphere pl_reuse_sphere = pl.pl_reuse_sphere;
						ds.pri.DSAABB pl_reuse_aabb = pl.pl_reuse_aabb;
						pl_reuse_sphere.set(getPosition(), getColRadius());
						pl_reuse_aabb.c.copy(_Target.getPosition());
						pl_reuse_aabb.r.copy(_Target.getColAabbRadius());
						if ((pl_reuse_aabb.c.x - pl_reuse_aabb.r.x < getPosition().x && getPosition().x < pl_reuse_aabb.c.x + pl_reuse_aabb.r.x) || (pl_reuse_aabb.c.z - pl_reuse_aabb.r.z < getPosition().z && getPosition().z < pl_reuse_aabb.c.z + pl_reuse_aabb.r.z))
						{
							nitro_reuse_pos2 = ds.pri.PrimitiveTest.closestPtPointAABB(pl_reuse_sphere.c, pl_reuse_aabb);
							if (nitro_reuse_pos2.x <= pl_reuse_aabb.c.x - pl_reuse_aabb.r.x)
							{
								nitro_reuse_pos2.x -= getColRadius();
							}
							else if (pl_reuse_aabb.c.x + pl_reuse_aabb.r.x <= nitro_reuse_pos2.x)
							{
								nitro_reuse_pos2.x += getColRadius();
							}
							if (nitro_reuse_pos2.z <= pl_reuse_aabb.c.z - pl_reuse_aabb.r.z)
							{
								nitro_reuse_pos2.z -= getColRadius();
							}
							else if (pl_reuse_aabb.c.z + pl_reuse_aabb.r.z <= nitro_reuse_pos2.z)
							{
								nitro_reuse_pos2.z += getColRadius();
							}
							setPosition(nitro_reuse_pos2);
							return;
						}
						ds.pri.DSSphere pl_reuse_sphere2 = pl.pl_reuse_sphere;
						pl_reuse_sphere2.r = 4096;
						if (getPosition().x < pl_reuse_aabb.c.x)
						{
							pl_reuse_sphere2.c.x = pl_reuse_aabb.c.x - pl_reuse_aabb.r.x + 4096;
						}
						else
						{
							pl_reuse_sphere2.c.x = pl_reuse_aabb.c.x + pl_reuse_aabb.r.x - 4096;
						}
						if (getPosition().z < pl_reuse_aabb.c.z)
						{
							pl_reuse_sphere2.c.z = pl_reuse_aabb.c.z - pl_reuse_aabb.r.z + 4096;
						}
						else
						{
							pl_reuse_sphere2.c.z = pl_reuse_aabb.c.z + pl_reuse_aabb.r.z - 4096;
						}
						pl_reuse_sphere2.c.y = getPosition().y;
						VecFx32 nitro_reuse_v2 = nitro_reuse_v0;
						VEC_Subtract(getPosition(), pl_reuse_sphere2.c, nitro_reuse_v2);
						int num2 = VEC_Mag(nitro_reuse_v2);
						VEC_Normalize(nitro_reuse_v2, nitro_reuse_v2);
						VEC_MultAdd(getColRadius() + pl_reuse_sphere2.r - num2, nitro_reuse_v2, getPosition(), nitro_reuse_pos2);
						setPosition(nitro_reuse_pos2);
					}
				}
				else
				{
					CPlayerCharacter cPlayerCharacter2 = static_cast<CPlayerCharacter>(this);
					if (cPlayerCharacter2.NPCAiManager().getAiKind() != CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW)
					{
						setNextAct(0);
						setPosition(getPrePosition());
						getParamMove().init();
						MoveSys().setStop(b: true);
					}
				}
			}

			public void setAction(ACTION_ID _Number)
			{
				setNowAct((int)_Number);
				setAction(m_apAction[getNowAct()]);
			}

			public act.CBaseAction getAction(int _Number)
			{
				return m_apAction[_Number];
			}

			public int getActionId()
			{
				for (int i = 0; i < 14; i++)
				{
					if (m_ActionMng.getAction() == getAction(i))
					{
						return (int)static_cast<ACTION_ID>(i);
					}
				}
				return -1;
			}

			public override bool canEncount()
			{
				bool flag = 1 == getNowAct() || 2 == getNowAct();
				bool result = 1 <= m_LandFormIndex && m_LandFormIndex <= 12;
				if (flag)
				{
					return result;
				}
				return false;
			}

			public override bool canOpenMenu()
			{
				if (isAutoPilot())
				{
					return false;
				}
				bool flag = getNowAct() == 0 || getNowAct() == 3 || getNowAct() == 1 || getNowAct() == 2;
				bool result = getNextAct() == -1 || getNextAct() == 0 || getNextAct() == 3 || getNextAct() == 1 || getNextAct() == 2;
				if (flag)
				{
					return result;
				}
				return false;
			}

			public void addMotion(string pMotname)
			{
				characterMng.addMotion(getCharacterId(), pMotname);
			}

			public void setBindObject(BindObject pBindObj, BIND_LOCATE locate)
			{
				pBindObjs_[(int)locate] = pBindObj;
				pBindObjs_[(int)locate].setLocate(LOCATE_NAME[(int)locate]);
			}

			public void deleteBindObject(BIND_LOCATE locate)
			{
				BindObject.deleteBindObject(pBindObjs_[(int)locate]);
				pBindObjs_[(int)locate] = null;
			}

			public void actionHumanWait()
			{
				setMass(0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).MoveSkg() * (60 / chr.CCharacterEureka.m_CharaFps)), (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NMveMax() * (60 / chr.CCharacterEureka.m_CharaFps)), (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnAcc() * (60 / chr.CCharacterEureka.m_CharaFps)), (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).TurnSkg() * (60 / chr.CCharacterEureka.m_CharaFps)), (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnMax() * (60 / chr.CCharacterEureka.m_CharaFps)));
			}

			public void actionHumanWalk()
			{
				setMass((int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NMveAcc() * (60 / chr.CCharacterEureka.m_CharaFps)), 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NMveMax() * (60 / chr.CCharacterEureka.m_CharaFps)), (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnAcc() * (60 / chr.CCharacterEureka.m_CharaFps)), 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnMax() * (60 / chr.CCharacterEureka.m_CharaFps)));
			}

			public void actionHumanRun()
			{
				int v = (int)(CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).SMveAcc() * 1.6f);
				int v2 = (int)(CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).SMveMax() * 1.6f);
				int v3 = 4736;
				v = FX_Mul(v3, v);
				v2 = FX_Mul(v3, v2);
				setMass(v, 0, v2, (int)(CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).STrnAcc() * 1.6f), 0, (int)(CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).STrnMax() * 1.6f));
			}

			public void actionHumanLeaveWait()
			{
				setMass(0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).MoveSkg() * (60 / chr.CCharacterEureka.m_CharaFps)), 0, 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).TurnSkg() * (60 / chr.CCharacterEureka.m_CharaFps)), 0);
			}

			public void actionHumanTalk()
			{
			}

			public void actionHumanCheck()
			{
				setMass(0, 0, 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnAcc() * (60 / chr.CCharacterEureka.m_CharaFps)), 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnMax() * (60 / chr.CCharacterEureka.m_CharaFps)));
			}

			public void actionHumanUseItem()
			{
				setMass(0, 0, 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnAcc() * (60 / chr.CCharacterEureka.m_CharaFps)), 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnMax() * (60 / chr.CCharacterEureka.m_CharaFps)));
			}

			public void actionHumanBoard()
			{
				setMass(1, 0, 1, 0, 0, 0);
			}

			public void actionHumanOut()
			{
				setMass(1, 0, 1, 0, 0, 0);
			}

			public void actionHumanEmbark()
			{
				setMass(0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).MoveSkg() * (60 / chr.CCharacterEureka.m_CharaFps)), 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnAcc() * (60 / chr.CCharacterEureka.m_CharaFps)), 0, 0);
			}

			public void actionHumanLeave()
			{
				setMass((int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NMveAcc() * (60 / chr.CCharacterEureka.m_CharaFps)), 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NMveMax() * (60 / chr.CCharacterEureka.m_CharaFps)), (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnAcc() * (60 / chr.CCharacterEureka.m_CharaFps)), 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnMax() * (60 / chr.CCharacterEureka.m_CharaFps)));
			}

			public void actionHumanKeyDoor()
			{
				setMass(0, 0, 0, 0, 0, 0);
			}

			public void actionHumanKeyDoorItemUse()
			{
				setMass(0, 0, 0, 0, 0, 0);
			}

			public void actionHumanInn()
			{
			}

			public void setMenuIcon(wld.CMenuButton pButton)
			{
				pMenuIcon_ = pButton;
			}

			public void setCameraIcon(wld.CMenuButton pButton)
			{
				pCameraIcon_ = pButton;
			}

			public void setTalkIcon(wld.CMenuButton pButton)
			{
				pTalkIcon_ = pButton;
			}

			public wld.CMenuButton getMenuIcon()
			{
				return pMenuIcon_;
			}

			public wld.CMenuButton getCameraIcon()
			{
				return pCameraIcon_;
			}

			public wld.CMenuButton getTalkIcon()
			{
				if (getNpc() == null)
				{
					return null;
				}
				if (!hasTalkNpc())
				{
					return null;
				}
				return pTalkIcon_;
			}

			public bool hasTalkNpc()
			{
				if (getNpc() == null)
				{
					return false;
				}
				if (strcmp("n272", getNpc().getModelName()) == 0)
				{
					return false;
				}
				return true;
			}

			public CPlayerHuman()
			{
				composit2 = new MapMarkerAccepterHuman(this);
				action[0] = actionHumanWait;
				action[1] = actionHumanWalk;
				action[2] = actionHumanRun;
				action[3] = actionHumanLeaveWait;
				action[4] = actionHumanTalk;
				action[5] = actionHumanCheck;
				action[6] = actionHumanUseItem;
				action[7] = actionHumanBoard;
				action[8] = actionHumanOut;
				action[9] = actionHumanEmbark;
				action[10] = actionHumanLeave;
				action[11] = actionHumanKeyDoor;
				action[12] = actionHumanKeyDoorItemUse;
				action[13] = actionHumanInn;
				m_apAction[0] = m_HumanWait;
				m_apAction[1] = m_HumanWalk;
				m_apAction[2] = m_HumanRun;
				m_apAction[3] = m_HumanLeaveWait;
				m_apAction[4] = m_HumanTalk;
				m_apAction[5] = m_HumanCheck;
				m_apAction[6] = m_HumanUseItem;
				m_apAction[7] = m_HumanBoard;
				m_apAction[8] = m_HumanOut;
				m_apAction[9] = m_HumanEmbark;
				m_apAction[10] = m_HumanLeave;
				m_apAction[11] = m_HumanKeyDoor;
				m_apAction[12] = m_HumanKeyDoorItemUse;
				m_apAction[13] = m_HumanInn;
			}

			public PLAYER_HUMAN_TYPE getHumanType()
			{
				return m_HumanType;
			}

			public PLAYER_HUMAN_TYPE HumanType()
			{
				return m_HumanType;
			}

			public void HumanType_set(PLAYER_HUMAN_TYPE arg0)
			{
				m_HumanType = arg0;
			}

			public void setNpc(CPlayerHuman pNpc)
			{
				m_pNpc = pNpc;
			}

			public CPlayerHuman getNpc()
			{
				return m_pNpc;
			}

			public BindObject getBindObject(BIND_LOCATE locate)
			{
				return pBindObjs_[(int)locate];
			}

			public int getShadowType()
			{
				return shadow_type;
			}

			public void setVisibleMarker(bool b)
			{
				visibleMarker_ = b;
			}
		}
	}
}
