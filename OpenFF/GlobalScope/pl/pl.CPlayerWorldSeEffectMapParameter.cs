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
		public class CPlayerWorldSeEffectMapParameter
		{
			private short m_SeNumber;

			private short m_WaitEffectCategory;

			private short m_WaitEffectNumber;

			private short m_MoveEffectCategory;

			private short m_MoveEffectNumber;

			public short SeNumber()
			{
				return m_SeNumber;
			}

			public short WaitEffectCategory()
			{
				return m_WaitEffectCategory;
			}

			public short WaitEffectNumber()
			{
				return m_WaitEffectNumber;
			}

			public short MoveEffectCategory()
			{
				return m_MoveEffectCategory;
			}

			public short MoveEffectNumber()
			{
				return m_MoveEffectNumber;
			}

			public static CPlayerWorldSeEffectMapParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 10;
				arrayReader.setPosition(num);
				CPlayerWorldSeEffectMapParameter[] array = new CPlayerWorldSeEffectMapParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new CPlayerWorldSeEffectMapParameter();
					array[i].parse(arrayReader);
				}
				arrayReader.dispose();
				return array;
			}

			public void parse(ArrayReader reader)
			{
				m_SeNumber = reader.readInt16();
				m_WaitEffectCategory = reader.readInt16();
				m_WaitEffectNumber = reader.readInt16();
				m_MoveEffectCategory = reader.readInt16();
				m_MoveEffectNumber = reader.readInt16();
			}
		}
	}
}
