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
							public class CStateSiteMove : CBaseState
							{
								public bool nowFading_;

								public override void start(CBaseSystem _sys)
								{
									nowFading_ = false;
								}

								public override void update(CBaseSystem _sys)
								{
									if (!nowFading_)
									{
										OS_AssignBackButton(1);
										if ((ds.g_Pad.edge() & 3) != 0 || ds.g_TouchPanel.isTouch())
										{
											dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
											dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
											nowFading_ = true;
										}
									}
									else if (nowFading_ && dgs.CFade.Main().isFaded())
									{
										setPhase(PHASE.END);
									}
								}

								public override void end(CBaseSystem _sys)
								{
									_sys.World2DMng().refWorldMap().finalizeWMap();
									_sys.World2DMng().OnePicture().cleanup();
									GXS_SetVisiblePlane(GXS_GetVisiblePlane() & -9);
									NNS_G2dBGSetupCell(7, null, NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB3);
									_sys.World2DMng().refWorldMap().hideMapMarker();
									MapMarkerUpdater.getSingleton().resetAccepter();
									menu.MenuManager.getSingleton().Set2d3dMode(2);
									menu.MenuManager.getSingleton().loadSprite();
									GX_Power3D(1);
									nowFading_ = false;
									_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_END);
								}

								public override bool canExecuteEvent(CBaseSystem arg0)
								{
									return false;
								}
							}
	}
}
