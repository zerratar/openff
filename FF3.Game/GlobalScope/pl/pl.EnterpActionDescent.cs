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
		public class EnterpActionDescent : CPlayerVehicleAction
		{
			public enum DESCENT_STATE
			{
				STATE_DESCENT,
				STATE_SPRAY,
				STATE_ONSEA,
				STATE_PROPELLER_STOP,
				STATE_PROPELLER_IN,
				STATE_MAST_OUT
			}

			public const DESCENT_STATE STATE_DESCENT = DESCENT_STATE.STATE_DESCENT;

			public const DESCENT_STATE STATE_SPRAY = DESCENT_STATE.STATE_SPRAY;

			public const DESCENT_STATE STATE_ONSEA = DESCENT_STATE.STATE_ONSEA;

			public const DESCENT_STATE STATE_PROPELLER_STOP = DESCENT_STATE.STATE_PROPELLER_STOP;

			public const DESCENT_STATE STATE_PROPELLER_IN = DESCENT_STATE.STATE_PROPELLER_IN;

			public const DESCENT_STATE STATE_MAST_OUT = DESCENT_STATE.STATE_MAST_OUT;

			private DESCENT_STATE _state;

			public override void start()
			{
				VehicleEnterp vehicleEnterp = static_cast<VehicleEnterp>(Player());
				_state = DESCENT_STATE.STATE_DESCENT;
				vehicleEnterp.getColFlag_not_and(2);
				vehicleEnterp.getColFlag_not_and(4);
				vehicleEnterp.setGrv(_GrvFlag: false);
				vehicleEnterp.setAutoPilot(_AutoPilot: true);
				vehicleEnterp.setOperater(_Operater: false);
				vehicleEnterp.InputPermission_set(arg0: false);
				vehicleEnterp.getParamMove().init();
				vehicleEnterp.getParamTurn().init();
				vehicleEnterp.MoveSys().setStop(b: true);
				vehicleEnterp.TurnSys().setStop(b: true);
				vehicleEnterp.stopNaviSE(0);
				vehicleEnterp.stopRiseSE(0);
				vehicleEnterp.playDescSE();
			}

			public override void update()
			{
				VehicleEnterp vehicleEnterp = static_cast<VehicleEnterp>(Player());
				VecFx32 vecFx = new VecFx32(0, -1, 0);
				VecFx32 vecFx2 = new VecFx32(vehicleEnterp.getPosition());
				switch (_state)
				{
				case DESCENT_STATE.STATE_DESCENT:
					vehicleEnterp.setDirection(vecFx);
					if (vecFx2.y <= VEHICLE_HEIGHT_GROUND)
					{
						short landFormIndex = vehicleEnterp.getLandFormIndex();
						if (landFormIndex == 5 || landFormIndex == 11)
						{
							_state = DESCENT_STATE.STATE_SPRAY;
						}
						else
						{
							vehicleEnterp.setNextAct(2);
						}
					}
					break;
				case DESCENT_STATE.STATE_SPRAY:
					vehicleEnterp.setDirection(vecFx);
					if (vecFx2.y <= map.MAP_HEIGHT_SEA)
					{
						int id = eff.CEffectMng.instance().create(EFFECT_CATEGORY_VEHICLE, EFFECT_MEMBER_SPRAY);
						vecFx2.y = map.MAP_HEIGHT_SEA;
						eff.CEffectMng.instance().setPosition(id, vecFx2);
						_state = DESCENT_STATE.STATE_ONSEA;
					}
					break;
				case DESCENT_STATE.STATE_ONSEA:
				{
					VecFx32 vecFx3 = new VecFx32(vecFx2.x, VEHICLE_HEIGHT_ONSEA, vecFx2.z);
					vehicleEnterp.setDirection(vecFx);
					if (vecFx2.y <= vecFx3.y)
					{
						vehicleEnterp.setPosition(vecFx3);
						vehicleEnterp.setMotionLoop(loop: false);
						vecFx.y = 0;
						vehicleEnterp.setDirection(vecFx);
						vehicleEnterp.setShadowType(2u);
						vehicleEnterp.setVisibleWave(b: true);
						_state = DESCENT_STATE.STATE_PROPELLER_STOP;
					}
					break;
				}
				case DESCENT_STATE.STATE_PROPELLER_STOP:
					if (vehicleEnterp.isEndOfMotion())
					{
						if (wld.MapSound.canChangeBGM())
						{
							wld.MapSound.stopBGM(15);
						}
						vehicleEnterp.startMotion(ENTERP_MOTIONNO_INPROPELLER, _Loop: false, 0u);
						_state = DESCENT_STATE.STATE_PROPELLER_IN;
					}
					break;
				case DESCENT_STATE.STATE_PROPELLER_IN:
					if (vehicleEnterp.isEndOfMotion())
					{
						vehicleEnterp.startMotion(ENTERP_MOTIONNO_OUTMAST, _Loop: false, 0u);
						vehicleEnterp.setMast();
						_state = DESCENT_STATE.STATE_MAST_OUT;
					}
					break;
				case DESCENT_STATE.STATE_MAST_OUT:
					if (vehicleEnterp.isEndOfMotion())
					{
						if (wld.MapSound.canChangeBGM())
						{
							wld.MapSound.playBGM(20, 192, 0);
						}
						vehicleEnterp.startMotion(ENTERP_MOTIONNO_SHIP_WAIT, _Loop: true, 0u);
						vehicleEnterp.setOnSea();
						vehicleEnterp.setNextAct(0);
					}
					break;
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
				VecFx32 direction = new VecFx32(1, 0, 1);
				vehicleEnterp.setDirection(direction);
			}
		}
	}
}
