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
	public static partial class btl
	{
		public class BattlePart : sys.FF3GamePart
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

			public static BattlePart instance_ = new BattlePart();

			private int partID_;

			private BattleSystem battleSystem_;

			private EndPhase phase_;

			private uint _pLinkTexel;

			private uint _pLinkPalette;

			private dgs.MSDINFO _MsdAddr;

			private Array _MsfAddr;

			private Array _MsfAddr_12;

			public static void registerPart()
			{
				sys.GGlobal.registerPart(GAMEPART.GAMEPART_BATTLE, instance_);
				instance_.setPartID(0);
			}

			public BattlePart()
			{
				partID_ = 0;
				battleSystem_ = null;
			}

			~BattlePart()
			{
			}

			public void setPartID(int i)
			{
				partID_ = i;
			}

			protected override void doInitialize()
			{
				ovl.overlayRegister.ChangeOverlay(ovl.OVERLAYINDEX.PART_BATTLE);
				battleSystem_ = new BattleSystem();
				GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_SUB_MAIN);
				ds.CDevice.setup();
				ds.CDevice.setup_main();
				ds.CDevice.setup_sub();
				GX_SetBGCharOffset(0);
				GX_SetBGScrOffset(0);
				G2_BlendNone();
				ds.CVram.clear();
				GXVRamTex gXVRamTex = GXVRamTex.GX_VRAM_TEX_0123_ABCD;
				GXVRamTexPltt gXVRamTexPltt = GXVRamTexPltt.GX_VRAM_TEXPLTT_01_FG;
				GX_SetBankForTex(gXVRamTex);
				GX_SetBankForTexPltt(gXVRamTexPltt);
				ds.CVram.getInstance().setBankForTex(gXVRamTex);
				ds.CVram.getInstance().setBankForPltt(gXVRamTexPltt);
				GX_SetBankForBG(GXVRamBG.GX_VRAM_BG_64_E);
				GX_SetBankForBGExtPltt(GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_NONE);
				GX_SetBankForOBJ(GXVRamOBJ.GX_VRAM_OBJ_NONE);
				GX_SetBankForOBJExtPltt(GXVRamOBJExtPltt.GX_VRAM_OBJEXTPLTT_NONE);
				GX_SetBankForSubOBJ(GXVRamSubOBJ.GX_VRAM_SUB_OBJ_NONE);
				GX_SetBankForSubOBJExtPltt(GXVRamSubOBJExtPltt.GX_VRAM_SUB_OBJEXTPLTT_NONE);
				ds.CVram.setMainPlaneVisiblity(bg0: true, bg1: false, bg2: true, bg3: false, obj: true);
				ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: true, bg2: true, bg3: true, obj: true);
				ds.CVram.setMainBGPriority(3, 2, 1, 0);
				ds.CVram.setSubBGPriority(0, 1, 2, 3);
				ds.CVram.getInstance().setupTexVramMng(393216u, 32768u, 64u, 0);
				ds.CVram.getInstance().setupPlttVramMng(32768u, 64u, 0);
				GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_GRAPHICS, GXBGMode.GX_BGMODE_0, 0);
				GXS_SetGraphicsMode(GXBGMode.GX_BGMODE_0);
				BattleEffect.instance().setup(battleDisplay.scene2nd());
				_MsdAddr = null;
				_MsfAddr = null;
				_MsfAddr_12 = null;
				changeCompanyDirectory();
				Array array = null;
				string filename = "eureka_battle.msd";
				uint size = ds.g_File.getSize(filename);
				if (size != 0)
				{
					array = ds.CHeap.alloc_app(size);
					if (array != null)
					{
						ds.g_File.load(array, filename);
					}
					_MsdAddr = (dgs.MSDINFO)array;
				}
				changeGlobalDirectory();
				dgs.msg.CMessageSys.getInstance().Main().initialize();
				G2_SetBG2ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x1000, GXBGCharBase.GX_BG_CHARBASE_0x08000);
				dgs.msg.CMessageSys.getInstance().Main().assignBG(2, 0, 0, 32, 24);
				dgs.msg.CMessageSys.getInstance().Main().setUpMSD(_MsdAddr, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON);
				BattleToOutside.getInstance().initialize();
				battleDisplay.initialize();
				sys2d.DS2DManager.d2dGetInstance().d2dInitialize();
				menu.BasicWindow.bwInitializeSystem(1u);
				menu.CommandWindow.cwInitializeSystem();
				u2d.PopUp.puInitializeSystem();
				Battle2DManager.instance().setup();
				getBattleSystem().initialize();
				menu.MenuManager.getSingleton().Set2d3dMode(2);
				menu.MenuManager.getSingleton().initialize();
				menu.MenuManager.getSingleton().Set2d3dMode(3);
				menu.MenuManager.getSingleton().initialize();
				changeGlobalDirectory();
				menu.MenuManager.getSingleton().LoadXbnFile("BattleDefine.xbn");
				changeCompanyDirectory();
				menu.MenuManager.getSingleton().CreateItemDataText();
				menu.MenuManager.getSingleton().CreateMenuDataText(1);
				changeGlobalDirectory();
				menu.MenuManager.getSingleton().SetUsingMenuType(0);
				phase_ = EndPhase.FadeStart;
				TexDivideLoader.getSingleton().tdlForceLoad();
				dgs.CCurtain.initialize();
				dgs.CCurtain.Bottom().setEnable(enable: true);
				dgs.CCurtain.Bottom().setColor(0, GX_RGB(0, 0, 0));
				dgs.CCurtain.Bottom().setAlpha(0, 31);
				dgs.CFade.Main().fadeIn(10);
				dgs.CFade.Sub().fadeIn(10);
			}

			protected override void doUninitialize()
			{
				dgs.msg.CMessageSys.getInstance().terminate();
				if (_MsdAddr != null)
				{
					ds.CHeap.free_app(_MsdAddr);
					_MsdAddr = null;
				}
				BattleEffect.instance().cleanup();
				getBattleSystem().terminate();
				Battle2DManager.instance().cleanup();
				menu.MenuManager.getSingleton().SetUsingMenuType(1);
				menu.MenuManager.getSingleton().ReleaseItemDataText();
				menu.MenuManager.getSingleton().ReleaseMenuDataText();
				menu.MenuManager.getSingleton().release();
				menu.MenuManager.getSingleton().releaseAll();
				menu.MenuManager.getSingleton().ReleaseXbnFile();
				menu.MenuManager.getSingleton().Set2d3dMode(2);
				menu.MenuManager.getSingleton().terminate();
				menu.MenuManager.getSingleton().Set2d3dMode(3);
				menu.MenuManager.getSingleton().terminate();
				menu.BasicWindow.bwReleaseSystem();
				menu.CommandWindow.cwReleaseSystem();
				u2d.PopUp.puReleaseSystem();
				battleDisplay.terminate();
				dgs.CCurtain.Bottom().terminate();
				ds.CVram.getInstance().releaseTexVramMng();
				ds.CVram.getInstance().releasePlttVramMng();
				battleSystem_.destruct();
				battleSystem_ = null;
			}

			protected override void doSleep()
			{
			}

			protected override void doWakeUp()
			{
			}

			protected override void onPreExecutePart()
			{
				getBattleSystem().preExecute();
			}

			protected override void onDrawPart()
			{
				int num = OS_GetTick();
				dgs.CCurtain.Bottom().draw();
				battleDisplay.draw1st();
				num = OS_GetTick() - num;
			}

			protected override void onExecutePart()
			{
				int num = OS_GetTick();
				dgs.CCurtain.execute();
				BattlePerformer.getInstance().progress();
				getBattleSystem().execute();
				num = OS_GetTick() - num;
				if (getBattleSystem().isEnd())
				{
					if (phase_ == EndPhase.FadeStart)
					{
						dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
						dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
						phase_ = EndPhase.NextPart;
					}
					else if (phase_ == EndPhase.NextPart && dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
					{
						if (BattleToOutside.getInstance().battleResult() == BATTLE_RESULT.LOSE)
						{
							if (sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_DEBUG_MENU)
							{
								sys.GGlobal.setNextPart(sys.GGlobal.getPreviousPart());
							}
							else if (OutsideToBattle.getInstance().restart())
							{
								sys.GGlobal.setNextPart(sys.GGlobal.getPreviousPart());
							}
							else
							{
								MatrixSound.MtxBGMNDS_Unload();
								NNS_SndPlayerStopSeqByPlayerNo(0, 0);
								sys.GGlobal.setNextPart(GAMEPART.GAMEPART_TITLE);
								SuspendSaveDataGlobal.getSingleton().setup();
								SuspendSaveDataGlobal.getSingleton().invalidate();
								SuspendSaveDataGlobal.getSingleton().release();
							}
						}
						else
						{
							sys.GGlobal.setNextPart(sys.GGlobal.getPreviousPart());
						}
						abort();
					}
				}
				num = OS_GetTick();
				NNS_G3dGlbFlushP();
				NNS_G3dGeFlushBuffer();
				Battle2DManager.instance().execute();
				BattleEffect.instance().execute();
				sys2d.DS2DManager.d2dGetInstance().d2dExecute();
				num = OS_GetTick() - num;
			}

			protected override void onDrawEffector()
			{
				sys2d.DS2DManager.d2dGetInstance().d2dDraw();
				dgs.msg.CMessageSys.getInstance().draw();
				sys2d.DS2DManager.d2dGetInstance().d2dDrawScreen(depthtest: false);
			}

			protected override void onUpdatePart()
			{
				battleDisplay.update();
				BattleEffect.instance().update();
				sys2d.DS2DManager.d2dGetInstance().d2dUpdate();
			}

			protected override void onDraw2ndPart()
			{
				battleDisplay.draw2nd();
				BattleEffect.instance().draw();
				getBattleSystem().playerWindow().execute();
				battleDisplay.execute();
			}

			private BattleSystem getBattleSystem()
			{
				return battleSystem_;
			}
		}
	}
}
