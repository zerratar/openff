// Rogue Mode's checks, without the game: the core (Core/) against the mod's own content (rogue/) and a
// made-up slice of FF3's data. Run from the repository:
//
//   dotnet run --project Samples/Rogue/Tests
//
// Each check prints ok or what went wrong; the exit code is the number that failed.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rogue.Core;

internal static class Program
{
	private static int _failed, _passed;

	private static void Check(string name, bool ok, string detail = null)
	{
		if (ok) { _passed++; Console.WriteLine("ok    " + name); }
		else { _failed++; Console.WriteLine("FAIL  " + name + (detail != null ? " - " + detail : "")); }
	}

	private static int Main(string[] args)
	{
		string folder = args.Length > 0 ? args[0] : FindContent();
		RogueContent content = RogueContent.Load(folder);
		Check("content loads without problems", content.Problems.Count == 0, string.Join("; ", content.Problems));
		GameData data = FakeData();

		// Weighted choice: weights hold over many rolls; a zero weight never comes.
		{
			RogueRandom r = new RogueRandom(RogueRandom.Derive(7, "w"));
			var items = new[] { ("a", 1.0), ("b", 3.0), ("z", 0.0) };
			var counts = new Dictionary<string, int> { ["a"] = 0, ["b"] = 0, ["z"] = 0 };
			for (int i = 0; i < 40000; i++) counts[r.WeightedChoose(items, x => x.Item2).Item1]++;
			double ratio = counts["b"] / (double)counts["a"];
			Check("weighted choice follows the weights", ratio > 2.7 && ratio < 3.3 && counts["z"] == 0, "b/a " + ratio.ToString("0.00") + ", z " + counts["z"]);
		}

		// Streams: the same seed and name give the same rolls; another name gives others.
		{
			RogueRandom a = new RogueRandom(RogueRandom.Derive(42, "encounter")), b = new RogueRandom(RogueRandom.Derive(42, "encounter")), c = new RogueRandom(RogueRandom.Derive(42, "reward"));
			var ra = Enumerable.Range(0, 10).Select(_ => a.Next(0, 1000)).ToList();
			var rb = Enumerable.Range(0, 10).Select(_ => b.Next(0, 1000)).ToList();
			var rc = Enumerable.Range(0, 10).Select(_ => c.Next(0, 1000)).ToList();
			Check("a stream is reproducible from its seed", ra.SequenceEqual(rb));
			Check("streams of one seed differ", !ra.SequenceEqual(rc));
		}

		// A whole run's encounters from a seed: the same twice, and within their budgets.
		{
			List<string> One(ulong seed)
			{
				RunState run = RunState.Begin(seed, new[] { "warrior", "monk", "white-mage", "black-mage" }, "test");
				List<string> list = new List<string>();
				for (run.Act = 0; run.Act < content.Run.Acts.Count; run.Act++)
					for (run.Step = 0; run.Step <= content.Run.Acts[run.Act].Encounters; run.Step++)
					{
						Encounter e = EncounterGenerator.Build(content, data, run, run.Stream("encounter"));
						list.Add(e.Kind + ":" + string.Join(",", e.Monsters.Select(m => m.Id + "x" + m.Count)) + ":" + string.Join("+", e.Elite));
					}
				return list;
			}
			Check("a seed gives the same run twice", One(1234).SequenceEqual(One(1234)));
			Check("two seeds give different runs", !One(1234).SequenceEqual(One(98765)));

			bool budgets = true, sizes = true, bosses = true, elites = true;
			string why = null;
			RunState run2 = RunState.Begin(555, new[] { "warrior", "monk", "white-mage", "black-mage" }, "test");
			for (run2.Act = 0; run2.Act < content.Run.Acts.Count; run2.Act++)
			{
				ActDef act = content.Run.Acts[run2.Act];
				for (run2.Step = 0; run2.Step <= act.Encounters; run2.Step++)
				{
					Encounter e = EncounterGenerator.Build(content, data, run2, run2.Stream("encounter"));
					int count = e.Monsters.Sum(m => m.Count);
					if (e.Kind == "boss") { if (count == 0) { bosses = false; why = "act " + act.Id + " boss empty"; } continue; }
					double max = act.BudgetMax + act.BudgetStep * run2.Step;
					// A lone monster may exceed a small budget (the cheapest has to fit somewhere); otherwise within it.
					if (e.Danger > max + 0.001 && e.Monsters.Count > 1) { budgets = false; why = act.Id + " step " + run2.Step + " danger " + e.Danger + " > " + max; }
					if (count < 1 || count > 6) { sizes = false; why = "count " + count; }
					if (e.Kind == "elite" && e.Elite.Count == 0) { elites = false; why = "elite with no modifiers"; }
				}
			}
			Check("battles keep to their danger budget", budgets, why);
			Check("battles field 1..6 monsters", sizes, why);
			Check("every act ends in a boss", bosses, why);
			Check("elite battles carry modifiers", elites, why);
		}

		// Boss placement: the act's last step is its boss, every EliteEvery-th an elite.
		{
			ActDef act = content.Run.Acts[0];
			var kinds = Enumerable.Range(0, act.Encounters + 1).Select(s => RunState.KindAt(act, s)).ToList();
			Check("the boss comes last in an act", kinds.Last() == "boss" && kinds.Count(k => k == "boss") == 1, string.Join(",", kinds));
			Check("elites come every " + act.EliteEvery, kinds.Take(act.Encounters).Select((k, i) => (k == "elite") == ((i + 1) % act.EliteEvery == 0)).All(x => x), string.Join(",", kinds));
		}

		// Rewards: three, of different kinds where possible, the same for a seed, an equipment choice wearable.
		{
			PartyView party = new PartyView { Jobs = { 2, 3, 4, 5 }, Names = { "Luneth", "Arc", "Refia", "Ingus" }, Spells = { new HashSet<int>(), new HashSet<int>(), new HashSet<int>(), new HashSet<int>() } };
			RunState run = RunState.Begin(77, new[] { "warrior", "monk", "white-mage", "black-mage" }, "test");
			var a = RewardGenerator.Roll(content, data, run, party, "battle", new RogueRandom(5));
			var b = RewardGenerator.Roll(content, data, run, party, "battle", new RogueRandom(5));
			Check("a reward roll is reproducible", a.Select(x => x.Kind + x.Ref).SequenceEqual(b.Select(x => x.Kind + x.Ref)));
			Check("a victory offers " + content.Rewards.Choices + " choices", a.Count == content.Rewards.Choices, a.Count + "");
			bool distinct = true, wearable = true, spells = true;
			for (int i = 0; i < 300; i++)
			{
				var roll = RewardGenerator.Roll(content, data, run, party, i % 3 == 0 ? "elite" : "battle", new RogueRandom((ulong)(i + 1)));
				if (roll.Select(x => x.Kind + ":" + x.Ref).Distinct().Count() != roll.Count) distinct = false;
				foreach (RewardOption o in roll)
				{
					if (o.Kind == "spell")
					{
						SpellInfo s = data.Spell(int.Parse(o.Ref));
						if (s == null || o.Hero < 0 || (s.Jobs & (1 << party.Jobs[o.Hero])) == 0 || !content.Run.Acts[0].SpellLevels.Contains(s.Level)) spells = false;
					}
				}
			}
			Check("a victory's choices are all different", distinct);
			Check("a spell goes to a hero whose job holds it", spells);
			// Speculative off: every equipment choice is wearable by a hero.
			double was = content.Rewards.Speculative;
			content.Rewards.Speculative = 0;
			int mask = party.Jobs.Aggregate(0, (m, j) => m | (1 << j));
			for (int i = 0; i < 200; i++)
			{
				RewardOption o = RewardGenerator.Equipment(content, data, run, party, content.Run.Acts[0], "battle", new RogueRandom((ulong)(i + 9)), new List<RewardOption>());
				if (o != null && (data.Item(int.Parse(o.Ref)).Jobs & mask) == 0) wearable = false;
			}
			content.Rewards.Speculative = was;
			Check("equipment choices are wearable unless speculative", wearable);
		}

		// Rerolls: the cost grows by its factor.
		{
			int c0 = RewardGenerator.RerollCost(content, 0), c1 = RewardGenerator.RerollCost(content, 1), c2 = RewardGenerator.RerollCost(content, 2);
			Check("rerolls cost more each time", c0 == content.Rewards.RerollCost && c1 > c0 && c2 > c1, c0 + ", " + c1 + ", " + c2);
		}

		// Unsold gear (price 1) is worth what sold gear as strong costs; past them all, twice the dearest.
		{
			ItemInfo ultima = data.Items.First(i => i.Id == 901), robe = data.Items.First(i => i.Id == 902);
			int u = content.ValueOf(ultima, data), r = content.ValueOf(robe, data);
			Check("unsold gear is valued by its power", u == 50000 && r == 1200 && content.RarityOf(ultima, data) == "legendary", u + ", " + r + ", " + content.RarityOf(ultima, data));
		}

		// Job costs: the default party of the cheapest starting jobs fits, every job has a cost.
		{
			var starting = content.Jobs.Jobs.Where(j => j.Starting).ToList();
			Check("there are starting jobs", starting.Count > 0);
			Check("four of the cheapest fit the budget", starting.Min(j => j.Cost) * 4 <= content.Jobs.Budget);
			Check("not every party fits (the budget means something)", starting.Max(j => j.Cost) * 4 > content.Jobs.Budget);
		}

		// Modifiers: percents add by stacks and filter by what the hit is.
		{
			RunState run = RunState.Begin(1, new[] { "black-mage" }, "test");
			run.Add("fire-mastery"); run.Add("fire-mastery"); run.Add("arcane-knowledge");
			double fire = Modifiers.Percent(content, run, "damage", new Modifiers.Hit { Kind = "magic", Element = "Fire", School = "black" });
			double ice = Modifiers.Percent(content, run, "damage", new Modifiers.Hit { Kind = "magic", Element = "Ice", School = "black" });
			double blow = Modifiers.Percent(content, run, "damage", new Modifiers.Hit { Kind = "physical" });
			Check("fire mastery II and arcane knowledge: fire +50%", Math.Abs(fire - 50) < 0.001, fire + "");
			Check("ice gets arcane knowledge only: +10%", Math.Abs(ice - 10) < 0.001, ice + "");
			Check("a blow gets none of them", blow == 0, blow + "");
			Check("an amount takes its percent", Modifiers.Apply(100, 50) == 150 && Modifiers.Apply(9000, 50) == 9999);
			run.Add("phoenix-blessing");
			var rev = Modifiers.Revival(content, run);
			Check("a revival is there once an act", rev != null && rev.Value.HpPercent == 25);
			run.Used[rev.Value.Passive] = 1;
			Check("and used up after", Modifiers.Revival(content, run) == null);
		}

		// Save and load: the run as JSON and back, the streams going on where they were.
		{
			RunState run = RunState.Begin(2024, new[] { "warrior", "monk", "white-mage", "black-mage" }, "test");
			run.Add("brawler");
			run.Act = 1; run.Step = 2;
			for (int i = 0; i < 5; i++) run.Stream("encounter").Next(0, 100);
			string json = run.ToJson();
			RunState back = RunState.FromJson(json);
			var after = Enumerable.Range(0, 8).Select(_ => run.Stream("encounter").Next(0, 1000)).ToList();
			var afterBack = Enumerable.Range(0, 8).Select(_ => back.Stream("encounter").Next(0, 1000)).ToList();
			Check("a saved run continues its streams exactly", after.SequenceEqual(afterBack));
			Check("a saved run keeps its place and modifiers", back.Act == 1 && back.Step == 2 && back.StacksOf("brawler") == 1 && back.Seed == 2024);
			Check("a seed from text is stable", RunState.SeedOf("dragon") == RunState.SeedOf("dragon") && RunState.SeedOf("123") == 123);
		}

		Console.WriteLine();
		Console.WriteLine(_passed + " passed, " + _failed + " failed");
		return _failed;
	}

	private static string FindContent()
	{
		for (string dir = AppContext.BaseDirectory; dir != null; dir = Path.GetDirectoryName(dir))
		{
			string here = Path.Combine(dir, "rogue");
			if (File.Exists(Path.Combine(here, "acts.json"))) return here;
			string sample = Path.Combine(dir, "Samples", "Rogue", "rogue");
			if (File.Exists(Path.Combine(sample, "acts.json"))) return sample;
		}
		return "rogue";
	}

	/// <summary>FF3's monsters as the biomes name them (the real table's ids, levels and HP), items and spells enough to roll with.</summary>
	private static GameData FakeData()
	{
		GameData d = new GameData();
		var monsters = new (int, string, int, int)[]
		{
			(1, "Goblin", 1, 7), (2, "Carbuncle", 1, 10), (3, "Eye Fang", 1, 11), (4, "Blue Wisp", 1, 14), (5, "Killer Bee", 2, 18), (6, "Werewolf", 3, 24),
			(7, "Berserker", 4, 28), (8, "Red Wisp", 5, 39), (9, "Dark Eye", 5, 43), (10, "Zombie", 6, 47), (11, "Mummy", 6, 52), (12, "Skeleton", 6, 57),
			(13, "Cursed Copper", 6, 42), (14, "Larva", 6, 44), (15, "Shadow", 7, 66), (16, "Revenant", 7, 70), (17, "Firefly", 9, 92), (18, "Helldiver", 8, 120),
			(19, "Rust Bird", 9, 135), (21, "Basilisk", 9, 100), (22, "Bugbear", 9, 110), (23, "Mandrake", 9, 120), (24, "Leprechaun", 9, 142), (26, "Petit", 9, 103),
			(27, "Poison Bat", 9, 98), (28, "Lilliputian", 10, 118), (29, "Wererat", 10, 130), (30, "Blood Worm", 11, 165), (32, "Hermit", 13, 173),
			(36, "Parademon", 16, 245), (38, "Lynx", 16, 265), (39, "Hornet", 15, 260), (40, "Knocker", 13, 131), (41, "Flyer", 12, 139), (42, "Lizardman", 13, 155),
			(43, "Gorgon", 13, 145), (44, "Red Cap", 18, 252), (46, "Slime", 17, 240), (47, "Tarantula", 18, 240), (49, "Pugman", 14, 171), (51, "Blood Bat", 14, 208),
			(52, "Petit Mage", 13, 196), (53, "Fury", 16, 216), (55, "Bomb", 16, 315), (56, "Manticore", 17, 375),
			(196, "Land Turtle", 4, 111), (197, "Djinn", 7, 600), (199, "Giant Rat", 11, 900), (200, "Medusa", 17, 3000)
		};
		foreach (var (id, name, level, hp) in monsters) d.Monsters.Add(new MonsterInfo { Id = id, Name = name, Level = level, MaxHp = hp, Size = id >= 196 ? 1 : 0 });
		int all = (1 << 23) - 1;
		int warriors = (1 << 0) | (1 << 2) | (1 << 8) | (1 << 9) | (1 << 6);
		int casters = (1 << 4) | (1 << 5) | (1 << 6) | (1 << 0);
		int id2 = 1;
		foreach (int price in new[] { 50, 120, 250, 400, 700, 1200, 2000, 3500, 7000, 12000, 25000 })
		{
			d.Items.Add(new ItemInfo { Id = id2++, Name = "Sword " + price, Kind = "weapon", Price = price, Jobs = warriors, Attack = price / 50 + 5 });
			d.Items.Add(new ItemInfo { Id = id2++, Name = "Staff " + price, Kind = "weapon", Price = price, Jobs = casters, Attack = price / 90 + 2 });
			d.Items.Add(new ItemInfo { Id = id2++, Name = "Mail " + price, Kind = "armour", Price = price, Jobs = warriors, Defense = price / 80 + 3 });
			d.Items.Add(new ItemInfo { Id = id2++, Name = "Robe " + price, Kind = "armour", Price = price, Jobs = all, Defense = price / 120 + 1 });
			d.Items.Add(new ItemInfo { Id = id2++, Name = "Claw " + price, Kind = "weapon", Price = price, Jobs = 1 << 3, Attack = price / 60 + 4 });
		}
		d.Items.Add(new ItemInfo { Id = 900, Name = "Potion", Kind = "item", Price = 50 });
		// Gear no shop sells: the game prices it at 1.
		d.Items.Add(new ItemInfo { Id = 901, Name = "Ultima Weapon", Kind = "weapon", Price = 1, Jobs = warriors, Attack = 900 });
		d.Items.Add(new ItemInfo { Id = 902, Name = "Mythril Robe", Kind = "armour", Price = 1, Jobs = all, Defense = 13 });
		int white = (1 << 4) | (1 << 6), black = (1 << 5) | (1 << 6);
		d.Spells.AddRange(new[]
		{
			new SpellInfo { Id = 4001, Name = "Cure", Level = 1, School = "white", Jobs = white, InBattle = true },
			new SpellInfo { Id = 4002, Name = "Poisona", Level = 1, School = "white", Jobs = white, InBattle = true },
			new SpellInfo { Id = 4003, Name = "Sight", Level = 1, School = "white", Jobs = white, InBattle = false },
			new SpellInfo { Id = 4101, Name = "Fire", Level = 1, School = "black", Jobs = black, InBattle = true },
			new SpellInfo { Id = 4102, Name = "Blizzard", Level = 1, School = "black", Jobs = black, InBattle = true },
			new SpellInfo { Id = 4103, Name = "Sleep", Level = 1, School = "black", Jobs = black, InBattle = true },
			new SpellInfo { Id = 4011, Name = "Aero", Level = 2, School = "white", Jobs = white, InBattle = true },
			new SpellInfo { Id = 4111, Name = "Thunder", Level = 2, School = "black", Jobs = black, InBattle = true },
		});
		return d;
	}
}
