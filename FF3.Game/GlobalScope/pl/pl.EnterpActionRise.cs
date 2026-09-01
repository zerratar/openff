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
		public class EnterpActionRise : CPlayerVehicleAction
		{
			public enum RISE_STATE
			{
				STATE_MAST_IN,
				STATE_PROPELLER_OUT,
				STATE_RISE
			}

			public const RISE_STATE STATE_MAST_IN = RISE_STATE.STATE_MAST_IN;

			public const RISE_STATE STATE_PROPELLER_OUT = RISE_STATE.STATE_PROPELLER_OUT;

			public const RISE_STATE STATE_RISE = RISE_STATE.STATE_RISE;

			private RISE_STATE _state;

			private bool changeBGM_;

			public override void start()
			{
				VehicleEnterp vehicleEnterp = static_cast<VehicleEnterp>(Player());
				if (vehicleEnterp.isOnSea())
				{
					_state = RISE_STATE.STATE_MAST_IN;
					vehicleEnterp.startMotion(ENTERP_MOTIONNO_INMAST, _Loop: false, 0u);
				}
				else
				{
					_state = RISE_STATE.STATE_RISE;
				}
				vehicleEnterp.setMCLCol(b: false);
				vehicleEnterp.getColFlag_not_and(2);
				vehicleEnterp.getColFlag_not_and(4);
				vehicleEnterp.setAutoPilot(_AutoPilot: true);
				vehicleEnterp.setOperater(_Operater: false);
				vehicleEnterp.InputPermission_set(arg0: false);
				vehicleEnterp.getParamMove().init();
				vehicleEnterp.getParamTurn().init();
				vehicleEnterp.MoveSys().setStop(b: true);
				vehicleEnterp.TurnSys().setStop(b: true);
				VecFx32 direction = new VecFx32(0, 0, 0);
				vehicleEnterp.setDirection(direction);
				vehicleEnterp.stopDescSE(0);
				vehicleEnterp.stopNaviSE(0);
				vehicleEnterp.playRiseSE();
				changeBGM_ = false;
			}

			public override void update()
			{
				VehicleEnterp vehicleEnterp = static_cast<VehicleEnterp>(Player());
				switch (_state)
				{
				case RISE_STATE.STATE_MAST_IN:
					if (vehicleEnterp.isEndOfMotion())
					{
						if (wld.MapSound.canChangeBGM())
						{
							wld.MapSound.stopBGM(15);
							changeBGM_ = true;
						}
						vehicleEnterp.startMotion(ENTERP_MOTIONNO_OUTPROPELLER, _Loop: false, 0u);
						vehicleEnterp.setPropeller();
						_state = RISE_STATE.STATE_PROPELLER_OUT;
					}
					break;
				case RISE_STATE.STATE_PROPELLER_OUT:
					if (vehicleEnterp.isEndOfMotion())
					{
						vehicleEnterp.startMotion(ENTERP_MOTIONNO_AIR_WAIT, _Loop: true, 0u);
						vehicleEnterp.setShadowType(4u);
						vehicleEnterp.setVisibleWave(b: false);
						vehicleEnterp.playDropEffect();
						_state = RISE_STATE.STATE_RISE;
					}
					break;
				case RISE_STATE.STATE_RISE:
				{
					VecFx32 direction = new VecFx32(0, 1, 0);
					VecFx32 vecFx = new VecFx32(vehicleEnterp.getPosition());
					VecFx32 vecFx2 = new VecFx32(vecFx.x, VEHICLE_HEIGHT_AIR, vecFx.z);
					vehicleEnterp.setDirection(direction);
					if (vecFx.y >= vecFx2.y)
					{
						if (wld.MapSound.canChangeBGM() && changeBGM_)
						{
							wld.MapSound.playBGM(9, 192, 0);
						}
						vehicleEnterp.setOnAir();
						vehicleEnterp.setPosition(vecFx2);
						vehicleEnterp.setNextAct(0);
					}
					break;
				}
				}
			}

			public override void end()
			{
				VehicleEnterp vehicleEnterp = static_cast<VehicleEnterp>(Player());
				vehicleEnterp.setAutoPilot(_AutoPilot: false);
				vehicleEnterp.setOperater(_Operater: true);
				vehicleEnterp.getParamMove().init();
				vehicleEnterp.getParamTurn().init();
				vehicleEnterp.getParamGrv().init();
				vehicleEnterp.InputPermission_set(arg0: true);
				vehicleEnterp.MoveSys().setStop(b: true);
				vehicleEnterp.TurnSys().setStop(b: false);
				VecFx32 targetDirection = new VecFx32(0, 0, 0);
				vehicleEnterp.setTargetDirection(targetDirection);
				vehicleEnterp.setMCLCol(b: true);
				vehicleEnterp.getColFlag_or(2);
				vehicleEnterp.getColFlag_or(4);
				vehicleEnterp.stopRiseSE(0);
			}
		}
	}
}
