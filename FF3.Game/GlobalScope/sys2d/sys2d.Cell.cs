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
	public static partial class sys2d
	{
		public class Cell : Sprite
		{
			public static uint ceFLAG_MASK = 4294901760u;

			private Ncgr m_Ncgr = new Ncgr();

			~Cell()
			{
				Release();
			}

			public override void Load(DS2D_OBJ_PLANE plane, string pCe, string pAn, string pCg, string pCl)
			{
				SetPlane(plane);
				ceLoadCg(plane, pCg);
				ceLoadCl(plane, pCl);
				NNS_G2dSetImageExtPaletteFlag(m_ImageProxy, m_PaletteProxy.bExtendedPlt);
				LoadCe(pCe);
				LoadAn(pAn);
			}

			public void Load(DS2D_OBJ_PLANE plane, string pCe, string pAn, byte[] pCg, string pCl)
			{
				SetPlane(plane);
				ceLoadCg(plane, pCg);
				ceLoadCl(plane, pCl);
				NNS_G2dSetImageExtPaletteFlag(m_ImageProxy, m_PaletteProxy.bExtendedPlt);
				LoadCe(pCe);
				LoadAn(pAn);
			}

			public override void Load2(DS2D_OBJ_PLANE plane, string pSame_name)
			{
				sprintf(out var arg, "%s.%s", pSame_name, "NCGR");
				sprintf(out var arg2, "%s.%s", pSame_name, "NCLR");
				sprintf(out var arg3, "%s.%s", pSame_name, "NCER");
				sprintf(out var arg4, "%s.%s", pSame_name, "NANR");
				Load(plane, arg3, arg4, arg, arg2);
			}

			public void ceLoadCg(DS2D_OBJ_PLANE plane, string pCg)
			{
				if (pCg != null)
				{
					m_Ncgr.Load(pCg);
					TexVram baseAddr = DS2DManager.d2dGetInstance().d2dGetObjCgOffset(plane);
					DS2DManager.d2dGetInstance().d2dAddObjCgOffset(plane, m_Ncgr.pDataCg().szByte);
					if (m_Ncgr.pDataCg().mapingType != 0)
					{
						SVC_WaitVBlankIntr();
						NNS_G2dLoadImage1DMapping(m_Ncgr.pDataCg(), baseAddr, DS2D_VRAM_TYPE[(int)plane], m_ImageProxy);
					}
					else
					{
						SVC_WaitVBlankIntr();
						NNS_G2dLoadImage2DMapping(m_Ncgr.pDataCg(), baseAddr, DS2D_VRAM_TYPE[(int)plane], m_ImageProxy);
					}
				}
			}

			public void ceLoadCg(DS2D_OBJ_PLANE plane, byte[] pCg)
			{
				if (pCg != null)
				{
					m_Ncgr.Load(pCg);
					TexVram baseAddr = DS2DManager.d2dGetInstance().d2dGetObjCgOffset(plane);
					DS2DManager.d2dGetInstance().d2dAddObjCgOffset(plane, m_Ncgr.pDataCg().szByte);
					if (m_Ncgr.pDataCg().mapingType != 0)
					{
						SVC_WaitVBlankIntr();
						NNS_G2dLoadImage1DMapping(m_Ncgr.pDataCg(), baseAddr, DS2D_VRAM_TYPE[(int)plane], m_ImageProxy);
					}
					else
					{
						SVC_WaitVBlankIntr();
						NNS_G2dLoadImage2DMapping(m_Ncgr.pDataCg(), baseAddr, DS2D_VRAM_TYPE[(int)plane], m_ImageProxy);
					}
				}
			}

			public void ceLoadCl(DS2D_OBJ_PLANE plane, string pCl)
			{
			}

			public void ceCopyCg(Cell ce)
			{
				m_Ncgr = ce.ceGetCg();
			}

			public void ceCopyCl(Cell ce)
			{
				m_Nclr = ce.ceGetCl();
			}

			public Ncgr ceGetCg()
			{
				return m_Ncgr;
			}

			public Nclr ceGetCl()
			{
				return m_Nclr;
			}

			public void ceReleaseCgCl()
			{
				m_Ncgr.Release();
				m_Nclr.Release();
			}

			public override void Release()
			{
				m_Ncer.Release();
				m_Nanr.Release();
				m_Ncgr.Release();
				m_Nclr.Release();
			}

			public void copy(Cell src)
			{
				copy((Sprite)src);
				m_Ncgr.copy(src.m_Ncgr);
			}
		}
	}
}
