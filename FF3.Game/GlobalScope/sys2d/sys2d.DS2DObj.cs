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
		public class DS2DObj
		{
			private TexVram CgOffset;

			private uint ClOffset;

			private NNSG2dRendererInstance Renderer = new NNSG2dRendererInstance();

			private NNSG2dRenderSurface Surface = new NNSG2dRenderSurface();

			private int OamManager;

			public void InitializeRenderer(DS2D_OBJ_PLANE plane)
			{
				NNS_G2dInitRenderer(Renderer);
				Renderer.overwriteEnableFlag = 1u;
				NNS_G2dInitRenderSurface(Surface);
				Surface.viewRect.posTopLeft.x = ds.S32toFX32(0);
				Surface.viewRect.posTopLeft.y = ds.S32toFX32(0);
				Surface.viewRect.sizeView.x = ds.S32toFX32(480);
				Surface.viewRect.sizeView.y = ds.S32toFX32(320);
				NNSG2dOamRegisterFunction[] array = new NNSG2dOamRegisterFunction[3]
				{
					DS2DManager.CallBackAddOamMainObj,
					DS2DManager.CallBackAddOamMainObj,
					DS2DManager.CallBackAddOamSubObj
				};
				NNSG2dAffineRegisterFunction[] array2 = new NNSG2dAffineRegisterFunction[3]
				{
					DS2DManager.CallBackAddAffineMainObj,
					DS2DManager.CallBackAddAffineMainObj,
					DS2DManager.CallBackAddAffineSubObj
				};
				Surface.pFuncOamRegister = array[(int)plane];
				Surface.pFuncOamAffineRegister = array2[(int)plane];
				Surface.type = DS2D_SURFACE_TYPE[(int)plane];
				NNS_G2dAddRendererTargetSurface(Renderer, Surface);
			}

			public void AddCgOffset(uint ofs)
			{
			}

			public TexVram GetCgOffset()
			{
				return CgOffset;
			}

			public void AddClOffset(uint ofs)
			{
				ClOffset += ds.alignment(ofs, 16u);
			}

			public uint GetClOffset()
			{
				return ClOffset;
			}

			public void ClearCgClOffset()
			{
				CgOffset = null;
				ClOffset = 0u;
			}

			public int GetOamManager()
			{
				return OamManager;
			}

			public NNSG2dRendererInstance GetRenderer()
			{
				return Renderer;
			}

			public NNSG2dRenderSurface GetSurface()
			{
				return Surface;
			}
		}
	}
}
