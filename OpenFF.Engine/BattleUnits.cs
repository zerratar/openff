// Someone in a running battle, as a mod reaches them: a hero or a monster, live.
//
// A BattleUnit is the battle's own state, not a copy: reading Hp reads the battle, setting a
// monster's Attack changes what its next swing does. A monster's numbers are the battle's copy
// of its record, so a change lasts for this battle and never touches the game's table (the next
// battle with the same monster starts from the record again). A hero's stats come from their
// sheet and equipment, so on a hero only Hp (and Revive) take; the stat setters do nothing.
//
// The battle hands them out through IBattle.Units and the battle events (BattleMonstersReady,
// BattleDamage, BattleTurnStarting, BattleUnitFell); one battle keeps one unit per character,
// so a unit can be kept in a dictionary for the battle's length.

namespace OpenFF
{
	/// <summary>Someone in the battle, live: a hero (Member set) or a monster (MonsterId set).</summary>
	public abstract class BattleUnit
	{
		/// <summary>The battle's id for them (0..11).</summary>
		public abstract int Id { get; }
		public abstract bool IsMonster { get; }
		/// <summary>The monster's id in the game's table (Game.Monsters.Find), or -1 for a hero.</summary>
		public abstract int MonsterId { get; }
		/// <summary>The party member, for a hero; null for a monster.</summary>
		public abstract PartyMember Member { get; }
		public abstract string Name { get; }
		/// <summary>Whether they still stand (alive, not stone).</summary>
		public abstract bool Alive { get; }
		public abstract int Hp { get; set; }
		public abstract int MaxHp { get; set; }
		/// <summary>A monster's level (its spells' power and the formulas read it); a hero's is their sheet's.</summary>
		public abstract int Level { get; set; }
		/// <summary>A monster's attack power (its swing's base).</summary>
		public abstract int Attack { get; set; }
		/// <summary>A monster's defence against blows.</summary>
		public abstract int Defense { get; set; }
		/// <summary>A monster's defence against spells.</summary>
		public abstract int MagicDefense { get; set; }
		public abstract int Strength { get; set; }
		public abstract int Agility { get; set; }
		public abstract int Vitality { get; set; }
		public abstract int Intellect { get; set; }
		public abstract int Mind { get; set; }
		/// <summary>Brings them back (or keeps them standing, from BattleUnitFell) with this many hit points.</summary>
		public abstract void Revive(int hp);
		public override string ToString() => Name + " (" + Hp + "/" + MaxHp + ")";
	}

	/// <summary>How a battle a mod starts runs (IBattle.Start).</summary>
	public sealed class BattleOptions
	{
		/// <summary>A wipe ends on the field with BattleEnded { Lost } - the game's own "a battle you may lose" - instead of the game over and the title.</summary>
		public bool LossReturns { get; set; }
	}

	/// <summary>What landed (BattleDamage).</summary>
	public enum DamageKind
	{
		/// <summary>A blow: a plain attack, a monster's swing, a thrown weapon.</summary>
		Physical,
		/// <summary>A spell's damage (or a recovery spell hurting the undead).</summary>
		Magic,
		/// <summary>A recovery spell's healing.</summary>
		Healing,
		/// <summary>An ability's blow: Jump, Dark Wave, and the like.</summary>
		Ability
	}
}
