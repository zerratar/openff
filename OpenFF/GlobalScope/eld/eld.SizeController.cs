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
		public class SizeController
		{
			private delegate int _size();

			private _size size;

			private int _enBase;

			private int _enRand;

			public void initialize(SizeSetup setup)
			{
				size = ((setup.nSizeRand != 0) ? new _size(sizeRand) : new _size(sizeBase));
				_enBase = WIN_FX32_TO_F32(setup.nSizeBase);
				_enRand = WIN_FX32_TO_F32(setup.nSizeRand);
			}

			public int getSize()
			{
				return size();
			}

			private int sizeBase()
			{
				return _enBase;
			}

			private int sizeRand()
			{
				return _enBase + ((_enRand != 0) ? ExecRand(_enRand) : 0);
			}
		}
	}
}
