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
		public class CStateSiteEnd : CBaseState
		{
			public override void start(CBaseSystem _sys)
			{
				dgs.CFade.Main().fadeIn(5);
				dgs.CFade.Sub().fadeIn(5);
			}

			public override void update(CBaseSystem _sys)
			{
				if (dgs.CFade.Main().isCleared())
				{
					setPhase(PHASE.END);
				}
			}

			public override void end(CBaseSystem _sys)
			{
				if (sceneMng.getStage()[0] == 'f')
				{
					_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD);
				}
				else
				{
					_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN);
				}
				_sys.World2DMng().MenuStartButton().create();
				_sys.World2DMng().MenuStartButton().setStateShow();
				_sys.World2DMng().CameraButton().create();
				if (_sys.World2DMng().visibleMap())
				{
					_sys.World2DMng().CameraButton().setStateShow();
				}
				pl.CPlayerHuman cPlayerHuman = _sys.PlayerMng().PlayerHuman(0);
				if (cPlayerHuman != null && cPlayerHuman.getNpc() != null && 3 != pl.PlayerParty.instance().npc().npcId())
				{
					_sys.World2DMng().TalkButton().create();
					_sys.World2DMng().TalkButton().setStateShow();
				}
				_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_MOVE);
				_sys.setMenu(b: false);
				_sys.CrtState().setPhase(PHASE.START);
			}

			public override bool canExecuteEvent(CBaseSystem arg0)
			{
				return false;
			}
		}
	}
}
