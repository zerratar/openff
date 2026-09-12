// Two clients computing one battle.
//
// FF3's battle is rounds: everyone picks a command, then the round plays out. That makes it
// fit for lockstep - both clients run the whole battle themselves, each choosing for its own
// heroes, the choices crossing the wire, and the rounds coming out the same on both because
// the rules roll on a generator seeded alike (ds.RandomNumber.logic32, apart from the effects'
// and camera's rolls). This class is the seeding and the proof: with a seed, the generator is
// set at the battle's start and again at the start of every round (so nothing consumed
// between rounds - a menu open longer on one side - can drift them apart), and each round
// starts with a digest line in the log that two clients can be compared by.
//
// --battle-seed=<n> seeds every battle so, for testing; a mod's lockstep sets Seed for one.

using System;
using System.Text;

namespace OpenFF.Client
{
	internal static class BattleSync
	{
		/// <summary>The seed for the next (and the current) battle; null for the game's own rolls.</summary>
		public static int? Seed;
		/// <summary>The round under way, 1 from the first; 0 before.</summary>
		public static int Round;

		private static int? Option => int.TryParse(Options.Get("battle-seed"), out int s) ? s : (int?)null;

		/// <summary>The battle is set up (BattleSystem.initialize): the generator is seeded before the monsters' places and the opening are rolled.</summary>
		public static void BattleStarts()
		{
			Round = 0;
			if (Seed == null) Seed = Option;
			if (Seed == null) return;
			GlobalScope.ds.RandomNumber.seedLogic(Seed.Value);
			Log.Write(LogChannel.General, "battle-sync: seed " + Seed.Value);
		}

		/// <summary>A round's rolls begin (BattleSetupEnemy.initialize, every hero's command in): seeded afresh from the seed and the round, then the digest.</summary>
		public static void RoundStarts(GlobalScope.btl.BattleSystem system)
		{
			if (Seed == null) return;
			Round++;
			GlobalScope.ds.RandomNumber.seedLogic(RoundSeed(Seed.Value, Round));
			Log.Write(LogChannel.General, "battle-sync: round " + Round + " " + Digest(system));
		}

		/// <summary>The battle is over: the seed is spent (a mod's lockstep sets the next).</summary>
		public static void BattleEnds()
		{
			if (Seed != null) Log.Write(LogChannel.General, "battle-sync: over after " + Round + " round(s)");
			Seed = null;
			Round = 0;
		}

		public static int RoundSeed(int seed, int round)
		{
			unchecked { return seed * 1000003 + round * 7919; }
		}

		/// <summary>Everyone's HP as "Luneth 32/32 Arc 28/30 | m0 12 m1 0": the same on both clients when the round before came out the same.</summary>
		public static string Digest(GlobalScope.btl.BattleSystem system)
		{
			try
			{
				StringBuilder sb = new StringBuilder();
				GlobalScope.btl.BattleParty party = system.characterManager().playerParty();
				for (int i = 0; i < 4; i++)
				{
					GlobalScope.btl.BattlePlayer p = party.battlePlayer(i);
					if (p == null || !p.isEnable()) continue;
					sb.Append(p.player().name()).Append(' ').Append(p.hp().getNow()).Append('/').Append(p.hp().getLimit()).Append(' ');
				}
				sb.Append('|');
				GlobalScope.btl.BattleMonsterParty monsters = system.characterManager().monsterParty();
				for (int i = 0; i < 6; i++)
				{
					GlobalScope.btl.BattleMonster m = monsters.battleMonster(i);
					if (m == null || !m.isEnable()) continue;
					sb.Append(" m").Append(i).Append(' ').Append(m.hp().getNow());
				}
				return sb.ToString();
			}
			catch (Exception ex) { return "(no digest: " + ex.Message + ")"; }
		}
	}
}
