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
		public class ShidoActionWait : CPlayerVehicleAction
		{
			public override void start()
			{
				VehicleShido vehicleShido = static_cast<VehicleShido>(Player());
				vehicleShido.AutoRun_set(arg0: false);
				if (vehicleShido.getMenuIcon() != null)
				{
					vehicleShido.getMenuIcon().setStateShow();
				}
				if (vehicleShido.getCameraIcon() != null)
				{
					vehicleShido.getCameraIcon().setStateShow();
				}
			}

			public override void update()
			{
				VehicleShido vehicleShido = static_cast<VehicleShido>(Player());
				if (vehicleShido.isOperater() && vehicleShido.getBoardPlayer() != null && vehicleShido.InputPermission())
				{
					vehicleShido.playNaviSE();
					if (vehicleShido.checkNextActionToDescent())
					{
						vehicleShido.setNextAct(3);
					}
					else if (vehicleShido.checkNextActionToNavigate())
					{
						vehicleShido.setNextAct(1);
					}
				}
			}

			public override void end()
			{
				VehicleShido vehicleShido = static_cast<VehicleShido>(Player());
				if (vehicleShido.getMenuIcon() != null)
				{
					vehicleShido.getMenuIcon().setStateHide();
				}
				if (vehicleShido.getCameraIcon() != null)
				{
					vehicleShido.getCameraIcon().setStateHide();
				}
			}
		}
	}
}
