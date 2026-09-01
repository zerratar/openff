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
	public static partial class menu
	{
		public class BasicWindow : sys2d.Window
		{
			public enum BW_STATE
			{
				BWS_OFF,
				BWS_ON,
				BWS_MAX
			}

			public const int bwWALL_PAPER_TEXTURE_W = 128;

			public const int bwWALL_PAPER_TEXTURE_H = 128;

			public const int bwWALL_PAPER_TEXTURE_W2 = 256;

			public const int bwWALL_PAPER_TEXTURE_H2 = 256;

			public const int bwWALL2D_H_MAX = 256;

			public const int bwWALL3D_H_MAX = 320;

			public const int bwWALL_PAPER_W_MAX = 2;

			public const int bwWALL_PAPER_H_MAX = 1;

			public const int bwWALL_PAPER_NUM_MAX = 2;

			public const int bwBAR_NONE = 0;

			public const int bwBAR_EVENT_ITEM = 1;

			public const int bwBAR_BATTLE_MAGIC = 2;

			public const int bwBAR_BATTLE_ITEM = 3;

			public const int bwBAR_BATTLE_EQUIP = 4;

			public const BW_STATE BWS_OFF = BW_STATE.BWS_OFF;

			public const BW_STATE BWS_ON = BW_STATE.BWS_ON;

			public const BW_STATE BWS_MAX = BW_STATE.BWS_MAX;

			public static sys2d.Sprite3d g_WindowSS = new sys2d.Sprite3d();

			private sys2d.DS2D_OBJ_PLANE m_Plane;

			private BW_STATE m_DispState;

			private int m_Alpha;

			protected sys2d.Sprite[] m_pWindow1dArray = new sys2d.Sprite[2];

			private ds.Vector2<short> m_nChip = new ds.Vector2<short>();

			private sys2d.Sprite[] m_pFrame = new sys2d.Sprite[18];

			private sys2d.Sprite m_pBar;

			public BasicWindow()
			{
				for (int i = 0; i < 2; i++)
				{
					m_pWindow1dArray[i] = null;
				}
				for (int j = 0; j < 18; j++)
				{
					m_pFrame[j] = null;
				}
				m_pBar = null;
				Initialize();
			}

			~BasicWindow()
			{
				Kill();
			}

			public static void bwInitializeSystem(uint plane_flag)
			{
				g_WindowSS.Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "m000_window.NCER", null, "m000_window.NCBR", "m000_window.NCLR");
			}

			public static void bwReleaseSystem()
			{
				g_WindowSS.Release();
			}

			public override void Initialize()
			{
				base.Initialize();
				m_Plane = sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAX;
				if (MenuManager.getSingleton() != null)
				{
					m_Alpha = (MenuManager.getSingleton().battleMode() ? 15 : 31);
				}
				else
				{
					m_Alpha = 31;
				}
				m_nChip.zero();
				bwFree();
				base.SetShow(show: true, user: true);
				SetShow(show: true, user: true);
			}

			public bool bwCreateCC(sys2d.DS2D_OBJ_PLANE plane, ds.Vector2<short> cc, ds.Vector2<short> size, byte pri)
			{
				Initialize();
				CreateCC(cc, new ds.Vector2<short>(-1, -1));
				m_Plane = plane;
				m_Priority = pri;
				SetSize(size, update: false);
				SetDepth(ds.S32toFX32(10));
				bwSetState(BW_STATE.BWS_OFF);
				return true;
			}

			public void bwSetState(BW_STATE ds)
			{
				if (m_DispState != ds)
				{
					m_DispState = ds;
					bwSetState();
					ds.Vector2<short> size = new ds.Vector2<short>(m_Size);
					m_Size.vx = 0;
					m_Size.vy = 0;
					SetSize(size, update: false);
				}
			}

			public void bwSetState()
			{
				ushort palette = (ushort)((m_DispState != BW_STATE.BWS_OFF) ? 1u : 10u);
				for (int i = 0; i < m_nChip.vy; i++)
				{
					for (int j = 0; j < m_nChip.vx; j++)
					{
						if (m_pWindow1dArray[i * 2 + j] != null)
						{
							m_pWindow1dArray[i * 2 + j].SetPalette(palette);
						}
					}
				}
				for (int k = 0; k < 18; k++)
				{
					if (m_pFrame[k] != null)
					{
						m_pFrame[k].SetCell((ushort)BW_FRAME_ANIM_NO[(m_DispState != BW_STATE.BWS_OFF) ? 1 : 0][k]);
					}
				}
			}

			protected override void SetPositionCC(ds.Vector2<short> cc)
			{
				base.SetPositionCC(cc);
				ds.Vector2<short> vector = new ds.Vector2<short>(base.GetPositionUL());
				ds.Vector2<short> vector2 = new ds.Vector2<short>(vector);
				for (int i = 0; i < m_nChip.vy; i++)
				{
					vector2.vx = vector.vx;
					for (int j = 0; j < m_nChip.vx; j++)
					{
						m_pWindow1dArray[i * 2 + j].SetPositionI(vector2.vx + 2, vector2.vy + 2);
						vector2.vx += 256;
					}
					vector2.vy += 256;
				}
				if (m_pFrame[0] == null)
				{
					return;
				}
				ds.Vector2<short> vector3 = new ds.Vector2<short>(base.GetSize());
				ds.Vector2<short>[] array = BW_FRAME_SIZE[(m_DispState != BW_STATE.BWS_OFF) ? 1 : 0];
				if (m_DispState == BW_STATE.BWS_OFF)
				{
					m_pFrame[0].SetPositionI(vector.vx + 8, vector.vy + 8);
					m_pFrame[6].SetPositionI(vector.vx + vector3.vx - 16 + 8, vector.vy + 8);
					m_pFrame[12].SetPositionI(vector.vx + 8, vector.vy + vector3.vy - 16 + 8);
					m_pFrame[17].SetPositionI(vector.vx + vector3.vx - 16 + 8, vector.vy + vector3.vy - 16 + 8);
					NNSG2dFVec2 nNSG2dFVec = new NNSG2dFVec2();
					if (vector3.w > 32)
					{
						short num = (short)(vector3.w - 16 - 16);
						nNSG2dFVec.x = ((num > array[5].w) ? ds.S32toFX32(vector.vx + vector3.w - 16 - array[5].w + 32) : FX32_CONST((float)vector.vx + (float)vector3.w * 0.5f));
						nNSG2dFVec.y = ds.S32toFX32(vector.vy + 8);
						m_pFrame[5].SetPosition(nNSG2dFVec);
						short num2 = (short)(num - array[5].w);
						if (num2 > 0)
						{
							nNSG2dFVec.x = FX32_CONST((float)(vector.vx + 16) + (float)num2 * 0.5f);
							m_pFrame[1].SetPosition(nNSG2dFVec);
						}
						if (num > 0)
						{
							nNSG2dFVec.y = ds.S32toFX32(vector.vy + vector3.h - 16 + 8);
							nNSG2dFVec.x = FX32_CONST((float)(vector.vx + vector.vx + vector3.w) * 0.5f);
							m_pFrame[13].SetPosition(nNSG2dFVec);
						}
					}
					if (vector3.h > 32)
					{
						short num3 = (short)(vector3.h - 16 - 16);
						nNSG2dFVec.x = ds.S32toFX32(vector.vx + 8);
						nNSG2dFVec.y = ((num3 > 16) ? ds.S32toFX32(vector.vy + vector3.h - 16 - 16 + 8) : FX32_CONST((float)vector.vy + (float)vector3.h * 0.5f));
						m_pFrame[9].SetPosition(nNSG2dFVec);
						short num4 = (short)(num3 - 16);
						if (num4 > 0)
						{
							nNSG2dFVec.y = FX32_CONST((float)(vector.vy + 16) + (float)num4 * 0.5f);
							m_pFrame[7].SetPosition(nNSG2dFVec);
						}
						if (num3 > 0)
						{
							nNSG2dFVec.x = ds.S32toFX32(vector.vx + vector3.w - 16 + 8);
							nNSG2dFVec.y = FX32_CONST((float)(vector.vy + vector.vy + vector3.h) * 0.5f);
							m_pFrame[10].SetPosition(nNSG2dFVec);
						}
					}
					return;
				}
				m_pFrame[0].SetPositionI(vector.vx + vector3.w - 16 + 8, vector.vy + vector3.h - 16 + 8);
				m_pFrame[6].SetPositionI(vector.vx + 8, vector.vy + vector3.h - 16 + 8);
				m_pFrame[12].SetPositionI(vector.vx + vector3.w - 16 + 8, vector.vy + 8);
				m_pFrame[17].SetPositionI(vector.vx + 8, vector.vy + 8);
				NNSG2dFVec2 nNSG2dFVec2 = new NNSG2dFVec2();
				if (vector3.w > 32)
				{
					short num5 = (short)(vector3.w - 16 - 16);
					nNSG2dFVec2.x = ((num5 > array[5].w) ? ds.S32toFX32(vector.vx + 16 + array[5].w / 2) : FX32_CONST((float)(vector.vx + 16) + (float)num5 * 0.5f));
					nNSG2dFVec2.y = ds.S32toFX32(vector.vy + vector3.h - 16 + 8);
					m_pFrame[5].SetPosition(nNSG2dFVec2);
					short num6 = (short)(num5 - array[5].w);
					if (num6 > 0)
					{
						nNSG2dFVec2.x = FX32_CONST((float)(vector.vx + 16 + array[5].w) + (float)num6 * 0.5f);
						m_pFrame[1].SetPosition(nNSG2dFVec2);
					}
					if (num5 > 0)
					{
						nNSG2dFVec2.y = ds.S32toFX32(vector.vy + 8);
						nNSG2dFVec2.x = FX32_CONST((float)(vector.vx + vector.vx + vector3.w) * 0.5f);
						m_pFrame[13].SetPosition(nNSG2dFVec2);
					}
				}
				if (vector3.h > 32)
				{
					short num7 = (short)(vector3.h - 16 - 16);
					nNSG2dFVec2.x = ds.S32toFX32(vector.vx + vector3.w - 16 + 8);
					nNSG2dFVec2.y = ((num7 > 16) ? ds.S32toFX32(vector.vy + 16 + 8) : FX32_CONST((float)vector.vy + (float)vector3.h * 0.5f));
					m_pFrame[9].SetPosition(nNSG2dFVec2);
					short num8 = (short)(num7 - 16);
					if (num8 > 0)
					{
						nNSG2dFVec2.y = FX32_CONST((float)(vector.vy + 16 + 16) + (float)num8 * 0.5f);
						m_pFrame[7].SetPosition(nNSG2dFVec2);
					}
					if (num7 > 0)
					{
						nNSG2dFVec2.x = ds.S32toFX32(vector.vx + 8);
						nNSG2dFVec2.y = FX32_CONST((float)(vector.vy + vector.vy + vector3.h) * 0.5f);
						m_pFrame[10].SetPosition(nNSG2dFVec2);
					}
				}
			}

			public override void SetSize(ds.Vector2<short> size, bool update)
			{
				if (!update && m_Size.w == size.w && m_Size.h == size.h)
				{
					return;
				}
				if (size.w < 12 || size.h < 12)
				{
					base.SetShow(show: false, user: true);
					return;
				}
				base.SetShow(show: true, user: true);
				ds.Vector2<short> vector = new ds.Vector2<short>();
				vector.w = (short)ds.clamp(size.w, 0, 480);
				vector.h = (short)ds.clamp(size.h, 0, 320);
				base.SetSize(vector, update: false);
				m_nChip.w = 1;
				m_nChip.h = 1;
				bwAlloc(m_nChip.w, m_nChip.h);
				m_pWindow1dArray[0].SetScaleF(FX32_CONST((float)(vector.w - 4) / 128f), FX32_CONST((float)(vector.h - 4) / 128f));
				for (int i = 0; i < 18; i++)
				{
					if (m_pFrame[i] != null)
					{
						m_pFrame[i].SetScaleF(ds.S32toFX32(1), ds.S32toFX32(1));
					}
				}
				ds.Vector2<short>[] array = BW_FRAME_SIZE[(m_DispState != BW_STATE.BWS_OFF) ? 1 : 0];
				if (size.w > array[0].w + array[6].w)
				{
					short num = (short)(size.w - array[0].w - array[6].w);
					int x = ((num > array[5].w) ? ds.S32toFX32(1) : FX32_CONST((float)num / (float)array[5].w));
					m_pFrame[5].SetScaleF(x, ds.S32toFX32(1));
					m_pFrame[5].SetShow(base.IsShow());
					num -= array[5].w;
					if (num <= 0)
					{
						m_pFrame[1].SetShow(show: false);
						m_pFrame[2].SetShow(show: false);
						m_pFrame[3].SetShow(show: false);
						m_pFrame[4].SetShow(show: false);
					}
					else
					{
						m_pFrame[1].SetScaleF(FX32_CONST((float)num / 64f), ds.S32toFX32(1));
						m_pFrame[1].SetShow(base.IsShow());
						for (int j = 1; j < 4; j++)
						{
							m_pFrame[1 + j].SetShow(show: false);
						}
					}
					num = (short)(size.w - 16 - 16);
					m_pFrame[13].SetScaleF(FX32_CONST((float)num / 64f), ds.S32toFX32(1));
					m_pFrame[13].SetShow(base.IsShow());
					for (int k = 1; k < 4; k++)
					{
						m_pFrame[13 + k].SetShow(show: false);
					}
				}
				else
				{
					m_pFrame[5].SetShow(show: false);
					for (int l = 0; l < 4; l++)
					{
						m_pFrame[1 + l].SetShow(show: false);
						m_pFrame[13 + l].SetShow(show: false);
					}
				}
				if (size.h > 32)
				{
					short num2 = (short)(size.h - 16 - 16);
					int y = ((num2 > 16) ? ds.S32toFX32(1) : FX32_CONST((float)num2 / 16f));
					m_pFrame[9].SetScaleF(ds.S32toFX32(1), y);
					m_pFrame[9].SetShow(base.IsShow());
					num2 -= 16;
					if (num2 <= 0)
					{
						m_pFrame[7].SetShow(show: false);
						m_pFrame[8].SetShow(show: false);
					}
					else
					{
						m_pFrame[7].SetScaleF(ds.S32toFX32(1), FX32_CONST((float)num2 / 64f));
						m_pFrame[7].SetShow(base.IsShow());
						for (int m = 1; m < 2; m++)
						{
							m_pFrame[7 + m].SetShow(show: false);
						}
					}
					num2 = (short)(size.h - 16 - 16);
					m_pFrame[10].SetScaleF(ds.S32toFX32(1), FX32_CONST((float)num2 / 64f));
					m_pFrame[10].SetShow(base.IsShow());
					for (int n = 1; n < 2; n++)
					{
						m_pFrame[10 + n].SetShow(show: false);
					}
				}
				else
				{
					m_pFrame[9].SetShow(show: false);
					for (int num3 = 0; num3 < 2; num3++)
					{
						m_pFrame[7 + num3].SetShow(show: false);
						m_pFrame[10 + num3].SetShow(show: false);
					}
				}
				SetPositionCC(base.GetPositionCC());
				SetPriority(m_Priority);
				SetDepth(m_Depth);
				SetAlpha((byte)m_Alpha);
				bwSetState();
			}

			public override void SetShow(bool show, bool user)
			{
				base.SetShow(show, user: true);
				if (base.IsShow())
				{
					if (m_pFrame[0] == null)
					{
						return;
					}
					SetSize(base.GetSize(), update: true);
					for (int i = 0; i < 2; i++)
					{
						if (m_pWindow1dArray[i] != null)
						{
							m_pWindow1dArray[i].SetShow(show);
						}
					}
					m_pFrame[0].SetShow(show);
					m_pFrame[6].SetShow(show);
					m_pFrame[12].SetShow(show);
					m_pFrame[17].SetShow(show);
					return;
				}
				for (int j = 0; j < 2; j++)
				{
					if (m_pWindow1dArray[j] != null)
					{
						m_pWindow1dArray[j].SetShow(show);
					}
				}
				for (int k = 0; k < 18; k++)
				{
					if (m_pFrame[k] != null)
					{
						m_pFrame[k].SetShow(show);
					}
				}
			}

			public override void SetPriority(byte pri)
			{
				m_Priority = pri;
				for (int i = 0; i < 2; i++)
				{
					if (m_pWindow1dArray[i] != null)
					{
						m_pWindow1dArray[i].SetPriority(pri);
					}
				}
				for (int j = 0; j < 18; j++)
				{
					if (m_pFrame[j] != null)
					{
						m_pFrame[j].SetPriority(pri);
					}
				}
			}

			protected override void SetDepth(int depth)
			{
				m_Depth = depth;
				for (int i = 0; i < 2; i++)
				{
					if (m_pWindow1dArray[i] != null)
					{
						m_pWindow1dArray[i].SetDepth(depth + ds.S32toFX32(16));
					}
				}
				for (int j = 0; j < 18; j++)
				{
					if (m_pFrame[j] != null)
					{
						m_pFrame[j].SetDepth(depth);
					}
				}
			}

			public void SetAlpha(byte alpha)
			{
				m_Alpha = alpha;
				for (int i = 0; i < 2; i++)
				{
					if (m_pWindow1dArray[i] != null)
					{
						m_pWindow1dArray[i].SetAlpha(alpha);
					}
				}
			}

			public void SetBar(int type)
			{
				if (m_pBar == null)
				{
					m_pBar = new sys2d.Sprite3d();
					m_pBar.Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "m015_bar.NCER", null, "m015_bar.NCBR", "m015_bar.NCLR");
					m_pBar.SetPriority(m_Priority);
					m_pBar.SetDepth(m_Depth + 1);
					m_pBar.SetShow(show: true);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(m_pBar);
				}
				m_pBar.SetCell((ushort)type);
			}

			public override void Release()
			{
				Kill();
			}

			public override void Kill()
			{
				base.Kill();
				bwFree();
				Initialize();
			}

			public bool bwAlloc(int w, int h)
			{
				for (int i = 0; i < h; i++)
				{
					for (int j = 0; j < w; j++)
					{
						sys2d.Sprite pSprite = m_pWindow1dArray[i * 2 + j];
						if (pSprite == null)
						{
							bwAllocAndCopy(ref pSprite, m_Plane);
							m_pWindow1dArray[i * 2 + j] = pSprite;
						}
					}
				}
				for (int i = 0; i < 1; i++)
				{
					for (int j = 0; j < 2; j++)
					{
						if (i >= h || j >= w)
						{
							bwFree(j, i);
						}
					}
				}
				if (m_pFrame[0] == null)
				{
					for (int k = 0; k < 18; k++)
					{
						bwAllocAndCopy(ref m_pFrame[k], m_Plane);
						m_pFrame[k].SetCell((ushort)BW_FRAME_ANIM_NO[0][k]);
					}
				}
				return true;
			}

			public void bwAllocAndCopy(ref sys2d.Sprite pSprite, sys2d.DS2D_OBJ_PLANE plane)
			{
				if (plane == sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D)
				{
					pSprite = (sys2d.Sprite)ds.CHeap.alloc_app(typeof(sys2d.Sprite3d));
					pSprite = new sys2d.Sprite3d(g_WindowSS);
				}
				sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(pSprite);
			}

			public void bwFree(int w, int h)
			{
				sys2d.Sprite sprite = m_pWindow1dArray[h * 2 + w];
				if (sprite != null)
				{
					sprite.Release();
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(sprite);
					sprite.destruct();
					ds.CHeap.free_app(sprite);
					sprite = null;
				}
			}

			public void bwFree()
			{
				for (int i = 0; i < 2; i++)
				{
					if (m_pWindow1dArray[i] != null)
					{
						m_pWindow1dArray[i].Release();
						sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_pWindow1dArray[i]);
						m_pWindow1dArray[i].destruct();
						ds.CHeap.free_app(m_pWindow1dArray[i]);
						m_pWindow1dArray[i] = null;
					}
				}
				if (m_pFrame[0] != null)
				{
					for (int j = 0; j < 18; j++)
					{
						m_pFrame[j].Release();
						sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_pFrame[j]);
						m_pFrame[j].destruct();
						ds.CHeap.free_app(m_pFrame[j]);
						m_pFrame[j] = null;
					}
				}
				if (m_pBar != null)
				{
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_pBar);
					m_pBar.Release();
					NNS_G2dReleaseImageProxy(m_pBar.GetImageProxy());
					m_pBar.destruct();
					m_pBar = null;
				}
			}

			public bool bwCreateUL(sys2d.DS2D_OBJ_PLANE plane, ds.Vector2<short> ul, ds.Vector2<short> size, byte pri)
			{
				ds.Vector2<short> positionCCfromUL = GetPositionCCfromUL(ul, size);
				return bwCreateCC(plane, positionCCfromUL, size, pri);
			}

			public BW_STATE bwGetState()
			{
				return m_DispState;
			}

			public override void SetPositionUL(ds.Vector2<short> ul)
			{
				ds.Vector2<short> positionCCfromUL = GetPositionCCfromUL(ul, GetSize());
				SetPositionCC(positionCCfromUL);
			}
		}
	}
}
