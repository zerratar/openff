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
		public class EnterpActionGetOn : VehicleGetOn
		{
			public override void start()
			{
				base.start();
			}

			public override void update()
			{
				VehicleEnterp vehicleEnterp = static_cast<VehicleEnterp>(Player());
				vehicleEnterp.MoveSys().setStop(b: true);
				if (wld.MapSound.canChangeBGM())
				{
					if (!wld.MapSound.isPlaying())
					{
						vehicleEnterp.playBGM();
						vehicleEnterp.setNextAct(vehicleEnterp.getStartActionID());
					}
				}
				else
				{
					vehicleEnterp.setNextAct(vehicleEnterp.getStartActionID());
				}
			}

			public override void end()
			{
				base.end();
			}
		}
	}
}
