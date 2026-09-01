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
	public class NNSG3dResPlttInfo
	{
		public TexVram vramKey;

		public ushort sizePltt;

		public ushort flag;

		public ushort ofsDict;

		public ushort dummy_;

		public uint ofsPlttData;

		public NNSG3dResDict dict;

		public ushort[] pal;

		public static explicit operator NNSG3dResPlttInfo(ArrayReader src)
		{
			NNSG3dResPlttInfo nNSG3dResPlttInfo = new NNSG3dResPlttInfo();
			src.readUInt32();
			nNSG3dResPlttInfo.sizePltt = src.readUInt16();
			nNSG3dResPlttInfo.flag = src.readUInt16();
			nNSG3dResPlttInfo.ofsDict = src.readUInt16();
			nNSG3dResPlttInfo.dummy_ = src.readUInt16();
			nNSG3dResPlttInfo.ofsPlttData = src.readUInt32();
			return nNSG3dResPlttInfo;
		}
	}
}
