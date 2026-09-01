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
	public class XbnFile
	{
		public int ID;

		public int version;

		public int numberOfNodes;

		public int reserved;

		public XbnNode[] xbnNode;

		public static explicit operator XbnFile(Array src)
		{
			XbnFile xbnFile = new XbnFile();
			ArrayReader arrayReader = new ArrayReader(src);
			xbnFile.ID = arrayReader.readInt32();
			xbnFile.version = arrayReader.readInt32();
			xbnFile.numberOfNodes = arrayReader.readInt32();
			xbnFile.reserved = arrayReader.readInt32();
			xbnFile.xbnNode = new XbnNode[xbnFile.numberOfNodes + 1];
			for (int i = 0; i < xbnFile.numberOfNodes; i++)
			{
				xbnFile.xbnNode[i] = new XbnNode();
				xbnFile.xbnNode[i].parse1(arrayReader);
			}
			for (int i = 0; i < xbnFile.numberOfNodes; i++)
			{
				xbnFile.xbnNode[i].parse2(arrayReader);
				xbnFile.xbnNode[i].m_Array = xbnFile.xbnNode;
				xbnFile.xbnNode[i].m_iId = i;
			}
			arrayReader.dispose();
			return xbnFile;
		}
	}
}
