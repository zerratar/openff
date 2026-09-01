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
		public class CommonFormula
		{
			public enum MAGIC_ATTRIBUTE
			{
				PLUS_ATTRIBUTE,
				NORMAL_ATTRIBUTE,
				MINUS_ATTRIBUTE,
				MAGIC_ATTRIBUTE_MAX
			}

			public const MAGIC_ATTRIBUTE PLUS_ATTRIBUTE = MAGIC_ATTRIBUTE.PLUS_ATTRIBUTE;

			public const MAGIC_ATTRIBUTE NORMAL_ATTRIBUTE = MAGIC_ATTRIBUTE.NORMAL_ATTRIBUTE;

			public const MAGIC_ATTRIBUTE MINUS_ATTRIBUTE = MAGIC_ATTRIBUTE.MINUS_ATTRIBUTE;

			public const MAGIC_ATTRIBUTE MAGIC_ATTRIBUTE_MAX = MAGIC_ATTRIBUTE.MAGIC_ATTRIBUTE_MAX;

			public int calcHandSkill(BaseBattleCharacter character, pl.HAND_TYPE handType)
			{
				int result = 0;
				if (character.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(character);
					pl.GET_SKILL_TYPE type = static_cast<pl.GET_SKILL_TYPE>(handType);
					result = battlePlayer.player().skillManager().skill(type)
						.skillLevel()
						.get();
				}
				else if (character.breed() == 1)
				{
					BattleMonster battleMonster = static_cast<BattleMonster>(character);
					result = battleMonster.monster().level() / 2;
					result = ds.max(result, 1);
				}
				else if (character.breed() == 2)
				{
					result = character.level();
				}
				return result;
			}

			public int calcJobSkill(BaseBattleCharacter character)
			{
				int result = 0;
				if (character.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(character);
					pl.PlayerJobManager playerJobManager = battlePlayer.player().jobManager();
					result = playerJobManager.nowJobParameter().skill().skillLevel()
						.get();
				}
				else if (character.breed() == 1)
				{
					BattleMonster battleMonster = static_cast<BattleMonster>(character);
					result = battleMonster.monster().level() / 2;
					result = ds.max(result, 1);
				}
				else if (character.breed() == 2)
				{
					result = character.level();
				}
				return result;
			}

			public int calcWeight(BaseBattleCharacter character)
			{
				int result = 0;
				if (character.breed() == 0)
				{
					BattlePlayer battlePlayer = static_cast<BattlePlayer>(character);
					if (battlePlayer != null)
					{
						result = battlePlayer.player().equipParameter().totalWeight();
					}
				}
				else if (character.breed() == 1)
				{
					BattleMonster battleMonster = static_cast<BattleMonster>(character);
					if (battleMonster != null)
					{
						result = battleMonster.monster().weight();
					}
				}
				else if (character.breed() == 2)
				{
					result = character.weight();
				}
				return result;
			}

			public int calcAttribute(short attack, short weak, short anti)
			{
				if ((attack & weak) != 0)
				{
					return 2;
				}
				if (OutsideToBattle.getInstance().magicDefenseInvalidation())
				{
					return 1;
				}
				if ((attack & anti) != 0)
				{
					return 0;
				}
				return 1;
			}
		}
	}
}
