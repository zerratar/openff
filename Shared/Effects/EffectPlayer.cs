// The new effect format played a game step at a time (Docs/Effects-Plan.md, format 1): the client's
// runtime, and Crystal's check of it. It is the twin of Crystal.Editor/wwwroot/effects.js's
// makeEffectPlayer - the same steps in the same order, the same random numbers drawn in the same
// order from the same generator (mulberry32), so a definition, a seed and a frame come to the same
// particles in both; Tools/EffectCases holds the cases both are checked against. A change to one
// is a change to the other.
//
// What it plays: emitters born on the timeline at their frame, riding their path; groups of
// particles burst every interval for the duration; a particle moves by its speed (turned by its
// spread) and gravity, orbits or gathers to the emitter, and takes its colour, scale and texture
// frame from keys by its age. Models (mesh tracks) are only placed and counted here - the one who
// draws them plays their motion. Units: frames (30 a second), world units, colours 0-255.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace OpenFF.Effects
{
	/// <summary>One quad to draw: where, how big (half its width and height), its colour 0-255, its texture frame, whose track, and which particle (null for an after-image).</summary>
	public struct EffectQuad
	{
		public double X, Y, Z;
		public double HalfWidth, HalfHeight;
		public double R, G, B, A;
		public int Cell;
		/// <summary>The quad's turn in the screen's plane, degrees (its spin).</summary>
		public double Roll;
		public EffectTrack Track;
		public object Particle;
		public int Trail;
	}

	/// <summary>A model of the effect's to draw: its track, where it is, and the frame of its motion (0 its first).</summary>
	public struct EffectModel
	{
		public EffectTrack Track;
		public double X, Y, Z;
		public int Frame;
		/// <summary>Its scale (x, y, z) and yaw (degrees) now - the track's own, or its curves' at its step.</summary>
		public double[] Scale;
		public double Yaw;
		public object Instance;
	}

	public sealed class EffectTexture
	{
		public string Image;
		public int Width = 1, Height = 1, Columns;
		public int[] Cell;
		public double[][] Frames;
	}

	/// <summary>A track of a definition, read once.</summary>
	public sealed class EffectTrack
	{
		public string Type, Name, Anchor = "target", Space, Blend = "alpha", Model;
		/// <summary>render.tint: "multiply" (the game's - the picture times the colour) or "recolour" (the picture's brightness in the colour).</summary>
		public string Tint = "multiply";
		public int Start, Stop = -1, Life = 1;
		public int? Id;
		public double[] Offset;
		public JsonObject Path;
		// emission
		public int Duration, Interval, Count, Bursts;
		public bool Loop;
		// the particle
		public double[] Box, Size;
		public double[] SpeedDirection, SpeedValue, Spread;
		public double[] GravityDirection, GravityValue;
		public bool Orbit; public double OrbitRadius, OrbitGrow, OrbitTurn;
		public bool Gather; public double GatherSpeed, GatherAccel; public double[] GatherSwirl;
		public int TrailCount; public double[] TrailColour;
		public double[][] Colour, Scale;
		public bool ColourSmooth, ScaleSmooth;
		/// <summary>Speed over life: keys [age, k] - how much of its speed a step carries a particle.</summary>
		public double[][] SpeedOverLife; public bool SpeedSmooth;
		/// <summary>Spin: the quad's angle and its turn a frame, degrees (ranges).</summary>
		public bool Spin; public double[] SpinAngle, SpinSpeed;
		/// <summary>Gravity over life, spin over life: keys [age, k], how much of its pull, of its spin, reaches a particle.</summary>
		public double[][] GravityOverLife, SpinOverLife; public bool GravitySmooth, SpinSmooth;
		/// <summary>Count over time: keys [frame of the emitter, count] - how many a burst then makes.</summary>
		public double[][] CountOverTime; public bool CountSmooth;
		/// <summary>Size over time, speed over time: keys [frame of the emitter, k] - what a particle is born with, times k.</summary>
		public double[][] SizeOverTime, SpeedOverTime; public bool SizeTimeSmooth, SpeedTimeSmooth;
		/// <summary>An orbit's grow and turn as curves over life (else the numbers).</summary>
		public double[][] OrbitGrowKeys, OrbitTurnKeys; public bool OrbitGrowSmooth, OrbitTurnSmooth;
		/// <summary>A mesh's scale and yaw as curves over its steps (else the numbers).</summary>
		public double[][] MeshScaleKeys, MeshYawKeys; public bool MeshScaleSmooth, MeshYawSmooth; public double MeshYaw;
		public EffectTexture Texture;
		public double[] MeshScale;
		public bool MeshLoop;
		// the other tracks: a sound (a group and a number of the game's), a flash, a shake
		public JsonObject Raw;
	}

	/// <summary>A definition, read into tracks once; the player reads this, not the JSON.</summary>
	public sealed class EffectDefinition
	{
		public int Length;
		public bool Loop;
		public string From;
		public readonly List<EffectTrack> Tracks = new List<EffectTrack>();

		public static EffectDefinition Read(JsonObject def)
		{
			EffectDefinition d = new EffectDefinition
			{
				Length = Int(def["length"]),
				Loop = Bool(def["loop"]),
				From = Text(def["from"]),
			};
			if (def["tracks"] is JsonArray tracks)
			{
				foreach (JsonNode n in tracks)
				{
					if (n is not JsonObject t) continue;
					EffectTrack k = new EffectTrack
					{
						Type = Text(t["type"]) ?? "emitter",
						Name = Text(t["name"]),
						Start = Int(t["start"]),
						Stop = t["stop"] != null ? Int(t["stop"]) : -1,
						Id = t["id"] != null ? Int(t["id"]) : (int?)null,
						Anchor = Text(t["anchor"]) ?? "target",
						Offset = Vector(t["offset"]),
						Path = t["path"] as JsonObject,
						Space = Text(t["space"]),
						Life = t["life"] != null ? Int(t["life"]) : 1,
						Raw = t,
					};
					if (t["emission"] is JsonObject em)
					{
						k.Duration = Int(em["duration"]); k.Interval = Int(em["interval"]); k.Count = Int(em["count"]); k.Bursts = Int(em["bursts"]); k.Loop = Bool(em["loop"]);
					}
					if (t["shape"] is JsonObject shape) k.Box = Vector(shape["box"]);
					k.Size = RangeOf(t["size"]);
					if (t["speed"] is JsonObject sp) { k.SpeedDirection = Vector(sp["direction"]) ?? new double[] { 0, 1, 0 }; k.SpeedValue = RangeOf(sp["value"]); k.Spread = Vector(sp["spread"]); }
					if (t["gravity"] is JsonObject g) { k.GravityDirection = Vector(g["direction"]) ?? new double[] { 0, -1, 0 }; k.GravityValue = RangeOf(g["value"]); }
					if (t["orbit"] is JsonObject o)
					{
						k.Orbit = true; k.OrbitRadius = Num(o["radius"]); k.OrbitGrow = Num(o["grow"]); k.OrbitTurn = Num(o["turn"]);
						if (o["grow"] is JsonObject || o["grow"] is JsonArray) { k.OrbitGrowKeys = Keys(o["grow"]); k.OrbitGrowSmooth = Smooth(o["grow"]); }
						if (o["turn"] is JsonObject || o["turn"] is JsonArray) { k.OrbitTurnKeys = Keys(o["turn"]); k.OrbitTurnSmooth = Smooth(o["turn"]); }
					}
					k.SizeOverTime = Keys(t["sizeOverTime"]); k.SizeTimeSmooth = Smooth(t["sizeOverTime"]);
					k.SpeedOverTime = Keys(t["speedOverTime"]); k.SpeedTimeSmooth = Smooth(t["speedOverTime"]);
					if (t["gather"] is JsonObject ga) { k.Gather = true; k.GatherSpeed = Num(ga["speed"]); k.GatherAccel = Num(ga["accel"]); k.GatherSwirl = Vector(ga["swirl"]) ?? new double[3]; }
					if (t["trail"] is JsonObject tr) { k.TrailCount = Int(tr["count"]); k.TrailColour = Numbers(tr["colour"]) ?? new double[4]; }
					k.Colour = Keys(t["colour"]); k.ColourSmooth = Smooth(t["colour"]);
					k.Scale = Keys(t["scale"]); k.ScaleSmooth = Smooth(t["scale"]);
					k.SpeedOverLife = Keys(t["speedOverLife"]); k.SpeedSmooth = Smooth(t["speedOverLife"]);
					k.GravityOverLife = Keys(t["gravityOverLife"]); k.GravitySmooth = Smooth(t["gravityOverLife"]);
					k.SpinOverLife = Keys(t["spinOverLife"]); k.SpinSmooth = Smooth(t["spinOverLife"]);
					k.CountOverTime = Keys(t["countOverTime"]); k.CountSmooth = Smooth(t["countOverTime"]);
					if (t["spin"] is JsonObject spin) { k.Spin = true; k.SpinAngle = RangeOf(spin["angle"]); k.SpinSpeed = RangeOf(spin["speed"]); }
					if (t["texture"] is JsonObject tex)
					{
						k.Texture = new EffectTexture
						{
							Image = Text(tex["image"]),
							Width = tex["width"] != null ? Math.Max(1, Int(tex["width"])) : 1,
							Height = tex["height"] != null ? Math.Max(1, Int(tex["height"])) : 1,
							Columns = Int(tex["columns"]),
							Cell = Numbers(tex["cell"])?.Select(v => (int)v).ToArray(),
							Frames = Keys(tex["frames"]),
						};
					}
					if (t["render"] is JsonObject r) { k.Blend = Text(r["blend"]) ?? "alpha"; k.Tint = Text(r["tint"]) ?? "multiply"; }
					k.Model = Text(t["model"]);
					if (k.Type == "mesh")
					{
						k.MeshScale = t["scale"] is JsonObject ? null : Numbers(t["scale"]); k.MeshLoop = Bool(t["loop"]); k.Scale = null; k.ScaleSmooth = false;
						if (t["scale"] is JsonObject) { k.MeshScaleKeys = Keys(t["scale"]); k.MeshScaleSmooth = Smooth(t["scale"]); }
						if (t["yaw"] is JsonObject || t["yaw"] is JsonArray) { k.MeshYawKeys = Keys(t["yaw"]); k.MeshYawSmooth = Smooth(t["yaw"]); } else k.MeshYaw = Num(t["yaw"]);
					}
					d.Tracks.Add(k);
				}
			}
			return d;
		}

		internal static string Text(JsonNode n) => n is JsonValue v && v.TryGetValue(out string s) ? s : null;
		internal static bool Bool(JsonNode n) => n is JsonValue v && v.TryGetValue(out bool b) && b;
		internal static double Num(JsonNode n) => n is JsonValue v ? (v.TryGetValue(out double d) ? d : v.TryGetValue(out int i) ? i : 0) : 0;
		internal static int Int(JsonNode n) => (int)Math.Round(Num(n));
		internal static double[] Numbers(JsonNode n)
		{
			if (n is JsonArray a) return a.Select(Num).ToArray();
			if (n is JsonValue) return new[] { Num(n) };
			return null;
		}
		/// <summary>A value that may be a range: a number is itself (one entry, nothing drawn), a list its low and high (two entries, a number drawn between - [a] as [a, a], as effects.js draws it too).</summary>
		internal static double[] RangeOf(JsonNode n)
		{
			if (n is JsonArray a) { double lo = a.Count > 0 ? Num(a[0]) : 0; return new[] { lo, a.Count > 1 ? Num(a[1]) : lo }; }
			if (n is JsonValue) return new[] { Num(n) };
			return null;
		}

		internal static double[] Vector(JsonNode n)
		{
			double[] v = n is JsonArray ? Numbers(n) : null;
			if (v == null) return null;
			if (v.Length < 3) Array.Resize(ref v, 3);
			return v;
		}
		/// <summary>A curve's keys: a list of [age, ...values], or { keys, smooth }.</summary>
		internal static double[][] Keys(JsonNode n) => n is JsonObject o ? Keys(o["keys"]) : n is JsonArray a && a.Count > 0 ? a.Select(k => Numbers(k) ?? new double[1]).ToArray() : null;
		internal static bool Smooth(JsonNode n) => n is JsonObject o && Bool(o["smooth"]);
	}

	/// <summary>mulberry32: the same numbers for the same seed as effects.js's effectRandom.</summary>
	public sealed class EffectRandom
	{
		private uint _a;
		public EffectRandom(uint seed) { _a = seed; }
		public double Next()
		{
			unchecked
			{
				_a += 0x6D2B79F5;
				uint t = _a;
				t = (t ^ (t >> 15)) * (t | 1);
				t ^= t + (t ^ (t >> 7)) * (t | 61);
				return (t ^ (t >> 14)) / 4294967296.0;
			}
		}
	}

	public sealed class EffectPlayer
	{
		private const double Deg = Math.PI / 180;

		private readonly EffectDefinition _def;
		private readonly uint _seed;
		private readonly bool _faithfulSpread;
		private EffectRandom _rand;
		private int _frame, _cycle;
		private List<Emitter> _live;
		private List<Mesh> _meshes;

		/// <summary>The anchor: where the effect plays (the battle's hit point for a spell), world units.</summary>
		public double[] Anchor = new double[3];

		/// <summary>The other anchors a track may play on, by name - "caster" (its hit point); "between" is the middle of the caster and the target, "world" the world's origin.</summary>
		public readonly Dictionary<string, double[]> Anchors = new Dictionary<string, double[]>(StringComparer.OrdinalIgnoreCase);

		/// <summary>An anchor by name: target (the default), caster (the target when there is none), between, world.</summary>
		public double[] AnchorOf(string name)
		{
			switch ((name ?? "target").ToLowerInvariant())
			{
				case "world": return new double[3];
				case "caster": return Anchors.TryGetValue("caster", out double[] c) ? c : Anchor;
				case "between":
				{
					double[] a = Anchors.TryGetValue("caster", out double[] k) ? k : Anchor;
					return new[] { (a[0] + Anchor[0]) / 2, (a[1] + Anchor[1]) / 2, (a[2] + Anchor[2]) / 2 };
				}
				default: return Anchors.TryGetValue(name, out double[] other) ? other : Anchor;
			}
		}

		/// <summary>The tracks of other kinds that started on the last step: a sound, a flash, a shake - the host plays them (effects.js's `started`).</summary>
		public readonly List<EffectTrack> Started = new List<EffectTrack>();

		public EffectPlayer(EffectDefinition def, uint seed = 1, bool faithfulSpread = true)
		{
			_def = def;
			_seed = seed;
			_faithfulSpread = faithfulSpread;
			Reset();
		}

		public EffectDefinition Definition => _def;
		public int Frame => _frame;

		/// <summary>Whether everything it started has finished (and it does not loop).</summary>
		public bool Finished => !_def.Loop && _frame >= _def.Length && _live.All(e => e.Done);

		/// <summary>Stops a looping effect: every loop finishes its cycle, and the effect ends when they are done.</summary>
		public bool Stopping { get; private set; }
		public void Stop() => Stopping = true;

		public void Reset()
		{
			_rand = new EffectRandom(_seed);
			_frame = -1;
			_cycle = 0;
			_live = new List<Emitter>();
			_meshes = new List<Mesh>();
			Stopping = false;
		}

		private double Rand() => _rand.Next();
		private double Between(double lo, double hi) => lo + (hi - lo) * Rand();
		private double Range(double[] r) => r == null ? 0 : r.Length == 1 ? r[0] : Between(r[0], r[1]);

		private sealed class Emitter
		{
			public EffectTrack Track;
			public int Steps, Life, Next, Made;
			public bool Playing = true, Stopped, Done;
			public Group[] Groups;
			public double[] At = new double[3];
		}

		private sealed class Group
		{
			public int Age;
			public bool Alive = true;
			public readonly List<Particle> Parts = new List<Particle>();
		}

		private sealed class Particle
		{
			public double[] Base, Pos, Local, Vel, Grav;
			public double Radius, Angle, Size, Roll, Spin, RollNow, RollBefore;
			public bool Shown;
			public GatherState Gather;
			public readonly List<(double[] Pos, bool Shown)?> Trail = new List<(double[], bool)?>();
		}

		private sealed class GatherState
		{
			public bool Going; public double Left, Done;
			public double[] Speed, Add, Turn;
		}

		private sealed class Mesh
		{
			public EffectTrack Track;
			public int Steps;
			public double[] At = new double[3];
			public readonly object Instance = new object();
		}

		private double[] Where(EffectTrack t, int steps)
		{
			double[] o = t.Offset ?? new double[3];
			if (t.Path != null && t.Path["from"] != null)
			{
				// From one anchor to another (a bolt from the caster to the target): its own place, the offset on top.
				double[] q = Between(t.Path, Math.Max(0, steps - 1));
				return new[] { q[0] + o[0], q[1] + o[1], q[2] + o[2] };
			}
			double[] p = PathAt(t.Path, Math.Max(0, steps - 1));
			double[] a = AnchorOf(t.Anchor);
			return new[] { a[0] + o[0] + p[0], a[1] + o[1] + p[1], a[2] + o[2] + p[2] };
		}

		/// <summary>A path from one anchor to another over its length in steps, rising by its arc at the middle; held, repeated or back and forth after.</summary>
		private double[] Between(JsonObject path, int steps)
		{
			double[] from = AnchorOf(EffectDefinition.Text(path["from"])), to = AnchorOf(EffectDefinition.Text(path["to"]) ?? "target");
			double length = Math.Max(1, EffectDefinition.Num(path["length"]));
			string end = EffectDefinition.Text(path["end"]);
			double s = steps / length;
			if (end == "repeat") s -= Math.Floor(s);
			else if (end == "pingpong") { double k = Math.Floor(s); s -= k; if (k % 2 != 0) s = 1 - s; }
			else s = Math.Min(1, s);
			double arc = EffectDefinition.Num(path["arc"]);
			return new[] { from[0] + (to[0] - from[0]) * s, from[1] + (to[1] - from[1]) * s + arc * 4 * s * (1 - s), from[2] + (to[2] - from[2]) * s };
		}

		private void Start(EffectTrack t)
		{
			_live.Add(new Emitter { Track = t, Next = t.Interval, Groups = new Group[Math.Max(0, t.Bursts)] });
		}

		private Group MakeGroup(Emitter e)
		{
			EffectTrack t = e.Track;
			bool local = t.Space == "local";
			Group g = new Group();
			int count = t.CountOverTime != null ? Math.Max(0, (int)Math.Floor(KeysAt(t.CountOverTime, e.Life, 1, t.CountSmooth)[0] + 0.5)) : t.Count;
			for (int i = 0; i < count; i++)
			{
				double[] box = t.Box ?? new double[3];
				double[] c = new double[3];
				for (int k = 0; k < 3; k++) c[k] = box[k] != 0 ? -box[k] + 2 * box[k] * Rand() : 0;
				Particle p = new Particle { Base = c };
				if (t.Gather)
				{
					double len = Hypot(c[0], c[1], c[2]);
					double l = len != 0 ? len : 1;
					double[] dir = { -c[0] / l, -c[1] / l, -c[2] / l };
					double[] swirl = t.GatherSwirl ?? new double[3];
					double[] turn = new double[3];
					for (int k = 0; k < 3; k++) turn[k] = swirl[k] != 0 ? swirl[k] * Rand() : 0;
					p.Gather = new GatherState
					{
						Going = l > 0, Left = l, Done = 0,
						Speed = dir.Select(v => v * t.GatherSpeed).ToArray(),
						Add = dir.Select(v => v * t.GatherAccel).ToArray(),
						Turn = turn,
					};
				}
				else
				{
					double[] v = new double[3];
					if (t.SpeedDirection != null)
					{
						double s = Range(t.SpeedValue) * (t.SpeedOverTime != null ? KeysAt(t.SpeedOverTime, e.Life, 1, t.SpeedTimeSmooth)[0] : 1);
						v = t.SpeedDirection.Select(d => d * s).ToArray();
						if (t.Spread != null)
						{
							double[] a = new double[3];
							for (int k = 0; k < 3; k++) a[k] = t.Spread[k] != 0 ? (_faithfulSpread ? -t.Spread[k] + 2 * t.Spread[k] * Rand() : 360 * Rand()) : 0;
							v = Turn(v, a[0], a[1], a[2]);
						}
					}
					p.Vel = v;
					if (t.GravityDirection != null) { double gv = Range(t.GravityValue); p.Grav = t.GravityDirection.Select(d => d * gv).ToArray(); }
					else p.Grav = new double[3];
					if (t.Orbit) { p.Radius = t.OrbitRadius; p.Angle = 360 * Rand(); }
				}
				if (!local && !t.Gather) p.Base = new[] { p.Base[0] + e.At[0], p.Base[1] + e.At[1], p.Base[2] + e.At[2] };
				double size = Range(t.Size);
				p.Size = (size != 0 ? size : 1) * (t.SizeOverTime != null ? KeysAt(t.SizeOverTime, e.Life, 1, t.SizeTimeSmooth)[0] : 1);
				if (t.Spin) { p.Roll = Range(t.SpinAngle); p.Spin = Range(t.SpinSpeed); }
				g.Parts.Add(p);
			}
			return g;
		}

		private void StepGroup(Emitter e, Group g)
		{
			EffectTrack t = e.Track;
			int life = t.Life != 0 ? t.Life : 1, after = t.TrailCount;
			if (g.Age > life + after) { g.Alive = false; return; }
			g.Age++;
			foreach (Particle p in g.Parts)
			{
				if (after > 0)
				{
					p.Trail.Insert(0, p.Pos != null ? ((double[])p.Pos.Clone(), p.Shown) : ((double[], bool)?)null);
					if (p.Trail.Count > after) p.Trail.RemoveRange(after, p.Trail.Count - after);
				}
				if (p.Gather != null)
				{
					GatherState q = p.Gather;
					if (q.Going)
					{
						for (int k = 0; k < 3; k++) p.Base[k] += q.Speed[k];
						q.Done += Hypot(q.Speed[0], q.Speed[1], q.Speed[2]);
						if (q.Done >= q.Left) { p.Base = new double[3]; q.Going = false; }
						else
						{
							for (int k = 0; k < 3; k++) q.Speed[k] += q.Add[k];
							q.Speed = Turn(q.Speed, q.Turn[0], q.Turn[1], q.Turn[2]);
							q.Add = Turn(q.Add, q.Turn[0], q.Turn[1], q.Turn[2]);
							p.Base = Turn(p.Base, q.Turn[0], q.Turn[1], q.Turn[2]);
						}
					}
					p.Local = (double[])p.Base.Clone();
				}
				else
				{
					double pace = t.SpeedOverLife != null ? KeysAt(t.SpeedOverLife, g.Age, 1, t.SpeedSmooth)[0] : 1;
					double pull = t.GravityOverLife != null ? KeysAt(t.GravityOverLife, g.Age, 1, t.GravitySmooth)[0] : 1;
					for (int k = 0; k < 3; k++) { p.Vel[k] += p.Grav[k] * pull; p.Base[k] += p.Vel[k] * pace; }
					p.Local = (double[])p.Base.Clone();
					if (t.Orbit)
					{
						p.Radius += t.OrbitGrowKeys != null ? KeysAt(t.OrbitGrowKeys, g.Age, 1, t.OrbitGrowSmooth)[0] : t.OrbitGrow;
						p.Angle += t.OrbitTurnKeys != null ? KeysAt(t.OrbitTurnKeys, g.Age, 1, t.OrbitTurnSmooth)[0] : t.OrbitTurn;
						p.Local[0] += p.Radius * Math.Sin(p.Angle * Deg);
						p.Local[2] += p.Radius * Math.Cos(p.Angle * Deg);
					}
				}
				double[] origin = t.Space == "local" ? e.At : new double[3];
				p.Pos = new[] { p.Local[0] + origin[0], p.Local[1] + origin[1], p.Local[2] + origin[2] };
				p.Shown = g.Age < life;
				if (g.Age == 1) { p.RollNow = p.Roll; p.RollBefore = p.Roll; }
				else { p.RollBefore = p.RollNow; p.RollNow += p.Spin * (t.SpinOverLife != null ? KeysAt(t.SpinOverLife, g.Age, 1, t.SpinSmooth)[0] : 1); }
			}
		}

		private void StepEmitter(Emitter e)
		{
			EffectTrack t = e.Track;
			e.Steps++;
			e.At = Where(t, e.Steps);
			if (e.Playing)
			{
				e.Life++; e.Next++;
				if (e.Next >= t.Interval && e.Made < e.Groups.Length)
				{
					e.Groups[e.Made] = MakeGroup(e);
					e.Made++;
					e.Next = 0;
				}
				foreach (Group g in e.Groups) if (g != null && g.Alive) StepGroup(e, g);
				if (e.Life >= t.Duration)
				{
					if (t.Loop && !e.Stopped) { e.Made = 0; e.Life = 0; e.Next = 0; }
					else e.Playing = false;
				}
			}
			else
			{
				foreach (Group g in e.Groups) if (g != null && g.Alive) StepGroup(e, g);
				Group last = e.Groups.Length > 0 ? e.Groups[e.Made != 0 ? e.Made - 1 : 0] : null;
				if (last == null || !last.Alive) e.Done = true;
			}
		}

		/// <summary>One game step.</summary>
		public void Step()
		{
			_frame++;
			Started.Clear();
			int length = _def.Length;
			if (_def.Loop && !Stopping && _frame - _cycle > length) _cycle = _frame;
			int at = _frame - _cycle;
			foreach (EffectTrack t in _def.Tracks)
			{
				if (t.Start != at) continue;
				if (_cycle > 0 && t.Id != null && _live.Any(e => e.Track == t && !e.Done && t.Loop)) continue;
				if (_cycle != 0 && !_def.Loop) continue;
				if (t.Type == "mesh") { _meshes.Add(new Mesh { Track = t }); continue; }
				if (t.Type == "emitter") { Start(t); continue; }
				Started.Add(t);
			}
			foreach (Mesh m in _meshes) { m.Steps++; m.At = Where(m.Track, m.Steps); }
			foreach (Emitter e in _live)
			{
				if (e.Done) continue;
				if ((!_def.Loop || Stopping) && at >= length && _cycle == 0) e.Stopped = true;
				if (Stopping) e.Stopped = true;
				if (e.Track.Stop >= 0 && at >= e.Track.Stop) e.Stopped = true;
				StepEmitter(e);
			}
			if (_live.Count > 64) _live = _live.Where(e => !e.Done).ToList();
		}

		/// <summary>Every quad to draw now, in the order the game draws them: emitter, group, particle, then its trail.</summary>
		public List<EffectQuad> Quads(List<EffectQuad> into = null)
		{
			List<EffectQuad> o = into ?? new List<EffectQuad>();
			o.Clear();
			foreach (Emitter e in _live)
			{
				if (e.Done) continue;
				EffectTrack t = e.Track;
				foreach (Group g in e.Groups)
				{
					if (g == null || !g.Alive || g.Age < 1) continue;
					double[] colour = KeysAt(t.Colour, g.Age, 4, t.ColourSmooth) ?? new double[] { 255, 255, 255, 255 };
					for (int k = 0; k < 4; k++) colour[k] = Math.Max(0, Math.Min(255, colour[k]));
					double[] scale = KeysAt(t.Scale, g.Age, 2, t.ScaleSmooth) ?? new double[] { 1, 1 };
					int cell = FrameAt(t.Texture?.Frames, g.Age);
					foreach (Particle p in g.Parts)
					{
						double w = p.Size * scale[0] / 2, h = p.Size * scale[1] / 2;
						double roll = p.RollNow;
						if (p.Shown && colour[3] > 0) o.Add(new EffectQuad { X = p.Pos[0], Y = p.Pos[1], Z = p.Pos[2], HalfWidth = w, HalfHeight = h, R = colour[0], G = colour[1], B = colour[2], A = colour[3], Cell = cell, Roll = roll, Track = t, Particle = p });
						if (t.TrailCount > 0 && p.Trail.Count > 0)
						{
							int n = t.TrailCount;
							double[] d = new double[4];
							for (int k = 0; k < 4; k++) d[k] = (colour[k] - Math.Max(0, Math.Min(255, colour[k] + (k < t.TrailColour.Length ? t.TrailColour[k] : 0)))) / (n + 1);
							for (int k = 1; k < n && k <= p.Trail.Count; k++)
							{
								(double[] Pos, bool Shown)? was = p.Trail[k - 1];
								if (was == null || !was.Value.Shown) continue;
								double a = colour[3] - k * d[3];
								if (a > 0) o.Add(new EffectQuad { X = was.Value.Pos[0], Y = was.Value.Pos[1], Z = was.Value.Pos[2], HalfWidth = w, HalfHeight = h, R = colour[0] - k * d[0], G = colour[1] - k * d[1], B = colour[2] - k * d[2], A = a, Cell = cell, Roll = roll, Track = t, Particle = p, Trail = k });
							}
						}
					}
				}
			}
			return o;
		}

		/// <summary>The models to draw now.</summary>
		public List<EffectModel> Models()
		{
			List<EffectModel> o = new List<EffectModel>();
			if (_frame >= _def.Length && !_def.Loop && _live.All(e => e.Done)) return o;
			foreach (Mesh m in _meshes)
			{
				// A mesh with a life of its own shows for it; else while the effect plays.
				if (m.Track.Raw?["life"] != null && m.Steps > m.Track.Life) continue;
				EffectTrack t = m.Track;
				double[] scale = t.MeshScaleKeys != null ? new[] { KeysAt(t.MeshScaleKeys, m.Steps, 1, t.MeshScaleSmooth)[0] } : t.MeshScale ?? new double[] { 1 };
				if (scale.Length < 3) scale = new[] { scale[0], scale[0], scale[0] };
				double yaw = t.MeshYawKeys != null ? KeysAt(t.MeshYawKeys, m.Steps, 1, t.MeshYawSmooth)[0] : t.MeshYaw;
				o.Add(new EffectModel { Track = t, X = m.At[0], Y = m.At[1], Z = m.At[2], Frame = m.Steps - 1, Instance = m.Instance, Scale = scale, Yaw = yaw });
			}
			return o;
		}

		// ------------------------------------------------------------------ helpers, as effects.js has them

		/// <summary>A value of a curve by age: straight lines between keys, or (smooth) a Hermite curve with the keys' tangents.</summary>
		public static double[] KeysAt(double[][] keys, double age, int width, bool smooth = false)
		{
			if (keys == null || keys.Length == 0) return null;
			double[] Slice(double[] k) { double[] r = new double[width]; for (int j = 0; j < width; j++) r[j] = 1 + j < k.Length ? k[1 + j] : 0; return r; }
			if (age <= keys[0][0]) return Slice(keys[0]);
			double[] last = keys[keys.Length - 1];
			if (age >= last[0]) return Slice(last);
			for (int i = 1; i < keys.Length; i++)
			{
				double[] b = keys[i];
				if (age > b[0]) continue;
				double[] a = keys[i - 1];
				double span = b[0] - a[0];
				double t = span == 0 ? 1 : (age - a[0]) / span;
				double[] o = new double[width];
				double t2 = t * t, t3 = t2 * t;
				double h00 = 2 * t3 - 3 * t2 + 1, h10 = t3 - 2 * t2 + t, h01 = -2 * t3 + 3 * t2, h11 = t3 - t2;
				for (int j = 0; j < width; j++)
				{
					double av = 1 + j < a.Length ? a[1 + j] : 0, bv = 1 + j < b.Length ? b[1 + j] : 0;
					if (!smooth || span == 0) o[j] = av + (bv - av) * t;
					else o[j] = h00 * av + h10 * span * Slope(keys, i - 1, j, width) + h01 * bv + h11 * span * Slope(keys, i, j, width);
				}
				return o;
			}
			return Slice(last);
		}

		/// <summary>A key's tangent on one of its values: its own (after its values), or the curve's - flat at the ends and at a turn, else as its neighbours lie, never overshooting.</summary>
		public static double Slope(double[][] keys, int i, int j, int width)
		{
			double[] k = keys[i];
			if (k.Length >= 1 + 2 * width) return k[1 + width + j];
			if (i == 0 || i == keys.Length - 1) return 0;
			double[] p = keys[i - 1], n = keys[i + 1];
			if (k[0] <= p[0] || n[0] <= k[0]) return 0;
			double V(double[] key) => 1 + j < key.Length ? key[1 + j] : 0;
			double left = (V(k) - V(p)) / (k[0] - p[0]), right = (V(n) - V(k)) / (n[0] - k[0]);
			if (left * right <= 0) return 0;
			double m = (V(n) - V(p)) / (n[0] - p[0]);
			return Math.Sign(m) * Math.Min(Math.Abs(m), Math.Min(3 * Math.Abs(left), 3 * Math.Abs(right)));
		}

		public static int FrameAt(double[][] frames, double age)
		{
			if (frames == null || frames.Length == 0) return 0;
			double cell = frames[0].Length > 1 ? frames[0][1] : 0;
			foreach (double[] f in frames) { if (f[0] <= age) cell = f.Length > 1 ? f[1] : 0; else break; }
			return (int)cell;
		}

		public static double[] Turn(double[] v, double ax, double ay, double az)
		{
			double x = v[0], y = v[1], z = v[2];
			if (ax != 0) { double c = Math.Cos(ax * Deg), s = Math.Sin(ax * Deg); double ny = y * c - z * s, nz = y * s + z * c; y = ny; z = nz; }
			if (ay != 0) { double c = Math.Cos(ay * Deg), s = Math.Sin(ay * Deg); double nx = x * c + z * s, nz = -x * s + z * c; x = nx; z = nz; }
			if (az != 0) { double c = Math.Cos(az * Deg), s = Math.Sin(az * Deg); double nx = x * c - y * s, ny = x * s + y * c; x = nx; y = ny; }
			return new[] { x, y, z };
		}

		private static double Hypot(double x, double y, double z) => Math.Sqrt(x * x + y * y + z * z);

		public static double[] PathAt(JsonObject path, int steps)
		{
			if (path == null) return new double[3];
			if (path["point"] is JsonArray point) return EffectDefinition.Vector(point);
			JsonArray segs = path["segments"] as JsonArray;
			if (segs == null || segs.Count == 0) return new double[3];
			double[] times = EffectDefinition.Numbers(path["times"]) ?? new double[0];
			double length = EffectDefinition.Num(path["length"]);
			string end = EffectDefinition.Text(path["end"]);
			bool curve = EffectDefinition.Bool(path["curve"]);
			double T(int i) => i < times.Length ? times[i] : 0;
			double Dur(int i) => Math.Max(1, i + 1 < segs.Count ? T(i + 1) - T(i) : length - T(i));
			double total = 0;
			for (int i = 0; i < segs.Count; i++) total += Dur(i);
			double left = steps;
			if (end == "repeat" && total > 0) left = steps % total;
			bool forward = true;
			if (end == "pingpong" && total > 0) { double k = Math.Floor(steps / total); left = steps % total; forward = k % 2 == 0; }
			int s = 0;
			while (s < segs.Count && left >= Dur(s)) { left -= Dur(s); s++; }
			double t;
			if (s >= segs.Count) { s = segs.Count - 1; t = 1; } else t = left / Dur(s);
			if (!forward) { s = segs.Count - 1 - s; t = 1 - t; }
			JsonArray seg = (JsonArray)segs[s];
			double[] p0 = EffectDefinition.Vector(seg[0]), p1 = EffectDefinition.Vector(seg[1]), t0 = EffectDefinition.Vector(seg[2]), t1 = EffectDefinition.Vector(seg[3]);
			double[] o = new double[3];
			if (!curve) { for (int k = 0; k < 3; k++) o[k] = p0[k] + (p1[k] - p0[k]) * t; return o; }
			double t2 = t * t, t3 = t2 * t;
			double h00 = 2 * t3 - 3 * t2 + 1, h01 = -2 * t3 + 3 * t2, h10 = t3 - 2 * t2 + t, h11 = t3 - t2;
			for (int k = 0; k < 3; k++) o[k] = h00 * p0[k] + h01 * p1[k] + h10 * t0[k] + h11 * t1[k];
			return o;
		}
	}
}
