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
		public class EquipmentMagic
		{
			public const int NO_MAGIC_ID = -999;

			private int[] magicId_ = new int[MAGIC_ONCE_LEVEL_EQUIP_MAX];

			public EquipmentMagic()
			{
				initialize();
			}

			public void initialize()
			{
				for (int i = 0; i < MAGIC_ONCE_LEVEL_EQUIP_MAX; i++)
				{
					magicId_[i] = -999;
				}
			}

			public short equip(int _id)
			{
				for (int i = 0; i < MAGIC_ONCE_LEVEL_EQUIP_MAX; i++)
				{
					if (magicId_[i] == _id)
					{
						return -999;
					}
				}
				for (int j = 0; j < MAGIC_ONCE_LEVEL_EQUIP_MAX; j++)
				{
					if (magicId_[j] <= 0)
					{
						magicId_[j] = _id;
						return (short)j;
					}
				}
				return -999;
			}

			public bool release(int i)
			{
				if (magicId_[i] != -999)
				{
					magicId_[i] = -999;
					if (i != MAGIC_ONCE_LEVEL_EQUIP_MAX - 1)
					{
						for (int j = i; j < MAGIC_ONCE_LEVEL_EQUIP_MAX - 1; j++)
						{
							magicId_[j] = magicId_[j + 1];
							magicId_[j + 1] = -999;
						}
					}
					return true;
				}
				return false;
			}

			public int magicId(int i)
			{
				return magicId_[i];
			}

			public void magicId_set(int i, int arg0)
			{
				magicId_[i] = arg0;
			}

			public void setDefault()
			{
				memset(magicId_, 0, MAGIC_ONCE_LEVEL_EQUIP_MAX * 4);
			}

			public void copy(EquipmentMagic src)
			{
				memcpy(magicId_, src.magicId_, MAGIC_ONCE_LEVEL_EQUIP_MAX * 4);
			}

			public void parse(ArrayReader reader)
			{
				reader.read(magicId_, 0, MAGIC_ONCE_LEVEL_EQUIP_MAX);
			}

			public void store(ArrayWriter writer)
			{
				writer.write(magicId_, 0, MAGIC_ONCE_LEVEL_EQUIP_MAX);
			}
		}
	}
}
