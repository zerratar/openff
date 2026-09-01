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
							public class CWorldOutSideData
							{
								public enum FLAG_ENCOUNT
								{
									ENCOUNT_NON,
									ENCOUNT_ACCEPT,
									ENCOUNT_FORCE
								}

								public const FLAG_ENCOUNT ENCOUNT_NON = FLAG_ENCOUNT.ENCOUNT_NON;

								public const FLAG_ENCOUNT ENCOUNT_ACCEPT = FLAG_ENCOUNT.ENCOUNT_ACCEPT;

								public const FLAG_ENCOUNT ENCOUNT_FORCE = FLAG_ENCOUNT.ENCOUNT_FORCE;

								public static CWorldOutSideData m_Instance = new CWorldOutSideData();

								private CPlayerData m_PlayerData = new CPlayerData();

								private CVehicleData m_VehicleData = new CVehicleData();

								private CMapData m_MapData = new CMapData();

								private CSoundData m_SoundData = new CSoundData();

								private byte m_encountFlag;

								private bool m_updataFlag;

								public void initialize()
								{
									PlayerData().initialize();
									VehicleData().initialize();
									MapData().initialize();
									SoundData().initialize();
									m_encountFlag = 0;
									m_encountFlag |= 1;
									m_updataFlag = false;
								}

								public void initialize2()
								{
									MapData().initialize2();
									MapData().setRideOnChokobo(b: false);
								}

								public void setCanEncount(bool b)
								{
									if (b)
									{
										m_encountFlag |= 1;
									}
									else
									{
										m_encountFlag &= 254;
									}
								}

								public void setUpData(bool b)
								{
									m_updataFlag = b;
								}

								public void setForceEncount(bool b)
								{
									if (b)
									{
										m_encountFlag |= 2;
									}
									else
									{
										m_encountFlag &= 253;
									}
								}

								public CWorldOutSideData()
								{
									initialize();
								}

								~CWorldOutSideData()
								{
									initialize();
								}

								public static CWorldOutSideData getInstance()
								{
									return m_Instance;
								}

								public static void getInstance_set(CWorldOutSideData arg0)
								{
									m_Instance = arg0;
								}

								public CPlayerData PlayerData()
								{
									return m_PlayerData;
								}

								public CVehicleData VehicleData()
								{
									return m_VehicleData;
								}

								public CMapData MapData()
								{
									return m_MapData;
								}

								public CSoundData SoundData()
								{
									return m_SoundData;
								}

								public bool canEncount()
								{
									return (m_encountFlag & 1) != 0;
								}

								public bool canUpdata()
								{
									return m_updataFlag;
								}

								public bool isForceEncount()
								{
									return (m_encountFlag & 2) != 0;
								}

								public void setDefault()
								{
									m_PlayerData.setDefault();
									m_VehicleData.setDefault();
									m_MapData.setDefault();
									m_SoundData.setDefault();
									m_encountFlag = 0;
								}

								public void parse(ArrayReader reader)
								{
									m_PlayerData.parse(reader);
									m_VehicleData.parse(reader);
									m_MapData.parse(reader);
									m_SoundData.parse(reader);
									m_encountFlag = reader.readByte();
								}

								public void store(ArrayWriter writer)
								{
									m_PlayerData.store(writer);
									m_VehicleData.store(writer);
									m_MapData.store(writer);
									m_SoundData.store(writer);
									writer.writeByte(m_encountFlag);
								}
							}
	}
}
