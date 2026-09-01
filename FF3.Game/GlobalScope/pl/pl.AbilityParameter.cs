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
	public static partial class pl
	{
		public class AbilityParameter
		{
			public short id_;

			public short nameId_;

			public short type_;

			public short substance_;

			public static AbilityParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 8;
				arrayReader.setPosition(num);
				AbilityParameter[] array = new AbilityParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new AbilityParameter();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				id_ = reader.readInt16();
				nameId_ = reader.readInt16();
				type_ = reader.readInt16();
				substance_ = reader.readInt16();
			}
		}
	}
}
