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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	public static partial class eld
	{
		public class EmmitController
		{
			private delegate int _angle(int index);

			private _angle[] angle = new _angle[3];

			private ds.Vector3<int> _evBase;

			private ds.Vector3<int> _evRand;

			public void initialize(EmmitSetup setup)
			{
				_evBase = setup.vEmmitAngle;
				_evRand = _evBase;
				EffMulVectorToScalar(_evRand, 8192);
				_evBase.neg();
				angle[0] = ((_evRand.vx != 0) ? new _angle(angleRand) : new _angle(angleBase));
				angle[1] = ((_evRand.vy != 0) ? new _angle(angleRand) : new _angle(angleBase));
				angle[2] = ((_evRand.vz != 0) ? new _angle(angleRand) : new _angle(angleBase));
			}

			public void getEmmitAngle(ds.Vector3<int> agl)
			{
				agl.set(angle[0](0), angle[1](1), angle[2](2));
			}

			public void getEmmitTransform(MtxFx43 trans)
			{
				ds.Vector3<int> vector = new ds.Vector3<int>();
				getEmmitAngle(vector);
				EffLoadIdentity(trans);
				EffSetRotation(trans, vector);
			}

			private int angleBase(int index)
			{
				return 0;
			}

			private int angleRand(int index)
			{
				return _evBase[index] + ExecRand(_evRand[index]);
			}
		}
	}
}
