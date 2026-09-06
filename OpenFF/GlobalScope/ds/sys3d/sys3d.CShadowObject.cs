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
			public class CShadowObject : SceneRenderObject
			{
				private CRenderObject _pRenderObject;

				private NNSG3dResMdl _pResMdl;

				private VecFx32 _scale = new VecFx32();

				private int _height;

				private sbyte _alpha;

				private SHADOW_TYPE _type;

				private bool _bEnable;

				// PORT (FF4): the joint the shadow stands under. FF4's CShadowObject::drawShadowPolygon takes
				// the shadow's x and z from this joint's stored matrix when the render object names one
				// (CCharacterMng::setShadowJntName, from ce_ShadowSetting's "kosi"), and keeps the base
				// height for y. Scene casts are moved by their motion's root, not by setPosition, so the
				// render object's own position stays at the origin - and so would the shadow.
				private string _jointName;

				~CShadowObject()
				{
				}

				public void setJointName(string name)
				{
					_jointName = string.IsNullOrEmpty(name) ? null : name;
				}

				/// <summary>
				/// PORT (FF4): the ground under a point, in engine units, or null off the collision - FF4's shadow
				/// keeps a ground interface and, when it has one, drops a joint-following shadow onto the ground
				/// under the joint (drawShadowPolygon: ground + height + 0x40). Set by the FF4 scene code; only
				/// shadows that follow a joint consult it.
				/// </summary>
				public static Func<VecFx32, VecFx32> GroundQuery;

				/// <summary>Where the shadow is drawn: the render object's position; with a joint, its x and z and the ground under it.</summary>
				private void basePosition(VecFx32 pos)
				{
					pos.copy(_pRenderObject.m_Position);
					if (_jointName != null)
					{
						MtxFx43 joint = sys3d_reuse_joint_mtx;
						if (_pRenderObject.getJntMtx(_jointName, joint))
						{
							pos.x = joint._30;
							pos.z = joint._32;
							VecFx32 ground = null;
							if (GroundQuery != null)
							{
								try { ground = GroundQuery(new VecFx32(joint._30, joint._31, joint._32)); }
								catch (Exception) { ground = null; }
							}
							if (ground != null)
							{
								pos.y = ground.y;
							}
							if (!_jointSeen)
							{
								_jointSeen = true;
								OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.File, "shadow: follows " + _jointName + " at " + (joint._30 / 4096f).ToString("0.0") + ", " + (joint._31 / 4096f).ToString("0.0") + ", " + (joint._32 / 4096f).ToString("0.0") + (ground != null ? ", ground " + (ground.y / 4096f).ToString("0.0") : ", no ground under it") + " (base " + (_pRenderObject.m_Position.y / 4096f).ToString("0.0") + ")");
							}
						}
						else if (++_jointMissed == 120)
						{
							OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.File, "shadow: " + _jointName + " not captured after 120 draws - the shadow stays at the model's position");
						}
					}
					pos.y += _height;
				}

				private static readonly MtxFx43 sys3d_reuse_joint_mtx = new MtxFx43();
				private bool _jointSeen;
				private int _jointMissed;

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
					basePosition(nitro_reuse_pos);
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
					VecFx32 pTrans = new VecFx32();
					basePosition(pTrans);
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
					_jointName = null;
					_jointSeen = false;
					_jointMissed = 0;
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
