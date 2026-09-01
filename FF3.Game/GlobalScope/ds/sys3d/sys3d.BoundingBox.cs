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
		public static partial class sys3d
		{
			public class BoundingBox
			{
				public VecFx16 pos;

				public VecFx16 size;

				public int scale;

				public void set(VecFx16 Pos, VecFx16 Size, int scl)
				{
					pos = Pos;
					size = Size;
					scale = scl;
				}

				public void draw()
				{
					G3_PushMtx();
					G3_Translate(0, 0, 0);
					G3_MaterialColorDiffAmb(GX_RGB(31, 31, 31), GX_RGB(16, 16, 16), 1);
					G3_MaterialColorSpecEmi(GX_RGB(16, 16, 16), GX_RGB(0, 0, 0), 0);
					G3_PolygonAttr(0, GXPolygonMode.GX_POLYGONMODE_MODULATE, GXCull.GX_CULL_BACK, 0, 31, 0);
					G3_Begin(GXBegin.GX_BEGIN_QUADS);
					quad(0, 1, 3, 2);
					quad(4, 5, 7, 6);
					G3_End();
					G3_PopMtx(1);
				}
			}
		}
	}
}
