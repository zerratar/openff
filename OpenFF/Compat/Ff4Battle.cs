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
	internal sealed class Ff4Battle : GameService
	{
		private enum Phase { Idle, Intro, Fight, Victory, Defeat, Outro }
		private enum Command { Fight, Magic, Item, Run, Defend, SwapRows, Darkness, Other }
		private enum Pick { None, Command, Target, Item, Spell, Ally }
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
			CmdBlackMagic or CmdWhiteMagic or CmdSummon => Command.Magic,
			CmdDarkness => Command.Darkness,
			CmdSwapRows => Command.SwapRows,
			_ => Command.Other,
		};

		private List<int> Commands => _acting?.Commands ?? new List<int> { CmdFight };
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
			public bool Alive => Hp > 0;
			public Vector3 Home;
			public bool Acted;   // a one-shot motion is playing; Idle() restarts the loop when it ends
			public List<int> Commands = new List<int>();   // the member's FF4 command list (CommandList)
			public bool Defending;                         // Defend: physical damage halved until their next turn
			public int Poise = -1, SwingA = -1, SwingB = -1, Swings;
			public int HitEffect = -1;                     // the weapon's hit effect (WEAPON_EFFECT), its pack e<nnn>   // the weapon's poise and two attack motions (b_poise, b_w), alternated
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
			bool started = Start(ids, inScene, battleMap);
			if (started) Log.Write(LogChannel.General, "battle: encounter group " + partyId);
			return started;
		}

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

			_party.Clear(); _foes.Clear(); _log.Clear(); _dropsWon.Clear();
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
					Gauge = StartGauge(),
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
						ally.HitEffect = HitEffectOf(WeaponSystem(ally.Member, tables));
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
					for (int k = 1; k <= 5; k++)
					{
						float part = k / 5f;
						After(k, () => { entering.Npc?.Teleport(from + (to - from) * part); Face(entering, entering.Facing + 180f); });
					}
					After(6, () => { Face(entering, entering.Facing); Play(entering, _heroMotionIdle, true, 0); entering.Acted = false; });
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
				Monster info = Game.Monsters.Find(id);
				Npc npc = Game.Npcs.SpawnModel(info?.Model ?? ("m" + m.Family.ToString("000") + "_00"), at, 0f);
				if (npc == null) { Say("no model for " + m.Name); continue; }
				try { npc.BindMotions(info?.MotionSet ?? ("b_m" + m.Family.ToString("000"))); npc.PlayMotion(101, true); } catch (Exception) { }
				if (facing.HasValue && npc is LegacyNpc exactNpc) exactNpc.FaceExactly(facing.Value);
				else if (facing.HasValue) npc.LookAt(at + Ff4BattleStage.Facing(facing.Value) * 10f);
				else npc.LookAt(hero);
				npc.Solid = false;
				_foes.Add(new Fighter
				{
					Name = m.Name ?? ("monster " + id), IsMonster = true, Monster = m, Npc = npc, Home = at,
					Hp = Math.Max(1, m.MaxHp), MaxHp = Math.Max(1, m.MaxHp),
					Attack = Math.Max(1, m.Attack), Defence = Math.Max(0, m.Defence), Agility = Math.Max(1, m.Stats.Agility),
					Level = Math.Max(1, m.Level), Intellect = m.Stats.Intellect, Spirit = m.Stats.Spirit, Vitality = m.Stats.Vitality, MagicDefence = Math.Max(0, m.MagicDefence),
					Strength = m.Stats.Strength, HitChance = m.Hit > 0 ? m.Hit : 90, Evade = Math.Max(0, m.Evade),
					Gauge = StartGauge(),
					AtbRate = m.AtbRateMin + (float)_random.NextDouble() * Math.Max(0f, m.AtbRateMax - m.AtbRateMin),
				});
				n++;
			}
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
				Ff4EventCamera.MoveTo((int)(cp.X * 4096), (int)(cp.Y * 4096), (int)(cp.Z * 4096), Ff4BattleStage.OpeningFrames, false);
				Ff4EventCamera.LookAt((int)(ct.X * 4096), (int)(ct.Y * 4096), (int)(ct.Z * 4096), Ff4BattleStage.OpeningFrames);
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
		private static void PlayEffect(int id, Vector3 at)
		{
			if (id < 0) return;
			try
			{
				GlobalScope.eff.CEffectMng effects = GlobalScope.eff.CEffectMng.instance();
				int made = effects.create(id, 1);
				if (made < 0) { Log.First(LogChannel.File, "battle-effect-" + id, 2, () => "battle: effect " + id + " could not be made"); return; }
				effects.enableBoxCulling(made, false);
				effects.setPosition(made, new GlobalScope.VecFx32((int)Math.Round(at.X * 4096), (int)Math.Round(at.Y * 4096), (int)Math.Round(at.Z * 4096)));
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
					if (_timer > 30) { _phase = Phase.Fight; _timer = 0; }
					break;
				case Phase.Fight:
					Fight();
					break;
				case Phase.Victory:
					// The result window (DrawResult) stands until A, B or a tap.
					if (_timer > 20 && (Game.Input.Pressed(Pad.A) || Game.Input.Pressed(Pad.B) || Game.Input.PointerReleased)) { _phase = Phase.Outro; _timer = 0; }
					break;
				case Phase.Defeat:
					// The message window holds the word; the legacy window's own A press is gated
					// off while the battle holds the input, so the press is read here.
					if (Game.Dialogue.IsOpen && _timer > 20 && (Game.Input.Pressed(Pad.A) || Game.Input.Pressed(Pad.B) || Game.Input.PointerReleased)) Game.Dialogue.Close();
					if (!Game.Dialogue.IsOpen && _timer > 20) { _phase = Phase.Outro; _timer = 0; }
					break;
				case Phase.Outro:
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
				if (done) { Play(f, _heroMotionIdle, true, 4); f.Acted = false; }
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
			if (_cues.Count > 0) return;   // an action is playing out
			if (Ff4BattleStage.Active) Log.Sample(LogChannel.File, "battle-camera", 120, () => "battle: camera at " + Game.Camera.Position + " hero at " + Game.Hero.Position);
			// FF4's active battle, as Steam plays it: the gauges go on filling while a member chooses, and a monster whose
			// gauge fills acts then and there.
			foreach (Fighter f in _party) if (f.Alive) f.Gauge = Math.Min(1f, f.Gauge + GaugeStep(f));
			foreach (Fighter f in _foes) if (f.Alive) f.Gauge = Math.Min(1f, f.Gauge + GaugeStep(f));
			foreach (Fighter f in _foes)
			{
				if (f.Alive && f.Gauge >= 1f) { MonsterActs(f); return; }
			}
			if (_acting == null)
			{
				foreach (Fighter f in _party)
				{
					if (f.Alive && f.Gauge >= 1f) { _acting = f; f.Defending = false; _pick = Pick.Command; _cursor = 0; _commandScroll = 0; Log.Write(LogChannel.File, "battle: " + f.Name + " may act (step " + LegacyStep.Count + ", agility " + f.Agility + ")"); return; }
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
				bool runNow = input.KeyPressed("M");   // Steam's "M Run away"
				if (input.Pressed(Pad.A) || runNow)
				{
					_command = runNow ? Command.Run : CommandOf(commands[_cursor]);
					if (_command == Command.Defend) { Defend(_acting); return; }
					if (_command == Command.SwapRows) { SwapRows(_acting); return; }
					if (_command == Command.Darkness) { Darkness(_acting); return; }
					if (_command == Command.Other) { Say(CommandName(commands[_cursor]) + " is not in yet."); return; }
					if (_command == Command.Fight) { _pick = Pick.Target; _cursor = FirstAliveFoe(); }
					else if (_command == Command.Magic)
					{
						_spellChoices.Clear();
						GameTables tables = Ff4Party.Tables;
						foreach (int id in _acting.Member.Spells)
						{
							SpellDefinition spell = tables.Spell(id);
							if (spell != null && spell.UsableInBattle) _spellChoices.Add(id);
						}
						foreach (int id in _acting.Member.Abilities)
						{
							SpellDefinition spell = id >= 1500 ? tables.Spell(id) : null;
							if (spell != null && spell.UsableInBattle && !_spellChoices.Contains(id)) _spellChoices.Add(id);
						}
						if (_spellChoices.Count == 0) { Say(_acting.Name + " knows no magic."); return; }
						_pick = Pick.Spell; _cursor = 0; _listScroll = 0;
					}
					else if (_command == Command.Item)
					{
						_itemChoices.Clear();
						foreach (OpenFF.Data.ItemStack s in Ff4Party.Party.Inventory)
						{
							ItemDefinition item = Ff4Party.Tables.Item(s.ItemId);
							if (item != null && item.Kind == ItemKind.Consumable && ItemEffect(item) != null) _itemChoices.Add(s.ItemId);
						}
						if (_itemChoices.Count == 0) { Say("Nothing to use."); return; }
						_pick = Pick.Item; _cursor = 0; _listScroll = 0;
					}
					else Run();
				}
				return;
			}
			if (_pick == Pick.Target)
			{
				if (input.Pressed(Pad.Left) || input.Pressed(Pad.Up)) _cursor = NextAliveFoe(_cursor, -1);
				if (input.Pressed(Pad.Right) || input.Pressed(Pad.Down)) _cursor = NextAliveFoe(_cursor, 1);
				if (input.Pressed(Pad.B)) { _pick = _casting != null ? Pick.Spell : Pick.Command; _cursor = 0; _casting = null; return; }
				if (input.Pressed(Pad.A) && _cursor >= 0)
				{
					if (_casting != null) Cast(_acting, _casting, _casting.HitsAll ? _foes.FindAll(f => f.Alive) : new List<Fighter> { _foes[_cursor] });
					else MemberAttacks(_acting, _foes[_cursor]);
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
					if (spell == null) return;
					if (_acting.Mp < spell.MpCost) { Say("Not enough MP for " + spell.Name + "."); return; }
					_casting = spell;
					if (Helps(spell)) { _pick = Pick.Ally; _cursor = _party.IndexOf(_acting); }
					else { _pick = Pick.Target; _cursor = FirstAliveFoe(); }
				}
				return;
			}
			if (_pick == Pick.Ally)
			{
				if (input.Pressed(Pad.Up) || input.Pressed(Pad.Left)) _cursor = (_cursor + _party.Count - 1) % _party.Count;
				if (input.Pressed(Pad.Down) || input.Pressed(Pad.Right)) _cursor = (_cursor + 1) % _party.Count;
				if (input.Pressed(Pad.B)) { _pick = _casting != null ? Pick.Spell : Pick.Item; _casting = null; _cursor = 0; return; }
				if (input.Pressed(Pad.A))
				{
					if (_casting != null) Cast(_acting, _casting, _casting.HitsAll ? new List<Fighter>(_party) : new List<Fighter> { _party[_cursor] });
					else UseItem(_acting, _usingItem, _party[_cursor]);
				}
				return;
			}
			if (_pick == Pick.Item)
			{
				GridMove(input, _itemChoices.Count);
				if (input.Pressed(Pad.B)) { _pick = Pick.Command; _cursor = Math.Max(0, Commands.FindIndex(id => id == CmdItems)); return; }
				if (input.Pressed(Pad.A)) { _usingItem = _itemChoices[_cursor]; _pick = Pick.Ally; _cursor = _party.IndexOf(_acting); }
			}
		}

		// FF4 lists spells and items in a grid of three columns (btl::BtlMagicMenu::BMTEXT_POS: x 24,
		// 98, 172 by rows of 10 DS pixels): Left and Right step along it, Up and Down move a row,
		// and the view scrolls by rows.
		private const int ListColumns = 3, ListRows = 4;

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
			return spell.Heals || spell.Revives || (spell.Power == 0 && spell.Inflicts == 0 && (spell.Grants != 0 || spell.Grants2 != 0) && spell.School == OpenFF.Data.MagicSchool.White);
		}

		// ---- magic, as btl::NewMagicFormula computes it ----

		private void Cast(Fighter caster, SpellDefinition spell, List<Fighter> targets)
		{
			_casting = null;
			caster.Member.Mp = Math.Max(0, caster.Member.Mp - spell.MpCost);
			Play(caster, _heroMotionAttack);
			Game.Audio.PlaySe(0, 5);
			string name = spell.Name ?? ("spell " + spell.Id);
			if (spell.Heals)
			{
				foreach (Fighter t in targets)
				{
					if (!t.Alive) continue;
					int value = HealingValue(caster, t, spell, targets.Count);
					int before = t.Hp;
					t.Hp = Math.Min(t.MaxHp, t.Hp + value);
					if (t.Member != null) t.Member.Hp = t.Hp;
					Pop(Where(t) + new Vector3(0, 12f, 0), t.Hp - before, true);
					Say(caster.Name + " casts " + name + ": " + t.Name + " +" + (t.Hp - before) + ".");
				}
			}
			else if (spell.Revives)
			{
				foreach (Fighter t in targets)
				{
					if (t.Alive) { Say(name + " does nothing for " + t.Name + "."); continue; }
					t.Hp = spell.Id == 4007 ? t.MaxHp : Math.Max(1, t.MaxHp / 4);
					if (t.Member != null) t.Member.Hp = t.Hp;
					Say(caster.Name + " casts " + name + ": " + t.Name + " rises.");
				}
			}
			else if (spell.Power > 0)
			{
				foreach (Fighter t in targets)
				{
					if (!t.Alive) continue;
					int damage = AttackMagicDamage(caster, t, spell, targets.Count);
					t.Hp = Math.Max(0, t.Hp - damage);
					if (t.Member != null) t.Member.Hp = t.Hp;
					Pop(Where(t) + new Vector3(0, 12f, 0), damage);
					Say(caster.Name + " casts " + name + ": " + t.Name + " takes " + damage + ".");
					if (!t.Alive) Fell(t);
				}
			}
			else
			{
				// A status spell: the hit rate decides; only death is carried out, the rest is told.
				foreach (Fighter t in targets)
				{
					if (!t.Alive) continue;
					bool hit = _random.Next(100) < spell.HitRate;
					if (hit && (spell.Inflicts & 0x200) != 0 && t.IsMonster) { t.Hp = 0; Say(caster.Name + " casts " + name + ": " + t.Name + " is slain."); Fell(t); }
					else Say(caster.Name + " casts " + name + " on " + t.Name + (hit ? "." : ": it misses."));
				}
			}
			caster.Gauge = 0f;
			_acting = null;
			_pick = Pick.None;
			if (_foes.FindAll(f => f.Alive).Count == 0) Win();
			else if (_party.FindAll(f => f.Alive).Count == 0) Lose();
		}

		/// <summary>A line for the fight's log on screen, and for the log file.</summary>
		private void Say(string line)
		{
			_log.Add(line);
			Log.Write(LogChannel.File, "battle: " + line);
		}

		private Vector3 Where(Fighter f) => f.Npc != null ? f.Npc.Position : Game.Hero.Position;

		// ---- the numbers and words that pop over a fighter, in FF4's battle digits ----

		private sealed class PopUp
		{
			public Vector3 At;
			public int Value;
			public int Word = -1;
			public bool Heal;
			public int Frame;
			public const int Frames = 70;
		}

		private readonly List<PopUp> _pops = new List<PopUp>();

		/// <summary>A number over a spot in the world: white for damage, green for a heal; it rises, bounces once and fades.</summary>
		private void Pop(Vector3 at, int value, bool heal = false)
		{
			_pops.Add(new PopUp { At = at, Value = Math.Abs(value), Heal = heal });
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
				if (++p.Frame > PopUp.Frames) { _pops.RemoveAt(i); continue; }
				Vector2? screen = Game.Camera.WorldToScreen(p.At);
				if (!screen.HasValue) continue;
				// Up fast, a small bounce, then a hold while it fades.
				float t = p.Frame;
				float rise = t < 12 ? -3.2f * t : t < 20 ? -38f + 2.2f * (t - 12) : t < 26 ? -20f - 1.2f * (t - 20) : -27f;
				byte alpha = (byte)(t > 55 ? Math.Max(0, 255 - (t - 55) * 17) : 255);
				Color tint = p.Heal ? new Color(Ff4Ui.HealTint.R, Ff4Ui.HealTint.G, Ff4Ui.HealTint.B, alpha) : new Color(255, 255, 255, alpha);
				float x = screen.Value.X, y = screen.Value.Y + rise;
				// Each number its own thing on the screen: two numbers share their digits' pictures, and a new one is
				// drawn ahead of the rest, so between two steps each rises from its own last place, not another's.
				d.Group(p);
				bool drawn = p.Word >= 0 ? Ff4Ui.Word(d, x, y, p.Word, tint) : Ff4Ui.Number(d, x, y, p.Value, tint);
				if (!drawn)
				{
					string text = p.Word >= 0 ? (p.Word == Ff4Ui.WordMiss ? "Miss" : "!") : p.Value.ToString();
					d.Text(text, x - d.MeasureText(text, 22) / 2 + 1, y - 10, new Color(0, 0, 0, alpha), 22);
					d.Text(text, x - d.MeasureText(text, 22) / 2, y - 11, tint, 22);
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

		/// <summary>btl::BATTLE_SPEED_RATE[the battle speed setting]: 1.5, 1.25, 1, 0.75, 0.5, 0.25 - the middle one here.</summary>
		private const float BattleSpeedRate = 1f;

		/// <summary>A normal encounter's start (BattlePlayer / BattleMonster::initializeATG): 45 to 65 of the gauge's 100, at random.</summary>
		private float StartGauge() => (45 + _random.Next(21)) / 100f;

		/// <summary>A frame's fill: (1 + agility / 32) at the battle's speed, times a monster's ATB rate - of the gauge's 100.</summary>
		private static float GaugeStep(Fighter f) => BattleSpeedRate * (1f + Math.Max(0, f.Agility) / 32f) * f.AtbRate / 100f;

		private void Face(Fighter f, float degrees)
		{
			if (f.Npc is LegacyNpc exact) exact.FaceExactly(degrees);
			else f.Npc?.LookAt(f.Npc.Position + Ff4BattleStage.Facing(degrees) * 10f);
		}

		private void Play(Fighter f, int motion, bool loop = false, int blend = 3)
		{
			if (!loop) f.Acted = true;
			try
			{
				if (f.Npc != null) f.Npc.PlayMotion(motion, loop, blend);
				else Game.Hero.PlayMotion(motion, loop, blend);
			}
			catch (Exception) { }
		}

		/// <summary>NewMagicFormula::calcAttackMagicDamage: power x level x stat over the target's will, level and magic defence, times 1.0..1.3.</summary>
		private int AttackMagicDamage(Fighter caster, Fighter target, SpellDefinition spell, int targetCount)
		{
			int stat = spell.School == OpenFF.Data.MagicSchool.White ? caster.Spirit : caster.Intellect;
			long numerator = (long)spell.Power * Math.Max(1, caster.Level) * Math.Max(1, stat);
			int denominator = Math.Max(1, target.Spirit + target.Level + target.MagicDefence);
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

		private void Fell(Fighter foe)
		{
			if (!foe.IsMonster) { Say(foe.Name + " falls."); return; }
			Say(foe.Name + " is defeated.");
			if (foe.Npc != null) { foe.Npc.Alpha = 8; foe.Npc.Hidden = true; }
			_expWon += foe.Monster.Experience;
			_gilWon += foe.Monster.Gil;
			foreach (DropChance drop in foe.Monster.Drops)
			{
				if (_random.Next(4096) < drop.Chance) { _dropsWon.Add(drop.ItemId); break; }
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
			int rate = Math.Clamp(attacker.HitChance + attacker.Agility - (target.Evade + target.Agility) + 20, 0, 100);
			return _random.Next(100) < rate;
		}

		/// <summary>NewAttackFormula::calcDamageValueForBabil, its core: attack x level x strength over defence + level + vitality, times 1.0..1.3, times 1.2 onto a monster and 0.7 onto a member.</summary>
		private int Damage(Fighter attacker, Fighter target)
		{
			long numerator = (long)Math.Max(1, attacker.Attack) * Math.Max(1, attacker.Level) * Math.Max(1, attacker.Strength);
			int denominator = Math.Max(1, target.Defence + target.Level + target.Vitality);
			double value = numerator / (double)denominator * (1.0 + _random.Next(301) / 1000.0);
			value *= target.IsMonster ? 1.2 : 0.7;
			return Math.Max(1, (int)value);
		}

		private void MemberAttacks(Fighter member, Fighter foe)
		{
			if (!foe.Alive) { _pick = Pick.Target; _cursor = FirstAliveFoe(); return; }
			if (Ff4BattleStage.Active && member.Poise > 0 && member.SwingA > 0)
			{
				// Steam's attack, frame by frame: the weapon's poise (1047) for 15 frames, the swing (96, then 95, by
				// turns) - the blow lands 8 frames in, its number pops when the swing ends - and back to the stance.
				int swing = member.Swings++ % 2 == 0 ? member.SwingA : (member.SwingB > 0 ? member.SwingB : member.SwingA);
				EndTurn(member);
				Play(member, member.Poise, false, 3);
				member.Acted = false;
				int damage = 0;
				bool hit = false;
				After(15, () => { Play(member, swing, false, 3); member.Acted = false; });
				After(23, () =>
				{
					hit = Hits(member, foe);
					if (!hit) return;
					PlayEffect(member.HitEffect, foe.Npc.Position + new Vector3(0, 8f, 0));
					damage = Damage(member, foe);
					foe.Hp = Math.Max(0, foe.Hp - damage);
					Game.Audio.PlaySe(0, 3);
				});
				After(31, () =>
				{
					Play(member, _heroMotionIdle, true, 3);
					member.Acted = false;
					if (!hit)
					{
						PopWord(Where(foe) + new Vector3(0, 12f, 0), Ff4Ui.WordMiss);
						Say(member.Name + " misses " + foe.Name + ".");
						return;
					}
					Pop(foe.Npc.Position + new Vector3(0, 12f, 0), damage);
					Say(member.Name + " hits " + foe.Name + " for " + damage + ".");
					if (!foe.Alive) Fell(foe);
					if (_foes.FindAll(f => f.Alive).Count == 0) After(30, VictoryPose);
				});
				return;
			}
			Play(member, _heroMotionAttack);
			if (!Hits(member, foe))
			{
				PopWord(Where(foe) + new Vector3(0, 12f, 0), Ff4Ui.WordMiss);
				Say(member.Name + " misses " + foe.Name + ".");
			}
			else
			{
				int damage = Damage(member, foe);
				foe.Hp = Math.Max(0, foe.Hp - damage);
				Pop(foe.Npc.Position + new Vector3(0, 12f, 0), damage);
				Game.Audio.PlaySe(0, 3);
				Say(member.Name + " hits " + foe.Name + " for " + damage + ".");
				if (!foe.Alive) Fell(foe);
			}
			member.Gauge = 0f;
			_acting = null;
			_pick = Pick.None;
			if (_foes.FindAll(f => f.Alive).Count == 0) Win();
		}

		private void MonsterActs(Fighter foe)
		{
			List<Fighter> alive = _party.FindAll(f => f.Alive);
			if (alive.Count == 0) return;
			Fighter target = alive[_random.Next(alive.Count)];
			try { foe.Npc.PlayMotion(201, false, 3); foe.Acted = true; } catch (Exception) { }
			foe.Gauge = 0f;
			// On the stage the blow lands as Steam's does, eight frames into the monster's attack (201).
			if (Ff4BattleStage.Active) After(8, () => MonsterBlow(foe, target));
			else MonsterBlow(foe, target);
		}

		private void MonsterBlow(Fighter foe, Fighter target)
		{
			if (!target.Alive) return;
			if (!Hits(foe, target))
			{
				PopWord(Where(target) + new Vector3(0, 12f, 0), Ff4Ui.WordMiss);
				Say(foe.Name + " misses " + target.Name + ".");
				foe.Gauge = 0f;
				return;
			}
			int damage = Damage(foe, target);
			if (target.Defending) damage = Math.Max(1, damage / 2);   // Defend halves a blow
			target.Hp = Math.Max(0, target.Hp - damage);
			target.Member.Hp = target.Hp;
			Pop(Where(target) + new Vector3(0, 12f, 0), damage);
			Game.Screen.Flash(new Color(255, 60, 40), 6, 2);
			// Steam's flinch: 1117 for a frame, then the stance again.
			Play(target, 1117, false, 0);
			Fighter hurt = target;
			After(1, () => { if (hurt.Alive) { Play(hurt, _heroMotionIdle, true, 3); hurt.Acted = false; } });
			Say(foe.Name + " hits " + target.Name + " for " + damage + ".");
			foe.Gauge = 0f;
			if (!target.Alive) Say(target.Name + " falls.");
			if (_party.FindAll(f => f.Alive).Count == 0) Lose();
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
			if (effect == null) { _acting = null; _pick = Pick.None; return; }
			bool revive = effect.Id == 17;
			if (revive == target.Alive)
			{
				Say(item.Name + " does nothing for " + target.Name + ".");
				return;
			}
			if (Ff4Party.Party.RemoveItem(itemId, 1))
			{
				int before = target.Hp;
				if (revive) target.Hp = Math.Max(1, target.MaxHp / 4);
				else if (effect.Hp > 0) target.Hp = Math.Min(target.MaxHp, target.Hp + effect.Hp);
				target.Member.Hp = target.Hp;
				if (effect.Mp > 0) target.Member.Mp = Math.Min(target.Member.MaxMp, target.Member.Mp + effect.Mp);
				if (target.Hp != before) Pop(Where(target) + new Vector3(0, 12f, 0), target.Hp - before, true);
				Say(member.Name + " uses " + item.Name + ": " + target.Name + (revive ? " rises." : (effect.Hp > 0 ? " +" + (target.Hp - before) + " HP" : "") + (effect.Mp > 0 ? " +" + effect.Mp + " MP" : "") + "."));
			}
			member.Gauge = 0f;
			_acting = null;
			_pick = Pick.None;
		}

		private const int CommandRows = 4;

		/// <summary>Defend: the member braces until their next turn, physical blows halved (FF4's Defend).</summary>
		private void Defend(Fighter member)
		{
			member.Defending = true;
			Say(member.Name + " defends.");
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
		private void Darkness(Fighter member)
		{
			member.Hp = Math.Max(1, member.Hp - member.MaxHp / 8);
			member.Member.Hp = member.Hp;
			Play(member, _heroMotionAttack);
			foreach (Fighter foe in _foes.FindAll(f => f.Alive))
			{
				int damage = Damage(member, foe);
				foe.Hp = Math.Max(0, foe.Hp - damage);
				Pop(foe.Npc.Position + new Vector3(0, 12f, 0), damage);
				if (!foe.Alive) Fell(foe);
			}
			Say(member.Name + " unleashes Darkness.");
			EndTurn(member);
			if (_foes.FindAll(f => f.Alive).Count == 0) Win();
		}

		private void EndTurn(Fighter member)
		{
			member.Gauge = 0f;
			_acting = null;
			_pick = Pick.None;
		}

		private void Run()
		{
			if (_random.Next(100) < 60)
			{
				Say("The party runs.");
				_phase = Phase.Outro;
			}
			else
			{
				Say("Could not run!");
				_acting.Gauge = 0f;
				_acting = null;
				_pick = Pick.None;
			}
		}

		// The victory as Steam stages it (btl::BattleWin::layout, pl::layoutCharacterScene): the members on the layout
		// for the party's size, turned as it says, in the win pose (2007, then 2009 after 45 frames), the camera easing
		// to the layout's over 56 frames from where Steam's starts (fitted off its frames for one member).
		private int _victoryCameraFrame = -1;
		private float[] _victoryFrom, _victoryFromTarget, _victoryTo, _victoryToTarget;

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
			_resultLines.Clear();
			List<string> lines = _resultLines;
			party.Gil += _gilWon;
			foreach (Fighter f in alive)
			{
				int before = f.Member.Level;
				f.Member.Experience += _expWon / Math.Max(1, alive.Count);
				int level = Ff4Party.Tables.LevelForExperience(f.Member.Experience);
				if (level > before)
				{
					f.Member.SetLevel(level, false);
					lines.Add(f.Name + " reaches level " + level + "!");
					foreach (int id in f.Member.Learn()) lines.Add(f.Name + " learns " + (Ff4Party.Tables.Spell(id)?.Name ?? ("spell " + id)) + "!");
				}
			}
			foreach (int id in _dropsWon)
			{
				party.AddItem(id, 1);
				lines.Add("Found " + (Ff4Party.Tables.Item(id)?.Name ?? ("item " + id)) + ".");
			}
			if (!Ff4BattleStage.Active) foreach (Fighter f in _party) { if (f.Alive) Play(f, 2007); }   // on the stage VictoryPose has posed them
			Log.Write(LogChannel.General, "battle: won - " + _expWon + " exp, " + _gilWon + " gil. " + string.Join(" ", lines));
		}

		private void Lose()
		{
			_phase = Phase.Defeat;
			_timer = 0;
			foreach (Fighter f in _party) { f.Hp = Math.Max(1, f.Hp); f.Member.Hp = f.Hp; }
			Game.Dialogue.Say("The party was defeated...\n(Everyone is left with 1 HP for now.)");
			Log.Write(LogChannel.General, "battle: lost");
		}

		// ---- random encounters: the map's encounter chain, rolled per unit walked ----

		private Vector3 _lastStep;
		private string _noTableLogged;
		private float _walked;
		private int _sinceBattle;

		private void Encounters()
		{
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
				if (party > 0 && StartParty(party, false, table.BattleMap)) _sinceBattle = 0;
			}
		}

		private void End()
		{
			_sinceBattle = 0;
			_lastStep = Game.Hero.Present ? Game.Hero.Position : Vector3.Zero;
			Action after = AfterBattle;
			AfterBattle = null;
			foreach (Fighter f in _foes)
			{
				try { f.Npc?.Remove(); } catch (Exception) { }
			}
			foreach (Fighter f in _party)
			{
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
				Ff4BattleStage.Leave(jumpBack: after == null);
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

		private void Draw()
		{
			DrawList d = Game.Draw;
			DrawPops(d);
			if (_phase == Phase.Defeat) return;
			if (_phase == Phase.Victory) { DrawResult(d); return; }
			bool choosing = _acting != null && _pick != Pick.None;

			// Bottom left: the commands while a member chooses (Steam opens them with the turn), the foes when a target is
			// picked, or the spell or item list.
			if (_pick == Pick.Target) Window(d, CmdX, CmdY, CmdW, CmdH);
			if (_pick == Pick.Target)
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
			else if (choosing)
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
			if (_pick == Pick.Target && _cursor >= 0 && _cursor < _foes.Count) Window(d, PartyX, PartyY, PartyW, PartyH);
			if (_pick == Pick.Target && _cursor >= 0 && _cursor < _foes.Count)
			{
				Fighter f = _foes[_cursor];
				Shadowed(d, f.Name, PartyX + 14, PartyY + 10, Gold, 18);
				Shadowed(d, "HP: " + f.Hp + " / " + f.MaxHp, PartyX + 14, PartyY + 38, Color.White, 16);
				Shadowed(d, "Weaknesses:", PartyX + 14, PartyY + 84, Color.White, 16);
				Shadowed(d, "Absorbs:", PartyX + 14, PartyY + 112, Color.White, 16);
			}
			else
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
			KeyHint(d, "C", "Auto battle", 504f, 309f);
			KeyHint(d, "M", "Run away", 618f, 309f);

			// The picked foe wears the glove, as FF4's does.
			if (_pick == Pick.Target && _cursor >= 0 && _cursor < _foes.Count && _foes[_cursor].Npc != null)
			{
				Vector2? head = Game.Camera.WorldToScreen(_foes[_cursor].Npc.Position + new Vector3(0, 8, 0));
				if (head.HasValue) Glove(d, head.Value.X + 8, head.Value.Y - 8);
			}

			// What happened last: FF4's help window at the top, one line.
			if (_log.Count > 0)
			{
				string line = _log[_log.Count - 1];
				float w = d.MeasureText(line, 15) + 40;
				Window(d, 400 - w / 2, 12, w, 34);
				Centred(d, line, 400, 20, Color.White, 15);
			}
		}

		/// <summary>FF4's result window: gil found and the new total on the left, the experience on the right; level-ups and drops below.</summary>
		private void DrawResult(DrawList d)
		{
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
