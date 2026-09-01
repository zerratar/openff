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
	public static partial class btl
	{
		public class DarkFormula
		{
			public int calcDarkSubHp(BattlePlayer player)
			{
				int now = player.hp().getNow();
				int num = now * 20 / 100;
				if (num >= now)
				{
					num = now - 1;
				}
				return num;
			}

			public int calcDamageDark(BaseBattleCharacter attacker)
			{
				int num = weaponAttackPower(attacker, pl.HAND_TYPE.RIGHT_HAND) + weaponAttackPower(attacker, pl.HAND_TYPE.LEFT_HAND);
				CommonFormula commonFormula = new CommonFormula();
				int num2 = commonFormula.calcJobSkill(attacker);
				int now = attacker.hp().getNow();
				int num3 = now * 30 / 100;
				return num * 5 + num2 * 10 + num3 * 3;
			}

			public int weaponAttackPower(BaseBattleCharacter attacker, pl.HAND_TYPE hand)
			{
				if (attacker.breed() != 0)
				{
					return 0;
				}
				BattlePlayer battlePlayer = static_cast<BattlePlayer>(attacker);
				if (battlePlayer == null)
				{
					return 0;
				}
				if (battlePlayer.player() == null)
				{
					return 0;
				}
				short itemId = battlePlayer.player().equipParameter().equipHand(hand)
					.itemId();
				if (battlePlayer.player().equipParameter().equipHand(hand)
					.equipNumber()
					.get() == 0)
				{
					return 0;
				}
				return itm.ItemManager.instance().weaponParameter(itemId)?.aggressivity() ?? 0;
			}
		}
	}
}
