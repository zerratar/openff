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
using OpenFF.Platform;
using OpenFF.Resources;

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

			// PORT: a texture another pack already holds by the same name takes that one's VRAM (applyShare) and is not
			// held itself. On the DS freeing the holder's VRAM leaves the texels where they are until something else is
			// loaded over them, so a pack sharing them still draws - FF4's opening loads each flashback effect (e593,
			// e595) while the one before it, with the same sparkle, is still in, and cleans that one up as the new one
			// starts. Here freeing deletes the texture, so the holder's VRAM goes to a texture still sharing it instead.
			private readonly System.Collections.Generic.Dictionary<ds.Texture, System.Collections.Generic.List<ds.Texture>> _sharers = new System.Collections.Generic.Dictionary<ds.Texture, System.Collections.Generic.List<ds.Texture>>();

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
				_sharers.Clear();
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
						if (_sharers.TryGetValue(pTexture, out System.Collections.Generic.List<ds.Texture> sharing) && sharing.Count != 0)
						{
							ds.Texture heir = sharing[0];
							sharing.RemoveAt(0);
							_sharers.Remove(pTexture);
							if (sharing.Count != 0) _sharers[heir] = sharing;
							_listTx.push_back(heir);
							pTexture.setAddress(null, 0u);
							return;
						}
						_sharers.Remove(pTexture);
						NNS_GfdFreeLnkTexVram(pTexture.getKeyTexel());
						NNS_GfdFreeLnkPlttVram(pTexture.getKeyPalette());
						pTexture.setAddress(null, 0u);
						pTexture.getSize(out var sizeTexel, out var sizePltt);
						_nTotalSizeTexel += (int)sizeTexel;
						_nTotalSizePalette += (int)sizePltt;
						return;
					}
				}
				foreach (System.Collections.Generic.List<ds.Texture> sharing in _sharers.Values)
				{
					sharing.Remove(pTexture);
				}
			}

			public bool isRegistered(ds.Texture pTexture)
			{
				uint num = (uint)_listTx.size();
				for (int i = 0; i < num; i++)
				{
					if (pTexture.applyShare(_listTx[i]))
					{
						if (!_sharers.TryGetValue(_listTx[i], out System.Collections.Generic.List<ds.Texture> sharing)) _sharers[_listTx[i]] = sharing = new System.Collections.Generic.List<ds.Texture>();
						if (!sharing.Contains(pTexture)) sharing.Add(pTexture);
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
