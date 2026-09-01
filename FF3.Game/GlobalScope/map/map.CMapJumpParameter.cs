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
							public class CMapJumpParameter
							{
								private int[] m_PlPos = new int[3];

								private int m_PlRot;

								private string m_NextMapName;

								private int m_NextMapIndex;

								private int m_ConditionFlag;

								private int m_Kind;

								public string NextMapName()
								{
									string text = "";
									int num = m_NextMapName.IndexOf('#');
									if (num >= 0)
									{
										return m_NextMapName.Substring(0, num);
									}
									return m_NextMapName;
								}

								public int ModelNo()
								{
									int num = m_NextMapName.IndexOf('#');
									if (num >= 0)
									{
										num++;
										return atoi(m_NextMapName.Substring(num));
									}
									return -1;
								}

								public int PlPos(int index)
								{
									return m_PlPos[index];
								}

								public int PlRot()
								{
									return m_PlRot;
								}

								public int NextMapIndex()
								{
									return m_NextMapIndex;
								}

								public int ConditionFlag()
								{
									return m_ConditionFlag;
								}

								public int Kind()
								{
									return m_Kind;
								}

								public void Kind_set(int arg0)
								{
									m_Kind = arg0;
								}

								public static CMapJumpParameter[] ChainPointer(byte[] abyData, int iId)
								{
									ArrayReader arrayReader = new ArrayReader(abyData);
									arrayReader.skip(16L);
									arrayReader.skip(8 * iId);
									uint num = arrayReader.readUInt32();
									uint num2 = arrayReader.readUInt32();
									uint num3 = num2 / 44;
									arrayReader.setPosition(num);
									CMapJumpParameter[] array = new CMapJumpParameter[16];
									int i;
									for (i = 0; i < num3; i++)
									{
										array[i] = new CMapJumpParameter();
										array[i].parse(arrayReader);
									}
									for (; i < 16; i++)
									{
										array[i] = new CMapJumpParameter();
									}
									arrayReader.dispose();
									return array;
								}

								public void parse(ArrayReader reader)
								{
									byte[] array = new byte[16];
									reader.read(m_PlPos, 0, 3);
									m_PlRot = reader.readInt32();
									reader.read(array, 0, 16);
									m_NextMapName = StringUtil.createString(array);
									m_NextMapIndex = reader.readInt32();
									m_ConditionFlag = reader.readInt32();
									m_Kind = reader.readInt32();
								}
							}
	}
}
