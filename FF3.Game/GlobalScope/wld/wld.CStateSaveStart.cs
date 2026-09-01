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
							public class CStateSaveStart : CBaseState
							{
								public enum CSTATESAVESTART
								{
									STEP0,
									STEP1,
									STEP2
								}

								public const CSTATESAVESTART STEP0 = CSTATESAVESTART.STEP0;

								public const CSTATESAVESTART STEP1 = CSTATESAVESTART.STEP1;

								public const CSTATESAVESTART STEP2 = CSTATESAVESTART.STEP2;

								private int step_;

								public override void start(CBaseSystem _sys)
								{
									dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
									dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
									ds.g_Pad.disable();
									ds.g_TouchPanel.disable();
									evt.CEventManager.getInstance().setEventStop(_EventStop: true);
									step_ = 0;
								}

								public override void update(CBaseSystem _sys)
								{
									switch (step_)
									{
									case 0:
										if (dgs.CFade.Sub().isFaded() && dgs.CFade.Main().isFaded())
										{
											step_ = 1;
										}
										break;
									case 1:
									{
										GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_MAIN_SUB);
										step_ = 2;
										dgs.CFade.Sub().fadeIn(15);
										dgs.CFade.Main().fadeIn(15);
										changeCompanyDirectory();
										menu.MenuManager.getSingleton().Set2d3dMode(2);
										menu.MenuManager.getSingleton().LoadXbnFile("MenuDefine.xbn");
										menu.MenuManager.getSingleton().CreateNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_2D);
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
										menu.MenuManager.getSingleton().SetUsingMenuType(1);
										menu.MenuManager.getSingleton().CreateItemDataText();
										menu.MenuManager.getSingleton().CreateMenuDataText(0);
										wmenu.CWMenuManager.Instance().SetStartupKind(wmenu.CWMenuMemberBase.WMENU_KIND.WMENU_KIND_SAVE);
										wmenu.CWMenuManager.Instance().initialize();
										wmenu.CWMenuManager.Instance().SetPrimaryBG(8);
										wmenu.CWMenuSave cWMenuSave = (wmenu.CWMenuSave)wmenu.CWMenuManager.Instance().pCurrent();
										cWMenuSave.endingSaveSetting(b: true);
										MatrixSound.MtxSENDS_Load(98);
										break;
									}
									case 2:
										if (dgs.CFade.Sub().isCleared())
										{
											_sys.CrtState().phase_set(PHASE.END);
										}
										break;
									}
								}

								public override void end(CBaseSystem _sys)
								{
									ds.g_Pad.enable();
									ds.g_TouchPanel.enable();
									_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_MOVE);
								}

								public override bool canExecuteEvent(CBaseSystem arg0)
								{
									return false;
								}
							}
	}
}
