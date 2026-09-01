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
		public enum enFLAG_PDS
		{
			enFLAG_LOOP = 1,
			enFLAG_AFTERIMAGE = 2,
			enFLAG_FADE = 4,
			enFLAG_MOVE_OFFSET = 8,
			enFLAG_XLU_DEPTH = 16,
			enFLAG_TEXOUT_AUTO = 32,
			enFLAG_TEXOUT_A5I3 = 64,
			enFLAG_TEXOUT_A3I5 = 128,
			enFLAG_END = 129
		}
	}
}
