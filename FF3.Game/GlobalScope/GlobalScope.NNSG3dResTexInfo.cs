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
	public class NNSG3dResTexInfo
	{
		public TexVram vramKey;

		public ushort sizeTex;

		public ushort ofsDict;

		public ushort flag;

		public ushort dummy_;

		public uint ofsTex;

		public NNSG3dResDict dict;

		public ushort[] tex;

		public static explicit operator NNSG3dResTexInfo(ArrayReader src)
		{
			NNSG3dResTexInfo nNSG3dResTexInfo = new NNSG3dResTexInfo();
			src.readUInt32();
			nNSG3dResTexInfo.sizeTex = src.readUInt16();
			nNSG3dResTexInfo.ofsDict = src.readUInt16();
			nNSG3dResTexInfo.flag = src.readUInt16();
			nNSG3dResTexInfo.dummy_ = src.readUInt16();
			nNSG3dResTexInfo.ofsTex = src.readUInt32();
			return nNSG3dResTexInfo;
		}
	}
}
