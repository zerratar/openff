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
		public class SummonManager
		{
			public enum SUMMON_TYPE
			{
				SUMMON_WHITE,
				SUMMON_BLACK,
				SUMMON_COMBINE,
				SUMMON_TYPE_MAX
			}

			public const SUMMON_TYPE SUMMON_WHITE = SUMMON_TYPE.SUMMON_WHITE;

			public const SUMMON_TYPE SUMMON_BLACK = SUMMON_TYPE.SUMMON_BLACK;

			public const SUMMON_TYPE SUMMON_COMBINE = SUMMON_TYPE.SUMMON_COMBINE;

			public const SUMMON_TYPE SUMMON_TYPE_MAX = SUMMON_TYPE.SUMMON_TYPE_MAX;

			public static BaseSummon[][] summonTable_ = new BaseSummon[8][]
			{
				new BaseSummon[3]
				{
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton()
				},
				new BaseSummon[3]
				{
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton()
				},
				new BaseSummon[3]
				{
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton()
				},
				new BaseSummon[3]
				{
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton()
				},
				new BaseSummon[3]
				{
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton()
				},
				new BaseSummon[3]
				{
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton()
				},
				new BaseSummon[3]
				{
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton()
				},
				new BaseSummon[3]
				{
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton(),
					IfritCombine.getSingleton()
				}
			};

			private SUMMON_TYPE summonType_;

			private int summonLevel_;

			private BaseSummon currentSummon_ = new BaseSummon();

			public void initialize(TurnSystem T)
			{
				CommandParameter[] pCmdParam = T.summonDataManager().commandParameter(summonLevel(), summonType());
				SummonCommand.getSingleton().setTurnSystem(T);
				SummonCommand.getSingleton().initializeCommand();
				currentSummon_.setup(pCmdParam);
				currentSummon_.start(T);
			}

			public void terminate(TurnSystem T)
			{
				currentSummon_.end(T);
			}

			public bool execute(TurnSystem T)
			{
				SummonCommand.getSingleton().executeAutoCamera();
				return currentSummon_.run(T);
			}

			public void setSummonType(SUMMON_TYPE type)
			{
				summonType_ = type;
			}

			public int summonType()
			{
				return (int)summonType_;
			}

			public void setSummonLevel(int level)
			{
				summonLevel_ = level;
			}

			public int summonLevel()
			{
				return summonLevel_;
			}
		}
	}
}
