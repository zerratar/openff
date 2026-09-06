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
	public static partial class wld
	{
							public class AreaDataHeader
							{
								public sbyte[] ID_ = new sbyte[4];

								public int numMarker_;

								public string imgFname_;

								public int orgX_;

								public int orgZ_;

								public int rangeX_;

								public int rangeZ_;

								public MarkerData[] m_aMarkerData;

								public static explicit operator AreaDataHeader(ArrayReader src)
								{
									AreaDataHeader areaDataHeader = new AreaDataHeader();
									byte[] array = new byte[16];
									src.read(areaDataHeader.ID_, 0, 4);
									areaDataHeader.numMarker_ = src.readInt32();
									src.read(array, 0, array.Length);
									areaDataHeader.imgFname_ = StringUtil.createString(array);
									areaDataHeader.orgX_ = src.readInt32();
									areaDataHeader.orgZ_ = src.readInt32();
									areaDataHeader.rangeX_ = src.readInt32();
									areaDataHeader.rangeZ_ = src.readInt32();
									areaDataHeader.m_aMarkerData = new MarkerData[areaDataHeader.numMarker_];
									for (int i = 0; i < areaDataHeader.numMarker_; i++)
									{
										areaDataHeader.m_aMarkerData[i] = (MarkerData)src;
									}
									return areaDataHeader;
								}
							}
	}
}
