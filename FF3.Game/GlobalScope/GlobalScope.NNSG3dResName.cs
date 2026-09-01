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
	public class NNSG3dResName
	{
		public string name;

		public sbyte[] nameBytes;

		public sbyte getNameByte(int iId)
		{
			if (iId >= nameBytes.Length)
			{
				return 0;
			}
			return nameBytes[iId];
		}

		public NNSG3dResName()
		{
		}

		public NNSG3dResName(string arg0)
		{
			name = arg0;
			nameBytes = StringUtil.getSBytes(name);
		}

		public static explicit operator NNSG3dResName(ArrayReader src)
		{
			NNSG3dResName nNSG3dResName = new NNSG3dResName();
			byte[] array = new byte[16];
			src.read(array, 0, array.Length);
			nNSG3dResName.name = StringUtil.createString(array);
			nNSG3dResName.nameBytes = StringUtil.getSBytes(nNSG3dResName.name);
			return nNSG3dResName;
		}

		public static explicit operator NNSG3dResName(string src)
		{
			NNSG3dResName nNSG3dResName = new NNSG3dResName();
			nNSG3dResName.name = src;
			nNSG3dResName.nameBytes = StringUtil.getSBytes(nNSG3dResName.name);
			return nNSG3dResName;
		}
	}
}
