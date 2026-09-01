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
		public class CPlayerHumanBoard : CPlayerHumanAction
		{
			private VecFx32 startPos_;

			public override void start()
			{
				Player().startMotion(1004, _Loop: true, 5u);
				Player().setMCLCol(b: false);
				Player().getColFlag_not_and(2);
				if (Player().isOperater())
				{
					Player().setAutoPilot(_AutoPilot: true);
					Player().InputPermission_set(arg0: false);
					Player().getColFlag_not_and(4096);
					CPlayerVehicle vehicle = getVehicle(Player());
					VecFx32 vecFx = new VecFx32(0, 0, 0);
					VecFx32 vecFx2 = new VecFx32(Player().getPosition());
					VecFx32 vecFx3 = new VecFx32(vehicle.getPosition());
					vecFx2.y = 0;
					vecFx3.y = 0;
					VEC_Subtract(vecFx3, vecFx2, vecFx);
					VEC_Normalize(vecFx, vecFx);
					vecFx.x = FX_Mul(vecFx.x, 2048);
					vecFx.y = FX_Mul(vecFx.y, 2048);
					vecFx.z = FX_Mul(vecFx.z, 2048);
					Player().MoveSys().setFlag(_Flag: false);
					Player().setTargetDirection(vecFx);
					startPos_ = vecFx2;
					m_Counter = 30;
					vehicle.stopBGM();
					CPlayerHuman cPlayerHuman = ((CPlayerHuman)Player()).getNpc();
					if (cPlayerHuman != null)
					{
						cPlayerHuman.setNextAct(7);
						cPlayerHuman.setAutoRun(_AutoRun: true);
					}
				}
				else
				{
					CPlayerHumanAction.m_WaitNPCFlag = true;
					Player().setAutoRun(_AutoRun: true);
				}
				byte b = 0;
				b = (byte)((!Player().isOperater()) ? 5 : 20);
				Player().setSucAlpha(0);
				Player().setAutoAlphaFrame(b);
				Player().setWorkAutoAlphaFrame(b);
				Player().setSucShadowAlpha(0);
				Player().setAutoShadowAlphaFrame(b);
				Player().setWorkAutoShadowAlphaFrame(b);
				Player().setShadowType(2u);
			}

			public override void update()
			{
				if (!Player().isOperater())
				{
					if (Player().getTransparencyRate() <= 0)
					{
						CPlayerHumanAction.m_WaitNPCFlag = false;
					}
					return;
				}
				CPlayerVehicle cPlayerVehicle = (CPlayerVehicle)Player().getTarget();
				if (PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_INVINSIBLE != cPlayerVehicle.getVehicleType())
				{
					if (m_Counter != 0)
					{
						m_Counter--;
						chr.CCharacterEureka vehicle = getVehicle(Player());
						VecFx32 vecFx = new VecFx32(0, 0, 0);
						VecFx32 vecFx2 = new VecFx32(0, 0, 0);
						VEC_Subtract(Player().getPosition(), startPos_, vecFx);
						VEC_Subtract(vehicle.getPosition(), startPos_, vecFx2);
						int num = FX_Div(VEC_Mag(vecFx), VEC_Mag(vecFx2));
						if (num >= 4096)
						{
							Player().setSucAlpha(-1);
							Player().setAutoAlphaFrame(-1);
							Player().setWorkAutoAlphaFrame(-1);
							Player().setTransparencyRate(0);
							Player().setPosition(vehicle.getPosition());
							m_Counter = 0;
						}
					}
					if (!CPlayerHumanAction.m_WaitNPCFlag && m_Counter == 0)
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
				if (Player().isOperater() && Player().getTarget() != null)
				{
					CPlayerVehicle cPlayerVehicle = static_cast<CPlayerVehicle>(Player().getTarget());
					PLAYER_VEHICLE_TYPE vehicleType = cPlayerVehicle.getVehicleType();
					cPlayerVehicle.setBoardSetting(Player());
					if (vehicleType == PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_INVINSIBLE)
					{
						chr.CBaseCharacter.setLookIndex(0);
						cPlayerVehicle.setEnableCalcCamHeight(b: false);
						cPlayerVehicle.setPullBoardPlayer(b: false);
					}
					cPlayerVehicle.setGetOnAction();
					cPlayerVehicle.setAutoPilot(_AutoPilot: true);
					cPlayerVehicle.InputPermission_set(arg0: false);
				}
			}
		}
	}
}
