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
	public static partial class btl
	{
		public class BattleCommandWindow
		{
			public const int NOT_TOUCH = -1;

			private menu.CommandWindow commandWindow_ = new menu.CommandWindow();

			private bool isCreated_;

			public bool create(BattlePlayer player)
			{
				pl.Command command = player.player().jobManager().command();
				bool flag = false;
				if (!isCreated())
				{
					Battle2DManager.instance().triangle().create();
					commandWindow_.Initialize();
					commandWindow_.createCommandWindow();
					for (int i = 0; i < pl.BATTLE_COMMAND_MAX; i++)
					{
						int i2 = ((i == pl.BATTLE_COMMAND_MAX - 1) ? 6 : i);
						commandWindow_.createCommandMessage(abilityID(player, command.commandId(i2)), i);
						if (player.condition().isFrog())
						{
							if (command.isSelectCommandFrog(i2))
							{
								commandWindow_.setMessageColor(i, 0);
							}
							else
							{
								commandWindow_.setMessageColor(i, 1);
							}
						}
						else if (command.commandId(i2) == 18 && !player.player().equipParameter().isEquipHarp())
						{
							commandWindow_.setMessageColor(i, 1);
						}
					}
					flag = true;
					onIsCreated();
				}
				else
				{
					Battle2DManager.instance().triangle().create();
					commandWindow_.setShowCommand(show: true);
					for (int j = 0; j < pl.BATTLE_COMMAND_MAX; j++)
					{
						int i3 = ((j == pl.BATTLE_COMMAND_MAX - 1) ? 6 : j);
						commandWindow_.createCommandMessage(abilityID(player, command.commandId(i3)), j);
						if (player.condition().isFrog())
						{
							if (command.isSelectCommandFrog(i3))
							{
								commandWindow_.setMessageColor(j, 0);
							}
							else
							{
								commandWindow_.setMessageColor(j, 1);
							}
						}
						else if (command.commandId(i3) == 18 && !player.player().equipParameter().isEquipHarp())
						{
							commandWindow_.setMessageColor(j, 1);
						}
					}
					flag = true;
				}
				return flag;
			}

			public void release()
			{
				Battle2DManager.instance().triangle().showAll(flag: false);
				commandWindow_.Release();
				commandWindow_.releaseCommandMessageAll();
			}

			public void display()
			{
				Battle2DManager.instance().triangle().showAll(flag: true);
				commandWindow_.setShowCommand(show: true);
			}

			public void nondisplay()
			{
				Battle2DManager.instance().triangle().showAll(flag: false);
				commandWindow_.setShowCommand(show: false);
			}

			public void setOnOff(BattlePlayer player)
			{
			}

			public void setOff()
			{
				commandWindow_.setShowCommand(show: true);
			}

			public bool isOpened()
			{
				bool result = false;
				if (!isCreated())
				{
					if (commandWindow_.GetState() != sys2d.Window.WINDOW_STATE.wsOPENED)
					{
						return result;
					}
				}
				else if (!commandWindow_.isShowCommand())
				{
					return result;
				}
				return true;
			}

			public bool isClosed()
			{
				bool result = false;
				if (!isCreated())
				{
					if (commandWindow_.GetState() != sys2d.Window.WINDOW_STATE.wsCLOSED)
					{
						return result;
					}
				}
				else if (commandWindow_.isShowCommand())
				{
					return result;
				}
				return true;
			}

			public void createCommandMessagePadUp(BattlePlayer player, bool edge)
			{
				pl.Command command = player.player().jobManager().command();
				if (command.nowCommand() < 0)
				{
					if (edge)
					{
						pl.PlayerParty.instance().setSaveStartCommand(player.playerId(), pl.COMMAND_MAX - pl.BATTLE_COMMAND_MAX);
						updateCommandMessage(player);
					}
				}
				else
				{
					pl.PlayerParty.instance().saveStartCommand_dec(player.playerId());
					updateCommandMessage(player);
				}
			}

			public void createCommandMessagePadDown(BattlePlayer player, bool edge)
			{
				pl.Command command = player.player().jobManager().command();
				if (command.nowCommand() > pl.COMMAND_MAX - 1)
				{
					if (edge)
					{
						pl.PlayerParty.instance().setSaveStartCommand(player.playerId(), 0);
						updateCommandMessage(player);
					}
				}
				else
				{
					pl.PlayerParty.instance().saveStartCommand_inc(player.playerId());
					updateCommandMessage(player);
				}
			}

			public void updateCommandMessage(BattlePlayer player)
			{
				pl.Command command = player.player().jobManager().command();
				commandWindow_.releaseCommandMessageAll();
				int num = pl.PlayerParty.instance().saveStartCommand(player.playerId());
				for (int i = 0; i < pl.BATTLE_COMMAND_MAX; i++)
				{
					int i2 = ((i == pl.BATTLE_COMMAND_MAX - 1) ? 6 : (i + num));
					commandWindow_.createCommandMessage(abilityID(player, command.commandId(i2)), i);
					if (player.condition().isFrog())
					{
						if (!command.isSelectCommandFrog(i2))
						{
							commandWindow_.setMessageColor(i, 1);
						}
						else
						{
							commandWindow_.setMessageColor(i, 0);
						}
					}
					else if (command.commandId(i2) == 18 && !player.player().equipParameter().isEquipHarp())
					{
						commandWindow_.setMessageColor(i, 1);
					}
				}
			}

			public pl.ABILITY_ID abilityID(BattlePlayer player, sbyte temp_id)
			{
				pl.ABILITY_ID aBILITY_ID = (pl.ABILITY_ID)temp_id;
				if (aBILITY_ID == pl.ABILITY_ID.ABILITY_CHANGE)
				{
					aBILITY_ID = ((player.player().formationType() != 0) ? pl.ABILITY_ID.ABILITY_FRONT : pl.ABILITY_ID.ABILITY_BACK);
				}
				return aBILITY_ID;
			}

			public VecFx32 isSelectTouchPanel(int x, int y, BattlePlayer player)
			{
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
				fnd_reuse_pos.x = -1;
				fnd_reuse_pos.y = 0;
				fnd_reuse_pos.z = 0;
				if (!commandWindow_.isShowCommand())
				{
					return fnd_reuse_pos;
				}
				int num = pl.PlayerParty.instance().saveStartCommand(player.playerId());
				for (int i = 0; i < pl.BATTLE_COMMAND_MAX; i++)
				{
					int x2 = ((i == pl.BATTLE_COMMAND_MAX - 1) ? 6 : (i + num));
					ds.Vector2<short> position = commandWindow_.commandWindowData(i).Position;
					ds.Vector2<short> size = commandWindow_.commandWindowData(i).Size;
					if (position.vx <= x && position.vx + size.vx >= x && position.vy <= y && position.vy + size.vy >= y)
					{
						fnd_reuse_pos.x = x2;
						fnd_reuse_pos.y = i;
						fnd_reuse_pos.z = 0;
						return fnd_reuse_pos;
					}
				}
				return fnd_reuse_pos;
			}

			public bool isCreated()
			{
				return isCreated_;
			}

			public void onIsCreated()
			{
				isCreated_ = true;
			}

			public void offIsCreated()
			{
				isCreated_ = false;
			}

			public menu.CommandWindow commandWindow()
			{
				return commandWindow_;
			}
		}
	}
}
