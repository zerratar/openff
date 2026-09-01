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
	public static partial class pl
	{
		public class JobGrowUpType
		{
			public byte[,] growUpType_ = new byte[23, 6];

			public byte growUpType(int job, int param)
			{
				return growUpType_[job, param];
			}

			public static JobGrowUpType[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 138;
				arrayReader.setPosition(num);
				JobGrowUpType[] array = new JobGrowUpType[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new JobGrowUpType();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				for (int i = 0; i < 23; i++)
				{
					for (int j = 0; j < 6; j++)
					{
						growUpType_[i, j] = reader.readByte();
					}
				}
			}
		}
	}
}
