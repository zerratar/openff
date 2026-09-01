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
		public class InvinsibleActionDescent : CPlayerVehicleAction
		{
			public override void start()
			{
				VehicleInvinsible vehicleInvinsible = static_cast<VehicleInvinsible>(Player());
				vehicleInvinsible.getColFlag_not_and(2);
				vehicleInvinsible.getColFlag_not_and(4);
				vehicleInvinsible.setAutoPilot(_AutoPilot: true);
				vehicleInvinsible.setOperater(_Operater: false);
				vehicleInvinsible.InputPermission_set(arg0: false);
				vehicleInvinsible.getParamMove().init();
				vehicleInvinsible.getParamTurn().init();
				vehicleInvinsible.MoveSys().setStop(b: true);
				vehicleInvinsible.TurnSys().setStop(b: true);
			}

			public override void update()
			{
				VehicleInvinsible vehicleInvinsible = static_cast<VehicleInvinsible>(Player());
				int motionSpeed = vehicleInvinsible.calcMotSpeedForHighNavi();
				vehicleInvinsible.setMotionSpeed(motionSpeed);
				VecFx32 direction = new VecFx32(0, -1, 0);
				VecFx32 vecFx = new VecFx32(vehicleInvinsible.getPosition());
				VecFx32 vecFx2 = new VecFx32(vecFx.x, VEHICLE_HEIGHT_AIR, vecFx.z);
				vehicleInvinsible.setDirection(direction);
				if (vecFx.y <= vecFx2.y)
				{
					vehicleInvinsible.setNextAct(0);
					vehicleInvinsible.setPosition(vecFx2);
				}
			}

			public override void end()
			{
				VehicleInvinsible vehicleInvinsible = static_cast<VehicleInvinsible>(Player());
				vehicleInvinsible.setAutoPilot(_AutoPilot: false);
				vehicleInvinsible.setOperater(_Operater: true);
				vehicleInvinsible.getParamMove().init();
				vehicleInvinsible.getParamTurn().init();
				vehicleInvinsible.getParamGrv().init();
				vehicleInvinsible.InputPermission_set(arg0: true);
				vehicleInvinsible.MoveSys().setStop(b: true);
				vehicleInvinsible.TurnSys().setStop(b: false);
				vehicleInvinsible.getColFlag_or(2);
				vehicleInvinsible.getColFlag_or(4);
				vehicleInvinsible.setMotionSpeed(4096);
				vehicleInvinsible.stopRiseSE(0);
				chr.CBaseCharacter.setLookIndex(vehicleInvinsible.getMyselfIdx());
			}
		}
	}
}
