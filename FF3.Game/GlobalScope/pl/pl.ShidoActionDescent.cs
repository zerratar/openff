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
		public class ShidoActionDescent : CPlayerVehicleAction
		{
			private bool canLand_;

			public override void start()
			{
				VehicleShido vehicleShido = static_cast<VehicleShido>(Player());
				vehicleShido.getColFlag_not_and(2);
				vehicleShido.getColFlag_not_and(4);
				vehicleShido.setAutoPilot(_AutoPilot: true);
				vehicleShido.setOperater(_Operater: false);
				vehicleShido.InputPermission_set(arg0: false);
				vehicleShido.getParamMove().init();
				vehicleShido.getParamTurn().init();
				vehicleShido.MoveSys().setStop(b: true);
				vehicleShido.TurnSys().setStop(b: true);
				vehicleShido.stopNaviSE(0);
				vehicleShido.stopRiseSE(0);
				vehicleShido.playDescSE();
				short num = (short)(vehicleShido.getLandFormIndex() - 1);
				canLand_ = num == 0 || num == 2 || num == 8;
				if (canLand_)
				{
					vehicleShido.stopBGM();
				}
			}

			public override void update()
			{
				VehicleShido vehicleShido = static_cast<VehicleShido>(Player());
				VecFx32 direction = new VecFx32(0, -1, 0);
				VecFx32 vecFx = new VecFx32(vehicleShido.getPosition());
				VecFx32 vecFx2 = new VecFx32(vecFx.x, 8192, vecFx.z);
				vehicleShido.setDirection(direction);
				if (vecFx.y <= vecFx2.y)
				{
					if (canLand_)
					{
						vehicleShido.stopBGM();
						vehicleShido.setPosition(vecFx2);
						vehicleShido.setNextAct(5);
					}
					else
					{
						vehicleShido.setMCLCol(b: false);
						vehicleShido.setNextAct(2);
					}
				}
			}

			public override void end()
			{
				VehicleShido vehicleShido = static_cast<VehicleShido>(Player());
				vehicleShido.getColFlag_or(2);
				vehicleShido.getColFlag_or(4);
				vehicleShido.InputPermission_set(arg0: true);
				vehicleShido.MoveSys().setStop(b: true);
				vehicleShido.TurnSys().setStop(b: false);
				VecFx32 targetDirection = new VecFx32(0, 0, 0);
				vehicleShido.setTargetDirection(targetDirection);
			}
		}
	}
}
