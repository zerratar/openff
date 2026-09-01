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
	public static class pack
	{
		public static byte[] ChainPointer(byte[] addr, uint index)
		{
			if (index >= ChainPointerCount(addr))
			{
				return null;
			}
			if (ArrayReader.packInt32(addr, (int)(index * 8 + 16)) == 0)
			{
				return null;
			}
			ArrayReader arrayReader = new ArrayReader(addr);
			byte[] array = new byte[ChainPointerSize(addr, index)];
			if (ArrayReader.packInt32(addr, 8) == 0)
			{
				arrayReader.setPosition(ArrayReader.packInt32(addr, (int)(index * 8 + 16)));
			}
			else
			{
				arrayReader.setPosition(ArrayReader.packInt32(addr, (int)(index * 8)));
			}
			arrayReader.read(array, 0, array.Length);
			return array;
		}

		public static uint ChainPointerSize(byte[] addr, uint index)
		{
			if (ArrayReader.packInt32(addr, 8) == 0)
			{
				return ArrayReader.packUInt32(addr, (int)(index * 8 + 4 + 16));
			}
			return ArrayReader.packUInt32(addr, (int)(index * 8 + 4));
		}

		public static uint ChainPointerCount(byte[] addr)
		{
			return ArrayReader.packUInt32(addr, 0);
		}
	}
}
