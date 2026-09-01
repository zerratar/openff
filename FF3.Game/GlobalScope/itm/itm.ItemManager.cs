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
	public static partial class itm
	{
		public class ItemManager
		{
			public static ItemManager instance_ = new ItemManager();

			private int m_ConsumptionItemMax;

			private int m_WeaponItemMax;

			private int m_ProtectionItemMax;

			private int m_MagicItemMax;

			private int m_ImportantItemMax;

			private Array fileAddr_;

			private ConsumptionParameter[] consumptionItem_;

			private WeaponParameter[] weaponItem_;

			private ProtectionParameter[] protectionItem_;

			private MagicParameter[] magicItem_;

			private ImportantParameter[] ImportantItem_;

			public bool load()
			{
				free();
				m_ConsumptionItemMax = 0;
				m_WeaponItemMax = 0;
				m_ProtectionItemMax = 0;
				m_MagicItemMax = 0;
				m_ImportantItemMax = 0;
				bool flag = false;
				strcpy(out var arg, "item_parameter.pak");
				uint size = ds.g_File.getSize(arg);
				fileAddr_ = ds.CHeap.alloc_app(size);
				flag = ds.g_File.load(fileAddr_, arg);
				pack.ChainPointerCount((byte[])fileAddr_);
				_ = 5;
				consumptionItem_ = ConsumptionParameter.ChainPointer((byte[])fileAddr_, 0);
				weaponItem_ = WeaponParameter.ChainPointer((byte[])fileAddr_, 1);
				protectionItem_ = ProtectionParameter.ChainPointer((byte[])fileAddr_, 2);
				magicItem_ = MagicParameter.ChainPointer((byte[])fileAddr_, 3);
				ImportantItem_ = ImportantParameter.ChainPointer((byte[])fileAddr_, 4);
				uint num = 0u;
				uint num2 = 0u;
				num2 = pack.ChainPointerSize((byte[])fileAddr_, 0u);
				for (num = 0u; num < num2; num += 44)
				{
					m_ConsumptionItemMax++;
				}
				num2 = pack.ChainPointerSize((byte[])fileAddr_, 1u);
				for (num = 0u; num < num2; num += 56)
				{
					m_WeaponItemMax++;
				}
				num2 = pack.ChainPointerSize((byte[])fileAddr_, 2u);
				for (num = 0u; num < num2; num += 60)
				{
					m_ProtectionItemMax++;
				}
				num2 = pack.ChainPointerSize((byte[])fileAddr_, 3u);
				for (num = 0u; num < num2; num += 52)
				{
					m_MagicItemMax++;
				}
				num2 = pack.ChainPointerSize((byte[])fileAddr_, 4u);
				for (num = 0u; num < num2; num += 28)
				{
					m_ImportantItemMax++;
				}
				return flag;
			}

			public void free()
			{
				if (fileAddr_ != null)
				{
					ds.CHeap.free_app(fileAddr_);
					fileAddr_ = null;
				}
			}

			public ItemBaseParameter itemParameter(short ItemId)
			{
				if (ItemId < 0)
				{
					return null;
				}
				ConsumptionParameter arg;
				if ((arg = consumptionParameter(ItemId)) != null)
				{
					return static_cast<ItemBaseParameter>(arg);
				}
				WeaponParameter arg2;
				if ((arg2 = weaponParameter(ItemId)) != null)
				{
					return static_cast<ItemBaseParameter>(arg2);
				}
				ProtectionParameter arg3;
				if ((arg3 = protectionParameter(ItemId)) != null)
				{
					return static_cast<ItemBaseParameter>(arg3);
				}
				MagicParameter arg4;
				if ((arg4 = magicParameter(ItemId)) != null)
				{
					return static_cast<ItemBaseParameter>(arg4);
				}
				ImportantParameter arg5;
				if ((arg5 = importantParameter(ItemId)) != null)
				{
					return static_cast<ItemBaseParameter>(arg5);
				}
				return null;
			}

			public short getItemParameterId(short index)
			{
				short result = 0;
				if (index < m_ConsumptionItemMax)
				{
					result = consumptionItem_[index].itemId();
				}
				else if (index < m_WeaponItemMax + m_ConsumptionItemMax)
				{
					index -= (short)m_ConsumptionItemMax;
					result = weaponItem_[index].itemId();
				}
				else if (index < m_ProtectionItemMax + m_WeaponItemMax + m_ConsumptionItemMax)
				{
					index -= (short)(m_WeaponItemMax + m_ConsumptionItemMax);
					result = protectionItem_[index].itemId();
				}
				else if (index < m_MagicItemMax + m_ProtectionItemMax + m_WeaponItemMax + m_ConsumptionItemMax)
				{
					index -= (short)(m_ProtectionItemMax + m_WeaponItemMax + m_ConsumptionItemMax);
					result = magicItem_[index].itemId();
				}
				else if (index < m_ImportantItemMax + m_MagicItemMax + m_ProtectionItemMax + m_WeaponItemMax + m_ConsumptionItemMax)
				{
					index -= (short)(m_MagicItemMax + m_ProtectionItemMax + m_WeaponItemMax + m_ConsumptionItemMax);
					result = ImportantItem_[index].itemId();
				}
				return result;
			}

			public CATEGORY itemCategory(short ItemId)
			{
				if (ItemId < 0)
				{
					return CATEGORY.CATEGORY_ERR;
				}
				if (consumptionParameter(ItemId) != null)
				{
					return CATEGORY.CATEGORY_CONSUMPTION;
				}
				if (weaponParameter(ItemId) != null)
				{
					return CATEGORY.CATEGORY_WEAPON;
				}
				if (protectionParameter(ItemId) != null)
				{
					return CATEGORY.CATEGORY_PROTECTION;
				}
				if (magicParameter(ItemId) != null)
				{
					return CATEGORY.CATEGORY_MAGIC;
				}
				if (importantParameter(ItemId) != null)
				{
					return CATEGORY.CATEGORY_IMPORTANT;
				}
				return CATEGORY.CATEGORY_ERR;
			}

			public ConsumptionParameter consumptionParameter(short ItemId)
			{
				if (ItemId < 0)
				{
					return null;
				}
				for (short num = 0; num < m_ConsumptionItemMax; num++)
				{
					if (ItemId == consumptionItem_[num].itemId())
					{
						return consumptionItem_[num];
					}
				}
				return null;
			}

			public WeaponParameter weaponParameter(short ItemId)
			{
				if (ItemId < 0)
				{
					return null;
				}
				for (short num = 0; num < m_WeaponItemMax; num++)
				{
					if (ItemId == weaponItem_[num].itemId())
					{
						return weaponItem_[num];
					}
				}
				return null;
			}

			public ProtectionParameter protectionParameter(short ItemId)
			{
				if (ItemId < 0)
				{
					return null;
				}
				for (short num = 0; num < m_ProtectionItemMax; num++)
				{
					if (ItemId == protectionItem_[num].itemId())
					{
						return protectionItem_[num];
					}
				}
				return null;
			}

			public MagicParameter magicParameter(short ItemId)
			{
				if (ItemId < 0)
				{
					return null;
				}
				for (short num = 0; num < m_MagicItemMax; num++)
				{
					if (ItemId == magicItem_[num].itemId())
					{
						return magicItem_[num];
					}
				}
				return null;
			}

			public ImportantParameter importantParameter(short ItemId)
			{
				if (ItemId < 0)
				{
					return null;
				}
				for (short num = 0; num < m_ImportantItemMax; num++)
				{
					if (ItemId == ImportantItem_[num].itemId())
					{
						return ImportantItem_[num];
					}
				}
				return null;
			}

			public static ItemManager instance()
			{
				return instance_;
			}
		}
	}
}
