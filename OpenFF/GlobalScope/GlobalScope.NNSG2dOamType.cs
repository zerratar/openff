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
	public enum NNSG2dOamType
	{
		NNS_G2D_OAMTYPE_MAIN,
		NNS_G2D_OAMTYPE_SUB,
		NNS_G2D_OAMTYPE_SOFTWAREEMULATION,
		NNS_G2D_OAMTYPE_INVALID,
		NNS_G2D_OAMTYPE_MAX
	}
}
