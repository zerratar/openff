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
	public static partial class ds
	{
		public static partial class sys3d
		{
			public class PolygonElement : SceneElement
			{
				private pt.Polygon[] _polg;

				private uint _nbPolgns;

				private BasicTextureObject _objTex;

				private Vector3<int> _vPosCenter;

				private Vector3<int> _vScale;

				private bool _bShow;

				public PolygonElement(SceneObject objScn)
					: base(objScn)
				{
					_polg = null;
					_bShow = true;
					_vPosCenter.zero();
					_vScale.set(1);
				}

				public override void draw(Scene scene)
				{
					if (_polg != null)
					{
						G3_MtxMode(GXMtxMode.GX_MTXMODE_POSITION);
						G3_PushMtx();
						pt.PrimitiveDisplay sys3d_reuse_disp = sys3d.sys3d_reuse_disp;
						sys3d_reuse_disp._scene = scene;
						sys3d_reuse_disp.drawPolygons(this);
						G3_PopMtx(1);
					}
				}

				~PolygonElement()
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

				public void setPolygon(pt.Polygon[] polg, uint nbPolgns)
				{
					_polg = polg;
					_nbPolgns = nbPolgns;
				}

				public pt.Polygon[] getPolygon()
				{
					return _polg;
				}

				public uint getNbPolygon()
				{
					return _nbPolgns;
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
