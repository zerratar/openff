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
		public class CStateTownEnd : CBaseState
		{
			public enum MAP_JUMP_STATE
			{
				MJ_WAIT_CLOSE,
				MJ_CLOSED,
				MJ_BGM_FADE_OUT,
				MJ_END
			}

			public const MAP_JUMP_STATE MJ_WAIT_CLOSE = MAP_JUMP_STATE.MJ_WAIT_CLOSE;

			public const MAP_JUMP_STATE MJ_CLOSED = MAP_JUMP_STATE.MJ_CLOSED;

			public const MAP_JUMP_STATE MJ_BGM_FADE_OUT = MAP_JUMP_STATE.MJ_BGM_FADE_OUT;

			public const MAP_JUMP_STATE MJ_END = MAP_JUMP_STATE.MJ_END;

			private int _localState;

			private int _localCounter;

			private map.CMapSoundParameter[] _mapSoundParameter;

			private uint _BattleEnCountState;

			private bool m_bCheckTrial;

			public override void start(CBaseSystem _sys)
			{
				CWorldOutSideData.getInstance().MapData().setNowShopMapName(sceneMng.getStage());
				if (_sys.getCustomFadeSetting(out var frame, out var color))
				{
					dgs.CFade.Main().fadeOut(frame, color);
					dgs.CFade.Sub().fadeOut(frame, color);
					_sys.cancelCustomFadeSetting();
				}
				else if (_sys.IsMapJump())
				{
					strcpy(out var _, sceneMng.getStage());
					_sys.MapJumpPosition();
					if (_sys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
					{
						AreaChange.getInstance().setClose(15, AreaChange.SWITCH_TYPE.SWITCH_TYPE_CENTER);
					}
					else if (_sys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN)
					{
						AreaChange.getInstance().setClose(15, AreaChange.SWITCH_TYPE.SWITCH_TYPE_CENTER);
					}
					dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
					if (sceneMng.getStage()[0] == 'f' && FlagManager.singleton().get(0u, 26u) == 1 && FlagManager.singleton().get(0u, 45u) == 0 && strcmp(sceneMng.getPreStage(), "t03_01") == 0)
					{
						m_bCheckTrial = true;
					}
					else
					{
						m_bCheckTrial = false;
					}
				}
				else if (_sys.IsBattle())
				{
					_BattleEnCountState = 0u;
					dgs.CFade.Sub().fadeOut(BATTLE_FADEOUT_TIME, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
				}
				else if (!_sys.IsMenu() && !_sys.IsTitle() && !_sys.IsInn())
				{
					if (_sys.IsTalk())
					{
						dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
						dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
					}
					else if (_sys.IsMogNet() || _sys.IsSave() || _sys.IsSpecial())
					{
						dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
						dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
					}
					else
					{
						dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
						dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
					}
				}
				if (_sys.IsMapJump())
				{
					int soundFlag = CWorldOutSideData.getInstance().SoundData().getSoundFlag();
					if ((soundFlag & 2) != 0)
					{
						if (strcmp(sceneMng.getStage(), "d01_02_e01") != 0)
						{
							MatrixSound.MtxSENDS_Play(1, 1, 192, 127);
						}
					}
					else
					{
						soundFlag |= 2;
						CWorldOutSideData.getInstance().SoundData().setSoundFlag(soundFlag);
					}
					if (sceneMng.getStage()[0] == 'f')
					{
						MapSound.setBGMVolume(0, 15);
					}
					_localState = 0;
				}
				else if (_sys.IsBattle())
				{
					MapSound.setBGMVolume(0, 15);
				}
				g_encountWorkFrame = 0;
				g_encountState = 0;
				pl.CBasePlayer cBasePlayer = _sys.PlayerMng().Player(0);
				if (ENCOUNT_MOTION_INDEX == cBasePlayer.getMotionIndex())
				{
					g_encountState = 1;
				}
				if (ENCOUNT_LOOP_MOTION_INDEX == cBasePlayer.getMotionIndex())
				{
					g_encountState = 2;
				}
			}

			public override void update(CBaseSystem _sys)
			{
				if (_sys.IsMapJump())
				{
					updateMapJump(_sys);
				}
				else if (_sys.IsBattle())
				{
					executeEncountMotion(_sys);
					switch (_BattleEnCountState)
					{
					case 0u:
						if (dgs.CFade.Sub().isFaded())
						{
							GX_ResetBankForSubBG();
							int x = 128;
							int y = 96;
							if (ds.g_TouchPanel.isTouch())
							{
								ds.g_TouchPanel.getPoint(out x, out y);
								x *= 256;
								y *= 192;
								x /= LCD_WIDTH;
								y /= LCD_HEIGHT;
							}
							Encount.getInstance().prepare(Encount.ENCOUNT_VRAM.ENCOUNT_VRAM_C, _sys.WorldCamera(), (byte)x, (byte)y);
							_BattleEnCountState = 1u;
						}
						break;
					case 1u:
						Encount.getInstance().execute();
						Encount.getInstance().draw();
						if (Encount.getInstance().isEnded())
						{
							setPhase(PHASE.END);
						}
						break;
					}
				}
				else if (_sys.IsMenu() || _sys.IsTitle() || _sys.IsInn())
				{
					setPhase(PHASE.END);
				}
				else if (dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
				{
					setPhase(PHASE.END);
				}
			}

			public void updateMapJump(CBaseSystem _sys)
			{
				switch (_localState)
				{
				case 0:
					if (AreaChange.getInstance().isClosed())
					{
						_localState = 1;
					}
					break;
				case 1:
					if (loadMapSoundParameter(sceneMng.getStage()))
					{
						int num = _mapSoundParameter[0].BGMIndex();
						int num2 = map.CMapParameterManager.Instance().MapSoundParameter(0).BGMIndex();
						if (-1 != _mapSoundParameter[0].CheckFlag() && evt.CEventManager.getInstance().FlagMng().get(0u, (uint)_mapSoundParameter[0].CheckFlag()) == 1)
						{
							num = _mapSoundParameter[0].ChangeBGMIndex();
						}
						if (-1 != map.CMapParameterManager.Instance().MapSoundParameter(0).CheckFlag() && evt.CEventManager.getInstance().FlagMng().get(0u, (uint)map.CMapParameterManager.Instance().MapSoundParameter(0).CheckFlag()) == 1)
						{
							num2 = map.CMapParameterManager.Instance().MapSoundParameter(0).ChangeBGMIndex();
						}
						if (num2 != num)
						{
							MapSound.setBGMVolume(0, 15);
							_localCounter = 30;
							_localState = 2;
						}
						else
						{
							_localState = 3;
						}
					}
					else
					{
						_localState = 3;
					}
					break;
				case 2:
					if (0 > _localCounter--)
					{
						_localState = 3;
					}
					break;
				case 3:
					if (m_bCheckTrial)
					{
						if (UserInfo.confirm_state == 0)
						{
							UserInfo.confirm_state = 1;
							MainActivity.confirmApp();
							break;
						}
						if (UserInfo.confirm_state == 1)
						{
							break;
						}
						if (UserInfo.confirm_state == 3)
						{
							UserInfo.confirm_state = 0;
							CBaseSystem.setTitle(b: true);
							break;
						}
						m_bCheckTrial = false;
					}
					setPhase(PHASE.END);
					break;
				}
			}

			public bool loadMapSoundParameter(string MapName)
			{
				Array array = null;
				bool result = true;
				string arg = "";
				if (MapName != null)
				{
					sprintf(out arg, "%6s%s", MapName, ".pak");
					char c = arg[0];
					if (c != 'd')
					{
						if (c != 't')
						{
							result = false;
							goto IL_00b3;
						}
						FS_ChangeDir("/MAP/TOWN/PARAMETER");
					}
					else
					{
						FS_ChangeDir("/MAP/DUNGEON/PARAMETER");
					}
					uint size = ds.g_File.getSize(arg);
					if (size == 0)
					{
						result = false;
					}
					else
					{
						array = ds.CHeap.alloc_app(size);
						if (array == null)
						{
							result = false;
						}
						else if (!ds.g_File.load(array, arg))
						{
							result = false;
						}
						else
						{
							_mapSoundParameter = map.CMapSoundParameter.ChainPointer((byte[])array, 3);
						}
					}
				}
				goto IL_00b3;
				IL_00b3:
				if (array != null)
				{
					ds.CHeap.free_app(array);
				}
				FS_ChangeDir("/");
				return result;
			}

			public override void end(CBaseSystem _sys)
			{
				if (_sys.IsMapJump())
				{
					if (sceneMng.getStage()[0] == 'f')
					{
						_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD);
					}
					else
					{
						_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN);
					}
					sys.GGlobal.setNextPart(GAMEPART.GAMEPART_WORLD);
				}
				else if (_sys.IsBattle())
				{
					_sys.BackUpPosition();
					sys.GGlobal.setNextPart(GAMEPART.GAMEPART_BATTLE);
				}
				else if (_sys.IsMogNet())
				{
					_sys.BackUpPosition();
					sys.GGlobal.setNextPart(GAMEPART.GAMEPART_MOG_NET);
					MapSound.storeSetupMapSoundSetting(_sys.getMapSoundSetting());
				}
				else if (_sys.IsShop())
				{
					_sys.BackUpPosition();
					sys.GGlobal.setNextPart(GAMEPART.GAMEPART_WORLD);
					_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_SHOP);
				}
				else
				{
					if (_sys.IsAreaMap())
					{
						_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_SITE);
						_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_START);
						_sys.CrtState().phase_set(PHASE.START);
						return;
					}
					if (_sys.IsMenu())
					{
						_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_MENU);
						_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_START);
						_sys.CrtState().phase_set(PHASE.START);
						return;
					}
					if (_sys.IsSave())
					{
						_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_SAVE);
						_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_START);
						_sys.CrtState().phase_set(PHASE.START);
						return;
					}
					if (_sys.IsTalk())
					{
						_sys.BackUpPosition();
						sys.GGlobal.setNextPart(GAMEPART.GAMEPART_WORLD);
						_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_TALK);
					}
					else if (_sys.IsTitle())
					{
						_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN);
						sys.GGlobal.setNextPart(GAMEPART.GAMEPART_TITLE);
					}
					else if (_sys.IsSpecial())
					{
						_sys.BackUpPosition();
						sys.GGlobal.setNextPart(GAMEPART.GAMEPART_SPECIAL);
					}
					else if (_sys.IsInn())
					{
						_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_INN);
						_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_START);
						_sys.CrtState().phase_set(PHASE.START);
						return;
					}
				}
				_sys.setEnd(_End: true);
			}

			public override bool canExecuteEvent(CBaseSystem arg0)
			{
				return false;
			}
		}
	}
}
