// One of the game's effects (a category and a member of effect.efi) as a definition of the new
// format - the effects the mods make (Docs/Effects-Plan.md). One-way and faithful enough to start
// from: a sequence becomes the effect's timeline, each template it boots a track on the target at
// the step the game boots it, riding the sequence's path; a particle template an emitter track.
//
// The emitter keeps the game's emission (groups of particles born every interval, for the play
// time, looping or not) and its motion (the birth box, speed and spread, gravity, the orbit, the
// gather). Its sprite animation - the UV, scale and colour tables a group steps through, with the
// fade over them - is baked into keys by running eld's own state machine (eld.spr) for every step of
// a particle's life, so a key between two whole steps is only ever a straight line the game drew too.
//
// Units: frames of the game's 30 a second; world units (4096 of the game's fx32); sizes the full
// width of a particle (the game keeps half); colours 0-255; angles in degrees, as the game turned
// them. An emit angle is written as radians x 4096 but turned as 65536 a turn - and the port turns
// any axis that has one a whole random turn (its two spread vectors are one object,
// eld.EmmitController), which is how the game's effects look in play; so that is what the import
// writes, 180 either way, and a modder can narrow it. What a track leaves out is its default.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace OpenFF.Effects
{
	public static class EffectImport
	{
		public const int Format = 1;

		/// <summary>A template of a category's member, and the pack it came from; null when there is none.</summary>
		public delegate (EffectTemplate Template, EfpPack Pack)? Resolve(int category, int member);

		/// <summary>The effect a category's member plays, as a definition. Notes say what was left out.</summary>
		public static JsonObject Import(int category, int member, Resolve resolve, List<string> notes = null)
		{
			(EffectTemplate Template, EfpPack Pack)? root = resolve(category, member);
			if (root == null) throw new InvalidOperationException("no effect " + category + "/" + member);
			JsonArray tracks = new JsonArray();
			int length = 0;
			bool loop = false;
			if (root.Value.Template is SequenceTemplate sequence)
			{
				length = Flatten(sequence, category, 0, new double[3], resolve, tracks, notes, 0);
				loop = sequence.Loop;
			}
			else
			{
				JsonObject track = Track(root.Value.Template, root.Value.Pack, category, member, 0, -1, null, new double[3], notes);
				if (track != null) tracks.Add(track);
			}
			JsonObject effect = new JsonObject
			{
				["format"] = Format,
				["from"] = "game:" + category + "/" + member,
				["length"] = length,
			};
			if (loop) effect["loop"] = true;
			effect["tracks"] = tracks;
			return effect;
		}

		// ------------------------------------------------------------------ sequences

		/// <summary>A sequence's commands step by step (eld.ImpSequenceDS): its boots as tracks. Its END's frame.</summary>
		private static int Flatten(SequenceTemplate s, int category, int start, double[] offset, Resolve resolve, JsonArray tracks, List<string> notes, int depth)
		{
			Dictionary<int, JsonObject> booted = new Dictionary<int, JsonObject>();
			List<JsonObject> mine = new List<JsonObject>();
			int wait = 1, next = 0, frame = 0;
			for (; frame < 10000; frame++)
			{
				wait--;
				bool ended = false;
				while (wait == 0 && !ended)
				{
					if (next >= s.Commands.Count) { ended = true; break; }
					SequenceCommand c = s.Commands[next++];
					switch (c.Op)
					{
						case SequenceOp.Wait:
							wait = c.Time;
							break;
						case SequenceOp.Boot:
							SequencePath path = c.Path >= 0 && c.Path < s.Paths.Count ? s.Paths[c.Path] : null;
							(EffectTemplate Template, EfpPack Pack)? t = resolve(c.Category, c.Member);
							if (t == null) { notes?.Add("boot " + c.Category + "/" + c.Member + ": no such effect"); break; }
							if (t.Value.Template is SequenceTemplate inner)
							{
								if (depth > 4) { notes?.Add("boot " + c.Category + "/" + c.Member + ": sequences nested too deep"); break; }
								// A sequence booted by a sequence: its tracks on this one's timeline, at its path's start.
								double[] at = path != null && path.Points.Count > 0 ? Add(offset, Units(path.Points[0])) : offset;
								Flatten(inner, c.Category, start + frame, at, resolve, tracks, notes, depth + 1);
								if (inner.Loop) notes?.Add(c.Category + "/" + c.Member + ": a looping sequence inside a sequence plays once");
								break;
							}
							JsonObject track = Track(t.Value.Template, t.Value.Pack, c.Category, c.Member, start + frame, c.BootId, path, offset, notes);
							if (track == null) break;
							tracks.Add(track);
							mine.Add(track);
							booted[c.BootId] = track;
							break;
						case SequenceOp.Halt:
							if (booted.TryGetValue(c.BootId, out JsonObject halted) && halted["stop"] == null) halted["stop"] = start + frame;
							break;
						case SequenceOp.End:
							ended = true;
							break;
					}
				}
				if (ended) break;
			}
			// A sequence inside another stops what it booted at its end, as the outer one would.
			if (depth > 0) foreach (JsonObject t in mine) if (t["stop"] == null) t["stop"] = start + frame;
			return frame;
		}

		// ------------------------------------------------------------------ tracks

		private static JsonObject Track(EffectTemplate template, EfpPack pack, int category, int member, int start, int bootId, SequencePath path, double[] offset, List<string> notes)
		{
			JsonObject track;
			string what = category + "/" + member;
			if (template is ParticleTemplate p) track = Emitter(p, pack, notes, what);
			else if (template is ModelTemplate m)
			{
				track = new JsonObject
				{
					["type"] = "mesh",
					["model"] = "game:" + pack?.Name + ":0x" + m.Id.ToString("x"),
					["scale"] = Vector(m.Scale.Select(v => v / 4096.0).ToArray()),
				};
				if (m.Loop) track["loop"] = true;
				if (!string.IsNullOrEmpty(m.Material)) notes?.Add(what + ": a model (" + System.IO.Path.GetFileNameWithoutExtension(m.Material) + ") - its material animation is not played yet");
			}
			else return null;
			string label = template is ParticleTemplate pt && pt.Texture != null ? " " + Stem(pt.Texture.Name) : template is ModelTemplate mt && !string.IsNullOrEmpty(mt.Material) ? " " + Stem(mt.Material) : "";
			JsonObject head = new JsonObject { ["type"] = track["type"]!.GetValue<string>(), ["name"] = what + label, ["start"] = start };
			if (bootId >= 0) head["id"] = bootId;
			head["anchor"] = "target";
			if (offset.Any(v => v != 0)) head["offset"] = Vector(offset);
			if (path != null) head["path"] = Path(path);
			foreach (var kv in track.ToList()) { if (kv.Key == "type") continue; track.Remove(kv.Key); head[kv.Key] = kv.Value; }
			return head;
		}

		private static JsonObject Emitter(ParticleTemplate p, EfpPack pack, List<string> notes, string what)
		{
			JsonObject e = new JsonObject { ["type"] = "emitter" };
			if ((p.Flags & ParticleFlags.MoveOffset) != 0 || p.Kind == EffectKind.ParticleGather) e["space"] = "local";
			JsonObject emission = new JsonObject { ["duration"] = p.TimePlay, ["interval"] = p.Interval, ["count"] = p.Childs, ["bursts"] = p.Groups };
			if ((p.Flags & ParticleFlags.Loop) != 0) emission["loop"] = true;
			e["emission"] = emission;
			e["life"] = p.GroupLife;
			if (p.Range.Any(v => v != 0)) e["shape"] = new JsonObject { ["box"] = Vector(p.Range.Select(v => v / 4096.0).ToArray()) };
			e["size"] = Range(2 * p.SizeBase / 4096.0, 2 * (p.SizeBase + Math.Max(0, p.SizeRand)) / 4096.0);
			if (p.Kind == EffectKind.ParticleGather)
			{
				e["gather"] = new JsonObject
				{
					["speed"] = Round(p.GatherSpeed / 4096.0),
					["accel"] = Round(p.GatherSpeedAdd / 4096.0),
					["swirl"] = Vector(p.GatherRotate.Select(Degrees).ToArray()),
				};
			}
			else
			{
				if (p.SpeedPow != 0 || p.SpeedRand != 0)
				{
					JsonObject speed = new JsonObject
					{
						["direction"] = Vector(p.SpeedDir.Select(v => v / 4096.0).ToArray()),
						["value"] = Range(p.SpeedPow / 4096.0, (p.SpeedPow + Math.Max(0, p.SpeedRand)) / 4096.0),
					};
					if (p.EmitAngle.Any(v => v != 0)) speed["spread"] = Vector(p.EmitAngle.Select(v => v != 0 ? 180.0 : 0.0).ToArray());
					e["speed"] = speed;
				}
				if (p.GravityPow != 0 || p.GravityRand != 0)
				{
					// The game applies the strength twice (the group's roll times the particle's): with no random part, dir x pow x pow.
					double lo = p.GravityPow / 4096.0, hi = (p.GravityPow + Math.Max(0, p.GravityRand)) / 4096.0;
					e["gravity"] = new JsonObject
					{
						["direction"] = Vector(p.GravityDir.Select(v => v / 4096.0).ToArray()),
						["value"] = Range(lo * lo, hi * hi),
					};
				}
				if (p.Kind == EffectKind.Particle && (p.CircleRadius != 0 || p.CircleRadiusAdd != 0 || p.CircleAngleAdd != 0))
					e["orbit"] = new JsonObject { ["radius"] = Round(p.CircleRadius / 4096.0), ["grow"] = Round(p.CircleRadiusAdd / 4096.0), ["turn"] = Degrees(p.CircleAngleAdd) };
			}
			if ((p.Flags & ParticleFlags.AfterImage) != 0 && p.AfterCount > 0)
				e["trail"] = new JsonObject { ["count"] = p.AfterCount, ["colour"] = new JsonArray(p.AfterColour.Select(c => (JsonNode)(int)Math.Round(c * 255 / 31.0)).ToArray()) };

			Bake(p, e);
			if (p.Texture != null)
			{
				JsonObject texture = new JsonObject
				{
					["image"] = "game:" + pack?.Name + ":" + p.Texture.Name,
					["width"] = p.Texture.Width,
					["height"] = p.Texture.Height,
				};
				SpriteAnimation a = p.Animation;
				if (a != null && a.CellWidth > 0 && a.CellHeight > 0)
				{
					texture["cell"] = new JsonArray(a.StartU, a.StartV, a.CellWidth, a.CellHeight);
					texture["columns"] = a.CellWidth > 0 ? a.SheetWidth / a.CellWidth : 0;
				}
				if (e["frames"] is JsonNode frames) { e.Remove("frames"); texture["frames"] = frames; }
				e["texture"] = texture;
			}
			else e.Remove("frames");
			e["render"] = new JsonObject { ["blend"] = "alpha", ["facing"] = "camera" };
			return e;
		}

		// ------------------------------------------------------------------ the sprite animation, baked

		/// <summary>One of eld.spr's tables stepping: the entry it is on, the steps left, and how far between it and the next.</summary>
		private sealed class Table
		{
			private readonly int[] _time;
			private readonly bool _loop;
			public int Pos, Wait, Inter;
			public int Count => _time.Length;
			private const int End = -1;
			public Table(int[] time, bool loop)
			{
				_time = time; _loop = loop;
				Pos = 0; Wait = time.Length != 0 ? time[0] : End;
				if (time.Length != 0) Inter = Div(4096, time[0]);
			}
			/// <summary>True when the entry changed.</summary>
			public bool Update()
			{
				if (Wait == End) return false;
				bool changed = false;
				if (Wait <= 0)
				{
					if (Pos + 1 >= _time.Length)
					{
						if (!_loop) { Wait = End; return false; }
						Pos = 0;
					}
					else Pos++;
					Wait = _time[Pos];
					if (Wait <= 0) Wait = 1;
					Inter = Div(4096, _time[Pos]);
					changed = true;
				}
				Wait--;
				return changed;
			}
			/// <summary>The fx32 fraction toward the next entry, or -1 when this one holds.</summary>
			public int Fraction(bool interpolate)
			{
				if (!interpolate || Pos + 1 >= _time.Length || Wait == End) return -1;
				return _time[Pos] != 0 ? 4096 - Mul(Wait, Inter) : 0;
			}
			public static int Lerp(int cur, int next, int t) => t < 0 ? cur : (int)(((long)(next - cur) * t) >> 12) + cur;
		}

		private static int Mul(int a, int b) => (int)(((long)a * b + 2048) >> 12);
		private static int Div(int n, int d) => d == 0 ? n : (int)(((long)n << 12) / d);

		/// <summary>Colour, scale and UV frames for every step of a particle's life (1 to life - 1, the steps it shows), as keys.</summary>
		private static void Bake(ParticleTemplate p, JsonObject e)
		{
			SpriteAnimation a = p.Animation;
			int last = Math.Max(1, p.GroupLife - 1);
			List<int[]> colours = new List<int[]>(), scales = new List<int[]>();
			List<(int Age, int Pattern)> frames = new List<(int, int)>();
			if (a != null)
			{
				Table uv = new Table(a.Uv.Select(u => u.Time).ToArray(), (a.UvFlags & SpriteAnimation.Loop) != 0);
				Table sc = new Table(a.Scale.Select(s => s.Time).ToArray(), (a.ScaleFlags & SpriteAnimation.Loop) != 0);
				Table cl = new Table(a.Colour.Select(c => c.Time).ToArray(), (a.ColourFlags & SpriteAnimation.Loop) != 0);
				int pattern = 0;
				int columns = a.CellWidth > 0 ? (ushort)(a.SheetWidth / a.CellWidth) : 0;
				bool fade = (p.Flags & ParticleFlags.Fade) != 0;
				for (int age = 1; age <= last; age++)
				{
					if (uv.Update()) pattern = columns == 0 ? 0 : a.Uv[uv.Pos].Pattern;
					sc.Update();
					cl.Update();
					if (frames.Count == 0 || frames[^1].Pattern != pattern) frames.Add((age, pattern));
					if (sc.Count > 0)
					{
						var s0 = a.Scale[sc.Pos]; var s1 = a.Scale[Math.Min(sc.Pos + 1, sc.Count - 1)];
						int t = sc.Fraction((a.ScaleFlags & SpriteAnimation.Interpolate) != 0);
						scales.Add(new[] { age, Table.Lerp(s0.X, s1.X, t), Table.Lerp(s0.Y, s1.Y, t) });
					}
					int r = 31, g = 31, b = 31, al = 31;
					if (cl.Count > 0)
					{
						var c0 = a.Colour[cl.Pos]; var c1 = a.Colour[Math.Min(cl.Pos + 1, cl.Count - 1)];
						int t = cl.Fraction((a.ColourFlags & SpriteAnimation.Interpolate) != 0);
						r = Table.Lerp(c0.R, c1.R, t); g = Table.Lerp(c0.G, c1.G, t); b = Table.Lerp(c0.B, c1.B, t); al = Table.Lerp(c0.A, c1.A, t);
					}
					double fr = 0, fg = 0, fb = 0, fa = 0;
					if (fade)
					{
						int endFade = p.FadeStart + p.FadeTime;
						// A fade carried without its flag is ignored, as the game ignores it.
						double k = age < p.FadeStart ? 0 : age >= endFade ? 1 : (age - p.FadeStart) / (double)p.FadeTime;
						fr = p.FadeColour[0] * k; fg = p.FadeColour[1] * k; fb = p.FadeColour[2] * k; fa = p.FadeColour[3] * k;
					}
					colours.Add(new[] { age, Clamp(r + fr), Clamp(g + fg), Clamp(b + fb), Clamp(al + fa) });
				}
			}
			if (colours.Count > 0 && colours.Any(c => c[1] != 31 || c[2] != 31 || c[3] != 31 || c[4] != 31))
				e["colour"] = Keys(colours, 1, v => (int)Math.Round(v * 255 / 31.0));
			if (scales.Count > 0 && scales.Any(s => s[1] != 4096 || s[2] != 4096))
				e["scale"] = Keys(scales, 2, v => Round(v / 4096.0));
			if (frames.Count > 1 || (frames.Count == 1 && frames[0].Pattern != 0))
				e["frames"] = new JsonArray(frames.Select(f => (JsonNode)new JsonArray(f.Age, f.Pattern)).ToArray());
		}

		private static int Clamp(double v) => (int)Math.Clamp(v, 0, 31);

		/// <summary>The rows as keys, those a straight line between their neighbours passes through (within a slack) left out.</summary>
		private static JsonArray Keys(List<int[]> rows, int slack, Func<int, JsonNode> value)
		{
			List<int[]> kept = new List<int[]>();
			for (int i = 0; i < rows.Count; i++)
			{
				if (i == 0 || i == rows.Count - 1) { kept.Add(rows[i]); continue; }
				int[] a = kept[^1], b = rows[i + 1];
				bool straight = true;
				// the next row as the line from the last kept one would have it: every value within the slack of every row between
				for (int j = 1; j < a.Length && straight; j++)
				{
					for (int k = rows.IndexOf(a) + 1; k <= i + 1 && straight; k++)
					{
						double t = (rows[k][0] - a[0]) / (double)(b[0] - a[0]);
						double line = a[j] + (b[j] - a[j]) * t;
						if (Math.Abs(rows[k][j] - line) > slack) straight = false;
					}
				}
				if (!straight) kept.Add(rows[i]);
			}
			return new JsonArray(kept.Select(r => (JsonNode)new JsonArray(r.Select((v, j) => j == 0 ? (JsonNode)v : value(v)).ToArray())).ToArray());
		}

		// ------------------------------------------------------------------ paths

		private static JsonObject Path(SequencePath p)
		{
			if (p.Points.Count < 4)
				return new JsonObject { ["point"] = Vector(p.Points.Count > 0 ? Units(p.Points[0]) : new double[3]) };
			int segments = p.Points.Count / 4;
			JsonArray list = new JsonArray();
			for (int i = 0; i < segments; i++)
				list.Add(new JsonArray(Enumerable.Range(0, 4).Select(k => (JsonNode)Vector(Units(p.Points[4 * i + k]))).ToArray()));
			int move = p.Flags & 0xE;
			JsonObject o = new JsonObject
			{
				["curve"] = (p.Flags & SequencePath.Curve) != 0,
				["segments"] = list,
				["times"] = new JsonArray(p.Timing.Take(segments).Select(t => (JsonNode)t).ToArray()),
				["length"] = p.FrameTime,
				["end"] = move == SequencePath.PingPong ? "pingpong" : move == SequencePath.Repeat ? "repeat" : "hold",
			};
			if ((p.Flags & SequencePath.World) != 0) o["space"] = "world";
			return o;
		}

		// ------------------------------------------------------------------ helpers

		private static double[] Units(int[] fx) => fx.Select(v => v / 4096.0).ToArray();
		private static double[] Add(double[] a, double[] b) => new[] { a[0] + b[0], a[1] + b[1], a[2] + b[2] };
		private static double Round(double v) => Math.Round(v, 4);
		private static double Degrees(int index) => Math.Round(index * 360.0 / 65536.0, 3);
		private static JsonArray Vector(double[] v) => new JsonArray(v.Select(x => (JsonNode)Round(x)).ToArray());
		private static JsonArray Range(double lo, double hi) => new JsonArray(Round(lo), Round(hi));
		private static string Stem(string name) => System.IO.Path.GetFileNameWithoutExtension(name ?? "");
	}
}
