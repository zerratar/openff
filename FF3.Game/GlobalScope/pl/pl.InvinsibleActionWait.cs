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
		public class InvinsibleActionWait : CPlayerVehicleAction
		{
			public override void start()
			{
				VehicleInvinsible vehicleInvinsible = static_cast<VehicleInvinsible>(Player());
				vehicleInvinsible.setMCLCol(b: true);
				if (vehicleInvinsible.getMenuIcon() != null)
				{
					vehicleInvinsible.getMenuIcon().setStateShow();
				}
				if (vehicleInvinsible.getCameraIcon() != null)
				{
					vehicleInvinsible.getCameraIcon().setStateShow();
				}
			}

			public override void update()
			{
				VehicleInvinsible vehicleInvinsible = static_cast<VehicleInvinsible>(Player());
				if (vehicleInvinsible.isOperater() && vehicleInvinsible.getBoardPlayer() != null && vehicleInvinsible.InputPermission())
				{
					vehicleInvinsible.playNaviSE();
					if (vehicleInvinsible.checkNextActionToRise())
					{
						vehicleInvinsible.MoveSys().setStop(b: true);
						vehicleInvinsible.setNextAct(2);
					}
					else if (vehicleInvinsible.checkNextActionToNavigate())
					{
						vehicleInvinsible.setNextAct(1);
					}
					else if (vehicleInvinsible.checkToEnterInside())
					{
						vehicleInvinsible.enterInside();
						vehicleInvinsible.setNextAct(0);
					}
				}
			}

			public override void end()
			{
				VehicleInvinsible vehicleInvinsible = static_cast<VehicleInvinsible>(Player());
				if (vehicleInvinsible.getMenuIcon() != null)
				{
					vehicleInvinsible.getMenuIcon().setStateHide();
				}
				if (vehicleInvinsible.getCameraIcon() != null)
				{
					vehicleInvinsible.getCameraIcon().setStateHide();
				}
			}
		}
	}
}
