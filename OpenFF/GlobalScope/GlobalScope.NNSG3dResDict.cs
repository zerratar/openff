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
	public class NNSG3dResDict
	{
		public byte revision;

		public byte numEntry;

		public ushort sizeDictBlk;

		public ushort dummy_;

		public ushort ofsEntry;

		public NNSG3dResDictTreeNode[] node;

		public NNSG3dResDictEntryHeader entry;

		public static explicit operator NNSG3dResDict(ArrayReader src)
		{
			NNSG3dResDict nNSG3dResDict = new NNSG3dResDict();
			nNSG3dResDict.revision = src.readByte();
			nNSG3dResDict.numEntry = src.readByte();
			nNSG3dResDict.sizeDictBlk = src.readUInt16();
			nNSG3dResDict.dummy_ = src.readUInt16();
			nNSG3dResDict.ofsEntry = src.readUInt16();
			int num = (nNSG3dResDict.ofsEntry - 8) / 4;
			nNSG3dResDict.node = new NNSG3dResDictTreeNode[num];
			for (int i = 0; i < num; i++)
			{
				nNSG3dResDict.node[i] = (NNSG3dResDictTreeNode)src;
			}
			nNSG3dResDict.entry = new NNSG3dResDictEntryHeader();
			nNSG3dResDict.entry.parse(src, nNSG3dResDict);
			return nNSG3dResDict;
		}
	}
}
