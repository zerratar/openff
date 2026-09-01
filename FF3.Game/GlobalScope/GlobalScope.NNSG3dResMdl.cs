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
	public class NNSG3dResMdl
	{
		public uint size;

		public uint ofsSbc;

		public uint ofsMat;

		public uint ofsShp;

		public uint ofsEvpMtx;

		public NNSG3dResMdlInfo info;

		public NNSG3dResNodeInfo nodeInfo;

		public byte[] sbc;

		public NNSG3dResMat mat;

		public NNSG3dResShp shp;

		public MtxFx43[] mtx;

		public static explicit operator NNSG3dResMdl(ArrayReader src)
		{
			NNSG3dResMdl nNSG3dResMdl = new NNSG3dResMdl();
			long position = src.getPosition();
			nNSG3dResMdl.size = src.readUInt32();
			nNSG3dResMdl.ofsSbc = src.readUInt32();
			nNSG3dResMdl.ofsMat = src.readUInt32();
			nNSG3dResMdl.ofsShp = src.readUInt32();
			nNSG3dResMdl.ofsEvpMtx = src.readUInt32();
			nNSG3dResMdl.info = (NNSG3dResMdlInfo)src;
			nNSG3dResMdl.nodeInfo = (NNSG3dResNodeInfo)src;
			if (nNSG3dResMdl.ofsSbc != 0)
			{
				src.setPosition(position + nNSG3dResMdl.ofsSbc);
				int num = (int)(nNSG3dResMdl.ofsMat - nNSG3dResMdl.ofsSbc);
				nNSG3dResMdl.sbc = new byte[num];
				src.read(nNSG3dResMdl.sbc, 0, num);
			}
			if (nNSG3dResMdl.ofsMat != 0)
			{
				src.setPosition(position + nNSG3dResMdl.ofsMat);
				nNSG3dResMdl.mat = (NNSG3dResMat)src;
			}
			if (nNSG3dResMdl.ofsShp != 0)
			{
				src.setPosition(position + nNSG3dResMdl.ofsShp);
				nNSG3dResMdl.shp = (NNSG3dResShp)src;
				int i;
				for (i = 0; 4096 << i < nNSG3dResMdl.info.posScale; i++)
				{
				}
				nNSG3dResMdl.shp.preBuild(i);
			}
			if (nNSG3dResMdl.ofsEvpMtx != 0)
			{
				nNSG3dResMdl.mtx = new MtxFx43[nNSG3dResMdl.nodeInfo.dict.numEntry];
				for (int j = 0; j < nNSG3dResMdl.nodeInfo.dict.numEntry; j++)
				{
					src.setPosition(position + nNSG3dResMdl.ofsEvpMtx + j * 84);
					if (src.rest() >= 84)
					{
						nNSG3dResMdl.mtx[j] = (MtxFx43)src;
					}
					else
					{
						nNSG3dResMdl.mtx[j] = new MtxFx43();
					}
				}
			}
			return nNSG3dResMdl;
		}
	}
}
