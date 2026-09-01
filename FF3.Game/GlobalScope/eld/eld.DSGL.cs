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
		public class DSGL : IGL
		{
			private TexVram _addrTexel;

			private uint _addrClut;

			public DSGL()
			{
				_addrTexel = new TexVram();
				_addrClut = 4096u;
			}

			~DSGL()
			{
			}

			public override void drawElementList(ds.sys3d.Scene scene)
			{
				NNS_G3dGlbFlushP();
				NNS_G3dGeFlushBuffer();
				base.drawElementList(scene);
			}

			public override ds.Texture CreateTexture(ds.Texture pAddr)
			{
				return ds.Texture.create(pAddr, _addrTexel, _addrClut);
			}

			public override void SetTexture(ds.Texture pTexture)
			{
			}

			public ds.sys3d.ParticleElement createParticleElement(ds.pt.Particle[] pParticle, uint nbParticles, ds.Texture pTexture)
			{
				ds.sys3d.ParticleElement particleElement = ds.sys3d.ElementGenerator.newElement<ds.sys3d.ParticleElement>(this);
				if (particleElement != null)
				{
					ds.Texture texture = reinterpret_cast<ds.Texture>(pTexture);
					particleElement.setTexture(texture);
					particleElement.setParticle(pParticle, nbParticles);
					addElement(particleElement);
					return particleElement;
				}
				return null;
			}

			public ds.sys3d.LargeParticleElement createLargeParticleElement(ds.pt.LargeParticle[] pParticle, uint nbParticles, ds.Texture pTexture)
			{
				ds.sys3d.LargeParticleElement largeParticleElement = ds.sys3d.ElementGenerator.newElement<ds.sys3d.LargeParticleElement>(this);
				if (largeParticleElement != null)
				{
					ds.Texture texture = reinterpret_cast<ds.Texture>(pTexture);
					largeParticleElement.setTexture(texture);
					largeParticleElement.setParticle(pParticle, nbParticles);
					addElement(largeParticleElement);
					return largeParticleElement;
				}
				return null;
			}

			public void deleteParticleElement(ds.sys3d.ParticleElement pElem)
			{
				if (pElem != null)
				{
					removeElement(pElem);
					ds.sys3d.ElementGenerator.deleteElement(pElem);
				}
			}

			public void deleteLargeParticleElement(ds.sys3d.LargeParticleElement pElem)
			{
				if (pElem != null)
				{
					removeElement(pElem);
					ds.sys3d.ElementGenerator.deleteElement(pElem);
				}
			}

			public bool addSkinModel(ds.sys3d.CRenderObject pRender)
			{
				ServerFF3 serverFF = (ServerFF3)IServer.Instance();
				serverFF.scene()?.addRenderObject(pRender, ds.sys3d.Scene.LAYER_BOTTOM);
				return true;
			}

			public void removeSkinModel(ds.sys3d.CRenderObject pRender)
			{
				ServerFF3 serverFF = (ServerFF3)IServer.Instance();
				serverFF.scene()?.removeRenderObject(pRender);
			}

			public void setTextureAddress(TexVram texel, uint clut)
			{
				_addrTexel = texel;
				_addrClut = clut;
			}
		}
	}
}
