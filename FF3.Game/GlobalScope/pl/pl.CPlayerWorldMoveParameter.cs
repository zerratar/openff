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
		public class CPlayerWorldMoveParameter
		{
			private float m_NMveAcc;

			private float m_SMveAcc;

			private float m_NMveMax;

			private float m_SMveMax;

			private float m_MoveSkg;

			private float m_NTrnAcc;

			private float m_STrnAcc;

			private float m_NTrnMax;

			private float m_STrnMax;

			private float m_TurnSkg;

			private float m_Weight;

			public float NMveAcc()
			{
				return m_NMveAcc;
			}

			public float SMveAcc()
			{
				return m_SMveAcc;
			}

			public float NMveMax()
			{
				return m_NMveMax;
			}

			public float SMveMax()
			{
				return m_SMveMax;
			}

			public float MoveSkg()
			{
				return m_MoveSkg;
			}

			public float NTrnAcc()
			{
				return m_NTrnAcc;
			}

			public float STrnAcc()
			{
				return m_STrnAcc;
			}

			public float NTrnMax()
			{
				return m_NTrnMax;
			}

			public float STrnMax()
			{
				return m_STrnMax;
			}

			public float TurnSkg()
			{
				return m_TurnSkg;
			}

			public float Weight()
			{
				return m_Weight;
			}

			public static CPlayerWorldMoveParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 44;
				arrayReader.setPosition(num);
				CPlayerWorldMoveParameter[] array = new CPlayerWorldMoveParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new CPlayerWorldMoveParameter();
					array[i].parse(arrayReader);
				}
				arrayReader.dispose();
				return array;
			}

			public void parse(ArrayReader reader)
			{
				m_NMveAcc = reader.readSingle();
				m_SMveAcc = reader.readSingle();
				m_NMveMax = reader.readSingle();
				m_SMveMax = reader.readSingle();
				m_MoveSkg = reader.readSingle();
				m_NTrnAcc = reader.readSingle();
				m_STrnAcc = reader.readSingle();
				m_NTrnMax = reader.readSingle();
				m_STrnMax = reader.readSingle();
				m_TurnSkg = reader.readSingle();
				m_Weight = reader.readSingle();
			}
		}
	}
}
