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
	public enum NNSG2dVerticalOrigin
	{
		NNS_G2D_VERTICALORIGIN_TOP = 1,
		NNS_G2D_VERTICALORIGIN_MIDDLE = 2,
		NNS_G2D_VERTICALORIGIN_BOTTOM = 4
	}
}
