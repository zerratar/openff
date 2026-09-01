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
		public class VehicleEnterp : CPlayerVehicle
		{
			public enum FLAG_TYPE
			{
				FLAG_ONSEA = 1
			}

			public const FLAG_TYPE FLAG_ONSEA = FLAG_TYPE.FLAG_ONSEA;

			private static byte WALL_HIT_FRAME = 10;

			private byte _flag;

			private byte _wallHitFrame;

			private sbyte _waveCharaIdx;

			private VecFx32 _wallNormal = new VecFx32();

			private bool _canLand;

			private int dropEffIdx_;

			public override void initialize()
			{
				base.initialize();
				_flag = 0;
				_flag |= 1;
				_actionList[0] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(EnterpActionWait));
				_actionList[1] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(EnterpActionNavigate));
				_actionList[2] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(EnterpActionRise));
				_actionList[3] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(EnterpActionDescent));
				_actionList[4] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(EnterpActionGetOn));
				_actionList[5] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(EnterpActionGetOff));
				registerAction(ACTION_ID.ACTION_ID_WAIT, _actionList[0]);
				registerAction(ACTION_ID.ACTION_ID_NAVIGATE, _actionList[1]);
				registerAction(ACTION_ID.ACTION_ID_RISE, _actionList[2]);
				registerAction(ACTION_ID.ACTION_ID_DESCENT, _actionList[3]);
				registerAction(ACTION_ID.ACTION_ID_APPEAR, _actionList[4]);
				registerAction(ACTION_ID.ACTION_ID_DISAPPEAR, _actionList[5]);
				MapMarkerUpdater.getSingleton().registerAccepter(composit2, 12);
			}

			public override void into()
			{
				base.into();
				int characterId = getCharacterId();
				characterMng.removeAllMotion(characterId);
				characterMng.addMotion(characterId, "w_act_n451");
				characterMng.startMotion(characterId, ENTERP_MOTIONNO_SHIP_WAIT, fLoop: true, 5u);
				setMast();
				setGrv(_GrvFlag: false);
				setOnSea();
				getPosition().y = VEHICLE_HEIGHT_ONSEA;
				_wallHitFrame = 0;
				_canLand = false;
				dropEffIdx_ = -1;
				_waveCharaIdx = (sbyte)characterMng.setCharacter("n452", CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST);
				characterMng.setShadowType(_waveCharaIdx, 2);
			}

			public override void terminate()
			{
				characterMng.delCharacter(_waveCharaIdx);
				_waveCharaIdx = -1;
				base.terminate();
			}

			public override void execute()
			{
				base.execute();
				if (1 == getColResultWall(3).hit)
				{
					VecFx32 vecFx = new VecFx32(getColResultWall(3).normal);
					VecFx32 vecFx2 = new VecFx32(getDirection());
					VEC_Normalize(vecFx2, vecFx2);
					_wallNormal.copy(vecFx);
					bool flag = true;
					if (vecFx.x == 0 && vecFx.y == 0 && vecFx.z == 0)
					{
						flag = false;
					}
					int num = VEC_DotProduct(vecFx, vecFx2);
					if (num <= -3547 && flag)
					{
						if (++_wallHitFrame >= byte.MaxValue)
						{
							_wallHitFrame = byte.MaxValue;
						}
					}
					else
					{
						_wallHitFrame = 0;
					}
				}
				else
				{
					_wallHitFrame = 0;
				}
			}

			public override void update()
			{
				base.update();
				MtxFx43 mtxFx = new MtxFx43();
				MtxFx43 mtxFx2 = new MtxFx43();
				characterMng.getPoseMtx(getCharacterId(), mtxFx);
				MTX_Scale43(mtxFx2, 4096, 4096, 4096);
				MTX_Concat43(mtxFx2, mtxFx, mtxFx);
				characterMng.setPoseMtx(_waveCharaIdx, mtxFx);
				if (-1 != dropEffIdx_)
				{
					if (eff.CEffectMng.instance().isEffectObject(dropEffIdx_))
					{
						eff.CEffectMng.instance().setPosition(dropEffIdx_, getPosition());
					}
					else
					{
						dropEffIdx_ = -1;
					}
				}
			}

			public void setMast()
			{
				characterMng.setFrame(getCharacterId(), 0u, ds.sys3d.CAnimSet.enTYPE.enTYPE_IVA);
				characterMng.setPause(getCharacterId(), pause: true, ds.sys3d.CAnimSet.enTYPE.enTYPE_IVA);
			}

			public void setPropeller()
			{
				characterMng.setFrame(getCharacterId(), 1u, ds.sys3d.CAnimSet.enTYPE.enTYPE_IVA);
				characterMng.setPause(getCharacterId(), pause: true, ds.sys3d.CAnimSet.enTYPE.enTYPE_IVA);
			}

			public void setOnSea()
			{
				if ((1 & _flag) == 0)
				{
					MapMarkerUpdater.getSingleton().deregisterAccepter(composit2);
					MapMarkerUpdater.getSingleton().registerAccepter(composit2, 12);
					composit2.dir_ = -1;
				}
				_flag |= 1;
				getColFlag_set(0);
				getColFlag_or(2);
				getColFlag_or(4);
				getColFlag_or(16);
				getColFlag_or(32);
				getColFlag_or(128);
				getColFlag_or(256);
				getColFlag_or(512);
				PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_ENTERP);
				setShadowType(2u);
			}

			public void setOnAir()
			{
				_flag &= 254;
				getColFlag_set(0);
				getColFlag_or(2);
				getColFlag_or(4);
				getColFlag_or(16);
				getColFlag_or(512);
				getColFlag_or(4096);
				getColFlag_or(131072);
				getColFlag_or(2097152);
				getColFlag_or(4194304);
				getColFlag_or(8388608);
				getColFlag_or(2048);
				PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_ENTERP_CTM);
				setShadowType(4u);
				MapMarkerUpdater.getSingleton().deregisterAccepter(composit2);
				MapMarkerUpdater.getSingleton().registerAccepter(composit2, 13);
				composit2.dir_ = -1;
			}

			public override void setBoard()
			{
				base.setBoard();
				setOnSea();
				setMotionSpeed(4096);
				setAutoPilot(_AutoPilot: false);
				setOperater(_Operater: true);
				_wallHitFrame = 0;
			}

			public override void dropPlayer()
			{
				CPlayerCharacter boardPlayer = getBoardPlayer();
				base.dropPlayer();
				setMotionSpeed(4096);
				VecFx32 vecFx = new VecFx32(_wallNormal);
				VecFx32 vecFx2 = new VecFx32(boardPlayer.getPosition());
				vecFx.x *= -1;
				vecFx.y *= -1;
				vecFx.z *= -1;
				boardPlayer.setDirection(vecFx);
				boardPlayer.setTargetDirection(vecFx);
				VecFx32 vecFx3 = new VecFx32(0, 0, 0);
				VEC_MultAdd(4096, vecFx, vecFx2, vecFx3);
				boardPlayer.MoveSys().setTargetPoint(0, vecFx2);
				boardPlayer.MoveSys().setTargetPoint(1, vecFx3);
				boardPlayer.MoveSys().setFlag(_Flag: false);
				_wallHitFrame = 0;
			}

			public override void setConditionOfAir()
			{
				getPosition().y = VEHICLE_HEIGHT_AIR;
				setOnAir();
				setPropeller();
				setPreAct(0);
				setNowAct(0);
				setNextAct(0);
				startMotion(ENTERP_MOTIONNO_AIR_WAIT, _Loop: true, 5u);
				setVisibleWave(b: false);
			}

			public override bool checkNextActionToRise()
			{
				if (((dv.CDeviceManager.getInstance().Pad().edge_trs(0) & 0x80) != 0 || (_tpFlag & 2) != 0) && getVehicleType() == PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM && isOnSea())
				{
					return true;
				}
				return false;
			}

			public override bool checkNextActionToDescent()
			{
				if (((dv.CDeviceManager.getInstance().Pad().edge_trs(0) & 0x20) != 0 || (_tpFlag & 2) != 0) && getVehicleType() == PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM && isOnAir())
				{
					return true;
				}
				return false;
			}

			public bool checkNextActionToLeave()
			{
				bool flag = getColResultWall(3).hit != 0;
				VecFx32 normal = getColResultWall(3).normal;
				if (_wallHitFrame >= WALL_HIT_FRAME && flag && (normal.x != 0 || normal.y != 0 || normal.z != 0) && isOnSea() && _canLand)
				{
					return true;
				}
				_canLand = false;
				return false;
			}

			public override bool canEncount()
			{
				if (1 != getNowAct())
				{
					return false;
				}
				if (isOnSea())
				{
					return 2 != sceneMng.getFieldNo();
				}
				return sAttr_;
			}

			public override int getBattleMapNo()
			{
				if (isOnSea())
				{
					return BATTLE_MAP_ONSEA;
				}
				if (isOnAir())
				{
					return BATTLE_MAP_ONAIR;
				}
				return 1;
			}

			public override int getMonsterPartyGroupNo()
			{
				if (isOnAir())
				{
					return MONSTER_GROUP_ONAIR;
				}
				return monPartyGroupNo_;
			}

			public override int getWaitMotionIndex()
			{
				if (isOnSea())
				{
					return ENTERP_MOTIONNO_SHIP_WAIT;
				}
				return ENTERP_MOTIONNO_AIR_WAIT;
			}

			public void setVisibleWave(bool b)
			{
				characterMng.setHidden(_waveCharaIdx, !b);
			}

			public override int getBGMNo()
			{
				if (isOnAir())
				{
					return 9;
				}
				return 20;
			}

			public override void playRiseSE()
			{
				if (!isPlayingRiseSE())
				{
					hRiseSE_ = MatrixSound.MtxSENDS_Play(1, 14, 192, 127);
				}
			}

			public override void playDescSE()
			{
				if (!isPlayingDescSE())
				{
					hDescSE_ = MatrixSound.MtxSENDS_Play(1, 15, 192, 127);
				}
			}

			public override void playNaviSE()
			{
				if (canPlayNaviSE())
				{
					int num = -1;
					num = ((!isOnSea()) ? 9 : 10);
					if (-1 != num)
					{
						hNaviSE_ = MatrixSound.MtxSENDS_Play(0, num, 192, 127);
					}
				}
			}

			public int playDropEffect()
			{
				if (-1 != dropEffIdx_ && eff.CEffectMng.instance().isEffectObject(dropEffIdx_))
				{
					eff.CEffectMng.instance().release(dropEffIdx_);
					dropEffIdx_ = -1;
				}
				VecFx32 pos = new VecFx32(getPosition());
				dropEffIdx_ = eff.CEffectMng.instance().create(EFFECT_CATEGORY_VEHICLE, EFFECT_MEMBER_DROP);
				eff.CEffectMng.instance().setPosition(dropEffIdx_, pos);
				return dropEffIdx_;
			}

			public override bool calculateWallCollision(dgs.CRestrictor ror, mcl.CollisionResult result, int attr, int radius, int revise, VecFx32 current_position, VecFx32 old_position)
			{
				bool flag = false;
				if (isOnSea())
				{
					int num = -VEHICLE_HEIGHT_ONSEA;
					current_position.y += num;
					old_position.y += num;
					VecFx32 vecFx = new VecFx32(0, 0, 0);
					VEC_Subtract(current_position, old_position, vecFx);
					if (vecFx.x == 0 && vecFx.y == 0 && vecFx.z == 0)
					{
						current_position.y -= num;
						old_position.y -= num;
						return false;
					}
					VEC_Normalize(vecFx, vecFx);
					if (ror.rorEvaluateSphere(current_position, vecFx, radius, attr, result))
					{
						VEC_MultAdd(radius - result.length, result.normal, current_position, current_position);
						flag = true;
					}
					current_position.y -= num;
					old_position.y -= num;
					if (flag && _wallHitFrame >= WALL_HIT_FRAME)
					{
						VecFx32 vecFx2 = new VecFx32(getColResultWall(3).normal);
						VecFx32 v = new VecFx32(-1 * vecFx2.x, -1 * vecFx2.y, -1 * vecFx2.z);
						VecFx32 direction = new VecFx32(0, -4096, 0);
						VecFx32 vecFx3 = new VecFx32(current_position);
						vecFx3.y = 0;
						vecFx3.y = 16384;
						VEC_MultAdd(20480, v, vecFx3, vecFx3);
						for (byte b = 0; b < 20; b++)
						{
							mcl.CollisionResult pl_reuse_result = pl.pl_reuse_result;
							if (ror.rorEvaluateArrow(vecFx3, direction, 20480, 1, pl_reuse_result) && (pl_reuse_result.material.getAttribute().isEnableFlag(8u) || pl_reuse_result.material.getAttribute().isEnableFlag(9u) || pl_reuse_result.material.getAttribute().isEnableFlag(10u) || pl_reuse_result.material.getAttribute().isEnableFlag(13u)))
							{
								_canLand = true;
								break;
							}
							VEC_MultAdd(4096, v, vecFx3, vecFx3);
						}
					}
				}
				else
				{
					flag = base.calculateWallCollision(ror, result, attr, radius, revise, current_position, old_position);
				}
				return flag;
			}

			public override byte getStartActionID()
			{
				return 0;
			}

			public bool isOnSea()
			{
				return (_flag & 1) != 0;
			}

			public override bool isOnAir()
			{
				return (_flag & 1) == 0;
			}

			public byte getWallHitFrame()
			{
				return _wallHitFrame;
			}
		}
	}
}
