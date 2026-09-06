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
			public class CModel
			{
				private NNSG3dResFileHeader m_MdlData;

				private NNSG3dResMdl m_MdlResource;

				private NNSG3dResFileHeader m_TexData;

				private NNSG3dResTex m_TexResource;

				private BoundingBox m_BB = new BoundingBox();

				private SLList<CModelTexture> m_TexList;

				~CModel()
				{
				}

				public void setup(NNSG3dResFileHeader mdlData, uint idx)
				{
					m_MdlData = mdlData;
					NNSG3dResMdlSet nNSG3dResMdlSet = NNS_G3dGetMdlSet(m_MdlData);
					m_TexResource = NNS_G3dGetTex(m_MdlData);
					ModelRef modelRef;
					for (modelRef = sys3d.modelRef; modelRef != null; modelRef = modelRef.next)
					{
						if (modelRef.mdlSet == nNSG3dResMdlSet)
						{
							modelRef.@ref++;
							break;
						}
					}
					if (modelRef == null)
					{
						ModelRef modelRef2 = new ModelRef();
						modelRef2.mdlSet = nNSG3dResMdlSet;
						modelRef2.@ref = 1;
						modelRef2.next = sys3d.modelRef;
						sys3d.modelRef = modelRef2;
					}
					if (m_TexResource != null)
					{
						setupTex(m_TexResource);
						if (modelRef == null)
						{
							NNS_G3dBindMdlSet(nNSG3dResMdlSet, m_TexResource);
						}
					}
					m_MdlResource = NNS_G3dGetMdlByIdx(NNS_G3dGetMdlSet(m_MdlData), idx);
					NNS_G3dMdlUseMdlDiff(m_MdlResource);
					NNS_G3dMdlUseMdlAmb(m_MdlResource);
					NNS_G3dMdlUseMdlSpec(m_MdlResource);
					NNS_G3dMdlUseMdlEmi(m_MdlResource);
					NNS_G3dMdlUseMdlAlpha(m_MdlResource);
					NNS_G3dMdlUseMdlPolygonID(m_MdlResource);
					NNS_G3dMdlUseMdlPolygonMode(m_MdlResource);
					NNS_G3dMdlSetMdlXLDepthUpdateAll(m_MdlResource, 0);
					NNSG3dResMdlInfo nNSG3dResMdlInfo = NNS_G3dGetMdlInfo(m_MdlResource);
					VecFx16 vecFx = new VecFx16();
					VecFx16 vecFx2 = new VecFx16();
					vecFx.x = nNSG3dResMdlInfo.boxX;
					vecFx.y = nNSG3dResMdlInfo.boxY;
					vecFx.z = (short)(nNSG3dResMdlInfo.boxZ * -1);
					vecFx2.x = nNSG3dResMdlInfo.boxW;
					vecFx2.y = nNSG3dResMdlInfo.boxH;
					vecFx2.z = nNSG3dResMdlInfo.boxD;
					m_BB.set(vecFx, vecFx2, nNSG3dResMdlInfo.boxPosScale);
				}

				public void cleanup()
				{
					int num = 0;
					NNSG3dResTex nNSG3dResTex = NNS_G3dGetTex(m_MdlData);
					NNSG3dResMdlSet nNSG3dResMdlSet = NNS_G3dGetMdlSet(m_MdlData);
					ModelRef modelRef = sys3d.modelRef;
					ModelRef modelRef2 = sys3d.modelRef;
					while (modelRef2 != null && modelRef2.mdlSet != nNSG3dResMdlSet)
					{
						num++;
						modelRef = modelRef2.next;
						modelRef2 = modelRef2.next;
					}
					if (modelRef2 != null && --modelRef2.@ref == 0)
					{
						if (num > 0)
						{
							modelRef = sys3d.modelRef;
							for (int i = 1; i < num; i++)
							{
								modelRef = modelRef.next;
							}
							modelRef.next = modelRef2.next;
						}
						else
						{
							sys3d.modelRef = modelRef2.next;
						}
						modelRef2.destruct();
						NNS_G3dReleaseMdlSet(nNSG3dResMdlSet);
					}
					if (nNSG3dResTex != null)
					{
						releaseTex(nNSG3dResTex);
					}
				}

				public void setupTex(NNSG3dResTex texture)
				{
					uint szByte = NNS_G3dTexGetRequiredSize(texture);
					uint num = NNS_G3dTex4x4GetRequiredSize(texture);
					uint szByte2 = NNS_G3dPlttGetRequiredSize(texture);
					TexVram texVram = null;
					TexVram texVram2 = null;
					TexVram texVram3 = null;
					NNS_GfdDumpLnkTexVramManager();
					NNS_GfdDumpLnkPlttVramManager();
					texVram = NNS_GfdAllocLnkTexVram(szByte, 0, 0u);
					texVram2 = NNS_GfdAllocLnkTexVram(num, 1, 0u);
					texVram3 = NNS_GfdAllocLnkPlttVram(szByte2, 0, 1u);
					NNS_GfdDumpLnkTexVramManager();
					NNS_GfdDumpLnkPlttVramManager();
					if (num != 0)
					{
					}
					NNS_GfdGetTexKeyAddr(texVram);
					NNS_GfdGetTexKeyAddr(texVram2);
					NNS_GfdGetPlttKeyAddr(texVram3);
					NNS_GfdGetTexKey4x4Flag(texVram2);
					NNS_G3dTexSetTexKey(texture, texVram, texVram2);
					NNS_G3dPlttSetPlttKey(texture, texVram3);
					NNS_G3dTexLoad(texture, 1);
					NNS_G3dPlttLoad(texture, 1);
				}

				public void releaseTex(NNSG3dResTex texture)
				{
					TexVram texVram = NNS_G3dPlttReleasePlttKey(texture);
					NNS_G3dTexReleaseTexKey(texture, out var texKey, out var tex4x4Key);
					if (texVram != null)
					{
						NNS_GfdFreeLnkPlttVram(texVram);
					}
					if (tex4x4Key != null)
					{
						NNS_GfdFreeLnkTexVram(tex4x4Key);
					}
					if (texKey != null)
					{
						NNS_GfdFreeLnkTexVram(texKey);
					}
				}

				public void setupReplaceTex(NNSG3dResFileHeader texData)
				{
					m_TexData = texData;
					NNSG3dResTex nNSG3dResTex = NNS_G3dGetTex(m_TexData);
					if (nNSG3dResTex != null)
					{
						setupTex(nNSG3dResTex);
					}
				}

				public void releaseReplaceTex()
				{
					if (m_TexData != null)
					{
						NNSG3dResTex nNSG3dResTex = NNS_G3dGetTex(m_TexData);
						if (nNSG3dResTex != null)
						{
							releaseTex(nNSG3dResTex);
						}
					}
				}

				public void replaceTex(string name)
				{
					NNSG3dUtilResName nNSG3dUtilResName = new NNSG3dUtilResName("eye1");
					NNSG3dUtilResName nNSG3dUtilResName2 = new NNSG3dUtilResName("eye1_pl");
					NNSG3dResTex pTex = NNS_G3dGetTex(m_TexData);
					NNS_G3dReleaseMdlTexEx(m_MdlResource, nNSG3dUtilResName.resName);
					NNS_G3dReleaseMdlPlttEx(m_MdlResource, nNSG3dUtilResName2.resName);
					NNS_G3dBindMdlTexEx(m_MdlResource, pTex, nNSG3dUtilResName.resName);
					NNS_G3dBindMdlPlttEx(m_MdlResource, pTex, nNSG3dUtilResName2.resName);
				}

				public bool hasMdlTex()
				{
					if (m_TexResource != null)
					{
						return true;
					}
					return false;
				}

				public void bindMdlTex()
				{
					unbindTex();
					NNS_G3dBindMdlSet(NNS_G3dGetMdlSet(m_MdlData), m_TexResource);
				}

				public void bindReplaceTex(ITexture pTex)
				{
					unbindTex();
					pTex.bindMdlSet(NNS_G3dGetMdlSet(m_MdlData));
				}

				public void unbindTex()
				{
					NNSG3dResMdlSet pMdlSet = NNS_G3dGetMdlSet(m_MdlData);
					NNS_G3dReleaseMdlSet(pMdlSet);
				}

				public void bindMdlTexel()
				{
					int num = 0;
					NNSG3dResMdl modelResource = getModelResource((uint)num);
					unbindTexel();
					while (modelResource != null)
					{
						NNS_G3dBindMdlTex(modelResource, m_TexResource);
						num++;
						modelResource = getModelResource((uint)num);
					}
				}

				public void bindReplaceTexel(ITexture pTexel)
				{
					int num = 0;
					NNSG3dResMdl modelResource = getModelResource((uint)num);
					unbindTexel();
					while (modelResource != null)
					{
						pTexel.bindMdlToTex(modelResource);
						num++;
						modelResource = getModelResource((uint)num);
					}
				}

				public void bindReplaceTexelByName(ITexture pTexel, string szTexelname)
				{
					int num = 0;
					NNSG3dResMdl modelResource = getModelResource((uint)num);
					unbindTexelByName(szTexelname);
					while (modelResource != null)
					{
						pTexel.bindMdlToTexByName(modelResource, szTexelname);
						num++;
						modelResource = getModelResource((uint)num);
					}
				}

				public void unbindTexel()
				{
					int num = 0;
					for (NNSG3dResMdl modelResource = getModelResource((uint)num); modelResource != null; modelResource = getModelResource((uint)num))
					{
						NNS_G3dReleaseMdlTex(modelResource);
						num++;
					}
				}

				public void unbindTexelByName(string szTexelname)
				{
					int num = 0;
					NNSG3dResMdl modelResource = getModelResource((uint)num);
					NNSG3dResName nNSG3dResName = new NNSG3dResName();
					strcpy(out nNSG3dResName.name, szTexelname);
					while (modelResource != null)
					{
						NNS_G3dReleaseMdlTexEx(modelResource, nNSG3dResName);
						num++;
						modelResource = getModelResource((uint)num);
					}
				}

				public void bindMdlPltt()
				{
					int num = 0;
					NNSG3dResMdl modelResource = getModelResource((uint)num);
					unbindPltt();
					while (modelResource != null)
					{
						NNS_G3dBindMdlPltt(modelResource, m_TexResource);
						num++;
						modelResource = getModelResource((uint)num);
					}
				}

				public void bindReplacePltt(ITexture pPltt)
				{
					int num = 0;
					NNSG3dResMdl modelResource = getModelResource((uint)num);
					unbindPltt();
					while (modelResource != null)
					{
						pPltt.bindMdlToPltt(modelResource);
						num++;
						modelResource = getModelResource((uint)num);
					}
				}

				public void bindReplacePlttByName(ITexture pPltt, string szPalettename)
				{
					int num = 0;
					NNSG3dResMdl modelResource = getModelResource((uint)num);
					unbindPlttByName(szPalettename);
					while (modelResource != null)
					{
						pPltt.bindMdlToPlttByName(modelResource, szPalettename);
						num++;
						modelResource = getModelResource((uint)num);
					}
				}

				public void unbindPltt()
				{
					int num = 0;
					for (NNSG3dResMdl modelResource = getModelResource((uint)num); modelResource != null; modelResource = getModelResource((uint)num))
					{
						NNS_G3dReleaseMdlPltt(modelResource);
						num++;
					}
				}

				public void unbindPlttByName(string szPlttname)
				{
					int num = 0;
					NNSG3dResMdl modelResource = getModelResource((uint)num);
					NNSG3dResName nNSG3dResName = new NNSG3dResName();
					strcpy(out nNSG3dResName.name, szPlttname);
					while (modelResource != null)
					{
						NNS_G3dReleaseMdlPlttEx(modelResource, nNSG3dResName);
						num++;
						modelResource = getModelResource((uint)num);
					}
				}

				public void setDiffuse(ushort diffuse)
				{
					NNS_G3dMdlSetMdlDiffAll(m_MdlResource, diffuse);
				}

				public void setAmbient(ushort ambient)
				{
					NNS_G3dMdlSetMdlAmbAll(m_MdlResource, ambient);
				}

				public void setSpecular(ushort specular)
				{
					NNS_G3dMdlSetMdlSpecAll(m_MdlResource, specular);
				}

				public void setEmission(ushort emission)
				{
					NNS_G3dMdlSetMdlEmiAll(m_MdlResource, emission);
				}

				public void setRenderObject(NNSG3dRenderObj rdObj)
				{
					NNS_G3dRenderObjInit(rdObj, m_MdlResource);
				}

				public NNSG3dResMdl getModelResource(uint index)
				{
					NNSG3dResMdlSet nNSG3dResMdlSet = NNS_G3dGetMdlSet(m_MdlData);
					if (index >= nNSG3dResMdlSet.dict.numEntry)
					{
						return null;
					}
					return NNS_G3dGetMdlByIdx(nNSG3dResMdlSet, index);
				}

				public void drawBB()
				{
					m_BB.draw();
				}

				public NNSG3dResTex getResTex()
				{
					return m_TexResource;
				}

				public BoundingBox getBoundingBox()
				{
					return m_BB;
				}

				public NNSG3dResMdl getMdlResource()
				{
					return m_MdlResource;
				}
			}
		}
	}
}
