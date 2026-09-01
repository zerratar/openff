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
	public class NNSG2dCellData
	{
		public ushort numOAMAttrs;

		public ushort cellAttr;

		public NNSG2dCellOAMAttrData[] pOamAttrArray;

		public void parse1(ArrayReader reader)
		{
			numOAMAttrs = reader.readUInt16();
			cellAttr = reader.readUInt16();
			reader.readInt32();
		}

		public void parse2(ArrayReader reader)
		{
			pOamAttrArray = new NNSG2dCellOAMAttrData[numOAMAttrs];
			for (int i = 0; i < numOAMAttrs; i++)
			{
				pOamAttrArray[i] = new NNSG2dCellOAMAttrData();
				pOamAttrArray[i].parse(reader);
			}
		}
	}
}
