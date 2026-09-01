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
	public class NNSG3dResMdlSet
	{
		public NNSG3dResDataBlockHeader header;

		public NNSG3dResDict dict;

		public NNSG3dResMdl[] mdl;

		public static explicit operator NNSG3dResMdlSet(ArrayReader src)
		{
			NNSG3dResMdlSet nNSG3dResMdlSet = new NNSG3dResMdlSet();
			long position = src.getPosition();
			nNSG3dResMdlSet.header = (NNSG3dResDataBlockHeader)src;
			nNSG3dResMdlSet.dict = (NNSG3dResDict)src;
			nNSG3dResMdlSet.mdl = new NNSG3dResMdl[nNSG3dResMdlSet.dict.numEntry];
			for (int i = 0; i < nNSG3dResMdlSet.dict.numEntry; i++)
			{
				src.setPosition(position + nNSG3dResMdlSet.dict.entry.getU32(i));
				nNSG3dResMdlSet.mdl[i] = (NNSG3dResMdl)src;
			}
			return nNSG3dResMdlSet;
		}
	}
}
