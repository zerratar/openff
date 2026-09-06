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
	public enum NNSG2dAnimationPlayMode
	{
		NNS_G2D_ANIMATIONPLAYMODE_INVALID,
		NNS_G2D_ANIMATIONPLAYMODE_FORWARD,
		NNS_G2D_ANIMATIONPLAYMODE_FORWARD_LOOP,
		NNS_G2D_ANIMATIONPLAYMODE_REVERSE,
		NNS_G2D_ANIMATIONPLAYMODE_REVERSE_LOOP,
		NNS_G2D_ANIMATIONPLAYMODE_MAX
	}
}
