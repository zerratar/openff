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
		public class CommandWindow : sys2d.Window
		{
			public enum COMMAND_WINDOW_SELECT
			{
				cwsOFF,
				cwsON,
				cwsDECIDED,
				cwsMAX,
				cwsINVALID
			}

			public class cwDATA
			{
				public enum cwdSTATE
				{
					cwdsOFF,
					cwdsON,
					cwdsMAX
				}

				public ds.Vector2<short> Position = new ds.Vector2<short>();

				public ds.Vector2<short> Size = new ds.Vector2<short>();
			}

			public const COMMAND_WINDOW_SELECT cwsOFF = COMMAND_WINDOW_SELECT.cwsOFF;

			public const COMMAND_WINDOW_SELECT cwsON = COMMAND_WINDOW_SELECT.cwsON;

			public const COMMAND_WINDOW_SELECT cwsDECIDED = COMMAND_WINDOW_SELECT.cwsDECIDED;

			public const COMMAND_WINDOW_SELECT cwsMAX = COMMAND_WINDOW_SELECT.cwsMAX;

			public const COMMAND_WINDOW_SELECT cwsINVALID = COMMAND_WINDOW_SELECT.cwsINVALID;

			public const int NO_MOVE = 0;

			public const int PUSH_ON = 1;

			public const int PUSH_OFF = 2;

			public static sys2d.Sprite3d g_WindowSprite = new sys2d.Sprite3d();

			public static int cwSTRING_MAX = 16;

			public static int cwMESSAGE_START = 11;

			private COMMAND_WINDOW_SELECT m_SelectState;

			private int m_MessageId;

			private pl.ABILITY_ID m_CommandId;

			private sys2d.Sprite3d m_Window = new sys2d.Sprite3d();

			private BasicWindow m_PushWindow = new BasicWindow();

			private BasicWindow[] commandWindow_ = new BasicWindow[4];

			private int[] messageId_ = new int[pl.BATTLE_COMMAND_MAX];

			private int pushState_;

			public cwDATA commandWindowData(int i)
			{
				cwDATA cwDATA2 = new cwDATA();
				if (i < 3)
				{
					cwDATA2.Position.vx = (short)BATTLE_COMMAND_X();
					cwDATA2.Position.vy = (short)(BATTLE_COMMAND_Y() + (i + 1) * 40);
					cwDATA2.Size.vx = 128;
					cwDATA2.Size.vy = 40;
				}
				else
				{
					cwDATA2.Position.vx = 400;
					cwDATA2.Position.vy = 0;
					cwDATA2.Size.vx = 80;
					cwDATA2.Size.vy = 40;
				}
				return cwDATA2;
			}

			public CommandWindow()
			{
				for (int i = 0; i < commandWindow_.Length; i++)
				{
					commandWindow_[i] = new BasicWindow();
				}
				Initialize();
			}

			public static void cwInitializeSystem()
			{
			}

			public static void cwReleaseSystem()
			{
				g_WindowSprite.Release();
			}

			public override void Initialize()
			{
				base.Initialize();
				m_SelectState = COMMAND_WINDOW_SELECT.cwsINVALID;
				m_MessageId = -1;
				m_CommandId = pl.ABILITY_ID.ABILITY_ERR_ID;
				SetShow(show: false, user: true);
				pushState_ = 0;
				for (int i = 0; i < 4; i++)
				{
					commandWindow_[i].Initialize();
				}
				for (int j = 0; j < pl.BATTLE_COMMAND_MAX; j++)
				{
					messageId_[j] = -1;
				}
				setShowCommand(show: false);
			}

			public void execute()
			{
				if (!IsShow())
				{
					return;
				}
				ds.g_TouchPanel.getPoint(out var x, out var y);
				for (int i = 0; i < 4; i++)
				{
					ds.Vector2<short> positionUL = commandWindow_[i].GetPositionUL();
					ds.Vector2<short> size = commandWindow_[i].GetSize();
					if (ds.g_TouchPanel.isTouch() && ds.g_TouchPanel.getDispPoint().drag == 0 && positionUL.vx <= x && x <= positionUL.vx + size.vx && positionUL.vy <= y && y <= positionUL.vy + size.vy)
					{
						NNS_G2dDragHilight(positionUL.vx, positionUL.vy, size.vx, size.vy, 0);
					}
				}
			}

			public void createCommandWindow()
			{
				for (int i = 0; i < 4; i++)
				{
					commandWindow_[i].bwCreateUL(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, commandWindowData(i).Position, commandWindowData(i).Size, 3);
					commandWindow_[i].SetShow(show: true, user: true);
				}
			}

			public void createCommandMessage(pl.ABILITY_ID _id, int i)
			{
				releaseCommandMessage(i);
				messageId_[i] = dgs.msg.CMessageSys.getInstance().Main().createMessage((uint)pl.PlayerParty.instance().abilityList((int)_id).nameId_, (ushort)(commandWindowData(i).Position.vx + 8), (ushort)(commandWindowData(i).Position.vy + 14), dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8);
				dgs.DGSMessage dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(messageId_[i]);
				dGSMessage.setDisplaySpeed(byte.MaxValue);
				dGSMessage.setDisplayWait(0);
				dGSMessage.setStyle(520u);
				dGSMessage.setMessageColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
			}

			public void releaseCommandMessage(int i)
			{
				if (messageId_[i] != -1)
				{
					dgs.msg.CMessageSys.getInstance().Main().releaseMessage(messageId_[i]);
					messageId_[i] = -1;
				}
			}

			public void releaseCommandMessageAll()
			{
				for (int i = 0; i < pl.BATTLE_COMMAND_MAX; i++)
				{
					releaseCommandMessage(i);
				}
			}

			public void setShowCommand(bool show)
			{
				base.SetShow(show, user: true);
				for (int i = 0; i < 4; i++)
				{
					commandWindow_[i].SetShow(show, user: true);
				}
				if (!show)
				{
					releaseCommandMessageAll();
				}
			}

			public void setMessageColor(int i, int color)
			{
				dgs.DGSMessage dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(messageId_[i]);
				if (color == 0)
				{
					dGSMessage.setMessageColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
				}
				else
				{
					dGSMessage.setMessageColor(dgs.TXT_COLOR.TXT_UCOLOR_4);
				}
			}

			public void moveMessage(int i)
			{
				dgs.DGSMessage dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(messageId_[i]);
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
				m_PushWindow.SetShow(show: false, user: true);
				m_Window.SetShow(show);
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
				m_PushWindow.Release();
				m_Window.Release();
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_Window);
				for (int i = 0; i < 4; i++)
				{
					commandWindow_[i].Release();
				}
				releaseCommandMessageAll();
				if (m_MessageId != -1)
				{
					dgs.msg.CMessageSys.getInstance().Main().releaseMessage(m_MessageId);
					m_MessageId = -1;
				}
				Initialize();
			}

			public bool isShowCommand()
			{
				return commandWindow_[0].IsShow();
			}

			public int pushState()
			{
				return pushState_;
			}

			public pl.ABILITY_ID cwGetCommand()
			{
				return m_CommandId;
			}

			public COMMAND_WINDOW_SELECT cwGetSelectState()
			{
				return m_SelectState;
			}

			public override void SetPriority(byte pri)
			{
				m_Priority = pri;
				m_Window.SetPriority(pri);
			}

			protected override void SetDepth(int depth)
			{
				m_Depth = depth;
				m_Window.SetDepth(depth);
			}
		}
	}
}
