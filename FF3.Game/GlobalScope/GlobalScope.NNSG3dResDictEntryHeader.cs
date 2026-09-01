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
	public class NNSG3dResDictEntryHeader
	{
		public ushort sizeUnit;

		public ushort ofsName;

		public byte[] data;

		public NNSG3dResName[] name;

		public uint getU32(int iId)
		{
			return ArrayReader.packUInt32(data, sizeUnit * iId);
		}

		public uint getU32(int iId, int iShift)
		{
			return ArrayReader.packUInt32(data, sizeUnit * iId + iShift);
		}

		public byte[] getBytes(int iId)
		{
			ArrayReader arrayReader = new ArrayReader(data);
			byte[] array = new byte[sizeUnit];
			arrayReader.setPosition(sizeUnit * iId);
			arrayReader.read(array, 0, array.Length);
			arrayReader.dispose();
			return array;
		}

		public void parse(ArrayReader reader, NNSG3dResDict dict)
		{
			long position = reader.getPosition();
			sizeUnit = reader.readUInt16();
			ofsName = reader.readUInt16();
			int num = ofsName - 4;
			data = new byte[num];
			reader.read(data, 0, num);
			name = new NNSG3dResName[dict.numEntry];
			for (int i = 0; i < dict.numEntry; i++)
			{
				name[i] = new NNSG3dResName();
				reader.setPosition(position + ofsName + 16 * i);
				name[i] = (NNSG3dResName)reader;
			}
		}
	}
}
