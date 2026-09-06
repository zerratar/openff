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
		public class InvinsibleActionRise : CPlayerVehicleAction
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
				VecFx32 vecFx = new VecFx32(vehicleInvinsible.getDirectionForRotY());
				VecFx32 vecFx2 = new VecFx32(vehicleInvinsible.getPosition());
				VecFx32 vecFx3 = new VecFx32(0, 0, 0);
				VEC_MultAdd(4096, vecFx, vecFx2, vecFx3);
				vehicleInvinsible.MoveSys().setTargetPoint(0, vecFx2);
				vehicleInvinsible.MoveSys().setTargetPoint(1, vecFx3);
				vecFx.x /= 682;
				vecFx.y /= 682;
				vecFx.z /= 682;
				vehicleInvinsible.setDirection(vecFx);
				vehicleInvinsible.setTargetDirection(vecFx);
				vehicleInvinsible.setMoveDirection(vecFx);
				vehicleInvinsible.stopNaviSE(0);
				vehicleInvinsible.stopRiseSE(0);
				vehicleInvinsible.playRiseSE();
				CCastCommandTransit.getInstance().cast_BaseSystem().EnCountManager()
					.clearEncountFrame();
				vehicleInvinsible.setMyselfIdx(chr.CBaseCharacter.getLookIndex());
				chr.CBaseCharacter.setLookIndex(vehicleInvinsible.getCameraTargetIdx());
			}

			public override void update()
			{
				VehicleInvinsible vehicleInvinsible = static_cast<VehicleInvinsible>(Player());
				int motionSpeed = vehicleInvinsible.calcMotSpeedForHighNavi();
				vehicleInvinsible.setMotionSpeed(motionSpeed);
				VecFx32 direction = new VecFx32(0, 1, 0);
				VecFx32 vecFx = new VecFx32(vehicleInvinsible.getPosition());
				VecFx32 vecFx2 = new VecFx32(vecFx.x, VehicleInvinsible.HIGHT_HIGH, vecFx.z);
				vehicleInvinsible.setDirection(direction);
				if (vecFx.y >= vecFx2.y)
				{
					vehicleInvinsible.setPosition(vecFx2);
					if (vehicleInvinsible.checkNextActionToHighNavigate())
					{
						vehicleInvinsible.setNextAct(ACTION_ID_HIGHNAVIGATE);
					}
					else
					{
						vehicleInvinsible.setNextAct(3);
					}
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
				vehicleInvinsible.setMCLCol(b: true);
				vehicleInvinsible.getColFlag_or(2);
				vehicleInvinsible.getColFlag_or(4);
			}
		}
	}
}
