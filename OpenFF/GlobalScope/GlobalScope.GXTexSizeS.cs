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
	public enum GXTexSizeS
	{
		GX_TEXSIZE_S8,
		GX_TEXSIZE_S16,
		GX_TEXSIZE_S32,
		GX_TEXSIZE_S64,
		GX_TEXSIZE_S128,
		GX_TEXSIZE_S256,
		GX_TEXSIZE_S512,
		GX_TEXSIZE_S1024
	}
}
