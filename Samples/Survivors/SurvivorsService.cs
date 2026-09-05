// Goblin Survivors - a real-time roguelite on the field, made of the game's own parts.
//
// Press T on any map. Goblins - the game's monster, model, stats and attack motion - come
// at the hero in waves and hit when they reach them. The hero turns to the nearest goblin
// and throws a spell at it on their own, with the battle's casting motion; kills give the
// goblins' own experience and gil, and this mod's experience. Levelling up here deals
// three cards: faster attacks, more power, longer reach, a second bolt, a new spell
// (Blizzard, Thunder), a stat, a heal. Goblins sometimes leave a chest with a piece of
// equipment the hero can wear. Between waves a shopkeeper appears: talk to open the
// game's own shop, talk again to start the next wave. The leader falling ends the run.
//
// Everything is the engine API: Game.Monsters/Magic/Items as data and formulas,
// Npcs.SpawnModel + BindMotions/PlayMotion for the goblins' attack and the hero's cast,
// Party.Hurt/GiveExperience/AddItem/Equip, Screen.PopNumber/Flash, Camera.WorldToScreen
// with Game.Draw for the HUD and the cards, Input.Capture while a card is chosen,
// Dialogue.Ask through a spawned shopkeeper, Shops.Open for the game's shop screen.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenFF;
using OpenFF.Events;

namespace Survivors
{
	public class SurvivorsService : GameService
	{
		public override bool WantsUpdate => true;

		public enum Phase { Idle, Fighting, Cards, Between, Shop, Over }

		// Public: handed over across a hot reload.
		public Phase State = Phase.Idle;
		public int Wave, Kills, Level = 1, Xp, XpNext = 30;
		public int AttackEvery = 60;      // frames between the hero's bolts
		public int Power = 100;           // percent of the formula's damage
		public float Range = 40f;
		public int Bolts = 1;             // targets per volley
		public int RegenEvery = 0;        // frames per hit point back; 0 none
		public int GreedPercent = 100;
		public int SpellId;               // the bolt's spell (Fire to begin with)

		private Spell _spell;
		private Monster _goblin;
		private string _model;
		private readonly List<Foe> _foes = new List<Foe>();
		private readonly List<Chest> _chests = new List<Chest>();
		private readonly Random _random = new Random();
		private int _attackIn, _regenIn;
		private string _banner;
		private int _bannerFrames;
		private Npc _shopkeeper;
		private string _boundOn;
		private IDisposable _mapLeaving;
		private Card[] _hand;
		private int _pick;

		public override void OnGameStart()
		{
			Game.Log("survivors: ready - press T on a map");
			_mapLeaving = Game.Events.Subscribe<MapLeaving>(_ => { if (State != Phase.Idle) End(null); });
		}

		public override void OnQuit() => _mapLeaving?.Dispose();

		public override void OnUpdate()
		{
			if (!Game.Hero.Present) return;
			switch (State)
			{
				case Phase.Idle:
				case Phase.Over:
					if (Game.Input.KeyPressed("T") && !Game.Dialogue.IsOpen && !Game.Battle.InBattle) Start();
					return;
				case Phase.Fighting:
					Fight();
					break;
				case Phase.Cards:
					ChooseCard();
					break;
				case Phase.Between:
					break;
				case Phase.Shop:
					ShopInput();
					break;
			}
			TickChests();
			DrawHud();
		}

		// ---- setting up ----

		private void Start()
		{
			_spell = Game.Magic.Find(SpellId) ?? Game.Magic.Find("Fire") ?? Game.Magic.All.FirstOrDefault(s => s.Kind == MagicKind.Attack && s.School == MagicSchool.Black);
			_goblin = Game.Monsters.Find("Goblin") ?? Game.Monsters.All.FirstOrDefault(m => m.MaxHp > 0 && m.MaxHp < 9999);
			if (_spell == null || _goblin == null)
			{
				Game.Warn("survivors: no spell or no monsters in the tables here; not starting");
				return;
			}
			SpellId = _spell.Id;
			_model = _goblin.Model;
			Wave = 0; Kills = 0; Level = 1; Xp = 0; XpNext = 30;
			AttackEvery = 60; Power = 100; Range = 40f; Bolts = 1; RegenEvery = 0; GreedPercent = 100;
			Game.Field.Encounters = false;
			BindHero();
			State = Phase.Fighting;
			Game.Log("survivors: start - " + _goblin.Name + " (" + _goblin.MaxHp + " hp) vs " + _spell.Name);
			NextWave();
		}

		private void BindHero()
		{
			string map = Game.Field.Map;
			if (_boundOn == map) return;
			Game.Hero.BindBattleMotions();
			_boundOn = map;
		}

		private void NextWave()
		{
			Wave++;
			int count = 1 + Wave * 2;
			Vector3 hero = Game.Hero.Position;
			// Points tagged "spawn" placed in Crystal (an OpenFF project's scene file) say where
			// the goblins come from; without them, a ring around the hero.
			List<GameObject> spawns = Game.World.Legacy.WithTag("spawn").ToList();
			int made = 0;
			for (int i = 0; i < count; i++)
			{
				double angle = Math.PI * 2 * i / count + _random.NextDouble() * 0.4;
				float radius = 34f + (float)_random.NextDouble() * 14f;
				Vector3 at = Game.Field.OnGround(hero + new Vector3((float)Math.Sin(angle), 0, (float)Math.Cos(angle)) * radius);
				if (spawns.Count > 0)
				{
					GameObject spawn = spawns[i % spawns.Count];
					at = Game.Field.OnGround(spawn.Transform.Position + new Vector3((float)(_random.NextDouble() - 0.5) * 6f, 0, (float)(_random.NextDouble() - 0.5) * 6f));
				}
				Npc npc = Game.Npcs.SpawnModel(_model, at, 0f);
				if (npc == null) continue;
				npc.Owner = Mod;
				npc.Solid = false;
				npc.BindMotions(_goblin.MotionSet);
				npc.LookAt(hero);
				GameObject o = Game.World.Legacy.Add("goblin " + Wave + "-" + i);
				o.Owner = Mod;
				o.Tags.Add("survivors");
				int hp = _goblin.MaxHp * 6 + Wave * 20;   // two bolts each at first, so some reach the hero
				Foe foe = new Foe { Arena = this, Npc = npc, Hp = hp, MaxHp = hp, Stats = _goblin.Stats, Speed = 0.16f + Wave * 0.015f, Cooldown = 30 + _random.Next(60) };
				o.AddComponent(foe);
				_foes.Add(foe);
				made++;
			}
			State = Phase.Fighting;
			Banner("Wave " + Wave + " - " + made + " " + _goblin.Name + "s", 90);
			Game.Log("survivors: wave " + Wave + ", " + made + " goblins" + (spawns.Count > 0 ? " from " + spawns.Count + " spawn point(s)" : ""));
		}

		private void End(string message)
		{
			foreach (Foe foe in _foes.ToArray()) foe.Vanish();
			_foes.Clear();
			foreach (Chest chest in _chests) chest.Npc?.Remove();
			_chests.Clear();
			RemoveShopkeeper();
			Game.Input.Capture = false;
			Game.Hero.Unfreeze();
			Game.Field.Encounters = true;
			_banner = null;
			State = message == null ? Phase.Idle : Phase.Over;
			if (message != null)
			{
				Game.Dialogue.Say(message);
				Game.Log("survivors: " + message);
			}
		}

		// ---- the fight ----

		private void Fight()
		{
			PartyMember hero = Hero();
			if (hero == null) return;
			if (hero.Hp <= 1)
			{
				End(hero.Name + " fell on wave " + Wave + " with " + Kills + " goblins down. Press T for another run.");
				return;
			}
			if (RegenEvery > 0 && --_regenIn <= 0)
			{
				_regenIn = RegenEvery;
				Game.Party.Heal(hero.Id, 1);
			}
			if (--_attackIn <= 0)
			{
				_attackIn = AttackEvery;
				Volley(hero);
				// A kill may have dealt cards: the wave's end waits until they are chosen.
				if (State != Phase.Fighting) return;
			}
			if (_foes.All(f => !f.IsAlive))
			{
				_foes.RemoveAll(f => f.Gone);
				State = Phase.Between;
				Banner("Wave " + Wave + " cleared - a trader is here", 120);
				Game.Log("survivors: wave " + Wave + " cleared");
				SpawnShopkeeper();
			}
		}

		/// <summary>The hero turns to the nearest goblins and throws the spell at them, with the battle's casting motion.</summary>
		private void Volley(PartyMember hero)
		{
			Vector3 at = Game.Hero.Position;
			List<Foe> targets = _foes.Where(f => f.IsAlive && Vector3.FlatDistance(f.Npc.Position, at) <= Range)
				.OrderBy(f => Vector3.FlatDistance(f.Npc.Position, at)).Take(Bolts).ToList();
			if (targets.Count == 0) return;
			Game.Hero.LookAt(targets[0].Npc.Position);
			Game.Hero.PlayMotion(HeroMotion.MagicShot, false, 3);
			foreach (Foe target in targets)
			{
				Game.Magic.CastOn(_spell, target.Npc, 0.8f);
				int damage = Math.Max(1, Game.Magic.Damage(_spell, hero.Stats, target.Stats) * Power / 100 / 2);
				target.Hp -= damage;
				Game.Screen.PopNumber(target.Npc.Position + new Vector3(0, 10, 0), damage);
				if (target.Hp <= 0) Kill(hero, target);
			}
		}

		private void Kill(PartyMember hero, Foe foe)
		{
			Kills++;
			int gil = _goblin.Gil * GreedPercent / 100;
			Game.Party.Gil += gil;
			bool levelled = Game.Party.GiveExperience(hero.Id, _goblin.Experience);
			if (levelled) Banner(hero.Name + " reached level " + Game.Party.Member(hero.Id)?.Level, 90);
			if (_random.Next(100) < 30) DropChest(foe.Npc.Position);
			foe.Die();
			Xp += 10 + Wave * 2;
			Game.Log("survivors: goblin down (" + Kills + "), +" + _goblin.Experience + " exp, +" + gil + " gil, xp " + Xp + "/" + XpNext);
			if (Xp >= XpNext) LevelUp();
		}

		// ---- the goblins' hits ----

		internal void Hit(Foe foe, int damage)
		{
			PartyMember hero = Hero();
			if (hero == null || State != Phase.Fighting) return;
			int left = Game.Party.Hurt(hero.Id, damage);
			Game.Screen.PopNumber(Game.Hero.Position + new Vector3(0, 10, 0), damage);
			Game.Screen.Flash(new Color(255, 60, 40), 6, 2);
			if (left <= 1) Game.Hero.PlayMotion(HeroMotion.Damage);
		}

		// ---- cards ----

		private sealed class Card
		{
			public string Name, Text;
			public Action Apply;
		}

		private List<Card> Pool()
		{
			PartyMember hero = Hero();
			List<Card> pool = new List<Card>
			{
				new Card { Name = "Quick hands", Text = "Bolts come 15% faster", Apply = () => AttackEvery = Math.Max(20, AttackEvery * 85 / 100) },
				new Card { Name = "Focus", Text = "+25% bolt damage", Apply = () => Power += 25 },
				new Card { Name = "Long reach", Text = "+10 range", Apply = () => Range += 10f },
				new Card { Name = "Regeneration", Text = "A hit point back every 2 seconds", Apply = () => RegenEvery = RegenEvery == 0 ? 120 : Math.Max(30, RegenEvery * 2 / 3) },
				new Card { Name = "Greed", Text = "+50% gil from goblins", Apply = () => GreedPercent += 50 },
				new Card { Name = "Vitality", Text = "+3 vitality, +10 max HP", Apply = () => { if (hero != null) { Game.Party.SetStat(hero.Id, Stat.Vitality, hero.Stats.Vitality + 3); Game.Party.SetHp(hero.Id, hero.Hp + 10, hero.MaxHp + 10); } } },
				new Card { Name = "Intellect", Text = "+3 intellect: harder bolts", Apply = () => { if (hero != null) Game.Party.SetStat(hero.Id, Stat.Intellect, hero.Stats.Intellect + 3); } },
				new Card { Name = "Second wind", Text = "Back to full health", Apply = () => { if (hero != null) Game.Party.Heal(hero.Id, hero.MaxHp); } },
			};
			if (Bolts < 3) pool.Add(new Card { Name = "Second bolt", Text = "One more goblin per volley", Apply = () => Bolts++ });
			foreach (string name in new[] { "Blizzard", "Thunder", "Fira", "Blizzara", "Thundara" })
			{
				Spell other = Game.Magic.Find(name);
				if (other != null && other.Id != _spell.Id && other.Power > _spell.Power)
				{
					Spell chosen = other;
					pool.Add(new Card { Name = other.Name, Text = "Throw " + other.Name + " instead (power " + other.Power + ")", Apply = () => { _spell = chosen; SpellId = chosen.Id; if (hero != null) Game.Party.LearnSpell(hero.Id, chosen.Id); } });
				}
			}
			return pool;
		}

		private void LevelUp()
		{
			Level++;
			Xp -= XpNext;
			XpNext = 30 + Level * 20;
			List<Card> pool = Pool();
			_hand = new Card[3];
			for (int i = 0; i < 3 && pool.Count > 0; i++)
			{
				int n = _random.Next(pool.Count);
				_hand[i] = pool[n];
				pool.RemoveAt(n);
			}
			_pick = 1;
			State = Phase.Cards;
			Game.Input.Capture = true;
			Game.Hero.Freeze();
			Game.Hero.PlayMotion(HeroMotion.LevelUp1);
			Game.Audio.PlaySe(0, 1);
			Game.Log("survivors: level " + Level + " - cards: " + string.Join(" / ", _hand.Where(c => c != null).Select(c => c.Name)));
		}

		private void ChooseCard()
		{
			InputState input = Game.Input;
			if (input.Pressed(Pad.Left) || input.KeyPressed("Left") || input.KeyPressed("A")) _pick = (_pick + 2) % 3;
			if (input.Pressed(Pad.Right) || input.KeyPressed("Right") || input.KeyPressed("D")) _pick = (_pick + 1) % 3;
			int chosen = -1;
			if (input.KeyPressed("D1")) chosen = 0;
			if (input.KeyPressed("D2")) chosen = 1;
			if (input.KeyPressed("D3")) chosen = 2;
			if (input.Pressed(Pad.A)) chosen = _pick;
			if (chosen < 0 || _hand == null || _hand[chosen] == null) return;
			Card card = _hand[chosen];
			Game.Guard("card " + card.Name, card.Apply);
			Game.Log("survivors: took " + card.Name);
			_hand = null;
			Game.Input.Capture = false;
			Game.Hero.Unfreeze();
			State = Phase.Fighting;
			Banner(card.Name, 60);
		}

		// ---- between waves: the trader ----

		private void SpawnShopkeeper()
		{
			RemoveShopkeeper();
			Vector3 hero = Game.Hero.Position;
			Vector3 at = Game.Field.OnGround(hero + Vector3.FromYaw(Game.Hero.Yaw) * 9f);
			_shopkeeper = Game.Npcs.Spawn("n011", at, Game.Hero.Yaw + 180f);
			if (_shopkeeper == null) { NextWave(); return; }
			_shopkeeper.Owner = Mod;
			_shopkeeper.Solid = false;
			_shopkeeper.LookAt(hero);
			_shopkeeper.Interacted += _ => Trade();
		}

		private void RemoveShopkeeper()
		{
			if (_shopkeeper != null && _shopkeeper.Alive) _shopkeeper.Remove();
			_shopkeeper = null;
		}

		private void Trade()
		{
			PartyMember hero = Hero();
			Game.Log("survivors: trader asked");
			Game.Dialogue.Ask("Wave " + Wave + " is done. " + (hero != null ? hero.Name + " has " + Game.Party.Gil + " gil. " : "") + "Buy something? (No: next wave)", yes =>
			{
				if (yes)
				{
					OpenTrader();
				}
				else
				{
					Game.Log("survivors: trader sends wave " + (Wave + 1));
					RemoveShopkeeper();
					NextWave();
				}
			});
		}

		// The trader's stock is drawn by the mod: the game's own shop screen is a map of its
		// own (the shop interior), and a map change would end the run. The wares are what the
		// first town's shops sell (Game.Shops.Info reads the table), at the game's prices.
		private readonly List<Item> _stock = new List<Item>();
		private int _stockPick, _stockTop;
		private long _stockOpened;

		private void OpenTrader()
		{
			PartyMember hero = Hero();
			_stock.Clear();
			int jobBit = hero != null ? 1 << hero.Job : 0;
			HashSet<int> seen = new HashSet<int>();
			for (int i = 0; i < 8; i++)
			{
				ShopInfo info = Game.Shops.Info(i, "t01");
				if (info == null) continue;
				foreach (int id in info.ItemIds)
				{
					Item item = Game.Items.Find(id);
					if (item == null || item.Price <= 0 || !seen.Add(id)) continue;
					bool equipment = item.Category == ItemCategory.Weapon || item.Category == ItemCategory.Armor;
					if (equipment && (item.Jobs & jobBit) == 0) continue;
					if (item.Category == ItemCategory.Magic || item.Category == ItemCategory.Key) continue;
					_stock.Add(item);
				}
			}
			_stock.Sort((a, b) => a.Category != b.Category ? a.Category.CompareTo(b.Category) : a.Price.CompareTo(b.Price));
			_stockPick = 0;
			_stockTop = 0;
			_stockOpened = Game.Time.Frame;
			State = Phase.Shop;
			Game.Input.Capture = true;
			Game.Hero.Freeze();
			Game.Log("survivors: trader shows " + _stock.Count + " wares");
		}

		private void ShopInput()
		{
			InputState input = Game.Input;
			if (_stock.Count == 0) { CloseTrader(); return; }
			// The press that answered the trader's question must not also buy the first ware.
			if (Game.Time.Frame - _stockOpened < 2) return;
			if (input.Pressed(Pad.Up) || input.KeyPressed("Up")) _stockPick = (_stockPick + _stock.Count - 1) % _stock.Count;
			if (input.Pressed(Pad.Down) || input.KeyPressed("Down")) _stockPick = (_stockPick + 1) % _stock.Count;
			if (_stockPick < _stockTop) _stockTop = _stockPick;
			if (_stockPick >= _stockTop + 8) _stockTop = _stockPick - 7;
			if (input.Pressed(Pad.B) || input.KeyPressed("Escape")) { CloseTrader(); return; }
			if (input.Pressed(Pad.A))
			{
				Item item = _stock[_stockPick];
				PartyMember hero = Hero();
				if (Game.Party.Gil < item.Price)
				{
					Banner("Not enough gil for " + item.Name, 60);
					Game.Audio.PlaySe(0, 2);
					return;
				}
				Game.Party.Gil -= item.Price;
				Game.Party.AddItem(item.Id, 1);
				string worn = "";
				if (hero != null && item.Slot >= 0)
				{
					int current = Game.Party.Equipped(hero.Id, item.Slot);
					Item have = current > 0 ? Game.Items.Find(current) : null;
					if ((have == null || item.Attack + item.Defense > have.Attack + have.Defense) && Game.Party.Equip(hero.Id, item.Id)) worn = " - equipped";
				}
				Game.Audio.PlaySe(0, 3);
				Banner("Bought " + item.Name + worn, 90);
				Game.Log("survivors: bought " + item.Name + " for " + item.Price + worn + ", gil left " + Game.Party.Gil);
			}
		}

		private void CloseTrader()
		{
			Game.Input.Capture = false;
			Game.Hero.Unfreeze();
			State = Phase.Between;
			Banner("Talk to the trader again: the next wave, or more wares", 120);
		}

		// ---- chests ----

		private sealed class Chest
		{
			public Npc Npc;
			public int ItemId;
		}

		private void DropChest(Vector3 at)
		{
			PartyMember hero = Hero();
			if (hero == null) return;
			int jobBit = 1 << hero.Job;
			List<Item> wearable = Game.Items.All.Where(i => (i.Category == ItemCategory.Weapon || i.Category == ItemCategory.Armor)
				&& (i.Jobs & jobBit) != 0 && i.Price > 0 && i.Price <= 300 + Wave * 400).ToList();
			if (wearable.Count == 0)
			{
				Game.Log("survivors: no wearable equipment for job " + hero.JobName + " among " + Game.Items.All.Count + " items");
				return;
			}
			Item item = wearable[_random.Next(wearable.Count)];
			// The treasure chest is an object model ("o" + number), not a character's.
			Npc box = Game.Npcs.SpawnModel("o001", Game.Field.OnGround(at), 0f) ?? Game.Npcs.SpawnModel("o004", Game.Field.OnGround(at), 0f);
			if (box == null) return;
			Game.Log("survivors: a chest with " + (item.Name ?? item.Id.ToString()) + " dropped");
			box.Owner = Mod;
			box.Solid = false;
			_chests.Add(new Chest { Npc = box, ItemId = item.Id });
		}

		private void TickChests()
		{
			if (_chests.Count == 0 || State == Phase.Cards) return;
			Vector3 hero = Game.Hero.Position;
			for (int i = _chests.Count - 1; i >= 0; i--)
			{
				Chest chest = _chests[i];
				if (chest.Npc == null || !chest.Npc.Alive) { _chests.RemoveAt(i); continue; }
				if (Vector3.FlatDistance(chest.Npc.Position, hero) > 6f) continue;
				Item item = Game.Items.Find(chest.ItemId);
				PartyMember member = Hero();
				Game.Party.AddItem(chest.ItemId, 1);
				string worn = "";
				if (item != null && member != null)
				{
					int current = Game.Party.Equipped(member.Id, item.Slot);
					Item have = current > 0 ? Game.Items.Find(current) : null;
					bool better = have == null || item.Attack + item.Defense > have.Attack + have.Defense;
					if (better && Game.Party.Equip(member.Id, item.Id)) worn = " - equipped";
				}
				Game.Audio.PlaySe(0, 3);
				Banner("Found " + (item?.Name ?? ("item " + chest.ItemId)) + worn, 120);
				Game.Log("survivors: chest gave " + (item?.Name ?? chest.ItemId.ToString()) + worn);
				chest.Npc.Remove();
				_chests.RemoveAt(i);
			}
		}

		// ---- the HUD ----

		private PartyMember Hero() => Game.Party.Members.Count > 0 ? Game.Party.Members[0] : null;

		private void Banner(string text, int frames)
		{
			_banner = text;
			_bannerFrames = frames;
		}

		private void DrawHud()
		{
			DrawList d = Game.Draw;
			PartyMember hero = Hero();
			if (hero != null)
			{
				d.Rect(8, 58, 250, 46, new Color(0, 0, 0, 150));
				d.Text(hero.Name + "  L" + hero.Level + " " + hero.JobName + "   wave " + Wave + "   kills " + Kills, 14, 61, Color.White, 12);
				float hp = hero.MaxHp > 0 ? Math.Clamp(hero.Hp / (float)hero.MaxHp, 0f, 1f) : 0f;
				d.Rect(14, 78, 160, 6, new Color(40, 40, 40, 200));
				d.Rect(14, 78, 160 * hp, 6, hp > 0.3f ? Color.Green : Color.Red);
				d.Text(hero.Hp + "/" + hero.MaxHp, 178, 74, Color.White, 11);
				float xp = XpNext > 0 ? Math.Clamp(Xp / (float)XpNext, 0f, 1f) : 0f;
				d.Rect(14, 88, 160, 4, new Color(40, 40, 40, 200));
				d.Rect(14, 88, 160 * xp, 4, new Color(120, 160, 255));
				d.Text("run level " + Level + "   " + (_spell?.Name ?? "") + " x" + Bolts + "  " + Power + "%  every " + AttackEvery + "f", 14, 93, new Color(200, 200, 210), 10);
			}
			foreach (Foe foe in _foes)
			{
				if (!foe.IsAlive) continue;
				Vector2? head = Game.Camera.WorldToScreen(foe.Npc.Position + new Vector3(0, 11, 0));
				if (!head.HasValue) continue;
				float frac = Math.Clamp(foe.Hp / (float)foe.MaxHp, 0f, 1f);
				d.Rect(head.Value.X - 16, head.Value.Y - 4, 32, 5, new Color(0, 0, 0, 160));
				d.Rect(head.Value.X - 15, head.Value.Y - 3, 30 * frac, 3, frac > 0.4f ? new Color(255, 200, 60) : Color.Red);
			}
			if (State == Phase.Cards && _hand != null)
			{
				d.Rect(0, 0, 800, 480, new Color(0, 0, 0, 120));
				d.Text("Level " + Level + " - choose a card (Left/Right, then A; or 1 2 3)", 400 - d.MeasureText("Level " + Level + " - choose a card (Left/Right, then A; or 1 2 3)", 16) / 2, 120, Color.Yellow, 16);
				for (int i = 0; i < 3; i++)
				{
					Card card = _hand[i];
					if (card == null) continue;
					float x = 110 + i * 200, y = 170, w = 180, h = 130;
					bool on = i == _pick;
					d.Rect(x, y, w, h, on ? new Color(40, 60, 120, 235) : new Color(20, 26, 46, 220));
					d.Rect(x, y, w, h, on ? Color.Yellow : new Color(120, 130, 170), filled: false);
					d.Text((i + 1) + ". " + card.Name, x + 12, y + 14, on ? Color.White : new Color(210, 210, 220), 15);
					// Wrap the text by hand: about 26 characters a line at this size.
					string[] words = card.Text.Split(' ');
					string line = "";
					float ly = y + 48;
					foreach (string word in words)
					{
						if ((line + " " + word).Trim().Length > 24) { d.Text(line, x + 12, ly, new Color(200, 200, 210), 12); ly += 18; line = word; }
						else line = (line + " " + word).Trim();
					}
					if (line.Length > 0) d.Text(line, x + 12, ly, new Color(200, 200, 210), 12);
				}
			}
			if (State == Phase.Shop)
			{
				d.Rect(0, 0, 800, 480, new Color(0, 0, 0, 110));
				float x = 150, y = 90, w = 500, h = 300;
				d.Rect(x, y, w, h, new Color(20, 28, 60, 235));
				d.Rect(x, y, w, h, new Color(200, 200, 220), filled: false);
				d.Text("The trader's wares", x + 14, y + 10, Color.Yellow, 16);
				string gil = Game.Party.Gil + " gil";
				d.Text(gil, x + w - 14 - d.MeasureText(gil, 14), y + 12, Color.White, 14);
				for (int row = 0; row < 8 && _stockTop + row < _stock.Count; row++)
				{
					Item item = _stock[_stockTop + row];
					bool on = _stockTop + row == _stockPick;
					float ry = y + 42 + row * 26;
					if (on) d.Rect(x + 8, ry - 2, w - 16, 24, new Color(60, 90, 170, 200));
					d.Text(item.Name ?? ("item " + item.Id), x + 20, ry + 2, on ? Color.White : new Color(210, 210, 220), 14);
					string what = item.Category == ItemCategory.Weapon ? "attack " + item.Attack : item.Category == ItemCategory.Armor ? "defence " + item.Defense : item.Category.ToString().ToLowerInvariant();
					d.Text(what, x + 240, ry + 4, new Color(170, 180, 200), 12);
					string price = item.Price.ToString();
					d.Text(price, x + w - 20 - d.MeasureText(price, 14), ry + 2, Game.Party.Gil >= item.Price ? Color.White : new Color(200, 90, 90), 14);
				}
				string hint = "Up/Down choose, A buys, B leaves";
				d.Text(hint, x + w / 2 - d.MeasureText(hint, 12) / 2, y + h - 24, new Color(200, 200, 210), 12);
			}
			if (_banner != null && _bannerFrames-- > 0)
			{
				float bw = d.MeasureText(_banner, 18);
				d.Rect(400 - bw / 2 - 12, 20, bw + 24, 30, new Color(0, 0, 0, 170));
				d.Text(_banner, 400 - bw / 2, 26, Color.Yellow, 18);
			}
			else if (_bannerFrames <= 0) _banner = null;
		}

		public override IEnumerable<string> DebugLines()
		{
			yield return "survivors " + State + " wave " + Wave + " foes " + _foes.Count(f => f.IsAlive) + " kills " + Kills + " run level " + Level + " (" + Xp + "/" + XpNext + ")";
		}
	}

	/// <summary>One goblin: walks at the hero, swings when it gets there, dies with a fade.</summary>
	public class Foe : Behaviour
	{
		public SurvivorsService Arena;
		public Npc Npc;
		public int Hp, MaxHp;
		public Stats Stats;
		public float Speed = 0.4f;
		public int Cooldown = 60;
		private bool _dead, _gone, _swinging;
		private int _swingFrames;

		public bool IsAlive => !_dead && Npc != null && Npc.Alive;
		public bool Gone => _gone;

		protected override void Update()
		{
			if (!IsAlive) return;
			Vector3 hero = Game.Hero.Position;
			float distance = Vector3.FlatDistance(Npc.Position, hero);
			if (_swinging)
			{
				if (--_swingFrames <= 0)
				{
					_swinging = false;
					if (distance <= 9f) Arena.Hit(this, Math.Max(1, Stats.Strength / 4) + Arena.Wave);
				}
				return;
			}
			if (distance > 7f)
			{
				// Step by step, so the goblin follows a moving hero; the walk is the host's.
				Vector3 step = Vector3.MoveToward(Npc.Position, hero, Math.Min(Speed * 20f, distance - 6f));
				if (!Npc.Moving) Npc.MoveTo(Game.Field.OnGround(step), 20);
			}
			else
			{
				Npc.Stop();
				Npc.LookAt(hero);
				if (--Cooldown <= 0)
				{
					Cooldown = 110;
					_swinging = true;
					_swingFrames = 18;   // the swing lands mid-motion
					Npc.PlayMotion(MonsterMotion.Attack, false, 2);
				}
			}
		}

		public void Die()
		{
			if (_dead) return;
			_dead = true;
			StartCoroutine(Fade());
		}

		public void Vanish()
		{
			_dead = true;
			_gone = true;
			if (Npc != null && Npc.Alive) Npc.Remove();
			if (GameObject != null) Game.World.Legacy.Destroy(GameObject);
		}

		private IEnumerator Fade()
		{
			Npc.Stop();
			for (int alpha = 255; alpha > 0; alpha -= 32)
			{
				Npc.Alpha = alpha;
				yield return Wait.Frames(2);
			}
			Vanish();
		}

		public override IEnumerable<string> DebugLines()
		{
			yield return "goblin " + Hp + "/" + MaxHp + (IsAlive ? "" : " dead");
		}
	}
}
