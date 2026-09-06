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
	public static partial class menu
	{
		public class TargetWindow : sys2d.Window
		{
			public const int NO_MOVE = 0;

			public const int PUSH_ON = 1;

			public const int PUSH_OFF = 2;

			private int m_MessageId;

			private int m_TargetId;

			private BasicWindow commandWindow_ = new BasicWindow();

			private int messageId_;

			private int pushState_;

			public TargetWindow()
			{
				Initialize();
			}

			public override void Initialize()
			{
				base.Initialize();
				m_MessageId = -1;
				m_TargetId = -1;
				SetShow(show: false, user: true);
				pushState_ = 0;
				commandWindow_.Initialize();
				messageId_ = -1;
				setShowTarget(show: false);
			}

			public void execute()
			{
				if (IsShow())
				{
					ds.g_TouchPanel.getPoint(out var x, out var y);
					if (ds.g_TouchPanel.isTouch() && ds.g_TouchPanel.getDispPoint().drag == 0 && isTouch(x, y))
					{
						ds.Vector2<short> positionUL = commandWindow_.GetPositionUL();
						ds.Vector2<short> size = commandWindow_.GetSize();
						NNS_G2dDragHilight(positionUL.vx, positionUL.vy, size.vx, size.vy, 0);
					}
				}
			}

			public void createTargetWindow(ds.Vector2<short> Position, ds.Vector2<short> Size)
			{
				commandWindow_.bwCreateUL(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, Position, Size, 3);
				commandWindow_.SetShow(show: false, user: true);
				m_Position.copy(GetPositionCCfromUL(Position, Size));
				m_Size.copy(Size);
			}

			public void createTargetMessage(int _id)
			{
				releaseTargetMessage();
				ds.Vector2<short> positionUL = commandWindow_.GetPositionUL();
				ds.Vector2<short> size = commandWindow_.GetSize();
				messageId_ = dgs.msg.CMessageSys.getInstance().Main().createMessage((uint)_id, (ushort)(positionUL.vx + 8), (ushort)(positionUL.vy + (size.vy - 12) / 2), dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8);
				dgs.DGSMessage dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(messageId_);
				dGSMessage.setDisplaySpeed(byte.MaxValue);
				dGSMessage.setDisplayWait(0);
				dGSMessage.setStyle(520u);
				dGSMessage.setMessageColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
			}

			public void createTargetMessage(string str)
			{
				releaseTargetMessage();
				ds.Vector2<short> positionUL = commandWindow_.GetPositionUL();
				ds.Vector2<short> size = commandWindow_.GetSize();
				messageId_ = dgs.msg.CMessageSys.getInstance().Main().createMessage(str, (ushort)(positionUL.vx + 8), (ushort)(positionUL.vy + (size.vy - 12) / 2), dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8);
				dgs.DGSMessage dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(messageId_);
				dGSMessage.setDisplaySpeed(byte.MaxValue);
				dGSMessage.setDisplayWait(0);
				dGSMessage.setStyle(520u);
				dGSMessage.setMessageColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
			}

			public void releaseTargetMessage()
			{
				if (messageId_ != -1)
				{
					dgs.msg.CMessageSys.getInstance().Main().releaseMessage(messageId_);
					messageId_ = -1;
				}
			}

			public void setShowTarget(bool show)
			{
				base.SetShow(show, user: true);
				commandWindow_.SetShow(show, user: true);
				if (!show)
				{
					releaseTargetMessage();
				}
			}

			public void setMessageColor(int color)
			{
				dgs.DGSMessage dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(messageId_);
				if (color == 0)
				{
					dGSMessage.setMessageColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
				}
				else
				{
					dGSMessage.setMessageColor(dgs.TXT_COLOR.TXT_UCOLOR_4);
				}
			}

			public int getMessageColor()
			{
				dgs.DGSMessage dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(messageId_);
				if (dGSMessage.getMessageColor() != 1)
				{
					return 1;
				}
				return 0;
			}

			public void moveMessage()
			{
				dgs.DGSMessage dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(messageId_);
				short x;
				short y;
				switch (pushState_)
				{
				case 0:
					pushState_ = 1;
					break;
				case 1:
					dGSMessage.position(out x, out y);
					x++;
					y++;
					dGSMessage.setPosition(x, y, erase: true);
					pushState_ = 2;
					break;
				case 2:
					dGSMessage.position(out x, out y);
					x--;
					y--;
					dGSMessage.setPosition(x, y, erase: true);
					pushState_ = 0;
					break;
				}
			}

			public override void SetShow(bool show, bool user)
			{
				base.SetShow(show, user: true);
				if (!show && m_MessageId != -1)
				{
					dgs.msg.CMessageSys.getInstance().Main().releaseMessage(m_MessageId);
					m_MessageId = -1;
				}
			}

			public override void Release()
			{
				Kill();
			}

			public override void Kill()
			{
				base.Kill();
				commandWindow_.Release();
				releaseTargetMessage();
				if (m_MessageId != -1)
				{
					dgs.msg.CMessageSys.getInstance().Main().releaseMessage(m_MessageId);
					m_MessageId = -1;
				}
				Initialize();
			}

			public bool isTouch(int x, int y)
			{
				ds.Vector2<short> positionUL = commandWindow_.GetPositionUL();
				ds.Vector2<short> size = commandWindow_.GetSize();
				if (positionUL.vx <= x && x <= positionUL.vx + size.vx && positionUL.vy <= y)
				{
					return y <= positionUL.vy + size.vy;
				}
				return false;
			}

			public bool isShowTarget()
			{
				return commandWindow_.IsShow();
			}

			public int pushState()
			{
				return pushState_;
			}

			public int targetId()
			{
				return m_TargetId;
			}

			public void targetId_set(int arg0)
			{
				m_TargetId = arg0;
			}

			public override void SetPriority(byte pri)
			{
				m_Priority = pri;
			}

			protected override void SetDepth(int depth)
			{
				m_Depth = depth;
			}
		}
	}
}
