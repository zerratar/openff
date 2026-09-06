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
	public static partial class sys2d
	{
		public class Bg
		{
			public static uint BG_FLAG_NOT_SHOW = 1u;

			public static uint BG_FLAG_AUTO_DELETE = 8u;

			public static uint BG_FLAG_MASK = 65535u;

			private Nscr _Nscr = new Nscr();

			private Ncgr _Ncgr = new Ncgr();

			private Nclr _Nclr = new Nclr();

			private NNSG2dScreenData _ScreenData;

			private NNSG2dBGSelect _BgSelect;

			private uint _Flag;

			private int[] _Position = new int[2];

			private GXBGScrBase _ScrBase;

			private GXBGCharBase _ChrBase;

			~Bg()
			{
				bgRelease();
			}

			public void bgLoad(string pSc, string pCg, string pCl)
			{
				if (pSc != null)
				{
					_Nscr.Load(pSc);
				}
				if (pCg != null)
				{
					_Ncgr.LoadBg(pCg);
				}
				if (pCl != null)
				{
					_Nclr.Load(pCl);
				}
			}

			public void bgLoad2(string pSame_name)
			{
				sprintf(out var arg, "%s.%s", pSame_name, "NCGR");
				sprintf(out var arg2, "%s.%s", pSame_name, "NCLR");
				sprintf(out var arg3, "%s.%s", pSame_name, "NSCR");
				bgLoad(arg3, arg, arg2);
			}

			public void bgSetUp(NNSG2dBGSelect bg_select, GXBGScrBase scr_base, GXBGCharBase chr_base)
			{
				if (_Nscr.pDataCe() != null)
				{
					NNS_G2dBGSetupCell((int)bg_select, _Nscr.pDataCe(), bg_select);
					NNS_G2dBGSetupChar((int)bg_select, _Ncgr.pDataCg());
					_BgSelect = bg_select;
					_Flag = BG_FLAG_NOT_SHOW;
					bgSetShow(show: true);
				}
				else
				{
					SVC_WaitVBlankIntr();
					NNS_G2dBGSetup(bg_select, _Nscr.pDataSc(), _Ncgr.pDataCg(), _Nclr.pDataCl(), scr_base, chr_base);
					_ScreenData = _Nscr.pDataSc();
					_BgSelect = bg_select;
					_ScrBase = scr_base;
					_ChrBase = chr_base;
					_Flag = BG_FLAG_NOT_SHOW;
					bgSetShow(show: true);
				}
			}

			public void bgSetUp(NNSG2dBGSelect bg_select)
			{
				bgGetBase(bg_select, out var scr_base, out var chr_base);
				bgSetUp(bg_select, scr_base, chr_base);
			}

			public void bgSetUpScr(NNSG2dBGSelect bg_select, GXBGScrBase scr_base, GXBGCharBase chr_base)
			{
				NNS_G2dBGSetup(bg_select, _Nscr.pDataSc(), null, null, scr_base, chr_base);
				_ScreenData = _Nscr.pDataSc();
				_BgSelect = bg_select;
				_ScrBase = scr_base;
				_ChrBase = chr_base;
				_Flag = BG_FLAG_NOT_SHOW;
				bgSetShow(show: true);
			}

			public void bgSetUpScr(NNSG2dBGSelect bg_select)
			{
				bgGetBase(bg_select, out var scr_base, out var chr_base);
				bgSetUpScr(bg_select, scr_base, chr_base);
			}

			public void bgGetBase(NNSG2dBGSelect bg, out GXBGScrBase scr_base, out GXBGCharBase chr_base)
			{
				scr_base = GXBGScrBase.GX_BG_SCRBASE_0x0000;
				chr_base = GXBGCharBase.GX_BG_CHARBASE_0x00000;
				switch (bg)
				{
				case NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN0:
					scr_base = (GXBGScrBase)G2_GetBG0Control().screenBase;
					chr_base = (GXBGCharBase)G2_GetBG0Control().charBase;
					break;
				case NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN1:
					scr_base = (GXBGScrBase)G2_GetBG1Control().screenBase;
					chr_base = (GXBGCharBase)G2_GetBG1Control().charBase;
					break;
				case NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN2:
					switch (GX_GetDispCnt().bgMode)
					{
					case 0:
					case 1:
					case 3:
						scr_base = (GXBGScrBase)G2_GetBG2ControlText().screenBase;
						chr_base = (GXBGCharBase)G2_GetBG2ControlText().charBase;
						break;
					case 2:
					case 4:
					case 5:
						switch (G2_GetBG2ExtMode())
						{
						case 0:
							scr_base = (GXBGScrBase)G2_GetBG2Control256x16Pltt().screenBase;
							chr_base = (GXBGCharBase)G2_GetBG2Control256x16Pltt().charBase;
							break;
						case 1:
							scr_base = (GXBGScrBase)G2_GetBG2Control256Bmp().screenBase;
							break;
						case 2:
							scr_base = (GXBGScrBase)G2_GetBG2ControlDCBmp().screenBase;
							break;
						}
						break;
					}
					break;
				case NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN3:
					if (GX_GetDispCnt().bgMode == 0)
					{
						scr_base = (GXBGScrBase)G2_GetBG3ControlText().screenBase;
						chr_base = (GXBGCharBase)G2_GetBG3ControlText().charBase;
						break;
					}
					switch (G2_GetBG3ExtMode())
					{
					case 0:
						scr_base = (GXBGScrBase)G2_GetBG3Control256x16Pltt().screenBase;
						chr_base = (GXBGCharBase)G2_GetBG3Control256x16Pltt().charBase;
						break;
					case 1:
						scr_base = (GXBGScrBase)G2_GetBG3Control256Bmp().screenBase;
						break;
					case 2:
						scr_base = (GXBGScrBase)G2_GetBG3ControlDCBmp().screenBase;
						break;
					}
					break;
				case NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB0:
					scr_base = (GXBGScrBase)G2S_GetBG0Control().screenBase;
					chr_base = (GXBGCharBase)G2S_GetBG0Control().charBase;
					break;
				case NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB1:
					scr_base = (GXBGScrBase)G2S_GetBG1Control().screenBase;
					chr_base = (GXBGCharBase)G2S_GetBG1Control().charBase;
					break;
				case NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB2:
					switch (GXS_GetDispCnt().bgMode)
					{
					case 0:
					case 1:
					case 3:
						scr_base = (GXBGScrBase)G2S_GetBG2ControlText().screenBase;
						chr_base = (GXBGCharBase)G2S_GetBG2ControlText().charBase;
						break;
					case 2:
					case 4:
					case 5:
						switch (G2S_GetBG2ExtMode())
						{
						case 0:
							scr_base = (GXBGScrBase)G2S_GetBG2Control256x16Pltt().screenBase;
							chr_base = (GXBGCharBase)G2S_GetBG2Control256x16Pltt().charBase;
							break;
						case 1:
							scr_base = (GXBGScrBase)G2S_GetBG2Control256Bmp().screenBase;
							break;
						case 2:
							scr_base = (GXBGScrBase)G2S_GetBG2ControlDCBmp().screenBase;
							break;
						}
						break;
					}
					break;
				case NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB3:
					if (GXS_GetDispCnt().bgMode == 0)
					{
						scr_base = (GXBGScrBase)G2S_GetBG3ControlText().screenBase;
						chr_base = (GXBGCharBase)G2S_GetBG3ControlText().charBase;
						break;
					}
					switch (G2S_GetBG3ExtMode())
					{
					case 0:
						scr_base = (GXBGScrBase)G2S_GetBG3Control256x16Pltt().screenBase;
						chr_base = (GXBGCharBase)G2S_GetBG3Control256x16Pltt().charBase;
						break;
					case 1:
						scr_base = (GXBGScrBase)G2S_GetBG3Control256Bmp().screenBase;
						break;
					case 2:
						scr_base = (GXBGScrBase)G2S_GetBG3ControlDCBmp().screenBase;
						break;
					}
					break;
				}
			}

			internal static int IsMainBG(NNSG2dBGSelect bg)
			{
				if (bg > NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN3)
				{
					return 0;
				}
				return 1;
			}

			internal static int GetBGCharOffset()
			{
				_ = reg_GX_DISPCNT;
				return 0;
			}

			internal static int GetBGScrOffset()
			{
				_ = reg_GX_DISPCNT;
				return 0;
			}

			public void LoadBGCharacter(NNSG2dBGSelect bg, NNSG2dCharacterData pChrData, NNSG2dCharacterPosInfo pPosInfo)
			{
				uint num = 1u;
				if (pPosInfo != null)
				{
					int num2 = pPosInfo.srcPosY * 8 * 16 * 8;
				}
				DC_FlushRange(pChrData.pRawData, pChrData.szByte);
				for (int i = 0; i < 4; i++)
				{
				}
			}

			public void bgReloadCg(string pCg, ushort x, ushort y)
			{
				Ncgr ncgr = new Ncgr();
				ncgr.LoadBg(pCg);
				NNSG2dCharacterData nNSG2dCharacterData = ncgr.pDataCg();
				NNSG2dCharacterPosInfo nNSG2dCharacterPosInfo = new NNSG2dCharacterPosInfo();
				nNSG2dCharacterPosInfo.srcPosX = x;
				nNSG2dCharacterPosInfo.srcPosY = y;
				nNSG2dCharacterPosInfo.srcW = nNSG2dCharacterData.W;
				nNSG2dCharacterPosInfo.srcH = nNSG2dCharacterData.H;
				LoadBGCharacter(_BgSelect, nNSG2dCharacterData, nNSG2dCharacterPosInfo);
			}

			public void bgReloadClEx(string pCl, uint pl_no)
			{
			}

			public void bgReloadCgClEx(string pSame_name, ushort x, ushort y, uint pl_no)
			{
				sprintf(out var arg, "%s.%s", pSame_name, "NCGR");
				bgReloadCg(arg, x, y);
				sprintf(out arg, "%s.%s", pSame_name, "NCLR");
				bgReloadClEx(arg, pl_no);
			}

			public void bgSetShow(bool show)
			{
				if (show != bgIsShow())
				{
					GXPlaneMask gXPlaneMask = static_cast<GXPlaneMask>(0);
					switch (_BgSelect)
					{
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN0:
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB0:
						gXPlaneMask = GXPlaneMask.GX_PLANEMASK_BG0;
						break;
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN1:
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB1:
						gXPlaneMask = GXPlaneMask.GX_PLANEMASK_BG1;
						break;
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN2:
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB2:
						gXPlaneMask = GXPlaneMask.GX_PLANEMASK_BG2;
						break;
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN3:
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB3:
						gXPlaneMask = GXPlaneMask.GX_PLANEMASK_BG3;
						break;
					}
					switch (_BgSelect)
					{
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN0:
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN1:
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN2:
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN3:
					{
						int num = GX_GetVisiblePlane();
						num = ((!show) ? (num & (int)(~gXPlaneMask)) : (num | (int)gXPlaneMask));
						GX_SetVisiblePlane(num);
						break;
					}
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB0:
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB1:
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB2:
					case NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB3:
					{
						int num = GXS_GetVisiblePlane();
						num = ((!show) ? (num & (int)(~gXPlaneMask)) : (num | (int)gXPlaneMask));
						GXS_SetVisiblePlane(num);
						break;
					}
					}
				}
				ds.switchFlag(BG_FLAG_NOT_SHOW, !show, ref _Flag);
			}

			public void bgClearScr()
			{
			}

			public void bgClearScr(ushort ch)
			{
			}

			public void bgClearScr(ushort sx, ushort sy, ushort w, ushort h, ushort ch)
			{
				for (ushort num = sy; num < sy + h; num++)
				{
					for (ushort num2 = sx; num2 < sx + w; num2++)
					{
						bgSetScrFmtTextCharacter(num2, num, ch);
					}
				}
			}

			public bool bgRewriteScreenData(int x, int y, ushort scr_data)
			{
				return true;
			}

			public ushort bgGetScreenData(int x, int y)
			{
				return 0;
			}

			public void bgSetPosition(int x, int y)
			{
				_Position[0] = x;
				_Position[1] = y;
			}

			public void bgCopyCg(Bg bg)
			{
				_Ncgr = bg.bgGetCg();
			}

			public void bgCopyCl(Bg bg)
			{
				_Nclr = bg.bgGetCl();
			}

			public bool bgIsShow()
			{
				return !ds.isFlag(BG_FLAG_NOT_SHOW, ref _Flag);
			}

			public Ncgr bgGetCg()
			{
				return _Ncgr;
			}

			public Nclr bgGetCl()
			{
				return _Nclr;
			}

			public Nscr bgGetSc()
			{
				return _Nscr;
			}

			public void bgRelease()
			{
				_Nscr.Release();
				_Ncgr.Release();
				_Nclr.Release();
			}

			public void bgSetScrFmtTextCharacter(int x, int y, ushort ch)
			{
				ushort num = bgGetScreenData(x, y);
				num = (ushort)((num & -1024) | ch);
			}

			public ushort bgGetScrFmtTextCharacter(int x, int y)
			{
				return (ushort)(bgGetScreenData(x, y) & 0x3FF);
			}

			public void bgGetPosition(out int x, out int y)
			{
				x = _Position[0];
				y = _Position[1];
			}
		}
	}
}
