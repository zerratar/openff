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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	public class NNSG2dCharacterData
	{
		public ushort H;

		public ushort W;

		public GXTexFmt pixelFmt;

		public int mapingType;

		public uint characterFmt;

		public uint szByte;

		public Array pRawData;

		public Array m_aPng;

		public void parse(Array src)
		{
			ArrayReader arrayReader = new ArrayReader(src);
			H = arrayReader.readUInt16();
			W = arrayReader.readUInt16();
			pixelFmt = (GXTexFmt)arrayReader.readInt32();
			mapingType = arrayReader.readInt32();
			characterFmt = arrayReader.readUInt32();
			szByte = arrayReader.readUInt32();
			arrayReader.dispose();
		}
	}
}
