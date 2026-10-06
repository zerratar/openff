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
		/// <summary>FF3: magic charges per magic level 1..8 at this level (player.chaindata GrowUpMp); null where a game has one MP pool.</summary>
		public int[] Charges;
	}

	/// <summary>
	/// A class or job that grows on its own (FF3: the 23 jobs - a character grows by the job it
	/// holds, from player.chaindata's growth-type table (chain 1, six bytes per job), the eight
	/// growth curves (chain 2, 99 bytes each) and seven charge tables (chains 4..10); hit points
	/// grow by level + vitality plus up to half the vitality again, from 32 at level 1; FF4's
	/// classes are the characters themselves, see CharacterDefinition).
	/// </summary>
	public sealed class JobDefinition
	{
		public int Id;
		public string Key;
		public string Name;
		public bool NameIsTentative;
		/// <summary>FF3: the six growth types - strength, vitality, agility, intellect, mind curves (0..7) and the charge table (0 none, 1..7).</summary>
		public int[] GrowthTypes = Array.Empty<int>();
		public LevelRow[] Levels = Array.Empty<LevelRow>();
		/// <summary>FF3: the job's four battle commands (pl.ABILITY_ID: 1 attack, 3 guard, 4 item, 5 black magic...) and its two passive abilities (0 none), from player.chaindata chain 14.</summary>
		public int[] Commands = Array.Empty<int>();
		public int[] Passives = Array.Empty<int>();

		public Stats StatsAt(int level)
		{
			if (Levels.Length == 0) return new Stats();
			return Levels[Math.Clamp(level, 1, Levels.Length) - 1].Stats.Clone();
		}

		/// <summary>Expected maximum hit points at a level: the middle of every row's range up to it.</summary>
		public int MaxHpAt(int level)
		{
			int hp = 0;
			for (int i = 0; i < Math.Min(level, Levels.Length); i++) hp += (Levels[i].HpGainMin + Levels[i].HpGainMax) / 2;
			return hp;
		}

		public override string ToString() => (Name ?? ("job " + Id)) + ", " + Levels.Length + " levels";
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
		/// <summary>
		/// FF4: maximum hit points are rolled level by level, as sys::PlayerHp::setMaxHp does - the minimum gains of the
		/// levels climbed plus a random part of their spread (RandomNumber::rand32(sum max - sum min + 1)); two new games
		/// give Cecil a different level-10 maximum (Steam: 214 in one, 225 in another). Otherwise the middle of each range.
		/// </summary>
		public bool RollsHp;

		private static readonly Random _hpRoll = new Random();

		/// <summary>The hit points gained climbing from <paramref name="from"/> to <paramref name="to"/>, rolled as the game rolls them.</summary>
		public int RollHp(int from, int to)
		{
			int min = 0, max = 0;
			for (int level = Math.Max(1, from + 1); level <= Math.Min(to, Levels.Length); level++)
			{
				min += Levels[level - 1].HpGainMin;
				max += Levels[level - 1].HpGainMax;
			}
			return max - min < 1 ? min : min + _hpRoll.Next(max - min + 1);
		}
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
		/// <summary>FF4: the model a worn weapon or shield shows as in battle, w&lt;ModelId:000&gt; (the item parameter's short at 10; pl::PlayerEquipmentSymbol::createModel) - the Dark Sword w000, the Dark Shield w094.</summary>
		public int ModelId = -1;
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
		/// <summary>FF3: the magic level (1..8) whose charges the spell spends; 0 where a game has one MP pool.</summary>
		public int Level;
		/// <summary>FF3: 0 attack, 1 recovery, 2 special, 3 status (magicUseKind); FF4 says it through the effect group.</summary>
		public int UseKind;
		/// <summary>FF3: which jobs may cast it (equipJob mask); 0 where the game does not say.</summary>
		public uint CanUse;
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
		public bool Revives => (EffectGroup == 0xA0 && Power == 0 && (Grants & 0x200) != 0) || (Kind == 1 && (Conditions & 0x200) != 0);
		/// <summary>FF4: the conditions it brings or (a recovery spell) takes away - magic_parameter +0x18, one u64 (bit n = ys::Condition n).</summary>
		public ulong Conditions;
		/// <summary>FF4: magic_parameter +0x14 - 0 an attack, 1 a recovery, 2 other (Libra and the like).</summary>
		public int Kind;

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
		/// <summary>FF4: the range a monster's ATB rate is rolled from as it enters a fight (mon::MonsterParameter 0x18 and 0x1c, fx32; btl::BattleMonster::setMonster) - the Floating Eye's 0.1 to 0.5, a Goblin's 0.3 to 1.1.</summary>
		public float AtbRateMin = 1f, AtbRateMax = 1f;
		/// <summary>FF4: where its damage number rises from, over its position (monster.chaindata chain 4 by monster id, MonsterManager::offset's +0x28).</summary>
		public float DamageX, DamageY = 12f, DamageZ;
		/// <summary>FF4: its blows' multipliers for the back rows (its ys::PhysicsAttackParameter's +8 and +0xC, fx32): when it stands in the back, and when its target does (the Floating Eye's 1.0 and 0.75).</summary>
		public float BackRowAttack = 1f, BackRowTarget = 1f;
		/// <summary>FF4: where an effect on it plays (BaseBattleCharacter::hitEffectPosition, chain 4's +4 and +8): this much up, and this much toward the camera.</summary>
		public float EffectHeight = 8f, EffectToCamera;
		/// <summary>FF4: its plain attack (monster.chaindata chain 2, ys::Effects, 28 bytes by monster id): the effect and the frame it starts, its sound (bank, number) and frame, the frame its number shows. -1 for none.</summary>
		public int AttackEffect = -1, AttackEffectFrame = 8, AttackSoundBank = -1, AttackSound = -1, AttackSoundFrame = 8, AttackNumberFrame = 8;
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
		/// <summary>FF4: the word at 0x20 (7 for a Goblin) and the hit chance at 0x22 (105); what btl::BattleMonster hands NewAttackFormula as its physics attack. Tentative.</summary>
		public int Attack;
		public int Hit;
		/// <summary>FF4: the block BattleMonster::setMonster copies from 0x4C - defence at 0x4C (20 for a Goblin), evade at 0x50 (5) - and the word at 0x68 read as magic defence (5). Tentative.</summary>
		public int Defence;
		public int Evade;
		public int MagicDefence;
		/// <summary>FF4: the magic evasion (record +0x68).</summary>
		public int MagicEvasion;
		/// <summary>FF4: the model's size in battle (monster.chaindata chain 4 +0x44, 1 most often) and the chant effect's (+0x50).</summary>
		public float Scale = 1f, ChantScale = 1f;
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
		/// <summary>FF4: the monster's facing on the battle stage, in degrees about y (party 900's Floating Eyes 60 and 80, as Steam stands them).</summary>
		public float W;
		public int Count = 1;
	}

	/// <summary>An encounter group: what appears together, and where.</summary>
	/// <summary>
	/// FF4: how a spell or a monster's ability plays - its effect pack and the effect's create parameter, where it is put
	/// on a target (0 the hit spot, 1 the feet, 2 the body), the sound, and the period N: the effects on several targets
	/// start N/2 frames apart. A monster ability's may wait on its own motion (Motion, played as it acts).
	/// </summary>
	public sealed class SpellShow
	{
		public int Pack = -1, Param = 1, Mode, SeBank = -1, SeNumber = -1, Period, Motion = -1;
	}

	/// <summary>
	/// FF4: a battle condition (ys::Condition id): its duration in frames (-1 none), its flags - 0x1 its landing cuts the
	/// victim's turn, 0x8 the gauge stands, 0x20 the ailing idle, 0x80 cleared when the battle ends, 0x200 the magic hit
	/// roll applies, 0x1000 a counter is still made - what it clears as it comes and what keeps it off.
	/// </summary>
	public sealed class ConditionParameter
	{
		public int Id, Duration = -1, Flags;
		public ulong Replaces, BlockedBy;
		public string Name;
		public bool Is(int bit) => (Flags >> bit & 1) != 0;
	}

	/// <summary>FF4: a boss's entrance (battle_parameter.chain chain 4): the camera's close-up and the frames it takes to the standing shot.</summary>
	public sealed class BossCamera
	{
		public float[] Position, Target;
		public int Frames;
	}

	/// <summary>FF4: who a monster's Alarm or Summon brings (battle_parameter.chain chain 24): the candidates, the effect and sound, the encounter slot.</summary>
	public sealed class MonsterSummon
	{
		public int Caller, Effect = -1, SeBank = -1, SeNumber = -1, Slot;
		public List<int> Candidates = new List<int>();
	}

	public sealed class MonsterParty
	{
		public int Id;
		public int Flags;
		/// <summary>FF4: the record's byte 3, which btl::CBattleDisplay::setBattleCamera uses to pick the battle camera (0 for all but five groups; 1 and 2 are closer shots).</summary>
		public int CameraType => (sbyte)((Flags >> 8) & 0xFF);
		/// <summary>FF4: the record's byte 2, the party root the members stand on (btl::BattleSystem::initialize: battle_parameter.chain's partyRoot id) - 0 the classic side view, 1 the opening's airship deck.</summary>
		public int PartyRootId => (sbyte)(Flags & 0xFF);
		public List<MonsterPartySlot> Slots = new List<MonsterPartySlot>();
		/// <summary>FF4: the record's six slots as they are, the empty ones too (BattleMonsterParty::addMember stands a called monster at its slot's).</summary>
		public MonsterPartySlot[] Places = new MonsterPartySlot[6];
		/// <summary>FF4: its battle events (battle_ai.bbd script ids at 0x7C, 0x80, 0x84; -1 none): the normal one, run every idle frame, the one before an action and the one after.</summary>
		public int NormalEvent = -1, BeforeEvent = -1, AfterEvent = -1;
		/// <summary>FF4: the record's word at 0x88 - bit 0 the party may escape, bit 2 the run control does nothing (checkEscape).</summary>
		public int EscapeFlags = 1;

		public override string ToString() => "party " + Id + ": " + string.Join(", ", Slots.ConvertAll(s => s.MonsterId + (s.Count > 1 ? " x" + s.Count : "")));
	}

	/// <summary>One spot a party member stands on in battle, and the way they face (degrees about y; FF4's -90 faces -x, towards the monsters).</summary>
	public sealed class PartyRootSlot
	{
		public float X, Y, Z;
		public float Facing;
	}

	/// <summary>
	/// Where the party stands in battle: FF4's battle_parameter.chain, chain 0 (btl::BattleParameter::partyRoot
	/// finds the record by id, BattlePartyPosition::position takes row x 80 + slot x 16 + 4 from it). One
	/// record per situation - 0 the normal fight, 1 a back attack (the party turned, spread across), 2 a
	/// pincer - with two rows (front, back) of five slots each, the slots running from the top of the
	/// screen (z -25) to the bottom (z 50) as the camera sees them.
	/// </summary>
	public sealed class PartyRoot
	{
		public int Id;
		/// <summary>[row][slot]: row 0 the front row, 1 the back row; five slots.</summary>
		public PartyRootSlot[][] Rows = new PartyRootSlot[2][];
	}

	/// <summary>FF4: where the party stands for its victory and where the camera looks (player.chaindata chain 34, by the party's size less one).</summary>
	public sealed class VictoryLayout
	{
		public int Id;
		public float[] CameraPosition, CameraTarget;
		public PartyRootSlot[] Spots = new PartyRootSlot[5];
	}

	/// <summary>FF4: a player type's battle motion sets (battle_parameter.chain chain 2): b_p&lt;PlayerSet&gt; holds the stance it stands in (Cecil's b_p1009: 2004), b_&lt;BasicSet&gt; more of its own.</summary>
	public sealed class BattlePlayerMotions
	{
		public int Type;
		public int PlayerSet;
		public int BasicSet;
	}

	/// <summary>FF4: a player type's motions with one weapon system (chains 8..: 16 bytes) - the poise motion (b_poise&lt;Poise&gt;) and the weapon's own set (b_w&lt;WeaponSet:00&gt;); Raw the record's eight words (the attack motions 2 and 3).</summary>
	public sealed class WeaponMotionRecord
	{
		public int PlayerType, WeaponSystem, Poise, WeaponSet;
		public short[] Raw;
	}

	/// <summary>FF4: a monster action set - drawn at random or taken in turn, its (ability, target type) entries.</summary>
	public sealed class MonsterTurnAction
	{
		public int Id;
		public bool Random;
		public List<(int Ability, int Target)> Entries = new List<(int, int)>();
	}

	/// <summary>Everything a game defines, read once from its files.</summary>
	public sealed class GameTables
	{
		public List<MonsterParty> MonsterParties = new List<MonsterParty>();
		/// <summary>Where the party stands in battle, by situation (FF4; empty for FF3 so far).</summary>
		public List<PartyRoot> PartyRoots = new List<PartyRoot>();
		public PartyRoot PartyRoot(int id) => PartyRoots.Find(r => r.Id == id);
		public List<BattlePlayerMotions> BattlePlayers = new List<BattlePlayerMotions>();
		public List<VictoryLayout> VictoryLayouts = new List<VictoryLayout>();
		public VictoryLayout VictoryLayout(int id) => VictoryLayouts.Find(v => v.Id == id);
		public List<WeaponMotionRecord> WeaponMotions = new List<WeaponMotionRecord>();
		/// <summary>FF4: battle_parameter chain 5, by weapon system - eleven (bank, number) sound pairs, a plain hit's first (btl::BattleParameter::playerWeaponSe; -1 none).</summary>
		public List<short[]> WeaponSounds = new List<short[]>();
		/// <summary>FF4: a monster's AI (monster.chaindata chain 7, 22 bytes by monster id): s16s - the id, its default action set, then up to four action conditions tried in order (-1 none), then the counters' (mon::MonsterManager::ai).</summary>
		public Dictionary<int, short[]> MonsterAi = new Dictionary<int, short[]>();
		/// <summary>FF4: an action set (chain 8, 44 bytes): whether its entry is drawn at random (byte 2), then up to ten (ability, target type) pairs, -1 ending them (mon::MonsterTurnAction).</summary>
		public Dictionary<int, MonsterTurnAction> MonsterTurnActions = new Dictionary<int, MonsterTurnAction>();
		/// <summary>FF4: an action condition (chain 9, 12 bytes): the action set it brings and the 64 checks (a mask) that must all hold (MonsterActionThinker::isEnableCondition).</summary>
		public Dictionary<int, (int TurnAction, ulong Checks)> MonsterActionConditions = new Dictionary<int, (int, ulong)>();

		/// <summary>FF4's monster counters (monster.chaindata chain 10, MonsterManager::counter): by id, two entries of an ability, a target type and a chance in percent.</summary>
		public Dictionary<int, (int Ability, int Target, int Chance)[]> MonsterCounters = new Dictionary<int, (int, int, int)[]>();
		/// <summary>FF4: ability.bbd's wait (s32 at 0x18 of its 44-byte records) by ability id - the frames a decided action waits before its turn (BaseBattleCharacter::atwMax): a spell by its id, an item by the ability it invokes.</summary>
		public Dictionary<int, int> AbilityWaits = new Dictionary<int, int>();

		/// <summary>FF4: an ability's name message (ability.bbd +8, in babil_ability.msd) - a spell's own id, Needles' 3134.</summary>
		public Dictionary<int, int> AbilityNameIds = new Dictionary<int, int>();

		/// <summary>FF4: the bosses' entrance cameras (battle_parameter.chain chain 4) by monster.</summary>
		public Dictionary<int, BossCamera> BossCameras = new Dictionary<int, BossCamera>();

		/// <summary>FF4: a command's invoke (battle_parameter.chain chain 1) by command: [0] the command, [1..15] the chant motion by player form, [16] the motion after, [17] the effect, [18] its parameter, [19] its place, [20] and [21] the sound.</summary>
		public Dictionary<int, short[]> AbilityInvokes = new Dictionary<int, short[]>();

		/// <summary>FF4: the Octomammoth's legs (monster.chaindata chain 6, MonsterManager::octmanmosLegInfo) - by the legs left and a leg, its place from the body and its turn (x, y, z, then degrees about x, y, z).</summary>
		public Dictionary<(int Count, int Index), float[]> OctomammothLegs = new Dictionary<(int, int), float[]>();

		/// <summary>FF4: the statuses each ability may be used under (ability.bbd +0x1C), for the ones that check.</summary>
		public Dictionary<int, ulong> AbilityUsableUnder = new Dictionary<int, ulong>();

		/// <summary>FF4: the battle conditions (condition_parameter.bbd), by id.</summary>
		public Dictionary<int, ConditionParameter> Conditions = new Dictionary<int, ConditionParameter>();

		/// <summary>FF4: the monsters that call others, by the caller's id.</summary>
		public Dictionary<int, MonsterSummon> MonsterSummons = new Dictionary<int, MonsterSummon>();

		/// <summary>FF4: how a spell is shown (player.chaindata chain 32, pl::PlayerParty::normalMagic) - for members and monsters alike.</summary>
		public Dictionary<int, SpellShow> SpellShows = new Dictionary<int, SpellShow>();

		/// <summary>FF4: how a monster's ability is shown (monster.chaindata chain 5 by ability; chain 11 by ability and monster first - MonsterManager::effectsInfo).</summary>
		public Dictionary<int, SpellShow> MonsterAbilityShows = new Dictionary<int, SpellShow>();
		public Dictionary<(int Ability, int Monster), SpellShow> MonsterAbilityShowsFor = new Dictionary<(int, int), SpellShow>();

		/// <summary>An ability's name by ability.bbd's message, else by its own id.</summary>
		public string AbilityTitle(int id) => AbilityNameIds.TryGetValue(id, out int name) && name >= 0 ? AbilityName(name) ?? AbilityName(id) : AbilityName(id);
		public int AbilityWait(int id) => AbilityWaits.TryGetValue(id, out int wait) ? wait : 0;
		public WeaponMotionRecord WeaponMotion(int playerType, int weaponSystem) => WeaponMotions.Find(w => w.PlayerType == playerType && w.WeaponSystem == weaponSystem);
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
		public List<JobDefinition> Jobs = new List<JobDefinition>();
		private Dictionary<int, JobDefinition> _jobs;

		public JobDefinition Job(int id)
		{
			if (_jobs == null)
			{
				_jobs = new Dictionary<int, JobDefinition>();
				foreach (JobDefinition j in Jobs) if (!_jobs.ContainsKey(j.Id)) _jobs[j.Id] = j;
			}
			return _jobs.TryGetValue(id, out JobDefinition found) ? found : null;
		}
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
		/// <summary>FF3: each ability's kind by id - 0 a battle command, 1 a passive (pl.ABILITY_TYPE), from player.chaindata chain 13.</summary>
		public Dictionary<int, int> AbilityKinds = new Dictionary<int, int>();
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
				.Append(Characters.Count).Append(" characters, ").Append(Jobs.Count).Append(" jobs, ").Append(Items.Count).Append(" items, ").Append(Spells.Count).Append(" spells, ").Append(Monsters.Count).Append(" monsters, ").Append(MonsterParties.Count).Append(" encounter groups");
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
