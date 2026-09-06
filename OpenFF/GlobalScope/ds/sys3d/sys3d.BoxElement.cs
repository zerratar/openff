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
			public class BoxElement : SceneElement
			{
				private pt.Box[] _box;

				private uint _nbBox;

				private Vector3<int> _vPosCenter;

				private bool _bShow;

				public BoxElement(SceneObject objScn)
					: base(objScn)
				{
					_box = null;
					_nbBox = 0u;
					_bShow = true;
					_vPosCenter.zero();
				}

				public override void draw(Scene scene)
				{
					if (_box != null)
					{
						G3_MtxMode(GXMtxMode.GX_MTXMODE_POSITION);
						G3_PushMtx();
						pt.PrimitiveDisplay sys3d_reuse_disp = sys3d.sys3d_reuse_disp;
						sys3d_reuse_disp._scene = scene;
						sys3d_reuse_disp.drawBox(this);
						G3_PopMtx(1);
					}
				}

				~BoxElement()
				{
				}

				public override bool isShow()
				{
					return _bShow;
				}

				public void show(bool bShow)
				{
					_bShow = bShow;
				}

				public void setBox(pt.Box[] box, uint nbBox)
				{
					_box = box;
					_nbBox = nbBox;
				}

				public pt.Box[] getBox()
				{
					return _box;
				}

				public uint getNbBox()
				{
					return _nbBox;
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
			}
		}
	}
}
