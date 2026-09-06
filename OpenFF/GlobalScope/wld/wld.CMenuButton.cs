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
							public class CMenuButton
							{
								public enum MENU_2D3D
								{
									MENU_2D,
									MENU_3D
								}

								public enum MENU_POSITION
								{
									MENU_POSITION_RIGHT_UP,
									MENU_POSITION_RIGHT_DOWN,
									MENU_POSITION_LEFT_UP,
									MENU_POSITION_LEFT_DOWN,
									MENU_POSITION_MAX
								}

								public enum MENU_PANEL_ANIM
								{
									MENU_CALL_BUTTON,
									MENU_PANEL_SHADOW,
									MENU_PANEL_BASE,
									MENU_PANEL_LBUTTON,
									MENU_PANEL_RBUTTON,
									MENU_PANEL_XBUTTON,
									MENU_PANEL_BBUTTON,
									MENU_PANEL_BASE_PUSHED,
									MENU_PANEL_LBUTTON_PUSHED,
									MENU_PANEL_RBUTTON_PUSHED,
									MENU_PANEL_XBUTTON_PUSHED,
									MENU_PANEL_BBUTTON_PUSHED,
									MENU_PANEL_QUICK,
									MENU_CAMERA_BUTTON,
									MENU_TALK_BUTTON,
									MENU_PANEL_ANIM_MAX
								}

								public enum STATE
								{
									STATE_SHOW,
									STATE_HIDE,
									STATE_SHOWING1,
									STATE_SHOWING2,
									STATE_HIDDING
								}

								public const MENU_2D3D MENU_2D = MENU_2D3D.MENU_2D;

								public const MENU_2D3D MENU_3D = MENU_2D3D.MENU_3D;

								public const MENU_POSITION MENU_POSITION_RIGHT_UP = MENU_POSITION.MENU_POSITION_RIGHT_UP;

								public const MENU_POSITION MENU_POSITION_RIGHT_DOWN = MENU_POSITION.MENU_POSITION_RIGHT_DOWN;

								public const MENU_POSITION MENU_POSITION_LEFT_UP = MENU_POSITION.MENU_POSITION_LEFT_UP;

								public const MENU_POSITION MENU_POSITION_LEFT_DOWN = MENU_POSITION.MENU_POSITION_LEFT_DOWN;

								public const MENU_POSITION MENU_POSITION_MAX = MENU_POSITION.MENU_POSITION_MAX;

								public const MENU_PANEL_ANIM MENU_CALL_BUTTON = MENU_PANEL_ANIM.MENU_CALL_BUTTON;

								public const MENU_PANEL_ANIM MENU_PANEL_SHADOW = MENU_PANEL_ANIM.MENU_PANEL_SHADOW;

								public const MENU_PANEL_ANIM MENU_PANEL_BASE = MENU_PANEL_ANIM.MENU_PANEL_BASE;

								public const MENU_PANEL_ANIM MENU_PANEL_LBUTTON = MENU_PANEL_ANIM.MENU_PANEL_LBUTTON;

								public const MENU_PANEL_ANIM MENU_PANEL_RBUTTON = MENU_PANEL_ANIM.MENU_PANEL_RBUTTON;

								public const MENU_PANEL_ANIM MENU_PANEL_XBUTTON = MENU_PANEL_ANIM.MENU_PANEL_XBUTTON;

								public const MENU_PANEL_ANIM MENU_PANEL_BBUTTON = MENU_PANEL_ANIM.MENU_PANEL_BBUTTON;

								public const MENU_PANEL_ANIM MENU_PANEL_BASE_PUSHED = MENU_PANEL_ANIM.MENU_PANEL_BASE_PUSHED;

								public const MENU_PANEL_ANIM MENU_PANEL_LBUTTON_PUSHED = MENU_PANEL_ANIM.MENU_PANEL_LBUTTON_PUSHED;

								public const MENU_PANEL_ANIM MENU_PANEL_RBUTTON_PUSHED = MENU_PANEL_ANIM.MENU_PANEL_RBUTTON_PUSHED;

								public const MENU_PANEL_ANIM MENU_PANEL_XBUTTON_PUSHED = MENU_PANEL_ANIM.MENU_PANEL_XBUTTON_PUSHED;

								public const MENU_PANEL_ANIM MENU_PANEL_BBUTTON_PUSHED = MENU_PANEL_ANIM.MENU_PANEL_BBUTTON_PUSHED;

								public const MENU_PANEL_ANIM MENU_PANEL_QUICK = MENU_PANEL_ANIM.MENU_PANEL_QUICK;

								public const MENU_PANEL_ANIM MENU_CAMERA_BUTTON = MENU_PANEL_ANIM.MENU_CAMERA_BUTTON;

								public const MENU_PANEL_ANIM MENU_TALK_BUTTON = MENU_PANEL_ANIM.MENU_TALK_BUTTON;

								public const MENU_PANEL_ANIM MENU_PANEL_ANIM_MAX = MENU_PANEL_ANIM.MENU_PANEL_ANIM_MAX;

								public const STATE STATE_SHOW = STATE.STATE_SHOW;

								public const STATE STATE_HIDE = STATE.STATE_HIDE;

								public const STATE STATE_SHOWING1 = STATE.STATE_SHOWING1;

								public const STATE STATE_SHOWING2 = STATE.STATE_SHOWING2;

								public const STATE STATE_HIDDING = STATE.STATE_HIDDING;

								private bool _show;

								private MENU_2D3D _Menu2d3d;

								private bool _isTouch;

								private bool _isEdgeAndRepeatTouch;

								private MENU_PANEL_ANIM _MenuPanelAnim;

								private ds.Vector2<short> _touchXY = new ds.Vector2<short>();

								private ds.Vector2<short> _touchHW = new ds.Vector2<short>();

								private ds.Vector2<short> _size = new ds.Vector2<short>();

								private sys2d.Cell _cell2d = new sys2d.Cell();

								private sys2d.Sprite3d _cell3d = new sys2d.Sprite3d();

								private int _waitFrame;

								private STATE state_;

								private sbyte frame_;

								public void setup(MENU_2D3D Menu2d3d, MENU_POSITION MenuPosition, MENU_PANEL_ANIM MenuPanelAnim, short width, short height)
								{
									_Menu2d3d = Menu2d3d;
									_MenuPanelAnim = MenuPanelAnim;
									_isTouch = false;
									_isEdgeAndRepeatTouch = false;
									_size.vx = width;
									_size.vy = height;
									changeCompanyDirectory();
									getCell().Release();
									getCell().Load2((_Menu2d3d == MENU_2D3D.MENU_2D) ? sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D : sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "m009_menubutton_i");
									sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(getCell());
									getCell().SetCell((ushort)_MenuPanelAnim);
									setPosition(MenuPosition);
									getCell().SetShow(show: false);
									_show = false;
									state_ = STATE.STATE_HIDE;
									getCell().SetAlpha(0);
									getCell().SetShow(show: false);
								}

								public void create()
								{
									_show = true;
									getCell().SetAlpha(0);
									state_ = STATE.STATE_HIDE;
								}

								public void execute(CWorld2DManager manager)
								{
									bool flag = false;
									int x = -1;
									int y = -1;
									if (manager.refMapNameWindow().isOpen())
									{
										getCell().SetShow(show: false);
										return;
									}
									flag = ds.g_TouchPanel.isEdge();
									dv.CDeviceManager.getInstance().Tp().TouchPanel_2d(out x, out y);
									byte b = 6;
									sbyte b2 = (sbyte)getCell().GetAlpha();
									switch (state_)
									{
									case STATE.STATE_SHOW:
										getCell().SetShow(show: true);
										break;
									case STATE.STATE_HIDE:
										getCell().SetShow(show: false);
										break;
									case STATE.STATE_SHOWING1:
										frame_--;
										if (frame_ <= 0)
										{
											b2 = 1;
											getCell().SetShow(show: true);
											state_ = STATE.STATE_SHOWING2;
										}
										break;
									case STATE.STATE_SHOWING2:
										b2 += (sbyte)b;
										if (b2 >= 31)
										{
											state_ = STATE.STATE_SHOW;
										}
										break;
									case STATE.STATE_HIDDING:
										b2 -= (sbyte)b;
										if (b2 <= 0)
										{
											state_ = STATE.STATE_HIDE;
										}
										break;
									}
									if (b2 > 31)
									{
										b2 = 31;
									}
									if (b2 <= 0)
									{
										b2 = 0;
										getCell().SetShow(show: false);
									}
									getCell().SetAlpha((byte)b2);
									if (!_show)
									{
										getCell().SetShow(_show);
										return;
									}
									_isTouch = false;
									if (_show && state_ != STATE.STATE_HIDE && flag && isButtonTouch(x, y))
									{
										_isTouch = true;
									}
									if (_MenuPanelAnim != MENU_PANEL_ANIM.MENU_CALL_BUTTON && _MenuPanelAnim != MENU_PANEL_ANIM.MENU_PANEL_SHADOW && _MenuPanelAnim != MENU_PANEL_ANIM.MENU_PANEL_QUICK && _MenuPanelAnim != MENU_PANEL_ANIM.MENU_CAMERA_BUTTON && _MenuPanelAnim != MENU_PANEL_ANIM.MENU_TALK_BUTTON)
									{
										getCell().SetCell((ushort)((!_isTouch) ? _MenuPanelAnim : (_MenuPanelAnim + 5)));
									}
								}

								public void erase()
								{
									_show = false;
								}

								public void cleanup()
								{
									getCell().Release();
									sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(getCell());
								}

								public bool isButtonTouch(int x, int y)
								{
									if (_touchXY.vx <= x && x <= _touchXY.vx + _touchHW.vx && _touchXY.vy <= y && y <= _touchXY.vy + _touchHW.vy)
									{
										return true;
									}
									return false;
								}

								public void setStateShow()
								{
									if (STATE.STATE_HIDDING == state_)
									{
										state_ = STATE.STATE_SHOWING2;
										return;
									}
									frame_ = 10;
									state_ = STATE.STATE_SHOWING1;
								}

								public void setStateHide()
								{
									state_ = STATE.STATE_HIDDING;
								}

								public void setPosition(MENU_POSITION menuPos)
								{
									if (isIPad())
									{
										switch (menuPos)
										{
										case MENU_POSITION.MENU_POSITION_RIGHT_UP:
											getCell().SetPositionI(428, 48);
											break;
										case MENU_POSITION.MENU_POSITION_RIGHT_DOWN:
											getCell().SetPositionI(428, 92);
											break;
										case MENU_POSITION.MENU_POSITION_LEFT_UP:
											getCell().SetPositionI(428, 4);
											break;
										case MENU_POSITION.MENU_POSITION_LEFT_DOWN:
											getCell().SetPositionI(428, 136);
											break;
										}
									}
									else
									{
										switch (menuPos)
										{
										case MENU_POSITION.MENU_POSITION_RIGHT_UP:
											getCell().SetPositionI(412, 4);
											break;
										case MENU_POSITION.MENU_POSITION_RIGHT_DOWN:
											getCell().SetPositionI(412, 48);
											break;
										case MENU_POSITION.MENU_POSITION_LEFT_UP:
											getCell().SetPositionI(344, 4);
											break;
										case MENU_POSITION.MENU_POSITION_LEFT_DOWN:
											getCell().SetPositionI(344, 48);
											break;
										}
									}
									_touchXY.set(getCell().GetPositionI().x, getCell().GetPositionI().y);
									_touchHW.set(_size.vx, _size.vy);
								}

								public void setShow(bool show)
								{
									_show = show;
								}

								public void setMenuPanelAnim(MENU_PANEL_ANIM MenuPanelAnim)
								{
									_MenuPanelAnim = MenuPanelAnim;
								}

								public void setTouchXY(ds.Vector2<short> touchXY)
								{
									_touchXY.copy(touchXY);
								}

								public void setTouchHW(ds.Vector2<short> touchHW)
								{
									_touchHW.copy(touchHW);
								}

								public bool isTouch()
								{
									return _isTouch;
								}

								public bool isEdgeAndRepeatTouch()
								{
									return _isEdgeAndRepeatTouch;
								}

								public void setWaitFrame(int waitFrame)
								{
									_waitFrame = waitFrame;
								}

								private sys2d.Sprite getCell()
								{
									if (_Menu2d3d != MENU_2D3D.MENU_2D)
									{
										return static_cast<sys2d.Sprite>(_cell3d);
									}
									return static_cast<sys2d.Sprite>(_cell2d);
								}
							}
	}
}
