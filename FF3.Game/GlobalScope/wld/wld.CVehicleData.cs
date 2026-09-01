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
							public class CVehicleData
							{
								private SHoldVehicleData[] m_HoldData = new SHoldVehicleData[8];

								private pl.PLAYER_VEHICLE_TYPE m_PreRidingVehicleNo;

								private bool m_EnterpOnAir;

								public CVehicleData()
								{
									for (int i = 0; i < m_HoldData.Length; i++)
									{
										m_HoldData[i] = new SHoldVehicleData();
									}
								}

								public void initialize()
								{
									for (int i = 0; i < 8; i++)
									{
										m_HoldData[i].initialize();
										switch (static_cast<pl.PLAYER_VEHICLE_TYPE>(i))
										{
										case pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_NORCHI:
										case pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_NORCHI_CTM:
										case pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_INVINSIBLE:
											m_HoldData[i].m_FieldNo = 3;
											break;
										}
									}
									m_PreRidingVehicleNo = pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ERR;
									m_EnterpOnAir = false;
								}

								public void setHoldData(int _Index, sbyte _fieldNo, VecFx32 _Position, VecFx32 _Rotation, bool _CanBoard)
								{
									m_HoldData[_Index].m_FieldNo = _fieldNo;
									m_HoldData[_Index].m_Position.copy(_Position);
									m_HoldData[_Index].m_Rotation.copy(_Rotation);
									m_HoldData[_Index].m_CanBoard = _CanBoard;
								}

								public SHoldVehicleData getHoldData(int _Index)
								{
									return m_HoldData[_Index];
								}

								public void setPreRidingOnVehicleNo(pl.PLAYER_VEHICLE_TYPE vehicleNo)
								{
									m_PreRidingVehicleNo = vehicleNo;
								}

								public pl.PLAYER_VEHICLE_TYPE getPreRidingOnVehicleNo()
								{
									return m_PreRidingVehicleNo;
								}

								public void setEnterpOnAir(bool b)
								{
									m_EnterpOnAir = b;
								}

								public bool getEnterpOnAir()
								{
									return m_EnterpOnAir;
								}

								public void setDefault()
								{
									m_PreRidingVehicleNo = pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CHOKOBO;
									m_EnterpOnAir = false;
								}

								public void parse(ArrayReader reader)
								{
									for (int i = 0; i < 8; i++)
									{
										m_HoldData[i].parse(reader);
									}
									m_PreRidingVehicleNo = (pl.PLAYER_VEHICLE_TYPE)reader.readInt32();
									m_EnterpOnAir = reader.readByte() != 0;
								}

								public void store(ArrayWriter writer)
								{
									for (int i = 0; i < 8; i++)
									{
										m_HoldData[i].store(writer);
									}
									writer.writeInt32((int)m_PreRidingVehicleNo);
									writer.writeByte((byte)(m_EnterpOnAir ? 1u : 0u));
								}
							}
	}
}
