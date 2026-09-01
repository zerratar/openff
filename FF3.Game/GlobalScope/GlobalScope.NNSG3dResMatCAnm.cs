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
	public class NNSG3dResMatCAnm
	{
		public class NNSG3dResMatCAnmP
		{
			public uint[] flag;

			public byte[][] table8;

			public ushort[][] table16;

			public NNSG3dResMatCAnmP(uint[] _flag, byte[][] _table8, ushort[][] _table16)
			{
				flag = _flag;
				table8 = _table8;
				table16 = _table16;
			}
		}

		public NNSG3dResAnmHeader anmHeader;

		public ushort numFrame;

		public ushort flag;

		public NNSG3dResDict dict;

		public NNSG3dResMatCAnmP[] p;

		public static explicit operator NNSG3dResMatCAnm(ArrayReader src)
		{
			NNSG3dResMatCAnm nNSG3dResMatCAnm = new NNSG3dResMatCAnm();
			long position = src.getPosition();
			nNSG3dResMatCAnm.anmHeader = (NNSG3dResAnmHeader)src;
			nNSG3dResMatCAnm.numFrame = src.readUInt16();
			nNSG3dResMatCAnm.flag = src.readUInt16();
			nNSG3dResMatCAnm.dict = (NNSG3dResDict)src;
			nNSG3dResMatCAnm.p = new NNSG3dResMatCAnmP[nNSG3dResMatCAnm.dict.numEntry];
			for (int i = 0; i < nNSG3dResMatCAnm.dict.numEntry; i++)
			{
				uint[] array = new uint[5];
				byte[][] array2 = new byte[5][];
				ushort[][] array3 = new ushort[5][];
				for (int j = 0; j < 5; j++)
				{
					array[j] = nNSG3dResMatCAnm.dict.entry.getU32(i, j * 4);
					if ((array[j] & 0x20000000) == 0)
					{
						long position2 = src.getPosition();
						src.setPosition(position + (array[j] & 0xFFFF));
						if (j == 4)
						{
							array2[j] = new byte[nNSG3dResMatCAnm.numFrame];
							src.read(array2[j], 0, nNSG3dResMatCAnm.numFrame);
						}
						else
						{
							array3[j] = new ushort[nNSG3dResMatCAnm.numFrame];
							src.read(array3[j], 0, nNSG3dResMatCAnm.numFrame);
						}
						src.setPosition(position2);
					}
				}
				nNSG3dResMatCAnm.p[i] = new NNSG3dResMatCAnmP(array, array2, array3);
			}
			nNSG3dResMatCAnm.anmHeader.m_Anm = nNSG3dResMatCAnm;
			return nNSG3dResMatCAnm;
		}
	}
}
