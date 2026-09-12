// Fellowship - walk the world together.
//
// A network mod: every OpenFF client with this mod on the same LAN becomes a traveller in
// the same world. No server - each client speaks UDP to the network's broadcast address a
// few times a second and listens for the others (fellowship.json beside the mod sets the
// port and your name; two clients on one machine work too).
//
// What a traveller can do:
//
//   - See the others. On the same map they walk about as figures in their hero's own model,
//     a name over the head, a mark when they are in battle or in a menu. On other maps they
//     are a line in the corner: who, where, how they are.
//   - Talk. F5..F8 send a line ("Hello!", "Follow me!", "Help!", "Well done!"); it shows over
//     the sender's head for those who can see them, and as a notice for the rest; the sender's
//     figure strikes the talk pose.
//   - Aid. A traveller in battle can be sent aid from the Fellowship screen: their party is
//     healed a quarter of its HP, a notice says who sent it; once every thirty seconds a giver.
//   - Give. Items out of your bag into a traveller's (pick the item, then how many), or gil
//     from your purse. A gift leaves at once and is sent again until the receiver acknowledges
//     it; one that cannot be delivered comes back.
//   - Travel to. Warp to a traveller's map and side (once the menus have closed and the
//     field is back - Game.Field.Busy says when).
//   - Share the moment. A battle won is told to the others on the map; a traveller who falls
//     (the party wiped) is mourned.
//
// The Fellowship screen (main menu ▸ Fellowship, after Config) lists the travellers with
// MenuList; a press opens the actions for one. Everything here is the engine's API: Game.Npcs
// for the figures, Game.Camera.WorldToScreen and Game.Draw for the tags, Game.Party for aid
// and gifts, Game.Field.Warp for travel, Game.Events for the battles, the menu system for the
// screens - and System.Net for the wire.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using OpenFF;
using OpenFF.Events;

namespace Fellowship
{
	/// <summary>Another traveller as last heard.</summary>
	public sealed class Traveller
	{
		public string Id;
		public string Name = "?";
		public string Map;
		public string Model = "j101";
		public Vector3 Position;
		public float Yaw;
		public int Hp, MaxHp, Level;
		public string Job = "";
		/// <summary>The hero they journey as (Journey), or -1.</summary>
		public int Hero = -1;
		/// <summary>0 on the field, 1 in battle, 2 in a menu.</summary>
		public int State;
		public DateTime LastHeard;
		public IPEndPoint From;
		/// <summary>The figure on our map, while they share it.</summary>
		public Npc Figure;
		/// <summary>The last line they said and when, for the bubble over the head.</summary>
		public string Said;
		public DateTime SaidAt;
		public string Where => Map ?? "nowhere";
		public string Standing => State == 1 ? "in battle" : State == 2 ? "in a menu" : "walking";
	}

	public sealed class FellowshipService : GameService, ISaveable
	{
		public const string Version = "1";
		private const int DefaultPort = 47474;
		private static readonly string[] QuickLines = { "Hello!", "Follow me!", "Help!", "Well done!" };
		private static readonly string[] QuickKeys = { "F5", "F6", "F7", "F8" };

		public static FellowshipService Instance { get; private set; }

		private UdpClient _socket;
		private IPEndPoint _broadcast;
		private int _port = DefaultPort;
		private string _id = Guid.NewGuid().ToString("N").Substring(0, 8);
		private string _name;
		private readonly Dictionary<string, Traveller> _travellers = new Dictionary<string, Traveller>();
		private int _frames;
		private DateTime _lastAid = DateTime.MinValue;
		private bool _hudOn = true;
		private int _lastState = -1;

		public override bool WantsUpdate => true;

		/// <summary>Everyone heard from lately, nearest map first.</summary>
		public IReadOnlyList<Traveller> Travellers => _travellers.Values.OrderBy(t => t.Map == Game.Field.Map ? 0 : 1).ThenBy(t => t.Name).ToList();
		public string MyName => _name;
		public string Id => _id;
		public bool AidReady => (DateTime.UtcNow - _lastAid).TotalSeconds >= 30;
		/// <summary>The journey together: one hero each (Journey.cs).</summary>
		public Journey Journey { get; private set; }
		/// <summary>Fighting together: a battle two travellers near each other compute as one (SharedFight.cs).</summary>
		public SharedFight Fight { get; private set; }
		private int _claim = -1;
		/// <summary>The hero this client holds on the wire: the journey's, or the one being claimed at the shrine.</summary>
		private int HeroOnWire => Journey != null && Journey.MyHero >= 0 ? Journey.MyHero : _claim;
		/// <summary>The shrine's claim (-1 to let go): everyone hears at once.</summary>
		public void Claim(int hero) { _claim = hero; SendState(_lastState); }
		/// <summary>The story reached another player's hero here: R|hero|map to everyone.</summary>
		public void SendStoryReached(int hero, string map) => Send("R|" + hero + "|" + (map ?? ""));

		public override void OnGameStart()
		{
			Instance = this;
			Journey = new Journey(this);
			Journey.OnGameStart();
			Fight = new SharedFight(this);
			Fight.OnGameStart();
			ReadSettings();
			try
			{
				_socket = new UdpClient();
				_socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				_socket.ExclusiveAddressUse = false;
				_socket.EnableBroadcast = true;
				_socket.Client.Bind(new IPEndPoint(IPAddress.Any, _port));
				_broadcast = new IPEndPoint(IPAddress.Broadcast, _port);
				Game.Log("fellowship: " + _name + " (" + _id + ") on UDP " + _port + " - F5..F8 to speak, the Fellowship screen in the main menu");
			}
			catch (Exception ex)
			{
				Game.Warn("fellowship: no network - " + ex.Message);
				_socket = null;
			}
			Game.Events.Subscribe<BattleEnded>(e =>
			{
				if (e.Result == BattleResult.Won) Send("W|" + Game.Party.Gil.ToString(CultureInfo.InvariantCulture));
				else if (e.Result == BattleResult.Lost) Send("L|");
			});
			Game.Events.Subscribe<MapLeaving>(e => { foreach (Traveller t in _travellers.Values) DropFigure(t); });
		}

		private void ReadSettings()
		{
			_name = Game.Party.Members.FirstOrDefault()?.Name ?? Environment.UserName;
			try
			{
				string path = Path.Combine(Mod.Directory, "fellowship.json");
				if (File.Exists(path))
				{
					using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(path));
					if (doc.RootElement.TryGetProperty("port", out JsonElement p) && p.TryGetInt32(out int port) && port > 0) _port = port;
					if (doc.RootElement.TryGetProperty("name", out JsonElement n) && n.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(n.GetString())) { _name = n.GetString().Trim(); _nameFixed = true; }
				}
			}
			catch (Exception ex) { Game.Warn("fellowship.json: " + ex.Message); }
			// FELLOWSHIP_NAME names this traveller for a run - two clients on one machine, two names.
			string env = Environment.GetEnvironmentVariable("FELLOWSHIP_NAME");
			if (!string.IsNullOrWhiteSpace(env)) { _name = env.Trim(); _nameFixed = true; }
		}

		public override void OnQuit()
		{
			Send("Q|");
			try { _socket?.Close(); } catch (Exception) { }
			_socket = null;
		}

		// ---- each frame ----

		public override void OnUpdate()
		{
			Journey.OnUpdate();
			Journey.Draw();
			Fight.Draw();
			if (_socket == null) return;
			_frames++;
			Receive();
			// Our state a few times a second, and at once when it changes (a battle begins).
			int state = Game.Battle.InBattle ? 1 : Game.Menus.Current != null ? 2 : 0;
			if (_frames % 6 == 0 || state != _lastState) { SendState(state); _lastState = state; }
			Expire();
			Resend();
			// Our hero's whole record every ten seconds - every three with company on the map, since a battle begun near us takes them as last heard - so the others could journey on as them, or fight beside them (Journey.Known, SharedFight).
			bool company = _travellers.Values.Any(t => t.Map != null && t.Map == Game.Field.Map);
			if (_frames % (company ? 180 : 600) == 120) { string record = Journey.MyRecord(); if (record != null) Send("E|" + Journey.MyHero + "|" + record); }
			Keys();
			Travel();
			Figures();
			Hud();
		}

		private void SendState(int state)
		{
			PartyMember lead = Game.Party.Members.FirstOrDefault();
			if (lead != null && !_nameFixed && !string.IsNullOrWhiteSpace(lead.Name)) _name = lead.Name;   // the leading hero's name unless fellowship.json or FELLOWSHIP_NAME chose one
			Vector3 p = Game.Hero.Present ? Game.Hero.Position : default;
			Send(string.Join("|", "S", _name, Game.Field.Map ?? "", F(p.X), F(p.Y), F(p.Z), F(Game.Hero.Present ? Game.Hero.Yaw : 0), Game.Hero.Present ? Game.Hero.Model ?? "j101" : "j101",
				lead?.Hp ?? 0, lead?.MaxHp ?? 0, lead?.Level ?? 0, lead?.JobTitle ?? "", state, HeroOnWire));
		}

		private bool _nameFixed;
		private static string F(float v) => v.ToString("0.#", CultureInfo.InvariantCulture);
		private static float P(string s) => float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : 0;
		private static int I(string s) => int.TryParse(s, out int v) ? v : 0;

		public void Send(string body)
		{
			if (_socket == null) return;
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes("FF3F|" + Version + "|" + _id + "|" + body);
				_socket.Send(bytes, bytes.Length, _broadcast);
			}
			catch (Exception ex) { Game.Warn("fellowship: send - " + ex.Message); }
		}

		private void Receive()
		{
			try
			{
				while (_socket.Available > 0)
				{
					IPEndPoint from = new IPEndPoint(IPAddress.Any, 0);
					byte[] data = _socket.Receive(ref from);
					string text = Encoding.UTF8.GetString(data);
					string[] f = text.Split('|');
					if (f.Length < 4 || f[0] != "FF3F" || f[2] == _id) continue;
					Handle(f, from);
				}
			}
			catch (SocketException) { }
			catch (Exception ex) { Game.Warn("fellowship: receive - " + ex.Message); }
		}

		private void Handle(string[] f, IPEndPoint from)
		{
			string id = f[2], kind = f[3];
			if (!_travellers.TryGetValue(id, out Traveller t))
			{
				if (kind == "Q") return;
				t = new Traveller { Id = id, From = from };
				_travellers[id] = t;
			}
			t.LastHeard = DateTime.UtcNow;
			switch (kind)
			{
				case "S":
					if (f.Length < 16) return;
					bool arrived = t.Map == null;
					string wasMap = t.Map;
					t.Name = f[4]; t.Map = f[5] == "" ? null : f[5];
					t.Position = new Vector3(P(f[6]), P(f[7]), P(f[8])); t.Yaw = P(f[9]); t.Model = f[10];
					t.Hp = I(f[11]); t.MaxHp = I(f[12]); t.Level = I(f[13]); t.Job = f[14]; t.State = I(f[15]);
					t.Hero = f.Length > 16 ? I(f[16]) : -1;
					Journey.Heard(t);
					if (arrived) { Game.Log("fellowship: " + t.Name + " is here (" + t.Where + ")"); Game.Screen.Notice(t.Name + " joined the fellowship"); }
					else if (wasMap != t.Map && t.Map != null && t.Map == Game.Field.Map) Game.Screen.Notice(t.Name + " arrives");
					break;
				case "C":
					t.Said = f.Length > 4 ? f[4] : ""; t.SaidAt = DateTime.UtcNow;
					if (t.Map != Game.Field.Map) Game.Screen.Notice(t.Name + ": " + t.Said);
					t.Figure?.PlayMotion(1001);
					break;
				case "A":   // aid for us
					if (f.Length > 4 && f[4] == _id)
					{
						foreach (PartyMember m in Game.Party.Members) if (m.Hp > 0) Game.Party.Heal(m.Id, Math.Max(1, m.MaxHp / 4));
						Game.Screen.Notice(t.Name + " sends aid! The party is healed.");
						Game.Log("fellowship: aid from " + t.Name);
					}
					break;
				case "G":   // items for us: G|us|item|count|number
					if (f.Length > 7 && f[4] == _id)
					{
						Send("K|" + id + "|" + f[7]);
						if (!_taken.Add(f[7])) break;   // sent again because our answer was lost
						int item = I(f[5]), count = Math.Max(1, I(f[6]));
						Game.Party.AddItem(item, count);
						Game.Screen.Notice(t.Name + " gives you " + Counted(Game.Items.Find(item)?.Name ?? ("item " + item), count) + "!");
						Game.Log("fellowship: " + t.Name + " gave " + item + " x" + count);
					}
					break;
				case "M":   // gil for us: M|us|amount|number
					if (f.Length > 6 && f[4] == _id)
					{
						Send("K|" + id + "|" + f[6]);
						if (!_taken.Add(f[6])) break;
						int amount = I(f[5]);
						Game.Party.Gil += amount;
						Game.Screen.Notice(t.Name + " gives you " + amount.ToString("N0", CultureInfo.InvariantCulture) + " gil!");
						Game.Log("fellowship: " + t.Name + " gave " + amount + " gil");
					}
					break;
				case "K":   // a gift of ours arrived
					if (f.Length > 5 && f[4] == _id) _gifts.Remove(f[5]);
					break;
				case "E":   // a hero's whole record: E|hero|<Game.Party.Export> (the record is JSON and may hold '|': the rest of the line is it)
					if (f.Length > 5) Journey.RecordHeard(I(f[4]), string.Join("|", f.Skip(5)), t.Name);
					break;
				case "BR":  // a record for a battle to come: BR|battle|hero|<record>
					if (f.Length > 6) Fight.RecordFor(f[4], I(f[5]), string.Join("|", f.Skip(6)));
					break;
				case "B":   // a battle begins with us in it
					Fight.Join(t, f);
					break;
				case "BC":  // a hero's command in the shared battle
					Fight.Command(f);
					break;
				case "W":
					if (t.Map == Game.Field.Map) Game.Screen.Notice(t.Name + " won a battle!");
					break;
				case "L":
					Game.Screen.Notice(t.Name + " has fallen...");
					break;
				case "R":   // the story reached a hero over there: R|hero|map
					if (f.Length > 5 && Journey.MyHero >= 0 && I(f[4]) == Journey.MyHero)
						Game.Screen.Notice("The story has reached " + Journey.HeroNames[Journey.MyHero] + " where " + t.Name + " is (" + f[5] + ") - travel to them to take it up.");
					break;
				case "Q":
					Game.Screen.Notice(t.Name + " left the fellowship");
					DropFigure(t);
					_travellers.Remove(id);
					break;
			}
		}

		private void Expire()
		{
			foreach (Traveller t in _travellers.Values.Where(t => (DateTime.UtcNow - t.LastHeard).TotalSeconds > 6).ToList())
			{
				Game.Screen.Notice(t.Name + " is gone");
				DropFigure(t);
				_travellers.Remove(t.Id);
			}
		}

		// ---- the save chunk: the journey's hero rides with the save (ISaveable) ----

		public string ChunkId => "Fellowship/Journey";
		public int ChunkVersion => 1;
		public object Save() => Journey?.Save();
		public void Load(int version, JsonElement data) => Journey?.Load(version, data);

		// ---- the keys ----

		private void Keys()
		{
			for (int i = 0; i < QuickKeys.Length; i++)
			{
				if (Game.Input.KeyPressed(QuickKeys[i])) Say(QuickLines[i]);
			}
			if (Game.Input.KeyPressed("F9")) _hudOn = !_hudOn;
		}

		public void Say(string line)
		{
			Send("C|" + line.Replace('|', '/'));
			Game.Screen.Notice("You: " + line);
			Game.Hero.PlayMotion(1001);
		}

		/// <summary>Aid to a traveller in battle: their party heals a quarter. Once every thirty seconds.</summary>
		public bool SendAid(Traveller t)
		{
			if (t == null || !AidReady) return false;
			_lastAid = DateTime.UtcNow;
			Send("A|" + t.Id);
			Game.Screen.Notice("Aid sent to " + t.Name);
			return true;
		}

		// ---- gifts: items and gil, acknowledged ----
		//
		// UDP drops packets now and then, and a gift that never arrives is an item lost: so a gift
		// leaves our bag at once, goes out with a number of its own, and is sent again each second
		// until the receiver answers K|<us>|<number> - five tries, then it comes back to the bag.
		// The receiver keeps the numbers it has taken, so a repeat is answered but not taken twice.

		private sealed class Gift { public string Body; public DateTime Sent; public int Tries; public int ItemId, Count, Gil; public string To; }
		private readonly Dictionary<string, Gift> _gifts = new Dictionary<string, Gift>();
		private readonly HashSet<string> _taken = new HashSet<string>();
		private int _giftSeq;

		/// <summary>Items from our bag into theirs.</summary>
		public bool Give(Traveller t, int itemId, int count)
		{
			Item item = Game.Items.Find(itemId);
			if (t == null || item == null || count <= 0 || !Game.Party.RemoveItem(itemId, count)) return false;
			string number = _id + "-" + (++_giftSeq);
			Post(number, new Gift { To = t.Id, ItemId = itemId, Count = count, Body = "G|" + t.Id + "|" + itemId + "|" + count + "|" + number });
			Game.Screen.Notice(Counted(item.Name, count) + " sent to " + t.Name);
			return true;
		}

		/// <summary>Gil from our purse into theirs.</summary>
		public bool GiveGil(Traveller t, int amount)
		{
			if (t == null || amount <= 0 || Game.Party.Gil < amount) return false;
			Game.Party.Gil -= amount;
			string number = _id + "-" + (++_giftSeq);
			Post(number, new Gift { To = t.Id, Gil = amount, Body = "M|" + t.Id + "|" + amount + "|" + number });
			Game.Screen.Notice(amount.ToString("N0", CultureInfo.InvariantCulture) + " gil sent to " + t.Name);
			return true;
		}

		private void Post(string number, Gift g)
		{
			g.Sent = DateTime.UtcNow; g.Tries = 1;
			_gifts[number] = g;
			Send(g.Body);
		}

		/// <summary>Gifts not yet acknowledged go out again; after five tries they come back.</summary>
		private void Resend()
		{
			if (_gifts.Count == 0) return;
			foreach (KeyValuePair<string, Gift> kv in _gifts.ToList())
			{
				Gift g = kv.Value;
				if ((DateTime.UtcNow - g.Sent).TotalSeconds < 1) continue;
				if (g.Tries < 5) { g.Tries++; g.Sent = DateTime.UtcNow; Send(g.Body); continue; }
				_gifts.Remove(kv.Key);
				if (g.Gil > 0) Game.Party.Gil += g.Gil; else Game.Party.AddItem(g.ItemId, g.Count);
				string to = _travellers.TryGetValue(g.To, out Traveller t) ? t.Name : "the traveller";
				Game.Screen.Notice("Could not reach " + to + " - the gift is back with you.");
			}
		}

		/// <summary>"Potion x3", "Phoenix Down" - the game's own way of counting.</summary>
		public static string Counted(string name, int count) => count == 1 ? name : name + " x" + count;

		/// <summary>Warp to a traveller's map and side - once the menus have closed and the field is back.</summary>
		public bool TravelTo(Traveller t)
		{
			if (t == null || t.Map == null) return false;
			_travel = t;
			Game.Screen.Notice("Travelling to " + t.Name);
			return true;
		}

		private Traveller _travel;

		private void Travel()
		{
			// The menu that asked for the travel is still closing; the field ignores a warp until it owns the screen again.
			if (_travel == null || Game.Field.Busy || Game.Menus.Current != null || !Game.Hero.Present) return;
			Traveller t = _travel;
			_travel = null;
			if (t.Map == null) return;
			Vector3 beside = new Vector3(t.Position.X + 8, t.Position.Y, t.Position.Z);
			if (t.Map == Game.Field.Map) Game.Hero.Teleport(Game.Field.OnGround(beside));
			else Game.Field.Warp(t.Map, beside);
		}

		// ---- the figures on our map ----

		private void Figures()
		{
			if (!Game.Hero.Present || Game.Battle.InBattle || Journey.Choosing) return;   // at the shrine the figures are the four heroes
			foreach (Traveller t in _travellers.Values)
			{
				bool here = t.Map != null && t.Map == Game.Field.Map;
				if (!here) { DropFigure(t); continue; }
				if (t.Figure == null || !t.Figure.Alive)
				{
					t.Figure = Game.Npcs.Spawn(t.Model ?? "j101", t.Position, t.Yaw);
					if (t.Figure == null) continue;
					t.Figure.Owner = Mod;
					t.Figure.Solid = false;
					t.Figure.InteractRadius = 0;
					Game.Log("fellowship: " + t.Name + " appears on " + t.Map);
				}
				// Walk to where they are now (a short walk plays the steps); stand still when they do.
				Vector3 d = new Vector3(t.Position.X - t.Figure.Position.X, t.Position.Y - t.Figure.Position.Y, t.Position.Z - t.Figure.Position.Z);
				float dist = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z);
				if (dist > 60) t.Figure.Teleport(t.Position);
				else if (dist > 0.5f) t.Figure.MoveTo(t.Position, 6);
				else if (!t.Figure.Moving && Math.Abs(t.Figure.Yaw - t.Yaw) > 5) t.Figure.Face(t.Yaw);
				t.Figure.Balloon = t.State == 1;
			}
		}

		private void DropFigure(Traveller t)
		{
			if (t.Figure == null) return;
			try { if (t.Figure.Alive) t.Figure.Remove(); } catch (Exception) { }
			t.Figure = null;
		}

		// ---- the tags and the corner ----

		private void Hud()
		{
			// The field's own moments only: not over the game's menus, a shop, a dialogue, an event (Field.Busy) or a battle - the screens have the list then, and the battle's Back button sits where the corner is.
			if (!_hudOn || _travellers.Count == 0 || Game.Menus.Current != null || Game.Dialogue.IsOpen || Journey.Choosing || Game.Field.Busy || Game.Battle.InBattle) return;
			Color name = new Color(255, 240, 160), dim = new Color(200, 200, 210), red = new Color(255, 120, 110), bubble = new Color(255, 255, 255);
			foreach (Traveller t in _travellers.Values)
			{
				if (t.Figure != null && t.Figure.Alive && !Game.Battle.InBattle)
				{
					Vector2? at = Game.Camera.WorldToScreen(new Vector3(t.Position.X, t.Position.Y + 22, t.Position.Z));
					if (at.HasValue)
					{
						string tag = t.Name + (t.State == 1 ? "  [battle]" : t.State == 2 ? "  [menu]" : "");
						Game.Draw.Text(tag, at.Value.X - Game.Draw.MeasureText(tag) / 2, at.Value.Y - 14, t.State == 1 ? red : name, 12);
						if (t.Said != null && (DateTime.UtcNow - t.SaidAt).TotalSeconds < 4)
						{
							float w = Game.Draw.MeasureText(t.Said) + 12;
							Game.Draw.Rect(at.Value.X - w / 2, at.Value.Y - 40, w, 20, new Color(20, 30, 60, 220));
							Game.Draw.Text(t.Said, at.Value.X - w / 2 + 6, at.Value.Y - 37, bubble, 12);
						}
					}
				}
			}
			// The corner, under the map's name and clear of the Map / Menu buttons: everyone, nearest first,
			// on a dark panel; 16 px a line for the 12 px text, the hint a little apart in the smaller size.
			List<Traveller> shown = Travellers.ToList();
			const float x = 10, line = 16, pad = 6;
			float widest = Math.Max(Game.Draw.MeasureText("Fellowship"), shown.Max(t => Game.Draw.MeasureText(Line(t))));
			float height = pad + line * (1 + shown.Count) + 4 + 12 + pad;
			Game.Draw.Rect(x - pad, 44 - pad, widest + pad * 2, height, new Color(10, 16, 40, 170));
			float y = 44;
			Game.Draw.Text("Fellowship", x, y, name, 12); y += line;
			foreach (Traveller t in shown)
			{
				bool here = t.Map == Game.Field.Map;
				Game.Draw.Text(Line(t), x, y, t.State == 1 ? red : here ? bubble : dim, 12);
				y += line;
			}
			Game.Draw.Text("F5-F8 speak   F9 hide", x, y + 4, dim, 10);
		}

		/// <summary>A traveller's line in the corner: who, where, how they stand, their HP.</summary>
		private string Line(Traveller t)
		{
			return t.Name + "   " + (t.Map == Game.Field.Map ? "here" : t.Where) + "   " + t.Standing + "   " + t.Hp + "/" + t.MaxHp;
		}
	}
}
