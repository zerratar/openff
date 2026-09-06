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
							public class WorldMap
							{
								public enum WORLD_TYPE
								{
									WORLD_01,
									WORLD_02,
									WORLD_03,
									WORLD_04
								}

								public const WORLD_TYPE WORLD_01 = WORLD_TYPE.WORLD_01;

								public const WORLD_TYPE WORLD_02 = WORLD_TYPE.WORLD_02;

								public const WORLD_TYPE WORLD_03 = WORLD_TYPE.WORLD_03;

								public const WORLD_TYPE WORLD_04 = WORLD_TYPE.WORLD_04;

								public static int MAP_MARKER_NUM_MAX = 32;

								private string imgFname_;

								private sys2d.Cell[] CellMarker_ = new sys2d.Cell[15];

								private MapMarker[] MapMarker_ = new MapMarker[MAP_MARKER_NUM_MAX];

								private VecFx32 AreaOrg_ = new VecFx32();

								private VecFx32 AreaWH_ = new VecFx32();

								public WorldMap()
								{
									for (int i = 0; i < MapMarker_.Length; i++)
									{
										MapMarker_[i] = new MapMarker();
									}
									for (int i = 0; i < CellMarker_.Length; i++)
									{
										CellMarker_[i] = new sys2d.Cell();
									}
								}

								public int initializeWMap()
								{
									int progress = evaluteProgress();
									return initializeWMap(progress, forceEnable: false);
								}

								public int initializeWMap(int Progress, bool forceEnable)
								{
									changeGlobalDirectory();
									CellMarker_[0].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "./w_map_mark.NCER", "./w_map_mark.NANR", "./w_map_mark.NCGR", null);
									CellMarker_[2].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "./map_marker_03.NCER", "./map_marker_03.NANR", "./map_marker_03.NCGR", null);
									CellMarker_[3].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "./map_marker_02.NCER", "./map_marker_02.NANR", "./map_marker_02.NCGR", null);
									CellMarker_[5].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "./map_marker_10.NCER", "./map_marker_10.NANR", "./map_marker_10.NCGR", null);
									CellMarker_[6].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "./map_marker_04.NCER", "./map_marker_04.NANR", "./map_marker_04.NCGR", null);
									CellMarker_[7].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "./map_marker_05.NCER", "./map_marker_05.NANR", "./map_marker_05.NCGR", null);
									CellMarker_[8].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "./map_marker_06.NCER", "./map_marker_06.NANR", "./map_marker_06.NCGR", null);
									CellMarker_[9].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "./map_marker_07.NCER", "./map_marker_07.NANR", "./map_marker_07.NCGR", null);
									CellMarker_[10].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "./map_marker_08.NCER", "./map_marker_08.NANR", "./map_marker_08.NCGR", null);
									CellMarker_[11].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "./map_marker_09.NCER", "./map_marker_09.NANR", "./map_marker_09.NCGR", null);
									CellMarker_[12].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "./w_map_ship01.NCER", "./w_map_ship01.NANR", "./w_map_ship01.NCGR", null);
									CellMarker_[13].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "./w_map_ship02.NCER", "./w_map_ship02.NANR", "./w_map_ship02.NCGR", null);
									CellMarker_[14].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "./w_map_ship03.NCER", "./w_map_ship03.NANR", "./w_map_ship03.NCGR", null);
									CellMarker_[0].ceReleaseCgCl();
									CellMarker_[1].ceReleaseCgCl();
									CellMarker_[2].ceReleaseCgCl();
									CellMarker_[3].ceReleaseCgCl();
									CellMarker_[4].ceReleaseCgCl();
									CellMarker_[5].ceReleaseCgCl();
									CellMarker_[6].ceReleaseCgCl();
									CellMarker_[7].ceReleaseCgCl();
									CellMarker_[8].ceReleaseCgCl();
									CellMarker_[9].ceReleaseCgCl();
									CellMarker_[10].ceReleaseCgCl();
									CellMarker_[12].ceReleaseCgCl();
									CellMarker_[13].ceReleaseCgCl();
									CellMarker_[14].ceReleaseCgCl();
									string[] array = new string[10] { "./ar00.area", "./ar00.area", "./ar01.area", "./ar02.area", "./ar03.area", "./ar04.area", "./ar05.area", "./ar06.area", "./ar07.area", "./ar08.area" };
									string filename = array[0];
									switch (sceneMng.getFieldNo())
									{
									case 1:
										filename = ((5 <= Progress) ? array[4] : array[Progress]);
										break;
									case 2:
										filename = ((5 >= Progress) ? "ar04.area" : array[Progress]);
										break;
									case 3:
										filename = array[Progress];
										break;
									case 4:
										filename = "ar09.area";
										break;
									case -1:
										switch (getCurrentWorld())
										{
										case 0:
											filename = ((5 <= Progress) ? array[4] : array[Progress]);
											break;
										case 1:
											filename = "ar04.area";
											break;
										case 2:
											filename = array[Progress];
											break;
										case 3:
											filename = "ar09.area";
											break;
										}
										break;
									}
									Array array2 = null;
									ArrayReader arrayReader = null;
									uint size = ds.g_File.getSize(filename);
									if (size != 0)
									{
										array2 = ds.CHeap.alloc_app(size);
										if (ds.g_File.load(array2, filename))
										{
											arrayReader = new ArrayReader(array2);
											AreaDataHeader areaDataHeader = (AreaDataHeader)arrayReader;
											AreaOrg_.x = areaDataHeader.orgX_ << 12;
											AreaOrg_.z = areaDataHeader.orgZ_ << 12;
											AreaWH_.x = areaDataHeader.rangeX_ << 12;
											AreaWH_.z = areaDataHeader.rangeZ_ << 12;
											strncpy(out imgFname_, areaDataHeader.imgFname_, 16);
											MarkerData[] aMarkerData = areaDataHeader.m_aMarkerData;
											for (int i = 0; i < areaDataHeader.numMarker_; i++)
											{
												if (!forceEnable && FlagManager.singleton().get(1u, (uint)aMarkerData[i].releaseFlag_) == 0)
												{
													continue;
												}
												int num = newMapMarker((MapMarkerType)aMarkerData[i].type_);
												if (num <= -1)
												{
													continue;
												}
												MapMarker mapMarker = getMapMarker(num);
												if (mapMarker != null)
												{
													VecFx32 worldPos = new VecFx32(aMarkerData[i].x_ << 12, 0, aMarkerData[i].z_ << 12);
													ds.Vector2<int> vector = transCoordWorldToAreaFx32(worldPos, getAreaOrg(), getAreaWH());
													int x = FX_Mul(LCD_HEIGHT * 4096 * 6 / 5, vector.vx - 2048) + 983040;
													int num2 = FX_Mul(LCD_HEIGHT * 4096, vector.vy - 2048) + 655360;
													switch (aMarkerData[i].id_)
													{
													case 3:
														num2 -= 4096;
														break;
													case 20:
														num2 += 4096;
														break;
													case 28:
														num2 += 8192;
														break;
													case 29:
														num2 -= 8192;
														break;
													case 57:
														num2 -= 4096;
														break;
													case 59:
														num2 += 4096;
														break;
													}
													mapMarker.Cell_.SetPositionF(x, num2);
													if (mapMarker.m_bCell_YSeparate)
													{
														mapMarker.Cell_YSeparate.SetPositionF(x, num2);
													}
												}
											}
											if (array2 != null)
											{
												ds.CHeap.free_app(array2);
												array2 = null;
											}
											arrayReader?.dispose();
											return 0;
										}
									}
									if (array2 != null)
									{
										ds.CHeap.free_app(array2);
										array2 = null;
									}
									arrayReader?.dispose();
									return 1;
								}

								public void finalizeWMap()
								{
									for (int i = 0; MAP_MARKER_NUM_MAX > i; i++)
									{
										if (MapMarkerType.MAP_MARKER_INVALID != MapMarker_[i].Type_)
										{
											sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(MapMarker_[i].Cell_);
											MapMarker_[i].Cell_.SetShow(show: false);
											MapMarker_[i].Cell_.Release();
											MapMarker_[i].Type_ = MapMarkerType.MAP_MARKER_INVALID;
											if (MapMarker_[i].m_bCell_YSeparate)
											{
												sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(MapMarker_[i].Cell_YSeparate);
												MapMarker_[i].Cell_YSeparate.SetShow(show: false);
												MapMarker_[i].Cell_YSeparate.Release();
											}
										}
									}
									for (int j = 0; j < 15; j++)
									{
										NNS_G2dReleaseImageProxy(CellMarker_[j].GetImageProxy());
									}
									CellMarker_[0].Release();
									CellMarker_[2].Release();
									CellMarker_[3].Release();
									CellMarker_[4].Release();
									CellMarker_[5].Release();
									CellMarker_[6].Release();
									CellMarker_[7].Release();
									CellMarker_[8].Release();
									CellMarker_[9].Release();
									CellMarker_[10].Release();
									CellMarker_[11].Release();
									CellMarker_[12].Release();
									CellMarker_[13].Release();
									CellMarker_[14].Release();
								}

								public void updateWMap()
								{
								}

								public string getMapFileName()
								{
									if (WorldPart.getInstance().getWorldSystem().Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
									{
										return imgFname_;
									}
									if (WorldPart.getInstance().getWorldSystem().Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN)
									{
										return imgFname_;
									}
									return imgFname_;
								}

								public void resetMapMarker()
								{
									for (int i = 0; MAP_MARKER_NUM_MAX > i; i++)
									{
										MapMarker_[i].Type_ = MapMarkerType.MAP_MARKER_INVALID;
									}
								}

								public int newMapMarker(MapMarkerType Type)
								{
									int num = -1;
									for (int i = 0; MAP_MARKER_NUM_MAX > i; i++)
									{
										if (MapMarkerType.MAP_MARKER_INVALID == MapMarker_[i].Type_)
										{
											num = i;
											break;
										}
									}
									if (-1 != num)
									{
										MapMarker_[num].x_ = -255;
										MapMarker_[num].y_ = -255;
										MapMarker_[num].Cell_.copy(CellMarker_[(int)Type]);
										MapMarker_[num].Type_ = Type;
										MapMarker_[num].Cell_.SetPositionI(MapMarker_[num].x_, MapMarker_[num].y_);
										MapMarker_[num].Cell_.SetPriority((byte)((Type == MapMarkerType.MAP_MARKER_PLAYER) ? 1u : 2u));
										MapMarker_[num].Cell_.SetShow(show: false);
										sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(MapMarker_[num].Cell_);
										if (Type == MapMarkerType.MAP_MARKER_PLAYER || Type == MapMarkerType.MAP_MARKER_VEHICLE1 || Type == MapMarkerType.MAP_MARKER_VEHICLE2 || Type == MapMarkerType.MAP_MARKER_VEHICLE3)
										{
											MapMarker_[num].m_bCell_YSeparate = true;
											MapMarker_[num].Cell_YSeparate.copy(CellMarker_[(int)Type]);
											MapMarker_[num].Cell_YSeparate.SetPositionI(MapMarker_[num].x_, MapMarker_[num].y_);
											MapMarker_[num].Cell_YSeparate.SetPriority((byte)((Type == MapMarkerType.MAP_MARKER_PLAYER) ? 1u : 2u));
											MapMarker_[num].Cell_YSeparate.SetShow(show: false);
											sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(MapMarker_[num].Cell_YSeparate);
										}
										else
										{
											MapMarker_[num].m_bCell_YSeparate = false;
										}
										return num;
									}
									return num;
								}

								public void delMapMarker(int ID)
								{
									if (ID >= 0 && MAP_MARKER_NUM_MAX > ID)
									{
										sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(MapMarker_[ID].Cell_);
										MapMarker_[ID].Cell_.SetShow(show: false);
										MapMarker_[ID].Cell_.Release();
										MapMarker_[ID].Type_ = MapMarkerType.MAP_MARKER_INVALID;
										if (MapMarker_[ID].m_bCell_YSeparate)
										{
											sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(MapMarker_[ID].Cell_YSeparate);
											MapMarker_[ID].Cell_YSeparate.SetShow(show: false);
											MapMarker_[ID].Cell_YSeparate.Release();
											MapMarker_[ID].Type_ = MapMarkerType.MAP_MARKER_INVALID;
										}
									}
								}

								public void showMapMarker()
								{
									for (int i = 0; MAP_MARKER_NUM_MAX > i; i++)
									{
										if (MapMarkerType.MAP_MARKER_INVALID != MapMarker_[i].Type_)
										{
											MapMarker_[i].Cell_.SetShow(show: true);
											if (MapMarker_[i].m_bCell_YSeparate)
											{
												MapMarker_[i].Cell_YSeparate.SetShow(show: true);
											}
										}
									}
								}

								public void hideMapMarker()
								{
									for (int i = 0; MAP_MARKER_NUM_MAX > i; i++)
									{
										if (MapMarkerType.MAP_MARKER_INVALID != MapMarker_[i].Type_)
										{
											MapMarker_[i].Cell_.SetShow(show: false);
											if (MapMarker_[i].m_bCell_YSeparate)
											{
												MapMarker_[i].Cell_YSeparate.SetShow(show: false);
											}
										}
									}
								}

								public MapMarker getMapMarker(int ID)
								{
									if (MAP_MARKER_NUM_MAX <= ID)
									{
										return null;
									}
									if (-1 >= ID)
									{
										return null;
									}
									return MapMarker_[ID];
								}

								public VecFx32 getAreaOrg()
								{
									return AreaOrg_;
								}

								public VecFx32 getAreaWH()
								{
									return AreaWH_;
								}

								public void setAreaOrg(VecFx32 org)
								{
									AreaOrg_.copy(org);
								}

								public void setAreaWH(VecFx32 wh)
								{
									AreaWH_.copy(wh);
								}

								public int getCurrentWorld()
								{
									int result = -1;
									string stage = sceneMng.getStage();
									for (int i = 0; g_TownInfoTable.Length > i; i++)
									{
										if (strncmp(g_TownInfoTable[i].namePrerfix_, stage, 3) == 0)
										{
											result = g_TownInfoTable[i].belongWorld_;
											break;
										}
									}
									return result;
								}

								public void releaseMarkerFlagByStageName(string StageName)
								{
									if (StageName == null)
									{
										return;
									}
									for (int i = 0; g_TownInfoTable.Length > i; i++)
									{
										if (strncmp(g_TownInfoTable[i].namePrerfix_, StageName, 3) == 0)
										{
											FlagManager.singleton().set(1u, (uint)g_TownInfoTable[i].flag_);
										}
									}
								}
							}
	}
}
