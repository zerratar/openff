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
	public class NNSG3dResTexSRTAnm
	{
		public class NNSG3dResTexSRTAnmP
		{
			public uint[] flag;

			public uint[] ex;

			public short[][] table16;

			public int[][] table32;

			public NNSG3dResTexSRTAnmP(uint[] _flag, uint[] _ex, short[][] _table16, int[][] _table32)
			{
				flag = _flag;
				ex = _ex;
				table16 = _table16;
				table32 = _table32;
			}
		}

		public NNSG3dResAnmHeader anmHeader;

		public ushort numFrame;

		public byte flag;

		public byte texMtxMode;

		public NNSG3dResDict dict;

		public NNSG3dResTexSRTAnmP[] p;

		public static explicit operator NNSG3dResTexSRTAnm(ArrayReader src)
		{
			NNSG3dResTexSRTAnm nNSG3dResTexSRTAnm = new NNSG3dResTexSRTAnm();
			long position = src.getPosition();
			nNSG3dResTexSRTAnm.anmHeader = (NNSG3dResAnmHeader)src;
			nNSG3dResTexSRTAnm.numFrame = src.readUInt16();
			nNSG3dResTexSRTAnm.flag = src.readByte();
			nNSG3dResTexSRTAnm.texMtxMode = src.readByte();
			nNSG3dResTexSRTAnm.dict = (NNSG3dResDict)src;
			nNSG3dResTexSRTAnm.p = new NNSG3dResTexSRTAnmP[nNSG3dResTexSRTAnm.dict.numEntry];
			for (int i = 0; i < nNSG3dResTexSRTAnm.dict.numEntry; i++)
			{
				uint[] array = new uint[5];
				uint[] array2 = new uint[5];
				short[][] array3 = new short[5][];
				int[][] array4 = new int[5][];
				for (int j = 0; j < 5; j++)
				{
					array[j] = nNSG3dResTexSRTAnm.dict.entry.getU32(i, j * 8);
					array2[j] = nNSG3dResTexSRTAnm.dict.entry.getU32(i, j * 8 + 4);
					if ((array[j] & 0x20000000) == 0)
					{
						long position2 = src.getPosition();
						src.setPosition(position + array2[j]);
						if ((array[j] & 0x10000000) != 0)
						{
							array3[j] = new short[nNSG3dResTexSRTAnm.numFrame];
							src.read(array3[j], 0, nNSG3dResTexSRTAnm.numFrame);
						}
						else
						{
							array4[j] = new int[nNSG3dResTexSRTAnm.numFrame];
							src.read(array4[j], 0, nNSG3dResTexSRTAnm.numFrame);
						}
						src.setPosition(position2);
					}
				}
				nNSG3dResTexSRTAnm.p[i] = new NNSG3dResTexSRTAnmP(array, array2, array3, array4);
			}
			nNSG3dResTexSRTAnm.anmHeader.m_Anm = nNSG3dResTexSRTAnm;
			return nNSG3dResTexSRTAnm;
		}
	}
}
