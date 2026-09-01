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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class ds
	{
		public class CDevice
		{
			public enum enFPS
			{
				enFPS_30 = 30,
				enFPS_60 = 60
			}

			public enum enLIMIT_TICK
			{
				enLIMIT_TICK_30 = 16000,
				enLIMIT_TICK_60 = 8000
			}

			public const enFPS enFPS_30 = enFPS.enFPS_30;

			public const enFPS enFPS_60 = enFPS.enFPS_60;

			public const enLIMIT_TICK enLIMIT_TICK_30 = enLIMIT_TICK.enLIMIT_TICK_30;

			public const enLIMIT_TICK enLIMIT_TICK_60 = enLIMIT_TICK.enLIMIT_TICK_60;

			public static PMSleepCallbackInfo m_PreCallbackInfo;

			public static PMSleepCallbackInfo m_PostCallbackInfo;

			public static bool _bSleepEnable = false;

			public static bool _bSleepCheckEnable = true;

			public static bool _bLCDC_OffEnable = false;

			public static CDevice instance_ = new CDevice();

			private static int DEFAULT_DMA_NUMBER = 3;

			private ulong _unBlankTick;

			private enFPS _enFPS;

			private int _gxBufferMode;

			public CDevice()
			{
				_unBlankTick = 0uL;
				_enFPS = enFPS.enFPS_60;
				_gxBufferMode = 0;
			}

			public static CDevice singleton()
			{
				return instance_;
			}

			public void waitVBlank()
			{
				SVC_WaitVBlankIntr();
				_unBlankTick = (ulong)OS_GetTick();
			}

			public ulong getPreVBlankTick()
			{
				return _unBlankTick;
			}

			public void present()
			{
				G3_SwapBuffers(0, _gxBufferMode);
			}

			public void setFPS(enFPS fps)
			{
				_enFPS = fps;
			}

			public enFPS getFPS()
			{
				return _enFPS;
			}

			public static void VBlankIntr()
			{
				sys.CBlankTask.btVTask();
				OS_SetIrqCheckFlag(1u);
			}

			public static void HBlankIntr()
			{
				OS_SetIrqCheckFlag(2u);
			}

			public static void initialize()
			{
				OS_Init();
				FX_Init();
				GX_SetPower(0);
				GX_Init();
				OS_InitTick();
				OS_InitAlarm();
				RTC_Init();
				PXI_Init();
				TP_Init();
				GX_DispOff();
				GXS_DispOff();
				OS_SetIrqFunction(1u, VBlankIntr);
				OS_SetIrqFunction(2u, HBlankIntr);
				OS_EnableIrqMask(1u);
				OS_EnableIrqMask(2u);
				OS_EnableIrqMask(262144u);
				OS_EnableIrq();
				FS_Init(2);
				GX_VBlankIntr(1);
				GX_HBlankIntr(1);
				GX_SetBankForLCDC(0);
				GX_DisableBankForLCDC();
				SetUpSleepMode();
			}

			public static void setup()
			{
				NNS_G3dInit();
				G3X_InitMtxStack();
				GX_Power3D(1);
				G3_SwapBuffers(0, 0);
				G3X_SetShading(0);
				G3X_AntiAlias(1);
				G3X_AlphaTest(0, 0);
				G3X_AlphaBlend(1);
				G3_ViewPort(0, 0, 255, 191);
			}

			public static void setup_main()
			{
				GX_DisableBankForBG();
				GX_DisableBankForOBJ();
				GX_DisableBankForTex();
				GX_DisableBankForTexPltt();
				GX_DisableBankForSubBG();
				GX_DisableBankForSubOBJ();
				GX_SetBankForTex(GXVRamTex.GX_VRAM_TEX_0123_ABCD);
				GX_SetBankForTexPltt(GXVRamTexPltt.GX_VRAM_TEXPLTT_0123_E);
				GX_SetBankForBG(GXVRamBG.GX_VRAM_BG_16_G);
				GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_GRAPHICS, GXBGMode.GX_BGMODE_0, 0);
				GX_SetVisiblePlane(3);
				GX_SetBGCharOffset(0);
				GX_SetBGScrOffset(0);
				G2_SetBG1Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x3800, GXBGCharBase.GX_BG_CHARBASE_0x00000, 0);
				G2_SetBG0Priority(1);
				G2_SetBG1Priority(0);
				G2_BlendNone();
				GX_DispOn();
				G3X_SetClearColor(GX_RGB(0, 0, 0), 0, 32767, 0, 0);
			}

			public static void setup_sub()
			{
				GXS_DispOn();
			}

			public static void SetUpSleepMode()
			{
				PM_SetSleepCallbackInfo(m_PreCallbackInfo, PreCallback, null);
				PM_SetSleepCallbackInfo(m_PostCallbackInfo, PostCallback, null);
				PM_AppendPreSleepCallback(m_PreCallbackInfo);
				PM_AppendPostSleepCallback(m_PostCallbackInfo);
			}

			public void CleanUpSleepMode()
			{
				PM_DeletePreSleepCallback(m_PreCallbackInfo);
				PM_DeletePostSleepCallback(m_PostCallbackInfo);
			}

			public static void CheckSleepMode()
			{
				if (!isEnableCheckSleep())
				{
					return;
				}
				if (PAD_DetectFold() != 0)
				{
					LCDC_OFF(flag: true);
					if (!card.Manager.GetInstance().IsExecute() && !isProhibitSleepMode())
					{
						PM_GoSleepMode(0, 0, 8);
						LCDC_OFF(flag: false);
					}
				}
				else
				{
					LCDC_OFF(flag: false);
				}
			}

			public static void PreCallback(object arg)
			{
				NNS_SndPlayerPauseAll(1);
				NNS_SndArcStrmStopAll(0);
				GlobalPlayTimeCounter.getSingleton().pause(b: true);
				OS_Printf("Go to sleep \n");
			}

			public static void PostCallback(object arg)
			{
				NNS_SndPlayerPauseAll(0);
				GlobalPlayTimeCounter.getSingleton().pause(b: false);
				OS_Printf("Return from sleep \n");
			}

			public static void LCDC_OFF(bool flag)
			{
				if (flag)
				{
					if (!_bLCDC_OffEnable)
					{
						PM_SetLCDPower(0);
						_bLCDC_OffEnable = true;
					}
				}
				else if (_bLCDC_OffEnable && PM_SetLCDPower(0))
				{
					_bLCDC_OffEnable = false;
				}
			}

			public bool isLCDC_OFF()
			{
				return _bLCDC_OffEnable;
			}

			public void setDepthBufferMode(int mode)
			{
				_gxBufferMode = mode;
			}

			public static bool isProhibitSleepMode()
			{
				return _bSleepEnable;
			}

			public static void setProhibitSleepMode(bool flag)
			{
				_bSleepEnable = flag;
			}

			public static bool isEnableCheckSleep()
			{
				return _bSleepCheckEnable;
			}

			public static void setEnableCheckSleep(bool flag)
			{
				_bSleepCheckEnable = flag;
			}
		}
	}
}
