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
	public static partial class btl
	{
		public class CommandParameter
		{
			public int param1_;

			public int param2_;

			public int param3_;

			public int param4_;

			public int param5_;

			public int param6_;

			public int param7_;

			public byte again_;

			public SUMMON_BEHAVIOR next_;

			public CommandParameter()
			{
			}

			public CommandParameter(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, byte arg7, SUMMON_BEHAVIOR arg8)
			{
				param1_ = arg0;
				param2_ = arg1;
				param3_ = arg2;
				param4_ = arg3;
				param5_ = arg4;
				param6_ = arg5;
				param7_ = arg6;
				again_ = arg7;
				next_ = arg8;
			}

			public static CommandParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 36;
				arrayReader.setPosition(num);
				CommandParameter[] array = new CommandParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new CommandParameter();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				param1_ = reader.readInt32();
				param2_ = reader.readInt32();
				param3_ = reader.readInt32();
				param4_ = reader.readInt32();
				param5_ = reader.readInt32();
				param6_ = reader.readInt32();
				param7_ = reader.readInt32();
				again_ = reader.readByte();
				reader.readByte();
				reader.readByte();
				reader.readByte();
				next_ = (SUMMON_BEHAVIOR)reader.readInt32();
			}
		}
	}
}
