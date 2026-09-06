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
	public class MICompressionHeader
	{
		public uint compParam_4;

		public uint compType_4;

		public uint destSize_24;

		public static explicit operator MICompressionHeader(Array src)
		{
			MICompressionHeader mICompressionHeader = new MICompressionHeader();
			ArrayReader arrayReader = new ArrayReader(src);
			uint num = arrayReader.readUInt32();
			mICompressionHeader.compParam_4 = num & 0xF;
			mICompressionHeader.compType_4 = (num & 0xF0) >> 4;
			mICompressionHeader.destSize_24 = (num & 0xFFFFFF00u) >> 8;
			arrayReader.dispose();
			return mICompressionHeader;
		}
	}
}
