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
		public class PlayerExp
		{
			private int[] exp_ = new int[PLAYER_LEVEL_MAX];

			public int exp(byte level)
			{
				if (level >= PLAYER_LEVEL_MAX)
				{
					return 0;
				}
				return exp_[level];
			}

			public static PlayerExp[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 396;
				arrayReader.setPosition(num);
				PlayerExp[] array = new PlayerExp[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new PlayerExp();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				reader.read(exp_, 0, PLAYER_LEVEL_MAX);
			}
		}
	}
}
