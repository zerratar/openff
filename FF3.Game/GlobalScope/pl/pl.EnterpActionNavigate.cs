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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class pl
	{
		public class EnterpActionNavigate : CPlayerVehicleAction
		{
			public static int MOVE_MOTION_BLEND = 5;

			public override void start()
			{
				VehicleEnterp vehicleEnterp = static_cast<VehicleEnterp>(Player());
				if (vehicleEnterp.isOnSea())
				{
					vehicleEnterp.startMotion(ENTERP_MOTIONNO_SHIP_MOVE, _Loop: true, 0u);
				}
				vehicleEnterp.setMCLCol(b: true);
			}

			public override void update()
			{
				VehicleEnterp vehicleEnterp = static_cast<VehicleEnterp>(Player());
				if (!vehicleEnterp.isOperater() || vehicleEnterp.getBoardPlayer() == null || !vehicleEnterp.InputPermission())
				{
					return;
				}
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
				if (vehicleEnterp.checkNextActionToWait())
				{
					vehicleEnterp.setNextAct(0);
				}
				else if (vehicleEnterp.checkNextActionToLeave())
				{
					vehicleEnterp.stopBGM();
					vehicleEnterp.stopNaviSE(0);
					vehicleEnterp.setNextAct(5);
				}
			}

			public override void end()
			{
			}
		}
	}
}
