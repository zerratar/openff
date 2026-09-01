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
	public class NNSG3dResFileHeader
	{
		public sbyte[] signature = new sbyte[4];

		public ushort byteOrder;

		public ushort version;

		public uint fileSize;

		public ushort headerSize;

		public ushort dataBlocks;

		public uint[] offset;

		public object[] m_Res;

		public uint sigVal
		{
			get
			{
				return (uint)(signature[0] | (signature[1] << 8) | (signature[2] << 16) | (signature[3] << 24));
			}
			set
			{
				signature[0] = (sbyte)(value & 0xFF);
				signature[1] = (sbyte)((value >> 8) & 0xFF);
				signature[2] = (sbyte)((value >> 16) & 0xFF);
				signature[3] = (sbyte)((value >> 24) & 0xFF);
			}
		}

		public static explicit operator NNSG3dResFileHeader(ArrayReader src)
		{
			NNSG3dResFileHeader nNSG3dResFileHeader = new NNSG3dResFileHeader();
			long position = src.getPosition();
			nNSG3dResFileHeader.signature[0] = src.readSByte();
			nNSG3dResFileHeader.signature[1] = src.readSByte();
			nNSG3dResFileHeader.signature[2] = src.readSByte();
			nNSG3dResFileHeader.signature[3] = src.readSByte();
			nNSG3dResFileHeader.byteOrder = src.readUInt16();
			nNSG3dResFileHeader.version = src.readUInt16();
			nNSG3dResFileHeader.fileSize = src.readUInt32();
			nNSG3dResFileHeader.headerSize = src.readUInt16();
			nNSG3dResFileHeader.dataBlocks = src.readUInt16();
			nNSG3dResFileHeader.offset = new uint[nNSG3dResFileHeader.dataBlocks];
			src.read(nNSG3dResFileHeader.offset, 0, nNSG3dResFileHeader.dataBlocks);
			nNSG3dResFileHeader.m_Res = new object[nNSG3dResFileHeader.dataBlocks];
			for (int i = 0; i < nNSG3dResFileHeader.dataBlocks; i++)
			{
				src.setPosition(position + nNSG3dResFileHeader.offset[i]);
				nNSG3dResFileHeader.m_Res[i] = createRes(src);
			}
			return nNSG3dResFileHeader;
		}
	}
}
