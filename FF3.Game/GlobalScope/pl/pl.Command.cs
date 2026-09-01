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
	public static partial class pl
	{
		public class Command
		{
			private sbyte nowCommand_;

			private sbyte prevCommand_;

			private sbyte[] commandId_ = new sbyte[COMMAND_MAX];

			public void initialize()
			{
				commandId_[0] = 1;
				commandId_[1] = 3;
				commandId_[2] = 5;
				commandId_[3] = 4;
				commandId_[4] = 47;
				commandId_[5] = 44;
				commandId_[6] = 2;
				nowCommand_ = 0;
				prevCommand_ = 0;
			}

			public void initializeAfterLoad()
			{
				nowCommand_ = 0;
				prevCommand_ = 0;
			}

			public void loopNowCommand()
			{
				if (nowCommand() < 0)
				{
					setNowCommand(0);
				}
				if (nowCommand() > COMMAND_MAX - 1)
				{
					setNowCommand((sbyte)(COMMAND_MAX - 1));
				}
			}

			public bool isSelectCommandFrog(int i)
			{
				if (commandId(i) == 1 || commandId(i) == 2 || commandId(i) == 3 || commandId(i) == 4 || commandId(i) == 5 || commandId(i) == 6 || commandId(i) == 13 || commandId(i) == 44 || commandId(i) == 46)
				{
					return true;
				}
				return false;
			}

			public void setNowCommand(sbyte command)
			{
				nowCommand_ = command;
			}

			public sbyte nowCommand()
			{
				return nowCommand_;
			}

			public void nowCommand_set(sbyte arg0)
			{
				nowCommand_ = arg0;
			}

			public void clearNowCommand()
			{
				nowCommand_ = 0;
			}

			public void incrementNowCommand()
			{
				nowCommand_++;
			}

			public void decrementNowCommand()
			{
				nowCommand_--;
			}

			public void setPrevCommand()
			{
				prevCommand_ = nowCommand_;
			}

			public sbyte prevCommand()
			{
				return prevCommand_;
			}

			public void setCommandId(int i, sbyte command)
			{
				commandId_[i] = command;
			}

			public sbyte commandId(int i)
			{
				return commandId_[i];
			}

			public void setDefault()
			{
				nowCommand_ = 0;
				prevCommand_ = 0;
				memset(commandId_, 0, COMMAND_MAX);
			}

			public void copy(Command src)
			{
				nowCommand_ = src.nowCommand_;
				prevCommand_ = src.prevCommand_;
				memcpy(commandId_, src.commandId_, COMMAND_MAX);
			}

			public void parse(ArrayReader reader)
			{
				nowCommand_ = reader.readSByte();
				prevCommand_ = reader.readSByte();
				reader.read(commandId_, 0, COMMAND_MAX);
			}

			public void store(ArrayWriter writer)
			{
				writer.writeSByte(nowCommand_);
				writer.writeSByte(prevCommand_);
				writer.write(commandId_, 0, COMMAND_MAX);
			}
		}
	}
}
