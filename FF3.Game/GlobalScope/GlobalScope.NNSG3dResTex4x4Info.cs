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
	public class NNSG3dResTex4x4Info
	{
		public TexVram vramKey;

		public ushort sizeTex;

		public ushort ofsDict;

		public ushort flag;

		public ushort dummy_;

		public uint ofsTex;

		public uint ofsTexPlttIdx;

		public NNSG3dResDict dict;

		public uint[] tex;

		public ushort[] pal;

		public static explicit operator NNSG3dResTex4x4Info(ArrayReader src)
		{
			NNSG3dResTex4x4Info nNSG3dResTex4x4Info = new NNSG3dResTex4x4Info();
			src.readUInt32();
			nNSG3dResTex4x4Info.sizeTex = src.readUInt16();
			nNSG3dResTex4x4Info.ofsDict = src.readUInt16();
			nNSG3dResTex4x4Info.flag = src.readUInt16();
			nNSG3dResTex4x4Info.dummy_ = src.readUInt16();
			nNSG3dResTex4x4Info.ofsTex = src.readUInt32();
			nNSG3dResTex4x4Info.ofsTexPlttIdx = src.readUInt32();
			return nNSG3dResTex4x4Info;
		}
	}
}
