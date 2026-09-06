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
						public class MapSound
						{
							public static MatrixSound.enMtxBGMSlot slot_;

							public static bool preserveSetupMapSoundSetting_ = true;

							public void setup(CBaseSystem _sys)
							{
								if (!map.CMapParameterManager.Instance().isLoaded())
								{
									setUpMapSoundDummy();
								}
								short num = map.CMapParameterManager.Instance().MapSoundParameter(0).BGMIndex();
								short num2 = CWorldOutSideData.getInstance().MapData().PreBGMIndex();
								if (map.CMapParameterManager.Instance().MapSoundParameter(0).CheckFlag() != -1 && evt.CEventManager.getInstance().FlagMng().get(0u, (uint)map.CMapParameterManager.Instance().MapSoundParameter(0).CheckFlag()) == 1)
								{
									num = map.CMapParameterManager.Instance().MapSoundParameter(0).ChangeBGMIndex();
								}
								pl.PLAYER_VEHICLE_TYPE preRidingOnVehicleNo = CWorldOutSideData.getInstance().VehicleData().getPreRidingOnVehicleNo();
								if (sceneMng.getFieldNo() != 2 && sceneMng.getFieldNo() != 4 && _sys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
								{
									switch (preRidingOnVehicleNo)
									{
									case pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP:
									case pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM:
										num = (short)((!CWorldOutSideData.getInstance().VehicleData().getEnterpOnAir()) ? 20 : 9);
										break;
									case pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_INVINSIBLE:
										num = 10;
										break;
									case pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CHOKOBO:
										num = 7;
										break;
									default:
										num = 9;
										break;
									case pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ERR:
									case pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_CANOE:
										break;
									}
								}
								if (sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_WORLD && (num != num2 || num2 == -1))
								{
									MatrixSound.MtxSENDS_Unload();
									MatrixSound.MtxSoundBGM.getSingleton().stop(0, slot_);
									MatrixSound.MtxBGMNDS_Unload();
									MatrixSound.MtxBGMNDS_Unload();
								}
								slot_ = getSlot(num);
								if (sys.GGlobal.getPreviousPart() == GAMEPART.GAMEPART_BATTLE)
								{
									MatrixSound.MtxBGMNDS_LoadEx(BGM_TABEL_INDEX[num], 0);
									MatrixSound.MtxSoundBGM.getSingleton().pause(Flag: false, slot_);
									MatrixSound.MtxSoundBGM.getSingleton().setVolume(192, (_sys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD) ? 90 : 15, slot_);
								}
								else if (strcmp(sceneMng.getStage(), "d01_02_e01") != 0)
								{
									if (num != num2 || num2 == -1)
									{
										MatrixSound.MtxBGMNDS_LoadEx(BGM_TABEL_INDEX[num], 0);
										MatrixSound.MtxBGMNDS_LoadEx(BGM_TABEL_INDEX[num], 0);
										MatrixSound.MtxSoundBGM.getSingleton().stop(0, slot_);
										MatrixSound.MtxSoundBGM.getSingleton().play(BGM_TABEL_INDEX[num], 192, 0, slot_);
										CWorldOutSideData.getInstance().MapData().PreBGMIndex_set((sbyte)num);
									}
								}
								else
								{
									num = 0;
									num2 = -1;
									MatrixSound.MtxBGMNDS_LoadEx(BGM_TABEL_INDEX[num], 0);
									MatrixSound.MtxBGMNDS_LoadEx(BGM_TABEL_INDEX[num], 0);
									CWorldOutSideData.getInstance().MapData().PreBGMIndex_set((sbyte)num);
								}
								if (sys.GGlobal.getPreviousPart() != GAMEPART.GAMEPART_WORLD && sys.GGlobal.getPreviousPart() != GAMEPART.GAMEPART_MOG_NET && sys.GGlobal.getPreviousPart() != GAMEPART.GAMEPART_SPECIAL)
								{
									MatrixSound.MtxSENDS_Load(1);
								}
								else if (num != num2 || num2 == -1)
								{
									MatrixSound.MtxSENDS_Load(1);
								}
								if (_sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_SHOP || _sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_MENU || _sys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_TALK || strcmp(sceneMng.getStage(), "d01_02_e01") == 0)
								{
									int soundFlag = CWorldOutSideData.getInstance().SoundData().getSoundFlag();
									soundFlag &= -2;
									CWorldOutSideData.getInstance().SoundData().setSoundFlag(soundFlag);
								}
							}

							public void cleanup(CBaseSystem _sys)
							{
								CWorldOutSideData.getInstance().MapData().PreBGMIndex_set((sbyte)MatrixSound.MtxBGMNDS_GetPlayBGMNo(slot_));
								if (_sys.IsBattle())
								{
									MatrixSound.MtxSENDS_Unload();
									MatrixSound.MtxSoundBGM.getSingleton().setVolume(0, 0, slot_);
									MatrixSound.MtxSoundBGM.getSingleton().pause(Flag: true, slot_);
									MatrixSound.MtxBGMNDS_Unload();
								}
							}

							public static void playBGM(int bgmNo, int volume, int frame)
							{
								if (-1 != bgmNo)
								{
									slot_ = getSlot(bgmNo);
									MatrixSound.MtxSoundBGM.getSingleton().stop(0, slot_);
									MatrixSound.MtxSENDS_Unload();
									MatrixSound.MtxBGMNDS_Unload();
									MatrixSound.MtxBGMNDS_Unload();
									MatrixSound.MtxBGMNDS_LoadEx(bgmNo, 0);
									MatrixSound.MtxBGMNDS_LoadEx(bgmNo, 0);
									MatrixSound.MtxSENDS_Load(1);
									MatrixSound.MtxSoundBGM.getSingleton().play(bgmNo, volume, frame, slot_);
								}
							}

							public static void stopBGM(int frame)
							{
								MatrixSound.MtxSoundBGM.getSingleton().stop(frame, slot_);
							}

							public static bool isPlaying()
							{
								return MatrixSound.enMtxBGMState.enMTX_BGM_PLAY == MatrixSound.MtxSoundBGM.getSingleton().getState(slot_);
							}

							public static void setBGMVolume(int volume, int frame)
							{
								MatrixSound.MtxSoundBGM.getSingleton().setVolume(volume, frame, slot_);
							}

							public static void storeSetupMapSoundSetting(bool b)
							{
								preserveSetupMapSoundSetting_ = b;
							}

							public static bool loadSetupMapSoundSetting()
							{
								return preserveSetupMapSoundSetting_;
							}

							public static bool canChangeBGM()
							{
								return 2 != sceneMng.getFieldNo();
							}
						}
	}
}
