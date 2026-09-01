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
	public class NNSG2dAnimFrameData
	{
		public Array pContent;

		public ushort frames;

		public ushort pad16;

		public void parse1(ArrayReader reader)
		{
			reader.readInt32();
			frames = reader.readUInt16();
			pad16 = reader.readUInt16();
		}

		public void parse2(ArrayReader reader)
		{
			byte[] abyData = new byte[8];
			reader.read(abyData, 0, 8);
			pContent = abyData;
		}
	}
}
