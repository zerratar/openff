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
	public static partial class map
	{
							public class CMapLandFormParameter
							{
								private short[] m_LandAttr = new short[MAP_LANDFORM_PARAM_MAX];

								private short[] m_BattleFieldIndex = new short[MAP_ENCOUNT_BTLFIELD_PARAM_MAX];

								public short LandAttr(int index)
								{
									if (index < 0)
									{
										index = 0;
									}
									return m_LandAttr[index];
								}

								public short BattleFieldIndex(int index)
								{
									if (index < 0)
									{
										index = 0;
									}
									return m_BattleFieldIndex[index];
								}

								public static CMapLandFormParameter[] ChainPointer(byte[] abyData, int iId)
								{
									ArrayReader arrayReader = new ArrayReader(abyData);
									arrayReader.skip(16L);
									arrayReader.skip(8 * iId);
									uint num = arrayReader.readUInt32();
									uint num2 = arrayReader.readUInt32();
									uint num3 = num2 / 48;
									arrayReader.setPosition(num);
									CMapLandFormParameter[] array = new CMapLandFormParameter[num3];
									for (int i = 0; i < num3; i++)
									{
										array[i] = new CMapLandFormParameter();
										array[i].parse(arrayReader);
									}
									arrayReader.dispose();
									return array;
								}

								public void parse(ArrayReader reader)
								{
									reader.read(m_LandAttr, 0, MAP_LANDFORM_PARAM_MAX);
									reader.read(m_BattleFieldIndex, 0, MAP_ENCOUNT_BTLFIELD_PARAM_MAX);
								}
							}
	}
}
