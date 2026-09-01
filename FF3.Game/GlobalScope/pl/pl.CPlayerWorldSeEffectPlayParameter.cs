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
		public class CPlayerWorldSeEffectPlayParameter
		{
			private short m_PlayWaitFrame;

			private short[] m_PlayMoveFrame = new short[2];

			private short[] m_PlaySpecialMoveFrame = new short[2];

			public short PlayWaitFrame()
			{
				return m_PlayWaitFrame;
			}

			public short PlayMoveFrame(int index)
			{
				return m_PlayMoveFrame[index];
			}

			public short PlaySpecialMoveFrame(int index)
			{
				return m_PlaySpecialMoveFrame[index];
			}

			public static CPlayerWorldSeEffectPlayParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 10;
				arrayReader.setPosition(num);
				CPlayerWorldSeEffectPlayParameter[] array = new CPlayerWorldSeEffectPlayParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new CPlayerWorldSeEffectPlayParameter();
					array[i].parse(arrayReader);
				}
				arrayReader.dispose();
				return array;
			}

			public void parse(ArrayReader reader)
			{
				m_PlayWaitFrame = reader.readInt16();
				reader.read(m_PlayMoveFrame, 0, 2);
				reader.read(m_PlaySpecialMoveFrame, 0, 2);
			}
		}
	}
}
