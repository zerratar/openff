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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	public static partial class pl
	{
		public class InvinsibleActionEnterInside : CPlayerVehicleAction
		{
			public override void start()
			{
				wld.CWorldOutSideData.getInstance().MapData().isColFlag_not_and(2048);
				wld.CWorldOutSideData.getInstance().MapData().MapJumpIndex_set(1);
				wld.CWorldOutSideData.getInstance().MapData().setSpMapType(wld.CMapData.SP_MAP_TYPE.SP_MAP_INVINSIBLE);
			}

			public override void update()
			{
				VehicleInvinsible vehicleInvinsible = static_cast<VehicleInvinsible>(Player());
				VecFx32 direction = new VecFx32(0, 0, 0);
				vehicleInvinsible.setDirection(direction);
				vehicleInvinsible.setAutoPilot(_AutoPilot: true);
				vehicleInvinsible.setOperater(_Operater: false);
			}

			public override void end()
			{
			}
		}
	}
}
