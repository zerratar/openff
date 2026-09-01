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
		public class EnterpActionGetOff : VehicleGetOff
		{
			public override void start()
			{
				VehicleEnterp vehicleEnterp = static_cast<VehicleEnterp>(Player());
				base.start();
				vehicleEnterp.getParamMove().init();
				vehicleEnterp.getParamTurn().init();
				vehicleEnterp.InputPermission_set(arg0: false);
				vehicleEnterp.MoveSys().setStop(b: true);
				vehicleEnterp.TurnSys().setStop(b: true);
			}

			public override void update()
			{
				base.update();
			}

			public override void end()
			{
				VehicleEnterp vehicleEnterp = static_cast<VehicleEnterp>(Player());
				base.end();
				vehicleEnterp.InputPermission_set(arg0: true);
				vehicleEnterp.MoveSys().setStop(b: true);
				vehicleEnterp.TurnSys().setStop(b: false);
				VecFx32 targetDirection = new VecFx32(0, 0, 0);
				vehicleEnterp.setTargetDirection(targetDirection);
			}
		}
	}
}
