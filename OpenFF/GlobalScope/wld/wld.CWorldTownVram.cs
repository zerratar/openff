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
	public static partial class wld
	{
							public class CWorldTownVram : CBaseWorldVram
							{
								protected override void enableBank()
								{
									GXVRamTex gXVRamTex = GXVRamTex.GX_VRAM_TEX_012_ABD;
									GXVRamTexPltt gXVRamTexPltt = GXVRamTexPltt.GX_VRAM_TEXPLTT_01_FG;
									GX_SetBankForTex(gXVRamTex);
									GX_SetBankForTexPltt(gXVRamTexPltt);
									ds.CVram.getInstance().setBankForTex(gXVRamTex);
									ds.CVram.getInstance().setBankForPltt(gXVRamTexPltt);
									GX_SetBankForBG(GXVRamBG.GX_VRAM_BG_64_E);
									GX_SetBankForBGExtPltt(GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_NONE);
									GX_SetBankForOBJ(GXVRamOBJ.GX_VRAM_OBJ_NONE);
									GX_SetBankForOBJExtPltt(GXVRamOBJExtPltt.GX_VRAM_OBJEXTPLTT_NONE);
									GX_SetBankForSubBG(GXVRamSubBG.GX_VRAM_SUB_BG_128_C);
									GX_SetBankForSubBGExtPltt(GXVRamSubBGExtPltt.GX_VRAM_SUB_BGEXTPLTT_0123_H);
									GX_SetBankForSubOBJ(GXVRamSubOBJ.GX_VRAM_SUB_OBJ_16_I);
									GX_SetBankForSubOBJExtPltt(GXVRamSubOBJExtPltt.GX_VRAM_SUB_OBJEXTPLTT_NONE);
									G3X_SetClearColor(GX_RGB(0, 0, 0), 31, 32767, 1, 0);
								}

								protected override void initBGandOBJ()
								{
									ds.CVram.setMainPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: true, obj: false);
									ds.CVram.setSubPlaneVisiblity(bg0: false, bg1: false, bg2: false, bg3: true, obj: true);
									ds.CVram.setMainBGPriority(3, 2, 1, 0);
									ds.CVram.setSubBGPriority(1, 2, 0, 3);
									GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_GRAPHICS, GXBGMode.GX_BGMODE_0, 0);
									GXS_SetGraphicsMode(GXBGMode.GX_BGMODE_0);
									G2_SetBG2ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0800, GXBGCharBase.GX_BG_CHARBASE_0x04000);
									G2_SetBG3ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x1800, GXBGCharBase.GX_BG_CHARBASE_0x08000);
									G2S_SetBG1Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x6800, GXBGCharBase.GX_BG_CHARBASE_0x08000, 0);
									G2S_SetBG2ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x7000, GXBGCharBase.GX_BG_CHARBASE_0x10000);
									G2S_SetBG3ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x7800, GXBGCharBase.GX_BG_CHARBASE_0x08000);
								}

								protected override void setUpTexPlttVramMng()
								{
									ds.CVram.getInstance().setupTexVramMng(393216u, 81920u, 64u, 0);
									ds.CVram.getInstance().setupPlttVramMng(32768u, 64u, 0);
								}

								protected override void cleanUpTexPlttVramMng()
								{
									ds.CVram.getInstance().releaseTexVramMng();
									ds.CVram.getInstance().releasePlttVramMng();
								}
							}
	}
}
