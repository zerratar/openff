// Journey together: the story played by several people, one hero each.
//
// From the title, *Journey together* starts a new game onto the Crystal's Choosing - one of the
// game's battle backgrounds with the mod's crystal on it, the four heroes in a row before it.
// Left and right walk the choice along the row; a hero another traveller holds is said so and
// cannot be picked; a press claims one. The claim goes out on the wire at once, and after a
// moment with no one else on the same hero (the lower client id keeps it when two press
// together) the party becomes that hero alone (Game.Party.Reset), held to them
// (Game.Party.Restrict - the story's joins of the other heroes are refused, since they are
// other players'), the story's lead is them (Game.Party.Protagonist), and the opening plays as
// it always does - the fall into the Altar Cave - with the hero picked.
//
// *Continue the journey* comes back through the choosing too: the save resumes, then the
// shrine, each hero with the level and job they were last seen at - your own from the save,
// the others as the wire last told (every client sends its hero's whole record now and then,
// Game.Party.Export, and keeps the others' in its chunk). Pick your own to go on where you
// stopped; pick another free hero and their record is put on (Game.Party.Import) - so a hero
// can change hands between sessions, and a player who cannot come can be played on.
//
// The saves live under the "fellowship" profile (Game.Title.NewGame's saveProfile; with
// FELLOWSHIP_NAME set, "fellowship-<name>"): a game played alone is never touched.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OpenFF;
using OpenFF.Events;

namespace Fellowship
{
	/// <summary>A hero as last known: whose, how far along, and their record when it has come over.</summary>
	public sealed class KnownHero
	{
		public int Level;
		public string Job = "";
		public string Owner = "";
		/// <summary>Game.Party.Export of them, or null until it has come over the wire.</summary>
		public string Record;
		public DateTime Seen;
	}

	/// <summary>The choosing and what it decided; owned by FellowshipService.</summary>
	public sealed class Journey
	{
		/// <summary>The save profile: "fellowship" - with FELLOWSHIP_NAME set (two clients on one machine), "fellowship-<name>", so they keep saves apart.</summary>
		public static string Profile => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FELLOWSHIP_NAME")) ? "fellowship" : "fellowship-" + Environment.GetEnvironmentVariable("FELLOWSHIP_NAME").Trim().ToLowerInvariant();
		/// <summary>The choosing's stage: one of the game's battle backgrounds (a battle stage loads as a map like any other) with the mod's crystal on it - nothing in the way of the figures.</summary>
		public const string ShrineMap = "b16";
		public const string OpeningMap = "d01_05";
		public static readonly string[] HeroNames = { "Luneth", "Arc", "Refia", "Ingus" };
		/// <summary>The heroes' field models in the starting job: j101 is Luneth the Freelancer, j201 Arc, and so on.</summary>
		public static string Model(int hero) => "j" + (hero + 1) + "01";

		private readonly FellowshipService _service;
		/// <summary>The hero this player journeys as, or -1 when the game is not a journey.</summary>
		public int MyHero = -1;
		/// <summary>Every hero as last known (the wire, the save).</summary>
		public readonly KnownHero[] Known = { new KnownHero(), new KnownHero(), new KnownHero(), new KnownHero() };
		/// <summary>Where the journey stood when it was saved, to go back to after the choosing.</summary>
		private string _savedMap;
		private Vector3 _savedPos;
		/// <summary>A save came back: the shrine is next, then the saved place.</summary>
		private bool _resume;
		private int _claiming = -1;
		private DateTime _claimedAt;

		// The choosing scene.
		private bool _choosing;
		/// <summary>Whether the shrine's choosing is up.</summary>
		public bool Choosing => _choosing;
		private readonly Npc[] _figures = new Npc[4];
		private int _cursor;
		private int _leaveIn = -1;
		private bool _saveAfterOpening;
		// A row before the crystal, a little bowed towards it; the camera low and back, as a battle's is.
		private static readonly Vector3[] Places = { new Vector3(-27, 0, 16), new Vector3(-9, 0, 20), new Vector3(9, 0, 20), new Vector3(27, 0, 16) };
		private static readonly Vector3 CameraAt = new Vector3(0, 30, 108);
		private static readonly Vector3 CameraLook = new Vector3(0, 10, 10);
		private static readonly Vector3 Aside = new Vector3(0, 0, 130);   // the party's walker, behind the camera

		public Journey(FellowshipService service)
		{
			_service = service;
		}

		/// <summary>On the title: the entries. A journey saved under the profile adds Continue.</summary>
		public void OnGameStart()
		{
			Game.Title.AddEntry("Journey together", () => Game.Title.NewGame(0, ShrineMap, Aside, Profile));
			if (Game.Title.HasSave(Profile)) Game.Title.AddEntry("Continue the journey", () => Game.Title.Continue(Profile));
			// FELLOWSHIP_HERO=<0..3> with a --map start: the journey as that hero without the shrine, for test drives of two travellers together.
			int testHero = int.TryParse(Environment.GetEnvironmentVariable("FELLOWSHIP_HERO"), out int th) && th >= 0 && th < 4 ? th : -1;
			Game.Events.Subscribe<MapEntered>(e =>
			{
				if (testHero >= 0 && MyHero < 0 && e.Scene?.Name != ShrineMap)
				{
					Game.Party.Reset(testHero);
					Hold(testHero);
					_service.Claim(testHero);
					Game.Log("fellowship: FELLOWSHIP_HERO - journeying as " + HeroNames[testHero] + " from here");
					return;
				}
				if (e.Scene?.Name == ShrineMap) { Begin(); return; }
				// The save came back on its own map: the shrine first, to choose again with the levels in view - a few frames on, once the field's move state has begun (it clears what is queued as it starts).
				if (_resume && MyHero >= 0) { _resume = false; _toShrineIn = 8; Game.Screen.FadeOut(1); }
			});
			Game.Events.Subscribe<MapLeaving>(e => End());
			// The story would have put another player's hero into this party: they are told, and can come to where the story is.
			Game.Events.Subscribe<PartyJoinRefused>(e =>
			{
				if (MyHero < 0) return;
				Game.Screen.Notice("The story reaches " + HeroNames[e.HeroId] + " here - their player can join you.");
				_service.SendStoryReached(e.HeroId, Game.Field.Map);
			});
		}

		/// <summary>What the wire says of a traveller's hero.</summary>
		public void Heard(Traveller t)
		{
			if (t.Hero < 0 || t.Hero > 3) return;
			KnownHero k = Known[t.Hero];
			k.Level = t.Level; k.Job = t.Job ?? ""; k.Owner = t.Name; k.Seen = DateTime.UtcNow;
		}

		/// <summary>A traveller's hero's whole record came over.</summary>
		public void RecordHeard(int hero, string record, string from)
		{
			if (hero < 0 || hero > 3 || string.IsNullOrEmpty(record)) return;
			Known[hero].Record = record;
			Known[hero].Owner = from;
			Known[hero].Seen = DateTime.UtcNow;
		}

		/// <summary>Our own hero as we know them, for the chunk and the shrine.</summary>
		private void NoteMine()
		{
			if (MyHero < 0) return;
			PartyMember m = Game.Party.Member(MyHero);
			if (m == null) return;
			KnownHero k = Known[MyHero];
			k.Level = m.Level; k.Job = m.JobTitle ?? ""; k.Owner = _service.MyName; k.Seen = DateTime.UtcNow; k.Record = null;   // our own record is the save's
		}

		/// <summary>The hero's record to send now and then (E on the wire), or null.</summary>
		public string MyRecord() => MyHero >= 0 ? Game.Party.Export(MyHero) : null;

		/// <summary>The journey's hero is theirs alone from a save on (the chunk loaded).</summary>
		private void Hold(int hero)
		{
			MyHero = hero;
			if (hero < 0) return;
			Game.Party.Protagonist = hero;   // the story's lead is this hero (the save does not keep it)
			Game.Party.Restrict(new[] { hero });
		}

		/// <summary>Whether a traveller on the wire plays this hero.</summary>
		public bool Taken(int hero, out Traveller by)
		{
			by = _service.Travellers.FirstOrDefault(t => t.Hero == hero);
			return by != null;
		}

		// ---- the choosing ----

		private void Begin()
		{
			_choosing = true;
			_cursor = MyHero >= 0 ? MyHero : 0;
			_claiming = -1;
			_leaveIn = -1;
			NoteMine();
			Game.Input.Capture = true;   // the game sees no input; the choosing reads it
			Game.Hero.Teleport(Aside);
			Game.Hero.Freeze();
			for (int i = 0; i < 4; i++)
			{
				_figures[i]?.Remove();
				_figures[i] = Game.Npcs.Spawn(Model(i), Places[i], 0f);
			}
			Game.Camera.MoveTo(CameraAt);
			Game.Camera.LookAt(CameraLook);
			Game.Screen.FadeIn(20);
			Game.Log("fellowship: the choosing - who will you journey as?" + (MyHero >= 0 ? " (last time: " + HeroNames[MyHero] + ")" : ""));
			Pose();
		}

		private void End()
		{
			if (!_choosing) return;
			_choosing = false;
			for (int i = 0; i < 4; i++) { _figures[i]?.Remove(); _figures[i] = null; }
			Game.Input.Capture = false;
			Game.Camera.Follow();
		}

		/// <summary>The chosen figure greets; the rest stand.</summary>
		private void Pose()
		{
			for (int i = 0; i < 4; i++) if (i == _cursor) _figures[i]?.PlayMotion(1001);
		}

		public void OnUpdate()
		{
			if (Game.Time.Frame % 30 == 0) NoteField();
			// The opening has let go of the field: a save now, so a player who stops here comes back as their hero (no map change follows the fall).
			if (_saveAfterOpening && MyHero >= 0 && !_choosing && Game.Field.Map == OpeningMap && Game.Hero.Present && !Game.Field.Busy)
			{
				_saveAfterOpening = false;
				Game.Field.Autosave();
				Game.Log("fellowship: the opening is over - the journey is saved");
			}
			if (_toShrineIn > 0 && --_toShrineIn == 0)
			{
				if (Game.Field.Busy) { _toShrineIn = 10; return; }   // not yet: again in a moment
				Game.Field.Warp(ShrineMap, Aside);
				return;
			}
			if (!_choosing) return;
			if (_warpIn > 0) { if (--_warpIn == 0) WarpOn(); return; }
			if (_leaveIn > 0 && --_leaveIn == 0) { Leave(); return; }
			if (_claiming >= 0) { Claiming(); return; }
			if (Game.Input.Pressed(Pad.Left) || Game.Input.KeyPressed("Left")) { _cursor = (_cursor + 3) % 4; Pose(); }
			if (Game.Input.Pressed(Pad.Right) || Game.Input.KeyPressed("Right")) { _cursor = (_cursor + 1) % 4; Pose(); }
			if (Game.Input.Pressed(Pad.A) || Game.Input.KeyPressed("Z") || Game.Input.KeyPressed("Enter"))
			{
				if (Taken(_cursor, out Traveller by)) { Game.Screen.Notice(HeroNames[_cursor] + " journeys with " + by.Name + " already."); return; }
				// Continuing as another hero wants their record; until it has come over the wire they stay with whoever has it.
				if (MyHero >= 0 && _cursor != MyHero && Known[_cursor].Record == null)
				{
					Game.Screen.Notice(HeroNames[_cursor] + "'s record is not here - it comes over once you and their player are on the network together.");
					return;
				}
				_claiming = _cursor;
				_claimedAt = DateTime.UtcNow;
				_service.Claim(_claiming);   // out on the wire at once: everyone else sees the hero taken
				Game.Log("fellowship: claiming " + HeroNames[_claiming]);
			}
		}

		/// <summary>A moment on the wire: another traveller on the same hero with the lower id keeps it.</summary>
		private void Claiming()
		{
			Traveller other = _service.Travellers.FirstOrDefault(t => t.Hero == _claiming && string.CompareOrdinal(t.Id, _service.Id) < 0);
			if (other != null)
			{
				Game.Screen.Notice(other.Name + " chose " + HeroNames[_claiming] + " first.");
				_service.Claim(MyHero);
				_claiming = -1;
				return;
			}
			if ((DateTime.UtcNow - _claimedAt).TotalSeconds < 1.0) return;
			int hero = _claiming;
			_claiming = -1;
			if (MyHero < 0)
			{
				// A new journey: a fresh party of this hero; the opening is next.
				Hold(hero);
				Game.Party.Protagonist = hero;
				Game.Party.Reset(hero);
				Game.Party.Restrict(new[] { hero });
				_saveAfterOpening = true;
			}
			else if (hero != MyHero)
			{
				// The journey goes on as another hero: their last record on, ours let go (it stays known, and in the save chunk).
				string mine = Game.Party.Export(MyHero);
				Game.Party.RemoveMember(MyHero);
				if (mine != null) { Known[MyHero].Record = mine; Known[MyHero].Owner = ""; }
				Game.Party.Import(hero, Known[hero].Record);
				Game.Party.AddMember(hero);
				Known[hero].Record = null;
				Hold(hero);
			}
			else Hold(hero);
			Game.Screen.Notice("You journey as " + HeroNames[hero] + ".");
			Game.Log("fellowship: journeying as " + HeroNames[hero]);
			_figures[hero]?.PlayMotion(1001);
			_leaveIn = 60;   // a second on the shrine, then the story
		}

		private int _warpIn = -1;
		private int _toShrineIn = -1;

		/// <summary>Under black to the story: the fade first, the map change once it is dark, so the walker is never seen standing where the scene begins.</summary>
		private void Leave()
		{
			Game.Screen.FadeOut(15);
			_warpIn = 20;
		}

		private void WarpOn()
		{
			bool opening = _saveAfterOpening;
			End();
			Game.Hero.Unfreeze();
			if (opening || _savedMap == null) Game.Field.Warp(OpeningMap, Vector3.Zero);
			else Game.Field.Warp(_savedMap, _savedPos);
		}

		/// <summary>Over the shrine: the names under the figures, their standing, who has whom, the choice.</summary>
		public void Draw()
		{
			if (!_choosing) return;
			Color white = new Color(255, 255, 255), yellow = new Color(255, 230, 120), dim = new Color(150, 150, 165), red = new Color(255, 120, 110), green = new Color(150, 230, 150), shade = new Color(0, 0, 0, 160);
			string title = _claiming >= 0 ? "Claiming " + HeroNames[_claiming] + "..." : MyHero >= 0 ? "Who will you journey on as?" : "Who will you journey as?";
			float tw = Game.Draw.MeasureText(title, 16);
			Game.Draw.Rect(400 - tw / 2 - 12, 22, tw + 24, 30, shade);
			Game.Draw.Text(title, 400 - tw / 2, 28, white, 16);
			for (int i = 0; i < 4; i++)
			{
				Vector2? at = Game.Camera.WorldToScreen(new Vector3(Places[i].X, Places[i].Y + 21, Places[i].Z));
				if (!at.HasValue) continue;
				bool taken = Taken(i, out Traveller by);
				KnownHero k = Known[i];
				string name = HeroNames[i];
				string standing = k.Level > 0 ? "Lv. " + k.Level + (k.Job.Length > 0 ? "  " + k.Job : "") : MyHero >= 0 ? "not yet met" : "";
				string whose = taken ? (by.Id == _service.Id ? "you" : by.Name) : i == MyHero ? "you, last time" : k.Level > 0 && k.Owner.Length > 0 ? "with " + k.Owner : "";
				float w = Math.Max(Game.Draw.MeasureText(name, 14), Math.Max(Game.Draw.MeasureText(standing, 10), Game.Draw.MeasureText(whose, 10)));
				Game.Draw.Rect(at.Value.X - w / 2 - 8, at.Value.Y - 4, w + 16, standing.Length > 0 || whose.Length > 0 ? 56 : 24, shade);
				Game.Draw.Text(name, at.Value.X - Game.Draw.MeasureText(name, 14) / 2, at.Value.Y, taken ? dim : i == _cursor ? yellow : white, 14);
				if (standing.Length > 0) Game.Draw.Text(standing, at.Value.X - Game.Draw.MeasureText(standing, 10) / 2, at.Value.Y + 20, taken ? dim : white, 10);
				if (whose.Length > 0) Game.Draw.Text(whose, at.Value.X - Game.Draw.MeasureText(whose, 10) / 2, at.Value.Y + 33, taken ? red : i == MyHero ? green : dim, 10);
				if (i == _cursor) Game.Draw.Text("v", at.Value.X - 4, at.Value.Y - 20, taken ? red : yellow, 14);
			}
			List<string> here = _service.Travellers.Select(t => t.Name + (t.Hero >= 0 ? " (" + HeroNames[t.Hero] + ")" : "")).ToList();
			string foot = "Left / Right: choose     Z: journey as this hero";
			float fw = Game.Draw.MeasureText(foot, 12);
			Game.Draw.Rect(400 - fw / 2 - 12, 420, fw + 24, 26, shade);
			Game.Draw.Text(foot, 400 - fw / 2, 426, white, 12);
			string others = here.Count == 0 ? "No one else here yet - they can join any time." : "Here: " + string.Join(", ", here);
			Game.Draw.Text(others, 12, 456, dim, 10);
		}

		// ---- the save chunk (through the service) ----

		private string _fieldMap;
		private Vector3 _fieldPos;

		/// <summary>Where the hero last stood on the field (not a battle's stage, not the shrine), for the save.</summary>
		private void NoteField()
		{
			if (Game.Battle.InBattle || _choosing || !Game.Hero.Present || Game.Field.Map == null || Game.Field.Map == ShrineMap) return;
			_fieldMap = Game.Field.Map;
			_fieldPos = Game.Hero.Position;
		}

		public object Save()
		{
			NoteMine();
			NoteField();
			return new
			{
				Hero = MyHero,
				Map = _fieldMap ?? _savedMap,
				X = _fieldPos.X, Y = _fieldPos.Y, Z = _fieldPos.Z,
				Known = Known.Select(k => new { k.Level, k.Job, k.Owner, k.Record }).ToArray(),
			};
		}

		public void Load(int version, JsonElement data)
		{
			int hero = data.TryGetProperty("Hero", out JsonElement h) && h.ValueKind == JsonValueKind.Number ? h.GetInt32() : -1;
			_savedMap = data.TryGetProperty("Map", out JsonElement m) && m.ValueKind == JsonValueKind.String ? m.GetString() : null;
			float X(string n) => data.TryGetProperty(n, out JsonElement v) && v.ValueKind == JsonValueKind.Number ? (float)v.GetDouble() : 0f;
			_savedPos = new Vector3(X("X"), X("Y"), X("Z"));
			if (data.TryGetProperty("Known", out JsonElement known) && known.ValueKind == JsonValueKind.Array)
			{
				int i = 0;
				foreach (JsonElement k in known.EnumerateArray())
				{
					if (i > 3) break;
					Known[i].Level = k.TryGetProperty("Level", out JsonElement l) ? l.GetInt32() : 0;
					Known[i].Job = k.TryGetProperty("Job", out JsonElement j) ? j.GetString() ?? "" : "";
					Known[i].Owner = k.TryGetProperty("Owner", out JsonElement o) ? o.GetString() ?? "" : "";
					Known[i].Record = k.TryGetProperty("Record", out JsonElement r) && r.ValueKind == JsonValueKind.String ? r.GetString() : null;
					i++;
				}
			}
			Hold(hero);
			// The shrine is next, once the saved map has come up (the choosing shows the levels; your own hero takes you back here) -
			// for a save coming back from the title, not for the game re-reading its suspend save mid-play (after a battle), when the hero is already on the field.
			_resume = hero >= 0 && _savedMap != null && _savedMap != ShrineMap && !Game.Hero.Present;
			if (hero >= 0) Game.Log("fellowship: the journey continues as " + HeroNames[hero] + " from " + (_savedMap ?? "?"));
		}
	}
}
