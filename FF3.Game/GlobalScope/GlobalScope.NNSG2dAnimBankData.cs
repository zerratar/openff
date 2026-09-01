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
	public class NNSG2dAnimBankData
	{
		public ushort numSequences;

		public ushort numTotalFrames;

		public NNSG2dAnimSequenceData[] pSequenceArrayHead;

		public NNSG2dAnimFrameData[] pFrameArrayHead;

		public Array pAnimContents;

		public Array pStringBank;

		public Array pExtendedData;

		public void parse(Array src)
		{
			ArrayReader arrayReader = new ArrayReader(src);
			numSequences = arrayReader.readUInt16();
			numTotalFrames = arrayReader.readUInt16();
			arrayReader.readInt32();
			arrayReader.readInt32();
			arrayReader.readInt32();
			arrayReader.readInt32();
			arrayReader.readInt32();
			pSequenceArrayHead = new NNSG2dAnimSequenceData[numSequences];
			for (int i = 0; i < numSequences; i++)
			{
				pSequenceArrayHead[i] = new NNSG2dAnimSequenceData();
				pSequenceArrayHead[i].parse1(arrayReader);
			}
			for (int i = 0; i < numSequences; i++)
			{
				pSequenceArrayHead[i].parse2(arrayReader);
			}
			for (int i = 0; i < numSequences; i++)
			{
				pSequenceArrayHead[i].parse3(arrayReader);
			}
			arrayReader.dispose();
		}
	}
}
