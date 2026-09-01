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
	public static partial class wld
	{
		public class CStateInnEnd : CBaseState
		{
			public override void start(CBaseSystem _sys)
			{
			}

			public override void update(CBaseSystem _sys)
			{
				setPhase(PHASE.END);
			}

			public override void end(CBaseSystem _sys)
			{
				_sys.setMode(_sys.PreviousMode());
				_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_MOVE);
				_sys.CrtState().phase_set(PHASE.UPDATE);
				_sys.setInn(b: false);
				evt.CEventManager.getInstance().setEventStop(_EventStop: false);
			}

			public override bool canExecuteEvent(CBaseSystem arg0)
			{
				return false;
			}
		}
	}
}
