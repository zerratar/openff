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
	public static partial class ds
	{
		public class CVram
		{
			public const int MODE_TEX = 0;

			public const int MODE_MAIN_BG = 1;

			public const int MODE_MAIN_OBJ = 2;

			public const int MODE_SUB_BG = 3;

			public const int MODE_SUB_OBJ = 4;

			public static CVram m_instance = new CVram();

			private uint m_szTexByte;

			private uint m_szTex4x4Byte;

			private uint m_szPlttByte;

			private uint m_numTexMemBlock;

			private uint m_numPlttMemBlock;

			private Array m_pTexMngWork;

			private Array m_pPlttMngWork;

			private GXVRamTex _texBank;

			private GXVRamTexPltt _plttBank;

			public static void initialize()
			{
				GX_SetBankForLCDC(0);
				GX_DisableBankForLCDC();
			}

			public static void setup()
			{
				NNS_GfdInitFrmTexVramManager(4, 1);
				NNS_GfdInitFrmPlttVramManager(32768u, 1);
				NNS_GfdResetFrmTexVramState();
			}

			public static void clear()
			{
				GX_SetBankForTex(GXVRamTex.GX_VRAM_TEX_NONE);
				GX_SetBankForTexPltt(GXVRamTexPltt.GX_VRAM_TEXPLTT_NONE);
				GX_SetBankForBG(GXVRamBG.GX_VRAM_BG_NONE);
				GX_SetBankForBGExtPltt(GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_NONE);
				GX_SetBankForOBJ(GXVRamOBJ.GX_VRAM_OBJ_NONE);
				GX_SetBankForOBJExtPltt(GXVRamOBJExtPltt.GX_VRAM_OBJEXTPLTT_NONE);
				GX_SetBankForSubBG(GXVRamSubBG.GX_VRAM_SUB_BG_NONE);
				GX_SetBankForSubBGExtPltt(GXVRamSubBGExtPltt.GX_VRAM_SUB_BGEXTPLTT_NONE);
				GX_SetBankForSubOBJ(GXVRamSubOBJ.GX_VRAM_SUB_OBJ_NONE);
				GX_SetBankForSubOBJExtPltt(GXVRamSubOBJExtPltt.GX_VRAM_SUB_OBJEXTPLTT_NONE);
				GX_DisableBankForTex();
				GX_DisableBankForTexPltt();
				GX_DisableBankForBG();
				GX_DisableBankForBGExtPltt();
				GX_DisableBankForOBJ();
				GX_DisableBankForOBJExtPltt();
				GX_DisableBankForSubBG();
				GX_DisableBankForSubBGExtPltt();
				GX_DisableBankForSubOBJ();
				GX_DisableBankForSubOBJExtPltt();
			}

			public CVram()
			{
				m_szTexByte = 0u;
				m_szTex4x4Byte = 0u;
				m_szPlttByte = 0u;
				m_numTexMemBlock = 0u;
				m_numPlttMemBlock = 0u;
				m_pTexMngWork = null;
				m_pPlttMngWork = null;
				_texBank = GXVRamTex.GX_VRAM_TEX_012_ACD;
				_plttBank = GXVRamTexPltt.GX_VRAM_TEXPLTT_01_FG;
			}

			public void setupTexVramMng(uint szByte, uint szByteFor4x4, uint numMemBlock, int useAsDefault)
			{
				m_szTexByte = szByte;
				m_szTex4x4Byte = szByteFor4x4;
				m_numTexMemBlock = numMemBlock;
				uint num = (uint)NNS_GfdGetLnkTexVramManagerWorkSize(m_numTexMemBlock);
				m_pTexMngWork = CHeap.alloc_app(num);
				NNS_GfdInitLnkTexVramManager(szByte, szByteFor4x4, m_pTexMngWork, num, useAsDefault);
			}

			public void setupPlttVramMng(uint szByte, uint numMemBlock, int useAsDefault)
			{
				m_szPlttByte = szByte;
				m_numPlttMemBlock = numMemBlock;
				uint num = NNS_GfdGetLnkPlttVramManagerWorkSize(m_numPlttMemBlock);
				m_pPlttMngWork = CHeap.alloc_app(num);
				NNS_GfdInitLnkPlttVramManager(szByte, m_pPlttMngWork, num, useAsDefault);
			}

			public void releaseTexVramMng()
			{
				if (m_pTexMngWork != null)
				{
					CHeap.free_app(m_pTexMngWork);
					m_pTexMngWork = null;
				}
				NNS_GfdResetLnkTexVramState();
			}

			public void releasePlttVramMng()
			{
				if (m_pPlttMngWork != null)
				{
					CHeap.free_app(m_pPlttMngWork);
					m_pPlttMngWork = null;
				}
				NNS_GfdResetLnkPlttVramState();
			}

			public void setupBankForTex()
			{
			}

			public void setupBankForPltt()
			{
			}

			public static void setMainBGPriority(int bg0, int bg1, int bg2, int bg3)
			{
				G2_SetBG0Priority(bg0);
				G2_SetBG1Priority(bg1);
				G2_SetBG2Priority(bg2);
				G2_SetBG3Priority(bg3);
			}

			public static void setSubBGPriority(int bg0, int bg1, int bg2, int bg3)
			{
				G2S_SetBG0Priority(bg0);
				G2S_SetBG1Priority(bg1);
				G2S_SetBG2Priority(bg2);
				G2S_SetBG3Priority(bg3);
			}

			public static void setMainPlaneVisiblity(bool bg0, bool bg1, bool bg2, bool bg3, bool obj)
			{
				uint num = 0u;
				if (bg0)
				{
					num |= 1;
				}
				if (bg1)
				{
					num |= 2;
				}
				if (bg2)
				{
					num |= 4;
				}
				if (bg3)
				{
					num |= 8;
				}
				if (obj)
				{
					num |= 0x10;
				}
				GX_SetVisiblePlane((int)num);
			}

			public static void setSubPlaneVisiblity(bool bg0, bool bg1, bool bg2, bool bg3, bool obj)
			{
				uint num = 0u;
				if (bg0)
				{
					num |= 1;
				}
				if (bg1)
				{
					num |= 2;
				}
				if (bg2)
				{
					num |= 4;
				}
				if (bg3)
				{
					num |= 8;
				}
				if (obj)
				{
					num |= 0x10;
				}
				GXS_SetVisiblePlane((int)num);
			}

			public VRAMKEY alloc(uint sizeTex, uint sizeTex4x4, uint sizePltt)
			{
				VRAMKEY vRAMKEY = new VRAMKEY();
				vRAMKEY.keyTex = 0u;
				vRAMKEY.keyTex4x4 = 0u;
				vRAMKEY.keyPltt = 0u;
				return vRAMKEY;
			}

			public void free(VRAMKEY key)
			{
			}

			public static CVram getInstance()
			{
				return m_instance;
			}

			public void setBankForTex(GXVRamTex bank)
			{
				_texBank = bank;
			}

			public GXVRamTex getBankForTex()
			{
				return _texBank;
			}

			public void setBankForPltt(GXVRamTexPltt bank)
			{
				_plttBank = bank;
			}

			public GXVRamTexPltt getBankForPltt()
			{
				return _plttBank;
			}
		}
	}
}
