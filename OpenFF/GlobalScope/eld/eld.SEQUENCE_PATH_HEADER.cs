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
	public static partial class eld
	{
		public class SEQUENCE_PATH_HEADER
		{
			public uint fileType;

			public uint version;

			public uint uiNumPathData;

			public uint res;

			public uint[] pIndex;

			public SPathBodyHeader[] pData;

			public static explicit operator SEQUENCE_PATH_HEADER(ArrayReader src)
			{
				SEQUENCE_PATH_HEADER sEQUENCE_PATH_HEADER = new SEQUENCE_PATH_HEADER();
				long position = src.getPosition();
				sEQUENCE_PATH_HEADER.fileType = src.readUInt32();
				sEQUENCE_PATH_HEADER.version = src.readUInt32();
				sEQUENCE_PATH_HEADER.uiNumPathData = src.readUInt32();
				sEQUENCE_PATH_HEADER.res = src.readUInt32();
				sEQUENCE_PATH_HEADER.pIndex = new uint[sEQUENCE_PATH_HEADER.uiNumPathData];
				sEQUENCE_PATH_HEADER.pData = new SPathBodyHeader[sEQUENCE_PATH_HEADER.uiNumPathData];
				src.read(sEQUENCE_PATH_HEADER.pIndex, 0, (int)sEQUENCE_PATH_HEADER.uiNumPathData);
				for (int i = 0; i < sEQUENCE_PATH_HEADER.uiNumPathData; i++)
				{
					src.setPosition(position + sEQUENCE_PATH_HEADER.pIndex[i]);
					sEQUENCE_PATH_HEADER.pData[i] = (SPathBodyHeader)src;
				}
				return sEQUENCE_PATH_HEADER;
			}
		}
	}
}
