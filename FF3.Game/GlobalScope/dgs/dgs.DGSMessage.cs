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
	public static partial class dgs
	{
		public class DGSMessage : DGSLinkedList<DGSMessage>
		{
			public class EraseArea
			{
				public short x;

				public short y;

				public short width;

				public short height;

				public void copy(EraseArea src)
				{
					x = src.x;
					y = src.y;
					width = src.width;
					height = src.height;
				}
			}

			public const int MSG_FLAG_ERASE = 1;

			public const int MSG_FLAG_DRAWN = 2;

			public const int MSG_FLAG_FORCEDRAW = 4;

			public const int MSG_FLAG_UNDEFINED1 = 8;

			public const int MSG_FLAG_SHADOW_OFF = 16;

			public const int MSG_FLAG_SUSPENDED = 32;

			public const int MSG_FLAG_ALLOCATED = 64;

			public const int MSG_FLAG_INVISIBLE = 128;

			public const int MSG_FLAG_CHECKMASK = 255;

			private int msg_color;

			protected NNSG2dTagCallback funcTagCallback;

			protected Array paramTagCallback;

			protected short m_PositionX;

			protected short m_PositionY;

			protected byte m_Flag;

			protected uint m_Style;

			protected byte m_CurrentPage;

			protected string m_CurrentStr;

			protected string m_CurrentChar;

			protected uint m_BufferLenght;

			protected string m_Buffer;

			protected string m_BufferHead;

			protected byte m_Speed;

			protected int m_Wait;

			protected int m_Counter;

			protected int m_Priority;

			private EraseArea erase_area = new EraseArea();

			private MSDINFO text;

			private MSDELEMENT msdElement;

			public DGSMessageManager m_Manager;

			public NNSG2dTextCanvas m_TextCanvas = new NNSG2dTextCanvas();

			public int m_CurrentCharArray_offset;

			public int m_BufferHeadArray_offset;

			public DGSMessage()
			{
				clear();
				dgsllLink();
			}

			~DGSMessage()
			{
			}

			public new void destruct()
			{
				erase(m_PositionX, m_PositionY, 0, 0);
				g_DelayedEraseOrder.push(new DelayedEraseArea(m_TextCanvas.pCanvas, m_TextCanvas.pFont, erase_area));
				if (m_Buffer != null)
				{
					ds.CHeap.free_app(m_Buffer);
				}
				m_Buffer = (m_BufferHead = null);
				dgsllUnlink();
			}

			public void release()
			{
				destruct();
			}

			public void setPosition(short x, short y, bool erase)
			{
				if (erase && (m_Flag & 2) != 0)
				{
					this.erase(m_PositionX, m_PositionY, 0, 0);
					m_Flag |= 4;
				}
				m_PositionX = x;
				m_PositionY = y;
			}

			public void position(out short x, out short y)
			{
				x = m_PositionX;
				y = m_PositionY;
			}

			public void setCanvas(int canvas_index)
			{
				m_TextCanvas.pCanvas = m_Manager.dgsmCanvasVector[canvas_index];
			}

			public void setVSpace(int vspace)
			{
				m_TextCanvas.vSpace = vspace;
			}

			public void setHSpace(int hspace)
			{
				m_TextCanvas.hSpace = hspace;
			}

			public void setStyle(uint style)
			{
				m_Style = style;
			}

			public void setShadow(bool b)
			{
				if (!b)
				{
					m_Flag |= 16;
					return;
				}
				m_Flag &= 239;
				m_Flag &= 253;
			}

			public void pageBack()
			{
				if (msdElement != null && text != null && m_CurrentStr != null)
				{
					reset(doErase: true);
				}
			}

			public void pageForward()
			{
				if (msdElement != null && text != null && m_CurrentStr != null)
				{
					reset(doErase: true);
					if (msdElement.num_pages == m_CurrentPage + 1)
					{
						m_CurrentStr = StringUtil.createString(text.m_abyData, (int)msdElement.offset);
						m_CurrentChar = m_CurrentStr;
						m_CurrentPage = 0;
					}
					else
					{
						m_CurrentStr = StringUtil.createString(text.m_abyData, ArrayReader.indexOf(text.m_abyData, (int)msdElement.offset, (int)(text.m_abyData.Length - msdElement.offset), 0, m_CurrentPage + 1) + 1);
						m_CurrentChar = m_CurrentStr;
						m_CurrentPage++;
					}
				}
			}

			public void pageChange(byte page)
			{
				if (msdElement != null && text != null && m_CurrentStr != null)
				{
					int num_pages = msdElement.num_pages;
					num_pages--;
					if (page > num_pages)
					{
						page = (byte)((num_pages > 0) ? ((byte)num_pages) : 0);
					}
					m_CurrentStr = StringUtil.createString(text.m_abyData, (int)msdElement.offset);
					m_CurrentChar = m_CurrentStr;
					m_CurrentPage = 0;
					for (int i = 0; i < page; i++)
					{
						pageForward();
					}
					reset(doErase: true);
				}
			}

			public void assignText(MSDINFO inf, MSDELEMENT elm)
			{
				text = inf;
				msdElement = elm;
				m_CurrentStr = StringUtil.createString(text.m_abyData, (int)msdElement.offset);
				m_CurrentChar = m_CurrentStr;
				NNSG2dTextRect nNSG2dTextRect = NNS_G2dFontGetTextRect(m_TextCanvas.pFont, m_TextCanvas.hSpace, m_TextCanvas.vSpace, m_CurrentStr);
				erase_area.x = (erase_area.y = 0);
				erase_area.width = (short)nNSG2dTextRect.width;
				erase_area.height = (short)nNSG2dTextRect.height;
				if (m_Buffer != null)
				{
					erase(m_PositionX, m_PositionY, 0, 0);
					ds.CHeap.free_app(m_Buffer);
				}
				m_Buffer = (m_BufferHead = null);
				m_BufferLenght = (uint)(getMaxLength() + 1 + EXPANDED_MARGIN);
				m_Buffer = (m_BufferHead = StringUtil.createString((byte[])ds.CHeap.alloc_app(m_BufferLenght)));
				m_Flag &= 253;
				m_Flag |= 4;
				progress();
			}

			public void assignText(string str)
			{
				text = null;
				msdElement = null;
				if (m_Buffer != null)
				{
					EraseArea eraseArea = new EraseArea();
					ds.Vector2<short> vector = new ds.Vector2<short>();
					getTextSize(vector);
					eraseArea.x = m_PositionX;
					eraseArea.y = m_PositionY;
					eraseArea.width = vector.vx;
					eraseArea.width = (short)(vector.vx + 1);
					eraseArea.height = (short)(vector.vy + 1);
					g_DelayedEraseOrder.push(new DelayedEraseArea(m_TextCanvas.pCanvas, m_TextCanvas.pFont, eraseArea));
					if (strlen(str) + 1 + EXPANDED_MARGIN > m_BufferLenght)
					{
						ds.CHeap.free_app(m_Buffer);
						m_Buffer = null;
					}
					else
					{
						m_BufferHead = m_Buffer;
					}
				}
				if (m_Buffer == null)
				{
					m_Buffer = (m_BufferHead = null);
					m_BufferLenght = (uint)(strlen(str) + 1 + EXPANDED_MARGIN);
					m_Buffer = (m_BufferHead = StringUtil.createString((byte[])ds.CHeap.alloc_app(m_BufferLenght)));
				}
				m_CurrentStr = null;
				m_CurrentChar = null;
				strcpy(out m_Buffer, str);
				NNSG2dTextRect nNSG2dTextRect = NNS_G2dFontGetTextRect(m_TextCanvas.pFont, m_TextCanvas.hSpace, m_TextCanvas.vSpace, m_Buffer);
				erase_area.x = (erase_area.y = 0);
				erase_area.width = (short)nNSG2dTextRect.width;
				erase_area.height = (short)nNSG2dTextRect.height;
				m_Flag &= 253;
				m_Flag |= 4;
				progress();
			}

			public byte numberOfChars()
			{
				if (m_CurrentStr == null)
				{
					return 0;
				}
				return (byte)strlen(m_CurrentStr);
			}

			public uint getMaxLength()
			{
				if (text == null || msdElement == null)
				{
					return 4u;
				}
				return 0u;
			}

			public void clear()
			{
				text = null;
				m_PositionX = 0;
				m_PositionY = 0;
				erase_area.x = (erase_area.y = (erase_area.width = (erase_area.height = 0)));
				m_Flag = 0;
				m_Style = 521u;
				m_CurrentPage = 0;
				m_CurrentStr = null;
				m_BufferLenght = 0u;
				m_Buffer = (m_BufferHead = null);
				m_CurrentChar = null;
				m_Speed = 1;
				m_Wait = 6;
				m_Counter = m_Wait;
				m_Manager = null;
				msg_color = 1;
				funcTagCallback = null;
				paramTagCallback = null;
				m_Priority = 0;
			}

			public void reset(bool doErase)
			{
				if (doErase)
				{
					erase(m_PositionX, m_PositionY, 0, 0);
				}
				m_CurrentChar = m_CurrentStr;
				m_CurrentCharArray_offset = 0;
				m_Counter = m_Wait;
				if (m_Buffer != null)
				{
					m_Buffer = "";
				}
				m_BufferHead = m_Buffer;
				m_BufferHeadArray_offset = 0;
				if (m_Wait <= 0)
				{
					progress();
				}
			}

			public string getString()
			{
				if (m_CurrentStr != null)
				{
					return m_CurrentStr;
				}
				if (m_Buffer != null)
				{
					return m_Buffer;
				}
				return null;
			}

			public string getStringBuffer()
			{
				if (m_Buffer != null)
				{
					return m_Buffer;
				}
				return null;
			}

			public void getTextSize(ds.Vector2<short> vec)
			{
				string txt = ((m_CurrentStr != null) ? m_CurrentStr : m_Buffer);
				NNSG2dTextRect nNSG2dTextRect = NNS_G2dFontGetTextRect(m_TextCanvas.pFont, m_TextCanvas.hSpace, m_TextCanvas.vSpace, txt);
				vec.vx = (short)nNSG2dTextRect.width;
				vec.vy = (short)nNSG2dTextRect.height;
			}

			public void getTextSize(string str, out NNSG2dTextRect rect)
			{
				rect = NNS_G2dFontGetTextRect(m_TextCanvas.pFont, m_TextCanvas.hSpace, m_TextCanvas.vSpace, str);
			}

			public void getDisplayTextSize(out NNSG2dTextRect rect)
			{
				rect = NNS_G2dFontGetTextRect(m_TextCanvas.pFont, m_TextCanvas.hSpace, m_TextCanvas.vSpace, m_Buffer);
			}

			public void getCompleteTextSize(ds.Vector2<short> vec)
			{
				vec.vx = erase_area.width;
				vec.vy = erase_area.height;
			}

			public void setActivity(bool b)
			{
				if (b)
				{
					m_Flag &= 223;
					m_Flag |= 4;
				}
				else
				{
					m_Flag |= 32;
				}
			}

			public void setVisibility(bool b)
			{
				if (b)
				{
					if ((m_Flag & 0x80) != 0)
					{
						m_Flag |= 4;
					}
					m_Flag &= 127;
				}
				else
				{
					m_Flag |= 128;
					erase(m_PositionX, m_PositionY, 0, 0);
					erase();
				}
			}

			public void setMessageColor(TXT_COLOR val)
			{
				msg_color = (int)val;
				m_Flag |= 4;
			}

			public void erase(short x, short y, short w, short h)
			{
				if (m_Buffer != null && (m_Flag & 1) == 0)
				{
					short num = (short)(x - 1);
					short num2 = (short)(y - 1);
					NNSG2dTextRect nNSG2dTextRect = new NNSG2dTextRect();
					if (w == 0 || h == 0)
					{
						nNSG2dTextRect = NNS_G2dFontGetTextRect(m_TextCanvas.pFont, m_TextCanvas.hSpace, m_TextCanvas.vSpace, m_Buffer);
					}
					else
					{
						nNSG2dTextRect.width = w;
						nNSG2dTextRect.height = h;
					}
					if ((m_Style & 0x10) != 0)
					{
						num -= (short)(nNSG2dTextRect.width / 2);
					}
					else if ((m_Style & 0x20) != 0)
					{
						num -= (short)nNSG2dTextRect.width;
					}
					if ((m_Style & 2) != 0)
					{
						num2 -= (short)(nNSG2dTextRect.height / 2);
					}
					else if ((m_Style & 4) != 0)
					{
						num2 -= (short)nNSG2dTextRect.height;
					}
					nNSG2dTextRect.width += 2;
					nNSG2dTextRect.height += 2;
					if (nNSG2dTextRect.width > 0 && nNSG2dTextRect.height > 0)
					{
						erase_area.x = num;
						erase_area.y = num2;
						erase_area.width = (short)nNSG2dTextRect.width;
						erase_area.height = (short)nNSG2dTextRect.height;
						m_Flag |= 1;
					}
				}
			}

			public void erase()
			{
				if ((m_Flag & 1) != 0)
				{
					NNS_G2dCharCanvasClearArea(m_TextCanvas.pCanvas, null, 0, erase_area.x, erase_area.y, erase_area.width, erase_area.height);
					m_Flag &= 254;
					m_Flag &= 253;
				}
			}

			public void setTaggedCallback(NNSG2dTagCallback cbf, Array param)
			{
				funcTagCallback = cbf;
				paramTagCallback = param;
			}

			public bool progress()
			{
				if (--m_Counter <= 0)
				{
					m_Flag &= 253;
					m_Counter = m_Wait;
					if (m_CurrentStr != null)
					{
						char[] array = m_CurrentStr.ToCharArray();
						char[] array2 = new char[array.Length * 2];
						char[] array3 = m_BufferHead.ToCharArray();
						Buffer.BlockCopy(array3, 0, array2, 0, array3.Length * 2);
						for (int i = 0; i < m_Speed; i++)
						{
							if (m_CurrentCharArray_offset == array.Length)
							{
								break;
							}
							if ((byte)((array[m_CurrentCharArray_offset] & 0xFF00) >> 8) < 128)
							{
								if (array[m_CurrentCharArray_offset] == '%')
								{
									if (array[m_CurrentCharArray_offset + 1] == '%')
									{
										array2[m_BufferHeadArray_offset] = '%';
										m_CurrentCharArray_offset++;
									}
									else
									{
										CtrlCodeProcessing(array, array2, ref m_CurrentCharArray_offset, ref m_BufferHeadArray_offset);
									}
								}
								else
								{
									array2[m_BufferHeadArray_offset] = array[m_CurrentCharArray_offset];
								}
							}
							else if ((byte)((array[m_CurrentCharArray_offset] & 0xFF00) >> 8) < 224)
							{
								array2[m_BufferHeadArray_offset] = array[m_CurrentCharArray_offset];
							}
							else if ((byte)((array[m_CurrentCharArray_offset] & 0xFF00) >> 8) < 240)
							{
								array2[m_BufferHeadArray_offset] = array[m_CurrentCharArray_offset];
							}
							else
							{
								array2[m_BufferHeadArray_offset] = array[m_CurrentCharArray_offset];
							}
							m_BufferHeadArray_offset++;
							m_CurrentCharArray_offset++;
						}
						if (m_CurrentCharArray_offset == array.Length)
						{
							m_Counter = int.MaxValue;
						}
						m_Buffer = (m_BufferHead = new string(array2, 0, m_BufferHeadArray_offset));
					}
					else
					{
						m_Counter = int.MaxValue;
					}
					return true;
				}
				return false;
			}

			internal static void funcDummyTagCallback(ushort c, NNSG2dTagCallbackInfo pInfo)
			{
			}

			public void draw()
			{
				erase();
				if (!visibility() || !activity() || (!progress() && (m_Flag & 4) == 0))
				{
					return;
				}
				if (funcTagCallback == null)
				{
					NNS_G2dTextCanvasDrawText(m_TextCanvas, m_PositionX, m_PositionY, msg_color, m_Style | (uint)(((m_Flag & 0x10) == 0) ? 16384 : 0), m_Priority, m_Buffer);
				}
				else
				{
					if ((m_Flag & 0x10) == 0 && (m_Flag & 2) == 0)
					{
						NNS_G2dTextCanvasDrawTaggedText(m_TextCanvas, m_PositionX + 1, m_PositionY + 1, 2, m_Buffer, funcDummyTagCallback, null);
					}
					NNS_G2dTextCanvasDrawTaggedText(m_TextCanvas, m_PositionX, m_PositionY, msg_color, m_Buffer, funcTagCallback, paramTagCallback);
				}
				m_Flag |= 2;
				m_Flag &= 251;
			}

			public bool visibility()
			{
				return (m_Flag & 0x80) == 0;
			}

			public bool activity()
			{
				return (m_Flag & 0x20) == 0;
			}

			public void setPriority(int p)
			{
				m_Priority = p;
			}

			public int priority()
			{
				return m_Priority;
			}

			public uint getStyle()
			{
				return m_Style;
			}

			public void setDisplaySpeed(byte s)
			{
				m_Speed = s;
			}

			public byte displaySpeed()
			{
				return m_Speed;
			}

			public void setDisplayWait(int w)
			{
				m_Counter = (m_Wait = w);
			}

			public int displayWait()
			{
				return m_Wait;
			}

			public uint getMessageID()
			{
				return msdElement.number;
			}

			public byte getCurrentPage()
			{
				return m_CurrentPage;
			}

			public byte numberOfPages()
			{
				return msdElement.num_pages;
			}

			public byte getCurrentChar()
			{
				return (byte)m_CurrentCharArray_offset;
			}

			public int getMessageColor()
			{
				return msg_color;
			}

			public NNSG2dTextCanvas getCanvas()
			{
				return m_TextCanvas;
			}
		}
	}
}
