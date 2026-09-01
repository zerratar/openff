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
	public class NNSG2dImageAttr
	{
		public GXTexSizeS sizeS;

		public GXTexSizeT sizeT;

		public GXTexFmt fmt;

		public int bExtendedPlt;

		public int plttUse;

		public int mappingType;

		public void copy(NNSG2dImageAttr src)
		{
			sizeS = src.sizeS;
			sizeT = src.sizeT;
			fmt = src.fmt;
			bExtendedPlt = src.bExtendedPlt;
			plttUse = src.plttUse;
			mappingType = src.mappingType;
		}

		public void setDefault()
		{
			sizeS = GXTexSizeS.GX_TEXSIZE_S8;
			sizeT = GXTexSizeT.GX_TEXSIZE_T8;
			fmt = GXTexFmt.GX_TEXFMT_NONE;
			bExtendedPlt = 0;
			plttUse = 0;
			mappingType = 0;
		}
	}
}
