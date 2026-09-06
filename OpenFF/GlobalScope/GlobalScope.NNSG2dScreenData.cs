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
	public class NNSG2dScreenData
	{
		private ushort screenWidth;

		private ushort screenHeight;

		private ushort colorMode;

		private ushort screenFormat;

		private uint szByte;

		private uint[] rawData = new uint[1];

		public void parse(Array src)
		{
			ArrayReader arrayReader = new ArrayReader(src);
			screenWidth = arrayReader.readUInt16();
			screenHeight = arrayReader.readUInt16();
			colorMode = arrayReader.readUInt16();
			screenFormat = arrayReader.readUInt16();
			szByte = arrayReader.readUInt32();
			arrayReader.dispose();
		}
	}
}
