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
		public class CanoeActionWait : CPlayerVehicleAction
		{
			public override void start()
			{
				VehicleCanoe vehicleCanoe = static_cast<VehicleCanoe>(Player());
				if (vehicleCanoe.isOperater() && vehicleCanoe.getBoardPlayer() != null)
				{
					if (1001 != vehicleCanoe.getMotionIndex())
					{
						vehicleCanoe.startMotion(1001, _Loop: true, 5u);
					}
					vehicleCanoe.AutoRun_set(arg0: false);
					vehicleCanoe.setMCLCol(b: true);
					if (vehicleCanoe.getMenuIcon() != null)
					{
						vehicleCanoe.getMenuIcon().setStateShow();
					}
					if (vehicleCanoe.getCameraIcon() != null)
					{
						vehicleCanoe.getCameraIcon().setStateShow();
					}
				}
			}

			public override void update()
			{
				VehicleCanoe vehicleCanoe = static_cast<VehicleCanoe>(Player());
				if (vehicleCanoe.isOperater() && vehicleCanoe.getBoardPlayer() != null && vehicleCanoe.InputPermission() && vehicleCanoe.checkNextActionToNavigate())
				{
					vehicleCanoe.setNextAct(1);
				}
			}

			public override void end()
			{
				VehicleCanoe vehicleCanoe = static_cast<VehicleCanoe>(Player());
				if (vehicleCanoe.getMenuIcon() != null)
				{
					vehicleCanoe.getMenuIcon().setStateHide();
				}
				if (vehicleCanoe.getCameraIcon() != null)
				{
					vehicleCanoe.getCameraIcon().setStateHide();
				}
			}
		}
	}
}
