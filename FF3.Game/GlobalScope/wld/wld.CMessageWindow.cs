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
							public class CMessageWindow
							{
								private menu.MessageWindow m_MesWindow = new menu.MessageWindow();

								private int m_MesDeleteFrame;

								private ds.Vector2<short> m_MessagePosition = new ds.Vector2<short>();

								public void setup()
								{
									menu.MessageWindow.mwInitializeSystem();
									m_MesWindow.Initialize();
									m_MesDeleteFrame = -1;
									m_MessagePosition.set((short)BASIC_MSG_POS_X, (short)BASIC_MSG_POS_Y);
								}

								public void cleanup()
								{
									menu.MessageWindow.mwReleaseSystem();
									release();
								}

								public bool createMessageWindow(int _Pos, int _MesNum, int _Who)
								{
									if (_MesNum == -1)
									{
										return false;
									}
									if (_Pos == 0)
									{
										_Pos = 1;
									}
									_Who = _MesNum + 1000000;
									ds.Vector2<short> name_message_pos = new ds.Vector2<short>(24, 139);
									ds.Vector2<short> message_pos = new ds.Vector2<short>(m_MessagePosition);
									return m_MesWindow.mwCreate(static_cast<menu.MessageWindow.MESSAGE_WINDOW_POSITION>(_Pos), message_pos, _MesNum, name_message_pos, _Who);
								}

								/// <summary>PORT: the speaker's name by its own text id (FF4 names it in the script).</summary>
								public void setName(int who)
								{
									m_MesWindow.mwSetNameMessage(new ds.Vector2<short>(24, 139), who);
								}

								/// <summary>PORT: no speaker's name.</summary>
								public void clearName()
								{
									m_MesWindow.NameMessageRelease();
								}

								public bool createWindow(int _Pos)
								{
									return m_MesWindow.mwSetWindow(static_cast<menu.MessageWindow.MESSAGE_WINDOW_POSITION>(_Pos));
								}

								/// <summary>PORT: a text that is in no message file, for the engine API.</summary>
								public void createText(string text, int _Display)
								{
									m_MesWindow.mwSetMessageText(m_MessagePosition, text, _Display);
								}

								public void createMessage(int _MesNum, int _Who, int _Display)
								{
									if (_MesNum != -1)
									{
										m_MesWindow.mwSetMessage(m_MessagePosition, _MesNum, _Display);
									}
								}

								public void execute()
								{
									m_MesWindow.mwExecute();
									if (!m_MesWindow.mwIsMessageId() || !isFinished())
									{
										return;
									}
									if (m_MesDeleteFrame >= 0)
									{
										m_MesDeleteFrame--;
										if (m_MesDeleteFrame < 0)
										{
											releaseMessage();
											m_MesDeleteFrame = -1;
										}
									}
									else if (isNextPage())
									{
										releaseMessage();
									}
								}

								public void releaseWindow()
								{
									m_MesWindow.WindowRelease();
								}

								public void releaseMessage()
								{
									m_MesWindow.MessageRelease();
									m_MesWindow.NameMessageRelease();
									m_MessagePosition.set((short)BASIC_MSG_POS_X, (short)BASIC_MSG_POS_Y);
								}

								public void release()
								{
									m_MesWindow.Release();
								}

								public void setShow(bool _Show)
								{
									m_MesWindow.SetShow(_Show, user: true);
								}

								public bool isShow()
								{
									return m_MesWindow.IsShow();
								}

								public bool isMadeWindow()
								{
									return m_MesWindow.mwIsMade();
								}
								/// <summary>PORT: the window has finished opening; text set before that shows over a half-drawn frame.</summary>
								public bool isWindowOpen()
								{
									return m_MesWindow.mwIsWindowOpen();
								}

								public bool isMadeMessage()
								{
									return m_MesWindow.mwIsMessageId();
								}

								public bool isNextPage()
								{
									return m_MesWindow.mwIsNextPage();
								}

								public bool isPageFinished()
								{
									return m_MesWindow.mwIsPageFinished();
								}

								public bool isNextPageButton()
								{
									return m_MesWindow.mwIsNextPageButton();
								}

								public bool isFinished()
								{
									return m_MesWindow.mwIsFinished();
								}

								public bool isMessageProgressEnded()
								{
									return m_MesWindow.mwIsMessageProgressEnded();
								}

								public void setMesDeleteFrame(int m_MesDeleteFrame)
								{
									this.m_MesDeleteFrame = m_MesDeleteFrame;
								}

								public void setSendMessage(bool _SendMessage)
								{
									m_MesWindow.mwSetSendMessage(_SendMessage);
								}

								public void setProgressIconActivity(bool _SendMessage)
								{
									m_MesWindow.mwSetProgressIconActivity(_SendMessage);
								}

								public bool isSendMessage()
								{
									return m_MesWindow.mwGetSendMessage();
								}

								public void setMessagePosition(ds.Vector2<short> _MessagePosition)
								{
									m_MessagePosition.copy(_MessagePosition);
								}

								public ds.Vector2<short> getMessagePosition()
								{
									return m_MessagePosition;
								}

								public void setMessageShadow(bool _MessageShadow)
								{
									m_MesWindow.SetMessageShadow(_MessageShadow);
								}

								public void setMessageStyle(int _MessageStyle)
								{
									m_MesWindow.SetMessageStyle(_MessageStyle);
								}

								public void setMessageAlignment(int align)
								{
									m_MesWindow.SetMessageAlignment((uint)align);
								}

								public void setMessageColor(int _MessageColor)
								{
									m_MesWindow.SetMessageColor(_MessageColor);
								}

								public void setMessageFontSize(int _MessageColor)
								{
									m_MesWindow.SetMessageFontSize(_MessageColor);
								}

								public void displayAllMessage()
								{
									m_MesWindow.mwDisplayAllMessage();
								}

								public int getMessageID()
								{
									return m_MesWindow.mwGetMessageID();
								}
							}
	}
}
