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
				mwShowNext(false);
				return true;
			}

			// PORT: whether the game wants its page-turn arrow up - the field_hud layout may hide it or show a picture of its
			// own in its place (OpenFF.Client.FieldHud), so the arrow's own IsShow no longer says.
			private bool m_NextWanted;

			private void mwShowNext(bool show)
			{
				m_NextWanted = show;
				m_ProgressIcon.SetShow(show && OpenFF.Client.FieldHud.NextGame && !LaidOut);
				OpenFF.Client.FieldHud.NextShown(show);
			}

			public void mwExecute()
			{
				OpenFF.Client.FieldHud.Tick();   // PORT: the layout's panels once the window is open, and what moves in it
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
						// PORT: the window open, its text goes on - also one set after it opened at the Fast message speed (a wait
						// of 0, where the phone build waited for more than 0 for ever: a mod's Say got no page-turn arrow).
						else if (dGSMessage != null && m_MessageId >= 0)
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
					if (!m_NextWanted && m_ProgressIconActivity && mwIsPageFinished())
					{
						mwShowNext(true);
					}
					if (mwIsNextPageButton())
					{
						mwShowNext(false);
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
				// PORT: the window from the field_hud layout of WorldDefine.xbn (OpenFF.Client.BattleHud) - the game's numbers unless a mod moved it.
				OpenFF.Client.BattleHud.Rect dlg = OpenFF.Client.BattleHud.Dialogue();
				ds.Vector2<short> vector = new ds.Vector2<short>((short)dlg.X, (short)dlg.Y);
				ds.Vector2<short> vector2 = new ds.Vector2<short>((short)dlg.Width, (short)dlg.Height);
				int flameCount = 5;
				m_Window.SetMaxWindowPos(vector);
				m_Window.SetMaxWindowSize(vector2);
				m_Window.ClearNowWindowSize();
				m_Window.CalcOneRatio(flameCount);
				if (OpenFF.Client.GameProfile.IsFf4)
				{
					// PORT: FF4's arrow is its own sprite (MessageWindow::mwInitialize: MENU_Common's button_up_down, entries
					// 13-15), the down arrow bobbing (sequence 1); its sheet is at 2x like all of FF4's, so drawn at half.
					m_ProgressIcon.Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "button_up_down.NCER", "button_up_down.NANR", "button_up_down.NCGR", null);
					m_ProgressIcon.SetScaleF(2048, 2048);
					m_ProgressIcon.PlayAnimation(1, NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD_LOOP);
				}
				else
				{
					m_ProgressIcon.copy(MenuManager.getSingleton().GetMenuButtonIcon3d());
					m_ProgressIcon.SetCell(24);
				}
				m_ProgressIcon.SetShow(show: false);
				m_NextWanted = false;
				(int nextX, int nextY) = OpenFF.Client.BattleHud.DialogueNext();
				m_ProgressIcon.SetPositionI(nextX, nextY);
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
				// PORT: the field_hud layout's look on the window (OpenFF.Client.FieldHud): its panel, its frames of the mod's.
				OpenFF.Client.FieldHud.DialogueMade(m_Window.GetWindowHandle(), mwIsWindowOpen, m_ProgressIcon);
				// PORT: FF4's window is drawn by its layout (OpenFF.Client.Ff4Dialogue): the game's own is kept for what it does - its
				// opening, its texts typing on - and drawn at nothing.
				if (OpenFF.Client.Ff4Dialogue.Drawn) m_Window.GetWindowHandle().SetLook(0f, null);
				return true;
			}

			// PORT: FF4's scene message bar - the whole width of the bottom, no frame, the text centred.
			private bool m_Bar;

			// PORT: and its line in the Steam game's lettering - smaller than a window's (MSF_HANDLE_KIND_BAR) and light grey,
			// (199, 197, 202) measured off the Steam game's frames.
			private bool BarLettering => m_Bar && OpenFF.Client.GameProfile.IsFf4;
			private const uint BarTextRgba = 0xC7C5CAFFu;
			private dgs.msg.CMessageMng.MSF_HANDLE_KIND MessageFont => BarLettering ? dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_BAR : m_MessageFontSize;

			public bool mwSetBarWindow()
			{
				if (m_Made_1 || m_Window.GetEnable() != -1)
				{
					return false;
				}
				// The Steam game's bar, measured (Tools/ff4hook): y 264 to 312 of the 480 x 320 screen.
				ds.Vector2<short> vector = new ds.Vector2<short>(0, 264);
				ds.Vector2<short> vector2 = new ds.Vector2<short>(480, 48);
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
				OpenFF.Client.Trace.MessageId(msg_no);
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
				m_MessageId = mm[display].createMessage((uint)msg_no, (ushort)message_pos.vx, (ushort)message_pos.vy, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, MessageFont);
				if (m_MessageId < 0)
				{
					m_MessageId = mm[display].createMessage((uint)msg_no, (ushort)message_pos.vx, (ushort)message_pos.vy, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_PERMANENT, MessageFont);
				}
				mm[display].Message(m_MessageId).setMessageColor(m_MessageColor);
				if (BarLettering) mm[display].Message(m_MessageId).getCanvas().rgba = BarTextRgba;
				dGSMessage = mm[display].Message(m_MessageId);
				ds.Vector2<short> vector = new ds.Vector2<short>(message_pos);
				ds.Vector2<short> vector2 = new ds.Vector2<short>();
				dGSMessage.setVSpace(mwMESSAGE_V_SPACE);
				if (m_MessageStyle == MESSAGE_STYLE.MESSAGE_STYLE_CENTER)
				{
					dGSMessage.getCompleteTextSize(vector2);
					vector.vx = (short)((m_Bar ? 240 : ds.DS_SCREEN_WIDTH_HALF) - (vector2.vx >> 1));   // PORT: the scene bar centres on its own width
					if (m_Bar) vector.vy = (short)(285 - (vector2.vy >> 1));   // PORT: and its capitals' middle at Steam's, 285.8 of 320
				}
				dGSMessage.setPosition(vector.vx, vector.vy, erase: true);
				dGSMessage.setDisplaySpeed(m_Bar ? byte.MaxValue : mwDEFAULT_DISPLAY_SPEED);   // PORT: FF4's scene bar shows its line whole (EventConteManager::createMessage draws the text at once)
				dGSMessage.setShadow(m_MessageShadow && !m_Bar);   // PORT: the Steam bar's line has no shadow
				mwNoteLine(dGSMessage, vector);
				mwPlaceAligned(dGSMessage, vector);
				if (m_MessageAlign != 0)
				{
					dGSMessage.setStyle(m_MessageAlign);
					m_MessageAlign = 0u;
				}
				mwResetMessageWait_();
				if (m_NextWanted) mwShowNext(false);   // PORT: a new text: its arrow once it is all there, not the last one's
				m_MessageNo = msg_no;
				m_StartCount = 0u;
				m_EndCount = 0u;
				OpenFF.Client.FieldHud.DialogueText(dGSMessage, msg_no, null);   // PORT: who says it, and the layout's look on it
				return true;
			}

			/// <summary>PORT: the same as mwSetMessage, for a text that is not in any message file (the engine API's Say).</summary>
			public bool mwSetMessageText(ds.Vector2<short> message_pos, string text, int display)
			{
				OpenFF.Client.Trace.Text(text);
				m_Display = display;
				if (text == null)
				{
					return false;
				}
				if (m_MessageId != -1)
				{
					mm[display].releaseMessage(m_MessageId);
				}
				m_MessageId = mm[display].createMessage(text, (ushort)message_pos.vx, (ushort)message_pos.vy, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, MessageFont, typed: true);
				if (m_MessageId < 0)
				{
					return false;
				}
				// PORT: a whole text broken at its words to the field_hud layout's text frame, measured at its font's size (made again when it was).
				string wrapped = OpenFF.Client.FieldHud.Wrap(text, mm[display].Message(m_MessageId)?.m_TextCanvas?.pFont?.size ?? 12);
				if (wrapped != text)
				{
					mm[display].releaseMessage(m_MessageId);
					text = wrapped;
					m_MessageId = mm[display].createMessage(text, (ushort)message_pos.vx, (ushort)message_pos.vy, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, MessageFont, typed: true);
					if (m_MessageId < 0)
					{
						return false;
					}
				}
				mm[display].Message(m_MessageId).setMessageColor(m_MessageColor);
				if (BarLettering) mm[display].Message(m_MessageId).getCanvas().rgba = BarTextRgba;
				dgs.DGSMessage dGSMessage = mm[display].Message(m_MessageId);
				ds.Vector2<short> vector = new ds.Vector2<short>(message_pos);
				ds.Vector2<short> vector2 = new ds.Vector2<short>();
				dGSMessage.setVSpace(mwMESSAGE_V_SPACE);
				if (m_MessageStyle == MESSAGE_STYLE.MESSAGE_STYLE_CENTER)
				{
					dGSMessage.getCompleteTextSize(vector2);
					vector.vx = (short)((m_Bar ? 240 : ds.DS_SCREEN_WIDTH_HALF) - (vector2.vx >> 1));   // PORT: the scene bar centres on its own width
					if (m_Bar) vector.vy = (short)(285 - (vector2.vy >> 1));   // PORT: and its capitals' middle at Steam's, 285.8 of 320
				}
				dGSMessage.setPosition(vector.vx, vector.vy, erase: true);
				dGSMessage.setDisplaySpeed(m_Bar ? byte.MaxValue : mwDEFAULT_DISPLAY_SPEED);
				dGSMessage.setShadow(m_MessageShadow && !m_Bar);   // PORT: the Steam bar's line has no shadow
				mwNoteLine(dGSMessage, vector);
				mwPlaceAligned(dGSMessage, vector);
				if (m_MessageAlign != 0)
				{
					dGSMessage.setStyle(m_MessageAlign);
					m_MessageAlign = 0u;
				}
				mwResetMessageWait_();
				if (m_NextWanted) mwShowNext(false);   // PORT: a new text: its arrow once it is all there, not the last one's
				m_MessageNo = -2;
				m_StartCount = 0u;
				m_EndCount = 0u;
				OpenFF.Client.FieldHud.DialogueText(dGSMessage, -1, text);   // PORT: who says it, and the layout's look on it
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
					// PORT: where it is asked for (the field_hud layout's dialogue/name), not the phone build's 24, 139.
					// FF4 writes the name in the message's own font (NameWindow::nwDrawMessage_), in a window of its own.
					bool ff4 = OpenFF.Client.GameProfile.IsFf4;
					m_NameId = mm[m_Display].createMessage((uint)who, (ushort)name_message_pos.vx, (ushort)name_message_pos.vy, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, ff4 ? dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12 : dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8);
					if (ff4 && LaidOut) mm[m_Display].Message(m_NameId).Silent = true;
					else if (ff4) mwOpenNameWindow(name_message_pos, who);
					m_NameWho = who;
				}
				dGSMessage = mm[m_Display].Message(m_NameId);
				dGSMessage.setDisplaySpeed(byte.MaxValue);
				dGSMessage.setDisplayWait(0);
				OpenFF.Client.FieldHud.DialogueName(dGSMessage);   // PORT: the layout's dialogue/name look
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
				return now.vx >> 12 >= max.vx && now.vy >> 12 >= max.vy;   // the size so far is fx32 (MenuWindow.SizeMoving), the full size whole units
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
					OpenFF.Client.FieldHud.DialogueClosed();
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
				m_NameWho = -1;
				if (m_NameId != -1)
				{
					mm[m_Display].releaseMessage(m_NameId);
					m_NameId = -1;
				}
				if (m_NameWindow != null)
				{
					m_NameWindow.SetShow(show: false, user: true);
					m_NameWindow.Release();
					m_NameWindow = null;
				}
			}

			// PORT: what FF4's layout (OpenFF.Client.Ff4Dialogue) draws in the game's place - the window (not the scene's bar) open,
			// the text typed so far, the speaker, the arrow, and where the script put a line of its own (setMessagePosition /
			// setMessageAlignment) in the 480 x 320 screen, its alignment (0 left, 1 centred, 2 right; down: 0 top, 1 middle,
			// 2 bottom) and its colour.
			private bool LaidOut => OpenFF.Client.Ff4Dialogue.Drawn && !m_Bar;
			private int m_NameWho = -1;
			private int m_LineX = 12, m_LineY = 252, m_LineAlign, m_LineDown, m_LineColour = 1;

			public bool Ff4Open => LaidOut && m_Made_1 && m_Window.GetWindowHandle().IsShow();

			/// <summary>How far the window has opened across and down (0..1): it grows to its size over five frames.</summary>
			public (float X, float Y) Ff4Openness
			{
				get
				{
					ds.Vector2<int> now = m_Window.GetNowWindowSize();
					ds.Vector2<short> max = m_Window.GetMaxWindowSize();
					// The size so far is fx32 (MenuWindow.SizeMoving), the full size whole units.
					return (max.vx > 0 ? Math.Clamp(now.vx / 4096f / max.vx, 0f, 1f) : 1f, max.vy > 0 ? Math.Clamp(now.vy / 4096f / max.vy, 0f, 1f) : 1f);
				}
			}
			public string Ff4Text => m_MessageId >= 0 ? mm[m_Display].Message(m_MessageId)?.getStringBuffer() : null;
			public int Ff4NameWho => m_NameWho;
			public bool Ff4Next => m_NextWanted;
			public (int X, int Y, int Align, int Down, int Colour) Ff4Line => (m_LineX, m_LineY, m_LineAlign, m_LineDown, m_LineColour);

			private void mwNoteLine(dgs.DGSMessage dGSMessage, ds.Vector2<short> at)
			{
				if (!LaidOut) return;
				dGSMessage.Silent = true;
				m_LineX = at.vx;
				m_LineY = at.vy;
				m_LineAlign = (m_MessageAlign & 0x10u) != 0 ? 1 : (m_MessageAlign & 0x20u) != 0 ? 2 : 0;
				m_LineDown = (m_MessageAlign & 0x2u) != 0 ? 1 : (m_MessageAlign & 0x4u) != 0 ? 2 : 0;
				m_LineColour = (int)m_MessageColor;
			}

			// PORT: FF4's name window (menu::NameWindow::nwOpen): from 8 left and 2 above the name, 24 high and the name's
			// width (made even) and 18 wide - so it sits on the message window's top edge, its left edges in line.
			private BasicWindow m_NameWindow;

			private void mwOpenNameWindow(ds.Vector2<short> name_message_pos, int who)
			{
				int width = 0;
				try
				{
					string name = dgs.msg.CMessageSys.getInstance().Main().getMessage((uint)who);
					if (!string.IsNullOrEmpty(name)) width = getStringWidth(name.Trim(), 16);
				}
				catch (Exception) { }
				width = width - (width & 1) + 18;
				if (m_NameWindow != null)
				{
					m_NameWindow.SetShow(show: false, user: true);
					m_NameWindow.Release();
				}
				m_NameWindow = new BasicWindow();
				m_NameWindow.Initialize();
				m_NameWindow.bwCreateUL(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, new ds.Vector2<short>((short)(name_message_pos.vx - 8), (short)(name_message_pos.vy - 2)), new ds.Vector2<short>((short)width, 24), 3);
				m_NameWindow.SetPriority(3);
				m_NameWindow.SetShow(show: true, user: true);
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
				if (!_ProgressIconActivity && m_NextWanted) mwShowNext(false);   // PORT: no page-turn arrow wanted: none up (a question's)
			}

			public bool mwGetSendMessage()
			{
				return m_SendMessage;
			}

			public override void SetShow(bool show, bool user)
			{
				m_Window.GetWindowHandle().SetShow(show, user);
				m_NameWindow?.SetShow(show, user);
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

			// PORT: FF4's centred (or right-set) line, its place its middle (Ff4Commands.SetMessageAlignment): placed once from
			// the whole line's size and typed from the left there - the typewriter's growing text, set about its middle at each
			// step, would be drawn over itself shifted.
			private void mwPlaceAligned(dgs.DGSMessage dGSMessage, ds.Vector2<short> at)
			{
				if (!OpenFF.Client.GameProfile.IsFf4 || (m_MessageAlign & 0x36u) == 0) return;
				ds.Vector2<short> whole = new ds.Vector2<short>();
				dGSMessage.getCompleteTextSize(whole);
				int x = at.vx, y = at.vy;
				if ((m_MessageAlign & 0x10u) != 0) x -= whole.vx / 2; else if ((m_MessageAlign & 0x20u) != 0) x -= whole.vx;
				if ((m_MessageAlign & 0x2u) != 0) y -= whole.vy / 2; else if ((m_MessageAlign & 0x4u) != 0) y -= whole.vy;
				dGSMessage.setPosition((short)x, (short)y, erase: true);
				m_MessageAlign = (m_MessageAlign & ~0x1F6u) | 0x8u | 0x1u | 0x40u;
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
