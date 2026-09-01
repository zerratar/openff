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
	public static partial class dgs
	{
		public class CRestrictor : DGSLinkedList<CRestrictor>
		{
			private bool m_bActivity;

			private mcl.CMapCollision m_Collision;

			public CRestrictor()
			{
				m_Collision = null;
				m_bActivity = false;
			}

			public void rorAppend(mcl.CMapCollision _obj)
			{
				m_Collision = _obj;
				if (m_Collision != null)
				{
					rorSetActivity(_b: true);
				}
			}

			public void rorRemove()
			{
				m_Collision = null;
				dgsllUnlink();
				rorSetActivity(_b: false);
			}

			public bool rorEvaluateArrow(VecFx32 _position, VecFx32 _direction, int _length, int mat, mcl.CollisionResult _result)
			{
				for (int i = 0; i < m_Collision.getNumberOfObject(); i++)
				{
					mcl.CObject cObject = const_cast<mcl.CObject>(m_Collision.getObject((uint)i));
					if (cObject.evaluateArrow(_position, _direction, _length, mat, _result))
					{
						return true;
					}
				}
				return false;
			}

			public bool rorEvaluateArrow2(VecFx32 pos, VecFx32 dir, int len, int[] matList, byte matNum, mcl.CollisionResult ret)
			{
				for (int i = 0; i < m_Collision.getNumberOfObject(); i++)
				{
					mcl.CObject cObject = const_cast<mcl.CObject>(m_Collision.getObject((uint)i));
					if (cObject.evaluateArrow2(pos, dir, len, matList, matNum, ret))
					{
						return true;
					}
				}
				return false;
			}

			public bool rorEvaluateSphere(VecFx32 _center, VecFx32 _dir, int _radius, int mat, mcl.CollisionResult _result)
			{
				for (int i = 0; i < m_Collision.getNumberOfObject(); i++)
				{
					mcl.CObject cObject = const_cast<mcl.CObject>(m_Collision.getObject((uint)i));
					if (cObject.evaluateSphere(_center, _dir, _radius, mat, _result))
					{
						return true;
					}
				}
				return false;
			}

			public bool rorEvaluateSphere2(VecFx32 center, VecFx32 prePos, VecFx32 dir, int radius, int[] matList, byte matNum, mcl.CollisionResult ret)
			{
				for (int i = 0; i < m_Collision.getNumberOfObject(); i++)
				{
					mcl.CObject cObject = const_cast<mcl.CObject>(m_Collision.getObject((uint)i));
					if (cObject.evaluateSphere2(center, prePos, dir, radius, matList, matNum, ret))
					{
						return true;
					}
				}
				return false;
			}

			public bool rorEvaluateCapsule(VecFx32 pos, VecFx32 nextPos, int rad, int mat, mcl.CollisionResult ret)
			{
				VecFx32 vecFx = new VecFx32(0, 0, 0);
				VEC_Subtract(nextPos, pos, vecFx);
				VEC_Mag(vecFx);
				VEC_Normalize(vecFx, vecFx);
				for (int i = 0; i < m_Collision.getNumberOfObject(); i++)
				{
					mcl.CObject cObject = const_cast<mcl.CObject>(m_Collision.getObject((uint)i));
					if (cObject.evaluateCapsule(pos, nextPos, rad, mat, ret))
					{
						return true;
					}
				}
				return false;
			}

			public bool rorActivity()
			{
				return m_bActivity;
			}

			public void rorSetActivity(bool _b)
			{
				if (_b)
				{
					if (!m_bActivity)
					{
						dgsllLink();
					}
				}
				else if (m_bActivity)
				{
					dgsllUnlink();
				}
				m_bActivity = _b;
			}
		}
	}
}
