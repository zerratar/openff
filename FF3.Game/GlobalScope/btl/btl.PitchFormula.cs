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
		public class PitchFormula
		{
			public bool calcPitchHitOdds(BattlePlayer attacker)
			{
				CommonFormula commonFormula = new CommonFormula();
				int num = commonFormula.calcJobSkill(attacker);
				int num2 = 95 + num / 20;
				if (num2 <= ds.RandomNumber.rand32(101u))
				{
					return false;
				}
				return true;
			}

			public int calcPitchDamage(BattlePlayer attacker)
			{
				itm.WeaponParameter weaponParameter = itm.ItemManager.instance().weaponParameter((short)attacker.useItemId());
				if (weaponParameter == null)
				{
					return 0;
				}
				int num = weaponParameter.aggressivity();
				CommonFormula commonFormula = new CommonFormula();
				int num2 = commonFormula.calcJobSkill(attacker);
				int num3 = 10 * (100 + num * 25 / 10) * ((100 + num2) * 10 * 10 / 99) / 10 / 10;
				return (int)(num3 * (ds.RandomNumber.rand32(81u) + 100) / 100);
			}
		}
	}
}
