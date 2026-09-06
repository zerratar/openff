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
		public class VehicleGetOff : CPlayerVehicleAction
		{
			public override void start()
			{
				CPlayerVehicle cPlayerVehicle = static_cast<CPlayerVehicle>(Player());
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
				if (wld.MapSound.canChangeBGM() && !wld.MapSound.isPlaying())
				{
					cPlayerVehicle.setNextAct(0);
				}
				else if (!wld.MapSound.canChangeBGM())
				{
					cPlayerVehicle.setNextAct(0);
				}
			}

			public override void end()
			{
				CPlayerVehicle cPlayerVehicle = static_cast<CPlayerVehicle>(Player());
				cPlayerVehicle.returnToFieldBGM();
				cPlayerVehicle.getColFlag_or(2);
				cPlayerVehicle.getColFlag_or(4);
				cPlayerVehicle.InputPermission_set(arg0: true);
				cPlayerVehicle.MoveSys().setStop(b: true);
				cPlayerVehicle.dropPlayer();
			}
		}
	}
}
