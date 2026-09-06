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
	public static partial class eld
	{
		public class SGuid
		{
			public uint Data1;

			public ushort Data2;

			public ushort Data3;

			public byte[] Data4 = new byte[8];

			public SGuid()
			{
			}

			public SGuid(uint arg0, ushort arg1, ushort arg2, byte[] arg3)
			{
				Data1 = arg0;
				Data2 = arg1;
				Data3 = arg2;
				Data4[0] = arg3[0];
				Data4[1] = arg3[1];
				Data4[2] = arg3[2];
				Data4[3] = arg3[3];
				Data4[4] = arg3[4];
				Data4[5] = arg3[5];
				Data4[6] = arg3[6];
				Data4[7] = arg3[7];
			}

			public static explicit operator SGuid(ArrayReader src)
			{
				SGuid sGuid = new SGuid();
				sGuid.Data1 = src.readUInt32();
				sGuid.Data2 = src.readUInt16();
				sGuid.Data3 = src.readUInt16();
				src.read(sGuid.Data4, 0, 8);
				return sGuid;
			}
		}
	}
}
