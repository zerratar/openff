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
	public static partial class pl
	{
		public class GrowUpMp
		{
			private byte[,] mp_ = new byte[PLAYER_LEVEL_MAX, 8];

			public byte mp(int level, int magicLevel)
			{
				return mp_[level, magicLevel];
			}

			public static GrowUpMp[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 792;
				arrayReader.setPosition(num);
				GrowUpMp[] array = new GrowUpMp[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new GrowUpMp();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				for (int i = 0; i < PLAYER_LEVEL_MAX; i++)
				{
					for (int j = 0; j < 8; j++)
					{
						mp_[i, j] = reader.readByte();
					}
				}
			}
		}
	}
}
