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
		public class MessageWindow : sys2d.Window
		{
			public enum MESSAGE_STYLE
			{
				MESSAGE_STYLE_LEFT,
				MESSAGE_STYLE_CENTER,
				MESSAGE_STYLE_RIGHT
			}

			public enum MESSAGE_WINDOW_POSITION
			{
				mwpUP,
				mwpDOWN,
				mwpMAX
			}

			public const MESSAGE_STYLE MESSAGE_STYLE_LEFT = MESSAGE_STYLE.MESSAGE_STYLE_LEFT;

			public const MESSAGE_STYLE MESSAGE_STYLE_CENTER = MESSAGE_STYLE.MESSAGE_STYLE_CENTER;

			public const MESSAGE_STYLE MESSAGE_STYLE_RIGHT = MESSAGE_STYLE.MESSAGE_STYLE_RIGHT;

			public static byte mwDEFAULT_DISPLAY_SPEED = 1;

			public static byte mwLOW_MESSAGE_SPEED = 4;

			public static byte mwDEFAULT_MESSAGE_SPEED = 2;

			public static byte mwHIGH_MESSAGE_SPEED = 0;

			public static byte mwMESSAGE_V_SPACE = 4;

			public static ushort mwNEXT_PAGE_BUTTON = 3075;

			public static uint mwVALID_BUTTON_START_FRAME = 2u;

			public static uint mwVALID_BUTTON_END_FRAME = 6u;

			private bool m_MessageShadow;

			private int m_MessageId;

			private int m_NameId;

			private int m_MessageNo;

			private int m_Who;

			private int m_Display;

			private uint m_StartCount;

			private uint m_EndCount;

			private bool m_Made_1;

			private bool m_Loaded_1;

			private bool m_SendMessage;

			private bool m_ProgressIconActivity;

			private int m_State;

			private MESSAGE_STYLE m_MessageStyle;

			private uint m_MessageAlign;

			private dgs.TXT_COLOR m_MessageColor;

			private dgs.msg.CMessageMng.MSF_HANDLE_KIND m_MessageFontSize;

			private MenuWindow m_Window = new MenuWindow();

			private sys2d.Sprite3d m_ProgressIcon = new sys2d.Sprite3d();

			public MessageWindow()
			{
				Initialize();
			}

			~MessageWindow()
			{
				Kill();
			}

			public static void mwInitializeSystem()
			{
			}

			public static void mwReleaseSystem()
			{
			}

			public override void Initialize()
			{
				base.Initialize();
				m_MessageShadow = true;
				m_MessageId = -1;
				m_NameId = -1;
				m_MessageNo = -1;
				m_Who = -1;
				m_Made_1 = false;
				m_Loaded_1 = false;
				m_SendMessage = true;
				m_StartCount = 0u;
				m_EndCount = 0u;
				m_Display = 0;
				m_State = 0;
				m_MessageStyle = MESSAGE_STYLE.MESSAGE_STYLE_LEFT;
				m_MessageAlign = 0u;
				m_MessageColor = dgs.TXT_COLOR.TXT_COLOR_WHITE;
				m_MessageFontSize = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12;
				m_ProgressIcon.Release();
				m_ProgressIconActivity = true;
			}

			public void mwSetUp(NNSG2dBGSelect bg_select, GXBGScrBase scn_base, GXBGCharBase chr_base)
			{
			}

			public void mwDisplayAllMessage()
			{
				dgs.DGSMessage dGSMessage = mm[m_Display].Message(m_MessageId);
				if (dGSMessage != null && !mwIsPageFinished())
				{
					dGSMessage.setDisplaySpeed(byte.MaxValue);
				}
			}

			public bool mwCreate(MESSAGE_WINDOW_POSITION window_pos, ds.Vector2<short> message_pos, int msg_no, ds.Vector2<short> name_message_pos, int who)
			{
				if (!mwSetMessage(message_pos, msg_no, 0))
				{
					return false;
				}
				if (!mwSetWindow(window_pos))
				{
					return false;
				}
				m_State = 0;
				m_ProgressIcon.SetShow(show: false);
				return true;
			}

			public void mwExecute()
			{
				dgs.DGSMessage dGSMessage = mm[m_Display].Message(m_MessageId);
				if (m_State == 0)
				{
					if (m_Made_1)
					{
						if (m_Window.SizeMoving(MenuWindow.MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_LARGE))
						{
							if (dGSMessage != null && m_MessageId >= 0)
							{
								dGSMessage.setDisplayWait(255);
							}
						}
						else if (dGSMessage != null && m_MessageId >= 0 && dGSMessage.displayWait() > 0)
						{
							mwResetMessageWait_();
							m_State = 1;
						}
					}
					else
					{
						m_State = 1;
					}
				}
				else
				{
					if (m_State != 1 || !m_SendMessage)
					{
						return;
					}
					if (!m_ProgressIcon.IsShow() && m_ProgressIconActivity && mwIsPageFinished())
					{
						m_ProgressIcon.SetShow(show: true);
					}
					if (mwIsNextPageButton())
					{
						m_ProgressIcon.SetShow(show: false);
						if (m_StartCount >= mwVALID_BUTTON_START_FRAME && !mwIsFinished())
						{
							if (!mwIsPageFinished())
							{
								dGSMessage.setDisplaySpeed(byte.MaxValue);
							}
							else if (mwIsNextPage())
							{
								m_StartCount = 0u;
								m_EndCount = 0u;
								dGSMessage.pageForward();
								dGSMessage.setDisplaySpeed(mwDEFAULT_DISPLAY_SPEED);
								mwResetMessageWait_();
							}
						}
					}
					if (m_StartCount < 268435456)
					{
						m_StartCount++;
					}
				}
			}

			public bool mwIsNextPageButton()
			{
				if ((ds.g_Pad.edge() & mwNEXT_PAGE_BUTTON) == 0 && !ds.g_TouchPanel.isTap())
				{
					return false;
				}
				return true;
			}

			public bool mwIsNextPage()
			{
				if (m_EndCount < 268435456)
				{
					m_EndCount++;
				}
				if (m_EndCount < mwVALID_BUTTON_END_FRAME)
				{
					return false;
				}
				return mwIsNextPageButton();
			}

			public bool mwSetWindow(MESSAGE_WINDOW_POSITION pos)
			{
				if (m_Made_1)
				{
					return false;
				}
				if (m_Window.GetEnable() != -1)
				{
					return false;
				}
				ds.Vector2<short> vector = new ds.Vector2<short>(5, 233);
				ds.Vector2<short> vector2 = new ds.Vector2<short>(470, 84);
				int flameCount = 5;
				m_Window.SetMaxWindowPos(vector);
				m_Window.SetMaxWindowSize(vector2);
				m_Window.ClearNowWindowSize();
				m_Window.CalcOneRatio(flameCount);
				m_ProgressIcon.copy(MenuManager.getSingleton().GetMenuButtonIcon3d());
				m_ProgressIcon.SetCell(24);
				m_ProgressIcon.SetShow(show: false);
				m_ProgressIcon.SetPositionI(vector.vx + vector2.vx - 24 - 4, vector.vy + vector2.vy - 24 - 4);
				m_ProgressIcon.SetAnimation(anm: true);
				sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(m_ProgressIcon);
				vector2.vx = (vector2.vy = 0);
				m_Window.GetWindowHandle().bwCreateUL(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, vector, vector2, 3);
				m_Window.GetWindowHandle().SetPriority(3);
				m_Window.GetWindowHandle().SetShow(show: true, user: true);
				m_Window.SetEnable(1);
				m_Loaded_1 = true;
				m_Made_1 = true;
				m_State = 0;
				return true;
			}

			// PORT: FF4's scene message bar - the whole width of the bottom, no frame, the text centred.
			private bool m_Bar;

			public bool mwSetBarWindow()
			{
				if (m_Made_1 || m_Window.GetEnable() != -1)
				{
					return false;
				}
				ds.Vector2<short> vector = new ds.Vector2<short>(0, 236);
				ds.Vector2<short> vector2 = new ds.Vector2<short>(480, 84);
				m_Window.SetMaxWindowPos(vector);
				m_Window.SetMaxWindowSize(vector2);
				m_Window.ClearNowWindowSize();
				m_Window.CalcOneRatio(1);
				m_Window.GetWindowHandle().SetBarStyle();
				vector2.vx = (vector2.vy = 0);
				m_Window.GetWindowHandle().bwCreateUL(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, vector, vector2, 3);
				m_Window.GetWindowHandle().SetBarStyle();
				m_Window.GetWindowHandle().SetPriority(3);
				m_Window.GetWindowHandle().SetShow(show: true, user: true);
				m_Window.SetEnable(1);
				m_Loaded_1 = true;
				m_Made_1 = true;
				m_Bar = true;
				m_MessageStyle = MESSAGE_STYLE.MESSAGE_STYLE_CENTER;
				m_State = 0;
				return true;
			}

			public bool mwIsBar() => m_Bar && m_Made_1;

			public bool mwSetMessage(ds.Vector2<short> message_pos, int msg_no, int display)
			{
				m_Display = display;
				if (msg_no < 0)
				{
					return false;
				}
				dgs.DGSMessage dGSMessage = null;
				if (m_MessageId != -1)
				{
					mm[display].releaseMessage(m_MessageId);
				}
				m_MessageId = mm[display].createMessage((uint)msg_no, (ushort)message_pos.vx, (ushort)message_pos.vy, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, m_MessageFontSize);
				if (m_MessageId < 0)
				{
					m_MessageId = mm[display].createMessage((uint)msg_no, (ushort)message_pos.vx, (ushort)message_pos.vy, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_PERMANENT, m_MessageFontSize);
				}
				mm[display].Message(m_MessageId).setMessageColor(m_MessageColor);
				dGSMessage = mm[display].Message(m_MessageId);
				ds.Vector2<short> vector = new ds.Vector2<short>(message_pos);
				ds.Vector2<short> vector2 = new ds.Vector2<short>();
				dGSMessage.setVSpace(mwMESSAGE_V_SPACE);
				if (m_MessageStyle == MESSAGE_STYLE.MESSAGE_STYLE_CENTER)
				{
					dGSMessage.getCompleteTextSize(vector2);
					vector.vx = (short)((m_Bar ? 240 : ds.DS_SCREEN_WIDTH_HALF) - (vector2.vx >> 1));   // PORT: the scene bar centres on its own width
				}
				dGSMessage.setPosition(vector.vx, vector.vy, erase: true);
				dGSMessage.setDisplaySpeed(mwDEFAULT_DISPLAY_SPEED);
				dGSMessage.setShadow(m_MessageShadow);
				if (m_MessageAlign != 0)
				{
					dGSMessage.setStyle(m_MessageAlign);
					m_MessageAlign = 0u;
				}
				mwResetMessageWait_();
				m_MessageNo = msg_no;
				m_StartCount = 0u;
				m_EndCount = 0u;
				return true;
			}

			/// <summary>PORT: the same as mwSetMessage, for a text that is not in any message file (the engine API's Say).</summary>
			public bool mwSetMessageText(ds.Vector2<short> message_pos, string text, int display)
			{
				m_Display = display;
				if (text == null)
				{
					return false;
				}
				if (m_MessageId != -1)
				{
					mm[display].releaseMessage(m_MessageId);
				}
				m_MessageId = mm[display].createMessage(text, (ushort)message_pos.vx, (ushort)message_pos.vy, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, m_MessageFontSize);
				if (m_MessageId < 0)
				{
					return false;
				}
				mm[display].Message(m_MessageId).setMessageColor(m_MessageColor);
				dgs.DGSMessage dGSMessage = mm[display].Message(m_MessageId);
				ds.Vector2<short> vector = new ds.Vector2<short>(message_pos);
				ds.Vector2<short> vector2 = new ds.Vector2<short>();
				dGSMessage.setVSpace(mwMESSAGE_V_SPACE);
				if (m_MessageStyle == MESSAGE_STYLE.MESSAGE_STYLE_CENTER)
				{
					dGSMessage.getCompleteTextSize(vector2);
					vector.vx = (short)((m_Bar ? 240 : ds.DS_SCREEN_WIDTH_HALF) - (vector2.vx >> 1));   // PORT: the scene bar centres on its own width
				}
				dGSMessage.setPosition(vector.vx, vector.vy, erase: true);
				dGSMessage.setDisplaySpeed(mwDEFAULT_DISPLAY_SPEED);
				dGSMessage.setShadow(m_MessageShadow);
				if (m_MessageAlign != 0)
				{
					dGSMessage.setStyle(m_MessageAlign);
					m_MessageAlign = 0u;
				}
				mwResetMessageWait_();
				m_MessageNo = -2;
				m_StartCount = 0u;
				m_EndCount = 0u;
				return true;
			}

			public bool mwSetNameMessage(ds.Vector2<short> name_message_pos, int who)
			{
				if (who < 0)
				{
					return false;
				}
				dgs.DGSMessage dGSMessage = null;
				if (m_NameId != -1)
				{
					mm[m_Display].releaseMessage(m_NameId);
					m_NameId = -1;
				}
				if (mm[m_Display].searchMessageIndexFromID((uint)who) >= 0)
				{
					m_NameId = mm[m_Display].createMessage((uint)who, 24, 139, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8);
				}
				dGSMessage = mm[m_Display].Message(m_NameId);
				dGSMessage.setDisplaySpeed(byte.MaxValue);
				dGSMessage.setDisplayWait(0);
				m_Who = who;
				return true;
			}

			public bool mwIsPageFinished()
			{
				if (m_MessageId == -1)
				{
					return true;
				}
				dgs.DGSMessage dGSMessage = mm[m_Display].Message(m_MessageId);
				if (dGSMessage.getCurrentChar() + 1 < dGSMessage.numberOfChars() || m_StartCount < mwVALID_BUTTON_START_FRAME)
				{
					return false;
				}
				return true;
			}

			public bool mwIsFinished()
			{
				if (m_MessageId == -1)
				{
					return true;
				}
				if (!mwIsPageFinished())
				{
					return false;
				}
				dgs.DGSMessage dGSMessage = mm[m_Display].Message(m_MessageId);
				if (dGSMessage.getCurrentPage() + 1 < dGSMessage.numberOfPages())
				{
					return false;
				}
				return true;
			}

			public bool mwIsMessageProgressEnded()
			{
				if (m_MessageId == -1)
				{
					return true;
				}
				dgs.DGSMessage dGSMessage = mm[m_Display].Message(m_MessageId);
				if (dGSMessage.getCurrentChar() + 1 < dGSMessage.numberOfChars())
				{
					return false;
				}
				return true;
			}

			public bool mwIsMade()
			{
				return m_Made_1;
			}

			/// <summary>PORT: whether the window's opening animation (five frames from the bottom centre) has reached its full size.</summary>
			public bool mwIsWindowOpen()
			{
				if (!m_Made_1)
				{
					return false;
				}
				ds.Vector2<int> now = m_Window.GetNowWindowSize();
				ds.Vector2<short> max = m_Window.GetMaxWindowSize();
				return now.vx >= max.vx && now.vy >= max.vy;
			}

			public bool mwIsMessageId()
			{
				if (m_MessageId != -1)
				{
					return true;
				}
				return false;
			}

			public void WindowRelease()
			{
				if (m_Bar)
				{
					m_Bar = false;
					m_MessageStyle = MESSAGE_STYLE.MESSAGE_STYLE_LEFT;
				}
				if (m_Made_1)
				{
					m_Window.GetWindowHandle().Release();
					m_Window.SetEnable(-1);
					m_Made_1 = false;
				}
				if (m_Loaded_1)
				{
					m_Loaded_1 = false;
				}
			}

			public void MessageRelease()
			{
				if (m_MessageId != -1)
				{
					mm[m_Display].releaseMessage(m_MessageId);
					m_MessageId = -1;
					m_MessageShadow = true;
				}
			}

			public void NameMessageRelease()
			{
				if (m_NameId != -1)
				{
					mm[m_Display].releaseMessage(m_NameId);
					m_NameId = -1;
				}
			}

			public override void Release()
			{
				Kill();
			}

			public override void Kill()
			{
				m_ProgressIcon.SetShow(show: false);
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_ProgressIcon);
				m_ProgressIcon.Release();
				WindowRelease();
				MessageRelease();
				NameMessageRelease();
				Initialize();
			}

			public void mwResetMessageWait_()
			{
				if (m_MessageId == -1)
				{
					return;
				}
				dgs.DGSMessage dGSMessage = mm[m_Display].Message(m_MessageId);
				if (dGSMessage != null)
				{
					switch (opt.COptionManager.getSingleton().messageOption().messageSpeed())
					{
					case opt.MESSAGE_SPEED.MESSAGE_SPEED_SLOW:
						dGSMessage.setDisplayWait(mwLOW_MESSAGE_SPEED);
						break;
					case opt.MESSAGE_SPEED.MESSAGE_SPEED_NORMAL:
						dGSMessage.setDisplayWait(mwDEFAULT_MESSAGE_SPEED);
						break;
					case opt.MESSAGE_SPEED.MESSAGE_SPEED_FAST:
						dGSMessage.setDisplayWait(mwHIGH_MESSAGE_SPEED);
						break;
					default:
						dGSMessage.setDisplayWait(mwDEFAULT_MESSAGE_SPEED);
						break;
					}
				}
			}

			public void mwSetSendMessage(bool _SendMessage)
			{
				m_SendMessage = _SendMessage;
			}

			public void mwSetProgressIconActivity(bool _ProgressIconActivity)
			{
				m_ProgressIconActivity = _ProgressIconActivity;
			}

			public bool mwGetSendMessage()
			{
				return m_SendMessage;
			}

			public override void SetShow(bool show, bool user)
			{
				m_Window.GetWindowHandle().SetShow(show, user);
			}

			public override void SetPriority(byte pri)
			{
				m_Priority = pri;
			}

			protected override void SetDepth(int depth)
			{
				m_Depth = depth;
			}

			public void SetMessageShadow(bool shadow)
			{
				m_MessageShadow = shadow;
			}

			public void SetMessageStyle(int _MessageStyle)
			{
				m_MessageStyle = (MESSAGE_STYLE)_MessageStyle;
			}

			public void SetMessageAlignment(uint align)
			{
				m_MessageAlign = align;
			}

			public void SetMessageColor(int _MessageColor)
			{
				m_MessageColor = (dgs.TXT_COLOR)_MessageColor;
			}

			public void SetMessageFontSize(int _FontSize)
			{
				m_MessageFontSize = ((_FontSize == 0) 
					? dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8 
					: dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
			}

			public int mwGetMessageID()
			{
				return m_MessageId;
			}
		}
	}
}
