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
		public class ShidoActionNavigate : CPlayerVehicleAction
		{
			public override void start()
			{
			}

			public override void update()
			{
				VehicleShido vehicleShido = static_cast<VehicleShido>(Player());
				if (vehicleShido.isOperater() && vehicleShido.getBoardPlayer() != null && vehicleShido.InputPermission())
				{
					if (vehicleShido.checkNextActionToDescent())
					{
						vehicleShido.setNextAct(3);
					}
					else if (vehicleShido.checkNextActionToWait())
					{
						vehicleShido.setNextAct(0);
					}
				}
			}

			public override void end()
			{
			}
		}
	}
}
