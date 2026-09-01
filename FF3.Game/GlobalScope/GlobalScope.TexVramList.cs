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
	public class TexVramList
	{
		public uint tex;

		public uint fmt;

		public int @ref;

		public NNSG3dResTex pTex;

		public uint[] dictTexData;

		public TexVramList()
		{
			tex = 0u;
			fmt = 0u;
			@ref = 1;
			pTex = null;
			dictTexData = null;
		}

		public void destruct()
		{
		}

		public void release()
		{
			if (--@ref == 0)
			{
				if (tex != 0)
				{
					DeleteTexture(tex);
					texCount--;
				}
				destruct();
			}
		}
	}
}
