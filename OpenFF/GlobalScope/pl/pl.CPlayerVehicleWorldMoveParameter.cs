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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class pl
	{
		public class CPlayerVehicleWorldMoveParameter
		{
			private short m_RiseSpd;

			private short m_DsntAcc;

			private short m_RiseSkg;

			private short m_DsntSkg;

			private short m_TgetPnt;

			public short RiseSpd()
			{
				return m_RiseSpd;
			}

			public short DsntAcc()
			{
				return m_DsntAcc;
			}

			public short RiseSkg()
			{
				return m_RiseSkg;
			}

			public short DsntSkg()
			{
				return m_DsntSkg;
			}

			public short TgetPnt()
			{
				return m_TgetPnt;
			}

			public static CPlayerVehicleWorldMoveParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 10;
				arrayReader.setPosition(num);
				CPlayerVehicleWorldMoveParameter[] array = new CPlayerVehicleWorldMoveParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new CPlayerVehicleWorldMoveParameter();
					array[i].parse(arrayReader);
				}
				arrayReader.dispose();
				return array;
			}

			public void parse(ArrayReader reader)
			{
				m_RiseSpd = reader.readInt16();
				m_DsntAcc = reader.readInt16();
				m_RiseSkg = reader.readInt16();
				m_DsntSkg = reader.readInt16();
				m_TgetPnt = reader.readInt16();
			}
		}
	}
}
