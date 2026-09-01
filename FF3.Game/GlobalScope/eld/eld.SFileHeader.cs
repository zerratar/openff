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
	public static partial class eld
	{
		public class SFileHeader
		{
			public uint uiName;

			public ushort uiNumTemplate;

			public ushort uiRegistFlag;

			public uint TexAddr;

			public uint version;

			public uint[] pIndex;

			public Template[] pTemplate;

			public static explicit operator SFileHeader(Array src)
			{
				SFileHeader sFileHeader = new SFileHeader();
				ArrayReader arrayReader = new ArrayReader(src);
				long position = arrayReader.getPosition();
				sFileHeader.uiName = arrayReader.readUInt32();
				sFileHeader.uiNumTemplate = arrayReader.readUInt16();
				sFileHeader.uiRegistFlag = arrayReader.readUInt16();
				sFileHeader.TexAddr = arrayReader.readUInt32();
				sFileHeader.version = arrayReader.readUInt32();
				sFileHeader.pIndex = new uint[sFileHeader.uiNumTemplate];
				sFileHeader.pTemplate = new Template[sFileHeader.uiNumTemplate];
				arrayReader.read(sFileHeader.pIndex, 0, sFileHeader.uiNumTemplate);
				for (int i = 0; i < sFileHeader.uiNumTemplate; i++)
				{
					arrayReader.setPosition(position + sFileHeader.pIndex[i]);
					sFileHeader.pTemplate[i] = (Template)arrayReader;
				}
				arrayReader.dispose();
				return sFileHeader;
			}
		}
	}
}
