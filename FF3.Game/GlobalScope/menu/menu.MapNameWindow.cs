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
	public static partial class menu
	{
		public class MapNameWindow
		{
			public static int MAP_NAME_WND_X = 240;

			public static int MAP_NAME_WND_Y = 18;

			public static int MAP_NAME_WND_W = 472;

			public static int MAP_NAME_WND_H = 28;

			private bool bVisiblity_;

			private BasicWindow Window_ = new BasicWindow();

			private int nMessageID_;

			private int nCloseCounter_;

			public MapNameWindow()
			{
				nMessageID_ = -1;
				bVisiblity_ = false;
				nCloseCounter_ = -1;
			}

			~MapNameWindow()
			{
				if (-1 != nMessageID_)
				{
					dgs.msg.CMessageSys.getInstance().Main().releaseMessage(nMessageID_);
					nMessageID_ = -1;
				}
				bVisiblity_ = false;
			}

			public int initialize()
			{
				BasicWindow.bwInitializeSystem(1u);
				Window_.Initialize();
				bVisiblity_ = false;
				return 0;
			}

			public void finalize()
			{
				close();
				Window_.Kill();
				BasicWindow.bwReleaseSystem();
			}

			public int open(int nMessageNo)
			{
				ds.Vector2<short> vector = new ds.Vector2<short>();
				ds.Vector2<short> vector2 = new ds.Vector2<short>();
				if (0 <= nMessageNo && !bVisiblity_)
				{
					vector.set((short)MAP_NAME_WND_X, (short)MAP_NAME_WND_Y);
					vector2.set((short)MAP_NAME_WND_W, (short)MAP_NAME_WND_H);
					Window_.bwCreateCC(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, vector, vector2, 3);
					Window_.SetPriority(3);
					Window_.SetShow(show: true, user: true);
					if (-1 != nMessageID_)
					{
						dgs.msg.CMessageSys.getInstance().Main().releaseMessage(nMessageID_);
						nMessageID_ = -1;
					}
					nMessageID_ = dgs.msg.CMessageSys.getInstance().Main().createMessage((uint)nMessageNo, (ushort)vector.vx, (ushort)vector.vy, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
					if (-1 != nMessageID_)
					{
						dgs.DGSMessage dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(nMessageID_);
						if (dGSMessage != null)
						{
							ds.Vector2<short> vector3 = new ds.Vector2<short>();
							dGSMessage.getCompleteTextSize(vector3);
							dGSMessage.setPosition((short)(vector.vx - vector3.vx / 2), (short)(vector.vy - vector3.vy / 2), erase: true);
							dGSMessage.setDisplaySpeed(byte.MaxValue);
							dGSMessage.setShadow(b: true);
							bVisiblity_ = true;
							return 0;
						}
					}
				}
				return 1;
			}

			public void close()
			{
				if (bVisiblity_)
				{
					dgs.msg.CMessageSys.getInstance().Main().releaseMessage(nMessageID_);
					nMessageID_ = -1;
					Window_.SetShow(show: false, user: true);
					Window_.Release();
					bVisiblity_ = false;
				}
			}

			public void setCloseCounter(int cnt)
			{
				if (bVisiblity_ && -1 == nCloseCounter_ && cnt >= 0)
				{
					nCloseCounter_ = cnt;
				}
			}

			public void countdownToClose()
			{
				if (bVisiblity_ && -1 != nCloseCounter_ && 0 >= --nCloseCounter_)
				{
					close();
				}
			}

			public bool isOpen()
			{
				return bVisiblity_;
			}
		}
	}
}
