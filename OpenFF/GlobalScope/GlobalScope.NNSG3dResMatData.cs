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
	public class NNSG3dResMatData
	{
		public ushort itemTag;

		public ushort size;

		public uint diffAmb;

		public uint specEmi;

		public uint polyAttr;

		public uint polyAttrMask;

		public TexVramList texImageParam;

		public uint texImageParamMask;
		// PORT: the material's own texImageParam word, kept for its wrap bits (16 repeat S,
		// 17 repeat T, 18 flip S, 19 flip T). The texture object it names is shared between
		// materials, so the wrap has to be set when the material binds it, not when the
		// texture is made - FF4's world map blends its tiles with "flip" (mirrored repeat).
		public uint texImageParamBits;

		public ushort texPlttBase;

		public ushort flag;

		public ushort origWidth;

		public ushort origHeight;

		public int magW;

		public int magH;

		public static explicit operator NNSG3dResMatData(ArrayReader src)
		{
			NNSG3dResMatData nNSG3dResMatData = new NNSG3dResMatData();
			nNSG3dResMatData.itemTag = src.readUInt16();
			nNSG3dResMatData.size = src.readUInt16();
			nNSG3dResMatData.diffAmb = src.readUInt32();
			nNSG3dResMatData.specEmi = src.readUInt32();
			nNSG3dResMatData.polyAttr = src.readUInt32();
			nNSG3dResMatData.polyAttrMask = src.readUInt32();
			nNSG3dResMatData.texImageParamBits = src.readUInt32();
			nNSG3dResMatData.texImageParamMask = src.readUInt32();
			nNSG3dResMatData.texPlttBase = src.readUInt16();
			nNSG3dResMatData.flag = src.readUInt16();
			nNSG3dResMatData.origWidth = src.readUInt16();
			nNSG3dResMatData.origHeight = src.readUInt16();
			nNSG3dResMatData.magW = src.readInt32();
			nNSG3dResMatData.magH = src.readInt32();
			return nNSG3dResMatData;
		}
	}
}
