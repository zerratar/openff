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
	public class NNSG3dResDataBlockHeader
	{
		public sbyte[] chr = new sbyte[4];

		public uint size;

		public uint kind
		{
			get
			{
				return (uint)(chr[0] | (chr[1] << 8) | (chr[2] << 16) | (chr[3] << 24));
			}
			set
			{
				chr[0] = (sbyte)(value & 0xFF);
				chr[1] = (sbyte)((value >> 8) & 0xFF);
				chr[2] = (sbyte)((value >> 16) & 0xFF);
				chr[3] = (sbyte)((value >> 24) & 0xFF);
			}
		}

		public static explicit operator NNSG3dResDataBlockHeader(ArrayReader src)
		{
			NNSG3dResDataBlockHeader nNSG3dResDataBlockHeader = new NNSG3dResDataBlockHeader();
			nNSG3dResDataBlockHeader.chr[0] = src.readSByte();
			nNSG3dResDataBlockHeader.chr[1] = src.readSByte();
			nNSG3dResDataBlockHeader.chr[2] = src.readSByte();
			nNSG3dResDataBlockHeader.chr[3] = src.readSByte();
			nNSG3dResDataBlockHeader.size = src.readUInt32();
			return nNSG3dResDataBlockHeader;
		}
	}
}
