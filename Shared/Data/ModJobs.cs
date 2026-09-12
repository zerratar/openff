// A mod's job ladders, for heroes on the mastery progression (FF5's system, ModCharacters.cs):
// defs/jobs/<id>.json, one per job the ladder is for.
//
// FF5's way: a character holds one of the jobs and fights with its commands; every battle
// won earns ABP for the job held, and each step of the job's ladder - so many ABP - hands
// the character an ability for good. A learned ability can then be set into a free command
// slot of whatever job they hold, so a Knight may cast white magic, or a White Mage steal.
// Passive abilities (Cover, Alchemy...) work while set or when a job has them innately; a
// job that "inherits" (FF5's Freelancer and Mime) has the innate passives of every job the
// character has mastered - climbed to the top of - and the best positive stat modifier of
// each of them. Equipment stays FF3's, by the job held (widened by grants); the stats are
// FF3's growth by the job held plus the ladder's modifiers.
//
//   { "id": "knight", "job": "knight", "name": "Knight",
//     "commands": ["attack", "defend", "*", "item"],
//     "innate": ["cover"],
//     "stats": { "strength": 5, "vitality": 4, "magic": -3 },
//     "abilities": [
//       { "abp": 10,  "ability": "cover" },
//       { "abp": 30,  "ability": "defend" },
//       { "abp": 60,  "ability": "equip-shields", "name": "Equip Shields", "passive": true, "grants": ["knight"], "carries": true },
//       { "abp": 100, "ability": "white-magic", "grants": ["white-mage"] }
//     ] }
//
// stats are FF5's job modifiers on FF3's five stats (strength, agility, vitality, intellect,
// mind; FF5's "stamina" is vitality, "magic" both intellect and mind), added to the held
// job's; carries on a step is FF5's rule for Equip abilities and spell lists - while it is
// set, the character has its ladder job's positive modifiers where they beat the held job's.
//
// A job of the mod's own - FF5's Samurai, Berserker, Time Mage, none of which FF3 has - says
// "base" instead of "job": the FF3 job it stands on. The game's party holds the base (its
// growth, charges, equipment permissions and battle motions), the layer remembers which job
// of the mod's the hero really has, and everything else - the name in the menus, the
// commands, the ladder, the stats, the figures ("look", an FF3 job's) - is the mod's:
//
//   { "id": "samurai", "base": "dark-knight", "look": "knight", "name": "Samurai",
//     "commands": ["attack", "*", "defend", "item"], "stats": { "strength": 4, "vitality": 4, "magic": -2 },
//     "abilities": [ ... ] }
//
// Such jobs are numbered from 23 up in load order; a hero takes one through the Abilities
// menu's Jobs page or Game.Party.ChangeJob(id, "samurai"), when its base is a job the
// crystals have opened. The game's own Job menu still lists FF3's 23 - picking one there
// leaves the mod's job.
//
// job is which of FF3's 23 jobs this ladder belongs to (the words ModCharacters takes).
// commands are the four battle commands of a character holding the job, each one of FF3's
// abilities by word (Ff3Abilities below: attack, guard, defend, item, steal, jump, throw,
// black-magic, white-magic, summon...) or "*" for a free slot the player fills; the game's
// own command set is the default, with the job's second command kept and its third made
// free. abilities is the ladder: abp is what the step costs from the one before; ability is
// one of FF3's by word, or a word of the mod's own for a passive of its own (then name says
// what the menu shows and passive is true). grants lends the character the equipment and
// magic permissions of other jobs while the ability is set or innate - what FF5's "Equip
// Swords" does - by the jobs' words; a magic command grants its ladder's job by default, so
// a learned "white-magic" can cast what a White Mage could. innate are passives the job has
// from the start, on top of the game's own (Knight's Cover, Scholar's Alchemy). inherits
// marks a Freelancer-like job that carries the innate passives of every mastered job.
//
// A passive of the mod's own has no effect of the engine's; it is a flag scripts read
// (PartyMember.Abilities) and a permission through grants. FF3's passives the engine acts
// on are Cover (a knight shields the weak), Alchemy (items work twice as well) and
// Counterattack (a plain attack back at every physical blow - FF5's Counter; the client
// added the effect, the table only named it).

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenFF.Data
{
	/// <summary>One of FF3's abilities (player.chaindata chain 13; pl.ABILITY_ID), with a word for definitions.</summary>
	internal sealed class Ff3Ability
	{
		public int Id;
		public string Word;
		public string Name;
		public bool Passive;
		/// <summary>Whether the battle acts on it: the command's case in the battle setup, or a passive the formulas read. The rest are the table's leftovers, shown but idle.</summary>
		public bool Works;
		public override string ToString() => Name + " (" + Word + ")";
	}

	internal static class Ff3Abilities
	{
		/// <summary>The table, in id order; ids the game gives no ability are absent.</summary>
		public static readonly Ff3Ability[] All =
		{
			A(1, "attack", "Attack", works: true), A(2, "run-away", "Run Away", works: true), A(3, "guard", "Guard", works: true), A(4, "item", "Item", works: true),
			A(5, "black-magic", "Black Magic", works: true), A(6, "white-magic", "White Magic", works: true), A(7, "steal", "Steal", works: true), A(8, "flee", "Flee", works: true),
			A(9, "berserk", "Berserk", works: true), P(10, "cover", "Cover", works: true), A(11, "advance", "Advance", works: true), A(12, "bash", "Bash"),
			A(13, "summon", "Summon", works: true), A(14, "chakra", "Chakra"), P(15, "counterattack", "Counterattack", works: true), A(16, "pillage", "Pillage"),
			A(17, "chainspell", "Chainspell"), A(18, "sing", "Sing", works: true), A(19, "hide", "Hide"), P(20, "air-support", "Air Support"),
			A(21, "defend", "Defend", works: true), A(22, "sentinel", "Sentinel"), A(23, "aim", "Aim"), A(24, "shadowbind", "Shadowbind"),
			A(25, "barrage", "Barrage", works: true), A(26, "study", "Study", works: true), A(27, "gauge", "Gauge", works: true), P(28, "alchemy", "Alchemy", works: true),
			A(29, "jump", "Jump", works: true), A(30, "souleater", "Souleater", works: true), A(31, "blood-weapon", "Blood Weapon"), A(32, "terrain", "Terrain", works: true),
			A(33, "hexed-quarter", "Hexed Quarter"), A(34, "ley-lines", "Ley Lines"), A(35, "boost", "Boost", works: true), A(36, "retaliate", "Retaliate", works: true),
			A(37, "meditate", "Meditate"), A(38, "trance", "Trance"), A(39, "charm", "Charm"), A(40, "throw", "Throw", works: true),
			P(41, "dual-wield", "Dual Wield"), A(42, "utsusemi", "Utsusemi"), A(43, "cancel", "Cancel", works: true), A(44, "line", "Line", works: true),
			A(45, "provoke", "Provoke", works: true), A(46, "magic", "Magic", works: true), A(47, "equipment", "Equipment", works: true), A(48, "front", "Front"), A(49, "rear", "Rear"),
			// The client's own passives (not in the game's table; ids 90..99): FF5's HP boosts, honoured by ProgressionLayer.
			P(90, "hp-10", "HP +10%", works: true), P(91, "hp-20", "HP +20%", works: true), P(92, "hp-30", "HP +30%", works: true),
		};

		/// <summary>The HP boost a passive id carries, in percent; 0 for none. Several in play add up, as FF5's do.</summary>
		public static int HpBoost(int id) => id == 90 ? 10 : id == 91 ? 20 : id == 92 ? 30 : 0;

		private static Ff3Ability A(int id, string word, string name, bool works = false) => new Ff3Ability { Id = id, Word = word, Name = name, Works = works };
		private static Ff3Ability P(int id, string word, string name, bool works = false) => new Ff3Ability { Id = id, Word = word, Name = name, Passive = true, Works = works };

		/// <summary>The first free id for a mod's own passives: FF3's end at 49.</summary>
		public const int FirstOwnId = 100;

		/// <summary>The game's magic commands: 5 black, 6 white, 13 summon, 46 both kinds.</summary>
		public static bool IsMagic(int id) => id == 5 || id == 6 || id == 13 || id == 46;

		public static Ff3Ability ById(int id) => All.FirstOrDefault(a => a.Id == id);

		/// <summary>An ability from a definition's word: the word, the name, or the number; null for none of FF3's.</summary>
		public static Ff3Ability ByWord(string word)
		{
			if (string.IsNullOrWhiteSpace(word)) return null;
			if (int.TryParse(word.Trim(), out int n)) return ById(n);
			string slug = ModCharacters.Slug(word);
			return All.FirstOrDefault(a => a.Word == slug || ModCharacters.Slug(a.Name) == slug);
		}

		/// <summary>The game's own four commands of a job (player.chaindata chain 14), for a ladder that does not say; id order of pl.JOB_TYPE.</summary>
		public static readonly int[][] JobCommands =
		{
			new[] { 1, 46, 3, 4 }, new[] { 1, 46, 3, 4 }, new[] { 1, 11, 3, 4 }, new[] { 1, 36, 3, 4 }, new[] { 1, 6, 3, 4 }, new[] { 1, 5, 3, 4 }, new[] { 1, 46, 3, 4 }, new[] { 1, 25, 3, 4 },
			new[] { 1, 6, 21, 4 }, new[] { 1, 7, 8, 4 }, new[] { 1, 46, 26, 4 }, new[] { 1, 32, 3, 4 }, new[] { 1, 29, 3, 4 }, new[] { 1, 45, 3, 4 }, new[] { 1, 30, 3, 4 }, new[] { 1, 13, 3, 4 },
			new[] { 1, 18, 3, 4 }, new[] { 1, 35, 3, 4 }, new[] { 1, 6, 3, 4 }, new[] { 1, 5, 3, 4 }, new[] { 1, 13, 3, 4 }, new[] { 1, 46, 3, 4 }, new[] { 1, 40, 3, 4 },
		};

		/// <summary>The game's own passives of a job: Knight's Cover, Scholar's Alchemy.</summary>
		public static int[] JobPassives(int job) => job == 8 ? new[] { 10 } : job == 10 ? new[] { 28 } : Array.Empty<int>();
	}

	/// <summary>A step of a ladder: so many ABP, then an ability.</summary>
	internal sealed class ModJobAbility
	{
		/// <summary>The step's cost from the one before.</summary>
		public int Abp = 10;
		/// <summary>The ability's word: one of FF3's, or the mod's own passive.</summary>
		public string Ability;
		/// <summary>The menu's name for a passive of the mod's own; FF3's have their own names.</summary>
		public string Name;
		/// <summary>A passive of the mod's own (FF3's carry their kind themselves).</summary>
		public bool Passive;
		/// <summary>Jobs whose equipment and magic permissions the character borrows while the ability is set or innate.</summary>
		public List<string> Grants = new List<string>();
		/// <summary>FF5's rule for Equip abilities and spell lists: while set, the ability passes along its ladder job's positive stat modifiers where they beat the held job's.</summary>
		public bool Carries;

		/// <summary>The id in play: FF3's, or one given to a passive of the mod's own by the loader (Ff3Abilities.FirstOwnId up).</summary>
		public int Id;
		/// <summary>Whether it is a passive in play (FF3's kind, or the mod's own).</summary>
		public bool IsPassive => Passive || (Ff3Abilities.ById(Id)?.Passive ?? false);
		/// <summary>The name in play.</summary>
		public string ShownName => !string.IsNullOrWhiteSpace(Name) ? Name : Ff3Abilities.ById(Id)?.Name ?? Ability;
	}

	internal sealed class ModJob
	{
		public string Id;
		/// <summary>Which of FF3's jobs the ladder is for, by word or number; null for a job of the mod's own (see Base).</summary>
		public string Job;
		/// <summary>
		/// A job of the mod's own stands on one of FF3's: base is that job, by word - its growth
		/// tables, magic charges, equipment permissions and battle motions are the base's, and the
		/// game's own party holds the base while the layer remembers the job of the mod's. A mod may
		/// stand several jobs on one base (a Samurai and a Berserker both on the Dark Knight).
		/// </summary>
		public string Base;
		/// <summary>Whose figures a job of the mod's own wears, by FF3 job word; the base's when unsaid. Until someone models new ones, the game's own.</summary>
		public string Look;
		public string Name;
		/// <summary>The job's number in play: FF3's 0..22, or one given to a job of the mod's own by the loader (FirstOwnNumber up, in load order).</summary>
		public int Number = -1;
		public const int FirstOwnNumber = 23;
		/// <summary>A job of the mod's own rather than a ladder for one of FF3's.</summary>
		public bool IsOwn => !string.IsNullOrWhiteSpace(Base);
		/// <summary>The FF3 job the game's party holds for this one: the base of a job of the mod's own, the job itself otherwise; -1 for none.</summary>
		public int BaseJob => IsOwn ? ModCharacters.JobNumber(Base) : ModCharacters.JobNumber(Job);
		/// <summary>The FF3 job whose figures are drawn: look, else the base.</summary>
		public int LookJob { get { int l = string.IsNullOrWhiteSpace(Look) ? -1 : ModCharacters.JobNumber(Look); return l >= 0 ? l : BaseJob; } }
		/// <summary>The four battle commands by word, "*" for a free slot; empty for the game's own set with the third made free.</summary>
		public List<string> Commands = new List<string>();
		/// <summary>Passives the job has from the start, by word.</summary>
		public List<string> Innate = new List<string>();
		/// <summary>Carries the innate passives and the best positive stat modifiers of every mastered job (FF5's Freelancer, Mime).</summary>
		public bool Inherits;
		/// <summary>FF5's stat modifiers of the job, added to the held job's stats: strength, agility, vitality, intellect, mind (Stats order); FF5's "stamina" reads as vitality, "magic" as intellect and mind both.</summary>
		public int[] Stats = new int[5];
		public List<ModJobAbility> Abilities = new List<ModJobAbility>();
		public string Source;

		public static readonly string[] StatWords = { "strength", "agility", "vitality", "intellect", "mind" };
		public bool HasStats => Stats.Any(s => s != 0);

		/// <summary>The job's number in play (Number once loaded; FF3's for a ladder that says a job), -1 for none.</summary>
		public int JobNumber => Number >= 0 ? Number : IsOwn ? -1 : ModCharacters.JobNumber(Job);

		/// <summary>The total ABP to the top of the ladder.</summary>
		public int TotalAbp => Abilities.Sum(a => Math.Max(0, a.Abp));

		/// <summary>The command ids in play, -1 for a free slot.</summary>
		public int[] CommandIds()
		{
			int job = BaseJob;
			int[] ids = job >= 0 && job < Ff3Abilities.JobCommands.Length ? (int[])Ff3Abilities.JobCommands[job].Clone() : new[] { 1, 46, 3, 4 };
			if (Commands.Count == 0) { ids[2] = -1; return ids; }
			for (int i = 0; i < 4; i++)
			{
				string w = i < Commands.Count ? Commands[i] : null;
				if (string.IsNullOrWhiteSpace(w)) continue;
				if (w.Trim() == "*") { ids[i] = -1; continue; }
				Ff3Ability a = Ff3Abilities.ByWord(w);
				if (a != null && !a.Passive) ids[i] = a.Id;
			}
			return ids;
		}

		/// <summary>How many free slots the commands leave.</summary>
		public int FreeSlots => CommandIds().Count(c => c < 0);

		public static ModJob Parse(string json, string source = null)
		{
			JsonNode node = JsonNode.Parse(json);
			if (node == null) return null;
			ModJob j = new ModJob
			{
				Id = node["id"]?.GetValue<string>(),
				Job = node["job"] is JsonValue jv ? (jv.TryGetValue(out int n) ? n.ToString() : jv.GetValue<string>()) : null,
				Base = node["base"] is JsonValue bv ? (bv.TryGetValue(out int bn) ? bn.ToString() : bv.GetValue<string>()) : null,
				Look = node["look"] is JsonValue lv ? (lv.TryGetValue(out int ln) ? ln.ToString() : lv.GetValue<string>()) : null,
				Name = node["name"]?.GetValue<string>(),
				Inherits = node["inherits"]?.GetValue<bool>() ?? false,
				Source = source
			};
			if (node["commands"] is JsonArray commands) foreach (JsonNode c in commands) j.Commands.Add(c?.GetValue<string>() ?? "");
			if (node["innate"] is JsonArray innate) foreach (JsonNode c in innate) if (c != null) j.Innate.Add(c.GetValue<string>());
			if (node["stats"] is JsonObject stats)
			{
				foreach (KeyValuePair<string, JsonNode> kv in stats)
				{
					int value = kv.Value?.GetValue<int>() ?? 0;
					switch (ModCharacters.Slug(kv.Key))
					{
						case "strength": case "str": j.Stats[0] = value; break;
						case "agility": case "agi": case "speed": j.Stats[1] = value; break;
						case "vitality": case "vit": case "stamina": case "sta": j.Stats[2] = value; break;
						case "intellect": case "int": case "intelligence": j.Stats[3] = value; break;
						case "mind": case "spirit": case "mnd": j.Stats[4] = value; break;
						case "magic": case "mag": j.Stats[3] = value; j.Stats[4] = value; break;
					}
				}
			}
			if (node["abilities"] is JsonArray abilities)
			{
				foreach (JsonNode a in abilities)
				{
					if (a == null) continue;
					ModJobAbility step = new ModJobAbility
					{
						Abp = a["abp"]?.GetValue<int>() ?? 10,
						Ability = a["ability"] is JsonValue av ? (av.TryGetValue(out int an) ? an.ToString() : av.GetValue<string>()) : null,
						Name = a["name"]?.GetValue<string>(),
						Passive = a["passive"]?.GetValue<bool>() ?? false,
						Carries = a["carries"]?.GetValue<bool>() ?? false
					};
					if (a["grants"] is JsonArray grants) foreach (JsonNode g in grants) if (g != null) step.Grants.Add(g.GetValue<string>());
					j.Abilities.Add(step);
				}
			}
			return j;
		}

		public string ToJson()
		{
			JsonObject node = new JsonObject { ["id"] = Id };
			if (IsOwn) { node["base"] = Base; if (!string.IsNullOrEmpty(Look)) node["look"] = Look; }
			else if (!string.IsNullOrEmpty(Job)) node["job"] = Job;
			if (!string.IsNullOrEmpty(Name)) node["name"] = Name;
			if (Commands.Count > 0) node["commands"] = new JsonArray(Commands.Select(c => (JsonNode)JsonValue.Create(c ?? "")).ToArray());
			if (Innate.Count > 0) node["innate"] = new JsonArray(Innate.Select(c => (JsonNode)JsonValue.Create(c)).ToArray());
			if (Inherits) node["inherits"] = true;
			if (HasStats)
			{
				JsonObject stats = new JsonObject();
				for (int i = 0; i < 5; i++) if (Stats[i] != 0) stats[StatWords[i]] = Stats[i];
				node["stats"] = stats;
			}
			JsonArray abilities = new JsonArray();
			foreach (ModJobAbility a in Abilities)
			{
				JsonObject o = new JsonObject { ["abp"] = a.Abp, ["ability"] = a.Ability ?? "" };
				if (!string.IsNullOrWhiteSpace(a.Name)) o["name"] = a.Name;
				if (a.Passive) o["passive"] = true;
				if (a.Grants.Count > 0) o["grants"] = new JsonArray(a.Grants.Select(g => (JsonNode)JsonValue.Create(g)).ToArray());
				if (a.Carries) o["carries"] = true;
				abilities.Add(o);
			}
			node["abilities"] = abilities;
			return node.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
		}
	}

	internal static class ModJobs
	{
		public const string Folder = "defs/jobs";

		/// <summary>
		/// Every ladder under the roots, checked: a known job, one ladder per job (the first loaded
		/// wins), abilities that exist - FF3's by word, or the mod's own passives, which get ids from
		/// FirstOwnId up, the same word the same id across ladders. Faults go to notes; a faulty step
		/// is dropped, a faulty ladder skipped.
		/// </summary>
		public static List<ModJob> Load(IEnumerable<string> roots, List<string> notes = null)
		{
			List<ModJob> all = new List<ModJob>();
			Dictionary<string, int> own = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			HashSet<int> jobs = new HashSet<int>();
			foreach (string root in roots ?? Enumerable.Empty<string>())
			{
				string directory = Path.Combine(root, Folder.Replace('/', Path.DirectorySeparatorChar));
				if (!Directory.Exists(directory)) continue;
				foreach (string file in Directory.EnumerateFiles(directory, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
				{
					try
					{
						ModJob j = ModJob.Parse(File.ReadAllText(file), file);
						if (j == null) continue;
						if (string.IsNullOrWhiteSpace(j.Id)) j.Id = Path.GetFileNameWithoutExtension(file);
						int job;
						if (j.IsOwn)
						{
							// A job of the mod's own: numbered from 23 up in load order; it needs a base among FF3's.
							if (j.BaseJob < 0) { notes?.Add(file + ": base '" + j.Base + "' is none of FF3's jobs"); continue; }
							if (!string.IsNullOrWhiteSpace(j.Look) && ModCharacters.JobNumber(j.Look) < 0) { notes?.Add(file + ": look '" + j.Look + "' is none of FF3's jobs; the base's figures are worn"); j.Look = null; }
							if (string.IsNullOrWhiteSpace(j.Name)) j.Name = j.Id;
							j.Number = job = ModJob.FirstOwnNumber + all.Count(x => x.IsOwn);
							jobs.Add(job);
						}
						else
						{
							if (string.IsNullOrWhiteSpace(j.Job)) j.Job = j.Id;
							job = ModCharacters.JobNumber(j.Job);
							if (job < 0) { notes?.Add(file + ": no job called '" + j.Job + "' (a job of the mod's own says \"base\" instead)"); continue; }
							if (!jobs.Add(job)) { notes?.Add(file + ": " + ModCharacters.Jobs[job].Name + " already has a ladder; skipped"); continue; }
							j.Number = job;
							if (string.IsNullOrWhiteSpace(j.Name)) j.Name = ModCharacters.Jobs[job].Name;
						}
						int baseJob = j.BaseJob;
						for (int i = 0; i < j.Commands.Count && i < 4; i++)
						{
							string w = j.Commands[i];
							if (string.IsNullOrWhiteSpace(w) || w.Trim() == "*") continue;
							Ff3Ability a = Ff3Abilities.ByWord(w);
							if (a == null) { notes?.Add(file + ": commands[" + i + "] '" + w + "' is none of FF3's abilities; the game's own is kept"); j.Commands[i] = ""; }
							else if (a.Passive) { notes?.Add(file + ": commands[" + i + "] " + a.Name + " is a passive, not a command; the game's own is kept"); j.Commands[i] = ""; }
						}
						foreach (ModJobAbility a in j.Abilities.ToList())
						{
							if (string.IsNullOrWhiteSpace(a.Ability)) { notes?.Add(file + ": a step without an ability; dropped"); j.Abilities.Remove(a); continue; }
							Ff3Ability known = Ff3Abilities.ByWord(a.Ability);
							if (known != null)
							{
								a.Id = known.Id;
								if (a.Passive && !known.Passive) { notes?.Add(file + ": " + known.Name + " is one of FF3's commands, not a passive"); a.Passive = false; }
								if (known.Id == 1 || known.Id == 4 || known.Id == 2 || known.Id == 47 || known.Id == 44 || known.Id == 48 || known.Id == 49) notes?.Add(file + ": " + known.Name + " is a command every job has; learning it is idle");
							}
							else
							{
								string word = ModCharacters.Slug(a.Ability);
								if (!own.TryGetValue(word, out int id)) own[word] = id = Ff3Abilities.FirstOwnId + own.Count;
								a.Id = id;
								a.Passive = true;   // a word of the mod's own can only be a passive: the battle has no code for it
								if (string.IsNullOrWhiteSpace(a.Name)) a.Name = a.Ability;
							}
							if (a.Abp < 0) a.Abp = 0;
							foreach (string g in a.Grants.ToList()) if (ModCharacters.JobNumber(g) < 0) { notes?.Add(file + ": grants '" + g + "' is no job"); a.Grants.Remove(g); }
							if (a.Grants.Count == 0 && Ff3Abilities.IsMagic(a.Id)) a.Grants.Add(ModCharacters.Jobs[baseJob].Enum);
						}
						foreach (string w in j.Innate.ToList())
						{
							Ff3Ability a = Ff3Abilities.ByWord(w);
							if (a == null && !own.ContainsKey(ModCharacters.Slug(w))) { notes?.Add(file + ": innate '" + w + "' is no ability of FF3's nor a passive of this mod's ladders"); j.Innate.Remove(w); }
							else if (a != null && !a.Passive) { notes?.Add(file + ": innate " + a.Name + " is a command; only passives are innate"); j.Innate.Remove(w); }
						}
						all.Add(j);
					}
					catch (Exception ex) { notes?.Add(file + ": " + ex.Message); }
				}
			}
			return all;
		}

		/// <summary>The id of an ability word across the loaded ladders: FF3's, or a mod passive's; -1 for none.</summary>
		public static int AbilityId(IEnumerable<ModJob> ladders, string word)
		{
			Ff3Ability a = Ff3Abilities.ByWord(word);
			if (a != null) return a.Id;
			string slug = ModCharacters.Slug(word);
			foreach (ModJob j in ladders) foreach (ModJobAbility step in j.Abilities) if (step.Id >= Ff3Abilities.FirstOwnId && ModCharacters.Slug(step.Ability) == slug) return step.Id;
			return -1;
		}

		/// <summary>The step that teaches an ability id, across the ladders; null for none.</summary>
		public static ModJobAbility Step(IEnumerable<ModJob> ladders, int id)
		{
			foreach (ModJob j in ladders) foreach (ModJobAbility step in j.Abilities) if (step.Id == id) return step;
			return null;
		}

		/// <summary>The name of an ability id across the ladders and FF3's table.</summary>
		public static string AbilityName(IEnumerable<ModJob> ladders, int id)
		{
			ModJobAbility step = Step(ladders, id);
			return step?.ShownName ?? Ff3Abilities.ById(id)?.Name ?? ("ability " + id);
		}
	}
}
