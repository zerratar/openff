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
	public static partial class wld
	{
		public class CStateFieldEnd : CBaseState
		{
			private uint _BattleEnCountState;

			public override void start(CBaseSystem _sys)
			{
				if (_sys.getCustomFadeSetting(out var frame, out var color))
				{
					dgs.CFade.Main().fadeOut(frame, color);
					dgs.CFade.Sub().fadeOut(frame, color);
					_sys.cancelCustomFadeSetting();
				}
				else if (_sys.IsMapJump())
				{
					if (sceneMng.getStage()[0] == 'f')
					{
						AreaChange.getInstance().setClose(15, AreaChange.SWITCH_TYPE.SWITCH_TYPE_OUTER);
					}
					else
					{
						AreaChange.getInstance().setClose(15, AreaChange.SWITCH_TYPE.SWITCH_TYPE_OUTER);
					}
					dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
				}
				else if (_sys.IsBattle())
				{
					_BattleEnCountState = 0u;
					dgs.CFade.Sub().fadeOut(BATTLE_FADEOUT_TIME, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
				}
				else if (_sys.IsTalk())
				{
					dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
					dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
				}
				else if (!_sys.IsMenu())
				{
					if (_sys.IsMogNet())
					{
						dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
						dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
					}
					else if (!_sys.IsTitle() && !_sys.IsSave())
					{
						if (_sys.IsAreaMap())
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
				}
				if (_sys.IsMapJump())
				{
					if ((CWorldOutSideData.getInstance().SoundData().getSoundFlag() & 2) != 0 && strcmp(sceneMng.getStage(), "d01_02_e01") != 0)
					{
						MatrixSound.MtxSENDS_Play(1, 1, 192, 127);
					}
					strcpy(out var _, sceneMng.getStage());
					_sys.MapJumpPosition();
					if (sceneMng.getCommonMdl()[0] != 's')
					{
						CWorldOutSideData.getInstance().MapData().setNowShopMapName(sceneMng.getCommonMdl());
					}
					_sys.BackUpPosition();
					if (sceneMng.getStage()[0] != 'f' || sceneMng.getFieldNo() == 4 || sceneMng.getPreFieldNo() == 4 || (sceneMng.getFieldNo() == 2 && evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_CHAPTER_FLAG[3]) == 0) || (sceneMng.getPreFieldNo() == 2 && evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_CHAPTER_FLAG[3]) == 0))
					{
						MapSound.setBGMVolume(0, 15);
					}
				}
				else if (_sys.IsBattle())
				{
					MapSound.setBGMVolume(0, 15);
					stopNaviSE(_sys);
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
					if (AreaChange.getInstance().isClosed())
					{
						setPhase(PHASE.END);
					}
				}
				else if (_sys.IsMenu())
				{
					setPhase(PHASE.END);
				}
				else if (_sys.IsSave())
				{
					setPhase(PHASE.END);
				}
				else if (_sys.IsTitle())
				{
					setPhase(PHASE.END);
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
				else if (dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
				{
					setPhase(PHASE.END);
				}
			}

			public override void end(CBaseSystem _sys)
			{
				if (_sys.IsMapJump())
				{
					sys.GGlobal.setNextPart(GAMEPART.GAMEPART_WORLD);
					if (sceneMng.getStage()[0] == 'f')
					{
						_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD);
					}
					else
					{
						_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN);
					}
					_sys.setMapJump(b: false);
				}
				else if (_sys.IsBattle())
				{
					sceneMng.gotoStage(sceneMng.getStage());
					_sys.BackUpPosition();
					sys.GGlobal.setNextPart(GAMEPART.GAMEPART_BATTLE);
				}
				else if (_sys.IsMogNet())
				{
					sceneMng.gotoStage(sceneMng.getStage());
					_sys.BackUpPosition();
					sys.GGlobal.setNextPart(GAMEPART.GAMEPART_MOG_NET);
				}
				else if (_sys.IsShop())
				{
					_sys.BackUpPosition();
					sys.GGlobal.setNextPart(GAMEPART.GAMEPART_WORLD);
					_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_SHOP);
				}
				else
				{
					if (_sys.IsMenu())
					{
						_sys.BackUpPosition();
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
					if (_sys.IsAreaMap())
					{
						_sys.setMode(CBaseSystem.WORLD_MODE.WORLD_MODE_SITE);
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
