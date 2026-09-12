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
using OpenFF;

namespace OpenFF.Client
{
	internal static class BattleSync
	{
		/// <summary>The seed for the next (and the current) battle; null for the game's own rolls.</summary>
		public static int? Seed;
		/// <summary>The round under way, 1 from the first; 0 before.</summary>
		public static int Round;

		private static int? Option => int.TryParse(Options.Get("battle-seed"), out int s) ? s : (int?)null;

		/// <summary>A battle two clients compute together (IBattle.Shared): its seed, whose heroes are whose, the commands' way over the wire.</summary>
		public static SharedBattle Shared;

		/// <summary>The battle is set up (BattleSystem.initialize): the generator is seeded before the monsters' places and the opening are rolled.</summary>
		public static void BattleStarts()
		{
			Round = 0;
			if (Shared != null) Seed = Shared.Seed;
			if (Seed == null) Seed = Option;
			if (Seed == null) return;
			GlobalScope.ds.RandomNumber.seedLogic(Seed.Value);
			Log.Write(LogChannel.General, "battle-sync: seed " + Seed.Value + (Shared != null ? " - shared, remote hero(es) " + string.Join(",", Shared.RemoteHeroes) : ""));
		}

		// ---- the shared battle's commands ----
		//
		// The command phase (BattleSetupPlayer) walks the party: for a hero another client commands, no window opens;
		// the command comes over the wire as the other side decided it, and goes onto the BattlePlayer as the window
		// would have put it. Our own heroes' decisions go out the same way. Both sides then hold the same commands
		// when the round's rolls begin.

		/// <summary>Whether another client commands this hero in the battle under way.</summary>
		public static bool IsRemote(int heroId) => Shared != null && Shared.RemoteHeroes.Contains(heroId);

		/// <summary>The round being planned: the one after the last that rolled.</summary>
		private static int Planning => Round + 1;

		/// <summary>
		/// A remote hero's turn to choose: their command, if it has come, onto the player - true; false while it is awaited
		/// (the caller tries again next frame, the battle's animations going on).
		/// </summary>
		public static bool ApplyRemote(GlobalScope.btl.BattlePlayer player)
		{
			if (Shared?.RemoteCommand == null) return true;
			int hero = player.playerId();
			string text = null;
			try { text = Shared.RemoteCommand(Planning, hero); } catch (Exception ex) { Log.Write(LogChannel.General, "battle-sync: RemoteCommand: " + ex.Message); }
			if (text == null) { Shared.WaitingFor = hero; return false; }
			Shared.WaitingFor = -1;
			try
			{
				// action|actionNumber|flags(targetType)|targets|magic|item
				string[] f = text.Split('|');
				player.setActionId(int.Parse(f[0]));
				player.setActionNumber(int.Parse(f[1]));
				player.clearTargetType();
				int flags = int.Parse(f[2]);
				if ((flags & 1) != 0) player.setFlag(GlobalScope.btl.PLAYER_FLAG.PF_TARGET_PLAYER);
				if ((flags & 2) != 0) player.setFlag(GlobalScope.btl.PLAYER_FLAG.PF_TARGET_MONSTER);
				player.clearTargetId();
				string[] targets = f[3].Split(',', StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < targets.Length && i < 12; i++) player.setTargetId(i, short.Parse(targets[i]));
				player.setUseMagicId(short.Parse(f[4]));
				player.setUseItemId(int.Parse(f[5]));
				player.setLastTargetId();
				Log.Write(LogChannel.File, "battle-sync: round " + Planning + " hero " + hero + " commands from the wire: " + text);
				return true;
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "battle-sync: a remote command could not be read (" + text + "): " + ex.Message); return true; }
		}

		/// <summary>A hero of ours decided (the window closed on a command): the command's text to the mod, for the wire.</summary>
		public static void LocalDecided(GlobalScope.btl.BattlePlayer player)
		{
			if (Shared?.LocalCommand == null) return;
			int hero = player.playerId();
			if (Shared.RemoteHeroes.Contains(hero)) return;
			try
			{
				System.Text.StringBuilder targets = new System.Text.StringBuilder();
				for (int i = 0; i < 12; i++) if (player.targetId(i) >= 0) { if (targets.Length > 0) targets.Append(','); targets.Append(player.targetId(i)); }
				int flags = (player.flag(GlobalScope.btl.PLAYER_FLAG.PF_TARGET_PLAYER) ? 1 : 0) | (player.flag(GlobalScope.btl.PLAYER_FLAG.PF_TARGET_MONSTER) ? 2 : 0);
				string text = player.actionId() + "|" + player.actionNumber() + "|" + flags + "|" + targets + "|" + player.useMagicId() + "|" + player.useItemId();
				Log.Write(LogChannel.File, "battle-sync: round " + Planning + " hero " + hero + " decided: " + text);
				Shared.LocalCommand(Planning, hero, text);
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "battle-sync: LocalCommand: " + ex.Message); }
		}

		/// <summary>The name over "Waiting for ..." for a hero.</summary>
		public static string NameOf(int hero)
		{
			try { string n = Shared?.HeroName?.Invoke(hero); if (!string.IsNullOrEmpty(n)) return n; } catch (Exception) { }
			try { return GlobalScope.pl.PlayerParty.instance().playerForId((byte)hero).name(); } catch (Exception) { return "hero " + hero; }
		}

		/// <summary>A round's rolls begin (BattleSetupEnemy.initialize, every hero's command in): seeded afresh from the seed and the round, then the digest.</summary>
		public static void RoundStarts(GlobalScope.btl.BattleSystem system)
		{
			if (Seed == null) return;
			Round++;
			GlobalScope.ds.RandomNumber.seedLogic(RoundSeed(Seed.Value, Round));
			Log.Write(LogChannel.General, "battle-sync: round " + Round + " " + Digest(system));
		}

		/// <summary>The battle is over: the seed is spent, the shared battle done (a mod's lockstep sets the next).</summary>
		public static void BattleEnds()
		{
			if (Seed != null) Log.Write(LogChannel.General, "battle-sync: over after " + Round + " round(s)");
			Seed = null;
			Round = 0;
			Shared = null;
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
