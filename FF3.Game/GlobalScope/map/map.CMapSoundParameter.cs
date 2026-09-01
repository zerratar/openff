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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class map
	{
							public class CMapSoundParameter
							{
								private short m_BGMIndex;

								private short m_CheckFlag;

								private short m_ChangeBGMIndex;

								public short BGMIndex()
								{
									return m_BGMIndex;
								}

								public void BGMIndex_set(short arg0)
								{
									m_BGMIndex = arg0;
								}

								public short CheckFlag()
								{
									return m_CheckFlag;
								}

								public short ChangeBGMIndex()
								{
									return m_ChangeBGMIndex;
								}

								public static CMapSoundParameter[] ChainPointer(byte[] abyData, int iId)
								{
									ArrayReader arrayReader = new ArrayReader(abyData);
									arrayReader.skip(16L);
									arrayReader.skip(8 * iId);
									uint num = arrayReader.readUInt32();
									uint num2 = arrayReader.readUInt32();
									uint num3 = num2 / 6;
									arrayReader.setPosition(num);
									CMapSoundParameter[] array = new CMapSoundParameter[num3];
									for (int i = 0; i < num3; i++)
									{
										array[i] = new CMapSoundParameter();
										array[i].parse(arrayReader);
									}
									arrayReader.dispose();
									return array;
								}

								public void parse(ArrayReader reader)
								{
									m_BGMIndex = reader.readInt16();
									m_CheckFlag = reader.readInt16();
									m_ChangeBGMIndex = reader.readInt16();
								}
							}
	}
}
