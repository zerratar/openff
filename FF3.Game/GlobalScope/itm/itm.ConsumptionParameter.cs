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
	public static partial class itm
	{
		public class ConsumptionParameter : NotImportantParameter
		{
			private short usedPower_;

			private short itemType_;

			private short changeCondition_;

			protected new byte _pad0;

			protected new byte _pad1;

			public short usedPower()
			{
				return usedPower_;
			}

			public short itemType()
			{
				return itemType_;
			}

			public short changeCondition()
			{
				return changeCondition_;
			}

			public static ConsumptionParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 44;
				arrayReader.setPosition(num);
				ConsumptionParameter[] array = new ConsumptionParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new ConsumptionParameter();
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
				usedPower_ = reader.readInt16();
				itemType_ = reader.readInt16();
				changeCondition_ = reader.readInt16();
				_pad0 = reader.readByte();
				_pad1 = reader.readByte();
			}
		}
	}
}
