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
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class pl
	{
		public class VehicleNorchi : CPlayerVehicle
		{
			public enum FLAG_TYPE
			{
				FLAG_ONSEA = 1
			}

			public const FLAG_TYPE FLAG_ONSEA = FLAG_TYPE.FLAG_ONSEA;

			private byte _flag;

			private byte canPlayDropEffect_;

			private byte _fromSeaToAir;

			private int dropEffIdx_;

			public override void initialize()
			{
				base.initialize();
				_actionList[0] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(NorchiActionWait));
				_actionList[1] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(NorchiActionNavigate));
				_actionList[2] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(NorchiActionRise));
				_actionList[3] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(NorchiActionDescent));
				_actionList[4] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(VehicleGetOn));
				_actionList[5] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(VehicleGetOff));
				registerAction(ACTION_ID.ACTION_ID_WAIT, _actionList[0]);
				registerAction(ACTION_ID.ACTION_ID_NAVIGATE, _actionList[1]);
				registerAction(ACTION_ID.ACTION_ID_RISE, _actionList[2]);
				registerAction(ACTION_ID.ACTION_ID_DESCENT, _actionList[3]);
				registerAction(ACTION_ID.ACTION_ID_APPEAR, _actionList[4]);
				registerAction(ACTION_ID.ACTION_ID_DISAPPEAR, _actionList[5]);
				MapMarkerUpdater.getSingleton().registerAccepter(composit2, 14);
				_fromSeaToAir = 0;
			}

			public override void into()
			{
				base.into();
				int characterId = getCharacterId();
				characterMng.removeAllMotion(characterId);
				characterMng.addMotion(characterId, "w_act_n491");
				characterMng.startMotion(characterId, VEHICLE_MOTIONNO_WAIT, fLoop: true, 5u);
				characterMng.setMotionSpeed(characterId, 0);
				setGrv(_GrvFlag: false);
				setOnAir();
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
				canPlayDropEffect_ = 0;
				dropEffIdx_ = -1;
			}

			public override void execute()
			{
				base.execute();
			}

			public override void update()
			{
				base.update();
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

			public void setOnSea()
			{
				_flag |= 1;
				PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_NORCHI_CTM);
				getColFlag_or(4096);
				getColFlag_or(8192);
				getColFlag_or(16384);
				getColFlag_or(32768);
				getColFlag_or(65536);
				getColFlag_or(131072);
			}

			public void setOnAir()
			{
				_flag &= 254;
				PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_NORCHI);
				getColFlag_or(4096);
				getColFlag_not_and(8192);
				getColFlag_not_and(16384);
				getColFlag_not_and(32768);
				getColFlag_not_and(65536);
				getColFlag_or(131072);
			}

			public override void setConditionOfAir()
			{
				getPosition().y = VEHICLE_HEIGHT_AIR;
				setOnAir();
				setPreAct(0);
				setNowAct(0);
				setNextAct(0);
				startMotion(VEHICLE_MOTIONNO_WAIT, _Loop: true, 5u);
				characterMng.setMotionSpeed(getCharacterId(), 0);
				if (4 == sceneMng.getPreFieldNo())
				{
					setNextAct(2);
					VecFx32 vecFx = new VecFx32(getPosition());
					vecFx.y = -122880;
					setPosition(vecFx);
					setAutoPilot(_AutoPilot: true);
					setOperater(_Operater: false);
					_fromSeaToAir = 1;
					canPlayDropEffect_ = 1;
				}
			}

			public override void setConditionOfDeepSea()
			{
				setOnSea();
				startMotion(VEHICLE_MOTIONNO_WAIT, _Loop: true, 5u);
				characterMng.setMotionSpeed(getCharacterId(), 0);
				VecFx32 scale = new VecFx32(6144, 40960, 6144);
				characterMng.setShadowHeight(getCharacterId(), 0);
				characterMng.setShadowScale(getCharacterId(), scale);
				if (3 == sceneMng.getPreFieldNo())
				{
					setNextAct(3);
					VecFx32 vecFx = new VecFx32(getPosition());
					vecFx.y = 69632;
					setPosition(vecFx);
					setAutoPilot(_AutoPilot: true);
					setOperater(_Operater: false);
				}
				else
				{
					setNextAct(0);
					VecFx32 vecFx2 = new VecFx32(getPosition());
					vecFx2.y = 8192;
					setPosition(vecFx2);
					setOperater(_Operater: true);
					setAutoPilot(_AutoPilot: false);
					setMCLCol(b: true);
					getColFlag_or(2);
					getColFlag_or(4);
				}
			}

			public override bool checkNextActionToRise()
			{
				if (((dv.CDeviceManager.getInstance().Pad().edge_trs(0) & 0x80) != 0 || (_tpFlag & 2) != 0) && isOnSea())
				{
					short num = (short)(getLandFormIndex() - 1);
					if (9 != num)
					{
						return true;
					}
				}
				return false;
			}

			public override bool checkNextActionToDescent()
			{
				if (((dv.CDeviceManager.getInstance().Pad().edge_trs(0) & 0x20) != 0 || (_tpFlag & 2) != 0) && isOnAir())
				{
					return true;
				}
				return false;
			}

			public override bool canEncount()
			{
				if (1 != getNowAct())
				{
					return false;
				}
				if (!isOnSea())
				{
					return sAttr_;
				}
				return true;
			}

			public override int getBattleMapNo()
			{
				if (isOnSea())
				{
					return BATTLE_MAP_INDEEPSEA;
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

			public override void updateCameraHeight()
			{
				if (!isOnSea())
				{
					base.updateCameraHeight();
				}
			}

			public override void playRiseSE()
			{
				if (!isPlayingRiseSE())
				{
					int num = -1;
					if (_fromSeaToAir == 0)
					{
						num = ((4 != sceneMng.getFieldNo()) ? 7 : 16);
					}
					else
					{
						num = 17;
						_fromSeaToAir = 0;
					}
					if (num != -1)
					{
						hRiseSE_ = MatrixSound.MtxSENDS_Play(1, num, 192, 127);
					}
				}
			}

			public override void playDescSE()
			{
				if (!isPlayingDescSE() && sceneMng.getFieldNo() != 4)
				{
					int num = -1;
					num = ((getLandFormIndex() - 1 != 10) ? 8 : 18);
					if (num != -1)
					{
						hDescSE_ = MatrixSound.MtxSENDS_Play(1, num, 192, 127);
					}
				}
			}

			public override void playNaviSE()
			{
				if (canPlayNaviSE())
				{
					if (isOnAir())
					{
						hNaviSE_ = MatrixSound.MtxSENDS_Play(0, 9, 192, 127);
					}
					else
					{
						hNaviSE_ = MatrixSound.MtxSENDS_Play(0, 22, 192, 127);
					}
				}
			}

			public override bool calculateWallCollision(dgs.CRestrictor ror, mcl.CollisionResult result, int attr, int radius, int revise, VecFx32 current_position, VecFx32 old_position)
			{
				bool result2 = false;
				if (isOnSea())
				{
					VecFx32 vecFx = new VecFx32(0, 0, 0);
					VEC_Subtract(current_position, old_position, vecFx);
					if (vecFx.x == 0 && vecFx.y == 0 && vecFx.z == 0)
					{
						return false;
					}
					VEC_Normalize(vecFx, vecFx);
					if (ror.rorEvaluateSphere(current_position, vecFx, radius, attr, result))
					{
						VEC_MultAdd(radius - result.length, result.normal, current_position, current_position);
						result2 = true;
					}
				}
				else
				{
					result2 = base.calculateWallCollision(ror, result, attr, radius, revise, current_position, old_position);
				}
				return result2;
			}

			public int playDropEffect()
			{
				if (canPlayDropEffect_ != 0)
				{
					canPlayDropEffect_ = 0;
					VecFx32 vecFx = new VecFx32(getPosition());
					vecFx.y = map.MAP_HEIGHT_SEA;
					int id = eff.CEffectMng.instance().create(EFFECT_CATEGORY_VEHICLE, EFFECT_MEMBER_SPRAY);
					eff.CEffectMng.instance().setPosition(id, vecFx);
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
				return -1;
			}

			public bool isOnSea()
			{
				return (_flag & 1) != 0;
			}

			public override bool isOnAir()
			{
				return (_flag & 1) == 0;
			}
		}
	}
}
