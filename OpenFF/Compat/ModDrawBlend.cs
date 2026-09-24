// The mods' draw list between two of the game's steps.
//
// Game.Draw is filled once a step - EngineHost.Tick clears it and the mods' Update draws into
// it, a tag's place taken from that step's camera (WorldToScreen) - and ModDraw draws it on
// every display frame until the next. With smoothing on (FramePacer.Blend under 1) the display
// draws two or more frames a step and the game's own frame is drawn part of the way from the
// step before's (FrameCapture); the list drawn as it stands would hold for a step and then
// jump, out of step with the scene under it. So the step before's list is kept (Keep, as the
// engine's frame begins), each command of the new one is paired with the one it was, and what
// is drawn stands where the pair does at the display's point between the two steps.
//
// A command is paired by what it draws: its kind, its picture and the part of it, its size,
// its words and colour (not the alpha: a fade is blended), and the thing it belongs to when
// the mod says (DrawList.Group). Within a group the n-th command drawing that pairs with the
// n-th in the other list, so a group drawn ahead of the rest (a new damage number: Ff4Battle
// draws its newest first) throws none of the others off. Commands of no group that draw alike
// (the bar over every foe, a marker on every chest) are paired by place: each with the nearest
// like it in the step before's list, the nearest pairs first, so one gone from among them (a
// foe killed, one off the screen) or the list drawn in another order takes nobody's neighbour
// for them. Only places slide - the corner, a line's two ends, a sprite's turn - and the alpha;
// a pair that moved further than a few units in the step (a cursor down a row, a list
// scrolled, a page turned) has not moved but changed, and is drawn where one step has it -
// further for a step that ran more of the game's (--speed), whose motion goes further too.
// What is drawn at all, the words and the colours, come from the nearer step: the step
// before's list until half-way, the new one from there, as FrameCapture draws the game's own
// frame, so nothing appears or changes a step ahead of the motion it goes with. A part change
// (EngineHost, with FrameCapture.Cut) leaves nothing to blend from; a camera cut
// (FrameCapture.SceneCut) draws the whole list as the nearer step has it, as the scene under it
// is drawn, so a tag over a character never slides across the cut.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenFF.Client
{
	using DrawCommand = OpenFF.DrawCommand;
	using DrawKind = OpenFF.DrawKind;

	internal static class ModDrawBlend
	{
		/// <summary>A move in one step further than this, in the 800 x 480 screen units, is a change and not motion: a cursor's hop to the next row, a list's scroll.</summary>
		public const float SnapDistance = 24f;

		/// <summary>The furthest a step that ran many of the game's steps (--speed) may take as motion: a third of the screen's height, about where FrameCapture takes a draw's move for a spawn or a cut.</summary>
		private const float MostSnap = OpenFF.DrawList.ScreenHeight / 3f;

		/// <summary>A sprite turned further than this in one step (radians) has been turned, not seen turning.</summary>
		private const float SnapTurn = MathF.PI / 2f;

		/// <summary>How many of the step before's commands alike, either side of a command's own place among them, are looked at for its nearest: more than go or come in ahead of it in a step, and few enough that thousands of alike things are still paired in a moment.</summary>
		private const int Reach = 32;

		private static readonly List<DrawCommand> _before = new List<DrawCommand>(256);
		private static readonly List<DrawCommand> _frame = new List<DrawCommand>(256);
		private static int[] _pairOfNow = new int[256], _pairOfBefore = new int[256];
		// Grouped commands: the n-th of a key in the step before's list (Slot), found by counting (Next).
		private static readonly Dictionary<int, int> _counts = new Dictionary<int, int>();
		private static readonly Dictionary<long, int> _slots = new Dictionary<long, int>();
		// The rest: for each key a run of _order holding the step before's commands of that key in the order drawn,
		// and the pairs of them near enough to be motion (Pairing), sorted nearest first.
		private static readonly Dictionary<int, int> _runs = new Dictionary<int, int>();
		private static Run[] _run = new Run[64];
		private static int[] _order = new int[256], _runOfBefore = new int[256], _runOfNow = new int[256];
		private static Pairing[] _pairings = new Pairing[256];
		private static bool _cut;
		private static int _kept, _paired = -1, _pairedCount = -1, _failed = -1;
		private static float _snap = SnapDistance;

		/// <summary>The step before's commands of one key, alike but for their place: where they start in _order, how many, how many of this step's have been met so far, and the first perhaps still unpaired.</summary>
		private struct Run
		{
			public int Start, Count, Met, Free;
		}

		/// <summary>A command of this step's and one of the step before's alike and near enough to be one thing moving.</summary>
		private struct Pairing
		{
			public float Distance;
			public int Before, Now;
		}

		/// <summary>The nearest first; on a tie the one drawn earlier in the step before's list, then in this one's.</summary>
		private static readonly Comparison<Pairing> NearestFirst = (a, b) =>
		{
			int c = a.Distance.CompareTo(b.Distance);
			if (c != 0) return c;
			c = a.Before.CompareTo(b.Before);
			return c != 0 ? c : a.Now.CompareTo(b.Now);
		};

		/// <summary>The last frame's commands that slid between the two steps, and those drawn where one step has them (for the overlay).</summary>
		public static int Blended, Snapped;

		/// <summary>Whether a list of the step before is kept to draw from.</summary>
		public static bool HasBefore => _before.Count > 0;

		/// <summary>The engine's frame begins (EngineHost.Tick, once a step): the list about to be cleared becomes the step before's.</summary>
		public static void Keep(IReadOnlyList<DrawCommand> commands)
		{
			_before.Clear();
			if (_cut) _cut = false;
			else for (int i = 0; i < commands.Count; i++) _before.Add(commands[i]);
			_kept++;
		}

		/// <summary>The scene changed under the list (a battle, the field again): the next list is drawn as it is, from nothing before it.</summary>
		public static void Cut()
		{
			_before.Clear();
			_cut = true;
		}

		/// <summary>
		/// The commands to draw at <paramref name="t"/> between the step before (0) and this step (1), FramePacer.Blend:
		/// the list itself at 1 or with nothing kept; else, from whichever step is nearer, each command at its pair's
		/// place between the two.
		/// </summary>
		public static IReadOnlyList<DrawCommand> At(IReadOnlyList<DrawCommand> now, float t)
		{
			Blended = 0;
			Snapped = 0;
			if (t >= 0.999f || _before.Count == 0 || _failed == _kept) return now;
			t = Math.Max(0f, t);
			try
			{
				if (_paired != _kept || _pairedCount != now.Count) Pair(now);
			}
			catch (Exception ex)
			{
				// A mod's group whose Equals or GetHashCode throws: this step's list is drawn as it stands.
				_failed = _kept;
				Log.First(LogChannel.General, "mod-draw-blend", 3, () => "engine: draw list not blended: " + ex.GetType().Name + ": " + ex.Message);
				return now;
			}
			// The nearer step's list, as FrameCapture takes the nearer step's draws: the step before's until half-way.
			bool early = t < 0.5f;
			IReadOnlyList<DrawCommand> from = early ? _before : now;
			int[] pairs = early ? _pairOfBefore : _pairOfNow;
			// The camera cut under the list: FrameCapture draws the scene as the nearer step has it, and what the mods set
			// over it by that step's camera (a tag, a damage number) goes the same way, nothing sliding across the cut.
			bool cut = FrameCapture.SceneCut;
			_frame.Clear();
			for (int i = 0; i < from.Count; i++)
			{
				DrawCommand c = from[i];
				int other = pairs[i];
				if (other >= 0)
				{
					DrawCommand a = early ? c : _before[other], b = early ? now[other] : c;
					if (!cut && Slid(a, b))
					{
						Between(ref c, a, b, t);
						Blended++;
					}
					else Snapped++;
				}
				_frame.Add(c);
			}
			return _frame;
		}

		/// <summary>Each command of this step's list with the step before's it was, both ways; -1 for none. Once a step.</summary>
		private static void Pair(IReadOnlyList<DrawCommand> now)
		{
			int before = _before.Count, count = now.Count;
			Room(ref _pairOfBefore, before);
			Room(ref _runOfBefore, before);
			Room(ref _order, before);
			Room(ref _pairOfNow, count);
			Room(ref _runOfNow, count);
			_snap = Snap();
			_counts.Clear();
			_slots.Clear();
			_runs.Clear();
			int runs = 0;
			// The step before's list: a grouped command by its place in its group's order, the rest counted into their key's run.
			for (int j = 0; j < before; j++)
			{
				_pairOfBefore[j] = -1;
				_runOfBefore[j] = -1;
				DrawCommand c = _before[j];
				int key = Key(c);
				if (c.Group != null)
				{
					_slots[Slot(key, Next(key))] = j;
					continue;
				}
				if (!_runs.TryGetValue(key, out int r))
				{
					r = runs++;
					Room(ref _run, runs);
					_run[r] = default;
					_runs.Add(key, r);
				}
				_run[r].Count++;
				_runOfBefore[j] = r;
			}
			for (int r = 0, start = 0; r < runs; r++)
			{
				_run[r].Start = start;
				start += _run[r].Count;
				_run[r].Count = 0;
			}
			for (int j = 0; j < before; j++)
			{
				int r = _runOfBefore[j];
				if (r >= 0) _order[_run[r].Start + _run[r].Count++] = j;
			}
			// This step's list: a grouped command with the one in the same place in its group's order; the rest with each
			// of the step before's alike and near enough to have moved there, of those about its own place in the run.
			Span<DrawCommand> old = CollectionsMarshal.AsSpan(_before);
			_counts.Clear();
			int pairings = 0;
			for (int i = 0; i < count; i++)
			{
				_pairOfNow[i] = -1;
				_runOfNow[i] = -1;
				DrawCommand c = now[i];
				int key = Key(c);
				if (c.Group != null)
				{
					if (_slots.TryGetValue(Slot(key, Next(key)), out int j) && Alike(old[j], c)) Join(i, j);
					continue;
				}
				if (!_runs.TryGetValue(key, out int r)) continue;
				_runOfNow[i] = r;
				ref Run run = ref _run[r];
				int n = run.Met++;
				for (int k = Math.Max(0, n - Reach), end = Math.Min(run.Count, n + Reach + 1); k < end; k++)
				{
					int j = _order[run.Start + k];
					float d = Moved(old[j], c);
					if (d > _snap || !Alike(old[j], c)) continue;
					Room(ref _pairings, pairings + 1);
					_pairings[pairings++] = new Pairing { Distance = d, Before = j, Now = i };
				}
			}
			// The nearest pairs first: each command with the nearest like it that no nearer one took.
			_pairings.AsSpan(0, pairings).Sort(NearestFirst);
			for (int p = 0; p < pairings; p++)
			{
				Pairing pairing = _pairings[p];
				if (_pairOfNow[pairing.Now] < 0 && _pairOfBefore[pairing.Before] < 0) Join(pairing.Now, pairing.Before);
			}
			// What is left went further than motion does (a cursor's hop, a list scrolled): with the first like it still
			// unpaired, in the order drawn - a change, drawn where one step has it.
			for (int i = 0; i < count; i++)
			{
				int r = _runOfNow[i];
				if (r < 0 || _pairOfNow[i] >= 0) continue;
				ref Run run = ref _run[r];
				while (run.Free < run.Count && _pairOfBefore[_order[run.Start + run.Free]] >= 0) run.Free++;
				DrawCommand c = now[i];
				for (int k = run.Free; k < run.Count; k++)
				{
					int j = _order[run.Start + k];
					if (_pairOfBefore[j] < 0 && Alike(old[j], c))
					{
						Join(i, j);
						break;
					}
				}
			}
			_paired = _kept;
			_pairedCount = count;
		}

		private static void Join(int now, int before)
		{
			_pairOfNow[now] = before;
			_pairOfBefore[before] = now;
		}

		private static void Room<T>(ref T[] array, int size)
		{
			if (array.Length < size) Array.Resize(ref array, Math.Max(size, array.Length * 2));
		}

		/// <summary>
		/// How far a command may go in this step and still be moving: SnapDistance for a step of up to three of the game's
		/// passes (its own pace, Tab's, a catch-up), and as much further for a step that ran more of them (--speed, where a
		/// tag over a walker goes as much further), up to MostSnap. What the step stood for is GameClock's.
		/// </summary>
		private static float Snap()
		{
			double passes = GameClock.StepSpan / FramePacer.Period;
			return (float)Math.Min(MostSnap, SnapDistance * Math.Max(1.0, passes / 3.0));
		}

		/// <summary>How many grouped commands with this key the list has had so far: the n in "the n-th command drawing that" within a group.</summary>
		private static int Next(int key)
		{
			_counts.TryGetValue(key, out int n);
			_counts[key] = n + 1;
			return n;
		}

		private static long Slot(int key, int n) => ((long)key << 32) | (uint)n;

		/// <summary>What a command draws, as a number: everything but its place and its alpha.</summary>
		private static int Key(in DrawCommand c)
		{
			int picture = c.Texture == null ? 0 : RuntimeHelpers.GetHashCode(c.Texture);
			int words = c.Text == null ? 0 : c.Text.GetHashCode();
			int colour = c.Color.R | c.Color.G << 8 | c.Color.B << 16;
			int shape;
			switch (c.Kind)
			{
				case DrawKind.Rect: shape = HashCode.Combine(c.W, c.H, c.Filled); break;
				case DrawKind.Line: shape = c.W.GetHashCode(); break;   // its thickness: the ends are its place
				case DrawKind.Sprite: shape = HashCode.Combine(c.W, c.H, c.SrcX, c.SrcY, c.SrcW, c.SrcH); break;
				default: shape = c.Size; break;
			}
			return HashCode.Combine((int)c.Kind, picture, words, colour, shape, c.Group == null ? 0 : c.Group.GetHashCode());
		}

		/// <summary>Whether two commands draw the same thing (the key's parts themselves, so two keys that happen to meet pair nothing).</summary>
		private static bool Alike(in DrawCommand a, in DrawCommand b)
		{
			if (a.Kind != b.Kind || a.Color.R != b.Color.R || a.Color.G != b.Color.G || a.Color.B != b.Color.B) return false;
			if (!ReferenceEquals(a.Texture, b.Texture) || !string.Equals(a.Text, b.Text, StringComparison.Ordinal) || !object.Equals(a.Group, b.Group)) return false;
			switch (a.Kind)
			{
				case DrawKind.Rect: return a.W == b.W && a.H == b.H && a.Filled == b.Filled;
				case DrawKind.Line: return a.W == b.W;
				case DrawKind.Sprite: return a.W == b.W && a.H == b.H && a.SrcX == b.SrcX && a.SrcY == b.SrcY && a.SrcW == b.SrcW && a.SrcH == b.SrcH;
				default: return a.Size == b.Size;
			}
		}

		/// <summary>Whether a pair moved as motion does in a step: a few units at most (both ends of a line; more for a step of --speed's, Snap), a quarter turn at most.</summary>
		private static bool Slid(in DrawCommand a, in DrawCommand b)
		{
			if (Moved(a, b) > _snap) return false;
			if (a.Kind == DrawKind.Sprite && Math.Abs(Arc(a.Rotation, b.Rotation)) > SnapTurn) return false;
			return true;
		}

		/// <summary>How far a command went from one step to the next: its corner, or the further of a line's two ends.</summary>
		private static float Moved(in DrawCommand a, in DrawCommand b)
		{
			float d = Distance(a.X, a.Y, b.X, b.Y);
			return a.Kind == DrawKind.Line ? Math.Max(d, Distance(a.X2, a.Y2, b.X2, b.Y2)) : d;
		}

		/// <summary>The drawn command's place, ends, turn and alpha at t between the pair; the rest is the nearer step's.</summary>
		private static void Between(ref DrawCommand c, in DrawCommand a, in DrawCommand b, float t)
		{
			c.X = a.X + (b.X - a.X) * t;
			c.Y = a.Y + (b.Y - a.Y) * t;
			if (c.Kind == DrawKind.Line)
			{
				c.X2 = a.X2 + (b.X2 - a.X2) * t;
				c.Y2 = a.Y2 + (b.Y2 - a.Y2) * t;
			}
			if (c.Kind == DrawKind.Sprite) c.Rotation = a.Rotation + Arc(a.Rotation, b.Rotation) * t;
			c.Color.A = (byte)Math.Clamp((int)MathF.Round(a.Color.A + (b.Color.A - a.Color.A) * t), 0, 255);
		}

		private static float Distance(float x0, float y0, float x1, float y1)
		{
			float dx = x1 - x0, dy = y1 - y0;
			return MathF.Sqrt(dx * dx + dy * dy);
		}

		/// <summary>The shorter way round from one turn to another, in radians.</summary>
		private static float Arc(float from, float to)
		{
			float d = (to - from) % (2f * MathF.PI);
			if (d > MathF.PI) d -= 2f * MathF.PI;
			else if (d < -MathF.PI) d += 2f * MathF.PI;
			return d;
		}
	}
}
