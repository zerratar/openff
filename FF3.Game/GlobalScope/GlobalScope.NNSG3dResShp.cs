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
	public class NNSG3dResShp
	{
		public NNSG3dResDict dict;

		public NNSG3dResShpData[] shp;

		public static explicit operator NNSG3dResShp(ArrayReader src)
		{
			NNSG3dResShp nNSG3dResShp = new NNSG3dResShp();
			long position = src.getPosition();
			nNSG3dResShp.dict = (NNSG3dResDict)src;
			src.getPosition();
			nNSG3dResShp.shp = new NNSG3dResShpData[nNSG3dResShp.dict.numEntry];
			for (int i = 0; i < nNSG3dResShp.dict.numEntry; i++)
			{
				src.setPosition(position + nNSG3dResShp.dict.entry.getU32(i));
				nNSG3dResShp.shp[i] = (NNSG3dResShpData)src;
			}
			return nNSG3dResShp;
		}

		public void preBuild(int scale)
		{
			for (int i = 0; i < dict.numEntry; i++)
			{
				shp[i].preBuild(scale);
			}
		}
	}
}
