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
		public class PlayerNormalMagicParameter
		{
			private short magicId_;

			private short offset_;

			private ys.Effects effect_;

			private ys.Effects se_;

			private short motionStartFrame_;

			private short effectPlayFrame_;

			public short magicId()
			{
				return magicId_;
			}

			public short offset()
			{
				return offset_;
			}

			public ys.Effects effect()
			{
				return effect_;
			}

			public ys.Effects se()
			{
				return se_;
			}

			public short motionStartFrame()
			{
				return motionStartFrame_;
			}

			public short effectPlayFrame()
			{
				return effectPlayFrame_;
			}

			public static PlayerNormalMagicParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 32;
				arrayReader.setPosition(num);
				PlayerNormalMagicParameter[] array = new PlayerNormalMagicParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new PlayerNormalMagicParameter();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				effect_ = new ys.Effects();
				se_ = new ys.Effects();
				magicId_ = reader.readInt16();
				offset_ = reader.readInt16();
				effect_.parse(reader);
				se_.parse(reader);
				motionStartFrame_ = reader.readInt16();
				effectPlayFrame_ = reader.readInt16();
			}
		}
	}
}
