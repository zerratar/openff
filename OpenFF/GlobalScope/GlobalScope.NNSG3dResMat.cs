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
	public class NNSG3dResMat
	{
		public ushort ofsDictTexToMatList;

		public ushort ofsDictPlttToMatList;

		public NNSG3dResDict dict;

		public NNSG3dResDict dictTex;

		public NNSG3dResDict dictPal;

		public NNSG3dResMatData[] mat;

		public byte[][] listT;

		public byte[][] listP;

		public static explicit operator NNSG3dResMat(ArrayReader src)
		{
			NNSG3dResMat nNSG3dResMat = new NNSG3dResMat();
			long position = src.getPosition();
			nNSG3dResMat.ofsDictTexToMatList = src.readUInt16();
			nNSG3dResMat.ofsDictPlttToMatList = src.readUInt16();
			nNSG3dResMat.dict = (NNSG3dResDict)src;
			nNSG3dResMat.dictTex = (NNSG3dResDict)src;
			nNSG3dResMat.dictPal = (NNSG3dResDict)src;
			nNSG3dResMat.mat = new NNSG3dResMatData[nNSG3dResMat.dict.numEntry];
			for (int i = 0; i < nNSG3dResMat.dict.numEntry; i++)
			{
				src.setPosition(position + nNSG3dResMat.dict.entry.getU32(i));
				nNSG3dResMat.mat[i] = (NNSG3dResMatData)src;
			}
			nNSG3dResMat.listT = new byte[nNSG3dResMat.dictTex.numEntry][];
			for (int i = 0; i < nNSG3dResMat.dictTex.numEntry; i++)
			{
				uint u = nNSG3dResMat.dictTex.entry.getU32(i);
				int num = (int)(u & 0xFFFF);
				int num2 = (int)((u & 0xFFFF0000u) >> 16);
				nNSG3dResMat.listT[i] = new byte[num2];
				src.setPosition(position + num);
				src.read(nNSG3dResMat.listT[i], 0, nNSG3dResMat.listT[i].Length);
			}
			nNSG3dResMat.listP = new byte[nNSG3dResMat.dictPal.numEntry][];
			for (int i = 0; i < nNSG3dResMat.dictPal.numEntry; i++)
			{
				uint u = nNSG3dResMat.dictPal.entry.getU32(i);
				int num = (int)(u & 0xFFFF);
				int num2 = (int)((u & 0xFFFF0000u) >> 16);
				nNSG3dResMat.listP[i] = new byte[num2];
				src.setPosition(position + num);
				src.read(nNSG3dResMat.listP[i], 0, nNSG3dResMat.listP[i].Length);
			}
			return nNSG3dResMat;
		}
	}
}
