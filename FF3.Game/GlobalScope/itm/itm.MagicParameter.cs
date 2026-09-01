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
		public class MagicParameter : EquipParameter
		{
			private byte magicClass_;

			protected new byte _pad0;

			private short magicAggressivity_;

			private byte successProbability_;

			private byte magicUseKind_;

			private short magicType_;

			private short changeCondition_;

			private byte calculate_;

			private byte reflect_;

			public byte magicClass()
			{
				return magicClass_;
			}

			public short magicAggressivity()
			{
				return magicAggressivity_;
			}

			public byte successProbability()
			{
				return successProbability_;
			}

			public byte magicUseKind()
			{
				return magicUseKind_;
			}

			public short magicType()
			{
				return magicType_;
			}

			public short changeCondition()
			{
				return changeCondition_;
			}

			public byte calculate()
			{
				return calculate_;
			}

			public byte isReflect()
			{
				return reflect_;
			}

			public static MagicParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 52;
				arrayReader.setPosition(num);
				MagicParameter[] array = new MagicParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new MagicParameter();
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
				magicClass_ = reader.readByte();
				_pad0 = reader.readByte();
				magicAggressivity_ = reader.readInt16();
				successProbability_ = reader.readByte();
				magicUseKind_ = reader.readByte();
				magicType_ = reader.readInt16();
				changeCondition_ = reader.readInt16();
				calculate_ = reader.readByte();
				reflect_ = reader.readByte();
			}
		}
	}
}
