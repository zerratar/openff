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
		public class ParticleGatherDS : IParticle
		{
			public ds.Vector3<int> _vSpeed = new ds.Vector3<int>();

			public ds.Vector3<int> _vAdd = new ds.Vector3<int>();

			public MtxFx43 _mtxRotate = new MtxFx43();

			public int _eLimitLength;

			public int _eLength;

			public bool _bGather;

			~ParticleGatherDS()
			{
				destruct();
			}

			public void destruct()
			{
			}

			public override void update(IGroup group)
			{
				base.update(group);
			}

			public void update(IGroup group, GatherController ctrlGather, spr.Eff_FRGBA color)
			{
				base.update(group);
				if (_bGather)
				{
					_pPrimitive.Center[0] += _vSpeed.vx;
					_pPrimitive.Center[1] += _vSpeed.vy;
					_pPrimitive.Center[2] += _vSpeed.vz;
					_eLength += EffVectorLength(_vSpeed);
					if (_eLength >= _eLimitLength)
					{
						ds.Vector3<int> center = _pPrimitive.Center;
						ds.Vector3<int> center2 = _pPrimitive.Center;
						int num = (_pPrimitive.Center[2] = 0);
						int value = (center2[1] = num);
						center[0] = value;
						_bGather = false;
					}
					else
					{
						_vSpeed += _vAdd;
						EffMulVectorToMatrix(_vSpeed, _mtxRotate);
						EffMulVectorToMatrix(_vAdd, _mtxRotate);
						EffMulVectorToMatrix(_pPrimitive.Center, _mtxRotate);
					}
				}
				ut.setColorToPrimitive(_pPrimitive, color);
				_pPrimitive.Disp = (short)((_pPrimitive.Color[3] != 0) ? 2 : 0);
			}
		}
	}
}
