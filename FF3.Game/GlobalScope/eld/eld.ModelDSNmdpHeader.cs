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
	public static partial class eld
	{
		public class ModelDSNmdpHeader
		{
			public Array unOffsetModel;

			public Array unOffsetTexture;

			public uint unInitialize;

			public ModelTexture unModelTexturePointer;

			public static ModelDSNmdpHeader cast(Array src, uint offset)
			{
				ModelDSNmdpHeader modelDSNmdpHeader = new ModelDSNmdpHeader();
				ArrayReader arrayReader = new ArrayReader(src);
				arrayReader.setPosition(offset);
				long position = arrayReader.getPosition();
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				modelDSNmdpHeader.unInitialize = arrayReader.readUInt32();
				arrayReader.readUInt32();
				if (num != 0)
				{
					arrayReader.setPosition(position + num);
					int num3 = (int)((num2 == 0) ? arrayReader.rest() : (num2 - num));
					byte[] array = new byte[num3];
					arrayReader.read(array, 0, array.Length);
					modelDSNmdpHeader.unOffsetModel = array;
				}
				if (num2 != 0)
				{
					arrayReader.setPosition(position + num2);
					int num3 = (int)arrayReader.rest();
					byte[] array = new byte[num3];
					arrayReader.read(array, 0, array.Length);
					modelDSNmdpHeader.unOffsetTexture = array;
				}
				arrayReader.dispose();
				return modelDSNmdpHeader;
			}
		}
	}
}
