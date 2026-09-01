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
	public static partial class wld
	{
							public class CStateSiteStart : CBaseState
							{
								public override void start(CBaseSystem _sys)
								{
									menu.MenuManager.getSingleton().Set2d3dMode(2);
									menu.MenuManager.getSingleton().releaseSprite();
									_sys.WorldCamera().composit.setZoomEnable(b: false);
									_sys.World2DMng().refWorldMap().finalizeWMap();
									_sys.World2DMng().OnePicture().cleanup();
									sys2d.NCDataManager.GetNCDataManager().dumpDebugInfo();
									if (_sys.IsAreaMap())
									{
										_sys.World2DMng().refWorldMap().initializeWMap();
									}
									else
									{
										int progress = 9;
										string stage = sceneMng.getStage();
										if (strncmp(stage, "f02", 3) == 0)
										{
											progress = 4;
										}
										_sys.World2DMng().refWorldMap().initializeWMap(progress, forceEnable: true);
									}
									_sys.World2DMng().OnePicture().setup(NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB3, _sys.World2DMng().refWorldMap().getMapFileName());
									_sys.World2DMng().refWorldMap().showMapMarker();
									GX_Power3D(0);
									MapMarkerUpdater.getSingleton().resetAccepter();
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
									_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_MOVE);
									ds.g_Pad.enable();
									ds.g_TouchPanel.enable();
								}

								public override bool canExecuteEvent(CBaseSystem arg0)
								{
									return false;
								}
							}
	}
}
