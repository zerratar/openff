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
		public class CPlayerHumanOut : CPlayerHumanAction
		{
			private byte state_;

			public override void start()
			{
				Player().setAutoPilot(_AutoPilot: true);
				Player().InputPermission_set(arg0: false);
				Player().startMotion(1004, _Loop: true, 5u);
				Player().setMCLCol(b: true);
				Player().getColFlag_not_and(4096);
				Player().getColFlag_not_and(16);
				byte b = 10;
				Player().setSucAlpha(100);
				Player().setAutoAlphaFrame(b);
				Player().setWorkAutoAlphaFrame(0);
				Player().setSucShadowAlpha(10);
				Player().setAutoShadowAlphaFrame(b);
				Player().setWorkAutoShadowAlphaFrame(0);
				CPlayerHuman cPlayerHuman = (CPlayerHuman)Player();
				Player().setShadowType((uint)cPlayerHuman.getShadowType());
				state_ = 0;
				m_Counter = 0;
			}

			public override void update()
			{
				CPlayerVehicle cPlayerVehicle = static_cast<CPlayerVehicle>(Player().getTarget());
				if (cPlayerVehicle != null)
				{
					if (PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP == cPlayerVehicle.getVehicleType() || PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM == cPlayerVehicle.getVehicleType())
					{
						switch (state_)
						{
						case 0:
							if (++m_Counter >= 5)
							{
								state_ = 1;
							}
							break;
						case 1:
						{
							short landFormIndex = Player().getLandFormIndex();
							if (landFormIndex == 1 || landFormIndex == 2 || landFormIndex == 3 || landFormIndex == 6)
							{
								Player().setNextAct(0);
							}
							break;
						}
						}
					}
					else if (Player().getTransparencyRate() >= 100)
					{
						Player().setNextAct(0);
					}
				}
				else
				{
					Player().setNextAct(0);
				}
			}

			public override void end()
			{
				if (Player().isOperater())
				{
					Player().setTarget(null);
					CPlayerHuman cPlayerHuman = (CPlayerHuman)Player();
					CPlayerHuman cPlayerHuman2 = cPlayerHuman.getNpc();
					if (cPlayerHuman2 != null)
					{
						cPlayerHuman2.setNextAct(0);
						cPlayerHuman2.setPosition(cPlayerHuman.getPosition());
						cPlayerHuman2.setTarget(Player());
						cPlayerHuman2.setAutoPilot(_AutoPilot: false);
						cPlayerHuman2.setShadowType((uint)cPlayerHuman2.getShadowType());
					}
					Player().setAutoPilot(_AutoPilot: false);
					Player().InputPermission_set(arg0: true);
					Player().setMCLCol(b: true);
					Player().getColFlag_or(2);
					Player().getColFlag_or(4096);
					Player().getColFlag_or(16);
					Player().MoveSys().setFlag(_Flag: true);
					VecFx32 targetDirection = new VecFx32(0, 0, 0);
					Player().setTargetDirection(targetDirection);
				}
			}
		}
	}
}
