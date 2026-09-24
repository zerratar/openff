// When the game steps, where between two of its steps the display stands, and when a frame
// goes out.
//
// The game is a DS-era engine counted in frames: every walk, camera move, script wait, text
// speed and battle timer is so many calls of NitroMain, thirty a second. That rate is the
// game, and it is not touched here. What this decides, once per frame the display draws, is
//
//   Steps()  how many of those 1/30 s steps have come due (0, or 1..3 when catching up), and
//   Blend()  how far the display should stand between the step before and the one just
//            taken - 0..1, moving by exactly the frame's own time each frame - for
//            FrameCapture to draw the frame in between (Fps "60" / "max"); 1 always when Fps
//            is "30" or nothing is captured to blend.
//
// The arithmetic is FrameClock's, with this Stopwatch's time handed in: an accumulator that
// steps once half a step is owed, intervals snapped to whole refreshes while VSync holds the
// loop, stalls forgiven. See FrameClock.cs.
//
// The presentation is PresentPlan's: the display's refresh is read from the system every
// second (DisplayTiming; a reading no display has is taken as not known, and nothing is paced
// below thirty a second), and the frames are held the most whole refreshes that still give
// the Fps setting's rate - the swap interval, set here before the present - so the swap does
// the waiting on the display's own beat, and Hold() is only a ceiling a quarter above the
// rate. Two seconds of frames coming faster than that (a present that never waits) and the
// plan gives way: every refresh, then the clock pacing the rate itself. With VSync off Hold
// paces 30, 60, or the display's rate (240 at least) for "max". Its wait is precise - a
// high-resolution timer to within a millisecond, then a spin that never sleeps - since
// SpinWait's own SpinOnce() falls back to Thread.Sleep(1), up to two milliseconds late.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace OpenFF.Client
{
	// MonoGame's Game by name: the engine's OpenFF.Game sits a namespace up.
	using Game = Microsoft.Xna.Framework.Game;

	internal static class FramePacer
	{
		/// <summary>The game's step: 1/30 s.</summary>
		public const double Period = FrameClock.Period;

		/// <summary>The most --speed multiplies a step by.</summary>
		public const int MostSpeed = 64;

		/// <summary>The most steps render() runs in one frame: three to catch up, times --speed's most.</summary>
		public const int MostPending = FrameClock.MostCatchUp * MostSpeed;

		private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
		private static readonly FrameClock _clock = new FrameClock();
		private static Game _game;
		private static PresentPlan _plan;
		private static bool _planned;
		private static PresentGrid _grid;
		private static HoldCheck _check;
		private static double _frameAt = -1;

		// What the plan is made from: the settings and the refresh as last read, how far it has given
		// way (PresentPlan.Choose's fallback), and the largest swap interval that can be set.
		private static bool _vsyncIn;
		private static string _fpsIn;
		private static double _refreshHz;
		private static double _refreshReadAt = -1;
		private static int _fallback;
		private static int _mostSwap = 8;
		private static int _appliedSwap = -1;

		/// <summary>How many steps the legacy frame is to run this time (render() reads it in place of its own clock); 1 outside a paced frame.</summary>
		public static int PendingSteps = 1;

		/// <summary>Whether frames are drawn between the game's own: Fps "60" or "max", with the native renderer's draws captured to blend.</summary>
		public static bool Smooth => (DisplaySettings.Current?.Fps ?? "60") != "30" && FrameCapture.Supported;

		/// <summary>How the frames go out now.</summary>
		public static PresentPlan Plan => _plan;

		/// <summary>The display's refresh as last read, in Hz (0 when not known; a reading outside 20-1000 Hz PresentPlan takes as not known).</summary>
		public static double RefreshHz => _refreshHz;

		/// <summary>The largest swap interval that can be set here: 8, or 1 when none can (SDL not reached, a driver that refused one).</summary>
		public static int MostSwap => _mostSwap;

		private static double Now => _stopwatch.Elapsed.TotalSeconds;

		/// <summary>The game whose window the refresh is read for and whose focus the VSync check heeds.</summary>
		public static void Attach(Game game) => _game = game;

		/// <summary>The device was made or reset (a VSync change, Alt+Enter, a mode switch): MonoGame has set its own swap interval again, and whether VSync holds is asked afresh.</summary>
		public static void DeviceChanged()
		{
			_appliedSwap = -1;
			_fallback = 0;
			_planned = false;
		}

		/// <summary>The steps due now, 0..3. The first call is one step.</summary>
		public static int Steps()
		{
			double now = Now;
			_clock.Smooth = Smooth;
			Replan(now);
			// Paced by the clock, the frame's picture goes out at its slot on Hold's grid, so that is its
			// time, not the moment it began (a moment the swap's own wait for a refresh has shaken by up
			// to a refresh). Where VSync holds the loop, the moment it began is the refresh's.
			double at = _plan.Held || !FrameCapture.Supported ? now : Math.Max(_clock.Last, _grid.Peek(now, 1.0 / _plan.Ceiling));
			int steps = _clock.Advance(at);
			if (_frameAt >= 0 && _plan.Held && _check.Frame(now - _frameAt, _game == null || _game.IsActive, _plan.Rate)) GiveWay();
			_frameAt = now;
			return steps;
		}

		/// <summary>Where the display stands between the step before and the one just taken, 0..1, moving evenly frame to frame (near 0 on a step's frame when the step has only just come due); 1 always when not smoothing.</summary>
		public static float Blend() => (float)_clock.Blend;

		/// <summary>Called once the frame is drawn, before it is presented: sets the plan's swap interval, and waits for Hold's slot - the plan's rate when the clock paces it, a ceiling above it when VSync holds, which a waiting present never reaches.</summary>
		public static void Hold()
		{
			double now = Now;
			Replan(now);
			ApplySwap();
			double ceiling = _plan.Ceiling;
			if (ceiling <= 0) return;
			WaitUntil(_grid.Due(now, 1.0 / ceiling));
		}

		/// <summary>Without the native renderer nothing is captured to draw in between: the frame waits out the time to the next step, as the phone build did, and takes it (the steps now due).</summary>
		public static int WaitForStep()
		{
			double target = _clock.Last + _clock.UntilStep + 1e-4;
			WaitUntil(target);
			return _clock.AdvanceWaited(Math.Max(Now, target));
		}

		/// <summary>The frames since the last call (FrameClock's window), and a fresh one begun.</summary>
		public static PacingWindow TakeWindow() => _clock.TakeWindow();

		/// <summary>The plan this Fps setting would have now, for the pause menu to say what it will do.</summary>
		public static PresentPlan PlanFor(string fps)
		{
			bool vsync = DisplaySettings.Current?.VSync ?? true;
			return PresentPlan.Choose(vsync, fps, _refreshHz, vsync == _vsyncIn && fps == _fpsIn ? _fallback : 0, _mostSwap);
		}

		private static void Replan(double now)
		{
			DisplaySettings s = DisplaySettings.Current;
			bool vsync = s?.VSync ?? true;
			string fps = s?.Fps ?? "60";
			if (_refreshReadAt < 0 || now - _refreshReadAt >= 1.0)
			{
				_refreshReadAt = now;
				double hz = 0;
				try { hz = DisplayTiming.RefreshHz(_game?.Window?.Handle ?? IntPtr.Zero); } catch (Exception) { }
				if (hz > 0 && Math.Abs(hz - _refreshHz) > 0.001)
				{
					if (_refreshHz > 0) _fallback = 0;   // another display, another mode: ask again whether VSync holds
					_refreshHz = hz;
					if (!PresentPlan.Believable(hz)) Log.Write(LogChannel.General, "pacing: the display's refresh read as " + hz.ToString("0.###", CultureInfo.InvariantCulture) + " Hz, which no display has; taken as not known");
				}
				if (_mostSwap > 1 && !DisplayTiming.CanSetSwapInterval) _mostSwap = 1;
			}
			if (vsync != _vsyncIn || fps != _fpsIn)
			{
				_vsyncIn = vsync;
				_fpsIn = fps;
				_fallback = 0;
			}
			PresentPlan plan = PresentPlan.Choose(vsync, fps, _refreshHz, _fallback, _mostSwap);
			if (_planned && plan.SameAs(_plan)) return;
			_planned = true;
			bool changed = !plan.SameAs(_plan);
			_plan = plan;
			// Snapping to whole refreshes only while the frames go out on them - not for the emulated
			// renderer, whose frames go out when its waits end.
			_clock.SetTiming(plan.Cadence > 0 && FrameCapture.Supported ? 1.0 / plan.RefreshHz : 0, plan.Cadence);
			_grid.Reset();
			_check.Reset();
			if (changed) Log.Write(LogChannel.General, "pacing: fps " + plan.Fps + ", " + plan.Describe() + (plan.RefreshHz > 0 ? " (refresh from " + DisplayTiming.Source + ")" : ""));
		}

		// The check says VSync is not holding the loop to the plan's rate: a swap interval over one gives
		// way to every refresh first, and that to the clock.
		private static void GiveWay()
		{
			int was = _fallback;
			_fallback = _plan.Swap > 1 && _fallback < 1 ? 1 : 2;
			if (_fallback == was) return;
			Log.Write(LogChannel.General, "pacing: frames came every " + (_check.MeanInterval * 1000).ToString("0.0", CultureInfo.InvariantCulture) + " ms, faster than VSync allows at " + _plan.Rate.ToString("0.##", CultureInfo.InvariantCulture) + " a second - "
				+ (_fallback == 1 ? "a swap interval of " + _plan.Swap + " is not holding; every refresh instead" : "VSync is not holding the loop; the clock paces it"));
			_planned = false;
		}

		// The plan's swap interval onto the GL context (the game's thread, before the present). MonoGame
		// sets 1 or 0 itself at every reset, so it is set again after one.
		private static void ApplySwap()
		{
			if (_appliedSwap == _plan.Swap) return;
			_appliedSwap = _plan.Swap;
			if (!DisplayTiming.SetSwapInterval(_plan.Swap) && _plan.Swap > 1)
			{
				Log.Write(LogChannel.General, "pacing: a swap interval of " + _plan.Swap + " could not be set; every refresh instead");
				_mostSwap = 1;
				_planned = false;
			}
		}

		// Sleeps to shortly before the time - on the high-resolution timer to within a millisecond, else by
		// Thread.Sleep's whole milliseconds to within two - then spins the rest, yielding but never
		// sleeping, so the wait ends within microseconds of its time.
		private static void WaitUntil(double target)
		{
			double wait = target - Now;
			if (wait <= 0) return;
			if (!(wait > 0.0015 && DisplayTiming.Sleep(wait - 0.001)) && wait > 0.002) Thread.Sleep((int)((wait - 0.002) * 1000));
			SpinWait spin = new SpinWait();
			while (Now < target) spin.SpinOnce(-1);
		}
	}
}
