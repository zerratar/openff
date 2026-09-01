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
		public class CNPCWorldRandomMoveParameter : CNPCWorldBaseMoveParameter
		{
			private short[] m_Distance = new short[2];

			public short Distance(int index)
			{
				return m_Distance[index];
			}

			public static CNPCWorldRandomMoveParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 8;
				arrayReader.setPosition(num);
				CNPCWorldRandomMoveParameter[] array = new CNPCWorldRandomMoveParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new CNPCWorldRandomMoveParameter();
					array[i].parse(arrayReader);
				}
				arrayReader.dispose();
				return array;
			}

			public void parse(ArrayReader reader)
			{
				m_StateFrame = reader.readInt16();
				m_RandStateFrame = reader.readInt16();
				reader.read(m_Distance, 0, 2);
			}
		}
	}
}
