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
	public class TexVramList
	{
		public uint tex;

		public uint fmt;

		public int @ref;

		public NNSG3dResTex pTex;

		public uint[] dictTexData;

		/// <summary>PORT: the texture's name in its package's dictionary, so a palette rebind (NNS_G3dBindMdlPltt) can find a mod's PNG for it again.</summary>
		public string texName;

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
