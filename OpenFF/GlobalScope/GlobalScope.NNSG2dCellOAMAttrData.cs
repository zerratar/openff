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

		/// <summary>
		/// PORT: the rectangle on the sheet, when it is not the drawn size. -1 means "the
		/// same as w and h", which is every cell the phone build shipped; SteamCells sets
		/// these when the phone geometry is drawn over one of Steam's larger sheets.
		/// </summary>
		public short srcW = -1;
		public short srcH = -1;

		public void set(short x, short y, short w, short h, short u, short v, short flags, short sw, short sh)
		{
			attr0 = (ushort)x; attr1 = (ushort)y; attr2 = (ushort)w; attr3 = (ushort)h;
			attr4 = (ushort)u; attr5 = (ushort)v; attr6 = (ushort)flags;
			srcW = sw; srcH = sh;
		}

		/// <summary>
		/// PORT: keeps the sheet rectangle the file gave (u, v and its size) as the source
		/// and places the drawn quad elsewhere, at another size. Used to draw the phone's
		/// layout from Steam's sheets.
		/// </summary>
		public void place(short x, short y, short w, short h, short flags)
		{
			if (srcW <= 0 || srcH <= 0)
			{
				srcW = (short)attr2; srcH = (short)attr3;
			}
			attr0 = (ushort)x; attr1 = (ushort)y; attr2 = (ushort)w; attr3 = (ushort)h; attr6 = (ushort)flags;
		}

		/// <summary>Source width and height: the explicit ones, or the drawn size.</summary>
		public void source(out short sw, out short sh)
		{
			sw = srcW > 0 ? srcW : (short)attr2;
			sh = srcH > 0 ? srcH : (short)attr3;
		}

		/// <summary>Nine shorts per entry - the seven attributes and the source size - for the BG path.</summary>
		public static void copyWithSource(short[] aDst, NNSG2dCellOAMAttrData[] aSrc, int count)
		{
			for (int i = 0; i < count; i++)
			{
				aSrc[i].copy(aDst, i * 9, 7);
				aSrc[i].source(out aDst[i * 9 + 7], out aDst[i * 9 + 8]);
			}
		}

		private void copy(short[] aDst, int at, int count)
		{
			short[] tmp = new short[7];
			copy(tmp, 14);
			for (int i = 0; i < count; i++)
			{
				aDst[at + i] = tmp[i];
			}
		}

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
