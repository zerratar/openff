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
			public class CAnimation
			{
				private const int FLAG_ENABLE = 1;

				private const int FLAG_ANIMATION = 2;

				private const int FLAG_ADD_RENDER = 4;

				private const int FLAG_LOOP = 8;

				private const int FLAG_PAUSE = 16;

				private int m_Flag;

				private int m_Type;

				private NNSG3dAnmObj m_AnmObject;

				private NNSG3dResFileHeader m_AnmData;

				private NNSG3dResAnmHeader m_AnmResource;

				private int m_FrameRate;

				private int m_CurrAnmNo;

				private NNSG3dResMdl m_RelateMdl;

				private NNSG3dResTex m_RelateTex;

				private NNSG3dRenderObj m_pRenderObj;

				public CAnimation()
				{
					m_CurrAnmNo = INVALID_ANIME_INDEX;
					m_AnmObject = null;
					m_RelateMdl = null;
					m_Flag = 0;
					m_FrameRate = 4096;
				}

				~CAnimation()
				{
				}

				public void setup(NNSG3dResFileHeader anmData, NNSG3dResMdl mdlResData, uint idx, NNSG3dResTex texResData)
				{
					if ((m_Flag & 1) == 0 || idx != m_CurrAnmNo)
					{
						cleanup();
						m_Flag = 3;
						m_AnmData = anmData;
						m_AnmResource = NNS_G3dGetAnmByIdx(m_AnmData, idx);
						m_AnmObject = NNS_G3dAllocAnmObj(CHeap.getAppAllocator(), m_AnmResource, mdlResData);
						m_RelateMdl = mdlResData;
						m_RelateTex = texResData;
						m_FrameRate = 4096;
						NNS_G3dAnmObjInit(m_AnmObject, m_AnmResource, m_RelateMdl, m_RelateTex);
						m_CurrAnmNo = (int)idx;
					}
				}

				public void cleanup()
				{
					if ((m_Flag & 1) != 0)
					{
						NNS_G3dFreeAnmObj(CHeap.getAppAllocator(), m_AnmObject);
						m_Flag = 0;
						m_AnmObject = null;
						m_RelateMdl = null;
						m_CurrAnmNo = INVALID_ANIME_INDEX;
						m_AnmData = null;
						m_AnmResource = null;
						m_RelateTex = null;
					}
				}

				public void start(int frm, int blendRatio)
				{
					NNS_G3dAnmObjSetFrame(m_AnmObject, frm);
					NNS_G3dAnmObjSetBlendRatio(m_AnmObject, blendRatio);
				}

				public void next()
				{
					if ((m_Flag & 0x10) != 0 || m_Flag == 0)
					{
						return;
					}
					m_AnmObject.frame += m_FrameRate;
					if (m_AnmObject.frame >= NNS_G3dAnmObjGetNumFrame(m_AnmObject) - 4096)
					{
						m_AnmObject.frame = 0;
						if ((m_Flag & 8) == 0)
						{
							m_AnmObject.frame = NNS_G3dAnmObjGetNumFrame(m_AnmObject) - 4096;
						}
					}
				}

				public void setBlend(int blend)
				{
					if (m_AnmObject != null)
					{
						NNS_G3dAnmObjSetBlendRatio(m_AnmObject, blend);
					}
				}

				public void addRenderObject(NNSG3dRenderObj rdObj)
				{
					if ((m_Flag & 4) == 0)
					{
						m_Flag |= 4;
						m_pRenderObj = rdObj;
						NNS_G3dRenderObjAddAnmObj(rdObj, m_AnmObject);
					}
				}

				public void removeRenderObject(NNSG3dRenderObj rdObj)
				{
					m_Flag &= -5;
					m_pRenderObj = null;
					NNS_G3dRenderObjRemoveAnmObj(rdObj, m_AnmObject);
				}

				public void setEnable(bool flag)
				{
					if (flag)
					{
						m_Flag |= 1;
					}
					else
					{
						m_Flag &= -2;
					}
				}

				public bool isEnable()
				{
					if ((m_Flag & 1) != 0)
					{
						return true;
					}
					return false;
				}

				public void setLoop(bool flag)
				{
					if (flag)
					{
						m_Flag |= 8;
					}
					else
					{
						m_Flag &= -9;
					}
				}

				public void setPause(bool flag)
				{
					if (flag)
					{
						m_Flag |= 16;
					}
					else
					{
						m_Flag &= -17;
					}
				}

				public bool isEndOfMotion()
				{
					if ((m_Flag & 8) == 0)
					{
						if (m_AnmObject == null)
						{
							return true;
						}
						if (m_AnmObject.frame == NNS_G3dAnmObjGetNumFrame(m_AnmObject) - 4096)
						{
							return true;
						}
					}
					return false;
				}

				public bool startAnimation(uint index, int frame, int blend)
				{
					if (m_AnmData == null)
					{
						return false;
					}
					// PORT: setup allocates a new animation object for a new index; the render object
					// still lists the old one (in C++ the freed block came back at the same address, so
					// the stale pointer kept working). Swap it over.
					NNSG3dAnmObj old = m_AnmObject;
					bool attached = (m_Flag & 4) != 0;
					NNSG3dRenderObj rdObj = m_pRenderObj;
					setup(m_AnmData, m_RelateMdl, index, m_RelateTex);
					start(frame, blend);
					if (attached && rdObj != null && m_AnmObject != old)
					{
						if (old != null)
						{
							NNS_G3dRenderObjRemoveAnmObj(rdObj, old);
						}
						m_Flag &= -5;
						addRenderObject(rdObj);
					}
					return true;
				}

				public void setFrameRate(int fps)
				{
					m_FrameRate = fps;
				}

				public int getFrameRate()
				{
					return m_FrameRate;
				}

				public void setFrame(uint frame)
				{
					m_AnmObject.frame = FX_Mul(m_FrameRate, (int)(frame << 12));
				}

				public uint getFrame()
				{
					return (uint)(FX_Div(m_AnmObject.frame, m_FrameRate) >> 12);
				}

				public uint getMaxFrame()
				{
					return (uint)(FX_Div(NNS_G3dAnmObjGetNumFrame(m_AnmObject) - 4096, m_FrameRate) >> 12);
				}

				public NNSG3dResAnmHeader getResource()
				{
					return m_AnmResource;
				}

				public NNSG3dAnmObj getObject()
				{
					return m_AnmObject;
				}

				public int getCurrentIndex()
				{
					return m_CurrAnmNo;
				}
			}
		}
	}
}
