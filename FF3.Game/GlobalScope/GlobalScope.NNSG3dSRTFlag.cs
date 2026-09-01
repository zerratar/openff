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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public enum NNSG3dSRTFlag
	{
		NNS_G3D_SRTFLAG_TRANS_ZERO = 1,
		NNS_G3D_SRTFLAG_ROT_ZERO = 2,
		NNS_G3D_SRTFLAG_SCALE_ONE = 4,
		NNS_G3D_SRTFLAG_PIVOT_EXIST = 8,
		NNS_G3D_SRTFLAG_IDXPIVOT_MASK = 240,
		NNS_G3D_SRTFLAG_PIVOT_MINUS = 256,
		NNS_G3D_SRTFLAG_SIGN_REVC = 512,
		NNS_G3D_SRTFLAG_SIGN_REVD = 1024,
		NNS_G3D_SRTFLAG_IDXMTXSTACK_MASK = 63488,
		NNS_G3D_SRTFLAG_IDENTITY = 7,
		NNS_G3D_SRTFLAG_IDXPIVOT_SHIFT = NNS_G3D_SRTFLAG_SCALE_ONE,
		NNS_G3D_SRTFLAG_IDXMTXSTACK_SHIFT = 11
	}
}
