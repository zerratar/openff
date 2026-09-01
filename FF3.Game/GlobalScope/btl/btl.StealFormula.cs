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
		public class StealFormula
		{
			public enum STEAL_ITEM_LEVEL
			{
				STEAL_ITEM_LEVEL_1 = 0,
				STEAL_ITEM_LEVEL_2 = 1,
				STEAL_ITEM_LEVEL_3 = 2,
				STEAL_ITEM_LEVEL_4 = 3,
				STEAL_ITEM_LEVEL_MAX = 4,
				NO_STEAL_ITEM = -1,
				NOT_HAVE_ITEM = -2
			}

			public enum STEAL_MAX_ODDS
			{
				LEVEL_1_ODDS = 19,
				LEVEL_2_ODDS = 17,
				LEVEL_3_ODDS = 15,
				LEVEL_4_ODDS = 12
			}

			public const STEAL_ITEM_LEVEL STEAL_ITEM_LEVEL_1 = STEAL_ITEM_LEVEL.STEAL_ITEM_LEVEL_1;

			public const STEAL_ITEM_LEVEL STEAL_ITEM_LEVEL_2 = STEAL_ITEM_LEVEL.STEAL_ITEM_LEVEL_2;

			public const STEAL_ITEM_LEVEL STEAL_ITEM_LEVEL_3 = STEAL_ITEM_LEVEL.STEAL_ITEM_LEVEL_3;

			public const STEAL_ITEM_LEVEL STEAL_ITEM_LEVEL_4 = STEAL_ITEM_LEVEL.STEAL_ITEM_LEVEL_4;

			public const STEAL_ITEM_LEVEL STEAL_ITEM_LEVEL_MAX = STEAL_ITEM_LEVEL.STEAL_ITEM_LEVEL_MAX;

			public const STEAL_ITEM_LEVEL NO_STEAL_ITEM = STEAL_ITEM_LEVEL.NO_STEAL_ITEM;

			public const STEAL_ITEM_LEVEL NOT_HAVE_ITEM = STEAL_ITEM_LEVEL.NOT_HAVE_ITEM;

			public const STEAL_MAX_ODDS LEVEL_1_ODDS = STEAL_MAX_ODDS.LEVEL_1_ODDS;

			public const STEAL_MAX_ODDS LEVEL_2_ODDS = STEAL_MAX_ODDS.LEVEL_2_ODDS;

			public const STEAL_MAX_ODDS LEVEL_3_ODDS = STEAL_MAX_ODDS.LEVEL_3_ODDS;

			public const STEAL_MAX_ODDS LEVEL_4_ODDS = STEAL_MAX_ODDS.LEVEL_4_ODDS;

			public int calcSteal(BattlePlayer player, BattleMonster monster)
			{
				int id = monster.monster().droppingParameter().droppingItemTableId();
				if (mon.MonsterManager.instance().dropItem(id) == null)
				{
					OS_Printf("こいつはアイテムを持ってません！！\n");
					return -2;
				}
				int num = player.player().jobManager().nowJobParameter()
					.skill()
					.skillLevel()
					.get();
				OS_Printf("シーフの熟練度は %d です\n", num);
				int num2 = itemLevel(num);
				OS_Printf("盗み出すアイテムの最大レベルは %d です\n", num2 + 1);
				num2 = calcStealItem(num, num2);
				if (num2 == -1)
				{
					return -1;
				}
				return mon.MonsterManager.instance().dropItem(id).itemId(num2);
			}

			public int itemLevel(int jobSkill)
			{
				int result = 0;
				if (31 <= jobSkill && jobSkill <= 70)
				{
					result = 1;
				}
				else if (71 <= jobSkill && jobSkill <= 98)
				{
					result = 2;
				}
				else if (99 == jobSkill)
				{
					result = 3;
				}
				return result;
			}

			public int stealAddOdds(int itemLevel)
			{
				int result = 0;
				switch (itemLevel)
				{
				case 0:
					result = 19;
					break;
				case 1:
					result = 17;
					break;
				case 2:
					result = 15;
					break;
				case 3:
					result = 12;
					break;
				}
				return result;
			}

			public int calcStealItem(int jobSkill, int itemLevel)
			{
				int num = (int)ds.RandomNumber.rand32(101u);
				OS_Printf("盗み出す確率は %d です\n", num);
				for (int num2 = itemLevel; num2 > -1; num2--)
				{
					int num3 = 30 + stealAddOdds(num2) + jobSkill / 5;
					OS_Printf("盗み出す最大確率は %d です\n", num3);
					if (num <= num3)
					{
						return num2;
					}
				}
				return -1;
			}
		}
	}
}
