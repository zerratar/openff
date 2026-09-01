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
		public class RangeController
		{
			private delegate int _position(int index);

			private _position[] position = new _position[3];

			private ds.Vector3<int> _evBase = new ds.Vector3<int>();

			private ds.Vector3<int> _evRand = new ds.Vector3<int>();

			public void initialize(RangeSetup setup)
			{
				_evBase.set(WIN_FX32_TO_F32(setup.vRangeBirth.vx), WIN_FX32_TO_F32(setup.vRangeBirth.vy), WIN_FX32_TO_F32(setup.vRangeBirth.vz));
				_evRand.copy(_evBase);
				_evRand *= 2;
				_evBase.neg();
				position[0] = ((_evRand.vx != 0) ? new _position(positionRand) : new _position(positionBase));
				position[1] = ((_evRand.vy != 0) ? new _position(positionRand) : new _position(positionBase));
				position[2] = ((_evRand.vz != 0) ? new _position(positionRand) : new _position(positionBase));
			}

			public void getCreatePosition(ds.Vector3<int> pos)
			{
				pos.set(position[0](0), position[1](1), position[2](2));
			}

			public void getCreatePosition(ds.pt.Particle prim)
			{
				prim.Center[0] = position[0](0);
				prim.Center[1] = position[1](1);
				prim.Center[2] = position[2](2);
			}

			public void getCreatePosition(ds.pt.LargeParticle prim)
			{
				prim.Center[0] = position[0](0);
				prim.Center[1] = position[1](1);
				prim.Center[2] = position[2](2);
			}

			private int positionBase(int index)
			{
				return 0;
			}

			private int positionRand(int index)
			{
				return _evBase[index] + ExecRand(_evRand[index]);
			}
		}
	}
}
