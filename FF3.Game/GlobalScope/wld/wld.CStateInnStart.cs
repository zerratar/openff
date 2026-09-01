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
							public class CStateInnStart : CBaseState
							{
								public override void start(CBaseSystem _sys)
								{
									evt.CEventManager.getInstance().setEventStop(_EventStop: true);
								}

								public override void update(CBaseSystem _sys)
								{
									setPhase(PHASE.END);
								}

								public override void end(CBaseSystem _sys)
								{
									_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_MOVE);
								}

								public override bool canExecuteEvent(CBaseSystem arg0)
								{
									return true;
								}
							}
	}
}
