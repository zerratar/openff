// The OpenFF battle on FF4: a fight on the current map, built on nothing but OpenFF.Data
// and the engine API - the way a mod would build one.
//
// FF3's battle part runs FF3's rules over FF3's tables and cannot take FF4's party; FF4's
// own battle is not ported. This is the first battle that reads the unified party and
// monsters: the party stands where it is, the monsters appear in front of it (FF4's
// m<model>_00 models with their b_m<model> motions - 101 idle, 201 attack), the camera
// takes a side view, and an ATB fight runs: every combatant's gauge fills with its agility;
// a full gauge gives a party member the command window (Fight, Item, Run) and a monster
// its attack. Damage is a placeholder formula until FF4's is read out of the binary
// (noted in Docs/Client-Plan.md). Victory pays experience, gil and drops into the party;
// defeat leaves everyone at 1 HP for now. K starts a test fight; scripted battles and
// encounters hook in through Start once their tables are read.

using System;
using System.Collections.Generic;
using OpenFF;
using OpenFF.Data;

namespace FF3
{
	internal sealed class Ff4Battle : GameService
	{
		private enum Phase { Idle, Intro, Fight, Victory, Defeat, Outro }
		private enum Command { Fight, Item, Run }
		private enum Pick { None, Command, Target, Item }

		private sealed class Fighter
		{
			public string Name;
			public bool IsMonster;
			public Character Member;          // party side
			public MonsterDefinition Monster; // monster side
			public Npc Npc;
			public int Hp, MaxHp;
			public int Attack, Defence, Agility;
			public float Gauge;               // 0..1
			public bool Alive => Hp > 0;
			public Vector3 Home;
		}

		private static Ff4Battle _instance;
		public static bool Active => _instance != null && _instance._phase != Phase.Idle;

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
		private readonly List<string> _log = new List<string>();
		private int _expWon, _gilWon;
		private readonly List<int> _dropsWon = new List<int>();
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

		/// <summary>A fight against these monsters (ids in the unified tables), on the spot.</summary>
		public bool Start(IEnumerable<int> monsterIds)
		{
			if (_phase != Phase.Idle || !EngineApi.InWorld || Ff4Cutscene.Active) return false;
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
					Attack = stats.Strength + weapon, Defence = Armour(c, tables) + stats.Vitality / 2, Agility = Math.Max(1, stats.Agility),
					Gauge = (float)_random.NextDouble() * 0.5f,
				});
			}
			Vector3 hero = Game.Hero.Position;
			Vector3 forward = Vector3.FromYaw(Game.Hero.Yaw).Flat.Normalized;
			if (forward.Length < 0.5f) forward = new Vector3(0, 0, -1);
			Vector3 side = new Vector3(-forward.Z, 0, forward.X);
			int n = 0;
			List<int> ids = new List<int>(monsterIds);
			foreach (int id in ids)
			{
				MonsterDefinition m = tables.Monster(id);
				if (m == null) { _log.Add("no monster " + id); continue; }
				float spread = (n - (ids.Count - 1) / 2f) * 14f;
				Vector3 at = Game.Field.OnGround(hero + forward * 34f + side * spread);
				Monster info = Game.Monsters.Find(id);
				Npc npc = Game.Npcs.SpawnModel(info?.Model ?? ("m" + m.ModelId.ToString("000") + "_00"), at, 0f);
				if (npc == null) { _log.Add("no model for " + m.Name); continue; }
				try { npc.BindMotions(info?.MotionSet ?? ("b_m" + m.ModelId.ToString("000"))); npc.PlayMotion(101, true); } catch (Exception) { }
				npc.LookAt(hero);
				npc.Solid = false;
				_foes.Add(new Fighter
				{
					Name = m.Name ?? ("monster " + id), IsMonster = true, Monster = m, Npc = npc, Home = at,
					Hp = Math.Max(1, m.MaxHp), MaxHp = Math.Max(1, m.MaxHp),
					Attack = Math.Max(1, ChainPack.U16(m.Raw, 0x20)), Defence = m.Stats.Vitality, Agility = Math.Max(1, m.Stats.Agility),
					Gauge = (float)_random.NextDouble() * 0.3f,
				});
				n++;
			}
			if (_foes.Count == 0) return false;

			Game.Input.Capture = true;
			Game.Hero.Freeze();
			Game.Hero.Face(forward.Yaw);
			try { Game.Hero.BindMotions("b_p_player_" + party.Leader.Id.ToString("00")); Game.Hero.PlayMotion(_heroMotionIdle, true); } catch (Exception) { }
			_centre = hero + forward * 17f;
			Game.Camera.MoveTo(_centre + side * 62f + new Vector3(0, 22f, 0) - forward * 6f);
			Game.Camera.LookAt(_centre + new Vector3(0, 6f, 0));
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

		// ---- the frame ----

		public override void OnUpdate()
		{
			if (_phase == Phase.Idle)
			{
				if (EngineApi.InWorld && !Ff4Cutscene.Active && !Game.Dialogue.IsOpen && !Game.Input.Capture && Game.Input.KeyPressed("K"))
				{
					// A test fight: the first two monsters of the tables (Goblin and Sword Rat).
					Start(new[] { 0, 1 });
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
				case Phase.Defeat:
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
					if (f.Alive && f.Gauge >= 1f) { _acting = f; _pick = Pick.Command; _cursor = 0; return; }
				}
				return;
			}
			InputState input = Game.Input;
			if (_pick == Pick.Command)
			{
				if (input.Pressed(Pad.Up)) _cursor = (_cursor + 2) % 3;
				if (input.Pressed(Pad.Down)) _cursor = (_cursor + 1) % 3;
				if (input.Pressed(Pad.A))
				{
					_command = (Command)_cursor;
					if (_command == Command.Fight) { _pick = Pick.Target; _cursor = FirstAliveFoe(); }
					else if (_command == Command.Item)
					{
						_itemChoices.Clear();
						foreach (OpenFF.Data.ItemStack s in Ff4Party.Party.Inventory)
						{
							ItemDefinition item = Ff4Party.Tables.Item(s.ItemId);
							if (item != null && item.Kind == ItemKind.Consumable && HealAmount(item) != 0) _itemChoices.Add(s.ItemId);
						}
						if (_itemChoices.Count == 0) { _log.Add("Nothing to use."); return; }
						_pick = Pick.Item; _cursor = 0;
					}
					else Run();
				}
				return;
			}
			if (_pick == Pick.Target)
			{
				if (input.Pressed(Pad.Left) || input.Pressed(Pad.Up)) _cursor = NextAliveFoe(_cursor, -1);
				if (input.Pressed(Pad.Right) || input.Pressed(Pad.Down)) _cursor = NextAliveFoe(_cursor, 1);
				if (input.Pressed(Pad.B)) { _pick = Pick.Command; _cursor = 0; return; }
				if (input.Pressed(Pad.A) && _cursor >= 0) MemberAttacks(_acting, _foes[_cursor]);
				return;
			}
			if (_pick == Pick.Item)
			{
				if (input.Pressed(Pad.Up)) _cursor = (_cursor + _itemChoices.Count - 1) % _itemChoices.Count;
				if (input.Pressed(Pad.Down)) _cursor = (_cursor + 1) % _itemChoices.Count;
				if (input.Pressed(Pad.B)) { _pick = Pick.Command; _cursor = 1; return; }
				if (input.Pressed(Pad.A)) UseItem(_acting, _itemChoices[_cursor]);
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

		private int Damage(int attack, int defence)
		{
			int raw = attack * 2 - defence;
			double roll = 0.9 + _random.NextDouble() * 0.2;
			return Math.Max(1, (int)Math.Round(raw * roll));
		}

		private void MemberAttacks(Fighter member, Fighter foe)
		{
			if (!foe.Alive) { _pick = Pick.Target; _cursor = FirstAliveFoe(); return; }
			int damage = Damage(member.Attack, foe.Defence);
			foe.Hp = Math.Max(0, foe.Hp - damage);
			try { Game.Hero.PlayMotion(_heroMotionAttack, false, 3); } catch (Exception) { }
			Game.Screen.PopNumber(foe.Npc.Position + new Vector3(0, 12f, 0), damage);
			Game.Audio.PlaySe(0, 3);
			_log.Add(member.Name + " hits " + foe.Name + " for " + damage + ".");
			if (!foe.Alive)
			{
				_log.Add(foe.Name + " is defeated.");
				foe.Npc.Alpha = 8;
				foe.Npc.Hidden = true;
				_expWon += foe.Monster.Experience;
				_gilWon += foe.Monster.Gil;
				foreach (DropChance drop in foe.Monster.Drops)
				{
					if (_random.Next(4096) < drop.Chance) { _dropsWon.Add(drop.ItemId); break; }
				}
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
			int damage = Damage(foe.Attack, target.Defence);
			target.Hp = Math.Max(0, target.Hp - damage);
			target.Member.Hp = target.Hp;
			try { foe.Npc.PlayMotion(201, false, 3); } catch (Exception) { }
			Game.Screen.PopNumber(Game.Hero.Position + new Vector3(0, 12f, 0), damage);
			Game.Screen.Flash(new Color(255, 60, 40), 6, 2);
			try { Game.Hero.PlayMotion(_heroMotionHurt, false, 3); } catch (Exception) { }
			_log.Add(foe.Name + " hits " + target.Name + " for " + damage + ".");
			foe.Gauge = 0f;
			if (!target.Alive) _log.Add(target.Name + " falls.");
			if (_party.FindAll(f => f.Alive).Count == 0) Lose();
		}

		private static int HealAmount(ItemDefinition item)
		{
			// FF4's consumables by id until the efficacy table is read: Potion 100, Hi-Potion 500, X-Potion 1000, Elixir all.
			switch (item.Id)
			{
				case 5001: return 100;
				case 5002: return 500;
				case 5003: return 1000;
				case 5006: return 99999;
				default: return 0;
			}
		}

		private void UseItem(Fighter member, int itemId)
		{
			ItemDefinition item = Ff4Party.Tables.Item(itemId);
			int heal = HealAmount(item);
			Fighter target = member;   // on oneself for now; the first fallen member if there is one
			foreach (Fighter f in _party) if (!f.Alive) { target = f; break; }
			if (!target.Alive && itemId != 5006) target = member;
			if (Ff4Party.Party.RemoveItem(itemId, 1))
			{
				int before = target.Hp;
				target.Hp = Math.Min(target.MaxHp, target.Hp + heal);
				target.Member.Hp = target.Hp;
				Game.Screen.PopNumber(Game.Hero.Position + new Vector3(0, 12f, 0), target.Hp - before, true);
				_log.Add(member.Name + " uses " + item.Name + ": " + target.Name + " +" + (target.Hp - before) + ".");
			}
			member.Gauge = 0f;
			_acting = null;
			_pick = Pick.None;
		}

		private void Run()
		{
			if (_random.Next(100) < 60)
			{
				_log.Add("The party runs.");
				_phase = Phase.Outro;
			}
			else
			{
				_log.Add("Could not run!");
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
			List<string> lines = new List<string> { "Victory! " + _expWon + " exp, " + _gilWon + " gil." };
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
				}
			}
			foreach (int id in _dropsWon)
			{
				party.AddItem(id, 1);
				lines.Add("Found " + (Ff4Party.Tables.Item(id)?.Name ?? ("item " + id)) + ".");
			}
			try { Game.Hero.PlayMotion(2010, false, 3); } catch (Exception) { }
			Game.Dialogue.Say(string.Join("\n", lines));
			Log.Write(LogChannel.General, "battle: won - " + string.Join(" ", lines));
		}

		private void Lose()
		{
			_phase = Phase.Defeat;
			_timer = 0;
			foreach (Fighter f in _party) { f.Hp = Math.Max(1, f.Hp); f.Member.Hp = f.Hp; }
			Game.Dialogue.Say("The party was defeated...\n(Everyone is left with 1 HP for now.)");
			Log.Write(LogChannel.General, "battle: lost");
		}

		private void End()
		{
			foreach (Fighter f in _foes)
			{
				try { f.Npc?.Remove(); } catch (Exception) { }
			}
			_foes.Clear();
			_party.Clear();
			try { Game.Hero.Unfreeze(); } catch (Exception) { }
			Game.Camera.Follow();
			Game.Input.Capture = false;
			_phase = Phase.Idle;
			_acting = null;
			_pick = Pick.None;
		}

		// ---- the HUD ----

		private void Draw()
		{
			DrawList d = Game.Draw;
			Color panel = new Color(16, 24, 72, 225);
			Color frame = new Color(230, 230, 240);
			Color dim = new Color(170, 175, 200);
			// The party: bottom right.
			float px = 420, py = 330, pw = 370, ph = 140;
			d.Rect(px, py, pw, ph, panel);
			d.Rect(px, py, pw, ph, frame, false);
			float y = py + 10;
			foreach (Fighter f in _party)
			{
				bool acting = f == _acting;
				d.Text((acting ? "> " : "  ") + f.Name, px + 10, y, acting ? Color.Yellow : (f.Alive ? Color.White : dim), 14);
				d.Text(f.Hp + "/" + f.MaxHp, px + 150, y + 1, f.Hp * 4 <= f.MaxHp ? Color.Red : Color.White, 13);
				d.Rect(px + 250, y + 5, 100, 8, new Color(40, 40, 40, 220));
				d.Rect(px + 250, y + 5, 100 * Math.Clamp(f.Gauge, 0f, 1f), 8, f.Gauge >= 1f ? Color.Yellow : new Color(90, 160, 255));
				y += 26;
			}
			// The foes: bottom left.
			float fx = 10, fy = 330, fw = 200, fh = 140;
			d.Rect(fx, fy, fw, fh, panel);
			d.Rect(fx, fy, fw, fh, frame, false);
			y = fy + 10;
			for (int i = 0; i < _foes.Count; i++)
			{
				Fighter f = _foes[i];
				bool target = _pick == Pick.Target && i == _cursor;
				d.Text((target ? "> " : "  ") + f.Name, fx + 10, y, target ? Color.Yellow : (f.Alive ? Color.White : dim), 13);
				if (f.Alive && f.Npc != null)
				{
					Vector2? head = Game.Camera.WorldToScreen(f.Npc.Position + new Vector3(0, 14, 0));
					if (head.HasValue)
					{
						float frac = Math.Clamp(f.Hp / (float)f.MaxHp, 0f, 1f);
						d.Rect(head.Value.X - 16, head.Value.Y - 4, 32, 5, new Color(0, 0, 0, 160));
						d.Rect(head.Value.X - 15, head.Value.Y - 3, 30 * frac, 3, frac > 0.4f ? new Color(255, 200, 60) : Color.Red);
						if (target) d.Text("v", head.Value.X - 4, head.Value.Y - 22, Color.Yellow, 16);
					}
				}
				y += 22;
			}
			// Commands: middle.
			if (_acting != null && _pick != Pick.None)
			{
				float cx = 220, cy = 330, cw = 190, ch = 140;
				d.Rect(cx, cy, cw, ch, panel);
				d.Rect(cx, cy, cw, ch, frame, false);
				if (_pick == Pick.Command)
				{
					string[] names = { "Fight", "Item", "Run" };
					for (int i = 0; i < 3; i++) d.Text((i == _cursor ? "> " : "  ") + names[i], cx + 12, cy + 12 + 26 * i, i == _cursor ? Color.Yellow : Color.White, 15);
				}
				else if (_pick == Pick.Target)
				{
					d.Text("Target?", cx + 12, cy + 12, dim, 13);
					d.Text("Left/Right, A", cx + 12, cy + 34, dim, 12);
				}
				else
				{
					for (int i = 0; i < _itemChoices.Count && i < 5; i++)
					{
						ItemDefinition item = Ff4Party.Tables.Item(_itemChoices[i]);
						d.Text((i == _cursor ? "> " : "  ") + (item?.Name ?? "?") + " x" + Ff4Party.Party.CountItem(_itemChoices[i]), cx + 12, cy + 12 + 24 * i, i == _cursor ? Color.Yellow : Color.White, 13);
					}
				}
			}
			// The log: top.
			int shown = Math.Min(3, _log.Count);
			for (int i = 0; i < shown; i++)
			{
				string line = _log[_log.Count - shown + i];
				d.Text(line, 400 - d.MeasureText(line, 13) / 2, 14 + 18 * i, i == shown - 1 ? Color.White : dim, 13);
			}
			if (_phase == Phase.Intro)
			{
				string title = "Encounter!";
				d.Text(title, 400 - d.MeasureText(title, 22) / 2, 120, Color.Yellow, 22);
			}
		}
	}
}
