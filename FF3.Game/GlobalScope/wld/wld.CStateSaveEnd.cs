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
							public class CStateSaveEnd : CBaseState
							{
								public enum CSTATESAVEEND
								{
									STEP0,
									STEP1
								}

								public const CSTATESAVEEND STEP0 = CSTATESAVEEND.STEP0;

								public const CSTATESAVEEND STEP1 = CSTATESAVEEND.STEP1;

								private int step_;

								public override void start(CBaseSystem _sys)
								{
									dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
									dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
									ds.g_Pad.disable();
									ds.g_TouchPanel.disable();
									wmenu.CWMenuManager.Instance().terminate();
									menu.MenuManager.getSingleton().ReleaseItemDataText();
									menu.MenuManager.getSingleton().ReleaseMenuDataText();
									menu.MenuManager.getSingleton().ReleaseXbnFile();
									menu.MenuManager.getSingleton().DeleteNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
									menu.MenuManager.getSingleton().Set2d3dMode(3);
									menu.MenuManager.getSingleton().release();
									step_ = 0;
								}

								public override void update(CBaseSystem _sys)
								{
									if (step_ == 0 && dgs.CFade.Sub().isFaded() && dgs.CFade.Main().isFaded())
									{
										GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_SUB_MAIN);
										_sys.World2DMng().initialize();
										_sys.CrtState().phase_set(PHASE.END);
									}
								}

								public override void end(CBaseSystem _sys)
								{
									dv.CDeviceManager.getInstance().Pad().setActivity(b: true);
									ds.g_Pad.enable();
									ds.g_TouchPanel.enable();
									evt.CEventManager.getInstance().setEventStop(_EventStop: false);
									_sys.setMode(_sys.PreviousMode());
									_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_MOVE);
									_sys.CrtState().phase_set(PHASE.UPDATE);
									_sys.setSave(b: false);
									MatrixSound.MtxSENDS_Unload();
								}

								public override bool canExecuteEvent(CBaseSystem arg0)
								{
									return false;
								}
							}
	}
}
