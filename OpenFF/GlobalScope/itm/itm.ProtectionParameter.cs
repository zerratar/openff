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
	public static partial class itm
	{
		public class ProtectionParameter : EquipParameter
		{
			private short phylacticPower_;

			private short magicPhylacticPower_;

			private byte avoidanceProbability_;

			private byte magicAvoidanceProbability_;

			private byte evasionNum_;

			protected new byte _pad0;

			private short armsWeakAttribute_;

			private short armsAttribute_;

			private short weakType_;

			private short antiType_;

			private short antiOption_;

			private short equipOption_;

			public short phylacticPower()
			{
				return phylacticPower_;
			}

			public short magicPhylacticPower()
			{
				return magicPhylacticPower_;
			}

			public byte avoidanceProbability()
			{
				return avoidanceProbability_;
			}

			public byte magicAvoidanceProbability()
			{
				return magicAvoidanceProbability_;
			}

			public byte evasionNum()
			{
				return evasionNum_;
			}

			public short armsWeakAttribute()
			{
				return armsWeakAttribute_;
			}

			public short armsAttribute()
			{
				return armsAttribute_;
			}

			public short weakType()
			{
				return weakType_;
			}

			public short antiType()
			{
				return antiType_;
			}

			public short antiOption()
			{
				return antiOption_;
			}

			public short equipOption()
			{
				return equipOption_;
			}

			public short armsAttribute(ARMS_ATTRIBUTE i)
			{
				return armsAttribute_;
			}

			public static ProtectionParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 60;
				arrayReader.setPosition(num);
				ProtectionParameter[] array = new ProtectionParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new ProtectionParameter();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				system_ = reader.readByte();
				_pad0 = reader.readByte();
				itemId_ = reader.readInt16();
				nameId_ = reader.readInt16();
				captionId_ = reader.readInt16();
				graphId_ = reader.readInt16();
				strength_ = reader.readByte();
				vitality_ = reader.readByte();
				dexterity_ = reader.readByte();
				intellect_ = reader.readByte();
				mind_ = reader.readByte();
				weight_ = reader.readByte();
				useBattle_ = reader.readByte();
				useField_ = reader.readByte();
				allTarget_ = reader.readByte();
				_pad1 = reader.readByte();
				useItemId_ = reader.readInt16();
				targetPossible_ = reader.readInt16();
				targetPosition_ = reader.readInt16();
				_pad2 = reader.readByte();
				_pad3 = reader.readByte();
				buy_ = reader.readInt32();
				price_ = reader.readInt32();
				equipJob_ = reader.readInt32();
				phylacticPower_ = reader.readInt16();
				magicPhylacticPower_ = reader.readInt16();
				avoidanceProbability_ = reader.readByte();
				magicAvoidanceProbability_ = reader.readByte();
				evasionNum_ = reader.readByte();
				_pad0 = reader.readByte();
				armsWeakAttribute_ = reader.readInt16();
				armsAttribute_ = reader.readInt16();
				weakType_ = reader.readInt16();
				antiType_ = reader.readInt16();
				antiOption_ = reader.readInt16();
				equipOption_ = reader.readInt16();
			}
		}
	}
}
