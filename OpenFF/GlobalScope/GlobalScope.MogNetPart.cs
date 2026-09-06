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
						public class MogNetPart : sys.FF3GamePart
						{
							public static MogNetPart instance_ = new MogNetPart();

							protected mognet.MNSMediator mnsMediator;

							public static void registerPart()
							{
								sys.GGlobal.registerPart(GAMEPART.GAMEPART_MOG_NET, getSingleton());
							}

							public MogNetPart()
							{
								mnsMediator = null;
							}

							~MogNetPart()
							{
							}

							protected override void doInitialize()
							{
								TexDivideLoader.getSingleton().tdlCancel();
								SVC_WaitVBlankIntr();
								ovl.overlayRegister.ChangeOverlay(ovl.OVERLAYINDEX.PART_MOGNET);
								SVC_WaitVBlankIntr();
								if (mnsMediator != null)
								{
									mnsMediator.destruct();
								}
								mnsMediator = new mognet.MNSMediator();
								GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_MAIN_SUB);
								ds.CDevice.setup();
								GX_SetBGCharOffset(0);
								GX_SetBGScrOffset(0);
								G2_BlendNone();
								GX_DispOn();
								GXS_DispOn();
								G3X_SetClearColor(GX_RGB(0, 8, 26), 31, 32767, 1, 0);
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
								mnsMediator.mnsMediatorVRAMSetting();
								dgs.msg.CMessageSys.getInstance().Main().initialize();
								dgs.msg.CMessageSys.getInstance().Sub().initialize();
								dgs.msg.CMessageSys.getInstance().Main().assignBG(3, 0, 0, 32, 24);
								dgs.msg.CMessageSys.getInstance().Sub().assignBG(3, 0, 0, 32, 24);
								sys2d.DS2DManager.d2dGetInstance().d2dInitialize();
								menu.MenuManager.getSingleton().Set2d3dMode(2);
								menu.MenuManager.getSingleton().initialize();
								menu.MenuManager.getSingleton().CreateNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_2D);
								menu.MenuManager.getSingleton().SetUsingMenuType(1);
								menu.MenuManager.getSingleton().SetWindowSystem();
								G2_SetBG0Offset(0, 0);
								G2_SetBG1Offset(0, 0);
								G2_SetBG2Offset(0, 0);
								G2_SetBG3Offset(0, 0);
								G2S_SetBG0Offset(0, 0);
								G2S_SetBG1Offset(0, 0);
								G2S_SetBG2Offset(0, 0);
								G2S_SetBG3Offset(0, 0);
								MatrixSound.MtxSENDS_Load(96);
								changeCompanyDirectory();
								menu.MenuManager.getSingleton().LoadXbnFile("MogNet.xbn");
								mnsMediator.mnsMediatorInitialize();
								mognet.MNMemento.getSingleton().mnmInitialize();
								mognet.MNNPCMailData.getSingleton().initialize();
							}

							protected override void doUninitialize()
							{
								mognet.MNMemento.getSingleton().mnmUpdateUserDataBackup();
								mognet.MNMemento.getSingleton().mnmUpdateFriendListBackup();
								ds.CDevice.setProhibitSleepMode(flag: false);
								mnsMediator.mnsMediatorTerminate();
								mnsMediator.destruct();
								mnsMediator = null;
								mognet.MNNPCMailData.getSingleton().finalize();
								menu.MenuManager.getSingleton().DeleteNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
								menu.MenuManager.getSingleton().ReleaseXbnFile();
								menu.MenuManager.getSingleton().releaseAll();
								menu.MenuManager.getSingleton().terminate();
								menu.MenuManager.getSingleton().ResetWindowSystem();
								MatrixSound.MtxSENDS_Unload();
							}

							protected override void doSleep()
							{
							}

							protected override void doWakeUp()
							{
							}

							protected override void onDrawPart()
							{
								sys2d.DS2DManager.d2dGetInstance().d2dDraw();
								dgs.msg.CMessageSys.getInstance().Main().dgsMMDraw();
								sys2d.DS2DManager.d2dGetInstance().d2dDrawScreen(depthtest: false);
							}

							protected override void onExecutePart()
							{
								sys2d.DS2DManager.d2dGetInstance().d2dExecute();
								menu.MenuManager.getSingleton().execute();
								if (!mnsMediator.mnsMediatorProcess())
								{
									sys.GGlobal.setNextPart(sys.GGlobal.getPreviousPart());
									abort();
								}
							}

							protected override void onDrawEffector()
							{
							}

							protected override void onUpdatePart()
							{
								sys2d.DS2DManager.d2dGetInstance().d2dUpdate();
							}

							public static MogNetPart getSingleton()
							{
								return instance_;
							}
						}
}
