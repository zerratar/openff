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
			public class CModelSet
			{
				public enum enFLAG
				{
					enFLAG_ENABLE = 1,
					enFLAG_TEXRELEASE,
					enFLAG_END
				}

				public const enFLAG enFLAG_ENABLE = enFLAG.enFLAG_ENABLE;

				public const enFLAG enFLAG_TEXRELEASE = enFLAG.enFLAG_TEXRELEASE;

				public const enFLAG enFLAG_END = enFLAG.enFLAG_END;

				private const int MODEL_MAX = 1;

				private int m_GlobalFlag;

				private int[] m_Flag = new int[1];

				private int m_UseIdx;

				private int m_MdlNum;

				private CModel[] m_MdlSet = new CModel[1];

				private Array m_pNmdpData;

				private NNSG3dResFileHeader m_pMdlData;

				private NNSG3dRenderObj m_pRenderObj;

				public CModelSet()
				{
					for (int i = 0; i < m_MdlSet.Length; i++)
					{
						m_MdlSet[i] = new CModel();
					}
					initValue();
				}

				~CModelSet()
				{
				}

				public void setup(Array nmdpData)
				{
					m_GlobalFlag = 1;
					m_UseIdx = 0;
					m_pNmdpData = nmdpData;
					ArrayReader arrayReader = new ArrayReader(nmdpData);
					nmdp.SModelFileHeader sModelFileHeader = (nmdp.SModelFileHeader)arrayReader;
					nmdp.SModelInfoHeader infoHeader = sModelFileHeader.m_InfoHeader;
					_ = sModelFileHeader.ucFileType;
					if ((infoHeader.unFlag & 1) == 0)
					{
						infoHeader.unFlag |= 1u;
					}
					m_pMdlData = sModelFileHeader.m_ResFileHeaderModel;
					for (int i = 0; i < infoHeader.ucNbModels; i++)
					{
						m_MdlNum++;
						m_Flag[i] = 1;
						m_MdlSet[i].setup(m_pMdlData, (uint)i);
					}
					arrayReader.dispose();
				}

				public void cleanup()
				{
					for (int i = 0; i < 1; i++)
					{
						if ((1 & m_Flag[i]) != 0)
						{
							m_MdlSet[i].cleanup();
						}
						m_Flag[i] = 0;
					}
					initValue();
				}

				public void addRenderObject(NNSG3dRenderObj pObj)
				{
					m_pRenderObj = pObj;
					if (-1 != m_UseIdx)
					{
						m_MdlSet[m_UseIdx].setRenderObject(m_pRenderObj);
					}
				}

				public void removeRenderObject(NNSG3dRenderObj pObj)
				{
					m_pRenderObj = null;
				}

				public void setUseMdl(uint mdlIdx)
				{
					m_UseIdx = (int)mdlIdx;
				}

				public uint getUseMdl()
				{
					return (uint)m_UseIdx;
				}

				public NNSG3dResMdl getMdlResource()
				{
					return m_MdlSet[m_UseIdx].getMdlResource();
				}

				public bool hasMdlTex()
				{
					return m_MdlSet[m_UseIdx].hasMdlTex();
				}

				public bool releaseTexResource()
				{
					if ((m_GlobalFlag & 1) == 0)
					{
						return false;
					}
					if ((m_GlobalFlag & 2) != 0)
					{
						return false;
					}
					if (m_MdlNum > 1)
					{
						return false;
					}
					if (!hasMdlTex())
					{
						return false;
					}
					m_GlobalFlag |= 2;
					m_MdlSet[m_UseIdx].bindMdlTex();
					m_MdlSet[m_UseIdx].getResTex();
					uint num = 0u;
					_ = 0;
					CHeap.resize_app(m_pNmdpData, num);
					OS_Printf("\n\n\n\n\n\n\n\n\n\n\n\n");
					OS_Printf("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%\n");
					OS_Printf(" releaseTexResource() Succeeded!!\n\n");
					return true;
				}

				public void bindMdlTex()
				{
					if ((m_GlobalFlag & 2) == 0)
					{
						m_MdlSet[m_UseIdx].bindMdlTex();
					}
				}

				public void bindReplaceTex(ITexture pTex)
				{
					if ((m_GlobalFlag & 2) == 0)
					{
						m_MdlSet[m_UseIdx].bindReplaceTex(pTex);
					}
				}

				public void unbindTex()
				{
					if ((m_GlobalFlag & 2) == 0)
					{
						m_MdlSet[m_UseIdx].unbindTex();
					}
				}

				public void bindMdlTexel()
				{
					if ((m_GlobalFlag & 2) == 0)
					{
						m_MdlSet[m_UseIdx].bindMdlTexel();
					}
				}

				public void bindReplaceTexel(ITexture pPltt)
				{
					if ((m_GlobalFlag & 2) == 0)
					{
						m_MdlSet[m_UseIdx].bindReplaceTexel(pPltt);
					}
				}

				public void bindReplaceTexelByName(ITexture pPltt, string szTexelname)
				{
					if ((m_GlobalFlag & 2) == 0)
					{
						m_MdlSet[m_UseIdx].bindReplaceTexelByName(pPltt, szTexelname);
					}
				}

				public void unbindTexel()
				{
					if ((m_GlobalFlag & 2) == 0)
					{
						m_MdlSet[m_UseIdx].unbindTexel();
					}
				}

				public void unbindTexelByName(string szTexelname)
				{
					if ((m_GlobalFlag & 2) == 0)
					{
						m_MdlSet[m_UseIdx].unbindTexelByName(szTexelname);
					}
				}

				public void bindMdlPltt()
				{
					if ((m_GlobalFlag & 2) == 0)
					{
						m_MdlSet[m_UseIdx].bindMdlPltt();
					}
				}

				public void bindReplacePltt(ITexture pPltt)
				{
					if ((m_GlobalFlag & 2) == 0)
					{
						m_MdlSet[m_UseIdx].bindReplacePltt(pPltt);
					}
				}

				public void bindReplacePlttByName(ITexture pPltt, string szPlttname)
				{
					if ((m_GlobalFlag & 2) == 0)
					{
						m_MdlSet[m_UseIdx].bindReplacePlttByName(pPltt, szPlttname);
					}
				}

				public void unbindPltt()
				{
					if ((m_GlobalFlag & 2) == 0)
					{
						m_MdlSet[m_UseIdx].unbindPltt();
					}
				}

				public void unbindPlttByName(string szPlttname)
				{
					if ((m_GlobalFlag & 2) == 0)
					{
						m_MdlSet[m_UseIdx].unbindPlttByName(szPlttname);
					}
				}

				public BoundingBox getBoundingBox()
				{
					return m_MdlSet[m_UseIdx].getBoundingBox();
				}

				public void drawBB()
				{
					m_MdlSet[m_UseIdx].drawBB();
				}

				public void initValue()
				{
					m_UseIdx = -1;
					m_MdlNum = 0;
					m_pNmdpData = null;
					m_pMdlData = null;
					m_pRenderObj = null;
					for (int i = 0; i < 1; i++)
					{
						m_Flag[i] = 0;
					}
				}

				public NNSG3dResFileHeader getMdlData()
				{
					return m_pMdlData;
				}

				public CModel getMdl(uint mdlIdx)
				{
					return m_MdlSet[mdlIdx];
				}
			}
		}
	}
}
