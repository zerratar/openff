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
	public class LogicContext : Logic
	{
		public int nextStatus()
		{
			switch (status_)
			{
			case STATUS_TYPE.CONSTRUCTOR:
				setExecute();
				break;
			case STATUS_TYPE.EXECUTE:
				setDisable();
				break;
			case STATUS_TYPE.DESTRUCTOR:
				status_ = STATUS_TYPE.DISABLE;
				break;
			}
			if (status_ == STATUS_TYPE.DISABLE)
			{
				return 0;
			}
			return 1;
		}

		public void load(out ScriptData scriptData, out uint pc)
		{
			scriptData = scriptData_;
			pc = pc_;
		}

		public void save(ScriptData scriptData, uint pc)
		{
			scriptData_ = scriptData;
			pc_ = pc;
		}

		public void push(ScriptData scriptData, uint pc)
		{
			scriptDataStack_[sp_] = scriptData;
			pcStack_[sp_] = pc;
			sp_++;
		}

		public void pop()
		{
			sp_--;
			scriptData_ = scriptDataStack_[sp_];
			pc_ = pcStack_[sp_];
		}

		public int isWaiting()
		{
			if (wait_ == 0)
			{
				return 0;
			}
			wait_--;
			return 1;
		}

		public void setWait(uint wait)
		{
			wait_ = wait - 1;
		}
	}
}
