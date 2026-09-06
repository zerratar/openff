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
		public class CanoeActionNavigate : CPlayerVehicleAction
		{
			public override void start()
			{
				VehicleCanoe vehicleCanoe = static_cast<VehicleCanoe>(Player());
				if (1002 != vehicleCanoe.getMotionIndex())
				{
					vehicleCanoe.startMotion(1002, _Loop: true, 5u);
				}
				vehicleCanoe.setMCLCol(b: true);
			}

			public override void update()
			{
				VehicleCanoe vehicleCanoe = static_cast<VehicleCanoe>(Player());
				if (vehicleCanoe.isOperater() && vehicleCanoe.getBoardPlayer() != null && vehicleCanoe.InputPermission())
				{
					vehicleCanoe.playNaviSE();
					if (vehicleCanoe.checkNextActionToDisappear())
					{
						vehicleCanoe.setNextAct(5);
					}
					else if (vehicleCanoe.checkNextActionToWait())
					{
						vehicleCanoe.setNextAct(0);
					}
				}
			}

			public override void end()
			{
			}
		}
	}
}
