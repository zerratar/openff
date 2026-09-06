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
	public static partial class ds
	{
		public static partial class sys3d
		{
			public class LargeParticleElement : SceneElement
			{
				private pt.LargeParticle[] _ptcl;

				private uint _nbPtcls;

				private BasicTextureObject _objTex = new BasicTextureObject();

				private Vector3<int> _vPosCenter = new Vector3<int>();

				private Vector3<int> _vScale = new Vector3<int>();

				private bool _bShow;

				public LargeParticleElement(SceneObject objScn)
					: base(objScn)
				{
					_ptcl = null;
					_bShow = true;
					_vPosCenter.zero();
					_vScale.set(1);
				}

				public LargeParticleElement()
					: this(null)
				{
				}

				public override void draw(Scene scene)
				{
					if (_ptcl != null)
					{
						G3_MtxMode(GXMtxMode.GX_MTXMODE_POSITION);
						G3_PushMtx();
						pt.PrimitiveDisplay sys3d_reuse_disp = sys3d.sys3d_reuse_disp;
						sys3d_reuse_disp._scene = scene;
						sys3d_reuse_disp.drawLargeParticles(this);
						G3_PopMtx(1);
					}
				}

				~LargeParticleElement()
				{
					destruct();
				}

				public void destruct()
				{
				}

				public override bool isShow()
				{
					return _bShow;
				}

				public override TextureObject getTextureObject()
				{
					return _objTex;
				}

				public void setParticle(pt.LargeParticle[] ptcl, uint nbPtcls)
				{
					_ptcl = ptcl;
					_nbPtcls = nbPtcls;
				}

				public pt.LargeParticle[] getParticle()
				{
					return _ptcl;
				}

				public uint getNbParticles()
				{
					return _nbPtcls;
				}

				public void setTexture(Texture pTexture)
				{
					_objTex.setTexture(pTexture);
				}

				public void setCenterPosition(Vector3<int> pos)
				{
					_vPosCenter.copy(pos);
				}

				public void getCenterPosition(Vector3<int> pos)
				{
					pos.copy(_vPosCenter);
				}

				public Vector3<int> getCenterPosition()
				{
					return _vPosCenter;
				}

				public void setScale(Vector3<int> scale)
				{
					_vScale.copy(scale);
				}

				public void getScale(Vector3<int> pos)
				{
					pos.copy(_vPosCenter);
				}

				public Vector3<int> getScale()
				{
					return _vScale;
				}

				public void show(bool bShow)
				{
					_bShow = bShow;
				}
			}
		}
	}
}
