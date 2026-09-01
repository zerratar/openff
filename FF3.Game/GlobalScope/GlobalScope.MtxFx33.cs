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
	public class MtxFx33
	{
		public int _00;

		public int _01;

		public int _02;

		public int _10;

		public int _11;

		public int _12;

		public int _20;

		public int _21;

		public int _22;

		public MtxFx33()
		{
		}

		public MtxFx33(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8)
		{
			_00 = arg0;
			_01 = arg1;
			_02 = arg2;
			_10 = arg3;
			_11 = arg4;
			_12 = arg5;
			_20 = arg6;
			_21 = arg7;
			_22 = arg8;
		}

		public void copy(MtxFx33 src)
		{
			_00 = src._00;
			_01 = src._01;
			_02 = src._02;
			_10 = src._10;
			_11 = src._11;
			_12 = src._12;
			_20 = src._20;
			_21 = src._21;
			_22 = src._22;
		}

		public void copy(MtxFx43 src)
		{
			_00 = src._00;
			_01 = src._01;
			_02 = src._02;
			_10 = src._10;
			_11 = src._11;
			_12 = src._12;
			_20 = src._20;
			_21 = src._21;
			_22 = src._22;
		}
	}
}
