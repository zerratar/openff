// Fighting together: two travellers near each other fight one battle.
//
// When a journeying traveller's battle begins (a random encounter, a monster on the map) and
// another traveller stands on the same map within reach, the battle is shared: the one it
// began for is the host. The host takes the others' heroes into its party from their last
// records over the wire (Game.Party.Import + AddMember), tells everyone the battle - which
// monsters, which ground, the seed, who is in it, every hero's record as the host holds them
// - and the guests start the same battle on their own client with the same party. From then
// on both compute the same fight (IBattle.Shared, BattleSync): each player chooses for their
// own heroes in the command window; the other heroes' turns show "Waiting for ..." while their
// player decides, the battle's animations going on; the commands cross the wire; the round
// plays out identically on both. Afterwards each party is its own again.
//
// The wire:  BR|battle|hero|<record>   a participant's record, before B
//            B|battle|formation|map|seed|heroes|host   the battle itself
//            BC|battle|round|hero|command   a hero's command for a round

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OpenFF;
using OpenFF.Events;

namespace Fellowship
{
	public sealed class SharedFight
	{
		/// <summary>How near another traveller must stand, in the map's units, to be in the fight.</summary>
		public const float Reach = 40f;

		private readonly FellowshipService _service;
		private readonly Random _random = new Random();
		private string _battleId;
		private readonly Dictionary<string, Dictionary<int, string>> _records = new Dictionary<string, Dictionary<int, string>>();   // battle -> hero -> record
		private readonly Dictionary<string, string> _commands = new Dictionary<string, string>();   // "battle/round/hero" -> command
		private readonly List<int> _added = new List<int>();   // heroes put into our party for the fight
		private bool _in;

		public SharedFight(FellowshipService service)
		{
			_service = service;
		}

		public bool InSharedBattle => _in;

		public void OnGameStart()
		{
			Game.Events.Subscribe<BattleStarting>(e => { if (!_in) Host(e); });
			Game.Events.Subscribe<BattleEnded>(e => Over());
		}

		/// <summary>The travellers who would be in a fight begun here now: journeying, on this map, within reach, not in a fight of their own.</summary>
		private List<Traveller> Companions()
		{
			if (!Game.Hero.Present) return new List<Traveller>();
			Vector3 me = Game.Hero.Position;
			return _service.Travellers.Where(t => t.Hero >= 0 && t.Hero != _service.Journey.MyHero && t.Map == Game.Field.Map && t.State != 1
				&& Vector3.Distance(new Vector3(t.Position.X, 0, t.Position.Z), new Vector3(me.X, 0, me.Z)) <= Reach).ToList();
		}

		/// <summary>Our battle begins: with company near, it is everyone's.</summary>
		private void Host(BattleStarting e)
		{
			Journey journey = _service.Journey;
			if (journey.MyHero < 0 || e.Formation < 0) return;
			List<Traveller> company = Companions().Where(t => journey.Known[t.Hero].Record != null).ToList();
			if (company.Count == 0) return;
			_battleId = _service.Id + "-" + DateTime.UtcNow.Ticks.ToString("x", CultureInfo.InvariantCulture);
			int seed = _random.Next(1, int.MaxValue);
			Dictionary<int, string> records = new Dictionary<int, string> { [journey.MyHero] = Game.Party.Export(journey.MyHero) };
			Game.Party.Restrict(null);
			foreach (Traveller t in company)
			{
				string record = journey.Known[t.Hero].Record;
				if (!Game.Party.Import(t.Hero, record) || !Game.Party.AddMember(t.Hero)) continue;
				records[t.Hero] = record;
				_added.Add(t.Hero);
			}
			Game.Party.Restrict(records.Keys);
			if (_added.Count == 0) { Game.Party.Restrict(new[] { journey.MyHero }); return; }
			List<int> order = records.Keys.OrderBy(h => h).ToList();
			Game.Party.Arrange(order);   // the same places on every client: the monsters aim by place
			Begin(seed, _added, order);
			foreach (KeyValuePair<int, string> r in records) _service.Send("BR|" + _battleId + "|" + r.Key + "|" + r.Value);
			_service.Send("B|" + _battleId + "|" + e.Formation + "|" + e.BattleMap + "|" + seed + "|" + string.Join(",", records.Keys.OrderBy(h => h)) + "|" + journey.MyHero);
			Game.Screen.Notice(string.Join(", ", company.Select(t => t.Name)) + (company.Count == 1 ? " joins" : " join") + " the fight!");
			Game.Log("fellowship: hosting battle " + _battleId + " - formation " + e.Formation + ", seed " + seed + ", with " + string.Join(", ", _added.Select(h => Journey.HeroNames[h])));
		}

		/// <summary>A record for a battle to come (BR).</summary>
		public void RecordFor(string battle, int hero, string record)
		{
			if (!_records.TryGetValue(battle, out Dictionary<int, string> r)) _records[battle] = r = new Dictionary<int, string>();
			r[hero] = record;
			if (_records.Count > 8) foreach (string old in _records.Keys.Where(k => k != battle).Take(_records.Count - 8).ToList()) _records.Remove(old);
		}

		/// <summary>Another traveller's battle begins with us in it (B): the same battle here, the same party.</summary>
		public void Join(Traveller host, string[] f)
		{
			// B|battle|formation|map|seed|heroes|host
			if (f.Length < 10) return;
			Journey journey = _service.Journey;
			string battle = f[4];
			int formation = int.Parse(f[5]), map = int.Parse(f[6]), seed = int.Parse(f[7]);
			List<int> heroes = f[8].Split(',').Select(int.Parse).ToList();
			if (journey.MyHero < 0 || !heroes.Contains(journey.MyHero)) return;
			if (_in || Game.Battle.InBattle) { Game.Log("fellowship: " + host.Name + "'s battle begins, but we are in one"); return; }
			if (!_records.TryGetValue(battle, out Dictionary<int, string> records) || heroes.Any(h => !records.ContainsKey(h)))
			{
				Game.Log("fellowship: " + host.Name + "'s battle " + battle + " - not every record came; not joining");
				return;
			}
			if (Game.Field.Busy) { Game.Screen.Notice(host.Name + " fights - but you are busy; they fight on without you."); return; }
			_battleId = battle;
			Game.Party.Restrict(null);
			// Every hero as the host holds them - ours too, so both sides start from one and the same party.
			foreach (int h in heroes)
			{
				Game.Party.Import(h, records[h]);
				if (h != journey.MyHero && Game.Party.AddMember(h)) _added.Add(h);
			}
			Game.Party.Restrict(heroes);
			Game.Party.Arrange(heroes);   // as the host has them
			Begin(seed, heroes.Where(h => h != journey.MyHero).ToList(), heroes);
			Game.Screen.Notice("You join " + host.Name + "'s fight!");
			Game.Log("fellowship: joining battle " + battle + " - formation " + formation + ", seed " + seed);
			Game.Battle.Start(formation, map);
		}

		private void Begin(int seed, List<int> remote, List<int> all)
		{
			_in = true;
			string battle = _battleId;
			Game.Battle.Shared = new SharedBattle
			{
				Seed = seed,
				RemoteHeroes = new HashSet<int>(remote),
				RemoteCommand = (round, hero) => _commands.TryGetValue(battle + "/" + round + "/" + hero, out string c) ? c : null,
				LocalCommand = (round, hero, text) => _service.Send("BC|" + battle + "|" + round + "|" + hero + "|" + text),
				HeroName = hero => Journey.HeroNames[hero],
			};
		}

		/// <summary>A hero's command for a round of the battle under way (BC).</summary>
		public void Command(string[] f)
		{
			// BC|battle|round|hero|command (the command may hold '|': the rest of the line)
			if (f.Length < 8) return;
			_commands[f[4] + "/" + f[5] + "/" + f[6]] = string.Join("|", f.Skip(7));
		}

		/// <summary>The fight is over: the others' heroes leave our party, ours is held to our hero again.</summary>
		private void Over()
		{
			if (!_in) return;
			_in = false;
			foreach (int h in _added) Game.Party.RemoveMember(h);
			_added.Clear();
			if (_service.Journey.MyHero >= 0) Game.Party.Arrange(new[] { _service.Journey.MyHero });   // our hero at the front again
			if (_service.Journey.MyHero >= 0) Game.Party.Restrict(new[] { _service.Journey.MyHero });
			Game.Battle.Shared = null;
			_commands.Clear();
			Game.Log("fellowship: the shared battle " + _battleId + " is over");
			_battleId = null;
		}

		/// <summary>"Waiting for Arc..." over the battle while a remote hero's player decides.</summary>
		public void Draw()
		{
			SharedBattle shared = Game.Battle.Shared;
			if (!_in || shared == null || !Game.Battle.InBattle || shared.WaitingFor < 0) return;
			string text = "Waiting for " + Journey.HeroNames[shared.WaitingFor] + (Game.Time.Frame / 20 % 3 == 0 ? "." : Game.Time.Frame / 20 % 3 == 1 ? ".." : "...");
			float w = Game.Draw.MeasureText(text, 14);
			Game.Draw.Rect(400 - w / 2 - 12, 40, w + 24, 28, new Color(0, 0, 0, 170));
			Game.Draw.Text(text, 400 - w / 2, 46, new Color(255, 240, 160), 14);
		}
	}
}
