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
	public static partial class eld
	{
		public class SPathBodyHeader
		{
			public uint uiNumArryCount;

			public uint uiFrameTime;

			public uint uiFlag;

			public uint res;

			public ds.Vector4<int>[] m_pPathArry;

			public ds.Vector4<int>[] m_pFigureArry;

			public uint[] m_pTimingArry;

			public static explicit operator SPathBodyHeader(ArrayReader src)
			{
				SPathBodyHeader sPathBodyHeader = new SPathBodyHeader();
				sPathBodyHeader.uiNumArryCount = src.readUInt32();
				sPathBodyHeader.uiFrameTime = src.readUInt32();
				sPathBodyHeader.uiFlag = src.readUInt32();
				sPathBodyHeader.res = src.readUInt32();
				int num = (int)sPathBodyHeader.uiNumArryCount;
				sPathBodyHeader.m_pPathArry = new ds.Vector4<int>[num];
				for (int i = 0; i < num; i++)
				{
					sPathBodyHeader.m_pPathArry[i] = new ds.Vector4<int>();
					sPathBodyHeader.m_pPathArry[i].vx = src.readInt32();
					sPathBodyHeader.m_pPathArry[i].vy = src.readInt32();
					sPathBodyHeader.m_pPathArry[i].vz = src.readInt32();
					sPathBodyHeader.m_pPathArry[i].vw = src.readInt32();
				}
				num = (int)ARRY2POINT_NUM(sPathBodyHeader.uiNumArryCount);
				sPathBodyHeader.m_pFigureArry = new ds.Vector4<int>[num];
				for (int i = 0; i < num; i++)
				{
					sPathBodyHeader.m_pFigureArry[i] = new ds.Vector4<int>();
					sPathBodyHeader.m_pFigureArry[i].vx = src.readInt32();
					sPathBodyHeader.m_pFigureArry[i].vy = src.readInt32();
					sPathBodyHeader.m_pFigureArry[i].vz = src.readInt32();
					sPathBodyHeader.m_pFigureArry[i].vw = src.readInt32();
				}
				num = (int)sPathBodyHeader.uiNumArryCount;
				sPathBodyHeader.m_pTimingArry = new uint[num];
				src.read(sPathBodyHeader.m_pTimingArry, 0, num);
				return sPathBodyHeader;
			}
		}
	}
}
