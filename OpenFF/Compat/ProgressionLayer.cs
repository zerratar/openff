// How each hero grows, by the mods' character definitions (Shared/Data/ModCharacters.cs):
// FF3's jobs (the game as it is), FF4's class (a fixed job, learning by level), or FF5's
// mastery (job ladders climbed on ABP, learned abilities set into free command slots;
// Shared/Data/ModJobs.cs). The game's own code keeps running underneath - this layer sits
// on the few points where the systems differ:
//
//   - Player.changeJob: after the game has set the job's own commands, a mastery hero's are
//     rewritten from the ladder (free slots filled from what they set), the passives in play
//     go into the job's passive slots, and the penalty time is cleared (FF5 has none).
//   - Player.isEquipItem: what the job may wear or cast is widened by the grants of the
//     abilities in play (FF5's "Equip Swords", or a set "white-magic" casting white spells).
//   - BattleWin: on a win, ABP for each mastery hero's job; the ladder climbs, abilities are
//     learned, the notices say so. After the experience, a class hero learns its level's spells.
//   - The save: the ladders' state (ABP, levels, what is set) is not in the game's save
//     format, so it rides beside it - save.progression.json in the save folder, by slot -
//     written when the game saves and read when it loads.
//
// State lives here by hero slot, not in the Player: four HeroStates, each with ABP and level
// per job and the abilities set into free slots.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenFF.Content;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal static class ProgressionLayer
	{
		private const int JobCount = 23;
		/// <summary>The most free slots a ladder's commands can leave (FF5's Mime has three).</summary>
		public const int FreeSlotMax = 3;

		/// <summary>One hero's climb: ABP toward the next step and the level reached, per job number (FF3's 0..22 and the mods' own from 23); the abilities set into free slots (0 for none); the job of the mod's own held, -1 for one of FF3's.</summary>
		internal sealed class HeroState
		{
			public Dictionary<int, int> Abp = new Dictionary<int, int>();
			public Dictionary<int, int> Level = new Dictionary<int, int>();
			public int[] Set = new int[FreeSlotMax];
			public int Job = -1;
			/// <summary>The HP the boost passives currently add to the hero's limit, so a recompute takes the old boost off before adding the new.</summary>
			public int HpBoostApplied;
			/// <summary>The spell charges the MP boost passives have added to every open level's limit.</summary>
			public int MpBoostApplied;
			public int AbpOf(int job) => Abp.TryGetValue(job, out int v) ? v : 0;
			public int LevelOf(int job) => Level.TryGetValue(job, out int v) ? v : 0;
		}

		private static readonly HeroState[] _heroes = { new HeroState(), new HeroState(), new HeroState(), new HeroState() };
		private static List<ModJob> _ladders;
		private static readonly HashSet<int> _noLadderNoted = new HashSet<int>();

		/// <summary>The loaded ladders (defs/jobs), read once with the characters' roots.</summary>
		public static IReadOnlyList<ModJob> Ladders
		{
			get
			{
				if (_ladders != null) return _ladders;
				_ladders = new List<ModJob>();
				if (!GameProfile.Ff3Party) return _ladders;
				List<string> notes = new List<string>();
				try { _ladders = ModJobs.Load(ModCharactersLayer.Roots(), notes); }
				catch (Exception ex) { notes.Add(ex.Message); }
				foreach (string note in notes) Log.Write(LogChannel.General, "jobs: " + note);
				if (_ladders.Count > 0) Log.Write(LogChannel.General, "jobs: " + _ladders.Count + " ladders: " + string.Join(", ", _ladders.Select(j => j.Name + " (" + j.Abilities.Count + " steps, " + j.TotalAbp + " ABP" + (j.Inherits ? ", inherits" : "") + ")")));
				return _ladders;
			}
		}

		/// <summary>How a hero grows: the definition's word, Jobs where the mods say nothing.</summary>
		public static Progression ModeOf(int playerId) => ModCharactersLayer.Definition(playerId)?.Progression ?? Progression.Jobs;

		public static bool IsMastery(int playerId) => ModeOf(playerId) == Progression.Mastery;

		/// <summary>Whether any hero is on the mastery progression - what shows the Abilities menu.</summary>
		public static bool AnyMastery
		{
			get
			{
				for (int i = 0; i < 4; i++) if (IsMastery(i)) return true;
				return false;
			}
		}

		public static ModJob LadderOf(int job) => Ladders.FirstOrDefault(j => j.JobNumber == job);

		public static HeroState StateOf(int playerId) => playerId >= 0 && playerId < 4 ? _heroes[playerId] : new HeroState();

		/// <summary>Every job number in play: FF3's 23, then the mods' own.</summary>
		public static IEnumerable<int> AllJobs
		{
			get
			{
				for (int j = 0; j < JobCount; j++) yield return j;
				foreach (ModJob l in Ladders) if (l.IsOwn) yield return l.JobNumber;
			}
		}

		/// <summary>A job's name: FF3's, or the mod's own ladder's.</summary>
		public static string JobName(int job)
		{
			ModJob l = LadderOf(job);
			if (l != null) return l.Name;
			return job >= 0 && job < ModCharacters.Jobs.Length ? ModCharacters.Jobs[job].Name : "job " + job;
		}

		/// <summary>The FF3 job the game's party holds for a job number: itself, or a job of the mod's own's base.</summary>
		public static int BaseOf(int job)
		{
			ModJob l = LadderOf(job);
			return l != null && l.IsOwn ? l.BaseJob : job;
		}

		/// <summary>
		/// The job a hero really holds: the job of the mod's own remembered for them when the game's
		/// party holds its base, else the game's own job. What every ladder question is asked about.
		/// </summary>
		public static int HeldJob(int playerId, int nowJob)
		{
			HeroState s = StateOf(playerId);
			if (s.Job >= 0)
			{
				ModJob own = LadderOf(s.Job);
				if (own != null && own.IsOwn && own.BaseJob == nowJob) return s.Job;
			}
			return nowJob;
		}

		public static int HeldJob(GlobalScope.pl.Player player) => HeldJob(player.playerId(), player.jobManager().nowJob());

		/// <summary>The FF3 job whose figures a hero wears: a job of the mod's own says which (its base's unless told); the game's job otherwise.</summary>
		public static int LookJob(int playerId, int nowJob)
		{
			try
			{
				int held = HeldJob(playerId, nowJob);
				if (held == nowJob) return nowJob;
				ModJob l = LadderOf(held);
				int look = l?.LookJob ?? -1;
				return look >= 0 ? look : nowJob;
			}
			catch (Exception) { return nowJob; }
		}

		/// <summary>The name the menus print for a hero's job: the mod's own job's, or null for the game's own text.</summary>
		public static string JobNameOverride(int playerId, int nowJob)
		{
			try
			{
				int held = HeldJob(playerId, nowJob);
				return held == nowJob ? null : LadderOf(held)?.Name;
			}
			catch (Exception) { return null; }
		}

		/// <summary>Whether a hero may take a job: FF3's when its crystal has opened it, a mod's own when its base is open.</summary>
		public static bool JobOpen(int job)
		{
			try
			{
				int b = BaseOf(job);
				if (b < 0 || b >= JobCount) return false;
				if (b == 0) return true;   // the Freelancer is everyone's from the start (the game's Job menu sets its flag itself)
				for (byte i = 0; i < 4; i++)
				{
					GlobalScope.pl.Player p = GlobalScope.pl.PlayerParty.instance().player(i);
					if (p != null && p.isEnable() && p.jobManager().nowJob() == b) return true;   // a job someone holds is open, whatever the flags say (a --map start, a definition's starting job)
				}
				return GlobalScope.evt.CEventManager.getInstance().FlagMng().get(0u, (uint)GlobalScope.evt.EVENT_JOB_FLAG[b]) != 0;
			}
			catch (Exception) { return job == 0; }
		}

		private static bool _changing;

		/// <summary>
		/// A mastery hero takes a job by number - one of FF3's (the game's own change, no penalty) or a
		/// job of the mod's own (the game's party takes its base, the layer remembers the job). False
		/// when the hero is not on the progression, the job is unknown or not open, or fixed.
		/// </summary>
		public static bool ChangeJob(int playerId, int job)
		{
			if (!IsMastery(playerId) || ModCharactersLayer.JobFixed(playerId)) return false;
			ModJob ladder = LadderOf(job);
			bool own = ladder != null && ladder.IsOwn;
			if (!own && (job < 0 || job >= JobCount)) return false;
			if (!JobOpen(job)) return false;
			GlobalScope.pl.Player player;
			try { player = GlobalScope.pl.PlayerParty.instance().playerForId((byte)playerId); } catch (Exception) { return false; }
			if (player == null) return false;
			HeroState s = StateOf(playerId);
			_changing = true;
			try
			{
				s.Job = own ? job : -1;
				player.changeJob((GlobalScope.pl.JOB_TYPE)BaseOf(job));
				Log.Write(LogChannel.General, "progression: " + player.name() + " is now a " + JobName(job) + (own ? " (on the " + ModCharacters.Jobs[BaseOf(job)].Name + ")" : ""));
				// The field's figure follows, as after the game's own Job menu.
				try
				{
					if (EngineApi.InWorld) { GlobalScope.CCastCommandTransit.getInstance().cast_BaseSystem().changePlayerCharDisplay(); Log.Write(LogChannel.File, "progression: the field figure is j" + (ModCharactersLayer.ModelSet(playerId) + 1) + (LookJob(playerId, player.jobManager().nowJob()) + 1).ToString("D2")); }
					else Log.Write(LogChannel.File, "progression: not in the world; the figure follows on the next map");
				}
				catch (Exception ex) { Log.Write(LogChannel.File, "progression: figure: " + ex.Message); }
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "progression: job change: " + ex.Message); return false; }
			finally { _changing = false; }
			return true;
		}

		/// <summary>A new game: every ladder at the bottom, nothing set.</summary>
		public static void Reset()
		{
			for (int i = 0; i < 4; i++) _heroes[i] = new HeroState();
		}

		/// <summary>A hero the definitions have just set up (new game): commands by the ladder; the class's spells for its level once the world is up (their names live in the messages, which the boot has not loaded).</summary>
		public static void OnPartyReady(GlobalScope.pl.Player player)
		{
			try
			{
				Refresh(player);
				_learnPending = true;
				// --set-ability=<hero>:<slot>:<word>[,...]: a free slot filled at the start, learned or not - for
				// trying a command or passive without climbing to it (test drives).
				string given = Options.Get("set-ability");
				if (!string.IsNullOrEmpty(given))
				{
					foreach (string part in given.Split(','))
					{
						string[] p = part.Split(':');
						if (p.Length != 3 || !int.TryParse(p[0], out int hero) || hero != player.playerId() || !int.TryParse(p[1], out int slot)) continue;
						int id = ModJobs.AbilityId(Ladders, p[2]);
						if (id <= 0 || !IsMastery(hero) || slot < 0 || slot >= FreeSlotMax) { Log.Write(LogChannel.General, "progression: --set-ability " + part + ": no such ability, or not a mastery hero"); continue; }
						StateOf(hero).Set[slot] = id;
						ApplyCommands(player);
						Log.Write(LogChannel.General, "progression: --set-ability: " + player.name() + " slot " + (slot + 1) + " = " + AbilityName(id) + " (" + id + ")");
					}
				}
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "progression: " + ex.Message); }
		}

		private static bool _learnPending;

		/// <summary>Each frame: a pending learn-by-level pass runs once the world (and its texts) is up.</summary>
		public static void Tick()
		{
			if (!_learnPending || !GameProfile.Ff3Party) return;
			try
			{
				if (!EngineApi.InWorld) return;
				_learnPending = false;
				Log.Write(LogChannel.File, "progression: the world is up; class heroes learn by level");
				for (byte i = 0; i < 4; i++)
				{
					GlobalScope.pl.Player player = GlobalScope.pl.PlayerParty.instance().player(i);
					if (player != null && player.isEnable()) LearnByLevel(player, announce: false);
				}
			}
			catch (Exception ex) { _learnPending = false; Log.Write(LogChannel.General, "progression: learning: " + ex.Message); }
		}

		// ---- the ladder ----

		public static int JobLevel(int playerId, int job) => StateOf(playerId).LevelOf(job);
		public static int Abp(int playerId, int job) => StateOf(playerId).AbpOf(job);

		/// <summary>ABP the next step costs, or 0 at the top (or without a ladder).</summary>
		public static int AbpToNext(int playerId, int job)
		{
			ModJob ladder = LadderOf(job);
			if (ladder == null) return 0;
			int level = JobLevel(playerId, job);
			return level < ladder.Abilities.Count ? ladder.Abilities[level].Abp : 0;
		}

		public static bool IsMastered(int playerId, int job)
		{
			ModJob ladder = LadderOf(job);
			return ladder != null && ladder.Abilities.Count > 0 && JobLevel(playerId, job) >= ladder.Abilities.Count;
		}

		/// <summary>Every ability a hero has learned, across the ladders, in ladder order.</summary>
		public static List<int> Learned(int playerId)
		{
			List<int> ids = new List<int>();
			foreach (ModJob ladder in Ladders)
			{
				int level = JobLevel(playerId, ladder.JobNumber);
				for (int i = 0; i < level && i < ladder.Abilities.Count; i++) if (!ids.Contains(ladder.Abilities[i].Id)) ids.Add(ladder.Abilities[i].Id);
			}
			return ids;
		}

		/// <summary>The passives a hero has innately in a job: the game's own, the ladder's innate, and - for a job that inherits - the innate passives of every mastered job.</summary>
		public static List<int> Innate(int playerId, int job)
		{
			List<int> ids = new List<int>();
			void Add(int id) { if (id > 0 && !ids.Contains(id)) ids.Add(id); }
			void OfJob(int j)
			{
				foreach (int p in Ff3Abilities.JobPassives(BaseOf(j))) Add(p);   // the game's own (Knight's Cover): a job of the mod's own has its base's
				ModJob l = LadderOf(j);
				if (l != null) foreach (string w in l.Innate) Add(ModJobs.AbilityId(Ladders, w));
			}
			OfJob(job);
			ModJob ladder = LadderOf(job);
			if (ladder != null && ladder.Inherits)
			{
				foreach (int j in AllJobs) if (j != job && IsMastered(playerId, j)) OfJob(j);
			}
			return ids;
		}

		/// <summary>Everything in play for a hero in their job: innate passives and what is set in free slots.</summary>
		public static List<int> Active(int playerId, int job)
		{
			List<int> ids = Innate(playerId, job);
			ModJob ladder = LadderOf(job);
			int free = ladder?.FreeSlots ?? (Ladders.Count > 0 ? 1 : 0);
			HeroState s = StateOf(playerId);
			for (int i = 0; i < free && i < FreeSlotMax; i++) if (s.Set[i] > 0 && !ids.Contains(s.Set[i])) ids.Add(s.Set[i]);
			return ids;
		}

		public static bool Has(GlobalScope.pl.Player player, int abilityId)
		{
			if (!IsMastery(player.playerId())) return false;
			return Active(player.playerId(), HeldJob(player)).Contains(abilityId);
		}

		/// <summary>Whether a battle character - a hero - has an ability in play; false for monsters and off the progression. Safe to call from the battle's formulas.</summary>
		public static bool HasInBattle(GlobalScope.btl.BaseBattleCharacter character, int abilityId)
		{
			try
			{
				if (!GameProfile.Ff3Party || character == null || character.breed() != 0) return false;
				GlobalScope.pl.Player player = (character as GlobalScope.btl.BattlePlayer)?.player();
				return player != null && Has(player, abilityId);
			}
			catch (Exception) { return false; }
		}

		/// <summary>The command layout of a hero's job: ids, -1 for free slots. Without a ladder the game's own set with the third command free.</summary>
		public static int[] CommandLayout(int job)
		{
			ModJob ladder = LadderOf(job);
			if (ladder != null) return ladder.CommandIds();
			job = BaseOf(job);
			int[] ids = job >= 0 && job < Ff3Abilities.JobCommands.Length ? (int[])Ff3Abilities.JobCommands[job].Clone() : new[] { 1, 46, 3, 4 };
			ids[2] = -1;
			return ids;
		}

		/// <summary>How many free slots a hero's job leaves.</summary>
		public static int FreeSlots(int job) => CommandLayout(job).Count(c => c < 0);

		/// <summary>Sets a learned ability into a free slot (0 clears it); false when not learned, a passive twice, or no such slot. The commands follow at once.</summary>
		public static bool SetAbility(int playerId, int slot, int abilityId)
		{
			if (!IsMastery(playerId) || slot < 0 || slot >= FreeSlotMax) return false;
			HeroState s = StateOf(playerId);
			if (abilityId != 0)
			{
				if (!Learned(playerId).Contains(abilityId)) return false;
				for (int i = 0; i < FreeSlotMax; i++) if (i != slot && s.Set[i] == abilityId) s.Set[i] = 0;   // the same ability once
			}
			s.Set[slot] = abilityId;
			try
			{
				GlobalScope.pl.Player player = GlobalScope.pl.PlayerParty.instance().playerForId((byte)playerId);
				if (player != null) { ApplyCommands(player); player.updateParameter(); }   // the stats follow a carried ability
			}
			catch (Exception) { }
			return true;
		}

		/// <summary>Gives ABP toward a hero's job and climbs the ladder; the steps reached, as notices. Nothing without a ladder for the job.</summary>
		public static List<string> GiveAbp(int playerId, int job, int amount)
		{
			List<string> news = new List<string>();
			if (amount <= 0 || job < 0) return news;
			ModJob ladder = LadderOf(job);
			if (ladder == null)
			{
				if (_noLadderNoted.Add(job)) Log.Write(LogChannel.File, "progression: no ladder for " + JobName(job) + " - its ABP go nowhere (defs/jobs/<id>.json gives it one)");
				return news;
			}
			HeroState s = StateOf(playerId);
			int level = s.LevelOf(job), abp = s.AbpOf(job);
			if (level >= ladder.Abilities.Count) return news;   // mastered: nothing more to climb
			abp += amount;
			string name = null;
			try { name = GlobalScope.pl.PlayerParty.instance().playerForId((byte)playerId)?.name(); } catch (Exception) { }
			name ??= "hero " + playerId;
			while (level < ladder.Abilities.Count && abp >= ladder.Abilities[level].Abp)
			{
				ModJobAbility step = ladder.Abilities[level];
				abp -= step.Abp;
				level++;
				bool top = level >= ladder.Abilities.Count;
				news.Add(name + ": " + ladder.Name + " level " + level + " - learned " + step.ShownName + (top ? " - " + ladder.Name + " mastered!" : ""));
				if (top) abp = 0;
			}
			s.Level[job] = level;
			s.Abp[job] = abp;
			try
			{
				GlobalScope.pl.Player player = GlobalScope.pl.PlayerParty.instance().playerForId((byte)playerId);
				if (player != null && news.Count > 0) { ApplyCommands(player); player.updateParameter(); }   // a mastered job may pass its stats on
			}
			catch (Exception) { }
			return news;
		}

		// ---- the game's hooks ----

		/// <summary>After Player.changeJob: a mastery hero's commands and passives from the ladder, no penalty time. A change the game's own Job menu made leaves any job of the mod's own the hero held (they picked one of FF3's).</summary>
		public static void OnJobChanged(GlobalScope.pl.Player player)
		{
			if (player == null || !GameProfile.Ff3Party) return;
			try
			{
				if (!IsMastery(player.playerId())) return;
				if (!_changing) StateOf(player.playerId()).Job = -1;
				Refresh(player);
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "progression: job change: " + ex.Message); }
		}

		/// <summary>A mastery hero's commands and passives from the ladder of the job they hold, and no penalty time - after a change, a load, a new game.</summary>
		public static void Refresh(GlobalScope.pl.Player player)
		{
			if (player == null || !GameProfile.Ff3Party) return;
			try
			{
				if (!IsMastery(player.playerId())) return;
				ApplyCommands(player);
				player.jobPenaltyTime_set(0);
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "progression: refresh: " + ex.Message); }
		}

		/// <summary>What an empty free slot shows in battle: Guard (the ability table has no blank row - id 0 is nobody's, and the command window needs a name).</summary>
		private const int EmptySlotCommand = 3;

		/// <summary>Whether an ability id is a passive: FF3's by its table, a mod's own by its ladder step (a command when the step says so).</summary>
		public static bool IsPassive(int id)
		{
			Ff3Ability a = Ff3Abilities.ById(id);
			if (a != null) return a.Passive;
			return !ModJobs.IsOwnCommand(id);
		}

		/// <summary>The four commands from the layout, free slots from what is set (Guard where nothing is); the job's passive slots from the passives in play the engine acts on.</summary>
		public static void ApplyCommands(GlobalScope.pl.Player player)
		{
			int playerId = player.playerId();
			if (!IsMastery(playerId)) return;
			int job = HeldJob(player);
			int[] layout = CommandLayout(job);
			HeroState s = StateOf(playerId);
			int free = 0;
			GlobalScope.pl.Command command = player.jobManager().command();
			for (int i = 0; i < 4; i++)
			{
				int id = layout[i];
				if (id < 0)
				{
					int set = free < FreeSlotMax ? s.Set[free] : 0;
					free++;
					Ff3Ability a = Ff3Abilities.ById(set);
					id = (a != null && !a.Passive) || ModJobs.IsOwnCommand(set) ? set : EmptySlotCommand;   // a mod's own command (64..99) goes in as it is: BattleCommands names and plays it
				}
				command.setCommandId(i, (sbyte)id);
			}
			List<int> active = Active(playerId, job);
			GlobalScope.pl.PlayerAbility ability = player.jobManager().nowJobParameter().ability().playerAbility();
			int slot = 0;
			for (int i = 0; i < ability.passive_.Length; i++) ability.passive_[i] = 0;
			foreach (int id in active)
			{
				Ff3Ability a = Ff3Abilities.ById(id);
				if (a == null || !a.Passive || (id != 10 && id != 28)) continue;   // Cover, Alchemy: the ones the game's formulas read from the slots; Counterattack the layer answers for itself
				if (slot < ability.passive_.Length) ability.passive_[slot++] = (short)id;
			}
		}

		/// <summary>
		/// FF5's stat modifiers for a mastery hero in their job, on FF3's five stats: the held job's
		/// ladder modifiers; for a job that inherits, the best positive modifier of every mastered job
		/// where it beats the held job's (penalties never pass on); and for each ability in play whose
		/// step carries, its ladder job's positive modifiers where they beat what stands. Zeros for a
		/// hero off the progression.
		/// </summary>
		public static int[] StatModifiers(int playerId, int job)
		{
			int[] mods = new int[5];
			if (!IsMastery(playerId)) return mods;
			ModJob held = LadderOf(job);
			if (held != null) for (int i = 0; i < 5; i++) mods[i] = held.Stats[i];
			if (held != null && held.Inherits)
			{
				foreach (int j in AllJobs)
				{
					if (j == job || !IsMastered(playerId, j)) continue;
					ModJob l = LadderOf(j);
					if (l == null) continue;
					for (int i = 0; i < 5; i++) if (l.Stats[i] > 0 && l.Stats[i] > mods[i]) mods[i] = l.Stats[i];
				}
			}
			List<int> active = Active(playerId, job);
			List<int> innate = Innate(playerId, job);
			foreach (ModJob ladder in Ladders)
			{
				if (ladder.JobNumber == job || !ladder.HasStats) continue;
				int level = JobLevel(playerId, ladder.JobNumber);
				for (int s = 0; s < ladder.Abilities.Count; s++)
				{
					ModJobAbility step = ladder.Abilities[s];
					if (!step.Carries || !active.Contains(step.Id) || (s >= level && !innate.Contains(step.Id))) continue;
					for (int i = 0; i < 5; i++) if (ladder.Stats[i] > 0 && ladder.Stats[i] > mods[i]) mods[i] = ladder.Stats[i];
				}
			}
			return mods;
		}

		/// <summary>After Player.updateParameter has the stats with equipment and the penalty: the ladder's modifiers on top, 1..99.</summary>
		public static void ApplyStats(GlobalScope.pl.Player player)
		{
			if (player == null || !GameProfile.Ff3Party) return;
			try
			{
				int playerId = player.playerId();
				if (!IsMastery(playerId)) return;
				ApplyHpBoost(player);
				ApplyMpBoost(player);
				int[] mods = StatModifiers(playerId, HeldJob(player));
				if (mods.All(m => m == 0)) return;
				GlobalScope.ys.BodyParameter b = player.bodyAndBonus();
				b.strength().set(Math.Clamp(b.strength().get() + mods[0], 1, 99));
				b.dexterity().set(Math.Clamp(b.dexterity().get() + mods[1], 1, 99));
				b.vitality().set(Math.Clamp(b.vitality().get() + mods[2], 1, 99));
				b.intelligence().set(Math.Clamp(b.intelligence().get() + mods[3], 1, 99));
				b.mind().set(Math.Clamp(b.mind().get() + mods[4], 1, 99));
			}
			catch (Exception ex) { Log.Write(LogChannel.File, "progression: stats: " + ex.Message); }
		}

		/// <summary>
		/// FF5's HP +10/20/30% passives (Ff3Abilities 90..92) in play add up on the hero's HP limit. The game
		/// grows the limit by accumulation (levelUp adds to it), so the boost is kept as an amount: the old
		/// one comes off, the new one goes on, current HP follows a rise and is clamped on a fall.
		/// </summary>
		/// <summary>
		/// FF5's MP boosts on FF3's spell charges: +n charges to every level's limit that has any. Kept as
		/// an amount like the HP boost; setMp (a job change, a level) resets the limits and tells us so.
		/// </summary>
		private static void ApplyMpBoost(GlobalScope.pl.Player player)
		{
			HeroState s = StateOf(player.playerId());
			int boost = Active(player.playerId(), HeldJob(player)).Sum(Ff3Abilities.MpBoost);
			if (boost == s.MpBoostApplied) return;
			for (int i = 0; i < 8; i++)
			{
				GlobalScope.ys.MPoint<int> mp = player.mp(i);
				int baseLimit = mp.getLimit() - s.MpBoostApplied;
				if (baseLimit <= 0) continue;   // a level the job has no charges at stays closed
				mp.setLimit(baseLimit + boost);
				if (boost > s.MpBoostApplied) mp.setNow(Math.Min(baseLimit + boost, mp.getNow() + (boost - s.MpBoostApplied)));
				else if (mp.getNow() > baseLimit + boost) mp.setNow(baseLimit + boost);
			}
			s.MpBoostApplied = boost;
		}

		/// <summary>setMp put the tables' limits back: nothing of the boost is on them now.</summary>
		public static void OnMpReset(GlobalScope.pl.Player player)
		{
			try
			{
				if (!GameProfile.Ff3Party || player == null || !IsMastery(player.playerId())) return;
				StateOf(player.playerId()).MpBoostApplied = 0;
				ApplyMpBoost(player);
			}
			catch (Exception) { }
		}

		/// <summary>FF5's First Strike and Vigilance on the battle's opening: a hero with Vigilance bars a back attack; one with First Strike turns a plain opening into the party's, one time in four.</summary>
		public static GlobalScope.btl.BATTLE_OPENING_TYPE AdjustOpening(GlobalScope.btl.BATTLE_OPENING_TYPE type)
		{
			try
			{
				if (!GameProfile.Ff3Party || !AnyMastery) return type;
				bool vigilance = false, first = false;
				for (byte i = 0; i < 4; i++)
				{
					GlobalScope.pl.Player p = GlobalScope.pl.PlayerParty.instance().player(i);
					if (p == null || !p.isEnable() || !IsMastery(p.playerId())) continue;
					vigilance |= Has(p, Ff3Abilities.Vigilance);
					first |= Has(p, Ff3Abilities.FirstStrike);
				}
				if (vigilance && type == GlobalScope.btl.BATTLE_OPENING_TYPE.BACK_ATTACK) { type = GlobalScope.btl.BATTLE_OPENING_TYPE.NORMAL_ATTACK; Log.Write(LogChannel.File, "progression: Vigilance - no back attack"); }
				if (first && type == GlobalScope.btl.BATTLE_OPENING_TYPE.NORMAL_ATTACK && GlobalScope.ds.RandomNumber.rand32(100u) < 25) { type = GlobalScope.btl.BATTLE_OPENING_TYPE.INITIATLVE_ATTACK; Log.Write(LogChannel.File, "progression: First Strike - the party opens"); }
			}
			catch (Exception) { }
			return type;
		}

		/// <summary>FF5's Two-Handed: the hero has the passive, one weapon in hand and nothing in the other (no shield, no second weapon).</summary>
		public static bool TwoHandedStrike(GlobalScope.btl.BattlePlayer attacker)
		{
			try
			{
				if (attacker == null || !GameProfile.Ff3Party) return false;
				GlobalScope.pl.Player p = attacker.player();
				if (p == null || !IsMastery(p.playerId()) || !Has(p, Ff3Abilities.TwoHanded)) return false;
				GlobalScope.pl.PlayerEquipParameter e = p.equipParameter();
				if (e.isEquipWeapon() != 1 || e.isBareHands()) return false;
				GlobalScope.pl.HAND_TYPE hand = e.checkEquipWeaponHand();
				GlobalScope.pl.HAND_TYPE other = hand == GlobalScope.pl.HAND_TYPE.RIGHT_HAND ? GlobalScope.pl.HAND_TYPE.LEFT_HAND : GlobalScope.pl.HAND_TYPE.RIGHT_HAND;
				bool free = e.equipHand(other).itemId() <= 0 || e.equipHand(other).equipNumber().get() <= 0;
				if (free) Log.Write(LogChannel.File, "progression: Two-Handed - " + p.name() + " strikes twice as hard");
				return free;
			}
			catch (Exception) { return false; }
		}

		private static void ApplyHpBoost(GlobalScope.pl.Player player)
		{
			HeroState s = StateOf(player.playerId());
			int percent = Active(player.playerId(), HeldJob(player)).Sum(Ff3Abilities.HpBoost);
			int limit = player.hp().getLimit();
			int baseLimit = Math.Max(1, limit - s.HpBoostApplied);
			int boost = baseLimit * percent / 100;
			if (boost == s.HpBoostApplied) return;
			int newLimit = Math.Min(9999, baseLimit + boost);
			int now = player.hp().getNow();
			player.hp().setLimit(newLimit);
			if (boost > s.HpBoostApplied && now > 0) player.hp().setNow(Math.Min(newLimit, now + (boost - s.HpBoostApplied)));
			else if (now > newLimit) player.hp().setNow(newLimit);
			s.HpBoostApplied = boost;
		}

		/// <summary>The number the job menu shows beside a job: the ladder's level for a mastery hero, the game's own job level otherwise.</summary>
		public static int JobMenuLevel(int playerId, int job, int gamesOwn)
		{
			try
			{
				if (!IsMastery(playerId)) return gamesOwn;
				int held = HeldJob(playerId, job);   // a job of the mod's own held on this base: its ladder's level
				return LadderOf(held) != null ? JobLevel(playerId, held) : gamesOwn;
			}
			catch (Exception) { return gamesOwn; }
		}

		/// <summary>The extra job bits a hero counts as for equipment and magic: the grants of the abilities in play.</summary>
		public static int GrantBits(GlobalScope.pl.Player player)
		{
			try
			{
				int playerId = player.playerId();
				if (!IsMastery(playerId)) return 0;
				int bits = 0;
				List<int> active = Active(playerId, HeldJob(player));
				// The same ability may sit on several ladders with different grants (white magic from the
				// White Mage's grants white-mage, from the Devout's devout): every step the hero has
				// actually climbed to counts, and an innate one every step that teaches it.
				List<int> innate = Innate(playerId, HeldJob(player));
				foreach (ModJob ladder in Ladders)
				{
					int level = JobLevel(playerId, ladder.JobNumber);
					for (int i = 0; i < ladder.Abilities.Count; i++)
					{
						ModJobAbility step = ladder.Abilities[i];
						if (!active.Contains(step.Id) || (i >= level && !innate.Contains(step.Id))) continue;
						foreach (string g in step.Grants)
						{
							int j = ModCharacters.JobNumber(g);
							if (j >= 0) bits |= 1 << j;
						}
					}
				}
				return bits;
			}
			catch (Exception) { return 0; }
		}

		/// <summary>A battle won: ABP for each living mastery hero's job - the formation's abp, or one per monster - and notices for the steps climbed.</summary>
		public static void OnBattleWon(GlobalScope.btl.BattleSystem B)
		{
			if (!GameProfile.Ff3Party) return;
			try
			{
				if (!AnyMastery) return;
				int abp = 0;
				try
				{
					int formation = GlobalScope.btl.OutsideToBattle.getInstance().initializeMonster().monsterPartyId();
					ModFormation f = ModItemsLayer.Formations.FirstOrDefault(x => x.Number == formation);
					if (f != null && f.Abp > 0) abp = f.Abp;
				}
				catch (Exception) { }
				if (abp <= 0)
				{
					try { abp = Math.Max(1, (int)B.characterManager().monsterParty().memberNumber()); } catch (Exception) { abp = 1; }
				}
				for (byte i = 0; i < 4; i++)
				{
					GlobalScope.pl.Player player = GlobalScope.pl.PlayerParty.instance().player(i);
					if (player == null || !player.isEnable() || player.condition().isNotBattleCondition()) continue;
					if (!IsMastery(player.playerId())) continue;
					int job = HeldJob(player);
					List<string> news = GiveAbp(player.playerId(), job, abp);
					Log.Write(LogChannel.File, "progression: " + player.name() + " +" + abp + " ABP in " + JobName(job) + " (" + Abp(player.playerId(), job) + "/" + AbpToNext(player.playerId(), job) + ", level " + JobLevel(player.playerId(), job) + ")");
					Notices.Post(player.name() + ": +" + abp + " ABP");
					foreach (string n in news) { Notices.Post(n); Log.Write(LogChannel.General, "progression: " + n); }
				}
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "progression: battle won: " + ex.Message); }
		}

		/// <summary>After experience is handed out: class heroes learn what their level brings.</summary>
		public static void OnExperienceGiven()
		{
			if (!GameProfile.Ff3Party) return;
			try
			{
				for (byte i = 0; i < 4; i++)
				{
					GlobalScope.pl.Player player = GlobalScope.pl.PlayerParty.instance().player(i);
					if (player != null && player.isEnable()) LearnByLevel(player, announce: true);
				}
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "progression: learning: " + ex.Message); }
		}

		/// <summary>A class hero's learn list against their level: every spell due is equipped (once).</summary>
		private static void LearnByLevel(GlobalScope.pl.Player player, bool announce)
		{
			ModCharacter def = ModCharactersLayer.Definition(player.playerId());
			if (def == null || def.Progression != Progression.Class || def.Learn.Count == 0) return;
			int level = player.level().get();
			foreach (ModLearned l in def.Learn)
			{
				if (l.Level > level) continue;
				int spell = SpellId(l.Spell);
				if (spell <= 0) { Note(def.Id + " learns '" + l.Spell + "' - no such spell"); continue; }
				if (HasSpell(player, spell)) continue;
				GlobalScope.itm.MagicParameter p = GlobalScope.itm.ItemManager.instance().magicParameter((short)spell);
				if (p == null) { Note(def.Id + " learns '" + l.Spell + "' - item " + spell + " is no spell"); continue; }
				if (!player.isEquipItem(p.equipJob())) { Note(def.Id + " learns '" + l.Spell + "' - " + JobName(HeldJob(player)) + " cannot hold it; skipped"); continue; }
				GlobalScope.pl.EquipmentMagic slots = player.equipParameter().equipMagic((GlobalScope.pl.MAGIC_LEVEL)p.magicClass());
				if (slots.equip(spell) == GlobalScope.pl.EquipmentMagic.NO_MAGIC_ID) continue;
				string name = _spellsByName?.FirstOrDefault(kv => kv.Value == spell).Key;
				string line = player.name() + " learned " + (string.IsNullOrEmpty(name) ? l.Spell : name);
				Log.Write(LogChannel.General, "progression: " + line);
				if (announce) Notices.Post(line);
			}
		}

		private static readonly HashSet<string> _noted = new HashSet<string>();

		/// <summary>A learn list's fault, logged once.</summary>
		private static void Note(string line)
		{
			if (_noted.Add(line)) Log.Write(LogChannel.General, "progression: " + line);
		}

		private static bool HasSpell(GlobalScope.pl.Player player, int spell)
		{
			for (int level = 0; level < 8; level++)
			{
				GlobalScope.pl.EquipmentMagic slots = player.equipParameter().equipMagic((GlobalScope.pl.MAGIC_LEVEL)level);
				for (int i = 0; i < GlobalScope.pl.MAGIC_ONCE_LEVEL_EQUIP_MAX; i++) if (slots.magicId(i) == spell) return true;
			}
			return false;
		}

		private static Dictionary<string, int> _spellsByName;

		/// <summary>A spell from a definition's word: an item id, or the spell's name (eureka_item.msd, read once - the message system only has it while a menu is up).</summary>
		private static int SpellId(string word)
		{
			if (string.IsNullOrWhiteSpace(word)) return -1;
			if (int.TryParse(word.Trim(), out int id)) return id;
			if (_spellsByName == null)
			{
				_spellsByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
				try
				{
					Dictionary<uint, string> names = TableFiles.ReadNames(GameArchive.Chain, "eureka_item.msd", new GameTables());
					GlobalScope.itm.ItemManager items = GlobalScope.itm.ItemManager.instance();
					for (int i = 0; i < items.magicCount(); i++)
					{
						GlobalScope.itm.MagicParameter p = items.magicAt(i);
						if (p == null || names == null || !names.TryGetValue((uint)p.nameId(), out string name) || string.IsNullOrWhiteSpace(name)) continue;
						if (!_spellsByName.ContainsKey(name.Trim())) _spellsByName[name.Trim()] = p.itemId();
					}
				}
				catch (Exception ex) { Log.Write(LogChannel.File, "progression: spell names: " + ex.Message); }
			}
			return _spellsByName.TryGetValue(word.Trim(), out int found) ? found : -1;
		}

		// ---- the save ----

		private static string SidecarPath => SaveFiles.PathFor("save.progression.json");

		/// <summary>The game is saving a slot: the ladders' state beside it.</summary>
		public static void OnSave(int slot)
		{
			if (!GameProfile.Ff3Party) return;
			try
			{
				JsonObject root = ReadSidecar();
				JsonObject slots = root["slots"] as JsonObject ?? new JsonObject();
				root["slots"] = slots;
				JsonArray heroes = new JsonArray();
				for (int i = 0; i < 4; i++)
				{
					HeroState s = _heroes[i];
					JsonObject abp = new JsonObject(), level = new JsonObject();
					foreach (KeyValuePair<int, int> kv in s.Abp.OrderBy(k => k.Key)) if (kv.Value > 0) abp[kv.Key.ToString()] = kv.Value;
					foreach (KeyValuePair<int, int> kv in s.Level.OrderBy(k => k.Key)) if (kv.Value > 0) level[kv.Key.ToString()] = kv.Value;
					JsonObject hero = new JsonObject { ["abp"] = abp, ["level"] = level, ["set"] = new JsonArray(s.Set.Select(v => (JsonNode)JsonValue.Create(v)).ToArray()) };
					if (s.Job >= 0) hero["job"] = s.Job;
					if (s.HpBoostApplied != 0) hero["hpBoost"] = s.HpBoostApplied;
					heroes.Add(hero);
				}
				slots[slot.ToString()] = new JsonObject { ["heroes"] = heroes, ["saved"] = DateTime.UtcNow.ToString("o") };
				File.WriteAllText(SidecarPath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
				Log.Write(LogChannel.File, "progression: state saved beside slot " + slot);
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "progression: save: " + ex.Message); }
		}

		/// <summary>The game is loading a slot: the ladders' state from beside it (the bottom of every ladder where there is none).</summary>
		public static void OnLoad(int slot)
		{
			if (!GameProfile.Ff3Party) return;
			try
			{
				Reset();
				JsonObject root = ReadSidecar();
				if (!((root["slots"] as JsonObject)?[slot.ToString()] is JsonObject saved) || !(saved["heroes"] is JsonArray heroes)) return;
				for (int i = 0; i < 4 && i < heroes.Count; i++)
				{
					if (!(heroes[i] is JsonObject h)) continue;
					HeroState s = _heroes[i];
					if (h["abp"] is JsonObject abp) foreach (KeyValuePair<string, JsonNode> kv in abp) if (int.TryParse(kv.Key, out int j) && j >= 0) s.Abp[j] = kv.Value?.GetValue<int>() ?? 0;
					if (h["level"] is JsonObject level) foreach (KeyValuePair<string, JsonNode> kv in level) if (int.TryParse(kv.Key, out int j) && j >= 0) s.Level[j] = kv.Value?.GetValue<int>() ?? 0;
					s.Job = h["job"]?.GetValue<int>() ?? -1;
					s.HpBoostApplied = h["hpBoost"]?.GetValue<int>() ?? 0;
					if (h["set"] is JsonArray set) for (int k = 0; k < FreeSlotMax && k < set.Count; k++) s.Set[k] = set[k]?.GetValue<int>() ?? 0;
				}
				Log.Write(LogChannel.File, "progression: state read from beside slot " + slot);
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "progression: load: " + ex.Message); }
		}

		/// <summary>The loaded save is in the party: mastery heroes' commands follow the ladders again.</summary>
		public static void OnLoaded()
		{
			if (!GameProfile.Ff3Party) return;
			try
			{
				for (byte i = 0; i < 4; i++)
				{
					GlobalScope.pl.Player player = GlobalScope.pl.PlayerParty.instance().player(i);
					if (player != null && player.isEnable()) Refresh(player);
				}
				_learnPending = true;   // a class hero's list may have grown since the save
			}
			catch (Exception) { }
		}

		private static JsonObject ReadSidecar()
		{
			try
			{
				if (File.Exists(SidecarPath) && JsonNode.Parse(File.ReadAllText(SidecarPath)) is JsonObject o) return o;
			}
			catch (Exception) { }
			return new JsonObject();
		}

		/// <summary>The name of an ability in play, for menus and the API.</summary>
		public static string AbilityName(int id) => ModJobs.AbilityName(Ladders, id);
	}
}
