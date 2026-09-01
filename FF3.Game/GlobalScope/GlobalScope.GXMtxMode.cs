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
	public enum GXMtxMode
	{
		GX_MTXMODE_PROJECTION,
		GX_MTXMODE_POSITION,
		GX_MTXMODE_POSITION_VECTOR,
		GX_MTXMODE_TEXTURE
	}
}
