// The unified party: who has joined, who is in the line-up, what they carry. Built on
// GameTables; the games' scripts and the engine's Game.Party act on it, and each game's
// battle and menu will read it once they exist on this side of the seam. FF3's own party
// (pl.PlayerParty) is mapped onto the same shape later.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFF.Data
{
	public enum EquipSlot { RightHand, LeftHand, Head, Body, Arms }

	public sealed class Character
	{
		public CharacterDefinition Definition { get; }
		public int Id => Definition.Id;
		public string Name { get; set; }
		public int Level { get; private set; }
		public int Experience { get; set; }
		public int Hp { get; set; }
		public int MaxHp { get; set; }
		public int Mp { get; set; }
		public int MaxMp { get; set; }
		/// <summary>The attributes from the growth table at the current level, before equipment.</summary>
		public Stats Base { get; private set; } = new Stats();
		/// <summary>Item ids by slot, 0 for nothing.</summary>
		public int[] Equipment { get; } = new int[5];
		public List<int> Abilities { get; } = new List<int>();
		public List<int> Spells { get; } = new List<int>();
		/// <summary>Position in the line-up, or -1 when not in the party.</summary>
		public int Slot { get; set; } = -1;
		public bool InParty => Slot >= 0;
		public bool Alive => Hp > 0;

		public Character(CharacterDefinition definition, int level)
		{
			Definition = definition;
			Name = definition.Name ?? ("player " + definition.Id);
			SetLevel(level, true);
		}

		/// <summary>Sets the level; maximums follow the growth table, and with <paramref name="fill"/> the current values too.</summary>
		public void SetLevel(int level, bool fill)
		{
			Level = Math.Max(1, level);
			if (Definition.Levels.Length > 0)
			{
				MaxHp = Math.Max(1, Definition.MaxHpAt(Level));
				MaxMp = Math.Max(0, Definition.MaxMpAt(Level));
				Base = Definition.StatsAt(Level);
			}
			else if (MaxHp == 0)
			{
				MaxHp = 1;
			}
			if (fill)
			{
				Hp = MaxHp;
				Mp = MaxMp;
			}
			else
			{
				Hp = Math.Min(Hp, MaxHp);
				Mp = Math.Min(Mp, MaxMp);
			}
		}

		/// <summary>Attributes with equipment bonuses, when the tables know the items.</summary>
		public Stats StatsWith(GameTables tables)
		{
			Stats stats = Base.Clone();
			foreach (int id in Equipment)
			{
				ItemDefinition item = id != 0 ? tables?.Item(id) : null;
				if (item?.Equip != null) stats = stats + item.Equip.Bonus;
			}
			return stats;
		}

		public override string ToString() => Name + " L" + Level + " " + Hp + "/" + MaxHp + " hp " + Mp + "/" + MaxMp + " mp" + (InParty ? " [slot " + Slot + "]" : "");
	}

	public sealed class ItemStack
	{
		public int ItemId;
		public int Count;
	}

	public sealed class Party
	{
		public GameTables Tables { get; }
		/// <summary>Everyone who exists so far, in the order they were created.</summary>
		public List<Character> Roster { get; } = new List<Character>();
		/// <summary>The line-up, in slot order.</summary>
		public List<Character> Members { get; } = new List<Character>();
		public List<ItemStack> Inventory { get; } = new List<ItemStack>();
		public int Gil { get; set; }
		public int MaxMembers { get; set; } = 5;

		public Party(GameTables tables)
		{
			Tables = tables;
		}

		public Character Get(int id)
		{
			foreach (Character c in Roster) if (c.Id == id) return c;
			return null;
		}

		/// <summary>The character by id, created at <paramref name="level"/> from the tables if it did not exist yet; null for an unknown id.</summary>
		public Character Ensure(int id, int level)
		{
			Character have = Get(id);
			if (have != null) return have;
			CharacterDefinition definition = Tables?.Character(id);
			if (definition == null) return null;
			Character made = new Character(definition, level);
			Roster.Add(made);
			return made;
		}

		/// <summary>Puts a character into the line-up; false when unknown or the party is full.</summary>
		public bool Join(int id, int level = 1)
		{
			Character c = Ensure(id, level);
			if (c == null) return false;
			if (c.InParty) return true;
			if (Members.Count >= MaxMembers) return false;
			c.Slot = Members.Count;
			Members.Add(c);
			return true;
		}

		public bool Leave(int id)
		{
			Character c = Get(id);
			if (c == null || !c.InParty) return false;
			Members.Remove(c);
			c.Slot = -1;
			for (int i = 0; i < Members.Count; i++) Members[i].Slot = i;
			return true;
		}

		public Character Leader => Members.Count > 0 ? Members[0] : null;

		public int CountItem(int itemId)
		{
			foreach (ItemStack s in Inventory) if (s.ItemId == itemId) return s.Count;
			return 0;
		}

		public void AddItem(int itemId, int count = 1)
		{
			if (itemId <= 0 || count <= 0) return;
			foreach (ItemStack s in Inventory)
			{
				if (s.ItemId == itemId) { s.Count = Math.Min(99, s.Count + count); return; }
			}
			Inventory.Add(new ItemStack { ItemId = itemId, Count = Math.Min(99, count) });
		}

		public bool RemoveItem(int itemId, int count = 1)
		{
			for (int i = 0; i < Inventory.Count; i++)
			{
				if (Inventory[i].ItemId != itemId) continue;
				if (Inventory[i].Count < count) return false;
				Inventory[i].Count -= count;
				if (Inventory[i].Count == 0) Inventory.RemoveAt(i);
				return true;
			}
			return false;
		}

		/// <summary>Wears an item (0 takes the slot's item off); the previous item goes back to the bag.</summary>
		public bool Equip(int characterId, EquipSlot slot, int itemId)
		{
			Character c = Get(characterId);
			if (c == null) return false;
			int previous = c.Equipment[(int)slot];
			if (itemId != 0 && Tables?.Item(itemId)?.Equip == null && Tables != null && Tables.Items.Count > 0) return false;
			c.Equipment[(int)slot] = itemId;
			if (previous != 0) AddItem(previous);
			return true;
		}

		public string Describe()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Members.Count).Append(" in the party, ").Append(Roster.Count).Append(" known, ").Append(Gil).Append(" gil, ").Append(Inventory.Count).Append(" item stack(s)");
			foreach (Character c in Members) sb.Append("\n  ").Append(c);
			foreach (ItemStack s in Inventory)
			{
				ItemDefinition item = Tables?.Item(s.ItemId);
				sb.Append("\n  ").Append(item?.Name ?? ("item " + s.ItemId)).Append(" x").Append(s.Count);
			}
			return sb.ToString();
		}
	}
}
