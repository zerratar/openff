// The run's randomness: one seed, separate streams.
//
// Every roll a run makes comes from a stream of the run's own, named for what it decides - "map",
// "encounter", "reward", "event", "shop", "battle" - each seeded from the run's seed and its name
// (SplitMix64), so a choice made in one place does not shift another: rerolling a reward leaves the
// next encounter as it was. A stream is a xorshift64* state, kept in the run's save (RunState.Rng), so
// a saved run continues exactly as it would have. Nothing in Rogue Mode calls System.Random.

using System;
using System.Collections.Generic;

namespace Rogue.Core
{
	/// <summary>One stream: a 64-bit state stepped by xorshift64*.</summary>
	public sealed class RogueRandom
	{
		public ulong State;

		public RogueRandom(ulong state) { State = state == 0 ? 0x9E3779B97F4A7C15UL : state; }

		/// <summary>A stream's starting state from the run's seed and its name.</summary>
		public static ulong Derive(ulong seed, string name)
		{
			ulong h = 1469598103934665603UL;   // FNV-1a of the name
			foreach (char c in name ?? "") { h ^= c; h *= 1099511628211UL; }
			return SplitMix(seed ^ h);
		}

		public static ulong SplitMix(ulong x)
		{
			x += 0x9E3779B97F4A7C15UL;
			x = (x ^ (x >> 30)) * 0xBF58476D1CE4E5B9UL;
			x = (x ^ (x >> 27)) * 0x94D049BB133111EBUL;
			x ^= x >> 31;
			return x == 0 ? 1 : x;
		}

		public ulong NextULong()
		{
			State ^= State >> 12;
			State ^= State << 25;
			State ^= State >> 27;
			return State * 2685821657736338717UL;
		}

		/// <summary>min..max-1 (min when the range is empty).</summary>
		public int Next(int min, int max)
		{
			if (max <= min) return min;
			return min + (int)(NextULong() % (ulong)(max - min));
		}

		/// <summary>0 (included) .. 1 (excluded).</summary>
		public double NextDouble() => (NextULong() >> 11) * (1.0 / (1UL << 53));

		/// <summary>True with this probability (0..1).</summary>
		public bool Chance(double p) => NextDouble() < p;

		public T Choose<T>(IReadOnlyList<T> items) => items == null || items.Count == 0 ? default : items[Next(0, items.Count)];

		/// <summary>One of the items, each as likely as its weight (none of weight 0); default when all weigh nothing.</summary>
		public T WeightedChoose<T>(IReadOnlyList<T> items, Func<T, double> weight)
		{
			if (items == null || items.Count == 0) return default;
			double total = 0;
			foreach (T it in items) total += Math.Max(0, weight(it));
			if (total <= 0) return default;
			double roll = NextDouble() * total;
			foreach (T it in items)
			{
				double w = Math.Max(0, weight(it));
				if (w <= 0) continue;
				if (roll < w) return it;
				roll -= w;
			}
			for (int i = items.Count - 1; i >= 0; i--) if (weight(items[i]) > 0) return items[i];
			return default;
		}

		/// <summary>The list in a shuffled order (Fisher-Yates), a new list.</summary>
		public List<T> Shuffled<T>(IEnumerable<T> items)
		{
			List<T> list = new List<T>(items);
			for (int i = list.Count - 1; i > 0; i--) { int j = Next(0, i + 1); (list[i], list[j]) = (list[j], list[i]); }
			return list;
		}
	}
}
