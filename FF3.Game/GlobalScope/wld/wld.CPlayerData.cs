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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class wld
	{
							public class CPlayerData
							{
								private byte m_PlayCharacterIndex;

								private byte m_FrontPlayerID;

								private SHoldCharacterData[] m_HoldData = new SHoldCharacterData[24];

								public CPlayerData()
								{
									for (int i = 0; i < m_HoldData.Length; i++)
									{
										m_HoldData[i] = new SHoldCharacterData();
									}
								}

								public void initialize()
								{
									m_PlayCharacterIndex = 0;
									m_FrontPlayerID = 0;
									for (int i = 0; i < 24; i++)
									{
										m_HoldData[i].initialize();
									}
								}

								public void setPlayCharacterIndex(int idx)
								{
									m_PlayCharacterIndex = (byte)idx;
								}

								public void setFrontPlayerID(int _id)
								{
									m_FrontPlayerID = (byte)_id;
								}

								public void setHoldData(int _Index, VecFx32 _Position, VecFx32 _Rotation)
								{
									m_HoldData[_Index].m_Position.copy(_Position);
									m_HoldData[_Index].m_Rotation.copy(_Rotation);
								}

								public int getPlayCharacterIndex()
								{
									return m_PlayCharacterIndex;
								}

								public int getFrontPlayerID()
								{
									return m_FrontPlayerID;
								}

								public SHoldCharacterData getHoldData(int _Index)
								{
									return m_HoldData[_Index];
								}

								public void setDefault()
								{
									m_PlayCharacterIndex = 0;
									m_FrontPlayerID = 0;
								}

								public void parse(ArrayReader reader)
								{
									m_PlayCharacterIndex = reader.readByte();
									m_FrontPlayerID = reader.readByte();
									for (int i = 0; (long)i < 24L; i++)
									{
										m_HoldData[i].parse(reader);
									}
								}

								public void store(ArrayWriter writer)
								{
									writer.writeByte(m_PlayCharacterIndex);
									writer.writeByte(m_FrontPlayerID);
									for (int i = 0; (long)i < 24L; i++)
									{
										m_HoldData[i].store(writer);
									}
								}
							}
	}
}
