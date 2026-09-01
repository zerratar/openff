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
	public static partial class pl
	{
		public class PlayerEquipParameter
		{
			public const int EQUIP_RIGHT_HAND = 0;

			public const int EQUIP_LEFT_HAND = 1;

			public const int EQUIP_HEAD = 2;

			public const int EQUIP_BODY = 3;

			public const int EQUIP_ARM = 4;

			public const int EQUIP_POINTS_MAX = 5;

			private byte weight_;

			private EquipmentItem[] equipPoint_ = new EquipmentItem[5];

			private EquipmentMagic[] magic_ = new EquipmentMagic[8];

			public PlayerEquipParameter()
			{
				for (int i = 0; i < equipPoint_.Length; i++)
				{
					equipPoint_[i] = new EquipmentItem();
				}
				for (int i = 0; i < magic_.Length; i++)
				{
					magic_[i] = new EquipmentMagic();
				}
			}

			public void initialize()
			{
				for (int i = 0; i < 5; i++)
				{
					equipPoint_[i].initialize();
				}
				for (int j = 0; j < 8; j++)
				{
					magic_[j].initialize();
				}
				for (int k = 1; k < 8; k++)
				{
					for (int l = 0; l < MAGIC_ONCE_LEVEL_EQUIP_MAX; l++)
					{
						magic_[k].magicId_set(l, -999);
					}
				}
			}

			public EquipItemInfo doEquipItem(int points, EquipItemInfo itemInfo)
			{
				return equipPoint(points).equip(itemInfo);
			}

			public bool doReleaseEquipItem(int points)
			{
				return false;
			}

			public bool isEquipHarp()
			{
				if (equipHand(HAND_TYPE.RIGHT_HAND).weaponSystem() == itm.WEAPON_SYSTEM.WEAPON_HARP || equipHand(HAND_TYPE.LEFT_HAND).weaponSystem() == itm.WEAPON_SYSTEM.WEAPON_HARP)
				{
					return true;
				}
				return false;
			}

			public bool isEquipBow()
			{
				if (equipHand(HAND_TYPE.RIGHT_HAND).weaponSystem() == itm.WEAPON_SYSTEM.WEAPON_BOW || equipHand(HAND_TYPE.LEFT_HAND).weaponSystem() == itm.WEAPON_SYSTEM.WEAPON_BOW)
				{
					return true;
				}
				return false;
			}

			public bool isEquipArrow()
			{
				if (equipHand(HAND_TYPE.RIGHT_HAND).isEquipArrow() || equipHand(HAND_TYPE.LEFT_HAND).isEquipArrow())
				{
					return true;
				}
				return false;
			}

			public bool isBareHands()
			{
				if (isEquipWeapon() == 0)
				{
					return true;
				}
				if (isEquipWeapon() == 1 && ((isEquipBow() && !isEquipArrow()) || (!isEquipBow() && isEquipArrow())))
				{
					return true;
				}
				return false;
			}

			public int isEquipWeapon()
			{
				int num = 0;
				if (itm.ItemManager.instance().itemCategory(equipHand(HAND_TYPE.RIGHT_HAND).itemId()) == itm.CATEGORY.CATEGORY_WEAPON && equipHand(HAND_TYPE.RIGHT_HAND).equipNumber().get() > 0)
				{
					num++;
				}
				if (itm.ItemManager.instance().itemCategory(equipHand(HAND_TYPE.LEFT_HAND).itemId()) == itm.CATEGORY.CATEGORY_WEAPON && equipHand(HAND_TYPE.LEFT_HAND).equipNumber().get() > 0)
				{
					num++;
				}
				return num;
			}

			public int TotalEquipWeaponWeight()
			{
				return equipHand(HAND_TYPE.RIGHT_HAND).weightEquipItem() + equipHand(HAND_TYPE.LEFT_HAND).weightEquipItem();
			}

			public HAND_TYPE checkEquipWeaponHand()
			{
				if (itm.ItemManager.instance().itemCategory(equipHand(HAND_TYPE.RIGHT_HAND).itemId()) == itm.CATEGORY.CATEGORY_WEAPON)
				{
					if (equipHand(HAND_TYPE.RIGHT_HAND).equipNumber().get() > 0)
					{
						return HAND_TYPE.RIGHT_HAND;
					}
				}
				else if (itm.ItemManager.instance().itemCategory(equipHand(HAND_TYPE.LEFT_HAND).itemId()) == itm.CATEGORY.CATEGORY_WEAPON && equipHand(HAND_TYPE.LEFT_HAND).equipNumber().get() > 0)
				{
					return HAND_TYPE.LEFT_HAND;
				}
				return HAND_TYPE.NO_HAND;
			}

			public HAND_TYPE checkEquipArrow()
			{
				if (equipHand(HAND_TYPE.RIGHT_HAND).isEquipArrow())
				{
					return HAND_TYPE.RIGHT_HAND;
				}
				if (equipHand(HAND_TYPE.LEFT_HAND).isEquipArrow())
				{
					return HAND_TYPE.LEFT_HAND;
				}
				return HAND_TYPE.NO_HAND;
			}

			public void decArrow()
			{
				if (!isEquipBow() || !isEquipArrow())
				{
					return;
				}
				HAND_TYPE hAND_TYPE = checkEquipArrow();
				if (hAND_TYPE != HAND_TYPE.NO_HAND)
				{
					equipHand(hAND_TYPE).equipNumber().sub(1);
					if (equipHand(hAND_TYPE).equipNumber().get() == 0)
					{
						equipHand(hAND_TYPE).release();
					}
				}
			}

			public int shieldAvoidanceNumber()
			{
				return 0;
			}

			public int totalWeight()
			{
				int num = 0;
				if (equipHead().equipNumber().get() > 0)
				{
					num += itemWeight(equipHead().itemId());
				}
				if (equipBody().equipNumber().get() > 0)
				{
					num += itemWeight(equipBody().itemId());
				}
				if (equipArm().equipNumber().get() > 0)
				{
					num += itemWeight(equipArm().itemId());
				}
				if (equipHand(HAND_TYPE.RIGHT_HAND).equipNumber().get() > 0)
				{
					num += itemWeight(equipHand(HAND_TYPE.RIGHT_HAND).itemId());
				}
				if (equipHand(HAND_TYPE.LEFT_HAND).equipNumber().get() > 0)
				{
					num += itemWeight(equipHand(HAND_TYPE.LEFT_HAND).itemId());
				}
				return num;
			}

			public int itemWeight(short _id)
			{
				return itm.ItemManager.instance().itemParameter(_id)?.weight() ?? 0;
			}

			public void setAllBlackMagic()
			{
				OS_Printf("黒魔法覚えるよ！\n\n");
				int num = 1;
				for (int i = 0; i < 8; i++)
				{
					int num2 = 0;
					while (num2 < MAGIC_ONCE_LEVEL_EQUIP_MAX)
					{
						magic_[i].magicId_set(num2, 4100 + num);
						OS_Printf("%d の魔法レベルで左から %d の位置に %d の魔法を覚えた!\n", i, num2, 4100 + num);
						num2++;
						num++;
					}
				}
				OS_Printf("\n黒魔法覚えたよ！\n");
			}

			public void setAllWhiteMagic()
			{
				OS_Printf("白魔法覚えるよ！\n\n");
				int num = 1;
				for (int i = 0; i < 8; i++)
				{
					int num2 = 0;
					while (num2 < MAGIC_ONCE_LEVEL_EQUIP_MAX)
					{
						magic_[i].magicId_set(num2, 4000 + num);
						OS_Printf("%d の魔法レベルで左から %d の位置に %d の魔法を覚えた!\n", i, num2, 4000 + num);
						num2++;
						num++;
					}
				}
				OS_Printf("\n白魔法覚えたよ！\n");
			}

			public void setAllSummonMagic()
			{
				OS_Printf("召還魔法覚えるよ！\n\n");
				int num = 1;
				int num2 = 0;
				while (num2 < 8)
				{
					magic_[num2].magicId_set(0, 4200 + num);
					OS_Printf("%d の魔法レベルに %d の魔法を覚えた!\n", num2, 4200 + num);
					num2++;
					num++;
				}
				OS_Printf("\n召還魔法覚えたよ！\n");
			}

			public void deleteAllMagic()
			{
				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < MAGIC_ONCE_LEVEL_EQUIP_MAX; j++)
					{
						magic_[i].magicId_set(j, -999);
					}
				}
			}

			public EquipmentItem equipHand(HAND_TYPE type)
			{
				if (type != HAND_TYPE.RIGHT_HAND)
				{
					return equipPoint_[1];
				}
				return equipPoint_[0];
			}

			public EquipmentItem equipHead()
			{
				return equipPoint_[2];
			}

			public EquipmentItem equipBody()
			{
				return equipPoint_[3];
			}

			public EquipmentItem equipArm()
			{
				return equipPoint_[4];
			}

			public EquipmentItem equipPoint(int points)
			{
				return equipPoint_[points];
			}

			public EquipmentMagic equipMagic(MAGIC_LEVEL level)
			{
				return magic_[(int)level];
			}

			public void equipMagic_set(MAGIC_LEVEL level, EquipmentMagic arg0)
			{
				magic_[(int)level] = arg0;
			}

			public void setDefault()
			{
				weight_ = 0;
				for (int i = 0; i < 5; i++)
				{
					equipPoint_[i].setDefault();
				}
				for (int i = 0; i < 8; i++)
				{
					magic_[i].setDefault();
				}
			}

			public void copy(PlayerEquipParameter src)
			{
				weight_ = src.weight_;
				for (int i = 0; i < 5; i++)
				{
					equipPoint_[i].copy(src.equipPoint_[i]);
				}
				for (int i = 0; i < 8; i++)
				{
					magic_[i].copy(src.magic_[i]);
				}
			}

			public void parse(ArrayReader reader)
			{
				weight_ = reader.readByte();
				for (int i = 0; i < 5; i++)
				{
					equipPoint_[i].parse(reader);
				}
				for (int i = 0; i < 8; i++)
				{
					magic_[i].parse(reader);
				}
			}

			public void store(ArrayWriter writer)
			{
				writer.writeByte(weight_);
				for (int i = 0; i < 5; i++)
				{
					equipPoint_[i].store(writer);
				}
				for (int i = 0; i < 8; i++)
				{
					magic_[i].store(writer);
				}
			}
		}
	}
}
