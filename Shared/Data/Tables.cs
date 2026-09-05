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
		/// <summary>Commands and spells with the level each arrives at, in the game's order; empty when the game has no such list.</summary>
		public List<Learned> Learning = new List<Learned>();

		public int MaxLevel => Levels.Length;

		/// <summary>The spells known at a level, in the game's order.</summary>
		public List<int> SpellsAt(int level)
		{
			List<int> spells = new List<int>();
			foreach (Learned l in Learning) if (l.IsSpell && l.Level <= level && !spells.Contains(l.Ability)) spells.Add(l.Ability);
			return spells;
		}

		/// <summary>The battle commands at a level (FF4: 1 fight, 3 item, 0x2e change... and the class's own).</summary>
		public List<int> CommandsAt(int level)
		{
			List<int> commands = new List<int>();
			foreach (Learned l in Learning) if (!l.IsSpell && l.Level <= level && !commands.Contains(l.Ability)) commands.Add(l.Ability);
			return commands;
		}

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

	/// <summary>Which magic a spell belongs to (FF4: the byte at 4 of magic_parameter.bbd; NewMagicFormula picks the stat by it - will for white, wisdom for the rest).</summary>
	public enum MagicSchool { White = 0, Black = 1, Summon = 2, Song = 3, Item = 4, Enemy = 5, Ninjutsu = 6, Other = 7 }

	/// <summary>
	/// A spell or ability with battle numbers (FF4: magic_parameter.bbd, 36 bytes per record,
	/// common::BabilMagicParameterManager - id s16 at 0, power s16 at 2, school byte at 4, MP
	/// cost byte at 5 (pl::Player::isUseMagic), hit rate u16 at 6, effect group u16 at 10 and
	/// its rank at 12, element bits at 22, the status it inflicts at 24, grants at 26 and 28,
	/// and a target byte at 32).
	/// </summary>
	public sealed class SpellDefinition
	{
		public int Id;
		public string Name;
		public MagicSchool School;
		public int MpCost;
		/// <summary>The attack or healing power; 0 for a status spell.</summary>
		public int Power;
		/// <summary>Out of 100.</summary>
		public int HitRate;
		public int EffectGroup;
		public int EffectRank;
		/// <summary>Element bits (FF4: 0x20 fire, 0x10 ice, 0x08 lightning, 0x80 earth, 0x100 holy, 0x02 poison/bio, 0x04 drain).</summary>
		public int Element;
		public int Inflicts;
		public int Grants;
		public int Grants2;
		/// <summary>FF4's byte at 32: 0x01 hits every target, 0x02 one target, 0x08 may spread to all, 0x10 usable in battle, 0x20 usable from the menu, 0x40 the player chooses the target.</summary>
		public int TargetFlags;
		public byte[] Raw;

		public bool UsableInBattle => (TargetFlags & 0x10) != 0;
		public bool UsableInMenu => (TargetFlags & 0x20) != 0;
		public bool HitsAll => (TargetFlags & 0x01) != 0;
		public bool CanSpread => (TargetFlags & 0x08) != 0;
		/// <summary>Cures rather than hurts: FF4's healing groups (0xA0 the cure line and its kin) and the white school's recovery entries.</summary>
		public bool Heals => Power > 0 && School == MagicSchool.White && EffectGroup == 0xA0;
		public bool Revives => EffectGroup == 0xA0 && Power == 0 && (Grants & 0x200) != 0;

		public override string ToString() => Id + " " + (Name ?? "?") + " (" + School + ", " + MpCost + " mp" + (Power > 0 ? ", power " + Power : "") + ")";
	}

	/// <summary>
	/// What an item or an ability does when used (FF4: efficacy.beld, common::EfficacyDataConvection):
	/// the potions' section gives hit and magic points restored (9999 for all of them); the
	/// abilities' section names the ability an item casts.
	/// </summary>
	public sealed class Efficacy
	{
		public int Id;
		public int Hp;
		public int Mp;
		/// <summary>The ability cast when this is used (a summon item, a rod), 0 for none.</summary>
		public int CastsAbility;
		public int X10;

		public override string ToString() => Id + (CastsAbility > 0 ? " casts " + CastsAbility : " hp " + Hp + " mp " + Mp);
	}

	/// <summary>One line of a character's learn list: a command or a spell and the level it comes at (FF4: player.chaindata chains 17.., u32 = ability << 16 | level).</summary>
	public struct Learned
	{
		/// <summary>The battle command the spell falls under (FF4: 6 white, 5 black, 13 summon, 4 sing, 0x53 ninjutsu), or the command itself when Ability is below 1500.</summary>
		public int Command;
		public int Ability;
		public int Level;

		public bool IsSpell => Ability >= 1500;
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

	/// <summary>One monster's place in an encounter group.</summary>
	public sealed class MonsterPartySlot
	{
		public int MonsterId;
		public int Flag;
		/// <summary>The game's placement, in world units (FF4: fx32 x, y, z; x across, z depth).</summary>
		public float X, Y, Z;
		/// <summary>FF4's fourth word, not yet named (66 for most groups, 20 for a boss).</summary>
		public float W;
		public int Count = 1;
	}

	/// <summary>An encounter group: what appears together, and where.</summary>
	public sealed class MonsterParty
	{
		public int Id;
		public int Flags;
		public List<MonsterPartySlot> Slots = new List<MonsterPartySlot>();

		public override string ToString() => "party " + Id + ": " + string.Join(", ", Slots.ConvertAll(s => s.MonsterId + (s.Count > 1 ? " x" + s.Count : "")));
	}

	/// <summary>Everything a game defines, read once from its files.</summary>
	public sealed class GameTables
	{
		public List<MonsterParty> MonsterParties = new List<MonsterParty>();
		private Dictionary<int, MonsterParty> _parties;

		public MonsterParty MonsterParty(int id)
		{
			if (_parties == null)
			{
				_parties = new Dictionary<int, MonsterParty>();
				foreach (MonsterParty p in MonsterParties) if (!_parties.ContainsKey(p.Id)) _parties[p.Id] = p;
			}
			return _parties.TryGetValue(id, out MonsterParty found) ? found : null;
		}

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
		public List<Efficacy> Efficacies = new List<Efficacy>();
		private Dictionary<int, SpellDefinition> _spells;
		private Dictionary<int, Efficacy> _efficacies;

		public SpellDefinition Spell(int id)
		{
			if (_spells == null)
			{
				_spells = new Dictionary<int, SpellDefinition>();
				foreach (SpellDefinition s in Spells) if (!_spells.ContainsKey(s.Id)) _spells[s.Id] = s;
			}
			return _spells.TryGetValue(id, out SpellDefinition found) ? found : null;
		}

		public Efficacy Efficacy(int id)
		{
			if (_efficacies == null)
			{
				_efficacies = new Dictionary<int, Efficacy>();
				foreach (Efficacy e in Efficacies) if (!_efficacies.ContainsKey(e.Id)) _efficacies[e.Id] = e;
			}
			return _efficacies.TryGetValue(id, out Efficacy found) ? found : null;
		}

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
				.Append(Characters.Count).Append(" characters, ").Append(Items.Count).Append(" items, ").Append(Spells.Count).Append(" spells, ").Append(Monsters.Count).Append(" monsters, ").Append(MonsterParties.Count).Append(" encounter groups");
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
