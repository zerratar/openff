// A run as it stands: what is saved, and what the next roll comes from.
//
// The party's own state - levels, job levels, HP, charges, equipment, spells - is the game's, and is kept
// in the save as each hero's record (Game.Party.Export) with the bag and the gil; everything else of the
// run is here: the seed and the streams' states, the act and the battle within it, the modifiers held,
// the rewards waiting to be chosen, and the run's tallies for its summary.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Rogue.Core
{
	public sealed class OwnedPassive
	{
		public string Id { get; set; }
		public int Stacks { get; set; } = 1;
	}

	/// <summary>One of a victory's choices.</summary>
	public sealed class RewardOption
	{
		/// <summary>"equipment", "spell", "passive", "gil", "heal", "experience".</summary>
		public string Kind { get; set; }
		/// <summary>The item, spell or passive it gives (an id or a passive's word).</summary>
		public string Ref { get; set; }
		public string Name { get; set; }
		/// <summary>What sort of thing it is, as the card says ("Rare weapon", "Passive", "White magic Lv. 1").</summary>
		public string Label { get; set; }
		public string Text { get; set; }
		public string Rarity { get; set; }
		public int Amount { get; set; }
		/// <summary>The hero it goes to (a spell's caster), or -1 for the party.</summary>
		public int Hero { get; set; } = -1;
	}

	public sealed class RunTally
	{
		public int Battles { get; set; }
		public int Elites { get; set; }
		public int Bosses { get; set; }
		public int Kills { get; set; }
		public int GilEarned { get; set; }
		public int Rewards { get; set; }
		public int Rerolls { get; set; }
		public int RareItems { get; set; }
		public string Started { get; set; }
	}

	/// <summary>A battle about to be fought: who, and what kind.</summary>
	public sealed class Encounter
	{
		/// <summary>"battle", "elite", "boss".</summary>
		public string Kind { get; set; } = "battle";
		public string Name { get; set; }
		public List<CountDef> Monsters { get; set; } = new List<CountDef>();
		public double Danger { get; set; }
		public List<string> Elite { get; set; } = new List<string>();
		/// <summary>The monsters' HP and stats, times this.</summary>
		public double Scale { get; set; } = 1;
		public int BattleMap { get; set; } = 1;
	}

	public sealed class RunState
	{
		public int Version { get; set; } = 1;
		public ulong Seed { get; set; }
		public Dictionary<string, ulong> Rng { get; set; } = new Dictionary<string, ulong>();
		/// <summary>The act (0-based) and the battle within it (0-based; the act's Encounters is its boss).</summary>
		public int Act { get; set; }
		public int Step { get; set; }
		/// <summary>The jobs the heroes began with, by word, in hero order.</summary>
		public List<string> Jobs { get; set; } = new List<string>();
		public List<OwnedPassive> Passives { get; set; } = new List<OwnedPassive>();
		/// <summary>A victory's choices not yet taken (the run waits on them), and how many times they were rerolled.</summary>
		public List<RewardOption> Pending { get; set; } = new List<RewardOption>();
		public int Rerolls { get; set; }
		/// <summary>The battle being fought, or about to be.</summary>
		public Encounter Next { get; set; }
		/// <summary>Revivals ("fall" effects) used in the act so far, by passive.</summary>
		public Dictionary<string, int> Used { get; set; } = new Dictionary<string, int>();
		public RunTally Tally { get; set; } = new RunTally();
		/// <summary>"" while it runs; "won" or "lost" once over.</summary>
		public string Outcome { get; set; } = "";
		// What the game keeps, written into the save with the rest.
		public List<string> Heroes { get; set; } = new List<string>();
		public List<int[]> Bag { get; set; } = new List<int[]>();
		public int Gil { get; set; }

		private readonly Dictionary<string, RogueRandom> _streams = new Dictionary<string, RogueRandom>();

		/// <summary>A stream of the run's: its state kept in Rng as it is used.</summary>
		public RogueRandom Stream(string name)
		{
			if (!_streams.TryGetValue(name, out RogueRandom r))
			{
				r = new RogueRandom(Rng.TryGetValue(name, out ulong s) ? s : RogueRandom.Derive(Seed, name));
				_streams[name] = r;
			}
			return r;
		}

		/// <summary>The streams' states into Rng, for the save.</summary>
		public void Settle()
		{
			foreach (KeyValuePair<string, RogueRandom> s in _streams) Rng[s.Key] = s.Value.State;
		}

		public static RunState Begin(ulong seed, IEnumerable<string> jobs, string started)
		{
			RunState r = new RunState { Seed = seed };
			r.Jobs.AddRange(jobs);
			r.Tally.Started = started;
			return r;
		}

		public string ToJson() { Settle(); return JsonSerializer.Serialize(this, RogueContent.Json); }
		public static RunState FromJson(string json) => JsonSerializer.Deserialize<RunState>(json, RogueContent.Json);

		public int StacksOf(string passive) => Passives.FirstOrDefault(p => string.Equals(p.Id, passive, StringComparison.OrdinalIgnoreCase))?.Stacks ?? 0;

		public void Add(string passive)
		{
			OwnedPassive p = Passives.FirstOrDefault(x => string.Equals(x.Id, passive, StringComparison.OrdinalIgnoreCase));
			if (p == null) Passives.Add(new OwnedPassive { Id = passive, Stacks = 1 });
			else p.Stacks++;
		}

		/// <summary>The kind of the step at hand: the act's last is its boss, every EliteEvery-th an elite.</summary>
		public static string KindAt(ActDef act, int step)
		{
			if (act == null) return "battle";
			if (step >= act.Encounters) return "boss";
			if (act.EliteEvery > 0 && (step + 1) % act.EliteEvery == 0) return "elite";
			return "battle";
		}

		/// <summary>A seed from text: a number as it is, anything else hashed.</summary>
		public static ulong SeedOf(string text)
		{
			if (ulong.TryParse(text?.Trim(), out ulong n)) return n;
			return RogueRandom.Derive(0, text ?? "");
		}
	}
}
