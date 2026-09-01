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
	public static partial class btl
	{
		public class HelpWindow
		{
			public const int ERR_MESSAGE_ID = -1;

			private menu.BasicWindow helpWindow_ = new menu.BasicWindow();

			private int messageManagerId_;

			private int messageId_;

			private int frameCounter_;

			private bool isCreate_;

			private int msdHandle_;

			private string message_;

			private sys2d.Sprite3d pageIcon_ = new sys2d.Sprite3d();

			public void setup()
			{
				helpWindow_.Initialize();
				messageId_ = 0;
				messageManagerId_ = -1;
				frameCounter_ = 0;
				isCreate_ = false;
				message_ = "";
				changeCompanyDirectory();
				pageIcon_.Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "icon_16dot");
				pageIcon_.SetCell(24);
				pageIcon_.SetShow(show: false);
				pageIcon_.SetPositionI(448, 4);
				pageIcon_.SetAnimation(anm: true);
				sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(pageIcon_);
			}

			public void cleanup()
			{
				releaseHelpWindow();
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(pageIcon_);
				pageIcon_.Release();
			}

			public void execute()
			{
				if (!isCreate_ && sys2d.Window.WINDOW_STATE.wsOPENED == helpWindow_.GetState())
				{
					createHelpMessage(msdHandle());
					isCreate_ = true;
				}
			}

			public void createHelpWindow(int _id, int page, int small)
			{
				if (!isCreate_ && helpWindow_.GetState() == sys2d.Window.WINDOW_STATE.wsCLOSED)
				{
					messageId_ = _id;
					helpWindow_.bwCreateCC(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, (small != 0) ? HelpWindowCenterPositionS : HelpWindowCenterPosition, (small != 0) ? HelpWindowSizeS : HelpWindowSize, 3);
					if (page != 0)
					{
						pageIcon_.SetShow(show: true);
					}
					else
					{
						pageIcon_.SetShow(show: false);
					}
				}
			}

			public void createHelpWindow(string str, int page, int small)
			{
				if (!isCreate_ && helpWindow_.GetState() == sys2d.Window.WINDOW_STATE.wsCLOSED)
				{
					strcpy(out message_, str);
					helpWindow_.bwCreateCC(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, (small != 0) ? HelpWindowCenterPositionS : HelpWindowCenterPosition, (small != 0) ? HelpWindowSizeS : HelpWindowSize, 3);
					if (page != 0)
					{
						pageIcon_.SetShow(show: true);
					}
					else
					{
						pageIcon_.SetShow(show: false);
					}
				}
			}

			public void releaseHelpWindow()
			{
				helpWindow_.Release();
				releaseHelpMessage();
				isCreate_ = false;
				messageId_ = 0;
				message_ = "";
				messageManagerId_ = -1;
				pageIcon_.SetShow(show: false);
			}

			public void createHelpMessage(int msdHandle)
			{
				ds.Vector2<short> positionCC = helpWindow_.GetPositionCC();
				if (messageId_ > 0)
				{
					messageManagerId_ = dgs.msg.CMessageSys.getInstance().Main().createMessage((uint)messageId_, (ushort)positionCC.vx, (ushort)positionCC.vy, (dgs.msg.CMessageMng.MSD_HANDLE_KIND)msdHandle, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8);
				}
				else
				{
					messageManagerId_ = dgs.msg.CMessageSys.getInstance().Main().createMessage(message_, (ushort)positionCC.vx, (ushort)positionCC.vy, (dgs.msg.CMessageMng.MSD_HANDLE_KIND)msdHandle, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8);
				}
				dgs.DGSMessage dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(messageManagerId_);
				dGSMessage.setDisplaySpeed(byte.MaxValue);
				dGSMessage.setDisplayWait(0);
				dGSMessage.setStyle(1170u);
			}

			public void releaseHelpMessage()
			{
				if (messageManagerId_ >= 0)
				{
					dgs.msg.CMessageSys.getInstance().Main().releaseMessage(messageManagerId_);
					messageManagerId_ = -1;
				}
			}

			public void updateMessage(int _id, int msdHandle)
			{
				releaseHelpMessage();
				messageId_ = _id;
				createHelpMessage(msdHandle);
			}

			public void updateMessage(string str)
			{
				releaseHelpMessage();
				strcpy(out message_, str);
				createHelpMessage(0);
			}

			public void show(bool onOff)
			{
				if (messageManagerId_ >= 0)
				{
					dgs.msg.CMessageSys.getInstance().Main().setVisibility(messageManagerId_, onOff);
					helpWindow_.SetShow(onOff, user: true);
					pageIcon_.SetShow(onOff);
				}
			}

			public bool isCreate()
			{
				return isCreate_;
			}

			public int messageId()
			{
				return messageId_;
			}

			public int msdHandle()
			{
				return msdHandle_;
			}

			public void setMsdHandle(int handle)
			{
				msdHandle_ = handle;
			}
		}
	}
}
