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
		public class EnterpActionWait : CPlayerVehicleAction
		{
			public static int WAIT_MOTION_BLEND = 5;

			public override void start()
			{
				VehicleEnterp vehicleEnterp = static_cast<VehicleEnterp>(Player());
				if (vehicleEnterp.isOnSea())
				{
					vehicleEnterp.startMotion(ENTERP_MOTIONNO_SHIP_WAIT, _Loop: true, 0u);
				}
				else if (vehicleEnterp.getPreAct() != 1)
				{
					vehicleEnterp.startMotion(ENTERP_MOTIONNO_AIR_WAIT, _Loop: true, 5u);
				}
				vehicleEnterp.AutoRun_set(arg0: false);
				vehicleEnterp.setMCLCol(b: true);
				if (vehicleEnterp.getMenuIcon() != null)
				{
					vehicleEnterp.getMenuIcon().setStateShow();
				}
				if (vehicleEnterp.getCameraIcon() != null)
				{
					vehicleEnterp.getCameraIcon().setStateShow();
				}
			}

			public override void update()
			{
				VehicleEnterp vehicleEnterp = static_cast<VehicleEnterp>(Player());
				if (!vehicleEnterp.isOperater() || vehicleEnterp.getBoardPlayer() == null || !vehicleEnterp.InputPermission())
				{
					return;
				}
				vehicleEnterp.playNaviSE();
				if (vehicleEnterp.isOnSea())
				{
					if (vehicleEnterp.checkNextActionToRise())
					{
						vehicleEnterp.setNextAct(2);
						return;
					}
				}
				else if (vehicleEnterp.checkNextActionToDescent())
				{
					vehicleEnterp.setNextAct(3);
					return;
				}
				if (vehicleEnterp.checkNextActionToNavigate())
				{
					vehicleEnterp.setNextAct(1);
				}
			}

			public override void end()
			{
				VehicleEnterp vehicleEnterp = static_cast<VehicleEnterp>(Player());
				if (vehicleEnterp.getMenuIcon() != null)
				{
					vehicleEnterp.getMenuIcon().setStateHide();
				}
				if (vehicleEnterp.getCameraIcon() != null)
				{
					vehicleEnterp.getCameraIcon().setStateHide();
				}
			}
		}
	}
}
