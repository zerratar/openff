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
	public static partial class ds
	{
		public delegate Array _g_pVXAllocFunc(uint size);

		public delegate void _g_pVXFreeFunc(Array arg0);

		public delegate Array _g_pSoundAllocFunc(uint size);

		public delegate void _g_pSoundFreeFunc(Array arg0);

	}
}
