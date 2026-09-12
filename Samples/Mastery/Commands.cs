// Battle commands of the Mastery sample's own: a class per command, named on a job ladder
// with "command": true (defs/jobs/samurai.json teaches Zeninage at 50 ABP). Learned and set
// into a free slot, a command shows in the battle's window under its Name and is played as
// the hero's plain attack whose damage the class decides - see OpenFF.Engine/BattleCommands.cs.

using System;
using OpenFF;

namespace Mastery
{
	/// <summary>FF5's Zeninage: throw gil at every enemy - the more thrown, the harder it lands; the party pays.</summary>
	public sealed class Zeninage : BattleCommand
	{
		public override string Name => "Zeninage";
		public override CommandTarget Target => CommandTarget.Enemies;

		private int _thrown;

		public override string Announce(BattleActor actor)
		{
			// Fifty gil a level, as far as the purse goes; taken once per use, before the targets are struck.
			_thrown = Math.Min(Game.Party.Gil, Math.Max(1, actor.Level) * 50);
			Game.Party.Gil -= _thrown;
			return actor.Name + " throws " + _thrown + " gil!";
		}

		public override int Damage(BattleActor actor, BattleActor target, int attackDamage)
		{
			// No miss, no defence: a coin's edge. Nothing to throw: a plain blow.
			if (_thrown <= 0) return attackDamage;
			return Math.Max(1, attackDamage / 2) + _thrown / 4;
		}
	}
}
