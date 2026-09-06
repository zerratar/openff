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
	public static partial class dgs
	{
		public static class msg
		{
			public class CMessageMng : DGSMessageManager
			{
				public enum MSD_HANDLE_KIND
				{
					MSD_HANDLE_KIND_COMMON,
					MSD_HANDLE_KIND_GAME_PART,
					MSD_HANDLE_KIND_GAME_PART2,
					MSD_HANDLE_KIND_PERMANENT,
					MSD_HANDLE_KIND_MAX
				}

				public enum MSF_HANDLE_KIND
				{
					MSF_HANDLE_KIND_12x12,
					MSF_HANDLE_KIND_8x8,
					MSF_HANDLE_KIND_MAX
				}

				public const int MESSAGE_ERR = -1;

				public const MSD_HANDLE_KIND MSD_HANDLE_KIND_COMMON = MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON;

				public const MSD_HANDLE_KIND MSD_HANDLE_KIND_GAME_PART = MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART;

				public const MSD_HANDLE_KIND MSD_HANDLE_KIND_GAME_PART2 = MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART2;

				public const MSD_HANDLE_KIND MSD_HANDLE_KIND_PERMANENT = MSD_HANDLE_KIND.MSD_HANDLE_KIND_PERMANENT;

				public const MSD_HANDLE_KIND MSD_HANDLE_KIND_MAX = MSD_HANDLE_KIND.MSD_HANDLE_KIND_MAX;

				public const MSF_HANDLE_KIND MSF_HANDLE_KIND_12x12 = MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12;

				public const MSF_HANDLE_KIND MSF_HANDLE_KIND_8x8 = MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;

				public const MSF_HANDLE_KIND MSF_HANDLE_KIND_MAX = MSF_HANDLE_KIND.MSF_HANDLE_KIND_MAX;

				private static uint MESSAGE_CONTROLL_MAX = 30u;

				public bool m_Init;

				public TARGETLCD m_Lcd;

				public int[] m_MsdHandle = new int[4];

				public int[] m_MsfHandle = new int[2];

				public DGSMessage[] m_Message = new DGSMessage[MESSAGE_CONTROLL_MAX];

				public MSDINFO[] m_MsdAddr = new MSDINFO[4];

				public new void initialize()
				{
					m_Init = true;
					for (int i = 0; i < 4; i++)
					{
						m_MsdHandle[i] = INVALID_MSDHANDLE;
						m_MsdAddr[i] = null;
					}
					for (int j = 0; j < 2; j++)
					{
					}
					for (int k = 0; k < MESSAGE_CONTROLL_MAX; k++)
					{
						m_Message[k] = null;
					}
				}

				public void draw()
				{
					if (m_Init)
					{
						dgsMMDraw();
					}
				}

				public void terminate()
				{
					if (m_Init)
					{
						for (int i = 0; i < 4; i++)
						{
							removeMSD(m_MsdAddr[i]);
						}
						for (int j = 0; j < MESSAGE_CONTROLL_MAX; j++)
						{
							releaseMessage(j);
						}
						m_Init = false;
					}
				}

				public bool setUpMSD(MSDINFO _Addr, MSD_HANDLE_KIND _MsdHandleKind)
				{
					if (_Addr == null)
					{
						return false;
					}
					m_MsdAddr[(int)_MsdHandleKind] = _Addr;
					m_MsdHandle[(int)_MsdHandleKind] = initMSD(m_MsdAddr[(int)_MsdHandleKind]);
					return true;
				}

				public bool setUpMSF(int _Font, MSF_HANDLE_KIND _MsfHandleKind)
				{
					m_MsfHandle[(int)_MsfHandleKind] = _Font;
					return true;
				}

				public void assignBG(int _BGNumber, int x, int y, int w, int h)
				{
					assignBG(_BGNumber, m_Lcd, x, y, w, h);
				}

				public int createMenuMessage(uint _MsgNumber, ushort x, ushort y, int handle, int font)
				{
					int i;
					for (i = 0; i < MESSAGE_CONTROLL_MAX && m_Message[i] != null; i++)
					{
					}
					if (i == MESSAGE_CONTROLL_MAX)
					{
						return -1;
					}
					m_Message[i] = createMessage(_MsgNumber, handle, font);
					if (m_Message[i] == null)
					{
						return -1;
					}
					m_Message[i].setPosition((short)x, (short)y, erase: true);
					m_Message[i].setDisplaySpeed(byte.MaxValue);
					m_Message[i].setDisplayWait(0);
					return i;
				}

				public int createMenuMessage(string str, ushort x, ushort y, int handle, int font)
				{
					int i;
					for (i = 0; i < MESSAGE_CONTROLL_MAX && m_Message[i] != null; i++)
					{
					}
					if (i == MESSAGE_CONTROLL_MAX)
					{
						return -1;
					}
					m_Message[i] = createMessage(str, font);
					if (m_Message[i] == null)
					{
						return -1;
					}
					m_Message[i].setPosition((short)x, (short)y, erase: true);
					m_Message[i].setDisplaySpeed(byte.MaxValue);
					m_Message[i].setDisplayWait(0);
					return i;
				}

				public int createMessage(uint _MsgNumber, ushort x, ushort y, MSD_HANDLE_KIND _MsdHandleKind, MSF_HANDLE_KIND _MsfHandleKind)
				{
					int i;
					for (i = 0; i < MESSAGE_CONTROLL_MAX && m_Message[i] != null; i++)
					{
					}
					_ = i;
					_ = MESSAGE_CONTROLL_MAX;
					DGSMessage dGSMessage = createMessage(_MsgNumber, INVALID_MSDHANDLE, m_MsfHandle[(int)_MsfHandleKind]);
					if (dGSMessage != null)
					{
						m_Message[i] = dGSMessage;
					}
					if (m_Message[i] == null)
					{
						// PORT: a number the text does not have. The phone's scripts never missed;
						// Steam's text lacks a few, and the engine API asks for the window's texts.
						return -1;
					}
					m_Message[i].setPosition((short)x, (short)y, erase: true);
					m_Message[i].setDisplaySpeed(1);
					m_Message[i].setDisplayWait(1);
					return i;
				}

				public int createMessage(string str, ushort x, ushort y, MSD_HANDLE_KIND _MsdHandleKind, MSF_HANDLE_KIND _MsfHandleKind)
				{
					int i;
					for (i = 0; i < MESSAGE_CONTROLL_MAX && m_Message[i] != null; i++)
					{
					}
					if (i == MESSAGE_CONTROLL_MAX)
					{
						return -1;
					}
					m_Message[i] = createMessage(str, m_MsfHandle[(int)_MsfHandleKind]);
					if (m_Message[i] == null)
					{
						return -1;
					}
					m_Message[i].setPosition((short)x, (short)y, erase: true);
					m_Message[i].setDisplaySpeed(1);
					m_Message[i].setDisplayWait(1);
					return i;
				}

				public void releaseMessage(int _Index)
				{
					if (_Index >= 0 && MESSAGE_CONTROLL_MAX > _Index && m_Message[_Index] != null)
					{
						m_Message[_Index].release();
						m_Message[_Index] = null;
					}
				}

				public void setVisibility(int _Index, bool _Visibility)
				{
					if (_Index >= 0 && m_Message[_Index] != null)
					{
						m_Message[_Index].setVisibility(_Visibility);
					}
				}

				public void setPosition(int _Index, short x, short y, bool erase)
				{
					if (_Index >= 0 && m_Message[_Index] != null)
					{
						m_Message[_Index].setPosition(x, y, erase);
					}
				}

				public void position(int _Index, ref short x, ref short y)
				{
					if (_Index >= 0 && m_Message[_Index] != null)
					{
						m_Message[_Index].position(out x, out y);
					}
				}

				public void setCanvas(int _Index, int _CanvasIndex)
				{
					if (_Index >= 0 && m_Message[_Index] != null)
					{
						m_Message[_Index].setCanvas(_CanvasIndex);
					}
				}

				public void setVSpace(int _Index, int _Vspace)
				{
					if (_Index >= 0 && m_Message[_Index] != null)
					{
						m_Message[_Index].setVSpace(_Vspace);
					}
				}

				public void setHSpace(int _Index, int _Hspace)
				{
					if (_Index >= 0 && m_Message[_Index] != null)
					{
						m_Message[_Index].setHSpace(_Hspace);
					}
				}

				public void setStyle(int _Index, int _Style)
				{
					if (_Index >= 0 && m_Message[_Index] != null)
					{
						m_Message[_Index].setStyle((uint)_Style);
					}
				}

				public CMessageMng()
				{
					m_Init = false;
					for (int i = 0; i < 4; i++)
					{
						m_MsdHandle[i] = INVALID_MSDHANDLE;
						m_MsdAddr[i] = null;
					}
					for (int j = 0; j < 2; j++)
					{
						m_MsfHandle[j] = INVALID_FONTHANDLE;
					}
					for (int k = 0; k < MESSAGE_CONTROLL_MAX; k++)
					{
						m_Message[k] = null;
					}
				}

				public TARGETLCD Lcd()
				{
					return m_Lcd;
				}

				public void Lcd_set(TARGETLCD arg0)
				{
					m_Lcd = arg0;
				}

				public int getMsdHandle(MSD_HANDLE_KIND _Index)
				{
					return m_MsdHandle[(int)_Index];
				}

				public int getMsfHandle(MSF_HANDLE_KIND _Index)
				{
					return m_MsfHandle[(int)_Index];
				}

				public DGSMessage Message(int _Index)
				{
					if (_Index < 0)
					{
						return null;
					}
					return m_Message[_Index];
				}
			}

			public class CMessageSys
			{
				public enum LCD
				{
					LCD_MAIN,
					LCD_SUB,
					LCD_MAX
				}

				public const LCD LCD_MAIN = LCD.LCD_MAIN;

				public const LCD LCD_SUB = LCD.LCD_SUB;

				public const LCD LCD_MAX = LCD.LCD_MAX;

				public static CMessageSys m_Instance = new CMessageSys();

				private CMessageMng[] m_MessageMng = new CMessageMng[2];

				public void initialize()
				{
					for (int i = 0; i < 2; i++)
					{
						m_MessageMng[i].initialize();
					}
				}

				public void draw()
				{
					m_MessageMng[0].draw();
				}

				public void terminate()
				{
					for (int i = 0; i < 2; i++)
					{
						m_MessageMng[i].terminate();
					}
				}

				public void changeValueFont(int value, out string after)
				{
					sprintf(out after, "%d", value);
				}

				public CMessageSys()
				{
					for (int i = 0; i < m_MessageMng.Length; i++)
					{
						m_MessageMng[i] = new CMessageMng();
					}
					m_MessageMng[0].Lcd_set(TARGETLCD.TLCD_MAIN);
					m_MessageMng[1].Lcd_set(TARGETLCD.TLCD_SUB);
				}

				~CMessageSys()
				{
				}

				public static CMessageSys getInstance()
				{
					return m_Instance;
				}

				public CMessageMng Main()
				{
					return m_MessageMng[0];
				}

				public CMessageMng Sub()
				{
					return m_MessageMng[1];
				}
			}
		}
	}
}
