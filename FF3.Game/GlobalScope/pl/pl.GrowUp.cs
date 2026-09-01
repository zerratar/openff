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
	public static partial class pl
	{
		public class GrowUp
		{
			private byte[,] parameter_ = new byte[8, PLAYER_LEVEL_MAX];

			public byte parameter(int growType, int level)
			{
				return parameter_[growType, level];
			}

			public static GrowUp[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 792;
				arrayReader.setPosition(num);
				GrowUp[] array = new GrowUp[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new GrowUp();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < PLAYER_LEVEL_MAX; j++)
					{
						parameter_[i, j] = reader.readByte();
					}
				}
			}
		}
	}
}
