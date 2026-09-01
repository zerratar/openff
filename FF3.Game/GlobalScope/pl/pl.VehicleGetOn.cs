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
		public class VehicleGetOn : CPlayerVehicleAction
		{
			public override void start()
			{
				CPlayerVehicle cPlayerVehicle = static_cast<CPlayerVehicle>(Player());
				cPlayerVehicle.getColFlag_not_and(2);
				cPlayerVehicle.getColFlag_not_and(4);
				cPlayerVehicle.setAutoPilot(_AutoPilot: true);
				cPlayerVehicle.setOperater(_Operater: false);
				cPlayerVehicle.InputPermission_set(arg0: false);
				cPlayerVehicle.getParamMove().init();
				cPlayerVehicle.getParamTurn().init();
				cPlayerVehicle.MoveSys().setStop(b: true);
			}

			public override void update()
			{
				CPlayerVehicle cPlayerVehicle = static_cast<CPlayerVehicle>(Player());
				cPlayerVehicle.MoveSys().setStop(b: true);
				if (wld.MapSound.canChangeBGM())
				{
					cPlayerVehicle.setReserveToPlayBGM(reserve: true);
					cPlayerVehicle.setNextAct(cPlayerVehicle.getStartActionID());
				}
				else
				{
					cPlayerVehicle.setNextAct(cPlayerVehicle.getStartActionID());
				}
			}

			public override void end()
			{
				CPlayerVehicle cPlayerVehicle = static_cast<CPlayerVehicle>(Player());
				cPlayerVehicle.setAutoPilot(_AutoPilot: false);
				cPlayerVehicle.setOperater(_Operater: true);
				cPlayerVehicle.InputPermission_set(arg0: true);
				cPlayerVehicle.MoveSys().setStop(b: true);
				cPlayerVehicle.getColFlag_or(2);
				cPlayerVehicle.getColFlag_or(4);
			}
		}
	}
}
