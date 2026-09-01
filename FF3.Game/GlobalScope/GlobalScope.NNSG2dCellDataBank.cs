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
	public class NNSG2dCellDataBank
	{
		public ushort numCells;

		public ushort cellBankAttr;

		public NNSG2dCellData[] pCellDataArrayHead;

		public int mappingMode;

		public NNSG2dVramTransferData pVramTransferData;

		public Array pStringBank;

		public Array pExtendedData;

		public void parse(Array src)
		{
			ArrayReader arrayReader = new ArrayReader(src);
			numCells = arrayReader.readUInt16();
			cellBankAttr = arrayReader.readUInt16();
			arrayReader.readInt32();
			mappingMode = arrayReader.readInt32();
			arrayReader.readInt32();
			arrayReader.readInt32();
			arrayReader.readInt32();
			pCellDataArrayHead = new NNSG2dCellData[numCells];
			for (int i = 0; i < numCells; i++)
			{
				pCellDataArrayHead[i] = new NNSG2dCellData();
				pCellDataArrayHead[i].parse1(arrayReader);
			}
			for (int i = 0; i < numCells; i++)
			{
				pCellDataArrayHead[i].parse2(arrayReader);
			}
			arrayReader.dispose();
		}
	}
}
