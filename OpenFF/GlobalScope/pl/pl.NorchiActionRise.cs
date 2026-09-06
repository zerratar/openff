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
		public class NorchiActionRise : CPlayerVehicleAction
		{
			private byte state_;

			public override void start()
			{
				VehicleNorchi vehicleNorchi = static_cast<VehicleNorchi>(Player());
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
				vehicleNorchi.stopDescSE(0);
				vehicleNorchi.playRiseSE();
				state_ = 0;
			}

			public override void update()
			{
				VehicleNorchi vehicleNorchi = static_cast<VehicleNorchi>(Player());
				VecFx32 vecFx = new VecFx32(vehicleNorchi.getPosition());
				VecFx32 direction = new VecFx32(0, 1, 0);
				vehicleNorchi.setDirection(direction);
				if (vehicleNorchi.isOnAir())
				{
					switch (state_)
					{
					case 0:
					{
						VecFx32 vecFx3 = new VecFx32(vecFx.x, map.MAP_HEIGHT_SEA, vecFx.z);
						if (vecFx.y >= vecFx3.y)
						{
							vehicleNorchi.playDropEffect();
							state_ = 1;
						}
						break;
					}
					case 1:
					{
						VecFx32 vecFx2 = new VecFx32(vecFx.x, VEHICLE_HEIGHT_AIR, vecFx.z);
						if (vecFx.y >= vecFx2.y)
						{
							if (!wld.MapSound.isPlaying())
							{
								vehicleNorchi.playBGM();
							}
							vehicleNorchi.setPosition(vecFx2);
							vehicleNorchi.setNextAct(0);
						}
						break;
					}
					}
				}
				else
				{
					VecFx32 vecFx4 = new VecFx32(vecFx.x, 98304, vecFx.z);
					vehicleNorchi.getLandFormIndex();
					if (vecFx.y >= vecFx4.y)
					{
						CPlayerCharacter boardPlayer = vehicleNorchi.getBoardPlayer();
						boardPlayer.setPosition(vehicleNorchi.getPosition());
						wld.CWorldOutSideData.getInstance().MapData().isColFlag_not_and(2048);
						wld.CWorldOutSideData.getInstance().MapData().MapJumpIndex_set(1);
						wld.CWorldOutSideData.getInstance().MapData().setSpMapType(wld.CMapData.SP_MAP_TYPE.SP_MAP_AIR);
						wld.CWorldOutSideData.getInstance().MapData().setBackupPosJump(b: true);
					}
				}
			}

			public override void end()
			{
				VehicleNorchi vehicleNorchi = static_cast<VehicleNorchi>(Player());
				vehicleNorchi.setAutoPilot(_AutoPilot: false);
				vehicleNorchi.setOperater(_Operater: true);
				vehicleNorchi.getParamMove().init();
				vehicleNorchi.getParamTurn().init();
				vehicleNorchi.getParamGrv().init();
				vehicleNorchi.InputPermission_set(arg0: true);
				vehicleNorchi.MoveSys().setStop(b: true);
				vehicleNorchi.TurnSys().setStop(b: false);
				VecFx32 targetDirection = new VecFx32(0, 0, 0);
				vehicleNorchi.setTargetDirection(targetDirection);
				vehicleNorchi.setMCLCol(b: true);
				vehicleNorchi.getColFlag_or(2);
				vehicleNorchi.getColFlag_or(4);
			}
		}
	}
}
