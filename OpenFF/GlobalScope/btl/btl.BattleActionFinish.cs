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
		public class BattleActionFinish : BattleActionBase
		{
			public override void initialize(BattlePlayer player)
			{
				itm.WEAPON_SYSTEM weapon = itm.WEAPON_SYSTEM.NON_WEAPON;
				if (player.breed() == 0)
				{
					weapon = player.player().equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND)
						.weaponSystem();
				}
				else if (player.breed() == 2)
				{
					weapon = (itm.WEAPON_SYSTEM)(itm.ItemManager.instance().weaponParameter(player.weaponId(pl.HAND_TYPE.RIGHT_HAND))?.system() ?? 20);
				}
				int num = player.equipWeaponMotionIndex(weapon) + 7;
				if (player.breed() == 0 && player.player().equipParameter().equipHand(pl.HAND_TYPE.RIGHT_HAND)
					.isEquipBow())
				{
					num++;
				}
				player.setNowAttackHand(pl.HAND_TYPE.RIGHT_HAND);
				player.setEffectNumber(1, 0);
				characterMng.startMotion(player.characterMngId(), num, fLoop: false, 0u);
			}

			public override void terminate(BattlePlayer player)
			{
			}

			public override bool execute(BattlePlayer player)
			{
				if (characterMng.isEndOfMotion(player.characterMngId()))
				{
					return true;
				}
				return false;
			}
		}
	}
}
