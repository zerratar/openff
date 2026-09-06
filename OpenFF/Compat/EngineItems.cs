// The engine's items and shops on the legacy game.
//
// Items come from itm.ItemManager's five tables (item_parameter.pak), names through the
// message system as the menus resolve them. Shops are the game's own shop screen: the
// script command BootShop sets a shop index on shop.CShopManager and puts the world into
// its shop state; the table it reads is the map's (<three letters>.shp, "t01" when the map
// has none), and a PORT hook lets a mod name the table so a shop can open anywhere.

using System;
using System.Collections.Generic;
using System.Linq;
using OpenFF;

namespace OpenFF.Client
{
	internal sealed class LegacyItems : GameService, IItems
	{
		private readonly List<Item> _items = new List<Item>();
		private readonly Dictionary<int, Item> _byId = new Dictionary<int, Item>();
		private readonly Dictionary<Item, short[]> _textIds = new Dictionary<Item, short[]>();
		private bool _read, _namesDone;

		public IReadOnlyList<Item> All { get { Ensure(); return _items; } }

		public Item Find(int id)
		{
			Ensure();
			return _byId.TryGetValue(id, out Item item) ? item : null;
		}

		public Item Find(string name)
		{
			Ensure();
			if (string.IsNullOrEmpty(name)) return null;
			foreach (Item item in _items)
			{
				if (item.Name == null) item.Name = Text(item, true);
				if (string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)) return item;
			}
			return null;
		}

		public IEnumerable<Item> Of(ItemCategory category)
		{
			Ensure();
			return _items.Where(i => i.Category == category);
		}

		private void Ensure()
		{
			if (_read) return;
			// FF4's tables have their own layouts; the engine's data facades are FF3's for now.
			if (GameProfile.IsFf4) { _read = true; return; }
			try
			{
				GlobalScope.itm.ItemManager items = GlobalScope.itm.ItemManager.instance();
				int total = items.consumptionCount() + items.weaponCount() + items.protectionCount() + items.magicCount() + items.importantCount();
				if (total == 0) return;
				for (int i = 0; i < items.consumptionCount(); i++)
				{
					GlobalScope.itm.ConsumptionParameter p = items.consumptionAt(i);
					if (p == null) continue;
					Item item = Base(p, ItemCategory.Consumable);
					item.Price = p.price();
					Add(item, p.nameId(), p.captionId());
				}
				for (int i = 0; i < items.weaponCount(); i++)
				{
					GlobalScope.itm.WeaponParameter p = items.weaponAt(i);
					if (p == null) continue;
					Item item = Base(p, ItemCategory.Weapon);
					item.Price = p.price();
					item.Jobs = p.equipJob();
					item.Slot = EquipSlot.RightHand;
					item.Attack = p.aggressivity();
					item.Accuracy = p.hitProbability();
					item.Elements = (Element)p.armsAttribute();
					item.Model = "w" + p.graphId().ToString("D3");
					Add(item, p.nameId(), p.captionId());
				}
				for (int i = 0; i < items.protectionCount(); i++)
				{
					GlobalScope.itm.ProtectionParameter p = items.protectionAt(i);
					if (p == null) continue;
					Item item = Base(p, ItemCategory.Armor);
					item.Price = p.price();
					item.Jobs = p.equipJob();
					item.Slot = LegacyParty.SlotOf(p.itemId());
					item.Defense = p.phylacticPower();
					item.MagicDefense = p.magicPhylacticPower();
					item.Evasion = p.avoidanceProbability();
					item.MagicEvasion = p.magicAvoidanceProbability();
					item.Resist = (Element)p.antiType();
					item.Weakness = (Element)p.weakType();
					Add(item, p.nameId(), p.captionId());
				}
				for (int i = 0; i < items.magicCount(); i++)
				{
					GlobalScope.itm.MagicParameter p = items.magicAt(i);
					if (p == null) continue;
					Item item = Base(p, ItemCategory.Magic);
					item.Price = p.price();
					item.Jobs = p.equipJob();
					Add(item, p.nameId(), p.captionId());
				}
				for (int i = 0; i < items.importantCount(); i++)
				{
					GlobalScope.itm.ImportantParameter p = items.importantAt(i);
					if (p == null) continue;
					Add(Base(p, ItemCategory.Key), p.nameId(), p.captionId());
				}
				_read = true;
				OpenFF.Game.Log("engine api: " + _items.Count + " items read from the game's tables");
			}
			catch (Exception ex) { EngineApi.Warn("item-table", "reading the item tables: " + ex.Message); }
		}

		private static Item Base(GlobalScope.itm.ItemBaseParameter p, ItemCategory category)
		{
			Item item = new Item { Id = p.itemId(), Category = category };
			try
			{
				item.Bonus.Strength = p.strength();
				item.Bonus.Vitality = p.vitality();
				item.Bonus.Agility = p.dexterity();
				item.Bonus.Intellect = p.intellect();
				item.Bonus.Mind = p.mind();
				item.Weight = p.weight();
				item.UsableInBattle = p.useBattle() != 0;
				item.UsableInField = p.useField() != 0;
			}
			catch (Exception) { }
			return item;
		}

		private void Add(Item item, short nameId, short captionId)
		{
			_textIds[item] = new[] { nameId, captionId };
			_items.Add(item);
			_byId[item.Id] = item;
		}

		private string Text(Item item, bool name)
		{
			if (!_textIds.TryGetValue(item, out short[] ids)) return null;
			short id = name ? ids[0] : ids[1];
			if (id <= 0) return null;
			try
			{
				string text = GlobalScope.dgs.msg.CMessageSys.getInstance().Main().getMessage((uint)id);
				return string.IsNullOrEmpty(text) ? null : text.Trim();
			}
			catch (Exception) { return null; }
		}

		public override bool WantsUpdate => true;

		public override void OnUpdate()
		{
			if (!EngineApi.InWorld) return;
			Ensure();
			if (!_read || _namesDone) return;
			_namesDone = true;
			foreach (Item item in _items)
			{
				if (item.Name == null) item.Name = Text(item, true);
				if (item.Caption == null) item.Caption = Text(item, false);
				if (item.Name == null) _namesDone = false;
			}
		}
	}

	internal sealed class LegacyShops : GameService, IShops
	{
		public bool IsOpen
		{
			get
			{
				try { return EngineApi.InWorld && GlobalScope.CCastCommandTransit.getInstance().cast_BaseSystem().IsShop(); }
				catch (Exception) { return false; }
			}
		}

		public bool Open(int index, string table = null)
		{
			if (!EngineApi.InWorld || index < 0) return false;
			try
			{
				GlobalScope.shop.CShopManager.OverrideTable = string.IsNullOrWhiteSpace(table) ? null : table.Trim();
				GlobalScope.shop.CShopManager.Instance().ShopIndex_set((uint)index);
				GlobalScope.CCastCommandTransit.getInstance().cast_BaseSystem().setShop(b: true);
				return true;
			}
			catch (Exception ex) { EngineApi.Warn("shop", "Shops.Open: " + ex.Message); return false; }
		}

		public ShopInfo Info(int index, string table = null)
		{
			try
			{
				string name = string.IsNullOrWhiteSpace(table) ? "t01" : table.Trim();
				GlobalScope.shop.CShopParameterManager manager = new GlobalScope.shop.CShopParameterManager();
				GlobalScope.changeGlobalDirectory();
				if (!manager.load(name + ".shp")) return null;
				try
				{
					GlobalScope.shop.CShopParameter shop = manager.ShopParameter((uint)index);
					if (shop == null) return null;
					ShopInfo info = new ShopInfo { Index = index, Kind = shop.ShopKind() };
					for (int i = 0; i < GlobalScope.shop.CShopParameter.ITEM_LIST_NUM; i++)
					{
						int id = shop.ItemId(i);
						if (id > 0) info.ItemIds.Add(id);
					}
					return info;
				}
				finally
				{
					manager.free();
				}
			}
			catch (Exception ex) { EngineApi.Warn("shop-info", "Shops.Info: " + ex.Message); return null; }
		}
	}
}
