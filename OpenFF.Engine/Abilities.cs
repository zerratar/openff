// Abilities: the game's spells and monsters as data, and the game's own arithmetic.
//
// A mod that runs its own kind of fight - real time, a bullet hell over the field, a duel
// on a single map - wants the game's spells (what they are called, what they cost, what
// they hit, how hard) and the game's monsters (their names, hit points, weaknesses), and
// the game's own formulas so "Fire from a level 12 black mage" does what the battle would
// do. This file is that: plain data classes, filled by the host from the game's tables,
// plus a place for a mod's own spells beside them. Casting a spell here means its effect
// and sound on the field and, if the mod wants, a number computed by Damage or Healing;
// applying that number is the mod's call (Game.Party.Hurt, or the mod's own monsters).

using System;
using System.Collections.Generic;

namespace OpenFF
{
	/// <summary>The schools of magic, as the game files them.</summary>
	public enum MagicSchool { White = 0, Black = 1, Summon = 2, Song = 3, Geomancy = 4, Enemy = 5 }

	/// <summary>What a spell does: hurts, heals, helps, or something of its own.</summary>
	public enum MagicKind { Attack = 0, Recovery = 1, Assist = 2, Special = 3 }

	/// <summary>Elements, as flags: a spell may carry several, a monster may be weak to or resist several.</summary>
	[Flags]
	public enum Element
	{
		None = 0, Recovery = 1, Poison = 2, Absorb = 4, Thunder = 8, Ice = 16, Fire = 32,
		Water = 64, Earth = 128, Holy = 256, Wind = 512, Dark = 1024
	}

	/// <summary>Whom a spell may be aimed at, as flags.</summary>
	[Flags]
	public enum Targeting
	{
		Nothing = 1, Enemy = 2, EnemyGroup = 4, EnemyAll = 8, RandomEnemy = 16,
		Self = 64, Friend = 128, FriendGroup = 256, FriendAll = 512, Everyone = 4096
	}

	/// <summary>The lasting conditions a character can be in, as flags.</summary>
	[Flags]
	public enum Condition
	{
		None = 0, Death = 1, Stone = 2, Frog = 4, Silence = 8, Mini = 16, Blind = 32, Poison = 64, NearDeath = 128
	}

	/// <summary>The jobs, in the game's order (the DS/phone names).</summary>
	public enum Job
	{
		Freelancer = 0, OnionKnight, Warrior, Monk, WhiteMage, BlackMage, RedMage, Ranger, Knight, Thief,
		Scholar, Geomancer, Dragoon, Viking, DarkKnight, Evoker, Bard, BlackBelt, Devout, Magus, Summoner, Sage, Ninja
	}

	/// <summary>The stats a formula needs, of a party member or a monster (or a mod's own creature).</summary>
	public sealed class Stats
	{
		public int Strength, Vitality, Agility, Intellect, Mind;
		/// <summary>Magic defence: taken off a spell's power before intellect multiplies it.</summary>
		public int MagicDefense;
		/// <summary>The job skill level (party) or half the level (monsters): added to a spell's power.</summary>
		public int JobSkill;
		/// <summary>Elements that hit twice as hard.</summary>
		public Element Weakness;
		/// <summary>Elements that do nothing.</summary>
		public Element Resist;
		public Stats Clone() => (Stats)MemberwiseClone();
	}

	/// <summary>A spell: the game's, or one a mod added with Game.Magic.Add.</summary>
	public sealed class Spell
	{
		/// <summary>The game's id (its item number; the game's spells are 4000 and up), or the mod's own.</summary>
		public int Id { get; set; }
		public string Name { get; set; }
		/// <summary>The one-line description the menu shows.</summary>
		public string Caption { get; set; }
		public MagicSchool School { get; set; }
		/// <summary>The magic level 1-8; casting costs one charge of that level.</summary>
		public int Level { get; set; } = 1;
		public MagicKind Kind { get; set; }
		/// <summary>The spell's power in the formulas.</summary>
		public int Power { get; set; }
		/// <summary>Percent chance of landing, as the table has it.</summary>
		public int Accuracy { get; set; } = 100;
		public Element Elements { get; set; }
		public Targeting Targets { get; set; } = Targeting.Enemy;
		/// <summary>Whether it may hit everyone on a side.</summary>
		public bool HitsAll { get; set; }
		public bool InBattle { get; set; } = true;
		public bool InField { get; set; }
		public bool Reflectable { get; set; }
		/// <summary>Conditions it inflicts (attack) or cures (recovery).</summary>
		public Condition Conditions { get; set; }
		/// <summary>The effect the game plays for it: category (its pack, e%03d.efp) and member.</summary>
		public int EffectCategory { get; set; } = -1;
		public int EffectMember { get; set; } = -1;
		/// <summary>The sound the game plays for it: archive and number, or -1.</summary>
		public int SoundArchive { get; set; } = -1;
		public int SoundNumber { get; set; } = -1;
		/// <summary>Jobs that may equip it, as a bitmask over Job; 0 when the table does not say.</summary>
		public int Jobs { get; set; }
		/// <summary>True for a mod's own spell.</summary>
		public bool Custom { get; set; }
		/// <summary>The mod that added it, or null for the game's.</summary>
		public object Owner { get; set; }
		/// <summary>For a mod's own spell: run when Cast plays it (after the effect), for whatever the spell does.</summary>
		public Action<SpellCast> OnCast { get; set; }
		public override string ToString() => (Name ?? ("spell " + Id)) + " (L" + Level + " " + School + ", power " + Power + ")";
	}

	/// <summary>What a cast knows: the spell, where, whom.</summary>
	public sealed class SpellCast
	{
		public Spell Spell;
		public Vector3 At;
		/// <summary>The character aimed at, or null for a point.</summary>
		public Npc Target;
		public bool OnHero;
		/// <summary>The effect the cast started, for Effects.Remove / Alive, or -1.</summary>
		public int Effect = -1;
		public float Scale = 1f;
	}

	/// <summary>A monster, from the game's table.</summary>
	public sealed class Monster
	{
		public int Id { get; set; }
		public string Name { get; set; }
		/// <summary>The family: monsters of one family share a model; the model's name is f + family, three digits.</summary>
		public int Family { get; set; }
		/// <summary>The model to SpawnModel: "f" + Family as three digits (its textures per monster are f + family + _ + id).</summary>
		public string Model { get; set; }
		public int Level { get; set; }
		public int MaxHp { get; set; }
		public int Size { get; set; }
		public Stats Stats { get; set; } = new Stats();
		/// <summary>Whether the bestiary files it as special (a boss).</summary>
		public bool Special { get; set; }
		/// <summary>Gil, experience and job points a battle with it gives; 0 when unknown.</summary>
		public int Gil { get; set; }
		public int Experience { get; set; }
		public override string ToString() => (Name ?? ("monster " + Id)) + " L" + Level + " (" + MaxHp + " hp)";
	}

	/// <summary>A monster party from the game's encounter table: which monsters, how many of each.</summary>
	public sealed class MonsterGroup
	{
		public int Id { get; set; }
		public List<MonsterCount> Members { get; } = new List<MonsterCount>();
		public override string ToString() => "party " + Id + ": " + string.Join(", ", Members);
	}

	public sealed class MonsterCount
	{
		public int MonsterId { get; set; }
		public int Min { get; set; }
		public int Max { get; set; }
		public override string ToString() => MonsterId + " x" + Min + (Max != Min ? "-" + Max : "");
	}

	/// <summary>The game's magic: its spells as data, the game's formulas, and a spell's effect on the field.</summary>
	public interface IMagic
	{
		/// <summary>Every spell the game has (from its tables, once a game part has loaded them) plus the mods' own.</summary>
		IReadOnlyList<Spell> All { get; }
		Spell Find(int id);
		/// <summary>By name, case-insensitively (the game's texts must be loaded: on a map they are).</summary>
		Spell Find(string name);
		/// <summary>Adds a mod's own spell (Custom is set for it). An id already in use replaces that spell.</summary>
		void Add(Spell spell);
		void Remove(int id);
		/// <summary>Plays a spell's effect and sound at a point (loading its effect pack if needed); the effect id, or -1. Runs OnCast for a mod's spell.</summary>
		int Cast(Spell spell, Vector3 at, float scale = 1f);
		/// <summary>As Cast, on a character (the effect follows it).</summary>
		int CastOn(Spell spell, Npc target, float scale = 1f);
		/// <summary>As Cast, on the hero.</summary>
		int CastOnHero(Spell spell, float scale = 1f);
		/// <summary>The game's battle damage of a spell: power and job skill against magic defence and mind, times intellect, a hit roll, elements, the number of targets. roll=false takes the average hit.</summary>
		int Damage(Spell spell, Stats caster, Stats target, int targets = 1, bool roll = true);
		/// <summary>The game's healing of a spell: mind, job skill and vitality times power, a roll, elements (undead take it as damage: Element.Recovery in their weakness), targets.</summary>
		int Healing(Spell spell, Stats caster, Stats target, int targets = 1, bool roll = true);
		/// <summary>Whether a character could cast the spell now: knows it, has a charge of its level, is not silenced or dead.</summary>
		bool CanCast(int memberId, Spell spell);
		/// <summary>Spends one charge of the spell's level from a party member; false when none left.</summary>
		bool Spend(int memberId, Spell spell);
		/// <summary>The game's own field use of a recovery spell (Cure, Poisona, Raise...) by one member on another (or all): effect, sound, healing, as the menu does it.</summary>
		bool UseInField(Spell spell, int casterId, int targetId, bool all = false);
	}

	/// <summary>The game's monsters and encounter groups, as data.</summary>
	public interface IMonsters
	{
		/// <summary>Every monster (loading the game's table on first use).</summary>
		IReadOnlyList<Monster> All { get; }
		Monster Find(int id);
		Monster Find(string name);
		/// <summary>An encounter group by the id Battle.Start takes; null when unknown.</summary>
		MonsterGroup Group(int partyId);
	}

	public static partial class Game
	{
		public static IMagic Magic => Services.Get<IMagic>();
		public static IMonsters Monsters => Services.Get<IMonsters>();
	}
}
