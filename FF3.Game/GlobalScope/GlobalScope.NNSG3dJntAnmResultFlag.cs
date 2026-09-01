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
	public enum NNSG3dJntAnmResultFlag
	{
		NNS_G3D_JNTANM_RESULTFLAG_SCALE_ONE = 1,
		NNS_G3D_JNTANM_RESULTFLAG_ROT_ZERO = 2,
		NNS_G3D_JNTANM_RESULTFLAG_TRANS_ZERO = 4,
		NNS_G3D_JNTANM_RESULTFLAG_SCALEEX0_ONE = 8,
		NNS_G3D_JNTANM_RESULTFLAG_SCALEEX1_ONE = 0x10,
		NNS_G3D_JNTANM_RESULTFLAG_MAYA_SSC = 0x20
	}
}
