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
	public static class logo
	{
		public class CampanyLogoPart : sys.FF3GamePart
		{
			public enum State
			{
				APPEAR,
				WAIT_APPEAR,
				DISAPPEAR,
				WAIT_DISAPPEAR,
				MAX_STATE
			}

			public enum Fade
			{
				START_IN,
				FADE_IN,
				LOGO_DRAW,
				END_OUT
			}

			public enum CARD_INIT_STATE
			{
				CARD_INIT_CHECK,
				CARD_INIT_WRITE,
				CARD_INIT_WRITE_END,
				CARD_INIT_WRITE_ERR,
				CARD_INIT_END
			}

			public const Fade START_IN = Fade.START_IN;

			public const Fade FADE_IN = Fade.FADE_IN;

			public const Fade LOGO_DRAW = Fade.LOGO_DRAW;

			public const Fade END_OUT = Fade.END_OUT;

			public const CARD_INIT_STATE CARD_INIT_CHECK = CARD_INIT_STATE.CARD_INIT_CHECK;

			public const CARD_INIT_STATE CARD_INIT_WRITE = CARD_INIT_STATE.CARD_INIT_WRITE;

			public const CARD_INIT_STATE CARD_INIT_WRITE_END = CARD_INIT_STATE.CARD_INIT_WRITE_END;

			public const CARD_INIT_STATE CARD_INIT_WRITE_ERR = CARD_INIT_STATE.CARD_INIT_WRITE_ERR;

			public const CARD_INIT_STATE CARD_INIT_END = CARD_INIT_STATE.CARD_INIT_END;

			public const int STATE_START = 0;

			public const int STATE_LOGO1_IN = 1;

			public const int STATE_LOGO1_OUT = 2;

			public const int STATE_LOGO2_IN = 3;

			public const int STATE_LOGO2_OUT = 4;

			public const int CAMPANYLOGO_PHASE_CHECKCARD = 0;

			public const int CAMPANYLOGO_PHASE_CARDERROR = 1;

			public const int CAMPANYLOGO_PHASE_CARDINIT = 2;

			public const int CAMPANYLOGO_PHASE_LOGOPARADE = 3;

			public static CampanyLogoPart instance_ = new CampanyLogoPart();

			private static string[] sub_logo_file_name = new string[5] { "se_logo", "mt_logo", null, null, null };

			protected dgs.MSDINFO m_pMessageData;

			protected dgs.DGSMessageManager m_MessageManager = new dgs.DGSMessageManager();

			private int partID_;

			private int m_Phase;

			private static int m_AppearFrame = 30;

			private static int m_WaitAppearFrame = 90;

			private static int m_DisAppearFrame = 30;

			private static int m_WaitDisAppearFrame = 30;

			private static int m_MaxLogoCnt = 2;

			private bool m_IsEnd;

			private Fade m_Fade;

			private int m_WorkFrame;

			private int m_BGNumber;

			private sys2d.Bg m_LogoBG = new sys2d.Bg();

			private sys2d.Bg m_LogoBGSub = new sys2d.Bg();

			private int m_localState;

			private Array m_pData;

			private bool m_Flag;

			public static void registerPart()
			{
				sys.GGlobal.registerPart(GAMEPART.GAMEPART_CAMPANY_LOGO, instance_);
				instance_.setPartID(0);
			}

			public CampanyLogoPart()
			{
				partID_ = 0;
				m_pMessageData = null;
			}

			~CampanyLogoPart()
			{
			}

			public void setPartID(int i)
			{
				partID_ = i;
			}

			public void setVramBank()
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
			}

			protected override void doInitialize()
			{
				GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_MAIN_SUB);
				ds.CDevice.setup();
				GX_DispOn();
				GXS_DispOn();
				GX_SetMasterBrightness(-16);
				GXS_SetMasterBrightness(-16);
				setVramBank();
				ds.CVram.setMainPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: false, obj: false);
				ds.CVram.setSubPlaneVisiblity(bg0: false, bg1: false, bg2: true, bg3: true, obj: false);
				ds.CVram.setMainBGPriority(3, 2, 1, 0);
				ds.CVram.setSubBGPriority(0, 1, 2, 3);
				GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_GRAPHICS, GXBGMode.GX_BGMODE_0, 0);
				GXS_SetGraphicsMode(GXBGMode.GX_BGMODE_0);
				G2_SetBG0Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0000, GXBGCharBase.GX_BG_CHARBASE_0x04000, 0);
				G2_SetBG1Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0800, GXBGCharBase.GX_BG_CHARBASE_0x18000, 0);
				G2_SetBG2ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x1000, GXBGCharBase.GX_BG_CHARBASE_0x1c000);
				G2_SetBG3ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x1800, GXBGCharBase.GX_BG_CHARBASE_0x20000);
				G2S_SetBG0Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0000, GXBGCharBase.GX_BG_CHARBASE_0x04000, 0);
				G2S_SetBG1Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0800, GXBGCharBase.GX_BG_CHARBASE_0x18000, 0);
				G2S_SetBG2ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x1000, GXBGCharBase.GX_BG_CHARBASE_0x1c000);
				G2S_SetBG3ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x1000, GXBGCharBase.GX_BG_CHARBASE_0x1c000);
				m_BGNumber = 0;
				dgs.CFade.Main().fadeOut(0, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
				dgs.CFade.Sub().fadeOut(0, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
				m_IsEnd = false;
				m_WorkFrame = 0;
				m_Fade = Fade.START_IN;
				m_Phase = 0;
				m_localState = 0;
				messageInitialize();
			}

			protected override void doUninitialize()
			{
				m_LogoBG.bgRelease();
				m_LogoBGSub.bgRelease();
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
				messageUninitialize();
			}

			protected override void doSleep()
			{
			}

			protected override void doWakeUp()
			{
			}

			protected override void onDrawPart()
			{
				m_MessageManager.dgsMMDraw();
				sys2d.DS2DManager.d2dGetInstance().d2dDrawScreen(depthtest: false);
			}

			protected override void onExecutePart()
			{
				switch (m_Phase)
				{
				case 0:
					onExecuteCheckCard();
					break;
				case 1:
					onExecuteCardError();
					break;
				case 2:
					onExecuteCardInit();
					break;
				case 3:
					onExecuteLogoParade();
					break;
				}
			}

			public void onExecuteCheckCard()
			{
				byte[] array = new byte[4];
				card.Manager.GetInstance().LoadData(array, (uint)array.Length, 0u);
				if (card.Manager.GetInstance().GetResult() != card.RESULT.RESULT_SUCCESS)
				{
					setupCardError(50061);
					m_Phase = 1;
				}
				else
				{
					m_Phase = 2;
					m_localState = 0;
				}
			}

			public void onExecuteCardError()
			{
				if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
				{
					OS_Terminate();
				}
			}

			public void onExecuteCardInit()
			{
				switch (m_localState)
				{
				case 0:
				{
					CARDBackupType type = CARDBackupType.CARD_BACKUP_TYPE_EEPROM_512KBITS;
					uint romByteSize = card.Manager.GetInstance().GetRomByteSize(type);
					m_pData = ds.CHeap.alloc_app(romByteSize);
					if (m_pData != null)
					{
						memset(m_pData, 0, (int)romByteSize);
						sbyte[] array = new sbyte[32];
						uint num = card.Manager.GetInstance().GetRomByteSize(card.Manager.GetInstance().GetBackupType()) - 32;
						card.Manager.GetInstance().LoadData(array, 32u, num);
						if (card.Manager.GetInstance().GetResult() != card.RESULT.RESULT_SUCCESS)
						{
							setupCardError(50061);
							m_Phase = 1;
						}
						else if (memcmp(array, card.ONCE_INITIALIZE_CODE, 32) != 0)
						{
							sbyte[] array2 = static_cast<sbyte[]>(m_pData);
							memcpy(array2, (int)num, card.ONCE_INITIALIZE_CODE, 32);
							card.Manager.GetInstance().StartSaveAddress(array2, romByteSize, 0u);
							setupCardError(50062);
							m_localState = 1;
							m_WorkFrame = 30;
							m_Flag = true;
						}
						else
						{
							m_localState = 4;
						}
					}
					break;
				}
				case 1:
					if (!dgs.CFade.Main().isCleared() || !dgs.CFade.Sub().isCleared())
					{
						break;
					}
					if (card.Manager.GetInstance().Execute())
					{
						dgs.CFade.Main().fadeOut(30, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
						dgs.CFade.Sub().fadeOut(30, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
						if (card.Manager.GetInstance().GetResult() == card.RESULT.RESULT_SUCCESS)
						{
							sbyte[] array3 = static_cast<sbyte[]>(m_pData);
							memset(array3, 0, 72);
							card.Manager.GetInstance().WriteData(array3, 72u, 0u);
							card.Manager.GetInstance().SetDataNum(3, 1);
							m_localState = 2;
						}
						else
						{
							m_localState = 3;
						}
					}
					if (m_WorkFrame-- < 0)
					{
						if (m_Flag)
						{
							ds.CVram.setSubPlaneVisiblity(bg0: false, bg1: false, bg2: false, bg3: false, obj: false);
						}
						else
						{
							ds.CVram.setSubPlaneVisiblity(bg0: false, bg1: true, bg2: false, bg3: false, obj: false);
						}
						m_Flag = !m_Flag;
						m_WorkFrame = 30;
					}
					break;
				case 2:
					if (dgs.CFade.Sub().isFaded() && dgs.CFade.Main().isFaded())
					{
						m_localState = 4;
						releaseCardError();
					}
					break;
				case 3:
					if (dgs.CFade.Sub().isFaded() && dgs.CFade.Main().isFaded())
					{
						if (m_pData != null)
						{
							ds.CHeap.free_app(m_pData);
						}
						dgs.CFade.Main().fadeIn(30);
						dgs.CFade.Sub().fadeIn(30);
						releaseCardError();
						setupCardError(50065);
						m_Phase = 1;
					}
					break;
				case 4:
					if (m_pData != null)
					{
						ds.CHeap.free_app(m_pData);
					}
					changeGlobalDirectory();
					m_LogoBGSub.bgLoad2(sub_logo_file_name[0]);
					m_LogoBGSub.bgSetUp(NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB0);
					m_LogoBGSub.bgRelease();
					m_Phase = 3;
					m_localState = 0;
					break;
				}
			}

			public void onExecuteLogoParade()
			{
				switch (m_localState)
				{
				case 0:
					if (dgs.CFade.Sub().isFaded() && dgs.CFade.Main().isFaded())
					{
						dgs.CFade.Main().fadeIn(30);
						dgs.CFade.Sub().fadeIn(30);
						m_WorkFrame = m_WaitAppearFrame;
						m_localState = 1;
					}
					break;
				case 1:
					if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared() && 0 > --m_WorkFrame)
					{
						dgs.CFade.Main().fadeOut(30, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
						dgs.CFade.Sub().fadeOut(30, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
						m_localState = 2;
					}
					break;
				case 2:
					if (dgs.CFade.Sub().isFaded())
					{
						changeGlobalDirectory();
						m_LogoBGSub.bgLoad2(sub_logo_file_name[1]);
						m_LogoBGSub.bgSetUp(NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB0);
						m_LogoBGSub.bgRelease();
						dgs.CFade.Main().fadeIn(30);
						dgs.CFade.Sub().fadeIn(30);
						m_localState = 3;
						m_WorkFrame = m_WaitAppearFrame;
					}
					break;
				case 3:
					if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared() && 0 > --m_WorkFrame)
					{
						dgs.CFade.Main().fadeOut(30, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
						dgs.CFade.Sub().fadeOut(30, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
						m_localState = 4;
					}
					break;
				case 4:
					if (dgs.CFade.Sub().isFaded() && dgs.CFade.Main().isFaded())
					{
						movie.MoviePart.getInstance().setAfterPart(GAMEPART.GAMEPART_TITLE);
						sys.GGlobal.setNextPart(GAMEPART.GAMEPART_MOVIE);
						abort();
					}
					break;
				}
			}

			protected override void onDrawEffector()
			{
			}

			public void setupCardError(int MessageID)
			{
				dgs.CFade.Sub().fadeIn(15);
				dgs.CFade.Main().fadeIn(15);
				ds.CVram.setSubBGPriority(3, 2, 1, 0);
				ds.CVram.setSubPlaneVisiblity(bg0: false, bg1: true, bg2: false, bg3: false, obj: false);
				ds.CVram.setMainPlaneVisiblity(bg0: false, bg1: false, bg2: false, bg3: false, obj: false);
				dgs.DGSMessageManager messageManager = m_MessageManager;
				dgs.DGSMessage dGSMessage = messageManager.createMessage((uint)MessageID, dgs.INVALID_MSDHANDLE, 0);
				ds.Vector2<short> vector = new ds.Vector2<short>(0, 0);
				if (dGSMessage != null)
				{
					dGSMessage.getCompleteTextSize(vector);
					dGSMessage.setVisibility(b: false);
					dGSMessage.release();
				}
				m_MessageManager.writeCharacterString((short)(128 - vector.vx / 2), (short)(96 - vector.vy / 2), 0, 4, dgs.TXT_COLOR.TXT_COLOR_WHITE, 1152u, (uint)MessageID, shadow: false, 0);
			}

			public void releaseCardError()
			{
				m_MessageManager.dgsMMAreaErase(0, 0, 256, 192);
				ds.CVram.setMainPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: false, obj: false);
				ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: false, obj: false);
				ds.CVram.setMainBGPriority(0, 1, 2, 3);
				ds.CVram.setSubBGPriority(0, 1, 2, 3);
			}

			public void messageInitialize()
			{
				m_MessageManager.initialize();
				m_MessageManager.assignBG(1, dgs.TARGETLCD.TLCD_SUB, 0, 0, 32, 24);
				if (m_pMessageData != null)
				{
					return;
				}
				changeCompanyDirectory();
				uint size = ds.g_File.getSize("eureka_menu.msd");
				Array array;
				if (0 < size && (array = ds.CHeap.alloc_app(size)) != null)
				{
					ds.g_File.load(array, "eureka_menu.msd");
					m_pMessageData = (dgs.MSDINFO)array;
					if (m_pMessageData != null)
					{
						m_MessageManager.initMSD(m_pMessageData);
					}
					changeGlobalDirectory();
				}
			}

			public void messageUninitialize()
			{
				if (m_pMessageData != null)
				{
					m_MessageManager.removeMSD(m_pMessageData);
					ds.CHeap.free_app(m_pMessageData);
					m_pMessageData = null;
				}
			}
		}
	}
}
