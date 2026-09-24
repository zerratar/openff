// The frame pacer against simulated displays: the client's own FrameClock and PresentPlan
// (compiled in from OpenFF/Compat), driven by a model of the game loop and of how a present
// reaches the screen, beside a line-by-line port of 0.1.7's FramePacer for comparison.
//
//   dotnet run -c Release --project Tools/FramePacerSim                 the full table
//   dotnet run -c Release --project Tools/FramePacerSim -- quick        fewer rates, shorter runs
//   dotnet run -c Release --project Tools/FramePacerSim -- replay <frames.csv>
//                    a recorded run's Steps() times (a "tsteps" column) through the new clock
//
// The loop: a frame starts when the last present returns (plus the thread's wake-up jitter),
// takes its steps, draws (longer on a step's frame, now and then a heavy one), waits out
// Hold, and presents. The display: a refresh with a random phase, and one of
//   queue1  the swap waits only once a frame is already waiting (one frame of render-ahead)
//   strict  the swap returns when its own frame is up
//   latest  the swap never waits and the compositor shows the latest frame at each refresh -
//           VSync not holding the loop at all
// VSync off shows each frame the moment it is presented.
//
// What is measured, over the pictures that reach the screen after the first three seconds:
// the game's steps a second (30.00 without stalls), and each picture's motion against the time
// it stood - its speed, 1 when even - as a spread, and how many stood still (repeated) or
// jumped twice as far (doubled); how many refreshes each picture was held, and how often that
// differs from the usual. Stalls (a load at one second, a hitch every seventeen) are left out
// of the motion and counted apart. Among the harder cases: a refresh the system misreads (1 Hz,
// Windows' "hardware default"; 20 Hz), a 540 Hz display, and a frame every two seconds drawn a
// refresh too long, where the catch-up steps that come apart from the picture held too long -
// a second stutter some frames after the first - are counted too.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OpenFF.Client
{
	internal enum Present { Queue1, Strict, Latest }

	internal sealed class Scenario
	{
		public double Hz = 60;
		public bool VSync = true;
		public string Fps = "60";
		public Present Model = Present.Queue1;
		public double JitterMs = 0.3;
		public bool Stalls;
		/// <summary>The refresh as SDL reports it, a whole number (59 for 59.94), not the exact one.</summary>
		public bool WholeHz;
		/// <summary>A driver that takes a swap interval over one as one.</summary>
		public bool IgnoresSwap;
		/// <summary>--renderer=emulated: nothing captured, each frame waits for its step (GameHost's other branch).</summary>
		public bool Emulated;
		/// <summary>The refresh the system reports when it is wrong (1: Windows' "hardware default" passed on by SDL, the display configuration not found); below 0, reported as it is.</summary>
		public double ReportHz = -1;
		/// <summary>Every two seconds a frame this many milliseconds longer to draw: a refresh missed.</summary>
		public double HeavyMs;
		public double Seconds = 60;
		public int Seed = 1;

		public string Name => Hz.ToString("0.##", CultureInfo.InvariantCulture) + " Hz " + (VSync ? Model.ToString().ToLowerInvariant() : "novsync") + " fps=" + Fps
			+ (Stalls ? " stalls" : "") + (WholeHz ? " whole-Hz" : "") + (IgnoresSwap ? " swap-ignored" : "") + (Emulated ? " emulated" : "")
			+ (ReportHz >= 0 ? " read-" + ReportHz.ToString("0.##", CultureInfo.InvariantCulture) + "Hz" : "") + (HeavyMs > 0 ? " missed" : "") + " jit " + JitterMs.ToString("0.0", CultureInfo.InvariantCulture);
	}

	internal interface IPacer
	{
		int Steps(double now);
		double Blend { get; }
		/// <summary>Hold at this time: when the frame goes to the present.</summary>
		double Hold(double now, Random rnd);
		/// <summary>The swap interval the pacer has set (0 VSync off).</summary>
		int Swap { get; }
		string Describe { get; }
		/// <summary>The emulated renderer (nothing captured): when the frame's wait for its step ends, and the steps it takes then.</summary>
		double WaitTarget { get; }
		int AfterWait(double now);
	}

	/// <summary>0.1.7's FramePacer (Steps, Blend, Hold), line by line, with Hold's SpinOnce() overshoot as measured (p50 0.8 ms, p99 2.1 ms).</summary>
	internal sealed class Shipped : IPacer
	{
		private const double Period = 1.0 / 30.0;
		private double _last = -1, _debt, _drift, _nextPresent, _steppedAt;
		private readonly Scenario _s;
		public Shipped(Scenario s) { _s = s; }
		private int Cap => _s.Fps == "30" ? 30 : _s.Fps == "60" ? 60 : 240;
		public int Swap => _s.VSync ? 1 : 0;
		public string Describe => "0.1.7 cap " + Cap;
		// 0.1.7's WaitForStep: to a period after the last step, and GameHost then ran one step, the
		// wait left on the pacer's clock for the next frame to count again.
		public double WaitTarget => _steppedAt + Period;
		public int AfterWait(double now) => 1;

		public int Steps(double now)
		{
			int n = StepsInner(now);
			if (n > 0) _steppedAt = now;
			return n;
		}

		private int StepsInner(double now)
		{
			if (_last < 0) { _last = now; _debt = 0; _steppedAt = now; return 1; }
			double real = Math.Min(now - _last, 0.25);
			_last = now;
			double dt = real;
			for (int n = 1; n <= 8; n++)
			{
				double q = Period / n;
				if (Math.Abs(real - q) < q * 0.08) { dt = q; break; }
			}
			_drift += real - dt;
			if (Math.Abs(_drift) > Period / 2) { dt += _drift; _drift = 0; }
			_debt += dt;
			int steps = 0;
			while (_debt >= Period - 1e-6 && steps < 3) { _debt -= Period; steps++; }
			if (_debt >= Period) _debt = 0;
			return steps;
		}

		public double Blend
		{
			get
			{
				if (_s.Fps == "30") return 1;
				double b = Math.Min(1.0, 0.5 + _debt / Period);
				return b >= 0.999 ? 1 : b;   // FrameCapture.Replay blends only under 0.999
			}
		}

		public double Hold(double now, Random rnd)
		{
			double interval = 1.0 / Cap;
			if (_nextPresent <= 0 || now - _nextPresent > interval) _nextPresent = now;
			_nextPresent += interval;
			double wait = _nextPresent - now;
			if (wait <= 0) return now;
			return _nextPresent + SpinOnceOvershoot(rnd, wait);
		}

		// SpinWait.SpinOnce() sleeps a millisecond at a time after twenty turns: any wait over about
		// 0.1 ms overshoots by p50 0.8, p90 1.7, p99 2.1 ms (present-hold/hb).
		private static double SpinOnceOvershoot(Random rnd, double wait)
		{
			if (wait < 0.0001) return 0.00001;
			double u = rnd.NextDouble();
			return Math.Min(0.0023, 0.00005 + 0.0021 * Math.Pow(u, 1.6));
		}
	}

	/// <summary>The new pacer: FrameClock and PresentPlan as compiled into the client, with FramePacer's glue (Replan, GiveWay, the swap interval, the precise wait) as it is there.</summary>
	internal sealed class Current : IPacer
	{
		private readonly Scenario _s;
		private readonly FrameClock _clock = new FrameClock();
		private PresentPlan _plan;
		private bool _planned;
		private PresentGrid _grid;
		private HoldCheck _check;
		private double _frameAt = -1;
		private int _fallback, _mostSwap = 8, _applied = -1;
		public readonly List<string> Log = new List<string>();

		public Current(Scenario s) { _s = s; }

		/// <summary>The refresh as the system reports it: exact (the display configuration), SDL's whole number, or a reading gone wrong.</summary>
		private double ReportedHz => _s.ReportHz >= 0 ? _s.ReportHz : _s.WholeHz ? Math.Floor(_s.Hz + 0.05) : _s.Hz;

		public int Swap => _applied < 0 ? (_s.VSync ? 1 : 0) : (_s.IgnoresSwap && _applied > 1 ? 1 : _applied);
		public string Describe => _plan.Describe();

		public int Steps(double now)
		{
			_clock.Smooth = _s.Fps != "30" && !_s.Emulated;
			Replan();
			double at = _plan.Held || _s.Emulated ? now : Math.Max(_clock.Last, _grid.Peek(now, 1.0 / _plan.Ceiling));
			int steps = _clock.Advance(at);
			if (_frameAt >= 0 && _plan.Held && _check.Frame(now - _frameAt, true, _plan.Rate)) GiveWay(now);
			_frameAt = now;
			return steps;
		}

		public double Blend => _clock.Blend;
		// FramePacer.WaitForStep, as GameHost's emulated branch loops on it.
		public double WaitTarget => _clock.Last + _clock.UntilStep + 1e-4;
		public int AfterWait(double now) => _clock.AdvanceWaited(now);

		public double Hold(double now, Random rnd)
		{
			Replan();
			if (_applied != _plan.Swap) _applied = _plan.Swap;
			double ceiling = _plan.Ceiling;
			double due = _grid.Due(now, 1.0 / ceiling);
			if (due <= now) return now;
			// The precise wait: the high-resolution timer to within a millisecond, then SpinOnce(-1) - p99 about 10 us.
			return due + 0.000003 + 0.000012 * rnd.NextDouble();
		}

		public PacingWindow TakeWindow() => _clock.TakeWindow();
		public double UntilStep => _clock.UntilStep;
		public double Drift => _clock.Drift;
		public string Fps => _s.Fps;

		private void Replan()
		{
			PresentPlan plan = PresentPlan.Choose(_s.VSync, _s.Fps, ReportedHz, _fallback, _mostSwap);
			if (_planned && plan.SameAs(_plan)) return;
			_planned = true;
			_plan = plan;
			_clock.SetTiming(plan.Cadence > 0 && !_s.Emulated ? 1.0 / plan.RefreshHz : 0, plan.Cadence);
			_grid.Reset();
			_check.Reset();
			Log.Add("plan: " + plan.Describe());
		}

		private void GiveWay(double now)
		{
			int was = _fallback;
			_fallback = _plan.Swap > 1 && _fallback < 1 ? 1 : 2;
			if (_fallback == was) return;
			Log.Add("gave way at " + now.ToString("0.0") + " s (frames every " + (_check.MeanInterval * 1000).ToString("0.00") + " ms): fallback " + _fallback);
			_planned = false;
		}
	}

	/// <summary>A display and the loop feeding it.</summary>
	internal sealed class Rig
	{
		private const double Period = 1.0 / 30.0;
		private readonly Scenario _s;
		private readonly IPacer _p;
		private readonly Random _rnd;
		private readonly double _refresh, _phase;
		private int _lastFlip = int.MinValue;
		private readonly List<int> _pending = new List<int>();
		// The pictures that reached the screen: the refresh (or, VSync off, the time) and where the game stood in it.
		private readonly List<(double At, double Pos, long Key)> _shown = new List<(double, double, long)>();
		public long StepsMeasured;
		public double MeasuredFrom = 3.0;
		public int Frames;
		private double _firstAt = -1, _lastAt;

		public Rig(Scenario s, IPacer p)
		{
			_s = s;
			_p = p;
			_rnd = new Random(s.Seed);
			_refresh = 1.0 / s.Hz;
			_phase = _rnd.NextDouble() * _refresh;
		}

		private double VBlank(int n) => _phase + n * _refresh;
		private int VAfter(double t) => (int)Math.Ceiling((t - _phase) / _refresh - 1e-9);

		private double Jitter()
		{
			double j = Math.Abs(Gauss()) * _s.JitterMs / 1000;
			if (_rnd.NextDouble() < 0.05) j += -Math.Log(1 - _rnd.NextDouble()) * 0.0015 * Math.Min(1, _s.JitterMs / 0.3);
			return j;
		}

		private double Gauss() => Math.Sqrt(-2 * Math.Log(1 - _rnd.NextDouble())) * Math.Cos(2 * Math.PI * _rnd.NextDouble());

		public void Run()
		{
			double t = 0.01;
			long total = 0;
			int lastSteps = 1;
			double nextHitch = 17.0, nextHeavy = 2.0;
			bool loaded = false;
			while (t < _s.Seconds)
			{
				t += 0.0002 + Jitter();                       // the loop's top: input, the frame's start
				int steps = _p.Steps(t);
				double at = t;
				// The emulated renderer: the frame waits out the time to its step and takes it.
				if (_s.Emulated) while (steps == 0) { t = Math.Max(t, _p.WaitTarget) + 0.00001; steps = _p.AfterWait(t); }
				if (at >= MeasuredFrom)
				{
					if (_firstAt < 0) _firstAt = at;
					else StepsMeasured += steps;
					_lastAt = at;
				}
				if (steps > 0) { total += steps; lastSteps = steps; }
				double blend = _s.Emulated ? 1 : _p.Blend;
				double pos = total - lastSteps + lastSteps * blend;   // Replay: the frame before the last Step call toward its own
				double work = 0.0012 + steps * 0.0020 + (_rnd.NextDouble() < 0.01 ? 0.006 : 0);
				if (_s.Stalls && !loaded && t > 1.0) { work += 0.4; loaded = true; }
				if (_s.Stalls && t > nextHitch) { work += 0.08; nextHitch += 17.0; }
				if (_s.HeavyMs > 0 && t > nextHeavy) { work += _s.HeavyMs / 1000; nextHeavy += 2.0; }
				t += work;
				t = _p.Hold(t, _rnd);
				t = PresentAt(t, pos, Frames);
				Frames++;
			}
		}

		// The present: where the picture lands and when the swap returns.
		private double PresentAt(double tp, double pos, long key)
		{
			int swap = _p.Swap;
			if (!_s.VSync || swap == 0)
			{
				_shown.Add((tp, pos, key));
				return tp + 0.00005;
			}
			if (_s.Model == Present.Latest)
			{
				int v = VAfter(tp + 0.0003);
				if (_shown.Count > 0 && _shown[^1].At == VBlank(v)) _shown[^1] = (VBlank(v), pos, key);
				else _shown.Add((VBlank(v), pos, key));
				return tp + 0.00005;
			}
			int flip = Math.Max(VAfter(tp), _lastFlip == int.MinValue ? int.MinValue : _lastFlip + swap);
			_lastFlip = flip;
			_shown.Add((VBlank(flip), pos, key));
			_pending.RemoveAll(f => VBlank(f) <= tp);
			_pending.Add(flip);
			int queue = _s.Model == Present.Strict ? 0 : 1;
			if (_pending.Count > queue) return Math.Max(tp, VBlank(_pending[_pending.Count - 1 - queue])) + 0.00002;
			return tp + 0.00005;
		}

		public sealed class Result
		{
			/// <summary>Steps a second between the first and last frame measured, and the steps over or under thirty a second over that time.</summary>
			public double StepsPerSecond, Owed, SpeedSd, MeanSpeed, HeldUsual, UnevenHold;
			public int Pictures, Repeated, Doubled, Stalls;
			/// <summary>Doubled pictures that do not come right after one held longer than the usual: a catch-up apart from the hitch it makes up for.</summary>
			public int Apart;
			public string Holds;
		}

		public Result Measure()
		{
			Result r = new Result();
			double span = _lastAt - _firstAt;
			r.StepsPerSecond = StepsMeasured / span;
			r.Owed = StepsMeasured - span * 30;
			List<double> speeds = new List<double>();
			Dictionary<int, int> holds = new Dictionary<int, int>();
			List<int> doubled = new List<int>();
			for (int i = 1; i < _shown.Count; i++)
			{
				if (_shown[i - 1].At < MeasuredFrom) continue;
				double dt = _shown[i].At - _shown[i - 1].At;
				if (dt <= 0) continue;
				if (dt > 0.1) { r.Stalls++; continue; }
				double speed = (_shown[i].Pos - _shown[i - 1].Pos) / (dt / Period);
				speeds.Add(speed);
				if (speed < 0.25) r.Repeated++;
				else if (speed > 1.75) { r.Doubled++; doubled.Add(i); }
				if (_s.VSync && _p.Swap > 0)
				{
					int h = (int)Math.Round(dt / _refresh);
					holds[h] = holds.TryGetValue(h, out int c) ? c + 1 : 1;
				}
			}
			r.Pictures = speeds.Count;
			if (speeds.Count > 1)
			{
				r.MeanSpeed = speeds.Average();
				r.SpeedSd = Math.Sqrt(speeds.Sum(x => (x - r.MeanSpeed) * (x - r.MeanSpeed)) / speeds.Count);
			}
			if (holds.Count > 0)
			{
				int n = holds.Values.Sum();
				var usual = holds.OrderByDescending(kv => kv.Value).First();
				r.HeldUsual = usual.Key;
				r.UnevenHold = 1.0 - usual.Value / (double)n;
				r.Holds = string.Join(" ", holds.OrderBy(kv => kv.Key).Where(kv => kv.Value * 200 >= n).Select(kv => kv.Key + ":" + (100.0 * kv.Value / n).ToString("0.#", CultureInfo.InvariantCulture) + "%"));
				// A catch-up that makes up for a picture held too long belongs right after it; one that comes
				// later is a stutter of its own.
				foreach (int i in doubled)
					if (i < 2 || Math.Round((_shown[i - 1].At - _shown[i - 2].At) / _refresh) <= r.HeldUsual) r.Apart++;
			}
			else r.Holds = "-";
			return r;
		}
	}

	internal static class Program
	{
		private static int Main(string[] args)
		{
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
			CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
			if (args.Length >= 2 && args[0] == "replay") return Replay(args[1]);
			if (args.Length >= 4 && args[0] == "trace") return Trace(double.Parse(args[1]), args[2], Enum.Parse<Present>(args[3], true), args.Length > 4 ? double.Parse(args[4]) : 0.3);
			bool quick = args.Length > 0 && args[0] == "quick";
			double[] rates = quick ? new[] { 59.94, 60, 144, 240 } : new[] { 59.94, 60, 75, 90, 100, 120, 143.9, 144, 165, 240 };
			double seconds = quick ? 30 : 120;
			Console.WriteLine("steps/s: the game's rate after 3 s (30.00 without stalls). speed: each picture's motion against the time it stood (1 = even):");
			Console.WriteLine("sd, and pictures that stood still (rep) or jumped 1.75x or more (dbl). held: refreshes each picture stood (share); uneven: pictures held other than the usual.");
			Console.WriteLine();
			int bad = 0;
			foreach (string fps in new[] { "60", "max", "30" })
			foreach (bool vsync in new[] { true, false })
			foreach (Present model in vsync ? new[] { Present.Queue1, Present.Strict, Present.Latest } : new[] { Present.Queue1 })
			{
				Console.WriteLine("== fps=" + fps + (vsync ? " vsync " + model.ToString().ToLowerInvariant() : " novsync"));
				Console.WriteLine(string.Format("{0,-8} {1,-44} {2,-44}", "Hz", "0.1.7: steps/s  sd   rep  dbl  held (uneven)", "new: steps/s  sd   rep  dbl  held (uneven)"));
				foreach (double hz in rates)
				{
					Scenario s = new Scenario { Hz = hz, VSync = vsync, Fps = fps, Model = model, Seconds = seconds, JitterMs = 0.3 };
					(Rig.Result a, _) = Run(s, false);
					(Rig.Result b, Current c) = Run(s, true);
					if (Math.Abs(b.Owed) > 1.5) bad++;
					Console.WriteLine(string.Format("{0,-8} {1,-44} {2,-44} {3}", hz.ToString("0.##"), Row(a), Row(b), string.Join("; ", c.Log.Skip(1))));
				}
				Console.WriteLine();
			}
			Console.WriteLine("== harder cases (new pacer; 0.1.7 beside it)");
			var hard = new List<Scenario>
			{
				new Scenario { Hz = 59.94, WholeHz = true },
				new Scenario { Hz = 119.88, WholeHz = true },
				new Scenario { Hz = 143.9, WholeHz = true, Fps = "max" },
				new Scenario { Hz = 143.9, WholeHz = true, Fps = "30" },
				new Scenario { Hz = 60.02 },
				new Scenario { Hz = 120.1, Fps = "max" },
				new Scenario { Hz = 120, IgnoresSwap = true },
				new Scenario { Hz = 240, IgnoresSwap = true },
				new Scenario { Hz = 60, JitterMs = 1.0 },
				new Scenario { Hz = 144, JitterMs = 1.0, Fps = "max" },
				new Scenario { Hz = 240, JitterMs = 1.0, Fps = "max" },
				new Scenario { Hz = 60, Stalls = true },
				new Scenario { Hz = 144, Stalls = true, Fps = "max" },
				new Scenario { Hz = 144, Stalls = true },
				new Scenario { Hz = 60, Model = Present.Strict, Fps = "30" },
				new Scenario { Hz = 144, Model = Present.Strict, Fps = "30" },
				new Scenario { Hz = 60, ReportHz = 1 },
				new Scenario { Hz = 60, ReportHz = 1, Fps = "max" },
				new Scenario { Hz = 60, ReportHz = 20 },
				new Scenario { Hz = 540 },
				new Scenario { Hz = 60, Model = Present.Strict, Fps = "30", HeavyMs = 38 },
				new Scenario { Hz = 120, Model = Present.Strict, Fps = "30", HeavyMs = 38 },
				new Scenario { Hz = 60, Model = Present.Strict, HeavyMs = 22 },
			};
			foreach (Scenario s in hard)
			{
				s.Seconds = seconds * 2;
				(Rig.Result a, _) = Run(s, false);
				(Rig.Result b, Current c) = Run(s, true);
				if (!s.Stalls && Math.Abs(b.Owed) > 1.5) bad++;
				Console.WriteLine(string.Format("{0,-44} {1,-44} {2,-44} {3}", s.Name, Row(a), Row(b), string.Join("; ", c.Log.Skip(1))));
				PacingWindow w = c.TakeWindow();
				Console.WriteLine(string.Format("{0,-44} pacer's own window: {1:0.0} fps {2:0.00} steps/s motion +-{3:0.0}% (+-{4:0.0}%) rep {5} dbl {6} missed {7} unsnapped {8} stalls {9} drift {10:0.0} ms measured {11:0.000} Hz; catch-ups apart from a held picture: 0.1.7 {12}, new {13}",
					"", w.Fps, w.StepsPerSecond, w.AdvanceSd * 100, w.MotionSd * 100, w.Repeated, w.Doubled, w.Missed, w.Unsnapped, w.Stalls, w.DriftMs, w.MeasuredHz, a.Apart, b.Apart));
			}
			Console.WriteLine();
			Console.WriteLine("== --renderer=emulated: the game's steps a second (each frame waits for its step)");
			foreach (string fps in new[] { "60", "max", "30" })
			foreach (double hz in new[] { 60.0, 75, 120, 144 })
			{
				Scenario s = new Scenario { Hz = hz, Fps = fps, Emulated = true, Seconds = seconds };
				(Rig.Result a, _) = Run(s, false);
				(Rig.Result b, _) = Run(s, true);
				if (Math.Abs(b.Owed) > 1.5) bad++;
				Console.WriteLine(string.Format("{0,-40} 0.1.7 {1,7:0.000} steps/s   new {2,7:0.000} steps/s", s.Name, a.StepsPerSecond, b.StepsPerSecond));
			}
			Console.WriteLine();
			Console.WriteLine("== --speed=4 with Tab held: the game's steps a second (0.1.7 ran three extra single steps every display frame; now each paced step runs four)");
			foreach ((double hz, string fps) in new[] { (60.0, "60"), (144.0, "60"), (144.0, "max"), (240.0, "max") })
			{
				Scenario s = new Scenario { Hz = hz, Fps = fps, Seconds = seconds };
				Rig old = new Rig(s, new Shipped(s)); old.Run();
				Rig cur = new Rig(s, new Current(s)); cur.Run();
				double oldFps = old.Frames / s.Seconds, newFps = cur.Frames / s.Seconds;
				Console.WriteLine(string.Format("{0,-40} 0.1.7 {1,6:0} steps/s ({2:0.0}x) at {3:0} fps   new {4,6:0} steps/s ({5:0.0}x) at {6:0} fps",
					s.Name, 30 + 3 * oldFps, (30 + 3 * oldFps) / 30, oldFps, 30 * 4.0, 4.0, newFps));
			}
			Console.WriteLine();
			Console.WriteLine(bad == 0 ? "every run without stalls kept thirty steps a second to within a step and a half" : bad + " run(s) more than a step and a half off thirty a second");
			return bad == 0 ? 0 : 1;
		}

		// One scenario, the new pacer, every frame whose steps differ from the one before's pattern
		// printed with the clock's state: what is owed, the drift, the interval and what it was taken for.
		private static int Trace(double hz, string fps, Present model, double jitter)
		{
			Scenario s = new Scenario { Hz = hz, Fps = fps, Model = model, JitterMs = jitter, Seconds = 60 };
			Current c = new Current(s);
			Tracer t = new Tracer(c);
			Rig rig = new Rig(s, t);
			rig.Run();
			Rig.Result r = rig.Measure();
			Console.WriteLine(s.Name + ": " + Row(r));
			return 0;
		}

		private sealed class Tracer : IPacer
		{
			private readonly Current _c;
			private double _last = -1;
			private int _n;
			private readonly Queue<string> _recent = new Queue<string>();
			public Tracer(Current c) { _c = c; }
			public double Blend => _c.Blend;
			public int Swap => _c.Swap;
			public string Describe => _c.Describe;
			public double WaitTarget => _c.WaitTarget;
			public int AfterWait(double now) => _c.AfterWait(now);
			public double Hold(double now, Random rnd) => _c.Hold(now, rnd);
			public int Steps(double now)
			{
				int steps = _c.Steps(now);
				double owed = FrameClock.Period / 2 - _c.UntilStep;
				string line = string.Format("{0,9:0.00000} frame {1,6} interval {2,7:0.000} ms  steps {3}  owed {4,7:0.000} ms  drift {5,7:0.000} ms  blend {6:0.000}",
					now, _n, _last < 0 ? 0 : (now - _last) * 1000, steps, owed * 1000, _c.Drift * 1000, _c.Blend);
				_recent.Enqueue(line);
				if (_recent.Count > 4) _recent.Dequeue();
				if (now > 3 && steps != 1 && _c.Fps == "30")
				{
					foreach (string l in _recent) Console.WriteLine(l);
					Console.WriteLine();
					_recent.Clear();
				}
				_last = now;
				_n++;
				return steps;
			}
		}

		private static (Rig.Result, Current) Run(Scenario s, bool current)
		{
			Current c = current ? new Current(s) : null;
			Rig rig = new Rig(s, current ? c : new Shipped(s));
			rig.Run();
			return (rig.Measure(), c);
		}

		private static string Row(Rig.Result r) =>
			string.Format("{0,7:0.000} {1,6:0.000} {2,4} {3,4}  {4} ({5:0.0}%)", r.StepsPerSecond, r.SpeedSd, r.Repeated, r.Doubled, r.Holds.Length > 20 ? r.Holds.Substring(0, 20) : r.Holds, r.UnevenHold * 100);

		// A recorded run's Steps() times through the new clock, snapping to the refresh given (or none),
		// measured the way the review measured the 0.1.7 clock on them: each frame's motion against half a step.
		private static int Replay(string path)
		{
			string[] lines = File.ReadAllLines(path);
			int col = Array.IndexOf(lines[0].Split(','), "tsteps");
			double[] ts = lines.Skip(1).Select(l => double.Parse(l.Split(',')[col], CultureInfo.InvariantCulture)).ToArray();
			foreach ((string name, double hz) in new[] { ("0.1.7", -1.0), ("new, raw", 0.0), ("new, snapped to 59.997 Hz", 59.997), ("new, snapped to 60 Hz (a whole-number report)", 60.0) })
			{
				FrameClock clock = new FrameClock();
				clock.SetTiming(hz > 0 ? 1 / hz : 0, 1);
				Shipped old = hz < 0 ? new Shipped(new Scenario { Fps = "60" }) : null;
				long total = 0; int last = 1;
				List<double> pos = new List<double>(), at = new List<double>();
				foreach (double t in ts)
				{
					int n = old != null ? old.Steps(t) : clock.Advance(t);
					if (n > 0) { total += n; last = n; }
					double b = old != null ? old.Blend : clock.Blend;
					pos.Add(total - last + last * b);
					at.Add(t);
				}
				List<double> m = new List<double>();
				for (int i = 1; i < pos.Count; i++) if (at[i] - at[i - 1] < 0.03) m.Add((pos[i] - pos[i - 1]) / 0.5);
				m = m.Skip(30).ToList();
				double mean = m.Average(), sd = Math.Sqrt(m.Sum(x => (x - mean) * (x - mean)) / m.Count);
				Console.WriteLine(string.Format("{0,-48} sd {1:0.000} repeat {2:0.000} double {3:0.000} off25 {4:0.000} min {5:0.00} max {6:0.00} steps {7} ({8:0.000}/s)",
					name, sd, m.Count(x => x < 0.05) / (double)m.Count, m.Count(x => x > 1.9) / (double)m.Count, m.Count(x => Math.Abs(x - 1) > 0.25) / (double)m.Count, m.Min(), m.Max(), total, total / (ts[^1] - ts[0])));
				if (old == null)
				{
					PacingWindow w = clock.TakeWindow();
					Console.WriteLine(string.Format("{0,-48} pacer's window: {1:0.0} fps {2:0.000} steps/s motion +-{3:0.0}% (+-{4:0.0}%) rep {5} dbl {6} unsnapped {7} stalls {8} measured {9:0.000} Hz", "", w.Fps, w.StepsPerSecond, w.AdvanceSd * 100, w.MotionSd * 100, w.Repeated, w.Doubled, w.Unsnapped, w.Stalls, w.MeasuredHz));
				}
			}
			return 0;
		}
	}
}
