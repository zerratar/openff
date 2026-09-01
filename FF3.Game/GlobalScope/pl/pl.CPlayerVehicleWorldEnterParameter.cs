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
		public class CPlayerVehicleWorldEnterParameter
		{
			private byte[] m_Flag = new byte[10];

			public byte Flag(int index)
			{
				return m_Flag[index];
			}

			public static CPlayerVehicleWorldEnterParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 10;
				arrayReader.setPosition(num);
				CPlayerVehicleWorldEnterParameter[] array = new CPlayerVehicleWorldEnterParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new CPlayerVehicleWorldEnterParameter();
					array[i].parse(arrayReader);
				}
				arrayReader.dispose();
				return array;
			}

			public void parse(ArrayReader reader)
			{
				reader.read(m_Flag, 0, 10);
			}
		}
	}
}
