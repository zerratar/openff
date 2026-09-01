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
	public class NNSG3dResMdlInfo
	{
		public byte sbcType;

		public byte scalingRule;

		public byte texMtxMode;

		public byte numNode;

		public byte numMat;

		public byte numShp;

		public byte firstUnusedMtxStackID;

		public byte dummy_;

		public int posScale;

		public int invPosScale;

		public ushort numVertex;

		public ushort numPolygon;

		public ushort numTriangle;

		public ushort numQuad;

		public short boxX;

		public short boxY;

		public short boxZ;

		public short boxW;

		public short boxH;

		public short boxD;

		public int boxPosScale;

		public int boxInvPosScale;

		public static explicit operator NNSG3dResMdlInfo(ArrayReader src)
		{
			NNSG3dResMdlInfo nNSG3dResMdlInfo = new NNSG3dResMdlInfo();
			nNSG3dResMdlInfo.sbcType = src.readByte();
			nNSG3dResMdlInfo.scalingRule = src.readByte();
			nNSG3dResMdlInfo.texMtxMode = src.readByte();
			nNSG3dResMdlInfo.numNode = src.readByte();
			nNSG3dResMdlInfo.numMat = src.readByte();
			nNSG3dResMdlInfo.numShp = src.readByte();
			nNSG3dResMdlInfo.firstUnusedMtxStackID = src.readByte();
			nNSG3dResMdlInfo.dummy_ = src.readByte();
			nNSG3dResMdlInfo.posScale = src.readInt32();
			nNSG3dResMdlInfo.invPosScale = src.readInt32();
			nNSG3dResMdlInfo.numVertex = src.readUInt16();
			nNSG3dResMdlInfo.numPolygon = src.readUInt16();
			nNSG3dResMdlInfo.numTriangle = src.readUInt16();
			nNSG3dResMdlInfo.numQuad = src.readUInt16();
			nNSG3dResMdlInfo.boxX = src.readInt16();
			nNSG3dResMdlInfo.boxY = src.readInt16();
			nNSG3dResMdlInfo.boxZ = src.readInt16();
			nNSG3dResMdlInfo.boxW = src.readInt16();
			nNSG3dResMdlInfo.boxH = src.readInt16();
			nNSG3dResMdlInfo.boxD = src.readInt16();
			nNSG3dResMdlInfo.boxPosScale = src.readInt32();
			nNSG3dResMdlInfo.boxInvPosScale = src.readInt32();
			if (nNSG3dResMdlInfo.boxZ < 0)
			{
				nNSG3dResMdlInfo.boxZ *= -1;
			}
			return nNSG3dResMdlInfo;
		}
	}
}
