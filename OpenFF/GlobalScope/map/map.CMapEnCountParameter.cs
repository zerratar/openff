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
	public static partial class map
	{
							public class CMapEnCountParameter
							{
								private short m_AreaLevel;

								private ushort _pad0;

								private float[] m_EncountRevise = new float[MAP_ENCOUNT_REVISE_PARAM_MAX];

								public short AreaLevel()
								{
									return m_AreaLevel;
								}

								public float EncountRevise(int index)
								{
									return m_EncountRevise[index];
								}

								public static CMapEnCountParameter[] ChainPointer(byte[] abyData, int iId)
								{
									ArrayReader arrayReader = new ArrayReader(abyData);
									arrayReader.skip(16L);
									arrayReader.skip(8 * iId);
									uint num = arrayReader.readUInt32();
									uint num2 = arrayReader.readUInt32();
									uint num3 = 1u;
									arrayReader.setPosition(num);
									CMapEnCountParameter[] array = new CMapEnCountParameter[num3];
									for (int i = 0; i < num3; i++)
									{
										array[i] = new CMapEnCountParameter();
										array[i].parse(arrayReader, (int)((num2 - 4) / 4));
									}
									arrayReader.dispose();
									return array;
								}

								public void parse(ArrayReader reader, int iReviseCount)
								{
									m_AreaLevel = reader.readInt16();
									_pad0 = reader.readUInt16();
									reader.read(m_EncountRevise, 0, iReviseCount);
								}
							}
	}
}
