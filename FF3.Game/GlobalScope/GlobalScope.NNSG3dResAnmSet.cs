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
	public class NNSG3dResAnmSet
	{
		public NNSG3dResDataBlockHeader header;

		public NNSG3dResDict dict;

		public NNSG3dResAnmHeader[] anm;

		public static explicit operator NNSG3dResAnmSet(ArrayReader src)
		{
			NNSG3dResAnmSet nNSG3dResAnmSet = new NNSG3dResAnmSet();
			long position = src.getPosition();
			nNSG3dResAnmSet.header = (NNSG3dResDataBlockHeader)src;
			nNSG3dResAnmSet.dict = (NNSG3dResDict)src;
			nNSG3dResAnmSet.anm = new NNSG3dResAnmHeader[nNSG3dResAnmSet.dict.numEntry];
			for (int i = 0; i < nNSG3dResAnmSet.dict.numEntry; i++)
			{
				src.setPosition(position + nNSG3dResAnmSet.dict.entry.getU32(i));
				nNSG3dResAnmSet.anm[i] = createAnmRes(src);
			}
			return nNSG3dResAnmSet;
		}
	}
}
