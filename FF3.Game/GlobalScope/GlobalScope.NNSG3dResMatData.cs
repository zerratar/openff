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
			src.readUInt32();
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
