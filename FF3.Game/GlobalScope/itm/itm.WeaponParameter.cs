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
	public static partial class itm
	{
		public class WeaponParameter : EquipParameter
		{
			private short aggressivity_;

			private byte hitProbability_;

			private byte optionProbability_;

			private short optionMagicItemId_;

			private short armsAttribute_;

			private short atckType_;

			private short atckOption_;

			private short equipOption_;

			protected new byte _pad0;

			protected new byte _pad1;

			public short aggressivity()
			{
				return aggressivity_;
			}

			public byte hitProbability()
			{
				return hitProbability_;
			}

			public byte optionProbability()
			{
				return optionProbability_;
			}

			public short optionMagicItemId()
			{
				return optionMagicItemId_;
			}

			public short armsAttribute()
			{
				return armsAttribute_;
			}

			public short atckType()
			{
				return atckType_;
			}

			public short atckOption()
			{
				return atckOption_;
			}

			public short equipOption()
			{
				return equipOption_;
			}

			public short armsAttributeValue()
			{
				return armsAttribute_;
			}

			public static WeaponParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 56;
				arrayReader.setPosition(num);
				WeaponParameter[] array = new WeaponParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new WeaponParameter();
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
				aggressivity_ = reader.readInt16();
				hitProbability_ = reader.readByte();
				optionProbability_ = reader.readByte();
				optionMagicItemId_ = reader.readInt16();
				armsAttribute_ = reader.readInt16();
				atckType_ = reader.readInt16();
				atckOption_ = reader.readInt16();
				equipOption_ = reader.readInt16();
				_pad0 = reader.readByte();
				_pad1 = reader.readByte();
			}
		}
	}
}
