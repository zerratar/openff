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
	public enum GXTexFmt
	{
		GX_TEXFMT_NONE,
		GX_TEXFMT_A3I5,
		GX_TEXFMT_PLTT4,
		GX_TEXFMT_PLTT16,
		GX_TEXFMT_PLTT256,
		GX_TEXFMT_COMP4x4,
		GX_TEXFMT_A5I3,
		GX_TEXFMT_DIRECT
	}
}
