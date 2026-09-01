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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public class NNSG2dCellOAMAttrData
	{
		private ushort attr0;

		private ushort attr1;

		private ushort attr2;

		private ushort attr3;

		private ushort attr4;

		private ushort attr5;

		private ushort attr6;

		public void parse(ArrayReader reader)
		{
			attr0 = reader.readUInt16();
			attr1 = reader.readUInt16();
			attr2 = reader.readUInt16();
			attr3 = reader.readUInt16();
			attr4 = reader.readUInt16();
			attr5 = reader.readUInt16();
			attr6 = reader.readUInt16();
		}

		public void copy(short[] aDst, int iSize)
		{
			int num = iSize / 2;
			int num2 = 0;
			aDst[num2++] = (short)attr0;
			if (num2 >= num)
			{
				return;
			}
			aDst[num2++] = (short)attr1;
			if (num2 >= num)
			{
				return;
			}
			aDst[num2++] = (short)attr2;
			if (num2 >= num)
			{
				return;
			}
			aDst[num2++] = (short)attr3;
			if (num2 >= num)
			{
				return;
			}
			aDst[num2++] = (short)attr4;
			if (num2 < num)
			{
				aDst[num2++] = (short)attr5;
				if (num2 < num)
				{
					aDst[num2++] = (short)attr6;
				}
			}
		}

		public static void copy(short[] aDst, NNSG2dCellOAMAttrData[] aSrc, int iSize)
		{
			int num = aSrc.Length;
			int num2 = iSize / 2;
			int num3 = 0;
			for (int i = 0; i < num; i++)
			{
				aDst[num3++] = (short)aSrc[i].attr0;
				if (num3 >= num2)
				{
					break;
				}
				aDst[num3++] = (short)aSrc[i].attr1;
				if (num3 >= num2)
				{
					break;
				}
				aDst[num3++] = (short)aSrc[i].attr2;
				if (num3 >= num2)
				{
					break;
				}
				aDst[num3++] = (short)aSrc[i].attr3;
				if (num3 >= num2)
				{
					break;
				}
				aDst[num3++] = (short)aSrc[i].attr4;
				if (num3 >= num2)
				{
					break;
				}
				aDst[num3++] = (short)aSrc[i].attr5;
				if (num3 >= num2)
				{
					break;
				}
				aDst[num3++] = (short)aSrc[i].attr6;
				if (num3 >= num2)
				{
					break;
				}
			}
		}
	}
}
