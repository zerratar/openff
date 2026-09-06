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
							public class CMoneyWindow
							{
								public static int MONEYWND_MSG = 50813;

								public static int MONEYWND_MONETARY = 50429;

								public static int MONEYWND_POS_X = 184;

								public static int MONEYWND_POS_Y = 160;

								public static int MONEYWND_WIDTH = 64;

								public static int MONEYWND_HEIGHT = 24;

								public static int MENEYWND_MONETARY_X = 40;

								public static int MENEYWND_MONETARY_Y = 16;

								private bool visiblity_;

								private int messageIDMoney_;

								private int messageIDMonetary_;

								private menu.BasicWindow window_;

								public CMoneyWindow()
								{
									messageIDMoney_ = -1;
									visiblity_ = false;
								}

								~CMoneyWindow()
								{
									close();
								}

								public void open()
								{
									ds.Vector2<short> vector = new ds.Vector2<short>(0, 0);
									vector.set((short)MONEYWND_POS_X, (short)MONEYWND_POS_Y);
									ds.Vector2<short> vector2 = new ds.Vector2<short>(0, 0);
									vector2.set((short)MONEYWND_WIDTH, (short)MONEYWND_HEIGHT);
									window_.bwCreateUL(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, vector, vector2, 3);
									window_.SetPriority(3);
									window_.SetShow(show: true, user: true);
									releaseMessage(ref messageIDMoney_);
									releaseMessage(ref messageIDMonetary_);
									messageIDMoney_ = dgs.msg.CMessageSys.getInstance().Main().createMessage((uint)MONEYWND_MSG, (ushort)(vector.vx + 4), (ushort)(vector.vy + 4), dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART2, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
									if (-1 != messageIDMoney_)
									{
										dgs.DGSMessage dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(messageIDMoney_);
										if (dGSMessage != null)
										{
											dGSMessage.setDisplaySpeed(byte.MaxValue);
											dGSMessage.setShadow(b: true);
											visiblity_ = true;
											return;
										}
									}
									close();
								}

								public void close()
								{
									releaseMessage(ref messageIDMoney_);
									releaseMessage(ref messageIDMonetary_);
									window_.Release();
									visiblity_ = false;
								}

								public int getMessageID()
								{
									return messageIDMoney_;
								}
							}
	}
}
