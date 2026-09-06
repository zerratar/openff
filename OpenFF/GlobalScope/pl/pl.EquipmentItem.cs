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
	public static partial class pl
	{
		public class EquipmentItem
		{
			public const int NO_ITEM_ID = -99;

			public const int KNUCKLE_ID = 1000;

			private short itemId_;

			private ys.ParameterPoint<int> equipNumber_ = new ys.ParameterPoint<int>(0, 99);

			public void initialize()
			{
				itemId_ = -99;
				equipNumber_.min();
			}

			public EquipItemInfo equipItemInfo()
			{
				return new EquipItemInfo(itemId_, (byte)equipNumber_.get());
			}

			public EquipItemInfo equip(EquipItemInfo itemInfo)
			{
				EquipItemInfo equipItemInfo = new EquipItemInfo();
				equipItemInfo.itemId_ = itemId_;
				equipItemInfo.itemNumber_ = (byte)equipNumber_.get();
				itemId_ = itemInfo.itemId_;
				equipNumber_.set(itemInfo.itemNumber_);
				return equipItemInfo;
			}

			public EquipItemInfo release()
			{
				EquipItemInfo equipItemInfo = new EquipItemInfo();
				equipItemInfo.itemId_ = itemId_;
				equipItemInfo.itemNumber_ = (byte)equipNumber_.get();
				initialize();
				return equipItemInfo;
			}

			public itm.WEAPON_SYSTEM weaponSystem()
			{
				if (equipNumber_.get() == 0)
				{
					return itm.WEAPON_SYSTEM.NON_WEAPON;
				}
				if (itemId_ <= 0)
				{
					return itm.WEAPON_SYSTEM.NON_WEAPON;
				}
				if (itm.ItemManager.instance().itemParameter(itemId_) == null)
				{
					return itm.WEAPON_SYSTEM.NON_WEAPON;
				}
				if (itm.ItemManager.instance().itemCategory(itemId_) != itm.CATEGORY.CATEGORY_WEAPON)
				{
					return itm.WEAPON_SYSTEM.NON_WEAPON;
				}
				return (itm.WEAPON_SYSTEM)itm.ItemManager.instance().itemParameter(itemId_).system();
			}

			public itm.PROTECTION_SYSTEM protectionSystem()
			{
				if (equipNumber_.get() == 0)
				{
					return itm.PROTECTION_SYSTEM.NON_PROTECTION;
				}
				if (itemId_ <= 0)
				{
					return itm.PROTECTION_SYSTEM.NON_PROTECTION;
				}
				if (itm.ItemManager.instance().itemParameter(itemId_) == null)
				{
					return itm.PROTECTION_SYSTEM.NON_PROTECTION;
				}
				if (itm.ItemManager.instance().itemCategory(itemId_) != itm.CATEGORY.CATEGORY_PROTECTION)
				{
					return itm.PROTECTION_SYSTEM.NON_PROTECTION;
				}
				return (itm.PROTECTION_SYSTEM)itm.ItemManager.instance().itemParameter(itemId_).system();
			}

			public short haveItem()
			{
				if (equipNumber_.get() == 0)
				{
					return -99;
				}
				return itemId_;
			}

			public int weightEquipItem()
			{
				int result = 0;
				switch (itm.ItemManager.instance().itemCategory(itemId_))
				{
				case itm.CATEGORY.CATEGORY_WEAPON:
					result = itm.ItemManager.instance().weaponParameter(itemId_).weight();
					break;
				case itm.CATEGORY.CATEGORY_PROTECTION:
					result = itm.ItemManager.instance().protectionParameter(itemId_).weight();
					break;
				}
				return result;
			}

			public bool isEquipBow()
			{
				if (equipNumber_.get() == 0)
				{
					return false;
				}
				itm.WeaponParameter weaponParameter = itm.ItemManager.instance().weaponParameter(itemId_);
				if (weaponParameter == null)
				{
					return false;
				}
				if (weaponParameter.system() == 7)
				{
					return true;
				}
				return false;
			}

			public bool isEquipArrow()
			{
				if (weaponSystem() == itm.WEAPON_SYSTEM.WEAPON_ARROW)
				{
					return true;
				}
				return false;
			}

			public bool isEquipHarp()
			{
				if (equipNumber_.get() == 0)
				{
					return false;
				}
				itm.WeaponParameter weaponParameter = itm.ItemManager.instance().weaponParameter(itemId_);
				if (weaponParameter == null)
				{
					return false;
				}
				if (weaponParameter.system() == 16)
				{
					return true;
				}
				return false;
			}

			public bool isEquipPitch()
			{
				if (equipNumber_.get() == 0)
				{
					return false;
				}
				itm.WeaponParameter weaponParameter = itm.ItemManager.instance().weaponParameter(itemId_);
				if (weaponParameter == null)
				{
					return false;
				}
				if (weaponParameter.system() == 14)
				{
					return true;
				}
				return false;
			}

			public itm.CATEGORY checkCategory()
			{
				return itm.ItemManager.instance().itemCategory(itemId_);
			}

			public bool checkClaw()
			{
				itm.CATEGORY cATEGORY = itm.ItemManager.instance().itemCategory(itemId_);
				if (cATEGORY != itm.CATEGORY.CATEGORY_WEAPON)
				{
					return false;
				}
				if (weaponSystem() == itm.WEAPON_SYSTEM.WEAPON_CLAW)
				{
					return true;
				}
				return false;
			}

			public short itemId()
			{
				return itemId_;
			}

			public ys.ParameterPoint<int> equipNumber()
			{
				return equipNumber_;
			}

			public void setDefault()
			{
				itemId_ = 0;
				equipNumber_.set(0);
			}

			public void copy(EquipmentItem src)
			{
				itemId_ = src.itemId_;
				equipNumber_.set(src.equipNumber_.get());
			}

			public void parse(ArrayReader reader)
			{
				itemId_ = reader.readInt16();
				equipNumber_.set(reader.readInt32());
			}

			public void store(ArrayWriter writer)
			{
				writer.writeInt16(itemId_);
				writer.writeInt32(equipNumber_.get());
			}
		}
	}
}
