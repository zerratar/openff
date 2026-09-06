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
			public class CAnimations
			{
				public const int TYPE_MOTION = 1;

				public const int TYPE_ANIMATION = 2;

				private const int animation_max = 32;

				private const int FLAG_ENABLE = 1;

				private const int FLAG_RENDER = 2;

				private const int FLAG_RENDER_PRE = 4;

				private const int FLAG_LOOP = 8;

				private const int FLAG_PAUSE = 16;

				private int m_Type;

				private int m_GlobalFlag;

				private int[] m_Flag = new int[32];

				private int m_BlendFrame;

				private int m_BlendFrameMax;

				private int m_BlendRate;

				private int m_AnimationNum;

				private int m_AnimationIndex;

				private CAnimation[] m_Animation = new CAnimation[32];

				private int m_FrameRate;

				private NNSG3dRenderObj m_RenderObject;

				public CAnimations()
				{
					m_RenderObject = null;
					m_AnimationIndex = -1;
				}

				~CAnimations()
				{
				}

				public bool isEnable()
				{
					if ((m_GlobalFlag & 1) != 0)
					{
						return true;
					}
					return false;
				}

				public void setup(Array anmData, Array mdlResData, Array texResData)
				{
					m_RenderObject = null;
					m_AnimationIndex = -1;
					m_GlobalFlag |= 1;
				}

				public void cleanup()
				{
					m_RenderObject = null;
					m_AnimationIndex = -1;
					m_GlobalFlag = 0;
					for (int i = 0; i < 32; i++)
					{
						if (m_Flag[i] != 0)
						{
							m_Animation[i].cleanup();
						}
						m_Flag[i] = 0;
					}
				}

				public void start(uint index, bool fLoop, uint blendFrame)
				{
					if (!isEnable() || m_AnimationNum <= index)
					{
						return;
					}
					if (m_AnimationIndex == index)
					{
						m_Animation[index].start(0, 4096);
						m_Animation[index].setLoop(fLoop);
						return;
					}
					m_AnimationIndex = (int)index;
					if (m_Type == 1)
					{
						start_motion_blend(index, fLoop, blendFrame);
					}
					if (m_Type == 2)
					{
						start_animation();
					}
				}

				public void start_motion(uint index, bool fLoop)
				{
					for (int i = 0; i < 32; i++)
					{
						if ((m_Flag[i] & 2) != 0)
						{
							m_Animation[i].removeRenderObject(m_RenderObject);
							m_Flag[i] &= -3;
							break;
						}
					}
					m_Animation[index].addRenderObject(m_RenderObject);
					m_Animation[index].start(0, 4096);
					m_Animation[index].setLoop(fLoop);
					m_Flag[index] |= 2;
				}

				public void start_motion_blend(uint index, bool fLoop, uint blendFrame)
				{
					if (blendFrame != 0)
					{
						for (int i = 0; i < 32; i++)
						{
							if ((m_Flag[i] & 4) != 0)
							{
								m_Animation[i].removeRenderObject(m_RenderObject);
								m_Flag[i] &= -5;
								break;
							}
						}
						for (int j = 0; j < 32; j++)
						{
							if ((m_Flag[j] & 2) != 0)
							{
								m_Flag[j] &= -3;
								m_Flag[j] |= 4;
								break;
							}
						}
					}
					else
					{
						for (int k = 0; k < 32; k++)
						{
							if ((m_Flag[k] & 2) != 0)
							{
								m_Animation[k].removeRenderObject(m_RenderObject);
								m_Flag[k] &= -3;
								break;
							}
						}
					}
					m_Animation[index].addRenderObject(m_RenderObject);
					m_Animation[index].start(0, 4096);
					m_Animation[index].setLoop(fLoop);
					m_Flag[index] |= 2;
					m_BlendFrame = 0;
					m_BlendFrameMax = (int)blendFrame;
					m_BlendRate = 0;
				}

				public void start_animation()
				{
					for (int i = 0; i < 32; i++)
					{
						if ((m_Flag[i] & 2) == 0)
						{
							m_Animation[i].addRenderObject(m_RenderObject);
							m_Animation[i].start(0, 4096);
							m_Animation[i].setLoop(flag: true);
							m_Flag[i] |= 2;
						}
					}
				}

				public void next()
				{
					if (!isPause())
					{
						if (m_Type == 1)
						{
							next_motion_blend();
						}
						if (m_Type == 2)
						{
							next_animation();
						}
					}
				}

				public void next_motion()
				{
					for (int i = 0; i < 32; i++)
					{
						if (m_Flag[i] != 0 && (m_Flag[i] & 2) != 0)
						{
							m_Animation[i].next();
						}
					}
				}

				public void next_motion_blend()
				{
					for (int i = 0; i < 32; i++)
					{
						if (m_Flag[i] == 0)
						{
							continue;
						}
						if ((m_Flag[i] & 2) != 0)
						{
							m_Animation[i].next();
							m_Animation[i].setBlend(m_BlendRate);
						}
						if ((m_Flag[i] & 4) != 0)
						{
							m_Animation[i].next();
							m_Animation[i].setBlend(4096 - m_BlendRate);
							m_BlendRate = 4096 * m_BlendFrame / m_BlendFrameMax;
							m_BlendFrame++;
							if (m_BlendFrame >= m_BlendFrameMax)
							{
								m_BlendRate = 4096;
								m_Animation[i].removeRenderObject(m_RenderObject);
								m_Flag[i] &= -5;
							}
						}
					}
				}

				public void next_animation()
				{
					for (int i = 0; i < 32; i++)
					{
						if (m_Flag[i] != 0 && (m_Flag[i] & 2) != 0)
						{
							m_Animation[i].next();
						}
					}
				}

				public void setCurrentFrame(uint frame)
				{
					for (int i = 0; i < 32; i++)
					{
						if ((m_Flag[i] & 2) != 0)
						{
							m_Animation[i].setFrame(frame);
							break;
						}
					}
				}

				public uint getCurrentFrame()
				{
					for (int i = 0; i < 32; i++)
					{
						if ((m_Flag[i] & 2) != 0)
						{
							return m_Animation[i].getFrame();
						}
					}
					return 0u;
				}

				public uint getMaxFrame()
				{
					for (int i = 0; i < 32; i++)
					{
						if ((m_Flag[i] & 2) != 0)
						{
							return m_Animation[i].getMaxFrame();
						}
					}
					return 0u;
				}

				public uint getIndex()
				{
					return (uint)m_AnimationIndex;
				}

				public void setPause(bool flag)
				{
					if (flag)
					{
						m_GlobalFlag |= 16;
					}
					else
					{
						m_GlobalFlag &= -17;
					}
				}

				public bool isPause()
				{
					if ((m_GlobalFlag & 0x10) != 0)
					{
						return true;
					}
					return false;
				}

				public void setFrameRate(int fps)
				{
					m_FrameRate = fps;
					for (int i = 0; i < 32; i++)
					{
						if (m_Flag[i] != 0)
						{
							m_Animation[i].setFrameRate(m_FrameRate);
						}
						m_Flag[i] = 0;
					}
				}

				public int getFrameRate()
				{
					return m_FrameRate;
				}

				public void addRenderObject(NNSG3dRenderObj rdObj)
				{
					m_RenderObject = rdObj;
				}

				public bool isEndOfMotion()
				{
					return m_Animation[m_AnimationIndex].isEndOfMotion();
				}

				public void setType(int type)
				{
					m_Type = type;
				}
			}
		}
	}
}
