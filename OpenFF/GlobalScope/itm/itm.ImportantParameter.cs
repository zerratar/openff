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
		public class ImportantParameter : ItemBaseParameter
		{
			private byte specialOptionId_;

			protected new byte _pad0;

			public byte specialOptionId()
			{
				return specialOptionId_;
			}

			public static ImportantParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 28;
				arrayReader.setPosition(num);
				ImportantParameter[] array = new ImportantParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new ImportantParameter();
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
				specialOptionId_ = reader.readByte();
				_pad0 = reader.readByte();
			}
		}
	}
}
