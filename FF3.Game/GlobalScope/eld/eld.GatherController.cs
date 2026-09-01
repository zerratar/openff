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
		public class GatherController
		{
			private ds.Vector3<int> _vRotate = new ds.Vector3<int>();

			private int _nSpeedPow;

			private int _nSpeedAdd;

			public void initialize(GatherSetup setup)
			{
				_vRotate.set(WIN_RAD_TO_DEG(WIN_FX32_TO_F32(setup.vRotate.vx)), WIN_RAD_TO_DEG(WIN_FX32_TO_F32(setup.vRotate.vy)), WIN_RAD_TO_DEG(WIN_FX32_TO_F32(setup.vRotate.vz)));
				_nSpeedPow = setup.nSpeedPow;
				_nSpeedAdd = setup.nSpeedAdd;
			}

			public void calculateGatherInfo(ds.Vector3<int> vSpeed, ds.Vector3<int> vAdd, out int eLimitLength, ds.Vector3<int> pos)
			{
				vSpeed.copy(pos);
				vSpeed.neg();
				eLimitLength = EffVectorLength(vSpeed);
				EffVectorNormalize(vSpeed);
				vAdd.copy(vSpeed);
				EffMulVectorToScalar(vAdd, WIN_FX32_TO_F32(_nSpeedAdd));
				EffMulVectorToScalar(vSpeed, WIN_FX32_TO_F32(_nSpeedPow));
			}

			public void createRotateMatrix(MtxFx43 mtxRotate)
			{
				ds.Vector3<int> vector = new ds.Vector3<int>();
				vector.set((_vRotate.vx != 0) ? ExecRand(_vRotate.vx) : 0, (_vRotate.vy != 0) ? ExecRand(_vRotate.vy) : 0, (_vRotate.vz != 0) ? ExecRand(_vRotate.vz) : 0);
				EffSetRotation(mtxRotate, vector);
			}
		}
	}
}
