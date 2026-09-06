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
			public class CMotSet
			{
				public enum enFLAG
				{
					enFLAG_ENABLE = 1,
					enFLAG_END
				}

				public const enFLAG enFLAG_ENABLE = enFLAG.enFLAG_ENABLE;

				public const enFLAG enFLAG_END = enFLAG.enFLAG_END;

				public static int MOTION_MAX = 128;

				private int m_GlobalFlag;

				private int[] m_Flag = new int[MOTION_MAX];

				private int m_MotNum;

				private int m_PlayIndex;

				private int m_PreIndex;

				private uint m_PlayNo;

				private uint m_BlendFrame;

				private uint m_BlendFrameMax;

				private int m_BlendRate;

				private int m_FrameRate;

				private NNSG3dResMdl m_pMdlResData;

				private CMotNode[] m_MotSet = new CMotNode[MOTION_MAX];

				private NNSG3dRenderObj m_pRenderObj;

				public CMotSet()
				{
					m_MotNum = 0;
					m_PlayIndex = -1;
					m_PreIndex = -1;
					m_GlobalFlag = 0;
					m_pMdlResData = null;
					m_pRenderObj = null;
					for (int i = 0; i < m_MotSet.Length; i++)
					{
						m_MotSet[i] = new CMotNode();
					}
					initValue();
				}

				~CMotSet()
				{
				}

				public void setup(NNSG3dResMdl mdlResData)
				{
					initValue();
					for (int i = 0; i < MOTION_MAX; i++)
					{
						if ((1 & m_Flag[i]) != 0)
						{
							m_MotSet[i].m_pMotData = null;
							m_MotSet[i].cleanup();
							m_MotSet[i].m_Index = 0u;
							m_MotSet[i].m_pMotData = null;
						}
					}
					m_pMdlResData = mdlResData;
					m_GlobalFlag = 1;
				}

				public void cleanup()
				{
					for (int i = 0; i < MOTION_MAX; i++)
					{
						if ((1 & m_Flag[i]) != 0)
						{
							m_MotSet[i].m_pMotData = null;
							m_MotSet[i].cleanup();
							m_MotSet[i].m_Index = 0u;
							m_MotSet[i].m_pMotData = null;
						}
					}
					initValue();
				}

				public void addMotion(ncap.SMotionFileHeader motData)
				{
					if (m_MotNum >= MOTION_MAX)
					{
						return;
					}
					ncap.SMotionInfoHeader infoHeader = motData.m_InfoHeader;
					_ = motData.ucFileType;
					if ((infoHeader.unFlag & 1) == 0)
					{
						infoHeader.unFlag |= 1u;
					}
					for (int i = 0; i < infoHeader.ucNbMotions; i++)
					{
						for (int j = 0; j < MOTION_MAX; j++)
						{
							if (m_Flag[j] == 0)
							{
								m_MotNum++;
								m_Flag[j] = 1;
								m_MotSet[j].m_pMotData = motData;
								// PORT: FF4 numbers its field motions differently (GameProfile.FieldMotionId).
								m_MotSet[j].setIndex(OpenFF.Client.GameProfile.FieldMotionId(motData.m_auiMotIdx[i]));
								m_MotSet[j].setup(motData.m_ResFileHeaderMot, m_pMdlResData, (uint)i, null);
								break;
							}
						}
					}
				}

				public void removeMotion(ncap.SMotionFileHeader motData)
				{
					if (m_MotNum <= 0)
					{
						return;
					}
					for (int i = 0; i < MOTION_MAX; i++)
					{
						if ((1 & m_Flag[i]) != 0 && motData == m_MotSet[i].m_pMotData)
						{
							m_MotSet[i].m_pMotData = null;
							m_MotSet[i].cleanup();
							m_Flag[i] = 0;
							m_MotNum--;
						}
					}
				}

				public void start(uint motIdx, bool fLoop, uint blendFrame)
				{
					int blendRatio = 4096;
					if (blendFrame != 0 && motIdx != m_PlayNo)
					{
						if (-1 != m_PreIndex)
						{
							m_MotSet[m_PreIndex].removeRenderObject(m_pRenderObj);
						}
						if (-1 != m_PlayIndex)
						{
							m_PreIndex = m_PlayIndex;
							m_MotSet[m_PreIndex].setBlend(4096);
							blendRatio = 0;
						}
					}
					else
					{
						if (-1 != m_PreIndex)
						{
							m_MotSet[m_PreIndex].removeRenderObject(m_pRenderObj);
						}
						if (-1 != m_PlayIndex)
						{
							m_MotSet[m_PlayIndex].removeRenderObject(m_pRenderObj);
						}
						m_PreIndex = -1;
					}
					m_PlayIndex = -1;
					for (int i = 0; i < m_MotNum; i++)
					{
						if (motIdx == m_MotSet[i].getIndex())
						{
							m_PlayIndex = i;
							break;
						}
					}
					if (-1 != m_PlayIndex)
					{
						m_MotSet[m_PlayIndex].addRenderObject(m_pRenderObj);
						m_MotSet[m_PlayIndex].start(0, blendRatio);
						m_MotSet[m_PlayIndex].setLoop(fLoop);
						m_MotSet[m_PlayIndex].setFrameRate(m_FrameRate);
						m_BlendFrame = 0u;
						m_BlendFrameMax = blendFrame;
						m_BlendRate = 0;
						m_PlayNo = motIdx;
					}
				}

				public void next()
				{
					if (-1 != m_PlayIndex)
					{
						m_MotSet[m_PlayIndex].setBlend(m_BlendRate);
						m_MotSet[m_PlayIndex].next();
					}
					if (-1 != m_PreIndex)
					{
						m_MotSet[m_PreIndex].setBlend(4096 - m_BlendRate);
						m_MotSet[m_PreIndex].next();
						m_BlendFrame++;
						m_BlendRate = (int)(4096 * m_BlendFrame);
						if (m_BlendFrameMax != 0)
						{
							m_BlendRate /= (int)m_BlendFrameMax;
						}
						if (m_BlendFrame >= m_BlendFrameMax)
						{
							m_BlendRate = 4096;
							m_MotSet[m_PreIndex].removeRenderObject(m_pRenderObj);
							m_PreIndex = -1;
						}
					}
				}

				public void setEnable(bool b)
				{
					if (b)
					{
						m_GlobalFlag |= 1;
					}
					else
					{
						m_GlobalFlag &= -2;
					}
				}

				public bool isEnable()
				{
					if ((1 & m_GlobalFlag) != 0)
					{
						return true;
					}
					return false;
				}

				public void addRenderObject(NNSG3dRenderObj pObj)
				{
					m_pRenderObj = pObj;
				}

				public void removeRenderObject(NNSG3dRenderObj pObj)
				{
					m_pRenderObj = null;
				}

				public void setFrameRate(int fps)
				{
					m_FrameRate = fps;
					for (int i = 0; i < MOTION_MAX; i++)
					{
						if (m_MotSet[i].isEnable())
						{
							m_MotSet[i].setFrameRate(m_FrameRate);
						}
					}
				}

				public int getFrameRate()
				{
					return m_FrameRate;
				}

				public void setFrame(uint frm)
				{
					if (-1 != m_PlayIndex)
					{
						m_MotSet[m_PlayIndex].setFrame(frm);
					}
				}

				public uint getFrame()
				{
					if (-1 != m_PlayIndex)
					{
						return m_MotSet[m_PlayIndex].getFrame();
					}
					return 0u;
				}

				public uint getMaxFrame()
				{
					if (-1 != m_PlayIndex)
					{
						return m_MotSet[m_PlayIndex].getMaxFrame();
					}
					return 0u;
				}

				public int getIndex()
				{
					if (-1 != m_PlayIndex)
					{
						return (int)m_MotSet[m_PlayIndex].getIndex();
					}
					return -1;
				}

				public int getPreIndex()
				{
					if (-1 != m_PreIndex)
					{
						return (int)m_MotSet[m_PreIndex].getIndex();
					}
					return -1;
				}

				public void setLoop(bool b)
				{
					if (-1 != m_PlayIndex)
					{
						m_MotSet[m_PlayIndex].setLoop(b);
					}
				}

				public void setPause(bool b)
				{
					if (-1 != m_PlayIndex)
					{
						m_MotSet[m_PlayIndex].setPause(b);
					}
				}

				public bool isEndOfMotion()
				{
					if (-1 != m_PlayIndex)
					{
						return m_MotSet[m_PlayIndex].isEndOfMotion();
					}
					return true;
				}

				public bool isMotion(uint motIdx)
				{
					for (int i = 0; i < MOTION_MAX; i++)
					{
						if (motIdx == m_MotSet[i].getIndex())
						{
							return true;
						}
					}
					return false;
				}

				public void initValue()
				{
					m_GlobalFlag = 0;
					for (byte b = 0; b < MOTION_MAX; b++)
					{
						m_Flag[b] = 0;
					}
					m_MotNum = 0;
					m_PlayIndex = -1;
					m_PreIndex = -1;
					m_PlayNo = 0u;
					m_BlendFrame = 0u;
					m_BlendFrameMax = 0u;
					m_BlendRate = 4096;
					m_FrameRate = 4096;
					m_pMdlResData = null;
					m_pRenderObj = null;
				}
			}
		}
	}
}
