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
	public static partial class wld
	{
		public class Encount
		{
			public enum ENCOUNT_VRAM
			{
				ENCOUNT_VRAM_ERROR = -1,
				ENCOUNT_VRAM_A,
				ENCOUNT_VRAM_B,
				ENCOUNT_VRAM_C,
				ENCOUNT_VRAM_D,
				ENCOUNT_VRAM_MAX
			}

			public enum ENCOUNT_STATE
			{
				STATE_ERROR = -1,
				STATE_INIT,
				STATE_MODESET,
				STATE_FALL1,
				STATE_FALL2,
				STATE_WHITOUT,
				STATE_END,
				STATE_MAX
			}

			public const ENCOUNT_VRAM ENCOUNT_VRAM_ERROR = ENCOUNT_VRAM.ENCOUNT_VRAM_ERROR;

			public const ENCOUNT_VRAM ENCOUNT_VRAM_A = ENCOUNT_VRAM.ENCOUNT_VRAM_A;

			public const ENCOUNT_VRAM ENCOUNT_VRAM_B = ENCOUNT_VRAM.ENCOUNT_VRAM_B;

			public const ENCOUNT_VRAM ENCOUNT_VRAM_C = ENCOUNT_VRAM.ENCOUNT_VRAM_C;

			public const ENCOUNT_VRAM ENCOUNT_VRAM_D = ENCOUNT_VRAM.ENCOUNT_VRAM_D;

			public const ENCOUNT_VRAM ENCOUNT_VRAM_MAX = ENCOUNT_VRAM.ENCOUNT_VRAM_MAX;

			public const ENCOUNT_STATE STATE_ERROR = ENCOUNT_STATE.STATE_ERROR;

			public const ENCOUNT_STATE STATE_INIT = ENCOUNT_STATE.STATE_INIT;

			public const ENCOUNT_STATE STATE_MODESET = ENCOUNT_STATE.STATE_MODESET;

			public const ENCOUNT_STATE STATE_FALL1 = ENCOUNT_STATE.STATE_FALL1;

			public const ENCOUNT_STATE STATE_FALL2 = ENCOUNT_STATE.STATE_FALL2;

			public const ENCOUNT_STATE STATE_WHITOUT = ENCOUNT_STATE.STATE_WHITOUT;

			public const ENCOUNT_STATE STATE_END = ENCOUNT_STATE.STATE_END;

			public const ENCOUNT_STATE STATE_MAX = ENCOUNT_STATE.STATE_MAX;

			public static Encount _instance = new Encount();

			private static uint FLAG_ENABLE = 1u;

			private static uint FLAG_VRAMSETTING = 2u;

			private static uint FLAG_ZOOMUP = 4u;

			private static uint FLAG_FLASH = 8u;

			private static uint FLAG_BLENDA = 16u;

			private static uint FLAG_WHITEOUT = 32u;

			private static uint FLAG_WAVE = 64u;

			private uint _flag;

			private ENCOUNT_STATE _state;

			private ENCOUNT_VRAM _vram;

			private GXDispMode _dispMode;

			private GXCaptureDest _capDest;

			private ds.sys3d.CCamera _pCamera;

			private ds.sys3d.CModelTexture _ringTex = new ds.sys3d.CModelTexture();

			private Array _pTexData;

			private short _x;

			private short _y;

			private sbyte _blendA;

			private sbyte _curtainAlpha;

			private int _count;

			private int _ringCount;

			private int _cameraSpeed;

			private int _cameraAccel;

			private int _fall1Bound;

			private int _nowRadIdx;

			private int _orgRadIdx;

			private int DEF_CAMERASPEED;

			private int DEF_CAMERAACCEL;

			private int DEF_REFLECT;

			private int DEF_FALL1BOUND;

			private int DEF_OUTSTART;

			private int DEF_OUTTIME;

			public Encount()
			{
				DEF_CAMERASPEED = -174;
				DEF_CAMERAACCEL = -8;
				DEF_REFLECT = 1024;
				DEF_FALL1BOUND = 12;
				DEF_OUTSTART = 15;
				DEF_OUTTIME = 15;
			}

			public void initialize()
			{
				initValue();
				_flag = FLAG_ENABLE;
				_state = ENCOUNT_STATE.STATE_INIT;
				string filename = "ring.ntxp";
				uint size = ds.g_File.getSize(filename);
				_pTexData = ds.CHeap.alloc_app(size);
				ds.g_File.load(_pTexData, filename);
				_ringTex.setup(_pTexData, tdl: false);
			}

			public void terminate()
			{
				_ringTex.cleanup();
				ds.CHeap.free_app(_pTexData);
				if (_pCamera != null)
				{
					int sin = FX_SinIdx((ushort)_orgRadIdx);
					int cos = FX_CosIdx((ushort)_orgRadIdx);
					_pCamera.setFOV(sin, cos);
				}
				initValue();
				GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_GRAPHICS, GXBGMode.GX_BGMODE_6, 0);
			}

			public void prepare(ENCOUNT_VRAM vram, ds.sys3d.CCamera pCamera, byte x, byte y)
			{
				_x = (short)(x - 128);
				_y = (short)(y - 96);
				_pCamera = pCamera;
				_state = ENCOUNT_STATE.STATE_MODESET;
				_vram = vram;
				_pCamera.getFOV(out var sin, out var cos);
				int x2 = FX_Div(sin, cos);
				_orgRadIdx = FX_AtanIdx(x2);
				_nowRadIdx = _orgRadIdx;
				switch (_vram)
				{
				case ENCOUNT_VRAM.ENCOUNT_VRAM_A:
					_capDest = GXCaptureDest.GX_CAPTURE_DEST_VRAM_A_0x00000;
					_dispMode = GXDispMode.GX_DISPMODE_VRAM_A;
					break;
				case ENCOUNT_VRAM.ENCOUNT_VRAM_B:
					_capDest = GXCaptureDest.GX_CAPTURE_DEST_VRAM_B_0x00000;
					_dispMode = GXDispMode.GX_DISPMODE_VRAM_B;
					break;
				case ENCOUNT_VRAM.ENCOUNT_VRAM_C:
					_capDest = GXCaptureDest.GX_CAPTURE_DEST_VRAM_C_0x00000;
					_dispMode = GXDispMode.GX_DISPMODE_VRAM_C;
					break;
				case ENCOUNT_VRAM.ENCOUNT_VRAM_D:
					_capDest = GXCaptureDest.GX_CAPTURE_DEST_VRAM_D_0x00000;
					_dispMode = GXDispMode.GX_DISPMODE_VRAM_D;
					break;
				}
			}

			public void draw()
			{
				if (_pTexData != null && (FLAG_WAVE & _flag) != 0)
				{
					G3_PushMtx();
					G3_OrthoW(-393216, 393216, -524288, 524288, -4194304, 4194304, 4194304, null);
					G3_MtxMode(GXMtxMode.GX_MTXMODE_TEXTURE);
					G3_Identity();
					G3_MtxMode(GXMtxMode.GX_MTXMODE_POSITION);
					G3_Identity();
					G3_Translate(4096 * _x, 4096 * _y, 0);
					int num = FX_Mul(4096 * _ringCount, 4096 * _ringCount);
					G3_Scale(num, num, 0);
					G3_PolygonAttr(0, GXPolygonMode.GX_POLYGONMODE_MODULATE, GXCull.GX_CULL_NONE, 63, 31, 0);
					TexVram addr = NNS_GfdGetTexKeyAddr(_ringTex.getResTex().texInfo.vramKey);
					uint addr2 = NNS_GfdGetPlttKeyAddr(_ringTex.getResTex().plttInfo.vramKey);
					G3_TexImageParam(GXTexFmt.GX_TEXFMT_A5I3, 0, GXTexSizeS.GX_TEXSIZE_S64, GXTexSizeT.GX_TEXSIZE_T64, 0, 0, 0, addr);
					G3_TexPlttBase(addr2, GXTexFmt.GX_TEXFMT_A5I3);
					G3_Begin(GXBegin.GX_BEGIN_QUADS);
					G3_Color(GX_RGB(31, 31, 31));
					G3_TexCoord(0, 0);
					G3_Vtx(-2048, -2048, 0);
					G3_TexCoord(0, 262144);
					G3_Vtx(-2048, 2048, 0);
					G3_TexCoord(262144, 262144);
					G3_Vtx(2048, 2048, 0);
					G3_TexCoord(262144, 0);
					G3_Vtx(2048, -2048, 0);
					G3_End();
					G3_PopMtx(1);
				}
			}

			public void execute()
			{
				if (_pCamera == null || _state < ENCOUNT_STATE.STATE_MODESET || ENCOUNT_STATE.STATE_WHITOUT < _state)
				{
					return;
				}
				_count++;
				_ringCount += 2;
				switch (_state)
				{
				case ENCOUNT_STATE.STATE_MODESET:
					if (_count >= 2)
					{
						GX_SetGraphicsMode(_dispMode, GXBGMode.GX_BGMODE_6, 0);
						_flag |= FLAG_WAVE;
						_flag |= FLAG_BLENDA;
						_blendA = 2;
						_count = 0;
						_ringCount = 0;
						_fall1Bound = DEF_FALL1BOUND;
						_state = ENCOUNT_STATE.STATE_FALL1;
					}
					break;
				case ENCOUNT_STATE.STATE_FALL1:
					if (_count == 5)
					{
						MatrixSound.MtxSENDS_Play(1, 2, 192, 127);
						_flag |= FLAG_ZOOMUP;
						_cameraSpeed = DEF_CAMERASPEED;
						_cameraAccel = DEF_CAMERAACCEL;
						_fall1Bound = DEF_FALL1BOUND;
					}
					if (_count >= _fall1Bound)
					{
						_cameraSpeed = FX_Mul(4096 * _cameraSpeed, -DEF_REFLECT) / 4096;
						_count = 0;
						_state = ENCOUNT_STATE.STATE_FALL2;
					}
					break;
				case ENCOUNT_STATE.STATE_FALL2:
					if (_count >= DEF_OUTSTART)
					{
						dgs.CFade.Main().fadeOut(DEF_OUTTIME, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
						_count = 0;
						_state = ENCOUNT_STATE.STATE_WHITOUT;
					}
					break;
				case ENCOUNT_STATE.STATE_WHITOUT:
					if (dgs.CFade.Main().isFaded())
					{
						_state = ENCOUNT_STATE.STATE_END;
					}
					break;
				}
				if ((_flag & FLAG_ZOOMUP) != 0)
				{
					_cameraSpeed += _cameraAccel;
					if (_cameraSpeed <= -250)
					{
						_cameraSpeed = -250;
					}
					_nowRadIdx += _cameraSpeed;
					if (_nowRadIdx < 50)
					{
						_nowRadIdx = 50;
					}
					int num = FX_SinIdx((ushort)_nowRadIdx);
					int num2 = FX_CosIdx((ushort)_nowRadIdx);
					if (num <= 0)
					{
						num = 1;
					}
					if (num >= 4096)
					{
						num = 4095;
					}
					if (num2 <= -4096)
					{
						num2 = -4095;
					}
					if (num2 >= 4096)
					{
						num2 = 4095;
					}
					_pCamera.setFOV(num, num2);
				}
				if ((_flag & FLAG_BLENDA) != 0)
				{
					_blendA -= 2;
					if (_blendA < 2)
					{
						_blendA = 2;
					}
				}
				GX_SetCapture(GXCaptureSize.GX_CAPTURE_SIZE_256x192, GXCaptureMode.GX_CAPTURE_MODE_AB, GXCaptureSrcA.GX_CAPTURE_SRCA_3D, GXCaptureSrcB.GX_CAPTURE_SRCB_VRAM_0x00000, _capDest, _blendA, 16 - _blendA);
			}

			public bool isEnded()
			{
				return _state == ENCOUNT_STATE.STATE_END;
			}

			public void initValue()
			{
				_flag = 0u;
				_state = ENCOUNT_STATE.STATE_ERROR;
				_capDest = GXCaptureDest.GX_CAPTURE_DEST_VRAM_A_0x00000;
				_pCamera = null;
				_pTexData = null;
				_x = 128;
				_y = 96;
				_blendA = 16;
				_count = 0;
				_ringCount = 0;
			}

			public static Encount getInstance()
			{
				return _instance;
			}
		}
	}
}
