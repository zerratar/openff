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

								// PORT: where each button is and a press reaches - the field_hud layout's a_button, b_button, l_button,
								// r_button (the game's numbers unless a mod moved them) -, and whether the game shows it (its sprite
								// may be taken away under a picture of the layout's, so its showing is kept here).
								private OpenFF.Client.BattleHud.Rect[] hit_ = new OpenFF.Client.BattleHud.Rect[4];

								private bool[] shown_ = new bool[4];

								private void Shown(int i, bool b)
								{
									shown_[i] = b;
									OpenFF.Client.FieldHud.MenuButtonShown(i, b);
								}

								public void initialize()
								{
									OpenFF.Client.ModMenus.PrepareFieldHud();
									for (int i = 0; i < 4; i++)
									{
										hit_[i] = OpenFF.Client.BattleHud.MenuButton(i, Cell_Pos[i][0] - Cell_Pos[i][2], Hit_Y, Cell_Pos[i][2] * 2, Hit_Height);
										button[i].pos.vx = (short)(hit_[i].X + hit_[i].Width / 2 - 24);
										button[i].pos.vy = (short)(hit_[i].Y + Cell_Pos[i][1] - Hit_Y);
										button[i].cell.copy(menu.MenuManager.getSingleton().GetMenuButtonIcon2d());
										button[i].cell.SetCell((ushort)cellType[i]);
										button[i].cell.SetPriority(1);
										button[i].cell.SetShow(show: false);
										button[i].cell.SetPositionI(button[i].pos.vx, button[i].pos.vy);
										sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(button[i].cell);
										OpenFF.Client.FieldHud.MenuButtonMade(i, button[i].cell);
									}
									button[0].cell.SetShow(show: false);
									button[1].cell.SetShow(show: true);
									button[0].cell.SetAnimation(anm: false);
									button[1].cell.SetAnimation(anm: false);
									createXAndBMessage();
									Shown(0, false);
									Shown(1, true);
									currentButton = 4;
								}

								public void terminate()
								{
									for (int i = 0; i < 4; i++)
									{
										OpenFF.Client.FieldHud.MenuButtonGone(i);
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
									if (!shown_[0])
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
										else if (HitArea(x, y, hit_[0].X, hit_[0].Y, hit_[0].Width, hit_[0].Height))
										{
											menu.MenuManager.getSingleton().playSEDecide();
											return true;
										}
									}
									else if (ds.g_TouchPanel.isEdge() && HitArea(x, y, hit_[0].X, hit_[0].Y, hit_[0].Width, hit_[0].Height))
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
									if (!shown_[1])
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
										else if (HitArea(x, y, hit_[1].X, hit_[1].Y, hit_[1].Width, hit_[1].Height))
										{
											menu.MenuManager.getSingleton().playSECancel();
											return true;
										}
									}
									else if (ds.g_TouchPanel.isEdge() && HitArea(x, y, hit_[1].X, hit_[1].Y, hit_[1].Width, hit_[1].Height))
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
									if (!shown_[2])
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
										else if (HitArea(x, y, hit_[2].X, hit_[2].Y, hit_[2].Width, hit_[2].Height))
										{
											return true;
										}
									}
									else if (ds.g_TouchPanel.isEdge() && HitArea(x, y, hit_[2].X, hit_[2].Y, hit_[2].Width, hit_[2].Height))
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
									if (!shown_[3])
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
										else if (HitArea(x, y, hit_[3].X, hit_[3].Y, hit_[3].Width, hit_[3].Height))
										{
											return true;
										}
									}
									else if (ds.g_TouchPanel.isEdge() && HitArea(x, y, hit_[3].X, hit_[3].Y, hit_[3].Width, hit_[3].Height))
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
										button[0].pos.vx = (short)(hit_[0].X + hit_[0].Width / 2 - vector.vx / 2);
										button[0].pos.vy = (short)(hit_[0].Y + Cell_Pos[0][1] - Hit_Y + 10);
										pAString.get().setPosition(button[0].pos.vx, button[0].pos.vy, erase: true);
										OpenFF.Client.FieldHud.MenuButtonText(0, pAString.get());
									}
									pBString = new dgs.SmartPtr<dgs.DGSMessage>(dGSMessageManager.createMessage(50021u, dgs.INVALID_MSDHANDLE, 1));
									if (pBString != null)
									{
										pBString.get().setDisplaySpeed(byte.MaxValue);
										pBString.get().setDisplayWait(0);
										ds.Vector2<short> vector2 = new ds.Vector2<short>();
										pBString.get().getTextSize(vector2);
										button[1].pos.vx = (short)(hit_[1].X + hit_[1].Width / 2 - vector2.vx / 2);
										button[1].pos.vy = (short)(hit_[1].Y + Cell_Pos[1][1] - Hit_Y + 10);
										pBString.get().setPosition(button[1].pos.vx, button[1].pos.vy, erase: true);
										OpenFF.Client.FieldHud.MenuButtonText(1, pBString.get());
									}
								}

								public void releaseXAndBMessage()
								{
									pAString.release();
									pBString.release();
								}

								/// <summary>PORT: whether button i (0 A, 1 B, 2 L, 3 R) is on, as last set - for a scene over the screen (the job change) to put back as it was. Its activity, not its sprite: a layout's look of the button hides the game's sprite.</summary>
								public bool IsButtonShown(int i)
								{
									return i >= 0 && i < shown_.Length && shown_[i];
								}

								public void SetButtonAActivity(bool b)
								{
									button[0].cell.SetShow(b);
									if (pAString != null)
									{
										pAString.get().setPosition(button[0].pos.vx, button[0].pos.vy, erase: true);
										pAString.get().setVisibility(b);
									}
									Shown(0, b);
								}

								public void SetButtonBActivity(bool b)
								{
									button[1].cell.SetShow(b);
									if (pBString != null)
									{
										pBString.get().setPosition(button[1].pos.vx, button[1].pos.vy, erase: true);
										pBString.get().setVisibility(b);
									}
									Shown(1, b);
								}

								public void SetButtonLActivity(bool b)
								{
									button[2].cell.SetShow(b);
									Shown(2, b);
								}

								public void SetButtonRActivity(bool b)
								{
									button[3].cell.SetShow(b);
									Shown(3, b);
								}

								public void SetUpNormalVer()
								{
									button[3].cell.SetShow(show: true);
									button[3].cell.SetAnimation(anm: false);
									button[2].cell.SetShow(show: true);
									button[2].cell.SetAnimation(anm: false);
									Shown(3, true);
									Shown(2, true);
								}

								public void SetUpSpecialVer()
								{
									button[3].cell.SetShow(show: false);
									button[3].cell.SetAnimation(anm: false);
									button[2].cell.SetShow(show: false);
									button[2].cell.SetAnimation(anm: false);
									Shown(3, false);
									Shown(2, false);
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
