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
		public class PlayerAbility
		{
			public short[] command_ = new short[COMMAND_ABILITY_MAX];

			public short[] passive_ = new short[PASSIVE_ABILITY_MAX];

			public static PlayerAbility[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 12;
				arrayReader.setPosition(num);
				PlayerAbility[] array = new PlayerAbility[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new PlayerAbility();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void setDefault()
			{
				memset(command_, 0, COMMAND_ABILITY_MAX * 2);
				memset(passive_, 0, PASSIVE_ABILITY_MAX * 2);
			}

			public void copy(PlayerAbility src)
			{
				memcpy(command_, src.command_, COMMAND_ABILITY_MAX * 2);
				memcpy(passive_, src.passive_, PASSIVE_ABILITY_MAX * 2);
			}

			public void parse(ArrayReader reader)
			{
				reader.read(command_, 0, COMMAND_ABILITY_MAX);
				reader.read(passive_, 0, PASSIVE_ABILITY_MAX);
			}

			public void store(ArrayWriter writer)
			{
				writer.write(command_, 0, COMMAND_ABILITY_MAX);
				writer.write(passive_, 0, PASSIVE_ABILITY_MAX);
			}
		}
	}
}
