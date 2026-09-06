// Game.Items on FF4: the unified tables (OpenFF.Data, read by Ff4Party.Tables) through the
// engine's item interface. FF3 keeps LegacyItems over the decompiled itm.ItemManager until
// its tables are served the same way.

using System;
using System.Collections.Generic;
using OpenFF;
using OpenFF.Data;

namespace FF3
{
	internal sealed class Ff4Items : GameService, IItems
	{
		private List<Item> _items;
		private Dictionary<int, Item> _byId;

		private void Ensure()
		{
			if (_items != null) return;
			_items = new List<Item>();
			_byId = new Dictionary<int, Item>();
			GameTables tables = Ff4Party.Tables;
			if (tables == null) return;
			foreach (ItemDefinition d in tables.Items)
			{
				Item item = new Item
				{
					Id = d.Id,
					Name = d.Name,
					Caption = d.Caption,
					Category = d.Kind switch
					{
						ItemKind.Weapon => ItemCategory.Weapon,
						ItemKind.Armour => ItemCategory.Armor,
						ItemKind.KeyItem => ItemCategory.Key,
						ItemKind.Spell => ItemCategory.Magic,
						_ => ItemCategory.Consumable,
					},
					Price = d.BuyPrice,
					UsableInField = d.Kind == ItemKind.Consumable,
					UsableInBattle = d.Kind == ItemKind.Consumable,
				};
				if (d.Equip != null)
				{
					item.Jobs = (int)d.Equip.CanEquip;
					item.Slot = d.Kind == ItemKind.Weapon ? OpenFF.EquipSlot.RightHand : OpenFF.EquipSlot.Body;
					item.Attack = d.Equip.Attack;
					item.Accuracy = d.Equip.Hit;
					item.Defense = d.Equip.Defence;
					item.MagicDefense = d.Equip.MagicDefence;
					item.Evasion = d.Equip.Evade;
					item.MagicEvasion = d.Equip.MagicEvade;
					item.Bonus.Strength = d.Equip.Bonus.Strength;
					item.Bonus.Vitality = d.Equip.Bonus.Vitality;
					item.Bonus.Agility = d.Equip.Bonus.Agility;
					item.Bonus.Intellect = d.Equip.Bonus.Intellect;
					item.Bonus.Mind = d.Equip.Bonus.Spirit;
				}
				_items.Add(item);
				_byId[item.Id] = item;
			}
			foreach (SpellDefinition s in tables.Spells)
			{
				if (_byId.ContainsKey(s.Id)) continue;
				Item spell = new Item { Id = s.Id, Name = s.Name, Category = ItemCategory.Magic };
				_items.Add(spell);
				_byId[spell.Id] = spell;
			}
			Log.Write(LogChannel.File, "items: " + _items.Count + " from the unified FF4 tables");
		}

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
				if (string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)) return item;
			}
			return null;
		}

		public IEnumerable<Item> Of(ItemCategory category)
		{
			Ensure();
			foreach (Item item in _items) if (item.Category == category) yield return item;
		}
	}
}
