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
	public static partial class eld
	{
		public class ModelTexture : ds.sys3d.ITexture
		{
			protected Array _pResource;

			protected NNSG3dResTex _pResTexture;

			protected TexVram _keyTexture;

			protected TexVram _keyPalette;

			protected bool _bSend;

			public ModelTexture()
			{
				_pResource = null;
				_pResTexture = null;
				_keyTexture = null;
				_keyPalette = null;
				_bSend = false;
			}

			~ModelTexture()
			{
				destruct();
			}

			public void destruct()
			{
				cleanup();
			}

			public bool initialize(Array pTexture)
			{
				if (!ds.sys3d.CModelTexture.isModelTexture(pTexture))
				{
					return false;
				}
				ArrayReader arrayReader = new ArrayReader(pTexture);
				ds.sys3d.nmdp.SModelFileHeader sModelFileHeader = (ds.sys3d.nmdp.SModelFileHeader)arrayReader;
				ds.sys3d.nmdp.SModelInfoHeader infoHeader = sModelFileHeader.m_InfoHeader;
				arrayReader.dispose();
				if ((infoHeader.unFlag & 1) == 0)
				{
					infoHeader.unFlag |= 1u;
					_pResTexture = NNS_G3dGetTex(sModelFileHeader.m_ResFileHeaderModel);
					uint szByte = NNS_G3dTexGetRequiredSize(_pResTexture);
					uint szByte2 = NNS_G3dPlttGetRequiredSize(_pResTexture);
					_keyTexture = NNS_GfdAllocLnkTexVram(szByte, 0, 0u);
					_keyPalette = NNS_GfdAllocLnkPlttVram(szByte2, 0, 1u);
					if (_keyTexture == null || _keyPalette == null)
					{
						cleanup();
						return false;
					}
					NNS_G3dTexSetTexKey(_pResTexture, _keyTexture, null);
					NNS_G3dPlttSetPlttKey(_pResTexture, _keyPalette);
					NNS_G3dTexLoad(_pResTexture, 1);
					NNS_G3dPlttLoad(_pResTexture, 1);
					_bSend = true;
					_pResource = pTexture;
					return true;
				}
				return true;
			}

			public void cleanup()
			{
				if (_keyTexture != null)
				{
					NNS_GfdFreeLnkTexVram(_keyTexture);
					_keyTexture = null;
				}
				if (_keyPalette != null)
				{
					NNS_GfdFreeLnkPlttVram(_keyPalette);
					_keyPalette = null;
				}
				_bSend = false;
			}

			public void bindMdlSet(NNSG3dResMdlSet set)
			{
				NNS_G3dBindMdlSet(set, _pResTexture);
			}

			public void bindMdl(NNSG3dResMdl mdl)
			{
				bindMdlToTex(mdl);
				bindMdlToPltt(mdl);
			}

			public void bindMdlToTex(NNSG3dResMdl mdl)
			{
				NNS_G3dBindMdlTex(mdl, _pResTexture);
			}

			public void bindMdlToPltt(NNSG3dResMdl mdl)
			{
				NNS_G3dBindMdlPltt(mdl, _pResTexture);
			}

			public void releaseMdlSet(NNSG3dResMdlSet set)
			{
				NNS_G3dReleaseMdlSet(set);
			}

			public void releaseMdl(NNSG3dResMdl mdl)
			{
				releaseMdlToTex(mdl);
				releaseMdlToPltt(mdl);
			}

			public void releaseMdlToTex(NNSG3dResMdl mdl)
			{
				NNS_G3dReleaseMdlTex(mdl);
			}

			public void releaseMdlToPltt(NNSG3dResMdl mdl)
			{
				NNS_G3dReleaseMdlPltt(mdl);
			}

			public void bindMdlToTexByName(NNSG3dResMdl mdl, string szTexelname)
			{
			}

			public void bindMdlToPlttByName(NNSG3dResMdl mdl, string szTexelname)
			{
			}

			public void releaseMdlToTexByName(NNSG3dResMdl mdl, string szTexelname)
			{
			}

			public void releaseMdlToPlttByName(NNSG3dResMdl mdl, string szTexelname)
			{
			}

			public Array getResource()
			{
				return _pResource;
			}

			public NNSG3dResTex getTextureResource()
			{
				return _pResTexture;
			}

			private void notifySendTexture()
			{
				_bSend = true;
			}
		}
	}
}
