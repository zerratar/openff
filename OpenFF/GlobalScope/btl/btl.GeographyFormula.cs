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
		public class GeographyFormula
		{
			public int calcGeographyDamage1(BattlePlayer player, short type)
			{
				int num = player.bodyAndBonus().intelligence().get();
				OS_Printf("知性は\u3000%d\u3000です\n", num);
				int num2 = player.bodyAndBonus().mind().get();
				OS_Printf("精神は\u3000%d\u3000です\n", num2);
				int num3 = player.player().jobManager().nowJobParameter()
					.skill()
					.skillLevel()
					.get();
				OS_Printf("熟練度は\u3000%d\u3000です\n", num3);
				int num4 = 0;
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(player.useMagicId());
				if (magicParameter != null)
				{
					num4 = magicParameter.magicAggressivity();
				}
				int num5 = (num + num2) / 3 * (num3 * 2 + num4);
				num5 = (int)(num5 * (ds.RandomNumber.rand32(21u) + 80) / 100);
				OS_Printf("ダメージは\u3000%d\u3000です\n", num5);
				return num5;
			}

			public int calcGeographyDamage2(BattlePlayer player, short type)
			{
				int num = player.bodyAndBonus().intelligence().get();
				OS_Printf("知性は\u3000%d\u3000です\n", num);
				int num2 = player.bodyAndBonus().mind().get();
				OS_Printf("精神は\u3000%d\u3000です\n", num2);
				int num3 = player.player().jobManager().nowJobParameter()
					.skill()
					.skillLevel()
					.get();
				OS_Printf("熟練度は\u3000%d\u3000です\n", num3);
				int num4 = 0;
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(player.useMagicId());
				if (magicParameter != null)
				{
					num4 = magicParameter.magicAggressivity();
				}
				int num5 = (num + num2) / 4 * (num3 * 2 + num4);
				num5 = (int)(num5 * (ds.RandomNumber.rand32(21u) + 80) / 100);
				OS_Printf("ダメージは\u3000%d\u3000です\n", num5);
				return num5;
			}

			public int calcGeographyDamage3(BattlePlayer player)
			{
				int num = player.bodyAndBonus().intelligence().get();
				OS_Printf("知性は\u3000%d\u3000です\n", num);
				int num2 = player.bodyAndBonus().mind().get();
				OS_Printf("精神は\u3000%d\u3000です\n", num2);
				int num3 = player.player().jobManager().nowJobParameter()
					.skill()
					.skillLevel()
					.get();
				OS_Printf("熟練度は\u3000%d\u3000です\n", num3);
				int num4 = 0;
				itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter(player.useMagicId());
				if (magicParameter != null)
				{
					num4 = magicParameter.magicAggressivity();
				}
				int num5 = (num + num2) * (num3 * 2 + num4);
				num5 = (int)(num5 * (ds.RandomNumber.rand32(21u) + 80) / 100);
				OS_Printf("ダメージは\u3000%d\u3000です\n", num5);
				return num5;
			}

			public bool calcGeographyDeath1(BattlePlayer player)
			{
				int num = (int)ds.RandomNumber.rand32(101u);
				if (num > 30)
				{
					return false;
				}
				return true;
			}

			public bool calcGeographyDeath2(BattlePlayer player)
			{
				int num = (int)ds.RandomNumber.rand32(101u);
				if (num > 80)
				{
					return false;
				}
				return true;
			}
		}
	}
}
