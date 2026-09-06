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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	public enum NNSG2dHorizontalOrigin
	{
		NNS_G2D_HORIZONTALORIGIN_LEFT = 8,
		NNS_G2D_HORIZONTALORIGIN_CENTER = 0x10,
		NNS_G2D_HORIZONTALORIGIN_RIGHT = 0x20
	}
}
