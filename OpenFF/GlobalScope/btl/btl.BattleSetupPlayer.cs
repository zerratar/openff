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
	public static partial class btl
	{
		public class BattleSetupPlayer : BaseBattle
		{
			public enum PLAYER_STATE
			{
				POISE_TO_IDLE,
				MOVE_FRONT_READY,
				MOVE_FRONT,
				CREATE_COMMANDS,
				IS_OPENED_COMMANDS,
				SELECT_COMMANDS,
				MOVE_BACK,
				POISE_START,
				PLAYER_STATE_MAX
			}

			public enum COMMAND_STATE
			{
				SELECT_COMMAND = 0,
				SELECT_ENEMY = 1,
				SELECT_PLAYER = 2,
				SELECT_MAGIC = 3,
				SELECT_ITEM = 4,
				SELECT_SONG = 5,
				COMMAND_STATE_MAX = 6,
				ERR_COMMAND = -1
			}

			public enum WINDOW_STATE
			{
				NO_WINDOW,
				WINDOW_CREATING,
				WINDOW_CREATE,
				WINDOW_RELEASING,
				WINDOW_RELEASE,
				WINDOW_STATE_MAX
			}

			public enum TOUCH_CHARACTER
			{
				NO_TOUCH,
				TOUCH_PLAYER,
				TOUCH_ENEMY,
				TOUCH_BUTTON,
				TOUCH_CANCEL,
				TOUCH_ALL
			}

			public enum ALL_SELECT_FLAG
			{
				NO_SELECT = 0,
				ONE_SELECT = 1,
				GROUP_SELECT = 2,
				ALL_SELECT = 4
			}

			public enum SELECT_STATE
			{
				TOUCH_IDLE,
				TARGET_EDGE,
				SELECT_TARGET_STATE_MAX
			}

			public class PlayerState
			{
				public delegate void _playerState(BattlePlayer player, BattleSystem B);

				public _playerState playerState;

				public PlayerState(_playerState arg0)
				{
					playerState = arg0;
				}
			}

			public class CommandState
			{
				public delegate bool _commandState(BattleSystem B);

				public _commandState commandState;

				public CommandState(_commandState arg0)
				{
					commandState = arg0;
				}
			}

			public const PLAYER_STATE POISE_TO_IDLE = PLAYER_STATE.POISE_TO_IDLE;

			public const PLAYER_STATE MOVE_FRONT_READY = PLAYER_STATE.MOVE_FRONT_READY;

			public const PLAYER_STATE MOVE_FRONT = PLAYER_STATE.MOVE_FRONT;

			public const PLAYER_STATE CREATE_COMMANDS = PLAYER_STATE.CREATE_COMMANDS;

			public const PLAYER_STATE IS_OPENED_COMMANDS = PLAYER_STATE.IS_OPENED_COMMANDS;

			public const PLAYER_STATE SELECT_COMMANDS = PLAYER_STATE.SELECT_COMMANDS;

			public const PLAYER_STATE MOVE_BACK = PLAYER_STATE.MOVE_BACK;

			public const PLAYER_STATE POISE_START = PLAYER_STATE.POISE_START;

			public const PLAYER_STATE PLAYER_STATE_MAX = PLAYER_STATE.PLAYER_STATE_MAX;

			public const COMMAND_STATE SELECT_COMMAND = COMMAND_STATE.SELECT_COMMAND;

			public const COMMAND_STATE SELECT_ENEMY = COMMAND_STATE.SELECT_ENEMY;

			public const COMMAND_STATE SELECT_PLAYER = COMMAND_STATE.SELECT_PLAYER;

			public const COMMAND_STATE SELECT_MAGIC = COMMAND_STATE.SELECT_MAGIC;

			public const COMMAND_STATE SELECT_ITEM = COMMAND_STATE.SELECT_ITEM;

			public const COMMAND_STATE SELECT_SONG = COMMAND_STATE.SELECT_SONG;

			public const COMMAND_STATE COMMAND_STATE_MAX = COMMAND_STATE.COMMAND_STATE_MAX;

			public const COMMAND_STATE ERR_COMMAND = COMMAND_STATE.ERR_COMMAND;

			public const WINDOW_STATE NO_WINDOW = WINDOW_STATE.NO_WINDOW;

			public const WINDOW_STATE WINDOW_CREATING = WINDOW_STATE.WINDOW_CREATING;

			public const WINDOW_STATE WINDOW_CREATE = WINDOW_STATE.WINDOW_CREATE;

			public const WINDOW_STATE WINDOW_RELEASING = WINDOW_STATE.WINDOW_RELEASING;

			public const WINDOW_STATE WINDOW_RELEASE = WINDOW_STATE.WINDOW_RELEASE;

			public const WINDOW_STATE WINDOW_STATE_MAX = WINDOW_STATE.WINDOW_STATE_MAX;

			public const TOUCH_CHARACTER NO_TOUCH = TOUCH_CHARACTER.NO_TOUCH;

			public const TOUCH_CHARACTER TOUCH_PLAYER = TOUCH_CHARACTER.TOUCH_PLAYER;

			public const TOUCH_CHARACTER TOUCH_ENEMY = TOUCH_CHARACTER.TOUCH_ENEMY;

			public const TOUCH_CHARACTER TOUCH_BUTTON = TOUCH_CHARACTER.TOUCH_BUTTON;

			public const TOUCH_CHARACTER TOUCH_CANCEL = TOUCH_CHARACTER.TOUCH_CANCEL;

			public const TOUCH_CHARACTER TOUCH_ALL = TOUCH_CHARACTER.TOUCH_ALL;

			public const ALL_SELECT_FLAG NO_SELECT = ALL_SELECT_FLAG.NO_SELECT;

			public const ALL_SELECT_FLAG ONE_SELECT = ALL_SELECT_FLAG.ONE_SELECT;

			public const ALL_SELECT_FLAG GROUP_SELECT = ALL_SELECT_FLAG.GROUP_SELECT;

			public const ALL_SELECT_FLAG ALL_SELECT = ALL_SELECT_FLAG.ALL_SELECT;

			public const SELECT_STATE TOUCH_IDLE = SELECT_STATE.TOUCH_IDLE;

			public const SELECT_STATE TARGET_EDGE = SELECT_STATE.TARGET_EDGE;

			public const SELECT_STATE SELECT_TARGET_STATE_MAX = SELECT_STATE.SELECT_TARGET_STATE_MAX;

			public PlayerState[] playerState_;

			public CommandState[] CommandState_;

			private static int WINDOW_MAX = 5;

			private sbyte nowPlayer_;

			private PLAYER_STATE state_;

			private COMMAND_STATE commandState_;

			private COMMAND_STATE prevCommandState_;

			private WINDOW_STATE windowState_;

			private bool isCancel_;

			private bool touchPlayer_;

			private int windowId_;

			private int cursorLine_;

			private int upOrDown_;

			private int touchStartX_;

			private int touchStartY_;

			private int[] touchTargetId_ = new int[12];

			private int allSelectFlag_;

			private bool startPushFlag_;

			private int selectTargetState_;

			private SELECT_STATE selectState_;

			private int firstTargetPlayerId_;

			private BattleCommandWindow battleWindow_;

			private PlayerWindow playerWindow_;

			private menu.TargetWindow[] targetWindow_;

			private menu.TargetWindow cancelWindow_;

			private menu.TargetWindow changeWindow_;

			private menu.TargetWindow allWindow_;

			private menu.TargetWindow groupWindow_;

			public override void initialize(BattleSystem B)
			{
				nowPlayer_ = 0;
				state(PLAYER_STATE.MOVE_FRONT_READY);
				commandState_ = COMMAND_STATE.SELECT_COMMAND;
				prevCommandState_ = commandState_;
				windowState_ = WINDOW_STATE.NO_WINDOW;
				isCancel_ = false;
				allSelectFlag_ = 0;
				clearTouchTargetId();
				clearTouchStartPosition();
				battleWindow_ = B.battleCommandWindow();
				battleWindow_.offIsCreated();
				windowId_ = -1;
				playerWindow_ = B.playerWindow();
				playerWindow_.create();
				startPushFlag_ = false;
				touchPlayer_ = false;
				selectTargetState_ = 1;
				selectState_ = SELECT_STATE.TOUCH_IDLE;
				firstTargetPlayerId_ = -1;
				targetWindow_ = B.targetWindow();
				for (int i = 0; i < 4; i++)
				{
					ds.Vector2<short> position = new ds.Vector2<short>((short)BATTLE_TARGET_X(), (short)(BATTLE_TARGET_Y() + i * 40));
					ds.Vector2<short> size = new ds.Vector2<short>(144, 40);
					targetWindow_[i].createTargetWindow(position, size);
				}
				ds.Vector2<short>[] array = new ds.Vector2<short>[4]
				{
					new ds.Vector2<short>(0, 0),
					new ds.Vector2<short>(400, 0),
					new ds.Vector2<short>(320, 0),
					new ds.Vector2<short>(240, 0)
				};
				ds.Vector2<short> size2 = new ds.Vector2<short>(80, 40);
				cancelWindow_ = targetWindow_[4];
				changeWindow_ = targetWindow_[5];
				allWindow_ = targetWindow_[6];
				groupWindow_ = targetWindow_[7];
				cancelWindow_.createTargetWindow(array[0], size2);
				changeWindow_.createTargetWindow(array[1], size2);
				allWindow_.createTargetWindow(array[2], size2);
				groupWindow_.createTargetWindow(array[3], size2);
				if (opt.COptionManager.getSingleton().cursorOption().cursorPosition() != opt.CURSOR_POSITION.CURSOR_POSITION_MEMORY)
				{
					for (int j = 0; j < 4; j++)
					{
						BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(j);
						if (battlePlayer != null && battlePlayer.player() != null && battlePlayer.isBattle() && battlePlayer.condition().isCanCommandSelect() && !battlePlayer.flag(PLAYER_FLAG.PF_JUMP))
						{
							pl.PlayerParty.instance().clearBattleCommandPlayer(j);
						}
					}
				}
				if (!OutsideToBattle.getInstance().isFreeMode() && OutsideToBattle.getInstance().battleCamera() != BATTLE_CAMERA.COMMAND_CAMERA)
				{
					OutsideToBattle.getInstance().setBattleCamera(BATTLE_CAMERA.COMMAND_CAMERA);
				}
				for (int k = 0; k < 4; k++)
				{
					BattlePlayer battlePlayer2 = B.characterManager().playerParty().battlePlayer(k);
					if (battlePlayer2 == null || !battlePlayer2.isEnable())
					{
						continue;
					}
					battlePlayer2.calcConditionTime();
					battlePlayer2.clearBattleFlag();
					battlePlayer2.clearSongFlag();
					battlePlayer2.resetParameterMagicFlag();
					CommonFormula commonFormula = new CommonFormula();
					battlePlayer2.reupdateParameter(commonFormula.calcJobSkill(battlePlayer2));
					battlePlayer2.setHiddenWeaponFlag(pl.HAND_TYPE.RIGHT_HAND, flag: false);
					battlePlayer2.setHiddenWeaponFlag(pl.HAND_TYPE.LEFT_HAND, flag: false);
					if (battlePlayer2.isBattle())
					{
						battlePlayer2.setActionNumber(1);
						if (!battlePlayer2.flag(PLAYER_FLAG.PF_JUMP))
						{
							battlePlayer2.clearTarget();
							battlePlayer2.setActionId(0);
						}
						battlePlayer2.clearUseMagicId();
						battlePlayer2.clearUseItemId();
						battlePlayer2.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION);
						battlePlayer2.setIdleType(0);
					}
				}
				B.characterManager().changeMagicColor();
			}

			public override void terminate(BattleSystem B)
			{
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(i);
					if (battlePlayer != null && battlePlayer.isEnable())
					{
						battlePlayer.rebornItemOrMagicNumber(deleteItem: true);
					}
				}
				battleWindow_.release();
				releaseData();
			}

			public override void execute(BattleSystem B)
			{
				if (OutsideToBattle.getInstance().battleCamera() != BATTLE_CAMERA.COMMAND_CAMERA && !OutsideToBattle.getInstance().isFreeMode())
				{
					return;
				}
				BattleParty battleParty = B.characterManager().playerParty();
				if (OutsideToBattle.getInstance().battleOpeningType() == BATTLE_OPENING_TYPE.BACK_ATTACK)
				{
					for (int i = 0; i < 4; i++)
					{
						BattlePlayer battlePlayer = battleParty.battlePlayer(i);
						if (battlePlayer != null && isCommand(battlePlayer))
						{
							battlePlayer.setActionId(0);
						}
					}
					setPhase(Phase.Terminate);
					static_cast<BattleMain>(B.battle()).setNextBattleMainState(BATTLE_MAIN_STATE.SETUP_ENEMY);
					return;
				}
				BattlePlayer player;
				while (true)
				{
					if (nowPlayer_ < 0)
					{
						nowPlayer_ = 0;
					}
					if (nowPlayer_ >= 4)
					{
						cancelWindow_.setShowTarget(show: false);
						B.playerWindow().nowPlayer_set(-1);
						setPhase(Phase.Terminate);
						static_cast<BattleMain>(B.battle()).setNextBattleMainState(BATTLE_MAIN_STATE.SETUP_ENEMY);
						return;
					}
					battleWindow_.commandWindow().execute();
					for (int j = 0; j < 8; j++)
					{
						targetWindow_[j].execute();
					}
					player = battleParty.battlePlayer(nowPlayer_);
					if (isCommand(player))
					{
						break;
					}
					nowPlayer_++;
				}
				playerState_[(int)state_].playerState(player, B);
				OS_AssignBackButton(cancelWindow_.isShowTarget() ? 1 : 0);
			}

			public void releaseData()
			{
				menu.MenuBattleItem.getSingleton().ReleaseBItemStatus();
				menu.MenuManager.getSingleton().release();
				menu.MenuManager.getSingleton().ClearBehaviorButton();
				menu.MenuManager.getSingleton().releaseWindow(windowId_);
				menu.MenuManager.getSingleton().DeleteNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
			}

			public bool edgePad(int pad)
			{
				if ((ds.g_Pad.edge() & pad) != 0)
				{
					menu.MenuManager.getSingleton().playSEMoveCursor();
					return true;
				}
				return false;
			}

			public bool repeatPad(int pad)
			{
				if ((ds.g_Pad.repeat() & pad) != 0)
				{
					menu.MenuManager.getSingleton().playSEMoveCursor();
					return true;
				}
				return false;
			}

			public bool edgeDecide()
			{
				if ((ds.g_Pad.edge() & 1) == 0)
				{
					return false;
				}
				return true;
			}

			public bool edgeCancel()
			{
				if ((ds.g_Pad.edge() & 2) == 0)
				{
					return false;
				}
				return true;
			}

			public void playerStatePoiseToIdle(BattlePlayer player, BattleSystem B)
			{
				if (player.isPlayerActionEnd())
				{
					player.clearTargetId();
					player.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_POISE_TO_IDLE);
					player.setIdleType(0);
					state(PLAYER_STATE.MOVE_FRONT_READY);
				}
			}

			public void playerStateMoveFrontReady(BattlePlayer player, BattleSystem B)
			{
				if (player.isPlayerActionEnd())
				{
					player.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_FRONT);
					state(PLAYER_STATE.MOVE_FRONT);
				}
			}

			public void playerStateMoveFront(BattlePlayer player, BattleSystem B)
			{
				if (player.isPlayerActionEnd())
				{
					player.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION);
					settingCommand(player);
					state(PLAYER_STATE.CREATE_COMMANDS);
					commandState_ = COMMAND_STATE.SELECT_COMMAND;
				}
			}

			public void playerStateCreateCommand(BattlePlayer player, BattleSystem B)
			{
				if (player.isPlayerActionEnd())
				{
					battleWindow_.create(player);
					battleWindow_.display();
					battleWindow_.setOff();
					playerWindow_.create();
					showTriangle(player);
					Battle2DManager.instance().cursor().active(0);
					cursorLine_ = 0;
					if (opt.COptionManager.getSingleton().cursorOption().cursorPosition() == opt.CURSOR_POSITION.CURSOR_POSITION_MEMORY)
					{
						cursorLine_ = pl.PlayerParty.instance().saveCommandPosition(player.playerId());
						battleWindow_.updateCommandMessage(player);
					}
					Battle2DManager.instance().cursor().setPositionCommand(0, cursorLine_);
					B.playerWindow().nowPlayer_set(nowPlayer_);
					state(PLAYER_STATE.IS_OPENED_COMMANDS);
				}
			}

			public void playerStateIsOpendCommand(BattlePlayer player, BattleSystem B)
			{
				if (battleWindow_.isOpened())
				{
					state(PLAYER_STATE.SELECT_COMMANDS);
				}
			}

			public void playerStateSelectCommand(BattlePlayer player, BattleSystem B)
			{
				if (decideCommand(B))
				{
					player.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_BACK);
					state(PLAYER_STATE.MOVE_BACK);
				}
			}

			public void playerStateMoveBack(BattlePlayer player, BattleSystem B)
			{
				if (!player.isPlayerActionEnd())
				{
					return;
				}
				if (isCancel_)
				{
					BattleParty battleParty = B.characterManager().playerParty();
					rebornParameter(player);
					do
					{
						nowPlayer_--;
						if (nowPlayer_ == battleParty.getMinBattlePlayerId())
						{
							rebornParameter(battleParty.battlePlayer(nowPlayer_));
							break;
						}
						if (nowPlayer_ < 0)
						{
							nowPlayer_ = 3;
						}
						rebornParameter(battleParty.battlePlayer(nowPlayer_));
					}
					while (!isCommand(battleParty.battlePlayer(nowPlayer_)));
					isCancel_ = false;
					player.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION);
					state(PLAYER_STATE.POISE_TO_IDLE);
				}
				else
				{
					if (player.actionId() == 5 || player.actionId() == 5)
					{
						player.setIdleType(0);
					}
					else
					{
						player.setIdleType(1);
					}
					player.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_IDLE_TO_POISE);
					state(PLAYER_STATE.POISE_START);
				}
			}

			public void playerStatePoiseStart(BattlePlayer player, BattleSystem B)
			{
				if (player.isPlayerActionEnd())
				{
					nowPlayer_++;
					player.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION);
					state(PLAYER_STATE.MOVE_FRONT_READY);
					commandState_ = COMMAND_STATE.SELECT_COMMAND;
				}
			}

			public bool decideCommand(BattleSystem B)
			{
				if (battleWindow_.commandWindow().pushState() == 0)
				{
					if (!CommandState_[(int)commandState_].commandState(B))
					{
						return false;
					}
					return true;
				}
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayer_);
				pl.Command command = battlePlayer.player().jobManager().command();
				battleWindow_.commandWindow().moveMessage((command.nowCommand() == 6) ? 3 : (command.nowCommand() - pl.PlayerParty.instance().saveStartCommand(battlePlayer.playerId())));
				if (battleWindow_.commandWindow().pushState() == 0)
				{
					return commandAction(B);
				}
				return false;
			}

			public void rebornParameter(BattlePlayer player)
			{
				player.clearTargetId();
				player.rebornItemOrMagicNumber(deleteItem: false);
				if (!player.flag(PLAYER_FLAG.PF_JUMP))
				{
					player.setActionId(0);
				}
				player.setIdleType(0);
			}

			public bool isCommand(BattlePlayer player)
			{
				if (!player.isBattle())
				{
					return false;
				}
				if (player.condition().isConfusion())
				{
					allSelectFlag_ = 1;
					selectTargetState_ = 1;
					player.setActionId(1);
				}
				if (!player.condition().isCanCommandSelect())
				{
					return false;
				}
				if (player.flag(PLAYER_FLAG.PF_JUMP))
				{
					return false;
				}
				return true;
			}

			public void settingCommand(BattlePlayer player)
			{
				if (opt.COptionManager.getSingleton().cursorOption().cursorPosition() == opt.CURSOR_POSITION.CURSOR_POSITION_MEMORY)
				{
					cursorLine_ = pl.PlayerParty.instance().saveCommandPosition(player.playerId());
				}
				else
				{
					player.player().jobManager().command()
						.clearNowCommand();
					pl.PlayerParty.instance().setSaveStartCommand(player.playerId(), 0);
					cursorLine_ = 0;
				}
				battleWindow_.create(player);
				battleWindow_.updateCommandMessage(player);
				showTriangle(player);
			}

			public void showTriangle(BattlePlayer player)
			{
				Battle2DManager.instance().triangle().show(0, flag: true);
				Battle2DManager.instance().triangle().show(1, flag: true);
				if (pl.PlayerParty.instance().saveStartCommand(player.playerId()) > 0)
				{
					Battle2DManager.instance().triangle().enable(0, flag: true);
				}
				else
				{
					Battle2DManager.instance().triangle().enable(0, flag: false);
				}
				if (pl.PlayerParty.instance().saveStartCommand(player.playerId()) < pl.COMMAND_MAX - pl.BATTLE_COMMAND_MAX)
				{
					Battle2DManager.instance().triangle().enable(1, flag: true);
				}
				else
				{
					Battle2DManager.instance().triangle().enable(1, flag: false);
				}
			}

			public bool selectCommand(BattleSystem B)
			{
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayer_);
				pl.Command command = battlePlayer.player().jobManager().command();
				bool flag = false;
				bool flag2 = false;
				if (nowPlayer_ != B.characterManager().playerParty().getMinBattlePlayerId())
				{
					cancelWindow_.setShowTarget(show: true);
					cancelWindow_.createTargetMessage(50021);
				}
				else
				{
					cancelWindow_.setShowTarget(show: false);
				}
				showTriangle(battlePlayer);
				if (!ds.g_TouchPanel.isTap())
				{
					if (edgePad(64))
					{
						command.decrementNowCommand();
						cursorLine_--;
						flag = true;
					}
					else if (repeatPad(64))
					{
						command.decrementNowCommand();
						cursorLine_--;
					}
					else if (edgePad(128))
					{
						command.incrementNowCommand();
						cursorLine_++;
						flag = true;
					}
					else if (repeatPad(128))
					{
						command.incrementNowCommand();
						cursorLine_++;
					}
					if (cursorLine_ < 0 || cursorLine_ > pl.BATTLE_COMMAND_MAX - 1)
					{
						if (cursorLine_ < 0)
						{
							battleWindow_.createCommandMessagePadUp(battlePlayer, flag);
							if (flag && command.nowCommand() < 0)
							{
								cursorLine_ = pl.BATTLE_COMMAND_MAX - 1;
								command.setNowCommand((sbyte)(pl.COMMAND_MAX - 1));
							}
						}
						else if (cursorLine_ > pl.BATTLE_COMMAND_MAX - 1)
						{
							battleWindow_.createCommandMessagePadDown(battlePlayer, flag);
							if (flag && command.nowCommand() > pl.COMMAND_MAX - 1)
							{
								cursorLine_ = 0;
								command.setNowCommand(0);
							}
						}
					}
				}
				else
				{
					ds.g_TouchPanel.getPoint(out var x, out var y);
					switch (Battle2DManager.instance().triangle().isPush(x, y))
					{
					case 0:
						if (pl.PlayerParty.instance().saveStartCommand(battlePlayer.playerId()) > 0)
						{
							menu.MenuManager.getSingleton().playSEMoveCursor();
						}
						flag = true;
						flag2 = true;
						pl.PlayerParty.instance().setSaveStartCommand(battlePlayer.playerId(), 0);
						battleWindow_.updateCommandMessage(battlePlayer);
						command.setNowCommand((sbyte)cursorLine_);
						break;
					case 1:
						if (pl.PlayerParty.instance().saveStartCommand(battlePlayer.playerId()) < pl.COMMAND_MAX - pl.BATTLE_COMMAND_MAX)
						{
							menu.MenuManager.getSingleton().playSEMoveCursor();
						}
						flag = true;
						flag2 = true;
						pl.PlayerParty.instance().setSaveStartCommand(battlePlayer.playerId(), pl.COMMAND_MAX - pl.BATTLE_COMMAND_MAX);
						battleWindow_.updateCommandMessage(battlePlayer);
						command.setNowCommand((sbyte)(cursorLine_ + pl.COMMAND_MAX - pl.BATTLE_COMMAND_MAX));
						break;
					}
				}
				int num = pl.PlayerParty.instance().saveStartCommand(battlePlayer.playerId());
				int flickOffset = ds.g_TouchPanel.getDispPoint().flickOffset;
				if (flickOffset < 0 && num > 0)
				{
					menu.MenuManager.getSingleton().playSEMoveCursor();
					pl.PlayerParty.instance().setSaveStartCommand(battlePlayer.playerId(), 0);
					battleWindow_.updateCommandMessage(battlePlayer);
					command.setNowCommand((sbyte)cursorLine_);
				}
				if (flickOffset > 0 && num < pl.COMMAND_MAX - pl.BATTLE_COMMAND_MAX)
				{
					menu.MenuManager.getSingleton().playSEMoveCursor();
					pl.PlayerParty.instance().setSaveStartCommand(battlePlayer.playerId(), pl.COMMAND_MAX - pl.BATTLE_COMMAND_MAX);
					battleWindow_.updateCommandMessage(battlePlayer);
					command.setNowCommand((sbyte)(cursorLine_ + pl.COMMAND_MAX - pl.BATTLE_COMMAND_MAX));
				}
				cursorLine_ = ds.clamp(cursorLine_, 0, pl.BATTLE_COMMAND_MAX - 1);
				command.loopNowCommand();
				Battle2DManager.instance().cursor().setPositionCommand(0, cursorLine_);
				pl.PlayerParty.instance().setSaveCommandPosition(battlePlayer.playerId(), cursorLine_);
				if (flag2)
				{
					return false;
				}
				if (edgeDecide())
				{
					if (battlePlayer.condition().isFrog())
					{
						if (!command.isSelectCommandFrog(command.nowCommand()))
						{
							menu.MenuManager.getSingleton().playSEBeep();
							return false;
						}
					}
					else if (command.commandId(command.nowCommand()) == 18 && !battlePlayer.player().equipParameter().isEquipHarp())
					{
						menu.MenuManager.getSingleton().playSEBeep();
						return false;
					}
					return selectCommandDecide(B);
				}
				if (edgeCancel())
				{
					return selectCommandCancel(B);
				}
				if ((ds.g_Pad.pad() & 0x200) != 0 && (ds.g_Pad.pad() & 0x100) != 0)
				{
					menu.MenuManager.getSingleton().playSEDecide();
					commandEscape(battlePlayer);
					return true;
				}
				if (ds.g_TouchPanel.isTap())
				{
					ds.g_TouchPanel.getPoint(out var x2, out var y2);
					VecFx32 vecFx = battleWindow_.isSelectTouchPanel(x2, y2, battlePlayer);
					int x3 = vecFx.x;
					if (x3 == -1)
					{
						switch (selectCharacterTouchPanel(B))
						{
						case 4:
							return selectCommandCancel(B);
						default:
							menu.MenuManager.getSingleton().playSEDecide();
							Battle2DManager.instance().cursor().passive(0);
							battleWindow_.setOnOff(battlePlayer);
							cancelWindow_.setShowTarget(show: true);
							cancelWindow_.createTargetMessage(50021);
							if (B.characterManager().getBaseBattleCharacterFromBreed(battlePlayer.targetId(0)).breed() == 0)
							{
								commandAttack2(battlePlayer, B.characterManager().playerParty(), 1);
							}
							else
							{
								commandAttack(battlePlayer, B.characterManager().monsterParty(), 1);
							}
							return false;
						case 0:
							if (battlePlayer.condition().isFrog())
							{
								if (!command.isSelectCommandFrog(command.nowCommand()))
								{
									menu.MenuManager.getSingleton().playSEBeep();
									return false;
								}
							}
							else if (command.commandId(command.nowCommand()) == 18 && !battlePlayer.player().equipParameter().isEquipHarp())
							{
								menu.MenuManager.getSingleton().playSEBeep();
								return false;
							}
							return selectCommandDecide(B);
						}
					}
					cursorLine_ = vecFx.y;
					cursorLine_ = ds.clamp(cursorLine_, 0, pl.BATTLE_COMMAND_MAX - 1);
					Battle2DManager.instance().cursor().setPositionCommand(0, cursorLine_);
					pl.PlayerParty.instance().setSaveCommandPosition(battlePlayer.playerId(), cursorLine_);
					command.nowCommand_set((sbyte)x3);
					if (battlePlayer.condition().isFrog())
					{
						if (!command.isSelectCommandFrog(command.nowCommand()))
						{
							menu.MenuManager.getSingleton().playSEBeep();
							return false;
						}
					}
					else if (command.commandId(command.nowCommand()) == 18 && !battlePlayer.player().equipParameter().isEquipHarp())
					{
						menu.MenuManager.getSingleton().playSEBeep();
						return false;
					}
					menu.MenuManager.getSingleton().playSEDecide();
					Battle2DManager.instance().cursor().passive(0);
					battleWindow_.commandWindow().moveMessage((command.nowCommand() == 6) ? 3 : (command.nowCommand() - pl.PlayerParty.instance().saveStartCommand(battlePlayer.playerId())));
					return commandAction(B);
				}
				if (!ds.g_TouchPanel.isTouch())
				{
					clearTouchTargetId();
					clearTouchStartPosition();
					touchPlayer_ = false;
				}
				return false;
			}

			public bool selectCommandDecide(BattleSystem B)
			{
				menu.MenuManager.getSingleton().playSEDecide();
				Battle2DManager.instance().cursor().passive(0);
				return commandAction(B);
			}

			public bool selectCommandCancel(BattleSystem B)
			{
				menu.MenuManager.getSingleton().playSECancel();
				if (nowPlayer_ != B.characterManager().playerParty().getMinBattlePlayerId())
				{
					Battle2DManager.instance().cursor().nondisplayAll();
					isCancel_ = true;
					return true;
				}
				return false;
			}

			public bool selectTargetTouchPanel(BattleSystem B)
			{
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayer_);
				SELECT_STATE sELECT_STATE = selectState_;
				if (sELECT_STATE == SELECT_STATE.TOUCH_IDLE && ds.g_TouchPanel.isTap())
				{
					ds.g_TouchPanel.getPoint(out touchStartX_, out touchStartY_);
					int num = selectCharacterTouchPanel(B);
					switch (num)
					{
					case 4:
						selectTargetCancel(battlePlayer);
						return false;
					default:
					{
						if (battlePlayer.targetId(0) == battlePlayer.lastTargetId() && (selectTargetState_ == 1 || num == 5))
						{
							if (battlePlayer.isSetAbility())
							{
								if (B.characterManager().playerParty().getbattleCharacterIdPlayer(battlePlayer.targetId(0)) != null)
								{
									selectTargetCancel(battlePlayer);
									return false;
								}
							}
							else if (battlePlayer.isDrain() && battlePlayer.battleCharacterId() == battlePlayer.targetId(0))
							{
								selectTargetCancel(battlePlayer);
								return false;
							}
							selectTargetDecide(B, battlePlayer);
							return true;
						}
						battlePlayer.setLastTargetId();
						if (battlePlayer.isSetAbility())
						{
							if (B.characterManager().playerParty().getbattleCharacterIdPlayer(battlePlayer.lastTargetId()) != null)
							{
								selectTargetCancel(battlePlayer);
								return false;
							}
						}
						else if (battlePlayer.isDrain() && battlePlayer.battleCharacterId() == battlePlayer.lastTargetId())
						{
							selectTargetCancel(battlePlayer);
							return false;
						}
						BaseBattleCharacter baseBattleCharacterFromBreed = B.characterManager().getBaseBattleCharacterFromBreed(battlePlayer.targetId(0));
						if (selectTargetState_ != 1)
						{
							selectTargetState_ = 1;
							Battle2DManager.instance().cursor().nondisplayAll();
							Battle2DManager.instance().cursor().active(1);
							Battle2DManager.instance().cursor().active(15);
						}
						if (baseBattleCharacterFromBreed.breed() == 0)
						{
							BattlePlayer player = static_cast<BattlePlayer>(baseBattleCharacterFromBreed);
							if (commandState_ != COMMAND_STATE.SELECT_PLAYER)
							{
								setTargetFriend(battlePlayer, B.characterManager().playerParty(), itm.TARGET_POSITION.TARGET_POSITION_FRIEND, 0, 1, 0);
								setCommandState(COMMAND_STATE.SELECT_PLAYER);
							}
							Battle2DManager.instance().cursor().setPositionPlayer(1, player);
							Battle2DManager.instance().cursor().setPositionTargetPlayer(15, player);
						}
						else
						{
							BattleMonster monster = static_cast<BattleMonster>(baseBattleCharacterFromBreed);
							if (commandState_ != COMMAND_STATE.SELECT_ENEMY)
							{
								setTargetEnemy(battlePlayer, B.characterManager().monsterParty(), 0, 1, 0);
								setCommandState(COMMAND_STATE.SELECT_ENEMY);
							}
							Battle2DManager.instance().cursor().setPositionMonster(1, monster);
							Battle2DManager.instance().cursor().setPositionTargetMonster(15, monster);
						}
						menu.MenuManager.getSingleton().playSEMoveCursor();
						return false;
					}
					case 0:
						selectTargetDecide(B, battlePlayer);
						return true;
					case 3:
						break;
					}
				}
				if (!ds.g_TouchPanel.isTouch())
				{
					selectState_ = SELECT_STATE.TOUCH_IDLE;
					clearTouchTargetId();
					clearTouchStartPosition();
				}
				return false;
			}

			public bool AandBandTouchPanel(BattleSystem B, BattlePlayer player)
			{
				if (edgeDecide())
				{
					selectTargetDecide(B, player);
					return true;
				}
				if (edgeCancel())
				{
					selectTargetCancel(player);
					return false;
				}
				if (selectTargetTouchPanel(B))
				{
					return true;
				}
				return false;
			}

			public bool selectEnemy(BattleSystem B)
			{
				bool result = false;
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayer_);
				if (!battlePlayer.isSetAbility() && !B.characterManager().playerParty().isTargetDrain(battlePlayer))
				{
					changeWindow_.setShowTarget(show: true);
					changeWindow_.createTargetMessage(82);
				}
				switch (selectTargetState_)
				{
				case 1:
					result = selectEnemyOne(B);
					break;
				case 2:
					result = selectEnemyGroup(B);
					break;
				case 4:
					result = selectEnemyAll(B);
					break;
				}
				return result;
			}

			public bool selectEnemyOne(BattleSystem B)
			{
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayer_);
				BattleMonsterParty battleMonsterParty = B.characterManager().monsterParty();
				bool flag = false;
				if (battlePlayer.targetId(0) == -1)
				{
					createTargetWindowEnemy(battlePlayer, battleMonsterParty);
					sbyte b = battleMonsterParty.isBattleMonsterFront();
					if (b == -1)
					{
						battlePlayer.targetId_set(0, battleMonsterParty.battleMonster(battleMonsterParty.getTopBattleMonsterId()).battleCharacterId());
					}
					else
					{
						battlePlayer.targetId_set(0, battleMonsterParty.battleMonster(b).battleCharacterId());
					}
					flag = true;
				}
				if (edgePad(32))
				{
					if (selectEnemyLeft(B) && battleMonsterParty.aliveNumber() > 1 && (allSelectFlag_ & 4) != 0)
					{
						selectTargetState_ = 4;
						B.characterManager().setMonsterAllTarget(battlePlayer);
						Battle2DManager.instance().cursor().hidden(1);
						Battle2DManager.instance().cursor().setPositionTargetAll(15);
						Battle2DManager.instance().cursor().setPositionMonsterAll(battleMonsterParty);
						return false;
					}
					flag = true;
				}
				else if (edgePad(16))
				{
					if (selectEnemyRight(B))
					{
						battlePlayer.clearTargetId();
						setCommandState(COMMAND_STATE.SELECT_PLAYER);
						return false;
					}
					flag = true;
				}
				else if (edgePad(64))
				{
					if (selectEnemyUp(B) && battleMonsterParty.aliveNumber() > 1 && (allSelectFlag_ & 2) != 0)
					{
						int num = battleMonsterParty.getbattleCharacterIdMonster(battlePlayer.targetId(0)).monsterId();
						if (battleMonsterParty.checkSameMonster(num))
						{
							upOrDown_ = 0;
							selectTargetState_ = 2;
							B.characterManager().setMonsterGroupTarget(battlePlayer);
							Battle2DManager.instance().cursor().hidden(1);
							Battle2DManager.instance().cursor().setPositionTargetAll(15);
							Battle2DManager.instance().cursor().setPositionMonsterGroup(battleMonsterParty, num);
							return false;
						}
						selectTargetState_ = 4;
						B.characterManager().setMonsterAllTarget(battlePlayer);
						Battle2DManager.instance().cursor().hidden(1);
						Battle2DManager.instance().cursor().setPositionTargetAll(15);
						Battle2DManager.instance().cursor().setPositionMonsterAll(battleMonsterParty);
						return false;
					}
					flag = true;
				}
				else if (edgePad(128))
				{
					if (selectEnemyDown(B) && battleMonsterParty.aliveNumber() > 1 && (allSelectFlag_ & 2) != 0)
					{
						int num2 = battleMonsterParty.getbattleCharacterIdMonster(battlePlayer.targetId(0)).monsterId();
						if (battleMonsterParty.checkSameMonster(num2))
						{
							upOrDown_ = 1;
							selectTargetState_ = 2;
							B.characterManager().setMonsterGroupTarget(battlePlayer);
							Battle2DManager.instance().cursor().hidden(1);
							Battle2DManager.instance().cursor().setPositionTargetAll(15);
							Battle2DManager.instance().cursor().setPositionMonsterGroup(battleMonsterParty, num2);
							return false;
						}
						selectTargetState_ = 4;
						B.characterManager().setMonsterAllTarget(battlePlayer);
						Battle2DManager.instance().cursor().hidden(1);
						Battle2DManager.instance().cursor().setPositionTargetAll(15);
						Battle2DManager.instance().cursor().setPositionMonsterAll(battleMonsterParty);
						return false;
					}
					flag = true;
				}
				BaseBattleCharacter baseBattleCharacterFromBreed = B.characterManager().getBaseBattleCharacterFromBreed(battlePlayer.targetId(0));
				if (baseBattleCharacterFromBreed.breed() == 1)
				{
					BattleMonster monster = battleMonsterParty.getbattleCharacterIdMonster(battlePlayer.targetId(0));
					Battle2DManager.instance().cursor().setPositionMonster(1, monster);
					Battle2DManager.instance().cursor().setPositionTargetMonster(15, monster);
					if (flag)
					{
						battlePlayer.setLastTargetId();
						return false;
					}
				}
				else
				{
					BattleParty battleParty = B.characterManager().playerParty();
					BattlePlayer player = battleParty.getbattleCharacterIdPlayer(battlePlayer.targetId(0));
					Battle2DManager.instance().cursor().setPositionPlayer(1, player);
					Battle2DManager.instance().cursor().setPositionTargetPlayer(15, player);
					if (flag)
					{
						battlePlayer.setLastTargetId();
						return false;
					}
				}
				return AandBandTouchPanel(B, battlePlayer);
			}

			public bool selectEnemyGroup(BattleSystem B)
			{
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayer_);
				if (upOrDown_ == 0 && edgePad(128))
				{
					selectTargetState_ = 1;
					int num = battlePlayer.lastTargetId();
					battlePlayer.clearTargetId();
					battlePlayer.targetId_set(0, (short)num);
					Battle2DManager.instance().cursor().nondisplayAll();
					Battle2DManager.instance().cursor().active(1);
					Battle2DManager.instance().cursor().active(15);
					return false;
				}
				if (upOrDown_ == 1 && edgePad(64))
				{
					selectTargetState_ = 1;
					int num2 = battlePlayer.lastTargetId();
					battlePlayer.clearTargetId();
					battlePlayer.targetId_set(0, (short)num2);
					Battle2DManager.instance().cursor().nondisplayAll();
					Battle2DManager.instance().cursor().active(1);
					Battle2DManager.instance().cursor().active(15);
					return false;
				}
				return AandBandTouchPanel(B, battlePlayer);
			}

			public bool selectEnemyAll(BattleSystem B)
			{
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayer_);
				BattleMonsterParty positionMonsterAll = B.characterManager().monsterParty();
				bool flag = false;
				if (battlePlayer.targetId(0) == -1)
				{
					B.characterManager().setMonsterAllTarget(battlePlayer);
					flag = true;
				}
				if (edgePad(16) && (allSelectFlag_ & 1) != 0)
				{
					selectTargetState_ = 1;
					int num = battlePlayer.lastTargetId();
					battlePlayer.clearTargetId();
					battlePlayer.targetId_set(0, (short)num);
					Battle2DManager.instance().cursor().nondisplayAll();
					Battle2DManager.instance().cursor().active(1);
					Battle2DManager.instance().cursor().active(15);
					return false;
				}
				if (flag)
				{
					B.characterManager().setMonsterAllTarget(battlePlayer);
					Battle2DManager.instance().cursor().hidden(1);
					Battle2DManager.instance().cursor().setPositionTargetAll(15);
					Battle2DManager.instance().cursor().setPositionMonsterAll(positionMonsterAll);
				}
				return AandBandTouchPanel(B, battlePlayer);
			}

			public bool selectPlayer(BattleSystem B)
			{
				bool result = false;
				changeWindow_.setShowTarget(show: true);
				changeWindow_.createTargetMessage(83);
				switch (selectTargetState_)
				{
				case 1:
					result = selectPlayerOne(B);
					break;
				case 2:
				case 4:
					result = selectPlayerAll(B);
					break;
				}
				return result;
			}

			public bool selectPlayerOne(BattleSystem B)
			{
				BattleParty battleParty = B.characterManager().playerParty();
				BattlePlayer battlePlayer = battleParty.battlePlayer(nowPlayer_);
				bool flag = false;
				if (battlePlayer.targetId(0) == -1)
				{
					createTargetWindowPlayer(battlePlayer, battleParty);
					flag = true;
					if (firstTargetPlayerId_ != -1)
					{
						battlePlayer.setTargetId(0, (short)firstTargetPlayerId_);
					}
					else if (battlePlayer.isSelectDeadOrStoneTargetCommand())
					{
						short condition = 0;
						if (battlePlayer.isSelectDeadTarget())
						{
							condition = 512;
						}
						if (battlePlayer.isSelectStoneTarget())
						{
							condition = 8;
						}
						setTargetBadConditionPlayer(battlePlayer, battleParty, condition);
					}
					else
					{
						battlePlayer.setTargetId(0, battleParty.battlePlayer(battleParty.getMinBattlePlayerId()).battleCharacterId());
						BattlePlayer battlePlayer2 = battleParty.getbattleCharacterIdPlayer(battlePlayer.targetId(0));
						if (battlePlayer2 != null)
						{
							if (battlePlayer2.battleCharacterId() == battlePlayer.battleCharacterId() && battlePlayer2.isDrain())
							{
								selectPlayerPad(B, 1);
							}
							if (battlePlayer2.flag(PLAYER_FLAG.PF_JUMP))
							{
								selectPlayerPad(B, 1);
							}
						}
					}
					if (battlePlayer.targetId(0) == -1)
					{
						battlePlayer.setTargetId(0, battleParty.battlePlayer(battleParty.getMinBattlePlayerId()).battleCharacterId());
					}
				}
				if (edgePad(32))
				{
					battlePlayer.clearTargetId();
					setCommandState(COMMAND_STATE.SELECT_ENEMY);
					return false;
				}
				if (edgePad(16))
				{
					if (battleParty.aliveNumber() > 1 && ((allSelectFlag_ & 2) != 0 || (allSelectFlag_ & 4) != 0))
					{
						selectTargetState_ = 4;
						B.characterManager().setPlayerAllTarget(battlePlayer, battlePlayer.isSelectDeadOrStoneTargetCommand() ? 1 : 0);
						Battle2DManager.instance().cursor().hidden(1);
						Battle2DManager.instance().cursor().setPositionTargetAll(15);
						Battle2DManager.instance().cursor().setPositionPlayerAll(battleParty, battlePlayer.isSelectDeadOrStoneTargetCommand() ? 1 : 0);
						return false;
					}
				}
				else if (edgePad(64))
				{
					selectPlayerPad(B, 0);
					flag = true;
				}
				else if (edgePad(128))
				{
					selectPlayerPad(B, 1);
					flag = true;
				}
				if (flag)
				{
					battlePlayer.setLastTargetId();
					BaseBattleCharacter baseBattleCharacterFromBreed = B.characterManager().getBaseBattleCharacterFromBreed(battlePlayer.targetId(0));
					if (baseBattleCharacterFromBreed.breed() == 0)
					{
						Battle2DManager.instance().cursor().setPositionPlayer(1, baseBattleCharacterFromBreed);
						Battle2DManager.instance().cursor().setPositionTargetPlayer(15, baseBattleCharacterFromBreed);
					}
					else
					{
						BattleMonsterParty battleMonsterParty = B.characterManager().monsterParty();
						BattleMonster monster = battleMonsterParty.getbattleCharacterIdMonster(battlePlayer.targetId(0));
						Battle2DManager.instance().cursor().setPositionMonster(1, monster);
						Battle2DManager.instance().cursor().setPositionTargetMonster(15, monster);
					}
				}
				return AandBandTouchPanel(B, battlePlayer);
			}

			public bool selectPlayerAll(BattleSystem B)
			{
				BattleParty battleParty = B.characterManager().playerParty();
				BattlePlayer battlePlayer = battleParty.battlePlayer(nowPlayer_);
				bool flag = false;
				if (!battlePlayer.isTargetId())
				{
					B.characterManager().setPlayerAllTarget(battlePlayer, battlePlayer.isSelectDeadOrStoneTargetCommand() ? 1 : 0);
					flag = true;
				}
				if (edgePad(32) && (allSelectFlag_ & 1) != 0)
				{
					selectTargetState_ = 1;
					int num = battlePlayer.lastTargetId();
					battlePlayer.clearTargetId();
					battlePlayer.targetId_set(0, (short)num);
					Battle2DManager.instance().cursor().nondisplayAll();
					Battle2DManager.instance().cursor().active(1);
					Battle2DManager.instance().cursor().active(15);
					return false;
				}
				if (flag)
				{
					BaseBattleCharacter baseBattleCharacter = null;
					for (int i = 0; i < 4; i++)
					{
						baseBattleCharacter = B.characterManager().getBaseBattleCharacterFromBreed(battlePlayer.targetId(i));
						if (baseBattleCharacter != null)
						{
							break;
						}
					}
					Battle2DManager.instance().cursor().setPositionPlayer(1, baseBattleCharacter);
					Battle2DManager.instance().cursor().setPositionTargetPlayer(15, baseBattleCharacter);
					Battle2DManager.instance().cursor().hidden(1);
					Battle2DManager.instance().cursor().setPositionTargetAll(15);
					Battle2DManager.instance().cursor().setPositionPlayerAll(battleParty, battlePlayer.isSelectDeadOrStoneTargetCommand() ? 1 : 0);
				}
				return AandBandTouchPanel(B, battlePlayer);
			}

			public void selectTargetDecide(BattleSystem B, BattlePlayer player)
			{
				if (B.characterManager().getTargetType(player) == 0)
				{
					player.setTargetType(PLAYER_FLAG.PF_TARGET_PLAYER);
				}
				else
				{
					player.setTargetType(PLAYER_FLAG.PF_TARGET_MONSTER);
				}
				if (selectTargetState_ == 1)
				{
					int num = -1;
					for (int i = 0; i < 12; i++)
					{
						if (player.targetId(i) >= 0)
						{
							num = player.targetId(i);
							break;
						}
					}
					player.clearTargetId();
					player.setTargetId(0, (short)num);
				}
				menu.MenuManager.getSingleton().playSEDecide();
				selectTargetState_ = 1;
				selectState_ = SELECT_STATE.TOUCH_IDLE;
				Battle2DManager.instance().cursor().nondisplayAll();
				Battle2DManager.instance().helpWindow().releaseHelpWindow();
				for (int j = 0; j < 4; j++)
				{
					targetWindow_[j].setShowTarget(show: false);
				}
				allWindow_.setShowTarget(show: false);
				changeWindow_.setShowTarget(show: false);
				groupWindow_.setShowTarget(show: false);
			}

			public void selectTargetCancel(BattlePlayer player)
			{
				player.clearTargetType();
				menu.MenuManager.getSingleton().playSECancel();
				selectTargetState_ = 1;
				selectState_ = SELECT_STATE.TOUCH_IDLE;
				rebornParameter(player);
				Battle2DManager.instance().cursor().nondisplayAll();
				Battle2DManager.instance().cursor().active(0);
				battleWindow_.setOff();
				setCommandState(COMMAND_STATE.SELECT_COMMAND);
				Battle2DManager.instance().helpWindow().releaseHelpWindow();
				for (int i = 0; i < 4; i++)
				{
					targetWindow_[i].setShowTarget(show: false);
				}
				allWindow_.setShowTarget(show: false);
				changeWindow_.setShowTarget(show: false);
				groupWindow_.setShowTarget(show: false);
				settingCommand(player);
				player.clearTargetId();
			}

			public bool selectMagic(BattleSystem B)
			{
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayer_);
				switch (windowState_)
				{
				case WINDOW_STATE.NO_WINDOW:
					if (createSelectWindow("battle_magic", battlePlayer))
					{
						windowState_ = WINDOW_STATE.WINDOW_CREATING;
					}
					break;
				case WINDOW_STATE.WINDOW_CREATING:
					if (creatingSelectWindow("battle_magic", battlePlayer))
					{
						menu.MenuManager.getSingleton().execute();
						int targetItemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
						itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter((short)targetItemNo);
						if (magicParameter != null)
						{
							Battle2DManager.instance().helpWindow().createHelpWindow(magicParameter.captionId(), 0, 1);
						}
						Battle2DManager.instance().helpWindow().setMsdHandle(1);
						menu.MenuManager.getSingleton().GetMenuWindowObj()[windowId_].GetWindowHandle().SetBar(2);
						windowState_ = WINDOW_STATE.WINDOW_CREATE;
					}
					break;
				case WINDOW_STATE.WINDOW_CREATE:
					if (createEndAndSelectMagic(battlePlayer))
					{
						windowState_ = WINDOW_STATE.WINDOW_RELEASING;
					}
					break;
				case WINDOW_STATE.WINDOW_RELEASING:
					windowState_ = WINDOW_STATE.WINDOW_RELEASE;
					break;
				case WINDOW_STATE.WINDOW_RELEASE:
					if (releaseMagicWindow(B, battlePlayer))
					{
						windowState_ = WINDOW_STATE.NO_WINDOW;
						if (battlePlayer.actionId() == 6)
						{
							return true;
						}
						if (battlePlayer.useMagicId() == 4008)
						{
							return true;
						}
						if (allSelectFlag_ == 4)
						{
							return true;
						}
					}
					break;
				}
				return false;
			}

			public bool selectItem(BattleSystem B)
			{
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayer_);
				switch (windowState_)
				{
				case WINDOW_STATE.NO_WINDOW:
					if (createSelectWindow("battle_item", battlePlayer))
					{
						windowState_ = WINDOW_STATE.WINDOW_CREATING;
					}
					break;
				case WINDOW_STATE.WINDOW_CREATING:
					if (creatingSelectWindow("battle_item", battlePlayer))
					{
						windowState_ = WINDOW_STATE.WINDOW_CREATE;
					}
					break;
				case WINDOW_STATE.WINDOW_CREATE:
					if (createEndAndSelectItem(battlePlayer))
					{
						windowState_ = WINDOW_STATE.WINDOW_RELEASING;
					}
					break;
				case WINDOW_STATE.WINDOW_RELEASING:
				{
					battlePlayer.unregisterWeapon(pl.HAND_TYPE.RIGHT_HAND);
					battlePlayer.registerWeapon(pl.HAND_TYPE.RIGHT_HAND);
					battlePlayer.unregisterWeapon(pl.HAND_TYPE.LEFT_HAND);
					battlePlayer.registerWeapon(pl.HAND_TYPE.LEFT_HAND);
					battlePlayer.resetParameterMagicFlag();
					CommonFormula commonFormula = new CommonFormula();
					battlePlayer.reupdateParameter(commonFormula.calcJobSkill(battlePlayer));
					windowState_ = WINDOW_STATE.WINDOW_RELEASE;
					break;
				}
				case WINDOW_STATE.WINDOW_RELEASE:
					if (releaseItemWindow(B, battlePlayer))
					{
						windowState_ = WINDOW_STATE.NO_WINDOW;
						if (allSelectFlag_ == 4)
						{
							return true;
						}
					}
					break;
				}
				return false;
			}

			public bool selectSong(BattleSystem B)
			{
				BattlePlayer player = B.characterManager().playerParty().battlePlayer(nowPlayer_);
				switch (windowState_)
				{
				case WINDOW_STATE.NO_WINDOW:
					if (createSelectWindow("battle_song", player))
					{
						windowState_ = WINDOW_STATE.WINDOW_CREATING;
					}
					break;
				case WINDOW_STATE.WINDOW_CREATING:
					if (creatingSelectWindow("battle_song", player))
					{
						Battle2DManager.instance().helpWindow().setMsdHandle(1);
						windowState_ = WINDOW_STATE.WINDOW_CREATE;
					}
					break;
				case WINDOW_STATE.WINDOW_CREATE:
					if (createEndAndSelectMagic(player))
					{
						windowState_ = WINDOW_STATE.WINDOW_RELEASING;
					}
					break;
				case WINDOW_STATE.WINDOW_RELEASING:
					windowState_ = WINDOW_STATE.WINDOW_RELEASE;
					break;
				case WINDOW_STATE.WINDOW_RELEASE:
					if (releaseMagicWindow(B, player))
					{
						windowState_ = WINDOW_STATE.NO_WINDOW;
					}
					break;
				}
				return false;
			}

			public bool createSelectWindow(string name, BattlePlayer player)
			{
				if (strcmp(name, "battle_item") == 0)
				{
					pl.Command command = player.player().jobManager().command();
					if (command.commandId(command.nowCommand()) == 47)
					{
						name = "battle_item_e";
					}
				}
				windowId_ = menu.MenuManager.getSingleton().buildWindow(name, TRANSCODE("m_main"));
				playerWindow_.release();
				return true;
			}

			public bool creatingSelectWindow(string name, BattlePlayer player)
			{
				if (menu.MenuManager.getSingleton().UpdateWindowState(windowId_, menu.MenuWindow.MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_LARGE))
				{
					return false;
				}
				player.player().updateParameter();
				menu.MenuManager.getSingleton().SetTargetCharNo(nowPlayer_);
				menu.MenuManager.getSingleton().CreateNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
				if (strcmp(name, "battle_item") == 0)
				{
					bool equipMode = false;
					pl.Command command = player.player().jobManager().command();
					if (command.commandId(command.nowCommand()) == 47)
					{
						equipMode = true;
					}
					if (command.commandId(command.nowCommand()) == 47)
					{
						menu.MenuManager.getSingleton().SetItemListPatern(3);
					}
					else if (command.commandId(command.nowCommand()) == 40)
					{
						menu.MenuManager.getSingleton().SetItemListPatern(5);
					}
					else
					{
						menu.MenuManager.getSingleton().SetItemListPatern(11);
					}
					menu.MenuBattleItem.getSingleton().SetUpBItemStatus(equipMode, windowId_);
					return true;
				}
				return menu.MenuManager.getSingleton().buildMenu(name);
			}

			public bool createEndAndSelectMagic(BattlePlayer player)
			{
				int targetItemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
				menu.MenuManager.getSingleton().execute();
				int targetItemNo2 = menu.MenuManager.getSingleton().GetTargetItemNo();
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter((short)targetItemNo2);
				int num = 0;
				if (magicParameter != null)
				{
					num = ((magicParameter.system() == 3) ? 99 : player.player().mp(magicParameter.magicClass()).getNow());
				}
				if (magicParameter != null)
				{
					if (magicParameter.captionId() != Battle2DManager.instance().helpWindow().messageId())
					{
						if (Battle2DManager.instance().helpWindow().messageId() != 0)
						{
							Battle2DManager.instance().helpWindow().updateMessage(magicParameter.captionId(), Battle2DManager.instance().helpWindow().msdHandle());
						}
						else
						{
							Battle2DManager.instance().helpWindow().createHelpWindow(magicParameter.captionId(), 0, 1);
							Battle2DManager.instance().helpWindow().setMsdHandle(1);
						}
					}
				}
				else
				{
					Battle2DManager.instance().helpWindow().releaseHelpWindow();
				}
				if (edgeDecide())
				{
					if (isUseMagic(targetItemNo2, player))
					{
						if (magicParameter.system() == 3)
						{
							player.setActionId(20);
						}
						else if (magicParameter.system() == 2)
						{
							player.setActionId(6);
						}
						else
						{
							player.setActionId(5);
						}
						player.setUseMagicId((short)targetItemNo2);
						menu.MenuManager.getSingleton().playSEDecide();
						return true;
					}
					menu.MenuManager.getSingleton().playSEBeep();
					allSelectFlag_ = 0;
				}
				else if (edgeCancel())
				{
					isCancel_ = true;
					allSelectFlag_ = 0;
					menu.MenuManager.getSingleton().playSECancel();
					return true;
				}
				if (ds.g_TouchPanel.isTap())
				{
					ds.g_TouchPanel.getPoint(out var x, out var y);
					if (cancelWindow_.isShowTarget() && cancelWindow_.isTouch(x, y))
					{
						isCancel_ = true;
						allSelectFlag_ = 0;
						menu.MenuManager.getSingleton().playSECancel();
						return true;
					}
					if (magicParameter == null && menu.MenuManager.getSingleton().checkTouchState() == menu.MenuManager.TOUCH_STATE.TOUCH_ITEM)
					{
						menu.MenuManager.getSingleton().playSEBeep();
						allSelectFlag_ = 0;
						return false;
					}
					bool flag = false;
					if (menu.MenuManager.getSingleton().TouchWindowOutArea(x, y))
					{
						flag = true;
					}
					if (menu.MenuManager.getSingleton().checkTouchState() == menu.MenuManager.TOUCH_STATE.TOUCH_ITEM)
					{
						if (targetItemNo != targetItemNo2)
						{
							return false;
						}
						flag = true;
					}
					if (flag)
					{
						if (targetItemNo != targetItemNo2)
						{
							return false;
						}
						if (isUseMagic(targetItemNo2, player))
						{
							if (magicParameter.system() == 3)
							{
								player.setActionId(20);
							}
							else if (magicParameter.system() == 2)
							{
								if (num == 0)
								{
									menu.MenuManager.getSingleton().playSEBeep();
									allSelectFlag_ = 0;
									return false;
								}
								player.setActionId(6);
							}
							else
							{
								if (num == 0)
								{
									menu.MenuManager.getSingleton().playSEBeep();
									allSelectFlag_ = 0;
									return false;
								}
								player.setActionId(5);
							}
							player.setUseMagicId((short)targetItemNo2);
							menu.MenuManager.getSingleton().playSEDecide();
							return true;
						}
						allSelectFlag_ = 0;
						menu.MenuManager.getSingleton().playSEBeep();
					}
				}
				return false;
			}

			public bool releaseMagicWindow(BattleSystem B, BattlePlayer player)
			{
				menu.MenuManager.getSingleton().release();
				menu.MenuManager.getSingleton().releaseWindow(windowId_);
				menu.MenuManager.getSingleton().ClearBehaviorButton();
				menu.MenuManager.getSingleton().DeleteNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
				if (isCancel_)
				{
					cancelWindow(player);
				}
				else
				{
					itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(player.useMagicId());
					setAllTargetFlag(magicParameter.targetPossible());
					COMMAND_STATE cOMMAND_STATE = selectCommandStateEnemyOrPlayer(magicParameter.targetPosition());
					setCommandState(cOMMAND_STATE);
					if (magicParameter.system() != 3)
					{
						player.player().mp(magicParameter.magicClass()).subNow(1);
						player.player().setJobChangeMp(magicParameter.magicClass(), (byte)player.player().mp(magicParameter.magicClass()).getNow());
					}
					battleWindow_.create(player);
					battleWindow_.setOnOff(player);
					battleWindow_.display();
					playerWindow_.create();
					if (opt.COptionManager.getSingleton().cursorOption().cursorPosition() == opt.CURSOR_POSITION.CURSOR_POSITION_MEMORY)
					{
						cursorLine_ = pl.PlayerParty.instance().saveCommandPosition(player.playerId());
						battleWindow_.updateCommandMessage(player);
					}
					showTriangle(player);
					Battle2DManager.instance().cursor().setShow(0, show: true);
					Battle2DManager.instance().cursor().setPositionCommand(0, cursorLine_);
					if (magicParameter.itemId() == 4008)
					{
						player.clearTargetId();
						B.characterManager().setPlayerAllTarget(player, 0);
						Battle2DManager.instance().cursor().hidden(0);
						Battle2DManager.instance().helpWindow().releaseHelpWindow();
					}
					else if (magicParameter.system() == 2)
					{
						Battle2DManager.instance().cursor().hidden(0);
						Battle2DManager.instance().helpWindow().releaseHelpWindow();
					}
					else
					{
						switch (cOMMAND_STATE)
						{
						case COMMAND_STATE.SELECT_ENEMY:
							if (allSelectFlag_ == 4)
							{
								B.characterManager().setMonsterAllTarget(player);
								Battle2DManager.instance().cursor().hidden(0);
								Battle2DManager.instance().helpWindow().releaseHelpWindow();
							}
							else
							{
								battleWindow_.nondisplay();
								Battle2DManager.instance().cursor().nondisplayAll();
								setTargetEnemy(player, B.characterManager().monsterParty(), 1, 1, 0);
							}
							break;
						case COMMAND_STATE.SELECT_PLAYER:
						{
							if (allSelectFlag_ == 4)
							{
								B.characterManager().setPlayerAllTarget(player, player.isSelectDeadOrStoneTargetCommand() ? 1 : 0);
								Battle2DManager.instance().cursor().hidden(0);
								Battle2DManager.instance().helpWindow().releaseHelpWindow();
								break;
							}
							short condition = 0;
							if (magicParameter != null && magicParameter.magicUseKind() == 1)
							{
								condition = magicParameter.changeCondition();
							}
							battleWindow_.nondisplay();
							Battle2DManager.instance().cursor().nondisplayAll();
							setTargetFriend(player, B.characterManager().playerParty(), (itm.TARGET_POSITION)magicParameter.targetPosition(), 1, 1, condition);
							break;
						}
						}
					}
				}
				return true;
			}

			public bool createEndAndSelectItem(BattlePlayer player)
			{
				if (ds.g_TouchPanel.isTap())
				{
					ds.g_TouchPanel.getPoint(out var x, out var y);
					if (cancelWindow_.isShowTarget() && cancelWindow_.isTouch(x, y))
					{
						isCancel_ = true;
						menu.MenuManager.getSingleton().playSECancel();
						cancelItem();
						return true;
					}
				}
				int targetItemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
				menu.MenuBattleItem.getSingleton().ProcessingBItem();
				int targetItemNo2 = menu.MenuManager.getSingleton().GetTargetItemNo();
				itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)targetItemNo2);
				int num = 0;
				if (itemBaseParameter != null)
				{
					if (menu.MenuBattleItem.getSingleton().usingItemType() == menu.MenuBattleItem.USING_ITEM_TYPE.U_LOCAL_NORMAL)
					{
						if (pl.PlayerParty.instance().item().serchNormalItem((short)targetItemNo2) != null)
						{
							num = pl.PlayerParty.instance().item().serchNormalItem((short)targetItemNo2)
								.itemNumber();
						}
						else if (player.player().equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND)
							.itemId() == targetItemNo2)
						{
							num = player.player().equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND)
								.equipNumber()
								.get();
						}
						else if (player.player().equipParameter().equipHand(pl.HAND_TYPE.LEFT_HAND)
							.itemId() == targetItemNo2)
						{
							num = player.player().equipParameter().equipHand(pl.HAND_TYPE.LEFT_HAND)
								.equipNumber()
								.get();
						}
					}
					else
					{
						num = 99;
					}
				}
				if (itemBaseParameter != null && num > 0)
				{
					if (itemBaseParameter.captionId() != Battle2DManager.instance().helpWindow().messageId())
					{
						if (Battle2DManager.instance().helpWindow().messageId() != 0)
						{
							Battle2DManager.instance().helpWindow().updateMessage(itemBaseParameter.captionId(), Battle2DManager.instance().helpWindow().msdHandle());
						}
						else
						{
							Battle2DManager.instance().helpWindow().createHelpWindow(itemBaseParameter.captionId(), 0, 1);
							Battle2DManager.instance().helpWindow().setMsdHandle(1);
						}
					}
				}
				else
				{
					Battle2DManager.instance().helpWindow().releaseHelpWindow();
				}
				if (menu.MenuBattleItem.getSingleton().GetUserContact() != 1)
				{
					return false;
				}
				if (menu.MenuManager.getSingleton().GetScrollType() != menu.MenuManager.SCROLL_TYPE.TYPE_WAIT)
				{
					return false;
				}
				bool flag = false;
				if (ds.g_TouchPanel.isTap())
				{
					ds.g_TouchPanel.getPoint(out var x2, out var y2);
					if (menu.MenuManager.getSingleton().TouchWindowOutArea(x2, y2))
					{
						flag = true;
					}
				}
				if (!menu.MenuBattleItem.getSingleton().changeWindow() && (flag || edgeDecide()))
				{
					itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter((short)targetItemNo2);
					if (magicParameter != null)
					{
						menu.MenuManager.getSingleton().playSEBeep();
						return cancelItem();
					}
					pl.Command command = player.player().jobManager().command();
					if (command.commandId(command.nowCommand()) == 40)
					{
						if (selectItemPitch((short)targetItemNo2) && num > 0)
						{
							return decideItem(player, (short)targetItemNo2, PLAYER_BATTLE_ACTION.PBA_PITCH);
						}
						menu.MenuManager.getSingleton().playSEBeep();
						return cancelItem();
					}
					if (isUseItem(targetItemNo2, player) && num > 0)
					{
						return decideItem(player, (short)targetItemNo2, PLAYER_BATTLE_ACTION.PBA_ITEM);
					}
					menu.MenuManager.getSingleton().playSEBeep();
					return cancelItem();
				}
				if (edgeCancel())
				{
					isCancel_ = true;
					menu.MenuManager.getSingleton().playSECancel();
					cancelItem();
					return true;
				}
				if (ds.g_TouchPanel.isTap() && itemBaseParameter != null && menu.MenuManager.getSingleton().checkTouchState() == menu.MenuManager.TOUCH_STATE.TOUCH_ITEM)
				{
					if (targetItemNo != targetItemNo2)
					{
						return false;
					}
					itm.MagicParameter magicParameter2 = itm.ItemManager.instance().magicParameter((short)targetItemNo2);
					if (magicParameter2 != null)
					{
						menu.MenuManager.getSingleton().playSEBeep();
						return cancelItem();
					}
					pl.Command command2 = player.player().jobManager().command();
					if (command2.commandId(command2.nowCommand()) == 40)
					{
						if (selectItemPitch((short)targetItemNo2) && num > 0)
						{
							return decideItem(player, (short)targetItemNo2, PLAYER_BATTLE_ACTION.PBA_PITCH);
						}
						menu.MenuManager.getSingleton().playSEBeep();
						return cancelItem();
					}
					if (isUseItem(targetItemNo2, player) && num > 0)
					{
						return decideItem(player, (short)targetItemNo2, PLAYER_BATTLE_ACTION.PBA_ITEM);
					}
					menu.MenuManager.getSingleton().playSEBeep();
					return cancelItem();
				}
				if (ds.g_TouchPanel.isTap() && itemBaseParameter == null && menu.MenuManager.getSingleton().checkTouchState() == menu.MenuManager.TOUCH_STATE.TOUCH_ITEM)
				{
					menu.MenuManager.getSingleton().playSEBeep();
					return cancelItem();
				}
				return false;
			}

			public bool selectItemPitch(short itemId)
			{
				if (itm.ItemManager.instance().itemParameter(itemId) == null)
				{
					return false;
				}
				if (itm.ItemManager.instance().itemCategory(itemId) == itm.CATEGORY.CATEGORY_WEAPON)
				{
					return true;
				}
				return false;
			}

			public bool decideItem(BattlePlayer player, short itemId, PLAYER_BATTLE_ACTION actionType)
			{
				player.setActionId((int)actionType);
				player.setUseItemId(itemId);
				menu.MenuManager.getSingleton().playSEDecide();
				return true;
			}

			public bool cancelItem()
			{
				allSelectFlag_ = 0;
				return false;
			}

			public bool releaseItemWindow(BattleSystem B, BattlePlayer player)
			{
				menu.MenuBattleItem.getSingleton().ReleaseBItemStatus();
				menu.MenuManager.getSingleton().release();
				menu.MenuManager.getSingleton().ClearBehaviorButton();
				menu.MenuManager.getSingleton().releaseWindow(windowId_);
				menu.MenuManager.getSingleton().DeleteNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
				if (isCancel_)
				{
					cancelWindow(player);
				}
				else
				{
					itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)player.useItemId());
					setAllTargetFlag(itemBaseParameter.targetPossible());
					COMMAND_STATE cOMMAND_STATE = selectCommandStateEnemyOrPlayer(itemBaseParameter.targetPosition());
					if (player.actionId() == 22)
					{
						cOMMAND_STATE = COMMAND_STATE.SELECT_ENEMY;
						allSelectFlag_ = 1;
					}
					setCommandState(cOMMAND_STATE);
					if (player.actionId() == 22)
					{
						if (itm.ItemManager.instance().itemParameter((short)player.useItemId()) != null && menu.MenuBattleItem.getSingleton().usingItemType() == menu.MenuBattleItem.USING_ITEM_TYPE.U_LOCAL_NORMAL)
						{
							int itemNumber = pl.PlayerParty.instance().item().serchNormalItem((short)player.useItemId())
								.itemNumber() - 1;
							pl.PlayerParty.instance().item().serchNormalItem((short)player.useItemId())
								.setItemNumber(itemNumber);
						}
					}
					else if (itm.ItemManager.instance().consumptionParameter((short)player.useItemId()) != null && menu.MenuBattleItem.getSingleton().usingItemType() == menu.MenuBattleItem.USING_ITEM_TYPE.U_LOCAL_NORMAL)
					{
						int itemNumber2 = pl.PlayerParty.instance().item().serchNormalItem((short)player.useItemId())
							.itemNumber() - 1;
						pl.PlayerParty.instance().item().serchNormalItem((short)player.useItemId())
							.setItemNumber(itemNumber2);
					}
					battleWindow_.create(player);
					battleWindow_.setOnOff(player);
					battleWindow_.display();
					playerWindow_.create();
					if (opt.COptionManager.getSingleton().cursorOption().cursorPosition() == opt.CURSOR_POSITION.CURSOR_POSITION_MEMORY)
					{
						cursorLine_ = pl.PlayerParty.instance().saveCommandPosition(player.playerId());
						battleWindow_.updateCommandMessage(player);
					}
					showTriangle(player);
					Battle2DManager.instance().cursor().setShow(0, show: true);
					Battle2DManager.instance().cursor().setPositionCommand(0, cursorLine_);
					switch (cOMMAND_STATE)
					{
					case COMMAND_STATE.SELECT_ENEMY:
						if (allSelectFlag_ == 4)
						{
							B.characterManager().setMonsterAllTarget(player);
							Battle2DManager.instance().cursor().hidden(0);
							Battle2DManager.instance().helpWindow().releaseHelpWindow();
						}
						else
						{
							battleWindow_.nondisplay();
							Battle2DManager.instance().cursor().nondisplayAll();
							setTargetEnemy(player, B.characterManager().monsterParty(), 1, 1, 0);
						}
						break;
					case COMMAND_STATE.SELECT_PLAYER:
					{
						if (allSelectFlag_ == 4)
						{
							B.characterManager().setPlayerAllTarget(player, player.isSelectDeadOrStoneTargetCommand() ? 1 : 0);
							Battle2DManager.instance().cursor().hidden(0);
							Battle2DManager.instance().helpWindow().releaseHelpWindow();
							break;
						}
						short condition = 0;
						itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(itemBaseParameter.useItemId());
						if (magicParameter != null)
						{
							if (magicParameter.magicUseKind() == 1)
							{
								condition = magicParameter.changeCondition();
							}
						}
						else
						{
							itm.ConsumptionParameter consumptionParameter = itm.ItemManager.instance().consumptionParameter((short)player.useItemId());
							if (consumptionParameter != null && (consumptionParameter.itemType() & 1) != 0 && consumptionParameter.changeCondition() > 0)
							{
								condition = consumptionParameter.changeCondition();
							}
						}
						battleWindow_.nondisplay();
						Battle2DManager.instance().cursor().nondisplayAll();
						setTargetFriend(player, B.characterManager().playerParty(), (itm.TARGET_POSITION)itemBaseParameter.targetPosition(), 1, 1, condition);
						break;
					}
					}
				}
				return true;
			}

			public void cancelWindow(BattlePlayer player)
			{
				settingCommand(player);
				player.clearTargetId();
				rebornParameter(player);
				player.setNextPlayerActionId(BATTLE_ACTION_TYPE.DBA_CONDITION_MOTION);
				state(PLAYER_STATE.CREATE_COMMANDS);
				setCommandState(COMMAND_STATE.SELECT_COMMAND);
				Battle2DManager.instance().helpWindow().releaseHelpWindow();
				isCancel_ = false;
			}

			public COMMAND_STATE selectCommandStateEnemyOrPlayer(short target)
			{
				return target switch
				{
					0 => COMMAND_STATE.SELECT_ENEMY, 
					1 => COMMAND_STATE.SELECT_ENEMY, 
					2 => COMMAND_STATE.SELECT_PLAYER, 
					3 => COMMAND_STATE.SELECT_PLAYER, 
					4 => COMMAND_STATE.SELECT_PLAYER, 
					_ => COMMAND_STATE.ERR_COMMAND, 
				};
			}

			public void createTargetWindowEnemy(BattlePlayer player, BattleMonsterParty monsterParty)
			{
				int i = 0;
				int[] array = new int[6] { 4, 3, 5, 1, 0, 2 };
				int num = 0;
				for (int j = 0; j < 6; j++)
				{
					BattleMonster battleMonster = monsterParty.battleMonster(array[j]);
					if (battleMonster.isBattle())
					{
						num++;
					}
				}
				if (num < 4)
				{
					targetWindow_[i++].setShowTarget(show: false);
				}
				for (int k = 0; k < 6; k++)
				{
					BattleMonster battleMonster2 = monsterParty.battleMonster(array[k]);
					if (battleMonster2.isBattle())
					{
						int arg = battleMonster2.battleCharacterId();
						menu.TargetWindow targetWindow = targetWindow_[i];
						targetWindow.createTargetMessage(battleMonster2.monster().nameId());
						targetWindow.setShowTarget(show: true);
						targetWindow.targetId_set(arg);
						battleMonster2.targetWindow_set(targetWindow);
						i++;
					}
				}
				for (; i < 4; i++)
				{
					targetWindow_[i].setShowTarget(show: false);
				}
				if (monsterParty.aliveNumber() > 1 && (allSelectFlag_ & 4) != 0)
				{
					allWindow_.setShowTarget(show: true);
					allWindow_.createTargetMessage(81);
				}
				else
				{
					allWindow_.setShowTarget(show: false);
				}
				if ((allSelectFlag_ & 2) != 0)
				{
					bool flag = false;
					bool flag2 = false;
					for (int l = 0; l < 6; l++)
					{
						BattleMonster battleMonster3 = monsterParty.battleMonster(l);
						if (!battleMonster3.isBattle())
						{
							continue;
						}
						for (int m = 0; m < 6; m++)
						{
							BattleMonster battleMonster4 = monsterParty.battleMonster(m);
							if (l != m && battleMonster4.isBattle())
							{
								if (battleMonster3.monsterId() == battleMonster4.monsterId())
								{
									flag = true;
								}
								else
								{
									flag2 = true;
								}
							}
						}
					}
					if (flag && flag2)
					{
						groupWindow_.setShowTarget(show: true);
						groupWindow_.createTargetMessage(126);
					}
					else
					{
						groupWindow_.setShowTarget(show: false);
					}
				}
				else
				{
					groupWindow_.setShowTarget(show: false);
				}
			}

			public void createTargetWindowPlayer(BattlePlayer player, BattleParty playerParty)
			{
				int i = 0;
				int num = 0;
				for (int j = 0; j < 4; j++)
				{
					BattlePlayer battlePlayer = playerParty.battlePlayer(j);
					if (battlePlayer.isEnable())
					{
						num++;
					}
				}
				if (num < 4)
				{
					targetWindow_[i++].setShowTarget(show: false);
				}
				for (int k = 0; k < 4; k++)
				{
					BattlePlayer battlePlayer2 = playerParty.battlePlayer(k);
					if (!battlePlayer2.isEnable())
					{
						continue;
					}
					int arg = battlePlayer2.battleCharacterId();
					menu.TargetWindow targetWindow = targetWindow_[i];
					targetWindow.createTargetMessage(battlePlayer2.player().name());
					targetWindow.setShowTarget(show: true);
					targetWindow.targetId_set(arg);
					battlePlayer2.targetWindow_set(targetWindow);
					bool flag = true;
					if (player.isSelectDeadTarget())
					{
						if (battlePlayer2.condition().isStone())
						{
							flag = false;
						}
					}
					else if (player.isSelectStoneTarget())
					{
						if (battlePlayer2.condition().isDeath())
						{
							flag = false;
						}
					}
					else if (!battlePlayer2.isBattle())
					{
						flag = false;
					}
					if (battlePlayer2.flag(PLAYER_FLAG.PF_JUMP))
					{
						flag = false;
					}
					if (player.isDrain() && player.battleCharacterId() == battlePlayer2.battleCharacterId())
					{
						flag = false;
					}
					targetWindow.setMessageColor((!flag) ? 1 : 0);
					i++;
				}
				for (; i < 4; i++)
				{
					targetWindow_[i].setShowTarget(show: false);
				}
				if (playerParty.aliveNumber() > 1 && (allSelectFlag_ & 4) != 0)
				{
					allWindow_.setShowTarget(show: true);
					allWindow_.createTargetMessage(81);
				}
				else
				{
					allWindow_.setShowTarget(show: false);
				}
				groupWindow_.setShowTarget(show: false);
			}

			public void setTargetEnemy(BattlePlayer player, BattleMonsterParty monsterParty, int update, int notClear, short condition)
			{
				firstTargetPlayerId_ = -1;
				if (notClear == 0)
				{
					player.clearTargetId();
				}
				Battle2DManager.instance().cursor().active(1);
				Battle2DManager.instance().cursor().active(15);
				createTargetWindowEnemy(player, monsterParty);
				if (allSelectFlag_ != 4)
				{
					if (player.targetId(0) == -1)
					{
						sbyte b = monsterParty.isBattleMonsterFront();
						if (b == -1)
						{
							player.setTargetId(0, monsterParty.battleMonster(monsterParty.getTopBattleMonsterId()).battleCharacterId());
						}
						else
						{
							player.setTargetId(0, monsterParty.battleMonster(b).battleCharacterId());
						}
					}
					BattleMonster monster = monsterParty.getbattleCharacterIdMonster(player.targetId(0));
					Battle2DManager.instance().cursor().setPositionMonster(1, monster);
					Battle2DManager.instance().cursor().setPositionTargetMonster(15, monster);
				}
				else
				{
					selectTargetState_ = 4;
					Battle2DManager.instance().cursor().hidden(1);
					Battle2DManager.instance().cursor().setPositionTargetAll(15);
				}
				Battle2DManager.instance().helpWindow().releaseHelpWindow();
				player.setLastTargetId();
			}

			public void setTargetFriend(BattlePlayer player, BattleParty playerParty, itm.TARGET_POSITION myself, int update, int notClear, short condition)
			{
				firstTargetPlayerId_ = -1;
				if (notClear == 0)
				{
					player.clearTargetId();
				}
				Battle2DManager.instance().cursor().active(1);
				Battle2DManager.instance().cursor().active(15);
				createTargetWindowPlayer(player, playerParty);
				if (allSelectFlag_ != 4)
				{
					if (condition > 0)
					{
						setTargetBadConditionPlayer(player, playerParty, condition);
					}
					if (myself == itm.TARGET_POSITION.TARGET_POSITION_SELF)
					{
						player.clearTargetId();
						player.setTargetIdMyself();
					}
					if (player.targetId(0) == -1)
					{
						player.setTargetId(0, playerParty.battlePlayer(playerParty.getMinBattlePlayerId()).battleCharacterId());
					}
					BattlePlayer player2 = playerParty.getbattleCharacterIdPlayer(player.targetId(0));
					Battle2DManager.instance().cursor().setPositionPlayer(1, player2);
					Battle2DManager.instance().cursor().setPositionTargetPlayer(15, player2);
					firstTargetPlayerId_ = player.targetId(0);
				}
				else
				{
					selectTargetState_ = 4;
					Battle2DManager.instance().cursor().hidden(1);
					Battle2DManager.instance().cursor().setPositionTargetAll(15);
				}
				Battle2DManager.instance().helpWindow().releaseHelpWindow();
				player.setLastTargetId();
			}

			public void setTargetBadConditionPlayer(BattlePlayer player, BattleParty party, short condition)
			{
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer = party.battlePlayer(i);
					if (battlePlayer != null && battlePlayer.isEnable() && !battlePlayer.flag(PLAYER_FLAG.PF_JUMP) && battlePlayer.isCondition(condition))
					{
						player.setTargetId(0, battlePlayer.battleCharacterId());
						break;
					}
				}
			}

			public void setCommandState(COMMAND_STATE state)
			{
				if (commandState_ != COMMAND_STATE.SELECT_ENEMY && commandState_ != COMMAND_STATE.SELECT_PLAYER)
				{
					prevCommandState_ = commandState_;
				}
				commandState_ = state;
			}

			public bool selectEnemyUp(BattleSystem B)
			{
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayer_);
				BattleMonsterParty battleMonsterParty = B.characterManager().monsterParty();
				sbyte b = battleMonsterParty.getbattleCharacterIdBattleMonsterId(battlePlayer.targetId(0));
				if (b <= 2)
				{
					int num = EnemyBackBattleMonsterId[b];
					do
					{
						num--;
						if (0 > num)
						{
							battlePlayer.targetId_set(0, battleMonsterParty.battleMonster(b).battleCharacterId());
							return true;
						}
					}
					while (!battleMonsterParty.battleMonster(EnemyBackBattleMonsterId[num]).isBattle());
					battlePlayer.targetId_set(0, battleMonsterParty.battleMonster(EnemyBackBattleMonsterId[num]).battleCharacterId());
					return false;
				}
				if (b <= 5)
				{
					int num2 = EnemyFrontBattleMonsterId[b - 3];
					do
					{
						num2--;
						if (3 > num2)
						{
							battlePlayer.targetId_set(0, battleMonsterParty.battleMonster(b).battleCharacterId());
							return true;
						}
					}
					while (!battleMonsterParty.battleMonster(EnemyFrontBattleMonsterId[num2 - 3]).isBattle());
					battlePlayer.targetId_set(0, battleMonsterParty.battleMonster(EnemyFrontBattleMonsterId[num2 - 3]).battleCharacterId());
					return false;
				}
				return false;
			}

			public bool selectEnemyDown(BattleSystem B)
			{
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayer_);
				BattleMonsterParty battleMonsterParty = B.characterManager().monsterParty();
				sbyte b = battleMonsterParty.getbattleCharacterIdBattleMonsterId(battlePlayer.targetId(0));
				if (b <= 2)
				{
					int num = EnemyBackBattleMonsterId[b];
					do
					{
						num++;
						if (2 < num)
						{
							battlePlayer.targetId_set(0, battleMonsterParty.battleMonster(b).battleCharacterId());
							return true;
						}
					}
					while (!battleMonsterParty.battleMonster(EnemyBackBattleMonsterId[num]).isBattle());
					battlePlayer.targetId_set(0, battleMonsterParty.battleMonster(EnemyBackBattleMonsterId[num]).battleCharacterId());
					return false;
				}
				if (b <= 5)
				{
					int num2 = EnemyFrontBattleMonsterId[b - 3];
					do
					{
						num2++;
						if (5 < num2)
						{
							battlePlayer.targetId_set(0, battleMonsterParty.battleMonster(b).battleCharacterId());
							return true;
						}
					}
					while (!battleMonsterParty.battleMonster(EnemyFrontBattleMonsterId[num2 - 3]).isBattle());
					battlePlayer.targetId_set(0, battleMonsterParty.battleMonster(EnemyFrontBattleMonsterId[num2 - 3]).battleCharacterId());
					return false;
				}
				return false;
			}

			public bool selectEnemyLeft(BattleSystem B)
			{
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayer_);
				BattleMonsterParty battleMonsterParty = B.characterManager().monsterParty();
				sbyte b = battleMonsterParty.getbattleCharacterIdBattleMonsterId(battlePlayer.targetId(0));
				if (b <= 2)
				{
					return true;
				}
				if (b <= 5)
				{
					if (battleMonsterParty.isBattleMonsterBack() == -1)
					{
						return true;
					}
					int num = EnemyBackBattleMonsterId[b - 3];
					while (true)
					{
						if (0 > num)
						{
							num = 2;
						}
						if (2 < num)
						{
							num = 0;
						}
						if (battleMonsterParty.battleMonster(EnemyBackBattleMonsterId[num]).isBattle())
						{
							break;
						}
						num--;
					}
					battlePlayer.targetId_set(0, battleMonsterParty.battleMonster(EnemyBackBattleMonsterId[num]).battleCharacterId());
					return false;
				}
				return false;
			}

			public bool selectEnemyRight(BattleSystem B)
			{
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayer_);
				BattleMonsterParty battleMonsterParty = B.characterManager().monsterParty();
				sbyte b = battleMonsterParty.getbattleCharacterIdBattleMonsterId(battlePlayer.targetId(0));
				if (b <= 2)
				{
					if (battleMonsterParty.isBattleMonsterFront() == -1)
					{
						if (battlePlayer.isSetAbility())
						{
							return false;
						}
						if (B.characterManager().playerParty().isTargetDrain(battlePlayer))
						{
							return false;
						}
						return true;
					}
					if (battleMonsterParty.isBattleMonsterFront() == -1)
					{
						return true;
					}
					int num = EnemyFrontBattleMonsterId[b];
					while (true)
					{
						if (3 > num)
						{
							num = 5;
						}
						if (5 < num)
						{
							num = 3;
						}
						if (battleMonsterParty.battleMonster(EnemyFrontBattleMonsterId[num - 3]).isBattle())
						{
							break;
						}
						num++;
					}
					battlePlayer.targetId_set(0, battleMonsterParty.battleMonster(EnemyFrontBattleMonsterId[num - 3]).battleCharacterId());
					return false;
				}
				if (b <= 5)
				{
					if (battlePlayer.isSetAbility())
					{
						return false;
					}
					if (B.characterManager().playerParty().isTargetDrain(battlePlayer))
					{
						return false;
					}
					return true;
				}
				return false;
			}

			public void selectPlayerPad(BattleSystem B, int upDown)
			{
				BattleParty battleParty = B.characterManager().playerParty();
				BattlePlayer battlePlayer = battleParty.battlePlayer(nowPlayer_);
				sbyte b = (sbyte)battleParty.getbattleCharacterIdBattlePlayerId(battlePlayer.targetId(0));
				BattlePlayer battlePlayer2;
				while (true)
				{
					b = ((upDown != 0) ? ((sbyte)(b + 1)) : ((sbyte)(b - 1)));
					if (b < 0)
					{
						int num = 3;
						b = (sbyte)num;
					}
					if (b > 3)
					{
						b = 0;
					}
					battlePlayer2 = battleParty.battlePlayer(b);
					if (battlePlayer.isDrain())
					{
						if (battlePlayer.battleCharacterId() != battlePlayer2.battleCharacterId() && battlePlayer2.isBattle() && !battlePlayer2.flag(PLAYER_FLAG.PF_JUMP))
						{
							battlePlayer.setTargetId(0, battlePlayer2.battleCharacterId());
							return;
						}
					}
					else if (battlePlayer.isEsuna())
					{
						if (battlePlayer2.isEnable() && !battlePlayer2.condition().isDeath() && !battlePlayer2.flag(PLAYER_FLAG.PF_JUMP))
						{
							battlePlayer.setTargetId(0, battlePlayer2.battleCharacterId());
							return;
						}
					}
					else if (battlePlayer.isSelectDeadTarget())
					{
						if (battlePlayer2.isEnable() && !battlePlayer2.condition().isStone() && !battlePlayer2.flag(PLAYER_FLAG.PF_JUMP))
						{
							battlePlayer.setTargetId(0, battlePlayer2.battleCharacterId());
							return;
						}
					}
					else if (battlePlayer.isSelectStoneTarget())
					{
						if (battlePlayer2.isEnable() && !battlePlayer2.condition().isDeath() && !battlePlayer2.flag(PLAYER_FLAG.PF_JUMP))
						{
							battlePlayer.setTargetId(0, battlePlayer2.battleCharacterId());
							return;
						}
					}
					else if (battlePlayer2.isEnable() && !battlePlayer2.condition().isDeath() && !battlePlayer2.condition().isStone() && !battlePlayer2.flag(PLAYER_FLAG.PF_JUMP))
					{
						break;
					}
				}
				battlePlayer.setTargetId(0, battlePlayer2.battleCharacterId());
			}

			public bool commandAction(BattleSystem B)
			{
				if (battleWindow_.commandWindow().pushState() != 0)
				{
					return false;
				}
				cancelWindow_.setShowTarget(show: true);
				cancelWindow_.createTargetMessage(50021);
				BattlePlayer battlePlayer = B.characterManager().playerParty().battlePlayer(nowPlayer_);
				BattleMonsterParty monsterParty = B.characterManager().monsterParty();
				pl.Command command = battlePlayer.player().jobManager().command();
				bool result = false;
				switch (command.commandId(command.nowCommand()))
				{
				case 1:
					commandAttack(battlePlayer, monsterParty, 0);
					result = false;
					break;
				case 2:
					commandEscape(battlePlayer);
					result = true;
					break;
				case 3:
					commandGuard(battlePlayer);
					result = true;
					break;
				case 5:
				case 6:
				case 13:
				case 46:
					commandMagic(battlePlayer);
					result = false;
					break;
				case 4:
					commandItem(battlePlayer);
					result = false;
					break;
				case 7:
					commandSteal(battlePlayer, monsterParty);
					result = false;
					break;
				case 8:
					commandTakeAPowder(battlePlayer);
					result = true;
					break;
				case 9:
					commandBerserk(battlePlayer, monsterParty);
					result = true;
					break;
				case 11:
					commandKnockOver(battlePlayer, monsterParty);
					break;
				case 36:
					commandPoise(battlePlayer);
					result = true;
					break;
				case 21:
					commandCover(battlePlayer);
					result = true;
					break;
				case 26:
					commandCheck(battlePlayer, monsterParty);
					break;
				case 27:
					commandDetect(battlePlayer, monsterParty);
					break;
				case 29:
					commandJump(battlePlayer, monsterParty);
					break;
				case 30:
					commandDark(battlePlayer, B);
					result = true;
					break;
				case 35:
					commandRollUp(battlePlayer);
					result = true;
					break;
				case 45:
					commandProvocation(battlePlayer, monsterParty);
					result = false;
					break;
				case 25:
					commandOverissue(battlePlayer, monsterParty);
					result = true;
					break;
				case 40:
					commandPitch(battlePlayer);
					result = false;
					break;
				case 32:
					commandGeography(battlePlayer);
					result = true;
					break;
				case 18:
					commandSong(battlePlayer);
					result = true;
					break;
				case 43:
					result = commandCancel(battlePlayer, B.characterManager().playerParty());
					break;
				case 44:
					commandFormationChange(battlePlayer);
					result = true;
					break;
				case 47:
					commandEquip(battlePlayer);
					result = false;
					break;
				default:
				{
					// PORT: a command of the mods' own (OpenFF.Client.BattleCommands): the plain attack's
					// action, aimed as the command's Target says; its damage is the mod's at the swing.
					bool? own = OpenFF.Client.BattleCommands.Select(this, battlePlayer, B, command.commandId(command.nowCommand()));
					if (own.HasValue)
					{
						result = own.Value;
					}
					break;
				}
				}
				return result;
			}

			public void commandAttack(BattlePlayer player, BattleMonsterParty monsterParty, int notClear)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.nondisplay();
				Battle2DManager.instance().cursor().nondisplayAll();
				player.actionId_set(1);
				setTargetEnemy(player, monsterParty, 0, notClear, 0);
				setCommandState(COMMAND_STATE.SELECT_ENEMY);
			}

			public void commandAttack2(BattlePlayer player, BattleParty playerParty, int notClear)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.nondisplay();
				Battle2DManager.instance().cursor().nondisplayAll();
				player.actionId_set(1);
				setTargetFriend(player, playerParty, itm.TARGET_POSITION.TARGET_POSITION_FRIEND, 0, notClear, 0);
				setCommandState(COMMAND_STATE.SELECT_PLAYER);
			}

			public void commandGuard(BattlePlayer player)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.setOnOff(player);
				Battle2DManager.instance().cursor().nondisplayAll();
				player.actionId_set(3);
			}

			public void commandMagic(BattlePlayer player)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				Battle2DManager.instance().cursor().nondisplayAll();
				battleWindow_.nondisplay();
				setCommandState(COMMAND_STATE.SELECT_MAGIC);
			}

			public void commandItem(BattlePlayer player)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				Battle2DManager.instance().cursor().nondisplayAll();
				battleWindow_.nondisplay();
				setCommandState(COMMAND_STATE.SELECT_ITEM);
			}

			public void commandTakeAPowder(BattlePlayer player)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.setOnOff(player);
				Battle2DManager.instance().cursor().nondisplayAll();
				player.actionId_set(9);
			}

			public void commandSteal(BattlePlayer player, BattleMonsterParty monsterParty)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.nondisplay();
				Battle2DManager.instance().cursor().nondisplayAll();
				player.actionId_set(8);
				setTargetEnemy(player, monsterParty, 0, 0, 0);
				setCommandState(COMMAND_STATE.SELECT_ENEMY);
			}

			public void commandEscape(BattlePlayer player)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.setOnOff(player);
				Battle2DManager.instance().cursor().nondisplayAll();
				player.actionId_set(2);
			}

			public void commandBerserk(BattlePlayer player, BattleMonsterParty monsterParty)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.nondisplay();
				Battle2DManager.instance().cursor().nondisplayAll();
				player.actionId_set(10);
				setTargetEnemy(player, monsterParty, 0, 0, 0);
				Battle2DManager.instance().cursor().nondisplayAll();
			}

			public void commandKnockOver(BattlePlayer player, BattleMonsterParty monsterParty)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.nondisplay();
				Battle2DManager.instance().cursor().nondisplayAll();
				player.setActionId(11);
				setTargetEnemy(player, monsterParty, 0, 0, 0);
				setCommandState(COMMAND_STATE.SELECT_ENEMY);
			}

			public void commandPoise(BattlePlayer player)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.setOnOff(player);
				Battle2DManager.instance().cursor().nondisplayAll();
				player.setActionId(12);
			}

			public void commandCover(BattlePlayer player)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.setOnOff(player);
				Battle2DManager.instance().cursor().nondisplayAll();
				player.setActionId(13);
			}

			public void commandCheck(BattlePlayer player, BattleMonsterParty monsterParty)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.nondisplay();
				Battle2DManager.instance().cursor().nondisplayAll();
				player.setActionId(14);
				setTargetEnemy(player, monsterParty, 0, 0, 0);
				setCommandState(COMMAND_STATE.SELECT_ENEMY);
			}

			public void commandDetect(BattlePlayer player, BattleMonsterParty monsterParty)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.nondisplay();
				Battle2DManager.instance().cursor().nondisplayAll();
				player.setActionId(15);
				setTargetEnemy(player, monsterParty, 0, 0, 0);
				setCommandState(COMMAND_STATE.SELECT_ENEMY);
			}

			public void commandJump(BattlePlayer player, BattleMonsterParty monsterParty)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.nondisplay();
				Battle2DManager.instance().cursor().nondisplayAll();
				player.setActionId(16);
				setTargetEnemy(player, monsterParty, 0, 0, 0);
				setCommandState(COMMAND_STATE.SELECT_ENEMY);
			}

			public void commandDark(BattlePlayer player, BattleSystem B)
			{
				allSelectFlag_ = 4;
				selectTargetState_ = 4;
				battleWindow_.setOnOff(player);
				player.setActionId(18);
				B.characterManager().setMonsterAllTarget(player);
				Battle2DManager.instance().cursor().nondisplayAll();
			}

			public void commandRollUp(BattlePlayer player)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.setOnOff(player);
				Battle2DManager.instance().cursor().nondisplayAll();
				player.setActionId(21);
			}

			public void commandProvocation(BattlePlayer player, BattleMonsterParty monsterParty)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.nondisplay();
				Battle2DManager.instance().cursor().nondisplayAll();
				player.setActionId(23);
				setTargetEnemy(player, monsterParty, 0, 0, 0);
				setCommandState(COMMAND_STATE.SELECT_ENEMY);
			}

			public void commandOverissue(BattlePlayer player, BattleMonsterParty monsterParty)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.setOnOff(player);
				Battle2DManager.instance().cursor().nondisplayAll();
				player.setActionId(24);
			}

			public void commandPitch(BattlePlayer player)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				Battle2DManager.instance().cursor().nondisplayAll();
				battleWindow_.nondisplay();
				setCommandState(COMMAND_STATE.SELECT_ITEM);
			}

			public void commandGeography(BattlePlayer player)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.setOnOff(player);
				Battle2DManager.instance().cursor().nondisplayAll();
				player.setActionId(19);
			}

			public void commandSong(BattlePlayer player)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				battleWindow_.setOnOff(player);
				Battle2DManager.instance().cursor().nondisplayAll();
				player.setActionId(20);
			}

			public bool commandCancel(BattlePlayer player, BattleParty party)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				player.clearTargetId();
				if (commandState_ == COMMAND_STATE.SELECT_COMMAND)
				{
					if (nowPlayer_ != party.getMinBattlePlayerId())
					{
						battleWindow_.setOnOff(player);
						Battle2DManager.instance().cursor().nondisplayAll();
						isCancel_ = true;
						return true;
					}
					return false;
				}
				if (commandState_ == COMMAND_STATE.SELECT_ENEMY || commandState_ == COMMAND_STATE.SELECT_PLAYER)
				{
					Battle2DManager.instance().cursor().setShow(1, show: false);
					Battle2DManager.instance().cursor().setShow(15, show: false);
					Battle2DManager.instance().cursor().active(0);
					battleWindow_.setOff();
					setCommandState(COMMAND_STATE.SELECT_COMMAND);
					return false;
				}
				return false;
			}

			public void commandFormationChange(BattlePlayer player)
			{
				battleWindow_.setOnOff(player);
				Battle2DManager.instance().cursor().nondisplayAll();
				player.actionId_set(26);
			}

			public void commandEquip(BattlePlayer player)
			{
				allSelectFlag_ = 1;
				selectTargetState_ = 1;
				Battle2DManager.instance().cursor().nondisplayAll();
				battleWindow_.nondisplay();
				setCommandState(COMMAND_STATE.SELECT_ITEM);
			}

			public int selectCharacterTouchPanel(BattleSystem B)
			{
				BattleParty battleParty = B.characterManager().playerParty();
				BattlePlayer battlePlayer = battleParty.battlePlayer(nowPlayer_);
				BattleMonsterParty battleMonsterParty = B.characterManager().monsterParty();
				ds.g_TouchPanel.getPoint(out var x, out var y);
				VecFx32 pos = new VecFx32();
				VecFx32 vecFx = new VecFx32();
				vecFx.x = 0;
				vecFx.y = 0;
				vecFx.z = 0;
				for (int i = 0; i < 4; i++)
				{
					menu.TargetWindow targetWindow = targetWindow_[i];
					if (!targetWindow.isShowTarget() || !targetWindow.isTouch(x, y))
					{
						continue;
					}
					if (commandState_ == COMMAND_STATE.SELECT_ENEMY)
					{
						battlePlayer.targetId_set(0, (short)targetWindow.targetId());
						return 2;
					}
					if (commandState_ == COMMAND_STATE.SELECT_PLAYER)
					{
						if (targetWindow.getMessageColor() == 0)
						{
							battlePlayer.targetId_set(0, (short)targetWindow.targetId());
							return 1;
						}
						menu.MenuManager.getSingleton().playSEBeep();
						return 3;
					}
				}
				if (groupWindow_.isShowTarget() && groupWindow_.isTouch(x, y))
				{
					if (selectTargetState_ == 2)
					{
						battlePlayer.setLastTargetId();
						return 5;
					}
					for (int i = 0; i < 6; i++)
					{
						BattleMonster battleMonster = battleMonsterParty.battleMonster(i);
						if (!battleMonster.isBattle())
						{
							continue;
						}
						int j;
						for (j = i + 1; j < 6; j++)
						{
							BattleMonster battleMonster2 = battleMonsterParty.battleMonster(j);
							if (battleMonster2.isBattle() && battleMonster.monsterId() == battleMonster2.monsterId())
							{
								break;
							}
						}
						if (j < 6)
						{
							Battle2DManager.instance().cursor().nondisplayAll();
							Battle2DManager.instance().cursor().active(15);
							battlePlayer.clearTargetId();
							battlePlayer.targetId_set(0, battleMonster.battleCharacterId());
							selectTargetState_ = 2;
							B.characterManager().setMonsterGroupTarget(battlePlayer);
							Battle2DManager.instance().cursor().hidden(1);
							Battle2DManager.instance().cursor().setPositionTargetGroup(15);
							Battle2DManager.instance().cursor().setPositionMonsterGroup(battleMonsterParty, battleMonster.monsterId());
							break;
						}
					}
					menu.MenuManager.getSingleton().playSEMoveCursor();
					return 3;
				}
				if (allWindow_.isShowTarget() && allWindow_.isTouch(x, y))
				{
					if (commandState_ == COMMAND_STATE.SELECT_ENEMY)
					{
						if (selectTargetState_ == 4)
						{
							battlePlayer.setLastTargetId();
							return 5;
						}
						menu.MenuManager.getSingleton().playSEMoveCursor();
						selectTargetState_ = 4;
						B.characterManager().setMonsterAllTarget(battlePlayer);
						Battle2DManager.instance().cursor().hidden(1);
						Battle2DManager.instance().cursor().setPositionTargetAll(15);
						Battle2DManager.instance().cursor().setPositionMonsterAll(battleMonsterParty);
						return 3;
					}
					if (commandState_ == COMMAND_STATE.SELECT_PLAYER)
					{
						if (selectTargetState_ == 4)
						{
							battlePlayer.setLastTargetId();
							return 5;
						}
						menu.MenuManager.getSingleton().playSEMoveCursor();
						selectTargetState_ = 4;
						B.characterManager().setPlayerAllTarget(battlePlayer, battlePlayer.isSelectDeadOrStoneTargetCommand() ? 1 : 0);
						Battle2DManager.instance().cursor().hidden(1);
						Battle2DManager.instance().cursor().setPositionTargetAll(15);
						Battle2DManager.instance().cursor().setPositionPlayerAll(battleParty, battlePlayer.isSelectDeadOrStoneTargetCommand() ? 1 : 0);
						return 3;
					}
				}
				if (changeWindow_.isShowTarget() && changeWindow_.isTouch(x, y))
				{
					if (commandState_ == COMMAND_STATE.SELECT_ENEMY)
					{
						if (selectTargetState_ != 1)
						{
							selectTargetState_ = 1;
							Battle2DManager.instance().cursor().nondisplayAll();
							Battle2DManager.instance().cursor().active(1);
							Battle2DManager.instance().cursor().active(15);
							battlePlayer.clearTargetId();
						}
						menu.MenuManager.getSingleton().playSEMoveCursor();
						battlePlayer.clearTargetId();
						setCommandState(COMMAND_STATE.SELECT_PLAYER);
						return 3;
					}
					if (commandState_ == COMMAND_STATE.SELECT_PLAYER)
					{
						if (selectTargetState_ != 1)
						{
							selectTargetState_ = 1;
							Battle2DManager.instance().cursor().nondisplayAll();
							Battle2DManager.instance().cursor().active(1);
							Battle2DManager.instance().cursor().active(15);
							battlePlayer.clearTargetId();
						}
						menu.MenuManager.getSingleton().playSEMoveCursor();
						battlePlayer.clearTargetId();
						setCommandState(COMMAND_STATE.SELECT_ENEMY);
						return 3;
					}
				}
				if (cancelWindow_.isShowTarget() && cancelWindow_.isTouch(x, y))
				{
					return 4;
				}
				for (int i = 0; i < 6; i++)
				{
					BattleMonster battleMonster3 = battleMonsterParty.battleMonster(i);
					if (battleMonster3.isBattle())
					{
						characterMng.getPosition(battleMonster3.characterMngId(), pos);
						vecFx = mon.MonsterManager.instance().offset(battleMonster3.monsterId()).touchPosition();
						int num = mon.MonsterManager.instance().offset(battleMonster3.monsterId()).touchRadius();
						if (calcTouchPanel(x, y, pos, vecFx, num * 2))
						{
							battlePlayer.targetId_set(0, battleMonster3.battleCharacterId());
							return 2;
						}
					}
				}
				vecFx.x = 0;
				vecFx.y = 0;
				vecFx.z = 0;
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer2 = battleParty.battlePlayer(i);
					if (!battlePlayer2.isEnable())
					{
						continue;
					}
					if (battlePlayer.isSelectDeadTarget())
					{
						if (battlePlayer2.condition().isStone())
						{
							continue;
						}
					}
					else if (battlePlayer.isSelectStoneTarget())
					{
						if (battlePlayer2.condition().isDeath())
						{
							continue;
						}
					}
					else if (!battlePlayer2.isBattle())
					{
						continue;
					}
					if (!battlePlayer2.flag(PLAYER_FLAG.PF_JUMP))
					{
						characterMng.getPosition(battlePlayer2.characterMngId(), pos);
						vecFx.y = 20480;
						int num = 49152;
						if (calcTouchPanel(x, y, pos, vecFx, num * 2))
						{
							battlePlayer.targetId_set(0, battlePlayer2.battleCharacterId());
							return 1;
						}
					}
				}
				return 0;
			}

			public bool calcTouchPanel(int x, int y, VecFx32 pos, VecFx32 offset, int radius)
			{
				VecFx32 vecFx = new VecFx32(pos);
				vecFx.x += offset.x;
				vecFx.y += offset.y;
				vecFx.z += offset.z;
				NNS_G3dWorldPosToScrPos(vecFx, out var px, out var py);
				int num = FX_Sqrt(((px - x) * (px - x) + (py - y) * (py - y)) * 4096);
				if (num < radius)
				{
					return true;
				}
				return false;
			}

			public void clearTouchTargetId()
			{
				for (int i = 0; i < 12; i++)
				{
					touchTargetId_[i] = -1;
				}
			}

			public void clearTouchStartPosition()
			{
				touchStartX_ = 0;
				touchStartY_ = 0;
			}

			public int setTargetTouch(BattleSystem B, BattlePlayer player)
			{
				int num = -1;
				for (int i = 0; i < 12; i++)
				{
					if (touchTargetId_[i] != -1)
					{
						BaseBattleCharacter baseBattleCharacterFromBreed = B.characterManager().getBaseBattleCharacterFromBreed((short)touchTargetId_[i]);
						if (baseBattleCharacterFromBreed == null || !baseBattleCharacterFromBreed.isBattle())
						{
							touchTargetId_[i] = -1;
						}
						else
						{
							num++;
						}
					}
				}
				if (num < 0)
				{
					return 0;
				}
				if (num == 0)
				{
					for (int i = 0; i < 12; i++)
					{
						if (touchTargetId_[i] != -1)
						{
							BaseBattleCharacter baseBattleCharacterFromBreed2 = B.characterManager().getBaseBattleCharacterFromBreed((short)touchTargetId_[i]);
							if (baseBattleCharacterFromBreed2 != null && baseBattleCharacterFromBreed2.isBattle())
							{
								player.setTargetId(0, baseBattleCharacterFromBreed2.battleCharacterId());
								return 1;
							}
						}
					}
				}
				if (allSelectFlag_ == 1 && num > 0)
				{
					return 0;
				}
				bool flag = false;
				int num2 = 0;
				for (int i = 0; i < 12; i++)
				{
					if (touchTargetId_[i] != -1)
					{
						BaseBattleCharacter baseBattleCharacterFromBreed3 = B.characterManager().getBaseBattleCharacterFromBreed((short)touchTargetId_[i]);
						if (baseBattleCharacterFromBreed3 != null && baseBattleCharacterFromBreed3.isBattle() && baseBattleCharacterFromBreed3.breed() == 0)
						{
							flag = true;
							num2++;
						}
					}
				}
				if (flag)
				{
					if (allSelectFlag_ == 4)
					{
						B.characterManager().setPlayerAllTarget(player, player.isSelectDeadOrStoneTargetCommand() ? 1 : 0);
						selectTargetState_ = 4;
						return 4;
					}
					if (num2 != 1)
					{
						B.characterManager().setPlayerAllTarget(player, player.isSelectDeadOrStoneTargetCommand() ? 1 : 0);
						selectTargetState_ = 4;
						return 4;
					}
					for (int i = 0; i < 12; i++)
					{
						if (touchTargetId_[i] != -1)
						{
							BaseBattleCharacter baseBattleCharacterFromBreed4 = B.characterManager().getBaseBattleCharacterFromBreed((short)touchTargetId_[i]);
							if (baseBattleCharacterFromBreed4 != null && baseBattleCharacterFromBreed4.isBattle() && baseBattleCharacterFromBreed4.breed() == 0)
							{
								player.setTargetId(0, baseBattleCharacterFromBreed4.battleCharacterId());
								selectTargetState_ = 1;
								return 1;
							}
						}
					}
				}
				B.characterManager().setMonsterAllTarget(player);
				selectTargetState_ = 4;
				return 4;
			}

			public void setAllTargetFlag(short all)
			{
				allSelectFlag_ = 0;
				if ((all & 2) != 0)
				{
					allSelectFlag_ |= 1;
				}
				if ((all & 4) != 0)
				{
					allSelectFlag_ |= 2;
				}
				if ((all & 8) != 0)
				{
					allSelectFlag_ |= 4;
				}
				if ((all & 0x80) != 0)
				{
					allSelectFlag_ |= 1;
				}
				if ((all & 0x100) != 0)
				{
					allSelectFlag_ |= 2;
				}
				if ((all & 0x200) != 0)
				{
					allSelectFlag_ |= 4;
				}
				if ((all & 8) == 0 && (all & 0x200) != 0)
				{
					allSelectFlag_ &= -3;
					allSelectFlag_ &= -5;
				}
			}

			public bool isUseMagic(int magicId, BattlePlayer player)
			{
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter((short)magicId);
				if (magicParameter == null)
				{
					return false;
				}
				if (magicParameter.system() != 3)
				{
					if (player.condition().isSilence())
					{
						return false;
					}
					if (player.condition().isFrog() && magicId != 4005)
					{
						return false;
					}
				}
				if (evt.CEventRestriction.getSingleton().check(magicId))
				{
					return false;
				}
				return player.player().isUseMagic(magicId, 1);
			}

			public bool isUseItem(int itemId, BattlePlayer player)
			{
				itm.WeaponParameter weaponParameter = itm.ItemManager.instance().weaponParameter((short)itemId);
				if (weaponParameter != null)
				{
					if (!player.player().isEquipItem(weaponParameter.equipJob()))
					{
						return false;
					}
					if (weaponParameter.useItemId() <= 0)
					{
						return false;
					}
				}
				itm.ItemUse itemUse = new itm.ItemUse();
				return itemUse.isUseInBattle(itemId);
			}

			public BattleSetupPlayer()
			{
				battleWindow_ = null;
				playerWindow_ = null;
				playerState_ = new PlayerState[8]
				{
					new PlayerState(playerStatePoiseToIdle),
					new PlayerState(playerStateMoveFrontReady),
					new PlayerState(playerStateMoveFront),
					new PlayerState(playerStateCreateCommand),
					new PlayerState(playerStateIsOpendCommand),
					new PlayerState(playerStateSelectCommand),
					new PlayerState(playerStateMoveBack),
					new PlayerState(playerStatePoiseStart)
				};
				CommandState_ = new CommandState[6]
				{
					new CommandState(selectCommand),
					new CommandState(selectEnemy),
					new CommandState(selectPlayer),
					new CommandState(selectMagic),
					new CommandState(selectItem),
					new CommandState(selectSong)
				};
			}

			private void state(PLAYER_STATE state)
			{
				state_ = state;
			}
		}
	}
}
