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
							public class CBaseWorldVram
							{
								public void setup()
								{
									disableBank();
									enableBank();
									setUpTexPlttVramMng();
									clearBGandOBJ();
									initBGandOBJ();
								}

								public void cleanup()
								{
									disableBank();
									clearBGandOBJ();
									cleanUpTexPlttVramMng();
								}

								public void disableBank()
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

								public void clearBGandOBJ()
								{
								}

								protected virtual void enableBank()
								{
								}

								protected virtual void initBGandOBJ()
								{
								}

								protected virtual void setUpTexPlttVramMng()
								{
								}

								protected virtual void cleanUpTexPlttVramMng()
								{
								}
							}
	}
}
