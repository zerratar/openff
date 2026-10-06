// The OpenFF battle on FF4: a fight on the current map, built on nothing but OpenFF.Data
// and the engine API - the way a mod would build one.
//
// FF3's battle part runs FF3's rules over FF3's tables and cannot take FF4's party; FF4's
// own battle is not ported. This is the first battle that reads the unified party and
// monsters: the party stands where it is, the monsters appear in front of it (FF4's
// m<family>_00 models with their b_m<family> motions - 101 idle, 201 attack), the field
// camera frames them, and an ATB fight runs: every combatant's gauge fills with its agility;
// a full gauge gives a party member the command window (Fight, Magic, Item, Run) and a
// monster its attack. Physical damage follows btl::NewAttackFormula::calcDamageValueForBabil
// as far as it was read: attack x attacker level x attacker strength / (target defence +
// target level + target vitality), times 1.0..1.3, times 1.2 from a party member onto a
// monster and 0.7 the other way (the element, row and status factors are not applied yet);
// the hit roll is calcHitRate's: weapon hit + agility - (evade + agility) + 20, out of 100.
// Magic follows btl::NewMagicFormula as read from libff4.so: attack damage =
// power x caster level x caster stat (will for white, wisdom otherwise) / (target will +
// target level + target magic defence), times 1.0..1.3; healing = (target vitality / 8 +
// caster will / 2) x power, times 0.90..1.00; MP cost, power, school, hit rate and targets
// from magic_parameter.bbd (SpellDefinition); items heal what efficacy.beld says. Victory
// pays experience, gil and drops into the party (and levels teach spells); defeat leaves
// everyone at 1 HP for now. K starts a test fight; scripted battles and encounters hook in
// through Start.

using System;
using System.Collections.Generic;
using OpenFF;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle : GameService
	{
		private enum Phase { Idle, Intro, Fight, Victory, Defeat, Outro }
		private enum Command { Fight, Magic, Item, Run, Defend, SwapRows, Darkness, Jump, Other }
		private enum Pick { None, Command, Target, Item, Spell, Ally, Hand, EquipItem }
		// FF4's commands by ability id (common::ABILITY_ID; their names are babil_ability.msd's 3000 + id).
		private const int CmdFight = 1, CmdFlee = 2, CmdDefend = 3, CmdItems = 4, CmdBlackMagic = 5, CmdWhiteMagic = 6, CmdSummon = 13, CmdDarkness = 32, CmdSwapRows = 46;

		/// <summary>
		/// A member's battle commands as pl::Player::initializeCommand lays them out: Fight (and 0, 2) first, then the
		/// learned commands from 4 up but for a few kept for the end, then those (6, 5, 13, 18, 0x53, 4) while there is
		/// room, five at most - and Defend (3) and Swap Rows (46) always in slots 5 and 6. Cecil: Attack, Darkness,
		/// Items, Defend, Swap Rows, as Steam's menu shows them.
		/// </summary>
		internal static List<int> CommandList(Character c)
		{
			HashSet<int> learned = new HashSet<int>(c.Definition.CommandsAt(c.Level));
			List<int> list = new List<int>();
			foreach (int id in new[] { 1, 2 }) if (learned.Contains(id)) list.Add(id);
			for (int id = 4; id < 256 && list.Count < 5; id++)
			{
				if (id == 4 || id == 5 || id == 6 || id == 13 || id == 18 || id == 0x53 || id == CmdDefend || id == CmdSwapRows) continue;
				if (learned.Contains(id)) list.Add(id);
			}
			foreach (int id in new[] { 6, 5, 13, 18, 0x53, 4 }) if (list.Count < 5 && learned.Contains(id)) list.Add(id);
			list.Add(CmdDefend);
			list.Add(CmdSwapRows);
			return list;
		}

		private static string CommandName(int id) => Ff4Party.Tables?.AbilityName(3000 + id)?.Trim() ?? ("command " + id);

		private static Command CommandOf(int id) => id switch
		{
			CmdFight => Command.Fight,
			CmdFlee => Command.Run,
			CmdDefend => Command.Defend,
			CmdItems => Command.Item,
			CmdBlackMagic or CmdWhiteMagic or CmdSummon or CmdBardsong or CmdNinjutsu => Command.Magic,
			CmdDarkness => Command.Darkness,
			CmdSwapRows => Command.SwapRows,
			CmdJump => Command.Jump,
			_ => Command.Other,
		};

		private List<int> Commands => _acting != null ? _acting.Commands.ConvertAll(id => ShownCommand(_acting, id)) : new List<int> { CmdFight };
		private int _commandScroll;

		private sealed class Fighter
		{
			public string Name;
			public bool IsMonster;
			public Character Member;          // party side
			public MonsterDefinition Monster; // monster side
			public Npc Npc;
			public int Hp, MaxHp;
			public int Attack, Defence, Agility;
			public int Level, Intellect, Spirit, Vitality, MagicDefence;
			public int Strength, HitChance, Evade;
			public int Mp => Member?.Mp ?? 0;
			public float Gauge;               // 0..1 (FF4's ATP over its 100)
			public float AtbRate = 1f;        // a monster's, rolled at the start (BattleMonster::atbRate); 1 for the party
			public bool Queued;               // its action is decided and waits its turn (ATG state 2 or 3, the gauge held full)
			public int AiCondition = -2, AiIndex; // a monster's AI: the condition that chose its set last, and where it is in it
			public int HpBefore;              // its HP as the last action began (an AI check: did that action hurt it)
			public int DarkFrames;            // Darkness's time left (ys::Condition 0x17), and whether a dark blow's cost is due
			public bool DarkCostDue;
			public ulong Conditions;          // its ys::Condition bits, and their timers and counters (Ff4BattleStatus)
			public int[] ConditionTimer = new int[39];
			public int BlinkCount, DoomCount, PoisonCount, SapCount, PetrifyCount, HpSeen, MagicEvasion;
			public int IdleMotion = 2004, StatusEffect = -1, StatusEffectKind;   // the idle it stands in, the effect over it and which
			public int WeakOverride = -1, ResistOverride = -1;   // a weakness and resistances a boss's action set (Barrier Shift), -1 its record's
			public Fighter Remembered;        // "Target"'s lock (BBC +0x304): its next type-1 action falls on this one
			public Npc[] Legs;
			public Npc FormNpc;
			public int TintType;              // the status colour it wears (BATTLE_CHARACTER_COLOR's type, 0 none) and its own colours under it
			public uint[] OwnColours;               // the frog or pig standing in for it (Toad, Pig), and which
			public int Form;                // the Octomammoth's eight (null once gone)
			public bool Airborne;             // in the air from a Jump (flag 0x15): no one's target; its gauge full, the landing
			public Fighter JumpTarget;
			public int JumpCounter;           // the air time (BattlePlayer +0x44), to 0x78000
			public bool Mist;                 // the Mist Dragon in mist (flag 0x1e), and its mist's model
			public Npc MistNpc;
			public int[] Free = new int[5];   // the battle events' variables on it (BaseBattleCharacter's free variables)
			public int DecidedAbility, DecidedTarget = 1;   // its turnAction as decided: the ability (1 Attack, a spell's or item's id...) and a monster's target type
			public bool NotDeath;             // an event's NotDeathFlagOn: it cannot fall
			public int AtwLeft, AtwMax;       // a decided action's wait before it joins the turns (ATG state 2), 1/4096ths of a frame
			public Action Pending;            // the action waiting it out
			public bool Alive => Hp > 0;
			public Vector3 Home;
			public bool Acted;   // a one-shot motion is playing; Idle() restarts the loop when it ends
			public List<int> Commands = new List<int>();   // the member's FF4 command list (CommandList)
			public bool Defending;                         // Defend (flag 3): physical damage halved until their next decision
			public bool Braced;                            // Brace (flag 4): physical damage a quarter until their next decision
			public bool Covering;                          // Cover (flag 5) and whom (+0x48)
			public Fighter CoverTarget;
			public int FocusCharge;                        // Focus's charge (+0x114, 0..3), and whether a blow has used it
			public bool FocusSpent;
			public int BluffCharge;                        // Bluff (+0x30c): intellect doubled until the action after it
			public bool Hiding;                            // Hide (condition 0x19)
			public int SongId, SongTimer, SongTick;        // the song being sung (+0x4c), its time (+0x2e0) and Life's Anthem's count
			public int ElementOverride = -1;               // Upgrade's attack element (+0x40)
			public bool Robbed;                            // a monster stolen from (flag 0x11)
			public bool Examined;                          // analyzed (flag 0xd): its card shows its HP and elements
			public bool TwinWaiting;                       // Twincast chosen, its partner awaited (PAIR_MAGIC_WAIT)
			public Fighter TwinPartner;
			public HashSet<string> BoundSets = new HashSet<string>();
			public bool Blessing;                          // Bless (flag 0x12): the party's MP every 45 frames
			public int BlessCount;
			public int Poise = -1, SwingA = -1, SwingB = -1, Swings;
			public int HitEffect = -1;                     // the weapon's hit effect (WEAPON_EFFECT), its pack e<nnn>
			public int HitBank = -1, HitSound = -1;        // the weapon's hit sound (battle_parameter chain 5's first pair)
			public float Facing;                           // degrees about y on the stage
		}

		private static Ff4Battle _instance;
		public static bool Active => _instance != null && (_instance._phase != Phase.Idle || Ff4BattleStage.Pending);

		private Phase _phase = Phase.Idle;
		private readonly List<Fighter> _party = new List<Fighter>();
		private readonly List<Fighter> _foes = new List<Fighter>();
		private readonly Random _random = new Random();
		private int _timer;
		private Fighter _acting;            // the member whose gauge filled, waiting on a command
		private Pick _pick = Pick.None;
		private int _cursor;
		private Command _command;
		private readonly List<int> _itemChoices = new List<int>();
		private readonly List<int> _spellChoices = new List<int>();
		private int _listScroll;
		private SpellDefinition _casting;   // the spell picked, waiting on a target
		private int _usingItem;             // the item picked, waiting on an ally
		private readonly List<string> _log = new List<string>();
		private int _expWon, _gilWon;
		private readonly List<int> _dropsWon = new List<int>();
		private readonly List<string> _resultLines = new List<string>();   // level-ups and drops for the result window
		private int _cameraType;            // the encounter group's battle camera (Ff4BattleStage.CameraPosition)
		private Vector3 _centre;
		private int _heroMotionIdle = 2004, _heroMotionAttack = 2008, _heroMotionHurt = 1117;   // 2004: the stance Steam's Cecil holds through a fight, 1117 its flinch (2007 / 2009 are its victory)

		public Ff4Battle()
		{
			_instance = this;
		}

		public override bool WantsUpdate => true;

		public override IEnumerable<string> DebugLines()
		{
			if (_phase != Phase.Idle) yield return "OpenFF battle: " + _phase + ", " + _foes.FindAll(f => f.Alive).Count + " foe(s) up";
		}

		// ---- starting ----

		public static Ff4Battle Instance => _instance;

		/// <summary>Runs once when the fight ends, however it ends (a scene's return jump, for one).</summary>
		public Action AfterBattle;

		/// <summary>An encounter group from the tables (FF4's monster_party_table.bbd), with its placements.</summary>
		public bool StartParty(int partyId, bool inScene = false, int battleMap = -1)
		{
			MonsterParty party = Ff4Party.Tables?.MonsterParty(partyId);
			if (party == null || party.Slots.Count == 0)
			{
				Log.Write(LogChannel.General, "battle: no encounter group " + partyId);
				return false;
			}
			List<int> ids = new List<int>();
			List<Vector3> places = new List<Vector3>();
			List<float> facings = new List<float>();
			foreach (MonsterPartySlot slot in party.Slots)
			{
				for (int k = 0; k < Math.Max(1, slot.Count); k++)
				{
					ids.Add(slot.MonsterId);
					places.Add(new Vector3(slot.X, slot.Y, slot.Z));
					facings.Add(slot.W);
				}
			}
			_placements = places;
			_facings = facings;
			_rootId = party.PartyRootId;
			_cameraType = party.CameraType;
			_startingParty = party;   // its events start with the fight (on a stage, once the stage has come)
			bool started = Start(ids, inScene, battleMap);
			if (started) Log.Write(LogChannel.General, "battle: encounter group " + partyId);
			else _startingParty = null;
			return started;
		}

		/// <summary>
		/// A monster model's motion set: b_m&lt;family&gt; for most; a custom monster's models each have their own,
		/// b_m&lt;family&gt;_&lt;model&gt; (BattleMistDragon::registerMonster: b_m069_00 for the dragon, _01 for its mist). Null when there is none.
		/// </summary>
		private static string MonsterMotionSet(int family, int model)
		{
			string plain = "b_m" + family.ToString("000"), own = plain + "_" + model.ToString("00");
			bool Exists(string name) => GameArchive.Chain.Exists("files/" + name + ".ncap.lz") || GameArchive.Chain.Exists(name + ".ncap.lz");
			if (Exists(own)) return own;
			return model == 0 ? plain : null;
		}

		/// <summary>A monster stood at a spot, facing as the group has it (or the hero), its model bound to its motions, in the fight.</summary>
		private Fighter SpawnFoe(MonsterDefinition m, Vector3 at, float? facing, Vector3 hero, bool join = true)
		{
			Monster info = Game.Monsters.Find(m.Id);
			bool octomammoth = m.Id == Octomammoth;
			string model = octomammoth ? "m" + m.Family.ToString("000") + "a" : info?.Model ?? ("m" + m.Family.ToString("000") + "_00");
			Npc npc = Game.Npcs.SpawnModel(model, at, 0f);
			if (npc == null) { Say("no model for " + m.Name); return null; }
			if (m.Scale > 0.01f && Math.Abs(m.Scale - 1f) > 0.001f) { try { npc.Scale = m.Scale; } catch (Exception) { } }
			try
			{
				string set = octomammoth ? "b_m" + m.Family.ToString("000") + "a" : MonsterMotionSet(m.Family, 0);
				npc.BindMotions(octomammoth || set.EndsWith("_00", StringComparison.Ordinal) ? set : info?.MotionSet ?? set);
				npc.PlayMotion(101, true);
			}
			catch (Exception) { }
			if (facing.HasValue && npc is LegacyNpc exactNpc) exactNpc.FaceExactly(facing.Value);
			else if (facing.HasValue) npc.LookAt(at + Ff4BattleStage.Facing(facing.Value) * 10f);
			else npc.LookAt(hero);
			npc.Solid = false;
			Fighter foe = new Fighter
			{
				Name = m.Name ?? ("monster " + m.Id), IsMonster = true, Monster = m, Npc = npc, Home = at, MagicEvasion = m.MagicEvasion,
				Hp = Math.Max(1, m.MaxHp), MaxHp = Math.Max(1, m.MaxHp),
				Attack = Math.Max(1, m.Attack), Defence = Math.Max(0, m.Defence), Agility = Math.Max(1, m.Stats.Agility),
				Level = Math.Max(1, m.Level), Intellect = m.Stats.Intellect, Spirit = m.Stats.Spirit, Vitality = m.Stats.Vitality, MagicDefence = Math.Max(0, m.MagicDefence),
				Strength = m.Stats.Strength, HitChance = m.Hit > 0 ? m.Hit : 90, Evade = Math.Max(0, m.Evade),
				Gauge = StartGauge(),
				AtbRate = m.AtbRateMin + (float)_random.NextDouble() * Math.Max(0f, m.AtbRateMax - m.AtbRateMin),
				Facing = facing ?? 0f,
			};
			if (octomammoth) SpawnLegs(foe);
			if (join) _foes.Add(foe);
			LoadEffect(m.AttackEffect);
			return foe;
		}

		private MonsterParty _startingParty;
		private List<Vector3> _placements;
		private List<float> _facings;       // the monsters' facings, degrees about y (the group's fourth word)
		private int _rootId;                // the party root the members stand on (the group's byte 2)
		private List<int> _pendingIds;

		/// <summary>A fight against these monsters (ids in the unified tables), on the spot.</summary>
		public bool Start(IEnumerable<int> monsterIds, bool inScene = false, int battleMap = -1)
		{
			if (_phase != Phase.Idle || !EngineApi.InWorld || (Ff4Cutscene.Active && !inScene)) return false;
			// FF4 fights on a battle stage (Ff4BattleStage): a jump there first, the fight once the
			// party has arrived; a scene's fight stays where the scene is.
			if (!inScene && !Ff4BattleStage.Active && battleMap >= 0 && Ff4BattleStage.Begin(battleMap))
			{
				_pendingIds = new List<int>(monsterIds);
				return true;
			}
			bool onStage = Ff4BattleStage.Active;
			GameTables tables = Ff4Party.Tables;
			Party party = Ff4Party.Party;
			if (tables == null || party.Members.Count == 0 || !Game.Hero.Present) return false;

			BeginBattleEvents(_startingParty);
			_startingParty = null;
			_runOn = _runReady = _cantEscapeShown = _escaping = _fledFade = false; _runFrames = 0;
			_closing = false; _queue.Clear(); _counterAbilities.Clear(); _dying.Clear(); _turnEffects.Clear(); _executing = null; _party.Clear(); _foes.Clear(); _log.Clear(); _dropsWon.Clear();
			_expWon = _gilWon = 0;
			foreach (Character c in party.Members)
			{
				OpenFF.Data.Stats stats = c.StatsWith(tables);
				int weapon = Weapon(c, tables);
				_party.Add(new Fighter
				{
					Name = c.Name, Member = c, Hp = c.Hp, MaxHp = c.MaxHp, Commands = CommandList(c),
					Attack = Math.Max(1, weapon > 0 ? weapon : stats.Strength / 2), Defence = Armour(c, tables), Agility = Math.Max(1, stats.Agility),
					Level = c.Level, Intellect = stats.Intellect, Spirit = stats.Spirit, Vitality = stats.Vitality, MagicDefence = MagicArmour(c, tables),
					Strength = stats.Strength, HitChance = weapon > 0 ? WeaponHit(c, tables) : 90, Evade = Evasion(c, tables),
					Gauge = StartGauge(), Conditions = c.Conditions, HpSeen = c.Hp, MagicEvasion = MagicEvasionOf(c, tables),
				});
			}
			if (onStage)
			{
				// FF4 fights with its battle models: pNN_00 per player type (the field walks pNN_01,
				// whose joints do not match the battle motion sets b_p_player_NN - binding one onto
				// the other crashes the joint animation). So every member, the leader included,
				// stands as a spawned battle model, and the field's hero waits unseen at the
				// leader's spot until the jump back.
				Game.Hero.Teleport(Ff4BattleStage.PartySpot(_rootId, Ff4Party.PositionOf(_party[0].Member.Id)));
				try { EngineApi.HeroPlayer?.setHidden(true); } catch (Exception) { }
				// A boss's entrance (BossNormalAttack): the party comes in as the camera leaves the boss's close-up.
				(int bossMonster, BossCamera bossCamera) = BossOf(monsterIds);
				int entryDelay = bossCamera != null ? BossMoveFrom : 0;
				for (int i = 0; i < _party.Count; i++)
				{
					Fighter ally = _party[i];
					CharacterDefinition who = ally.Member.Definition;
					// FF4's party position and, under the formation, its row (Ff4Party): the spot and the facing the root gives it.
					int position = Ff4Party.PositionOf(ally.Member.Id), row = Ff4Party.RowOf(position);
					Vector3 spot = Ff4BattleStage.PartySpot(_rootId, position, row);
					Npc npc = null;
					try { npc = Game.Npcs.SpawnModel("p" + who.Id.ToString("00") + "_00", spot, 0f); } catch (Exception) { }
					if (npc == null) { Log.Write(LogChannel.File, "battle: no battle model for " + ally.Name); continue; }
					try
					{
						WeaponMotionRecord weapon = BindBattleMotions(npc, who.Id, ally.Member, tables);
						if (weapon != null) { ally.Poise = weapon.Poise; ally.SwingA = weapon.Raw[3]; ally.SwingB = weapon.Raw[2]; }
						int system = WeaponSystem(ally.Member, tables);
						ally.HitEffect = HitEffectOf(system);
						if (system >= 0 && system < tables.WeaponSounds.Count && tables.WeaponSounds[system][0] >= 0 && tables.WeaponSounds[system][1] >= 0)
						{
							ally.HitBank = tables.WeaponSounds[system][0];
							ally.HitSound = tables.WeaponSounds[system][1];
						}
						LoadEffect(ally.HitEffect);
					}
					catch (Exception) { }
					HoldEquipment(npc, ally.Member, tables);
					npc.Solid = false;
					ally.Facing = Ff4BattleStage.PartyFacingDegrees(_rootId, position, row);
					// The entrance Steam's frames show: from 25 behind the spot, turned about, running in (1115) over six
					// frames, then facing the foes in the stance (2004).
					// Placed frame by frame as Steam's trace has it (25 back, then 5 equal steps in; turned 180 all the way -
					// a walk would let the turn system swing the model round).
					Fighter entering = ally;
					Vector3 from = spot - Ff4BattleStage.Facing(ally.Facing) * 25f, to = spot;
					npc.Teleport(from);
					Face(ally, ally.Facing + 180f);
					try { npc.PlayMotion(1115, true, 0); } catch (Exception) { }
					if (entryDelay > 0) { npc.Hidden = true; After(entryDelay, () => { if (entering.Npc != null) entering.Npc.Hidden = false; }); }
					for (int k = 1; k <= 5; k++)
					{
						float part = k / 5f;
						After(entryDelay + k, () => { entering.Npc?.Teleport(from + (to - from) * part); Face(entering, entering.Facing + 180f); });
					}
					After(entryDelay + 6, () => { Face(entering, entering.Facing); Play(entering, _heroMotionIdle, true, 0); entering.Acted = false; });
					ally.Npc = npc;
					ally.Home = spot;
				}
			}
			Vector3 hero = Game.Hero.Position;
			// Ahead as the camera sees it: the monsters stand between the leader and the far side of
			// the view, whichever way the leader was facing, so the follow camera frames them.
			Vector3 forward = (hero - Game.Camera.Position).Flat.Normalized;
			if (onStage) forward = new Vector3(-1f, 0f, 0f);
			if (forward.Length < 0.5f) forward = Vector3.FromYaw(Game.Hero.Yaw).Flat.Normalized;
			if (forward.Length < 0.5f) forward = new Vector3(0, 0, -1);
			Vector3 side = new Vector3(-forward.Z, 0, forward.X);
			int n = 0;
			List<int> ids = new List<int>(monsterIds);
			foreach (int id in ids)
			{
				MonsterDefinition m = tables.Monster(id);
				if (m == null) { Say("no monster " + id); continue; }
				// The game's placement (x across, z depth, in its battle units - roughly halved for the field) or a row.
				Vector3 at;
				float? facing = null;
				if (onStage)
				{
					at = Ff4BattleStage.MonsterSpot(_placements != null && n < _placements.Count ? _placements[n] : Vector3.Zero, n, ids.Count);
					if (_facings != null && n < _facings.Count) facing = _facings[n];
				}
				else if (_placements != null && n < _placements.Count)
				{
					Vector3 p = _placements[n];
					at = Game.Field.OnGround(hero + forward * (22f + Math.Abs(p.Z) * 0.2f) + side * (p.X * 0.45f));
				}
				else
				{
					float spread = (n - (ids.Count - 1) / 2f) * 12f;
					at = Game.Field.OnGround(hero + forward * 26f + side * spread);
				}
				if (SpawnFoe(m, at, facing, hero) == null) continue;
				n++;
			}
			LoadEffect(MissEffect);
			_placements = null;
			_facings = null;
			_pendingIds = null;
			if (_foes.Count == 0) { if (Ff4BattleStage.Active) Ff4BattleStage.Leave(); return false; }

			Game.Input.Capture = true;
			Game.Hero.Freeze();
			Game.Hero.Face(forward.Yaw);
			if (onStage)
			{
				// The battle part fades in once it stands (btl::BattleNormalAttack::initialize: fadeIn 5), from the white
				// the encounter left or the black of a field's jump.
				GlobalScope.dgs.CFade.Main().fadeIn(5);
				GlobalScope.dgs.CFade.Sub().fadeIn(5);
				// FF4's own battle camera (btl::CBattleDisplay, Ff4BattleStage): the opening pose, a
				// short slide back to the standing shot the encounter group's camera type names, its
				// 18-degree field of view and 10..2000 clip; driven through the event camera.
				Vector3 op = Ff4BattleStage.OpeningPosition, ot = Ff4BattleStage.OpeningTarget;
				Vector3 cp = Ff4BattleStage.CameraPosition(_cameraType), ct = Ff4BattleStage.CameraTarget(_cameraType);
				Ff4EventCamera.MoveTo((int)(op.X * 4096), (int)(op.Y * 4096), (int)(op.Z * 4096), 0, false);
				Ff4EventCamera.LookAt((int)(ot.X * 4096), (int)(ot.Y * 4096), (int)(ot.Z * 4096), 0);
				Ff4EventCamera.SetFov(Ff4BattleStage.CameraFov);
				Ff4EventCamera.SetClip(Ff4BattleStage.ClipNear, Ff4BattleStage.ClipFar);
				(int bossId, BossCamera boss) = BossOf(ids);
				if (boss != null) BeginBossEntrance(bossId, boss);
				else
				{
					Ff4EventCamera.MoveTo((int)(cp.X * 4096), (int)(cp.Y * 4096), (int)(cp.Z * 4096), Ff4BattleStage.OpeningFrames, false);
					Ff4EventCamera.LookAt((int)(ct.X * 4096), (int)(ct.Y * 4096), (int)(ct.Z * 4096), Ff4BattleStage.OpeningFrames);
				}
			}
			if (!onStage)
			{
				try { Game.Hero.BindMotions("b_p_player_" + party.Leader.Id.ToString("00")); Game.Hero.PlayMotion(_heroMotionIdle, true); } catch (Exception) { }
			}
			// The field camera stays: behind and above the leader it frames the monsters ahead on any
			// map, where a side view walks into cave walls. FF4's own side camera can come with its stage.
			_centre = hero + forward * 13f;
			_phase = Phase.Intro;
			_timer = 0;
			_acting = null;
			_pick = Pick.None;
			Log.Write(LogChannel.General, "battle: " + _foes.Count + " foe(s): " + string.Join(", ", _foes.ConvertAll(f => f.Name + " (" + f.MaxHp + " hp)")) + " against " + string.Join(", ", _party.ConvertAll(f => f.Name + " L" + f.Member.Level)) + " (step " + LegacyStep.Count + ")");
			return true;
		}

		/// <summary>
		/// What a member holds, as pl::PlayerEquipmentSymbol shows it: each hand's weapon or shield as w<ModelId:000> at the
		/// joint boneName names - a weapon at R_wepon / L_wepon (a bow's at the forearm, R_ude / L_ude), a shield at the
		/// forearm (Steam's Cecil: w000, the Dark Sword, in the right hand; w094, the Dark Shield, on the left arm).
		/// </summary>
		private static void HoldEquipment(Npc npc, Character c, GameTables tables)
		{
			if (!(npc is LegacyNpc legacy) || legacy.CharacterId < 0) return;
			foreach ((int slot, bool left) in new[] { ((int)EquipSlot.RightHand, false), ((int)EquipSlot.LeftHand, true) })
			{
				ItemDefinition item = c.Equipment[slot] != 0 ? tables.Item(c.Equipment[slot]) : null;
				if (item == null || item.ModelId < 0) continue;
				bool weapon = item.Kind == ItemKind.Weapon;
				bool forearm = !weapon || WeaponSystem(c, tables) == 10;
				string joint = (left ? "L_" : "R_") + (forearm ? "ude" : "wepon");
				string model = "w" + item.ModelId.ToString("000");
				int bound = Ff4Cutscene.BindModel(legacy.CharacterId, model, joint);
				Log.Write(LogChannel.File, "battle: " + c.Name + " holds " + model + " (" + item.Name + ") at " + joint + (bound < 0 ? " - did not load" : ""));
			}
		}

		// btl::BattleParameter::WEAPON_EFFECT, its first column: the effect a weapon system's ordinary blow plays on the
		// target (the Dark Sword's 17: 161), loaded as EFFECT.dat's e<nnn> (btl::BattleEffect::load).
		private static readonly int[] WeaponHitEffect = { 160, 175, 161, 160, 160, 182, 182, 189, 189, 160, 210, 198, 203, 168, 160, 160, 196, 161, 223, 215, 222, 161, 161 };

		private static int HitEffectOf(int weaponSystem) => weaponSystem >= 0 && weaponSystem < WeaponHitEffect.Length ? WeaponHitEffect[weaponSystem] : 160;

		private static void LoadEffect(int id)
		{
			if (id < 0) return;
			string pack = "e" + id.ToString("000");
			try
			{
				if (!GlobalScope.eff.CEffectMng.instance().loadEfpNamed(pack, "/EFFECT/" + pack + ".efp")) Log.Write(LogChannel.File, "battle: effect pack " + pack + " did not load");
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "battle: effect pack " + pack + ": " + ex.Message); }
		}

		/// <summary>A battle effect at a spot (btl::BattleEffect::create: the effect manager's, box culling off).</summary>
		private void PlayEffect(int id, Vector3 at, int variant = 1, bool holdsTurn = true)
		{
			if (id < 0) return;
			try
			{
				GlobalScope.eff.CEffectMng effects = GlobalScope.eff.CEffectMng.instance();
				int made = effects.create(id, variant);
				if (made < 0) { Log.First(LogChannel.File, "battle-effect-" + id, 2, () => "battle: effect " + id + " could not be made"); return; }
				effects.enableBoxCulling(made, false);
				effects.setPosition(made, new GlobalScope.VecFx32((int)Math.Round(at.X * 4096), (int)Math.Round(at.Y * 4096), (int)Math.Round(at.Z * 4096)));
				if (holdsTurn) { _turnEffects.Add(made); _turnEffectStart[made] = _clock; }
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "battle: effect " + id + ": " + ex.Message); }
		}

		// pl::PLAYER_FORM: the b_pc_form_<nn> set of each player type.
		private static readonly int[] PlayerForm = { 0, 0, 0, 1, 2, 1, 3, 2, 2, 0, 0, 0, 0, 3, 0 };

		/// <summary>
		/// The weapon system a member fights with (itm::EquipParameter::weaponSystem): the weapon's system byte through
		/// FF4's table - 2..22 are 0..20, 29 and 30 are 21 and 22, the rest (and no weapon) 24. The Dark Sword (19) is 17.
		/// </summary>
		internal static int WeaponSystem(Character c, GameTables tables)
		{
			foreach (int slot in new[] { (int)EquipSlot.RightHand, (int)EquipSlot.LeftHand })
			{
				ItemDefinition item = c.Equipment[slot] != 0 ? tables.Item(c.Equipment[slot]) : null;
				if (item == null || item.Kind != ItemKind.Weapon) continue;
				int sys = item.System;
				if (sys >= 2 && sys <= 22) return sys - 2;
				if (sys == 29) return 21;
				if (sys == 30) return 22;
				return 24;
			}
			return 24;
		}

		/// <summary>
		/// A member's battle motions as btl::BattlePlayer::addBasicMotion and addPoiseMotion bind them: b_p_common,
		/// b_pc_form_<PLAYER_FORM>, b_<the player's basic set>, b_poise<the weapon's poise>, b_p<the player's set> (Cecil's
		/// b_p1009 holds 2004, the stance Steam's Cecil stands in) and the weapon's b_w<nn> (its attacks).
		/// </summary>
		private static WeaponMotionRecord BindBattleMotions(Npc npc, int type, Character c, GameTables tables)
		{
			BattlePlayerMotions player = type >= 0 && type < tables.BattlePlayers.Count ? tables.BattlePlayers[type] : null;
			int system = WeaponSystem(c, tables);
			WeaponMotionRecord weapon = tables.WeaponMotion(type, system == 24 ? 0 : system);
			List<string> sets = new List<string> { "b_p_common", "b_pc_form_" + PlayerForm[Math.Clamp(type, 0, PlayerForm.Length - 1)].ToString("00") };
			if (player != null && player.BasicSet > 0) sets.Add("b_" + player.BasicSet.ToString("0000"));
			if (weapon != null && weapon.Poise > 0) sets.Add("b_poise" + weapon.Poise);
			if (player != null && player.PlayerSet > 0) sets.Add("b_p" + player.PlayerSet.ToString("0000"));
			if (weapon != null && weapon.WeaponSet >= 0) sets.Add("b_w" + weapon.WeaponSet.ToString("00"));
			sets.Add("b_p_player_" + type.ToString("00"));   // the victory's 2007 and 2009 (BattleWin)
			foreach (string set in sets) npc.BindMotions(set);
			Log.Write(LogChannel.File, "battle: " + c.Name + " (weapon system " + system + ") binds " + string.Join(", ", sets));
			return weapon;
		}

		internal static int Weapon(Character c, GameTables tables)
		{
			int best = 0;
			foreach (int id in c.Equipment)
			{
				ItemDefinition item = id != 0 ? tables.Item(id) : null;
				if (item?.Equip != null && item.Kind == ItemKind.Weapon) best = Math.Max(best, item.Equip.Attack);
			}
			return best;
		}

		internal static int Armour(Character c, GameTables tables)
		{
			int total = 0;
			foreach (int id in c.Equipment)
			{
				ItemDefinition item = id != 0 ? tables.Item(id) : null;
				if (item?.Equip != null && item.Kind == ItemKind.Armour) total += item.Equip.Defence;
			}
			return total;
		}

		internal static int WeaponHit(Character c, GameTables tables)
		{
			int best = 0;
			foreach (int id in c.Equipment)
			{
				ItemDefinition item = id != 0 ? tables.Item(id) : null;
				if (item?.Equip != null && item.Kind == ItemKind.Weapon) best = Math.Max(best, item.Equip.Hit);
			}
			return best > 0 ? best : 90;
		}

		internal static int Evasion(Character c, GameTables tables)
		{
			int total = 0;
			foreach (int id in c.Equipment)
			{
				ItemDefinition item = id != 0 ? tables.Item(id) : null;
				if (item?.Equip != null && item.Kind == ItemKind.Armour) total += item.Equip.Evade;
			}
			return total;
		}

		internal static int MagicEvasionOf(Character c, GameTables tables)
		{
			int total = 0;
			foreach (int id in c.Equipment)
			{
				ItemDefinition item = id != 0 ? tables.Item(id) : null;
				if (item?.Equip != null && item.Kind == ItemKind.Armour) total += item.Equip.MagicEvade;
			}
			return total;
		}

		internal static int MagicArmour(Character c, GameTables tables)
		{
			int total = 0;
			foreach (int id in c.Equipment)
			{
				ItemDefinition item = id != 0 ? tables.Item(id) : null;
				if (item?.Equip != null && item.Kind == ItemKind.Armour) total += item.Equip.MagicDefence;
			}
			return total;
		}

		// ---- the frame ----

		public override void OnUpdate()
		{
			if (_phase == Phase.Idle)
			{
				if (Ff4BattleStage.Pending)
				{
					// On the way to the battle stage; the fight starts when the party stands on it.
					if (Ff4BattleStage.Arrived)
					{
						Ff4BattleStage.Arrive();
						if (_pendingIds == null || !Start(_pendingIds, false, -1)) { _pendingIds = null; Ff4BattleStage.Leave(); }
					}
					return;
				}
				if (EngineApi.InWorld && !Ff4Cutscene.Active && !Game.Dialogue.IsOpen && !Game.Input.Capture)
				{
					if (Game.Input.KeyPressed("K"))
					{
						// A test fight: the tables' first encounter group (two Goblins) - or the first two monsters.
						Ff4Encounters.Table here = Ff4Encounters.For(Game.Field.Map);
						int stage = here != null && here.BattleMap >= 0 ? here.BattleMap : 1;
						if (!StartParty(1, false, stage)) Start(new[] { 0, 1 }, false, stage);
					}
					else
					{
						Encounters();
					}
				}
				return;
			}
			_timer++;
			_clock++;
			RunCues();
			StepVictoryCamera();
			switch (_phase)
			{
				case Phase.Intro:
					StepBossCamera();
					if (_timer > IntroFrames) { _phase = Phase.Fight; _timer = 0; _bossCamera = null; }
					break;
				case Phase.Fight:
					Fight();
					break;
				case Phase.Victory:
					// The result window (DrawResult) stands until A, B or a tap.
					if (_timer > (_page > 0 ? _pageFrom + WinArrowAfter : WinFade + WinHold + WinArrowAfter) && (Game.Input.Pressed(Pad.A) || Game.Input.Pressed(Pad.B) || Game.Input.PointerReleased))
					{
						if (_page + 1 < _pages.Count)
						{
							// The next page: a member's level-up (with getExpPhase's sound, 0x65 0), then the items.
							_page++;
							_pageFrom = _timer;
							if (_pages[_page].Kind == "levelup") Game.Audio.PlaySe(0x65, 0);
							break;
						}
						_phase = Phase.Outro;
						_timer = 0;
						if (_closing)
						{
							// The key closes the window and the battle fades to black over 16 frames (Steam's frames: the
							// window gone at once, black 16 frames on), the battle's music stopping over 15 (getPhaseEnd).
							GlobalScope.dgs.CFade.Main().fadeOut(WinEndFade, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
							GlobalScope.dgs.CFade.Sub().fadeOut(WinEndFade, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
						}
					}
					break;
				case Phase.Defeat:
					// BattleLose::execute: after 30 frames, the music stopped, a key ends it - at once when the event said
					// so (BattleParameter flag 0xB, a fight meant to be lost).
					if (_timer > 30 && (BattleParameterFlag(0xB) || Game.Input.Pressed(Pad.A) || Game.Input.Pressed(Pad.B) || Game.Input.PointerReleased))
					{
						// BattlePart::setNextPart after a loss: the party made whole (PlayerParty::fineAll - full HP and MP,
						// no statuses); a fight a scene may lose goes back to it, any other is the game's end - the screen
						// fades out over 15 frames and the title comes (part 3).
						foreach (Fighter f in _party)
						{
							f.Hp = f.MaxHp;
							f.Conditions = 0;
							if (f.Member != null) { f.Member.Hp = f.MaxHp; f.Member.Mp = f.Member.MaxMp; f.Member.Conditions = 0; }
						}
						_help = null;
						_gameOver = AfterBattle == null && !BattleParameterFlag(0xB);
						if (_gameOver)
						{
							GlobalScope.dgs.CFade.Main().fadeOut(15, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
							GlobalScope.dgs.CFade.Sub().fadeOut(15, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
						}
						_phase = Phase.Outro;
						_timer = 0;
					}
					break;
				case Phase.Outro:
					if (_closing && _timer <= WinEndFade) break;
					if (_fledFade && _timer <= 15) break;
					if (_gameOver && _timer <= 15) break;
					End();
					return;
			}
			Draw();
		}

		/// <summary>A fighter whose one-shot motion has finished goes back to its idle loop.</summary>
		private void Idle()
		{
			foreach (Fighter f in _party)
			{
				if (!f.Alive || !f.Acted) continue;
				bool done = f.Npc != null ? f.Npc.MotionDone : Game.Hero.MotionDone;
				if (done) { f.IdleMotion = AnyFlag(f, 5) || f.Hp <= f.MaxHp / 4 ? 2001 : _heroMotionIdle; Play(f, f.IdleMotion, true, 4); f.Acted = false; }
			}
			foreach (Fighter f in _foes)
			{
				if (!f.Alive || !f.Acted || f.Npc == null) continue;
				if (f.Npc.MotionDone) { try { f.Npc.PlayMotion(101, true, 4); } catch (Exception) { } f.Acted = false; }
			}
		}

		private void Fight()
		{
			Idle();
			ShowConditions();
			if (_closing) return;
			if (Ff4BattleStage.Active) Log.Sample(LogChannel.File, "battle-camera", 120, () => "battle: camera at " + Game.Camera.Position + " hero at " + Game.Hero.Position);
			// btl::BattleActiveTimeMain::execute, a frame of it. The behaviour manager first: a turn is over once its steps
			// are done and no effect it set off still plays (turnExecute: BattleBehavior's end and !BattleEffect::isPlay),
			// and the dead go then (dieState); with nothing under way the next decided action starts (idleState,
			// startBehavior). Then the gauges - only while nothing is under way or a monster's action is
			// (isValidAdvanceATB: a member's own action stops the clock, a monster's does not), and in the wait mode not
			// while a member has a list open. A monster whose gauge fills decides there and then (requestBehavior); a
			// member whose gauge fills gets the commands, which stay open whatever is happening, so a member who takes
			// their time is hit meanwhile.
			// The events (BattleScriptEngine, battle_ai.bbd): one running is pumped a frame; the normal one restarts every
			// idle frame (a poll), the one before an action runs as it is about to start (and may call it off), the one
			// after a normal action as its turn ends.
			if (_eventKind != EventKind.None)
			{
				EventKind was = _eventKind;
				if (TickEvent())
				{
					if (was == EventKind.After) FinishTurn();
					else if (was == EventKind.Before && _queue.Count > 0 && !_eventMode) StartNextAction();
				}
			}
			if (_executing != null && _cues.Count == 0 && !TurnEffectsPlaying()) TurnEnd();
			if (_closing || _phase != Phase.Fight) return;
			bool busy = _executing != null || _cues.Count > 0 || _eventKind != EventKind.None;
			if (!busy && _eventParty != null && _eventParty.NormalEvent >= 0 && !StartEvent(EventKind.Normal, _eventParty.NormalEvent)) busy = true;
			if (_closing) return;
			if (!busy && _queue.Count > 0 && !_eventMode)
			{
				if (_eventParty != null && _eventParty.BeforeEvent >= 0)
				{
					_attacker = _queue[0].Actor;
					_lastAbility = _counterAbilities.TryGetValue(_queue[0].Act, out int countering) ? countering : _queue[0].Actor.DecidedAbility;
				}
				if (_eventParty != null && _eventParty.BeforeEvent >= 0 && !StartEvent(EventKind.Before, _eventParty.BeforeEvent)) busy = true;
				else { StartNextAction(); busy = true; }
			}
			bool listOpen = _acting != null && _pick != Pick.Command && _pick != Pick.None;
			bool advance = (!busy || (_executing != null && _executing.IsMonster)) && !(WaitMode && listOpen) && !_eventMode;
			CheckEscape(advance);
			if (advance)
			{
				foreach (Fighter f in _party)
				{
					if (!f.Alive) continue;
					if (f.DarkFrames > 0) f.DarkFrames = Math.Max(0, f.DarkFrames - SpeedRate);   // calcConditionTime
					TickConditions(f);
					if (!f.Alive || GaugeStands(f)) continue;
					if (f.SongId != 0) { TickSong(f); continue; }   // singing: no gauge, no turn
					if (f.Airborne && !f.Queued)
					{
						// In the air the gauge stands (ATG state 4); the jump counter runs, and full, the landing goes to the back of the queue.
						if (JumpCounts(f)) { Fighter jumper = f; Decide(jumper, () => JumpLand(jumper, jumper.JumpTarget), 0, CmdJump); }
						continue;
					}
					if (!f.Queued) { f.Gauge = Math.Min(1f, f.Gauge + GaugeStep(f)); continue; }
					if (f.Pending == null) continue;
					f.AtwLeft -= SpeedRate;
					if (f.AtwLeft <= 0) { _queue.Add((f, f.Pending)); f.Pending = null; }
				}
				TickBless();
				foreach (Fighter f in _foes)
				{
					TickConditions(f);
					if (!f.Alive || f.Queued || GaugeStands(f) || !CanAct(f)) continue;
					f.Gauge = Math.Min(1f, f.Gauge + GaugeStep(f));
					if (f.Gauge >= 1f)
					{
						Fighter monster = f;
						monster.Queued = true;
						(monster.DecidedAbility, monster.DecidedTarget) = DecideMonsterAction(monster);
						_queue.Add((monster, () => MonsterActs(monster)));
					}
				}
			}
			if (_acting != null && (!_acting.Alive || _acting.Queued || !CanAct(_acting))) { _acting = null; _pick = Pick.None; }
			UpdateReady();
			if (_acting != null && _ready.Count > 1 && Game.Input.KeyPressed("Tab"))
			{
				// BattleCommandSelectorManager::skip: the window to the next member ready, this one to the back of the line.
				_ready.Remove(_acting);
				_ready.Add(_acting);
				_acting = null;
				_pick = Pick.None;
				_abilityCmd = 0;
				_casting = null;
				_castItem = 0;
			}
			if (_acting == null)
			{
				foreach (Fighter f in new List<Fighter>(_ready))
				{
					if (f.Airborne || f == _executing) continue;   // its own action under way (the invoke stage before its gauge empties): no window
					if (f.Alive && !f.Queued && f.Gauge >= 1f && CanAct(f) && ActsAlone(f)) continue;
					if (f.Alive && !f.Queued && f.Gauge >= 1f && CanAct(f) && AutoTakes(f)) { AutoDecide(f); continue; }
					if (f.Alive && !f.Queued && f != _executing && f.Gauge >= 1f && CanAct(f)) { _acting = f; _pick = Pick.Command; _cursor = 0; _commandScroll = 0; Log.Write(LogChannel.File, "battle: " + f.Name + " may act (step " + LegacyStep.Count + ", agility " + f.Agility + ")"); return; }
				}
				return;
			}
			InputState input = Game.Input;
			if (_pick == Pick.Command)
			{
				List<int> commands = Commands;
				int count = commands.Count;
				if (input.Pressed(Pad.Up)) _cursor = (_cursor + count - 1) % count;
				if (input.Pressed(Pad.Down)) _cursor = (_cursor + 1) % count;
				if (_cursor < _commandScroll) _commandScroll = _cursor;
				if (_cursor >= _commandScroll + CommandRows) _commandScroll = _cursor - CommandRows + 1;
				if (input.Pressed(Pad.A))
				{
					_command = CommandOf(commands[_cursor]);
					Fighter who = _acting;
					if (_command == Command.Defend) { who.Braced = false; Defend(who); Instant(who); return; }   // decideAbility: flag 3, the gauge empty, no action
					if (_command == Command.SwapRows) { Decide(who, () => Invoke(who, 46, () => SwapRows(who)), 0, 46); return; }
					if (_command == Command.Darkness) { Decide(who, () => Darkness(who), 0, 32); return; }
					if (_command == Command.Other && AbilityChosen(who, commands[_cursor])) return;
					if (_command == Command.Other) { Say(CommandName(commands[_cursor]) + " is not in yet."); return; }
					if (_command == Command.Fight || _command == Command.Jump) { _pick = Pick.Target; _cursor = FirstAliveFoe(); }
					else if (_command == Command.Magic)
					{
						_spellChoices.Clear();
						GameTables tables = Ff4Party.Tables;
						// The command's own school: White Magic its white spells, Black Magic its black, Summon its summons.
						int chosen = commands[_cursor];
						OpenFF.Data.MagicSchool school = chosen == CmdWhiteMagic ? OpenFF.Data.MagicSchool.White : chosen == CmdBlackMagic ? OpenFF.Data.MagicSchool.Black
							: chosen == CmdBardsong ? OpenFF.Data.MagicSchool.Song : chosen == CmdNinjutsu ? OpenFF.Data.MagicSchool.Ninjutsu : OpenFF.Data.MagicSchool.Summon;
						_listSchool = school;
						// Every one the member knows of the school; those not for a battle (Sight) greyed, as Steam lists them.
						foreach (int id in _acting.Member.Spells)
						{
							SpellDefinition spell = tables.Spell(id);
							if (spell != null && spell.School == school) _spellChoices.Add(id);
						}
						foreach (int id in _acting.Member.Abilities)
						{
							SpellDefinition spell = id >= 1500 ? tables.Spell(id) : null;
							if (spell != null && spell.School == school && !_spellChoices.Contains(id)) _spellChoices.Add(id);
						}
						if (_spellChoices.Count == 0) { Say(_acting.Name + " knows no magic."); return; }
						_pick = Pick.Spell; _cursor = 0; _listScroll = 0;
					}
					else if (_command == Command.Item)
					{
						_itemChoices.Clear();
						// Steam: every item the bag holds that is not worn - those a fight has no use for greyed.
						foreach (OpenFF.Data.ItemStack s in Ff4Party.Party.Inventory)
						{
							ItemDefinition item = Ff4Party.Tables.Item(s.ItemId);
							if (item != null && item.Kind == ItemKind.Consumable) _itemChoices.Add(s.ItemId);
						}
						AddReequipRow();   // Steam: Re-equip heads the list, the hand on it
						_pick = Pick.Item; _cursor = 0; _listScroll = 0;
					}
					else Decide(who, () => Run(who), 0, 2);
				}
				return;
			}
			if (_pick == Pick.Target)
			{
				if (input.Pressed(Pad.Left) || input.Pressed(Pad.Up)) _cursor = NextAliveFoe(_cursor, -1);
				if (input.Pressed(Pad.Right) || input.Pressed(Pad.Down)) _cursor = NextAliveFoe(_cursor, 1);
				if (input.Pressed(Pad.B)) { _pick = _casting != null ? Pick.Spell : _castItem > 0 ? Pick.Item : Pick.Command; _cursor = 0; _casting = null; _castItem = 0; return; }
				if (input.Pressed(Pad.A) && _cursor >= 0)
				{
					Fighter who = _acting, foe = _foes[_cursor];
					SpellDefinition spell = _casting;
					if (_castItem > 0 && IsFang(_castItem))
					{
						int fang = _castItem;
						_castItem = 0;
						Decide(who, () => UseFang(who, fang, _foes.FindAll(f => f.Alive && !OutOfFight(f))), ItemWait(fang), fang);
					}
					else if (_castItem > 0 && CastOf(_castItem) is SpellDefinition casts)
					{
						int item = _castItem;
						_castItem = 0;
						bool all = (Ff4Party.Tables.AbilityTargets(item) & 0x4) != 0;   // ability.bbd: the item's own targets (a Bomb Fragment: all foes)
						Decide(who, () => UseCastItem(who, item, casts, all ? _foes.FindAll(f => f.Alive && !OutOfFight(f)) : new List<Fighter> { foe.Alive ? foe : FirstAlive(_foes) }), ItemWait(item), item);
					}
					else if (spell != null) Decide(who, () => Cast(who, spell, spell.HitsAll ? _foes.FindAll(f => f.Alive) : new List<Fighter> { foe.Alive ? foe : FirstAlive(_foes) }), SpellWait(spell), spell.Id);
					else if (_command == Command.Jump) Decide(who, () => Invoke(who, CmdJump, () => JumpStart(who, foe)), 0, CmdJump);
					else if (_abilityCmd != 0) AbilityOnFoe(who, foe);
					else Decide(who, () => MemberAttacks(who, foe), 0, 1);
				}
				return;
			}
			if (_pick == Pick.Spell)
			{
				GridMove(input, _spellChoices.Count);
				if (input.Pressed(Pad.B)) { _pick = Pick.Command; _cursor = Math.Max(0, Commands.FindIndex(id => CommandOf(id) == Command.Magic)); return; }
				if (input.Pressed(Pad.A))
				{
					SpellDefinition spell = Ff4Party.Tables.Spell(_spellChoices[_cursor]);
					if (spell == null || !spell.UsableInBattle) return;
					if (_acting.Mp < spell.MpCost) { Say("Not enough MP for " + spell.Name + "."); return; }
					_casting = spell;
					if (Helps(spell)) { _pick = Pick.Ally; _cursor = _party.IndexOf(_acting); }
					else { _pick = Pick.Target; _cursor = FirstAliveFoe(); }
				}
				return;
			}
			if (_pick == Pick.Ally)
			{
				bool self = SelfOnly(_abilityCmd);
				if (!self && (input.Pressed(Pad.Up) || input.Pressed(Pad.Left))) _cursor = (_cursor + _party.Count - 1) % _party.Count;
				if (!self && (input.Pressed(Pad.Down) || input.Pressed(Pad.Right))) _cursor = (_cursor + 1) % _party.Count;
				if (input.Pressed(Pad.B) && _abilityCmd != 0) { int back = _abilityCmd; _abilityCmd = 0; _pick = Pick.Command; _cursor = Math.Max(0, Commands.FindIndex(id => id == back)); return; }
				if (input.Pressed(Pad.B)) { _pick = _casting != null ? Pick.Spell : Pick.Item; _casting = null; _cursor = 0; return; }
				if (input.Pressed(Pad.A) && self) { AbilityOnSelf(_acting); return; }
				if (input.Pressed(Pad.A) && _abilityCmd == CmdCover) { AbilityOnAlly(_acting, _party[_cursor]); return; }
				if (input.Pressed(Pad.A))
				{
					Fighter who = _acting, ally = _party[_cursor];
					SpellDefinition spell = _casting;
					int item = _usingItem;
					if (spell != null) Decide(who, () => Cast(who, spell, spell.HitsAll ? new List<Fighter>(_party) : new List<Fighter> { ally }), SpellWait(spell), spell.Id);
					else if (_castItem > 0 && CastOf(_castItem) is SpellDefinition casts) { _castItem = 0; Decide(who, () => UseCastItem(who, item, casts, casts.HitsAll ? new List<Fighter>(_party) : new List<Fighter> { ally }), ItemWait(item), item); }
					else Decide(who, () => UseItem(who, item, ally), ItemWait(item), item);
				}
				return;
			}
			if (_pick == Pick.Hand) { UpdateHand(input); return; }
			if (_pick == Pick.EquipItem) { UpdateEquipItem(input); return; }
			if (_pick == Pick.Item)
			{
				GridMove(input, _itemChoices.Count);
				if (_cursor == 1 && _itemChoices.Count > 1 && _itemChoices[1] == ReequipEntry) _cursor = 0;   // the head row is one
				if (input.Pressed(Pad.A) && _cursor < _itemChoices.Count && _itemChoices[_cursor] == ReequipEntry)
				{
					_pick = Pick.Hand; _hand = 0; _cursor = 0; _listScroll = 0;
					FillEquipChoices();
					return;
				}
				if (input.Pressed(Pad.B)) { int back = _abilityCmd != 0 ? _abilityCmd : CmdItems; _pick = Pick.Command; _cursor = Math.Max(0, Commands.FindIndex(id => id == back)); _abilityCmd = 0; return; }
				if (input.Pressed(Pad.A) && _abilityCmd != 0) { AbilityItem(_acting, _itemChoices[_cursor]); return; }
				if (input.Pressed(Pad.A) && _cursor < _itemChoices.Count && !ItemUsable(_itemChoices[_cursor])) return;
				if (input.Pressed(Pad.A) && (IsFang(_itemChoices[_cursor]) || CastOf(_itemChoices[_cursor]) is SpellDefinition casts && !Helps(casts)))
				{
					// An item that casts at the foes (Red Fang, a Bomb Fragment): the foes picked as for a spell.
					_usingItem = _castItem = _itemChoices[_cursor];
					_pick = Pick.Target; _cursor = FirstAliveFoe();
					return;
				}
				if (input.Pressed(Pad.A)) { _usingItem = _itemChoices[_cursor]; _castItem = CastOf(_usingItem) != null ? _usingItem : 0; _pick = Pick.Ally; _cursor = _party.IndexOf(_acting); }
			}
		}

		// FF4 lists spells and items in a grid of three columns (btl::BtlMagicMenu::BMTEXT_POS: x 24,
		// 98, 172 by rows of 10 DS pixels): Left and Right step along it, Up and Down move a row,
		// and the view scrolls by rows.
		// Steam's battle lists: three rows; three columns for the magic and ninjutsu, two for items and songs.
		private int ListColumns => _pick == Pick.Item || _pick == Pick.Hand || _pick == Pick.EquipItem || _pick == Pick.Spell && _listSchool == OpenFF.Data.MagicSchool.Song ? 2 : 3;
		private const int ListRows = 3;
		private OpenFF.Data.MagicSchool _listSchool;

		private void GridMove(InputState input, int count)
		{
			if (count <= 0) return;
			if (input.Pressed(Pad.Left)) _cursor = (_cursor + count - 1) % count;
			if (input.Pressed(Pad.Right)) _cursor = (_cursor + 1) % count;
			if (input.Pressed(Pad.Up)) _cursor = _cursor - ListColumns >= 0 ? _cursor - ListColumns : Math.Min(count - 1, _cursor + ((count - 1) / ListColumns) * ListColumns);
			if (input.Pressed(Pad.Down)) _cursor = _cursor + ListColumns < count ? _cursor + ListColumns : _cursor % ListColumns;
			_cursor = Math.Clamp(_cursor, 0, count - 1);
			int row = _cursor / ListColumns, top = _listScroll / ListColumns;
			if (row < top) _listScroll = row * ListColumns;
			if (row >= top + ListRows) _listScroll = (row - ListRows + 1) * ListColumns;
		}

		/// <summary>A spell cast on one's own side: healing, reviving, or a white spell that grants something.</summary>
		private static bool Helps(SpellDefinition spell)
		{
			if (spell.School == OpenFF.Data.MagicSchool.Song || spell.School == OpenFF.Data.MagicSchool.Ninjutsu)
				return spell.Raw != null && spell.Raw.Length > 0x20 && ((spell.Raw[0x20] & 0x08) != 0 || spell.Raw[0x20] == 0x50 || spell.Raw[0x20] == 0x51);   // magic_parameter +0x20: 0x19 the party, 0x50 / 0x51 self and allies
			return spell.Heals || spell.Revives || (spell.Power == 0 && spell.Inflicts == 0 && (spell.Grants != 0 || spell.Grants2 != 0) && spell.School == OpenFF.Data.MagicSchool.White);
		}

		// ---- magic, as btl::NewMagicFormula computes it ----

		/// <summary>What an action was, for the events' GetAbility, IsMonsterTargeted and SufferPhysical.</summary>
		private void Acted(Fighter actor, int ability, params Fighter[] targets)
		{
			_attacker = actor;
			_lastAbility = ability;
			_lastSpell = null;
			_lastTargets.Clear();
			_lastTargets.AddRange(targets);
			foreach (Fighter f in _party) f.HpBefore = f.Hp;
			foreach (Fighter f in _foes) f.HpBefore = f.Hp;
		}

		/// <summary>A line for the fight's log on screen, and for the log file.</summary>
		private void Say(string line)
		{
			_log.Add(line);
			Log.Write(LogChannel.File, "battle: " + line);
			if (_help == null || _helpUntil >= 0) { _help = line; _helpUntil = _clock + 60; }
		}

		/// <summary>A line for the log file only: FF4 shows no window for a plain blow, a miss, a defeat or a guard - the numbers and the motions say it (Steam's frames).</summary>
		private static void Note(string line) => Log.Write(LogChannel.File, "battle: " + line);

		private Vector3 Where(Fighter f) => f.Npc != null ? f.Npc.Position : Game.Hero.Position;

		/// <summary>
		/// Where an effect on a fighter plays (btl::BaseBattleCharacter::hitEffectPosition): its position, raised, and moved
		/// toward the camera in x and y along the line to it - a monster by its chain 4 record (the Floating Eye 15 up, 20
		/// toward the camera), a member 5 up and 9 toward it.
		/// </summary>
		private Vector3 HitEffectSpot(Fighter f)
		{
			Vector3 at = Where(f);
			float up = 5f, toward = 9f;
			if (f.IsMonster && f.Monster != null) { up = f.Monster.EffectHeight; toward = f.Monster.EffectToCamera; }
			Vector3 look = Game.Camera.Position - at;
			float length = (float)Math.Sqrt(look.X * look.X + look.Y * look.Y + look.Z * look.Z);
			if (length > 0.001f) look = new Vector3(look.X / length, look.Y / length, look.Z / length);
			return new Vector3(at.X + look.X * toward, at.Y + up + look.Y * toward, at.Z);
		}

		/// <summary>Where a fighter's numbers rise from (btl::BattleBehavior::createDamage): a monster's position and its model's offset (monster.chaindata chain 4 - the Floating Eye's 15 up), a member's 6 up.</summary>
		private Vector3 DamageSpot(Fighter f)
		{
			MonsterDefinition m = f.IsMonster ? f.Monster : null;
			return Where(f) + (m != null ? new Vector3(m.DamageX, m.DamageY, m.DamageZ) : new Vector3(0, 6f, 0));
		}

		// ---- the numbers and words that pop over a fighter, in FF4's battle digits ----

		private sealed class PopUp
		{
			public Vector3 At;
			public int Value;
			public int Word = -1;
			public bool Heal;
			public long Start = LegacyStep.Count;
			public const int Frames = 70;
		}

		// u2d::PopUpDamageNumber: one sprite a digit (up to five, 99999 at most), 10 DS pixels apart - the sheet's 20 -
		// each playing battle_number.NANR's first sequence when its turn comes: the most significant first, the next
		// two frames on (update starts digit 5 - (digits - counter / 2) on the even counts). The sequence moves the digit
		// up and back, a frame each (DS pixels, the sheet's half): -12, -24, -28, -18, -12, -14, -12; a digit stays at
		// its last place until every digit's sequence has ended, and the number goes with the last. Its colour by type:
		// 0x7070f8 for damage, 0xa0f868 for a heal (BGR). Steam's frames (steam-b2, a 37 on a Floating Eye): the 3 at
		// rest at 3712, the 7 at the top of its jump at 3716, both at rest at 3720, gone at 3724.
		private static readonly int[] DigitRise = { -12, -24, -28, -18, -12, -14, -12 };
		private const int DigitDelay = 2;
		private static readonly Color DamageTint = new Color(248, 112, 112, 255), HealTint = new Color(104, 248, 160, 255);

		private readonly List<PopUp> _pops = new List<PopUp>();

		/// <summary>A number over a spot in the world, FF4's damage digits: salmon for damage, green for a heal.</summary>
		private void Pop(Vector3 at, int value, bool heal = false)
		{
			_pops.Add(new PopUp { At = at, Value = Math.Min(99999, Math.Abs(value)), Heal = heal });
		}

		private void PopWord(Vector3 at, int word)
		{
			_pops.Add(new PopUp { At = at, Word = word });
		}

		private void DrawPops(DrawList d)
		{
			for (int i = _pops.Count - 1; i >= 0; i--)
			{
				PopUp p = _pops[i];
				int frame = (int)(LegacyStep.Count - p.Start);
				string digits = p.Word < 0 ? p.Value.ToString() : null;
				int frames = digits != null ? (digits.Length - 1) * DigitDelay + DigitRise.Length : PopUp.Frames;
				if (frame >= frames) { _pops.RemoveAt(i); continue; }
				Vector2? screen = Game.Camera.WorldToScreen(p.At);
				if (!screen.HasValue) continue;
				float x = screen.Value.X, y = screen.Value.Y;
				// Each number its own thing on the screen: two numbers share their digits' pictures, and a new one is
				// drawn ahead of the rest, so between two steps each rises from its own last place, not another's.
				d.Group(p);
				if (digits != null)
				{
					float step = 20f * Ff4Ui.Scale;
					// btl::Damage::create: the first digit 4 DS pixels left of the spot for each digit after it.
					float cx = x - 8f * Ff4Ui.Scale * (digits.Length - 1);
					for (int k = 0; k < digits.Length; k++, cx += step)
					{
						int f = frame - k * DigitDelay;
						if (f < 0) break;
						float rise = DigitRise[Math.Min(f, DigitRise.Length - 1)] * 2f * Ff4Ui.Scale;
						if (!Ff4Ui.Number(d, cx, y + rise, digits[k] - '0', p.Heal ? HealTint : DamageTint))
							d.Text(digits[k].ToString(), cx - 6, y + rise - 11, p.Heal ? HealTint : DamageTint, 22);
					}
				}
				else
				{
					// The words (Miss and the like) are not u2d's digits: up fast, a small bounce, then a hold while they fade.
					float t = frame;
					float rise = t < 12 ? -3.2f * t : t < 20 ? -38f + 2.2f * (t - 12) : t < 26 ? -20f - 1.2f * (t - 20) : -27f;
					byte alpha = (byte)(t > 55 ? Math.Max(0, 255 - (t - 55) * 17) : 255);
					Color tint = new Color(255, 255, 255, alpha);
					if (!Ff4Ui.Word(d, x, y + rise, p.Word, tint))
						d.Text(p.Word == Ff4Ui.WordMiss ? "Miss" : "!", x - 20, y + rise - 11, tint, 22);
				}
			}
			d.Group(null);
		}

		/// <summary>Plays a battle motion on a member: the leader is the hero, the others their spawned models.</summary>
		// ---- timed steps of the stage's choreography (Steam's frames: the entrance, an attack's poise and swing, the
		// victory), counted in battle frames ----

		private readonly List<(int At, Action Do)> _cues = new List<(int, Action)>();
		private int _clock;

		private void After(int frames, Action action) => _cues.Add((_clock + frames, action));

		private void RunCues()
		{
			for (int i = 0; i < _cues.Count;)
			{
				if (_cues[i].At > _clock) { i++; continue; }
				Action action = _cues[i].Do;
				_cues.RemoveAt(i);
				try { action(); } catch (Exception ex) { Log.Write(LogChannel.General, "battle: step: " + ex.Message); }
			}
		}

		// ---- FF4's active time gauge (btl::BaseBattleCharacter::atpAddValue, BattleMonster::addActiveTimeGage) ----

		/// <summary>btl::BATTLE_SPEED_RATE by the battle speed setting (1..6, 3 the game's default): the time a tick counts, in
		/// 1/4096ths of a frame - every timer below counts in those (a frame is 4096 of them).</summary>
		private static readonly int[] SpeedRates = { 6144, 5120, 4096, 3072, 2048, 1024 };
		private const int Tick = 4096;   // a frame, in the timers' 1/4096ths
		private static int SpeedRate => SpeedRates[Math.Clamp((int.TryParse(Options.Get("ff4-battle-speed"), out int s) ? s : 3) - 1, 0, 5)];
		private static float BattleSpeedRate => SpeedRate / (float)Tick;

		/// <summary>A normal encounter's start (BattlePlayer / BattleMonster::initializeATG): 45 to 65 of the gauge's 100, at random.</summary>
		private float StartGauge() => (45 + _random.Next(21)) / 100f;

		/// <summary>A frame's fill: (1 + agility / 32) at the battle's speed, times a monster's ATB rate - of the gauge's 100.</summary>
		private static float GaugeStep(Fighter f) => BattleSpeedRate * (1f + Math.Max(0, f.Agility) / 32f) * f.AtbRate / 100f * PaceOf(f);

		private void Face(Fighter f, float degrees)
		{
			if (f.Npc is LegacyNpc exact) exact.FaceExactly(degrees);
			else f.Npc?.LookAt(f.Npc.Position + Ff4BattleStage.Facing(degrees) * 10f);
		}

		private static readonly bool LogMotions = Options.Get("ff4-battle-motions") != null;

		private void Play(Fighter f, int motion, bool loop = false, int blend = 3)
		{
			// --ff4-battle-motions: each motion a fighter is given, by step and place - beside the Steam hook's chars.tsv.
			if (LogMotions) Log.Write(LogChannel.General, "motion: step " + LegacyStep.Count + " " + f.Name + " " + motion + (loop ? " loop" : "") + " at " + (f.Npc != null ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.0} {1:0.0} {2:0.0}", f.Npc.Position.X, f.Npc.Position.Y, f.Npc.Position.Z) : "?"));
			if (!loop) f.Acted = true;
			try
			{
				if (f.Npc != null) f.Npc.PlayMotion(motion, loop, blend);
				else if (_party.Count > 0 && f == _party[0]) Game.Hero.PlayMotion(motion, loop, blend);   // the leader is the hero's model; the others have none yet
			}
			catch (Exception) { }
		}

		/// <summary>NewMagicFormula::calcAttackMagicDamage: power x level x stat over the target's will, level and magic defence, times 1.0..1.3.</summary>
		private int AttackMagicDamage(Fighter caster, Fighter target, SpellDefinition spell, int targetCount)
		{
			int stat = spell.School == OpenFF.Data.MagicSchool.White ? caster.Spirit : caster.BluffCharge > 0 ? Math.Min(99, caster.Intellect * 2) : caster.Intellect;
			long numerator = (long)spell.Power * Math.Max(1, caster.Level) * Math.Max(1, stat);
			int magicDefence = Has(target, CShell) ? Math.Min(9999, target.MagicDefence * 3 / 2) : target.MagicDefence;
			if (Has(target, CCry)) magicDefence /= 2;   // magicDefense: Cry halves it
			int denominator = Math.Max(1, target.Spirit + target.Level + magicDefence);
			double value = numerator / (double)denominator * (1.0 + _random.Next(301) / 1000.0);
			if (targetCount > 1) value *= Math.Max(0.3, (90 - 10 * targetCount) / 100.0);
			return Math.Max(1, (int)value);
		}

		/// <summary>NewMagicFormula::healingMagicValue: (target vitality / 8 + caster will / 2) x power, times 0.90..1.00, less when spread.</summary>
		private int HealingValue(Fighter caster, Fighter target, SpellDefinition spell, int targetCount)
		{
			int value = (target.Vitality / 8 + caster.Spirit / 2) * spell.Power;
			value = value * (100 - _random.Next(10)) / 100;
			if (targetCount > 1) value = value * Math.Max(30, 90 - 10 * targetCount) / 100;
			return Math.Max(1, value);
		}

		// btl::BattleBehavior's dead process (the manager's dieState, once the turn of the blow is over): startDeadPerformance
		// plays the death sound (0x65, 6) and BPTranslucence takes every fallen monster from whole to nothing in
		// transFrameMax frames - 10 for a plain monster (readyDeadPerformance) - its transparency rate (10 - frame) * 10 a
		// frame, then it is gone (Steam's frames: a 38 on a Floating Eye at 3896, the Eye half there at 3904, gone at 3912).
		private const int DeathFrames = 10;

		/// <summary>A fighter falls; <paramref name="number"/> is the blow's damage, whose number the monster's fade waits on.</summary>
		private void Fell(Fighter foe, int number = -1)
		{
			if (!foe.IsMonster) { Note(foe.Name + " falls."); Play(foe, 2003, false, 3); return; }   // setConditionDeath: the KO motion, held on its last frame
			if (foe.NotDeath) { foe.Hp = 1; return; }   // an event's NotDeathFlagOn
			Note(foe.Name + " is defeated.");
			if (foe.Npc != null && Ff4BattleStage.Active) _dying.Add(foe);   // it goes once the turn is over (TurnEnd)
			else if (foe.Npc != null) { foe.Npc.Alpha = 8; foe.Npc.Hidden = true; }
			_expWon += foe.Monster.Experience;
			_gilWon += foe.Monster.Gil;
		}

		/// <summary>The dead process for one fallen monster: the sound, then the fade a frame at a time.</summary>
		private void StartDeathFade(Fighter foe)
		{
			{
				Npc npc = foe.Npc;
				if (npc == null) return;
				for (int f = 1; f <= DeathFrames; f++)
				{
					int rate = (DeathFrames - f) * 100 / DeathFrames;
					After(f, () => { npc.Alpha = rate; if (rate == 0) npc.Hidden = true; });
				}
			}
		}

		/// <summary>
		/// The fight's one item, as btl::BattleMonsterParty::prepareGift picks it: a kind of monster drawn by how many
		/// of it there were, one roll of 4096, its drop slots taken from the last (the rarest) to the first, each as
		/// wide as its chance - past them all, nothing. (A rare-item accessory doubles the chances; none is worn yet.)
		/// </summary>
		private void RollGift()
		{
			List<MonsterDefinition> kinds = new List<MonsterDefinition>();
			List<int> counts = new List<int>();
			foreach (Fighter f in _foes)
			{
				if (!f.IsMonster || f.Monster == null) continue;
				int at = kinds.IndexOf(f.Monster);
				if (at < 0) { kinds.Add(f.Monster); counts.Add(1); } else counts[at]++;
			}
			int total = 0;
			foreach (int c in counts) total += c;
			if (total == 0) return;
			int pick = _random.Next(total), k = 0;
			while (pick >= counts[k]) pick -= counts[k++];
			List<DropChance> drops = kinds[k].Drops;
			int roll = _random.Next(4096);
			for (int i = drops.Count - 1; i >= 0; i--)
			{
				if (roll < drops[i].Chance) { _dropsWon.Add(drops[i].ItemId); return; }
				roll -= drops[i].Chance;
			}
		}

		private int FirstAliveFoe()
		{
			for (int i = 0; i < _foes.Count; i++) if (_foes[i].Alive) return i;
			return -1;
		}

		private int NextAliveFoe(int from, int step)
		{
			for (int k = 1; k <= _foes.Count; k++)
			{
				int i = ((from + step * k) % _foes.Count + _foes.Count) % _foes.Count;
				if (_foes[i].Alive) return i;
			}
			return from;
		}

		/// <summary>NewAttackFormula::calcHitRate: attack hit + agility - (evade + agility) + 20, clamped to 0..100.</summary>
		private bool Hits(Fighter attacker, Fighter target)
		{
			if (target.Mist) return false;   // reviseMist: a blow on the mist misses
			if (target.Airborne) return false;   // in the air: out of reach
			if (Has(target, CBlink) && target.BlinkCount > 0)
			{
				// reviseBlink: the image takes the blow; the last one gone, Blink goes.
				if (--target.BlinkCount <= 0) ConditionOff(target, CBlink);
				return false;
			}
			int rate = attacker.HitChance + attacker.Agility - (target.Evade + target.Agility) + 20;
			if (Has(attacker, CBlind)) rate /= 10;   // calcHitRate: Blind a tenth
			rate = Math.Clamp(rate, 0, 100);
			return _random.Next(100) < rate;
		}

		/// <summary>
		/// NewAttackFormula::calcDamage: calcDamageValueForBabil's core - level x strength x attack over defence + level +
		/// vitality, times (1 + rand(301) / 1000), times 1.2 onto a monster and 0.7 onto a member - then calcCritical: a
		/// critical one time in (agility - the target's agility + 5) of 100, at most 25, for 120 % (setFlag 0xf). At
		/// least 1. Not yet: the elements' and races' multipliers, the back rows' (backPenalty) and the low-HP double.
		/// </summary>
		private bool _critical;   // the last blow's calcCritical (flag 0xf)

		private int Damage(Fighter attacker, Fighter target, bool fromAir = false, int kickTargets = 0)
		{
			// Toad and Mini: strength, vitality, attack and defence all 1; Protect: defence x1.5.
			bool small = Has(attacker, CToad) || Has(attacker, CMini), smallTarget = Has(target, CToad) || Has(target, CMini);
			long numerator = (long)Math.Max(1, attacker.Level) * (small ? 1 : Math.Max(1, attacker.Strength)) * (small ? 1 : Math.Max(1, attacker.Attack));
			int defence = smallTarget ? 1 : Has(target, CProtect) ? Math.Min(9999, target.Defence * 3 / 2) : target.Defence;
			if (Has(target, CCry)) defence /= 2;              // physicsDefense: Cry halves it
			if (target.FocusCharge > 0) defence = 1;         // and Focus's charge leaves it wide open
			int denominator = Math.Max(1, defence + target.Level + (smallTarget ? 1 : target.Vitality));
			long core = numerator / denominator;
			long value = (long)((4096 + (_random.Next(301) << 12) / 1000) * core) >> 12;
			float elements = ElementFactor(attacker, target), races = RaceFactor(attacker, target);
			bool dark = Dark(attacker) && attacker != target;
			if (elements != 1f || races != 1f) Note(attacker.Name + " on " + target.Name + ": elements x" + elements + ", races x" + races);
			value = (long)(value * (fromAir ? 1f : BackRowFactor(attacker, target)) * elements * races);   // backPenalty: none from the air
			value = value * (target.IsMonster ? 12 : 7) / 10;
			int chance = Math.Clamp(attacker.Agility - target.Agility + 5, 0, 25);
			if (_random.Next(100) < chance)
			{
				value = value * 120 / 100;
				_critical = true;
				Note(attacker.Name + "'s blow is critical.");
			}
			if (target.Braced) value = Math.Max(1, value / 4);   // reviseEndure: Brace a quarter
			if (Has(attacker, CBerserk)) value = value * 3 / 2;   // reviseBerserk: x1.5
			if (dark) { value *= 2; attacker.DarkCostDue = true; }   // reviseDarkness: x2 (0x2000), the cost after
			if (attacker.FocusCharge > 0) { value = value * RollUpRate[attacker.FocusCharge] >> 12; attacker.FocusSpent = true; }   // reviseGather
			if (kickTargets > 0) value = Math.Max(1, value * KickRate[Math.Min(kickTargets, KickRate.Length - 1)] >> 12);   // reviseKick
			if (BattleParameterFlag(2)) value = 99999;   // an event's OnForceMaxDamage
			return (int)Math.Max(1, value);
		}

		/// <summary>The bits a blow and its target bring to calcDamageValueForBabil's multipliers.</summary>
		private readonly struct Affinity
		{
			public readonly int Elements, Killer, Absorbs, Resists, RaceResists, Weak, Family;
			public Affinity(int elements, int killer, int absorbs, int resists, int raceResists, int weak, int family)
			{ Elements = elements; Killer = killer; Absorbs = absorbs; Resists = resists; RaceResists = raceResists; Weak = weak; Family = family; }
		}

		/// <summary>
		/// A fighter's affinities. A monster's from its record: the blow's elements at 0x26 and the races it is deadly to at
		/// 0x48 (its physical attack at 0x20), what it absorbs at 0x54, resists at 0x56 and the races it resists at 0x60
		/// (its physical defence at 0x4C), its weaknesses at 0x64 (its magic defence), its own race at 0x48 (family). A
		/// member's from the equipment (pl::Player::physicsAttack / physicsDefense): the weapons' elements (0x52) and
		/// killers (0x4E), the armour's elements as resisted - or absorbed, when a piece says so (0x52 bit 2) - and its
		/// races (0x4E) as resisted.
		/// </summary>
		private static Affinity AffinityOf(Fighter f)
		{
			if (f.IsMonster && f.Monster?.Raw != null && f.Monster.Raw.Length >= 0x66)
			{
				byte[] r = f.Monster.Raw;
				int U(int at) => BitConverter.ToUInt16(r, at);
				return new Affinity(U(0x26), U(0x48), U(0x54), f.ResistOverride >= 0 ? f.ResistOverride : U(0x56), U(0x60), f.WeakOverride >= 0 ? f.WeakOverride : U(0x64), U(0x48));
			}
			if (f.Member == null) return default;
			GameTables tables = Ff4Party.Tables;
			int elements = 0, killer = 0, armour = 0, races = 0;
			bool absorbing = false;
			foreach (int id in f.Member.Equipment)
			{
				ItemDefinition item = id != 0 ? tables.Item(id) : null;
				if (item?.Raw == null || item.Raw.Length < 0x54) continue;
				int bits = BitConverter.ToUInt16(item.Raw, 0x52), kill = BitConverter.ToUInt16(item.Raw, 0x4E);
				if (item.Kind == ItemKind.Weapon) { elements |= bits; killer |= kill; }
				else if (item.Kind == ItemKind.Armour) { armour |= bits; races |= kill; absorbing |= (bits & 4) != 0; }
			}
			if (f.ElementOverride >= 0 && f.ElementOverride != 0xFFFF) elements = (elements & 4) | f.ElementOverride;   // physicsAttack: Upgrade's element
			return new Affinity(elements, killer, absorbing ? armour : 0, absorbing ? 0 : armour, races, 0, 0);
		}

		/// <summary>
		/// calcDamageValueForBabil's elements: of the blow's elements (but 0x800 and 0x1000 and the low three), each the
		/// target is weak to multiplies it by 1.5 and each it resists halves it - or, with the resists' 0x1000 set, doubles
		/// and quarters. What it absorbs (the blow's elements and its absorbing ones in common) turns the resisted ones to
		/// its weaknesses' word, as the game does.
		/// </summary>
		private static float ElementFactor(Fighter attacker, Fighter target)
		{
			Affinity a = AffinityOf(attacker), t = AffinityOf(target);
			int elements = a.Elements | (Dark(attacker) && attacker != target ? 0x400 : 0);   // Darkness adds the dark element
			int common = t.Absorbs & elements & 0xf7f8;
			if ((t.Family & 0x100) != 0) common |= elements & 4;
			int counted = elements & 0xe7f8;
			int halving = common == 0 ? t.Resists : t.Weak;
			float factor = 1f;
			bool strong = (t.Resists >> 12 & 1) != 0;
			for (int bit = 0; bit < 16; bit++)
			{
				if ((counted >> bit & 1) == 0) continue;
				if (!strong)
				{
					if ((t.Weak >> bit & 1) != 0) factor *= 1.5f;
					if ((halving >> bit & 1) != 0) factor /= 2f;
				}
				else
				{
					if ((t.Weak >> bit & 1) != 0) factor *= 2f;
					if ((halving >> bit & 1) != 0) factor /= 4f;
				}
			}
			return factor;
		}

		/// <summary>calcDamageValueForBabil's races: each race the blow is deadly to that the target is multiplies it by 1.5, each it resists halves it.</summary>
		private static float RaceFactor(Fighter attacker, Fighter target)
		{
			Affinity a = AffinityOf(attacker), t = AffinityOf(target);
			int common = t.Absorbs & a.Elements & 0xf7f8;
			int resisted = common == 0 ? t.RaceResists : t.Family;
			float factor = 1f;
			for (int bit = 0; bit < 16; bit++)
			{
				if ((a.Killer >> bit & 1) == 0) continue;
				if ((t.Family >> bit & 1) != 0) factor *= 1.5f;
				if ((resisted >> bit & 1) != 0) factor /= 2f;
			}
			return factor;
		}

		/// <summary>
		/// NewAttackFormula::backPenalty: the attacker's own multipliers for the back rows (BaseBattleCharacter::formation
		/// 1, PlayerParty::formation's table), the one for standing there and the one for a target standing there. A
		/// member's: half from the back unless the weapon reaches (its record's 0x54 bit 1; bare hands do not), a target
		/// in the back untouched. A monster's: its record's (the Floating Eye's 1.0 and 0.75). Monsters stand in front.
		/// </summary>
		private static float BackRowFactor(Fighter attacker, Fighter target)
		{
			float own = 1f, theirs = 1f;
			if (attacker.IsMonster && attacker.Monster != null) { own = attacker.Monster.BackRowAttack; theirs = attacker.Monster.BackRowTarget; }
			else if (attacker.Member != null) own = WeaponReaches(attacker.Member) ? 1f : 0.5f;
			float factor = 1f;
			if (InBackRow(attacker)) factor *= own;
			if (InBackRow(target)) factor *= theirs;
			return factor;
		}

		private static bool InBackRow(Fighter f) => !f.IsMonster && f.Member != null && Ff4Party.RowOf(Ff4Party.PositionOf(f.Member.Id)) == 1;

		/// <summary>Whether a member's weapon reaches from the back row (pl::Player::physicsAttack: the weapon's 0x54 bit 1 - a bow, a boomerang).</summary>
		private static bool WeaponReaches(Character c)
		{
			GameTables tables = Ff4Party.Tables;
			foreach (int slot in new[] { (int)EquipSlot.RightHand, (int)EquipSlot.LeftHand })
			{
				ItemDefinition item = c.Equipment[slot] != 0 ? tables.Item(c.Equipment[slot]) : null;
				if (item?.Kind != ItemKind.Weapon || item.Raw == null || item.Raw.Length < 0x56) continue;
				return (BitConverter.ToUInt16(item.Raw, 0x54) & 2) != 0;
			}
			return false;
		}

		private void MemberAttacks(Fighter member, Fighter foe, bool aim = false)
		{
			// The one picked fell meanwhile: FF4 turns the blow on another (none left, nothing).
			if (!foe.Alive) foe = FirstAlive(_foes);
			if (foe == null) { member.Gauge = 0f; return; }
			Acted(member, aim ? CmdAim : 1, foe);
			if (Ff4BattleStage.Active && member.Poise > 0 && member.SwingA > 0)
			{
				// Steam's attack, frame by frame: the weapon's poise (1047) for 15 frames, the swing (96, then 95, by
				// turns) - the blow lands 8 frames in, its number pops when the swing ends - and back to the stance.
				// Aim (Steam's frames): the poise held since the decision, the swing 2 frames after the invoke stage.
				int swing = member.Swings++ % 2 == 0 ? member.SwingA : (member.SwingB > 0 ? member.SwingB : member.SwingA);
				int lead = aim ? 0 : 15;   // Aim: the invoke's 2-frame gap is its lead
				EndTurn(member);
				if (!aim) Play(member, member.Poise, false, 3);
				member.Acted = false;
				int damage = 0;
				bool hit = false;
				After(lead, () => Play(member, swing, false, 3));
				After(lead + 8, () =>
				{
					hit = aim ? !foe.Mist : Hits(member, foe);   // Aim: calcDamage skips the hit roll
					if (!hit) return;
					PlayEffect(member.HitEffect, HitEffectSpot(foe));
					damage = Damage(member, foe);
					foe.Hp = Math.Max(0, foe.Hp - damage);
					if (foe.Member != null) foe.Member.Hp = foe.Hp;
					BlowLands(member, foe);
					// With the effect, its sound: the weapon system's own (playerWeaponSe - a sword's 106, 1).
					if (member.HitBank >= 0) Game.Audio.PlaySe(member.HitBank, member.HitSound);
					else Game.Audio.PlaySe(0, 3);
				});
				// The swing's end (Cecil's sword 16 frames, Rosa's bow 19 in Steam's frames): the stance and the number.
				int swingWait = 0;
				void SwingEnd()
				{
					if (member.Npc != null && member.Acted && !member.Npc.MotionDone && ++swingWait < 30) { After(1, SwingEnd); return; }
					Play(member, _heroMotionIdle, true, 3);
					member.Acted = false;
					if (!hit)
					{
						PopWord(DamageSpot(foe), Ff4Ui.WordMiss);
						Note(member.Name + " misses " + foe.Name + ".");
						return;
					}
					Pop(DamageSpot(foe), damage);
					Note(member.Name + " hits " + foe.Name + " for " + damage + ".");
					if (!foe.Alive) Fell(foe, damage);
				}
				After(lead + 16, SwingEnd);
				return;
			}
			Play(member, _heroMotionAttack);
			if (!(aim ? !foe.Mist : Hits(member, foe)))
			{
				PopWord(DamageSpot(foe), Ff4Ui.WordMiss);
				Note(member.Name + " misses " + foe.Name + ".");
			}
			else
			{
				int damage = Damage(member, foe);
				foe.Hp = Math.Max(0, foe.Hp - damage);
				if (foe.Member != null) foe.Member.Hp = foe.Hp;
				BlowLands(member, foe);
				Pop(DamageSpot(foe), damage);
				Game.Audio.PlaySe(0, 3);
				Note(member.Name + " hits " + foe.Name + " for " + damage + ".");
				if (!foe.Alive) Fell(foe, damage);
			}
			member.Gauge = 0f;
		}

		// A monster's plain attack (btl::BattleMonsterBehavior::normalAttack): its record in monster.chaindata chain 2 says
		// when, counted from the action's start, its effect plays on the target (createEffect, at hitEffectPosition), its
		// sound (playSE) and its number (createHit2D) - the Floating Eye's all at 7. A miss plays effect 240 instead. The
		// blow is settled as the action starts (calcBattleParameter) and shown on those frames. Ours count one more than
		// the record's (Steam's frames: the Eye's 7 lands 8 frames into its 201).
		private const int MonsterFrameLead = 1;
		private const int MissEffect = 240;

		// ---- a monster's turn as btl::MonsterActionThinker::calculationAction decides it ----

		/// <summary>
		/// The monster's action this turn: its AI record's conditions tried in order (agreeConditionAction, every check of a
		/// condition's mask holding - isEnableCondition), the first that holds bringing its action set, else the record's
		/// default set; a new set starts from its first entry. The entry drawn at random, or taken in turn (the index moving
		/// on each turn, back to the first past the last). An entry is an ability (1 the plain attack, 0 nothing) and a
		/// target type.
		/// </summary>
		private (int Ability, int Target) DecideMonsterAction(Fighter foe)
		{
			GameTables t = Ff4Party.Tables;
			if (ForcedMonsterAction(foe) is (int, int) forced)
			{
				// checkRestrictionConditionAction: a status drives it; its place in its set still moves on.
				foe.AiIndex = foe.AiIndex + 1 < 10 ? foe.AiIndex + 1 : 0;
				Note(foe.Name + " is driven: ability " + forced.Item1 + " on target type " + forced.Item2);
				return forced;
			}
			if (foe.Monster == null || !t.MonsterAi.TryGetValue(foe.Monster.Id, out short[] ai)) return (1, 1);
			int condition = -1, set = ai[1];
			for (int k = 2; k <= 5; k++)
			{
				if (ai[k] < 0 || !t.MonsterActionConditions.TryGetValue(ai[k], out (int TurnAction, ulong Checks) c)) continue;
				if (!ConditionHolds(foe, c.Checks)) continue;
				condition = ai[k];
				set = c.TurnAction;
				break;
			}
			if (foe.AiCondition != condition) { foe.AiCondition = condition; foe.AiIndex = 0; }
			if (!t.MonsterTurnActions.TryGetValue(set, out MonsterTurnAction actions) || actions.Entries.Count == 0) return (1, 1);
			int i = actions.Random ? _random.Next(actions.Entries.Count) : foe.AiIndex;
			if (i >= actions.Entries.Count) i = 0;
			foe.AiIndex = i + 1 < 10 ? i + 1 : 0;
			Note(foe.Name + " thinks: set " + set + (condition >= 0 ? " (condition " + condition + ")" : "") + ", entry " + i + " - ability " + actions.Entries[i].Ability + " on target type " + actions.Entries[i].Target);
			return actions.Entries[i];
		}

		/// <summary>A monster's plain attack on one target (normalAttack's record: the effect, sound and number on their frames).</summary>
		private void MonsterAttack(Fighter foe, Fighter target, MonsterAttackStyle style = null)
		{
			// serchExecuteCoverMan: a plain blow on a member may be taken by one who covers them (or, the target in Critical, by
			// anyone with the Cover command): placed just in front of them, it takes the blow at three quarters.
			Fighter covered = null;
			if (foe.DecidedAbility <= 1 && style == null && target.Member != null && CoverMan(target) is Fighter coverer)
			{
				covered = target;
				target = coverer;
			}
			Acted(foe, foe.DecidedAbility > 1 ? foe.DecidedAbility : 1, target);   // an ability not in yet is struck as a blow, but known by its id
			try { foe.Npc.PlayMotion(style != null && style.Motion > 0 ? style.Motion : 201, false, 3); foe.Acted = true; } catch (Exception) { }
			foe.Gauge = 0f;
			bool hit = Hits(foe, target);
			int damage = hit ? Damage(foe, target) * (style?.Factor ?? 1) : 0;
			if (covered != null && hit) damage = Math.Max(1, damage * 3 / 4);   // reviseCover
			if (!Ff4BattleStage.Active) { MonsterBlow(foe, target, hit, damage); return; }
			MonsterDefinition m = foe.Monster;
			int effect = style != null && style.Pack >= 0 ? style.Pack : m.AttackEffect;
			int effectFrame = style != null && style.EffectFrame >= 0 ? style.EffectFrame : m.AttackEffectFrame;
			int seBank = style != null && style.SeBank >= 0 ? style.SeBank : m.AttackSoundBank, se = style != null && style.SeNumber >= 0 ? style.SeNumber : m.AttackSound;
			int seFrame = style != null && style.SeFrame >= 0 ? style.SeFrame : m.AttackSoundFrame;
			int numberFrame = style != null && style.NumberFrame >= 0 ? style.NumberFrame : m.AttackNumberFrame;
			bool feet = style != null && style.Feet;
			if (covered != null)
			{
				Fighter guard = target, under = covered;
				After(effectFrame + MonsterFrameLead, () => CoverStep(guard, under));
				After(effectFrame + MonsterFrameLead + 16, () => CoverBack(guard));   // Steam's frames: 2002 for 15, the stance 1, home on the 16th
			}
			After(effectFrame + MonsterFrameLead, () => PlayEffect(hit ? effect : MissEffect, feet ? Where(target) : HitEffectSpot(target)));
			if (hit && seBank >= 0 && se >= 0) After(seFrame + MonsterFrameLead, () => Game.Audio.PlaySe(seBank, se));
			After(numberFrame + MonsterFrameLead, () => MonsterBlow(foe, target, hit, damage));
		}

		private void MonsterBlow(Fighter foe, Fighter target, bool hit, int damage)
		{
			if (!target.Alive) return;
			if (!hit)
			{
				PopWord(DamageSpot(target), Ff4Ui.WordMiss);
				Note(foe.Name + " misses " + target.Name + ".");
				return;
			}
			if (target.Defending) damage = Math.Max(1, damage / 2);   // Defend halves a blow
			target.Hp = Math.Max(0, target.Hp - damage);
			if (target.Member != null) target.Member.Hp = target.Hp;
			BlowLands(foe, target);
			Pop(DamageSpot(target), damage);
			// FF4 starts 1117 here (btl::BattleActionDamage, the b_ set's clip C117) and Steam's trace has it for a frame
			// before the stance - but its frames show the stance throughout (3784, the 1117 frame, and 3788), where ours draws
			// C117 as a turn of the whole body over several frames. Until that difference is found the stance holds, as
			// Steam's frames show.
			Fighter hurt = target;
			After(1, () => { if (hurt.Alive && hurt.Acted) { Play(hurt, _heroMotionIdle, true, 0); hurt.Acted = false; } });
			Note(foe.Name + " hits " + target.Name + " for " + damage + ".");
			if (!target.Alive) Fell(target, damage);
		}

		/// <summary>What a consumable does in a fight, from efficacy.beld: hit or magic points back (9999 for all), or a revival (Phoenix Down's efficacy 17 restores nothing by number). Null for anything else.</summary>
		private static Efficacy ItemEffect(ItemDefinition item)
		{
			Efficacy e = item.EfficacyId > 0 ? Ff4Party.Tables.Efficacy(item.EfficacyId) : null;
			if (e == null || e.CastsAbility > 0) return null;
			return e.Hp > 0 || e.Mp > 0 || e.Id == 17 ? e : null;
		}

		private void UseItem(Fighter member, int itemId, Fighter target)
		{
			ItemDefinition item = Ff4Party.Tables.Item(itemId);
			Efficacy effect = item != null ? ItemEffect(item) : null;
			if (effect == null) return;
			bool revive = effect.Id == 17;
			if (revive == target.Alive)
			{
				Say(item.Name + " does nothing for " + target.Name + ".");
				return;
			}
			if (!Ff4Party.Party.RemoveItem(itemId, 1)) { member.Gauge = 0f; return; }
			member.Gauge = 0f;
			// The item motion (62) held 30 frames, as Steam's Salve shows it, then the item's work and its number.
			Play(member, 62, false, 3);
			member.Acted = false;
			After(30, () =>
			{
				Play(member, member.IdleMotion, true, 3);
				if (revive == target.Alive) return;
				int before = target.Hp;
				if (revive) target.Hp = Math.Max(1, target.MaxHp / 4);
				else if (effect.Hp > 0) target.Hp = Math.Min(target.MaxHp, target.Hp + effect.Hp);
				target.Member.Hp = target.Hp;
				if (effect.Mp > 0) target.Member.Mp = Math.Min(target.Member.MaxMp, target.Member.Mp + effect.Mp);
				if (target.Hp != before) Pop(DamageSpot(target), target.Hp - before, true);
				Say(member.Name + " uses " + item.Name + ": " + target.Name + (revive ? " rises." : (effect.Hp > 0 ? " +" + (target.Hp - before) + " HP" : "") + (effect.Mp > 0 ? " +" + effect.Mp + " MP" : "") + "."));
			});
		}

		private const int CommandRows = 4;

		/// <summary>Defend: the member braces until their next turn, physical blows halved (FF4's Defend).</summary>
		private void Defend(Fighter member)
		{
			member.Defending = true;
			Note(member.Name + " defends.");
			EndTurn(member);
		}

		/// <summary>Swap Rows: the party's formation turns over (PlayerParty::formation's other half) - front row to back and back to front - and everyone walks to their new spot.</summary>
		private void SwapRows(Fighter member)
		{
			Ff4Party.Formation = 1 - Ff4Party.Formation;
			foreach (Fighter f in _party)
			{
				int position = Ff4Party.PositionOf(f.Member.Id), row = Ff4Party.RowOf(position, Ff4Party.Formation);
				Vector3 spot = Ff4BattleStage.PartySpot(_rootId, position, row);
				f.Home = spot;
				try { f.Npc?.MoveTo(spot, 10); } catch (Exception) { }
			}
			Say("The party swaps rows.");
			EndTurn(member);
		}

		/// <summary>Darkness: a blow at every foe at once that costs the knight an eighth of their hit points (as the DS remake's Darkness reads; its own formula is not ported yet).</summary>
		/// <summary>
		/// Darkness (BattlePlayerBehavior::stateDark): the state comes on (ys::Condition 0x17) and the turn is over, with no
		/// blow. It lasts condition_parameter.bbd's 450 (the timer that times 4096 less the battle's speed rate a frame -
		/// calcConditionTime); while it is on and HP is over 1, a blow carries the dark element (0x400) and is doubled
		/// (reviseDarkness), each costing its striker HP after (DarkCost).
		/// </summary>
		private void Darkness(Fighter member) => Invoke(member, 32, () => DarknessOn(member));

		private void DarknessOn(Fighter member)
		{
			member.DarkFrames = DarknessFrames * Tick;
			Note(member.Name + " is wreathed in darkness (" + DarknessFrames + " frames).");
			EndTurn(member);
		}

		private const int DarknessFrames = 450;

		/// <summary>Whether a fighter's blows are dark now: the state on and more than 1 HP left.</summary>
		private static bool Dark(Fighter f) => f.DarkFrames > 0 && f.Hp > 1;

		/// <summary>DarkFormula::calcDarkSubHp, after a dark blow (BaseBattleCharacter flag 0x39): a tenth of the maximum, never the last point, shown on the striker.</summary>
		private void DarkCost(Fighter f)
		{
			if (!f.DarkCostDue) return;
			f.DarkCostDue = false;
			int cost = f.MaxHp / 10;
			if (f.Hp <= cost) cost = f.Hp - 1;
			if (cost <= 0) return;
			f.Hp -= cost;
			if (f.Member != null) f.Member.Hp = f.Hp;
			Pop(DamageSpot(f), cost);
			Note(f.Name + " pays " + cost + " HP to the darkness.");
		}

		// ---- btl::BattleBehaviorManager: decided actions wait their turn and go one at a time ----

		private readonly List<(Fighter Actor, Action Act)> _queue = new List<(Fighter, Action)>();
		private readonly List<Fighter> _dying = new List<Fighter>();
		private readonly List<int> _turnEffects = new List<int>();
		private readonly Dictionary<int, int> _turnEffectStart = new Dictionary<int, int>();
		private const int TurnEffectMost = 600;   // ours: an effect that loops would hold the turn for ever - let go after 20 s
		private Fighter _executing;

		/// <summary>The wait mode (the config's battle mode, gpInstance 0x94 bit 0): the gauges stand while a member has a list open. Off, the active mode, unless --ff4-battle-wait.</summary>
		private static bool WaitMode => Options.Get("ff4-battle-wait") != null;

		/// <summary>
		/// A member's command is decided (commandSelected): the menu closes and the action waits its turn, the gauge held
		/// full. An action with a wait (atwMax: ability.bbd's, a spell's own, an item's ability's) first waits it out - the
		/// ATW filling a frame at a time while the gauges run (addActiveTimeGage, ATG state 2) - and only then joins the
		/// turns (state 3).
		/// </summary>
		/// <summary>The command a decision is by: a spell's school's (Ninjutsu 0x53, Bardsong 18, the magic commands), else the ability itself.</summary>
		private static int DecisionCommand(int ability)
		{
			SpellDefinition spell = ability >= 1000 ? Ff4Party.Tables?.Spell(ability) : null;
			return spell != null ? SchoolCommand[Math.Clamp((int)spell.School, 0, 7)] : ability;
		}

		private void Decide(Fighter member, Action act, int wait = 0, int ability = 1)
		{
			// Steam's frames: a member whose command has ability.bbd +0x24 bit 6 (Kick, Aim, Steal, Throw - not Pray) stands in its
			// weapon's poise (Yang's 1058, Rosa's 1060) from the decision until the action starts.
			if (member.Member != null && member.Poise > 0 && member.Alive && !member.Airborne && ability != CmdTwincast && (Ff4Party.Tables?.AbilityFlags(DecisionCommand(ability)) & 0x40) != 0) Play(member, member.Poise, true, 3);   // Twincast: its pair wait (99) instead
			member.Defending = member.Braced = false;   // decideAbility: any decision ends Defend (flag 3) and Brace (flag 4)
			_abilityCmd = 0;
			member.Queued = true;
			member.DecidedAbility = ability;
			if (wait > 0)
			{
				member.AtwLeft = member.AtwMax = wait * Tick;
				member.Pending = act;
			}
			else
			{
				member.AtwMax = 0;
				_queue.Add((member, act));
			}
			_acting = null;
			_pick = Pick.None;
			_casting = null;
		}

		/// <summary>The wait a spell has before it is cast (ability.bbd by its id).</summary>
		private static int SpellWait(SpellDefinition spell) => spell != null ? Ff4Party.Tables.AbilityWait(spell.Id) : 0;

		/// <summary>The wait an item has before it is used: the ability it invokes (its record's s16 at 0x14), else its own id, in ability.bbd.</summary>
		private static int ItemWait(int itemId)
		{
			ItemDefinition item = Ff4Party.Tables.Item(itemId);
			int ability = item?.Raw != null && item.Raw.Length >= 0x16 ? BitConverter.ToInt16(item.Raw, 0x14) : 0;
			return Ff4Party.Tables.AbilityWait(ability > 0 ? ability : itemId);
		}

		/// <summary>The next decided action starts (startBehavior), its actor's gauge emptied by it.</summary>
		private void StartNextAction()
		{
			(Fighter actor, Action act) = _queue[0];
			_queue.RemoveAt(0);
			actor.Queued = _queue.Exists(e => e.Actor == actor);   // a counter run first leaves its own turn queued
			if (!actor.Alive) return;
			if (!CanAct(actor) && !_poisonTicks.Contains(act)) { Note(actor.Name + " cannot act."); return; }
			_executing = actor;
			_attacker = actor;
			_isCounter = false;
			_lastTargets.Clear();
			_turnEffects.Clear();
			try { act(); } catch (Exception ex) { Log.Write(LogChannel.General, "battle: action: " + ex.Message); }
		}

		/// <summary>Whether an effect the turn set off still plays (btl::BattleEffect::isPlay); the finished ones are let go.</summary>
		private bool TurnEffectsPlaying()
		{
			bool playing = false;
			GlobalScope.eff.CEffectMng effects = GlobalScope.eff.CEffectMng.instance();
			for (int i = _turnEffects.Count - 1; i >= 0; i--)
			{
				bool stale = _turnEffectStart.TryGetValue(_turnEffects[i], out int since) && _clock - since > TurnEffectMost;
				if (stale) Log.Write(LogChannel.General, "battle: turn effect " + _turnEffects[i] + " still playing after " + TurnEffectMost + " frames - let go");
				if (!stale && effects.isPlay(_turnEffects[i])) { playing = true; continue; }
				_turnEffectStart.Remove(_turnEffects[i]);
				try { effects.release(_turnEffects[i]); } catch (Exception) { }
				_turnEffects.RemoveAt(i);
			}
			return playing;
		}

		/// <summary>
		/// The turn is over (turnExecute done). The fallen go (dieState: deadCharacters, the death sound, BPTranslucence),
		/// and then the battle may be over (isEndOfBattle): every monster down is the win, every member the loss.
		/// </summary>
		private void TurnEnd()
		{
			Log.Write(LogChannel.File, "battle: " + _executing.Name + "'s turn is over (step " + LegacyStep.Count + ")");
			DarkCost(_executing);
			AfterAction(_executing);
			_executing = null;
			// The after event (executeState: a normal action's turn over), the turn finishing once it is.
			if (_eventParty != null && _eventParty.AfterEvent >= 0 && !StartEvent(EventKind.After, _eventParty.AfterEvent)) return;
			FinishTurn();
		}

		/// <summary>The rest of a turn's end, after its event: the fallen go, then the battle may be over.</summary>
		private void FinishTurn()
		{
			if (_closing) return;
			int dying = 0;
			if (_dying.Count > 0)
			{
				Game.Audio.PlaySe(0x65, 6);
				foreach (Fighter f in _dying) StartDeathFade(f);
				_dying.Clear();
				dying = DeathFrames;
			}
			if (_foes.TrueForAll(OutOfFight))
			{
				_queue.Clear();
				if (Ff4BattleStage.Active) After(dying + WinAfterDeath, BeginWin);
				else Win();
			}
			else if (_party.TrueForAll(OutOfFight))
			{
				_queue.Clear();
				Lose();
			}
			else CheckCounters();
		}

		private static Fighter FirstAlive(List<Fighter> side) => side.Find(f => f.Alive);

		private void EndTurn(Fighter member)
		{
			member.Gauge = 0f;
		}

		private void Run(Fighter member)
		{
			if (_random.Next(100) < 60)
			{
				Say("The party runs.");
				_phase = Phase.Outro;
			}
			else
			{
				Say("Could not run!");
				member.Gauge = 0f;
			}
		}

		// The victory as Steam stages it (btl::BattleWin::layout, pl::layoutCharacterScene): the members on the layout
		// for the party's size, turned as it says, in the win pose (2007, then 2009 after 45 frames), the camera easing
		// to the layout's over 56 frames from where Steam's starts (fitted off its frames for one member).
		private int _victoryCameraFrame = -1;
		private float[] _victoryFrom, _victoryFromTarget, _victoryTo, _victoryToTarget;

		// btl::BattleWin: initialize fades both screens out to black in 6 frames (the panel gone with it); at black
		// waitFadePhase sets the result window up and changeBGMPhase poses the winners and readies the ending camera;
		// startFadeInPhase fades in over 6; enjoyPhase holds 30 frames once in; then the gil (windowOpenPhase), the
		// experience, and the page arrow waiting on a key. Steam's frames (two fights): the fade under way 24 frames
		// after the last number's first digit (7 after the dead fade), black at +31, the gil at +67, the experience at
		// +76, the arrow at +79.
		private const int WinAfterDeath = 7, WinFade = 6, WinHold = 30, WinExpAfter = 9, WinArrowAfter = 12, WinEndFade = 16;
		private bool _closing;
		private bool _gameOver;   // the loss ends the game: the title comes once the fight is closed

		/// <summary>The last foe is down: the panel goes and the screens fade to black, the result coming in as they return.</summary>
		private void BeginWin()
		{
			_closing = true;
			Note("the last foe is down - closing (step " + LegacyStep.Count + ")");
			GlobalScope.dgs.CFade.Main().fadeOut(WinFade, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			GlobalScope.dgs.CFade.Sub().fadeOut(WinFade, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			After(WinFade, () =>
			{
				VictoryPose();
				GlobalScope.dgs.CFade.Main().fadeIn(WinFade);
				GlobalScope.dgs.CFade.Sub().fadeIn(WinFade);
			});
		}

		private void VictoryPose()
		{
			VictoryLayout layout = Ff4BattleStage.Active ? Ff4Party.Tables?.VictoryLayout(_party.FindAll(f => f.Alive).Count - 1) : null;
			if (layout != null)
			{
				int k = 0;
				foreach (Fighter f in _party)
				{
					if (!f.Alive || f.Npc == null || k >= 5) continue;
					PartyRootSlot spot = layout.Spots[k++];
					f.Npc.Teleport(new Vector3(spot.X, spot.Y, spot.Z));
					Face(f, spot.Facing);
					Play(f, 2007, false, 0);
					f.Acted = false;
					Fighter winner = f;
					After(45, () => { Play(winner, 2009, true, 3); winner.Acted = false; });
				}
				_victoryFrom = new[] { 2.694f, 27.396f, 114.497f };
				_victoryFromTarget = new[] { -3.997f, -2.996f, -18.2f };
				_victoryTo = layout.CameraPosition;
				_victoryToTarget = layout.CameraTarget;
				_victoryCameraFrame = 0;
			}
			Win();
		}

		private void StepVictoryCamera()
		{
			if (_victoryCameraFrame < 0 || _victoryTo == null) return;
			const int frames = 56;
			float t = Math.Min(1f, _victoryCameraFrame / (float)frames);
			float p = (float)Math.Sin(t * Math.PI / 2);
			int Fx(float a, float b) => (int)Math.Round((a + (b - a) * p) * 4096);
			try
			{
				Ff4EventCamera.MoveTo(Fx(_victoryFrom[0], _victoryTo[0]), Fx(_victoryFrom[1], _victoryTo[1]), Fx(_victoryFrom[2], _victoryTo[2]), 0, false);
				Ff4EventCamera.LookAt(Fx(_victoryFromTarget[0], _victoryToTarget[0]), Fx(_victoryFromTarget[1], _victoryToTarget[1]), Fx(_victoryFromTarget[2], _victoryToTarget[2]), 0);
			}
			catch (Exception) { }
			if (_victoryCameraFrame++ >= frames) _victoryCameraFrame = -1;
		}

		private void Win()
		{
			_phase = Phase.Victory;
			_timer = 0;
			Party party = Ff4Party.Party;
			List<Fighter> alive = _party.FindAll(f => f.Alive);
			_gilBefore = party.Gil;
			_gilShown = party.Gil;
			_resultLines.Clear();
			_dropsWon.Clear();
			RollGift();
			List<string> lines = _resultLines;
			party.Gil += _gilWon;
			_pages.Clear();
			_page = 0;
			_pages.Add(new ResultPage { Kind = "gil" });
			foreach (Fighter f in alive)
			{
				Character c = f.Member;
				int before = c.Level, hp = c.MaxHp, mp = c.MaxMp;
				OpenFF.Data.Stats stats = c.Base.Clone();
				c.Experience += _expWon / Math.Max(1, alive.Count);
				int level = Ff4Party.Tables.LevelForExperience(c.Experience);
				if (level <= before) continue;
				c.SetLevel(level, false);
				lines.Add(f.Name + " reaches level " + level + "!");
				// btl::BattleLevelupBehavior: a page for the member - "<name>'s level increased!" (babil_battle 109), the
				// level, HP and MP on the left and the five stats on the right, each the old value, the arrow, the new.
				ResultPage page = new ResultPage { Kind = "levelup", Title = BattleText(109, "%SCC00%'s level increased!").Replace("%SCC00%", f.Name) };
				page.Rows.Add((BattleText(200, "Lv"), before, c.Level));
				page.Rows.Add((BattleText(201, "HP"), hp, c.MaxHp));
				page.Rows.Add((BattleText(202, "MP"), mp, c.MaxMp));
				page.Rows.Add((BattleText(203, "Strength"), stats.Strength, c.Base.Strength));
				page.Rows.Add((BattleText(204, "Speed"), stats.Agility, c.Base.Agility));
				page.Rows.Add((BattleText(205, "Stamina"), stats.Vitality, c.Base.Vitality));
				page.Rows.Add((BattleText(206, "Intellect"), stats.Intellect, c.Base.Intellect));
				page.Rows.Add((BattleText(207, "Spirit"), stats.Spirit, c.Base.Spirit));
				foreach (int id in c.Learn())
				{
					string name = Ff4Party.Tables.Spell(id)?.Name ?? ("spell " + id);
					page.Lines.Add(name);
					lines.Add(f.Name + " learns " + name + "!");
				}
				_pages.Add(page);
			}
			if (_dropsWon.Count > 0)
			{
				// drawAcquiredItem: "Item" (babil_battle 111) and what the fight brought.
				ResultPage items = new ResultPage { Kind = "item", Title = BattleText(111, "Item") };
				foreach (int id in _dropsWon)
				{
					party.AddItem(id, 1);
					string name = Ff4Party.Tables.Item(id)?.Name ?? ("item " + id);
					items.Lines.Add(name);
					lines.Add("Found " + name + ".");
				}
				_pages.Add(items);
			}
			if (!Ff4BattleStage.Active) foreach (Fighter f in _party) { if (f.Alive) Play(f, 2007); }   // on the stage VictoryPose has posed them
			Log.Write(LogChannel.General, "battle: won - " + _expWon + " exp, " + _gilWon + " gil. " + string.Join(" ", lines));
		}

		private void Lose()
		{
			_phase = Phase.Defeat;
			_timer = 0;
			// BattleLose::initialize: the help line's babil_battle 0x70 (not when the event hides it - flag 0xB), the music
			// stopping over 15 frames.
			if (!BattleParameterFlag(0xB)) { _help = BattleText(0x70, "Your party has been defeated."); _helpUntil = -1; }
			// Every slot: a scene's battle plays its music in whichever slot its script chose.
			try { for (int slot = 0; slot < 4; slot++) GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().stop(15, (GlobalScope.MatrixSound.enMtxBGMSlot)slot); } catch (Exception) { }
			Log.Write(LogChannel.General, "battle: lost - " + (_help ?? "(no line)"));
		}

		// ---- random encounters: the map's encounter chain, rolled per unit walked ----

		private Vector3 _lastStep;
		private string _noTableLogged;
		private float _walked;
		private int _sinceBattle;

		private void Encounters()
		{
			if (EncounterZoom.Playing) return;
			_sinceBattle++;
			if (_sinceBattle < 90 || !Game.Hero.Present) return;
			Vector3 at = Game.Hero.Position;
			float step = (at - _lastStep).Flat.Length;
			_lastStep = at;
			if (step <= 0.01f || step > 20f) return;
			Ff4Encounters.Table table = Ff4Encounters.For(Game.Field.Map);
			if (table == null || table.Rate <= 0 || table.Parties.Count == 0)
			{
				if (_noTableLogged != Game.Field.Map) { _noTableLogged = Game.Field.Map; Log.Write(LogChannel.File, "encounters: map '" + Game.Field.Map + "' has no encounter table here"); }
				return;
			}
			_walked += step;
			if (_walked < 1f) return;
			_walked -= 1f;
			// The rate is the map's own per-land-form number (1 on the Baron plain, 9 in the Watery Pass);
			// one unit here is a small stride, so about one fight in a few hundred units at rate 9.
			if (_random.Next(4096) < table.Rate * 3)
			{
				int party = table.Roll(_random);
				if (party > 0 && Ff4Party.Tables?.MonsterParty(party) != null)
				{
					// WSEncountDirection1: the field stops and the encounter's zoom plays to white; the fight then.
					_sinceBattle = 0;
					Game.Hero.Freeze();
					Game.Input.Capture = true;
					try { Game.Hero.PlayMotion(1000, true); } catch (Exception) { }
					int battleMap = table.BattleMap;
					EncounterZoom.Start(() =>
					{
						if (StartParty(party, false, battleMap)) return;
						// It could not start: the field again.
						try { Game.Hero.Unfreeze(); } catch (Exception) { }
						Game.Input.Capture = false;
						GlobalScope.dgs.CFade.Main().fadeIn(15);
						GlobalScope.dgs.CFade.Sub().fadeIn(15);
					});
				}
			}
		}

		private void End()
		{
			Log.Write(LogChannel.File, "battle: over (step " + LegacyStep.Count + ")");
			_sinceBattle = 0;
			_lastStep = Game.Hero.Present ? Game.Hero.Position : Vector3.Zero;
			Action after = AfterBattle;
			AfterBattle = null;
			foreach (Fighter f in _foes)
			{
				try { f.Npc?.Remove(); } catch (Exception) { }
				try { f.MistNpc?.Remove(); } catch (Exception) { }
				try { f.FormNpc?.Remove(); } catch (Exception) { }
				RemoveLegs(f);
			}
			foreach (Fighter f in _party)
			{
				f.Airborne = false;
				if (f.Member != null) f.Member.Conditions = KeptAfterBattle(f.Conditions);   // clearBattleCondition
				try { f.FormNpc?.Remove(); } catch (Exception) { }
				DropStatusEffect(f);
				try { if (f.Npc is LegacyNpc held && held.CharacterId >= 0) Ff4Cutscene.UnbindAll(held.CharacterId); } catch (Exception) { }
				try { f.Npc?.Remove(); } catch (Exception) { }
			}
			_foes.Clear();
			_party.Clear();
			_pops.Clear();
			_cues.Clear();
			_victoryCameraFrame = -1;
			if (Ff4BattleStage.Active)
			{
				try { EngineApi.HeroPlayer?.setHidden(false); } catch (Exception) { }
				try { Ff4EventCamera.Release(); } catch (Exception) { }
				Ff4BattleStage.Leave(jumpBack: after == null && !_gameOver);
			}
			if (_gameOver)
			{
				// The game's end: on to the title, as from the logos.
				_gameOver = false;
				Log.Write(LogChannel.General, "battle: the party has fallen - to the title");
				try { GlobalScope.ds.g_Pad.enable(); GlobalScope.ds.g_TouchPanel.enable(); GlobalScope.wld.CBaseSystem.setTitle(true); } catch (Exception ex) { Log.Write(LogChannel.General, "battle: to the title: " + ex.Message); }   // the field ends into the title part (ff3Command_GoToTitle's way)
			}
			try { Game.Hero.Unfreeze(); } catch (Exception) { }
			Game.Input.Capture = false;
			_phase = Phase.Idle;
			_acting = null;
			_pick = Pick.None;
			after?.Invoke();
		}

		// ---- the HUD from FF4's own pieces (Ff4Ui): its window frames over its fill, the glove, the
		// ATB gauge - laid out as the Steam build shows them (a 1136 x 640 UI drawn into 800 x 480):
		// the command window bottom left, the party's rows bottom right, "Z Confirm  M Run away"
		// over them, the target pick listing the foes with the chance to hit and a card of the picked
		// one, the result as a window at the top. Drawn rectangles stand in when a sheet is missing.

		private static readonly Color PanelFill = new Color(20, 34, 74, 210);
		private static readonly Color PanelEdge = new Color(214, 218, 242, 255);
		private static readonly Color RowLine = new Color(170, 176, 230, 110);
		private static readonly Color Dim = new Color(186, 190, 218);
		private static readonly Color Gold = new Color(255, 232, 110);
		// The command window: four rows; the party window: a row per member.
		// Steam's battle panel, measured off its 1920 x 1080 frames into the 800 x 480 view: the commands a column of
		// separate rows (x 62.5..250, 42 high every 45 from y 301) with a scroll bar beside (to 276), the party five rows
		// (x 278..733, 26 high every 27.9 from y 339), each row a translucent lavender panel - (55, 58, 113) over half
		// of what is under it, the lit one (86, 106, 178) over 72% - the keys over them at y 309.
		private const float CmdX = 62.5f, CmdY = 301f, CmdW = 187.5f, CmdH = 177.7f, CmdRow = 45f, CmdRowH = 42.2f;
		private const float ScrollX = 251f, ScrollW = 25f;
		private const float PartyX = 278f, PartyY = 339f, PartyW = 455f, PartyH = 137.5f, PartyRow = 27.9f, PartyRowH = 26f;
		private const int TextSize = 11, HintSize = 10;
		private static readonly Color RowFill = new Color(55, 58, 113, 128), RowLit = new Color(86, 106, 178, 184);
		private static readonly Color RowEdgeLight = new Color(150, 156, 222, 220), RowEdgeDark = new Color(26, 28, 66, 200);

		private void Window(DrawList d, float x, float y, float w, float h)
		{
			if (Ff4Ui.Window(d, x, y, w, h)) return;
			d.Rect(x, y, w, h, PanelFill);
			d.Rect(x, y, w, h, PanelEdge, false);
		}

		/// <summary>One of Steam's row panels: the translucent fill, a light edge along the top and left, a dark one along the bottom and right.</summary>
		private void RowPanel(DrawList d, float x, float y, float w, float h, bool lit)
		{
			d.Rect(x, y, w, h, lit ? RowLit : RowFill);
			d.Line(x, y, x + w, y, RowEdgeLight);
			d.Line(x, y, x, y + h, RowEdgeLight);
			d.Line(x, y + h, x + w, y + h, RowEdgeDark);
			d.Line(x + w, y, x + w, y + h, RowEdgeDark);
		}

		/// <summary>The command list's scroll bar: a track the height of the rows, arrows at its ends, the knob showing the rows in view.</summary>
		private void ScrollBar(DrawList d, int first, int shown, int count)
		{
			RowPanel(d, ScrollX, CmdY, ScrollW, CmdH, false);
			float inner = CmdH - 2 * ScrollW * 0.6f, top = CmdY + ScrollW * 0.6f;
			float knob = count <= shown ? inner : inner * shown / count;
			float at = count <= shown ? top : top + (inner - knob) * first / Math.Max(1, count - shown);
			d.Rect(ScrollX + 3, at, ScrollW - 6, knob, new Color(170, 172, 196, 150));
			d.Text("▲", ScrollX + 6, CmdY + 1, new Color(200, 204, 230, 200), 10);
			d.Text("▼", ScrollX + 6, CmdY + CmdH - 13, new Color(200, 204, 230, 200), 10);
		}

		private void Glove(DrawList d, float x, float y, bool pressed = false)
		{
			if (Ff4Ui.Glove(d, x, y, pressed)) return;
			for (int i = 0; i < 6; i++) d.Rect(x - 14 + 2 * i, y - 6 + i, 2, 12 - 2 * i, Color.White);
		}

		private void Gauge(DrawList d, float x, float y, float fraction, bool alive)
		{
			if (Ff4Ui.Gauge(d, x, y, alive ? fraction : 0f, fraction >= 1f ? 2 : 1)) return;
			d.Rect(x - 3, y - 4, 60, 8, new Color(30, 30, 40, 255));
			if (alive) d.Rect(x - 3, y - 4, 60 * Math.Clamp(fraction, 0f, 1f), 8, fraction >= 1f ? Gold : new Color(210, 190, 90));
		}

		private void Shadowed(DrawList d, string text, float x, float y, Color color, int size = 16)
		{
			d.Text(text, x + 1, y + 1, new Color(0, 0, 0, 160), size);
			d.Text(text, x, y, color, size);
		}

		private void RightAligned(DrawList d, string text, float right, float y, Color color, int size = 16)
		{
			Shadowed(d, text, right - d.MeasureText(text, size), y, color, size);
		}

		private void Centred(DrawList d, string text, float centre, float y, Color color, int size = 16)
		{
			Shadowed(d, text, centre - d.MeasureText(text, size) / 2, y, color, size);
		}

		/// <summary>A key and what it does, as Steam's panel shows them: the letter on a dark blue key cap (23 square), the words after it.</summary>
		private void KeyHint(DrawList d, string key, string what, float x, float y)
		{
			d.Rect(x, y, 23, 23, new Color(14, 54, 96, 235));
			d.Rect(x, y, 23, 23, new Color(70, 110, 160, 255), false);
			d.Text(key, x + 11.5f - d.MeasureText(key, 13) / 2, y + 3, new Color(200, 216, 236), 13);
			Shadowed(d, what, x + 29, y + 5, Color.White, HintSize);
		}

		private int Accuracy(Fighter attacker, Fighter target) => Math.Clamp(attacker.HitChance + attacker.Agility - (target.Evade + target.Agility) + 20, 0, 100);

		// ---- the HUD: Data/hud/ff4_battle_hud.xml and its sheets, through LayoutScreen - what it binds to ----

		/// <summary>What the battle HUD's layout binds to, as the root "battle" (ff4_battle_hud.xml lists the paths).</summary>
		public sealed class HudData
		{
			public bool Panel, Commands, Party, Keys, Targets, Auto, Running, Skip;
			public List<TargetRow> Target = new List<TargetRow> { new TargetRow(), new TargetRow(), new TargetRow(), new TargetRow() };
			public CardData Card = new CardData();
			public GridData Grid = new GridData();
			public EquipData Equip = new EquipData();
			public string Message = "";
			public int MessageWidth = 800, MessageLeft = 560;   // the help window: FF4's 800, wider for a line our font draws wider, centred
			public List<CommandRow> Command = new List<CommandRow> { new CommandRow(), new CommandRow(), new CommandRow(), new CommandRow() };
			public ScrollData Scroll = new ScrollData();
			public List<MemberRow> Member = new List<MemberRow> { new MemberRow(), new MemberRow(), new MemberRow(), new MemberRow(), new MemberRow() };
			public ResultData Result = new ResultData();
		}

		public sealed class CommandRow
		{
			public bool Present, Lit, Disabled;
			public string Name = "", Sub = "";
			public bool HasSub => !string.IsNullOrEmpty(Sub);
			public bool Plain => string.IsNullOrEmpty(Sub);
		}
		public sealed class TargetRow { public bool Present, Lit; public string Name = "", Sub = ""; }
		public sealed class IconSlot { public int Cell = -1; }

		public sealed class CardData
		{
			public bool Shown;
			public string Name = "", Hp = "", WeakText = "", AbsorbText = "";
			public List<IconSlot> Weak = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(System.Linq.Enumerable.Range(0, 8), _ => new IconSlot()));
			public List<IconSlot> Absorb = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(System.Linq.Enumerable.Range(0, 8), _ => new IconSlot()));
		}
		public sealed class GridCell { public bool Present, Can, Lit; public string Name = "", Value = ""; public int Icon = -1; }

		public sealed class GridData
		{
			public bool Shown, ShowMp, Three, Two, Reequip, ReequipLit;
			public bool EquipStats, AttackDown, DefenseDown;   // Re-equip's numbers, a fall shown red
			public string AttackFrom = "", AttackArrow = "", AttackTo = "", DefenseFrom = "", DefenseArrow = "", DefenseTo = "";
			public int LineIcon = -1, LineIconX;   // an icon after line1 (a ninjutsu's element), at that x
			public string Title = "", Line1 = "", Line2 = "";
			public int TitleIcon = -1;
			public int Mp, MaxMp;
			public List<GridCell> Cell = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(System.Linq.Enumerable.Range(0, 12), _ => new GridCell()));
			public ScrollData Scroll = new ScrollData();
		}
		public sealed class ScrollData { public bool Shown; public float Top, Size = 100; }
		public sealed class MemberRow { public bool Present, Alive, Low, Acting, Picked, ShowMp; public string Name = "", Status = ""; public int Hp, MaxHp, Mp, Gauge, Fill = 1; }

		public sealed class ResultData
		{
			public bool Shown, ShowGil, ShowExp, Arrow;
			public int Panel, GilFound, NewTotal, Exp, LineCount, Height = 157;
			public List<string> Lines = new List<string>();
			public string Page = "gil", Title = "";
			public List<ResultRow> Row = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(System.Linq.Enumerable.Range(0, 8), _ => new ResultRow()));
		}

		public sealed class ResultRow { public string Label = "", Old = "", New = ""; public bool Up; }

		/// <summary>A page of the result (btl::BattleWin): the gil and experience, a member's level-up, the items.</summary>
		private sealed class ResultPage
		{
			public string Kind, Title = "";
			public List<(string Label, int Old, int New)> Rows = new List<(string, int, int)>();
			public List<string> Lines = new List<string>();
		}

		private readonly List<ResultPage> _pages = new List<ResultPage>();
		private int _page, _pageFrom;

		/// <summary>A message of babil_battle.msd, or the fallback.</summary>
		private string BattleText(int id, string fallback)
		{
			if (_battleTexts == null)
			{
				try { _battleTexts = TableFiles.ReadNames(GameArchive.Chain, "babil_battle.msd", new GameTables()) ?? new Dictionary<uint, string>(); }
				catch (Exception) { _battleTexts = new Dictionary<uint, string>(); }
			}
			return _battleTexts.TryGetValue((uint)id, out string text) && !string.IsNullOrEmpty(text) ? text : fallback;
		}

		private readonly HudData _hudData = new HudData();
		private LayoutScreen _hud;
		private string _measured;
		private bool _hudLoaded;

		private void EnsureHud()
		{
			if (_hudLoaded) return;
			_hudLoaded = true;
			Ff4Ui.RegisterLayoutCells();
			_hud = LayoutScreen.Load("ff4_battle_hud");
		}

		private (bool, object) HudRoot(string name) => string.Equals(name, "battle", StringComparison.OrdinalIgnoreCase) ? (true, _hudData) : (Game.Hud.TryGet(name, out object hud) ? (true, hud) : (false, null));

		/// <summary>The HUD's data as the fight stands this frame.</summary>
		private void FillHud()
		{
			HudData h = _hudData;
			bool choosing = _acting != null && _pick != Pick.None;
			h.Panel = (_phase == Phase.Fight || _phase == Phase.Intro) && !_closing && !_summonScene;
			h.Commands = choosing && _pick == Pick.Command;
			h.Party = _pick != Pick.Target && _pick != Pick.Spell && _pick != Pick.Item && _pick != Pick.Hand && _pick != Pick.EquipItem;
			h.Keys = _pick != Pick.Spell && _pick != Pick.Item && _pick != Pick.Hand && _pick != Pick.EquipItem;
			h.Auto = AutoBattle.On;
			h.Running = _runOn;
			h.Skip = _acting != null && _ready.Count > 1;
			h.Targets = choosing && _pick == Pick.Target;
			int shownFoe = 0;
			foreach (TargetRow t in h.Target) t.Present = false;
			bool all = h.Targets && (_abilityCmd == CmdKick || _abilityCmd == CmdCry || _abilityCmd == CmdAnalyze || _castItem > 0 && (Ff4Party.Tables.AbilityTargets(_castItem) & 0x4) != 0);
			if (all)
			{
				// Steam: one row, "Target All", over the card of the foe the hand is on.
				TargetRow t = h.Target[0];
				t.Present = true;
				t.Name = "Target All";
				t.Sub = "";
				t.Lit = true;
			}
			for (int i = 0; i < _foes.Count && shownFoe < h.Target.Count && h.Targets && !all; i++)
			{
				Fighter f = _foes[i];
				if (!f.Alive) continue;
				TargetRow t = h.Target[shownFoe++];
				t.Present = true;
				t.Name = f.Name;
				t.Sub = _casting != null ? _casting.Name : TargetLine(_acting, f);
				t.Lit = i == _cursor;
			}
			// Steam's card: the picked foe's name and HP, which it keeps hidden until the foe is studied.
			// Steam's card: the picked one's name, HP and elements - a monster's kept hidden until it is analyzed, a member's
			// HP with "None" (the target window on allies too).
			Fighter carded = h.Targets && _cursor >= 0 && _cursor < _foes.Count ? _foes[_cursor]
				: choosing && _pick == Pick.Ally && _cursor >= 0 && _cursor < _party.Count ? _party[_cursor] : null;
			FillCard(h.Card, carded);
			GridData g = h.Grid;
			bool equipping = _pick == Pick.Hand || _pick == Pick.EquipItem;
			g.Shown = choosing && (_pick == Pick.Spell || _pick == Pick.Item || equipping);
			List<int> list = _pick == Pick.Spell ? _spellChoices : equipping ? _equipChoices : _itemChoices;
			g.Reequip = g.Shown && _pick == Pick.Item && list.Count > 0 && list[0] == ReequipEntry && _listScroll == 0;
			g.ReequipLit = g.Reequip && _cursor <= 1;
			FillEquipHud(h);
			for (int k = 0; k < g.Cell.Count; k++)
			{
				GridCell c = g.Cell[k];
				int i = _listScroll + k;
				c.Present = g.Shown && i < list.Count && list[i] != ReequipEntry;
				if (!c.Present) { c.Lit = false; continue; }
				c.Lit = i == _cursor && _pick != Pick.Hand;
				if (equipping)
				{
					ItemDefinition gear = list[i] != 0 ? Ff4Party.Tables.Item(list[i]) : null;
					c.Name = gear?.Name ?? "";
					c.Icon = gear?.Icon ?? -1;
					c.Value = gear != null ? Ff4Party.Party.CountItem(list[i]).ToString() : "";
					c.Can = true;
					continue;
				}
				if (_pick == Pick.Spell)
				{
					SpellDefinition spell = Ff4Party.Tables.Spell(list[i]);
					c.Name = spell?.Name ?? "?";
					c.Icon = Ff4Party.Tables.AbilityIcon(list[i]);
					c.Value = "";
					c.Can = spell != null && spell.UsableInBattle && _acting.Mp >= spell.MpCost;
				}
				else
				{
					c.Name = Ff4Party.Tables.Item(list[i])?.Name ?? "?";
					c.Icon = Ff4Party.Tables.Item(list[i])?.Icon ?? -1;
					c.Value = Ff4Party.Party.CountItem(list[i]).ToString();
					c.Can = ItemUsable(list[i]);
				}
			}
			g.Three = g.Shown && ListColumns == 3;
			g.Two = g.Shown && ListColumns == 2;
			for (int k = ListColumns * ListRows; k < g.Cell.Count; k++) g.Cell[k].Present = false;
			g.ShowMp = g.Shown && _pick == Pick.Spell;
			if (g.ShowMp) { g.Mp = _acting.Mp; g.MaxMp = _acting.Member.MaxMp; }
			(g.Title, g.Line1, g.Line2) = equipping ? ("", "", "")
				: g.Shown && _cursor >= 0 && _cursor < list.Count ? ListDescription(list[_cursor]) : ("", "", "");
			g.EquipStats = false;
			if (equipping) EquipChange(g, _pick == Pick.EquipItem && _cursor < list.Count ? list[_cursor] : _acting.Member.Equipment[_hand]);
			// A ninjutsu's element as Steam shows it: the icon after "Element:", the line measured at the layout's size.
			g.LineIcon = -1;
			if (g.Shown && _pick == Pick.Spell && _cursor >= 0 && _cursor < list.Count && Ff4Party.Tables.Spell(list[_cursor]) is SpellDefinition ninjutsu
				&& ninjutsu.School == OpenFF.Data.MagicSchool.Ninjutsu && ElementCell(ninjutsu.Element) >= 0)
			{
				g.LineIcon = ElementCell(ninjutsu.Element);
				float sy = DrawList.ScreenHeight / 1080f, sx = DrawList.ScreenWidth / 1920f;
				g.LineIconX = 76 + (int)Math.Ceiling(TrueTypeText.Width(g.Line1, Math.Max(4, (int)Math.Round(31 * sy))) / sx) + 10;
			}
			g.TitleIcon = !g.Shown || equipping || _cursor < 0 || _cursor >= list.Count ? -1 : _pick == Pick.Spell ? Ff4Party.Tables.AbilityIcon(list[_cursor]) : Ff4Party.Tables.Item(list[_cursor])?.Icon ?? -1;
			int gridRows = (list.Count + ListColumns - 1) / ListColumns;
			g.Scroll.Shown = g.Shown && list.Count > ListColumns * ListRows;
			g.Scroll.Size = gridRows <= ListRows ? 100 : 100f * ListRows / gridRows;
			g.Scroll.Top = gridRows <= ListRows ? 0 : (100 - g.Scroll.Size) * (_listScroll / ListColumns) / Math.Max(1, gridRows - ListRows);
			if (_help != null && _helpUntil >= 0 && _clock >= _helpUntil) _help = null;
			h.Message = _help ?? "";
			if (h.Message != _measured)
			{
				// The line at the layout's 34px as the screen draws it, back in the layout's 1920 units, with a margin.
				_measured = h.Message;
				float sy = DrawList.ScreenHeight / 1080f, sx = DrawList.ScreenWidth / 1920f;
				float width = string.IsNullOrEmpty(h.Message) ? 0f : TrueTypeText.Width(h.Message, Math.Max(4, (int)Math.Round(34 * sy))) / sx + 70f;
				h.MessageWidth = Math.Min(1840, Math.Max(800, (int)Math.Ceiling(width)));
				h.MessageLeft = 960 - h.MessageWidth / 2;
			}
			List<int> commands = choosing ? Commands : null;
			int count = commands?.Count ?? 0;
			for (int row = 0; row < h.Command.Count; row++)
			{
				int i = _commandScroll + row;
				CommandRow c = h.Command[row];
				c.Present = i < count;
				c.Name = c.Present ? CommandName(commands[i]) : "";
				c.Lit = c.Present && _pick == Pick.Command && i == _cursor;
				c.Disabled = c.Present && !CommandUsable(_acting, commands[i]);
				c.Sub = c.Present ? CommandSub(_acting, commands[i]) : "";
			}
			h.Scroll.Shown = h.Commands || h.Targets;
			h.Scroll.Size = count <= CommandRows ? 100 : 100f * CommandRows / count;
			h.Scroll.Top = count <= CommandRows ? 0 : (100 - h.Scroll.Size) * _commandScroll / Math.Max(1, count - CommandRows);
			for (int i = 0; i < h.Member.Count; i++)
			{
				MemberRow m = h.Member[i];
				Fighter f = i < _party.Count ? _party[i] : null;
				m.Present = f != null;
				if (f == null) { m.Acting = m.Picked = false; continue; }
				m.Name = f.Name;
				m.Status = StatusShown(f);
				m.Hp = f.Hp;
				m.MaxHp = f.MaxHp;
				m.Mp = f.Mp;
				m.ShowMp = f.Member != null && f.Member.MaxMp > 0;
				m.Alive = f.Alive;
				m.Low = f.Alive && f.Hp * 4 <= f.MaxHp;
				m.Acting = f == _acting && choosing;
				m.Picked = _pick == Pick.Ally && i == _cursor;
				// Steam's gauge: grey while it fills, yellow while the member chooses, red once the command is decided -
				// filling again over the wait when it has one (drawATW's bar) - and empty after the action.
				float shown = f.Queued ? (f.Pending != null && f.AtwMax > 0 ? 1f - f.AtwLeft / (float)f.AtwMax : 1f) : f.Gauge;
				m.Gauge = f.Alive ? (int)Math.Round(Math.Clamp(shown, 0f, 1f) * 100) : 0;
				m.Fill = f.Queued ? 3 : f.Gauge >= 1f ? 2 : 1;
			}
			ResultData r = h.Result;
			r.Shown = _phase == Phase.Victory;   // off the stage too (a battle with no battle map)
			r.Panel = (int)Math.Round(Math.Clamp(_timer / 18f, 0f, 1f) * 100);
			r.GilFound = _gilWon;
			r.NewTotal = _gilShown;
			r.ShowGil = _timer >= WinFade + WinHold;
			r.Exp = _expWon;
			r.ShowExp = _timer >= WinFade + WinHold + WinExpAfter;
			r.Arrow = _timer >= WinFade + WinHold + WinArrowAfter;
			r.Lines.Clear();
			ResultPage page = _page < _pages.Count ? _pages[_page] : null;
			r.Page = page?.Kind ?? "gil";
			r.Title = page?.Title ?? "";
			if (page != null && page.Kind != "gil")
			{
				r.Arrow = _timer >= _pageFrom + WinArrowAfter;
				for (int i = 0; i < r.Row.Count; i++)
				{
					ResultRow row = r.Row[i];
					bool has = i < page.Rows.Count;
					row.Label = has ? page.Rows[i].Label : "";
					row.Old = has ? page.Rows[i].Old.ToString() : "";
					row.New = has ? page.Rows[i].New.ToString() : "";
					row.Up = has && page.Rows[i].New > page.Rows[i].Old;
				}
				r.Lines.AddRange(page.Lines);
			}
			r.LineCount = r.Lines.Count;
			// The window: the gil page's 157, a level-up's 400 (its eight rows) and a line more for each ability learnt.
			r.Height = r.Page == "levelup" ? 400 + 54 * r.LineCount : r.Page == "item" ? 70 + 54 * Math.Max(1, r.LineCount) : 157;
		}

		// btl::AcquiredGoldDrawer: the new total counts up from what the party had, each frame by the lowest place the
		// rest still has (14 gil: 1, 2, 3, 4, then 14), a key there ending it at once.
		private int _gilShown, _gilBefore;

		private void CountGil()
		{
			if (_phase != Phase.Victory || _timer < WinFade + WinHold) return;
			int target = _gilBefore + _gilWon;
			if (_gilShown >= target) return;
			if (Game.Input.Pressed(Pad.A) || Game.Input.Pressed(Pad.B) || Game.Input.PointerReleased) { _gilShown = target; return; }
			int rest = target - _gilShown, step = 1;
			while (rest % (step * 10) == 0 && step < 1000000000) step *= 10;
			_gilShown += step;
		}

		private void Draw()
		{
			DrawList d = Game.Draw;
			DrawPops(d);
			EnsureHud();
			if (_hud != null)
			{
				CountGil();
				FillHud();
				_hud.Draw(d, HudRoot);
			}
			if (_phase == Phase.Victory) { if (_hud == null) DrawResult(d); return; }
			if (_closing) return;   // the fade before the result, and the one after it
			bool choosing = _acting != null && _pick != Pick.None;

			// Bottom left: the commands while a member chooses (Steam opens them with the turn), the foes when a target is
			// picked, or the spell or item list.
			if (_pick == Pick.Target && _hud == null) Window(d, CmdX, CmdY, CmdW, CmdH);
			if (_pick == Pick.Target && _hud == null)
			{
				int row = 0;
				for (int i = 0; i < _foes.Count && row < 4; i++)
				{
					Fighter f = _foes[i];
					if (!f.Alive) continue;
					float y = CmdY + CmdRow * row;
					bool target = i == _cursor;
					Centred(d, f.Name, CmdX + CmdW / 2 + 8, y + 5, target ? Gold : Color.White, 15);
					Centred(d, _casting != null ? _casting.Name : "Accuracy: " + Accuracy(_acting, f) + "%", CmdX + CmdW / 2 + 8, y + 25, Color.White, 12);
					if (target) Glove(d, CmdX + 36, y + 14);
					if (row > 0) d.Line(CmdX + 4, y, CmdX + CmdW - 4, y, RowLine);
					row++;
				}
			}
			else if ((_pick == Pick.Spell || _pick == Pick.Item) && _hud != null) return;
			else if (_pick == Pick.Spell || _pick == Pick.Item)
			{
				// FF4's magic and item grid: one wide window over both, three columns of four rows.
				List<int> list = _pick == Pick.Spell ? _spellChoices : _itemChoices;
				float gx = CmdX, gy = CmdY, gw = PartyX + PartyW - CmdX, gh = CmdH;
				Window(d, gx, gy, gw, gh);
				float colW = (gw - 16) / ListColumns;
				for (int i = _listScroll; i < list.Count && i < _listScroll + ListColumns * ListRows; i++)
				{
					int k = i - _listScroll;
					float x = gx + 8 + colW * (k % ListColumns), y = gy + CmdRow * (k / ListColumns);
					bool picked = i == _cursor;
					if (_pick == Pick.Spell)
					{
						SpellDefinition spell = Ff4Party.Tables.Spell(list[i]);
						bool can = spell != null && _acting.Mp >= spell.MpCost;
						Shadowed(d, spell?.Name ?? "?", x + 40, y + 13, can ? Color.White : Dim, 16);
						RightAligned(d, (spell?.MpCost ?? 0).ToString(), x + colW - 10, y + 16, can ? Dim : new Color(255, 120, 110), 12);
					}
					else
					{
						ItemDefinition item = Ff4Party.Tables.Item(list[i]);
						Shadowed(d, item?.Name ?? "?", x + 40, y + 13, Color.White, 16);
						RightAligned(d, Ff4Party.Party.CountItem(list[i]).ToString(), x + colW - 10, y + 16, Dim, 12);
					}
					if (picked) Glove(d, x + 32, y + 14);
				}
				for (int r = 1; r < ListRows; r++) d.Line(gx + 6, gy + CmdRow * r, gx + gw - 6, gy + CmdRow * r, RowLine);
				if (_pick == Pick.Spell) RightAligned(d, "MP " + _acting.Mp + " / " + _acting.Member.MaxMp, gx + gw - 12, gy - 24, Color.White, 14);
				if (list.Count > ListColumns * ListRows)
				{
					int rows = (list.Count + ListColumns - 1) / ListColumns, top = _listScroll / ListColumns;
					float track = gh - 12, knob = Math.Max(12f, track * ListRows / rows);
					d.Rect(gx + gw - 6, gy + 6, 3, track, new Color(20, 22, 60, 160));
					d.Rect(gx + gw - 6, gy + 6 + (track - knob) * top / Math.Max(1, rows - ListRows), 3, knob, PanelEdge);
				}
				return;
			}
			else if (choosing && _hud == null)
			{
				List<int> commands = Commands;
				for (int row = 0; row < CommandRows && _commandScroll + row < commands.Count; row++)
				{
					int i = _commandScroll + row;
					float y = CmdY + CmdRow * row;
					bool lit = _pick == Pick.Command && i == _cursor;
					RowPanel(d, CmdX, y, CmdW, CmdRowH, lit);
					Centred(d, CommandName(commands[i]), CmdX + CmdW / 2, y + CmdRowH / 2 - 8, Color.White, TextSize);
					if (lit) Glove(d, CmdX + 30, y + CmdRowH / 2);
				}
				ScrollBar(d, _commandScroll, CommandRows, commands.Count);
			}

			// Bottom right: the party's rows - name, hit points, magic points, gauge - or the picked foe's card.
			if (_pick == Pick.Target && _cursor >= 0 && _cursor < _foes.Count && _hud == null) Window(d, PartyX, PartyY, PartyW, PartyH);
			if (_pick == Pick.Target && _cursor >= 0 && _cursor < _foes.Count)
			{
				if (_hud == null)
				{
				Fighter f = _foes[_cursor];
				Shadowed(d, f.Name, PartyX + 14, PartyY + 10, Gold, 18);
				Shadowed(d, "HP: " + f.Hp + " / " + f.MaxHp, PartyX + 14, PartyY + 38, Color.White, 16);
				Shadowed(d, "Weaknesses:", PartyX + 14, PartyY + 84, Color.White, 16);
				Shadowed(d, "Absorbs:", PartyX + 14, PartyY + 112, Color.White, 16);
				}
			}
			else if (_hud == null)
			{
				// Five rows always, as Steam's: the members' in their order, the rest empty.
				for (int i = 0; i < 5; i++)
				{
					float y = PartyY + PartyRow * i;
					Fighter f = i < _party.Count ? _party[i] : null;
					bool acting = f != null && f == _acting && choosing;
					RowPanel(d, PartyX, y, PartyW, PartyRowH, acting);
					if (f == null) continue;
					bool picked = _pick == Pick.Ally && i == _cursor;
					Color name = !f.Alive ? Dim : Color.White;
					float ty = y + PartyRowH / 2 - 8;
					Shadowed(d, f.Name, PartyX + 10, ty, name, TextSize);
					Color hp = !f.Alive ? Dim : f.Hp * 4 <= f.MaxHp ? new Color(255, 120, 110) : Color.White;
					RightAligned(d, f.Hp + " / " + f.MaxHp, PartyX + 187, ty, hp, TextSize);
					if (f.Member != null && f.Member.MaxMp > 0) RightAligned(d, f.Mp.ToString(), PartyX + 241, ty, Color.White, TextSize);
					Gauge(d, PartyX + 334, y + PartyRowH / 2, f.Gauge, f.Alive);
					if (picked) Glove(d, PartyX + 6, y + PartyRowH / 2);
				}
			}
			// The keys, over the party's rows (FF4 writes "C Auto battle  M Run away" there).
			if (_hud == null)
			{
				KeyHint(d, "C", "Auto battle", 504f, 309f);
				KeyHint(d, "M", "Run away", 618f, 309f);
			}

			// The picked foe wears the glove, as FF4's does.
			if (_pick == Pick.Target && _cursor >= 0 && _cursor < _foes.Count && _foes[_cursor].Npc != null)
			{
				Vector2? head = Game.Camera.WorldToScreen(_foes[_cursor].Npc.Position + new Vector3(0, 8, 0));
				if (head.HasValue) Glove(d, head.Value.X + 8, head.Value.Y - 8);
			}

			// What happened last: FF4's help window at the top, one line.
			if (_log.Count > 0 && _hud == null)
			{
				string line = _log[_log.Count - 1];
				float w = d.MeasureText(line, 15) + 40;
				Window(d, 400 - w / 2, 12, w, 34);
				Centred(d, line, 400, 20, Color.White, 15);
			}
		}

		/// <summary>FF4's result window: gil found and the new total on the left, the experience on the right; level-ups and drops below.</summary>
		// Steam's result window (1080p, its frames): one panel across the top from 150 to 1770 and 3 to 160, "Gil Found"
		// and "New Total" at 342 on lines whose letters top at 41 and 95, their sums at 619, "EXP" at 988 and its sum at
		// 1150; the page arrow (HelpWindow::setResultPageIcon) at 1650..1755, 115..150. The panel comes in with the
		// screens over about 18 frames.
		private const float ResultX = 62.5f, ResultY = 1.3f, ResultW = 675f, ResultH = 69.8f;

		private void DrawResultStage(DrawList d)
		{
			float a = Math.Clamp(_timer / 18f, 0f, 1f);
			Color Fade(Color c) => new Color(c.R, c.G, c.B, (byte)(c.A * a));
			// The panel's frame: a near-white line 3 pixels thick (240, 246, 248), a soft shadow 3 more outside it.
			const float t = 1.3f;
			Color edge = Fade(new Color(238, 244, 247, 255)), shade = Fade(new Color(20, 24, 30, 70));
			d.Rect(ResultX, ResultY, ResultW, ResultH, Fade(RowFill));
			d.Rect(ResultX - 2 * t, ResultY, t, ResultH + 2 * t, shade);
			d.Rect(ResultX + ResultW + t, ResultY, t, ResultH + 2 * t, shade);
			d.Rect(ResultX - t, ResultY + ResultH + t, ResultW + 2 * t, t, shade);
			d.Rect(ResultX - t, ResultY - t, ResultW + 2 * t, t, edge);
			d.Rect(ResultX - t, ResultY + ResultH, ResultW + 2 * t, t, edge);
			d.Rect(ResultX - t, ResultY, t, ResultH, edge);
			d.Rect(ResultX + ResultW, ResultY, t, ResultH, edge);
			Color text = Fade(Color.White);
			Shadowed(d, "Gil Found", 142.5f, 16.2f, text, TextSize);
			Shadowed(d, "New Total", 142.5f, 40.2f, text, TextSize);
			Shadowed(d, "EXP", 411.7f, 16.2f, text, TextSize);
			if (_timer >= WinFade + WinHold)
			{
				Shadowed(d, _gilWon.ToString(), 258f, 16.2f, Color.White, TextSize);
				Shadowed(d, Ff4Party.Party.Gil.ToString(), 258f, 40.2f, Color.White, TextSize);
			}
			if (_timer >= WinFade + WinHold + WinExpAfter) Shadowed(d, _expWon.ToString(), 479f, 16.2f, Color.White, TextSize);
			if (_timer >= WinFade + WinHold + WinArrowAfter)
			{
				// The arrow: a white triangle pointing down, 44 wide and 16 high.
				for (int r = 0; r < 16; r++)
				{
					float half = 22f * (1f - r / 16f);
					d.Line(709.3f - half, 51f + r, 709.3f + half, 51f + r, r < 2 ? new Color(150, 150, 160) : new Color(235, 235, 240));
				}
			}
			if (_resultLines.Count > 0 && _timer >= WinFade + WinHold + WinArrowAfter)
			{
				float ly = ResultY + ResultH + 12, lh = 14 + 24 * Math.Min(_resultLines.Count, 5);
				RowPanel(d, ResultX, ly, ResultW, lh, false);
				for (int i = 0; i < _resultLines.Count && i < 5; i++) Shadowed(d, _resultLines[i], 142.5f, ly + 8 + 24 * i, _resultLines[i].EndsWith("!") ? Gold : Color.White, TextSize);
			}
		}

		private void DrawResult(DrawList d)
		{
			if (_closing) { DrawResultStage(d); return; }
			float x = 100, y = 24, w = 600, h = 92;
			Window(d, x, y, w, h);
			d.Line(x + w / 2, y + 8, x + w / 2, y + h - 8, RowLine);
			Shadowed(d, "Gil Found", x + 24, y + 16, Color.White, 16);
			RightAligned(d, _gilWon.ToString(), x + w / 2 - 24, y + 16, Color.White, 16);
			Shadowed(d, "New Total", x + 24, y + 52, Color.White, 16);
			RightAligned(d, Ff4Party.Party.Gil.ToString(), x + w / 2 - 24, y + 52, Color.White, 16);
			Shadowed(d, "EXP", x + w / 2 + 24, y + 16, Color.White, 16);
			RightAligned(d, _expWon.ToString(), x + w - 24, y + 16, Color.White, 16);
			if (_resultLines.Count > 0)
			{
				float ly = y + h + 12, lh = 14 + 24 * Math.Min(_resultLines.Count, 5);
				Window(d, x, ly, w, lh);
				for (int i = 0; i < _resultLines.Count && i < 5; i++) Shadowed(d, _resultLines[i], x + 24, ly + 8 + 24 * i, _resultLines[i].EndsWith("!") ? Gold : Color.White, 16);
			}
		}
	}
}
