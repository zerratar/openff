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
		public class JobEquipInfo
		{
			private int[] equipInfo_ = new int[23];

			public bool equipInfo(int job, int weaponType)
			{
				int num = 1 << weaponType;
				OS_Printf("[YS]   WEAPON ATTRIBUTE %d\n", num);
				OS_Printf("[YS]   EQUIP INFO %d\n", equipInfo_[job]);
				if ((equipInfo_[job] & num) == 0)
				{
					return false;
				}
				return true;
			}

			public static JobEquipInfo[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 92;
				arrayReader.setPosition(num);
				JobEquipInfo[] array = new JobEquipInfo[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new JobEquipInfo();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				reader.read(equipInfo_, 0, 23);
			}
		}
	}
}
