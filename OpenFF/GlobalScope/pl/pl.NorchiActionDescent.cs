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
		public class NorchiActionDescent : CPlayerVehicleAction
		{
			public enum DESCENT_STATE
			{
				STATE_DESCENT,
				STATE_GOTO_DEEPSEA1,
				STATE_GOTO_DEEPSEA2,
				STATE_GOTO_BOTTOM
			}

			public const DESCENT_STATE STATE_DESCENT = DESCENT_STATE.STATE_DESCENT;

			public const DESCENT_STATE STATE_GOTO_DEEPSEA1 = DESCENT_STATE.STATE_GOTO_DEEPSEA1;

			public const DESCENT_STATE STATE_GOTO_DEEPSEA2 = DESCENT_STATE.STATE_GOTO_DEEPSEA2;

			public const DESCENT_STATE STATE_GOTO_BOTTOM = DESCENT_STATE.STATE_GOTO_BOTTOM;

			private DESCENT_STATE _state;

			public override void start()
			{
				VehicleNorchi vehicleNorchi = static_cast<VehicleNorchi>(Player());
				if (vehicleNorchi.isOnAir())
				{
					_state = DESCENT_STATE.STATE_DESCENT;
				}
				else
				{
					_state = DESCENT_STATE.STATE_GOTO_BOTTOM;
				}
				vehicleNorchi.setMotionSpeed(4096);
				vehicleNorchi.getColFlag_not_and(2);
				vehicleNorchi.getColFlag_not_and(4);
				vehicleNorchi.setAutoPilot(_AutoPilot: true);
				vehicleNorchi.setOperater(_Operater: false);
				vehicleNorchi.InputPermission_set(arg0: false);
				vehicleNorchi.getParamMove().init();
				vehicleNorchi.getParamTurn().init();
				vehicleNorchi.MoveSys().setStop(b: true);
				vehicleNorchi.TurnSys().setStop(b: true);
				vehicleNorchi.stopNaviSE(0);
				vehicleNorchi.stopRiseSE(0);
				vehicleNorchi.playDescSE();
			}

			public override void update()
			{
				VehicleNorchi vehicleNorchi = static_cast<VehicleNorchi>(Player());
				VecFx32 vecFx = new VecFx32(vehicleNorchi.getPosition());
				VecFx32 direction = new VecFx32(0, -1, 0);
				vehicleNorchi.setDirection(direction);
				switch (_state)
				{
				case DESCENT_STATE.STATE_DESCENT:
				{
					VecFx32 vecFx5 = new VecFx32(vecFx.x, VEHICLE_HEIGHT_GROUND, vecFx.z);
					if (vecFx.y > vecFx5.y)
					{
						break;
					}
					switch ((short)(vehicleNorchi.getLandFormIndex() - 1))
					{
					case 10:
						if (PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_NORCHI_CTM == vehicleNorchi.getVehicleType())
						{
							_state = DESCENT_STATE.STATE_GOTO_DEEPSEA1;
						}
						else
						{
							vehicleNorchi.setNextAct(2);
						}
						break;
					case 0:
					case 2:
					case 8:
						vehicleNorchi.stopBGM();
						vehicleNorchi.setPosition(vecFx5);
						vehicleNorchi.setNextAct(5);
						break;
					default:
						vehicleNorchi.setNextAct(2);
						break;
					}
					break;
				}
				case DESCENT_STATE.STATE_GOTO_DEEPSEA1:
				{
					VecFx32 vecFx3 = new VecFx32(vecFx.x, map.MAP_HEIGHT_SEA, vecFx.z);
					if (vecFx.y <= vecFx3.y)
					{
						int id = eff.CEffectMng.instance().create(EFFECT_CATEGORY_VEHICLE, EFFECT_MEMBER_SPRAY);
						eff.CEffectMng.instance().setPosition(id, vecFx3);
						_state = DESCENT_STATE.STATE_GOTO_DEEPSEA2;
					}
					break;
				}
				case DESCENT_STATE.STATE_GOTO_DEEPSEA2:
				{
					VecFx32 vecFx4 = new VecFx32(vecFx.x, -122880, vecFx.z);
					if (vecFx.y <= vecFx4.y)
					{
						CPlayerCharacter boardPlayer = vehicleNorchi.getBoardPlayer();
						boardPlayer.setPosition(vehicleNorchi.getPosition());
						vehicleNorchi.setOnSea();
						wld.CWorldOutSideData.getInstance().MapData().isColFlag_not_and(2048);
						wld.CWorldOutSideData.getInstance().MapData().MapJumpIndex_set(1);
						wld.CWorldOutSideData.getInstance().MapData().setSpMapType(wld.CMapData.SP_MAP_TYPE.SP_MAP_DEEPSEA);
						wld.CWorldOutSideData.getInstance().MapData().setBackupPosJump(b: true);
					}
					break;
				}
				case DESCENT_STATE.STATE_GOTO_BOTTOM:
				{
					VecFx32 vecFx2 = new VecFx32(vecFx.x, 8192, vecFx.z);
					if (vecFx.y <= vecFx2.y)
					{
						vehicleNorchi.setOnSea();
						vehicleNorchi.setPosition(vecFx2);
						vehicleNorchi.setNextAct(0);
						vehicleNorchi.setOperater(_Operater: true);
						vehicleNorchi.setAutoPilot(_AutoPilot: false);
					}
					break;
				}
				}
			}

			public override void end()
			{
				VehicleNorchi vehicleNorchi = static_cast<VehicleNorchi>(Player());
				vehicleNorchi.setMCLCol(b: true);
				vehicleNorchi.getColFlag_or(2);
				vehicleNorchi.getColFlag_or(4);
				vehicleNorchi.InputPermission_set(arg0: true);
				vehicleNorchi.getParamMove().init();
				vehicleNorchi.getParamTurn().init();
				vehicleNorchi.getParamGrv().init();
				vehicleNorchi.MoveSys().setStop(b: true);
				vehicleNorchi.TurnSys().setStop(b: false);
				VecFx32 targetDirection = new VecFx32(0, 0, 0);
				vehicleNorchi.setTargetDirection(targetDirection);
				VecFx32 direction = new VecFx32(1, 0, 1);
				vehicleNorchi.setDirection(direction);
			}
		}
	}
}
