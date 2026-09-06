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
	public static partial class pl
	{
		public class ShidoActionRise : CPlayerVehicleAction
		{
			public override void start()
			{
				VehicleShido vehicleShido = static_cast<VehicleShido>(Player());
				vehicleShido.setMotionSpeed(4096);
				vehicleShido.getColFlag_not_and(2);
				vehicleShido.getColFlag_not_and(4);
				vehicleShido.setGrv(_GrvFlag: false);
				vehicleShido.setAutoPilot(_AutoPilot: true);
				vehicleShido.setOperater(_Operater: false);
				vehicleShido.InputPermission_set(arg0: false);
				vehicleShido.getParamMove().init();
				vehicleShido.getParamTurn().init();
				vehicleShido.MoveSys().setStop(b: true);
				vehicleShido.TurnSys().setStop(b: true);
				vehicleShido.stopNaviSE(0);
				vehicleShido.stopDescSE(0);
				vehicleShido.playRiseSE();
			}

			public override void update()
			{
				VehicleShido vehicleShido = static_cast<VehicleShido>(Player());
				VecFx32 direction = new VecFx32(0, 1, 0);
				VecFx32 vecFx = new VecFx32(vehicleShido.getPosition());
				VecFx32 vecFx2 = new VecFx32(vecFx.x, 49152, vecFx.z);
				vehicleShido.setDirection(direction);
				if (vecFx.y >= vecFx2.y)
				{
					if (!wld.MapSound.isPlaying())
					{
						vehicleShido.playBGM();
					}
					vehicleShido.setPosition(vecFx2);
					vehicleShido.setNextAct(0);
				}
			}

			public override void end()
			{
				VehicleShido vehicleShido = static_cast<VehicleShido>(Player());
				vehicleShido.setAutoPilot(_AutoPilot: false);
				vehicleShido.setOperater(_Operater: true);
				vehicleShido.getParamMove().init();
				vehicleShido.getParamTurn().init();
				vehicleShido.getParamGrv().init();
				vehicleShido.InputPermission_set(arg0: true);
				vehicleShido.MoveSys().setStop(b: true);
				vehicleShido.TurnSys().setStop(b: false);
				vehicleShido.setMCLCol(b: true);
				vehicleShido.getColFlag_set(0);
				vehicleShido.getColFlag_or(2);
				vehicleShido.getColFlag_or(4);
				vehicleShido.getColFlag_or(16);
				vehicleShido.getColFlag_or(128);
				vehicleShido.getColFlag_or(256);
				vehicleShido.getColFlag_or(512);
				vehicleShido.stopDescSE(0);
			}
		}
	}
}
