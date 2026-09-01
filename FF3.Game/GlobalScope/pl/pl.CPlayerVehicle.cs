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
		public class CPlayerVehicle : CPlayerCharacter
		{
			private class MapMarkerAccepterVehicle : MapMarkerAccepter
			{
				private CPlayerVehicle m_Owner;

				public MapMarkerAccepterVehicle(CPlayerVehicle owner)
				{
					m_Owner = owner;
				}

				public override VecFx32 acceptPos()
				{
					return m_Owner.getPosition();
				}

				public override int acceptDir()
				{
					int[] array = new int[9] { 7, 7, 5, 1, 3, 6, 2, 0, 4 };
					int[][] array2 = new int[9][]
					{
						new int[2] { 61440, 0 },
						new int[2] { 0, 4095 },
						new int[2] { 4096, 12287 },
						new int[2] { 12288, 20479 },
						new int[2] { 20480, 28671 },
						new int[2] { 28672, 36863 },
						new int[2] { 36864, 45055 },
						new int[2] { 45056, 53247 },
						new int[2] { 53248, 61439 }
					};
					VecFx32 rotation = m_Owner.getRotation();
					int result = -1;
					for (int i = 0; 9 > i; i++)
					{
						if (array2[i][0] <= rotation.y && array2[i][1] >= rotation.y)
						{
							dir_ = array[i];
							result = array[i];
							break;
						}
					}
					return result;
				}

				public override bool acceptVisibility()
				{
					return true;
				}

				public override bool acceptAnimationState()
				{
					if (m_Owner.getBoardPlayer() == null)
					{
						return false;
					}
					return true;
				}
			}

			public enum ACTION_ID
			{
				ACTION_ID_ERR = -1,
				ACTION_ID_WAIT,
				ACTION_ID_NAVIGATE,
				ACTION_ID_RISE,
				ACTION_ID_DESCENT,
				ACTION_ID_APPEAR,
				ACTION_ID_DISAPPEAR,
				ACTION_ID_MAX
			}

			private delegate void _action();

			public enum TP_FLAG
			{
				TP_FLAG_TOUCH = 1,
				TP_FLAG_MYSELF = 2,
				TP_FLAG_SOMEWHERE = 4
			}

			public const ACTION_ID ACTION_ID_ERR = ACTION_ID.ACTION_ID_ERR;

			public const ACTION_ID ACTION_ID_WAIT = ACTION_ID.ACTION_ID_WAIT;

			public const ACTION_ID ACTION_ID_NAVIGATE = ACTION_ID.ACTION_ID_NAVIGATE;

			public const ACTION_ID ACTION_ID_RISE = ACTION_ID.ACTION_ID_RISE;

			public const ACTION_ID ACTION_ID_DESCENT = ACTION_ID.ACTION_ID_DESCENT;

			public const ACTION_ID ACTION_ID_APPEAR = ACTION_ID.ACTION_ID_APPEAR;

			public const ACTION_ID ACTION_ID_DISAPPEAR = ACTION_ID.ACTION_ID_DISAPPEAR;

			public const ACTION_ID ACTION_ID_MAX = ACTION_ID.ACTION_ID_MAX;

			public const TP_FLAG TP_FLAG_TOUCH = TP_FLAG.TP_FLAG_TOUCH;

			public const TP_FLAG TP_FLAG_MYSELF = TP_FLAG.TP_FLAG_MYSELF;

			public const TP_FLAG TP_FLAG_SOMEWHERE = TP_FLAG.TP_FLAG_SOMEWHERE;

			private _action[] action = new _action[6];

			private act.CBaseAction[] m_apAction = new act.CBaseAction[6];

			private PLAYER_VEHICLE_TYPE m_VehicleType;

			private CPlayerCharacter m_BoardPlayer;

			private bool m_canBoard;

			private bool reservedToPlayBGM_;

			private wld.CMenuButton pMenuIcon_;

			private wld.CMenuButton pCameraIcon_;

			private wld.CMenuButton pTalkIcon_;

			protected act.CBaseAction[] _actionList = new act.CBaseAction[6];

			protected byte _tpFlag;

			protected byte _bgmStartFlag;

			protected MatrixSound.MtxSEHandle hRiseSE_;

			protected MatrixSound.MtxSEHandle hDescSE_;

			protected MatrixSound.MtxSEHandle hNaviSE_;

			protected bool enableCalcCameraHeight_;

			protected bool enablePlayNaviSE_;

			protected bool pullBoardPlayer_;

			public MapMarkerAccepter composit2;

			public void registerAction(ACTION_ID _id, act.CBaseAction pAction)
			{
				if (ACTION_ID.ACTION_ID_ERR < _id && _id < ACTION_ID.ACTION_ID_MAX)
				{
					m_apAction[(int)_id] = pAction;
				}
			}

			public virtual byte getStartActionID()
			{
				return 2;
			}

			public void setCanBoard(bool can)
			{
				m_canBoard = can;
			}

			public bool canBoard()
			{
				return m_canBoard;
			}

			public override void initialize()
			{
				for (byte b = 0; b < 6; b++)
				{
					_actionList[b] = null;
				}
				base.initialize();
				CharaKind_set(chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE);
				CharaCheckType_set(chr.CHARACTER_CHECK_TYPE.CHARACTER_CHECK_TYPE_CHECK);
				m_VehicleType = PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ERR;
				m_BoardPlayer = null;
				m_canBoard = true;
				_tpFlag = 0;
				_bgmStartFlag = 0;
				pMenuIcon_ = null;
				pCameraIcon_ = null;
				pTalkIcon_ = null;
				hRiseSE_ = null;
				hDescSE_ = null;
				hNaviSE_ = null;
			}

			public override void execute()
			{
				checkTouchPanel();
				base.execute();
				CPlayerCharacter boardPlayer = getBoardPlayer();
				if (boardPlayer != null)
				{
					if (pullBoardPlayer_)
					{
						boardPlayer.setPosition(getPosition());
					}
					updateCameraHeight();
				}
				else
				{
					setMCLCol(b: false);
				}
				updateShadowScale();
				if (getNextAct() == 2 || getNextAct() == 3 || getNextAct() == 4 || getNextAct() == 5 || getNowAct() == 2 || getNowAct() == 3 || getNowAct() == 4 || getNowAct() == 5 || !isAutoPilot())
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
				stopRiseSE(0);
				stopDescSE(0);
				stopNaviSE(0);
				for (byte b = 0; b < 6; b++)
				{
					if (_actionList[b] != null)
					{
						_actionList[b].destruct();
						ds.CHeap.free_app(_actionList[b]);
					}
				}
				base.terminate();
			}

			public override void into()
			{
				base.into();
				getColFlag_not_and(32);
				getColFlag_or(64);
				getColFlag_or(128);
				getColFlag_or(256);
				getColFlag_or(512);
				getColFlag_not_and(8);
				setShadowType(4u);
				setShadowScale(g_shadowScaleOrg);
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
				if ((getColType() & 2) != 0)
				{
					setTarget(_Target);
				}
				if ((getColType() & 1) == 0 || !isOperater() || _Target.CharaKind() != chr.CHARACTER_KIND.CHARACTER_KIND_MAP_OBJECT)
				{
					return;
				}
				VecFx32 pl_reuse_v = pl_reuse_v0;
				ds.pri.DSSphere pl_reuse_sphere = pl.pl_reuse_sphere;
				ds.pri.DSAABB pl_reuse_aabb = pl.pl_reuse_aabb;
				pl_reuse_sphere.set(getPosition(), getColRadius());
				pl_reuse_aabb.c.copy(_Target.getPosition());
				pl_reuse_aabb.r.copy(_Target.getColAabbRadius());
				if ((pl_reuse_aabb.c.x - pl_reuse_aabb.r.x < getPosition().x && getPosition().x < pl_reuse_aabb.c.x + pl_reuse_aabb.r.x) || (pl_reuse_aabb.c.z - pl_reuse_aabb.r.z < getPosition().z && getPosition().z < pl_reuse_aabb.c.z + pl_reuse_aabb.r.z))
				{
					pl_reuse_v = ds.pri.PrimitiveTest.closestPtPointAABB(pl_reuse_sphere.c, pl_reuse_aabb);
					if (pl_reuse_v.x <= pl_reuse_aabb.c.x - pl_reuse_aabb.r.x)
					{
						pl_reuse_v.x -= getColRadius();
					}
					else if (pl_reuse_aabb.c.x + pl_reuse_aabb.r.x <= pl_reuse_v.x)
					{
						pl_reuse_v.x += getColRadius();
					}
					if (pl_reuse_v.z <= pl_reuse_aabb.c.z - pl_reuse_aabb.r.z)
					{
						pl_reuse_v.z -= getColRadius();
					}
					else if (pl_reuse_aabb.c.z + pl_reuse_aabb.r.z <= pl_reuse_v.z)
					{
						pl_reuse_v.z += getColRadius();
					}
					setPosition(pl_reuse_v);
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
				VecFx32 pl_reuse_v2 = pl_reuse_v1;
				VEC_Subtract(getPosition(), pl_reuse_sphere2.c, pl_reuse_v2);
				int num = VEC_Mag(pl_reuse_v2);
				VEC_Normalize(pl_reuse_v2, pl_reuse_v2);
				VEC_MultAdd(getColRadius() + pl_reuse_sphere2.r - num, pl_reuse_v2, getPosition(), pl_reuse_v);
				setPosition(pl_reuse_v);
			}

			public void setBoardSetting(CPlayerCharacter pBoardChara)
			{
				setNextAct(getStartActionID());
				setBoardPlayer(pBoardChara);
				setBoard();
				g_cameraHeightOrg = 0;
				if (CCastCommandTransit.getInstance().cast_FieldCamera() != null)
				{
					g_cameraHeightOrg = CCastCommandTransit.getInstance().cast_FieldCamera().PosOffset()
						.y;
				}
				byte b = 0;
				b = 24;
				while (b < FIELD_CHARACTER_NUM && getCharacterId() != CCastCommandTransit.getInstance().cast_PlayerMng().Player(b)
					.getCharacterId())
				{
					b++;
				}
				chr.CBaseCharacter.setLookIndex(b);
				wld.CWorldOutSideData.getInstance().PlayerData().setPlayCharacterIndex(b);
				CPlayerHuman cPlayerHuman = (CPlayerHuman)pBoardChara;
				setMenuIcon(cPlayerHuman.getMenuIcon());
				setCameraIcon(cPlayerHuman.getCameraIcon());
				setTalkIcon(cPlayerHuman.getTalkIcon());
				cPlayerHuman.setMenuIcon(null);
				cPlayerHuman.setCameraIcon(null);
				cPlayerHuman.setTalkIcon(null);
				cPlayerHuman.setOnVehicle(b: true);
				if (m_VehicleType != PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CHOKOBO && PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CANOE != m_VehicleType)
				{
					cPlayerHuman.setVisibleMarker(b: false);
				}
			}

			public virtual void setGetOnAction()
			{
				setNextAct(4);
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
				for (int i = 0; i < 6; i++)
				{
					if (m_ActionMng.getAction() == getAction(i))
					{
						return (int)static_cast<ACTION_ID>(i);
					}
				}
				return -1;
			}

			public bool checkNextActionToWait()
			{
				if (touchMoveErr_)
				{
					return true;
				}
				if ((dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 0xF) == 0 && (_tpFlag & 1) == 0)
				{
					return true;
				}
				return false;
			}

			public bool checkNextActionToNavigate()
			{
				if (touchMoveErr_)
				{
					return false;
				}
				if (dv.CDeviceManager.getInstance().Tp().TouchPanel_2d(out var x, out var y))
				{
					if (pMenuIcon_ != null && pMenuIcon_.isButtonTouch(x, y))
					{
						return false;
					}
					if (pCameraIcon_ != null)
					{
						if (pCameraIcon_.isEdgeAndRepeatTouch())
						{
							return false;
						}
						if (ds.g_TouchPanel.isEdge() && pCameraIcon_.isButtonTouch(x, y))
						{
							return false;
						}
					}
				}
				if ((dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 0xF) != 0 || (_tpFlag & 4) != 0)
				{
					return true;
				}
				return false;
			}

			public virtual bool checkNextActionToRise()
			{
				if ((dv.CDeviceManager.getInstance().Pad().edge_trs(0) & 0x80) != 0 || (_tpFlag & 2) != 0)
				{
					return true;
				}
				return true;
			}

			public virtual bool checkNextActionToDescent()
			{
				if (!dv.CDeviceManager.getInstance().Pad().activity())
				{
					return false;
				}
				if ((dv.CDeviceManager.getInstance().Pad().edge_trs(0) & 0x20) != 0 || (_tpFlag & 2) != 0)
				{
					return true;
				}
				return false;
			}

			public virtual void updateCameraHeight()
			{
				if (CCastCommandTransit.getInstance().cast_FieldCamera() != null && PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CANOE != getVehicleType() && enableCalcCameraHeight_)
				{
					int y = getPosition().y;
					int num = y;
					if (num < VEHICLE_HEIGHT_GROUND)
					{
						num = VEHICLE_HEIGHT_GROUND;
					}
					else if (num > VEHICLE_HEIGHT_AIR)
					{
						num = VEHICLE_HEIGHT_AIR;
					}
					int num2 = 0;
					if (getPosition().y < 0)
					{
						num2 = -1 * y;
					}
					CCastCommandTransit.getInstance().cast_FieldCamera().PosOffset()
						.y = FX_Mul(CAMERAHEIGHT_PARAM, num) + g_cameraHeightOrg + num2;
				}
			}

			public void updateShadowScale()
			{
				int num = getPosition().y;
				if (num < VEHICLE_HEIGHT_GROUND)
				{
					num = VEHICLE_HEIGHT_GROUND;
				}
				else if (num > VEHICLE_HEIGHT_AIR)
				{
					num = VEHICLE_HEIGHT_AIR;
				}
				VecFx32 pl_reuse_v = pl_reuse_v0;
				pl_reuse_v.x = FX_Mul(SHADOWSCALE_PARAM, num) + g_shadowScaleOrg.x;
				pl_reuse_v.y = g_shadowScaleOrg.y;
				pl_reuse_v.z = FX_Mul(SHADOWSCALE_PARAM, num) + g_shadowScaleOrg.z;
				setShadowScale(pl_reuse_v);
			}

			public virtual void dropPlayer()
			{
				setMotionSpeed(0);
				CPlayerHuman cPlayerHuman = static_cast<CPlayerHuman>(getBoardPlayer());
				setBoardPlayer(null);
				setTarget(null);
				cPlayerHuman.setNextAct(8);
				cPlayerHuman.getParamObj_set(getParamObj());
				cPlayerHuman.getPreParamObj_set(getParamObj());
				cPlayerHuman.getParamObj().m_Pos.y = 0;
				cPlayerHuman.setAutoPilot(_AutoPilot: true);
				cPlayerHuman.InputPermission_set(arg0: false);
				cPlayerHuman.setVisibleMarker(b: true);
				VecFx32 directionForRotY = getDirectionForRotY();
				VEC_MultAdd(-2048, directionForRotY, cPlayerHuman.getPosition(), cPlayerHuman.getPosition());
				cPlayerHuman.setMenuIcon(getMenuIcon());
				cPlayerHuman.setCameraIcon(getCameraIcon());
				cPlayerHuman.setTalkIcon(getTalkIcon());
				setMenuIcon(null);
				setCameraIcon(null);
				setTalkIcon(null);
				cPlayerHuman.setOnVehicle(b: false);
				CCastCommandTransit.getInstance().cast_FieldCamera().PosOffset()
					.y = g_cameraHeightOrg;
				chr.CBaseCharacter.setLookIndex(0);
				wld.CWorldOutSideData.getInstance().PlayerData().setPlayCharacterIndex(0);
				setAutoPilot(_AutoPilot: true);
				setOperater(_Operater: false);
			}

			public void checkTouchPanel()
			{
				_tpFlag = 0;
				VecFx32 pl_reuse_v = pl_reuse_v0;
				if (!dv.CDeviceManager.getInstance().Tp().TouchPanel_3d(pl_reuse_v))
				{
					return;
				}
				_tpFlag |= 1;
				if (ds.g_TouchPanel.isEdge() && (getColType() & 4) != 0 && this == getTarget())
				{
					_tpFlag |= 2;
					return;
				}
				pl_reuse_v.y = getPosition().y;
				int num = dv.CDeviceManager.getInstance().Tp().culDistance(getPosition(), pl_reuse_v);
				if (num > 2)
				{
					_tpFlag |= 4;
				}
			}

			public void playBGM()
			{
				if (wld.MapSound.canChangeBGM())
				{
					wld.MapSound.stopBGM(0);
					wld.MapSound.playBGM(getBGMNo(), 192, 0);
				}
			}

			public void returnToFieldBGM()
			{
				if (wld.MapSound.canChangeBGM())
				{
					int bgmNo = map.CMapParameterManager.Instance().MapSoundParameter(0).BGMIndex();
					wld.MapSound.playBGM(bgmNo, 192, 0);
				}
			}

			public void stopBGM()
			{
				if (wld.MapSound.canChangeBGM())
				{
					wld.MapSound.stopBGM(15);
				}
			}

			public virtual void playRiseSE()
			{
				if (!isPlayingRiseSE())
				{
					hRiseSE_ = MatrixSound.MtxSENDS_Play(1, 7, 192, 127);
				}
			}

			public virtual void playDescSE()
			{
				if (!isPlayingDescSE())
				{
					hDescSE_ = MatrixSound.MtxSENDS_Play(1, 8, 192, 127);
				}
			}

			public virtual void playNaviSE()
			{
				if (canPlayNaviSE())
				{
					hNaviSE_ = MatrixSound.MtxSENDS_Play(0, 9, 192, 127);
				}
			}

			public void stopRiseSE(int frame)
			{
				if (MatrixSound.MtxSENDS_isPlaying(hRiseSE_))
				{
					MatrixSound.MtxSENDS_Stop(hRiseSE_, frame);
					hRiseSE_ = null;
				}
			}

			public void stopDescSE(int frame)
			{
				if (MatrixSound.MtxSENDS_isPlaying(hDescSE_))
				{
					MatrixSound.MtxSENDS_Stop(hDescSE_, frame);
					hDescSE_ = null;
				}
			}

			public void stopNaviSE(int frame)
			{
				if (MatrixSound.MtxSENDS_isPlaying(hNaviSE_))
				{
					MatrixSound.MtxSENDS_Stop(hNaviSE_, frame);
					hNaviSE_ = null;
				}
			}

			public bool canPlayNaviSE()
			{
				if (!isPlayingNaviSE() && getBoardPlayer() != null)
				{
					return enablePlayNaviSE_;
				}
				return false;
			}

			public override bool calculateWallCollision(dgs.CRestrictor ror, mcl.CollisionResult result, int attr, int radius, int revise, VecFx32 current_position, VecFx32 old_position)
			{
				mcl.CollisionResult pl_reuse_result = pl.pl_reuse_result;
				VecFx32 pl_reuse_v = pl_reuse_v0;
				VecFx32 pl_reuse_v2 = pl_reuse_v1;
				VecFx32 pl_reuse_v3 = pl.pl_reuse_v2;
				pl_reuse_v.copy(old_position);
				pl_reuse_v2.copy(current_position);
				int num = 0;
				bool result2 = false;
				int num2 = 216;
				VEC_Subtract(pl_reuse_v2, pl_reuse_v, pl_reuse_v3);
				if (pl_reuse_v3.x == 0 && pl_reuse_v3.y == 0 && pl_reuse_v3.z == 0)
				{
					return false;
				}
				if (strncmp(sceneMng.getStage(), "f03", 3) == 0)
				{
					int[] array = new int[12]
					{
						-4044219, -2048027, 4334961, -414455, 3882873, 4808976, -3934088, -393403, 1043474, -2358025,
						3225952, -4345328
					};
					int num3 = 16384;
					VecFx32 pl_reuse_v4 = pl.pl_reuse_v3;
					MtxFx43 mtxFx = new MtxFx43();
					stageMng.getWldMtx(mtxFx);
					MTX_MultVec43(pl_reuse_v, mtxFx, pl_reuse_v4);
					for (int i = 0; i < array.Length; i += 2)
					{
						int num4 = pl_reuse_v4.x - array[i];
						int num5 = pl_reuse_v4.z - array[i + 1];
						if (num4 > -num3 && num4 < num3 && num5 > -num3 && num5 < num3)
						{
							mcl.CObject.setCheckTwice(check: true);
						}
					}
				}
				VecFx32 pl_reuse_v5 = pl.pl_reuse_v3;
				num = VEC_Mag(pl_reuse_v3);
				VEC_Normalize(pl_reuse_v3, pl_reuse_v5);
				if (ror.rorEvaluateArrow(pl_reuse_v, pl_reuse_v5, num, attr, pl_reuse_result))
				{
					result2 = true;
					int num6 = -VEC_DotProduct(pl_reuse_result.normal, pl_reuse_v3);
					VecFx32 pl_reuse_v6 = pl.pl_reuse_v4;
					int num7 = 0;
					VEC_Subtract(pl_reuse_result.v0, pl_reuse_v, pl_reuse_v6);
					num7 = -VEC_DotProduct(pl_reuse_result.normal, pl_reuse_v6);
					int num8 = num6 - num7;
					VEC_MultAdd(num8 + num2, pl_reuse_result.normal, pl_reuse_v2, pl_reuse_v2);
					VEC_MultAdd(num2, pl_reuse_result.normal, pl_reuse_result.pos, pl_reuse_v);
					VEC_Subtract(pl_reuse_v2, pl_reuse_v, pl_reuse_v3);
					num = VEC_Mag(pl_reuse_v3);
					VEC_Normalize(pl_reuse_v3, pl_reuse_v5);
					mcl.CollisionResult pl_reuse_result2 = pl.pl_reuse_result2;
					if (ror.rorEvaluateArrow(pl_reuse_v, pl_reuse_v5, num, attr, pl_reuse_result2))
					{
						VEC_MultAdd(num2, pl_reuse_result2.normal, pl_reuse_result2.pos, pl_reuse_v2);
					}
				}
				mcl.CObject.setCheckTwice(check: false);
				result.copy(pl_reuse_result);
				current_position.copy(pl_reuse_v2);
				return result2;
			}

			protected override int getJumpCollisionRadius()
			{
				return 18432;
			}

			public void actionVehicleWait()
			{
				setMass(0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).MoveSkg() * (60 / chr.CCharacterEureka.m_CharaFps)), (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NMveMax() * (60 / chr.CCharacterEureka.m_CharaFps)), (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnAcc() * (60 / chr.CCharacterEureka.m_CharaFps)), (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).TurnSkg() * (60 / chr.CCharacterEureka.m_CharaFps)), (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnMax() * (60 / chr.CCharacterEureka.m_CharaFps)));
			}

			public virtual void actionVehicleNavigate()
			{
				setMass((int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NMveAcc() * (60 / chr.CCharacterEureka.m_CharaFps)), 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NMveMax() * (60 / chr.CCharacterEureka.m_CharaFps)), (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnAcc() * (60 / chr.CCharacterEureka.m_CharaFps)), 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnMax() * (60 / chr.CCharacterEureka.m_CharaFps)));
			}

			public void actionVehicleRise()
			{
				setMass(3000, 0, 3000, 0, 0, 0);
			}

			public void actionVehicleDescent()
			{
				setMass(3000, 0, 3000, 0, 0, 0);
			}

			public void actionVehicleAppear()
			{
				setMass((int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NMveAcc() * (60 / chr.CCharacterEureka.m_CharaFps)), 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NMveMax() * (60 / chr.CCharacterEureka.m_CharaFps)), (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnAcc() * (60 / chr.CCharacterEureka.m_CharaFps)), 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnMax() * (60 / chr.CCharacterEureka.m_CharaFps)));
			}

			public void actionVehicleDisAppear()
			{
				setMass(2500, 0, 2500, 0, 0, 0);
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
				return pTalkIcon_;
			}

			public override bool canOpenMenu()
			{
				if (isAutoPilot())
				{
					return false;
				}
				bool flag = getNowAct() == 0 || getNowAct() == 1;
				bool result = getNextAct() == -1 || getNextAct() == 0 || getNextAct() == 1;
				if (flag)
				{
					return result;
				}
				return false;
			}

			public bool isPlayingRiseSE()
			{
				return MatrixSound.MtxSENDS_isPlaying(hRiseSE_);
			}

			public bool isPlayingDescSE()
			{
				return MatrixSound.MtxSENDS_isPlaying(hDescSE_);
			}

			public bool isPlayingNaviSE()
			{
				return MatrixSound.MtxSENDS_isPlaying(hNaviSE_);
			}

			public CPlayerVehicle()
			{
				composit2 = new MapMarkerAccepterVehicle(this);
				action[0] = actionVehicleWait;
				action[1] = actionVehicleNavigate;
				action[2] = actionVehicleRise;
				action[3] = actionVehicleDescent;
				action[4] = actionVehicleAppear;
				action[5] = actionVehicleDisAppear;
				enableCalcCameraHeight_ = true;
				enablePlayNaviSE_ = true;
				pullBoardPlayer_ = true;
			}

			public new void destruct()
			{
			}

			public PLAYER_VEHICLE_TYPE getVehicleType()
			{
				return m_VehicleType;
			}

			public PLAYER_VEHICLE_TYPE VehicleType()
			{
				return m_VehicleType;
			}

			public void VehicleType_set(PLAYER_VEHICLE_TYPE arg0)
			{
				m_VehicleType = arg0;
			}

			public void setBoardPlayer(CPlayerCharacter _BoardPlayer)
			{
				m_BoardPlayer = _BoardPlayer;
			}

			public CPlayerCharacter getBoardPlayer()
			{
				return m_BoardPlayer;
			}

			public virtual void setBoard()
			{
				_bgmStartFlag = 1;
			}

			public virtual void setConditionOfAir()
			{
			}

			public virtual void setConditionOfDeepSea()
			{
			}

			public virtual bool isOnAir()
			{
				return false;
			}

			public virtual int getBGMNo()
			{
				return 9;
			}

			public override bool canEncount()
			{
				return false;
			}

			public virtual int getWaitMotionIndex()
			{
				return VEHICLE_MOTIONNO_WAIT;
			}

			public void setReserveToPlayBGM(bool reserve)
			{
				reservedToPlayBGM_ = reserve;
			}

			public bool hadReservedToPlayBGM()
			{
				return reservedToPlayBGM_;
			}

			public void setEnableCalcCamHeight(bool b)
			{
				enableCalcCameraHeight_ = b;
			}

			public void setEnablePlayNaviSE(bool b)
			{
				enablePlayNaviSE_ = b;
			}

			public void setPullBoardPlayer(bool b)
			{
				pullBoardPlayer_ = b;
			}

			protected byte getTPFlag()
			{
				return _tpFlag;
			}
		}
	}
}
