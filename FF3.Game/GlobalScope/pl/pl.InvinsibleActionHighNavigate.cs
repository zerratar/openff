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
		public class InvinsibleActionHighNavigate : CPlayerVehicleAction
		{
			private sbyte state_;

			public override void start()
			{
				VehicleInvinsible vehicleInvinsible = static_cast<VehicleInvinsible>(Player());
				vehicleInvinsible.setAutoPilot(_AutoPilot: false);
				vehicleInvinsible.setOperater(_Operater: true);
				vehicleInvinsible.getColFlag_not_and(2);
				vehicleInvinsible.getColFlag_not_and(4);
				vehicleInvinsible.getColFlag_not_and(512);
				state_ = 0;
			}

			public override void update()
			{
				VehicleInvinsible vehicleInvinsible = static_cast<VehicleInvinsible>(Player());
				int motionSpeed = vehicleInvinsible.calcMotSpeedForHighNavi();
				vehicleInvinsible.setMotionSpeed(motionSpeed);
				VecFx32 direction = new VecFx32(vehicleInvinsible.getMoveDirection());
				vehicleInvinsible.setDirection(direction);
				sbyte landFormIndex = vehicleInvinsible.getLandFormIndex();
				switch (state_)
				{
				case 0:
					if (landFormIndex == 12)
					{
						state_ = 1;
					}
					break;
				case 1:
					if (landFormIndex != 12)
					{
						m_Counter = 0;
						state_ = 2;
					}
					break;
				case 2:
					vehicleInvinsible.setNextAct(3);
					break;
				}
			}

			public override void end()
			{
				VehicleInvinsible vehicleInvinsible = static_cast<VehicleInvinsible>(Player());
				vehicleInvinsible.getColFlag_or(512);
			}
		}
	}
}
