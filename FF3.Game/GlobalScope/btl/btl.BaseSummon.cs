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
	public static partial class btl
	{
		public class BaseSummon
		{
			protected int currentIndex_;

			protected SUMMON_BEHAVIOR currentBehavior_;

			protected CommandParameter[] pCommandParameter_;

			public void setup(CommandParameter[] pCmdParam)
			{
				pCommandParameter_ = pCmdParam;
			}

			public virtual void start(TurnSystem T)
			{
				currentIndex_ = 0;
				currentBehavior_ = pCommandParameter_[currentIndex_].next_;
			}

			public virtual void end(TurnSystem T)
			{
			}

			public virtual bool run(TurnSystem T)
			{
				while (SummonCommand.getSingleton().execute(pCommandParameter_[currentIndex_], (int)currentBehavior_))
				{
					if (currentBehavior_ == SUMMON_BEHAVIOR.SUMMON_BEHAVIOR_END)
					{
						return true;
					}
					currentIndex_++;
					currentBehavior_ = pCommandParameter_[currentIndex_].next_;
					if (pCommandParameter_[currentIndex_].again_ == 0)
					{
						break;
					}
				}
				return false;
			}

			public BaseSummon()
			{
				pCommandParameter_ = null;
				currentIndex_ = 0;
				currentBehavior_ = SUMMON_BEHAVIOR.SUMMON_BEHAVIOR_END;
			}
		}
	}
}
