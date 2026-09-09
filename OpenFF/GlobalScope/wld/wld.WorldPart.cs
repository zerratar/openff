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
		public class WorldPart : sys.FF3GamePart
		{
			public static WorldPart instance_ = new WorldPart();

			private int partID_;

			private ds.sys3d.Scene m_Scene = new ds.sys3d.Scene();

			private ds.sys3d.Scene m_Scene2nd = new ds.sys3d.Scene();

			private CWorldSystem m_WorldSystem;

			private int EndSuspend_;

			public static void registerPart()
			{
				sys.GGlobal.registerPart(GAMEPART.GAMEPART_WORLD, instance_);
				instance_.setPartID(0);
			}

			public WorldPart()
			{
				partID_ = 0;
				m_WorldSystem = null;
			}

			~WorldPart()
			{
			}

			public void setPartID(int i)
			{
				partID_ = i;
			}

			protected override void doInitialize()
			{
				int num = 0;
				if (ovl.overlayRegister.GetOverlayIndex() != ovl.OVERLAYINDEX.PART_WORLD)
				{
					CBaseSystem.resetOverlay();
				}
				num = OS_GetTick();
				ovl.overlayRegister.ChangeOverlay(ovl.OVERLAYINDEX.PART_WORLD);
				OS_GetTick();
				m_WorldSystem = new CWorldSystem();
				m_WorldSystem.registerWorldMode();
				if (m_WorldSystem.IsMenu())
				{
					return;
				}
				evt.CEventRestriction.getSingleton().clear();
				ushort num2 = 0;
				ds.CHeap.setID_app(0);
				ds.CDevice.setup();
				GX_SetBGCharOffset(0);
				GX_SetBGScrOffset(0);
				G2_BlendNone();
				GX_DispOn();
				GXS_DispOn();
				G2_SetBG0Offset(0, 0);
				G2_SetBG1Offset(0, 0);
				G2_SetBG2Offset(0, 0);
				G2_SetBG3Offset(0, 0);
				G2S_SetBG0Offset(0, 0);
				G2S_SetBG1Offset(0, 0);
				G2S_SetBG2Offset(0, 0);
				G2S_SetBG3Offset(0, 0);
				if (sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_DEBUG_MENU || sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_LOAD || sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_TITLE || sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_SUSPEND_LOAD)
				{
					if (sceneMng.getStage()[0] == 'f')
					{
						m_WorldSystem.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD);
					}
					else
					{
						m_WorldSystem.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN);
					}
				}
				else if (m_WorldSystem.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_ERR)
				{
					if (sceneMng.getStage()[0] == 'f')
					{
						m_WorldSystem.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD);
					}
					else
					{
						m_WorldSystem.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN);
					}
				}
				num2 = 70;
				ds.CHeap.setID_app(num2);
				if (m_WorldSystem.CrtVram() != null)
				{
					m_WorldSystem.CrtVram().setup();
				}
				num2 = 71;
				ds.CHeap.setID_app(num2);
				m_Scene.initialize();
				m_Scene2nd.initialize();
				m_Scene.setCamera(m_WorldSystem.WorldCamera());
				m_Scene2nd.setCamera(m_WorldSystem.WorldCamera());
				num2 = 72;
				ds.CHeap.setID_app(num2);
				eff.CEffectMng.instance().initialize(m_Scene);
				num2 = 73;
				ds.CHeap.setID_app(num2);
				num = OS_GetTick();
				dgs.msg.CMessageSys.getInstance().initialize();
				OS_GetTick();
				num2 = 74;
				ds.CHeap.setID_app(num2);
				num = OS_GetTick();
				sys2d.DS2DManager.d2dGetInstance().d2dInitialize();
				OS_GetTick();
				num2 = 75;
				ds.CHeap.setID_app(num2);
				num = OS_GetTick();
				changeGlobalDirectory();
				characterMng.initialize(m_Scene, m_Scene2nd);
				OS_GetTick();
				num2 = 76;
				ds.CHeap.setID_app(num2);
				num = OS_GetTick();
				stageMng.initialize(m_Scene);
				OS_GetTick();
				num2 = 77;
				ds.CHeap.setID_app(num2);
				if (sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_DEBUG_MENU)
				{
					ds.CDevice.singleton().setFPS(ds.CDevice.enFPS.enFPS_30);
				}
				else
				{
					ds.CDevice.singleton().setFPS(ds.CDevice.enFPS.enFPS_30);
				}
				if (ds.CDevice.singleton().getFPS() == ds.CDevice.enFPS.enFPS_30)
				{
					characterMng.setFrameRate(4096);
				}
				else
				{
					characterMng.setFrameRate(2048);
				}
				num2 = 78;
				ds.CHeap.setID_app(num2);
				num = OS_GetTick();
				dgs.CCurtain.initialize();
				OS_GetTick();
				num2 = 79;
				ds.CHeap.setID_app(num2);
				num = OS_GetTick();
				AreaChange.getInstance().initialize();
				OS_GetTick();
				num2 = 80;
				ds.CHeap.setID_app(num2);
				num = OS_GetTick();
				Encount.getInstance().initialize();
				OS_GetTick();
				EndSuspend_ = 0;
				num2 = 81;
				ds.CHeap.setID_app(num2);
				num = OS_GetTick();
				menu.MenuManager.getSingleton().SetWindowSystem();
				OS_GetTick();
				num2 = 82;
				ds.CHeap.setID_app(num2);
				num = OS_GetTick();
				menu.MenuManager.getSingleton().Set2d3dMode(2);
				menu.MenuManager.getSingleton().initialize();
				wmenu.CWMenuManager.Instance().clear();
				OS_GetTick();
				num2 = 83;
				ds.CHeap.setID_app(num2);
				num = OS_GetTick();
				menu.MenuManager.getSingleton().Set2d3dMode(3);
				menu.MenuManager.getSingleton().initialize();
				OS_GetTick();
				num2 = 84;
				ds.CHeap.setID_app(num2);
				changeCompanyDirectory();
				num = OS_GetTick();
				menu.MenuManager.getSingleton().LoadXbnFile("WorldDefine.xbn");
				OS_GetTick();
				changeGlobalDirectory();
				num2 = 85;
				ds.CHeap.setID_app(num2);
				if (sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_DEBUG_MENU || (m_WorldSystem.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD && m_WorldSystem.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN))
				{
					CWorldOutSideData.getInstance().MapData().getHoldDoorData()
						.initialize();
				}
				num2 = 86;
				ds.CHeap.setID_app(num2);
				num = OS_GetTick();
				m_WorldSystem.World2DMng().createButton();
				m_WorldSystem.World2DMng().refWorldMap().releaseMarkerFlagByStageName(sceneMng.getStage());
				m_WorldSystem.World2DMng().refWorldMap().resetMapMarker();
				m_WorldSystem.World2DMng().MessageWindow().setup();
				OS_GetTick();
				num2 = 87;
				ds.CHeap.setID_app(num2);
				num = OS_GetTick();
				m_WorldSystem.initialize();
				OS_GetTick();
				if (sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_DEBUG_MENU || GAMEPART.GAMEPART_TITLE == sys.GGlobal.getPreviousPart())
				{
					m_WorldSystem.setAreaChangeShutterFlag(flag: true);
				}
				m_WorldSystem.appendPWS(MapMarkerUpdater.getSingleton());
				dv.CDeviceManager.getInstance().Pad().setActivity(b: true);
				num2 = 88;
				ds.CHeap.setID_app(num2);
				chr.CCharacterEureka.m_CharaFps = (uint)ds.CDevice.singleton().getFPS();
				cmr.CWorldCamera.m_CameraFps = (uint)ds.CDevice.singleton().getFPS();
			}

			protected override void doUninitialize()
			{
				menu.MenuManager.getSingleton().Set2d3dMode(2);
				menu.MenuManager.getSingleton().ReleaseItemDataText();
				menu.MenuManager.getSingleton().ReleaseMenuDataText();
				wmenu.CWMenuManager.Instance().SetKind(wmenu.CWMenuManager.Instance().GetKind());
				if (!wmenu.CWMenuManager.Instance().pCurrent().isFinalize())
				{
					wmenu.CWMenuManager.Instance().pCurrent().terminate();
				}
				wmenu.CWMenuManager.Instance().terminate();
				menu.MenuManager.getSingleton().release();
				menu.MenuManager.getSingleton().releaseWindowAll();
				menu.MenuManager.getSingleton().ReleaseXbnFile();
				menu.MenuManager.getSingleton().terminate();
				menu.MenuManager.getSingleton().Set2d3dMode(3);
				menu.MenuManager.getSingleton().ResetWindowSystem();
				menu.MenuManager.getSingleton().release();
				menu.MenuManager.getSingleton().releaseWindowAll();
				menu.MenuManager.getSingleton().releaseAll();
				menu.MenuManager.getSingleton().ReleaseXbnFile();
				menu.MenuManager.getSingleton().terminate();
				m_WorldSystem.removePWS(MapMarkerUpdater.getSingleton());
				m_WorldSystem.World2DMng().MenuStartButton().cleanup();
				m_WorldSystem.World2DMng().CameraButton().cleanup();
				m_WorldSystem.World2DMng().TalkButton().cleanup();
				m_WorldSystem.World2DMng().refWorldMap().finalizeWMap();
				m_WorldSystem.terminate();
				if (false || 1 == EndSuspend_)
				{
					MatrixSound.MtxSENDS_Unload();
					MatrixSound.MtxSoundBGM.getSingleton().stop(0, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
					MatrixSound.MtxBGMNDS_Unload();
					MatrixSound.MtxBGMNDS_Unload();
					CWorldOutSideData.getInstance().initialize2();
					evt.CEventManager.getInstance().initialize();
				}
				if (sys.GGlobal.getNextPart() == GAMEPART.GAMEPART_BATTLE || sys.GGlobal.getNextPart() == GAMEPART.GAMEPART_MOG_NET)
				{
					if (m_WorldSystem.CrtVram() != null)
					{
						m_WorldSystem.CrtVram().cleanup();
					}
				}
				else if (m_WorldSystem.PreVram() != null)
				{
					m_WorldSystem.PreVram().cleanup();
				}
				characterMng.terminate();
				stageMng.terminate();
				dgs.msg.CMessageSys.getInstance().terminate();
				eff.CEffectMng.instance().cleanup();
				snd.CSoundMng.instance().cleanup();
				AreaChange.getInstance().terminate();
				Encount.getInstance().terminate();
				m_WorldSystem.destruct();
			}

			protected override void doSleep()
			{
			}

			protected override void doWakeUp()
			{
			}

			protected override void onDrawPart()
			{
				dgs.CCurtain.Bottom().draw();
				if (GX_GetPrioriry3D() != 0)
				{
					sys2d.DS2DManager.d2dGetInstance().d2dDraw();
					dgs.msg.CMessageSys.getInstance().draw();
					sys2d.DS2DManager.d2dGetInstance().d2dDrawScreen(depthtest: false);
				}
				eff.CEffectMng.instance().draw();
				m_Scene.draw(bVBlank: true);
				// PORT: the mod's own models (glTF), with the scene's camera, before the curtains and the 2D.
				// Not on a catch-up frame (render() draws only the last of the frames it runs), as the game's models.
				if (skipFrame == 0)
				{
					OpenFF.Client.ModMeshes.DrawWorld();
				}
				dgs.CCurtain.Middle().draw();
				getWorldSystem().ScrFlash().draw();
				if (GX_GetPrioriry3D() == 0)
				{
					sys2d.DS2DManager.d2dGetInstance().d2dDraw();
					dgs.msg.CMessageSys.getInstance().draw();
					bool depthtest = false;
					if (strcmp(sceneMng.getStage(), "t03_12") == 0)
					{
						depthtest = true;
					}
					sys2d.DS2DManager.d2dGetInstance().d2dDrawScreen(depthtest);
				}
				dgs.CCurtain.Top().draw();
				AreaChange.getInstance().draw();
			}

			protected override void onDraw2ndPart()
			{
				m_Scene2nd.draw(bVBlank: false);
			}

			protected override void onExecutePart()
			{
				characterMng.execute();
				stageMng.execute();
				AreaChange.getInstance().execute();
				if (!characterMng.isLoadingVramAsync())
				{
					m_WorldSystem.execute();
					m_WorldSystem.update();
				}
				sys2d.DS2DManager.d2dGetInstance().d2dExecute();
				eff.CEffectMng.instance().execute();
				if (m_WorldSystem.isEnd())
				{
					abort();
				}
			}

			protected override void onDrawEffector()
			{
			}

			protected override void onUpdatePart()
			{
				sys2d.DS2DManager.d2dGetInstance().d2dUpdate();
				eff.CEffectMng.instance().update();
			}

			public static WorldPart getInstance()
			{
				return instance_;
			}

			public CWorldSystem getWorldSystem()
			{
				return m_WorldSystem;
			}

			public void setSuspendEnd()
			{
				EndSuspend_ = 1;
			}

			public ds.sys3d.Scene getScene()
			{
				return m_Scene;
			}
		}
	}
}
