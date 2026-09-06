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
							public class CConfirmWindow
							{
								public static int CONFIRMWND_MSG_YES = 50101;

								public static int CONFIRMWND_MSG_NO = 50102;

								public static int CONFIRMWND_MSG_CONFIRM = 1000007;

								public static int CONFIRMWND_OFFSET_MSG_X = 8;

								public static int CONFIRMWND_OFFSET_MSG_Y = 8;

								public static int CONFIRMWND_OFFSET_YES_X = 24;

								public static int CONFIRMWND_OFFSET_YES_Y = 28;

								public static int CONFIRMWND_OFFSET_NO_X = CONFIRMWND_OFFSET_YES_X;

								public static int CONFIRMWND_OFFSET_NO_Y = 44;

								public static int CONFIRMWND_WIDTH = 84;

								public static int CONFIRMWND_HEIGHT = 64;

								public static int CONFIRMWND_POS_X = 86;

								public static int CONFIRMWND_POS_Y = 120;

								private bool visiblity_;

								private int messageIDYes_;

								private int messageIDNo_;

								private int messageIDConfirm_;

								// PORT: the phone build never constructed this window (its inn confirm went another
								// way), so these were never made; the engine API's Ask uses it.
								private menu.BasicWindow window_ = new menu.BasicWindow();

								private sys2d.Sprite3d cellCursor3d_ = new sys2d.Sprite3d();

								public CConfirmWindow()
								{
									visiblity_ = false;
									messageIDYes_ = (messageIDNo_ = (messageIDConfirm_ = -1));
								}

								~CConfirmWindow()
								{
									releaseMessage(ref messageIDYes_);
									releaseMessage(ref messageIDNo_);
									releaseMessage(ref messageIDConfirm_);
									visiblity_ = false;
								}

								public void open()
								{
									ds.Vector2<short> vector = new ds.Vector2<short>(0, 0);
									vector.set((short)CONFIRMWND_POS_X, (short)CONFIRMWND_POS_Y);
									ds.Vector2<short> vector2 = new ds.Vector2<short>(0, 0);
									vector2.set((short)CONFIRMWND_WIDTH, (short)CONFIRMWND_HEIGHT);
									changeGlobalDirectory();
									window_.bwCreateUL(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, vector, vector2, 3);
									window_.SetPriority(3);
									window_.SetShow(show: true, user: true);
									releaseMessage(ref messageIDYes_);
									releaseMessage(ref messageIDNo_);
									releaseMessage(ref messageIDConfirm_);
									// PORT: Yes and No must exist; the "Confirm?" line above them is optional (Steam's
									// text has no entry for it, and the engine API puts its question in the message
									// window instead). The phone build never made this window at all.
									messageIDYes_ = dgs.msg.CMessageSys.getInstance().Main().createMessage((uint)CONFIRMWND_MSG_YES, (ushort)(vector.vx + CONFIRMWND_OFFSET_YES_X), (ushort)(vector.vy + CONFIRMWND_OFFSET_YES_Y), dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART2, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
									messageIDNo_ = dgs.msg.CMessageSys.getInstance().Main().createMessage((uint)CONFIRMWND_MSG_NO, (ushort)(vector.vx + CONFIRMWND_OFFSET_NO_X), (ushort)(vector.vy + CONFIRMWND_OFFSET_NO_Y), dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART2, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
									if (-1 == messageIDYes_ || -1 == messageIDNo_)
									{
										close();
										return;
									}
									messageIDConfirm_ = dgs.msg.CMessageSys.getInstance().Main().createMessage((uint)CONFIRMWND_MSG_CONFIRM, (ushort)(vector.vx + CONFIRMWND_OFFSET_MSG_X), (ushort)(vector.vy + CONFIRMWND_OFFSET_MSG_Y), dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART2, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
									foreach (int id in new[] { messageIDYes_, messageIDNo_, messageIDConfirm_ })
									{
										if (id == -1)
										{
											continue;
										}
										dgs.DGSMessage message = dgs.msg.CMessageSys.getInstance().Main().Message(id);
										if (message != null)
										{
											message.setDisplaySpeed(byte.MaxValue);
											message.setShadow(b: true);
										}
									}
									changeGlobalDirectory();
									cellCursor3d_.Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "icon_yubi");
									cellCursor3d_.SetShow(show: true);
									cellCursor3d_.SetCell(0);
									cellCursor3d_.SetDepth(0);
									cellCursor3d_.SetPositionI(CONFIRMWND_POS_X + CONFIRMWND_OFFSET_YES_X, CONFIRMWND_POS_Y + CONFIRMWND_OFFSET_YES_Y);
									sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(cellCursor3d_);
									visiblity_ = true;
								}
								public void close()
								{
									cellCursor3d_.Release();
									sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(cellCursor3d_);
									releaseMessage(ref messageIDYes_);
									releaseMessage(ref messageIDNo_);
									releaseMessage(ref messageIDConfirm_);
									window_.Release();
									visiblity_ = false;
								}

								public void update()
								{
								}

								public void setPos(int x, int y)
								{
									x = x;
									y = y;
								}

								/// <summary>PORT: whether the box is up.</summary>
								public bool isOpen()
								{
									return visiblity_;
								}

								/// <summary>PORT: which answer a tap at an LCD point lands on: 1 yes, 0 no, -1 neither.</summary>
								public int hitTest(int x, int y)
								{
									int left = CONFIRMWND_POS_X;
									int right = CONFIRMWND_POS_X + CONFIRMWND_WIDTH;
									if (x < left || x > right)
									{
										return -1;
									}
									int yesTop = CONFIRMWND_POS_Y + CONFIRMWND_OFFSET_YES_Y - 6;
									int noTop = CONFIRMWND_POS_Y + CONFIRMWND_OFFSET_NO_Y - 6;
									if (y >= yesTop && y < noTop)
									{
										return 1;
									}
									if (y >= noTop && y < CONFIRMWND_POS_Y + CONFIRMWND_HEIGHT + 4)
									{
										return 0;
									}
									return -1;
								}

								public void swCurPos(bool b)
								{
									if (b)
									{
										cellCursor3d_.SetPositionI(CONFIRMWND_POS_X + CONFIRMWND_OFFSET_YES_X, CONFIRMWND_POS_Y + CONFIRMWND_OFFSET_YES_Y);
									}
									else
									{
										cellCursor3d_.SetPositionI(CONFIRMWND_POS_X + CONFIRMWND_OFFSET_NO_X, CONFIRMWND_POS_Y + CONFIRMWND_OFFSET_NO_Y);
									}
								}
							}
	}
}
