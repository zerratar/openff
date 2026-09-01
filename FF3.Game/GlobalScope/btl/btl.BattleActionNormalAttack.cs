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
		public class BattleActionNormalAttack : BattleActionBase
		{
			private int[] attackMotionCount_ = new int[2];

			public override void initialize(BattlePlayer player)
			{
				if (player.breed() != 2 && player.condition().isFrog())
				{
					characterMng.startMotion(player.characterMngId(), 118, fLoop: false, 0u);
					player.setNowAttackHand(pl.HAND_TYPE.RIGHT_HAND);
					player.setEffectNumber(1, 0);
					return;
				}
				attackMotionCount_[0] = player.attackMotionNumber(0);
				attackMotionCount_[1] = player.attackMotionNumber(1);
				if (player.flag(PLAYER_FLAG.PF_FINISH))
				{
					if (attackMotionCount_[1] > 0)
					{
						attackMotionCount_[1]--;
						if (attackMotionCount_[1] == 0)
						{
							attackMotionCount_[1] = -1;
						}
					}
					else if (attackMotionCount_[0] > 0)
					{
						attackMotionCount_[0]--;
						if (attackMotionCount_[0] == 0)
						{
							attackMotionCount_[0] = -1;
						}
					}
				}
				if (attackMotionCount_[0] > 0)
				{
					setEquipWeaponMotion(player, pl.HAND_TYPE.RIGHT_HAND, 5);
					player.setNowAttackHand(pl.HAND_TYPE.RIGHT_HAND);
				}
				else if (attackMotionCount_[1] > 0)
				{
					setEquipWeaponMotion(player, pl.HAND_TYPE.LEFT_HAND, 0);
					player.setNowAttackHand(pl.HAND_TYPE.LEFT_HAND);
				}
			}

			public override void terminate(BattlePlayer player)
			{
			}

			public override bool execute(BattlePlayer player)
			{
				return executeNormalWeaponAttack(player);
			}

			public void setEquipWeaponMotion(BattlePlayer player, pl.HAND_TYPE type, int frame)
			{
				itm.WEAPON_SYSTEM wEAPON_SYSTEM = itm.WEAPON_SYSTEM.NON_WEAPON;
				if (player.breed() == 0)
				{
					wEAPON_SYSTEM = player.player().equipParameter().equipHand(type)
						.weaponSystem();
				}
				else if (player.breed() == 2)
				{
					wEAPON_SYSTEM = (itm.WEAPON_SYSTEM)(itm.ItemManager.instance().weaponParameter(player.weaponId(type))?.system() ?? 20);
				}
				if (player.breed() == 0 && ((player.player().equipParameter().isEquipBow() && !player.player().equipParameter().isEquipArrow()) || (!player.player().equipParameter().isEquipBow() && player.player().equipParameter().isEquipArrow())))
				{
					wEAPON_SYSTEM = itm.WEAPON_SYSTEM.NON_WEAPON;
				}
				int num = -1;
				int num2 = -1;
				num2 = motionIndexOffset(type, wEAPON_SYSTEM);
				num = player.equipWeaponMotionIndex(wEAPON_SYSTEM) + num2;
				characterMng.startMotion(player.characterMngId(), num, fLoop: false, (uint)frame);
			}

			public int motionIndexOffset(pl.HAND_TYPE type, itm.WEAPON_SYSTEM weaponSystem)
			{
				int num = -1;
				num = ((type == pl.HAND_TYPE.RIGHT_HAND) ? 1 : 3);
				if (weaponSystem != itm.WEAPON_SYSTEM.WEAPON_THROW && weaponSystem != itm.WEAPON_SYSTEM.WEAPON_BOW && weaponSystem != itm.WEAPON_SYSTEM.WEAPON_ARROW && weaponSystem != itm.WEAPON_SYSTEM.WEAPON_HARP)
				{
					num += (int)ds.RandomNumber.rand32(2u);
				}
				else
				{
					switch (weaponSystem)
					{
					case itm.WEAPON_SYSTEM.WEAPON_HARP:
						num = ((type == pl.HAND_TYPE.RIGHT_HAND) ? 1 : 2);
						break;
					case itm.WEAPON_SYSTEM.WEAPON_BOW:
						num = ((type != pl.HAND_TYPE.RIGHT_HAND) ? 1 : 2);
						break;
					case itm.WEAPON_SYSTEM.WEAPON_ARROW:
						num = ((type == pl.HAND_TYPE.RIGHT_HAND) ? 1 : 2);
						break;
					}
				}
				return num;
			}

			public bool executeNormalWeaponAttack(BattlePlayer player)
			{
				if (player.breed() != 2 && player.condition().isFrog())
				{
					if (characterMng.isEndOfMotion(player.characterMngId()))
					{
						return true;
					}
					return false;
				}
				if (attackMotionCount_[0] == 0)
				{
					if (characterMng.isEndOfMotion(player.characterMngId()))
					{
						attackMotionCount_[0] = -1;
						if (attackMotionCount_[1] == -1)
						{
							return true;
						}
						player.setNowAttackHand(pl.HAND_TYPE.LEFT_HAND);
						setEquipWeaponMotion(player, pl.HAND_TYPE.LEFT_HAND, 0);
					}
				}
				else if (attackMotionCount_[0] > 0)
				{
					controlMotionCancel(player, pl.HAND_TYPE.RIGHT_HAND);
				}
				else if (attackMotionCount_[1] == 0)
				{
					if (characterMng.isEndOfMotion(player.characterMngId()))
					{
						attackMotionCount_[0] = -1;
						return true;
					}
				}
				else if (attackMotionCount_[1] > 0)
				{
					controlMotionCancel(player, pl.HAND_TYPE.LEFT_HAND);
				}
				else if (attackMotionCount_[1] < 0 && attackMotionCount_[0] < 0)
				{
					return true;
				}
				return false;
			}

			public void controlMotionCancel(BattlePlayer player, pl.HAND_TYPE type)
			{
				int motionIndex = characterMng.getMotionIndex(player.characterMngId());
				int frame = pl.PlayerParty.instance().normalAttack(motionIndex).cancelStartFrame();
				int num = pl.PlayerParty.instance().normalAttack(motionIndex).cancelEndFrame();
				if (characterMng.getCurrentFrame(player.characterMngId()) == num)
				{
					attackMotionCount_[(int)type]--;
					if (attackMotionCount_[(int)type] > 0)
					{
						setEquipWeaponMotion(player, type, 0);
						characterMng.setCurrentFrame(player.characterMngId(), (uint)frame);
					}
				}
			}
		}
	}
}
