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
	private class NNSG3dUtilResName
	{
		private string _0;

		public NNSG3dResName resName
		{
			get
			{
				return new NNSG3dResName(_0);
			}
			set
			{
				_0 = value.name;
			}
		}

		public NNSG3dUtilResName(string arg0)
		{
			_0 = arg0;
		}
	}
}
