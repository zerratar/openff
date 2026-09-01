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
	public static partial class ds
	{
		public static partial class sys3d
		{
			public class ILodImplement
			{
				public const int MaxNumRegisterUserData = 4;

				protected ushort _unLevel;

				protected ushort _unLevelMax;

				protected uint _unDistance;

				protected Array[] _pUserData = new Array[4];

				public ILodImplement()
				{
					_unLevel = 0;
					_unLevelMax = 0;
				}

				~ILodImplement()
				{
				}

				public bool setup(ushort unLevelMax, uint unDistance)
				{
					_unDistance = unDistance;
					_unLevelMax = unLevelMax;
					_unLevel = 0;
					return true;
				}

				public ushort calculateDistanceLevel(CRenderObject pObj)
				{
					VecFx32 v = new VecFx32(NNS_G3dGlbGetCameraPos());
					VecFx32 vecFx = new VecFx32();
					pObj.getPosition(vecFx);
					int numer = VEC_Distance(v, vecFx);
					numer = FX_Div(numer, (int)_unDistance) >> 12;
					return (ushort)((numer >= _unLevelMax) ? (_unLevelMax - 1) : numer);
				}

				public void setUserData<T>(uint index, T pUserData)
				{
					if (index < 4)
					{
						_pUserData[index] = reinterpret_cast<Array>(pUserData);
					}
				}

				public T getUserData<T>(uint index)
				{
					if (index >= 4)
					{
						return default(T);
					}
					return reinterpret_cast<T>(_pUserData[index]);
				}

				public ushort getLevelMax()
				{
					return _unLevelMax;
				}

				public virtual void executeLod(CRenderObject pObj)
				{
				}
			}
		}
	}
}
