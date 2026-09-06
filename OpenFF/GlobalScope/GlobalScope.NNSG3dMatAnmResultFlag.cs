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
	public enum NNSG3dMatAnmResultFlag
	{
		NNS_G3D_MATANM_RESULTFLAG_TEXMTX_SCALEONE = 1,
		NNS_G3D_MATANM_RESULTFLAG_TEXMTX_ROTZERO = 2,
		NNS_G3D_MATANM_RESULTFLAG_TEXMTX_TRANSZERO = 4,
		NNS_G3D_MATANM_RESULTFLAG_TEXMTX_SET = 8,
		NNS_G3D_MATANM_RESULTFLAG_TEXMTX_MULT = 0x10,
		NNS_G3D_MATANM_RESULTFLAG_WIREFRAME = 0x20
	}
}
