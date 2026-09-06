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
	public static partial class wld
	{
							public class CStateTalkMove : CBaseState
							{
								public override void start(CBaseSystem _sys)
								{
									CBaseSystem.initNextMode();
								}

								public override void update(CBaseSystem _sys)
								{
									if ((_sys.WorldCamera().Mode() == cmr.CWorldCamera.MODE.MODE_AUTOFOLLOW_DEFAULT || _sys.WorldCamera().Mode() == cmr.CWorldCamera.MODE.MODE_AUTOFOLLOW) && _sys.PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getCharacterId() != -1)
									{
										_sys.WorldCamera().setTrg(_sys.PlayerMng().Player(chr.CBaseCharacter.getLookIndex()).getPosition());
									}
									if (_sys.IsMapJump() | _sys.IsBattle() | _sys.IsTalk() | _sys.IsTitle())
									{
										setPhase(PHASE.END);
									}
								}

								public override void end(CBaseSystem _sys)
								{
									_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_END);
								}

								public override bool canExecuteEvent(CBaseSystem arg0)
								{
									return true;
								}
							}
	}
}
