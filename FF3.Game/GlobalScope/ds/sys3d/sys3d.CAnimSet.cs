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
			public class CAnimSet
			{
				public enum enTYPE
				{
					enTYPE_ITA,
					enTYPE_ITP,
					enTYPE_IMA,
					enTYPE_IVA,
					enTYPE_END
				}

				public enum enFLAG
				{
					enFLAG_ENABLE = 1,
					enFLAG_END
				}

				public const enTYPE enTYPE_ITA = enTYPE.enTYPE_ITA;

				public const enTYPE enTYPE_ITP = enTYPE.enTYPE_ITP;

				public const enTYPE enTYPE_IMA = enTYPE.enTYPE_IMA;

				public const enTYPE enTYPE_IVA = enTYPE.enTYPE_IVA;

				public const enTYPE enTYPE_END = enTYPE.enTYPE_END;

				public const enFLAG enFLAG_ENABLE = enFLAG.enFLAG_ENABLE;

				public const enFLAG enFLAG_END = enFLAG.enFLAG_END;

				private int m_Flag;

				private CAnimation[] m_AnimSet = new CAnimation[4];

				public CAnimSet()
				{
					for (int i = 0; i < m_AnimSet.Length; i++)
					{
						m_AnimSet[i] = new CAnimation();
					}
					m_Flag = 0;
				}

				~CAnimSet()
				{
				}

				public void setup(Array anmData, NNSG3dResMdl mdlResData, NNSG3dResTex texResData)
				{
					m_Flag = 1;
					ArrayReader arrayReader = new ArrayReader(anmData);
					namp.SAnimationFileHeader sAnimationFileHeader = (namp.SAnimationFileHeader)arrayReader;
					namp.SAnimationInfoHeader infoHeader = sAnimationFileHeader.m_InfoHeader;
					_ = sAnimationFileHeader.ucFileType;
					if ((infoHeader.unFlag & 1) == 0)
					{
						infoHeader.unFlag |= 1u;
					}
					if (0 < infoHeader.ucNbIma)
					{
						m_AnimSet[2].setup(sAnimationFileHeader.m_ResFileHeaderIma, mdlResData, 0u, null);
					}
					if (0 < infoHeader.ucNbIta)
					{
						m_AnimSet[0].setup(sAnimationFileHeader.m_ResFileHeaderIta, mdlResData, 0u, null);
					}
					if (0 < infoHeader.ucNbItp)
					{
						m_AnimSet[1].setup(sAnimationFileHeader.m_ResFileHeaderItp, mdlResData, 0u, texResData);
					}
					if (0 < infoHeader.ucNbIva)
					{
						m_AnimSet[3].setup(sAnimationFileHeader.m_ResFileHeaderIva, mdlResData, 0u, null);
					}
					arrayReader.dispose();
				}

				public void cleanup()
				{
					for (byte b = 0; b < 4; b++)
					{
						if (m_AnimSet[b].isEnable())
						{
							m_AnimSet[b].cleanup();
						}
					}
				}

				public void start(int frm, enTYPE type)
				{
					if (enTYPE.enTYPE_END == type)
					{
						for (byte b = 0; b < 4; b++)
						{
							if (m_AnimSet[b].isEnable())
							{
								m_AnimSet[b].start(frm, 4096);
							}
						}
					}
					else if (m_AnimSet[(int)type].isEnable())
					{
						m_AnimSet[(int)type].start(frm, 4096);
					}
				}

				public void next()
				{
					for (byte b = 0; b < 4; b++)
					{
						if (m_AnimSet[b].isEnable())
						{
							m_AnimSet[b].next();
						}
					}
				}

				public bool startAnimation(uint index, enTYPE type, int frame)
				{
					if (m_AnimSet[(int)type].isEnable())
					{
						return m_AnimSet[(int)type].startAnimation(index, frame, 4096);
					}
					return false;
				}

				public void setEnable(bool b)
				{
					if (b)
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

				public void addRenderObject(NNSG3dRenderObj pObj)
				{
					for (byte b = 0; b < 4; b++)
					{
						if (m_AnimSet[b].isEnable())
						{
							m_AnimSet[b].addRenderObject(pObj);
						}
					}
				}

				public void removeRenderObject(NNSG3dRenderObj pObj)
				{
					for (byte b = 0; b < 4; b++)
					{
						if (m_AnimSet[b].isEnable())
						{
							m_AnimSet[b].removeRenderObject(pObj);
						}
					}
				}

				public void setFrameRate(int fps, enTYPE type)
				{
					if (enTYPE.enTYPE_END == type)
					{
						for (byte b = 0; b < 4; b++)
						{
							if (m_AnimSet[b].isEnable())
							{
								m_AnimSet[b].setFrameRate(fps);
							}
						}
					}
					else if (m_AnimSet[(int)type].isEnable())
					{
						m_AnimSet[(int)type].setFrameRate(fps);
					}
				}

				public int getFrameRate(enTYPE type)
				{
					int result = 0;
					if (m_AnimSet[(int)type].isEnable())
					{
						result = m_AnimSet[(int)type].getFrameRate();
					}
					return result;
				}

				public void setFrame(uint frm, enTYPE type)
				{
					if (enTYPE.enTYPE_END == type)
					{
						for (byte b = 0; b < 4; b++)
						{
							if (m_AnimSet[b].isEnable())
							{
								m_AnimSet[b].setFrame(frm);
							}
						}
					}
					else if (m_AnimSet[(int)type].isEnable())
					{
						m_AnimSet[(int)type].setFrame(frm);
					}
				}

				public uint getFrame(enTYPE type)
				{
					uint result = 0u;
					if (m_AnimSet[(int)type].isEnable())
					{
						result = m_AnimSet[(int)type].getFrame();
					}
					return result;
				}

				public uint getMaxFrame(enTYPE type)
				{
					uint result = 0u;
					if (m_AnimSet[(int)type].isEnable())
					{
						result = m_AnimSet[(int)type].getMaxFrame();
					}
					return result;
				}

				public void setLoop(bool b, enTYPE type)
				{
					if (enTYPE.enTYPE_END == type)
					{
						for (byte b2 = 0; b2 < 4; b2++)
						{
							if (m_AnimSet[b2].isEnable())
							{
								m_AnimSet[b2].setLoop(b);
							}
						}
					}
					else if (m_AnimSet[(int)type].isEnable())
					{
						m_AnimSet[(int)type].setLoop(b);
					}
				}

				public void setPause(bool b, enTYPE type)
				{
					if (enTYPE.enTYPE_END == type)
					{
						for (byte b2 = 0; b2 < 4; b2++)
						{
							if (m_AnimSet[b2].isEnable())
							{
								m_AnimSet[b2].setPause(b);
							}
						}
					}
					else if (m_AnimSet[(int)type].isEnable())
					{
						m_AnimSet[(int)type].setPause(b);
					}
				}

				public bool isEndOfMotion(enTYPE type)
				{
					if (m_AnimSet[(int)type].isEnable())
					{
						return m_AnimSet[(int)type].isEndOfMotion();
					}
					return false;
				}
			}
		}
	}
}
