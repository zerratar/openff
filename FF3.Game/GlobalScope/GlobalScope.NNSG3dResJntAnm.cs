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
	public class NNSG3dResJntAnm
	{
		public class NNSG3dResJntAnmP
		{
			public uint flag;

			public uint[] p;

			public short[][] tableT16;

			public int[][] tableT32;

			public ushort[] tableR;

			public short[][] tableS16;

			public int[][] tableS32;

			public NNSG3dResJntAnmP(uint _flag, uint[] _buf, int _iBufCount, short[][] _tableT16, int[][] _tableT32, ushort[] _tableR, short[][] _tableS16, int[][] _tableS32)
			{
				flag = _flag;
				p = new uint[_iBufCount];
				for (int i = 0; i < _iBufCount; i++)
				{
					p[i] = _buf[i];
				}
				tableT16 = _tableT16;
				tableT32 = _tableT32;
				tableR = _tableR;
				tableS16 = _tableS16;
				tableS32 = _tableS32;
			}
		}

		public NNSG3dResAnmHeader anmHeader;

		public ushort numFrame;

		public ushort numNode;

		public uint flag;

		public uint ofsRot3;

		public uint ofsRot5;

		public ushort[] ofsTag;

		public NNSG3dResJntAnmP[] p;

		public short[] rot3;

		public short[] rot5;

		public static explicit operator NNSG3dResJntAnm(ArrayReader src)
		{
			NNSG3dResJntAnm nNSG3dResJntAnm = new NNSG3dResJntAnm();
			long position = src.getPosition();
			uint[] array = new uint[256];
			uint num = 0u;
			uint num2 = 0u;
			nNSG3dResJntAnm.anmHeader = (NNSG3dResAnmHeader)src;
			nNSG3dResJntAnm.numFrame = src.readUInt16();
			nNSG3dResJntAnm.numNode = src.readUInt16();
			nNSG3dResJntAnm.flag = src.readUInt32();
			nNSG3dResJntAnm.ofsRot3 = src.readUInt32();
			nNSG3dResJntAnm.ofsRot5 = src.readUInt32();
			nNSG3dResJntAnm.ofsTag = new ushort[nNSG3dResJntAnm.numNode];
			src.read(nNSG3dResJntAnm.ofsTag, 0, nNSG3dResJntAnm.numNode);
			nNSG3dResJntAnm.p = new NNSG3dResJntAnmP[nNSG3dResJntAnm.numNode];
			for (int i = 0; i < nNSG3dResJntAnm.numNode; i++)
			{
				src.setPosition(position + nNSG3dResJntAnm.ofsTag[i]);
				uint num3 = src.readUInt32();
				int num4 = 0;
				short[][] array2 = new short[3][];
				int[][] array3 = new int[3][];
				ushort[] array4 = null;
				short[][] array5 = new short[3][];
				int[][] array6 = new int[3][];
				if ((num3 & 1) == 0)
				{
					if ((num3 & 2) == 0 && (num3 & 4) == 0)
					{
						for (int j = 0; j < 3; j++)
						{
							if ((num3 & (uint)(8 << j)) != 0)
							{
								array[num4++] = src.readUInt32();
								continue;
							}
							uint num5 = (array[num4++] = src.readUInt32());
							uint num6 = (array[num4++] = src.readUInt32());
							long position2 = src.getPosition();
							src.setPosition(position + num6);
							if (num2 == 0)
							{
								num2 = (uint)(position + num6);
							}
							if ((num5 & 0x20000000) != 0)
							{
								array2[j] = new short[nNSG3dResJntAnm.numFrame];
								src.read(array2[j], 0, nNSG3dResJntAnm.numFrame);
							}
							else
							{
								array3[j] = new int[nNSG3dResJntAnm.numFrame];
								src.read(array3[j], 0, nNSG3dResJntAnm.numFrame);
							}
							src.setPosition(position2);
						}
					}
					if ((num3 & 0x40) == 0 && (num3 & 0x80) == 0)
					{
						if ((num3 & 0x100) != 0)
						{
							array[num4++] = src.readUInt16();
							src.readUInt16();
						}
						else
						{
							uint num5 = (array[num4++] = src.readUInt32());
							uint num6 = (array[num4++] = src.readUInt32());
							long position2 = src.getPosition();
							src.setPosition(position + num6);
							if (num2 == 0)
							{
								num2 = (uint)(position + num6);
							}
							array4 = new ushort[nNSG3dResJntAnm.numFrame];
							src.read(array4, 0, nNSG3dResJntAnm.numFrame);
							src.setPosition(position2);
						}
					}
					if ((num3 & 0x200) == 0 && (num3 & 0x400) == 0)
					{
						for (int j = 0; j < 3; j++)
						{
							if ((num3 & (uint)(2048 << j)) != 0)
							{
								array[num4++] = src.readUInt32();
								num4++;
								src.readUInt32();
								continue;
							}
							uint num5 = (array[num4++] = src.readUInt32());
							uint num6 = (array[num4++] = src.readUInt32());
							long position2 = src.getPosition();
							src.setPosition(position + num6);
							if (num2 == 0)
							{
								num2 = (uint)(position + num6);
							}
							if ((num5 & 0x20000000) != 0)
							{
								array5[j] = new short[nNSG3dResJntAnm.numFrame * 2];
								src.read(array5[j], 0, nNSG3dResJntAnm.numFrame * 2);
							}
							else
							{
								array6[j] = new int[nNSG3dResJntAnm.numFrame * 2];
								src.read(array6[j], 0, nNSG3dResJntAnm.numFrame * 2);
							}
							src.setPosition(position2);
						}
					}
				}
				nNSG3dResJntAnm.p[i] = new NNSG3dResJntAnmP(num3, array, num4, array2, array3, array4, array5, array6);
			}
			num = nNSG3dResJntAnm.ofsRot5;
			int num7 = (int)((num - nNSG3dResJntAnm.ofsRot3) / 2);
			if (num7 != 0 && num != 0)
			{
				nNSG3dResJntAnm.rot3 = new short[num7];
				src.setPosition(position + nNSG3dResJntAnm.ofsRot3);
				src.read(nNSG3dResJntAnm.rot3, 0, num7);
			}
			if (num2 == 0)
			{
				num2 = (uint)(nNSG3dResJntAnm.ofsRot5 + src.rest());
			}
			num7 = (int)((num2 - nNSG3dResJntAnm.ofsRot5) / 2);
			if (num7 != 0 && num2 != 0)
			{
				nNSG3dResJntAnm.rot5 = new short[num7];
				src.setPosition(position + nNSG3dResJntAnm.ofsRot5);
				src.read(nNSG3dResJntAnm.rot5, 0, num7);
			}
			nNSG3dResJntAnm.anmHeader.m_Anm = nNSG3dResJntAnm;
			return nNSG3dResJntAnm;
		}
	}
}
