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
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class ds
	{
		public static partial class sys3d
		{
			public interface ITexture
			{
				void bindMdlSet(NNSG3dResMdlSet arg0);

				void bindMdl(NNSG3dResMdl arg0);

				void bindMdlToTex(NNSG3dResMdl arg0);

				void bindMdlToPltt(NNSG3dResMdl arg0);

				void bindMdlToTexByName(NNSG3dResMdl arg0, string arg1);

				void bindMdlToPlttByName(NNSG3dResMdl arg0, string arg1);

				void releaseMdlSet(NNSG3dResMdlSet arg0);

				void releaseMdl(NNSG3dResMdl arg0);

				void releaseMdlToTex(NNSG3dResMdl arg0);

				void releaseMdlToPltt(NNSG3dResMdl arg0);

				void releaseMdlToTexByName(NNSG3dResMdl arg0, string arg1);

				void releaseMdlToPlttByName(NNSG3dResMdl arg0, string arg1);
			}
		}
	}
}
