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

namespace FF3
{
	internal sealed class Ff4Battle : GameService
	{
		private enum Phase { Idle, Intro, Fight, Victory, Defeat, Outro }
		private enum Command { Fight, Magic, Item, Run }
		private enum Pick { None, Command, Target, Item, Spell, Ally }
		private static readonly string[] CommandNames = { "Attack", "Magic", "Items", "Run" };

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
			public float Gauge;               // 0..1
			public bool Alive => Hp > 0;
			public Vector3 Home;
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
		private int _heroMotionIdle = 2007, _heroMotionAttack = 2008, _heroMotionHurt = 2009;

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
			foreach (MonsterPartySlot slot in party.Slots)
			{
				for (int k = 0; k < Math.Max(1, slot.Count); k++)
				{
					ids.Add(slot.MonsterId);
					places.Add(new Vector3(slot.X, 0, slot.Z));
				}
			}
			_placements = places;
			_cameraType = party.CameraType;
			bool started = Start(ids, inScene, battleMap);
			if (started) Log.Write(LogChannel.General, "battle: encounter group " + partyId);
			return started;
		}

		private List<Vector3> _placements;
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
					Name = c.Name, Member = c, Hp = c.Hp, MaxHp = c.MaxHp,
					Attack = Math.Max(1, weapon > 0 ? weapon : stats.Strength / 2), Defence = Armour(c, tables), Agility = Math.Max(1, stats.Agility),
					Level = c.Level, Intellect = stats.Intellect, Spirit = stats.Spirit, Vitality = stats.Vitality, MagicDefence = MagicArmour(c, tables),
					Strength = stats.Strength, HitChance = weapon > 0 ? WeaponHit(c, tables) : 90, Evade = Evasion(c, tables),
					Gauge = Math.Max(0f, 0.5f - 0.12f * _party.Count) + (float)_random.NextDouble() * 0.08f,
				});
			}
			if (onStage)
			{
				// FF4 fights with its battle models: pNN_00 per player type (the field walks pNN_01,
				// whose joints do not match the battle motion sets b_p_player_NN - binding one onto
				// the other crashes the joint animation). So every member, the leader included,
				// stands as a spawned battle model, and the field's hero waits unseen at the
				// leader's spot until the jump back.
				Game.Hero.Teleport(Ff4BattleStage.PartySpot(0, _party.Count));
				try { EngineApi.HeroPlayer?.setTransparencyRate(100); } catch (Exception) { }
				for (int i = 0; i < _party.Count; i++)
				{
					Fighter ally = _party[i];
					CharacterDefinition who = ally.Member.Definition;
					Vector3 spot = Ff4BattleStage.PartySpot(i, _party.Count);
					Npc npc = null;
					try { npc = Game.Npcs.SpawnModel("p" + who.Id.ToString("00") + "_00", spot, 0f); } catch (Exception) { }
					if (npc == null) { Log.Write(LogChannel.File, "battle: no battle model for " + ally.Name); continue; }
					try { npc.BindMotions("b_p_player_" + who.Id.ToString("00")); npc.PlayMotion(_heroMotionIdle, true); } catch (Exception) { }
					npc.Solid = false;
					npc.LookAt(spot + Ff4BattleStage.PartyFacing(i) * 10f);
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
				if (onStage)
				{
					at = Ff4BattleStage.MonsterSpot(_placements != null && n < _placements.Count ? _placements[n] : Vector3.Zero, n, ids.Count);
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
				npc.LookAt(hero);
				npc.Solid = false;
				_foes.Add(new Fighter
				{
					Name = m.Name ?? ("monster " + id), IsMonster = true, Monster = m, Npc = npc, Home = at,
					Hp = Math.Max(1, m.MaxHp), MaxHp = Math.Max(1, m.MaxHp),
					Attack = Math.Max(1, m.Attack), Defence = Math.Max(0, m.Defence), Agility = Math.Max(1, m.Stats.Agility),
					Level = Math.Max(1, m.Level), Intellect = m.Stats.Intellect, Spirit = m.Stats.Spirit, Vitality = m.Stats.Vitality, MagicDefence = Math.Max(0, m.MagicDefence),
					Strength = m.Stats.Strength, HitChance = m.Hit > 0 ? m.Hit : 90, Evade = Math.Max(0, m.Evade),
					Gauge = (float)_random.NextDouble() * 0.3f,
				});
				n++;
			}
			_placements = null;
			_pendingIds = null;
			if (_foes.Count == 0) { if (Ff4BattleStage.Active) Ff4BattleStage.Leave(); return false; }

			Game.Input.Capture = true;
			Game.Hero.Freeze();
			Game.Hero.Face(forward.Yaw);
			if (onStage)
			{
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
			Log.Write(LogChannel.General, "battle: " + _foes.Count + " foe(s): " + string.Join(", ", _foes.ConvertAll(f => f.Name + " (" + f.MaxHp + " hp)")) + " against " + string.Join(", ", _party.ConvertAll(f => f.Name + " L" + f.Member.Level)));
			return true;
		}

		private static int Weapon(Character c, GameTables tables)
		{
			int best = 0;
			foreach (int id in c.Equipment)
			{
				ItemDefinition item = id != 0 ? tables.Item(id) : null;
				if (item?.Equip != null && item.Kind == ItemKind.Weapon) best = Math.Max(best, item.Equip.Attack);
			}
			return best;
		}

		private static int Armour(Character c, GameTables tables)
		{
			int total = 0;
			foreach (int id in c.Equipment)
			{
				ItemDefinition item = id != 0 ? tables.Item(id) : null;
				if (item?.Equip != null && item.Kind == ItemKind.Armour) total += item.Equip.Defence;
			}
			return total;
		}

		private static int WeaponHit(Character c, GameTables tables)
		{
			int best = 0;
			foreach (int id in c.Equipment)
			{
				ItemDefinition item = id != 0 ? tables.Item(id) : null;
				if (item?.Equip != null && item.Kind == ItemKind.Weapon) best = Math.Max(best, item.Equip.Hit);
			}
			return best > 0 ? best : 90;
		}

		private static int Evasion(Character c, GameTables tables)
		{
			int total = 0;
			foreach (int id in c.Equipment)
			{
				ItemDefinition item = id != 0 ? tables.Item(id) : null;
				if (item?.Equip != null && item.Kind == ItemKind.Armour) total += item.Equip.Evade;
			}
			return total;
		}

		private static int MagicArmour(Character c, GameTables tables)
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

		private void Fight()
		{
			if (Ff4BattleStage.Active) Log.Sample(LogChannel.File, "battle-camera", 120, () => "battle: camera at " + Game.Camera.Position + " hero at " + Game.Hero.Position);
			if (_acting == null)
			{
				// Gauges fill; the first full one acts.
				foreach (Fighter f in _party) if (f.Alive) f.Gauge = Math.Min(1f, f.Gauge + 0.0025f + f.Agility * 0.00045f);
				foreach (Fighter f in _foes) if (f.Alive) f.Gauge = Math.Min(1f, f.Gauge + 0.0025f + f.Agility * 0.00045f);
				foreach (Fighter f in _foes)
				{
					if (f.Alive && f.Gauge >= 1f) { MonsterActs(f); return; }
				}
				foreach (Fighter f in _party)
				{
					if (f.Alive && f.Gauge >= 1f) { _acting = f; _pick = Pick.Command; _cursor = 0; Log.Write(LogChannel.File, "battle: " + f.Name + " may act"); return; }
				}
				return;
			}
			InputState input = Game.Input;
			if (_pick == Pick.Command)
			{
				int count = CommandNames.Length;
				if (input.Pressed(Pad.Up)) _cursor = (_cursor + count - 1) % count;
				if (input.Pressed(Pad.Down)) _cursor = (_cursor + 1) % count;
				bool runNow = input.KeyPressed("M");   // FF4's "M Run away"
				if (runNow) _cursor = (int)Command.Run;
				if (input.Pressed(Pad.A) || runNow)
				{
					_command = (Command)_cursor;
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
				if (input.Pressed(Pad.B)) { _pick = Pick.Command; _cursor = (int)Command.Magic; return; }
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
				if (input.Pressed(Pad.B)) { _pick = Pick.Command; _cursor = (int)Command.Item; return; }
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
					Game.Screen.PopNumber(Where(t) + new Vector3(0, 12f, 0), t.Hp - before, true);
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
					Game.Screen.PopNumber(Where(t) + new Vector3(0, 12f, 0), damage);
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

		/// <summary>Plays a battle motion on a member: the leader is the hero, the others their spawned models.</summary>
		private void Play(Fighter f, int motion, bool loop = false, int blend = 3)
		{
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
			Play(member, _heroMotionAttack);
			if (!Hits(member, foe))
			{
				Say(member.Name + " misses " + foe.Name + ".");
			}
			else
			{
				int damage = Damage(member, foe);
				foe.Hp = Math.Max(0, foe.Hp - damage);
				Game.Screen.PopNumber(foe.Npc.Position + new Vector3(0, 12f, 0), damage);
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
			try { foe.Npc.PlayMotion(201, false, 3); } catch (Exception) { }
			if (!Hits(foe, target))
			{
				Say(foe.Name + " misses " + target.Name + ".");
				foe.Gauge = 0f;
				return;
			}
			int damage = Damage(foe, target);
			target.Hp = Math.Max(0, target.Hp - damage);
			target.Member.Hp = target.Hp;
			Game.Screen.PopNumber(Where(target) + new Vector3(0, 12f, 0), damage);
			Game.Screen.Flash(new Color(255, 60, 40), 6, 2);
			Play(target, _heroMotionHurt);
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
				if (target.Hp != before) Game.Screen.PopNumber(Where(target) + new Vector3(0, 12f, 0), target.Hp - before, true);
				Say(member.Name + " uses " + item.Name + ": " + target.Name + (revive ? " rises." : (effect.Hp > 0 ? " +" + (target.Hp - before) + " HP" : "") + (effect.Mp > 0 ? " +" + effect.Mp + " MP" : "") + "."));
			}
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
			foreach (Fighter f in _party) { if (f.Alive) Play(f, 2010); }
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
				try { f.Npc?.Remove(); } catch (Exception) { }
			}
			_foes.Clear();
			_party.Clear();
			if (Ff4BattleStage.Active)
			{
				try { EngineApi.HeroPlayer?.setTransparencyRate(0); } catch (Exception) { }
				try { Ff4EventCamera.Release(); } catch (Exception) { }
				Ff4BattleStage.Leave();
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
		private const float CmdX = 58f, CmdY = 300f, CmdW = 188f, CmdH = 180f, CmdRow = 45f;
		private const float PartyX = 276f, PartyY = 338f, PartyW = 460f, PartyH = 142f, PartyRow = 28f;

		private void Window(DrawList d, float x, float y, float w, float h)
		{
			if (Ff4Ui.Window(d, x, y, w, h)) return;
			d.Rect(x, y, w, h, PanelFill);
			d.Rect(x, y, w, h, PanelEdge, false);
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

		private void KeyHint(DrawList d, string key, string what, float x, float y)
		{
			d.Rect(x, y, 18, 18, new Color(30, 90, 150, 230));
			d.Rect(x, y, 18, 18, PanelEdge, false);
			d.Text(key, x + 9 - d.MeasureText(key, 12) / 2, y + 2, Color.White, 12);
			Shadowed(d, what, x + 24, y + 1, Color.White, 14);
		}

		private int Accuracy(Fighter attacker, Fighter target) => Math.Clamp(attacker.HitChance + attacker.Agility - (target.Evade + target.Agility) + 20, 0, 100);

		private void Draw()
		{
			DrawList d = Game.Draw;
			if (_phase == Phase.Defeat) return;
			if (_phase == Phase.Victory) { DrawResult(d); return; }
			bool choosing = _acting != null && _pick != Pick.None;

			// Bottom left: the commands, the foes when a target is picked, or the spell or item list.
			Window(d, CmdX, CmdY, CmdW, CmdH);
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
			else
			{
				for (int i = 0; i < CommandNames.Length; i++)
				{
					float y = CmdY + CmdRow * i;
					if (i > 0) d.Line(CmdX + 4, y, CmdX + CmdW - 4, y, RowLine);
					Centred(d, CommandNames[i], CmdX + CmdW / 2 + 8, y + 13, choosing ? Color.White : Dim, 16);
					if (choosing && _pick == Pick.Command && i == _cursor) Glove(d, CmdX + 36, y + 14);
				}
			}

			// Bottom right: the party's rows - name, hit points, magic points, gauge - or the picked foe's card.
			Window(d, PartyX, PartyY, PartyW, PartyH);
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
				for (int i = 0; i < _party.Count && i < 5; i++)
				{
					Fighter f = _party[i];
					float y = PartyY + PartyRow * i;
					if (i > 0) d.Line(PartyX + 4, y, PartyX + PartyW - 4, y, RowLine);
					bool acting = f == _acting && choosing;
					bool picked = _pick == Pick.Ally && i == _cursor;
					Color name = !f.Alive ? Dim : acting ? Gold : Color.White;
					Shadowed(d, f.Name, PartyX + 10, y + 5, name, 16);
					Color hp = !f.Alive ? Dim : f.Hp * 4 <= f.MaxHp ? new Color(255, 120, 110) : Color.White;
					RightAligned(d, f.Hp.ToString(), PartyX + 150, y + 5, hp, 16);
					Shadowed(d, "/ " + f.MaxHp, PartyX + 156, y + 5, hp, 16);
					if (f.Member != null && f.Member.MaxMp > 0) RightAligned(d, f.Mp.ToString(), PartyX + 292, y + 5, Color.White, 16);
					Gauge(d, PartyX + 340, y + PartyRow / 2, f.Gauge, f.Alive);
					if (picked) Glove(d, PartyX + 6, y + 6);
				}
			}
			// The keys, over the party's rows (FF4 writes "C Auto battle  M Run away" there).
			KeyHint(d, "Z", "Confirm", PartyX + 230, PartyY - 24);
			KeyHint(d, "M", "Run away", PartyX + 344, PartyY - 24);

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
