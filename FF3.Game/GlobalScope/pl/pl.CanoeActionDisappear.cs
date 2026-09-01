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
		public class CanoeActionDisappear : CPlayerVehicleAction
		{
			public override void start()
			{
				VehicleCanoe vehicleCanoe = static_cast<VehicleCanoe>(Player());
				vehicleCanoe.setAutoPilot(_AutoPilot: true);
			}

			public override void update()
			{
				VehicleCanoe vehicleCanoe = static_cast<VehicleCanoe>(Player());
				vehicleCanoe.setNextAct(0);
			}

			public override void end()
			{
				VehicleCanoe vehicleCanoe = static_cast<VehicleCanoe>(Player());
				vehicleCanoe.setOperater(_Operater: false);
				vehicleCanoe.setAutoPilot(_AutoPilot: true);
				vehicleCanoe.setMCLCol(b: false);
				byte b = 4;
				vehicleCanoe.setSucAlpha(0);
				vehicleCanoe.setAutoAlphaFrame(b);
				vehicleCanoe.setWorkAutoAlphaFrame(b);
				vehicleCanoe.setSucShadowAlpha(0);
				vehicleCanoe.setAutoShadowAlphaFrame(b);
				vehicleCanoe.setWorkAutoShadowAlphaFrame(b);
				CPlayerHuman cPlayerHuman = static_cast<CPlayerHuman>(vehicleCanoe.getBoardPlayer());
				vehicleCanoe.dropPlayer();
				cPlayerHuman.setNextAct(10);
				vehicleCanoe.getPosition().y = -409600;
			}
		}
	}
}
