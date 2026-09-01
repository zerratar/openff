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
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class btl
	{
		public class BattleNpcData
		{
			private short npcId_;

			private short modelId_;

			private byte breed_;

			private byte level_;

			private byte weight_;

			private ys.BodyParameter body_;

			private short[] weaponId_ = new short[2];

			private short[] messageId_ = new short[NPC_MESSAGE_MAX];

			private NpcAttack[] attack_ = new NpcAttack[NPC_ATTACK_MAX];

			public short npcId()
			{
				return npcId_;
			}

			public short modelId()
			{
				return modelId_;
			}

			public byte breed()
			{
				return breed_;
			}

			public byte level()
			{
				return level_;
			}

			public byte weight()
			{
				return weight_;
			}

			public ys.BodyParameter body()
			{
				return body_;
			}

			public short weaponId(int handType)
			{
				return weaponId_[handType];
			}

			public short messageId(int _id)
			{
				return messageId_[_id];
			}

			public NpcAttack npcAttack(int attackType)
			{
				return attack_[attackType];
			}

			public static BattleNpcData[] castArray(Array src)
			{
				BattleNpcData[] array = new BattleNpcData[src.Length / 40];
				ArrayReader arrayReader = new ArrayReader(src);
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = (BattleNpcData)arrayReader;
				}
				arrayReader.dispose();
				return array;
			}

			public static explicit operator BattleNpcData(ArrayReader src)
			{
				BattleNpcData battleNpcData = new BattleNpcData();
				battleNpcData.body_ = new ys.BodyParameter();
				battleNpcData.npcId_ = src.readInt16();
				battleNpcData.modelId_ = src.readInt16();
				battleNpcData.breed_ = src.readByte();
				battleNpcData.level_ = src.readByte();
				battleNpcData.weight_ = src.readByte();
				battleNpcData.body_.parse(src);
				src.read(battleNpcData.weaponId_, 0, 2);
				src.read(battleNpcData.messageId_, 0, NPC_MESSAGE_MAX);
				for (int i = 0; i < NPC_ATTACK_MAX; i++)
				{
					battleNpcData.attack_[i] = (NpcAttack)src;
				}
				return battleNpcData;
			}
		}
	}
}
