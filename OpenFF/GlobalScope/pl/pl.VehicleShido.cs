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
		public class VehicleShido : CPlayerVehicle
		{
			public override void initialize()
			{
				base.initialize();
				_actionList[0] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(ShidoActionWait));
				_actionList[1] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(ShidoActionNavigate));
				_actionList[2] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(ShidoActionRise));
				_actionList[3] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(ShidoActionDescent));
				_actionList[4] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(VehicleGetOn));
				_actionList[5] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(VehicleGetOff));
				registerAction(ACTION_ID.ACTION_ID_WAIT, _actionList[0]);
				registerAction(ACTION_ID.ACTION_ID_NAVIGATE, _actionList[1]);
				registerAction(ACTION_ID.ACTION_ID_RISE, _actionList[2]);
				registerAction(ACTION_ID.ACTION_ID_DESCENT, _actionList[3]);
				registerAction(ACTION_ID.ACTION_ID_APPEAR, _actionList[4]);
				registerAction(ACTION_ID.ACTION_ID_DISAPPEAR, _actionList[5]);
				MapMarkerUpdater.getSingleton().registerAccepter(composit2, 13);
			}

			public override void into()
			{
				base.into();
				characterMng.removeAllMotion(getCharacterId());
				characterMng.addMotion(getCharacterId(), "w_act_n451");
				characterMng.startMotion(getCharacterId(), VEHICLE_MOTIONNO_WAIT, fLoop: true, 5u);
				characterMng.setMotionSpeed(getCharacterId(), 0);
				characterMng.setFrame(getCharacterId(), 1u, ds.sys3d.CAnimSet.enTYPE.enTYPE_IVA);
				characterMng.setPause(getCharacterId(), pause: true, ds.sys3d.CAnimSet.enTYPE.enTYPE_IVA);
				setGrv(_GrvFlag: false);
				getColFlag_set(0);
				getColFlag_or(2);
				getColFlag_or(4);
				getColFlag_or(16);
				getColFlag_or(512);
				getColFlag_or(4096);
				getColFlag_or(131072);
			}

			public override void execute()
			{
				base.execute();
			}

			public override void update()
			{
				base.update();
			}

			public override void setConditionOfAir()
			{
				getPosition().y = VEHICLE_HEIGHT_AIR;
				setPreAct(0);
				setNowAct(0);
				setNextAct(0);
				startMotion(VEHICLE_MOTIONNO_WAIT, _Loop: true, 5u);
				setMotionSpeed(4096);
				setMCLCol(b: true);
				getColFlag_set(0);
				getColFlag_or(2);
				getColFlag_or(4);
				getColFlag_or(16);
				getColFlag_or(128);
				getColFlag_or(256);
				getColFlag_or(512);
			}
		}
	}
}
