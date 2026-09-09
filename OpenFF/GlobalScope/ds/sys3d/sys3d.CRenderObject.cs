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
			public class CRenderObject : SceneRenderObject
			{
				public class JntMtx
				{
					public MtxFx43 mtx = new MtxFx43();

					public NNSG3dResName nodeName = new NNSG3dResName();

					public uint flag;
				}

				public const byte JNT_MTX_NUM = 4;

				public const byte NODE_NAME_MAX = 17;

				private const uint FLAG_DROP_SHADOW = 2u;

				private const uint FLAG_CLIPPING = 4u;

				private const uint FLAG_HIDDEN = 8u;

				private const uint FLAG_SHADOW = 16u;

				private const uint FLAG_VIEWVOLUME_CLIP = 32u;

				private const uint FLAG_USE = 1u;

				public const uint FLAG_SUCCEEDED = 2u;

				private const uint SHADOW_TYPE_A = 0u;

				private const uint SHADOW_TYPE_B = 1u;

				public CLightObject m_LightObject = new CLightObject();

				private uint m_Flag;

				private NNSG3dRenderObj m_Object = new NNSG3dRenderObj();

				public VecFx32 m_Position = new VecFx32();

				private VecFx32 m_Scale = new VecFx32();

				private ushort m_RotX;

				private ushort m_RotY;

				private ushort m_RotZ;

				private MtxFx33 m_RotMtx = new MtxFx33();

				private MtxFx43 m_PoseMtx = new MtxFx43();

				private int m_Alpha;

				private int m_AlphaRate;

				private byte[] m_pOrgAlpha;

				private NNSG3dResMdl m_MdlRes;

				private NNSG3dAnmObj m_MotObject;

				private NNSG3dResMdl m_Shadow;

				private VecFx32 m_ShadowScale = new VecFx32();

				private sbyte m_ShadowAlpha;

				private int m_ShadowHeight;

				private GXPolygonMode m_PolyMode;

				private NNSG3dJntAnmResult[] m_JntAnmResult;

				private ILodImplement m_pLodObj;

				private ushort m_unDistanceLevel;

				private uint m_LodLevel;

				private uint m_LodCnt;

				public JntMtx[] m_JntMtx = new JntMtx[4];

				private uint m_ShadowType;

				private BoundingBox m_BB;

				private int m_Priority;

				/// <summary>
				/// PORT: something drawn in this model's place - a mod's glTF in a weapon's hand
				/// (OpenFF.Client.WeaponMeshes). The object goes on as the game's: it is posed, hidden,
				/// faded and shadowed as before; only the draw is the stand-in's, given this object
				/// for its pose matrix and alpha. Cleared when the model is (cleanup).
				/// </summary>
				public Action<CRenderObject> StandIn;

				public CRenderObject()
				{
					m_pLodObj = null;
					m_unDistanceLevel = 0;
					for (int i = 0; i < m_JntMtx.Length; i++)
					{
						m_JntMtx[i] = new JntMtx();
					}
					m_MotObject = null;
					m_Shadow = null;
					VEC_Set(m_Scale, 4096, 4096, 4096);
					MTX_Identity33(m_RotMtx);
				}

				~CRenderObject()
				{
				}

				public void initialize()
				{
					m_Flag = 0u;
					m_Alpha = -1;
					m_MdlRes = null;
				}

				public void setup(NNSG3dResMdl pMdlRes)
				{
					m_Flag = 0u;
					m_Alpha = -1;
					m_AlphaRate = 100;
					m_MdlRes = pMdlRes;
					m_BB = null;
					MTX_Identity43(m_PoseMtx);
					m_Flag |= 16u;
					m_Flag |= 32u;
					VEC_Set(m_Position, 0, 0, 0);
					VEC_Set(m_Scale, 4096, 4096, 4096);
					m_RotX = 0;
					m_RotY = 0;
					m_RotZ = 0;
					MTX_Identity33(m_RotMtx);
					NNS_G3dRenderObjInit(m_Object, pMdlRes);
					m_ShadowScale.x = 8192;
					m_ShadowScale.y = 8192;
					m_ShadowScale.z = 8192;
					m_ShadowHeight = 0;
					m_ShadowType = 0u;
					m_ShadowAlpha = 10;
					m_Priority = 0;
					m_PolyMode = GXPolygonMode.GX_POLYGONMODE_MODULATE;
					m_JntAnmResult = NNS_G3dAllocRecBufferJnt(CHeap.getAppAllocator(), m_MdlRes);
					m_LodLevel = 1u;
					m_LodCnt = 0u;
					m_pOrgAlpha = (byte[])CHeap.alloc_app(m_MdlRes.info.numMat);
					for (uint num = 0u; num < m_MdlRes.info.numMat; num++)
					{
						m_pOrgAlpha[num] = (byte)NNS_G3dMdlGetMdlAlpha(m_MdlRes, num);
					}
					initJntMtx();
				}

				public void cleanup()
				{
					StandIn = null;
					m_Shadow = null;
					m_MdlRes = null;
					if (m_pOrgAlpha != null)
					{
						CHeap.free_app(m_pOrgAlpha);
						m_pOrgAlpha = null;
					}
					if (m_JntAnmResult != null)
					{
						NNS_G3dFreeRecBufferJnt(CHeap.getAppAllocator(), m_JntAnmResult);
						m_JntAnmResult = null;
					}
				}

				public override void draw()
				{
					if (m_MdlRes == null || isHidden())
					{
						return;
					}
					if (StandIn != null)
					{
						StandIn(this);
						return;
					}
					if (m_LodLevel == 1)
					{
						NNS_G3dRenderObjReleaseJntAnmBuffer(m_Object);
						m_LodCnt = 0u;
					}
					else if (m_LodLevel != 0)
					{
						NNS_G3dRenderObjSetJntAnmBuffer(m_Object, m_JntAnmResult);
						if (m_LodCnt != 0)
						{
							NNS_G3dRenderObjResetFlag(m_Object, NNSG3dRenderObjFlag.NNS_G3D_RENDEROBJ_FLAG_RECORD);
						}
						else
						{
							NNS_G3dRenderObjSetFlag(m_Object, NNSG3dRenderObjFlag.NNS_G3D_RENDEROBJ_FLAG_RECORD);
						}
						m_LodCnt = (m_LodCnt + 1) % m_LodLevel;
					}
					m_LightObject.calculate();
					if (m_Alpha != -1)
					{
						_ = m_Flag & 2;
					}
					NNS_G3dGlbPolygonAttr(0, m_PolyMode, GXCull.GX_CULL_BACK, 0, 31, 0);
					NNS_G3dGlbFlushP();
					NNS_G3dGeFlushBuffer();
					G3_MtxMode(GXMtxMode.GX_MTXMODE_POSITION_VECTOR);
					G3_MultMtx43(m_PoseMtx);
					if (m_BB != null && (0x20 & m_Flag) != 0)
					{
						testBB();
						if (isClipping())
						{
							return;
						}
					}
					if (m_pLodObj != null)
					{
						m_pLodObj.executeLod(this);
					}
					currentMtx2.copy(currentMtx);
					NNS_G3dDraw(m_Object);
					VecFx32 sys3d_reuse_zero_vec = sys3d.sys3d_reuse_zero_vec;
					VecFx32 sys3d_reuse_one_vec = sys3d.sys3d_reuse_one_vec;
					MtxFx33 sys3d_reuse_unit_mat = sys3d.sys3d_reuse_unit_mat;
					NNS_G3dGlbSetBaseTrans(sys3d_reuse_zero_vec);
					NNS_G3dGlbSetBaseScale(sys3d_reuse_one_vec);
					NNS_G3dGlbSetBaseRot(sys3d_reuse_unit_mat);
					NNS_G3dGlbFlushP();
				}

				public void drawBB()
				{
					if (m_BB != null)
					{
						G3_PushMtx();
						G3_Scale(m_BB.scale, m_BB.scale, m_BB.scale);
						pt.Box box = new pt.Box();
						box.Center.set(m_BB.pos.x + FX_Mul(m_BB.size.x, 2048), m_BB.pos.y + FX_Mul(m_BB.size.y, 2048), m_BB.pos.z + FX_Mul(m_BB.size.z, 2048));
						box.Size.set(FX_Mul(m_BB.size.x, 2048), FX_Mul(m_BB.size.y, 2048), FX_Mul(m_BB.size.z, 2048));
						box.Disp = 3;
						box.setPolygonIDArray(56);
						box.Color.set(5, 31, 5, 16);
						box.drawDirect();
						G3_PopMtx(1);
					}
				}

				public void testBB()
				{
					if (m_BB != null)
					{
						G3_MtxMode(GXMtxMode.GX_MTXMODE_TEXTURE);
						G3_Identity();
						G3_MtxMode(GXMtxMode.GX_MTXMODE_POSITION_VECTOR);
						G3_PushMtx();
						G3_Scale(m_BB.scale, m_BB.scale, m_BB.scale);
						G3_PolygonAttr(1, GXPolygonMode.GX_POLYGONMODE_MODULATE, GXCull.GX_CULL_NONE, 0, 0, 12288);
						G3_Begin(GXBegin.GX_BEGIN_TRIANGLES);
						G3_End();
						GXBoxTestParam gXBoxTestParam = new GXBoxTestParam();
						gXBoxTestParam.x = m_BB.pos.x;
						gXBoxTestParam.y = m_BB.pos.y;
						gXBoxTestParam.z = m_BB.pos.z;
						gXBoxTestParam.width = m_BB.size.x;
						gXBoxTestParam.height = m_BB.size.y;
						gXBoxTestParam.depth = m_BB.size.z;
						G3_BoxTest(gXBoxTestParam);
						int @in;
						while (G3X_GetBoxTestResult(out @in) != 0)
						{
						}
						G3_PopMtx(1);
						if (@in == 0)
						{
							m_Flag |= 4u;
						}
						else
						{
							m_Flag &= 4294967291u;
						}
					}
				}

				public void setPosition(VecFx32 pos)
				{
					m_Position.copy(pos);
					compPoseMtx();
				}

				public void setRotation(ushort x, ushort y, ushort z)
				{
					MtxFx33 mtxFx = new MtxFx33();
					MtxFx33 mtxFx2 = new MtxFx33();
					MtxFx33 mtxFx3 = new MtxFx33();
					m_RotX = x;
					m_RotY = y;
					m_RotZ = z;
					short sinVal = FX_SinIdx(m_RotX);
					short cosVal = FX_CosIdx(m_RotX);
					MTX_RotX33(mtxFx, sinVal, cosVal);
					sinVal = FX_SinIdx(m_RotY);
					cosVal = FX_CosIdx(m_RotY);
					MTX_RotY33(mtxFx2, sinVal, cosVal);
					sinVal = FX_SinIdx(m_RotZ);
					cosVal = FX_CosIdx(m_RotZ);
					MTX_RotZ33(mtxFx3, sinVal, cosVal);
					MTX_Concat33(mtxFx3, mtxFx, m_RotMtx);
					MTX_Concat33(m_RotMtx, mtxFx2, m_RotMtx);
					compPoseMtx();
				}

				public void getRotation(out ushort x, out ushort y, out ushort z)
				{
					x = m_RotX;
					y = m_RotY;
					z = m_RotZ;
				}

				public void setScale(VecFx32 scl)
				{
					m_Scale.copy(scl);
					compPoseMtx();
				}

				public void setPoseMtx(MtxFx43 src)
				{
					m_PoseMtx.copy(src);
					VecFx32 scale = m_Scale;
					VecFx32 position = m_Position;
					VecFx32 sys3d_reuse_rot = sys3d.sys3d_reuse_rot;
					CpuMatrix.getScale(scale, m_PoseMtx);
					CpuMatrix.getTranslate(position, m_PoseMtx);
					CpuMatrix.getRotate(sys3d_reuse_rot, m_PoseMtx);
					m_RotX = (ushort)(sys3d_reuse_rot.x >> 12);
					m_RotY = (ushort)(sys3d_reuse_rot.y >> 12);
					m_RotZ = (ushort)(sys3d_reuse_rot.z >> 12);
				}

				public void setAlpha(int alpha)
				{
					m_Alpha = alpha;
					if (-1 == m_Alpha)
					{
						m_AlphaRate = 100;
						for (uint num = 0u; num < m_MdlRes.info.numMat; num++)
						{
							NNS_G3dMdlSetMdlAlpha(m_MdlRes, num, m_pOrgAlpha[num]);
						}
					}
					else
					{
						m_AlphaRate = 100 * m_Alpha / DS_ALPHA_MAX;
						NNS_G3dMdlSetMdlAlphaAll(m_MdlRes, m_Alpha);
					}
				}

				public void setAlphaRate(int alphaRate)
				{
					m_AlphaRate = alphaRate;
					m_Alpha = DS_ALPHA_MAX * m_AlphaRate / 100;
					if (100 == m_AlphaRate)
					{
						m_Alpha = -1;
					}
					for (uint num = 0u; num < m_MdlRes.info.numMat; num++)
					{
						int num2 = m_pOrgAlpha[num];
						num2 = num2 * m_AlphaRate / 100;
						if (num2 < 0)
						{
							num2 = 0;
						}
						if (num2 > DS_ALPHA_MAX)
						{
							num2 = DS_ALPHA_MAX;
						}
						NNS_G3dMdlSetMdlAlpha(m_MdlRes, num, num2);
					}
				}

				public void setMaterialAlpha(uint idx, uint alpha)
				{
					NNS_G3dMdlSetMdlAlpha(m_MdlRes, idx, (int)alpha);
				}

				public void setMaterialAlpha(string pMatname, uint alpha)
				{
					int materialIdByName = getMaterialIdByName(pMatname);
					if (materialIdByName >= 0)
					{
						setMaterialAlpha((uint)materialIdByName, alpha);
					}
				}

				public uint getMaterialAlpha(uint idx)
				{
					return (uint)NNS_G3dMdlGetMdlAlpha(m_MdlRes, idx);
				}

				public uint getMaterialAlpha(string pMatname)
				{
					int materialIdByName = getMaterialIdByName(pMatname);
					if (materialIdByName < 0)
					{
						return 0u;
					}
					return getMaterialAlpha((uint)materialIdByName);
				}

				public int getMaterialIdByName(string pMatname)
				{
					NNSG3dResMat mat = NNS_G3dGetMat(m_MdlRes);
					string arg = "";
					strcpy(out arg, pMatname);
					return NNS_G3dGetMatIdxByName(mat, (NNSG3dResName)arg);
				}

				public void setLightObject(CLightObject light)
				{
					m_LightObject = light;
				}

				public void setLightOne(uint index, Light light)
				{
					m_LightObject.setLight(index, light);
				}

				public void setShadow(NNSG3dResMdl pShadowMdlRes, uint type)
				{
					m_Shadow = pShadowMdlRes;
					m_ShadowType = type;
				}

				public void setShadowEnable(bool b)
				{
					if (b)
					{
						m_Flag |= 16u;
					}
					else
					{
						m_Flag &= 4294967279u;
					}
				}

				public void setDropShadow(bool flag, NNSG3dResMdl pMdlRes)
				{
					if (flag)
					{
						m_Flag |= 2u;
						m_MdlRes = pMdlRes;
					}
					else
					{
						m_Flag &= 4294967293u;
						m_MdlRes = null;
					}
				}

				public void setShadowAlphaRate(int rate)
				{
					m_ShadowAlpha = (sbyte)(DS_ALPHA_MAX * rate / 100);
				}

				public int getShadowAlphaRate()
				{
					return m_ShadowAlpha * 100 / DS_ALPHA_MAX;
				}

				public bool isClipping()
				{
					if ((m_Flag & 4) != 0)
					{
						return true;
					}
					return false;
				}

				public bool isHidden()
				{
					if ((m_Flag & 8) != 0)
					{
						return true;
					}
					return false;
				}

				public void setHidden(bool b)
				{
					if (b)
					{
						m_Flag |= 8u;
					}
					else
					{
						m_Flag &= 4294967287u;
					}
				}

				public bool isEnableViewVolumeClip()
				{
					return (0x20 & m_Flag) != 0;
				}

				public void setEnableViewVolumeClip(bool b)
				{
					if (b)
					{
						m_Flag |= 32u;
					}
					else
					{
						m_Flag &= 4294967263u;
					}
				}

				public void initJntMtx()
				{
					for (byte b = 0; b < 4; b++)
					{
						m_JntMtx[b].nodeName.name = "";
						m_JntMtx[b].nodeName.nameBytes = StringUtil.getSBytes(m_JntMtx[b].nodeName.name);
						m_JntMtx[b].flag = 0u;
					}
					NNS_G3dRenderObjResetCallBack(m_Object);
				}

				public bool reserveToGetJntMtx(string pNodeName)
				{
					for (byte b = 0; b < 4; b++)
					{
						if ((1 & m_JntMtx[b].flag) == 0)
						{
							MTX_Identity43(m_JntMtx[b].mtx);
							strcpy(out m_JntMtx[b].nodeName.name, pNodeName);
							m_JntMtx[b].nodeName.nameBytes = StringUtil.getSBytes(m_JntMtx[b].nodeName.name);
							m_JntMtx[b].flag |= 1u;
							NNS_G3dRenderObjSetCallBack(m_Object, storeJntMtx, null, 6, NNSG3dSbcCallBackTiming.NNS_G3D_SBC_CALLBACK_TIMING_C);
							m_Object.ptrUser = this;
							return true;
						}
					}
					return false;
				}

				public bool getJntMtx(string pNodeName, MtxFx43 @out)
				{
					for (byte b = 0; b < 4; b++)
					{
						if ((1 & m_JntMtx[b].flag) != 0 && (2 & m_JntMtx[b].flag) != 0 && strcmp(m_JntMtx[b].nodeName.name, pNodeName) == 0)
						{
							@out.copy(m_JntMtx[b].mtx);
							return true;
						}
					}
					return false;
				}

				public void setPolygonMode(GXPolygonMode mode)
				{
					m_PolyMode = mode;
					NNS_G3dMdlUseGlbPolygonMode(m_MdlRes);
				}

				public void registerLodObject(ILodImplement pObj)
				{
					m_pLodObj = pObj;
				}

				public void deregisterLodObject()
				{
					m_pLodObj = null;
				}

				public void compPoseMtx()
				{
					MTX_Copy33To43(m_RotMtx, m_PoseMtx);
					MTX_ScaleApply43(m_PoseMtx, m_PoseMtx, m_Scale.x, m_Scale.y, m_Scale.z);
					m_PoseMtx._30 = m_Position.x;
					m_PoseMtx._31 = m_Position.y;
					m_PoseMtx._32 = m_Position.z;
				}

				public void getPosition(VecFx32 pos)
				{
					pos.copy(m_Position);
				}

				public void getPoseMtx(MtxFx43 @out)
				{
					@out.copy(m_PoseMtx);
				}

				public void getScale(VecFx32 scl)
				{
					scl.copy(m_Scale);
				}

				public uint getAlpha()
				{
					return (uint)m_Alpha;
				}

				public int getAlphaRate()
				{
					return m_AlphaRate;
				}

				public override int getPriority()
				{
					return m_Priority;
				}

				public void setPriority(int priority)
				{
					m_Priority = priority;
				}

				public void setShadowScale(VecFx32 scale)
				{
					m_ShadowScale.copy(scale);
				}

				public void setShadowHeight(int height)
				{
					m_ShadowHeight = height;
				}

				public void setShadowAlpha(sbyte alpha)
				{
					m_ShadowAlpha = alpha;
				}

				public sbyte getShadowAlpha()
				{
					return m_ShadowAlpha;
				}

				public void setShadowType(uint type)
				{
					m_ShadowType = type;
				}

				public void setBoundingBox(BoundingBox bb)
				{
					m_BB = bb;
				}

				public NNSG3dRenderObj getRenderObject()
				{
					return m_Object;
				}

				public void setLOD(uint lod)
				{
					m_LodLevel = lod;
				}

				public void setLODCnt(uint cnt)
				{
					m_LodCnt = cnt;
				}
			}
		}
	}
}
