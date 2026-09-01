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
		public class FadeController
		{
			private ds.Vector4<float> _vColSub = new ds.Vector4<float>();

			private ds.Vector4<float> _vColFade = new ds.Vector4<float>();

			private ushort _usStart;

			private ushort _usEnd;

			public void initialize(FadeSetup setup)
			{
				_vColFade.set(setup.vColorSub.cr, setup.vColorSub.cg, setup.vColorSub.cb, setup.vColorSub.ca);
				_vColSub.copy(_vColFade);
				if (setup.usTime != 0)
				{
					_vColSub.cr /= (int)setup.usTime;
					_vColSub.cg /= (int)setup.usTime;
					_vColSub.cb /= (int)setup.usTime;
					_vColSub.ca /= (int)setup.usTime;
				}
				_usStart = setup.usStart;
				_usEnd = (ushort)(setup.usStart + setup.usTime);
			}

			public void getFadeColor(ds.Vector4<float> colFade, ushort usTime)
			{
				if (usTime < _usStart)
				{
					colFade.zero();
					return;
				}
				if (usTime >= _usEnd)
				{
					colFade.copy(_vColFade);
					return;
				}
				colFade.copy(_vColSub);
				colFade.cr *= usTime - _usStart;
				colFade.cg *= usTime - _usStart;
				colFade.cb *= usTime - _usStart;
				colFade.ca *= usTime - _usStart;
			}
		}
	}
}
