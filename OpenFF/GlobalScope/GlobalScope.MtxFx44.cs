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
	public class MtxFx44
	{
		public int _00;

		public int _01;

		public int _02;

		public int _03;

		public int _10;

		public int _11;

		public int _12;

		public int _13;

		public int _20;

		public int _21;

		public int _22;

		public int _23;

		public int _30;

		public int _31;

		public int _32;

		public int _33;

		public MtxFx44()
		{
		}

		public MtxFx44(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14, int arg15)
		{
			_00 = arg0;
			_01 = arg1;
			_02 = arg2;
			_03 = arg3;
			_10 = arg4;
			_11 = arg5;
			_12 = arg6;
			_13 = arg7;
			_20 = arg8;
			_21 = arg9;
			_22 = arg10;
			_23 = arg11;
			_30 = arg12;
			_31 = arg13;
			_32 = arg14;
			_33 = arg15;
		}

		public MtxFx44(ds.Vector4<int>[] arg0, int arg1)
		{
			ds.Vector4<int> vector = arg0[arg1];
			_00 = vector.vx;
			_01 = vector.vy;
			_02 = vector.vz;
			_03 = vector.vw;
			vector = arg0[arg1 + 1];
			_10 = vector.vx;
			_11 = vector.vy;
			_12 = vector.vz;
			_13 = vector.vw;
			vector = arg0[arg1 + 2];
			_20 = vector.vx;
			_21 = vector.vy;
			_22 = vector.vz;
			_23 = vector.vw;
			vector = arg0[arg1 + 3];
			_30 = vector.vx;
			_31 = vector.vy;
			_32 = vector.vz;
			_33 = vector.vw;
		}

		public void copy(MtxFx44 src)
		{
			_00 = src._00;
			_01 = src._01;
			_02 = src._02;
			_03 = src._03;
			_10 = src._10;
			_11 = src._11;
			_12 = src._12;
			_13 = src._13;
			_20 = src._20;
			_21 = src._21;
			_22 = src._22;
			_23 = src._23;
			_30 = src._30;
			_31 = src._31;
			_32 = src._32;
			_33 = src._33;
		}
	}
}
