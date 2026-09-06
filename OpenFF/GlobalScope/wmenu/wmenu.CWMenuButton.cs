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
	public static partial class wmenu
	{
							public class CWMenuButton
							{
								public class MBUTTON
								{
									public sys2d.Cell cell = new sys2d.Cell();

									public ds.Vector2<short> pos = new ds.Vector2<short>();
								}

								public const int BUTTON_A = 0;

								public const int BUTTON_B = 1;

								public const int BUTTON_L = 2;

								public const int BUTTON_R = 3;

								public const int MENU_BUTTON_MAX = 4;

								private dgs.SmartPtr<dgs.DGSMessage> pAString = new dgs.SmartPtr<dgs.DGSMessage>();

								private dgs.SmartPtr<dgs.DGSMessage> pBString = new dgs.SmartPtr<dgs.DGSMessage>();

								private MBUTTON[] button = new MBUTTON[4];

								private int currentButton;

								public void initialize()
								{
									for (int i = 0; i < 4; i++)
									{
										button[i].pos.vx = (short)(Cell_Pos[i][0] - 24);
										button[i].pos.vy = (short)Cell_Pos[i][1];
										button[i].cell.copy(menu.MenuManager.getSingleton().GetMenuButtonIcon2d());
										button[i].cell.SetCell((ushort)cellType[i]);
										button[i].cell.SetPriority(1);
										button[i].cell.SetShow(show: false);
										button[i].cell.SetPositionI(button[i].pos.vx, button[i].pos.vy);
										sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(button[i].cell);
									}
									button[0].cell.SetShow(show: false);
									button[1].cell.SetShow(show: true);
									button[0].cell.SetAnimation(anm: false);
									button[1].cell.SetAnimation(anm: false);
									createXAndBMessage();
									currentButton = 4;
								}

								public void terminate()
								{
									for (int i = 0; i < 4; i++)
									{
										sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(button[i].cell);
										button[i].cell.Release();
									}
									releaseXAndBMessage();
								}

								public bool HitArea(int x, int y, int sx, int sy, int w, int h)
								{
									if (x > sx && x < sx + w && y > sy && y < sy + h)
									{
										return true;
									}
									return false;
								}

								public bool TouchButtonA()
								{
									if (!button[0].cell.IsShow())
									{
										return false;
									}
									ds.Vector2<short> pos = button[0].pos;
									ds.g_TouchPanel.getPoint(out var x, out var y);
									if (!ds.g_TouchPanel.isTouch() && currentButton == 0)
									{
										if (pAString != null)
										{
											pAString.get().setPosition(pos.vx, pos.vy, erase: true);
										}
										if (!ds.g_TouchPanel.isRelease())
										{
											currentButton = 4;
										}
										else if (HitArea(x, y, Cell_Pos[0][0] - Cell_Pos[0][2], Hit_Y, Cell_Pos[0][2] * 2, Hit_Height))
										{
											menu.MenuManager.getSingleton().playSEDecide();
											return true;
										}
									}
									else if (ds.g_TouchPanel.isEdge() && HitArea(x, y, Cell_Pos[0][0] - Cell_Pos[0][2], Hit_Y, Cell_Pos[0][2] * 2, Hit_Height))
									{
										if (pAString != null)
										{
											pAString.get().setPosition((short)(pos.vx + 1), (short)(pos.vy + 1), erase: true);
										}
										currentButton = 0;
									}
									return false;
								}

								public bool TouchButtonB()
								{
									if (!button[1].cell.IsShow())
									{
										return false;
									}
									ds.Vector2<short> pos = button[1].pos;
									ds.g_TouchPanel.getPoint(out var x, out var y);
									if (!ds.g_TouchPanel.isTouch() && currentButton == 1)
									{
										if (pBString != null)
										{
											pBString.get().setPosition(pos.vx, pos.vy, erase: true);
										}
										if (!ds.g_TouchPanel.isRelease())
										{
											currentButton = 4;
										}
										else if (HitArea(x, y, Cell_Pos[1][0] - Cell_Pos[1][2], Hit_Y, Cell_Pos[1][2] * 2, Hit_Height))
										{
											menu.MenuManager.getSingleton().playSECancel();
											return true;
										}
									}
									else if (ds.g_TouchPanel.isEdge() && HitArea(x, y, Cell_Pos[1][0] - Cell_Pos[1][2], Hit_Y, Cell_Pos[1][2] * 2, Hit_Height))
									{
										if (pBString != null)
										{
											pBString.get().setPosition((short)(pos.vx + 1), (short)(pos.vy + 1), erase: true);
										}
										currentButton = 1;
									}
									return false;
								}

								public bool TouchButtonL()
								{
									if (!button[2].cell.IsShow())
									{
										return false;
									}
									ds.Vector2<short> pos = button[2].pos;
									ds.g_TouchPanel.getPoint(out var x, out var y);
									if (!ds.g_TouchPanel.isTouch() && currentButton == 2)
									{
										button[2].cell.SetPositionI(pos.vx, pos.vy);
										if (!ds.g_TouchPanel.isRelease())
										{
											currentButton = 4;
										}
										else if (HitArea(x, y, Cell_Pos[2][0] - Cell_Pos[2][2], Hit_Y, Cell_Pos[2][2] * 2, Hit_Height))
										{
											return true;
										}
									}
									else if (ds.g_TouchPanel.isEdge() && HitArea(x, y, Cell_Pos[2][0] - Cell_Pos[2][2], Hit_Y, Cell_Pos[2][2] * 2, Hit_Height))
									{
										button[2].cell.SetPositionI(pos.vx + 1, pos.vy + 1);
										currentButton = 2;
									}
									if (ds.g_TouchPanel.getDispPoint().flickOffsetH < 0)
									{
										return true;
									}
									return false;
								}

								public bool TouchButtonR()
								{
									if (!button[3].cell.IsShow())
									{
										return false;
									}
									ds.Vector2<short> pos = button[3].pos;
									ds.g_TouchPanel.getPoint(out var x, out var y);
									if (!ds.g_TouchPanel.isTouch() && currentButton == 3)
									{
										button[3].cell.SetPositionI(pos.vx, pos.vy);
										if (!ds.g_TouchPanel.isRelease())
										{
											currentButton = 4;
										}
										else if (HitArea(x, y, Cell_Pos[3][0] - Cell_Pos[3][2], Hit_Y, Cell_Pos[3][2] * 2, Hit_Height))
										{
											return true;
										}
									}
									else if (ds.g_TouchPanel.isEdge() && HitArea(x, y, Cell_Pos[3][0] - Cell_Pos[3][2], Hit_Y, Cell_Pos[3][2] * 2, Hit_Height))
									{
										button[3].cell.SetPositionI(pos.vx + 1, pos.vy + 1);
										currentButton = 3;
									}
									if (ds.g_TouchPanel.getDispPoint().flickOffsetH > 0)
									{
										return true;
									}
									return false;
								}

								public void createXAndBMessage()
								{
									dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
									pAString.release();
									pBString.release();
									pAString = new dgs.SmartPtr<dgs.DGSMessage>(dGSMessageManager.createMessage(50018u, dgs.INVALID_MSDHANDLE, 1));
									if (pAString != null)
									{
										pAString.get().setDisplaySpeed(byte.MaxValue);
										pAString.get().setDisplayWait(0);
										pAString.get().setVisibility(b: false);
										ds.Vector2<short> vector = new ds.Vector2<short>();
										pAString.get().getTextSize(vector);
										button[0].pos.vx = (short)(Cell_Pos[0][0] - vector.vx / 2);
										button[0].pos.vy = (short)(Cell_Pos[0][1] + 10);
										pAString.get().setPosition(button[0].pos.vx, button[0].pos.vy, erase: true);
									}
									pBString = new dgs.SmartPtr<dgs.DGSMessage>(dGSMessageManager.createMessage(50021u, dgs.INVALID_MSDHANDLE, 1));
									if (pBString != null)
									{
										pBString.get().setDisplaySpeed(byte.MaxValue);
										pBString.get().setDisplayWait(0);
										ds.Vector2<short> vector2 = new ds.Vector2<short>();
										pBString.get().getTextSize(vector2);
										button[1].pos.vx = (short)(Cell_Pos[1][0] - vector2.vx / 2);
										button[1].pos.vy = (short)(Cell_Pos[1][1] + 10);
										pBString.get().setPosition(button[1].pos.vx, button[1].pos.vy, erase: true);
									}
								}

								public void releaseXAndBMessage()
								{
									pAString.release();
									pBString.release();
								}

								public void SetButtonAActivity(bool b)
								{
									button[0].cell.SetShow(b);
									if (pAString != null)
									{
										pAString.get().setPosition(button[0].pos.vx, button[0].pos.vy, erase: true);
										pAString.get().setVisibility(b);
									}
								}

								public void SetButtonBActivity(bool b)
								{
									button[1].cell.SetShow(b);
									if (pBString != null)
									{
										pBString.get().setPosition(button[1].pos.vx, button[1].pos.vy, erase: true);
										pBString.get().setVisibility(b);
									}
								}

								public void SetButtonLActivity(bool b)
								{
									button[2].cell.SetShow(b);
								}

								public void SetButtonRActivity(bool b)
								{
									button[3].cell.SetShow(b);
								}

								public void SetUpNormalVer()
								{
									button[3].cell.SetShow(show: true);
									button[3].cell.SetAnimation(anm: false);
									button[2].cell.SetShow(show: true);
									button[2].cell.SetAnimation(anm: false);
								}

								public void SetUpSpecialVer()
								{
									button[3].cell.SetShow(show: false);
									button[3].cell.SetAnimation(anm: false);
									button[2].cell.SetShow(show: false);
									button[2].cell.SetAnimation(anm: false);
								}

								public CWMenuButton()
								{
									for (int i = 0; i < button.Length; i++)
									{
										button[i] = new MBUTTON();
									}
								}
							}
	}
}
