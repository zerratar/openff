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
	public enum NNSG2dVerticalAlign
	{
		NNS_G2D_VERTICALALIGN_TOP = 0x40,
		NNS_G2D_VERTICALALIGN_MIDDLE = 0x80,
		NNS_G2D_VERTICALALIGN_BOTTOM = 0x100
	}
}
