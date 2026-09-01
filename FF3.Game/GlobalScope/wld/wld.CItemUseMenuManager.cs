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
							public class CItemUseMenuManager
							{
								public enum ITEM_USE_STATE
								{
									ITEM_USE_STATE_WAIT,
									ITEM_USE_STATE_APPEAR,
									ITEM_USE_STATE_EXECUTE,
									ITEM_USE_STATE_DISAPPEAR,
									ITEM_USE_STATE_MAX
								}

								public const ITEM_USE_STATE ITEM_USE_STATE_WAIT = ITEM_USE_STATE.ITEM_USE_STATE_WAIT;

								public const ITEM_USE_STATE ITEM_USE_STATE_APPEAR = ITEM_USE_STATE.ITEM_USE_STATE_APPEAR;

								public const ITEM_USE_STATE ITEM_USE_STATE_EXECUTE = ITEM_USE_STATE.ITEM_USE_STATE_EXECUTE;

								public const ITEM_USE_STATE ITEM_USE_STATE_DISAPPEAR = ITEM_USE_STATE.ITEM_USE_STATE_DISAPPEAR;

								public const ITEM_USE_STATE ITEM_USE_STATE_MAX = ITEM_USE_STATE.ITEM_USE_STATE_MAX;

								private int m_WindowID;

								private ITEM_USE_STATE m_State;

								public void setup()
								{
									m_WindowID = -1;
									m_State = ITEM_USE_STATE.ITEM_USE_STATE_WAIT;
									menu.MenuManager.getSingleton().Set2d3dMode(3);
									menu.MenuManager.getSingleton().CreateNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
									menu.MenuManager.getSingleton().GetCursor3d().SetShow(show: false);
								}

								public void create()
								{
									m_WindowID = -1;
									m_State = ITEM_USE_STATE.ITEM_USE_STATE_APPEAR;
									menu.MenuManager.getSingleton().Set2d3dMode(3);
									menu.MenuManager.getSingleton().SetItemListPatern(0);
									m_WindowID = menu.MenuManager.getSingleton().buildWindow(TRANSCODE("item_use_list"), TRANSCODE("item_use_window"));
								}

								public void erase()
								{
									m_State = ITEM_USE_STATE.ITEM_USE_STATE_DISAPPEAR;
									menu.MenuManager.getSingleton().Set2d3dMode(3);
									menu.MenuManager.getSingleton().release();
									menu.MenuManager.getSingleton().GetCursor3d().SetShow(show: false);
								}

								public void execute()
								{
									bool flag = true;
									switch (m_State)
									{
									case ITEM_USE_STATE.ITEM_USE_STATE_APPEAR:
										if (!menu.MenuManager.getSingleton().UpdateWindowState(m_WindowID, menu.MenuWindow.MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_LARGE))
										{
											m_State = ITEM_USE_STATE.ITEM_USE_STATE_EXECUTE;
											menu.MenuManager.getSingleton().buildMenu(TRANSCODE("item_use_list"));
											menu.MenuManager.getSingleton().GetCursor3d().SetShow(show: true);
											menu.MenuManager.getSingleton().GetMenuWindowObj()[m_WindowID].GetWindowHandle().SetBar(1);
										}
										break;
									case ITEM_USE_STATE.ITEM_USE_STATE_EXECUTE:
										OS_AssignBackButton(1);
										menu.MenuManager.getSingleton().execute();
										break;
									case ITEM_USE_STATE.ITEM_USE_STATE_DISAPPEAR:
										if (!menu.MenuManager.getSingleton().UpdateWindowState(m_WindowID, menu.MenuWindow.MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_SMALL))
										{
											m_State = ITEM_USE_STATE.ITEM_USE_STATE_WAIT;
											if (m_WindowID != -1)
											{
												menu.MenuManager.getSingleton().releaseWindow(m_WindowID);
												m_WindowID = -1;
											}
										}
										break;
									case ITEM_USE_STATE.ITEM_USE_STATE_WAIT:
										break;
									}
								}

								public void cleanup()
								{
									if (m_WindowID != -1)
									{
										menu.MenuManager.getSingleton().releaseWindow(m_WindowID);
										m_WindowID = -1;
									}
									m_State = ITEM_USE_STATE.ITEM_USE_STATE_WAIT;
								}

								public bool isItemMenu()
								{
									if (m_State == ITEM_USE_STATE.ITEM_USE_STATE_WAIT)
									{
										return false;
									}
									return true;
								}
							}
	}
}
