// When the game steps, and where between two of its steps the display stands.
//
// The game is a DS-era engine counted in frames: every walk, camera move, script wait, text
// speed and battle timer is so many calls of NitroMain, thirty a second. That rate is the
// game, and it is not touched here. What this decides, once per frame the display draws, is
//
//   Steps()  how many of those 1/30 s steps have come due (0, or 1..3 when catching up), and
//   Blend()  how far the display should stand between the step before and the one just
//            taken - 0.5 right at the step, 1 half a period later - for FrameCapture to
//            draw the frame in between (Fps "60" / "max"); 1 always when Fps is "30".
//
// The clock is an accumulator against a Stopwatch, not the millisecond counter the phone
// build paced by (millis * 3 / 100, slept for a millisecond at a time), whose 33/34 ms
// frames drifted against the display's refresh and showed as judder. A refresh that divides
// the period - 60 Hz, 120, 90 - is taken as exactly that fraction, so on a 60 Hz display the
// game steps on every second refresh without ever slipping a phase. A stall of over a
// quarter second is forgiven rather than caught up on.
//
// Hold() paces the presentation itself to 30 or 60 a second by the Fps setting (240 for
// "max"): a ceiling, for a display whose VSync is off or does not hold, so the loop never
// spins; where VSync holds at that rate the present takes the time and Hold waits for nothing.

using System;
using System.Diagnostics;
using System.Threading;

namespace OpenFF.Client
{
	internal static class FramePacer
	{
		/// <summary>The game's step: 1/30 s.</summary>
		public const double Period = 1.0 / 30.0;

		private static readonly Stopwatch _clock = Stopwatch.StartNew();
		private static double _last = -1;
		private static double _debt;
		private static double _drift;
		private static double _nextPresent;
		private static double _steppedAt;

		/// <summary>How many steps the legacy frame is to run this time (render() reads it in place of its own clock); 1 outside a paced frame, as for --speed's extra ticks.</summary>
		public static int PendingSteps = 1;

		/// <summary>Whether frames are drawn between the game's own: Fps "60" or "max".</summary>
		public static bool Smooth => (DisplaySettings.Current?.Fps ?? "60") != "30";

		/// <summary>The presentation cap: 30 or 60 by the setting, 240 for "max" - a ceiling for when VSync is off or does not hold (a window the driver will not wait for), so the loop never spins.</summary>
		public static int Cap
		{
			get
			{
				string fps = DisplaySettings.Current?.Fps ?? "60";
				return fps == "30" ? 30 : fps == "60" ? 60 : 240;
			}
		}

		/// <summary>The steps due now, 0..3. The first call is one step.</summary>
		public static int Steps()
		{
			double now = _clock.Elapsed.TotalSeconds;
			if (_last < 0)
			{
				_last = now;
				_debt = 0;
				_steppedAt = now;
				return 1;
			}
			double real = Math.Min(now - _last, 0.25);
			_last = now;
			double dt = real;
			// A refresh that is a whole fraction of the period, within 8%, counts as exactly that
			// fraction: 60 Hz is two refreshes a step, 120 four, 90 three - and the phase holds.
			// The quantised clock is kept within half a step of the real one, so a display that
			// only happens to run near such a rate (VSync off, a frame rate wandering about 150)
			// still gets its thirty steps a second, with a phase slip now and then instead of a drift.
			for (int n = 1; n <= 8; n++)
			{
				double q = Period / n;
				if (Math.Abs(real - q) < q * 0.08)
				{
					dt = q;
					break;
				}
			}
			_drift += real - dt;
			if (Math.Abs(_drift) > Period / 2)
			{
				dt += _drift;
				_drift = 0;
			}
			_debt += dt;
			int steps = 0;
			while (_debt >= Period - 1e-6 && steps < 3)
			{
				_debt -= Period;
				steps++;
			}
			if (_debt >= Period)
			{
				// Hopelessly behind (a stall, a debugger): the rest is forgiven, not run.
				_debt = 0;
			}
			if (steps > 0) _steppedAt = now;
			return steps;
		}

		/// <summary>Where the display stands between the step before and the one just taken: 0.5 at the step, 1 half a period on; 1 always when not smoothing.</summary>
		public static float Blend()
		{
			if (!Smooth) return 1f;
			return (float)Math.Min(1.0, 0.5 + _debt / Period);
		}

		/// <summary>Called once the frame is drawn, before it is presented: waits out the presentation cap. With VSync holding at the cap's rate the present already takes the time and this waits for nothing.</summary>
		public static void Hold()
		{
			int cap = Cap;
			if (cap <= 0) return;
			double interval = 1.0 / cap;
			double now = _clock.Elapsed.TotalSeconds;
			if (_nextPresent <= 0 || now - _nextPresent > interval) _nextPresent = now;
			_nextPresent += interval;
			double wait = _nextPresent - now;
			if (wait <= 0) return;
			// Sleep to within two milliseconds, then spin: Sleep's grain is a millisecond or worse.
			if (wait > 0.002) Thread.Sleep((int)((wait - 0.002) * 1000));
			SpinWait spin = new SpinWait();
			while (_clock.Elapsed.TotalSeconds < _nextPresent) spin.SpinOnce();
		}

		/// <summary>Without the native renderer nothing is captured to draw in between: wait out the time to the next step instead, as the phone build did.</summary>
		public static void WaitForStep()
		{
			double target = _steppedAt + Period;
			double now = _clock.Elapsed.TotalSeconds;
			double wait = target - now;
			if (wait > 0.002) Thread.Sleep((int)((wait - 0.002) * 1000));
			SpinWait spin = new SpinWait();
			while (_clock.Elapsed.TotalSeconds < target) spin.SpinOnce();
		}
	}
}
