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
	public class NNSG2dAnimSequenceData
	{
		public ushort numFrames;

		public ushort loopStartFrameIdx;

		public uint animType;

		public NNSG2dAnimationPlayMode playMode;

		public NNSG2dAnimFrameData[] pAnmFrameArray;

		public void parse1(ArrayReader reader)
		{
			numFrames = reader.readUInt16();
			loopStartFrameIdx = reader.readUInt16();
			animType = reader.readUInt32();
			playMode = (NNSG2dAnimationPlayMode)reader.readInt32();
			reader.readInt32();
		}

		public void parse2(ArrayReader reader)
		{
			pAnmFrameArray = new NNSG2dAnimFrameData[numFrames];
			for (int i = 0; i < numFrames; i++)
			{
				pAnmFrameArray[i] = new NNSG2dAnimFrameData();
				pAnmFrameArray[i].parse1(reader);
			}
		}

		public void parse3(ArrayReader reader)
		{
			for (int i = 0; i < numFrames; i++)
			{
				pAnmFrameArray[i].parse2(reader);
			}
		}
	}
}
