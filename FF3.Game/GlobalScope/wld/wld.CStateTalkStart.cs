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
							public class CStateTalkStart : CBaseState
							{
								public override void start(CBaseSystem _sys)
								{
									GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_SUB_MAIN);
									G3X_SetClearColor(GX_RGB(0, 0, 0), 1, 32767, 1, 0);
									dv.CDeviceManager.getInstance().initialize();
									_sys.PlayerMng().initialize();
									dgs.msg.CMessageSys.getInstance().Main().assignBG(3, 0, 0, 32, 24);
									_sys.setupCamera();
									_sys.setUpEventData();
									_sys.setUpNpcParameter();
									int num = 0;
									CBaseSystem.CONTENT cONTENT = new CBaseSystem.CONTENT();
									num = ((!_sys.getContent(1414551379u, cONTENT)) ? ((int)CWorldOutSideData.getInstance().MapData().BattleMapIndex()) : ((int)cONTENT.DATA.L));
									sprintf(out var arg, "b%02d", num);
									sprintf(out var _, "%s.nmdp.lz", arg);
									_sys.setupStage(arg, CBaseSystem.WORLD_MODE.WORLD_MODE_TALK);
									if (num == 22)
									{
										_sys.WorldCamera().setClip(40960, 4096000);
									}
									_sys.setUpMapParameter();
									_sys.setUpPcParameter();
									CWorldOutSideData.getInstance().SoundData().setSoundFlag(CWorldOutSideData.getInstance().SoundData().getSoundFlag() & -2);
									ds.CVram.setMainPlaneVisiblity(bg0: true, bg1: true, bg2: true, bg3: true, obj: true);
									dgs.CFade.Main().fadeIn(20);
									dgs.CFade.Sub().fadeIn(20);
								}

								public override void update(CBaseSystem _sys)
								{
									if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
									{
										setPhase(PHASE.END);
									}
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
