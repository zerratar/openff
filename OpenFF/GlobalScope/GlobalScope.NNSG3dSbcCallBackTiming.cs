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
	public enum NNSG3dSbcCallBackTiming
	{
		NNS_G3D_SBC_CALLBACK_TIMING_NONE,
		NNS_G3D_SBC_CALLBACK_TIMING_A,
		NNS_G3D_SBC_CALLBACK_TIMING_B,
		NNS_G3D_SBC_CALLBACK_TIMING_C
	}
}
