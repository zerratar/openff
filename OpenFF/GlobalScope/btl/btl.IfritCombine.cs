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
		public class IfritCombine : BaseSummon
		{
			public static IfritCombine instance_ = new IfritCombine();

			public override void start(TurnSystem T)
			{
				currentIndex_ = 0;
				currentBehavior_ = IfritCombineCommand[currentIndex_].next_;
			}

			public override void end(TurnSystem T)
			{
			}

			public override bool run(TurnSystem T)
			{
				while (SummonCommand.getSingleton().execute(IfritCombineCommand[currentIndex_], (int)currentBehavior_))
				{
					if (currentBehavior_ == SUMMON_BEHAVIOR.SUMMON_BEHAVIOR_END)
					{
						return true;
					}
					currentIndex_++;
					currentBehavior_ = IfritCombineCommand[currentIndex_].next_;
					if (IfritCombineCommand[currentIndex_].again_ == 0)
					{
						break;
					}
				}
				return false;
			}

			public static IfritCombine getSingleton()
			{
				return instance_;
			}
		}
	}
}
