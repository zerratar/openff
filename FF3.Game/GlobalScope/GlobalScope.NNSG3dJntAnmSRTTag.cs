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
	public enum NNSG3dJntAnmSRTTag
	{
		NNS_G3D_JNTANM_SRTINFO_IDENTITY = 1,
		NNS_G3D_JNTANM_SRTINFO_IDENTITY_T = 2,
		NNS_G3D_JNTANM_SRTINFO_BASE_T = 4,
		NNS_G3D_JNTANM_SRTINFO_CONST_TX = 8,
		NNS_G3D_JNTANM_SRTINFO_CONST_TY = 16,
		NNS_G3D_JNTANM_SRTINFO_CONST_TZ = 32,
		NNS_G3D_JNTANM_SRTINFO_IDENTITY_R = 64,
		NNS_G3D_JNTANM_SRTINFO_BASE_R = 128,
		NNS_G3D_JNTANM_SRTINFO_CONST_R = 256,
		NNS_G3D_JNTANM_SRTINFO_IDENTITY_S = 512,
		NNS_G3D_JNTANM_SRTINFO_BASE_S = 1024,
		NNS_G3D_JNTANM_SRTINFO_CONST_SX = 2048,
		NNS_G3D_JNTANM_SRTINFO_CONST_SY = 4096,
		NNS_G3D_JNTANM_SRTINFO_CONST_SZ = 8192,
		NNS_G3D_JNTANM_SRTINFO_NODE_MASK = -16777216,
		NNS_G3D_JNTANM_SRTINFO_NODE_SHIFT = 24
	}
}
