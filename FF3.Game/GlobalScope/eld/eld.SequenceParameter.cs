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
		public class SequenceParameter
		{
			public uint PathOffset;

			public uint SeqTime;

			public uint uiFlag;

			public uint res;

			public ArrayReader pSeqData;

			public uint[] dummy = new uint[3];

			public SEQUENCE_PATH_HEADER pHeader;

			public static explicit operator SequenceParameter(ArrayReader src)
			{
				SequenceParameter sequenceParameter = new SequenceParameter();
				long position = src.getPosition();
				sequenceParameter.PathOffset = src.readUInt32();
				sequenceParameter.SeqTime = src.readUInt32();
				sequenceParameter.uiFlag = src.readUInt32();
				sequenceParameter.res = src.readUInt32();
				src.readUInt32();
				src.read(sequenceParameter.dummy, 0, 3);
				byte[] array = new byte[position + sequenceParameter.PathOffset - src.getPosition()];
				src.read(array, 0, array.Length);
				sequenceParameter.pSeqData = new ArrayReader(array);
				src.setPosition(position + sequenceParameter.PathOffset);
				sequenceParameter.pHeader = (SEQUENCE_PATH_HEADER)src;
				return sequenceParameter;
			}
		}
	}
}
