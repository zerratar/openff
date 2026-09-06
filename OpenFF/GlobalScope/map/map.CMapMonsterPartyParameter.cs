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
							public class CMapMonsterPartyParameter
							{
								private short[,] m_MonsterParty = new short[MAP_MONSTER_PARTY_GROUP_MAX, MAP_MONSTER_PARTY_PARAM_MAX];

								public short MonsterParty(int group, int index)
								{
									return m_MonsterParty[group, index];
								}

								public static CMapMonsterPartyParameter[] ChainPointer(byte[] abyData, int iId)
								{
									ArrayReader arrayReader = new ArrayReader(abyData);
									arrayReader.skip(16L);
									arrayReader.skip(8 * iId);
									uint num = arrayReader.readUInt32();
									uint num2 = arrayReader.readUInt32();
									uint num3 = num2 / 40;
									arrayReader.setPosition(num);
									CMapMonsterPartyParameter[] array = new CMapMonsterPartyParameter[num3];
									for (int i = 0; i < num3; i++)
									{
										array[i] = new CMapMonsterPartyParameter();
										array[i].parse(arrayReader);
									}
									arrayReader.dispose();
									return array;
								}

								public void parse(ArrayReader reader)
								{
									for (int i = 0; i < MAP_MONSTER_PARTY_GROUP_MAX; i++)
									{
										for (int j = 0; j < MAP_MONSTER_PARTY_PARAM_MAX; j++)
										{
											m_MonsterParty[i, j] = reader.readInt16();
										}
									}
								}
							}
	}
}
