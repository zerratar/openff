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
		public class MoveOrderFormula
		{
			public int moveDexterityPlayer(BattlePlayer battlePlayer)
			{
				int num = 0;
				num = magicWeight(battlePlayer);
				num += itemWeight(battlePlayer);
				int t = battlePlayer.bodyAndBonus().dexterity().get() + 1 + 1 - battlePlayer.player().equipParameter().totalWeight() - num;
				return ds.max(t, 0);
			}

			public int moveDexterityMonster(BattleMonster battleMonster)
			{
				int num = 0;
				num = magicWeight(battleMonster);
				num += itemWeight(battleMonster);
				int t = battleMonster.bodyAndBonus().dexterity().get() + 1 - battleMonster.monster().weight() - num;
				return ds.max(t, 0);
			}

			public int magicWeight(BaseBattleCharacter user)
			{
				int num = 0;
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(user.useMagicId());
				if (magicParameter != null)
				{
					num += magicParameter.weight();
				}
				return num;
			}

			public int itemWeight(BaseBattleCharacter user)
			{
				int result = 0;
				itm.ConsumptionParameter consumptionParameter = itm.ItemManager.instance().consumptionParameter((short)user.useItemId());
				if (consumptionParameter != null)
				{
					result = consumptionParameter.weight();
				}
				return result;
			}
		}
	}
}
