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
	public static partial class dgs
	{
		public class MSDINFO
		{
			public uint type;

			public uint version;

			public uint num_msg;

			public MSDELEMENT[] elements;

			public byte[] m_abyData;

			public static explicit operator MSDINFO(Array src)
			{
				MSDINFO mSDINFO = new MSDINFO();
				ArrayReader arrayReader = new ArrayReader(src);
				mSDINFO.type = arrayReader.readUInt32();
				mSDINFO.version = arrayReader.readUInt32();
				mSDINFO.num_msg = arrayReader.readUInt32();
				arrayReader.readUInt32();
				mSDINFO.elements = new MSDELEMENT[mSDINFO.num_msg];
				for (int i = 0; i < mSDINFO.num_msg; i++)
				{
					mSDINFO.elements[i] = new MSDELEMENT();
					mSDINFO.elements[i].number = arrayReader.readUInt32();
					mSDINFO.elements[i].num_pages = arrayReader.readByte();
					mSDINFO.elements[i].font = arrayReader.readByte();
					mSDINFO.elements[i].padding = arrayReader.readUInt16();
					mSDINFO.elements[i].offset = arrayReader.readUInt32();
				}
				for (int i = 0; i < mSDINFO.num_msg; i++)
				{
					mSDINFO.elements[i].offset -= (uint)(int)arrayReader.getPosition();
				}
				mSDINFO.m_abyData = new byte[arrayReader.rest()];
				arrayReader.read(mSDINFO.m_abyData, 0, mSDINFO.m_abyData.Length);
				arrayReader.dispose();
				return mSDINFO;
			}
		}
	}
}
