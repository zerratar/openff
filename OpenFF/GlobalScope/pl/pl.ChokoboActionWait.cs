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
		public class ChokoboActionWait : CPlayerVehicleAction
		{
			public override void start()
			{
				VehicleChokobo vehicleChokobo = static_cast<VehicleChokobo>(Player());
				vehicleChokobo.AutoRun_set(arg0: false);
				vehicleChokobo.startMotion(CHOKOBO_MOTIONNO_WAIT, _Loop: true, 5u);
				if (vehicleChokobo.getMenuIcon() != null)
				{
					vehicleChokobo.getMenuIcon().setStateShow();
				}
				if (vehicleChokobo.getCameraIcon() != null)
				{
					vehicleChokobo.getCameraIcon().setStateShow();
				}
			}

			public override void update()
			{
				VehicleChokobo vehicleChokobo = static_cast<VehicleChokobo>(Player());
				if (vehicleChokobo.isOperater() && vehicleChokobo.getBoardPlayer() != null && vehicleChokobo.InputPermission())
				{
					if (vehicleChokobo.checkNextActionToNavigate())
					{
						vehicleChokobo.setNextAct(1);
					}
					else if (vehicleChokobo.checkNextActionToDisappear())
					{
						vehicleChokobo.stopBGM();
						vehicleChokobo.setNextAct(5);
					}
				}
			}

			public override void end()
			{
				VehicleChokobo vehicleChokobo = static_cast<VehicleChokobo>(Player());
				if (vehicleChokobo.getMenuIcon() != null)
				{
					vehicleChokobo.getMenuIcon().setStateHide();
				}
				if (vehicleChokobo.getCameraIcon() != null)
				{
					vehicleChokobo.getCameraIcon().setStateHide();
				}
			}
		}
	}
}
