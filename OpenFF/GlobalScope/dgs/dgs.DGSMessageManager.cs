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
		public class DGSMessageManager
		{
			public class POINT
			{
				public int x;

				public int y;

				public POINT(int arg0, int arg1)
				{
					x = arg0;
					y = arg1;
				}
			}

			private static int LIMIT_OF_CANVAS = 4;

			public ds.Vector<NNSG2dCharCanvas, ds.FastErasePolicy<NNSG2dCharCanvas>> dgsmCanvasVector = new ds.Vector<NNSG2dCharCanvas, ds.FastErasePolicy<NNSG2dCharCanvas>>(LIMIT_OF_CANVAS);

			private ds.Vector<POINT, ds.FastErasePolicy<POINT>> dgsmCanvasPointVector = new ds.Vector<POINT, ds.FastErasePolicy<POINT>>(LIMIT_OF_CANVAS);

			private static int LIMIT_OF_MSD = 5;

			private ds.Vector<MSDINFO, ds.FastErasePolicy<MSDINFO>> dgsmTextVector = new ds.Vector<MSDINFO, ds.FastErasePolicy<MSDINFO>>(LIMIT_OF_MSD);

			private TARGETLCD currentTarget;

			private DGSPlane dgspTarget;

			public DGSMessageManager()
			{
				dgsmTextVector.clear();
				dgspTarget = null;
				currentTarget = TARGETLCD.TLCD_MAIN;
			}

			public void assignBG(int bg_number, TARGETLCD target, int x, int y, int w, int h)
			{
				initialize();
				dgspTarget = dgsPlanes[bg_number];
				currentTarget = target;
				dgspTarget.changeMode((uint)currentTarget);
				dgspTarget.Clear();
				GX_LoadBGPltt(TXTColorPalette, 480u, 32u);
				GXS_LoadBGPltt(TXTColorPalette, 480u, 32u);
				createCanvas(x, y, w, h);
			}

			public void initialize()
			{
				dgsMMErase();
				dgsmCanvasVector.clear();
				dgsmCanvasPointVector.clear();
				dgsmTextVector.clear();
			}

			public int initMSD(MSDINFO addr)
			{
				int result = INVALID_MSDHANDLE;
				bool flag = true;
				for (int num = dgsmTextVector.size() - 1; num >= 0; num--)
				{
					if (dgsmTextVector[num] == addr)
					{
						result = num;
						flag = false;
						break;
					}
				}
				if (flag)
				{
					dgsmTextVector.push_back(addr);
					result = dgsmTextVector.size() - 1;
				}
				return result;
			}

			public int searchMessageIndexFromID(uint msg_number)
			{
				for (int num = dgsmTextVector.size() - 1; num >= 0; num--)
				{
					MSDINFO mSDINFO = dgsmTextVector[num];
					for (int i = 0; i < mSDINFO.num_msg; i++)
					{
						if (mSDINFO.elements[i].number == msg_number)
						{
							return num;
						}
					}
				}
				return -1;
			}

			public void removeMSD(MSDINFO addr)
			{
				for (int num = dgsmTextVector.size() - 1; num >= 0; num--)
				{
					if (dgsmTextVector[num] == addr)
					{
						dgsmTextVector.erase(num);
						break;
					}
				}
			}

			public int createCanvas(int x, int y, int w, int h)
			{
				int[] array = (int[])dgspTarget.GetBGCharPtr();
				int num = 0;
				for (int num2 = dgsmCanvasVector.size() - 1; num2 >= 0; num2--)
				{
					num += dgsmCanvasVector[num2].areaWidth * dgsmCanvasVector[num2].areaHeight;
				}
				OS_Printf("pCharBase [0x%08x] + cOffset [%d]\n", array, num);
				dgsmCanvasVector.push_back(new NNSG2dCharCanvas());
				dgsmCanvasPointVector.push_back(new POINT(x, y));
				int num3 = dgsmCanvasVector.size() - 1;
				OS_Printf("dgsmCanvasVector.size() [%d]\n", dgsmCanvasVector.size());
				NNS_G2dCharCanvasInitForBG(dgsmCanvasVector[num3], array, num, w, h, NNSG2dCharaColorMode.NNS_G2D_CHARA_COLORMODE_16, (int)currentTarget);
				OS_Printf("NNSG2dCharCanvas\n");
				OS_Printf("\tcharBase[0x%08x]\n", dgsmCanvasVector[num3].charBase);
				OS_Printf("\tareaWidth[%d]\n", dgsmCanvasVector[num3].areaWidth);
				OS_Printf("\tareaHeight[%d]\n", dgsmCanvasVector[num3].areaHeight);
				OS_Printf("\tdstBpp[%d]\n", dgsmCanvasVector[num3].dstBpp);
				OS_Printf("\tparam[%d]\n", dgsmCanvasVector[num3].param);
				OS_Printf("\tparam[%d]\n", dgsmCanvasVector[num3].param);
				NNS_G2dMapScrToCharText(dgspTarget.GetBGScrPtr(), w, h, 0, 0, NNSG2dTextBGWidth.NNS_G2D_TEXT_BG_WIDTH_256, num, 15);
				OS_Printf("dgspTarget.GetBGScrPtr() [0x%08x]\n", dgspTarget.GetBGScrPtr());
				NNS_G2dCharCanvasClear(dgsmCanvasVector[num3], 0);
				return num3;
			}

			public DGSMessage createMessage(uint msg_number, int handle, int font)
			{
				DGSMessage dGSMessage = null;
				if (handle == INVALID_MSDHANDLE)
				{
					for (int num = dgsmTextVector.size() - 1; num >= 0; num--)
					{
						MSDINFO mSDINFO = dgsmTextVector[num];
						for (int i = 0; i < mSDINFO.num_msg; i++)
						{
							if (mSDINFO.elements[i].number == msg_number)
							{
								dGSMessage = new DGSMessage();
								NNS_G2dTextCanvasInit(dGSMessage.m_TextCanvas, dgsmCanvasVector[0], dgsmFontVector[font], 0, 0);
								dGSMessage.assignText(mSDINFO, mSDINFO.elements[i]);
							}
							if (dGSMessage != null)
							{
								break;
							}
						}
						if (dGSMessage != null)
						{
							break;
						}
					}
					if (dGSMessage == null)
					{
						return null;
					}
				}
				else
				{
					MSDINFO mSDINFO2 = dgsmTextVector[handle];
					for (int j = 0; j < mSDINFO2.num_msg; j++)
					{
						if (mSDINFO2.elements[j].number == msg_number)
						{
							dGSMessage = new DGSMessage();
							NNS_G2dTextCanvasInit(dGSMessage.m_TextCanvas, dgsmCanvasVector[0], dgsmFontVector[font], 0, 0);
							dGSMessage.assignText(mSDINFO2, mSDINFO2.elements[j]);
							break;
						}
					}
					if (dGSMessage == null)
					{
						return null;
					}
				}
				dGSMessage.m_Manager = this;
				dGSMessage.reset(doErase: false);
				return dGSMessage;
			}

			public DGSMessage createMessage(string str, int font)
			{
				DGSMessage dGSMessage = new DGSMessage();
				if (dGSMessage != null)
				{
					NNS_G2dTextCanvasInit(dGSMessage.m_TextCanvas, dgsmCanvasVector[0], dgsmFontVector[font], 0, 0);
					dGSMessage.m_Manager = this;
					dGSMessage.reset(doErase: false);
					dGSMessage.assignText(str);
				}
				return dGSMessage;
			}

			public void writeCharacterString(short x, short y, short hspace, short vspace, TXT_COLOR color, uint align, uint number, bool shadow, int font)
			{
				if (dgsmTextVector.empty())
				{
					OS_Printf("\nMSDが登録されていない\n");
					return;
				}
				string text = null;
				for (int num = dgsmTextVector.size() - 1; num >= 0; num--)
				{
					MSDINFO mSDINFO = dgsmTextVector[num];
					for (int i = 0; i < mSDINFO.num_msg; i++)
					{
						if (mSDINFO.elements[i].number == number)
						{
							text = StringUtil.createString(mSDINFO.m_abyData, (int)mSDINFO.elements[i].offset);
						}
						if (text != null)
						{
							break;
						}
					}
					if (text != null)
					{
						break;
					}
				}
				writeCharacterString(x, y, hspace, vspace, color, align, text, shadow, font);
			}

			public void writeCharacterString(short x, short y, short w, short h, short hspace, short vspace, TXT_COLOR color, uint align, uint number, bool shadow, int font)
			{
				if (dgsmTextVector.empty())
				{
					OS_Printf("\nMSDが登録されていない\n");
					return;
				}
				string text = null;
				for (int num = dgsmTextVector.size() - 1; num >= 0; num--)
				{
					MSDINFO mSDINFO = dgsmTextVector[num];
					for (int i = 0; i < mSDINFO.num_msg; i++)
					{
						if (mSDINFO.elements[i].number == number)
						{
							text = StringUtil.createString(mSDINFO.m_abyData, (int)mSDINFO.elements[i].offset);
						}
						if (text != null)
						{
							break;
						}
					}
					if (text != null)
					{
						break;
					}
				}
				writeCharacterString(x, y, w, h, hspace, vspace, color, align, text, shadow, font);
			}

			public void writeCharacterString(short x, short y, short hspace, short vspace, TXT_COLOR color, uint align, string cstr, bool shadow, int font)
			{
				if (dgsmCanvasVector.empty())
				{
					OS_Printf("\nassignBG()されていない\n");
					return;
				}
				NNSG2dTextCanvas pTxn = new NNSG2dTextCanvas();
				NNS_G2dTextCanvasInit(pTxn, dgsmCanvasVector[0], dgsmFontVector[font], hspace, vspace);
				NNS_G2dTextCanvasDrawText(pTxn, x, y, (int)color, align | (uint)(shadow ? 16384 : 0), 0, cstr);
			}

			internal static string escape_sequence(string current, short linefeed, ref short pX, ref short pY, short pBaseX)
			{
				int i;
				for (i = 0; current[i] != 0; i++)
				{
					if (current[i] == '\n')
					{
						pY += linefeed;
						pX = pBaseX;
						continue;
					}
					return current.Substring(i);
				}
				return current.Substring(i);
			}

			public void writeCharacterString(short x, short y, short w, short h, short hspace, short vspace, TXT_COLOR color, uint align, string cstr, bool shadow, int font)
			{
				if (dgsmCanvasVector.empty())
				{
					OS_Printf("\nassignBG()されていない\n");
					return;
				}
				short pX;
				short num = (pX = x);
				short pY = y;
				short num2 = (short)(NNS_G2dFontGetHeight(dgsmFontVector[font]) + vspace);
				string text;
				for (text = cstr; text != null; text = text.Substring(1))
				{
					text = escape_sequence(text, num2, ref pX, ref pY, num);
					if (text[0] == '\0')
					{
						break;
					}
					ushort ccode = (ushort)(text[0] & 0xFF);
					NNS_G2dCharCanvasDrawChar(dgsmCanvasVector[0], dgsmFontVector[font], pX + 1, pY + 1, 2, ccode);
					int num3 = NNS_G2dCharCanvasDrawChar(dgsmCanvasVector[0], dgsmFontVector[font], pX, pY, (int)color, ccode);
					pX += (short)num3;
					if (pX >= w)
					{
						pX = num;
						pY += num2;
					}
				}
			}

			public void writeCharacterStringCC(short x, short y, short hspace, short vspace, TXT_COLOR color, uint align, uint number, ref string buffer, bool shadow, int font)
			{
				if (dgsmTextVector.empty())
				{
					OS_Printf("\nMSDが登録されていない\n");
					return;
				}
				string text = null;
				for (int num = dgsmTextVector.size() - 1; num >= 0; num--)
				{
					MSDINFO mSDINFO = dgsmTextVector[num];
					for (int i = 0; i < mSDINFO.num_msg; i++)
					{
						if (mSDINFO.elements[i].number == number)
						{
							text = StringUtil.createString(mSDINFO.m_abyData, (int)mSDINFO.elements[i].offset);
						}
						if (text != null)
						{
							break;
						}
					}
					if (text != null)
					{
						break;
					}
				}
				int iStrOffset = 0;
				int iBufferOffset = 0;
				char[] array = text.ToCharArray();
				char[] array2 = new char[array.Length * 2];
				while (iStrOffset != text.Length)
				{
					if (text[iStrOffset] == '%')
					{
						CtrlCodeProcessing(text.ToCharArray(), array2, ref iStrOffset, ref iBufferOffset);
						iStrOffset++;
						iBufferOffset++;
					}
					array2[iBufferOffset] = text[iStrOffset];
					iStrOffset++;
					iBufferOffset++;
				}
				buffer = new string(array2, 0, iBufferOffset);
				writeCharacterString(x, y, hspace, vspace, color, align, buffer, shadow, font);
			}

			public string getMessage(uint msg_number)
			{
				for (int num = dgsmTextVector.size() - 1; num >= 0; num--)
				{
					MSDINFO mSDINFO = dgsmTextVector[num];
					for (int i = 0; i < mSDINFO.num_msg; i++)
					{
						if (mSDINFO.elements[i].number == msg_number)
						{
							return StringUtil.createString(mSDINFO.m_abyData, (int)mSDINFO.elements[i].offset);
						}
					}
				}
				return null;
			}

			public void dgsMMErase()
			{
				while (DGSLinkedList<DGSMessage>.dgsllBase() != null)
				{
					((DGSMessage)DGSLinkedList<DGSMessage>.dgsllBase()).release();
				}
			}

			public void dgsMMAreaErase(short x, short y, short w, short h)
			{
				for (int num = dgsmCanvasVector.size() - 1; num >= 0; num--)
				{
					short x2 = (short)(x - dgsmCanvasPointVector[num].x);
					short y2 = (short)(y - dgsmCanvasPointVector[num].y);
					short num2 = w;
					short num3 = h;
					if (0 < num2 && 0 < num3)
					{
						NNS_G2dCharCanvasClearArea(dgsmCanvasVector[num], null, 0, x2, y2, num2, num3);
					}
				}
			}

			public void dgsMMDraw()
			{
				while (g_DelayedEraseOrder.empty() == 0)
				{
					DelayedEraseArea delayedEraseArea = g_DelayedEraseOrder.top();
					NNS_G2dCharCanvasClearArea(delayedEraseArea.canvas, delayedEraseArea.font, 0, delayedEraseArea.area.x, delayedEraseArea.area.y, delayedEraseArea.area.width, delayedEraseArea.area.height);
					g_DelayedEraseOrder.pop();
				}
				if (DGSLinkedList<DGSMessage>.dgsllBase() != null)
				{
					GX_LoadBGPltt(TXTColorPalette, 480u, 32u);
					GXS_LoadBGPltt(TXTColorPalette, 480u, 32u);
				}
				for (DGSMessage dGSMessage = (DGSMessage)DGSLinkedList<DGSMessage>.dgsllBase(); dGSMessage != null; dGSMessage = (DGSMessage)dGSMessage.dgsllNext())
				{
					dGSMessage.draw();
				}
			}
		}
	}
}
