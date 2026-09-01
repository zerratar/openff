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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class eld
	{
		public class ImpSequencePath
		{
			private SPathBodyHeader m_pBodyHeader;

			private ds.Vector3<int> m_vBasePos = new ds.Vector3<int>();

			private ds.Vector4<int>[] m_pPathArry;

			private ds.Vector4<int>[] m_pFigureArry;

			private IObject m_pObject;

			private uint[] m_pTimingArry;

			private uint m_NowIndex;

			private uint m_ElapsedTime;

			private float m_fNextTime;

			private cv.Ferguson _ferguson = new cv.Ferguson();

			private uint m_uiMoveMode;

			public ImpSequencePath()
			{
				m_pPathArry = null;
				m_pFigureArry = null;
				m_pTimingArry = null;
				m_pObject = null;
			}

			public IObject GetObject()
			{
				return m_pObject;
			}

			public void SetObject(IObject pObj)
			{
				m_pObject = pObj;
			}

			public void DeleteObject()
			{
				if (m_pObject != null)
				{
					m_pObject.DeleteObject();
					m_pObject = null;
				}
			}

			~ImpSequencePath()
			{
				destruct();
			}

			public void destruct()
			{
				DeleteObject();
			}

			public void SetData(SPathBodyHeader pData)
			{
				m_pBodyHeader = pData;
				m_pPathArry = pData.m_pPathArry;
				m_pFigureArry = pData.m_pFigureArry;
				m_pTimingArry = pData.m_pTimingArry;
			}

			public void initialize(IObject pObject, ds.Vector3<int> vBasePos)
			{
				m_NowIndex = 0u;
				m_ElapsedTime = 0u;
				m_uiMoveMode = 0u;
				m_pObject = pObject;
				setBasePosition(vBasePos);
				CalcNextTimeNormal();
			}

			public void Stop()
			{
				if (m_pObject != null)
				{
					m_pObject.Stop();
				}
			}

			public bool isPlay()
			{
				if (m_pObject == null)
				{
					return false;
				}
				return m_pObject.isPlay();
			}

			public void setBasePosition(ds.Vector3<int> pos)
			{
				m_vBasePos.copy(pos);
			}

			public void CalcNextTimeNormal()
			{
				if (m_NowIndex + 1 < ARRY2POINT_NUM(m_pBodyHeader.uiNumArryCount) - 1)
				{
					m_fNextTime = m_pTimingArry[m_NowIndex + 1] - m_pTimingArry[m_NowIndex];
				}
				else
				{
					m_fNextTime = m_pBodyHeader.uiFrameTime - m_pTimingArry[m_NowIndex];
				}
				m_ElapsedTime = 0u;
			}

			public void CalcNextTimeReverse()
			{
				if (m_NowIndex != 0)
				{
					m_fNextTime = m_pTimingArry[m_NowIndex] - m_pTimingArry[m_NowIndex - 1];
				}
				else
				{
					m_fNextTime = m_pTimingArry[m_NowIndex];
				}
				m_ElapsedTime = 0u;
			}

			public void AfterProcDecide()
			{
				switch (path.typeMove(m_pBodyHeader.uiFlag))
				{
				case 2u:
					m_uiMoveMode = 1u;
					m_ElapsedTime = 1u;
					m_fNextTime = 1f;
					m_NowIndex = ARRY2POINT_NUM(m_pBodyHeader.uiNumArryCount) - 1 - 1;
					break;
				case 4u:
					if (m_uiMoveMode == 0)
					{
						m_NowIndex = ARRY2POINT_NUM(m_pBodyHeader.uiNumArryCount) - 1;
						m_uiMoveMode = 2u;
					}
					else
					{
						m_NowIndex = 0u;
						m_uiMoveMode = 0u;
					}
					break;
				case 8u:
					m_NowIndex = 0u;
					m_uiMoveMode = 0u;
					break;
				default:
					m_NowIndex = 0u;
					m_ElapsedTime = uint.MaxValue;
					break;
				}
			}

			public void CalcNextTime()
			{
				switch (m_uiMoveMode)
				{
				case 0u:
					if (++m_NowIndex >= ARRY2POINT_NUM(m_pBodyHeader.uiNumArryCount) - 1)
					{
						AfterProcDecide();
					}
					break;
				case 2u:
					if (--m_NowIndex == 0)
					{
						AfterProcDecide();
					}
					break;
				}
				switch (m_uiMoveMode)
				{
				case 1u:
					AfterProcDecide();
					break;
				case 0u:
					CalcNextTimeNormal();
					break;
				case 2u:
					CalcNextTimeReverse();
					break;
				}
			}

			public void FigureUpdate(int t, uint index, MtxFx43 mat)
			{
			}

			public void update(MtxFx43 mat, ds.Vector3<int> scale)
			{
				if (m_ElapsedTime == uint.MaxValue)
				{
					return;
				}
				if (m_pBodyHeader.uiNumArryCount == 1)
				{
					updatePositionS(mat, scale);
					return;
				}
				if (m_uiMoveMode != 1)
				{
					m_ElapsedTime++;
				}
				int t = FX_Div((int)(m_ElapsedTime << 12), FX_F32_TO_FX32(m_fNextTime));
				updatePositionM(mat, t, scale);
				if (m_fNextTime <= (float)m_ElapsedTime)
				{
					CalcNextTime();
				}
			}

			public void updatePositionS(MtxFx43 mat, ds.Vector3<int> scale)
			{
				if (m_pBodyHeader.uiNumArryCount == 1)
				{
					ds.Vector3<int> vector = new ds.Vector3<int>();
					if (path.flagFigure(m_pBodyHeader.uiFlag) != 0)
					{
						FigureUpdate(0, 0u, mat);
					}
					if (m_pObject != null)
					{
						vector.set(m_pPathArry[0].vx, m_pPathArry[0].vy, m_pPathArry[0].vz);
						EffMulVectorToMatrix(vector, mat);
						vector += m_vBasePos;
						m_pObject.SetPosition(vector);
					}
				}
			}

			public void updatePositionM(MtxFx43 mat, int t, ds.Vector3<int> scale)
			{
				int num = 4096;
				if (m_pBodyHeader.uiNumArryCount == 1)
				{
					return;
				}
				ds.Vector3<int> vector = new ds.Vector3<int>();
				_ferguson.RegisterCtrlPointArry(m_pPathArry, ARRY2POINT_NUM(m_pBodyHeader.uiNumArryCount));
				if (m_uiMoveMode == 2)
				{
					_ferguson.getCurvePoint(vector, num - t, m_NowIndex - 1, path.typeLine(m_pBodyHeader.uiFlag));
				}
				else
				{
					_ferguson.getCurvePoint(vector, t, m_NowIndex, path.typeLine(m_pBodyHeader.uiFlag));
				}
				EffMulVectorToMatrix(vector, mat);
				if (path.flagFigure(m_pBodyHeader.uiFlag) != 0)
				{
					if (m_uiMoveMode == 2)
					{
						FigureUpdate(num - t, m_NowIndex - 1, mat);
					}
					else
					{
						FigureUpdate(t, m_NowIndex, mat);
					}
				}
				switch (path.typeCoord(m_pBodyHeader.uiFlag))
				{
				case 16u:
					vector += m_vBasePos;
					break;
				}
				if (m_pObject != null)
				{
					m_pObject.SetPosition(vector);
				}
			}
		}
	}
}
