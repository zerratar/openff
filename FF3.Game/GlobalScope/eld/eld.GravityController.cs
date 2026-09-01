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
		public class GravityController
		{
			private delegate void _gravity(ds.Vector3<int> grav);

			private _gravity gravity;

			private int _enPower;

			private int _enRand;

			private ds.Vector3<int> _evDir = new ds.Vector3<int>();

			public void initialize(GravitySetup setup)
			{
				gravity = ((setup.nGravityRand != 0) ? new _gravity(gravityRand) : new _gravity(gravityBase));
				_enPower = WIN_FX32_TO_F32(setup.nGravityPow);
				_enRand = WIN_FX32_TO_F32(setup.nGravityRand);
				_evDir.set(WIN_FX32_TO_F32(setup.vGravityDir.vx), WIN_FX32_TO_F32(setup.vGravityDir.vy), WIN_FX32_TO_F32(setup.vGravityDir.vz));
				EffMulVectorToScalar(_evDir, _enPower + ((_enRand != 0) ? ExecRand(_enRand) : 0));
			}

			public void getGravity(ds.Vector3<int> grav)
			{
				gravity(grav);
			}

			public void gravityBase(ds.Vector3<int> grav)
			{
				grav.copy(_evDir);
				EffMulVectorToScalar(grav, _enPower);
			}

			public void gravityRand(ds.Vector3<int> grav)
			{
				grav.copy(_evDir);
				EffMulVectorToScalar(grav, _enPower + ((_enRand != 0) ? ExecRand(_enRand) : 0));
			}

			public GravityController(GravitySetup setup)
			{
				initialize(setup);
			}
		}
	}
}
