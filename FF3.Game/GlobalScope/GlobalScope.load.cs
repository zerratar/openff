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
using android.text;
using android.widget;
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static class load
	{
		public class CLoadVram : wld.CBaseWorldVram
		{
			protected override void enableBank()
			{
				GX_SetBGCharOffset(0);
				GX_SetBGScrOffset(0);
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
				GXVRamTex gXVRamTex = GXVRamTex.GX_VRAM_TEX_NONE;
				GXVRamTexPltt gXVRamTexPltt = GXVRamTexPltt.GX_VRAM_TEXPLTT_NONE;
				GX_SetBankForTex(gXVRamTex);
				GX_SetBankForTexPltt(gXVRamTexPltt);
				ds.CVram.getInstance().setBankForTex(gXVRamTex);
				ds.CVram.getInstance().setBankForPltt(gXVRamTexPltt);
				GX_SetBankForBG(GXVRamBG.GX_VRAM_BG_128_A);
				GX_SetBankForBGExtPltt(GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_0123_E);
				GX_SetBankForOBJ(GXVRamOBJ.GX_VRAM_OBJ_128_B);
				GX_SetBankForOBJExtPltt(GXVRamOBJExtPltt.GX_VRAM_OBJEXTPLTT_0_F);
				GX_SetBankForSubBG(GXVRamSubBG.GX_VRAM_SUB_BG_128_C);
				GX_SetBankForSubBGExtPltt(GXVRamSubBGExtPltt.GX_VRAM_SUB_BGEXTPLTT_0123_H);
				GX_SetBankForSubOBJ(GXVRamSubOBJ.GX_VRAM_SUB_OBJ_128_D);
				GX_SetBankForSubOBJExtPltt(GXVRamSubOBJExtPltt.GX_VRAM_SUB_OBJEXTPLTT_0_I);
			}

			protected override void setUpTexPlttVramMng()
			{
			}

			protected override void cleanUpTexPlttVramMng()
			{
			}

			protected override void initBGandOBJ()
			{
				ds.CVram.setMainPlaneVisiblity(bg0: false, bg1: false, bg2: false, bg3: false, obj: false);
				ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: true, bg2: true, bg3: false, obj: true);
				ds.CVram.setMainBGPriority(3, 2, 1, 0);
				ds.CVram.setSubBGPriority(1, 2, 0, 3);
				GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_GRAPHICS, GXBGMode.GX_BGMODE_0, 0);
				GXS_SetGraphicsMode(GXBGMode.GX_BGMODE_0);
				G2S_SetBG0Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0000, GXBGCharBase.GX_BG_CHARBASE_0x04000, 0);
				G2S_SetBG1Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0800, GXBGCharBase.GX_BG_CHARBASE_0x0c000, 0);
				G2S_SetBG2ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x1000, GXBGCharBase.GX_BG_CHARBASE_0x14000);
				G2S_SetBG3ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x1800, GXBGCharBase.GX_BG_CHARBASE_0x18000);
				G2S_SetBlendBrightness(0, 0);
			}
		}

		public class LoadPart : sys.FF3GamePart
		{
			public enum EndPhase
			{
				FadeStart,
				NextPart,
				EndPhaseMax
			}

			public const EndPhase FadeStart = EndPhase.FadeStart;

			public const EndPhase NextPart = EndPhase.NextPart;

			public const EndPhase EndPhaseMax = EndPhase.EndPhaseMax;

			public static LoadPart instance_ = new LoadPart();

			private int partID_;

			private GAMEPART partIDAfter_;

			private EndPhase m_Phase;

			private CLoadVram m_Vram = new CLoadVram();

			private bool backupAccess_;

			public static void registerPart()
			{
				sys.GGlobal.registerPart(GAMEPART.GAMEPART_LOAD, instance_);
				instance_.setPartID(0);
			}

			public LoadPart()
			{
				partID_ = 0;
			}

			~LoadPart()
			{
			}

			public void setPartID(int i)
			{
				partID_ = i;
			}

			protected override void doInitialize()
			{
				ovl.overlayRegister.ChangeOverlay(ovl.OVERLAYINDEX.PART_WORLD);
				ds.CHeap.setID_app(0);
				m_Vram.setup();
				GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_MAIN_SUB);
				G2_BlendNone();
				sys2d.DS2DManager.d2dGetInstance().d2dInitialize();
				dgs.msg.CMessageSys.getInstance().initialize();
				dgs.msg.CMessageSys.getInstance().Sub().assignBG(0, 0, 0, 32, 24);
				ds.CDevice.singleton().setFPS(ds.CDevice.enFPS.enFPS_30);
				dgs.CCurtain.Top().setVisible(visible: false);
				dgs.CCurtain.Middle().setVisible(visible: false);
				dgs.CCurtain.Bottom().setVisible(visible: false);
				dgs.CCurtain.Top().setEnable(enable: false);
				dgs.CCurtain.Middle().setEnable(enable: false);
				dgs.CCurtain.Bottom().setEnable(enable: false);
				ds.CHeap.setID_app(50);
				menu.MenuManager.getSingleton().Set2d3dMode(2);
				ds.CHeap.setID_app(51);
				changeCompanyDirectory();
				menu.MenuManager.getSingleton().initialize();
				menu.MenuManager.getSingleton().LoadXbnFile("MenuDefine.xbn");
				ds.CHeap.setID_app(52);
				menu.MenuManager.getSingleton().CreateNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_2D);
				menu.MenuManager.getSingleton().SetUsingMenuType(1);
				ds.CHeap.setID_app(50);
				menu.MenuManager.getSingleton().CreateMenuDataText(0);
				menu.MenuManager.getSingleton().SetWindowSystem();
				dgs.CFade.Main().fadeIn(15);
				dgs.CFade.Sub().fadeIn(15);
				if (!checkCard())
				{
					ds.Sound.Stop();
					menu.MenuManager.getSingleton().buildMenu("suspend_failed");
					backupAccessFailedSetting();
					backupAccess_ = false;
				}
				else
				{
					backupAccess_ = true;
					changeGlobalDirectory();
					ds.CHeap.setID_app(55);
					_backBg.bgLoad("menu_009_save.NSCR", "menu_bg_01.NCGR", "new_menu_bg.NCLR");
					_backBg.bgSetUp(NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB3);
					_backBg.bgRelease();
					wmenu.CWMenuManager.Instance().SetupSecondlyBG(2);
					wmenu.CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
					ds.CHeap.setID_app(53);
					menu.CMenuSaveLoad.singleton().setCurrentMode(menu.CMenuSaveLoad.MODE.MODE_LOAD);
					menu.CMenuSaveLoad.singleton().setupClearMark();
					ds.CHeap.setID_app(54);
					menu.CMenuSaveLoad.singleton().initialize();
					wmenu.CWMenuManager.Instance().GetMenuButton().initialize();
				}
				ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: true, obj: true);
				MatrixSound.MtxSENDS_Load(98);
				GX_DispOn();
				GXS_DispOn();
				m_Phase = EndPhase.FadeStart;
			}

			protected override void doUninitialize()
			{
				menu.MenuManager.getSingleton().Set2d3dMode(2);
				menu.MenuManager.getSingleton().ReleaseMenuDataText();
				menu.MenuManager.getSingleton().release();
				menu.MenuManager.getSingleton().releaseWindowAll();
				menu.MenuManager.getSingleton().ReleaseXbnFile();
				menu.MenuManager.getSingleton().terminate();
				menu.MenuManager.getSingleton().DeleteNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
				menu.MenuManager.getSingleton().Set2d3dMode(3);
				menu.MenuManager.getSingleton().ResetWindowSystem();
				menu.CMenuSaveLoad.singleton().terminate();
				menu.CMenuSaveLoad.singleton().releaseClearMark();
				wmenu.CWMenuManager.Instance().GetMenuButton().terminate();
				wmenu.CWMenuManager.Instance().GetPcFace().pcfmCleanup();
				SuspendSaveDataGlobal.getSingleton().release();
				MatrixSound.MtxSENDS_Unload();
				MatrixSound.MtxBGMNDS_Unload();
				wld.CWorldOutSideData.getInstance().SoundData().setSoundFlag(2);
				m_Vram.cleanup();
				dgs.msg.CMessageSys.getInstance().terminate();
			}

			protected override void doSleep()
			{
			}

			protected override void doWakeUp()
			{
			}

			protected override void onDrawPart()
			{
				dgs.msg.CMessageSys.getInstance().draw();
				sys2d.DS2DManager.d2dGetInstance().d2dDrawScreen(depthtest: false);
			}

			protected override void onExecutePart()
			{
				OS_AssignBackButton(1);
				sys2d.DS2DManager.d2dGetInstance().d2dExecute();
				if (!menu.CMenuSaveLoad.singleton().isEndFlag())
				{
					menu.CMenuSaveLoad.singleton().execute();
				}
				if (!backupAccess_)
				{
					if (!dgs.CFade.Main().isCleared() || !dgs.CFade.Sub().isCleared())
					{
						return;
					}
					OS_Terminate();
				}
				if (menu.CMenuSaveLoad.singleton().isEndFlag())
				{
					if (m_Phase == EndPhase.FadeStart)
					{
						dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
						dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
						MatrixSound.MtxSoundBGM.getSingleton().stop(15, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
						m_Phase = EndPhase.NextPart;
					}
					else if (m_Phase == EndPhase.NextPart && dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded() && backupAccess_)
					{
						abort();
					}
				}
			}

			protected override void onDrawEffector()
			{
				sys2d.DS2DManager.d2dGetInstance().d2dDraw();
			}

			protected override void onUpdatePart()
			{
				sys2d.DS2DManager.d2dGetInstance().d2dUpdate();
			}

			public void setAfterPart(GAMEPART part)
			{
				partIDAfter_ = part;
			}

			public GAMEPART getAfterPart()
			{
				return partIDAfter_;
			}

			public static LoadPart getInstance()
			{
				return instance_;
			}
		}

		public class SuspendLoadPart : sys.FF3GamePart
		{
			public const int SL_INIT = 0;

			public const int SL_LOAD = 1;

			public const int SL_CARD_ERR = 2;

			public const int SL_TERM = 3;

			public static SuspendLoadPart instance_ = new SuspendLoadPart();

			private int partID_;

			private int localState_;

			~SuspendLoadPart()
			{
			}

			public static void registerPart()
			{
				sys.GGlobal.registerPart(GAMEPART.GAMEPART_SUSPEND_LOAD, instance_);
				instance_.setPartID(0);
			}

			protected override void doInitialize()
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
				GX_SetBankForTex(GXVRamTex.GX_VRAM_TEX_NONE);
				GX_SetBankForTexPltt(GXVRamTexPltt.GX_VRAM_TEXPLTT_NONE);
				GX_SetBankForBG(GXVRamBG.GX_VRAM_BG_128_A);
				GX_SetBankForBGExtPltt(GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_0123_E);
				GX_SetBankForOBJ(GXVRamOBJ.GX_VRAM_OBJ_NONE);
				GX_SetBankForOBJExtPltt(GXVRamOBJExtPltt.GX_VRAM_OBJEXTPLTT_NONE);
				GX_SetBankForSubBG(GXVRamSubBG.GX_VRAM_SUB_BG_128_C);
				GX_SetBankForSubBGExtPltt(GXVRamSubBGExtPltt.GX_VRAM_SUB_BGEXTPLTT_0123_H);
				GX_SetBankForSubOBJ(GXVRamSubOBJ.GX_VRAM_SUB_OBJ_NONE);
				GX_SetBankForSubOBJExtPltt(GXVRamSubOBJExtPltt.GX_VRAM_SUB_OBJEXTPLTT_NONE);
				ds.CVram.setMainPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: false, obj: false);
				ds.CVram.setSubPlaneVisiblity(bg0: false, bg1: true, bg2: false, bg3: true, obj: false);
				GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_GRAPHICS, GXBGMode.GX_BGMODE_0, 0);
				GXS_SetGraphicsMode(GXBGMode.GX_BGMODE_0);
				G2S_SetBG0Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0000, GXBGCharBase.GX_BG_CHARBASE_0x04000, 0);
				G2S_SetBG1Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0800, GXBGCharBase.GX_BG_CHARBASE_0x18000, 0);
				dgs.msg.CMessageSys.getInstance().initialize();
				dgs.msg.CMessageSys.getInstance().Sub().assignBG(1, 0, 0, 32, 24);
				changeCompanyDirectory();
				menu.MenuManager.getSingleton().CreateMenuDataText(0);
				changeGlobalDirectory();
				localState_ = 0;
			}

			protected override void doUninitialize()
			{
				menu.MenuManager.getSingleton().ReleaseMenuDataText();
				dgs.msg.CMessageSys.getInstance().terminate();
				wld.CWorldOutSideData.getInstance().SoundData().setSoundFlag(2);
			}

			protected override void onExecutePart()
			{
				if (localState_ == 0)
				{
					if (!checkCard())
					{
						cardAcceccFailedSetting();
						localState_ = 2;
					}
					else
					{
						localState_ = 1;
					}
				}
				else if (1 == localState_)
				{
					SuspendSaveDataGlobal.getSingleton().reflect();
					SuspendSaveDataGlobal.getSingleton().release();
					if (!checkCard())
					{
						cardAcceccFailedSetting();
						localState_ = 2;
					}
					else
					{
						sys.GGlobal.setNextPart(GAMEPART.GAMEPART_WORLD);
						abort();
					}
				}
				else if (localState_ == 2)
				{
					dgs.CFade.Main().fadeIn(15);
					dgs.CFade.Sub().fadeIn(15);
					localState_ = 3;
				}
				else if (localState_ == 3 && dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
				{
					OS_Terminate();
				}
			}

			protected override void onDrawPart()
			{
				dgs.msg.CMessageSys.getInstance().draw();
			}

			public void setPartID(int i)
			{
				partID_ = i;
			}
		}

		public static sys2d.Bg _backBg = new sys2d.Bg();

		public static sys2d.Bg _backBg2;

		internal static bool checkCard()
		{
			bool result = true;
			byte[] array = new byte[4];
			card.Manager.GetInstance().LoadData(array, (uint)array.Length, 0u);
			if (card.Manager.GetInstance().GetResult() != card.RESULT.RESULT_SUCCESS)
			{
				result = false;
			}
			return result;
		}

		internal static void backupAccessFailedSetting()
		{
			menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID("suspend_message_2");
			if (nodeByID != null && nodeByID.behavior() != null)
			{
				menu.MBText mBText = (menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier());
				if (mBText != null && mBText.getMessage() != null)
				{
					mBText.getMessage().setStyle(1024u);
					mBText.getMessage().setVSpace(4);
				}
			}
			ds.CVram.setMainPlaneVisiblity(bg0: false, bg1: false, bg2: false, bg3: false, obj: false);
			ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: false, obj: false);
		}
	}
}
