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
	public static partial class ds
	{
		public static partial class sys3d
		{
			public class CShadowObject : SceneRenderObject
			{
				private CRenderObject _pRenderObject;

				private NNSG3dResMdl _pResMdl;

				private VecFx32 _scale = new VecFx32();

				private int _height;

				private sbyte _alpha;

				private SHADOW_TYPE _type;

				private bool _bEnable;

				~CShadowObject()
				{
				}

				public void initialize()
				{
					initValue();
				}

				public void cleanup()
				{
					initValue();
				}

				public void setup(NNSG3dResMdl pShadowResMdl, CRenderObject pRenderObj)
				{
					_pRenderObject = pRenderObj;
					_pResMdl = pShadowResMdl;
				}

				public void setAlphaRate(byte rate)
				{
					_alpha = (sbyte)(DS_ALPHA_MAX * rate / 100);
				}

				public byte getAlphaRate()
				{
					return (byte)(_alpha * 100 / DS_ALPHA_MAX);
				}

				public override void draw()
				{
					if (_alpha > 0 && _bEnable && _pResMdl != null && _pRenderObject != null && SHADOW_TYPE.SHADOW_TYPE_ERROR != _type)
					{
						if (_type == SHADOW_TYPE.SHADOW_TYPE_VOLUME)
						{
							drawShadowVolume();
						}
						if (SHADOW_TYPE.SHADOW_TYPE_POLYGON == _type)
						{
							drawShadowPolygon();
						}
					}
				}

				public void drawShadowPolygon()
				{
					VecFx32 nitro_reuse_pos = GlobalScope.nitro_reuse_pos;
					nitro_reuse_pos.copy(_pRenderObject.m_Position);
					nitro_reuse_pos.y += _height;
					NNS_G3dGlbSetBaseScale(_scale);
					NNS_G3dGlbSetBaseTrans(nitro_reuse_pos);
					NNS_G3dGlbFlushP();
					NNS_G3dMdlSetMdlAlpha(_pResMdl, 0u, _alpha);
					NNS_G3dMdlSetMdlPolygonMode(_pResMdl, 0u, GXPolygonMode.GX_POLYGONMODE_MODULATE);
					NNS_G3dMdlSetMdlPolygonIDAll(_pResMdl, 19);
					NNS_G3dDraw1Mat1Shp(_pResMdl, 0u, 0u, 1);
					VecFx32 sys3d_reuse_zero_vec = sys3d.sys3d_reuse_zero_vec;
					VecFx32 sys3d_reuse_one_vec = sys3d.sys3d_reuse_one_vec;
					MtxFx33 sys3d_reuse_unit_mat = sys3d.sys3d_reuse_unit_mat;
					NNS_G3dGlbSetBaseTrans(sys3d_reuse_zero_vec);
					NNS_G3dGlbSetBaseScale(sys3d_reuse_one_vec);
					NNS_G3dGlbSetBaseRot(sys3d_reuse_unit_mat);
					NNS_G3dGlbFlushP();
				}

				public void drawShadowVolume()
				{
					VecFx32 pTrans = new VecFx32(_pRenderObject.m_Position.x, _pRenderObject.m_Position.y + _height, _pRenderObject.m_Position.z);
					NNS_G3dGlbSetBaseScale(_scale);
					NNS_G3dGlbSetBaseTrans(pTrans);
					NNS_G3dGlbFlushP();
					NNS_G3dMdlSetMdlLightEnableFlag(_pResMdl, 0u, 0);
					NNS_G3dMdlSetMdlPolygonID(_pResMdl, 0u, 0);
					NNS_G3dMdlSetMdlCullMode(_pResMdl, 0u, GXCull.GX_CULL_FRONT);
					NNS_G3dMdlSetMdlAlpha(_pResMdl, 0u, _alpha);
					NNS_G3dMdlSetMdlPolygonMode(_pResMdl, 0u, GXPolygonMode.GX_POLYGONMODE_SHADOW);
					NNS_G3dDraw1Mat1Shp(_pResMdl, 0u, 0u, 1);
					NNS_G3dMdlSetMdlLightEnableFlag(_pResMdl, 0u, 0);
					NNS_G3dMdlSetMdlPolygonID(_pResMdl, 0u, 63);
					NNS_G3dMdlSetMdlCullMode(_pResMdl, 0u, GXCull.GX_CULL_NONE);
					NNS_G3dMdlSetMdlAlpha(_pResMdl, 0u, _alpha);
					NNS_G3dMdlSetMdlPolygonMode(_pResMdl, 0u, GXPolygonMode.GX_POLYGONMODE_SHADOW);
					NNS_G3dDraw1Mat1Shp(_pResMdl, 0u, 0u, 1);
					VecFx32 sys3d_reuse_zero_vec = sys3d.sys3d_reuse_zero_vec;
					VecFx32 sys3d_reuse_one_vec = sys3d.sys3d_reuse_one_vec;
					MtxFx33 sys3d_reuse_unit_mat = sys3d.sys3d_reuse_unit_mat;
					NNS_G3dGlbSetBaseTrans(sys3d_reuse_zero_vec);
					NNS_G3dGlbSetBaseScale(sys3d_reuse_one_vec);
					NNS_G3dGlbSetBaseRot(sys3d_reuse_unit_mat);
					NNS_G3dGlbFlushP();
				}

				public void initValue()
				{
					_pRenderObject = null;
					_pResMdl = null;
					_scale.x = 8192;
					_scale.y = 8192;
					_scale.z = 8192;
					_height = 0;
					_type = SHADOW_TYPE.SHADOW_TYPE_POLYGON;
					_alpha = 10;
					_bEnable = true;
				}

				public void setScale(VecFx32 scale)
				{
					_scale.copy(scale);
				}

				public void getScale(VecFx32 scale)
				{
					scale.copy(_scale);
				}

				public void setHeight(int height)
				{
					_height = height;
				}

				public int getHeight()
				{
					return _height;
				}

				public void setAlpha(sbyte alpha)
				{
					_alpha = alpha;
				}

				public sbyte getAlpha()
				{
					return _alpha;
				}

				public void setType(SHADOW_TYPE type)
				{
					_type = type;
				}

				public void setEnable(bool b)
				{
					_bEnable = b;
				}
			}
		}
	}
}
