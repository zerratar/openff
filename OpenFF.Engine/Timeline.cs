// A timeline: tracks of clips on a time axis, as Unity's Timeline lays a cutscene out, and
// the Cutscene component that plays one.
//
// A Timeline is data - the scene file holds it on a Cutscene's field, Crystal's timeline
// panel edits it - and the Cutscene is the player: each frame it moves the playhead, begins
// the clips it reaches, steps the ones it is inside (a walk's progress, a camera's glide, a
// fade's alpha) and ends the ones it passes, so a clip's final state always lands even on a
// dropped frame. A dialogue clip holds the playhead until the message is dismissed - the one
// thing a film timeline does not do and a game cutscene must. Skippable ones jump to the end
// on B, every remaining clip run to its final state in order.
//
// The tracks and what their clips do:
//   object   one of the scene's objects (Target: its path; "" is the Cutscene's own object) -
//            move (walk or glide to a point), turn (to a yaw or toward an object), motion (a
//            motion by index, a set bound first), show, fade (alpha), scale, clip (a glTF
//            animation on a Mesh)
//   hero     the same clips, on the hero (move walks with the hero's own animation)
//   camera   camera (glide to a position looking at a target), follow (back on the hero),
//            shake, zoom
//   dialogue say (holds until dismissed), ask (a yes/no; flags set either way)
//   screen   fadeOut, fadeIn, flash
//   audio    se, bgm, stopBgm
//   game     flags, wait, warp, battle, item, signal (a CutsceneSignal for code to hear)
//
// Every clip carries `args` by name - what it needs of: to, yaw, at, ease, index, loop, set,
// visible, alpha, scale, name, position, target, strength, speed, degrees, text, speaker,
// question, yes, no, white, archive, number, volume, fade, set, map, facing, formation,
// item, count. Crystal's timeline panel knows the same list (timeline.js); the two are kept
// in step by hand.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFF
{
	/// <summary>Tracks of clips on a time axis, in seconds: what a Cutscene plays. Edited in Crystal's timeline panel.</summary>
	public sealed class Timeline
	{
		[JsonPropertyName("tracks")]
		public List<Track> Tracks { get; set; } = new List<Track>();

		/// <summary>When the last clip ends, in seconds; 0 for an empty timeline.</summary>
		[JsonIgnore]
		public float Length => Tracks.Count == 0 ? 0f : Tracks.SelectMany(t => t.Clips).Select(c => c.End).DefaultIfEmpty(0f).Max();

		/// <summary>How many clips there are, over all tracks.</summary>
		[JsonIgnore]
		public int ClipCount => Tracks.Sum(t => t.Clips.Count);

		public override string ToString() => Tracks.Count + " track(s), " + ClipCount + " clip(s), " + Length.ToString("0.#", CultureInfo.InvariantCulture) + " s";
	}

	/// <summary>One row of a timeline: what it acts on, and its clips.</summary>
	public sealed class Track
	{
		/// <summary>The row's label in the editor.</summary>
		[JsonPropertyName("name")]
		public string Name { get; set; } = "";
		/// <summary>object, hero, camera, dialogue, screen, audio, game.</summary>
		[JsonPropertyName("kind")]
		public string Kind { get; set; } = "object";
		/// <summary>For an object track: the object's path in the scene; empty for the Cutscene's own object.</summary>
		[JsonPropertyName("target")]
		public string Target { get; set; } = "";
		/// <summary>A muted track plays nothing.</summary>
		[JsonPropertyName("muted")]
		public bool Muted { get; set; }
		[JsonPropertyName("clips")]
		public List<Clip> Clips { get; set; } = new List<Clip>();
	}

	/// <summary>One thing that happens on a track: its type, when, for how long, and its arguments by name.</summary>
	public sealed class Clip
	{
		[JsonPropertyName("type")]
		public string Type { get; set; } = "";
		/// <summary>Seconds from the timeline's start.</summary>
		[JsonPropertyName("start")]
		public float Start { get; set; }
		/// <summary>Seconds; 0 for something that happens at once.</summary>
		[JsonPropertyName("length")]
		public float Length { get; set; }
		[JsonPropertyName("args")]
		public Dictionary<string, JsonElement> Args { get; set; } = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

		[JsonIgnore]
		public float End => Start + Math.Max(0f, Length);

		public bool Has(string name) => Args != null && Args.ContainsKey(name);

		public float Num(string name, float fallback = 0f)
		{
			if (Args == null || !Args.TryGetValue(name, out JsonElement e)) return fallback;
			if (e.ValueKind == JsonValueKind.Number) return (float)e.GetDouble();
			if (e.ValueKind == JsonValueKind.String && float.TryParse(e.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float f)) return f;
			return fallback;
		}

		public int Int(string name, int fallback = 0) => (int)Math.Round(Num(name, fallback));

		public bool Bool(string name, bool fallback = false)
		{
			if (Args == null || !Args.TryGetValue(name, out JsonElement e)) return fallback;
			if (e.ValueKind == JsonValueKind.True) return true;
			if (e.ValueKind == JsonValueKind.False) return false;
			if (e.ValueKind == JsonValueKind.Number) return e.GetDouble() != 0;
			if (e.ValueKind == JsonValueKind.String && bool.TryParse(e.GetString(), out bool b)) return b;
			return fallback;
		}

		public string Str(string name, string fallback = "")
		{
			if (Args == null || !Args.TryGetValue(name, out JsonElement e)) return fallback;
			return e.ValueKind == JsonValueKind.String ? e.GetString() : e.ValueKind == JsonValueKind.Null ? fallback : e.ToString();
		}

		/// <summary>A point as {x, y, z} or [x, y, z]; null when the clip has none.</summary>
		public Vector3? Vec(string name)
		{
			if (Args == null || !Args.TryGetValue(name, out JsonElement e)) return null;
			if (e.ValueKind == JsonValueKind.Array)
			{
				float[] a = e.EnumerateArray().Select(v => v.ValueKind == JsonValueKind.Number ? (float)v.GetDouble() : 0f).ToArray();
				return new Vector3(a.Length > 0 ? a[0] : 0, a.Length > 1 ? a[1] : 0, a.Length > 2 ? a[2] : 0);
			}
			if (e.ValueKind == JsonValueKind.Object)
			{
				float Part(string p) => e.TryGetProperty(p, out JsonElement v) && v.ValueKind == JsonValueKind.Number ? (float)v.GetDouble() : 0f;
				return new Vector3(Part("x"), Part("y"), Part("z"));
			}
			return null;
		}

		public override string ToString() => Type + " @" + Start.ToString("0.##", CultureInfo.InvariantCulture) + (Length > 0 ? " for " + Length.ToString("0.##", CultureInfo.InvariantCulture) : "");
	}

	/// <summary>How a Cutscene begins.</summary>
	public enum CutsceneStart
	{
		/// <summary>On talking to the object (A facing its model; A within Radius, or walking in with OnWalkIn, without one).</summary>
		OnTalk = 0,
		/// <summary>The moment the map is entered (with the flags in If holding).</summary>
		OnEnterMap = 1,
		/// <summary>Only from code (Play()) or another cutscene's signal.</summary>
		Manual = 2,
	}

	/// <summary>A game clip's "signal", for code: Game.Events.Subscribe&lt;CutsceneSignal&gt;.</summary>
	public sealed class CutsceneSignal
	{
		public string Name { get; }
		public Cutscene Cutscene { get; }
		public CutsceneSignal(string name, Cutscene cutscene) { Name = name; Cutscene = cutscene; }
	}

	/// <summary>
	/// Plays a Timeline: characters walk and turn, the camera glides, lines are said, the
	/// screen fades, flags are set - laid out on tracks in Crystal's timeline panel. Starts on
	/// talking to the object, on entering the map, or from code (Play). The hero is frozen
	/// while it plays and the camera handed back at the end; a skippable one jumps to its end
	/// on B, every remaining clip run to its final state.
	/// </summary>
	public sealed class Cutscene : Interactable
	{
		/// <summary>The tracks and clips.</summary>
		[Header("The cutscene")]
		[Tooltip("The tracks and clips - edit them in the timeline panel")]
		public Timeline Timeline = new Timeline();
		/// <summary>How it begins.</summary>
		[Tooltip("On talking to the object (or walking in), the moment the map is entered, or only from code")]
		public CutsceneStart Begins = CutsceneStart.OnTalk;
		/// <summary>Flags that must hold for it to play ("0:14 !0:11"); empty for always.</summary>
		[FlagField]
		[Tooltip("Flags that must hold for it to play; empty for always")]
		public string If = "";
		/// <summary>Flags set when it ends ("0:14 !0:11").</summary>
		[FlagField]
		[Tooltip("Flags set when it ends")]
		public string Then = "";
		/// <summary>Plays once per save; remembered with the scene's other one-time events.</summary>
		[Tooltip("Plays once per save (remembered in the save)")]
		public bool Once;
		/// <summary>The hero stands still and the pad is ignored while it plays.</summary>
		[Header("While it plays")]
		[Tooltip("The hero stands still and the pad is ignored while it plays")]
		public bool FreezeHero = true;
		/// <summary>B skips to the end.</summary>
		[Tooltip("B skips to the end - every remaining clip runs to its final state")]
		public bool Skippable = true;
		/// <summary>The camera goes back to following the hero when it ends (after any camera clip).</summary>
		[Tooltip("The camera goes back to following the hero when it ends")]
		public bool CameraBack = true;

		/// <summary>The object path of a Cutscene the host wants played the moment its map is up (Crystal's Play in OpenFF); null for none.</summary>
		public static string AutoPlay;

		/// <summary>The cutscene playing right now, or null: Interactables stand aside while one plays, so A on a line does not also talk to a villager.</summary>
		public static Cutscene Active { get; private set; }

		/// <summary>Whether it is playing right now.</summary>
		public bool Playing { get; private set; }
		/// <summary>The playhead, in seconds.</summary>
		public float Time { get; private set; }
		/// <summary>The clip playing (or last begun) on each track, for the debug overlay and code.</summary>
		public event Action<Cutscene> Finished;

		private readonly HashSet<Clip> _begun = new HashSet<Clip>();
		private readonly HashSet<Clip> _ended = new HashSet<Clip>();
		private readonly Dictionary<Clip, object> _state = new Dictionary<Clip, object>();
		private Func<bool> _hold;
		private bool _froze;
		private bool _cameraMoved;
		private bool _autoPlayed;

		private string Key => "cutscene:" + (Game.Field.Map ?? "?") + "/" + (GetComponent<MapObject>()?.Path ?? GameObject?.Name ?? "?");

		protected override bool Applies() => Begins == CutsceneStart.OnTalk && !Playing && FlagsHold(If) && !(Once && SceneMemory.Instance.Has(Key));

		protected override void Activate() => Play();

		protected override void Update()
		{
			if (Playing)
			{
				Advance();
				return;
			}
			base.Update();
			if (!_autoPlayed && AutoPlay != null && GameObject != null && string.Equals(AutoPlay, GetComponent<MapObject>()?.Path ?? "", StringComparison.OrdinalIgnoreCase))
			{
				_autoPlayed = true;
				AutoPlay = null;
				Play(force: true);
				return;
			}
			if (Begins == CutsceneStart.OnEnterMap && !_autoPlayed)
			{
				_autoPlayed = true;
				if (FlagsHold(If) && !(Once && SceneMemory.Instance.Has(Key))) Play();
			}
		}

		/// <summary>Starts it from the top, if it is not playing (force: even when If or Once say not to).</summary>
		public void Play(bool force = false)
		{
			if (Playing) return;
			if (!force && (!FlagsHold(If) || (Once && SceneMemory.Instance.Has(Key)))) return;
			Playing = true;
			Time = 0f;
			_begun.Clear();
			_ended.Clear();
			_state.Clear();
			_hold = null;
			_cameraMoved = false;
			if (FreezeHero && Game.Hero.Present) { Game.Hero.Freeze(keepInput: true); _froze = true; }
			Active = this;
			Game.Log("cutscene: " + Name + " plays (" + Timeline + ")");
			Game.Events.Publish(new Events.CutsceneStarted());
			if (Timeline.ClipCount == 0) Finish();
		}

		/// <summary>Ends it where it is: nothing more happens, the hero and camera are given back.</summary>
		public void Stop()
		{
			if (!Playing) return;
			Finish();
		}

		/// <summary>Jumps to the end: every clip not yet ended runs to its final state, in order.</summary>
		public void Skip()
		{
			if (!Playing) return;
			if (Game.Dialogue.IsOpen) Game.Dialogue.Close();
			_hold = null;
			Time = Timeline.Length;
			foreach (Clip clip in Ordered())
			{
				if (_ended.Contains(clip)) continue;
				if (!_begun.Contains(clip)) { _begun.Add(clip); Game.Guard(Name + "." + clip.Type, () => Begin(clip)); }
				if (clip.Type == "say" || clip.Type == "ask") { _ended.Add(clip); if (Game.Dialogue.IsOpen) Game.Dialogue.Close(); continue; }
				Game.Guard(Name + "." + clip.Type, () => { Step(clip, 1f); End(clip); });
				_ended.Add(clip);
			}
			Finish();
		}

		private IEnumerable<Clip> Ordered()
		{
			return Timeline.Tracks.Where(t => !t.Muted).SelectMany(t => t.Clips.Select(c => (track: t, clip: c))).OrderBy(p => p.clip.Start).ThenBy(p => p.clip.End).Select(p => p.clip);
		}

		private Track TrackOf(Clip clip) => Timeline.Tracks.FirstOrDefault(t => t.Clips.Contains(clip));

		private void Advance()
		{
			if (Skippable && Game.Input.Pressed(Pad.B))
			{
				Skip();
				return;
			}
			if (_hold != null)
			{
				if (!_hold()) return;
				_hold = null;
			}
			float dt = (float)Math.Min(0.1, Math.Max(0.0, Game.Time.Delta));
			if (dt <= 0f) dt = 1f / 30f;
			Time += dt;
			foreach (Track track in Timeline.Tracks)
			{
				if (track.Muted) continue;
				foreach (Clip clip in track.Clips)
				{
					if (_ended.Contains(clip)) continue;
					if (Time < clip.Start) continue;
					if (!_begun.Contains(clip))
					{
						_begun.Add(clip);
						Game.Guard(Name + "." + clip.Type, () => Begin(clip));
						if (_hold != null && !_hold()) return;   // a line said: the playhead waits here
					}
					float progress = clip.Length > 0f ? Math.Min(1f, (Time - clip.Start) / clip.Length) : 1f;
					Game.Guard(Name + "." + clip.Type, () => Step(clip, progress));
					if (Time >= clip.End)
					{
						_ended.Add(clip);
						Game.Guard(Name + "." + clip.Type, () => End(clip));
					}
				}
			}
			if (Time >= Timeline.Length && _hold == null && _ended.Count >= Ordered().Count()) Finish();
		}

		private void Finish()
		{
			Playing = false;
			_hold = null;
			if (Active == this) Active = null;
			if (_froze) { Game.Hero.Unfreeze(); _froze = false; }
			if (_cameraMoved && CameraBack) Game.Camera.Follow();
			SetFlags(Then);
			if (Once) SceneMemory.Instance.Mark(Key);
			Game.Log("cutscene: " + Name + " ends at " + Time.ToString("0.##", CultureInfo.InvariantCulture) + " s");
			Game.Events.Publish(new Events.CutsceneEnded());
			Action<Cutscene> finished = Finished;
			if (finished != null) Game.Guard(Name + ".Finished", () => finished(this));
		}

		public override IEnumerable<string> DebugLines()
		{
			if (!Playing) return null;
			return new[] { "cutscene " + Time.ToString("0.0", CultureInfo.InvariantCulture) + " / " + Timeline.Length.ToString("0.0", CultureInfo.InvariantCulture) + " s" + (_hold != null ? " (holding)" : "") };
		}

		// ------------------------------------------------------------------ the actors

		/// <summary>What an object or hero track acts on: the hero, a character on the map, or a plain object's Transform.</summary>
		private sealed class Actor
		{
			public bool IsHero;
			public GameObject Object;
			public Npc Npc;

			public bool Valid => IsHero ? Game.Hero.Present : Object != null;

			public Vector3 Position
			{
				get => IsHero ? Game.Hero.Position : Npc != null && Npc.Alive ? Npc.Position : Object.Transform.WorldPosition;
				set
				{
					if (IsHero) { Game.Hero.Teleport(value); return; }
					if (Npc != null && Npc.Alive) Npc.Teleport(value);
					Object.Transform.WorldPosition = value;
				}
			}

			public float Yaw
			{
				get => IsHero ? Game.Hero.Yaw : Npc != null && Npc.Alive ? Npc.Yaw : Object.Transform.WorldYaw;
				set
				{
					if (IsHero) { Game.Hero.Face(value); return; }
					if (Npc != null && Npc.Alive) Npc.Face(value);
					Object.Transform.WorldYaw = value;
				}
			}

			/// <summary>Whether it walks on its own (the game's walker, with the walking animation).</summary>
			public bool Walks => IsHero || (Npc != null && Npc.Alive);

			public void Walk(Vector3 to, int frames)
			{
				if (IsHero) Game.Hero.MoveTo(to, frames);
				else Npc.MoveTo(to, frames);
			}

			public void Motion(int index, bool loop, string set)
			{
				if (IsHero)
				{
					if (!string.IsNullOrWhiteSpace(set)) Game.Hero.BindMotions(set);
					Game.Hero.PlayMotion(index, loop);
				}
				else if (Npc != null && Npc.Alive)
				{
					if (!string.IsNullOrWhiteSpace(set)) Npc.BindMotions(set);
					Npc.PlayMotion(index, loop);
				}
			}

			public bool Visible
			{
				set
				{
					if (IsHero) return;
					if (Npc != null && Npc.Alive) Npc.Hidden = !value;
					else if (Object != null) Object.Active = value;
				}
			}

			public int Alpha
			{
				get => Npc != null && Npc.Alive ? Npc.Alpha : 100;
				set { if (Npc != null && Npc.Alive) Npc.Alpha = Math.Max(0, Math.Min(100, value)); }
			}

			public float Scale
			{
				get => IsHero ? 1f : Npc != null && Npc.Alive ? Npc.Scale : Object.Transform.WorldScale;
				set
				{
					if (IsHero) return;
					if (Npc != null && Npc.Alive) Npc.Scale = value;
					Object.Transform.WorldScale = value;
				}
			}
		}

		private Actor ActorOf(Track track)
		{
			if (track == null) return null;
			if (track.Kind == "hero" || string.Equals(track.Target, "@hero", StringComparison.OrdinalIgnoreCase)) return new Actor { IsHero = true };
			GameObject o = string.IsNullOrWhiteSpace(track.Target) ? GameObject : SceneObjects.Find(track.Target);
			if (o == null)
			{
				Game.Warn("cutscene " + Name + ": no object '" + track.Target + "' on this map");
				return null;
			}
			return new Actor { Object = o, Npc = o.GetComponent<MapObject>()?.Npc };
		}

		// ------------------------------------------------------------------ the clips

		private sealed class Tween
		{
			public Vector3 From, To;
			public float FromYaw, ToYaw;
			public float FromScale, ToScale;
			public int FromAlpha, ToAlpha;
			public Vector3 FromTarget, ToTarget;
			public Actor Actor;
			public bool Walking;
		}

		private static float Eased(string ease, float p)
		{
			p = Math.Max(0f, Math.Min(1f, p));
			switch ((ease ?? "").ToLowerInvariant())
			{
				case "smooth": return p * p * (3f - 2f * p);
				case "in": return p * p;
				case "out": return 1f - (1f - p) * (1f - p);
				default: return p;
			}
		}

		private static float LerpYaw(float from, float to, float p)
		{
			float d = ((to - from) % 360f + 540f) % 360f - 180f;
			return from + d * p;
		}

		private void Begin(Clip clip)
		{
			Track track = TrackOf(clip);
			string kind = track?.Kind ?? "object";
			switch (clip.Type)
			{
				case "move":
				{
					Actor actor = ActorOf(track);
					if (actor == null || !actor.Valid) return;
					Vector3 to = clip.Vec("to") ?? actor.Position;
					Tween t = new Tween { Actor = actor, From = actor.Position, To = to };
					int frames = (int)Math.Round(clip.Length * 30f);
					if (actor.Walks && clip.Bool("walk", true) && frames > 0)
					{
						t.Walking = true;
						actor.Walk(to, frames);
					}
					_state[clip] = t;
					break;
				}
				case "turn":
				{
					Actor actor = ActorOf(track);
					if (actor == null || !actor.Valid) return;
					float toYaw = clip.Num("yaw", actor.Yaw);
					string at = clip.Str("at");
					if (!string.IsNullOrWhiteSpace(at))
					{
						Vector3? point = LookPoint(at);
						if (point.HasValue) toYaw = (point.Value - actor.Position).Flat.Yaw;
					}
					_state[clip] = new Tween { Actor = actor, FromYaw = actor.Yaw, ToYaw = toYaw };
					break;
				}
				case "motion":
				{
					Actor actor = ActorOf(track);
					if (actor == null || !actor.Valid) return;
					actor.Motion(clip.Int("index", 1001), clip.Bool("loop", false), clip.Str("set"));
					break;
				}
				case "show":
				{
					Actor actor = ActorOf(track);
					if (actor == null || !actor.Valid) return;
					actor.Visible = clip.Bool("visible", true);
					break;
				}
				case "fade":
				{
					Actor actor = ActorOf(track);
					if (actor == null || !actor.Valid) return;
					_state[clip] = new Tween { Actor = actor, FromAlpha = actor.Alpha, ToAlpha = clip.Int("alpha", 100) };
					break;
				}
				case "scale":
				{
					Actor actor = ActorOf(track);
					if (actor == null || !actor.Valid) return;
					_state[clip] = new Tween { Actor = actor, FromScale = actor.Scale, ToScale = clip.Num("scale", 1f) };
					break;
				}
				case "clip":
				{
					Actor actor = ActorOf(track);
					Mesh mesh = actor?.Object?.GetComponent<Mesh>();
					if (mesh == null) { Game.Warn("cutscene " + Name + ": a clip needs a Mesh on '" + track?.Target + "'"); return; }
					mesh.Clip = clip.Str("name");
					if (clip.Has("speed")) mesh.Speed = clip.Num("speed", 1f);
					// The same clip at a new speed is a change the Mesh would not see on its own.
					if (mesh.Handle != null && mesh.Handle.Problem == null)
					{
						if (string.IsNullOrWhiteSpace(mesh.Clip)) mesh.Handle.Stop();
						else mesh.Handle.Play(mesh.Clip, true, mesh.Speed);
					}
					break;
				}
				case "camera":
				{
					Vector3 from = clip.Vec("from") ?? Game.Camera.Position;
					Vector3 fromTarget = clip.Vec("fromTarget") ?? Game.Camera.Target;
					Tween t = new Tween { From = from, To = clip.Vec("position") ?? from, FromTarget = fromTarget, ToTarget = clip.Vec("target") ?? fromTarget };
					_state[clip] = t;
					_cameraMoved = true;
					Game.Camera.MoveTo(t.From);
					Game.Camera.LookAt(t.FromTarget);
					break;
				}
				case "follow":
					Game.Camera.Follow();
					_cameraMoved = false;
					break;
				case "shake":
					Game.Camera.Shake(Math.Max(1, (int)Math.Round(clip.Length * 30f)), clip.Num("strength", 1f), Math.Max(1, clip.Int("speed", 2)));
					break;
				case "zoom":
					Game.Camera.Zoom(clip.Int("degrees", 0));
					_cameraMoved = true;
					break;
				case "say":
				{
					string text = clip.Str("text"), speaker = clip.Str("speaker");
					Game.Dialogue.Say(text, string.IsNullOrWhiteSpace(speaker) ? null : speaker);
					if (clip.Bool("hold", true)) _hold = () => !Game.Dialogue.IsOpen;
					break;
				}
				case "ask":
				{
					string yes = clip.Str("yes"), no = clip.Str("no");
					bool answered = false;
					Game.Dialogue.Ask(clip.Str("question"), a => { answered = true; SetFlags(a ? yes : no); });
					_hold = () => answered && !Game.Dialogue.IsOpen;
					break;
				}
				case "fadeOut":
					Game.Screen.FadeOut(Math.Max(1, (int)Math.Round(clip.Length * 30f)), clip.Bool("white", false));
					break;
				case "fadeIn":
					Game.Screen.FadeIn(Math.Max(1, (int)Math.Round(clip.Length * 30f)));
					break;
				case "flash":
					Game.Screen.Flash(clip.Bool("white", true) ? new Color(255, 255, 255, 255) : new Color(255, 0, 0, 255), Math.Max(1, (int)Math.Round(Math.Max(clip.Length, 0.25f) * 30f)), Math.Max(1, clip.Int("interval", 2)));
					break;
				case "se":
					Game.Audio.PlaySe(clip.Int("archive", 1), clip.Int("number", 0), clip.Int("volume", 127));
					break;
				case "bgm":
					Game.Audio.PlayBgm(clip.Int("number", 0), clip.Int("volume", 127), clip.Int("fade", 0));
					break;
				case "stopBgm":
					Game.Audio.StopBgm(clip.Int("fade", 15));
					break;
				case "flags":
					SetFlags(clip.Str("set"));
					break;
				case "wait":
					break;
				case "warp":
				{
					string map = clip.Str("map");
					if (string.IsNullOrWhiteSpace(map)) return;
					Vector3 at = clip.Vec("position") ?? Vector3.Zero;
					Finish();
					Game.Field.Warp(map, at, clip.Int("facing", 0));
					break;
				}
				case "battle":
					Finish();
					Game.Battle.Start(clip.Int("formation", 0), clip.Int("map", 0));
					break;
				case "item":
					Game.Party.AddItem(clip.Int("item", 0), Math.Max(1, clip.Int("count", 1)));
					break;
				case "signal":
					Game.Events.Publish(new CutsceneSignal(clip.Str("name"), this));
					break;
				default:
					Game.Warn("cutscene " + Name + ": no clip type '" + clip.Type + "' on a " + kind + " track");
					break;
			}
		}

		private void Step(Clip clip, float progress)
		{
			if (!_state.TryGetValue(clip, out object state) || !(state is Tween t)) return;
			float p = Eased(clip.Str("ease", "smooth"), progress);
			switch (clip.Type)
			{
				case "move":
					if (t.Walking) return;   // the game's walker carries it
					if (!t.Actor.Valid) return;
					t.Actor.Position = Vector3.Lerp(t.From, t.To, p);
					if (clip.Bool("face", true) && Vector3.FlatDistance(t.From, t.To) > 0.01f) t.Actor.Yaw = (t.To - t.From).Flat.Yaw;
					break;
				case "turn":
					if (t.Actor.Valid) t.Actor.Yaw = LerpYaw(t.FromYaw, t.ToYaw, p);
					break;
				case "fade":
					if (t.Actor.Valid) t.Actor.Alpha = (int)Math.Round(t.FromAlpha + (t.ToAlpha - t.FromAlpha) * p);
					break;
				case "scale":
					if (t.Actor.Valid) t.Actor.Scale = t.FromScale + (t.ToScale - t.FromScale) * p;
					break;
				case "camera":
					Game.Camera.MoveTo(Vector3.Lerp(t.From, t.To, p));
					Game.Camera.LookAt(Vector3.Lerp(t.FromTarget, t.ToTarget, p));
					break;
			}
		}

		private void End(Clip clip)
		{
			if (!_state.TryGetValue(clip, out object state) || !(state is Tween t)) return;
			switch (clip.Type)
			{
				case "move":
					if (t.Actor.Valid)
					{
						if (t.Walking) { if (t.Actor.IsHero) Game.Hero.Stop(); else t.Actor.Npc?.Stop(); }
						t.Actor.Position = t.To;
					}
					break;
			}
			_state.Remove(clip);
		}

		/// <summary>A point to face: "@hero", an object's path, or "x,y,z".</summary>
		private Vector3? LookPoint(string at)
		{
			if (string.Equals(at, "@hero", StringComparison.OrdinalIgnoreCase)) return Game.Hero.Present ? Game.Hero.Position : (Vector3?)null;
			string[] parts = at.Split(',');
			if (parts.Length == 3 && float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
				return new Vector3(x, y, z);
			GameObject o = SceneObjects.Find(at);
			if (o == null) return null;
			Npc npc = o.GetComponent<MapObject>()?.Npc;
			return npc != null && npc.Alive ? npc.Position : o.Transform.WorldPosition;
		}
	}
}
