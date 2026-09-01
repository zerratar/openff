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
	public class NNSG3dResTex
	{
		public NNSG3dResDataBlockHeader header;

		public NNSG3dResTexInfo texInfo;

		public NNSG3dResTex4x4Info tex4x4Info;

		public NNSG3dResPlttInfo plttInfo;

		public NNSG3dResDict dict;

		public static explicit operator NNSG3dResTex(ArrayReader src)
		{
			NNSG3dResTex nNSG3dResTex = new NNSG3dResTex();
			long position = src.getPosition();
			nNSG3dResTex.header = (NNSG3dResDataBlockHeader)src;
			nNSG3dResTex.texInfo = (NNSG3dResTexInfo)src;
			nNSG3dResTex.tex4x4Info = (NNSG3dResTex4x4Info)src;
			nNSG3dResTex.plttInfo = (NNSG3dResPlttInfo)src;
			nNSG3dResTex.dict = (NNSG3dResDict)src;
			src.setPosition(position + nNSG3dResTex.texInfo.ofsDict);
			nNSG3dResTex.texInfo.dict = (NNSG3dResDict)src;
			src.setPosition(position + nNSG3dResTex.texInfo.ofsTex);
			int num = nNSG3dResTex.texInfo.sizeTex << 3;
			if (num != nNSG3dResTex.plttInfo.ofsPlttData - nNSG3dResTex.texInfo.ofsTex)
			{
				num = (int)(nNSG3dResTex.plttInfo.ofsPlttData - nNSG3dResTex.texInfo.ofsTex);
			}
			nNSG3dResTex.texInfo.tex = new ushort[num / 2];
			src.read(nNSG3dResTex.texInfo.tex, 0, num / 2);
			src.setPosition(position + nNSG3dResTex.tex4x4Info.ofsDict);
			nNSG3dResTex.tex4x4Info.dict = (NNSG3dResDict)src;
			src.setPosition(position + nNSG3dResTex.tex4x4Info.ofsTex);
			num = nNSG3dResTex.tex4x4Info.sizeTex << 1;
			nNSG3dResTex.tex4x4Info.tex = new uint[num];
			src.read(nNSG3dResTex.tex4x4Info.tex, 0, num);
			src.setPosition(position + nNSG3dResTex.tex4x4Info.ofsTexPlttIdx);
			num = nNSG3dResTex.tex4x4Info.sizeTex << 1;
			nNSG3dResTex.tex4x4Info.pal = new ushort[num];
			src.read(nNSG3dResTex.tex4x4Info.pal, 0, num);
			src.setPosition(position + nNSG3dResTex.plttInfo.ofsDict);
			nNSG3dResTex.plttInfo.dict = (NNSG3dResDict)src;
			src.setPosition(position + nNSG3dResTex.plttInfo.ofsPlttData);
			num = nNSG3dResTex.plttInfo.sizePltt << 3;
			nNSG3dResTex.plttInfo.pal = new ushort[num / 2];
			src.read(nNSG3dResTex.plttInfo.pal, 0, num / 2);
			return nNSG3dResTex;
		}
	}
}
