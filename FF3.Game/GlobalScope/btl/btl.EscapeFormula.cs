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
		public class EscapeFormula
		{
			public bool calcEscapePlayer(BattleParty party, BattleMonsterParty monsterParty)
			{
				if (OutsideToBattle.getInstance().battleType() != BATTLE_TYPE.NORMAL_BATTLE)
				{
					return false;
				}
				if (party.getMinLevel() - monsterParty.getMaxLevel() > 0)
				{
					return true;
				}
				byte b = (byte)ds.RandomNumber.rand32(101u);
				if (b < party.escapeActionNumber() * 30)
				{
					return true;
				}
				party.addEscapeActionNumber();
				return false;
			}

			public bool calcEscapeMonster(BattleParty party, BattleMonsterParty monsterParty)
			{
				return false;
			}

			public bool calcTakeAPowder(BattlePlayer player, BattleParty party, BattleMonsterParty monsterParty)
			{
				if (OutsideToBattle.getInstance().battleType() != BATTLE_TYPE.NORMAL_BATTLE)
				{
					return false;
				}
				int num = player.player().level().get() + player.player().jobManager().nowJobParameter()
					.skill()
					.skillLevel()
					.get() / 16;
				num *= (int)((ds.RandomNumber.rand32(41u) + 80) / 100);
				int maxLevel = monsterParty.getMaxLevel();
				maxLevel *= (int)((ds.RandomNumber.rand32(41u) + 80) / 100);
				if (num - maxLevel > 0)
				{
					return true;
				}
				byte b = (byte)ds.RandomNumber.rand32(101u);
				if (b < party.escapeActionNumber() * 60)
				{
					return true;
				}
				party.addEscapeActionNumber();
				return false;
			}
		}
	}
}
