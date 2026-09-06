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
		public const int PAD_DOWN = 128;

		public const int PAD_UP = 64;

		public const int PAD_RIGHT = 16;

		public const int PAD_LEFT = 32;

		public const int PAD_A = 1;

		public const int PAD_B = 2;

		public const int PAD_X = 1024;

		public const int PAD_Y = 2048;

		public const int PAD_L = 512;

		public const int PAD_R = 256;

		public const int PAD_START = 8;

		public const int PAD_SELECT = 4;

		public const int PAD_DBG = 8192;

		public const enVRAM_BANK enVRAM_A = enVRAM_BANK.enVRAM_A;

		public const enVRAM_BANK enVRAM_B = enVRAM_BANK.enVRAM_B;

		public const enVRAM_BANK enVRAM_C = enVRAM_BANK.enVRAM_C;

		public const enVRAM_BANK enVRAM_D = enVRAM_BANK.enVRAM_D;

		public const enVRAM_BANK enVRAM_E = enVRAM_BANK.enVRAM_E;

		public const enVRAM_BANK enVRAM_F = enVRAM_BANK.enVRAM_F;

		public const enVRAM_BANK enVRAM_G = enVRAM_BANK.enVRAM_G;

		public const enVRAM_BANK enVRAM_H = enVRAM_BANK.enVRAM_H;

		public const enVRAM_BANK enVRAM_I = enVRAM_BANK.enVRAM_I;

		public static uint unLimitWorkSize = 16384u;

		private static int DS_SCREEN_WIDTH = 480;

		private static int DS_SCREEN_HEIGHT = 320;

		public static int DS_SCREEN_WIDTH_HALF = DS_SCREEN_WIDTH >> 1;

		private static int DS_SCREEN_HEIGHT_HALF = DS_SCREEN_HEIGHT >> 1;

		private static int DS_ALPHA_MIN = 0;

		private static int DS_ALPHA_MAX = 31;

		private static ushort DS_COLOR_BLACK = 0;

		public static ushort DS_COLOR_WHITE = 32767;

		public static CPad g_Pad = new CPad();

		public static _Pad Pad;

		public static uint SOFTRESET_KEYDEF = 768u;

		public static TouchPanel g_TouchPanel = new TouchPanel();

		public static ushort INDEX_PI = 32767;

		public static ushort INDEX_PI_2 = 16384;

		public static ushort INDEX_PI_3 = 10922;

		public static ushort INDEX_PI_4 = 8192;

		public static ushort INDEX_3PI_4 = 24575;

		public static MtxFx43 g_mtxIdentity43 = new MtxFx43(4096, 0, 0, 0, 4096, 0, 0, 0, 4096, 0, 0, 0);

		public static _g_pVXAllocFunc g_pVXAllocFunc = null;

		public static _g_pVXFreeFunc g_pVXFreeFunc = null;

		private static _g_pSoundAllocFunc g_pSoundAllocFunc = null;

		private static _g_pSoundFreeFunc g_pSoundFreeFunc = null;

		public static int g_DSVXFlipStatus;

		public static int g_DSVXBlitImageFlag;

		public static int g_DSVXDisplayedBuffer;

		public static int g_DSVXAlarm;

		public static GXOamAttr[] gOamAttr = new GXOamAttr[128]
		{
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr(),
			new GXOamAttr()
		};

		public static int g_IsInit = 0;

		public static int g_OptionVolume = 0;

		public static int g_ProgramVolume = 0;

		public static int g_MasterVolume = 0;

		public static int g_SlaveVolume = 0;

		public static int sRDE_VCOUNT_START = 191;

		public static int sRDE_VCOUNT_SAFETY = 191;

		public static int sRDE_VCOUNT_END = 213;

		public static string TextureCode = "NTPK";

		public static HeapSystemImp _impSys = new HeapSystemImp();

		public static HeapSystemImp _impApp = new HeapSystemImp();

		public static HeapSystemImp _impDTCM = new HeapSystemImp();

		public static MATHRandContext16 Rand16;

		public static PerformanceCounter g_PFC = new PerformanceCounter();

		public static CFile g_File = new CFile();

		public static void setCompressInfo(Archive.CompressInfo info, MICompressionHeader header)
		{
			info.enCompType = Archive.enCOMP_TYPE.enCOMP_LZ;
			info.unCompParam = header.compParam_4;
			info.unExtractSize = header.destSize_24;
		}

		internal static ArchiveImp createArchiveImplement()
		{
			return (ArchiveImp)CHeap.alloc_sys(typeof(ArchiveImp));
		}

		internal static void deleteArchiveImplement(ArchiveImp pImp)
		{
			if (pImp != null)
			{
				pImp.destruct();
				CHeap.free_sys(pImp);
			}
		}

		internal static uint secondToHH(uint nSecond)
		{
			return nSecond / 3600;
		}

		internal static uint secondToMM(uint nSecond)
		{
			return nSecond % 3600 / 60;
		}

		internal static uint secondToSS(uint nSecond)
		{
			return nSecond % 3600 % 60;
		}

		internal static ushort asinIdx(int sin)
		{
			int num = clamp(sin, -4096, 4096);
			int x = FX_Div(num, FX_Sqrt(4096 - FX_Mul(num, num)));
			return (ushort)FX_AtanIdx(x);
		}

		internal static ushort acosIdx(int cos)
		{
			int num = clamp(cos, -4096, 4096);
			int x = FX_Div(num, FX_Sqrt(4096 - FX_Mul(num, num)));
			return (ushort)(FX_AtanIdx(x) + INDEX_PI_2);
		}

		public static T clamp<T>(T t, T tmin, T tmax)
		{
			if (!_isL(t, tmin))
			{
				if (!_isG(t, tmax))
				{
					return t;
				}
				return tmax;
			}
			return tmin;
		}

		public static T abs<T>(T t)
		{
			if (!_isL(t, default(T)))
			{
				return t;
			}
			return _neg(t);
		}

		public static T min<T>(T t1, T t2)
		{
			if (!_isLE(t1, t2))
			{
				return t2;
			}
			return t1;
		}

		public static T min3<T>(T t1, T t2, T t3)
		{
			return min(_isLE(t1, t2) ? t1 : t2, t3);
		}

		public static T max<T>(T t1, T t2)
		{
			if (!_isGE(t1, t2))
			{
				return t2;
			}
			return t1;
		}

		public static T max3<T>(T t1, T t2, T t3)
		{
			return max(_isGE(t1, t2) ? t1 : t2, t3);
		}

		public static void swap<T>(ref T a, ref T b)
		{
			T val = a;
			a = b;
			b = val;
		}

		public static int S32toFX32(int i)
		{
			return 4096 * i;
		}

		public static ushort DEGto65536(int i)
		{
			return (ushort)(i * 65536 / 360);
		}

		public static T limitRange<T>(T val, T a, T b)
		{
			return clamp(val, a, b);
		}

		public static bool isLimitRange<T>(T val, T a, T b)
		{
			if (_isLE(a, val) && _isLE(val, b))
			{
				return true;
			}
			return false;
		}

		public static T pow2<T>(T a)
		{
			return _mul(a, a);
		}

		public static int limitLoop(int x, int _min, int _max)
		{
			if (x >= _min)
			{
				if (x <= _max)
				{
					return x;
				}
				return _min + (x - _max - 1);
			}
			return _max + (x - _min + 1);
		}

		public static int limitLoop2(int x, int _min, int _max)
		{
			if (x >= _min)
			{
				if (x <= _max)
				{
					return x;
				}
				return _min;
			}
			return _max;
		}

		public static uint alignment(uint x, uint y)
		{
			return (x + (y - 1)) / y * y;
		}

		public static bool moveLinear<T>(ref T pos, ref T to, ref uint c)
		{
			if (c <= 1)
			{
				pos = to;
				c = 0u;
				return false;
			}
			T arg = to;
			arg = _sub(arg, pos);
			arg = _div2(arg, c);
			pos = _add(pos, arg);
			c--;
			return true;
		}

		public static VecFx32 DsToFxVector(Vector3<int> v)
		{
			return new VecFx32(v.vx, v.vy, v.vz);
		}

		public static Vector3<int> FxToDsVector(VecFx32 v)
		{
			return new Vector3<int>(v.x, v.y, v.z);
		}

		public static Vector3<float> VecFx32ToFVector3(VecFx32 v)
		{
			Vector3<float> vector = new Vector3<float>();
			vector.vx = FX_FX32_TO_F32(v.x);
			vector.vy = FX_FX32_TO_F32(v.y);
			vector.vz = FX_FX32_TO_F32(v.z);
			return vector;
		}

		public static VecFx32 FVector3ToVecFx32(Vector3<float> v)
		{
			VecFx32 vecFx = new VecFx32();
			vecFx.x = FX_F32_TO_FX32(v.vx);
			vecFx.y = FX_F32_TO_FX32(v.vy);
			vecFx.z = FX_F32_TO_FX32(v.vz);
			return vecFx;
		}

		public static ushort getGXRgb(Vector3<short> v)
		{
			return GX_RGB(v.cr, v.cg, v.cb);
		}

		public static ushort getGXRgb(Vector4<short> v)
		{
			return GX_RGB(v.cr, v.cg, v.cb);
		}

		internal static void soundCallback(object pArg)
		{
			((MovieHandleDS)pArg).soundPacketsStreamed_++;
		}

		internal static void preSleepCallback(MovieHandleDS pArg)
		{
			if (pArg != null)
			{
				pArg.sleeping_ = true;
				PM_SetLCDPower(0);
			}
		}

		internal static void postSleepCallback(MovieHandleDS pArg)
		{
			if (pArg != null)
			{
				pArg.sleeping_ = false;
				PM_SetLCDPower(0);
			}
		}

		public static void DSVX_setVXMalloc(_g_pVXAllocFunc pAllocFunc)
		{
			g_pVXAllocFunc = pAllocFunc;
		}

		public static void DSVX_setVXFree(_g_pVXFreeFunc pFreeFunc)
		{
			g_pVXFreeFunc = pFreeFunc;
		}

		public static void DSVX_setSoundMalloc(_g_pSoundAllocFunc pAllocFunc)
		{
			g_pSoundAllocFunc = pAllocFunc;
		}

		public static void DSVX_setSoundFree(_g_pSoundFreeFunc pFreeFunc)
		{
			g_pSoundFreeFunc = pFreeFunc;
		}

		internal static Array DSVX_SoundMalloc(uint size)
		{
			return g_pSoundAllocFunc(size);
		}

		internal static void DSVX_SoundFree(Array p_mem)
		{
			g_pSoundFreeFunc(p_mem);
		}

		internal static void DSVX_MovieSetup(bool bBufferMode)
		{
			OS_EnableIrqMask(1u);
			OS_EnableIrq();
			GX_VBlankIntr(1);
			GX_DisableBankForARM7();
			GX_DisableBankForClearImage();
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
			GX_SetBankForLCDC(0);
			GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_VRAM_A, GXBGMode.GX_BGMODE_0, 0);
		}

		internal static void DSVX_MovieSetupDualScreen()
		{
			OS_SetIrqFunction(1u, DSVX_VBlankIntr);
			OS_EnableIrqMask(1u);
			OS_EnableIrq();
			GX_VBlankIntr(1);
			GX_SetBankForLCDC(0);
			GX_DisableBankForLCDC();
			GX_SetBankForLCDC(0);
			GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_VRAM_A, GXBGMode.GX_BGMODE_0, 0);
			GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_MAIN_SUB);
			GXS_SetOBJVRamModeBmp(0);
			for (int i = 0; i < 128; i++)
			{
				gOamAttr[i].attr01 = 0;
				gOamAttr[i].attr23 = 0;
			}
			int num = 0;
			for (int j = 0; j < 192; j += 64)
			{
				int num2 = 0;
				while (num2 < 256)
				{
					G2_SetOBJAttr(gOamAttr[num], num2, j, 0, 0, 0, 0, 0, 0, j / 8 * 32 + num2 / 8, 15, 0);
					num2 += 64;
					num++;
				}
			}
			GX_ResetBankForSubBG();
			GX_ResetBankForSubOBJ();
			GX_SetBankForSubOBJ(GXVRamSubOBJ.GX_VRAM_SUB_OBJ_128_D);
			GX_SetBankForSubBG(GXVRamSubBG.GX_VRAM_SUB_BG_128_C);
			GXS_SetGraphicsMode(GXBGMode.GX_BGMODE_5);
			G2S_SetBG2ControlDCBmp(0, 0, 0);
			G2S_SetBG2Priority(0);
			G2S_BG2Mosaic(0);
			GXS_SetVisiblePlane(4);
			GX_DispOn();
			GXS_DispOn();
			g_DSVXDisplayedBuffer = 0;
			g_DSVXBlitImageFlag = 0;
			g_DSVXFlipStatus = 1;
		}

		internal static void DSVX_MovieCloseDualScreen()
		{
			GX_SetBankForLCDC(0);
			GX_DisableBankForLCDC();
			GX_DisableBankForSubOBJ();
			GX_DisableBankForSubBG();
			GX_DisableBankForOBJ();
			GX_DisableBankForBG();
		}

		internal static void DSVX_StartCallbackDefault()
		{
		}

		internal static void DSVX_StopCallbackDefault()
		{
		}

		internal static void DSVX_VBlankIntr()
		{
			if (g_DSVXFlipStatus == 0)
			{
				g_DSVXFlipStatus = 1;
				DSVX_FlipBackBuffer();
			}
			OS_SetIrqCheckFlag(1u);
		}

		internal static void DSVX_AlarmIntr(Array pArg)
		{
			pArg = pArg;
			g_DSVXBlitImageFlag = 1;
		}

		internal static void DSVX_FlipBackBuffer()
		{
			if (g_DSVXDisplayedBuffer == 0)
			{
				GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_VRAM_B, GXBGMode.GX_BGMODE_0, 0);
				GXS_SetVisiblePlane(16);
			}
			else
			{
				GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_VRAM_A, GXBGMode.GX_BGMODE_0, 0);
				GXS_SetVisiblePlane(4);
			}
			g_DSVXDisplayedBuffer ^= 1;
		}

		internal static ushort[] DSVX_GetMainBackBuffer()
		{
			return null;
		}

		internal static ushort[] DSVX_GetSubBackBuffer()
		{
			return null;
		}

		internal static int DSVX_getFlipStatus()
		{
			return g_DSVXFlipStatus;
		}

		internal static int DSVX_getBlitImageFlag()
		{
			return g_DSVXBlitImageFlag;
		}

		internal static void DSVX_resetFlipStatus()
		{
			g_DSVXFlipStatus = 0;
		}

		internal static void DSVX_resetBlitImageFlag()
		{
			g_DSVXBlitImageFlag = 0;
		}

		internal static int SndArcStrmCallback(int Status, NNSSndArcStrmCallbackInfo pInfo, int pParam, Array pArg)
		{
			pInfo = pInfo;
			pParam = pParam;
			int result = 0;
			if (Status == 0 && reinterpret_cast<StrmHandle>(pArg).m_bLoopFlag == 1)
			{
				result = 1;
			}
			return result;
		}

		internal static bool validateIDCode(byte[] pCode, string pID)
		{
			if ((byte)pID[0] == pCode[0] && (byte)pID[1] == pCode[1] && (byte)pID[2] == pCode[2])
			{
				return (byte)pID[3] == pCode[3];
			}
			return false;
		}

		internal static void HVFreeAllBlockByID(Array memblock, int handle, uint _id)
		{
			if (_id >= 0)
			{
				_ = 255;
			}
			ushort num = NNS_FndGetGroupIDForMBlockExpHeap(memblock);
			if (_id == num)
			{
				CHeap.free_app(memblock);
			}
		}

		internal static void init_rand(uint _seed)
		{
			MATH_InitRand16(Rand16, _seed);
		}

		internal static ushort rand(ushort _max)
		{
			return RandomNumber.rand16(_max);
		}

		public static bool isFlag(int x, byte[] pFlag)
		{
			return ((pFlag[x >> 3] >> (x & 7)) & 1) != 0;
		}

		public static void onFlag(int x, byte[] pFlag)
		{
			pFlag[x >> 3] |= (byte)(1 << (x & 7));
		}

		public static void offFlag(int x, byte[] pFlag)
		{
			pFlag[x >> 3] &= (byte)(~(1 << (x & 7)));
		}

		public static void switchFlag(int x, bool sw, byte[] pFlag)
		{
			if (sw)
			{
				onFlag(x, pFlag);
			}
			else
			{
				offFlag(x, pFlag);
			}
		}

		public static bool isFlag(int x, ref int flag)
		{
			if ((flag & x) == 0)
			{
				return false;
			}
			return true;
		}

		public static bool isFlag(uint x, ref uint flag)
		{
			if ((flag & x) == 0)
			{
				return false;
			}
			return true;
		}

		public static void onFlag(int x, ref int flag)
		{
			flag |= x;
		}

		public static void onFlag(uint x, ref uint flag)
		{
			flag |= x;
		}

		public static void offFlag(int x, ref int flag)
		{
			flag &= ~x;
		}

		public static void offFlag(uint x, ref uint flag)
		{
			flag &= ~x;
		}

		public static void switchFlag(int x, bool sw, ref int flag)
		{
			if (sw)
			{
				onFlag(x, ref flag);
			}
			else
			{
				offFlag(x, ref flag);
			}
		}

		public static void switchFlag(uint x, bool sw, ref uint flag)
		{
			if (sw)
			{
				onFlag(x, ref flag);
			}
			else
			{
				offFlag(x, ref flag);
			}
		}

		public static ushort setGXRgb(byte r, byte g, byte b)
		{
			return (ushort)(r | (g << 5) | (b << 10));
		}

		public static byte getGXR(ushort color)
		{
			return (byte)(color & 0x1F);
		}

		public static byte getGXG(ushort color)
		{
			return (byte)((color & 0x3E0) >> 5);
		}

		public static byte getGXB(ushort color)
		{
			return (byte)((color & 0x7C00) >> 10);
		}

	}
}
