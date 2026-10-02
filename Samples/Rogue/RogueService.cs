// Rogue Mode - FF3 as a run-based roguelite, built on the game's own battles, jobs, magic and equipment.
//
// From the title, Rogue Mode starts a game of its own (the "rogue" save profile, so the campaign's saves are
// never touched) on a camp: the act's battlefield loaded as a map, the four heroes standing on it in their
// jobs' figures. A run is a line of battles through three acts, every third an elite, each act ending in one
// of FF3's bosses; a victory offers a choice of three rewards - equipment, a spell, a passive modifier, gil,
// rest, experience - and a party wipe ends the run with its summary.
//
// What is Rogue Mode's own lives in Core/ (no engine in it - Tests/ checks it) and in data/*.json; FF3's data
// is never copied: monsters, items, spells and jobs are named by the game's ids, and the rest is the game's
// (Game.Monsters, Game.Items, Game.Magic, Game.Party). FF3's battle is FF3's battle: Rogue Mode starts it
// (Game.Battle.Start with a party it made, a loss returning to the field) and has its say through the
// engine's battle events - the monsters' stats as a battle begins (BattleMonstersReady: the run's scaling and
// the elites), each hit, spell and heal (BattleDamage: the passives), each turn (BattleTurnStarting:
// regenerating elites), each fall (BattleUnitFell: Phoenix Blessing, Bloodlust), and the spoils.
//
// The screens are the game's own menus (menus/*.xml, drawn in Crystal's Menus tab), driven by
// RogueScreens.cs. The run is saved after every step to %LOCALAPPDATA%/OpenFF/rogue/run.json: the run's state
// with each hero's record (Game.Party.Export), the bag and the gil - Continue Run puts them back.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using OpenFF;
using OpenFF.Events;
using Rogue.Core;
using Encounter = Rogue.Core.Encounter;

namespace Rogue
{
	public sealed class RogueService : GameService
	{
		public static RogueService Instance;
		public const string Profile = "rogue";
		public static readonly string[] HeroNames = { "Luneth", "Arc", "Refia", "Ingus" };

		public RogueContent Content;
		public GameData Data;
		public RunState Run;
		/// <summary>The last run's end, for the summary screen.</summary>
		public RunState Ended;

		// The camp: the heroes as figures on the battlefield, the camera low and back as a battle's.
		private static readonly Vector3[] Places = { new Vector3(-27, 0, 16), new Vector3(-9, 0, 20), new Vector3(9, 0, 20), new Vector3(27, 0, 16) };
		// High and back, looking down on them: the battleground's floor and crystals fill the view, not the dark past its edge.
		private static readonly Vector3 CameraAt = new Vector3(0, 50, 100);
		private static readonly Vector3 CameraLook = new Vector3(0, 10, 18);
		// Up and away from the battleground, where nothing is drawn: what the field shows while it is not to be seen.
		private static readonly Vector3 IntoTheDark = new Vector3(0, 600, 1200);
		private static readonly Vector3 Aside = new Vector3(0, 0, 130);
		private readonly Npc[] _figures = new Npc[4];
		private readonly string[] _figureModels = new string[4];

		/// <summary>What to do once the field is quiet: open a screen, start the battle, restore a saved run.</summary>
		private string _openNext;
		private int _wait = -1;
		private bool _fieldUp;
		// The field kept out of sight till a screen of the mode's is up: set when one is on its way (After) or a battle starts
		// (the field comes back before the battle says how it ended), let go once the menus show.
		private bool _veil;
		private bool _battleNext, _inBattle;
		private readonly Dictionary<BattleUnit, double> _regen = new Dictionary<BattleUnit, double>();
		private readonly Dictionary<BattleUnit, double> _drain = new Dictionary<BattleUnit, double>();

		public override bool WantsUpdate => true;

		/// <summary>Whether this game is Rogue Mode's (started from its title entry).</summary>
		public static bool Active => Game.Title.SaveProfile == Profile;

		public static string Folder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenFF", "rogue");
		public static string RunFile => Path.Combine(Folder, "run.json");
		public static bool HasSavedRun => File.Exists(RunFile);

		/// <summary>The run waiting to be continued, read from its file (null when there is none or it does not read).</summary>
		public static RunState SavedRun()
		{
			try { return HasSavedRun ? RunState.FromJson(File.ReadAllText(RunFile)) : null; }
			catch (Exception) { return null; }
		}

		public override void OnGameStart()
		{
			Instance = this;
			string dir = Mod?.Directory ?? AppContext.BaseDirectory;
			Content = RogueContent.Load(Path.Combine(dir, "data"));
			foreach (string p in Content.Problems) Game.Log("rogue: " + p);
			Game.Log("rogue: " + Content.Run.Acts.Count + " acts, " + Content.Biomes.Count + " biomes, " + Content.Passives.Count + " passives, " + Content.Jobs.Jobs.Count + " jobs");
			Game.Title.AddEntry("Rogue Mode", Begin);

			// The scene staged as the map comes up, before the first frame shows the game's own camera on it.
			Game.Events.Subscribe<MapEntered>(e => { _fieldUp = true; if (Active) { Stage(); _wait = 12; if (_openNext == null) _openNext = Run != null ? (Run.Pending.Count > 0 ? "rogue-reward" : "rogue-camp") : "rogue"; } });
			Game.Events.Subscribe<MapLeaving>(e => { _fieldUp = false; ClearFigures(); });
			Game.Events.Subscribe<TitleShown>(e => { ClearFigures(); Run = null; _inBattle = false; _battleNext = false; _openNext = null; });
			Game.Events.Subscribe<BattleMonstersReady>(OnMonstersReady);
			Game.Events.Subscribe<BattleDamage>(OnDamage);
			Game.Events.Subscribe<BattleTurnStarting>(OnTurn);
			Game.Events.Subscribe<BattleUnitFell>(OnFell);
			Game.Events.Subscribe<BattleRewards>(OnRewards);
			Game.Events.Subscribe<BattleEnded>(OnBattleEnded);
		}

		/// <summary>The title's entry: a game of Rogue Mode's own profile, on the first act's battlefield.</summary>
		private void Begin() => Game.Title.NewGame(0, Content.BiomeOf(Content.Act(0))?.Backdrop ?? "b28", Aside, Profile);

		/// <summary>A test drive's seed (ROGUE_SEED), or 0 for none.</summary>
		public static ulong TestSeed => ulong.TryParse(Environment.GetEnvironmentVariable("ROGUE_SEED"), out ulong s) ? s : 0;

		public override void OnUpdate()
		{
			// The field's buttons down while Rogue Mode has the field (it is only the camp's scene between its screens), up again after.
			if (Game.Hud.FieldButtons == Active) Game.Hud.FieldButtons = !Active;
			if (!Active) return;
			// A screen of the mode's about to open over the field (the start, after a battle, a warp): the field held black till it
			// is up, so what shows is the screen, not the battleground on the way to it. Only on the way into a battle is the camp seen.
			// With no run (the mode's start, a run just ended) the field has nothing to show at all. Only once the map is up:
			// the field's own start waits for its fade-in, which a fade held from the first frame would never let finish.
			if (Game.Menus.Showing) _veil = false;
			bool hidden = _veil || _openNext != null || Run == null;

			if (hidden && _fieldUp && !Game.Menus.Showing && !Game.Battle.InBattle) Game.Screen.FadeOut(0);
			// The camp's eye, every frame the field shows: the game sets its own as a map comes up (after a battle, a warp),
			// before the map counts as entered, and that one looks past the battleground's edge.
			// The figures too, as soon as the party they stand for has changed - so the scene fades in with all of them.
			// Till then (the field fading in on its own), a field that is not to be seen is looked away from: up, into the dark.
			if (!Game.Battle.InBattle && Game.Menus.Current == null && OnBattleground)
			{
				Game.Camera.MoveTo(CameraAt);
				Game.Camera.LookAt(hidden ? IntoTheDark : CameraLook);
				if (FiguresStale() && Game.Hero.Present) Stage();   // once the map's characters are up: spawning before that fails
			}
			if (_wait < 0) return;
			if (Game.Field.Busy || Game.Battle.InBattle || Game.Menus.Current != null) return;
			if (_wait-- > 0) return;
			_wait = -1;
			Stage();
			if (_battleNext) { _battleNext = false; StartBattle(); return; }
			if (_openNext != null) { string id = _openNext; _openNext = null; _veil = true; if (!Game.Menus.Open(id)) Game.Log("rogue: no screen " + id); }
		}

		/// <summary>Once the field is quiet: open a screen (or start the battle, after Fight).</summary>
		public void After(string screen, int frames = 4) { _openNext = screen; _wait = frames; _veil = true; }

		// ---------------------------------------------------------------- the camp

		/// <summary>The camp scene: the walker out of sight, the heroes as figures in their jobs, the camera before them.</summary>
		private void Stage()
		{
			try
			{
				Game.Hero.Teleport(Aside);
				for (int h = 0; h < 4; h++)
				{
					string model = FigureFor(h);
					bool wanted = model != null;
					if (_figures[h] != null && _figureModels[h] == model && wanted) continue;
					_figures[h]?.Remove();
					_figures[h] = wanted ? Game.Npcs.Spawn(model, Places[h], 0f) : null;
					_figureModels[h] = _figures[h] != null ? model : null;   // one that could not be placed yet is tried again next frame
				}
				Game.Camera.MoveTo(CameraAt);
				Game.Camera.LookAt(_veil || _openNext != null || Run == null ? IntoTheDark : CameraLook);
			}
			catch (Exception ex) { Game.Log("rogue: camp: " + ex.Message); }
		}

		/// <summary>The figure each hero should stand as now ("j" hero job), or null for none.</summary>
		private string FigureFor(int h)
		{
			PartyMember m = Game.Party.Member(h);
			// No run, no party to stand: the mode's own menu is over an empty battleground.
			return Run != null ? "j" + (h + 1) + ((m?.Job ?? 0) + 1).ToString("00", CultureInfo.InvariantCulture) : null;
		}

		private bool FiguresStale()
		{
			for (int h = 0; h < 4; h++) if (FigureFor(h) != _figureModels[h]) return true;
			return false;
		}

		private void ClearFigures()
		{
			for (int h = 0; h < 4; h++) { try { _figures[h]?.Remove(); } catch (Exception) { } _figures[h] = null; _figureModels[h] = null; }
		}

		// ---------------------------------------------------------------- the game's data, as the core reads it

		public GameData GameData()
		{
			if (Data != null && Data.Spells.Count > 0) return Data;
			GameData d = new GameData();
			foreach (Monster m in Game.Monsters.All) d.Monsters.Add(new MonsterInfo { Id = m.Id, Name = m.Name, Level = m.Level, MaxHp = m.MaxHp, Size = m.Size });
			foreach (Item i in Game.Items.All)
			{
				string kind = i.Category == ItemCategory.Weapon ? "weapon"
					: i.Category == ItemCategory.Armor ? (i.Slot == EquipSlot.Head ? "helmet" : i.Slot == EquipSlot.Arm ? "gloves" : i.Slot == EquipSlot.LeftHand ? "shield" : "armour")
					: i.Category == ItemCategory.Consumable ? "item" : "other";
				d.Items.Add(new ItemInfo { Id = i.Id, Name = i.Name, Kind = kind, Price = i.Price, Jobs = i.Jobs, Attack = i.Attack, Defense = i.Defense, Caption = i.Caption });
			}
			foreach (Spell s in Game.Magic.All)
			{
				if (s.Custom || (s.School != MagicSchool.White && s.School != MagicSchool.Black && s.School != MagicSchool.Summon)) continue;
				d.Spells.Add(new SpellInfo { Id = s.Id, Name = s.Name, Level = s.Level, School = s.School.ToString().ToLowerInvariant(), Jobs = s.Jobs, Caption = s.Caption, InBattle = s.InBattle });
			}
			Data = d;
			if (Environment.GetEnvironmentVariable("ROGUE_DUMP") != null) foreach (MonsterInfo m in d.Monsters) Game.Log("rogue-dump: monster " + m.Id + " " + m.Name + " lv " + m.Level + " hp " + m.MaxHp);
			if (Environment.GetEnvironmentVariable("ROGUE_DUMP") != null) foreach (ItemInfo i in d.Items) if (i.Kind != "item" && i.Kind != "other") Game.Log("rogue-dump: " + i.Id + " " + i.Name + " " + i.Kind + " price " + i.Price + " atk " + i.Attack + " def " + i.Defense + " jobs " + i.Jobs);
			return d;
		}

		public PartyView Party()
		{
			PartyView v = new PartyView();
			for (int h = 0; h < 4; h++)
			{
				PartyMember m = Game.Party.Member(h);
				v.Jobs.Add(m?.Job ?? 0);
				v.Names.Add(m?.Name ?? HeroNames[h]);
				v.Spells.Add(new HashSet<int>(m?.Spells ?? new List<int>()));
				for (int slot = 0; slot <= 4; slot++) { int it = Game.Party.Equipped(h, (EquipSlot)slot); if (it > 0) v.Owned.Add(it); }
			}
			foreach (ItemStack s in Game.Party.Items) v.Owned.Add(s.ItemId);
			GameData data = GameData();
			// FF3 holds three spells a magic level.
			v.HasRoomFor = (hero, level) => v.Spells[hero].Count(id => data.Spell(id)?.Level == level) < 3;
			return v;
		}

		// ---------------------------------------------------------------- a run

		/// <summary>A new run: the four heroes in their jobs at the starting level, their kits and spells, the first battle rolled.</summary>
		public void StartRun(ulong seed, IList<string> jobs)
		{
			GameData data = GameData();
			for (int h = 1; h < 4; h++) Game.Party.AddMember(h);
			for (int h = 0; h < 4; h++)
			{
				JobDef job = Content.Job(jobs[h]) ?? Content.Jobs.Jobs.First();
				Game.Party.SetJob(h, (Job)job.Job);
				Game.Party.SetLevel(h, Content.Run.StartLevel);
				foreach (int item in Kit(job, data)) { Game.Party.AddItem(item, 1); Game.Party.Equip(h, item); }
				foreach (int spell in StartingSpells(job, data)) Game.Party.LearnSpell(h, spell);
			}
			Game.Party.HealAll();
			Game.Party.Gil = 200;
			Run = RunState.Begin(seed, jobs, DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
			Run.Next = EncounterGenerator.Build(Content, data, Run, Run.Stream("encounter"));
			Save();
			Game.Log("rogue: a run begins, seed " + seed + ": " + string.Join(", ", jobs));
		}

		/// <summary>
		/// A job's starting gear: its kit, else the strongest weapon and body armour it may wear that a shop sells for
		/// the kit's price at most (jobs.json's kitPrice; the game prices what no shop sells at 1), kitSkip left out.
		/// </summary>
		public static IEnumerable<int> Kit(JobDef job, GameData data)
		{
			if (job.Kit.Count > 0) return job.Kit;
			JobsConfig jobs = Instance.Content.Jobs;
			int bit = 1 << job.Job;
			var mine = data.Items.Where(i => (i.Jobs & bit) != 0 && i.Price > 1 && i.Price <= jobs.KitPrice && !jobs.Skipped(i.Id)).ToList();
			var picks = new List<int>();
			ItemInfo weapon = mine.Where(i => i.Kind == "weapon").OrderByDescending(i => i.Attack).ThenBy(i => i.Price).FirstOrDefault();
			ItemInfo armour = mine.Where(i => i.Kind == "armour").OrderByDescending(i => i.Defense).ThenBy(i => i.Price).FirstOrDefault();
			if (weapon != null) picks.Add(weapon.Id);
			if (armour != null) picks.Add(armour.Id);
			return picks;
		}

		/// <summary>A caster's starting spells: its list, else SpellCount level-1 battle spells it may hold, the cheapest first by id.</summary>
		public static IEnumerable<int> StartingSpells(JobDef job, GameData data)
		{
			if (job.Spells != null && job.Spells.Count > 0) return job.Spells;
			if (job.SpellCount <= 0) return Array.Empty<int>();
			int bit = 1 << job.Job;
			return data.Spells.Where(s => s.Level == 1 && s.InBattle && (s.Jobs & bit) != 0 && s.School != "summon").OrderBy(s => s.Id).Take(job.SpellCount).Select(s => s.Id).ToList();
		}

		public ActDef Act => Content.Act(Run?.Act ?? 0);
		/// <summary>Whether the field is the act's battleground, where the camp stands.</summary>
		private bool OnBattleground => string.Equals(Game.Field.Map, Biome?.Backdrop, StringComparison.OrdinalIgnoreCase);
		public BiomeDef Biome => Content.BiomeOf(Act);

		/// <summary>Fight (the camp's): the menus close, then the battle starts.</summary>
		public void Fight()
		{
			_battleNext = true;
			_wait = 2;
		}

		private void StartBattle()
		{
			if (Run?.Next == null) return;
			MonsterGroup group = new MonsterGroup();
			foreach (CountDef m in Run.Next.Monsters.Take(4)) group.Members.Add(new MonsterCount { MonsterId = m.Id, Min = m.Count, Max = m.Count });
			_inBattle = true;
			Game.Battle.EscapeAllowed = false;
			_veil = true;
			Game.Battle.Start(group, Run.Next.BattleMap, new BattleOptions { LossReturns = true });
			Game.Log("rogue: battle " + (Run.Step + 1) + " of act " + (Run.Act + 1) + " (" + Run.Next.Kind + "): " + Run.Next.Name + (Run.Next.Elite.Count > 0 ? " [" + string.Join(", ", Run.Next.Elite) + "]" : ""));
		}

		// ---------------------------------------------------------------- the battle

		private void OnMonstersReady(BattleMonstersReady e)
		{
			if (!_inBattle || Run?.Next == null) return;
			_regen.Clear(); _drain.Clear();
			Encounter enc = Run.Next;
			double hp = enc.Scale * (enc.Kind == "elite" ? Content.Elites.Hp : 1);
			Dictionary<string, double> more = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
			double regen = 0, drain = 0;
			foreach (string id in enc.Elite)
			{
				EliteDef d = Content.Elite(id);
				if (d == null) continue;
				foreach (var s in d.Stats) more[s.Key] = (more.TryGetValue(s.Key, out double v) ? v : 0) + s.Value;
				regen += d.Regen; drain += d.Drain;
			}
			double M(string stat) => enc.Scale * (1 + (more.TryGetValue(stat, out double v) ? v : 0) / 100.0);
			foreach (BattleUnit u in e.Monsters)
			{
				u.MaxHp = (int)Math.Round(u.MaxHp * hp * (1 + (more.TryGetValue("hp", out double h) ? h : 0) / 100.0));
				u.Hp = u.MaxHp;
				u.Attack = (int)Math.Round(u.Attack * M("attack"));
				u.Defense = (int)Math.Round(u.Defense * M("defense"));
				u.MagicDefense = (int)Math.Round(u.MagicDefense * M("magicDefense"));
				u.Strength = (int)Math.Round(u.Strength * M("strength"));
				u.Agility = (int)Math.Round(u.Agility * M("agility"));
				u.Intellect = (int)Math.Round(u.Intellect * M("intellect"));
				if (regen > 0) _regen[u] = regen;
				if (drain > 0) _drain[u] = drain;
			}
			// "start" effects: the heroes heal as the battle begins.
			double heal = Modifiers.Sum(Content, Run, "start", x => x.HealPercent);
			if (heal > 0) foreach (BattleUnit u in Game.Battle.Units.Where(u => !u.IsMonster && u.Alive)) u.Hp = Math.Min(u.MaxHp, u.Hp + (int)Math.Round(u.MaxHp * heal / 100));
		}

		private void OnDamage(BattleDamage e)
		{
			if (!_inBattle || Run == null || e.Missed || e.Target == null) return;
			bool fromHero = e.Attacker != null && !e.Attacker.IsMonster;
			if (e.Kind == DamageKind.Healing)
			{
				if (fromHero) e.Amount = Modifiers.Apply(e.Amount, Modifiers.Percent(Content, Run, "healing", new Modifiers.Hit { Kind = "healing" }));
				return;
			}
			if (fromHero && e.Target.IsMonster)
			{
				Modifiers.Hit hit = new Modifiers.Hit
				{
					Kind = e.Kind == DamageKind.Magic ? "magic" : e.Kind == DamageKind.Ability ? "ability" : "physical",
					Element = e.Element == Element.None ? null : e.Element.ToString(),
					School = e.SpellId > 0 ? GameData().Spell(e.SpellId)?.School : null,
					Critical = e.Critical, Jump = e.Jump,
					AttackerHp = e.Attacker.MaxHp > 0 ? e.Attacker.Hp * 100.0 / e.Attacker.MaxHp : 100
				};
				e.Amount = Modifiers.Apply(e.Amount, Modifiers.Percent(Content, Run, "damage", hit));
			}
			else if (!e.Target.IsMonster && e.Attacker != null && e.Attacker.IsMonster)
			{
				e.Amount = Modifiers.Apply(e.Amount, Modifiers.Percent(Content, Run, "taken", new Modifiers.Hit { Kind = "any" }));
				if (_drain.TryGetValue(e.Attacker, out double drain) && e.Attacker.Alive) e.Attacker.Hp = Math.Min(e.Attacker.MaxHp, e.Attacker.Hp + (int)Math.Round(e.Amount * drain / 100));
			}
		}

		private void OnTurn(BattleTurnStarting e)
		{
			if (!_inBattle || e.Unit == null || !e.Unit.Alive) return;
			if (_regen.TryGetValue(e.Unit, out double pct)) e.Unit.Hp = Math.Min(e.Unit.MaxHp, e.Unit.Hp + Math.Max(1, (int)Math.Round(e.Unit.MaxHp * pct / 100)));
		}

		private void OnFell(BattleUnitFell e)
		{
			if (!_inBattle || Run == null || e.Unit == null) return;
			if (e.Unit.IsMonster)
			{
				Run.Tally.Kills++;
				double heal = Modifiers.Sum(Content, Run, "kill", x => x.HealPercent);
				if (heal > 0 && e.By != null && !e.By.IsMonster && e.By.Alive) e.By.Hp = Math.Min(e.By.MaxHp, e.By.Hp + Math.Max(1, (int)Math.Round(e.By.MaxHp * heal / 100)));
				return;
			}
			var revival = Modifiers.Revival(Content, Run);
			if (revival != null)
			{
				Run.Used[revival.Value.Passive] = (Run.Used.TryGetValue(revival.Value.Passive, out int u) ? u : 0) + 1;
				e.Unit.Revive(Math.Max(1, (int)Math.Round(e.Unit.MaxHp * revival.Value.HpPercent / 100)));
				Game.Log("rogue: " + Content.Passive(revival.Value.Passive)?.Name + " raises " + e.Unit.Name);
			}
		}

		private void OnRewards(BattleRewards e)
		{
			if (!_inBattle || Run == null) return;
			double gil = Modifiers.Sum(Content, Run, "rewards", x => x.GilPercent), exp = Modifiers.Sum(Content, Run, "rewards", x => x.ExpPercent);
			e.Gil = (int)Math.Round(e.Gil * (1 + gil / 100));
			e.Exp = (int)Math.Round(e.Exp * Content.Run.ExpScale * (1 + exp / 100));
			Run.Tally.GilEarned += e.Gil;
			Game.Log("rogue: spoils " + e.Gil + " gil, " + e.Exp + " exp");
		}

		private void OnBattleEnded(BattleEnded e)
		{
			if (!_inBattle) return;
			_inBattle = false;
			Game.Battle.EscapeAllowed = true;
			if (Run == null) return;
			string kind = Run.Next?.Kind ?? "battle";
			Game.Log("rogue: the " + kind + " is " + (e.Result == BattleResult.Won ? "won" : "lost"));
			if (e.Result != BattleResult.Won)
			{
				EndRun("lost");
				return;
			}
			Run.Tally.Battles++;
			if (kind == "elite") Run.Tally.Elites++;
			if (kind == "boss") Run.Tally.Bosses++;
			if (kind == "boss" && Run.Act + 1 >= Content.Run.Acts.Count) { EndRun("won"); return; }
			Run.Pending = RewardGenerator.Roll(Content, GameData(), Run, Party(), kind, Run.Stream("reward"));
			Run.Rerolls = 0;
			Save();
			After("rogue-reward");
		}

		// ---------------------------------------------------------------- rewards

		/// <summary>Takes one of the choices; returns what it did, for a notice.</summary>
		public string Take(int index)
		{
			if (Run == null || index < 0 || index >= Run.Pending.Count) return null;
			RewardOption o = Run.Pending[index];
			string said = Apply(o);
			Run.Tally.Rewards++;
			if (o.Rarity != null && Content.RarityIndex(o.Rarity) >= 2 && o.Kind == "equipment") Run.Tally.RareItems++;
			Run.Pending.Clear();
			Advance();
			return said;
		}

		private string Apply(RewardOption o)
		{
			switch (o.Kind)
			{
				case "equipment":
				{
					int id = int.Parse(o.Ref, CultureInfo.InvariantCulture);
					Game.Party.AddItem(id, 1);
					Item item = Game.Items.Find(id);
					int best = -1, gain = 0;
					for (int h = 0; h < 4; h++)
					{
						if (!Game.Party.CanEquip(h, id)) continue;
						EquipSlot slot = item.Slot == EquipSlot.LeftHand && item.Category == ItemCategory.Weapon ? EquipSlot.RightHand : item.Slot;
						Item worn = Game.Items.Find(Game.Party.Equipped(h, slot));
						int now = worn == null ? 0 : (item.Category == ItemCategory.Weapon ? worn.Attack : worn.Defense);
						int then = item.Category == ItemCategory.Weapon ? item.Attack : item.Defense;
						if (then - now > gain) { gain = then - now; best = h; }
					}
					if (best >= 0 && Game.Party.Equip(best, id)) return Game.Party.Member(best).Name + " equips " + o.Name + ".";
					return o.Name + " is in the bag.";
				}
				case "spell":
				{
					int id = int.Parse(o.Ref, CultureInfo.InvariantCulture);
					if (o.Hero >= 0 && Game.Party.LearnSpell(o.Hero, id)) return Game.Party.Member(o.Hero).Name + " learns " + o.Name + ".";
					return "No room for " + o.Name + ".";
				}
				case "passive": Run.Add(o.Ref); return o.Name + " is yours.";
				case "gil": Game.Party.Gil += o.Amount; return o.Amount + " gil.";
				case "heal": Game.Party.HealAll(); return "The party is rested.";
				case "experience": for (int h = 0; h < 4; h++) Game.Party.GiveExperience(h, o.Amount); return "Everyone gains " + o.Amount + " experience.";
				default: return null;
			}
		}

		public int RerollCost => Run == null ? 0 : RewardGenerator.RerollCost(Content, Run.Rerolls);

		public bool Reroll()
		{
			if (Run == null || Game.Party.Gil < RerollCost) return false;
			Game.Party.Gil -= RerollCost;
			Run.Rerolls++;
			Run.Tally.Rerolls++;
			Run.Pending = RewardGenerator.Roll(Content, GameData(), Run, Party(), Run.Next?.Kind ?? "battle", Run.Stream("reward"));
			Save();
			return true;
		}

		/// <summary>
		/// The crystals of the acts cleared (the first this many acts' bosses): their jobs opened - the game's own flags, so
		/// the game's Job screen offers them - and the menu's Job with them. A new game starts with none; Continue opens them again.
		/// </summary>
		private void OpenCrystals(int actsCleared)
		{
			bool any = false;
			for (int a = 0; a < actsCleared && a < Content.Run.Acts.Count; a++)
			{
				CrystalDef crystal = Content.Run.Acts[a].Crystal;
				if (crystal == null) continue;
				any = true;
				foreach (int job in crystal.Jobs) if (job >= 0 && job < 23) Game.Flags.Set(0u, (uint)(901 + job), true);   // EVENT_JOB_FLAG: 901 + the job
			}
			if (!any) return;
			Game.Flags.Set(0u, 901u, true);   // the Freelancer
			Game.Flags.Set(0u, 36u, true);    // the menu's Job, as the Wind Crystal's gift opens it
			Game.Log("rogue: crystals of " + actsCleared + " act(s) open");
		}

		/// <summary>The heroes below a level raised to it (an act's floor: acts.json's "level").</summary>
		private void RaiseTo(int level)
		{
			if (level <= 0) return;
			for (int h = 0; h < 4; h++)
			{
				PartyMember m = Game.Party.Member(h);
				if (m != null && m.Level < level) Game.Party.SetLevel(h, level);
			}
			Game.Log("rogue: the party is raised to level " + level + " for act " + (Run.Act + 1));
		}

		/// <summary>Whether the next act's battlefield is being warped to: the camp opens there (a screen closes to let the warp happen).</summary>
		public bool Warping { get; private set; }

		/// <summary>On to the next step: the next battle, or after a boss the next act (healed, on its battlefield).</summary>
		private void Advance()
		{
			Warping = false;
			bool actDone = Run.Next?.Kind == "boss";
			if (actDone)
			{
				CrystalDef crystal = Act?.Crystal;
				Run.Act++;
				Run.Step = 0;
				Run.Used.Clear();
				RaiseTo(Act?.Level ?? 0);
				if (crystal != null)
				{
					OpenCrystals(Run.Act);
					RogueCamp.News = "The " + crystal.Name + " shines: " + (crystal.Text ?? "new jobs") + " open - change jobs under Party > Job.";
				}
				Game.Party.HealAll();
			}
			else Run.Step++;
			Run.Next = EncounterGenerator.Build(Content, GameData(), Run, Run.Stream("encounter"));
			Save();
			if (actDone && Biome != null && !string.Equals(Game.Field.Map, Biome.Backdrop, StringComparison.OrdinalIgnoreCase))
			{
				_openNext = "rogue-camp";
				Warping = true;
				Game.Field.Warp(Biome.Backdrop, Aside);
			}
			// Otherwise the reward screen goes on to the camp itself, without leaving the menus.
		}

		/// <summary>The run over; the summary opens once the field is quiet (after a battle), or the caller opens it (from a screen).</summary>
		public void EndRun(string outcome, bool openSummary = true)
		{
			if (Run == null) return;
			Run.Outcome = outcome;
			Ended = Run;
			Run = null;
			try { if (File.Exists(RunFile)) File.Delete(RunFile); } catch (Exception) { }
			Game.Log("rogue: the run is over (" + outcome + "): act " + (Ended.Act + 1) + ", " + Ended.Tally.Battles + " battles, " + Ended.Tally.Kills + " monsters");
			// A wipe leaves the heroes down: up again for the summary and the next run.
			Game.Party.HealAll();
			if (openSummary) After("rogue-summary");
		}

		// ---------------------------------------------------------------- saving

		public void Save()
		{
			if (Run == null) return;
			try
			{
				Run.Heroes = Enumerable.Range(0, 4).Select(h => Game.Party.Export(h)).ToList();
				Run.Bag = Game.Party.Items.Select(s => new[] { s.ItemId, s.Count }).ToList();
				Run.Gil = Game.Party.Gil;
				Directory.CreateDirectory(Folder);
				File.WriteAllText(RunFile, Run.ToJson());
			}
			catch (Exception ex) { Game.Log("rogue: not saved: " + ex.Message); }
		}

		/// <summary>Continue Run: the saved run back, its heroes, bag and gil put on the new game's party.</summary>
		public bool Continue()
		{
			Warping = false;
			try
			{
				Run = RunState.FromJson(File.ReadAllText(RunFile));
				RestoreParty();
				OpenCrystals(Run.Act);
				Game.Log("rogue: a run continues: act " + (Run.Act + 1) + ", battle " + (Run.Step + 1));
				if (Biome != null && !string.Equals(Game.Field.Map, Biome.Backdrop, StringComparison.OrdinalIgnoreCase))
				{
					_openNext = Run.Pending.Count > 0 ? "rogue-reward" : "rogue-camp";
					Warping = true;
					Game.Field.Warp(Biome.Backdrop, Aside);
				}
				// Otherwise the hub goes on to the camp (or the reward waiting) in the menus.
				return true;
			}
			catch (Exception ex) { Game.Log("rogue: no run to continue: " + ex.Message); Run = null; return false; }
		}

		private void RestoreParty()
		{
			if (Run == null) return;
			for (int h = 0; h < 4; h++)
			{
				if (h > 0) Game.Party.AddMember(h);
				if (h < Run.Heroes.Count && Run.Heroes[h] != null) Game.Party.Import(h, Run.Heroes[h]);
			}
			foreach (ItemStack s in Game.Party.Items.ToList()) Game.Party.RemoveItem(s.ItemId, s.Count);
			foreach (int[] s in Run.Bag) if (s.Length >= 2) Game.Party.AddItem(s[0], s[1]);
			Game.Party.Gil = Run.Gil;
		}

		public void Abandon() => EndRun("abandoned", openSummary: false);

		public override IEnumerable<string> DebugLines()
		{
			if (!Active) yield break;
			if (Run == null) { yield return "rogue: no run"; yield break; }
			yield return "rogue: seed " + Run.Seed + "  act " + (Run.Act + 1) + " step " + (Run.Step + 1) + "  next " + Run.Next?.Kind + " " + Run.Next?.Name;
			yield return "rogue: passives " + string.Join(", ", Run.Passives.Select(p => p.Id + (p.Stacks > 1 ? " x" + p.Stacks : "")));
		}
	}
}
