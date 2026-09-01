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
	public class NNSG3dResAnmHeader
	{
		public byte category0;

		public byte revision;

		public ushort category1;

		public object m_Anm;

		public static explicit operator NNSG3dResAnmHeader(ArrayReader src)
		{
			NNSG3dResAnmHeader nNSG3dResAnmHeader = new NNSG3dResAnmHeader();
			nNSG3dResAnmHeader.category0 = src.readByte();
			nNSG3dResAnmHeader.revision = src.readByte();
			nNSG3dResAnmHeader.category1 = src.readUInt16();
			return nNSG3dResAnmHeader;
		}
	}
}
