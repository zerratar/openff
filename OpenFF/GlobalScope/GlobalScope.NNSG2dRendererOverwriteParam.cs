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
	public enum NNSG2dRendererOverwriteParam
	{
		NNS_G2D_RND_OVERWRITE_NONE = 0,
		NNS_G2D_RND_OVERWRITE_PRIORITY = 1,
		NNS_G2D_RND_OVERWRITE_PLTTNO = 2,
		NNS_G2D_RND_OVERWRITE_MOSAIC = 4,
		NNS_G2D_RND_OVERWRITE_OBJMODE = 8,
		NNS_G2D_RND_OVERWRITE_PLTTNO_OFFS = 16,
		NNS_G2D_RND_OVERWRITE_MAX = 17
	}
}
