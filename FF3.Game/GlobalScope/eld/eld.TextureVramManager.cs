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
	public static partial class eld
	{
		public class TextureVramManager
		{
			private ds.Vector<ds.Texture, ds.FastErasePolicy<ds.Texture>> _listTx = new ds.Vector<ds.Texture, ds.FastErasePolicy<ds.Texture>>(32);

			private ds.Vector<ModelTexture, ds.FastErasePolicy<ModelTexture>> _listMdlTx = new ds.Vector<ModelTexture, ds.FastErasePolicy<ModelTexture>>(32);

			private uint _unMaxSizeTexel;

			private uint _unMaxSizePalette;

			private int _nTotalSizeTexel;

			private int _nTotalSizePalette;

			public TextureVramManager()
			{
				_unMaxSizeTexel = 0u;
				_unMaxSizePalette = 0u;
				resetTotal();
			}

			~TextureVramManager()
			{
				cleanup();
			}

			public void initialize(uint unMaxSizeTexel, uint unMaxSizePalette)
			{
				_unMaxSizeTexel = unMaxSizeTexel;
				_unMaxSizePalette = unMaxSizePalette;
				resetTotal();
			}

			public void cleanup()
			{
				while (_listTx.size() != 0)
				{
					ds.Texture texture = _listTx.at(0);
					NNS_GfdFreeLnkTexVram(texture.getKeyTexel());
					NNS_GfdFreeLnkPlttVram(texture.getKeyPalette());
					texture.setAddress(null, 0u);
					_listTx.erase(0);
				}
				resetTotal();
			}

			public bool registerTexture(ds.Texture pTexture)
			{
				TexVram texVram = null;
				TexVram texVram2 = null;
				if (!ds.Texture.isTexture(pTexture))
				{
					return false;
				}
				if (isRegistered(pTexture))
				{
					return false;
				}
				ds.Texture.getSize(pTexture, out var sizeTexel, out var sizePltt);
				texVram = NNS_GfdAllocLnkTexVram(sizeTexel, 0, 0u);
				texVram2 = NNS_GfdAllocLnkPlttVram(sizePltt, 0, 0u);
				ds.Texture texture = ds.Texture.createStation(pTexture, texVram, texVram2);
				if (texture != null)
				{
					_listTx.push_back(texture);
					texture.getSize(out sizeTexel, out sizePltt);
					_nTotalSizeTexel += (int)sizeTexel;
					_nTotalSizePalette += (int)sizePltt;
					return true;
				}
				if (texVram != null)
				{
					NNS_GfdFreeLnkTexVram(texVram);
				}
				if (texVram2 != null)
				{
					NNS_GfdFreeLnkPlttVram(texVram2);
				}
				return false;
			}

			public void deregisterTexture(ds.Texture pTexture)
			{
				uint num = (uint)_listTx.size();
				for (int i = 0; i < num; i++)
				{
					if (_listTx[i] == pTexture)
					{
						_listTx.erase(i);
						NNS_GfdFreeLnkTexVram(pTexture.getKeyTexel());
						NNS_GfdFreeLnkPlttVram(pTexture.getKeyPalette());
						pTexture.setAddress(null, 0u);
						pTexture.getSize(out var sizeTexel, out var sizePltt);
						_nTotalSizeTexel += (int)sizeTexel;
						_nTotalSizePalette += (int)sizePltt;
						break;
					}
				}
			}

			public bool isRegistered(ds.Texture pTexture)
			{
				uint num = (uint)_listTx.size();
				for (int i = 0; i < num; i++)
				{
					if (pTexture.applyShare(_listTx[i]))
					{
						return true;
					}
				}
				return false;
			}

			public ModelTexture registerModelTexture(Array pTexture)
			{
				if (isRegisteredModelTexture(pTexture))
				{
					return null;
				}
				ModelTexture modelTexture = (ModelTexture)ds.CHeap.alloc_app(typeof(ModelTexture));
				if (modelTexture == null)
				{
					return null;
				}
				if (!modelTexture.initialize(pTexture))
				{
					ds.CHeap.free_app(modelTexture);
					return null;
				}
				_listMdlTx.push_back(modelTexture);
				return modelTexture;
			}

			public void deregisterModelTexture(ModelTexture pTexture)
			{
				uint num = (uint)_listMdlTx.size();
				for (int i = 0; i < num; i++)
				{
					if (_listMdlTx[i] == pTexture)
					{
						_listMdlTx.erase(i);
						pTexture.cleanup();
						pTexture.destruct();
						ds.CHeap.free_app(pTexture);
						break;
					}
				}
			}

			public bool isRegisteredModelTexture(Array pTexture)
			{
				uint num = (uint)_listMdlTx.size();
				for (int i = 0; i < num; i++)
				{
					if (_listMdlTx[i].getResource() == pTexture)
					{
						return true;
					}
				}
				return false;
			}

			private void resetTotal()
			{
				_nTotalSizeTexel = (_nTotalSizePalette = 0);
			}
		}
	}
}
