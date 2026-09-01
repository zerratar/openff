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
	public static partial class ds
	{
		public static class tx
		{
			internal static void sendNoTextureParam()
			{
				G3_TexImageParam(GXTexFmt.GX_TEXFMT_NONE, 0, GXTexSizeS.GX_TEXSIZE_S8, GXTexSizeT.GX_TEXSIZE_T8, 0, 0, 0, null);
				G3_TexPlttBase(0u, GXTexFmt.GX_TEXFMT_NONE);
			}
		}
	}
}
