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
		public class VehicleInvinsible : CPlayerVehicle
		{
			public static int HIGHNAVIGATE_DOT_RANGE = -3686;

			public static int HIGHT_HIGH = 98304;

			public static int MOT_SPEED_NORMAL = 4096;

			public static int MOT_SPEED_HIGH = 20480;

			private VecFx32 dir_ = new VecFx32();

			private VecFx32 jump12Normal_ = new VecFx32();

			private byte jump12HitFrame_;

			private bool canHiNaviByTouch_;

			private int myselfIdx_;

			private int cameraTargetIdx_;

			public override void initialize()
			{
				base.initialize();
				_actionList[0] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(InvinsibleActionWait));
				_actionList[1] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(InvinsibleActionNavigate));
				_actionList[2] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(InvinsibleActionRise));
				_actionList[3] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(InvinsibleActionDescent));
				_actionList[4] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(InvinsibleActionHighNavigate));
				_actionList[5] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(InvinsibleActionEnterInside));
				registerAction(ACTION_ID.ACTION_ID_WAIT, _actionList[0]);
				registerAction(ACTION_ID.ACTION_ID_NAVIGATE, _actionList[1]);
				registerAction(ACTION_ID.ACTION_ID_RISE, _actionList[2]);
				registerAction(ACTION_ID.ACTION_ID_DESCENT, _actionList[3]);
				registerAction(ACTION_ID.ACTION_ID_APPEAR, _actionList[4]);
				registerAction(ACTION_ID.ACTION_ID_DISAPPEAR, _actionList[5]);
				MapMarkerUpdater.getSingleton().registerAccepter(composit2, 13);
			}

			public override void into()
			{
				base.into();
				int characterId = getCharacterId();
				characterMng.removeAllMotion(characterId);
				characterMng.addMotion(characterId, "w_act_n511");
				characterMng.startMotion(characterId, VEHICLE_MOTIONNO_WAIT, fLoop: true, 5u);
				setMCLCol(b: true);
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
				getColFlag_or(16777216);
				getColFlag_or(2048);
				getParamObj().m_Pos.y = VEHICLE_HEIGHT_AIR;
				setGrv(_GrvFlag: false);
				VEC_Set(dir_, 0, 0, 0);
				VEC_Set(jump12Normal_, 0, 0, 0);
				jump12HitFrame_ = 0;
				canHiNaviByTouch_ = false;
				VecFx32 vecFx = new VecFx32(0, 0, 0);
				cameraTargetIdx_ = CCastCommandTransit.getInstance().cast_PlayerMng().setUpWorldCharacter(vecFx, vecFx, vecFx, vecFx, "o000", _AutoPilot: false, _Operater: false);
				CCastCommandTransit.getInstance().cast_PlayerMng().Player(cameraTargetIdx_)
					.getColAabbRadius_set(vecFx);
				CCastCommandTransit.getInstance().cast_PlayerMng().Player(cameraTargetIdx_)
					.getColOffset()
					.x = 0;
				CCastCommandTransit.getInstance().cast_PlayerMng().Player(cameraTargetIdx_)
					.getColOffset()
					.y = 409600;
				CCastCommandTransit.getInstance().cast_PlayerMng().Player(cameraTargetIdx_)
					.getColOffset()
					.z = 0;
				CCastCommandTransit.getInstance().cast_PlayerMng().Player(cameraTargetIdx_)
					.getPosition()
					.y = VEHICLE_HEIGHT_AIR;
				myselfIdx_ = -1;
			}

			public override void execute()
			{
				base.execute();
			}

			public override void update()
			{
				base.update();
			}

			public override void setBoard()
			{
				base.setBoard();
				setMotionSpeed(4096);
				wld.CWorldOutSideData.getInstance().MapData().setMapJumpEnable(b: true);
			}

			public override void dropPlayer()
			{
				base.dropPlayer();
				setMotionSpeed(4096);
			}

			public override void setConditionOfAir()
			{
				CCastCommandTransit.getInstance().cast_PlayerMng().Player(cameraTargetIdx_)
					.getPosition_set(getPosition());
				setAutoPilot(_AutoPilot: false);
				setOperater(_Operater: true);
				setNextAct(0);
			}

			public bool checkNextActionToHighNavigate()
			{
				sbyte b = wld.CWorldOutSideData.getInstance().MapData().MapJumpIndex();
				if (12 == b)
				{
					VecFx32 vecFx = new VecFx32(wld.CWorldOutSideData.getInstance().MapData().MapJumpNormal());
					VecFx32 b2 = new VecFx32(getDirectionForRotY());
					VEC_Normalize(vecFx, vecFx);
					int num = VEC_DotProduct(vecFx, b2);
					if (num > -4096)
					{
						return false;
					}
					return true;
				}
				return false;
			}

			public override bool checkNextActionToRise()
			{
				if (checkActionTrigger() && checkNextActionToHighNavigate())
				{
					return true;
				}
				return false;
			}

			public bool checkToEnterInside()
			{
				VecFx32 pos = getParamObj().m_Pos;
				VecFx32 pos2 = getPreParamObj().m_Pos;
				if (pos.x != pos2.x || pos.y != pos2.y || pos.z != pos2.z)
				{
					return false;
				}
				if ((dv.CDeviceManager.getInstance().Pad().edge_trs(0) & 0x20) != 0 || (_tpFlag & 2) != 0)
				{
					return true;
				}
				return false;
			}

			public bool checkToDropPlayer()
			{
				short landFormIndex = getLandFormIndex();
				if (landFormIndex < 1 || landFormIndex > 10)
				{
					return false;
				}
				if (1 == landFormIndex || 2 == landFormIndex || 3 == landFormIndex || 6 == landFormIndex || 9 == landFormIndex)
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
				return sAttr_;
			}

			public void enterInside()
			{
				bool mapJumpEnable = false;
				if (checkToDropPlayer())
				{
					mapJumpEnable = true;
				}
				wld.CWorldOutSideData.getInstance().MapData().setMapJumpEnable(mapJumpEnable);
				wld.CWorldOutSideData.getInstance().MapData().isColFlag_not_and(2048);
				wld.CWorldOutSideData.getInstance().MapData().MapJumpIndex_set(1);
				wld.CWorldOutSideData.getInstance().MapData().setSpMapType(wld.CMapData.SP_MAP_TYPE.SP_MAP_INVINSIBLE);
				setBoardPlayer(null);
				stopNaviSE(0);
			}

			public override void playRiseSE()
			{
				if (!isPlayingRiseSE())
				{
					hRiseSE_ = MatrixSound.MtxSENDS_Play(1, 12, 192, 127);
				}
			}

			public override void playNaviSE()
			{
				if (canPlayNaviSE())
				{
					hNaviSE_ = MatrixSound.MtxSENDS_Play(0, 11, 192, 127);
				}
			}

			public int calcMotSpeedForHighNavi()
			{
				if (getPosition().y <= VEHICLE_HEIGHT_AIR)
				{
					return MOT_SPEED_NORMAL;
				}
				if (getPosition().y >= HIGHT_HIGH)
				{
					return MOT_SPEED_HIGH;
				}
				int v = FX_Div(MOT_SPEED_HIGH - MOT_SPEED_NORMAL, HIGHT_HIGH - VEHICLE_HEIGHT_AIR);
				return FX_Mul(v, getPosition().y - VEHICLE_HEIGHT_AIR) + MOT_SPEED_NORMAL;
			}

			public override short calculateJumpCollision(dgs.CRestrictor ror, mcl.CollisionResult ret, int attr, int rad, VecFx32 nowPos, VecFx32 prePos)
			{
				short num = -1;
				VecFx32 vecFx = new VecFx32(getDirectionForRotY());
				VecFx32 vecFx2 = new VecFx32(0, 0, 0);
				int num2 = 20480;
				VEC_MultAdd(-num2, vecFx, prePos, vecFx2);
				VEC_Set(jump12Normal_, 0, 0, 0);
				if (ror.rorEvaluateArrow(vecFx2, vecFx, num2, 36, ret) && ret.material.getAttribute().isEnableFlag(36u))
				{
					jump12Normal_.copy(ret.normal);
					num = 12;
				}
				if (-1 == num)
				{
					num = base.calculateJumpCollision(ror, ret, attr, rad, nowPos, prePos);
					if (12 == num)
					{
						num = -1;
					}
				}
				return num;
			}

			public void countJump12HitFrame()
			{
				VecFx32 vecFx = new VecFx32();
				if (ds.g_TouchPanel.isTouch() && 1 == getColResultWall(4).hit && (jump12Normal_.x != 0 || jump12Normal_.y != 0 || jump12Normal_.z != 0))
				{
					vecFx.copy(getDirectionForRotY());
					int num = VEC_DotProduct(vecFx, jump12Normal_);
					if (num <= -4014)
					{
						if (++jump12HitFrame_ >= byte.MaxValue)
						{
							jump12HitFrame_ = byte.MaxValue;
						}
						return;
					}
				}
				jump12HitFrame_ = 0;
			}

			public override void dgsredAccept(dgs.CRestrictor ror)
			{
				base.dgsredAccept(ror);
				if (-1 != cameraTargetIdx_ && enableCalcCameraHeight_ && pullBoardPlayer_ && getBoardPlayer() != null)
				{
					CCastCommandTransit.getInstance().cast_PlayerMng().Player(cameraTargetIdx_)
						.getPosition()
						.x = getPosition().x;
					CCastCommandTransit.getInstance().cast_PlayerMng().Player(cameraTargetIdx_)
						.getPosition()
						.y = VEHICLE_HEIGHT_AIR;
					CCastCommandTransit.getInstance().cast_PlayerMng().Player(cameraTargetIdx_)
						.getPosition()
						.z = getPosition().z;
				}
			}

			public override bool checkJump12()
			{
				return checkNextActionToHighNavigate();
			}

			public override byte getStartActionID()
			{
				return (byte)ACTION_ID_ENTERINSIDE;
			}

			public override void setGetOnAction()
			{
			}

			public override int getBattleMapNo()
			{
				return BATTLE_MAP_ONAIR;
			}

			public override int getMonsterPartyGroupNo()
			{
				return MONSTER_GROUP_ONAIR;
			}

			public void setMoveDirection(VecFx32 dir)
			{
				dir_.copy(dir);
			}

			public VecFx32 getMoveDirection()
			{
				return dir_;
			}

			public override int getBGMNo()
			{
				return -1;
			}

			public void setMyselfIdx(int idx)
			{
				myselfIdx_ = idx;
			}

			public int getMyselfIdx()
			{
				return myselfIdx_;
			}

			public int getCameraTargetIdx()
			{
				return cameraTargetIdx_;
			}
		}
	}
}
