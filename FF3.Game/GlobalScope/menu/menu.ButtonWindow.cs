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
	public static partial class menu
	{
		public class ButtonWindow : sys2d.Window
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

			public const BW_STATE BWS_OFF = BW_STATE.BWS_OFF;

			public const BW_STATE BWS_ON = BW_STATE.BWS_ON;

			public const BW_STATE BWS_MAX = BW_STATE.BWS_MAX;

			public static sys2d.Sprite3d g_WindowSS = new sys2d.Sprite3d();

			private sys2d.DS2D_OBJ_PLANE m_Plane;

			private BW_STATE m_DispState;

			private sys2d.Sprite[] m_pFrame = new sys2d.Sprite[8];

			public ButtonWindow()
			{
				for (int i = 0; i < 8; i++)
				{
					m_pFrame[i] = null;
				}
				Initialize();
			}

			~ButtonWindow()
			{
				Kill();
			}

			public static void bwInitializeSystem(uint plane_flag)
			{
				g_WindowSS.Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "m014_button.NCER", null, "m014_button.NCBR", "m014_button.NCLR");
			}

			public static void bwReleaseSystem()
			{
				g_WindowSS.Release();
			}

			public override void Initialize()
			{
				base.Initialize();
				m_Plane = sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAX;
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
				for (int i = 0; i < 8; i++)
				{
					if (m_pFrame[i] != null)
					{
						m_pFrame[i].SetCell((ushort)(((m_DispState != BW_STATE.BWS_OFF) ? 8 : 0) + i));
					}
				}
			}

			protected override void SetPositionCC(ds.Vector2<short> cc)
			{
				base.SetPositionCC(cc);
				ds.Vector2<short> vector = new ds.Vector2<short>(base.GetPositionUL());
				if (m_pFrame[0] != null)
				{
					ds.Vector2<short> vector2 = new ds.Vector2<short>(base.GetSize());
					m_pFrame[0].SetPositionI(vector.vx + 4, vector.vy + 4);
					m_pFrame[6].SetPositionI(vector.vx + vector2.vx - 4, vector.vy + 4);
					m_pFrame[2].SetPositionI(vector.vx + 4, vector.vy + vector2.vy - 4);
					m_pFrame[4].SetPositionI(vector.vx + vector2.vx - 4, vector.vy + vector2.vy - 4);
					NNSG2dFVec2 nNSG2dFVec = new NNSG2dFVec2();
					short num = (short)MATH_MAX(vector2.w - 8, 0);
					nNSG2dFVec.x = FX32_CONST((float)(vector.vx + 4) + (float)num * 0.5f);
					nNSG2dFVec.y = FX32_CONST(vector.vy + 4);
					m_pFrame[7].SetPosition(nNSG2dFVec);
					nNSG2dFVec.y = ds.S32toFX32(vector.vy + vector2.h - 4);
					m_pFrame[3].SetPosition(nNSG2dFVec);
					short num2 = (short)MATH_MAX(vector2.h - 8, 0);
					nNSG2dFVec.x = FX32_CONST(vector.vx + 4);
					nNSG2dFVec.y = FX32_CONST((float)(vector.vy + 4) + (float)num2 * 0.5f);
					m_pFrame[1].SetPosition(nNSG2dFVec);
					nNSG2dFVec.x = ds.S32toFX32(vector.vx + vector2.w - 4);
					m_pFrame[5].SetPosition(nNSG2dFVec);
				}
			}

			public override void SetSize(ds.Vector2<short> size, bool update)
			{
				if (update || m_Size.w != size.w || m_Size.h != size.h)
				{
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
					bwAlloc(1, 1);
					short num = (short)MATH_MAX(size.w - 16, 0);
					m_pFrame[7].SetScaleF(FX32_CONST((float)num / 16f), ds.S32toFX32(1));
					m_pFrame[3].SetScaleF(FX32_CONST((float)num / 16f), ds.S32toFX32(1));
					short num2 = (short)MATH_MAX(size.h - 16, 0);
					m_pFrame[1].SetScaleF(ds.S32toFX32(1), FX32_CONST((float)num2 / 16f));
					m_pFrame[5].SetScaleF(ds.S32toFX32(1), FX32_CONST((float)num2 / 16f));
					SetPositionCC(base.GetPositionCC());
					SetPriority(m_Priority);
					SetDepth(m_Depth);
					bwSetState();
				}
			}

			public override void SetShow(bool show, bool user)
			{
				base.SetShow(show, user: true);
				for (int i = 0; i < 8; i++)
				{
					if (m_pFrame[i] != null)
					{
						m_pFrame[i].SetShow(show);
					}
				}
			}

			public override void SetPriority(byte pri)
			{
				m_Priority = pri;
				for (int i = 0; i < 8; i++)
				{
					if (m_pFrame[i] != null)
					{
						m_pFrame[i].SetPriority(pri);
					}
				}
			}

			protected override void SetDepth(int depth)
			{
				m_Depth = depth;
				for (int i = 0; i < 8; i++)
				{
					if (m_pFrame[i] != null)
					{
						m_pFrame[i].SetDepth(depth);
					}
				}
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
				if (m_pFrame[0] == null)
				{
					for (int i = 0; i < 8; i++)
					{
						bwAllocAndCopy(ref m_pFrame[i], m_Plane);
						m_pFrame[i].SetCell((ushort)BW_FRAME_ANIM_NO[0][i]);
					}
				}
				return true;
			}

			public void bwAllocAndCopy(ref sys2d.Sprite pSprite, sys2d.DS2D_OBJ_PLANE plane)
			{
				pSprite = (sys2d.Sprite)ds.CHeap.alloc_app(typeof(sys2d.Sprite3d));
				pSprite = new sys2d.Sprite3d(g_WindowSS);
				sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(pSprite);
			}

			public void bwFree(int w, int h)
			{
			}

			public void bwFree()
			{
				if (m_pFrame[0] != null)
				{
					for (int i = 0; i < 8; i++)
					{
						m_pFrame[i].Release();
						sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_pFrame[i]);
						m_pFrame[i].destruct();
						ds.CHeap.free_app(m_pFrame[i]);
						m_pFrame[i] = null;
					}
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
