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
		public class PlayerNormalAttackParameter
		{
			private int motionId_;

			private ys.Effects[] effect_ = new ys.Effects[PLAYER_EFFEXTS_MAX];

			private ys.Effects[] se_ = new ys.Effects[PLAYER_EFFEXTS_MAX];

			private short[] targetMotionStartFrame_ = new short[PLAYER_EFFEXTS_MAX];

			private short cancelStartFrame_;

			private short cancelEndFrame_;

			private int randamFlag_;

			public int motionId()
			{
				return motionId_;
			}

			public ys.Effects effect(int i)
			{
				return effect_[i];
			}

			public ys.Effects se(int i)
			{
				return se_[i];
			}

			public short targetMotionStartFrame(int i)
			{
				return targetMotionStartFrame_[i];
			}

			public short cancelStartFrame()
			{
				return cancelStartFrame_;
			}

			public short cancelEndFrame()
			{
				return cancelEndFrame_;
			}

			public int randamFlag()
			{
				return randamFlag_;
			}

			public static PlayerNormalAttackParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 64;
				arrayReader.setPosition(num);
				PlayerNormalAttackParameter[] array = new PlayerNormalAttackParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new PlayerNormalAttackParameter();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				motionId_ = reader.readInt32();
				for (int i = 0; i < PLAYER_EFFEXTS_MAX; i++)
				{
					effect_[i] = new ys.Effects();
					effect_[i].parse(reader);
				}
				for (int i = 0; i < PLAYER_EFFEXTS_MAX; i++)
				{
					se_[i] = new ys.Effects();
					se_[i].parse(reader);
				}
				reader.read(targetMotionStartFrame_, 0, PLAYER_EFFEXTS_MAX);
				cancelStartFrame_ = reader.readInt16();
				cancelEndFrame_ = reader.readInt16();
				randamFlag_ = reader.readInt32();
			}
		}
	}
}
