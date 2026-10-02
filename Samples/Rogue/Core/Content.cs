// Rogue Mode's content: what the JSON beside the mod (data/*.json) says, and the slice of FF3's own data
// the generators read.
//
// The content never copies FF3: a monster, an item, a spell or a job is named by the game's id or word,
// and everything else about it is the game's (Game.Monsters, Game.Items, Game.Magic). What the files add is
// only Rogue Mode's: which monsters a biome fields and how dangerous they count, how much a job costs to
// start with, how rare an item counts (from its own price unless said), the modifiers and the elites.
//
// The game's data comes in as GameData - plain lists filled by the engine side (RogueService) or by the
// checks (Tests/), so nothing here needs the game running.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rogue.Core
{
	// ---------------------------------------------------------------- FF3's data, as the generators see it

	public sealed class MonsterInfo
	{
		public int Id;
		public string Name;
		public int Level;
		public int MaxHp;
		/// <summary>0 small (six fit), 1 and 2 large (three fit) - the battle's own limits.</summary>
		public int Size;
	}

	public sealed class ItemInfo
	{
		public int Id;
		public string Name;
		/// <summary>"weapon", "shield", "helmet", "armour", "gloves", "item" or "other".</summary>
		public string Kind;
		public int Price;
		/// <summary>The jobs that may equip it, a bitmask over the game's job index.</summary>
		public int Jobs;
		public int Attack;
		public int Defense;
		public string Caption;
	}

	public sealed class SpellInfo
	{
		public int Id;
		public string Name;
		public int Level;
		/// <summary>"white", "black", "summon".</summary>
		public string School;
		public int Jobs;
		public string Caption;
		public bool InBattle;
	}

	public sealed class GameData
	{
		public List<MonsterInfo> Monsters = new List<MonsterInfo>();
		public List<ItemInfo> Items = new List<ItemInfo>();
		public List<SpellInfo> Spells = new List<SpellInfo>();
		public MonsterInfo Monster(int id) => Monsters.FirstOrDefault(m => m.Id == id);
		public ItemInfo Item(int id) => Items.FirstOrDefault(i => i.Id == id);
		public SpellInfo Spell(int id) => Spells.FirstOrDefault(s => s.Id == id);
	}

	// ---------------------------------------------------------------- the content files

	/// <summary>data/acts.json: the run's shape.</summary>
	public sealed class RunConfig
	{
		/// <summary>The heroes' level when a run begins.</summary>
		public int StartLevel { get; set; } = 3;
		/// <summary>A monster's danger when its biome does not say: Level x LevelWeight + MaxHp / HpPer, at least 1.</summary>
		public double LevelWeight { get; set; } = 0.5;
		public double HpPer { get; set; } = 25;
		/// <summary>A battle's experience, times this: FF3's early monsters give little, and a run is a few dozen battles, not the game's hundreds.</summary>
		public double ExpScale { get; set; } = 1;
		public List<ActDef> Acts { get; set; } = new List<ActDef>();
	}

	public sealed class ActDef
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Biome { get; set; }
		/// <summary>Battles in the act before its boss.</summary>
		public int Encounters { get; set; } = 6;
		/// <summary>The level the party is raised to (those below it) as the act begins; 0 for none. Keeps an act's monsters in reach.</summary>
		public int Level { get; set; }
		/// <summary>Every this many battles an elite (0: none).</summary>
		public int EliteEvery { get; set; } = 3;
		/// <summary>The danger a battle may hold: Min..Max at the act's first, each later one Step more.</summary>
		public double BudgetMin { get; set; } = 3;
		public double BudgetMax { get; set; } = 5;
		public double BudgetStep { get; set; } = 1;
		/// <summary>The act's monsters' HP and stats, times this (1 = as the game has them) - and each battle into the act this much more.</summary>
		public double Scale { get; set; } = 1;
		public double ScaleStep { get; set; } = 0;
		/// <summary>The magic levels its spell rewards are drawn from.</summary>
		public List<int> SpellLevels { get; set; } = new List<int> { 1 };
		/// <summary>The item rarities its rewards favour: a weight per rarity name (rewards.json's).</summary>
		public Dictionary<string, double> Rarity { get; set; } = new Dictionary<string, double>();
		/// <summary>Gil a "gil" reward gives, about.</summary>
		public int Gil { get; set; } = 150;
		/// <summary>Experience an "experience" reward gives each hero.</summary>
		public int Exp { get; set; } = 60;
		/// <summary>The crystal the act's boss gives, or null: its jobs open, and with them the menu's Job.</summary>
		public CrystalDef Crystal { get; set; }
	}

	/// <summary>A crystal: FF3's jobs it opens, by the game's job number (2 Warrior ... 22 Ninja), and how the camp says so.</summary>
	public sealed class CrystalDef
	{
		public string Name { get; set; }
		public List<int> Jobs { get; set; } = new List<int>();
		/// <summary>The jobs in words, for the camp ("Knight, Thief, Scholar and Geomancer").</summary>
		public string Text { get; set; }
	}

	/// <summary>data/biomes/*.json: where a stretch of the run takes place.</summary>
	public sealed class BiomeDef
	{
		public string Id { get; set; }
		public string Name { get; set; }
		/// <summary>FF3's battlefield (1..43) its battles are fought on.</summary>
		public int BattleMap { get; set; } = 1;
		/// <summary>The battle background the camp stands on between battles (a map: "b01").</summary>
		public string Backdrop { get; set; } = "b01";
		/// <summary>The screens' backdrop for the act (menus/styles: "cave", "woods", "tower").</summary>
		public string Look { get; set; } = "cave";
		public List<PoolEntry> Monsters { get; set; } = new List<PoolEntry>();
		public List<BossDef> Bosses { get; set; } = new List<BossDef>();
	}

	public sealed class PoolEntry
	{
		/// <summary>A monster by its id in the game's table.</summary>
		public int Id { get; set; }
		public double Weight { get; set; } = 1;
		/// <summary>Its danger, when the default from its record is not right.</summary>
		public double? Danger { get; set; }
		/// <summary>The most of it in one battle.</summary>
		public int Max { get; set; } = 6;
	}

	public sealed class BossDef
	{
		public string Name { get; set; }
		public double Weight { get; set; } = 1;
		/// <summary>The monsters, by id and how many.</summary>
		public List<CountDef> Monsters { get; set; } = new List<CountDef>();
	}

	public sealed class CountDef
	{
		public int Id { get; set; }
		public int Count { get; set; } = 1;
	}

	/// <summary>data/jobs.json: the starting budget and the jobs' costs.</summary>
	public sealed class JobsConfig
	{
		public int Budget { get; set; } = 10;
		/// <summary>A job without a kit starts with the strongest weapon and body armour it may wear that a shop sells for at most this.</summary>
		public int KitPrice { get; set; } = 100;
		/// <summary>Item ids no kit takes, as [first, last] ranges (the game's arrows, 1500-1599: no bow comes with them).</summary>
		public List<int[]> KitSkip { get; set; } = new List<int[]>();
		public List<JobDef> Jobs { get; set; } = new List<JobDef>();

		public bool Skipped(int item) => KitSkip.Any(r => r != null && r.Length >= 2 && item >= r[0] && item <= r[1]);
	}

	public sealed class JobDef
	{
		/// <summary>The game's job by word ("warrior", "black-mage").</summary>
		public string Word { get; set; }
		/// <summary>The game's job index (the API's Job: 0 Freelancer, 2 Warrior ...).</summary>
		public int Job { get; set; }
		public string Name { get; set; }
		public int Cost { get; set; } = 2;
		/// <summary>Whether a run may begin with it (otherwise a crystal opens it).</summary>
		public bool Starting { get; set; } = true;
		/// <summary>Its starting equipment by item id; empty: the cheapest weapon and body armour it may wear.</summary>
		public List<int> Kit { get; set; } = new List<int>();
		/// <summary>Its starting spells by id; empty: as many level-1 spells of its school as Spells says.</summary>
		public List<int> Spells { get; set; }
		public int SpellCount { get; set; } = 0;
	}

	/// <summary>data/rewards.json: how a victory's choices are made.</summary>
	public sealed class RewardsConfig
	{
		public int Choices { get; set; } = 3;
		public int RerollCost { get; set; } = 200;
		/// <summary>Each reroll at one reward costs this many times the one before.</summary>
		public double RerollGrowth { get; set; } = 2;
		/// <summary>How likely each kind of reward is, by node kind ("battle", "elite", "boss").</summary>
		public Dictionary<string, Dictionary<string, double>> Kinds { get; set; } = new Dictionary<string, Dictionary<string, double>>();
		/// <summary>The rarities, commonest first: an item is the last whose MinPrice its price reaches.</summary>
		public List<RarityDef> Rarities { get; set; } = new List<RarityDef>();
		/// <summary>The chance an equipment choice is one no hero can wear now (for a job change later).</summary>
		public double Speculative { get; set; } = 0.15;
		/// <summary>Items left out of the rewards by id (a key item, a broken one).</summary>
		public List<int> Exclude { get; set; } = new List<int>();
		/// <summary>An item's rarity when its price says wrong, by id.</summary>
		public Dictionary<string, string> RarityOf { get; set; } = new Dictionary<string, string>();
	}

	public sealed class RarityDef
	{
		public string Name { get; set; }
		public int MinPrice { get; set; }
	}

	/// <summary>data/passives.json: the run's modifiers, each made of effect primitives (Effect).</summary>
	public sealed class PassiveDef
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Rarity { get; set; } = "common";
		public int MaxStacks { get; set; } = 1;
		public double Weight { get; set; } = 1;
		/// <summary>Its description; {p} is the first effect's percent at the stacks held, {n} its count.</summary>
		public string Text { get; set; }
		public List<Effect> Effects { get; set; } = new List<Effect>();
	}

	/// <summary>
	/// One primitive of a modifier. On says when it applies:
	///   "damage"   - a hit, spell or ability a hero deals: Percent more (filters below);
	///   "taken"    - damage a hero takes: Percent more (negative: less);
	///   "healing"  - healing a hero's spell does: Percent more;
	///   "kill"     - a hero downs a monster: the hero heals HealPercent of their HP;
	///   "fall"     - a hero falls: revived with HpPercent, Count times an act;
	///   "rewards"  - after a victory: GilPercent / ExpPercent more;
	///   "rarity"   - item rewards roll their rarity Count more times, keeping the best;
	///   "start"    - a battle begins: the heroes heal HealPercent of their HP.
	/// Filters for damage: Kind (physical, magic, ability, any), Element (fire, ice...), School (white, black,
	/// summon), Critical, Jump, BelowHp (the attacker under this percent of their HP), Weak (the target weak to it).
	/// </summary>
	public sealed class Effect
	{
		public string On { get; set; }
		public string Kind { get; set; }
		public string Element { get; set; }
		public string School { get; set; }
		public bool? Critical { get; set; }
		public bool? Jump { get; set; }
		public double? BelowHp { get; set; }
		public double Percent { get; set; }
		public double HealPercent { get; set; }
		public double HpPercent { get; set; }
		public double GilPercent { get; set; }
		public double ExpPercent { get; set; }
		public int Count { get; set; }
	}

	/// <summary>data/elites.json: what makes a monster an elite.</summary>
	public sealed class EliteDef
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public double Weight { get; set; } = 1;
		/// <summary>Percent more of each: hp, attack, defense, magicDefense, intellect, agility, strength.</summary>
		public Dictionary<string, double> Stats { get; set; } = new Dictionary<string, double>();
		/// <summary>Percent of its HP back at each of its turns.</summary>
		public double Regen { get; set; }
		/// <summary>Percent of the damage it deals it heals.</summary>
		public double Drain { get; set; }
		public string Text { get; set; }
	}

	public sealed class ElitesConfig
	{
		/// <summary>Modifiers an elite carries: Min..Max of them.</summary>
		public int Min { get; set; } = 1;
		public int Max { get; set; } = 2;
		/// <summary>An elite's HP, times this, before its modifiers.</summary>
		public double Hp { get; set; } = 1.5;
		public List<EliteDef> Modifiers { get; set; } = new List<EliteDef>();
	}

	/// <summary>Everything in data/, loaded and checked.</summary>
	public sealed class RogueContent
	{
		public RunConfig Run = new RunConfig();
		public Dictionary<string, BiomeDef> Biomes = new Dictionary<string, BiomeDef>(StringComparer.OrdinalIgnoreCase);
		public JobsConfig Jobs = new JobsConfig();
		public RewardsConfig Rewards = new RewardsConfig();
		public List<PassiveDef> Passives = new List<PassiveDef>();
		public ElitesConfig Elites = new ElitesConfig();
		/// <summary>What was wrong in the files, for the log (the content still loads what it can).</summary>
		public List<string> Problems = new List<string>();

		public static readonly JsonSerializerOptions Json = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			ReadCommentHandling = JsonCommentHandling.Skip,
			AllowTrailingCommas = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			WriteIndented = true
		};

		public PassiveDef Passive(string id) => Passives.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));
		public JobDef Job(string word) => Jobs.Jobs.FirstOrDefault(j => string.Equals(j.Word, word, StringComparison.OrdinalIgnoreCase));
		public EliteDef Elite(string id) => Elites.Modifiers.FirstOrDefault(e => string.Equals(e.Id, id, StringComparison.OrdinalIgnoreCase));
		public ActDef Act(int index) => index >= 0 && index < Run.Acts.Count ? Run.Acts[index] : null;
		public BiomeDef BiomeOf(ActDef act) => act != null && act.Biome != null && Biomes.TryGetValue(act.Biome, out BiomeDef b) ? b : Biomes.Values.FirstOrDefault();

		/// <summary>The folder's files: acts.json, jobs.json, rewards.json, passives.json, elites.json, biomes/*.json.</summary>
		public static RogueContent Load(string folder)
		{
			RogueContent c = new RogueContent();
			T Read<T>(string file) where T : class, new()
			{
				string path = Path.Combine(folder, file);
				if (!File.Exists(path)) { c.Problems.Add(file + ": missing"); return new T(); }
				try { return JsonSerializer.Deserialize<T>(File.ReadAllText(path), Json) ?? new T(); }
				catch (Exception ex) { c.Problems.Add(file + ": " + ex.Message); return new T(); }
			}
			c.Run = Read<RunConfig>("acts.json");
			c.Jobs = Read<JobsConfig>("jobs.json");
			c.Rewards = Read<RewardsConfig>("rewards.json");
			c.Elites = Read<ElitesConfig>("elites.json");
			PassiveList list = Read<PassiveList>("passives.json");
			c.Passives = list.Passives ?? new List<PassiveDef>();
			string biomes = Path.Combine(folder, "biomes");
			if (Directory.Exists(biomes))
			{
				foreach (string f in Directory.EnumerateFiles(biomes, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
				{
					try
					{
						BiomeDef b = JsonSerializer.Deserialize<BiomeDef>(File.ReadAllText(f), Json);
						if (b == null) continue;
						if (string.IsNullOrWhiteSpace(b.Id)) b.Id = Path.GetFileNameWithoutExtension(f);
						c.Biomes[b.Id] = b;
					}
					catch (Exception ex) { c.Problems.Add("biomes/" + Path.GetFileName(f) + ": " + ex.Message); }
				}
			}
			c.Check();
			return c;
		}

		private sealed class PassiveList { public List<PassiveDef> Passives { get; set; } = new List<PassiveDef>(); }

		/// <summary>What would break a run: an act with no biome, a biome with no monsters or boss, a job with no word.</summary>
		public void Check()
		{
			if (Run.Acts.Count == 0) Problems.Add("acts.json: no acts");
			foreach (ActDef a in Run.Acts)
			{
				BiomeDef b = BiomeOf(a);
				if (b == null) { Problems.Add("act " + a.Id + ": no biome '" + a.Biome + "'"); continue; }
				if (b.Monsters.Count == 0) Problems.Add("biome " + b.Id + ": no monsters");
				if (b.Bosses.Count == 0) Problems.Add("biome " + b.Id + ": no boss");
			}
			foreach (JobDef j in Jobs.Jobs) if (string.IsNullOrWhiteSpace(j.Word)) Problems.Add("jobs.json: a job with no word");
			if (Rewards.Rarities.Count == 0) Rewards.Rarities.Add(new RarityDef { Name = "common", MinPrice = 0 });
		}

		/// <summary>An item's rarity: rewards.json's word for it, else by its price.</summary>
		/// <summary>An item's rarity: "rarityOf" if it says, else by its worth (ValueOf) against the rarities' minPrice.</summary>
		public string RarityOf(ItemInfo item, GameData data = null)
		{
			if (item != null && Rewards.RarityOf.TryGetValue(item.Id.ToString(), out string said)) return said;
			int value = ValueOf(item, data);
			string r = Rewards.Rarities[0].Name;
			foreach (RarityDef d in Rewards.Rarities) if (item != null && value >= d.MinPrice) r = d.Name;
			return r;
		}

		/// <summary>
		/// What an item is worth: its price, or for gear the game sells for 1 (no shop has it - Ultima Weapon, the Onion
		/// set) the dearest price among the same kind's sold gear no stronger than it, twice the dearest past them all.
		/// </summary>
		public int ValueOf(ItemInfo item, GameData data = null)
		{
			if (item == null) return 0;
			if (item.Price > 1 || data == null) return item.Price;
			int power = Math.Max(item.Attack, item.Defense);
			var sold = data.Items.Where(i => i.Kind == item.Kind && i.Price > 1).ToList();
			if (sold.Count == 0) return item.Price;
			var weaker = sold.Where(i => Math.Max(i.Attack, i.Defense) <= power).ToList();
			if (weaker.Count == sold.Count) return sold.Max(i => i.Price) * 2;
			return weaker.Count == 0 ? item.Price : weaker.Max(i => i.Price);
		}

		public int RarityIndex(string rarity)
		{
			int i = Rewards.Rarities.FindIndex(r => string.Equals(r.Name, rarity, StringComparison.OrdinalIgnoreCase));
			return i < 0 ? 0 : i;
		}

		/// <summary>A monster's danger: its biome entry's, else from its record.</summary>
		public double Danger(MonsterInfo m, PoolEntry entry = null)
		{
			if (entry?.Danger != null) return entry.Danger.Value;
			if (m == null) return 1;
			return Math.Max(1, Math.Round(m.Level * Run.LevelWeight + m.MaxHp / Math.Max(1, Run.HpPer)));
		}
	}
}
