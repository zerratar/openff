// The unified tables: one shape for what a game defines - its characters and how they
// grow, its items, its spells - whichever game the bytes came from. The readers
// (Ff4Tables, FF3's to follow) fill these from each game's own files; the engine, the
// editor and mods read only these. Fields a reader could not name keep their offset in
// the name (X4, X0B) so the column is there and its ignorance is visible.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFF.Data
{
	/// <summary>The five base attributes. FF4 calls them strength, agility, vitality, wisdom, will; FF3 strength, agility, vitality, intellect, mind.</summary>
	public enum Attribute { Strength, Agility, Vitality, Intellect, Spirit }

	public sealed class Stats
	{
		public int Strength, Agility, Vitality, Intellect, Spirit;

		public int this[Attribute a]
		{
			get => a switch { Attribute.Strength => Strength, Attribute.Agility => Agility, Attribute.Vitality => Vitality, Attribute.Intellect => Intellect, _ => Spirit };
			set
			{
				switch (a)
				{
					case Attribute.Strength: Strength = value; break;
					case Attribute.Agility: Agility = value; break;
					case Attribute.Vitality: Vitality = value; break;
					case Attribute.Intellect: Intellect = value; break;
					default: Spirit = value; break;
				}
			}
		}

		public Stats Clone() => new Stats { Strength = Strength, Agility = Agility, Vitality = Vitality, Intellect = Intellect, Spirit = Spirit };

		public static Stats operator +(Stats a, Stats b) => new Stats
		{
			Strength = a.Strength + b.Strength, Agility = a.Agility + b.Agility, Vitality = a.Vitality + b.Vitality,
			Intellect = a.Intellect + b.Intellect, Spirit = a.Spirit + b.Spirit
		};

		public override string ToString() => "str " + Strength + " agi " + Agility + " vit " + Vitality + " int " + Intellect + " spi " + Spirit;
	}

	/// <summary>One line of a character's growth table: what reaching this level brings.</summary>
	public sealed class LevelRow
	{
		public int Level;
		/// <summary>The least hit points gained on reaching the level (FF4: the u16 at 0; level 1's row is the starting value). FF4 rolls between the two.</summary>
		public int HpGainMin;
		/// <summary>The most hit points gained on reaching the level (FF4: the u16 at 2).</summary>
		public int HpGainMax;
		/// <summary>Magic points at the level (FF4: the u16 at 4 - it climbs 3, 6, 8, 11... so it reads as a total, not a gain; tentative).</summary>
		public int Mp;
		/// <summary>The attributes at this level, absolute (FF4: five bytes at 6).</summary>
		public Stats Stats = new Stats();
		/// <summary>FF4: the byte at 11, not yet named.</summary>
		public int X0B;
	}

	public sealed class CharacterDefinition
	{
		/// <summary>The game's id: FF4's PLAYER_TYPES (0 Cecil...), FF3's character index.</summary>
		public int Id;
		/// <summary>A stable, readable key for scenes and mods: "cecil", "kain"...</summary>
		public string Key;
		public string Name;
		/// <summary>The class or job the character starts as ("Dark Knight"); null when the game does not say.</summary>
		public string ClassName;
		/// <summary>The field model's name (FF4: p00_01) and the battle model's (b_p_player_00), when known.</summary>
		public string FieldModel;
		public string BattleModel;
		/// <summary>Growth, one row per level from level 1; empty when the game has no table for the character.</summary>
		public LevelRow[] Levels = Array.Empty<LevelRow>();
		/// <summary>True where the name and class were filled from knowledge of the game rather than its files.</summary>
		public bool NameIsTentative;

		public int MaxLevel => Levels.Length;

		/// <summary>Maximum hit points at a level: the gains of every row up to it, the middle of each row's range (the game rolls).</summary>
		public int MaxHpAt(int level)
		{
			int hp = 0;
			for (int i = 0; i < Math.Min(level, Levels.Length); i++) hp += (Levels[i].HpGainMin + Levels[i].HpGainMax) / 2;
			return hp;
		}

		public int MaxMpAt(int level)
		{
			if (Levels.Length == 0) return 0;
			return Levels[Math.Clamp(level, 1, Levels.Length) - 1].Mp;
		}

		public Stats StatsAt(int level)
		{
			if (Levels.Length == 0) return new Stats();
			return Levels[Math.Clamp(level, 1, Levels.Length) - 1].Stats.Clone();
		}

		public override string ToString() => (Name ?? ("player " + Id)) + (ClassName != null ? " (" + ClassName + ")" : "") + ", " + Levels.Length + " levels";
	}

	public enum ItemKind { Consumable, Weapon, Armour, KeyItem, Spell }

	/// <summary>What a weapon or a piece of armour does when worn.</summary>
	public sealed class EquipStats
	{
		/// <summary>A mask of who may wear it (FF4: by PLAYER_TYPE; FF3: by job).</summary>
		public uint CanEquip;
		/// <summary>Where it goes (FF4's canEquipOnPosition).</summary>
		public int Position;
		public int Attack;
		public int Hit;
		public int Defence;
		public int Evade;
		public int MagicDefence;
		public int MagicEvade;
		/// <summary>Attribute bonuses while worn.</summary>
		public Stats Bonus = new Stats();

		public override string ToString() => "atk " + Attack + " hit " + Hit + " def " + Defence + " eva " + Evade + " mdef " + MagicDefence + " meva " + MagicEvade;
	}

	public sealed class ItemDefinition
	{
		/// <summary>The game's item id (FF4: 5001+ consumables, 6001+ weapons, 8001+ armour, 9001+ key items).</summary>
		public int Id;
		public ItemKind Kind;
		/// <summary>The game's "system" byte at the head of the record.</summary>
		public int System;
		public int NameId;
		public string Name;
		public int CaptionId;
		public string Caption;
		public int GraphId;
		public int EfficacyId;
		public int BuyPrice;
		public int SellPrice;
		/// <summary>Null for anything that is not worn.</summary>
		public EquipStats Equip;
		/// <summary>The record as the game keeps it, for fields nobody has named.</summary>
		public byte[] Raw;

		public override string ToString() => Id + " " + (Name ?? "?") + " (" + Kind + (BuyPrice > 0 ? ", " + BuyPrice + " gil" : "") + ")";
	}

	public sealed class SpellDefinition
	{
		public int Id;
		public string Name;
		public byte[] Raw;

		public override string ToString() => Id + " " + (Name ?? "?");
	}

	/// <summary>One thing a monster may leave behind.</summary>
	public sealed class DropChance
	{
		public int ItemId;
		/// <summary>FF4: out of 4096 (819 = one in five); FF3 keeps a table id and a probability on the record instead.</summary>
		public int Chance;
	}

	public sealed class MonsterDefinition
	{
		/// <summary>The game's monster id (monsterId at 8), what encounters and scripts name.</summary>
		public int Id;
		public int NameId;
		public string Name;
		public int TextId;
		public int Family;
		public int ModelId;
		public int Level;
		public int MaxHp;
		public int Size;
		/// <summary>FF4's five bytes at 0x12; FF3 keeps them deeper in the record and they are not read yet.</summary>
		public Stats Stats = new Stats();
		public int Experience;
		public int Gil;
		public List<DropChance> Drops = new List<DropChance>();
		/// <summary>FF3: the drop table id and probability of the record's DroppingDataParameter.</summary>
		public int DropTable = -1;
		public int DropProbability;
		public byte[] Raw;

		public override string ToString() => Id + " " + (Name ?? "?") + " L" + Level + " (" + MaxHp + " hp, " + Experience + " exp, " + Gil + " gil)";
	}

	/// <summary>Everything a game defines, read once from its files.</summary>
	public sealed class GameTables
	{
		public List<MonsterDefinition> Monsters = new List<MonsterDefinition>();
		private Dictionary<int, MonsterDefinition> _monsters;

		public MonsterDefinition Monster(int id)
		{
			if (_monsters == null)
			{
				_monsters = new Dictionary<int, MonsterDefinition>();
				foreach (MonsterDefinition m in Monsters) if (!_monsters.ContainsKey(m.Id)) _monsters[m.Id] = m;
			}
			return _monsters.TryGetValue(id, out MonsterDefinition found) ? found : null;
		}

		public string Game;
		/// <summary>Experience needed to reach each level: index 0 is level 1 (0), index 1 level 2...</summary>
		public int[] ExperienceToLevel = Array.Empty<int>();
		public List<CharacterDefinition> Characters = new List<CharacterDefinition>();
		public List<ItemDefinition> Items = new List<ItemDefinition>();
		public List<SpellDefinition> Spells = new List<SpellDefinition>();
		/// <summary>Names of abilities, summons and spells by the game's id (FF4: babil_ability.msd, whose message ids are the ability ids).</summary>
		public Dictionary<int, string> AbilityNames = new Dictionary<int, string>();
		/// <summary>What the reader could not do (a missing file, a name table it did not find), for the log.</summary>
		public List<string> Notes = new List<string>();

		public string AbilityName(int id) => AbilityNames.TryGetValue(id, out string name) ? name : null;

		private Dictionary<int, ItemDefinition> _items;
		private Dictionary<int, CharacterDefinition> _characters;

		public ItemDefinition Item(int id)
		{
			if (_items == null)
			{
				_items = new Dictionary<int, ItemDefinition>();
				foreach (ItemDefinition item in Items) _items[item.Id] = item;
			}
			return _items.TryGetValue(id, out ItemDefinition found) ? found : null;
		}

		public CharacterDefinition Character(int id)
		{
			if (_characters == null)
			{
				_characters = new Dictionary<int, CharacterDefinition>();
				foreach (CharacterDefinition c in Characters) _characters[c.Id] = c;
			}
			return _characters.TryGetValue(id, out CharacterDefinition found) ? found : null;
		}

		/// <summary>The level an experience total has reached.</summary>
		public int LevelForExperience(int experience)
		{
			int level = 1;
			for (int i = 1; i < ExperienceToLevel.Length; i++)
			{
				if (experience >= ExperienceToLevel[i]) level = i + 1; else break;
			}
			return level;
		}

		public string Describe()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Game).Append(" tables: ").Append(ExperienceToLevel.Length).Append(" levels, ")
				.Append(Characters.Count).Append(" characters, ").Append(Items.Count).Append(" items, ").Append(Spells.Count).Append(" spells, ").Append(Monsters.Count).Append(" monsters");
			if (ExperienceToLevel.Length > 10)
			{
				sb.Append("\n  exp to level 2..11: ");
				for (int i = 1; i <= 10; i++) sb.Append(ExperienceToLevel[i]).Append(i < 10 ? ", " : "");
			}
			foreach (CharacterDefinition c in Characters)
			{
				sb.Append("\n  ").Append(c.Id).Append(' ').Append(c.Name ?? "?").Append(c.NameIsTentative ? "*" : "")
					.Append(c.ClassName != null ? " (" + c.ClassName + ")" : "");
				if (c.Levels.Length > 0)
				{
					sb.Append(": L1 hp ").Append(c.MaxHpAt(1)).Append(" mp ").Append(c.MaxMpAt(1)).Append(' ').Append(c.StatsAt(1))
						.Append("; L10 hp ").Append(c.MaxHpAt(10)).Append(" mp ").Append(c.MaxMpAt(10)).Append(' ').Append(c.StatsAt(10))
						.Append("; L99 hp ").Append(c.MaxHpAt(99));
				}
			}
			int shown = 0;
			foreach (ItemDefinition item in Items)
			{
				if (shown++ >= 12) break;
				sb.Append("\n  ").Append(item);
				if (item.Equip != null) sb.Append(" ").Append(item.Equip);
			}
			if (Items.Count > shown) sb.Append("\n  ... ").Append(Items.Count - shown).Append(" more items");
			shown = 0;
			foreach (MonsterDefinition m in Monsters)
			{
				if (shown++ >= 6) break;
				sb.Append("\n  ").Append(m);
				if (m.Drops.Count > 0)
				{
					sb.Append(" drops");
					foreach (DropChance d in m.Drops) sb.Append(' ').Append(Item(d.ItemId)?.Name ?? d.ItemId.ToString()).Append(' ').Append(d.Chance).Append(';');
				}
			}
			if (Monsters.Count > shown) sb.Append("\n  ... ").Append(Monsters.Count - shown).Append(" more monsters");
			foreach (string note in Notes) sb.Append("\n  note: ").Append(note);
			return sb.ToString();
		}
	}
}
