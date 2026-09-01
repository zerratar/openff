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
	public class NNSG3dResNodeInfo
	{
		public NNSG3dResDict dict;

		public uint[][] p;

		public static explicit operator NNSG3dResNodeInfo(ArrayReader src)
		{
			NNSG3dResNodeInfo nNSG3dResNodeInfo = new NNSG3dResNodeInfo();
			long position = src.getPosition();
			nNSG3dResNodeInfo.dict = (NNSG3dResDict)src;
			nNSG3dResNodeInfo.p = new uint[nNSG3dResNodeInfo.dict.numEntry][];
			for (int i = 0; i < nNSG3dResNodeInfo.dict.numEntry; i++)
			{
				src.setPosition(position + nNSG3dResNodeInfo.dict.entry.getU32(i));
				nNSG3dResNodeInfo.p[i] = new uint[16];
				src.read(nNSG3dResNodeInfo.p[i], 0, 16);
			}
			return nNSG3dResNodeInfo;
		}
	}
}
