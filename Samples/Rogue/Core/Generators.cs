// What the run rolls: the next battle (from its biome's pool, inside a danger budget) and a victory's
// choices (equipment, spells, modifiers, gil, healing, experience).
//
// Both read only the content, the game's data as GameData, the party as PartyView, and one of the run's
// streams - so for a seed and a content they always give the same.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Rogue.Core
{
	/// <summary>The party as the reward generator needs it.</summary>
	public sealed class PartyView
	{
		/// <summary>Each hero's job index, in hero order.</summary>
		public List<int> Jobs = new List<int>();
		/// <summary>Each hero's spells known.</summary>
		public List<HashSet<int>> Spells = new List<HashSet<int>>();
		/// <summary>Items the party has (bag and worn).</summary>
		public HashSet<int> Owned = new HashSet<int>();
		public List<string> Names = new List<string>();
		/// <summary>Whether a hero has a free slot for a spell of that level.</summary>
		public Func<int, int, bool> HasRoomFor = (hero, level) => true;
	}

	public static class EncounterGenerator
	{
		/// <summary>The battle at a step of an act.</summary>
		public static Encounter Build(RogueContent content, GameData data, RunState run, RogueRandom rng)
		{
			ActDef act = content.Act(run.Act) ?? new ActDef();
			BiomeDef biome = content.BiomeOf(act) ?? new BiomeDef();
			string kind = RunState.KindAt(act, run.Step);
			Encounter e = new Encounter { Kind = kind, BattleMap = biome.BattleMap, Scale = act.Scale + act.ScaleStep * run.Step };
			if (kind == "boss")
			{
				BossDef boss = rng.WeightedChoose(biome.Bosses, b => b.Weight) ?? biome.Bosses.FirstOrDefault();
				if (boss != null)
				{
					e.Name = boss.Name;
					e.Monsters.AddRange(boss.Monsters.Select(m => new CountDef { Id = m.Id, Count = Math.Max(1, m.Count) }));
					e.Danger = boss.Monsters.Sum(m => content.Danger(data.Monster(m.Id)) * m.Count);
				}
				return e;
			}
			double budget = act.BudgetMin + act.BudgetStep * run.Step;
			double budgetMax = act.BudgetMax + act.BudgetStep * run.Step;
			Fill(e, content, data, biome, rng, budget, budgetMax);
			if (kind == "elite")
			{
				int n = rng.Next(content.Elites.Min, content.Elites.Max + 1);
				List<EliteDef> left = new List<EliteDef>(content.Elites.Modifiers);
				for (int i = 0; i < n && left.Count > 0; i++)
				{
					EliteDef pick = rng.WeightedChoose(left, x => x.Weight);
					if (pick == null) break;
					e.Elite.Add(pick.Id);
					left.Remove(pick);
				}
			}
			e.Name = string.Join(", ", e.Monsters.Select(m => (data.Monster(m.Id)?.Name ?? ("#" + m.Id)) + (m.Count > 1 ? " x" + m.Count : "")));
			return e;
		}

		/// <summary>Monsters from the pool until the danger is within the budget: one to three kinds, six at most (three with a large one).</summary>
		public static void Fill(Encounter e, RogueContent content, GameData data, BiomeDef biome, RogueRandom rng, double budget, double budgetMax)
		{
			var pool = biome.Monsters.Select(p => (Entry: p, Info: data.Monster(p.Id))).Where(p => p.Info != null).ToList();
			if (pool.Count == 0) return;
			double target = budget + rng.NextDouble() * Math.Max(0, budgetMax - budget);
			// The kinds: those that fit the budget alone, one to three of them.
			var fit = pool.Where(p => content.Danger(p.Info, p.Entry) <= Math.Max(target, 1)).ToList();
			if (fit.Count == 0) fit = new List<(PoolEntry, MonsterInfo)> { pool.OrderBy(p => content.Danger(p.Info, p.Entry)).First() };
			int kinds = Math.Min(fit.Count, rng.Next(1, 4));
			var chosen = new List<(PoolEntry Entry, MonsterInfo Info)>();
			for (int i = 0; i < kinds; i++)
			{
				var left = fit.Where(f => !chosen.Any(c => c.Entry.Id == f.Entry.Id)).ToList();
				var one = rng.WeightedChoose(left, f => f.Entry.Weight);
				if (one.Entry == null) break;
				chosen.Add(one);
			}
			double total = 0;
			var counts = chosen.ToDictionary(c => c.Entry.Id, c => 0);
			int limit = 6;
			for (int guard = 0; guard < 24; guard++)
			{
				var can = chosen.Where(c => counts[c.Entry.Id] < c.Entry.Max && total + content.Danger(c.Info, c.Entry) <= Math.Max(target, 1) + 0.001).ToList();
				int all = counts.Values.Sum();
				bool large = chosen.Any(c => counts[c.Entry.Id] > 0 && c.Info.Size > 0);
				if (all >= (large ? 3 : limit)) break;
				// Every kind chosen appears at least once before any has two.
				var unseen = can.Where(c => counts[c.Entry.Id] == 0).ToList();
				var pick = unseen.Count > 0 ? unseen[0] : rng.WeightedChoose(can, c => c.Entry.Weight);
				if (pick.Entry == null) break;
				if (pick.Info.Size > 0 && all >= 3) break;
				counts[pick.Entry.Id]++;
				total += content.Danger(pick.Info, pick.Entry);
			}
			if (counts.Values.Sum() == 0) counts[chosen[0].Entry.Id] = 1;
			foreach (var c in chosen) if (counts[c.Entry.Id] > 0) e.Monsters.Add(new CountDef { Id = c.Entry.Id, Count = counts[c.Entry.Id] });
			e.Danger = e.Monsters.Sum(m => content.Danger(data.Monster(m.Id), biome.Monsters.First(p => p.Id == m.Id)) * m.Count);
		}
	}

	public static class RewardGenerator
	{
		/// <summary>A victory's choices: Choices of them, of different kinds where it can.</summary>
		public static List<RewardOption> Roll(RogueContent content, GameData data, RunState run, PartyView party, string nodeKind, RogueRandom rng)
		{
			ActDef act = content.Act(Math.Min(run.Act, content.Run.Acts.Count - 1)) ?? new ActDef();
			if (!content.Rewards.Kinds.TryGetValue(nodeKind ?? "battle", out Dictionary<string, double> kinds) && !content.Rewards.Kinds.TryGetValue("battle", out kinds))
				kinds = new Dictionary<string, double> { ["equipment"] = 1, ["spell"] = 1, ["passive"] = 1, ["gil"] = 1 };
			List<RewardOption> picked = new List<RewardOption>();
			List<string> left = kinds.Where(k => k.Value > 0).Select(k => k.Key).ToList();
			int tries = 0;
			while (picked.Count < Math.Max(1, content.Rewards.Choices) && tries++ < 40)
			{
				if (left.Count == 0) left = kinds.Where(k => k.Value > 0).Select(k => k.Key).ToList();
				string kind = rng.WeightedChoose(left, k => kinds[k]);
				if (kind == null) break;
				RewardOption o = Make(kind, content, data, run, party, act, nodeKind, rng, picked);
				if (o == null) { left.Remove(kind); continue; }
				picked.Add(o);
				// A second of the same kind only once the others have had a turn.
				left.Remove(kind);
			}
			return picked;
		}

		private static RewardOption Make(string kind, RogueContent content, GameData data, RunState run, PartyView party, ActDef act, string nodeKind, RogueRandom rng, List<RewardOption> already)
		{
			switch (kind)
			{
				case "equipment": return Equipment(content, data, run, party, act, nodeKind, rng, already);
				case "spell": return Spell(data, party, act, rng, already);
				case "passive": return Passive(content, run, rng, already);
				case "gil":
				{
					int gil = (int)Math.Round(act.Gil * (0.75 + rng.NextDouble() * 0.5) * (nodeKind == "boss" ? 3 : nodeKind == "elite" ? 1.5 : 1));
					return new RewardOption { Kind = "gil", Name = gil + " Gil", Label = "Gil", Text = "Gil to spend on rerolls.", Amount = gil, Rarity = "common" };
				}
				case "heal": return new RewardOption { Kind = "heal", Name = "Rest", Label = "Recovery", Text = "The party's HP and magic charges all restored, the fallen raised.", Rarity = "common" };
				case "experience":
				{
					int exp = (int)Math.Round(act.Exp * (nodeKind == "boss" ? 3 : nodeKind == "elite" ? 1.5 : 1));
					return new RewardOption { Kind = "experience", Name = "Battle Lore", Label = "Experience", Text = "Every hero gains " + exp + " experience.", Amount = exp, Rarity = "common" };
				}
				default: return null;
			}
		}

		/// <summary>A piece of equipment of a rarity rolled from the act's weights (an elite's and a boss's a step and two better), one a hero may wear (now and then one nobody can).</summary>
		public static RewardOption Equipment(RogueContent content, GameData data, RunState run, PartyView party, ActDef act, string nodeKind, RogueRandom rng, List<RewardOption> already)
		{
			List<string> names = content.Rewards.Rarities.Select(r => r.Name).ToList();
			int rolls = 1 + Modifiers.Count(content, run, "rarity");
			int best = 0;
			for (int i = 0; i < rolls; i++)
			{
				string r = rng.WeightedChoose(names, n => act.Rarity.TryGetValue(n, out double w) ? w : 0) ?? names[0];
				best = Math.Max(best, content.RarityIndex(r));
			}
			if (nodeKind == "elite") best++;
			if (nodeKind == "boss") best += 2;
			best = Math.Min(best, names.Count - 1);
			int jobMask = party.Jobs.Aggregate(0, (m, j) => m | (1 << j));
			bool speculative = rng.Chance(content.Rewards.Speculative);
			var gear = data.Items.Where(i => i.Kind != "item" && i.Kind != "other" && i.Jobs != 0 && i.Price > 0
				&& !content.Rewards.Exclude.Contains(i.Id) && !already.Any(a => a.Kind == "equipment" && a.Ref == i.Id.ToString())).ToList();
			// From the rarity rolled down to the commonest: the first that has something.
			for (int r = best; r >= 0; r--)
			{
				var at = gear.Where(i => content.RarityIndex(content.RarityOf(i, data)) == r && ((i.Jobs & jobMask) != 0) != speculative && !party.Owned.Contains(i.Id)).ToList();
				if (at.Count == 0) at = gear.Where(i => content.RarityIndex(content.RarityOf(i, data)) == r && (i.Jobs & jobMask) != 0).ToList();
				if (at.Count == 0) continue;
				ItemInfo item = rng.Choose(at);
				string rarity = content.RarityOf(item, data);
				string who = string.Join(", ", party.Jobs.Select((j, h) => (j, h)).Where(x => (item.Jobs & (1 << x.j)) != 0).Select(x => party.Names.ElementAtOrDefault(x.h) ?? ("hero " + x.h)));
				return new RewardOption
				{
					Kind = "equipment", Ref = item.Id.ToString(), Name = item.Name, Rarity = rarity,
					Label = Title(rarity) + " " + item.Kind,
					Text = Stat(item) + (who.Length > 0 ? "  For " + who + "." : "  Nobody's job wears it now."),
				};
			}
			return null;
		}

		private static string Stat(ItemInfo i) => i.Kind == "weapon" ? "Attack " + i.Attack + "." : "Defense " + i.Defense + ".";

		/// <summary>A spell of the act's levels that a hero's job may hold and does not know yet; it goes to that hero.</summary>
		public static RewardOption Spell(GameData data, PartyView party, ActDef act, RogueRandom rng, List<RewardOption> already)
		{
			var options = new List<(SpellInfo Spell, int Hero)>();
			foreach (SpellInfo s in data.Spells.Where(s => s.InBattle && act.SpellLevels.Contains(s.Level) && !already.Any(a => a.Kind == "spell" && a.Ref == s.Id.ToString())))
			{
				for (int h = 0; h < party.Jobs.Count; h++)
				{
					if ((s.Jobs & (1 << party.Jobs[h])) == 0) continue;
					if (party.Spells.ElementAtOrDefault(h)?.Contains(s.Id) == true) continue;
					if (!party.HasRoomFor(h, s.Level)) continue;
					options.Add((s, h));
				}
			}
			if (options.Count == 0) return null;
			var pick = rng.Choose(options);
			string who = party.Names.ElementAtOrDefault(pick.Hero) ?? ("hero " + pick.Hero);
			return new RewardOption
			{
				Kind = "spell", Ref = pick.Spell.Id.ToString(), Hero = pick.Hero, Name = pick.Spell.Name, Rarity = pick.Spell.Level >= 3 ? "uncommon" : "common",
				Label = Title(pick.Spell.School) + " magic Lv. " + pick.Spell.Level,
				Text = (pick.Spell.Caption ?? "").Trim() + "  " + who + " learns it."
			};
		}

		/// <summary>A modifier not yet held to its most, weighted by its own weight.</summary>
		public static RewardOption Passive(RogueContent content, RunState run, RogueRandom rng, List<RewardOption> already)
		{
			var can = content.Passives.Where(p => run.StacksOf(p.Id) < p.MaxStacks && !already.Any(a => a.Kind == "passive" && a.Ref == p.Id)).ToList();
			PassiveDef pick = rng.WeightedChoose(can, p => p.Weight);
			if (pick == null) return null;
			int stacks = run.StacksOf(pick.Id) + 1;
			return new RewardOption
			{
				Kind = "passive", Ref = pick.Id, Rarity = pick.Rarity,
				Name = pick.Name + (pick.MaxStacks > 1 ? " " + Roman(stacks) : ""),
				Label = "Passive", Text = Modifiers.Describe(pick, stacks)
			};
		}

		public static string Title(string s) => string.IsNullOrEmpty(s) ? "" : char.ToUpperInvariant(s[0]) + s.Substring(1);
		public static string Roman(int n) => n switch { 1 => "I", 2 => "II", 3 => "III", 4 => "IV", 5 => "V", _ => n.ToString() };

		/// <summary>A reroll's cost: the base, times the growth for each reroll at this reward.</summary>
		public static int RerollCost(RogueContent content, int rerolls) => (int)Math.Round(content.Rewards.RerollCost * Math.Pow(Math.Max(1, content.Rewards.RerollGrowth), rerolls));
	}

	/// <summary>The modifiers held, worked out for a moment of a battle.</summary>
	public static class Modifiers
	{
		/// <summary>What a hit is, for the "damage", "taken" and "healing" effects.</summary>
		public sealed class Hit
		{
			/// <summary>"physical", "magic", "ability", "healing".</summary>
			public string Kind;
			public string Element;
			public string School;
			public bool Critical;
			public bool Jump;
			/// <summary>The attacker's HP left, percent.</summary>
			public double AttackerHp = 100;
			public bool TargetWeak;
		}

		private static IEnumerable<(Effect Effect, int Stacks, PassiveDef Def)> Effects(RogueContent content, RunState run, string on)
		{
			foreach (OwnedPassive o in run.Passives)
			{
				PassiveDef d = content.Passive(o.Id);
				if (d == null) continue;
				foreach (Effect e in d.Effects) if (string.Equals(e.On, on, StringComparison.OrdinalIgnoreCase)) yield return (e, o.Stacks, d);
			}
		}

		private static bool Matches(Effect e, Hit h)
		{
			if (!string.IsNullOrEmpty(e.Kind) && e.Kind != "any" && !string.Equals(e.Kind, h.Kind, StringComparison.OrdinalIgnoreCase)) return false;
			if (!string.IsNullOrEmpty(e.Element) && (h.Element == null || h.Element.IndexOf(e.Element, StringComparison.OrdinalIgnoreCase) < 0)) return false;
			if (!string.IsNullOrEmpty(e.School) && !string.Equals(e.School, h.School, StringComparison.OrdinalIgnoreCase)) return false;
			if (e.Critical == true && !h.Critical) return false;
			if (e.Jump == true && !h.Jump) return false;
			if (e.BelowHp != null && h.AttackerHp >= e.BelowHp.Value) return false;
			return true;
		}

		/// <summary>The percent more an "on" effect gives a hit (damage dealt, damage taken, healing).</summary>
		public static double Percent(RogueContent content, RunState run, string on, Hit hit) =>
			Effects(content, run, on).Where(x => Matches(x.Effect, hit)).Sum(x => x.Effect.Percent * x.Stacks);

		/// <summary>An amount with the percent more on it, rounded, 0..9999.</summary>
		public static int Apply(int amount, double percent) => Math.Max(0, Math.Min(9999, (int)Math.Round(amount * (1 + percent / 100.0))));

		public static double Sum(RogueContent content, RunState run, string on, Func<Effect, double> field) =>
			Effects(content, run, on).Sum(x => field(x.Effect) * x.Stacks);

		public static int Count(RogueContent content, RunState run, string on) => (int)Effects(content, run, on).Sum(x => Math.Max(1, x.Effect.Count) * x.Stacks);

		/// <summary>A "fall" effect with a revival left this act: its passive's id and the HP percent; null when none.</summary>
		public static (string Passive, double HpPercent)? Revival(RogueContent content, RunState run)
		{
			foreach (var x in Effects(content, run, "fall"))
			{
				int allowed = Math.Max(1, x.Effect.Count) * x.Stacks;
				run.Used.TryGetValue(x.Def.Id, out int used);
				if (used < allowed) return (x.Def.Id, x.Effect.HpPercent <= 0 ? 25 : x.Effect.HpPercent);
			}
			return null;
		}

		/// <summary>A modifier's description at a number of stacks: {p} its first effect's percent, {n} its count.</summary>
		public static string Describe(PassiveDef p, int stacks)
		{
			Effect e = p.Effects.FirstOrDefault();
			string text = p.Text ?? p.Name;
			if (e == null) return text;
			double pct = e.Percent != 0 ? e.Percent : e.HealPercent != 0 ? e.HealPercent : e.GilPercent != 0 ? e.GilPercent : e.ExpPercent != 0 ? e.ExpPercent : e.HpPercent;
			int n = Math.Max(1, e.Count) * stacks;
			// {p} the percent, {n} the count, {s} an "s" when the count is not 1 ("{n} more time{s}").
			return text.Replace("{p}", (pct * (e.On == "fall" ? 1 : stacks)).ToString("0.#")).Replace("{n}", n.ToString()).Replace("{s}", n == 1 ? "" : "s");
		}
	}
}
