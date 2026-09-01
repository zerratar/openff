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
		public class ChokoboActionNavigate : CPlayerVehicleAction
		{
			public override void start()
			{
				VehicleChokobo vehicleChokobo = static_cast<VehicleChokobo>(Player());
				vehicleChokobo.startMotion(CHOKOBO_MOTIONNO_RUN, _Loop: true, 5u);
				vehicleChokobo.setRunning(running: true);
			}

			public override void update()
			{
				VehicleChokobo vehicleChokobo = static_cast<VehicleChokobo>(Player());
				if (vehicleChokobo.isOperater() && vehicleChokobo.getBoardPlayer() != null && vehicleChokobo.InputPermission())
				{
					if (vehicleChokobo.checkNextActionToWait())
					{
						vehicleChokobo.setNextAct(0);
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
			}
		}
	}
}
