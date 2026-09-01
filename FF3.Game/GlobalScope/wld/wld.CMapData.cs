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
	public static partial class wld
	{
							public class CMapData
							{
								public enum SP_MAP_TYPE
								{
									SP_MAP_DEFAULT,
									SP_MAP_AIR,
									SP_MAP_DEEPSEA,
									SP_MAP_INVINSIBLE
								}

								public enum MAP_ENABLE_FLAG
								{
									MAP_ENABLE_RIDEONCHOKOBO = 1,
									MAP_ENABLE_BACKUPPOSJUMP = 2,
									MAP_ENABLE_MAPJUMPENABLE = 4
								}

								public const SP_MAP_TYPE SP_MAP_DEFAULT = SP_MAP_TYPE.SP_MAP_DEFAULT;

								public const SP_MAP_TYPE SP_MAP_AIR = SP_MAP_TYPE.SP_MAP_AIR;

								public const SP_MAP_TYPE SP_MAP_DEEPSEA = SP_MAP_TYPE.SP_MAP_DEEPSEA;

								public const SP_MAP_TYPE SP_MAP_INVINSIBLE = SP_MAP_TYPE.SP_MAP_INVINSIBLE;

								public const MAP_ENABLE_FLAG MAP_ENABLE_RIDEONCHOKOBO = MAP_ENABLE_FLAG.MAP_ENABLE_RIDEONCHOKOBO;

								public const MAP_ENABLE_FLAG MAP_ENABLE_BACKUPPOSJUMP = MAP_ENABLE_FLAG.MAP_ENABLE_BACKUPPOSJUMP;

								public const MAP_ENABLE_FLAG MAP_ENABLE_MAPJUMPENABLE = MAP_ENABLE_FLAG.MAP_ENABLE_MAPJUMPENABLE;

								private int m_ColFlag;

								private string m_NowMapName;

								private string m_NowShopMapName;

								private string m_BeforeFieldMapName;

								private sbyte m_BeforeFieldMapJumpIndex;

								private string m_BeforeTownMapName;

								private sbyte m_BeforeTownMapJumpIndex;

								private sbyte m_NextMapIndex;

								private sbyte m_PreBGMIndex;

								private sbyte m_WallIndex;

								private sbyte m_MonsterPartyIndex;

								private sbyte m_MapJumpIndex;

								private sbyte m_BattleMapIndex;

								private VecFx32 m_MapJumpNormal = new VecFx32();

								private SP_MAP_TYPE m_spMapType;

								private SHoldDoorData HoldDoorData = new SHoldDoorData();

								private sbyte m_enableFlag;

								private int m_commonMdlNo;

								private int[] m_reserved = new int[7];

								public void initialize()
								{
									m_NowMapName = "";
									m_NowShopMapName = "";
									m_BeforeFieldMapName = "";
									m_BeforeFieldMapJumpIndex = 0;
									m_BeforeTownMapName = "";
									m_BeforeTownMapJumpIndex = 0;
									m_NextMapIndex = -1;
									m_PreBGMIndex = -1;
									m_enableFlag = 0;
									m_enableFlag |= 4;
									m_commonMdlNo = -1;
									initialize2();
								}

								public void initialize2()
								{
									m_ColFlag &= -2;
									m_ColFlag |= 2;
									m_ColFlag |= 4;
									m_ColFlag |= 8;
									m_ColFlag |= 16;
									m_ColFlag |= 32;
									m_ColFlag |= 64;
									m_ColFlag |= 128;
									m_ColFlag |= 256;
									m_ColFlag |= 512;
									m_ColFlag |= 1024;
									if ((m_enableFlag & 4) != 0)
									{
										m_ColFlag |= 2048;
										evt.CEventManager.getInstance().FlagMng().reset(0u, 980u);
									}
									else
									{
										m_ColFlag &= -2049;
										evt.CEventManager.getInstance().FlagMng().set(0u, 980u);
									}
									m_ColFlag |= 4096;
									m_ColFlag |= 8192;
									m_ColFlag |= 16384;
									m_ColFlag |= 32768;
									m_ColFlag |= 65536;
									m_ColFlag |= 131072;
									m_ColFlag |= 262144;
									m_ColFlag |= 524288;
									m_ColFlag |= 1048576;
									m_ColFlag |= 2097152;
									m_ColFlag |= 4194304;
									m_ColFlag |= 8388608;
									m_ColFlag |= 16777216;
									m_ColFlag |= 33554432;
									m_ColFlag |= 67108864;
									m_ColFlag |= 134217728;
									m_ColFlag |= 268435456;
									m_ColFlag |= 536870912;
									m_WallIndex = -1;
									m_MonsterPartyIndex = -1;
									m_MapJumpIndex = -1;
									m_BattleMapIndex = 0;
									VEC_Set(m_MapJumpNormal, 0, 0, 0);
									m_spMapType = SP_MAP_TYPE.SP_MAP_DEFAULT;
								}

								public sbyte MapJumpIndex()
								{
									return m_MapJumpIndex;
								}

								public void MapJumpIndex_set(sbyte arg0)
								{
									m_MapJumpIndex = arg0;
								}

								public void setRideOnChokobo(bool b)
								{
									if (b)
									{
										m_enableFlag |= 1;
									}
									else
									{
										m_enableFlag &= -2;
									}
								}

								public bool getRideOnChokobo()
								{
									return (m_enableFlag & 1) != 0;
								}

								public void setBackupPosJump(bool b)
								{
									if (b)
									{
										m_enableFlag |= 2;
									}
									else
									{
										m_enableFlag &= -3;
									}
								}

								public bool getBackupPosJump()
								{
									return (m_enableFlag & 2) != 0;
								}

								public void setMapJumpEnable(bool b)
								{
									if (b)
									{
										m_enableFlag |= 4;
										m_ColFlag |= 2048;
									}
									else
									{
										m_enableFlag &= -5;
										m_ColFlag &= -2049;
									}
								}

								public bool getMapJumpEnable()
								{
									return (m_enableFlag & 4) != 0;
								}

								public int isColFlag()
								{
									return m_ColFlag;
								}

								public void isColFlag_or(int arg0)
								{
									m_ColFlag |= arg0;
								}

								public void isColFlag_not_and(int arg0)
								{
									m_ColFlag &= ~arg0;
								}

								public void setNowMapName(string _NowMapName)
								{
									strcpy(out m_NowMapName, _NowMapName);
								}

								public string getNowMapName()
								{
									return m_NowMapName;
								}

								public void setNowShopMapName(string _NowShopMapName)
								{
									strcpy(out m_NowShopMapName, _NowShopMapName);
								}

								public string getNowShopMapName()
								{
									return m_NowShopMapName;
								}

								public void setBeforeFieldMapName(string _BeforeFieldMapName)
								{
									strcpy(out m_BeforeFieldMapName, _BeforeFieldMapName);
								}

								public string getBeforeFieldMapName()
								{
									return m_BeforeFieldMapName;
								}

								public void setBeforeFieldMapJumpIndex(sbyte _BeforeFieldMapJumpIndex)
								{
									m_BeforeFieldMapJumpIndex = _BeforeFieldMapJumpIndex;
								}

								public sbyte getBeforeFieldMapJumpIndex()
								{
									return m_BeforeFieldMapJumpIndex;
								}

								public void setBeforeTownMapName(string _BeforeTownMapName)
								{
									strcpy(out m_BeforeTownMapName, _BeforeTownMapName);
								}

								public string getBeforeTownMapName()
								{
									return m_BeforeTownMapName;
								}

								public void setBeforeTownMapJumpIndex(sbyte _BeforeTownMapJumpIndex)
								{
									m_BeforeTownMapJumpIndex = _BeforeTownMapJumpIndex;
								}

								public sbyte getBeforeTownMapJumpIndex()
								{
									return m_BeforeTownMapJumpIndex;
								}

								public sbyte NextMapIndex()
								{
									return m_NextMapIndex;
								}

								public void NextMapIndex_set(sbyte arg0)
								{
									m_NextMapIndex = arg0;
								}

								public sbyte PreBGMIndex()
								{
									return m_PreBGMIndex;
								}

								public void PreBGMIndex_set(sbyte arg0)
								{
									m_PreBGMIndex = arg0;
								}

								public sbyte WallIndex()
								{
									return m_WallIndex;
								}

								public void WallIndex_set(sbyte arg0)
								{
									m_WallIndex = arg0;
								}

								public sbyte MonsterPartyIndex()
								{
									return m_MonsterPartyIndex;
								}

								public void MonsterPartyIndex_set(sbyte arg0)
								{
									m_MonsterPartyIndex = arg0;
								}

								public VecFx32 MapJumpNormal()
								{
									return m_MapJumpNormal;
								}

								public void MapJumpNormal_set(VecFx32 arg0)
								{
									m_MapJumpNormal.copy(arg0);
								}

								public sbyte BattleMapIndex()
								{
									return m_BattleMapIndex;
								}

								public void BattleMapIndex_set(sbyte arg0)
								{
									m_BattleMapIndex = arg0;
								}

								public void setHoldDoorData(string _MapName, bool _IsOpen, sbyte _MaterialIndex)
								{
									strcpy(out HoldDoorData.m_MapName, _MapName);
									HoldDoorData.m_IsOpen = _IsOpen;
									HoldDoorData.m_MaterialIndex = _MaterialIndex;
								}

								public SHoldDoorData getHoldDoorData()
								{
									return HoldDoorData;
								}

								public void setSpMapType(SP_MAP_TYPE type)
								{
									m_spMapType = type;
								}

								public SP_MAP_TYPE getSpMapType()
								{
									return m_spMapType;
								}

								public void setCommonMdlNo(int no)
								{
									m_commonMdlNo = no;
								}

								public int getCommonMdlNo()
								{
									return m_commonMdlNo;
								}

								public void setDefault()
								{
									m_ColFlag = 0;
									m_NowMapName = "";
									m_NowShopMapName = "";
									m_BeforeFieldMapName = "";
									m_BeforeFieldMapJumpIndex = 0;
									m_BeforeTownMapName = "";
									m_BeforeTownMapJumpIndex = 0;
									m_NextMapIndex = 0;
									m_PreBGMIndex = 0;
									m_WallIndex = 0;
									m_MonsterPartyIndex = 0;
									m_MapJumpIndex = 0;
									m_BattleMapIndex = 0;
									m_MapJumpNormal.setDefault();
									m_spMapType = SP_MAP_TYPE.SP_MAP_DEFAULT;
									HoldDoorData.setDefault();
									m_enableFlag = 0;
									m_commonMdlNo = 0;
									m_reserved[0] = 0;
									m_reserved[1] = 0;
									m_reserved[2] = 0;
									m_reserved[3] = 0;
									m_reserved[4] = 0;
									m_reserved[5] = 0;
									m_reserved[6] = 0;
								}

								public void parse(ArrayReader reader)
								{
									byte[] array = new byte[16];
									m_ColFlag = reader.readInt32();
									reader.read(array, 0, 16);
									m_NowMapName = StringUtil.createString(array);
									reader.read(array, 0, 16);
									m_NowShopMapName = StringUtil.createString(array);
									reader.read(array, 0, 16);
									m_BeforeFieldMapName = StringUtil.createString(array);
									m_BeforeFieldMapJumpIndex = reader.readSByte();
									reader.read(array, 0, 16);
									m_BeforeTownMapName = StringUtil.createString(array);
									m_BeforeTownMapJumpIndex = reader.readSByte();
									m_NextMapIndex = reader.readSByte();
									m_PreBGMIndex = reader.readSByte();
									m_WallIndex = reader.readSByte();
									m_MonsterPartyIndex = reader.readSByte();
									m_MapJumpIndex = reader.readSByte();
									m_BattleMapIndex = reader.readSByte();
									m_MapJumpNormal.parse(reader);
									m_spMapType = (SP_MAP_TYPE)reader.readInt32();
									HoldDoorData.parse(reader);
									m_enableFlag = reader.readSByte();
									m_commonMdlNo = reader.readInt32();
									reader.read(m_reserved, 0, 7);
								}

								public void store(ArrayWriter writer)
								{
									byte[] array = new byte[16];
									writer.writeInt32(m_ColFlag);
									byte[] bytes = StringUtil.getBytes(m_NowMapName);
									memcpy(array, bytes, bytes.Length);
									writer.write(array, 0, 16);
									bytes = StringUtil.getBytes(m_NowShopMapName);
									memcpy(array, bytes, bytes.Length);
									writer.write(array, 0, 16);
									bytes = StringUtil.getBytes(m_BeforeFieldMapName);
									memcpy(array, bytes, bytes.Length);
									writer.write(array, 0, 16);
									writer.writeSByte(m_BeforeFieldMapJumpIndex);
									bytes = StringUtil.getBytes(m_BeforeTownMapName);
									memcpy(array, bytes, bytes.Length);
									writer.write(array, 0, 16);
									writer.writeSByte(m_BeforeTownMapJumpIndex);
									writer.writeSByte(m_NextMapIndex);
									writer.writeSByte(m_PreBGMIndex);
									writer.writeSByte(m_WallIndex);
									writer.writeSByte(m_MonsterPartyIndex);
									writer.writeSByte(m_MapJumpIndex);
									writer.writeSByte(m_BattleMapIndex);
									m_MapJumpNormal.store(writer);
									writer.writeInt32((int)m_spMapType);
									HoldDoorData.store(writer);
									writer.writeSByte(m_enableFlag);
									writer.writeInt32(m_commonMdlNo);
									writer.write(m_reserved, 0, 7);
								}
							}
	}
}
