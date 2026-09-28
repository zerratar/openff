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
								// PORT: the box and its parts where the field_hud layout's confirm frame puts them (the game's numbers unless a mod moved them).
								private OpenFF.Client.BattleHud.Rect box_;
								private (int X, int Y) yes_, no_, question_;
								private int lift_;
								// PORT: where the hand stands on each answer (its frame's cursor), from the box's corner.
								private (int X, int Y) yesCursor_, noCursor_;

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

								/// <summary>PORT: withQuestion false leaves the game's question line out (the engine API's Ask puts its own in the message window).</summary>
								public void open(bool withQuestion = true)
								{
									box_ = OpenFF.Client.BattleHud.Confirm();
									yes_ = OpenFF.Client.BattleHud.ConfirmPart("yes", CONFIRMWND_OFFSET_YES_X, CONFIRMWND_OFFSET_YES_Y);
									no_ = OpenFF.Client.BattleHud.ConfirmPart("no", CONFIRMWND_OFFSET_NO_X, CONFIRMWND_OFFSET_NO_Y);
									question_ = OpenFF.Client.BattleHud.ConfirmPart("question", CONFIRMWND_OFFSET_MSG_X, CONFIRMWND_OFFSET_MSG_Y);
									// PORT: without its question line (a script's Ask, its question in the message window) the answers
									// move up into its place and the box is that much shorter.
									lift_ = withQuestion ? 0 : System.Math.Max(0, System.Math.Min(yes_.Y, no_.Y) - question_.Y);
									yes_.Y -= lift_;
									no_.Y -= lift_;
									box_.Height -= lift_;
									yesCursor_ = OpenFF.Client.BattleHud.ConfirmPart("yes/cursor", yes_.X - 12, yes_.Y + lift_ + 6);
									noCursor_ = OpenFF.Client.BattleHud.ConfirmPart("no/cursor", no_.X - 12, no_.Y + lift_ + 6);
									yesCursor_.Y -= lift_;
									noCursor_.Y -= lift_;
									ds.Vector2<short> vector = new ds.Vector2<short>(0, 0);
									vector.set((short)box_.X, (short)box_.Y);
									ds.Vector2<short> vector2 = new ds.Vector2<short>(0, 0);
									vector2.set((short)box_.Width, (short)box_.Height);
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
									messageIDYes_ = dgs.msg.CMessageSys.getInstance().Main().createMessage((uint)CONFIRMWND_MSG_YES, (ushort)(vector.vx + yes_.X), (ushort)(vector.vy + yes_.Y), dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART2, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
									messageIDNo_ = dgs.msg.CMessageSys.getInstance().Main().createMessage((uint)CONFIRMWND_MSG_NO, (ushort)(vector.vx + no_.X), (ushort)(vector.vy + no_.Y), dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART2, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
									// PORT: the words themselves where the field has not the game's lines for them (Steam's text part here has none).
									if (-1 == messageIDYes_) messageIDYes_ = dgs.msg.CMessageSys.getInstance().Main().createMessage("Yes", (ushort)(vector.vx + yes_.X), (ushort)(vector.vy + yes_.Y), dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
									if (-1 == messageIDNo_) messageIDNo_ = dgs.msg.CMessageSys.getInstance().Main().createMessage("No", (ushort)(vector.vx + no_.X), (ushort)(vector.vy + no_.Y), dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
									if (-1 == messageIDYes_ || -1 == messageIDNo_)
									{
										close();
										return;
									}
									if (withQuestion) messageIDConfirm_ = dgs.msg.CMessageSys.getInstance().Main().createMessage((uint)CONFIRMWND_MSG_CONFIRM, (ushort)(vector.vx + question_.X), (ushort)(vector.vy + question_.Y), dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART2, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
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
									cellCursor3d_.SetPositionI(box_.X + yesCursor_.X, box_.Y + yesCursor_.Y);
									sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(cellCursor3d_);
									visiblity_ = true;
									dgs.msg.CMessageMng main = dgs.msg.CMessageSys.getInstance().Main();
									OpenFF.Client.FieldHud.ConfirmMade(window_, main.Message(messageIDYes_), main.Message(messageIDNo_), messageIDConfirm_ >= 0 && withQuestion ? main.Message(messageIDConfirm_) : null, cellCursor3d_, lift_);
								}
								public void close()
								{
									OpenFF.Client.FieldHud.ConfirmClosed();
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
									int left = box_.X;
									int right = box_.X + box_.Width;
									if (x < left || x > right)
									{
										return -1;
									}
									int yesTop = box_.Y + yes_.Y - 6;
									int noTop = box_.Y + no_.Y - 6;
									if (y >= yesTop && y < noTop)
									{
										return 1;
									}
									if (y >= noTop && y < box_.Y + box_.Height + 4)
									{
										return 0;
									}
									return -1;
								}

								public void swCurPos(bool b)
								{
									if (b)
									{
										cellCursor3d_.SetPositionI(box_.X + yesCursor_.X, box_.Y + yesCursor_.Y);
									}
									else
									{
										cellCursor3d_.SetPositionI(box_.X + noCursor_.X, box_.Y + noCursor_.Y);
									}
									OpenFF.Client.FieldHud.ConfirmFocus(b);   // PORT: the answer the hand is on in :focus for the layout's sheets
								}
							}
	}
}
