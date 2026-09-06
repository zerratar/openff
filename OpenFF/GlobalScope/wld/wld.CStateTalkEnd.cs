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
	public static partial class wld
	{
							public class CStateTalkEnd : CBaseState
							{
								public override void start(CBaseSystem _sys)
								{
									if (_sys.getCustomFadeSetting(out var frame, out var color))
									{
										dgs.CFade.Main().fadeOut(frame, color);
										dgs.CFade.Sub().fadeOut(frame, color);
										_sys.cancelCustomFadeSetting();
										return;
									}
									if (dgs.CFade.Main().isCleared())
									{
										dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
									}
									if (dgs.CFade.Sub().isCleared())
									{
										dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
									}
								}

								public override void update(CBaseSystem _sys)
								{
									if (dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
									{
										setPhase(PHASE.END);
									}
								}

								public override void end(CBaseSystem _sys)
								{
									stageMng.delStage();
									_sys.cleanUpMessageData();
									_sys.cleanUpEventData();
									_sys.cleanUpNpcParameter();
									if (_sys.IsTitle())
									{
										_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN);
										sys.GGlobal.setNextPart(GAMEPART.GAMEPART_TITLE);
										_sys.setEnd(_End: true);
										return;
									}
									CBaseSystem.WORLD_MODE mode = _sys.PreviousMode();
									if (CCastCommandTransit.getInstance().castParam_MapJump().m_Flag)
									{
										_sys.MapJumpPosition();
										mode = ((sceneMng.getStage()[0] != 'f') ? CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN : CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD);
									}
									_sys.setMode(mode);
									sys.GGlobal.setNextPart(sys.GGlobal.getPreviousPart());
									_sys.setEnd(_End: true);
								}

								public override bool canExecuteEvent(CBaseSystem arg0)
								{
									return false;
								}
							}
	}
}
