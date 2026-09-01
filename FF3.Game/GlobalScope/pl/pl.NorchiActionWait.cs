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
		public class NorchiActionWait : CPlayerVehicleAction
		{
			public override void start()
			{
				VehicleNorchi vehicleNorchi = static_cast<VehicleNorchi>(Player());
				vehicleNorchi.AutoRun_set(arg0: false);
				vehicleNorchi.setMCLCol(b: true);
				if (vehicleNorchi.getMenuIcon() != null)
				{
					vehicleNorchi.getMenuIcon().setStateShow();
				}
				if (vehicleNorchi.getCameraIcon() != null)
				{
					vehicleNorchi.getCameraIcon().setStateShow();
				}
			}

			public override void update()
			{
				VehicleNorchi vehicleNorchi = static_cast<VehicleNorchi>(Player());
				if (!vehicleNorchi.isOperater() || vehicleNorchi.getBoardPlayer() == null || !vehicleNorchi.InputPermission())
				{
					return;
				}
				vehicleNorchi.playNaviSE();
				vehicleNorchi.setMotionSpeed(4096);
				if (vehicleNorchi.isOnSea())
				{
					if (vehicleNorchi.checkNextActionToRise())
					{
						vehicleNorchi.setNextAct(2);
						return;
					}
				}
				else if (vehicleNorchi.checkNextActionToDescent())
				{
					vehicleNorchi.setNextAct(3);
					return;
				}
				if (vehicleNorchi.checkNextActionToNavigate())
				{
					vehicleNorchi.setNextAct(1);
				}
			}

			public override void end()
			{
				VehicleNorchi vehicleNorchi = static_cast<VehicleNorchi>(Player());
				if (vehicleNorchi.getMenuIcon() != null)
				{
					vehicleNorchi.getMenuIcon().setStateHide();
				}
				if (vehicleNorchi.getCameraIcon() != null)
				{
					vehicleNorchi.getCameraIcon().setStateHide();
				}
			}
		}
	}
}
