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
	public static partial class sys2d
	{
		public class Sprite3d : Sprite
		{
			public static uint s3dFLAG_MASK = 4294901760u;

			private Ncbr m_Ncbr = new Ncbr();

			private TexVram m_TexKey;

			private TexVram m_PlttKey;

			public Sprite3d()
			{
			}

			public Sprite3d(Sprite3d src)
			{
				copy(src);
			}

			~Sprite3d()
			{
				Release();
			}

			public override void Load(DS2D_OBJ_PLANE plane, string pCe, string pAn, string pCb, string pCl)
			{
				SetPlane(plane);
				s3dLoadCb(pCb);
				s3dLoadCl(pCl);
				LoadCe(pCe);
				LoadAn(pAn);
			}

			public override void Load2(DS2D_OBJ_PLANE plane, string pSame_name)
			{
				sprintf(out var arg, "%s.%s", pSame_name, "NCBR");
				sprintf(out var arg2, "%s.%s", pSame_name, "NCLR");
				sprintf(out var arg3, "%s.%s", pSame_name, "NCER");
				sprintf(out var arg4, "%s.%s", pSame_name, "NANR");
				Load(plane, arg3, arg4, arg, arg2);
			}

			public void s3dLoadCb(string pCb)
			{
				if (pCb != null)
				{
					m_Ncbr.Load(pCb);
					uint szByte = ds.alignment(m_Ncbr.pDataCb().szByte, 16u);
					m_TexKey = NNS_GfdAllocLnkTexVram(szByte, 0, 0u);
					TexVram baseAddr = NNS_GfdGetTexKeyAddr(m_TexKey);
					if (ds.CVram.getInstance().getBankForTex() != GX_GetBankForTex())
					{
						ds.CVram.getInstance().setupBankForTex();
					}
					if (ds.CVram.getInstance().getBankForPltt() != GX_GetBankForTexPltt())
					{
						ds.CVram.getInstance().setupBankForPltt();
					}
					if (m_Ncbr.pDataCg().mapingType != 0)
					{
						SVC_WaitVBlankIntr();
						NNS_G2dLoadImage1DMapping(m_Ncbr.pDataCb(), baseAddr, DS2D_VRAM_TYPE[0], m_ImageProxy);
					}
					else
					{
						SVC_WaitVBlankIntr();
						NNS_G2dLoadImage2DMapping(m_Ncbr.pDataCb(), baseAddr, DS2D_VRAM_TYPE[0], m_ImageProxy);
					}
				}
			}

			public void s3dLoadCl(string pCl)
			{
			}

			public void s3dReleaseCgCl(bool vram_free)
			{
				if (m_Ncbr.Release() && m_TexKey != null)
				{
					NNS_GfdFreeLnkTexVram(m_TexKey);
				}
				if (m_Nclr.Release() && m_Ncer.pData() == null && m_PlttKey != null)
				{
					NNS_GfdFreeLnkPlttVram(m_PlttKey);
				}
			}

			public void Load(string pCe, string pAn, string pCb, string pCl)
			{
				Load(DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, pCe, pAn, pCb, pCl);
			}

			public void Load2(string pSame_name)
			{
				Load2(DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, pSame_name);
			}

			public void s3dCopyCb(Sprite3d s3d)
			{
				m_Ncbr = s3d.s3dGetCb();
			}

			public void s3dCopyCl(Sprite3d s3d)
			{
				m_Nclr = s3d.s3dGetCl();
			}

			public Ncbr s3dGetCb()
			{
				return m_Ncbr;
			}

			public Nclr s3dGetCl()
			{
				return m_Nclr;
			}

			public override void Release()
			{
				m_Ncer.Release();
				m_Nanr.Release();
				s3dReleaseCgCl(vram_free: true);
			}

			public void copy(Sprite3d src)
			{
				copy((Sprite)src);
				m_Ncbr.copy(src.m_Ncbr);
				m_TexKey = src.m_TexKey;
				m_PlttKey = src.m_PlttKey;
			}
		}
	}
}
