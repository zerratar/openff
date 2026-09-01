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
	public static partial class eld
	{
		public class SpeedController
		{
			private delegate void _speed(ds.Vector3<int> spd);

			private _speed speed;

			private int _enPower;

			private int _enRand;

			private ds.Vector3<int> _evDir = new ds.Vector3<int>();

			public void initialize(SpeedSetup setup)
			{
				speed = ((setup.nSpeedRand != 0) ? new _speed(speedRand) : new _speed(speedBase));
				_enPower = WIN_FX32_TO_F32(setup.nSpeedPow);
				_enRand = WIN_FX32_TO_F32(setup.nSpeedRand);
				_evDir.set(WIN_FX32_TO_F32(setup.vSpeedDir.vx), WIN_FX32_TO_F32(setup.vSpeedDir.vy), WIN_FX32_TO_F32(setup.vSpeedDir.vz));
			}

			public void getSpeed(ds.Vector3<int> spd)
			{
				speed(spd);
			}

			public void getSpeed(ds.Vector3<int> spd, MtxFx43 mxRot)
			{
				speed(spd);
				EffMulVectorToMatrix(spd, mxRot);
			}

			public void speedBase(ds.Vector3<int> spd)
			{
				spd.copy(_evDir);
				EffMulVectorToScalar(spd, _enPower);
			}

			public void speedRand(ds.Vector3<int> spd)
			{
				spd.copy(_evDir);
				EffMulVectorToScalar(spd, _enPower + ((_enRand != 0) ? ExecRand(_enRand) : 0));
			}
		}
	}
}
