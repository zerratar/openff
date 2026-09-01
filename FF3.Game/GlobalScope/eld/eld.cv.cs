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
	public static partial class eld
	{
		public static class cv
		{
			public class Ferguson
			{
				public static MtxFx44 _hermite = new MtxFx44(8192, -8192, 4096, 4096, -12288, 12288, -8192, -4096, 0, 0, 4096, 0, 4096, 0, 0, 0);

				private ds.Vector4<int>[] m_vCtrlPoint;

				private uint m_uiNumCtrlPoint;

				private MtxFx44 m_matHermite;

				public bool getCurvePoint(ds.Vector3<int> pPos, int t, uint index, uint flag)
				{
					if (m_uiNumCtrlPoint - 1 < index)
					{
						return false;
					}
					index *= 4;
					if (flag == 0)
					{
						ds.Vector4<int> vector = new ds.Vector4<int>(m_vCtrlPoint[index + 1]);
						vector -= m_vCtrlPoint[index];
						EffMulVectorToScalar(vector, t);
						vector += m_vCtrlPoint[index];
						pPos.vx = vector.vx;
						pPos.vy = vector.vy;
						pPos.vz = vector.vz;
					}
					else
					{
						switch (t)
						{
						case 0:
							pPos.vx = m_vCtrlPoint[index].vx;
							pPos.vy = m_vCtrlPoint[index].vy;
							pPos.vz = m_vCtrlPoint[index].vz;
							break;
						case 4096:
							pPos.vx = m_vCtrlPoint[index + 1].vx;
							pPos.vy = m_vCtrlPoint[index + 1].vy;
							pPos.vz = m_vCtrlPoint[index + 1].vz;
							break;
						default:
							getCurvePoint(pPos, t, new MtxFx44(m_vCtrlPoint, (int)index));
							break;
						}
					}
					return true;
				}

				public void getCurvePoint(ds.Vector3<int> pos, int t, MtxFx44 mtxCtrl)
				{
					m_matHermite = _hermite;
					ds.Vector4<int> vector = new ds.Vector4<int>();
					vector.set(EffFxPow3(t), EffFxPow2(t), t, 4096);
					multVectorToMatrix(vector, vector, m_matHermite);
					multVectorToMatrix(vector, vector, mtxCtrl);
					pos.vx = vector.vx;
					pos.vy = vector.vy;
					pos.vz = vector.vz;
				}

				public void multVectorToMatrix(ds.Vector4<int> dest, ds.Vector4<int> vec, MtxFx44 m)
				{
					int vx = vec.vx;
					int vy = vec.vy;
					int vz = vec.vz;
					int vw = vec.vw;
					dest.vx = (int)((long)vx * (long)m._00 + (long)vy * (long)m._10 + (long)vz * (long)m._20 + (long)vw * (long)m._30 >> 12);
					dest.vy = (int)((long)vx * (long)m._01 + (long)vy * (long)m._11 + (long)vz * (long)m._21 + (long)vw * (long)m._31 >> 12);
					dest.vz = (int)((long)vx * (long)m._02 + (long)vy * (long)m._12 + (long)vz * (long)m._22 + (long)vw * (long)m._32 >> 12);
					dest.vw = (int)((long)vx * (long)m._03 + (long)vy * (long)m._13 + (long)vz * (long)m._23 + (long)vw * (long)m._33 >> 12);
				}

				public void RegisterCtrlPointArry(ds.Vector4<int>[] pCtrlPoint, uint NumCtrlPoint)
				{
					m_vCtrlPoint = pCtrlPoint;
					m_uiNumCtrlPoint = NumCtrlPoint;
				}
			}

			public class SPassPoint
			{
				private ds.Vector3<int> vPosition;

				private uint uiFrame;
			}
		}
	}
}
